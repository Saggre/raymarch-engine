"""Generates the RaymarchEngine icon.

The mark is a signed distance field, drawn the way one is: a solid primitive with the isolines of
its distance function ringing it. The rings are the actual field, evaluated per sample, not drawn
shapes, which is the point.
"""
import math
import struct
import sys
import zlib

# --- palette, taken from the scene the engine renders ------------------------------------------
BG_TOP = (0x22, 0x30, 0x4C)
BG_BOTTOM = (0x0D, 0x13, 0x1F)
CORE_TOP = (0xFF, 0x8A, 0x3D)
CORE_BOTTOM = (0xE0, 0x33, 0x0A)
RING = (0x9A, 0xD8, 0xFF)

CORNER = 0.21          # Rounded square, as a fraction of the icon
RING_ALPHA = (0.92, 0.60, 0.34)


def variant(size):
    """Half width of the diamond and the isolines to draw around it, for one icon size.

    A small icon cannot hold three isolines and a shape with corners at the same time. Rather than
    let them all turn to mush, the smaller entries carry fewer rings and a larger core, and the
    16 pixel one carries none at all: at that size the diamond alone is the mark.
    """
    if size >= 64:
        return 0.205, (0.080, 0.163, 0.246)
    if size >= 32:
        return 0.235, (0.083, 0.170)

    return 0.310, ()


def diamond(x, y, half):
    """Exact distance to a diamond, negative inside.

    Taken to the nearest point on one edge rather than to the edge's line, so the isolines round
    off at the vertices the way a real distance field does.
    """
    qx, qy = abs(x), abs(y)

    ex, ey = -half, half
    wx, wy = qx - half, qy
    t = max(0.0, min(1.0, (wx * ex + wy * ey) / (ex * ex + ey * ey)))
    d = math.hypot(wx - t * ex, wy - t * ey)

    return -d if qx + qy < half else d


def rounded_square(x, y, half, radius):
    qx, qy = abs(x) - (half - radius), abs(y) - (half - radius)

    return math.hypot(max(qx, 0.0), max(qy, 0.0)) + min(max(qx, qy), 0.0) - radius


def over(dst, src, alpha):
    return tuple(s * alpha + d * (1.0 - alpha) for s, d in zip(src, dst))


def sample(x, y, core, rings, width):
    """Colour at a point, in a space running -0.5 to 0.5 with y downwards."""
    if rounded_square(x, y, 0.5, CORNER) > 0.0:
        return None

    lift = y + 0.5
    color = tuple(t + (b - t) * lift for t, b in zip(BG_TOP, BG_BOTTOM))

    d = diamond(x, y, core)

    # Outermost first, so a nearer isoline covers a further one where they meet
    for radius, alpha in reversed(list(zip(rings, RING_ALPHA))):
        if abs(d - radius) < width:
            color = over(color, RING, alpha)

    if d < 0.0:
        span = lift  # the diamond is lit from above, like everything else in the scene
        color = tuple(t + (b - t) * span for t, b in zip(CORE_TOP, CORE_BOTTOM))

    return color


def render(size, supersample=4):
    """One icon, box filtered down from a supersampled grid."""
    core, rings = variant(size)
    width = max(0.016, 0.62 / size)

    n = size * supersample
    out = bytearray(size * size * 4)

    for py in range(size):
        for px in range(size):
            r = g = b = a = 0.0
            for sy in range(supersample):
                for sx in range(supersample):
                    x = (px * supersample + sx + 0.5) / n - 0.5
                    y = (py * supersample + sy + 0.5) / n - 0.5
                    c = sample(x, y, core, rings, width)
                    if c is not None:
                        r += c[0]
                        g += c[1]
                        b += c[2]
                        a += 255.0

            count = supersample * supersample
            covered = a / 255.0
            i = (py * size + px) * 4

            # Colour was only accumulated where the sample landed inside, so it averages over
            # those samples, while alpha averages over all of them
            if covered > 0.0:
                out[i] = min(255, int(r / covered + 0.5))
                out[i + 1] = min(255, int(g / covered + 0.5))
                out[i + 2] = min(255, int(b / covered + 0.5))
            out[i + 3] = int(a / count + 0.5)

    return bytes(out)


def png(size, rgba):
    raw = b''.join(b'\x00' + rgba[y * size * 4:(y + 1) * size * 4] for y in range(size))

    def chunk(tag, data):
        body = tag + data
        return struct.pack('>I', len(data)) + body + struct.pack('>I', zlib.crc32(body) & 0xFFFFFFFF)

    return (b'\x89PNG\r\n\x1a\n' +
            chunk(b'IHDR', struct.pack('>IIBBBBB', size, size, 8, 6, 0, 0, 0)) +
            chunk(b'IDAT', zlib.compress(raw, 9)) +
            chunk(b'IEND', b''))


def bmp(size, rgba):
    """A 32 bit DIB, which is what the shell expects for the smaller entries."""
    header = struct.pack('<IiiHHIIiiII', 40, size, size * 2, 1, 32, 0, size * size * 4, 0, 0, 0, 0)

    rows = []
    for y in range(size - 1, -1, -1):
        row = bytearray()
        for x in range(size):
            i = (y * size + x) * 4
            row += bytes((rgba[i + 2], rgba[i + 1], rgba[i], rgba[i + 3]))
        rows.append(bytes(row))

    stride = ((size + 31) // 32) * 4

    return header + b''.join(rows) + b'\x00' * (stride * size)


def ico(path, entries):
    out = struct.pack('<HHH', 0, 1, len(entries))
    offset = 6 + 16 * len(entries)

    directory = b''
    blobs = b''
    for size, data in entries:
        stored = 0 if size >= 256 else size
        directory += struct.pack('<BBBBHHII', stored, stored, 0, 0, 1, 32, len(data), offset)
        offset += len(data)
        blobs += data

    with open(path, 'wb') as handle:
        handle.write(out + directory + blobs)


def main():
    target = sys.argv[1]

    entries = []
    for size in (16, 32, 48, 64, 128, 256):
        rgba = render(size)
        entries.append((size, png(size, rgba) if size >= 128 else bmp(size, rgba)))
        if size == 256:
            with open(sys.argv[2], 'wb') as handle:
                handle.write(png(size, rgba))
        print('rendered %d' % size, file=sys.stderr)

    ico(target, entries)


main()
