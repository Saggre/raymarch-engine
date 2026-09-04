// Screen overlay: a crosshair and a numeric readout.
//
// Drawn after the tone curve, so it is a flat overlay at full brightness rather than something
// the exposure and the fog act on.
//
// There is no font here and nowhere to put one: this renderer draws a single fullscreen quad and
// has no text pipeline, so the digits are seven segment shapes built out of rectangles. That is
// enough for a debug number and costs no texture and no vertex data.
//
// Positions are in a space where y runs 0 to 1 down the screen and x is scaled by the aspect
// ratio, so a square is square and sizes read as fractions of screen height.

// ---------------------------------------------------------------------------------------------
// Shapes
// ---------------------------------------------------------------------------------------------

// Softened over roughly a pixel. The render target is a fixed size, so this is a constant rather
// than a screen space derivative, which keeps it out of any question about divergent flow.
#define HUD_AA 0.0007

float hudRect(float2 p, float2 center, float2 halfSize)
{
    float2 distance = abs(p - center) - halfSize;
    float outside = max(distance.x, distance.y);

    return 1.0 - smoothstep(-HUD_AA, HUD_AA, outside);
}

// ---------------------------------------------------------------------------------------------
// Crosshair
// ---------------------------------------------------------------------------------------------

// One arm, measured across its width and along its length from the centre
float hudArm(float across, float along, float thickness, float gap, float length)
{
    float inside = 1.0 - smoothstep(thickness - HUD_AA, thickness + HUD_AA, abs(across));
    float begins = smoothstep(gap - HUD_AA, gap + HUD_AA, along);
    float ends = 1.0 - smoothstep(gap + length - HUD_AA, gap + length + HUD_AA, along);

    return inside * begins * ends;
}

float hudCross(float2 p, float thickness, float gap, float length)
{
    return saturate(hudArm(p.y, abs(p.x), thickness, gap, length) +
                    hudArm(p.x, abs(p.y), thickness, gap, length));
}

// A dark cross slightly larger than the light one, so the crosshair stays readable against both
// the sky and a lit surface
float3 hudCrosshair(float3 color, float2 centered)
{
    float outline = hudCross(centered,
                             CROSSHAIR_THICKNESS + CROSSHAIR_OUTLINE,
                             CROSSHAIR_GAP - CROSSHAIR_OUTLINE,
                             CROSSHAIR_LENGTH + 2.0 * CROSSHAIR_OUTLINE);

    float core = hudCross(centered, CROSSHAIR_THICKNESS, CROSSHAIR_GAP, CROSSHAIR_LENGTH);

    color = lerp(color, float3(0, 0, 0), outline * CROSSHAIR_OUTLINE_ALPHA);

    return lerp(color, CROSSHAIR_COLOR, core);
}

// ---------------------------------------------------------------------------------------------
// Seven segment digits
// ---------------------------------------------------------------------------------------------

// Bit 1 top, 2 top right, 4 bottom right, 8 bottom, 16 bottom left, 32 top left, 64 middle
static const int HUD_DIGIT_SEGMENTS[10] = {63, 6, 91, 79, 102, 109, 125, 7, 127, 111};

static const uint HUD_PLACE_VALUES[4] = {1000, 100, 10, 1};

// q is inside the digit's own box, 0 to 1 on both axes with y running down
float hudDigit(float2 q, int value)
{
    int bits = HUD_DIGIT_SEGMENTS[clamp(value, 0, 9)];

    float2 acrossHalf = float2(0.34, 0.05);
    float2 downHalf = float2(0.05, 0.20);

    float mask = 0.0;

    if ((bits & 1) != 0) { mask += hudRect(q, float2(0.50, 0.04), acrossHalf); }
    if ((bits & 2) != 0) { mask += hudRect(q, float2(0.94, 0.27), downHalf); }
    if ((bits & 4) != 0) { mask += hudRect(q, float2(0.94, 0.73), downHalf); }
    if ((bits & 8) != 0) { mask += hudRect(q, float2(0.50, 0.96), acrossHalf); }
    if ((bits & 16) != 0) { mask += hudRect(q, float2(0.06, 0.73), downHalf); }
    if ((bits & 32) != 0) { mask += hudRect(q, float2(0.06, 0.27), downHalf); }
    if ((bits & 64) != 0) { mask += hudRect(q, float2(0.50, 0.50), acrossHalf); }

    return saturate(mask);
}

// The readout, as two whole digits and two decimals. Fixed width rather than trimmed, so the
// number does not jitter sideways as it changes.
float3 hudNumber(float3 color, float2 screen, float value)
{
    float2 cell = float2(HUD_DIGIT_WIDTH, HUD_DIGIT_HEIGHT);
    float advance = HUD_DIGIT_WIDTH + HUD_DIGIT_GAP;

    uint scaled = (uint) clamp(round(value * 100.0), 0.0, 9999.0);

    float mask = 0.0;

    [unroll]
    for (int i = 0; i < 4; i++)
    {
        // The last two digits are pushed right to leave room for the point
        float slot = i * advance + (i >= 2 ? HUD_POINT_ADVANCE : 0.0);

        float2 q = (screen - HUD_ORIGIN - float2(slot, 0.0)) / cell;
        if (q.x < 0.0 || q.x > 1.0 || q.y < 0.0 || q.y > 1.0)
        {
            continue;
        }

        mask += hudDigit(q, (int) ((scaled / HUD_PLACE_VALUES[i]) % 10));
    }

    // The decimal point, sitting on the baseline between the second and third digit
    float2 pointCenter = HUD_ORIGIN + float2(2.0 * advance - HUD_DIGIT_GAP, HUD_DIGIT_HEIGHT * 0.92);
    mask += hudRect(screen, pointCenter, HUD_POINT_SIZE.xx);

    return lerp(color, HUD_TEXT_COLOR, saturate(mask) * HUD_TEXT_ALPHA);
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
