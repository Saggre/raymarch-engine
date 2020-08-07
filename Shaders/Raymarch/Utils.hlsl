float smin(float a, float b, float k)
{
	float h = clamp(0.5 + 0.5 * (a - b) / k, 0.0, 1.0);
	return lerp(a, b, h) - k * h * (1.0 - h);
}

float opUnion(float d1, float d2)
{
	return min(d1, d2);
}

float opOnion(in float d, in float h)
{
	return abs(d) - h;
}

float opSubtraction(float d1, float d2)
{
	return max(-d1, d2);
}

float opIntersection(float d1, float d2)
{
	return max(d1, d2);
}

// Round the object
float opRound(in float obj, float rad)
{
	return obj - rad;
}

float mod(float x, float y)
{
	return x - y * floor(x / y);
}

float mod(float3 x, float3 y)
{
	return float3(mod(x.x, y.x), mod(x.y, y.y), mod(x.z, y.z));
}

// Infinite repetition
float3 infinite(in float3 p, in float3 c)
{
	float3 q = fmod(p + 0.5 * c, c) - 0.5 * c;
	return q;
}

void boxFold(inout float3 z, float3 r)
{
	z = clamp(z, -r, r) * 2.0 - z;
}

void planeFold(inout float3 z, float3 n, float d)
{
	z -= 2.0 * min(0.0, dot(z, n) - d) * n;
}

void absFold(inout float3 z, float3 c)
{
	z = abs(z - c) + c;
}

void sierpinskiFold(inout float3 z)
{
	z.xy -= min(z.x + z.y, 0.0);
	z.xz -= min(z.x + z.z, 0.0);
	z.yz -= min(z.y + z.z, 0.0);
}

void mengerFold(inout float3 z)
{
	float a = min(z.x - z.y, 0.0);
	z.x -= a;
	z.y += a;
	a = min(z.x - z.z, 0.0);
	z.x -= a;
	z.z += a;
	a = min(z.y - z.z, 0.0);
	z.y -= a;
	z.z += a;
}

void sphereFold(inout float3 z, float minR, float maxR)
{
	float r2 = dot(z.xyz, z.xyz);
	z *= max(maxR / max(minR, r2), 1.0);
}