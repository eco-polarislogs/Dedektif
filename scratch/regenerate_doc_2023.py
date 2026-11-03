from PIL import Image, ImageDraw, ImageFont
import os

deliller_dir = r"c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\images\towns\sisoren\deliller"
book_cover_path = os.path.join(deliller_dir, "sisoren_book_bg.png")
torn_page_path = r"c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\images\torn_notebook_texture.png"

# Deri kapak tabanı
base = Image.open(book_cover_path).convert("RGBA").resize((900, 650), Image.Resampling.LANCZOS)
page_tex = Image.open(torn_page_path).convert("RGBA")

# Sol ve sağ sayfa kağıtları (açık krem/parşömen kağıt)
left_page = page_tex.resize((380, 560), Image.Resampling.LANCZOS)
right_page = page_tex.resize((380, 560), Image.Resampling.LANCZOS)

# Sayfaları deri kapağın içine yapıştır
base.paste(left_page, (55, 45))
base.paste(right_page, (465, 45))

# Deri orta dikiş (spine)
draw = ImageDraw.Draw(base)
draw.rectangle([435, 35, 465, 615], fill=(35, 20, 10, 255), outline=(15, 8, 4, 255), width=2)
draw.line([(450, 40), (450, 610)], fill=(200, 170, 100, 200), width=2)

fonts = {}
fonts["title"] = ImageFont.truetype(r"C:\Windows\Fonts\timesbd.ttf", 20)
fonts["sub"] = ImageFont.truetype(r"C:\Windows\Fonts\timesi.ttf", 16)
fonts["body"] = ImageFont.truetype(r"C:\Windows\Fonts\times.ttf", 18)
fonts["body_b"] = ImageFont.truetype(r"C:\Windows\Fonts\timesbd.ttf", 18)
fonts["hand"] = ImageFont.truetype(r"C:\Windows\Fonts\segoepr.ttf", 18)
fonts["hand_b"] = ImageFont.truetype(r"C:\Windows\Fonts\segoeprb.ttf", 19)

# Sol Sayfa (Kağıdın üzerine koyu mürekkeple)
draw.text((245, 75), "SİSÖREN KAHVEHANESİ VERESİYE", fill=(30, 20, 10, 255), font=fonts["title"], anchor="mm")
draw.text((245, 105), "1952 Güz Dönemi Hesapları", fill=(90, 50, 20, 255), font=fonts["sub"], anchor="mm")
draw.line([(80, 125), (410, 125)], fill=(120, 90, 60, 200), width=1)

rows = [
    ("Çoban Durmuş:", "120 TL (Ödendi)"),
    ("Tüpçü Şevket:", "310 TL"),
    ("Hurdacı Zehra:", "480 TL"),
    ("Halil Efendi (Maden):", "65.000 TL"),
    ("Sinemacı Nejat:", "250 TL"),
    ("Sahaf Hikmet:", "85 TL")
]
y = 150
for name, val in rows:
    if "Halil" in name:
        draw.text((80, y), name, fill=(180, 20, 20, 255), font=fonts["body_b"])
        draw.text((270, y), val, fill=(180, 20, 20, 255), font=fonts["body_b"])
        draw.line([(75, y+10), (410, y+10)], fill=(200, 20, 20, 255), width=3)
    else:
        draw.text((80, y), name, fill=(20, 15, 10, 255), font=fonts["body"])
        draw.text((270, y), val, fill=(70, 45, 20, 255), font=fonts["body"])
    y += 42

draw.rectangle([80, 450, 410, 540], outline=(180, 30, 30, 220), width=2)
draw.text((245, 495), "KAHVEHANE HACİZLİDİR\nİCRA DAİRESİ", fill=(180, 30, 30, 240), font=fonts["title"], anchor="mm")

# Sağ Sayfa (Kağıdın üzerine İrfan'ın el yazısı)
draw.text((655, 75), "İRFAN'IN KİŞİSEL NOTLARI", fill=(130, 20, 20, 255), font=fonts["title"], anchor="mm")
draw.line([(490, 105), (820, 105)], fill=(120, 90, 60, 200), width=1)

notes = [
    "• Halil kahvehanemi ipotek ettirip",
    "  ocağımı söndürmek istiyor.",
    "• Her gün gelip yüzüme sırıtıyor,",
    "  beni kasabada küçük düşürüyor.",
    "• Sabrım tamamen bitti.",
    "• Altıpatları yağlayıp hazırladım.",
    "• Bu hesap dağ yolunda kanla kapanacak!"
]
y = 135
for n in notes:
    draw.text((490, y), n, fill=(25, 15, 10, 255), font=fonts["hand"])
    y += 36

base.save(os.path.join(deliller_dir, "doc_2023.png"))
print("doc_2023.png regenerated with open paper pages successfully!")
