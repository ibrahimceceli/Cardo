"""Generates the neon grid tunnel background used behind the game (Assets/Textures/NeonGridBackground.png).

The image is drawn procedurally: a rectangular tunnel is projected in perspective and its floor,
ceiling and walls are covered with glowing grid lines, with a star field on the far wall.

Usage (requires Pillow):  python Tools/generate_background.py [output.png]
"""
import random
import sys

from PIL import Image, ImageDraw, ImageFilter

WIDTH, HEIGHT = 1920, 1080
SUPERSAMPLE = 2  # draw at 2x and downscale for smooth lines

BASE_EDGE = (10, 5, 26)
BASE_CENTER = (36, 20, 66)
FAR_WALL = (24, 14, 48)
GRID_NEAR = (214, 70, 255)
GRID_FAR = (110, 40, 170)

NEAR_Z = 0.85          # slightly in front of the screen so lines start off-canvas
FAR_Z = 2.4            # depth of the far wall; inner rectangle is NEAR/FAR of the screen size
RING_STEP = 0.11       # distance between cross-section rings
FLOOR_LINES = 18       # lines running into the distance on floor and ceiling
WALL_LINES = 10        # lines running into the distance on each side wall
STAR_COUNT = 140
SEED = 7


def lerp(a, b, t):
    return a + (b - a) * t


def lerp_color(c1, c2, t):
    return tuple(int(round(lerp(x, y, t))) for x, y in zip(c1, c2))


def main(output_path):
    w, h = WIDTH * SUPERSAMPLE, HEIGHT * SUPERSAMPLE
    cx, cy = w / 2, h / 2
    half_w, half_h = w / 2, h / 2

    def project(x, y, z):
        # Tunnel cross-section at z = 1 exactly fills the screen.
        return cx + half_w * x / z, cy - half_h * y / z

    # Background: radial gradient from the centre to the edges.
    base = Image.new("RGB", (w, h))
    gradient = Image.radial_gradient("L").resize((w, h))
    base = Image.composite(Image.new("RGB", (w, h), BASE_EDGE), Image.new("RGB", (w, h), BASE_CENTER), gradient)

    draw = ImageDraw.Draw(base)
    far_left, far_top = project(-1, 1, FAR_Z)
    far_right, far_bottom = project(1, -1, FAR_Z)
    draw.rectangle([far_left, far_top, far_right, far_bottom], fill=FAR_WALL)

    # Stars on the far wall and faintly around it.
    rng = random.Random(SEED)
    for _ in range(STAR_COUNT):
        sx = rng.uniform(far_left, far_right)
        sy = rng.uniform(far_top, far_bottom)
        radius = rng.choice([1, 1, 1, 2, 2, 3]) * SUPERSAMPLE / 2
        brightness = rng.randint(120, 255)
        draw.ellipse([sx - radius, sy - radius, sx + radius, sy + radius], fill=(brightness, brightness, min(255, brightness + 20)))

    # Grid lines are drawn on their own layer so they can be blurred into a glow.
    lines = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    line_draw = ImageDraw.Draw(lines)
    line_width = 5  # 2.5 px after downscaling

    def depth_color(z):
        t = (z - NEAR_Z) / (FAR_Z - NEAR_Z)
        color = lerp_color(GRID_NEAR, GRID_FAR, t)
        alpha = int(lerp(255, 140, t))
        return color + (alpha,)

    # Cross-section rings, denser towards the far wall because of the perspective.
    z = NEAR_Z
    while z < FAR_Z:
        left, top = project(-1, 1, z)
        right, bottom = project(1, -1, z)
        line_draw.rectangle([left, top, right, bottom], outline=depth_color(z), width=line_width)
        z += RING_STEP
    line_draw.rectangle([far_left, far_top, far_right, far_bottom], outline=depth_color(FAR_Z), width=line_width)

    # Lines running into the distance, drawn in short segments so they fade with depth.
    def long_line(x, y):
        segments = 24
        for i in range(segments):
            z0 = lerp(NEAR_Z, FAR_Z, i / segments)
            z1 = lerp(NEAR_Z, FAR_Z, (i + 1) / segments)
            line_draw.line([project(x, y, z0), project(x, y, z1)], fill=depth_color(z0), width=line_width)

    for i in range(FLOOR_LINES + 1):
        x = lerp(-1, 1, i / FLOOR_LINES)
        long_line(x, -1)  # floor
        long_line(x, 1)   # ceiling
    for i in range(1, WALL_LINES):
        y = lerp(-1, 1, i / WALL_LINES)
        long_line(-1, y)  # left wall
        long_line(1, y)   # right wall

    glow = lines.filter(ImageFilter.GaussianBlur(radius=9 * SUPERSAMPLE))
    image = base.convert("RGBA")
    for _ in range(3):
        image.alpha_composite(glow)
    image.alpha_composite(lines)

    # Soft vignette to keep the edges calm behind the HUD.
    vignette = Image.radial_gradient("L").resize((w, h)).point(lambda v: int(max(0, v - 150) * 0.9))
    image = Image.composite(Image.new("RGBA", (w, h), (0, 0, 0, 255)), image, vignette)

    image = image.convert("RGB").resize((WIDTH, HEIGHT), Image.LANCZOS)
    image.save(output_path, optimize=True)
    print(f"Saved {output_path} ({WIDTH}x{HEIGHT})")


if __name__ == "__main__":
    main(sys.argv[1] if len(sys.argv) > 1 else "Assets/Textures/NeonGridBackground.png")
