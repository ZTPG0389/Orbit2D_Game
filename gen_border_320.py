from PIL import Image, ImageDraw

W, H = 320, 440
OUT  = "Assets/Resources/Sprites/UI/circuit_border_frame_320x440.png"

img  = Image.new("RGBA", (W, H), (0, 0, 0, 0))
draw = ImageDraw.Draw(img, "RGBA")

CYAN_MAIN    = (93,  216, 236, 255)
CYAN_MED     = (74,  184, 200, 255)
CYAN_BRIGHT  = (125, 224, 240, 255)
INNER_BORDER = (58,  168, 190,  89)   # 35 % opacity
BRACKET      = (142, 240, 255, 255)
DOTS         = (125, 224, 240, 255)
PAD          = (125, 224, 240, 153)   # 60 % opacity

# ── outer border ─────────────────────────────────────────────────────────────
draw.rectangle([3, 3, 317, 437], outline=CYAN_MAIN, width=3)

# ── inner border ─────────────────────────────────────────────────────────────
draw.rectangle([8, 8, 312, 432], outline=INNER_BORDER, width=1)

# ── corner L-brackets ────────────────────────────────────────────────────────
# top-left
draw.line([(3,3),(38,3)],   fill=BRACKET, width=3)
draw.line([(3,3),(3,38)],   fill=BRACKET, width=3)
# top-right
draw.line([(317,3),(282,3)], fill=BRACKET, width=3)
draw.line([(317,3),(317,38)],fill=BRACKET, width=3)
# bottom-left
draw.line([(3,437),(38,437)], fill=BRACKET, width=3)
draw.line([(3,437),(3,402)],  fill=BRACKET, width=3)
# bottom-right
draw.line([(317,437),(282,437)], fill=BRACKET, width=3)
draw.line([(317,437),(317,402)], fill=BRACKET, width=3)

# ── circuit traces left ───────────────────────────────────────────────────────
left_traces = [
    [(3,60),(3,80),(20,80),(20,110)],
    [(3,130),(20,130),(20,160),(3,160)],
    [(3,220),(20,220),(20,250)],
    [(3,290),(20,290),(20,320),(3,320)],
    [(3,370),(20,370),(20,400)],
]
for pts in left_traces:
    draw.line(pts, fill=CYAN_MED, width=1)

# ── circuit traces right ──────────────────────────────────────────────────────
right_traces = [
    [(317,60),(317,80),(300,80),(300,110)],
    [(317,130),(300,130),(300,160),(317,160)],
    [(317,220),(300,220),(300,250)],
    [(317,290),(300,290),(300,320),(317,320)],
]
for pts in right_traces:
    draw.line(pts, fill=CYAN_MED, width=1)

# ── circuit traces top ────────────────────────────────────────────────────────
top_traces = [
    [(50,3),(80,3),(80,18),(110,18)],
    [(130,3),(130,18),(160,18),(160,3)],
    [(180,3),(180,18),(210,18),(210,3)],
]
for pts in top_traces:
    draw.line(pts, fill=CYAN_MED, width=1)

# ── circuit traces bottom ─────────────────────────────────────────────────────
bottom_traces = [
    [(50,437),(80,437),(80,422),(110,422)],
    [(130,437),(130,422),(160,422),(160,437)],
    [(180,437),(180,422),(210,422),(210,437)],
]
for pts in bottom_traces:
    draw.line(pts, fill=CYAN_MED, width=1)

# ── junction dots ─────────────────────────────────────────────────────────────
def dot(cx, cy, r=2):
    draw.ellipse([cx-r, cy-r, cx+r, cy+r], fill=DOTS)

for cx, cy in [(20,80),(20,130),(20,220),(20,290),(20,370)]:    dot(cx, cy)
for cx, cy in [(300,80),(300,130),(300,220),(300,290)]:         dot(cx, cy)
for cx, cy in [(80,3),(160,3),(210,3)]:                         dot(cx, cy)
for cx, cy in [(80,437),(160,437),(210,437)]:                   dot(cx, cy)

# ── open circle pads ──────────────────────────────────────────────────────────
def pad(cx, cy, r=3):
    draw.ellipse([cx-r, cy-r, cx+r, cy+r], outline=PAD, width=1)

for cx, cy in [(3,200),(3,360),(317,200),(317,360),(160,3),(160,437)]:
    pad(cx, cy)

img.save(OUT)
print("Done! circuit_border_frame_320x440.png saved")
