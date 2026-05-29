from PIL import Image, ImageDraw

W, H = 540, 720
OUT  = "Assets/Resources/Sprites/UI/circuit_border_frame.png"

img  = Image.new("RGBA", (W, H), (0, 0, 0, 0))
draw = ImageDraw.Draw(img, "RGBA")

# ── palette ────────────────────────────────────────────────────────────────
CYAN_MAIN     = (93,  216, 236, 255)   # #5dd8ec
CYAN_MED      = (74,  184, 200, 140)   # #4ab8c8 55 %
CYAN_BRIGHT   = (125, 224, 240, 204)   # #7de0f0 80 %
CORNER_FILL   = (7,   24,  40,  255)   # #071828
DOTS          = (125, 224, 240, 255)   # #7de0f0
BRACKET       = (142, 240, 255, 255)   # #8ef0ff
INNER_BORDER  = (58,  168, 190,  89)   # #3aa8be 35 %
INNER_BRACKET = (93,  216, 236, 128)   # #5dd8ec 50 %
PAD           = (125, 224, 240, 153)   # #7de0f0 60 %

# ── outer border ────────────────────────────────────────────────────────────
draw.rounded_rectangle([4, 4, 536, 716], radius=6, outline=CYAN_MAIN, width=2)

# ── inner border ────────────────────────────────────────────────────────────
draw.rounded_rectangle([12, 12, 528, 708], radius=4, outline=INNER_BORDER, width=1)

# ── corner decorations ──────────────────────────────────────────────────────
def draw_corner(ox, oy, dx, dy):
    """ox,oy = corner pixel; dx,dy = ±1 direction of arms"""
    arm = 56
    # dark filled square
    fx1 = ox if dx > 0 else ox - 30
    fy1 = oy if dy > 0 else oy - 30
    draw.rectangle([fx1, fy1, fx1 + 30, fy1 + 30], fill=CORNER_FILL)
    # outer L-bracket 3 px
    draw.line([(ox, oy), (ox + dx * arm, oy)],       fill=BRACKET, width=3)
    draw.line([(ox, oy), (ox, oy + dy * arm)],        fill=BRACKET, width=3)
    # inner L-bracket 1 px (inset 6 px)
    ix, iy = ox + dx * 6, oy + dy * 6
    draw.line([(ix, iy), (ix + dx * (arm - 10), iy)], fill=INNER_BRACKET, width=1)
    draw.line([(ix, iy), (ix, iy + dy * (arm - 10))], fill=INNER_BRACKET, width=1)

draw_corner(  4,   4,  1,  1)   # top-left
draw_corner(536,   4, -1,  1)   # top-right
draw_corner(  4, 716,  1, -1)   # bottom-left
draw_corner(536, 716, -1, -1)   # bottom-right

# ── circuit traces ───────────────────────────────────────────────────────────
left_traces = [
    [(20,80),(20,120),(50,120),(50,160)],
    [(20,200),(20,240),(45,240),(45,280),(20,280)],
    [(20,320),(55,320),(55,360),(20,360)],
    [(20,400),(45,400),(45,440),(20,440)],
    [(20,480),(50,480),(50,520),(20,520)],
    [(55,80),(55,100),(80,100)],
    [(55,180),(80,180),(80,220),(55,220)],
]
top_traces = [
    [(80,20),(120,20),(120,50),(160,50)],
    [(200,20),(200,50),(240,50),(240,20)],
    [(300,20),(300,50),(340,50),(340,20)],
    [(380,20),(380,50),(420,50),(420,20)],
]

for pts in left_traces:
    draw.line(pts, fill=CYAN_MED, width=1)
for pts in [([(W - x, y) for x, y in t]) for t in left_traces]:   # right mirror
    draw.line(pts, fill=CYAN_MED, width=1)
for pts in top_traces:
    draw.line(pts, fill=CYAN_MED, width=1)
for pts in [([(x, H - y) for x, y in t]) for t in top_traces]:    # bottom mirror
    draw.line(pts, fill=CYAN_MED, width=1)

# ── junction dots ────────────────────────────────────────────────────────────
def dot(cx, cy, r=2.5):
    draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=DOTS)

for cx, cy in [(50,120),(45,240),(55,320),(45,400),(50,480)]:          dot(cx, cy)
for cx, cy in [(490,120),(495,240),(485,320),(495,400),(490,480)]:     dot(cx, cy)
for cx, cy in [(120,20),(240,50),(300,50)]:                            dot(cx, cy)
for cx, cy in [(120,700),(240,670),(340,670)]:                         dot(cx, cy)

# ── open circle pads ─────────────────────────────────────────────────────────
def pad(cx, cy, r=4):
    draw.ellipse([cx - r, cy - r, cx + r, cy + r], outline=PAD, width=1)

for cx, cy in [(20,280),(20,440),(520,280),(520,440),
               (200,20),(380,20),(200,700),(380,700)]:
    pad(cx, cy)

# ── side accent marks ─────────────────────────────────────────────────────────
for y in (355, 362):
    draw.line([(4,  y),(14, y)], fill=CYAN_BRIGHT, width=2)   # left
    draw.line([(526,y),(536,y)], fill=CYAN_BRIGHT, width=2)   # right
for x in (265, 275):
    draw.line([(x,4),(x,14)],    fill=CYAN_BRIGHT, width=2)   # top
    draw.line([(x,706),(x,716)], fill=CYAN_BRIGHT, width=2)   # bottom

img.save(OUT)
print("Saved:", OUT)
