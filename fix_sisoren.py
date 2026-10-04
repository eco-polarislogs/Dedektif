import re
import json

with open('wwwroot/js/towns/sisoren/SisorenConfig.js', 'r', encoding='utf-8') as f:
    content = f.read()

# Fix the street NPCs mapPos and bg/talkBg for extraNpcs
# Şerife Teyze
content = content.replace("id: 'sokak_serif',", "id: 'sokak_serif',\n            mapPos: { top: '35%', left: '35%' },\n            bg: 'images/towns/sisoren/sisoren_map_v5.jpg',\n            talkBg: 'images/towns/sisoren/sisoren_map_v5.jpg',")
# Cemal
content = content.replace("id: 'sokak_cemal',", "id: 'sokak_cemal',\n            mapPos: { top: '75%', left: '55%' },\n            bg: 'images/towns/sisoren/sisoren_map_v5.jpg',\n            talkBg: 'images/towns/sisoren/sisoren_map_v5.jpg',")
# Postacı
content = content.replace("id: 'sokak_postaci',", "id: 'sokak_postaci',\n            mapPos: { top: '55%', left: '25%' },\n            bg: 'images/towns/sisoren/sisoren_map_v5.jpg',\n            talkBg: 'images/towns/sisoren/sisoren_map_v5.jpg',")
# Kerem (Çoban Çocuğu)
content = content.replace("id: 'sokak_cocuk1',", "id: 'sokak_cocuk1',\n            mapPos: { top: '85%', left: '75%' },\n            bg: 'images/towns/sisoren/sisoren_map_v5.jpg',\n            talkBg: 'images/towns/sisoren/sisoren_map_v5.jpg',")
# Ayşe (Bakkal Çırağı)
content = content.replace("id: 'sokak_cocuk2',", "id: 'sokak_cocuk2',\n            mapPos: { top: '85%', left: '15%' },\n            bg: 'images/towns/sisoren/sisoren_map_v5.jpg',\n            talkBg: 'images/towns/sisoren/sisoren_map_v5.jpg',")

# For other extra NPCs that are inside buildings, give them their building's interior image
content = content.replace("id: 'kahve_celal',", "id: 'kahve_celal',\n            bg: 'images/towns/sisoren/interiors/kahvehane_interior.jpg',\n            talkBg: 'images/towns/sisoren/interiors/kahvehane_interior.jpg',")
content = content.replace("id: 'kahve_hamdi',", "id: 'kahve_hamdi',\n            bg: 'images/towns/sisoren/interiors/kahvehane_interior.jpg',\n            talkBg: 'images/towns/sisoren/interiors/kahvehane_interior.jpg',")
content = content.replace("id: 'kahve_irfan',", "id: 'kahve_irfan',\n            bg: 'images/towns/sisoren/interiors/kahvehane_interior.jpg',\n            talkBg: 'images/towns/sisoren/interiors/kahvehane_interior.jpg',")
content = content.replace("id: 'telgraf_yusuf',", "id: 'telgraf_yusuf',\n            bg: 'images/towns/sisoren/interiors/telgrafhane_interior.jpg',\n            talkBg: 'images/towns/sisoren/interiors/telgrafhane_interior.jpg',")
content = content.replace("id: 'sinema_nejat',", "id: 'sinema_nejat',\n            bg: 'images/towns/sisoren/interiors/sinema_interior.jpg',\n            talkBg: 'images/towns/sisoren/interiors/sinema_interior.jpg',")
content = content.replace("id: 'muhtar_katiP',", "id: 'muhtar_katiP',\n            bg: 'images/towns/sisoren/interiors/muhtarlik_interior.jpg',\n            talkBg: 'images/towns/sisoren/interiors/muhtarlik_interior.jpg',")
content = content.replace("id: 'tutuncu_hasan',", "id: 'tutuncu_hasan',\n            bg: 'images/towns/sisoren/interiors/tutuncu_interior.jpg',\n            talkBg: 'images/towns/sisoren/interiors/tutuncu_interior.jpg',")
content = content.replace("id: 'hurdaci_kocasi',", "id: 'hurdaci_kocasi',\n            bg: 'images/towns/sisoren/interiors/hurdaci_interior.jpg',\n            talkBg: 'images/towns/sisoren/interiors/hurdaci_interior.jpg',")
content = content.replace("id: 'tupcu_ciragi',", "id: 'tupcu_ciragi',\n            bg: 'images/towns/sisoren/interiors/tupcu_interior.jpg',\n            talkBg: 'images/towns/sisoren/interiors/tupcu_interior.jpg',")

# Re-insert hotspots for buildings exactly as intended, replacing whatever is there.
hotspots_map = {
    'telgrafhane': '''            hotspots: [
                { id: 2011, name: 'Şifreli Telgraf Şeridi', desc: 'Çöpe atılmış, sadece son birkaç kelimesi ("...gece yarısı gölde...") okunabilen yırtık bir mesaj kopyası.', img: 'images/towns/sisoren/deliller/2011.jpg', top: '75%', left: '20%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: { xRatio: 0.6, yRatio: 0.4, angle: 10 } },
                { id: 2012, name: 'Kanlı Parmak İzi', desc: 'Mors alfabesi tuş takımının (telgraf manilesi) tam altına bulaşmış ve silinmeyi unutulmuş taze bir iz.', img: 'images/towns/sisoren/deliller/2012.jpg', top: '55%', left: '40%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 } },
                { id: 2013, name: 'Rüşvet Kesesi', desc: 'Telgraf memurunun masasının gizli çekmecesinde duran, kasaba dışından gönderilmiş isimsiz yüklü miktar para.', img: 'images/towns/sisoren/deliller/2013.jpg', top: '65%', left: '60%', fingerprintSpot: { xRatio: 0.3, yRatio: 0.7, angle: 15 }, bloodSpot: null },
                { id: 2014, name: 'Kesik Hat Kablosu', desc: 'Arka odada kasten kesilmiş ve iletişimi koparmak için saklanmış yedek telgraf telleri.', img: 'images/towns/sisoren/deliller/2014.jpg', top: '35%', left: '80%', fingerprintSpot: { xRatio: 0.2, yRatio: 0.2, angle: -10 }, bloodSpot: { xRatio: 0.8, yRatio: 0.8, angle: 45 } },
                { id: 2015, name: 'Tehdit Mesajı Taslağı', desc: 'Karalama kâğıtlarının arasında bulunan, gönderici adı karalanmış tehditkâr bir not.', img: 'images/towns/sisoren/deliller/2015.jpg', top: '45%', left: '10%', fingerprintSpot: { xRatio: 0.7, yRatio: 0.3, angle: 20 }, bloodSpot: null }
            ]''',
    'kahvehane': '''            hotspots: [
                { id: 2021, name: 'Gizli Mesajlı Fincan', desc: 'Altına sert bir cisimle buluşma saati kazınmış, masada yarım bırakılmış kulpsuz kahve fincanı.', img: 'images/towns/sisoren/deliller/2021.jpg', top: '60%', left: '30%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null },
                { id: 2022, name: 'İşaretli Okey Taşı', desc: 'Yerdeki talaşların arasına düşmüş, üzerinde yasa dışı bir örgütün sembolü kazınmış sahte bir taş.', img: 'images/towns/sisoren/deliller/2022.jpg', top: '80%', left: '50%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null },
                { id: 2023, name: 'Yırtık Borç Listesi', desc: 'Kırık bir sandalyenin bacağına sıkıştırılmış, bazı isimlerin üzeri kırmızıyla çizilmiş kâğıt.', img: 'images/towns/sisoren/deliller/2023.jpg', top: '75%', left: '70%', fingerprintSpot: { xRatio: 0.4, yRatio: 0.6, angle: -15 }, bloodSpot: null },
                { id: 2024, name: 'Yanık Kasket', desc: 'Sobanın içine atılmış ama tam yanmamış, kenarında kan lekesi bulunan tanıdık bir şapka.', img: 'images/towns/sisoren/deliller/2024.jpg', top: '40%', left: '85%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: { xRatio: 0.6, yRatio: 0.2, angle: 0 } },
                { id: 2025, name: 'Zulalanmış Altıpatlar', desc: 'Çay ocağının altındaki tahtaların arkasına gizlenmiş, bir mermisi eksik ruhsatsız tabanca.', img: 'images/towns/sisoren/deliller/2025.jpg', top: '50%', left: '15%', fingerprintSpot: { xRatio: 0.3, yRatio: 0.8, angle: 45 }, bloodSpot: { xRatio: 0.7, yRatio: 0.5, angle: 90 } }
            ]''',
    'sinema': '''            hotspots: [
                { id: 2031, name: 'Kesilmiş Film Bobini', desc: 'Makine dairesinde yer alan bir filmin en kritik sahnesinin kasten kesilip saklanmış parçası.', img: 'images/towns/sisoren/deliller/2031.jpg', top: '40%', left: '25%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null },
                { id: 2032, name: 'Şantajlı Afiş', desc: 'Gişe memurunun çekmecesinde, arka yüzüne "Ne yaptığını biliyorum" yazılmış eski bir film afişi.', img: 'images/towns/sisoren/deliller/2032.jpg', top: '65%', left: '15%', fingerprintSpot: { xRatio: 0.8, yRatio: 0.2, angle: -30 }, bloodSpot: null },
                { id: 2033, name: 'Düşmüş Bilet Koçanı', desc: 'Arka sıradaki koltuklardan birinin altına düşmüş, cinayet saatine ait yırtık bir bilet parçası.', img: 'images/towns/sisoren/deliller/2033.jpg', top: '85%', left: '45%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null },
                { id: 2034, name: 'Kanlı Deri Eldiven', desc: 'Perdenin arkasındaki karanlık köşede unutulmuş, pahalı bir kumaştan yapılmış eldiven teki.', img: 'images/towns/sisoren/deliller/2034.jpg', top: '70%', left: '75%', fingerprintSpot: null, bloodSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 } },
                { id: 2035, name: 'Rujlu Aynadaki Saat', desc: 'Sahne arkasındaki eski bir makyaj aynasına aceleyle rujla yazılmış bir rıhtım adresi.', img: 'images/towns/sisoren/deliller/2035.jpg', top: '50%', left: '85%', fingerprintSpot: { xRatio: 0.2, yRatio: 0.8, angle: 10 }, bloodSpot: null }
            ]''',
    'bakkal': '''            hotspots: [
                { id: 2041, name: 'Saklanmış Fare Zehri', desc: 'Tezgâhın arkasına gizlenmiş, yarısı kullanılmış ve kutusu ezilmiş ağır bir kimyasal zehir.', img: 'images/towns/sisoren/deliller/2041.jpg', top: '55%', left: '20%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null },
                { id: 2042, name: 'Kopuk Veresiye Sayfası', desc: 'Veresiye defterinde, cinayet gecesine ait sayfaların kasten koparılmış olması.', img: 'images/towns/sisoren/deliller/2042.jpg', top: '45%', left: '60%', fingerprintSpot: { xRatio: 0.7, yRatio: 0.3, angle: -10 }, bloodSpot: null },
                { id: 2043, name: 'Yabancı Gümüş Sikke', desc: 'Kasanın yanına düşmüş, kasabadan kimseye ait olmayan ve başka bir şehre ait para.', img: 'images/towns/sisoren/deliller/2043.jpg', top: '65%', left: '70%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null },
                { id: 2044, name: 'Kaçak Tütün', desc: 'Rafların en arkasında, un çuvallarının ardına zulalanmış faturasız ithal tütün paketleri.', img: 'images/towns/sisoren/deliller/2044.jpg', top: '30%', left: '85%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null },
                { id: 2045, name: 'İsimli Senet', desc: 'Yerdeki boş kasaların arasına düşmüş, üzerinde kurbanın adının yazılı olduğu buruşuk bir borç senedi.', img: 'images/towns/sisoren/deliller/2045.jpg', top: '80%', left: '30%', fingerprintSpot: { xRatio: 0.2, yRatio: 0.8, angle: 15 }, bloodSpot: { xRatio: 0.8, yRatio: 0.2, angle: 45 } }
            ]''',
    'sahaf': '''            hotspots: [
                { id: 2051, name: 'İçi Oyulmuş Ansiklopedi', desc: 'Sayfaları kesilerek gizli bir kutu haline getirilmiş, içinde şifreli bir harita barındıran kalın ciltli kitap.', img: 'images/towns/sisoren/deliller/2051.jpg', top: '45%', left: '20%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null },
                { id: 2052, name: 'Şifreli Kenar Notları', desc: 'Kasaba tarihini anlatan eski bir kitabın kenarlarına kırmızı mürekkeple düşülmüş garip rakamlar.', img: 'images/towns/sisoren/deliller/2052.jpg', top: '35%', left: '50%', fingerprintSpot: { xRatio: 0.8, yRatio: 0.2, angle: -5 }, bloodSpot: null },
                { id: 2053, name: 'Kurumuş Kan Damlası', desc: 'Vitrindeki satılık eski bir cep saatinin kapağına içeriden yapışmış şüpheli leke.', img: 'images/towns/sisoren/deliller/2053.jpg', top: '55%', left: '75%', fingerprintSpot: null, bloodSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 } },
                { id: 2054, name: 'Kasa İzi', desc: 'Yerdeki kalın toz tabakasında, kısa süre önce ağır bir eşyanın (veya kasanın) yerinden çekildiğini gösteren sürtünme izi.', img: 'images/towns/sisoren/deliller/2054.jpg', top: '85%', left: '40%', fingerprintSpot: null, bloodSpot: null },
                { id: 2055, name: 'Sahte Vasiyetname', desc: 'Nadir eserlerin arasına sıkıştırılmış, kurbanın imzasının taklit edildiği yırtık bir vasiyet taslağı.', img: 'images/towns/sisoren/deliller/2055.jpg', top: '65%', left: '10%', fingerprintSpot: { xRatio: 0.3, yRatio: 0.7, angle: 25 }, bloodSpot: null }
            ]''',
    'muhtarlik': '''            hotspots: [
                { id: 2061, name: 'Asitle Silinmiş Kütük', desc: 'Nüfus kütük defterinde, maktulün ve ailesinin adının üzerinin asit/mürekkep ile yok edilmeye çalışıldığı sayfa.', img: 'images/towns/sisoren/deliller/2061.jpg', top: '50%', left: '40%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null },
                { id: 2062, name: 'Kırık Mühür', desc: 'Sahte belgelere basılırken fazla baskı uygulandığı için kenarı çatlamış resmi kasaba damgası.', img: 'images/towns/sisoren/deliller/2062.jpg', top: '55%', left: '60%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null },
                { id: 2063, name: 'Gizli Orman Tapusu', desc: 'Kilitli dolabın dibinde bulunan, kasabanın dışındaki kime ait olduğu belirsiz bir arazi haritası.', img: 'images/towns/sisoren/deliller/2063.jpg', top: '75%', left: '20%', fingerprintSpot: { xRatio: 0.2, yRatio: 0.2, angle: -10 }, bloodSpot: null },
                { id: 2064, name: 'Boş Kovan', desc: 'Muhtarın masasının altına, süpürgeliğin dibine yuvarlanmış cinayet silahına ait mermi kovanı.', img: 'images/towns/sisoren/deliller/2064.jpg', top: '85%', left: '50%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null },
                { id: 2065, name: 'Parçalanmış Uyarı Mektubu', desc: 'Çöp kutusunda bulunan, üst makamlardan gelmiş ancak yırtılıp yok edilmek istenen resmi evrak.', img: 'images/towns/sisoren/deliller/2065.jpg', top: '65%', left: '80%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null }
            ]''',
    'tutuncu': '''            hotspots: [
                { id: 2071, name: 'İthal Puro İzmariti', desc: 'Normalde kasabada satılmayan, sadece çok zengin bir şüpheliye ait olduğu bilinen yarım içilmiş puro.', img: 'images/towns/sisoren/deliller/2071.jpg', top: '65%', left: '30%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null },
                { id: 2072, name: 'Şifreli Sigara Kâğıdı', desc: 'Sigara sarma kâğıtlarının içine gizlenmiş, ısıtıldığında ortaya çıkan mürekkeple yazılmış adres listesi.', img: 'images/towns/sisoren/deliller/2072.jpg', top: '45%', left: '55%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null },
                { id: 2073, name: 'İçi Boşaltılmış Pipo', desc: 'Şüpheli bir şekilde ağırlaşmış, tütün haznesinin içine rulo yapılmış banknotlar sıkıştırılmış pipo.', img: 'images/towns/sisoren/deliller/2073.jpg', top: '55%', left: '75%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null },
                { id: 2074, name: 'Lekeli Gümüş Tabaka', desc: 'Kasanın altında, üzerinde temizlenmeye çalışılmış kan izleri bulunan şık bir sigara tabakası.', img: 'images/towns/sisoren/deliller/2074.jpg', top: '75%', left: '45%', fingerprintSpot: { xRatio: 0.2, yRatio: 0.8, angle: 10 }, bloodSpot: { xRatio: 0.8, yRatio: 0.2, angle: 0 } },
                { id: 2075, name: 'Zehirli Sıvı Şişesi', desc: 'Tütün balyalarının arasına gizlenmiş, tütün aroması gibi kokan ama aslında felce yol açan küçük şişe.', img: 'images/towns/sisoren/deliller/2075.jpg', top: '35%', left: '20%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null }
            ]''',
    'ahir': '''            hotspots: [
                { id: 2081, name: 'Kesik Eyer Kayışı', desc: 'Maktulün atına ait, kazaya sebep olmak için kasten yarıya kadar kesilmiş deri eyer parçası.', img: 'images/towns/sisoren/deliller/2081.jpg', top: '50%', left: '25%', fingerprintSpot: null, bloodSpot: null },
                { id: 2082, name: 'Kanlı Nalbant Çivisi', desc: 'Saman balyalarının derinine gömülmüş, üzerinde doku ve kan izleri olan keskin bir at nalı çivisi.', img: 'images/towns/sisoren/deliller/2082.jpg', top: '70%', left: '15%', fingerprintSpot: null, bloodSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 } },
                { id: 2083, name: 'Kopuk Altın Zincir', desc: 'Hayvanların su yalağının dibinde parlayan, kurbana ait koptuğu belli olan cep saati zinciri.', img: 'images/towns/sisoren/deliller/2083.jpg', top: '80%', left: '55%', fingerprintSpot: null, bloodSpot: null },
                { id: 2084, name: 'Yabancı Bot İzi', desc: 'Hayvanların huzursuzlandığı köşede toprağa kalıbı çıkmış, kasabalıya ait olmayan sivri burunlu bot izi.', img: 'images/towns/sisoren/deliller/2084.jpg', top: '85%', left: '80%', fingerprintSpot: null, bloodSpot: null },
                { id: 2085, name: 'Saplı Bıçak', desc: 'Tahta direğe sertçe saplanmış, üzerinde kurbana yönelik kısa bir tehdit notu bulunan paslı çakı.', img: 'images/towns/sisoren/deliller/2085.jpg', top: '40%', left: '65%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null }
            ]''',
    'tupcu': '''            hotspots: [
                { id: 2091, name: 'Sabote Edilmiş Vana', desc: 'Valfi kasten bozulmuş ve ufak bir kıvılcımla gaz sızdırıp patlamaya hazır hale getirilmiş piknik tüpü.', img: 'images/towns/sisoren/deliller/2091.jpg', top: '65%', left: '20%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null },
                { id: 2092, name: 'İngiliz Anahtarındaki Leke', desc: 'Demir anahtarın üzerinde, sadece yağ veya pas lekesi olmayan, silinmeye çalışılmış kurumuş kan izleri.', img: 'images/towns/sisoren/deliller/2092.jpg', top: '75%', left: '35%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: { xRatio: 0.7, yRatio: 0.3, angle: 0 } },
                { id: 2093, name: 'Sahte Teslimat Kaydı', desc: 'Müşteri defterinde, olay yeri olan eve cinayet gecesi teslimat yapılmış gibi gösterilen sonradan eklenme kayıt.', img: 'images/towns/sisoren/deliller/2093.jpg', top: '45%', left: '60%', fingerprintSpot: { xRatio: 0.2, yRatio: 0.8, angle: -10 }, bloodSpot: null },
                { id: 2094, name: 'Zulalanmış Mücevher', desc: 'Dükkanın arkasındaki kullanılamaz boş tüplerin birinin içine beze sarılarak saklanmış çalıntı kolye.', img: 'images/towns/sisoren/deliller/2094.jpg', top: '55%', left: '80%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null },
                { id: 2095, name: 'Kopuk Düğme', desc: 'Kırık bir vana parçasının yanına düşmüş, arbede sırasında failin ceketinden koptuğu belli olan özel düğme.', img: 'images/towns/sisoren/deliller/2095.jpg', top: '85%', left: '50%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null }
            ]''',
    'hurdaci': '''            hotspots: [
                { id: 2101, name: 'Çalıntı Çelik Kasa', desc: 'Preslenmek üzere olan bir hurda yığınının arasına sıkıştırılarak yok edilmeye çalışılan kilitli kasa.', img: 'images/towns/sisoren/deliller/2101.jpg', top: '50%', left: '20%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null },
                { id: 2102, name: 'Kazınmış Plaka', desc: 'Parçalanmış eşyaların arasında yeni sayılabilecek, aidiyeti belli olmasın diye üzeri zımparalanmış araç/araba plakası.', img: 'images/towns/sisoren/deliller/2102.jpg', top: '65%', left: '40%', fingerprintSpot: null, bloodSpot: null },
                { id: 2103, name: 'Seri Numarası Silinmiş Tüfek', desc: 'Yağ varillerinin arkasına gizlenmiş, namlusu kesilmiş ve seri numarası eğelenmiş silah.', img: 'images/towns/sisoren/deliller/2103.jpg', top: '75%', left: '60%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: { xRatio: 0.6, yRatio: 0.4, angle: 0 } },
                { id: 2104, name: 'Kanlı Tartım Fişi', desc: 'Hurdacı kantarının altında, şüpheli bir gece yarısı tartımını gösteren ve üzerinde kan damlası olan makbuz.', img: 'images/towns/sisoren/deliller/2104.jpg', top: '45%', left: '75%', fingerprintSpot: { xRatio: 0.3, yRatio: 0.7, angle: -15 }, bloodSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 } },
                { id: 2105, name: 'Kurbana Ait Yüzük', desc: 'Eritilecek bakır tellerin içine karışmış, kurbana ait baş harflerin kazılı olduğu deforme olmuş gümüş yüzük.', img: 'images/towns/sisoren/deliller/2105.jpg', top: '85%', left: '30%', fingerprintSpot: null, bloodSpot: null }
            ]''',
    'kasabali_evi_1': '''            hotspots: [
                { id: 2111, name: 'Yırtık Mektup', desc: 'Masanın altına düşmüş, tehditkâr sözler içeren eksik bir mektup.', img: 'images/towns/sisoren/deliller/placeholder.jpg', top: '50%', left: '20%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null },
                { id: 2112, name: 'Çamurlu Ayak İzi', desc: 'Halının kenarında belirgin bir şekilde duran, kasabaya yabancı bir çizme izi.', img: 'images/towns/sisoren/deliller/placeholder.jpg', top: '70%', left: '50%', fingerprintSpot: null, bloodSpot: null },
                { id: 2113, name: 'Kırık Cam Parçası', desc: 'Pencerenin kenarında içeriden dışarıya doğru kırılmış küçük bir cam parçası.', img: 'images/towns/sisoren/deliller/placeholder.jpg', top: '40%', left: '80%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: { xRatio: 0.6, yRatio: 0.4, angle: 0 } }
            ]''',
    'kasabali_evi_2': '''            hotspots: [
                { id: 2121, name: 'Eski Fotoğraf Albümü', desc: 'Sayfalarından biri yeni koparılmış, kurbanla ilgili sırları barındıran albüm.', img: 'images/towns/sisoren/deliller/placeholder.jpg', top: '60%', left: '30%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null },
                { id: 2122, name: 'Zehirli Bitki Kökü', desc: 'Mutfak tezgahında bırakılmış, normalde bu yörede yetişmeyen zehirli bir bitki parçası.', img: 'images/towns/sisoren/deliller/placeholder.jpg', top: '45%', left: '60%', fingerprintSpot: null, bloodSpot: null },
                { id: 2123, name: 'Gümüş Düğme', desc: 'Yer minderinin arasına sıkışmış, maktulün ceketine ait olduğu anlaşılan gümüş düğme.', img: 'images/towns/sisoren/deliller/placeholder.jpg', top: '75%', left: '80%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null }
            ]''',
    'kasabali_evi_3': '''            hotspots: [
                { id: 2131, name: 'Kanlı Yün Yumağı', desc: 'Dokuma tezgahının altında bulunan, üzerine kan damlamış yün ip.', img: 'images/towns/sisoren/deliller/placeholder.jpg', top: '55%', left: '25%', fingerprintSpot: null, bloodSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 } },
                { id: 2132, name: 'Gizli Çekmece', desc: 'Sandığın içinde gizli bir bölme. İçinde kurbanın sahte kimliği bulunuyor.', img: 'images/towns/sisoren/deliller/placeholder.jpg', top: '65%', left: '70%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null },
                { id: 2133, name: 'Kopuk Kolye', desc: 'Yerdeki talaşların arasında parlayan, arbede sırasında koptuğu anlaşılan kolye.', img: 'images/towns/sisoren/deliller/placeholder.jpg', top: '80%', left: '45%', fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null }
            ]'''
}

# The trick is to replace the hotspots array within the building definition, but since my regex broke last time, I'll match exactly up to `hotspots:` and replace the whole array.
for key, replacement in hotspots_map.items():
    pattern = rf"(id:\s*'{key}',\s*[\s\S]*?)hotspots:\s*\[[\s\S]*?\]"
    content = re.sub(pattern, r"\1" + replacement, content)

# I will also add logic to SisorenEngine.js to handle the extraNpc logic if needed, but we already added mapPos so they should render correctly now!

with open('wwwroot/js/towns/sisoren/SisorenConfig.js', 'w', encoding='utf-8') as f:
    f.write(content)
