import os, random, math
from PIL import Image, ImageDraw, ImageFont, ImageFilter

deliller_dir = r"c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\images\towns\sisoren\deliller"
os.makedirs(deliller_dir, exist_ok=True)

# Font hazırlığı
def get_fonts():
    f = {}
    try:
        f["title"] = ImageFont.truetype(r"C:\Windows\Fonts\timesbd.ttf", 26)
        f["title_big"] = ImageFont.truetype(r"C:\Windows\Fonts\timesbd.ttf", 32)
        f["subtitle"] = ImageFont.truetype(r"C:\Windows\Fonts\timesi.ttf", 20)
        f["body"] = ImageFont.truetype(r"C:\Windows\Fonts\times.ttf", 21)
        f["body_b"] = ImageFont.truetype(r"C:\Windows\Fonts\timesbd.ttf", 21)
        f["typewriter"] = ImageFont.truetype(r"C:\Windows\Fonts\cour.ttf", 20)
        f["typewriter_b"] = ImageFont.truetype(r"C:\Windows\Fonts\courbd.ttf", 22)
        f["typewriter_sm"] = ImageFont.truetype(r"C:\Windows\Fonts\cour.ttf", 16)
        f["hand"] = ImageFont.truetype(r"C:\Windows\Fonts\segoepr.ttf", 20)
        f["hand_b"] = ImageFont.truetype(r"C:\Windows\Fonts\segoeprb.ttf", 22)
        f["hand_big"] = ImageFont.truetype(r"C:\Windows\Fonts\segoeprb.ttf", 30)
        f["stencil"] = ImageFont.truetype(r"C:\Windows\Fonts\impact.ttf", 28)
    except:
        d = ImageFont.load_default()
        for k in ["title", "title_big", "subtitle", "body", "body_b", "typewriter", "typewriter_b", "typewriter_sm", "hand", "hand_b", "hand_big", "stencil"]:
            f[k] = d
    return f

fonts = get_fonts()

# -------------------------------------------------------------------------
# DOKU ÜRETECİLERİ (Her belge türü için sıfırdan benzersiz zeminler)
# -------------------------------------------------------------------------

# 1. 2011: Sarı rulo telgraf şeritleri ve resmi telgraf formu
def make_doc_2011():
    # 700x950 ebadında grimsi-mavi soluk resmi telgraf formu
    w, h = 750, 950
    img = Image.new("RGBA", (w, h), (230, 235, 238, 255))
    draw = ImageDraw.Draw(img)
    
    # İnce ızgara kılavuz çizgileri (resmi form deseni)
    for y in range(40, h - 40, 20):
        draw.line([(40, y), (w - 40, y)], fill=(210, 218, 222, 255), width=1)
    for x in range(40, w - 40, 20):
        draw.line([(x, 40), (x, h - 40)], fill=(210, 218, 222, 255), width=1)
        
    # Kenar çift çerçeve
    draw.rectangle([35, 35, w - 35, h - 35], outline=(90, 110, 125, 255), width=3)
    draw.rectangle([42, 42, w - 42, h - 42], outline=(140, 160, 175, 255), width=1)
    
    # Resmi Başlık
    draw.text((w//2, 75), "T.C. POSTA VE TELGRAF UMUM MÜDÜRLÜĞÜ", fill=(30, 50, 65, 255), font=fonts["title"], anchor="mm")
    draw.text((w//2, 110), "SİSÖREN TELGRAF ŞUBESİ — ACELE VE GİZLİ ŞİFRE FORMU", fill=(100, 30, 30, 255), font=fonts["subtitle"], anchor="mm")
    draw.line([(60, 135), (w - 60, 135)], fill=(70, 90, 105, 255), width=2)
    
    # Form Bilgileri Kutuları
    draw.rectangle([60, 150, 360, 220], outline=(100, 120, 135, 255), width=1)
    draw.text((70, 160), "MENŞEİ: Sisören Dağ İstasyonu", fill=(50, 60, 70, 255), font=fonts["typewriter_sm"])
    draw.text((70, 185), "TARİH: 24 Teşrinievvel 1952", fill=(50, 60, 70, 255), font=fonts["typewriter_sm"])
    
    draw.rectangle([380, 150, w - 60, 220], outline=(100, 120, 135, 255), width=1)
    draw.text((390, 160), "HEDEF: Maden Sevkiyat Bürosu", fill=(50, 60, 70, 255), font=fonts["typewriter_sm"])
    draw.text((390, 185), "TESLİM KODU: #TEL-882-SIS", fill=(120, 20, 20, 255), font=fonts["typewriter_b"])

    # ÜZERİNE GERÇEKTEN YAPIŞTIRILMIŞ SARI TELGRAF ŞERİTLERİ (TAPE STRIPS)
    strips = [
        "--. . -.-. .   -.-- .- .-. .. ... ..   --. --- .-.. -.. .",
        "HALIL EFENDI SANTAJ DEFTERINI MADEN",
        "YOLUNDAKI ESKI VAGONA SAKLAMIS STOP",
        "GECE YARISI GOLDE BULUSUP TELLERI",
        "KESECEGIZ JANDARMAYA HABER UCUSAZ STOP"
    ]
    sy = 270
    for s in strips:
        # Sarı kauçuk telgraf bandı dokusu
        draw.rectangle([70, sy - 6, w - 70, sy + 38], fill=(248, 238, 175, 255), outline=(195, 175, 95, 255), width=1)
        # Bant perforasyon/kesik kenarları
        draw.line([(70, sy-6), (70, sy+38)], fill=(160, 140, 70, 255), width=2)
        draw.line([(w-70, sy-6), (w-70, sy+38)], fill=(160, 140, 70, 255), width=2)
        draw.text((90, sy), s, fill=(15, 15, 15, 255), font=fonts["typewriter_b"])
        sy += 65

    # Alt Kısım: Rüstem'in Mors Çözüm Notu ve İtirafı
    draw.rectangle([60, 620, w - 60, 830], fill=(240, 243, 245, 255), outline=(140, 160, 175, 255), width=1)
    draw.text((80, 635), "TELGRAFÇI RÜSTEM'İN ŞİFRE ÇÖZÜM NOTU:", fill=(140, 30, 20, 255), font=fonts["body_b"])
    draw.text((80, 675), "Mors şeritlerini Halil'in çöpünden topladım. Kurban,", fill=(30, 30, 30, 255), font=fonts["hand"])
    draw.text((80, 715), "şantaj belgeleriyle jandarmaya kaçacaktı. Tellere sabotaj", fill=(30, 30, 30, 255), font=fonts["hand"])
    draw.text((80, 755), "yapıp bağlantıyı bizzat kopardım. Bu sır açığa çıkamaz!", fill=(160, 20, 20, 255), font=fonts["hand_b"])
    
    # Resmi Mühür Kaşesi (Mor mürekkep daire kaşe)
    draw.ellipse([520, 700, 660, 840], outline=(110, 30, 130, 220), width=3)
    draw.ellipse([530, 710, 650, 830], outline=(110, 30, 130, 180), width=1)
    draw.text((590, 750), "SİSÖREN\nTELGRAF\n1952", fill=(110, 30, 130, 220), font=fonts["body_b"], anchor="mm")
    
    img.save(os.path.join(deliller_dir, "doc_2011.png"))
    print("make_doc_2011 done")

# 2. 2015: Tehdit Mesajı Taslağı (Kırışık, sarı saman kağıdı, tükenmez/kurşun kalem karalamaları)
def make_doc_2015():
    w, h = 750, 950
    # Sarı saman kağıdı tabanı
    img = Image.new("RGBA", (w, h), (238, 222, 178, 255))
    draw = ImageDraw.Draw(img)
    
    # Saman kağıdı lif dokusu
    for _ in range(600):
        rx = random.randint(10, w - 10)
        ry = random.randint(10, h - 10)
        draw.line([(rx, ry), (rx + random.randint(-4, 4), ry + random.randint(2, 6))], fill=(215, 195, 150, 180), width=1)
        
    # Kenar yıpranmaları ve koyu gölge sınır
    draw.rectangle([25, 25, w - 25, h - 25], outline=(170, 140, 95, 200), width=2)
    
    draw.text((w//2, 85), "KARALAMA KAĞIDI & İSİMSİZ TEHDİTNAME", fill=(130, 30, 20, 255), font=fonts["title_big"], anchor="mm")
    draw.line([(80, 115), (w - 80, 115)], fill=(130, 30, 20, 180), width=2)
    
    draw.text((80, 160), "Halil Efendi,", fill=(20, 20, 20, 255), font=fonts["hand_big"])
    
    lines = [
        "Bu dağda telgraf tellerinden geçen her ses, her fısıltı",
        "benim kulağımda biter. Madendeki kaçak vagonları,",
        "madenci ailelerinden sülük gibi emdiğin paraları ve",
        "vilayete yalan beyanlarını bildiğimi inkâr etmeye kalkma.",
        "",
        "Elimde vilayet savcısına gidecek resmi şeritler duruyor.",
        "Ya bu akşam telgrafhaneye 50.000 Lira nakit bırakırsın,",
        "ya da yarın sabah jandarma cipi kapına dayanır.",
        "",
        "Ecelin o incecik telgraf tellerinin ucunda sallanıyor!",
        "Bu sana son ihtarımdır..."
    ]
    y = 230
    for l in lines:
        draw.text((80, y), l, fill=(25, 20, 15, 255), font=fonts["hand_b"])
        y += 44
        
    # Gönderici kısmı karalanmış / jiletlenmiş alan
    draw.rectangle([80, 750, 340, 810], fill=(30, 25, 20, 255))
    for _ in range(40):
        draw.line([(80 + random.randint(0, 260), 750), (80 + random.randint(0, 260), 810)], fill=(200, 180, 140, 200), width=2)
    draw.text((80, 825), "[İmza ve Gönderici adı jiletle kazınarak yakılmış]", fill=(150, 30, 20, 255), font=fonts["subtitle"])

    img.save(os.path.join(deliller_dir, "doc_2015.png"))
    print("make_doc_2015 done")

# 4. 2032: Sinema Afişi Arka Yüzü (Kalın gri karton, mor damgalar, soluk film baskısı)
def make_doc_2032():
    w, h = 750, 950
    # Ağır gri/bej matbaacı karton rengi
    img = Image.new("RGBA", (w, h), (218, 214, 202, 255))
    draw = ImageDraw.Draw(img)
    
    # Karton lifleri
    for _ in range(400):
        x = random.randint(20, w - 20)
        y = random.randint(20, h - 20)
        draw.point((x, y), fill=(180, 175, 160, 255))
        
    draw.rectangle([30, 30, w - 30, h - 30], outline=(140, 135, 120, 255), width=3)
    
    # Sinema damgası ve ters film başlığı izi
    draw.text((w//2, 80), "SİSÖREN BELEDİYE SİNEMASI", fill=(45, 55, 65, 255), font=fonts["title_big"], anchor="mm")
    draw.text((w//2, 120), "\"GECENİN GÖLGELERİ\" (1949 SEZONU) — AFİŞ ARKA YÜZÜ", fill=(80, 40, 90, 255), font=fonts["subtitle"], anchor="mm")
    draw.line([(60, 145), (w - 60, 145)], fill=(120, 110, 100, 255), width=2)
    
    # Mor mürekkepli şantaj kutusu
    draw.rectangle([70, 180, w - 70, 480], fill=(230, 226, 215, 255), outline=(110, 40, 120, 255), width=2)
    draw.text((90, 200), "HALİL EFENDİ'DEN NEJAT'A MOR MÜREKKEPLİ MEKTUP:", fill=(110, 30, 120, 255), font=fonts["body_b"])
    
    t_lines = [
        "\"Nejat Efendi,",
        "Projeksiyon odasındaki metal film kutularında ne",
        "sakladığını gözlerimle gördüm. Sisin içinde çevirdiğin",
        "kaçak işler o 35 milimetre makaralara saniye saniye yansıdı.",
        "Ya 100.000 Lirayı getirip ellerinle teslim edersin,",
        "ya da yarınki matinede jandarma seni localardan toplar!\""
    ]
    ty = 240
    for tl in t_lines:
        draw.text((90, ty), tl, fill=(90, 20, 100, 255), font=fonts["hand_b"])
        ty += 38

    # Nejat'ın sinematik intikam karalaması
    draw.rectangle([70, 520, w - 70, 780], fill=(210, 205, 195, 255), outline=(150, 40, 40, 255), width=2)
    draw.text((90, 540), "[Nejat'ın aceleyle kurşun kalemle aldığı cinayet planı]:", fill=(160, 20, 20, 255), font=fonts["body_b"])
    
    n_lines = [
        "• Bu gece saat 02:00 son seans bittiğinde randevu verdim.",
        "• Makine dairesine tek başına gelecek.",
        "• Ağır film bobini kafasına indiğinde perde kapanacak.",
        "• Dönen çelik dişliler arbede izini tamamen yok edecek!",
        "• Artık kimse sinemamı elimden alamaz."
    ]
    ny = 580
    for nl in n_lines:
        draw.text((90, ny), nl, fill=(35, 30, 25, 255), font=fonts["hand"])
        ny += 36

    img.save(os.path.join(deliller_dir, "doc_2032.png"))
    print("make_doc_2032 done")

# 5. 2042: Bakkal Cemile'nin Yırtık Veresiye Sayfası (Kareli/çizgili esnaf defteri, yırtılmış sağ alt kenar)
def make_doc_2042():
    w, h = 750, 950
    # Açık sarı kareli esnaf defteri kağıdı
    img = Image.new("RGBA", (w, h), (252, 248, 230, 255))
    draw = ImageDraw.Draw(img)
    
    # Mavi soluk kareli çizgiler
    for y in range(40, h - 40, 24):
        draw.line([(40, y), (w - 40, y)], fill=(215, 228, 242, 255), width=1)
    for x in range(40, w - 40, 24):
        draw.line([(x, 40), (x, h - 40)], fill=(215, 228, 242, 255), width=1)
        
    # Kırmızı dikey sol marj çizgisi
    draw.line([(100, 30), (100, h - 30)], fill=(225, 120, 120, 255), width=2)
    
    draw.text((w//2 + 20, 75), "SİSÖREN DAĞ BAKKALI — VERESİYE SAYFASI #412", fill=(40, 45, 55, 255), font=fonts["title"], anchor="mm")
    draw.text((120, 115), "MÜŞTERİ: Madenci Halil Efendi | TARİH: 18 Ekim", fill=(120, 30, 20, 255), font=fonts["typewriter_b"])
    draw.line([(110, 145), (w - 50, 145)], fill=(60, 70, 80, 255), width=2)
    
    items = [
        ("10 Çuval Beyaz Un:", "320 TL"),
        ("5 Kilo İthal Rize Çayı:", "180 TL"),
        ("Gazyağı ve İspirto Tenekesi:", "140 TL"),
        ("Dükkân Rehin Faizi (Ekim):", "15.000 TL"),
        ("TOPLAM ÖDENMEYEN BORÇ:", "40.640 TL")
    ]
    iy = 175
    for item, price in items:
        draw.text((120, iy), item, fill=(30, 30, 30, 255), font=fonts["typewriter"])
        draw.text((540, iy), price, fill=(160, 20, 20, 255) if "TOPLAM" in item else (40, 40, 40, 255), font=fonts["typewriter_b"])
        iy += 45

    # Ortada Yırtık Alacak Notu (Maktulün tehdidi)
    draw.rectangle([110, 410, w - 60, 560], fill=(255, 242, 242, 255), outline=(200, 40, 40, 255), width=2)
    draw.text((130, 425), "[YIRTILMIŞ ALACAK TEHDİDİ]:", fill=(180, 20, 20, 255), font=fonts["body_b"])
    draw.text((130, 465), "\"Cemile, borcu cuma gününe kadar kapatmazsan", fill=(40, 20, 20, 255), font=fonts["hand"])
    draw.text((130, 505), "dükkânı icrayla alıyorum, seni de sokağa atarım!\" - Halil", fill=(40, 20, 20, 255), font=fonts["hand_b"])

    # Cemile'nin Kırmızı Mürekkeple Düşülen Zehir İtirafı
    draw.text((120, 610), "[Cemile'nin Titrek Kırmızı Mürekkep Notu]:", fill=(180, 20, 20, 255), font=fonts["body_b"])
    draw.text((120, 655), "Dükkânımı elimden alıp beni aç bırakamazsın...", fill=(180, 20, 20, 255), font=fonts["hand"])
    draw.text((120, 700), "Kilerdeki fare zehri bu akşam ayırdığım taze kaşara", fill=(180, 20, 20, 255), font=fonts["hand_b"])
    draw.text((120, 745), "şırıngayla karışacak. Kalbin durduğunda helallik iste!", fill=(180, 20, 20, 255), font=fonts["hand_big"])

    # Sağ alt köşede yırtık kağıt efekti (üçgen gri gölge)
    draw.polygon([(w - 180, h), (w, h - 180), (w, h)], fill=(40, 35, 30, 255))
    draw.line([(w - 180, h), (w, h - 180)], fill=(120, 100, 80, 255), width=3)
    draw.text((w - 120, h - 80), "[YIRTILMIŞ]", fill=(200, 180, 160, 255), font=fonts["typewriter_sm"], rotation=45 if hasattr(draw, 'rotation') else 0)

    img.save(os.path.join(deliller_dir, "doc_2042.png"))
    print("make_doc_2042 done")

# 6. 2051: İçi Oyulmuş Ansiklopedi & Harita (Tozlu parşömen, elle çizilmiş maden galerisi krokisi)
def make_doc_2051():
    w, h = 750, 950
    # Antika sararmış parşömen dokusu
    img = Image.new("RGBA", (w, h), (242, 230, 205, 255))
    draw = ImageDraw.Draw(img)
    
    # Parşömen lekeleri
    for _ in range(15):
        cx = random.randint(50, w - 50)
        cy = random.randint(50, h - 50)
        cr = random.randint(20, 80)
        draw.ellipse([cx - cr, cy - cr, cx + cr, cy + cr], fill=(225, 210, 180, 80))
        
    draw.rectangle([30, 30, w - 30, h - 30], outline=(120, 90, 50, 255), width=3)
    
    draw.text((w//2, 75), "BÜYÜK OSMANLI TARİHİ — CİLT IV", fill=(45, 35, 20, 255), font=fonts["title_big"], anchor="mm")
    draw.text((w//2, 115), "OYULMUŞ SAYFA ARASINA GİZLENEN KADİM KROKİ", fill=(120, 40, 20, 255), font=fonts["subtitle"], anchor="mm")
    draw.line([(60, 140), (w - 60, 140)], fill=(120, 90, 50, 255), width=2)
    
    # Harita Çizim Alanı (Teknik çizim gibi kahverengi mürekkeple)
    draw.rectangle([70, 160, w - 70, 480], fill=(248, 240, 220, 255), outline=(70, 50, 30, 255), width=2)
    draw.text((w//2, 190), "SİSÖREN ESKİ MADEN GALERİSİ & KAYIP ALTIN SAKLANTISI", fill=(110, 30, 15, 255), font=fonts["body_b"], anchor="mm")
    
    # Tünel Krokisi Çizgileri
    draw.line([(120, 310), (630, 310)], fill=(70, 45, 25, 255), width=4)
    draw.line([(375, 310), (375, 430)], fill=(70, 45, 25, 255), width=4)
    draw.line([(220, 310), (220, 250)], fill=(70, 45, 25, 255), width=3)
    draw.line([(530, 310), (530, 250)], fill=(70, 45, 25, 255), width=3)
    
    draw.rectangle([345, 395, 405, 455], fill=(210, 170, 70, 255), outline=(90, 60, 20, 255), width=2)
    draw.text((375, 425), "[SANDIK]", fill=(50, 30, 10, 255), font=fonts["typewriter_b"], anchor="mm")
    draw.text((220, 235), "1. Kuzey Kuyusu", fill=(80, 50, 30, 255), font=fonts["typewriter_sm"], anchor="mm")
    draw.text((530, 235), "Çıkış Galerisi (Sisli)", fill=(80, 50, 30, 255), font=fonts["typewriter_sm"], anchor="mm")

    # Sahaf Hikmet'in İtiraf Notu
    draw.rectangle([70, 520, w - 70, 800], fill=(235, 222, 195, 255), outline=(130, 80, 40, 255), width=2)
    draw.text((90, 540), "[Sahaf Hikmet'in Kaligrafi Kalemiyle Düştüğü Not]:", fill=(140, 30, 20, 255), font=fonts["body_b"])
    draw.text((90, 585), "Halil bu haritayı çalmak için sahafı basacağını söyledi.", fill=(30, 25, 20, 255), font=fonts["hand"])
    draw.text((90, 630), "Nesillerdir sakladığım ecdat mirasını o haramiye teslim", fill=(30, 25, 20, 255), font=fonts["hand"])
    draw.text((90, 675), "etmeyeceğim. Dükkâna adım atarsa bu ağır cilt kafasını ezecek!", fill=(160, 20, 20, 255), font=fonts["hand_b"])
    draw.text((90, 725), "Cilt IV sayfaları kanla boyanacak ama sır güvende kalacak.", fill=(30, 25, 20, 255), font=fonts["hand"])

    img.save(os.path.join(deliller_dir, "doc_2051.png"))
    print("make_doc_2051 done")

# 7. 2061: Asitle Silinmiş Köy Nüfus Kütüğü (Eski yeşil ciltli resmi kütük sayfası, asitle yanmış kararmış delik)
def make_doc_2061():
    w, h = 750, 950
    # Resmi soluk yeşil devlet matbu kağıdı
    img = Image.new("RGBA", (w, h), (235, 242, 232, 255))
    draw = ImageDraw.Draw(img)
    
    # Kütük Tablo Çizgileri
    for y in range(50, h - 50, 30):
        draw.line([(50, y), (w - 50, y)], fill=(185, 205, 185, 255), width=1)
    draw.line([(180, 50), (180, h - 50)], fill=(150, 180, 150, 255), width=2)
    draw.line([(420, 50), (420, h - 50)], fill=(150, 180, 150, 255), width=2)

    # Başlık
    draw.text((w//2, 75), "T.C. SİSÖREN KÖYÜ İHTİYAR HEYETİ", fill=(20, 45, 25, 255), font=fonts["title_big"], anchor="mm")
    draw.text((w//2, 115), "RESMİ NÜFUS VE ARAZİ KÜTÜK DEFTERİ — HANE NO: 29", fill=(120, 30, 20, 255), font=fonts["subtitle"], anchor="mm")
    draw.line([(50, 140), (w - 50, 140)], fill=(20, 50, 25, 255), width=2)

    draw.text((60, 160), "HAK SAHİBİ:", fill=(30, 50, 35, 255), font=fonts["typewriter_b"])
    draw.text((200, 160), "Halil Efendi (Maden Müteahhidi)", fill=(30, 30, 30, 255), font=fonts["typewriter"])

    # ASİTLE YANMIŞ, KARARMIŞ VE KİMYASAL LEKELİ ALAN (Büyük siyah-kahve krater deliği)
    draw.rectangle([60, 230, w - 60, 480], fill=(30, 25, 20, 255), outline=(15, 12, 10, 255), width=3)
    # Asit sıçrama damlaları
    for _ in range(80):
        ax = random.randint(65, w - 65)
        ay = random.randint(235, 475)
        ar = random.randint(3, 15)
        draw.ellipse([ax - ar, ay - ar, ax + ar, ay + ar], fill=(70, 45, 25, 200))
        
    draw.text((w//2, 290), "[KİMYASAL SÜLFÜRİK ASİTLE YAKILMIŞ BÖLÜM]", fill=(220, 70, 60, 255), font=fonts["title"], anchor="mm")
    draw.text((w//2, 340), "Maktul Halil Efendi'nin 840 Dönüm Yayla Mera Payı...", fill=(160, 160, 160, 255), font=fonts["typewriter"], anchor="mm")
    draw.text((w//2, 380), "-> Tamamı Köy Muhtarı Meliha Hanım zimmetine aktarılmıştır.", fill=(240, 200, 90, 255), font=fonts["typewriter_b"], anchor="mm")
    draw.text((w//2, 420), "Resmi Kasaba Mührü ile tasdik kılınmıştır.", fill=(180, 180, 180, 255), font=fonts["typewriter"], anchor="mm")

    # Dedektif İnceleme & Muhtar Tehdit Notu
    draw.rectangle([60, 520, w - 60, 780], fill=(245, 250, 245, 255), outline=(140, 170, 140, 255), width=2)
    draw.text((80, 540), "[Adli Belge İnceleme Şubesi Rapor Notu]:", fill=(140, 30, 20, 255), font=fonts["body_b"])
    draw.text((80, 585), "Asit kasten dökülmüş, maktulün hak iddiaları yakılarak yok edilmiş.", fill=(30, 35, 30, 255), font=fonts["hand"])
    draw.text((80, 630), "Halil bu tahrifatı bulup savcılığa gideceğini söyleyince,", fill=(30, 35, 30, 255), font=fonts["hand"])
    draw.text((80, 675), "Muhtar Meliha Hanım kütüğün kenarına bizzat not düşmüş:", fill=(30, 35, 30, 255), font=fonts["hand"])
    draw.text((80, 720), "\"Halil yarın savcıya gidecek nefes bulamayacak!\"", fill=(180, 20, 20, 255), font=fonts["hand_big"])

    img.save(os.path.join(deliller_dir, "doc_2061.png"))
    print("make_doc_2061 done")

# 8. 2085: Ahırdaki Saplı Bıçak Notu (Kaba kahverengi çuval kağıdı / kraft kağıt, ortasında hançer deliği)
def make_doc_2085():
    w, h = 750, 950
    # Koyu kraft / çuval kağıdı tabanı
    img = Image.new("RGBA", (w, h), (195, 160, 115, 255))
    draw = ImageDraw.Draw(img)
    
    # Kaba lifler
    for _ in range(800):
        kx = random.randint(15, w - 15)
        ky = random.randint(15, h - 15)
        draw.line([(kx, ky), (kx + random.randint(-5, 5), ky + random.randint(2, 8))], fill=(160, 130, 90, 200), width=1)
        
    draw.rectangle([25, 25, w - 25, h - 25], outline=(120, 90, 50, 255), width=3)
    
    # Ortadaki Bıçak / Hançer Yarası (Dikey yırtık delik)
    draw.polygon([(365, 80), (385, 80), (395, 200), (375, 240), (355, 200)], fill=(20, 12, 8, 255))
    # Bıçak deliği etrafında koyu kırmızı kan lekesi
    draw.ellipse([340, 150, 410, 220], outline=(150, 20, 20, 220), width=3)
    draw.text((w//2, 60), "TAHTA DİREĞE SAPLANMIŞ KANLI KAĞIT", fill=(140, 20, 20, 255), font=fonts["title_big"], anchor="mm")
    
    draw.text((70, 250), "HALİL HARAMİSİ!", fill=(160, 20, 20, 255), font=fonts["hand_big"])
    
    durmus_lines = [
        "Sürümün içtiği dağ deresine maden zehri dökmeyi bırak",
        "dedik, oralı olmadın. Dün gece üç gebe koyunum kan kusarak",
        "ahırda can verdi. O hayvanların tek bir tırnağı senin",
        "o mundar canından bin kat daha kıymetlidir!",
        "",
        "Dağ yolunda karşıma çıktığın an, o paslı nalbant",
        "çivisini gözünü kırpmadan kalbine çakacağım!",
        "Bu dağın adaleti namludan da çividen de ağırdır!"
    ]
    dy = 320
    for dl in durmus_lines:
        draw.text((70, dy), dl, fill=(25, 18, 12, 255), font=fonts["hand_b"])
        dy += 44
        
    draw.text((420, 750), "— Çoban Durmuş", fill=(140, 20, 20, 255), font=fonts["hand_big"])

    img.save(os.path.join(deliller_dir, "doc_2085.png"))
    print("make_doc_2085 done")

# 9. 2104: Hurdacı Zehra'nın Kanlı Tartım Fişi (Mavi matbu kantar tartım makbuzu + gerçek koyu kan damlası)
def make_doc_2104():
    w, h = 750, 950
    # Soluk mavi-gri endüstriyel kantar fişi kağıdı
    img = Image.new("RGBA", (w, h), (225, 230, 235, 255))
    draw = ImageDraw.Draw(img)
    
    draw.rectangle([35, 35, w - 35, h - 35], outline=(70, 90, 110, 255), width=2)
    
    # Makbuz Başlığı
    draw.text((w//2, 75), "SİSÖREN SANAYİ & HURDA KANTAR İŞLETMESİ", fill=(30, 50, 70, 255), font=fonts["title"], anchor="mm")
    draw.text((w//2, 110), "RESMİ KANTAR TARTIM MAKBUZU — VARDİYA 3", fill=(100, 30, 30, 255), font=fonts["subtitle"], anchor="mm")
    draw.line([(55, 135), (w - 55, 135)], fill=(70, 90, 110, 255), width=2)
    
    # Tablo formatı
    draw.text((70, 160), "FİŞ NO: #8921", fill=(30, 40, 50, 255), font=fonts["typewriter_b"])
    draw.text((380, 160), "TARTIM SAATİ: 02:18 (GECE YARISI)", fill=(160, 20, 20, 255), font=fonts["typewriter_b"])
    draw.text((70, 200), "GELEN ARAÇ: Plakasız El Arabası", fill=(40, 50, 60, 255), font=fonts["typewriter"])
    draw.text((380, 200), "DARA: 420 KG ÇELİK RAY & HURDA", fill=(40, 50, 60, 255), font=fonts["typewriter"])

    # Büyük Koyu Kan Lekesi ve Parmak Sürtünmesi
    draw.ellipse([460, 240, 610, 390], fill=(130, 20, 20, 240))
    draw.ellipse([480, 260, 590, 370], fill=(165, 30, 30, 255))
    draw.line([(500, 380), (560, 460)], fill=(120, 20, 20, 200), width=8) # Kan damlama izi
    draw.text((535, 315), "TAZE KAN", fill=(250, 210, 210, 255), font=fonts["body_b"], anchor="mm")

    # Zehra'nın Arka Yüze Düştüğü Kurşun Kalem İtirafı
    draw.rectangle([65, 480, w - 65, 780], fill=(235, 240, 245, 255), outline=(90, 110, 130, 255), width=2)
    draw.text((85, 505), "[Fişin Arka Yüzüne Düşülen Kurşun Kalem Notu]:", fill=(140, 30, 20, 255), font=fonts["body_b"])
    draw.text((85, 555), "\"Halil gece yarısı dükkâna geldi. Çalıntı rayların", fill=(30, 35, 40, 255), font=fonts["hand"])
    draw.text((85, 600), "ve eritilen altınların sus payını istedi. Pres makinesinin", fill=(30, 35, 40, 255), font=fonts["hand"])
    draw.text((85, 645), "çelik kapağını arkasından kilitledim. Çığlıkları presin", fill=(30, 35, 40, 255), font=fonts["hand_b"])
    draw.text((85, 690), "gıcırtısında kayboldu. Artık o da bir hurda yığını!\"", fill=(160, 20, 20, 255), font=fonts["hand_big"])

    draw.text((450, 810), "— Hurdacı Zehra", fill=(100, 20, 20, 255), font=fonts["hand_big"])

    img.save(os.path.join(deliller_dir, "doc_2104.png"))
    print("make_doc_2104 done")

# 10. 2125: Şöminede Kısmen Yanmış Sahte Köy Satış Tapusu (Kömürleşmiş kenarlar, kızıl yanık lekeleri)
def make_doc_2125():
    w, h = 750, 950
    # Eski yanık sarı tapu kağıdı
    img = Image.new("RGBA", (w, h), (245, 235, 215, 255))
    draw = ImageDraw.Draw(img)
    
    # Üst ve sol çerçeve
    draw.rectangle([35, 35, w - 35, h - 35], outline=(140, 80, 40, 255), width=3)
    
    draw.text((w//2, 75), "ARAZİ SATIŞ VE MÜLKİYET FERAGAT SENEDİ", fill=(130, 25, 20, 255), font=fonts["title_big"], anchor="mm")
    draw.text((w//2, 115), "SİSÖREN DAĞ KÖYÜ TOPLU SATIŞ VE TAHLİYE PROTOKOLÜ", fill=(60, 40, 20, 255), font=fonts["subtitle"], anchor="mm")
    draw.line([(55, 140), (w - 55, 140)], fill=(130, 25, 20, 255), width=2)
    
    draw.text((70, 165), "SATICI VEKİLİ: Halil Efendi (Yetkisiz Mümessil)", fill=(30, 30, 30, 255), font=fonts["typewriter_b"])
    draw.text((70, 195), "ALICI ŞİRKET: Doğu Madencilik ve Taşocağı T.A.Ş.", fill=(30, 30, 30, 255), font=fonts["typewriter"])
    draw.text((70, 225), "MEVKİİ: Sisören Yaylası ve 40 Adet Köy Parseli", fill=(30, 30, 30, 255), font=fonts["typewriter"])

    # YANMIŞ / KÖMÜRLEŞMİŞ ŞÖMİNE ATEŞİ ALANI (Alt ve Sağ kenar yanmış siyah tabaka)
    draw.rectangle([55, 280, w - 55, 520], fill=(35, 22, 15, 255), outline=(20, 10, 5, 255), width=3)
    # Alev rengi yanık sınır
    for _ in range(60):
        fx = random.randint(60, w - 60)
        fy = random.randint(285, 515)
        fr = random.randint(4, 20)
        draw.ellipse([fx - fr, fy - fr, fx + fr, fy + fr], fill=(180, 70, 20, 180))
        
    draw.text((w//2, 335), "[ŞÖMİNEDE KISMEN YANMIŞ VE KÜL OLMUŞ MADDE]", fill=(240, 140, 50, 255), font=fonts["title"], anchor="mm")
    draw.text((w//2, 385), "Köylüler evlerinden derhal tahliye edilecek...", fill=(180, 180, 180, 255), font=fonts["typewriter"], anchor="mm")
    draw.text((w//2, 425), "ve tüm su kaynakları maden işletmesine devredilecektir.", fill=(220, 190, 150, 255), font=fonts["typewriter"], anchor="mm")
    draw.text((w//2, 465), "[SAHTE İMZA — İTİRAZLI EVRAK]", fill=(230, 60, 60, 255), font=fonts["typewriter_b"], anchor="mm")

    # Hatice Nine'nin El Notu
    draw.rectangle([55, 560, w - 55, 820], fill=(250, 242, 230, 255), outline=(150, 100, 50, 255), width=2)
    draw.text((75, 585), "[Hatice Nine'nin Titrek ve Kararlı El Yazısı]:", fill=(140, 30, 20, 255), font=fonts["body_b"])
    draw.text((75, 630), "Bu sahte tapuyu ateşe attım ama Halil vazgeçmedi.", fill=(35, 25, 20, 255), font=fonts["hand"])
    draw.text((75, 675), "Toprağımızı bu şirkete peşkeş çekmesine izin vermem.", fill=(35, 25, 20, 255), font=fonts["hand"])
    draw.text((75, 720), "Gerekirse ormanın en zehirli kara mantarları kaynar,", fill=(180, 20, 20, 255), font=fonts["hand_b"])
    draw.text((75, 765), "köyümüzün suyuna dokunan kendi zehrinde boğulur!", fill=(180, 20, 20, 255), font=fonts["hand_big"])

    img.save(os.path.join(deliller_dir, "doc_2125.png"))
    print("make_doc_2125 done")

# Çalıştır
make_doc_2011()
make_doc_2015()
make_doc_2032()
make_doc_2042()
make_doc_2051()
make_doc_2061()
make_doc_2085()
make_doc_2104()
make_doc_2125()
print("All 9 distinct texture documents generated perfectly!")

