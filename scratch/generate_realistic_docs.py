import os
from PIL import Image, ImageDraw, ImageFont, ImageFilter

deliller_dir = r"c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\images\towns\sisoren\deliller"
old_paper_path = os.path.join(deliller_dir, "sisoren_old_paper_bg.png")
pres_paper_path = os.path.join(deliller_dir, "sisoren_prescription_bg.png")
book_bg_path = os.path.join(deliller_dir, "sisoren_book_bg.png")

# Font yükleme fonksiyonu
def get_fonts():
    font_paths = [
        r"C:\Windows\Fonts\times.ttf",
        r"C:\Windows\Fonts\cour.ttf",
        r"C:\Windows\Fonts\arial.ttf",
        r"C:\Windows\Fonts\georgia.ttf",
    ]
    fonts = {}
    try:
        fonts["title"] = ImageFont.truetype(r"C:\Windows\Fonts\timesbd.ttf", 32)
        fonts["subtitle"] = ImageFont.truetype(r"C:\Windows\Fonts\timesi.ttf", 24)
        fonts["body"] = ImageFont.truetype(r"C:\Windows\Fonts\times.ttf", 22)
        fonts["body_bold"] = ImageFont.truetype(r"C:\Windows\Fonts\timesbd.ttf", 22)
        fonts["typewriter"] = ImageFont.truetype(r"C:\Windows\Fonts\cour.ttf", 20)
        fonts["typewriter_bold"] = ImageFont.truetype(r"C:\Windows\Fonts\courbd.ttf", 22)
        fonts["hand"] = ImageFont.truetype(r"C:\Windows\Fonts\segoepr.ttf", 22) # Segoe Print
        fonts["hand_bold"] = ImageFont.truetype(r"C:\Windows\Fonts\segoeprb.ttf", 24)
        fonts["hand_big"] = ImageFont.truetype(r"C:\Windows\Fonts\segoeprb.ttf", 34)
    except:
        default = ImageFont.load_default()
        for k in ["title", "subtitle", "body", "body_bold", "typewriter", "typewriter_bold", "hand", "hand_bold", "hand_big"]:
            fonts[k] = default
    return fonts

fonts = get_fonts()

def create_base(bg_path, size=(750, 950)):
    bg = Image.open(bg_path).convert("RGBA")
    return bg.resize(size, Image.Resampling.LANCZOS)

# 1. 2011 - Şifreli Telgraf Şeridi (Şeritli telgraf kağıdı + mors kodları)
def gen_2011():
    img = create_base(old_paper_path)
    draw = ImageDraw.Draw(img)
    # Başlık
    draw.text((375, 80), "T.C. POSTA VE TELGRAF İDARESİ", fill=(40, 25, 15, 255), font=fonts["title"], anchor="mm")
    draw.text((375, 120), "SİSÖREN TELGRAF MERKEZİ — GİZLİ ŞİFRE ŞERİDİ", fill=(120, 30, 20, 255), font=fonts["subtitle"], anchor="mm")
    draw.line([(80, 145), (670, 145)], fill=(120, 30, 20, 200), width=2)
    
    draw.text((100, 180), "KOD NO: #TEL-882-SIS | TARİH: 24 EKİM 1952", fill=(60, 40, 20, 255), font=fonts["typewriter_bold"])
    draw.text((100, 215), "ALICI: HALİL EFENDİ (Maden İşletmecisi)", fill=(60, 40, 20, 255), font=fonts["typewriter"])

    # Yapıştırılmış Telgraf Kağıt Şeritleri Efekti
    strips = [
        "--. . -.-. .   -.-- .- .-. .. ... ..   --. --- .-.. -.. .",
        "HALIL EFENDI SANTAJ DEFTERINI MADEN",
        "YOLUNDAKI ESKI VAGONA SAKLAMIS STOP",
        "GECE YARISI GOLDE BULUSUP TELLERI",
        "KESECEGIZ JANDARMAYA HABER UCUSAZ STOP"
    ]
    y = 300
    for s in strips:
        # Sarı kağıt şerit arka planı
        draw.rectangle([90, y-5, 660, y+35], fill=(245, 235, 190, 230), outline=(180, 150, 100, 200), width=1)
        draw.text((110, y), s, fill=(20, 20, 20, 255), font=fonts["typewriter_bold"])
        y += 55

    # El yazısı ve mürekkep damlası
    draw.text((100, 620), "[Mors Çözüm Notu]:", fill=(140, 30, 20, 255), font=fonts["body_bold"])
    draw.text((100, 660), "Bu şeridi kurbanın çöpünden aldım. Rüstem", fill=(30, 20, 15, 255), font=fonts["hand"])
    draw.text((100, 695), "tellere kasten sabotaj yapıp iletişimi kesti.", fill=(30, 20, 15, 255), font=fonts["hand"])
    draw.text((500, 780), "- Rüstem", fill=(120, 30, 20, 255), font=fonts["hand_big"])
    
    # Kırmızı Kaşe
    draw.rectangle([480, 160, 660, 230], outline=(180, 40, 40, 200), width=3)
    draw.text((570, 195), "GİZLİ KAYIT\nSİSÖREN", fill=(180, 40, 40, 220), font=fonts["body_bold"], anchor="mm")

    img.save(os.path.join(deliller_dir, "doc_2011.png"))
    print("doc_2011.png created")

# 2. 2015 - Tehdit Mesajı Taslağı (Karalanmış öfkeli daktilo/el yazısı mektup)
def gen_2015():
    img = create_base(old_paper_path)
    draw = ImageDraw.Draw(img)
    draw.text((375, 90), "KARALAMA KAĞIDI & TEHDİTNAME", fill=(140, 20, 20, 255), font=fonts["title"], anchor="mm")
    draw.line([(100, 120), (650, 120)], fill=(140, 20, 20, 200), width=2)
    
    draw.text((100, 170), "Halil Efendi,", fill=(30, 20, 15, 255), font=fonts["hand_bold"])
    
    lines = [
        "Telgraf tellerinden geçen her sır benim kulağımda biter.",
        "Madendeki o kanlı kaçak sevkiyatları ve madenci",
        "ailelerinden çaldığın yetim haklarını bildiğimi inkâr etme.",
        "Elimde vilayete gidecek resmi telgraf kopyaları var.",
        "",
        "Ya bu akşam telgrafhaneye 50.000 Lirayı getirip bırakırsın,",
        "ya da yarın şafakla birlikte savcılık kapına dayanır.",
        "Ecelin o incecik telgraf tellerinin ucunda sallanıyor!",
        "Bu senin son şansındır..."
    ]
    y = 230
    for line in lines:
        draw.text((100, y), line, fill=(35, 25, 20, 255), font=fonts["hand"])
        y += 40
        
    # Karalanmış imza
    draw.line([(100, 680), (320, 680)], fill=(20, 20, 20, 255), width=4)
    draw.line([(95, 685), (325, 675)], fill=(20, 20, 20, 255), width=3)
    draw.text((100, 710), "[Gönderen adı jiletle kazınmış]", fill=(150, 30, 30, 255), font=fonts["subtitle"])
    
    img.save(os.path.join(deliller_dir, "doc_2015.png"))
    print("doc_2015.png created")

# 3. 2023 - Yırtık Borç Listesi (Kahvehane açık defteri, kırmızı çizilmiş maktul borcu)
def gen_2023():
    img = create_base(book_bg_path, size=(900, 650))
    draw = ImageDraw.Draw(img)
    
    # Sol Sayfa
    draw.text((250, 70), "SİSÖREN KAHVEHANESİ VERESİYE", fill=(40, 25, 15, 255), font=fonts["body_bold"], anchor="mm")
    draw.text((250, 105), "1952 Güz Dönemi Hesap Defteri", fill=(100, 60, 30, 255), font=fonts["subtitle"], anchor="mm")
    draw.line([(80, 125), (420, 125)], fill=(80, 50, 20, 180), width=1)
    
    rows = [
        ("Çoban Durmuş:", "120 TL (Ödendi)"),
        ("Tüpçü Şevket:", "310 TL"),
        ("Hurdacı Zehra:", "480 TL"),
        ("Halil Efendi (Maden):", "65.000 TL"),
        ("Sinemacı Nejat:", "250 TL"),
        ("Sahaf Hikmet:", "85 TL")
    ]
    y = 160
    for name, val in rows:
        if "Halil" in name:
            # Kırmızıyla üzeri çizilmiş
            draw.text((80, y), name, fill=(180, 20, 20, 255), font=fonts["body_bold"])
            draw.text((280, y), val, fill=(180, 20, 20, 255), font=fonts["body_bold"])
            draw.line([(75, y+12), (410, y+12)], fill=(200, 30, 30, 255), width=3)
        else:
            draw.text((80, y), name, fill=(30, 20, 15, 255), font=fonts["body"])
            draw.text((280, y), val, fill=(80, 50, 30, 255), font=fonts["body"])
        y += 45
        
    draw.rectangle([80, 460, 420, 550], outline=(180, 40, 40, 220), width=2)
    draw.text((250, 505), "KAHVEHANE HACİZLİDİR\nİCRA MEMURLUĞU", fill=(180, 40, 40, 240), font=fonts["body_bold"], anchor="mm")

    # Sağ Sayfa
    draw.text((670, 70), "İRFAN'IN KİŞİSEL NOTLARI", fill=(120, 20, 20, 255), font=fonts["body_bold"], anchor="mm")
    draw.line([(510, 100), (830, 100)], fill=(80, 50, 20, 180), width=1)
    
    notes = [
        "• Halil kahvehanemi ipotek ettirip",
        "  ocağımı söndürmek istiyor.",
        "• Her gün gelip yüzüme bıyık altından",
        "  sırıtıyor, bana hakaret ediyor.",
        "• Sabrım tamamen tükendi.",
        "• Altıpatları yağladım, fişekleri dizdim.",
        "• Bu hesap dağ yolunda kanla kapanacak!"
    ]
    y = 140
    for n in notes:
        draw.text((510, y), n, fill=(25, 20, 15, 255), font=fonts["hand"])
        y += 38
        
    img.save(os.path.join(deliller_dir, "doc_2023.png"))
    print("doc_2023.png created")

# 4. 2032 - Şantajlı Sinema Afişi (Film afişi arkasında mor mürekkepli şantaj)
def gen_2032():
    img = create_base(old_paper_path)
    draw = ImageDraw.Draw(img)
    draw.text((375, 80), "SİSÖREN BELEDİYE SİNEMASI", fill=(40, 30, 20, 255), font=fonts["title"], anchor="mm")
    draw.text((375, 120), "\"GECENİN GÖLGELERİ\" (1949) — AFİŞ ARKA YÜZÜ", fill=(20, 80, 80, 255), font=fonts["subtitle"], anchor="mm")
    draw.line([(90, 145), (660, 145)], fill=(30, 90, 90, 200), width=2)
    
    draw.rectangle([100, 180, 650, 430], fill=(240, 235, 220, 200), outline=(80, 50, 100, 220), width=2)
    draw.text((120, 205), "MOR MÜREKKEPLE DÜŞÜLEN TEHDİT:", fill=(110, 30, 120, 255), font=fonts["body_bold"])
    
    threat_lines = [
        "\"Nejat, projeksiyon odasındaki kaçak film",
        "kutularında altın külçeleri sakladığını Halil",
        "Efendi gözleriyle gördü. O karanlık işlerin",
        "hepsi 35 mm makaralara kaydedildi.",
        "Ya 100.000 Lira getirirsin ya da yarın filmin",
        "başrolünde savcı ve jandarma oynar!\""
    ]
    y = 245
    for tl in threat_lines:
        draw.text((130, y), tl, fill=(80, 20, 90, 255), font=fonts["hand_bold"])
        y += 30

    # Nejat'ın el yazısı cevabı
    draw.text((100, 480), "[Nejat'ın aceleyle yazdığı el notu]:", fill=(140, 30, 20, 255), font=fonts["body_bold"])
    draw.text((100, 520), "Bu gece son seans... Gece 02:00'de perde", fill=(30, 25, 20, 255), font=fonts["hand"])
    draw.text((100, 560), "kapanınca Halil'in de hayat perdesi inecek!", fill=(160, 20, 20, 255), font=fonts["hand_bold"])
    draw.text((100, 600), "Dönen projeksiyon çarkları her şeyi ezecek.", fill=(30, 25, 20, 255), font=fonts["hand"])

    img.save(os.path.join(deliller_dir, "doc_2032.png"))
    print("doc_2032.png created")

# 5. 2042 - Bakkal Cemile'nin Yırtık Veresiye Sayfası (Yırtık reçete kağıdı texture'ı üzerinde)
def gen_2042():
    img = create_base(pres_paper_path)
    draw = ImageDraw.Draw(img)
    draw.text((375, 90), "SİSÖREN DAĞ BAKKALI", fill=(50, 30, 15, 255), font=fonts["title"], anchor="mm")
    draw.text((375, 125), "YIRTIK VERESİYE SAYFASI #412", fill=(140, 40, 20, 255), font=fonts["subtitle"], anchor="mm")
    draw.line([(100, 150), (650, 150)], fill=(120, 40, 20, 200), width=1)
    
    draw.text((110, 180), "Müşteri: Madenci Halil Efendi", fill=(40, 30, 20, 255), font=fonts["typewriter_bold"])
    draw.text((110, 215), "• 10 Çuval Un, 5 Kilo Çay: 450 Lira (Ödenmedi)", fill=(40, 30, 20, 255), font=fonts["typewriter"])
    draw.text((110, 250), "• Dükkân İpotek Borcu: 40.000 Lira", fill=(160, 30, 20, 255), font=fonts["typewriter_bold"])

    # Yırtık kenar uyarısı
    draw.rectangle([100, 310, 650, 450], fill=(250, 240, 230, 180), outline=(160, 40, 40, 200), width=2)
    draw.text((120, 325), "[YIRTILMIŞ ALACAK TEHDİDİ]:", fill=(180, 20, 20, 255), font=fonts["body_bold"])
    draw.text((120, 360), "\"Cemile, borcu bu hafta kapatmazsan dükkâna", fill=(40, 20, 20, 255), font=fonts["hand"])
    draw.text((120, 395), "el koyup seni kapı dışarı edeceğim!\" - Halil", fill=(40, 20, 20, 255), font=fonts["hand_bold"])
    
    # Cemile'nin el yazısı
    draw.text((110, 500), "[Cemile'nin kırmızı tükenmez notu]:", fill=(180, 20, 20, 255), font=fonts["body_bold"])
    draw.text((110, 540), "Dükkânımı elimden alamazsın harami...", fill=(180, 20, 20, 255), font=fonts["hand"])
    draw.text((110, 580), "Kilerdeki fare zehri bu akşam peynirine", fill=(180, 20, 20, 255), font=fonts["hand_bold"])
    draw.text((110, 620), "karışacak. Kalbin durduğunda helallik iste!", fill=(180, 20, 20, 255), font=fonts["hand_bold"])

    img.save(os.path.join(deliller_dir, "doc_2042.png"))
    print("doc_2042.png created")

# 6. 2051 - İçi Oyulmuş Ansiklopedi (Şifreli Harita & Hazine Bilgisi)
def gen_2051():
    img = create_base(old_paper_path)
    draw = ImageDraw.Draw(img)
    draw.text((375, 80), "BÜYÜK OSMANLI TARİHİ — CİLT IV", fill=(40, 30, 20, 255), font=fonts["title"], anchor="mm")
    draw.text((375, 120), "OYULMUŞ SAYFA ARASINDA GİZLİ HARİTA", fill=(120, 40, 20, 255), font=fonts["subtitle"], anchor="mm")
    draw.line([(90, 145), (660, 145)], fill=(120, 40, 20, 200), width=2)
    
    # Harita Çizimi Kutusu
    draw.rectangle([110, 180, 640, 480], fill=(245, 238, 220, 230), outline=(60, 40, 20, 240), width=3)
    draw.text((375, 210), "SİSÖREN ESKİ MADEN GALERİSİ KROKİSİ", fill=(120, 30, 20, 255), font=fonts["body_bold"], anchor="mm")
    
    # Basit kroki çizgileri
    draw.line([(150, 300), (600, 300)], fill=(80, 50, 30, 255), width=3)
    draw.line([(375, 300), (375, 420)], fill=(80, 50, 30, 255), width=3)
    draw.rectangle([350, 390, 400, 440], fill=(200, 160, 80, 255), outline=(100, 50, 10, 255), width=2)
    draw.text((375, 415), "[ALTIN]", fill=(80, 20, 10, 255), font=fonts["typewriter_bold"], anchor="mm")
    draw.text((220, 275), "Kuzey Galerisi (Kilitli)", fill=(80, 50, 30, 255), font=fonts["typewriter"])
    draw.text((450, 275), "Maden Çıkışı (Sisli Patika)", fill=(80, 50, 30, 255), font=fonts["typewriter"])

    # Hikmet'in Notu
    draw.text((100, 520), "[Sahaf Hikmet'in El Yazısı Notu]:", fill=(140, 30, 20, 255), font=fonts["body_bold"])
    draw.text((100, 560), "Halil bu haritayı çalmak için sahafı basacağını söyledi.", fill=(30, 25, 20, 255), font=fonts["hand"])
    draw.text((100, 600), "Atalarımın mirasını o açgözlü hırsıza yedirmem.", fill=(30, 25, 20, 255), font=fonts["hand"])
    draw.text((100, 640), "Gerekirse bu kalın ciltli ansiklopedi kafasını ezer!", fill=(160, 20, 20, 255), font=fonts["hand_bold"])

    img.save(os.path.join(deliller_dir, "doc_2051.png"))
    print("doc_2051.png created")

# 7. 2061 - Asitle Silinmiş Köy Nüfus Kütüğü (Muhtarlık)
def gen_2061():
    img = create_base(old_paper_path)
    draw = ImageDraw.Draw(img)
    draw.text((375, 80), "T.C. SİSÖREN KÖYÜ MUHTARLIĞI", fill=(20, 40, 30, 255), font=fonts["title"], anchor="mm")
    draw.text((375, 120), "RESMİ NÜFUS VE ARAZİ KÜTÜK DEFTERİ #12", fill=(120, 30, 20, 255), font=fonts["subtitle"], anchor="mm")
    draw.line([(80, 145), (670, 145)], fill=(120, 30, 20, 200), width=2)
    
    draw.text((100, 180), "Hane: 29 | Sayfa: 184 | Cilt: IV", fill=(40, 30, 20, 255), font=fonts["typewriter_bold"])
    draw.text((100, 220), "Kişi Adı: Halil Efendi (Maden Müteahhidi)", fill=(40, 30, 20, 255), font=fonts["typewriter"])

    # Asitle Silinmiş / Yanmış Kutu Alanı
    draw.rectangle([100, 270, 650, 450], fill=(40, 35, 30, 250), outline=(20, 20, 20, 255), width=3)
    draw.text((375, 320), "[KİMYASAL ASİTLE ERİTİLMİŞ VE KAZINMIŞ ALAN]", fill=(200, 70, 70, 255), font=fonts["body_bold"], anchor="mm")
    draw.text((375, 360), "Miras Payı: 840 Dönüm Yayla Arazisi...", fill=(180, 180, 180, 255), font=fonts["typewriter"], anchor="mm")
    draw.text((375, 395), "-> Meliha Hanım mülkiyetine tescil edilmiştir.", fill=(220, 180, 80, 255), font=fonts["typewriter_bold"], anchor="mm")

    draw.text((100, 500), "[Dedektif İnceleme Notu]:", fill=(140, 30, 20, 255), font=fonts["body_bold"])
    draw.text((100, 540), "Maktul Halil Efendi bu sahteciliği ortaya çıkarmıştı.", fill=(30, 25, 20, 255), font=fonts["hand"])
    draw.text((100, 580), "Muhtar Meliha Hanım'ı hapse attırmakla tehdit ediyordu.", fill=(30, 25, 20, 255), font=fonts["hand"])
    draw.text((100, 620), "Meliha Hanım'ın el yazısı: 'Halil konuşamayacak!'", fill=(180, 20, 20, 255), font=fonts["hand_bold"])

    img.save(os.path.join(deliller_dir, "doc_2061.png"))
    print("doc_2061.png created")

# 8. 2085 - Ahırdaki Saplı Bıçak Notu (Dirseğe saplanmış kanlı mektup)
def gen_2085():
    img = create_base(old_paper_path)
    draw = ImageDraw.Draw(img)
    draw.text((375, 90), "KANLI MEYDAN OKUMA NOTU", fill=(180, 20, 20, 255), font=fonts["title"], anchor="mm")
    draw.line([(100, 120), (650, 120)], fill=(180, 20, 20, 200), width=2)
    
    # Bıçak deliği efekti
    draw.ellipse([365, 140, 385, 190], fill=(20, 15, 10, 255), outline=(140, 20, 20, 255), width=3)
    
    draw.text((100, 220), "HALİL HARAMİSİ!", fill=(160, 20, 20, 255), font=fonts["hand_big"])
    
    text_durmus = [
        "Sürümün su içtiği dağ deresine kimyasal maden",
        "zehrini dökmeyi bırakacaksın dedik, dinlemedin.",
        "Dün gece üç gebe koyunum kan kusarak can verdi.",
        "Hayvanlarımın canı senin kirli canından kıymetlidir.",
        "",
        "Dağ yolunda sisin içinde karşıma çıkarsan,",
        "o paslı nalbant çivisini kalbine çakarım!",
        "Bu dağda kanun da savcı da benim!"
    ]
    y = 290
    for td in text_durmus:
        draw.text((100, y), td, fill=(35, 25, 20, 255), font=fonts["hand_bold"])
        y += 42
        
    draw.text((450, 680), "— Çoban Durmuş", fill=(140, 20, 20, 255), font=fonts["hand_big"])
    
    img.save(os.path.join(deliller_dir, "doc_2085.png"))
    print("doc_2085.png created")

# 9. 2104 - Hurdacı Zehra'nın Kanlı Tartım Fişi
def gen_2104():
    img = create_base(old_paper_path)
    draw = ImageDraw.Draw(img)
    draw.text((375, 80), "SİSÖREN HURDA & KANTAR İŞLETMESİ", fill=(40, 30, 20, 255), font=fonts["title"], anchor="mm")
    draw.text((375, 120), "RESMİ KANTAR TARTIM MAKBUZU", fill=(120, 30, 20, 255), font=fonts["subtitle"], anchor="mm")
    draw.line([(80, 145), (670, 145)], fill=(120, 30, 20, 200), width=2)
    
    draw.text((100, 180), "MAKBUZ NO: #8921 | SAAT: 02:18 (GECE)", fill=(40, 30, 20, 255), font=fonts["typewriter_bold"])
    draw.text((100, 215), "MALZEME: 420 KG AĞIR SANAYİ RAYI & ÇELİK HURDA", fill=(40, 30, 20, 255), font=fonts["typewriter"])

    # Kan Lekesi
    draw.ellipse([450, 250, 580, 380], fill=(130, 20, 20, 220))
    draw.ellipse([470, 270, 560, 360], fill=(160, 30, 30, 240))
    draw.text((515, 315), "KAN İZİ", fill=(240, 200, 200, 255), font=fonts["body_bold"], anchor="mm")

    draw.rectangle([100, 420, 650, 580], fill=(245, 238, 225, 200), outline=(80, 50, 20, 200), width=2)
    draw.text((120, 440), "[ARKA YÜZÜNE KURŞUN KALEMLE DÜŞÜLEN NOT]:", fill=(140, 20, 20, 255), font=fonts["body_bold"])
    draw.text((120, 480), "\"Halil pres makinesinin yanına kadar geldi, pay istedi.", fill=(30, 25, 20, 255), font=fonts["hand"])
    draw.text((120, 520), "Presin çelik kapağı kapandı... Artık hurdalardan farksız!\"", fill=(160, 20, 20, 255), font=fonts["hand_bold"])

    draw.text((450, 620), "— Hurdacı Zehra", fill=(120, 30, 20, 255), font=fonts["hand_big"])

    img.save(os.path.join(deliller_dir, "doc_2104.png"))
    print("doc_2104.png created")

# 10. 2125 - Şöminede Kısmen Yanmış Sahte Köy Satış Tapusu
def gen_2125():
    img = create_base(old_paper_path)
    draw = ImageDraw.Draw(img)
    draw.text((375, 80), "ARAZİ DEVİR VE MÜLKİYET FERAGAT SENEDİ", fill=(120, 20, 20, 255), font=fonts["title"], anchor="mm")
    draw.text((375, 120), "SİSÖREN DAĞ KÖYÜ TOPLU SATIŞ PROTOKOLÜ", fill=(40, 30, 20, 255), font=fonts["subtitle"], anchor="mm")
    draw.line([(80, 145), (670, 145)], fill=(120, 20, 20, 200), width=2)
    
    draw.text((100, 180), "Satıcı: Madenci Halil Efendi (Yetkisiz Köy Temsilcisi)", fill=(40, 30, 20, 255), font=fonts["typewriter_bold"])
    draw.text((100, 215), "Alıcı: Doğu Madencilik ve Taşocağı Anonim Şirketi", fill=(40, 30, 20, 255), font=fonts["typewriter"])
    draw.text((100, 250), "Devredilen: Sisören Yaylası ve 40 Adet Köy Parseli", fill=(40, 30, 20, 255), font=fonts["typewriter"])

    # Yanık Kenar Efekti (Şömine ateşi)
    draw.rectangle([100, 300, 650, 440], fill=(50, 35, 25, 240), outline=(20, 15, 10, 255), width=2)
    draw.text((375, 350), "[ŞÖMİNEDE KISMEN YANMIŞ VE KÜL OLMUŞ BÖLÜM]", fill=(220, 120, 60, 255), font=fonts["body_bold"], anchor="mm")
    draw.text((375, 390), "Köylüler evlerinden tahliye edilecek ve maden genişletilecek...", fill=(180, 180, 180, 255), font=fonts["typewriter"], anchor="mm")

    # Hatice Nine'nin Notu
    draw.text((100, 500), "[Hatice Nine'nin El Yazısı]:", fill=(140, 30, 20, 255), font=fonts["body_bold"])
    draw.text((100, 540), "Bu tapuyu ateşe attım ama Halil vazgeçmedi.", fill=(30, 25, 20, 255), font=fonts["hand"])
    draw.text((100, 580), "Topraklarımızı kimseye vermeyiz oğul.", fill=(30, 25, 20, 255), font=fonts["hand"])
    draw.text((100, 620), "Gerekirse dağın kara zehirli mantarı kaynar!", fill=(180, 20, 20, 255), font=fonts["hand_bold"])

    img.save(os.path.join(deliller_dir, "doc_2125.png"))
    print("doc_2125.png created")

# Hepsini çalıştır
gen_2011()
gen_2015()
gen_2023()
gen_2032()
gen_2042()
gen_2051()
gen_2061()
gen_2085()
gen_2104()
gen_2125()
print("All realistic document textures generated successfully!")

