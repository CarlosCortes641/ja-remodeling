#!/usr/bin/env python3
"""Generate social/brand images (Open Graph, logo, touch icon, favicon) with Pillow."""
from __future__ import annotations

from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
WWW = ROOT / "src" / "JARemodeling.Web" / "wwwroot"
FONTS = Path("/System/Library/Fonts/Supplemental")

INK = "#163746"
DEEPEST = "#123f50"
DEEP = "#1d5668"
TEAL = "#168c7e"
MINT = "#c9f0e7"
PAPER = "#fdfcf8"
ORANGE = "#ef7045"


def font(name: str, size: int) -> ImageFont.FreeTypeFont:
    return ImageFont.truetype(str(FONTS / name), size)


def draw_mark(draw: ImageDraw.ImageDraw, x: int, y: int, size: int, color: str) -> int:
    """Draw the J&A mark with tight tracking and a teal ampersand; return its width."""
    f = font("Arial Black.ttf", size)
    tracking = -int(size * 0.08)
    cursor = x
    for ch, fill in (("J", color), ("&", TEAL), ("A", color)):
        draw.text((cursor, y), ch, font=f, fill=fill)
        cursor += int(draw.textlength(ch, font=f)) + tracking
    return cursor - x - tracking


def og_image() -> None:
    img = Image.new("RGB", (1200, 630), DEEPEST)
    d = ImageDraw.Draw(img)
    d.rectangle([0, 0, 1200, 46], fill=DEEP)
    small = font("Arial Bold.ttf", 17)
    d.text((64, 13), "J&A REMODELING AND SERVICES LLC", font=small, fill=MINT)
    right = "CHARLOTTE, NORTH CAROLINA"
    d.text((1136 - d.textlength(right, font=small), 13), right, font=small, fill=MINT)

    width = draw_mark(d, 64, 96, 84, PAPER)
    words = font("Arial Bold.ttf", 19)
    d.text((64 + width + 22, 112), "REMODELING", font=words, fill=PAPER)
    d.text((64 + width + 22, 138), "+ SERVICES", font=words, fill=PAPER)

    d.text((64, 252), "Vacant to rent-ready.", font=font("Arial Bold.ttf", 78), fill=PAPER)
    d.text((64, 342), "One accountable team.", font=font("Georgia Italic.ttf", 78), fill=MINT)

    d.rectangle([64, 470, 164, 476], fill=ORANGE)
    d.text((64, 500), "Unit turns · Make-ready · Painting · Drywall · Flooring · Renovation",
           font=font("Arial.ttf", 26), fill=MINT)
    d.text((64, 546), "Insured · English / Español · 300+ unit capacity · 980-298-4600",
           font=font("Arial Bold.ttf", 26), fill=PAPER)

    d.ellipse([1010, 120, 1250, 360], outline=TEAL, width=3)
    (WWW / "img").mkdir(parents=True, exist_ok=True)
    img.save(WWW / "img" / "og-image.png", optimize=True)


def square_icon(size: int) -> Image.Image:
    scale = 4
    big = size * scale
    img = Image.new("RGBA", (big, big), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rounded_rectangle([0, 0, big - 1, big - 1], radius=int(big * 0.18), fill=INK)
    f = font("Arial Black.ttf", int(big * 0.33))
    tracking = -int(big * 0.035)
    widths = [d.textlength(ch, font=f) for ch in "J&A"]
    total = sum(widths) + tracking * 2
    bbox = d.textbbox((0, 0), "J&A", font=f)
    x = (big - total) / 2
    y = (big - (bbox[3] - bbox[1])) / 2 - bbox[1]
    for ch, w, fill in zip("J&A", widths, (PAPER, TEAL, PAPER)):
        d.text((x, y), ch, font=f, fill=fill)
        x += w + tracking
    return img.resize((size, size), Image.LANCZOS)


def main() -> None:
    og_image()
    square_icon(512).save(WWW / "img" / "logo.png", optimize=True)
    square_icon(180).convert("RGB").save(WWW / "img" / "apple-touch-icon.png", optimize=True)
    square_icon(64).save(WWW / "favicon.ico", sizes=[(16, 16), (32, 32), (48, 48), (64, 64)])
    print("wrote og-image.png, logo.png, apple-touch-icon.png, favicon.ico")


if __name__ == "__main__":
    main()
