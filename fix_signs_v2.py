"""
Sisören Harita Tabela Düzelticisi
Orijinal temiz wwwroot/sisoren_base.jpg üzerinden tabelaları Türkçe olarak kusursuz yerleştirir.
"""
from PIL import Image, ImageDraw, ImageFont
import os

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
INPUT_PATH = os.path.join(BASE_DIR, "wwwroot", "sisoren_base.jpg")
OUTPUT_PATH = os.path.join(BASE_DIR, "wwwroot", "images", "towns", "sisoren", "sisoren_map_v2.png")

img = Image.open(INPUT_PATH).convert("RGBA")
W, H = img.size

draw = ImageDraw.Draw(img)

def get_bold_font(size):
    font_paths = [
        "C:/Windows/Fonts/segoeuib.ttf",
        "C:/Windows/Fonts/arialbd.ttf",
        "C:/Windows/Fonts/calibrib.ttf",
        "C:/Windows/Fonts/segoeui.ttf",
    ]
    for fp in font_paths:
        if os.path.exists(fp):
            return ImageFont.truetype(fp, size)
    return ImageFont.load_default()

def draw_sign(text, cx, cy, cw, ch, sx, sy, sw, sh, style='wooden', text_color='#F0E4D0', font_size=None):
    # Çevredeki renkle eski bozuk yazıyı ört
    sample_x = max(0, min(cx - 3, W-1))
    sample_y = max(0, min(cy - 3, H-1))
    nearby_color = img.getpixel((sample_x, sample_y))
    if len(nearby_color) == 4:
        nearby_color = nearby_color[:3]
    cover_color = tuple(max(0, c - 15) for c in nearby_color)
    draw.rectangle([cx, cy, cx+cw, cy+ch], fill=cover_color + (255,))

    # Tabela çerçevesi ve arka planı
    if style == 'marquee':
        draw.rectangle([sx-2, sy-2, sx+sw+2, sy+sh+2], fill=(21, 18, 16, 255))
        draw.rectangle([sx, sy, sx+sw, sy+sh], fill=(30, 24, 14, 255))
        draw.rectangle([sx, sy, sx+sw, sy+sh], outline=(212, 160, 23, 255), width=2)
        text_color = '#e8b810'
    elif style == 'stone':
        draw.rectangle([sx-2, sy-2, sx+sw+2, sy+sh+2], fill=(45, 45, 45, 255))
        draw.rectangle([sx, sy, sx+sw, sy+sh], fill=(62, 62, 62, 255))
        draw.rectangle([sx, sy, sx+sw, sy+sh], outline=(80, 80, 80, 255), width=1)
        text_color = '#DDD8C8'
    elif style == 'rustic':
        draw.rectangle([sx-2, sy-2, sx+sw+2, sy+sh+2], fill=(40, 22, 8, 255))
        draw.rectangle([sx, sy, sx+sw, sy+sh], fill=(58, 34, 18, 255))
        draw.rectangle([sx, sy, sx+sw, sy+sh], outline=(90, 58, 30, 255), width=1)
    elif style == 'hanging':
        draw.rectangle([sx-3, sy-3, sx+sw+3, sy+sh+3], fill=(61, 42, 20, 255))
        draw.rectangle([sx, sy, sx+sw, sy+sh], fill=(196, 176, 138, 255))
        draw.rectangle([sx-1, sy-1, sx+sw+1, sy+sh+1], outline=(122, 104, 64, 255), width=1)
        text_color = '#2a1808'
    else:
        # Ahşap tabela
        draw.rectangle([sx-2, sy-2, sx+sw+2, sy+sh+2], fill=(32, 16, 8, 255))
        draw.rectangle([sx, sy, sx+sw, sy+sh], fill=(44, 26, 12, 255))
        draw.rectangle([sx, sy, sx+sw, sy+sh], outline=(106, 80, 48, 255), width=1)

    fs = font_size or min(int(sh * 0.65), int(sw / (len(text) * 0.62)))
    font = get_bold_font(fs)
    
    bbox = draw.textbbox((0, 0), text, font=font)
    text_w = bbox[2] - bbox[0]
    text_h = bbox[3] - bbox[1]
    text_x = sx + (sw - text_w) // 2
    text_y = sy + (sh - text_h) // 2 - 1
    
    # Gölge
    if style == 'marquee':
        draw.text((text_x + 1, text_y + 1), text, fill=(0, 0, 0, 180), font=font)
    elif style == 'hanging':
        draw.text((text_x + 1, text_y + 1), text, fill=(255, 255, 255, 80), font=font)
    else:
        draw.text((text_x + 1, text_y + 1), text, fill=(0, 0, 0, 140), font=font)
    
    draw.text((text_x, text_y), text, fill=text_color, font=font)

# 1. SİSÖREN asılı tabela - sol üst köşe
draw_sign('SİSÖREN',
    22, 18, 170, 60,
    30, 25, 115, 40,
    style='hanging', font_size=17)

# 2. TELGRAFHANE - üst sol bina
draw_sign('TELGRAFHANE',
    420, 68, 175, 35,
    425, 72, 165, 28,
    style='wooden', text_color='#E8DCC8', font_size=13)

# 3. KAHVEHANE - üst orta bina
draw_sign('KAHVEHANE',
    595, 44, 170, 36,
    600, 48, 160, 28,
    style='wooden', text_color='#E8DCC8', font_size=14)

# 4. SİNEMA - sağ üst ışıklı marquee
draw_sign('SİNEMA',
    848, 100, 145, 40,
    852, 104, 136, 32,
    style='marquee', font_size=18)

# 5. BAKKAL - merkez ahşap dükkan
draw_sign('BAKKAL',
    484, 278, 125, 30,
    488, 282, 116, 24,
    style='wooden', text_color='#E8DCC8', font_size=13)

# 6. MUHTARLIK - merkez gri taş bina
draw_sign('MUHTARLIK',
    672, 262, 155, 32,
    676, 266, 148, 26,
    style='stone', text_color='#DDD8C8', font_size=13)

# 7. SAHAF - merkez sol sahaf
draw_sign('SAHAF',
    424, 388, 100, 28,
    428, 392, 92, 22,
    style='wooden', text_color='#E8DCC8', font_size=12)

# 8. TÜTÜNCÜ - sağ orta tütüncü
draw_sign('TÜTÜNCÜ',
    794, 362, 132, 30,
    798, 366, 124, 24,
    style='wooden', text_color='#E8DCC8', font_size=13)

# 9. AHIR - alt sol ahır
draw_sign('AHIR',
    248, 594, 95, 28,
    252, 598, 85, 22,
    style='rustic', text_color='#fde68a', font_size=14)

# 10. TÜPÇÜ - alt orta tüpçü
draw_sign('TÜPÇÜ',
    612, 616, 108, 28,
    616, 620, 100, 22,
    style='wooden', text_color='#E0E0E0', font_size=13)

# 11. HURDACI - alt sağ hurdacı
draw_sign('HURDACI',
    738, 666, 120, 28,
    742, 670, 112, 22,
    style='rustic', text_color='#D8C8B8', font_size=12)

# Kaydet
output_rgb = img.convert("RGB")
output_rgb.save(OUTPUT_PATH, "PNG", quality=95)
print("SUCCESS: sisoren_base.jpg uzerinden temiz Turkce tabelali sisoren_map_v2.png basariyla olusturuldu!")
