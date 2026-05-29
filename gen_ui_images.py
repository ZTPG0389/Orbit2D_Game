from PIL import Image, ImageDraw

OUT = "Assets/Resources/Sprites/UI/"

# ── IMAGE 1: Background Overlay ───────────────────────────────────────────────
bg = Image.new("RGBA", (1080, 1920), (0, 0, 0, 180))
bg.save(OUT + "bg_overlay.png")

# ── IMAGE 2: Card Border Glow ─────────────────────────────────────────────────
img  = Image.new("RGBA", (320, 440), (0, 0, 0, 0))
draw = ImageDraw.Draw(img, "RGBA")

CYAN      = (0, 229, 255)
CYAN_FULL = (0, 255, 255)
INNER     = (0, 136, 170)

# Outer glow layers (drawn first so main border sits on top)
glow_alphas = [153, 102, 64, 38, 20]   # 60%, 40%, 25%, 15%, 8%
for i, alpha in enumerate(glow_alphas):
    offset = i + 1
    color  = (*CYAN, alpha)
    draw.rectangle(
        [2 - offset, 2 - offset, 318 + offset, 438 + offset],
        outline=color, width=1
    )

# Outer border — 3px
draw.rectangle([2, 2, 318, 438], outline=(*CYAN, 255), width=3)

# Inner border — 1px, 40% opacity
draw.rectangle([6, 6, 314, 434], outline=(*INNER, 102), width=1)

# Corner L-brackets — 3px, #00ffff
bracket_segs = [
    # Top-Left
    [(2,2),(42,2)], [(2,2),(2,42)],
    # Top-Right
    [(318,2),(278,2)], [(318,2),(318,42)],
    # Bottom-Left
    [(2,438),(42,438)], [(2,438),(2,398)],
    # Bottom-Right
    [(318,438),(278,438)], [(318,438),(318,398)],
]
for seg in bracket_segs:
    draw.line(seg, fill=(*CYAN_FULL, 255), width=3)

img.save(OUT + "card_border_glow.png")

print("Done! Both images saved.")
