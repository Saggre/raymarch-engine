// Screen overlay: a crosshair and a numeric readout.
//
// Drawn after the tone curve, so it is a flat overlay at full brightness rather than something
// the exposure and the fog act on.
//
// There is no font here and nowhere to put one: this renderer draws a single fullscreen quad
// and has no text pipeline, so the digits are seven segment shapes built out of rectangles. That
// is enough for a debug number and costs no texture and no vertex data.
//
// Positions are in a space where y runs 0 to 1 down the screen and x is scaled by the aspect
// ratio, so a square is square and sizes read as fractions of screen height.

// ---------------------------------------------------------------------------------------------
// Shapes
// ---------------------------------------------------------------------------------------------

// Softened over about half a pixel. The render target is a fixed size, so this is a constant
// rather than a screen space derivative, which keeps it out of any question about divergent flow.
#define HUD_AA 0.00035

float hudRect(float2 p, float2 center, float2 halfSize)
{
    float2 distance = abs(p - center) - halfSize;
    float outside = max(distance.x, distance.y);

    return 1.0 - smoothstep(-HUD_AA, HUD_AA, outside);
}

// ---------------------------------------------------------------------------------------------
// Crosshair
// ---------------------------------------------------------------------------------------------

float hudDisc(float2 p, float radius)
{
    return 1.0 - smoothstep(radius - HUD_AA, radius + HUD_AA, length(p));
}

// A dot, on a dark disc slightly larger than it, so the crosshair reads against both the sky and
// a lit surface.
//
// A cross was a few pixels of bar in each direction, and at that size the arms landed on a whole
// number of pixels one way and straddled the centre line the other, which made a symmetric shape
// come out visibly wider than it was tall. A disc has no axis to disagree about.
float3 hudCrosshair(float3 color, float2 centered)
{
    float outline = hudDisc(centered, CROSSHAIR_RADIUS + CROSSHAIR_OUTLINE);
    float core = hudDisc(centered, CROSSHAIR_RADIUS);

    color = lerp(color, float3(0, 0, 0), outline * CROSSHAIR_OUTLINE_ALPHA);

    return lerp(color, CROSSHAIR_COLOR, core);
}

// ---------------------------------------------------------------------------------------------
// Seven segment digits
// ---------------------------------------------------------------------------------------------

// Bit 1 top, 2 top right, 4 bottom right, 8 bottom, 16 bottom left, 32 top left, 64 middle
static const int HUD_DIGIT_SEGMENTS[10] = {63, 6, 91, 79, 102, 109, 125, 7, 127, 111};

static const uint HUD_PLACE_VALUES[4] = {1000, 100, 10, 1};

// q is inside the digit's own box, 0 to 1 on both axes with y running down. grow widens every
// segment, which is what draws the dark backing behind the glyph.
float hudDigit(float2 q, int value, float grow)
{
    int bits = HUD_DIGIT_SEGMENTS[clamp(value, 0, 9)];

    float thickness = HUD_SEGMENT_THICKNESS + grow;
    float2 acrossHalf = float2(0.34 + grow, thickness);
    float2 downHalf = float2(thickness, 0.20 + grow);

    float mask = 0.0;

    if ((bits & 1) != 0) { mask += hudRect(q, float2(0.50, 0.08), acrossHalf); }
    if ((bits & 2) != 0) { mask += hudRect(q, float2(0.90, 0.29), downHalf); }
    if ((bits & 4) != 0) { mask += hudRect(q, float2(0.90, 0.71), downHalf); }
    if ((bits & 8) != 0) { mask += hudRect(q, float2(0.50, 0.92), acrossHalf); }
    if ((bits & 16) != 0) { mask += hudRect(q, float2(0.10, 0.71), downHalf); }
    if ((bits & 32) != 0) { mask += hudRect(q, float2(0.10, 0.29), downHalf); }
    if ((bits & 64) != 0) { mask += hudRect(q, float2(0.50, 0.50), acrossHalf); }

    return saturate(mask);
}

// The readout, as a whole number of up to four digits. Leading zeros are dropped, but the digits
// keep their columns, so the number does not jitter sideways as it changes.
//
// Both passes are accumulated across every digit before either is composited. Drawing a digit's
// backing and glyph together would let the next digit's backing cut into the previous glyph.
float3 hudNumber(float3 color, float2 screen, float value)
{
    float2 cell = float2(HUD_DIGIT_WIDTH, HUD_DIGIT_HEIGHT);
    float advance = HUD_DIGIT_WIDTH + HUD_DIGIT_GAP;

    uint scaled = (uint) clamp(round(value), 0.0, 9999.0);

    float glyph = 0.0;
    float backing = 0.0;

    [unroll]
    for (int i = 0; i < 4; i++)
    {
        // Blank until the number is big enough to reach this column, except the last, which is
        // always drawn so that a speed of zero still reads as a zero
        if (i < 3 && scaled < HUD_PLACE_VALUES[i])
        {
            continue;
        }

        float2 q = (screen - HUD_ORIGIN - float2(i * advance, 0.0)) / cell;

        // Widened by the backing, so a glyph's outline is not clipped at the cell edge
        if (q.x < -HUD_TEXT_OUTLINE || q.x > 1.0 + HUD_TEXT_OUTLINE ||
            q.y < -HUD_TEXT_OUTLINE || q.y > 1.0 + HUD_TEXT_OUTLINE)
        {
            continue;
        }

        int digit = (int) ((scaled / HUD_PLACE_VALUES[i]) % 10);

        glyph += hudDigit(q, digit, 0.0);
        backing += hudDigit(q, digit, HUD_TEXT_OUTLINE);
    }

    color = lerp(color, float3(0, 0, 0), saturate(backing) * HUD_TEXT_OUTLINE_ALPHA);

    return lerp(color, HUD_TEXT_COLOR, saturate(glyph) * HUD_TEXT_ALPHA);
}

// ---------------------------------------------------------------------------------------------
// Entry point
// ---------------------------------------------------------------------------------------------

// texCoord is the raw 0 to 1 screen coordinate, and its y runs up the screen here. Both spaces
// below flip it, so that y runs down and the segment layout above reads the way it is drawn: one
// centred for the crosshair, one anchored to the top left corner for the readout.
float3 applyHud(float3 color, float2 texCoord, float speed)
{
    float2 screen = float2(texCoord.x * aspectRatio, 1.0 - texCoord.y);
    float2 centered = float2((texCoord.x - 0.5) * aspectRatio, 0.5 - texCoord.y);

    color = hudNumber(color, screen, speed);

    return hudCrosshair(color, centered);
}
