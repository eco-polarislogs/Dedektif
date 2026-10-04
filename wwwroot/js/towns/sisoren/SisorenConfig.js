/**
 * SİSÖREN KASABASI KONFİGÜRASYON MODÜLÜ (v3.0)
 * Dağ Yamacında Yoğun Sisli, Karanlık Noir Tema
 * 100% Türkçe Tabelalı 11 Bina + 2 Kasabalı Evi = 13 Girilebilir Alan
 * NPC Aralığı: 201-212 | Delil ID: 2011-xxxx
 * Otopsi Sayacı: 5 Bina + 5 Delil
 */

window.SISOREN_CONFIG = {
    townId: 'sisoren',
    townName: 'Sisören',
    mapImage: 'images/towns/sisoren/sisoren_map_v5.jpg',

    // Otopsi sayacı gereksinimleri (5 Bina + 5 Lab)
    requiredBuildings: 5,
    requiredLabs: 5,

    storyIntroText: "Uçsuz bucaksız dağların zirvesinde, çam ormanlarıyla çevrili, dış dünyadan kopuk tekinsiz bir dağ kasabası: SİSÖREN...\n\nSarp yamaçların arasına sıkışmış bu kasabanın üzerine, göz gözü görmeyen ağır ve yoğun bir sis tabakası çökmüş durumda. Telgraf tellerinin rüzgardaki ürkütücü vızıltısı, bakkalın önünde çürümeye yüz tutmuş ahşap kasalar ve tüpçünün deposundan sızan hafif gaz kokusu... Burada sırlar, sadece fısıltılarla dağdan dağa yayılır.\n\nKasabanın ileri gelenlerinden biri, sisli bir gece yarısı esrarengiz bir şekilde vahşice öldürüldü. 13 farklı mekan, 13 farklı şüpheli. Kimin dost, kimin düşman olduğu belirsiz. Bu dağ kasabasında paranın, yalanların ve gizli ittifakların izini sürmen gerekecek. Şüpheliler listesini bu sefer sen oluşturuyorsun... Sis perdesini aralamaya ve katili bulmaya hazır mısın?",

    assistants: {
        primary: {
            name: 'Yardımcı Dedektif Çetin',
            portrait: 'images/dedektif_helper.png',
            subtitle: 'Olay Yeri & Adli Analiz Uzmanı'
        },
        introDialogue: [
            { speaker: 'Çetin', text: 'Sisören\'e nihayet ulaştık amirim. Burası Gölge Şehir\'den bile daha soğuk ve karanlık. Her yeri sis kaplamış.' },
            { speaker: 'İlyas', text: 'Sisören kasabasına hoş geldiniz amirim. Buralar tehlikelidir, ama ben arkanızdayım. Dağcı Tüccar İlyas emrinizde.' },
            { speaker: 'Çetin', text: 'Sen de kimsin? Olay yerine sivillerin girmesi yasak!' },
            { speaker: 'İlyas', text: 'Ben bu dağları avcumun içi gibi bilirim memur bey. Sisören\'de kimin ne sakladığını, kimin kimin kuyusunu kazdığını benden iyi kimse bilemez. Katili bulmak istiyorsanız rehberliğime ihtiyacınız olacak.' },
            { speaker: 'Çetin', text: 'Amirim, bu adama güvenebilir miyiz? Şüphelilerin evlerini ve binaları gezerken bu tüccarın söylediklerine dikkat etmeliyiz.' },
            { speaker: 'İlyas', text: 'Bana güvenebilirsiniz amirim. Şimdi, soruşturmaya binaları gezerek başlayalım. Her binanın gizli bir hikayesi vardır, yeter ki nereye bakacağınızı bilin.' }
        ]
    },

    // 13 SUÇLANABİLİR ŞÜPHELİ (7 KADIN, 6 ERKEK) + BAZI BİNALARDA SUÇLANAMAYAN MASUM ÇOCUKLAR
    buildings: [
        // 1. TELGRAFHANE (NPC 201) — Rüstem (Erkek #1)
        {
            id: 'telgrafhane',
            npcId: 201,
            title: 'Telgrafhane',
            hoverTag: 'TELGRAFHANE',
            icon: 'fa-solid fa-tower-broadcast',
            hasSign: true,
            interiorImg: 'images/towns/sisoren/interiors/telgrafhane_interior.jpg',
            details: 'Çatısında yüksek telgraf anteni, elektrik telleri ve pencereleri olan dağ zirve iletişim merkezi.',
            style: { top: '11%', left: '50%', width: '11%', height: '17%' },
            npc: {
                id: 201,
                name: 'Telgrafçı Rüstem',
                gender: 'male',
                building: 'Telgrafhane',
                role: 'Telgrafçı',
                portrait: 'images/towns/sisoren/npcler/npc_201.jpg',
                bg: 'images/towns/sisoren/interiors/telgrafhane_interior.jpg',
                talkBg: 'images/towns/sisoren/interiors/telgrafhane_interior.jpg',
                greeting: 'Tik tak tik tak... Bu telgraf telleri her şeyi duyar amirim.',
                questions: ['Cinayet gecesi telgrafhaneye mesaj geldi mi?', 'Hangi kasabalılar düzenli telgraf çekiyor?', 'Şifreli mesajlar hakkında ne biliyorsun?', 'O gece tellerde garip bir sinyal fark ettin mi?'],
                murderStory: 'Telgrafçı Rüstem, yıllardır kasabanın en gizli sırlarını o cızırtılı tellerin ucunda dinleyen adamdı. O gece kurbandan gelen şantaj telgrafını bizzat çözdüğünde, geçmişindeki o karanlık lekenin ifşa edileceğini anladı. Telgrafhanenin loş, ahşap odasında, tellerin aralıksız vızıltısı beynini kemirirken kontrolünü tamamen kaybetti. Gece yarısı yoğun sisin çöktüğü dağ yolunda kurbanı kıstırdı. Kurban daha ne olduğunu anlayamadan, Rüstem elindeki sert kauçuk kaplamalı, kalın telgraf kablosunu acımasızca boynuna doladı. Kurbanın son çırpınışları sisin içinde kaybolurken, nefesi tamamen kesilene kadar kabloyu sıktı. Bedenini ormanın derinliklerinde çürümeye terk edip, ellerindeki kanı bile tam yıkamadan makinesinin başına döndü; hiçbir şey olmamış gibi, ölümcül bir soğukkanlılıkla mors alfabesi tuşlarına basmaya devam etti.'
            },
                        hotspots: [
                { id: 2011, name: 'Şifreli Telgraf Şeridi', desc: 'Çöpe atılmış, sadece son birkaç kelimesi ("...gece yarısı gölde...") okunabilen yırtık bir mesaj kopyası.', img: 'images/towns/sisoren/deliller/2011.jpg', top: '75%', left: '20%', fingerprintSpot: { xRatio: 0.69, yRatio: 0.72, angle: -35 }, bloodSpot: { xRatio: 0.6, yRatio: 0.4, angle: 0 } },
                { id: 2012, name: 'Kanlı Parmak İzi', desc: 'Mors alfabesi tuş takımının (telgraf manilesi) tam altına bulaşmış ve silinmeyi unutulmuş taze bir iz.', img: 'images/towns/sisoren/deliller/2012.jpg', top: '55%', left: '40%', fingerprintSpot: { xRatio: 0.77, yRatio: 0.30, angle: 26 }, bloodSpot: { xRatio: 0.68, yRatio: 0.65, angle: 27 } },
                { id: 2013, name: 'Rüşvet Kesesi', desc: 'Telgraf memurunun masasının gizli çekmecesinde duran, kasaba dışından gönderilmiş isimsiz yüklü miktar para.', img: 'images/towns/sisoren/deliller/2013.jpg', top: '65%', left: '60%', fingerprintSpot: { xRatio: 0.3, yRatio: 0.7, angle: 0 }, bloodSpot: null },
                { id: 2014, name: 'Kesik Hat Kablosu', desc: 'Arka odada kasten kesilmiş ve iletişimi koparmak için saklanmış yedek telgraf telleri.', img: 'images/towns/sisoren/deliller/2014.jpg', top: '35%', left: '80%', fingerprintSpot: { xRatio: 0.2, yRatio: 0.2, angle: 0 }, bloodSpot: { xRatio: 0.8, yRatio: 0.8, angle: 0 } },
                { id: 2015, name: 'Tehdit Mesajı Taslağı', desc: 'Karalama kâğıtlarının arasında bulunan, gönderici adı karalanmış tehditkâr bir not.', img: 'images/towns/sisoren/deliller/2015.jpg', top: '45%', left: '10%', fingerprintSpot: { xRatio: 0.7, yRatio: 0.3, angle: 0 }, bloodSpot: null }
            ]
        },
        // 2. KAHVEHANE (NPC 202) — İrfan (Erkek #2)
        {
            id: 'kahvehane',
            npcId: 202,
            title: 'Kahvehane',
            hoverTag: 'KAHVEHANE',
            icon: 'fa-solid fa-mug-hot',
            hasSign: true,
            interiorImg: 'images/towns/sisoren/interiors/kahvehane_interior.jpg',
            details: 'Önünde tahta tabureler, dumanı tüten semaver ve çay bardakları bulunan dağ kahvehanesi.',
            style: { top: '12%', left: '62.5%', width: '12%', height: '17%' },
            npc: {
                id: 202,
                name: 'Kahveci İrfan',
                gender: 'male',
                building: 'Kahvehane',
                role: 'Kahveci',
                portrait: 'images/towns/sisoren/npcler/npc_202.jpg',
                bg: 'images/towns/sisoren/interiors/kahvehane_interior.jpg',
                talkBg: 'images/towns/sisoren/interiors/kahvehane_interior.jpg',
                greeting: 'Buyurun amirim, taze demlenmiş dağ çayı... Sohbet de bedava.',
                questions: ['Cinayet gecesi kahvehanede kimler vardı?', 'En son kim geç saatte kaldı?', 'Kasabalılar arasında kavga çıktı mı?', 'Ekrem hakkında dedikodular neler?'],
                murderStory: 'Kahveci İrfan, yılların yorgunluğunu demliğiyle atan, ancak öfkesini de içinde demleyen bir adamdı. Kurbanın, kahvehaneyi hacizle elinden almak için yıllardır sinsi bir kumpas kurduğunu o geceki son tartışmalarında kesin olarak öğrendi. Kahvehanedeki son müşteri de kalkıp yoğun sise karıştıktan sonra İrfan kepenkleri sertçe kapattı. Çay ocağının altındaki gizli zuladan, dedesinden kalma altıpatlar tabancasını çıkardı. Gecenin dondurucu karanlığında kurbanı dağ yoluna kadar adım adım takip etti. Rüzgarın ıslığı merminin sesini bastırırken, yılların birikmiş öfkesiyle tetiği tek seferde, tam kalbine nişan alarak çekti. Kurban yere yığılırken yüzünde sadece soğuk bir ifade vardı. Sabaha karşı kahvehaneye sessizce geri döndü, ocağın ateşini harlayıp, kasabalılara ikram edeceği o taze sabah çayını hiçbir vicdan azabı çekmeden demlemeye koyuldu.'
            },
                        hotspots: [
                { id: 2021, name: 'Gizli Mesajlı Fincan', desc: 'Altına sert bir cisimle buluşma saati kazınmış, masada yarım bırakılmış kulpsuz kahve fincanı.', img: 'images/towns/sisoren/deliller/2021.jpg', top: '60%', left: '30%', fingerprintSpot: { xRatio: 0.72, yRatio: 0.32, angle: 37 }, bloodSpot: null },
                { id: 2022, name: 'İşaretli Okey Taşı', desc: 'Yerdeki talaşların arasına düşmüş, üzerinde yasa dışı bir örgütün sembolü kazınmış sahte bir taş.', img: 'images/towns/sisoren/deliller/2022.jpg', top: '80%', left: '50%', fingerprintSpot: { xRatio: 0.77, yRatio: 0.51, angle: -45 }, bloodSpot: null },
                { id: 2023, name: 'Yırtık Borç Listesi', desc: 'Kırık bir sandalyenin bacağına sıkıştırılmış, bazı isimlerin üzeri kırmızıyla çizilmiş kâğıt.', img: 'images/towns/sisoren/deliller/2023.jpg', top: '75%', left: '70%', fingerprintSpot: { xRatio: 0.4, yRatio: 0.6, angle: 0 }, bloodSpot: null },
                { id: 2024, name: 'Yanık Kasket', desc: 'Sobanın içine atılmış ama tam yanmamış, kenarında kan lekesi bulunan tanıdık bir şapka.', img: 'images/towns/sisoren/deliller/2024.jpg', top: '40%', left: '85%', fingerprintSpot: { xRatio: 0.41, yRatio: 0.30, angle: -34 }, bloodSpot: { xRatio: 0.6, yRatio: 0.2, angle: 0 } },
                { id: 2025, name: 'Zulalanmış Altıpatlar', desc: 'Çay ocağının altındaki tahtaların arkasına gizlenmiş, bir mermisi eksik ruhsatsız tabanca.', img: 'images/towns/sisoren/deliller/2025.jpg', top: '50%', left: '15%', fingerprintSpot: { xRatio: 0.3, yRatio: 0.8, angle: 0 }, bloodSpot: { xRatio: 0.7, yRatio: 0.5, angle: 0 } }
            ]
        },
        // 3. SİNEMA (NPC 203) — Nejat (Erkek #3)
        {
            id: 'sinema',
            npcId: 203,
            title: 'Sinema',
            hoverTag: 'SİNEMA',
            icon: 'fa-solid fa-film',
            hasSign: true,
            interiorImg: 'images/towns/sisoren/interiors/sinema_interior.jpg',
            details: 'Işıklı sarı SİNEMA tabelası ve duvarında nostaljik film afişleri bulunan sinema salonu.',
            style: { top: '22%', left: '72%', width: '11%', height: '21%' },
            npc: {
                id: 203,
                name: 'Sinemacı Nejat',
                gender: 'male',
                building: 'Sinema',
                role: 'Sinemacı',
                portrait: 'images/towns/sisoren/npcler/npc_203.jpg',
                bg: 'images/towns/sisoren/interiors/sinema_interior.jpg',
                talkBg: 'images/towns/sisoren/interiors/sinema_interior.jpg',
                greeting: 'Film bitti ama perde henüz inmedi amirim...',
                questions: ['O gece sinema salonunda kimler vardı?', 'Arka kapıdan biri girip çıktı mı?', 'Film makinesi odasında saklanan ne?', 'Karanlık salonda şüpheli bir şey gördün mü?'],
                murderStory: 'Sinemacı Nejat, hayatı hep perdelerdeki kurmaca hikayelerden ibaret sanırdı; ta ki kurban, onun yasadışı işlerini ve gizli kaçakçılık ağını eski bir film rulosuna saniye saniye kaydedene kadar. O gece son seansın ardından kurban, şantaj için makine dairesine geldiğinde Nejat\'ın gözü dönmüştü. İkili arasındaki itiş kakış sırasında Nejat, sinema dünyasının o sahte büyüsünü gerçeğe çevirdi; kurbanı, devasa film projeksiyon makinesinin dönen keskin çarklarının arasına acımasızca itti. Çarklar kurbanın bedenini ezerken, Nejat kasten perdedeki filmin sesini sonuna kadar açıp çığlıkları bastırdı. Kan izlerini ve yırtılan giysi parçalarını ustalıkla temizledikten sonra, perdede dönen mutlu son sahnesini, elinde bir kadeh içkiyle, hayatının en karanlık başyapıtı gibi seyretti.'
            },
                        hotspots: [
                { id: 2031, name: 'Kesilmiş Film Bobini', desc: 'Makine dairesinde yer alan bir filmin en kritik sahnesinin kasten kesilip saklanmış parçası.', img: 'images/towns/sisoren/deliller/2031.jpg', top: '40%', left: '25%', fingerprintSpot: { xRatio: 0.37, yRatio: 0.56, angle: -19 }, bloodSpot: null },
                { id: 2032, name: 'Şantajlı Afiş', desc: 'Gişe memurunun çekmecesinde, arka yüzüne "Ne yaptığını biliyorum" yazılmış eski bir film afişi.', img: 'images/towns/sisoren/deliller/2032.jpg', top: '65%', left: '15%', fingerprintSpot: { xRatio: 0.8, yRatio: 0.2, angle: 0 }, bloodSpot: null },
                { id: 2033, name: 'Düşmüş Bilet Koçanı', desc: 'Arka sıradaki koltuklardan birinin altına düşmüş, cinayet saatine ait yırtık bir bilet parçası.', img: 'images/towns/sisoren/deliller/2033.jpg', top: '85%', left: '45%', fingerprintSpot: { xRatio: 0.70, yRatio: 0.62, angle: 1 }, bloodSpot: null },
                { id: 2034, name: 'Kanlı Deri Eldiven', desc: 'Perdenin arkasındaki karanlık köşede unutulmuş, pahalı bir kumaştan yapılmış eldiven teki.', img: 'images/towns/sisoren/deliller/2034.jpg', top: '70%', left: '75%', fingerprintSpot: null, bloodSpot: { xRatio: 0.78, yRatio: 0.63, angle: 39 } },
                { id: 2035, name: 'Rujlu Aynadaki Saat', desc: 'Sahne arkasındaki eski bir makyaj aynasına aceleyle rujla yazılmış bir rıhtım adresi.', img: 'images/towns/sisoren/deliller/2035.jpg', top: '50%', left: '85%', fingerprintSpot: { xRatio: 0.2, yRatio: 0.8, angle: 0 }, bloodSpot: null }
            ]
        },
        // 4. BAKKAL (NPC 204) — Cemile (Kadın #1) + Gofret Alan Kız Çocuk
        {
            id: 'bakkal',
            npcId: 204,
            title: 'Bakkal',
            hoverTag: 'BAKKAL',
            icon: 'fa-solid fa-store',
            hasSign: true,
            interiorImg: 'images/towns/sisoren/interiors/bakkal_interior.jpg',
            details: 'Önünde taze dağ ekmekleri ve meyve-sebze sandıkları olan karanlık dağ bakkalı.',
            style: { top: '38%', left: '38%', width: '11.5%', height: '16.5%' },
            npc: {
                id: 204,
                name: 'Bakkal Cemile',
                gender: 'female',
                building: 'Bakkal',
                role: 'Bakkal',
                portrait: 'images/towns/sisoren/npcler/npc_204.jpg',
                bg: 'images/towns/sisoren/interiors/bakkal_interior.jpg',
                talkBg: 'images/towns/sisoren/interiors/bakkal_interior.jpg',
                greeting: 'Hoş geldiniz amirim, burada her şey taze... Dedikodular da dahil.',
                questions: ['Son günlerde dükkâna gelen şüpheli biri oldu mu?', 'Veresiye defterinde silinen isim kimin?', 'O gece dükkândan çalınan ne?', 'Kasabalıların borç kavgaları hakkında ne biliyorsun?'],
                murderStory: 'Bakkal Cemile, yıllardır kuruş hesabı yaparak ayakta tuttuğu o küçük dükkânın, kurbanın bitmek bilmeyen borçları ve bitmek bilmeyen tehditleri yüzünden elinden kayıp gitmesine seyirci kalamazdı. Kurbanın veresiye defterindeki kabaran borcu bir yana, dükkâna el koyma planı Cemile\'nin son sabır damlasını da taşırdı. O gece kurban, sırıtarak haftalık erzaklarını almaya geldiğinde, Cemile ona her zamankinden daha kibar davrandı. Ancak kurbanın paketlediği pahalı ithal peynirin içine, arka odada fareler için özel olarak sakladığı o son derece zehirli, ağır kimyasalı şırıngayla dikkatlice enjekte etmişti. Kurban evinde o lokmayı yuttuğu an kalbi durdu ve ölümü tamamen doğal bir kalp krizi gibi kayıtlara geçti. Ta ki bakkalın karanlık kilerinde unutulan o içi boşaltılmış zehir şişesi asıl gerçeği tüm vahşetiyle haykırana dek.'
            },
            // Bakkalda gofret alan kız çocuğu (Konuşulabilir, suçlanamaz)
            children: [
                {
                    id: 'bakkal_cocuk_1',
                    numericId: 315,
                    name: 'Küçük Elif (Gofret Alan Kız)',
                    gender: 'female',
                    role: 'Çocuk Müşteri',
                    portrait: 'images/towns/sisoren/npcler/npc_315.jpg',
                    isChild: true,
                    canAccuse: false,
                    greeting: 'Amca bana bakkal teyze çilekli gofret verdi... Dün gece dışarıda çok koşan birini gördüm!',
                    questions: [
                        { q: 'Dün gece dışarıda kimi gördün?', a: 'Çok uzun boylu biriydi... Simsiyah bir paltosu vardı, yüzünü göremedim amca.', difficulty: 1, category: 'tanisma' },
                        { q: 'Gofretini nereden aldın?', a: 'Bakkal Cemile teyze verdi! Ama verirken elleri çok titriyordu...', difficulty: 1, category: 'tanisma' },
                        { q: 'Bakkalda garip bir ses duydun mu?', a: 'Evet! Arka odadan tıkır tıkır sesler geliyordu, sanki biri ağır kutuları çekiyordu.', difficulty: 2, category: 'derinlesme' },
                        { q: 'Paltolu adam nereye koşuyordu?', a: 'Sinemaya doğru koşuyordu, sonra sisin içinde kayboldu.', difficulty: 2, category: 'derinlesme' }
                    ]
                }
            ],
                        hotspots: [
                { id: 2041, name: 'Saklanmış Fare Zehri', desc: 'Tezgâhın arkasına gizlenmiş, yarısı kullanılmış ve kutusu ezilmiş ağır bir kimyasal zehir.', img: 'images/towns/sisoren/deliller/2041.jpg', top: '55%', left: '20%', fingerprintSpot: { xRatio: 0.55, yRatio: 0.63, angle: 28 }, bloodSpot: null },
                { id: 2042, name: 'Kopuk Veresiye Sayfası', desc: 'Veresiye defterinde, cinayet gecesine ait sayfaların kasten koparılmış olması.', img: 'images/towns/sisoren/deliller/2042.jpg', top: '45%', left: '60%', fingerprintSpot: { xRatio: 0.7, yRatio: 0.3, angle: 0 }, bloodSpot: null },
                { id: 2043, name: 'Yabancı Gümüş Sikke', desc: 'Kasanın yanına düşmüş, kasabadan kimseye ait olmayan ve başka bir şehre ait para.', img: 'images/towns/sisoren/deliller/2043.jpg', top: '65%', left: '70%', fingerprintSpot: { xRatio: 0.41, yRatio: 0.36, angle: -12 }, bloodSpot: null },
                { id: 2044, name: 'Kaçak Tütün', desc: 'Rafların en arkasında, un çuvallarının ardına zulalanmış faturasız ithal tütün paketleri.', img: 'images/towns/sisoren/deliller/2044.jpg', top: '30%', left: '85%', fingerprintSpot: { xRatio: 0.34, yRatio: 0.52, angle: 39 }, bloodSpot: null },
                { id: 2045, name: 'İsimli Senet', desc: 'Yerdeki boş kasaların arasına düşmüş, üzerinde kurbanın adının yazılı olduğu buruşuk bir borç senedi.', img: 'images/towns/sisoren/deliller/2045.jpg', top: '80%', left: '30%', fingerprintSpot: { xRatio: 0.2, yRatio: 0.8, angle: 0 }, bloodSpot: { xRatio: 0.8, yRatio: 0.2, angle: 0 } }
            ]
        },
        // 5. SAHAF (NPC 205) — Hikmet (Erkek #4) + Kitap Okuyan 2 Çocuk
        {
            id: 'sahaf',
            npcId: 205,
            title: 'Sahaf',
            hoverTag: 'SAHAF',
            icon: 'fa-solid fa-book-bookmark',
            hasSign: true,
            interiorImg: 'images/towns/sisoren/interiors/sahaf_interior.jpg',
            details: 'Sundurması altında sararmış kitaplar ve el yazmaları sergilenen tozlu sahaf.',
            style: { top: '41.5%', left: '49%', width: '10%', height: '17%' },
            npc: {
                id: 205,
                name: 'Sahaf Hikmet',
                gender: 'male',
                building: 'Sahaf',
                role: 'Sahaf',
                portrait: 'images/towns/sisoren/npcler/npc_205.jpg',
                bg: 'images/towns/sisoren/interiors/sahaf_interior.jpg',
                talkBg: 'images/towns/sisoren/interiors/sahaf_interior.jpg',
                greeting: 'Kitaplarda her şeyin cevabı vardır amirim... Cinayetin de.',
                questions: ['El yazmalarında şifreli notlar var mı?', 'Son müşterin kim ve ne aldı?', 'Eski haritalar arasında saklanan belge ne?', 'O gece dükkânına gelen oldu mu?'],
                murderStory: 'Sahaf Hikmet, hayatını eski sayfaların tozuna ve unutulmuş sırların büyüsüne adamış bir adamdı. Ancak kurban, dükkânın en nadide el yazmasının içindeki şifreyi gizlice çözmüş ve kasabanın altındaki efsanevi altınların haritasını ele geçirmişti. Hikmet, nesillerdir koruduğu bu sırrın basit bir açgözlü tarafından yağmalanmasına izin veremezdi. Gece yarısı dükkânın kapıları kapandıktan sonra kurban haritayı çalmaya yeltendiğinde, Hikmet kütüphanenin en karanlık köşesinde onun karşısına dikildi. Gözünü kırpmadan, eline geçirdiği Osmanlı döneminden kalma, demir kaplamalı ve oldukça ağır bir ansiklopediyi kurbanın kafasına defalarca, hınçla indirdi. Kanlar eski kitapların sayfalarına sıçrarken, Hikmet haritayı alıp cesedi gölgelerin arasına sürükledi; bilgi uğruna işlediği bu cinayeti, tozlu rafların ardına sonsuza dek gömdü.'
            },
            // Sahafta kitap okuyan 2 çocuk (Konuşulabilir, suçlanamaz)
            children: [
                {
                    id: 'sahaf_cocuk_1',
                    numericId: 316,
                    name: 'Can (Masal Kitabı Okuyan Çocuk)',
                    gender: 'male',
                    role: 'Kitapsever Çocuk',
                    portrait: 'images/towns/sisoren/npcler/npc_316.jpg',
                    isChild: true,
                    canAccuse: false,
                    greeting: 'Sessiz olun dedektif amca, Hikmet amca kızıyor... Biz eski haritaları inceliyoruz.',
                    questions: [
                        { q: 'Hangi kitabı okuyorsun?', a: 'Sisli Dağların Sırrı diye bir masal kitabı okuyorum. İçinde kayıp altınlardan bahsediyor!', difficulty: 1, category: 'tanisma' },
                        { q: 'Buraya gece gelen birini gördün mü?', a: 'Dün akşam dükkân kapanırken sert bakışlı bir adam geldi, harita sordu.', difficulty: 2, category: 'derinlesme' },
                        { q: 'Hikmet amca ne yapıyordu?', a: 'Kalın bir kitabı oyuyordu, içine bir kağıt sakladı.', difficulty: 2, category: 'derinlesme' },
                        { q: 'Başka dikkatini çeken bir şey oldu mu?', a: 'Vitrin camında kurumuş bir leke gördüm, kırmızı kırmızı...', difficulty: 3, category: 'yuzlestirme' }
                    ]
                },
                {
                    id: 'sahaf_cocuk_2',
                    numericId: 317,
                    name: 'Selin (Resim Çizen Kız Çocuğu)',
                    gender: 'female',
                    role: 'Resim Çizen Çocuk',
                    portrait: 'images/towns/sisoren/npcler/npc_317.jpg',
                    isChild: true,
                    canAccuse: false,
                    greeting: 'Ben sisli dağları çiziyorum dedektif amca... Çizimimdeki şu siyah paltolu adamı dün gece gördüm!',
                    questions: [
                        { q: 'Çizdiğin siyah paltolu adam kim?', a: 'Bilmiyorum, yüzü görünmüyordu. Sadece elinde parlak bir şey tutuyordu.', difficulty: 1, category: 'tanisma' },
                        { q: 'O adam nereye doğru yürüyordu?', a: 'Göl kenarına doğru, hurdacının oralara gidiyordu.', difficulty: 2, category: 'derinlesme' },
                        { q: 'Parlak şey neye benziyordu?', a: 'Bıçak gibiydi amca, ama sapı çok garipti, gümüş gibi parlıyordu.', difficulty: 3, category: 'yuzlestirme' },
                        { q: 'Başka resim çizdin mi?', a: 'Evet, muhtar teyzenin de resmini çizdim. O da gece yarısı dışarıdaydı.', difficulty: 2, category: 'derinlesme' }
                    ]
                }
            ],
                        hotspots: [
                { id: 2051, name: 'İçi Oyulmuş Ansiklopedi', desc: 'Sayfaları kesilerek gizli bir kutu haline getirilmiş, içinde şifreli bir harita barındıran kalın ciltli kitap.', img: 'images/towns/sisoren/deliller/2051.jpg', top: '45%', left: '20%', fingerprintSpot: { xRatio: 0.61, yRatio: 0.27, angle: -21 }, bloodSpot: null },
                { id: 2052, name: 'Şifreli Kenar Notları', desc: 'Kasaba tarihini anlatan eski bir kitabın kenarlarına kırmızı mürekkeple düşülmüş garip rakamlar.', img: 'images/towns/sisoren/deliller/2052.jpg', top: '35%', left: '50%', fingerprintSpot: { xRatio: 0.8, yRatio: 0.2, angle: 0 }, bloodSpot: null },
                { id: 2053, name: 'Kurumuş Kan Damlası', desc: 'Vitrindeki satılık eski bir cep saatinin kapağına içeriden yapışmış şüpheli leke.', img: 'images/towns/sisoren/deliller/2053.jpg', top: '55%', left: '75%', fingerprintSpot: null, bloodSpot: { xRatio: 0.29, yRatio: 0.40, angle: -10 } },
                { id: 2054, name: 'Kasa İzi', desc: 'Yerdeki kalın toz tabakasında, kısa süre önce ağır bir eşyanın (veya kasanın) yerinden çekildiğini gösteren sürtünme izi.', img: 'images/towns/sisoren/deliller/2054.jpg', top: '85%', left: '40%', fingerprintSpot: null, bloodSpot: null },
                { id: 2055, name: 'Sahte Vasiyetname', desc: 'Nadir eserlerin arasına sıkıştırılmış, kurbanın imzasının taklit edildiği yırtık bir vasiyet taslağı.', img: 'images/towns/sisoren/deliller/2055.jpg', top: '65%', left: '10%', fingerprintSpot: { xRatio: 0.3, yRatio: 0.7, angle: 0 }, bloodSpot: null }
            ]
        },
        // 6. MUHTARLIK (NPC 206) — Muhtar Meliha (Kadın #2)
        {
            id: 'muhtarlik',
            npcId: 206,
            title: 'Muhtarlık',
            hoverTag: 'MUHTARLIK',
            icon: 'fa-solid fa-building-flag',
            hasSign: true,
            interiorImg: 'images/towns/sisoren/interiors/muhtarlik_interior.jpg',
            details: 'Sağlam gri taştan yapılmış iki katlı resmi köy muhtarlık binası.',
            style: { top: '39%', left: '59.5%', width: '14.5%', height: '23%' },
            npc: {
                id: 206,
                name: 'Muhtar Meliha Hanım',
                gender: 'female',
                building: 'Muhtarlık',
                role: 'Köy Muhtarı',
                portrait: 'images/towns/sisoren/npcler/npc_206.jpg',
                bg: 'images/towns/sisoren/interiors/muhtarlik_interior.jpg',
                talkBg: 'images/towns/sisoren/interiors/muhtarlik_interior.jpg',
                greeting: 'Bu dağ kasabasında herkes birbirini tanır amirim. Sırlar sisin ardına saklanır.',
                questions: ['Arazi kavgaları kimler arasında?', 'Resmi mühürü en son kim kullandı?', 'Cinayet gecesi köy meydanında kimler vardı?', 'Kasabanın karanlık geçmişi hakkında ne biliyorsun?'],
                murderStory: 'Muhtar Meliha Hanım, kasabadaki o sarsılmaz otoritesini korumak için elini kana bulamaktan asla çekinmeyecek kadar hırslı bir kadındı. Kurban, onun resmi mühürleri kullanarak kasabanın en değerli arazilerini kendi üstüne geçirdiğini belgeleyen sahte evrakları bulmuş ve tüm kasabaya duyurmakla tehdit etmişti. Meliha Hanım, o gece kurbanı "anlaşmak" bahanesiyle, kimsenin olmadığı resmi muhtarlık binasına çağırdı. Kurban daha konuyu açamadan, Meliha Hanım ona kendi elleriyle doldurduğu, içine ölümcül bir felç edici zehir kattığı o acı dağ kahvesini ikram etti. Kurban saniyeler içinde nefessiz kalıp yere yığılırken, Meliha Hanım büyük bir soğukkanlılıkla kurbanın cebindeki tüm sahte evrakları alıp şöminede yaktı. Cesedi ise gece yarısı ormanın girişine taşıyıp, vahşi bir hayvan saldırısı süsü verdi.'
            },
                        hotspots: [
                { id: 2061, name: 'Asitle Silinmiş Kütük', desc: 'Nüfus kütük defterinde, maktulün ve ailesinin adının üzerinin asit/mürekkep ile yok edilmeye çalışıldığı sayfa.', img: 'images/towns/sisoren/deliller/2061.jpg', top: '50%', left: '40%', fingerprintSpot: { xRatio: 0.41, yRatio: 0.22, angle: -9 }, bloodSpot: null },
                { id: 2062, name: 'Kırık Mühür', desc: 'Sahte belgelere basılırken fazla baskı uygulandığı için kenarı çatlamış resmi kasaba damgası.', img: 'images/towns/sisoren/deliller/2062.jpg', top: '55%', left: '60%', fingerprintSpot: { xRatio: 0.27, yRatio: 0.65, angle: -44 }, bloodSpot: null },
                { id: 2063, name: 'Gizli Orman Tapusu', desc: 'Kilitli dolabın dibinde bulunan, kasabanın dışındaki kime ait olduğu belirsiz bir arazi haritası.', img: 'images/towns/sisoren/deliller/2063.jpg', top: '75%', left: '20%', fingerprintSpot: { xRatio: 0.2, yRatio: 0.2, angle: 0 }, bloodSpot: null },
                { id: 2064, name: 'Boş Kovan', desc: 'Muhtarın masasının altına, süpürgeliğin dibine yuvarlanmış cinayet silahına ait mermi kovanı.', img: 'images/towns/sisoren/deliller/2064.jpg', top: '85%', left: '50%', fingerprintSpot: { xRatio: 0.62, yRatio: 0.35, angle: -38 }, bloodSpot: null },
                { id: 2065, name: 'Parçalanmış Uyarı Mektubu', desc: 'Çöp kutusunda bulunan, üst makamlardan gelmiş ancak yırtılıp yok edilmek istenen resmi evrak.', img: 'images/towns/sisoren/deliller/2065.jpg', top: '65%', left: '80%', fingerprintSpot: { xRatio: 0.30, yRatio: 0.36, angle: 2 }, bloodSpot: null }
            ]
        },
        // 7. TÜTÜNCÜ (NPC 207) — Nermin (Kadın #3)
        {
            id: 'tutuncu',
            npcId: 207,
            title: 'Tütüncü',
            hoverTag: 'TÜTÜNCÜ',
            icon: 'fa-solid fa-smoking',
            hasSign: true,
            interiorImg: 'images/towns/sisoren/interiors/tutuncu_interior.jpg',
            details: 'Saçaklarında ipe dizili kurutulmuş tütün yaprakları sarkan dağ tütüncüsü.',
            style: { top: '45.5%', left: '72.5%', width: '13%', height: '20%' },
            npc: {
                id: 207,
                name: 'Tütüncü Nermin Hanım',
                gender: 'female',
                building: 'Tütüncü',
                role: 'Tütüncü',
                portrait: 'images/towns/sisoren/npcler/npc_207.jpg',
                bg: 'images/towns/sisoren/interiors/tutuncu_interior.jpg',
                talkBg: 'images/towns/sisoren/interiors/tutuncu_interior.jpg',
                greeting: 'Duman her zaman bir iz bırakır amirim...',
                questions: ['Özel karışım tütününü kimler satın alıyor?', 'O gece dükkânın arka odasında ne oldu?', 'Tütün yaprakları arasında saklanan ne?', 'Kaçak tütün ticareti hakkında ne biliyorsun?'],
                murderStory: 'Tütüncü Nermin, o zarif görüntüsünün altında acımasız bir kaçakçılık şebekesini yönetiyordu. Sattığı o pahalı, ithal tütünlerin aslında yasadışı yollarla kasabaya sokulduğunu tesadüfen öğrenen kurban, sus payı olarak Nermin\'den yüklü miktarda haraç istemeye kalkıştı. O gece yoğun sisin altında, tütün deposunun o boğucu ve keskin kokulu havasında ikili arasında çıkan tartışma kısa sürede ölümcül bir kavgaya dönüştü. Nermin, kurbanın arkasını dönmesini fırsat bilip, tezgahta duran içi kurşun dolu, antika ve son derece ağır o meşe pipoyu kurbanın şakağına acımasızca, tüm gücüyle indirdi. Cesetten sızan kanlar tütün yapraklarına karışırken, Nermin bedeni tütün balyalarının en diplerine gömdü. Çürüyen bedenin kokusunu bastırmak için günlerce dükkânında en ağır, baharatlı tütsüleri yaktı.'
            },
                        hotspots: [
                { id: 2071, name: 'İthal Puro İzmariti', desc: 'Normalde kasabada satılmayan, sadece çok zengin bir şüpheliye ait olduğu bilinen yarım içilmiş puro.', img: 'images/towns/sisoren/deliller/2071.jpg', top: '65%', left: '30%', fingerprintSpot: { xRatio: 0.69, yRatio: 0.61, angle: -32 }, bloodSpot: null },
                { id: 2072, name: 'Şifreli Sigara Kâğıdı', desc: 'Sigara sarma kâğıtlarının içine gizlenmiş, ısıtıldığında ortaya çıkan mürekkeple yazılmış adres listesi.', img: 'images/towns/sisoren/deliller/2072.jpg', top: '45%', left: '55%', fingerprintSpot: { xRatio: 0.45, yRatio: 0.69, angle: 28 }, bloodSpot: null },
                { id: 2073, name: 'İçi Boşaltılmış Pipo', desc: 'Şüpheli bir şekilde ağırlaşmış, tütün haznesinin içine rulo yapılmış banknotlar sıkıştırılmış pipo.', img: 'images/towns/sisoren/deliller/2073.jpg', top: '55%', left: '75%', fingerprintSpot: { xRatio: 0.43, yRatio: 0.60, angle: -9 }, bloodSpot: null },
                { id: 2074, name: 'Lekeli Gümüş Tabaka', desc: 'Kasanın altında, üzerinde temizlenmeye çalışılmış kan izleri bulunan şık bir sigara tabakası.', img: 'images/towns/sisoren/deliller/2074.jpg', top: '75%', left: '45%', fingerprintSpot: { xRatio: 0.2, yRatio: 0.8, angle: 0 }, bloodSpot: { xRatio: 0.8, yRatio: 0.2, angle: 0 } },
                { id: 2075, name: 'Zehirli Sıvı Şişesi', desc: 'Tütün balyalarının arasına gizlenmiş, tütün aroması gibi kokan ama aslında felce yol açan küçük şişe.', img: 'images/towns/sisoren/deliller/2075.jpg', top: '35%', left: '20%', fingerprintSpot: { xRatio: 0.26, yRatio: 0.46, angle: 29 }, bloodSpot: null }
            ]
        },
        // 8. AHIR (NPC 208) — Çoban Durmuş (Erkek #5) + Çoban Olarak Yetiştirilen Çocuk (Oğlu Kerem)
        {
            id: 'ahir',
            npcId: 208,
            title: 'Ahır',
            hoverTag: 'AHIR',
            icon: 'fa-solid fa-cow',
            hasSign: true,
            signType: 'rustic_wood',
            interiorImg: 'images/towns/sisoren/interiors/ahir_interior.jpg',
            details: 'Rustik tahta tabelalı, çamurlu çitlerinde inekler, atlar, çoban köpeği ve saman balyaları bulunan geniş ahır.',
            style: { top: '54.5%', left: '18%', width: '20%', height: '23.5%' },
            npc: {
                id: 208,
                name: 'Çoban Durmuş',
                gender: 'male',
                building: 'Ahır',
                role: 'Çiftçi & Çoban',
                portrait: 'images/towns/sisoren/npcler/npc_208.jpg',
                bg: 'images/towns/sisoren/interiors/ahir_interior.jpg',
                talkBg: 'images/towns/sisoren/interiors/ahir_interior.jpg',
                greeting: 'Hayvanlar bile o gece huzursuzdu amirim... Oğlumu da çoban yetiştiriyorum, göz kulak oluyoruz sürüye.',
                questions: ['Ahırda saklanan yabancı kim?', 'Çamurlu ayak izleri nereye gidiyor?', 'Saman balyalarının altında ne var?', 'O gece hayvanlar neden rahatsız oldu?'],
                murderStory: 'Çoban Durmuş için sürüsü, bu dünyadaki her şeyden daha değerliydi. Kurbanın gizlice dereye akıttığı zehirli atıklar yüzünden Durmuş\'un en değer verdiği hayvanları tek tek can çekişerek ölmeye başlamıştı. Yasalar kurbanı koruyunca, Durmuş kendi adaletini sağlamaya yemin etti. O gece fırtınalı bir havada, kurbanı ıssız bir ahırda kıstırdı. Elindeki o paslı, sivri ve devasa nalbant çivisini hiç tereddüt etmeden kurbanın boynuna sapladı. Kurban kanlar içinde yere yığılırken, Durmuş\'un gözünde sadece ölen koyunlarının intikamı vardı. Bedeni atının arkasına bağlayıp, dağın görünmeyen, sarp ve kayalık arka yüzündeki uçurumdan aşağı fırlattı. Ertesi sabah, sanki gece hiç cinayet işlememiş gibi, o çamurlu çizmeleriyle sürüsünün başında ıslık çalarak otlatmaya devam etti.'
            },
            // Çoban olarak yetiştirilen oğlu (Konuşulabilir, suçlanamaz)
            children: [
                {
                    id: 'ahir_cocuk_1',
                    numericId: 318,
                    name: 'Çoban Çırağı Kerem (Durmuş\'un Oğlu)',
                    gender: 'male',
                    role: 'Çoban Çırağı',
                    portrait: 'images/towns/sisoren/npcler/npc_318.jpg',
                    isChild: true,
                    canAccuse: false,
                    greeting: 'Babam bana koyunları gütmeyi öğretiyor dedektif amca... Gece sürünün yanından hızla biri geçti!',
                    questions: [
                        { q: 'Sürünün yanından geçen adam nasıldı?', a: 'Çok hızlı koşuyordu, elinde feneri bile yoktu amca. Nefes nefeseydi.', difficulty: 1, category: 'tanisma' },
                        { q: 'Babana yardım ederken şüpheli bir iz buldun mu?', a: 'Evet, samanların arkasında yırtık bir mektup parçası buldum ama rüzgar uçurdu.', difficulty: 2, category: 'derinlesme' },
                        { q: 'Köpek Karabaş kime havladı?', a: 'Karabaş yabancılara hep havlar ama o gece havlamadı, demek ki tanıdık biriydi!', difficulty: 3, category: 'yuzlestirme' },
                        { q: 'O gece baban yanınızda mıydı?', a: 'Babam bir ara "su getireceğim" diye ahırdan çıktı, uzun süre gelmedi.', difficulty: 2, category: 'derinlesme' }
                    ]
                }
            ],
                        hotspots: [
                { id: 2081, name: 'Kesik Eyer Kayışı', desc: 'Maktulün atına ait, kazaya sebep olmak için kasten yarıya kadar kesilmiş deri eyer parçası.', img: 'images/towns/sisoren/deliller/2081.jpg', top: '50%', left: '25%', fingerprintSpot: null, bloodSpot: null },
                { id: 2082, name: 'Kanlı Nalbant Çivisi', desc: 'Saman balyalarının derinine gömülmüş, üzerinde doku ve kan izleri olan keskin bir at nalı çivisi.', img: 'images/towns/sisoren/deliller/2082.jpg', top: '70%', left: '15%', fingerprintSpot: null, bloodSpot: { xRatio: 0.70, yRatio: 0.75, angle: 26 } },
                { id: 2083, name: 'Kopuk Altın Zincir', desc: 'Hayvanların su yalağının dibinde parlayan, kurbana ait koptuğu belli olan cep saati zinciri.', img: 'images/towns/sisoren/deliller/2083.jpg', top: '80%', left: '55%', fingerprintSpot: null, bloodSpot: null },
                { id: 2084, name: 'Yabancı Bot İzi', desc: 'Hayvanların huzursuzlandığı köşede toprağa kalıbı çıkmış, kasabalıya ait olmayan sivri burunlu bot izi.', img: 'images/towns/sisoren/deliller/2084.jpg', top: '85%', left: '80%', fingerprintSpot: null, bloodSpot: null },
                { id: 2085, name: 'Saplı Bıçak', desc: 'Tahta direğe sertçe saplanmış, üzerinde kurbana yönelik kısa bir tehdit notu bulunan paslı çakı.', img: 'images/towns/sisoren/deliller/2085.jpg', top: '40%', left: '65%', fingerprintSpot: { xRatio: 0.23, yRatio: 0.62, angle: 0 }, bloodSpot: null }
            ]
        },
        // 9. TÜPÇÜ (NPC 209) — Şevket (Erkek #6)
        {
            id: 'tupcu',
            npcId: 209,
            title: 'Tüpçü',
            hoverTag: 'TÜPÇÜ',
            icon: 'fa-solid fa-fire-flame-simple',
            hasSign: true,
            interiorImg: 'images/towns/sisoren/interiors/tupcu_interior.jpg',
            details: 'Sundurması altında sıra sıra mavi ve gri çelik mutfak tüpleri dizilmiş tüp bayii.',
            style: { top: '67.5%', left: '60%', width: '12%', height: '17%' },
            npc: {
                id: 209,
                name: 'Tüpçü Şevket',
                gender: 'male',
                building: 'Tüpçü',
                role: 'Tüpçü',
                portrait: 'images/towns/sisoren/npcler/npc_209.jpg',
                bg: 'images/towns/sisoren/interiors/tupcu_interior.jpg',
                talkBg: 'images/towns/sisoren/interiors/tupcu_interior.jpg',
                greeting: 'Gaz sızıntısı her zaman tehlikelidir amirim... Ama bazı patlamalar kasıtlıdır.',
                questions: ['Son tüp teslimatını kime yaptın?', 'Depo kayıtlarında eksik tüp var mı?', 'O gece dükkânına gelen oldu mu?', 'Gaz kokusu nereden geliyor?'],
                murderStory: 'Tüpçü Şevket, her zaman kasabanın arka planında kalan, görünmez bir adamdı; ama kurbanın ona attığı iftiralar yüzünden ailesinin onuru lekelenmişti. İntikam ateşiyle yanan Şevket, o gece kurbanın evine sözde tüp teslimatı yapmak için gitti. Mahzenin karanlığında, ustalıkla ve büyük bir sessizlikle gaz vanasını bilerek gevşetti ve boruda ufak bir çatlak oluşturdu. Amacı belki sadece kurbanı korkutmak, hafif bir zehirlenmeyle hastanelik etmekti; ancak kurban karanlıkta sızan gazın kokusunu almayıp ışığı yakmak için o kibriti çaktığında, tüm ev devasa bir ateş topuna dönüşerek patladı. Şevket, uzaktan alevleri izlerken yüzünde korkunç bir tatmin duygusu vardı. Bu kusursuz cinayeti trajik bir "kaza" gibi gösterip sessizliğini yıllarca başarıyla korudu.'
            },
                        hotspots: [
                { id: 2091, name: 'Sabote Edilmiş Vana', desc: 'Valfi kasten bozulmuş ve ufak bir kıvılcımla gaz sızdırıp patlamaya hazır hale getirilmiş piknik tüpü.', img: 'images/towns/sisoren/deliller/2091.jpg', top: '65%', left: '20%', fingerprintSpot: { xRatio: 0.38, yRatio: 0.49, angle: -29 }, bloodSpot: null },
                { id: 2092, name: 'İngiliz Anahtarındaki Leke', desc: 'Demir anahtarın üzerinde, sadece yağ veya pas lekesi olmayan, silinmeye çalışılmış kurumuş kan izleri.', img: 'images/towns/sisoren/deliller/2092.jpg', top: '75%', left: '35%', fingerprintSpot: { xRatio: 0.27, yRatio: 0.39, angle: 20 }, bloodSpot: { xRatio: 0.7, yRatio: 0.3, angle: 0 } },
                { id: 2093, name: 'Sahte Teslimat Kaydı', desc: 'Müşteri defterinde, olay yeri olan eve cinayet gecesi teslimat yapılmış gibi gösterilen sonradan eklenme kayıt.', img: 'images/towns/sisoren/deliller/2093.jpg', top: '45%', left: '60%', fingerprintSpot: { xRatio: 0.2, yRatio: 0.8, angle: 0 }, bloodSpot: null },
                { id: 2094, name: 'Zulalanmış Mücevher', desc: 'Dükkanın arkasındaki kullanılamaz boş tüplerin birinin içine beze sarılarak saklanmış çalıntı kolye.', img: 'images/towns/sisoren/deliller/2094.jpg', top: '55%', left: '80%', fingerprintSpot: { xRatio: 0.24, yRatio: 0.49, angle: -18 }, bloodSpot: null },
                { id: 2095, name: 'Kopuk Düğme', desc: 'Kırık bir vana parçasının yanına düşmüş, arbede sırasında failin ceketinden koptuğu belli olan özel düğme.', img: 'images/towns/sisoren/deliller/2095.jpg', top: '85%', left: '50%', fingerprintSpot: { xRatio: 0.64, yRatio: 0.79, angle: -16 }, bloodSpot: null }
            ]
        },
        // 10. HURDACI (NPC 210) — Zehra (Kadın #4)
        {
            id: 'hurdaci',
            npcId: 210,
            title: 'Hurdacı',
            hoverTag: 'HURDACI',
            icon: 'fa-solid fa-gears',
            hasSign: true,
            interiorImg: 'images/towns/sisoren/interiors/hurdaci_interior.jpg',
            details: 'Önünde ıslak paslı çarklar, vagon tekerlekleri ve hurda demir parçaları yığılı kulübe.',
            style: { top: '70%', left: '76%', width: '16%', height: '19%' },
            npc: {
                id: 210,
                name: 'Hurdacı Zehra',
                gender: 'female',
                building: 'Hurdacı',
                role: 'Hurdacı',
                portrait: 'images/towns/sisoren/npcler/npc_210.jpg',
                bg: 'images/towns/sisoren/interiors/hurdaci_interior.jpg',
                talkBg: 'images/towns/sisoren/interiors/hurdaci_interior.jpg',
                greeting: 'Paslanmış demir bile bir hikâye anlatır amirim... Buraya gelen her hurdanın izi bendedir.',
                questions: ['Son getirilen hurda parçalar nereden geldi?', 'Paslı bıçak kime ait?', 'Hurda yığınları arasında saklanan ne?', 'O gece dükkânından garip sesler duyuldu mu?'],
                murderStory: 'Hurdacı Zehra\'nın o döküntü ve paslı dükkânı, aslında çevre kasabalardan çalınan malların iz bırakmadan eritildiği devasa bir kara para aklama merkeziydi. Kurban, bu kirli çarkı fark edip Zehra\'dan her ay düzenli pay isteyerek onu ihbar etmekle tehdit edince, kendi ölüm fermanını imzalamış oldu. Zehra o gece kurbanı "parayı vermek" bahanesiyle dükkânın en izbe köşesindeki sanayi tipi pres makinesinin yanına çekti. Kurban parayı saymak için eğildiği an, Zehra hızla onu pres makinesinin çelik çeneleri arasına itip kapağı kilitledi ve makineyi tam güçte çalıştırdı. Kurbanın kulakları sağır eden çığlıkları ve kırılan kemik sesleri, hurdaların kesici gürültüsü ve metalin soğuk ezici gücü arasında tamamen kaybolup gitti.'
            },
                        hotspots: [
                { id: 2101, name: 'Çalıntı Çelik Kasa', desc: 'Preslenmek üzere olan bir hurda yığınının arasına sıkıştırılarak yok edilmeye çalışılan kilitli kasa.', img: 'images/towns/sisoren/deliller/2101.jpg', top: '50%', left: '20%', fingerprintSpot: { xRatio: 0.52, yRatio: 0.57, angle: -11 }, bloodSpot: null },
                { id: 2102, name: 'Kazınmış Plaka', desc: 'Parçalanmış eşyaların arasında yeni sayılabilecek, aidiyeti belli olmasın diye üzeri zımparalanmış araç/araba plakası.', img: 'images/towns/sisoren/deliller/2102.jpg', top: '65%', left: '40%', fingerprintSpot: null, bloodSpot: null },
                { id: 2103, name: 'Seri Numarası Silinmiş Tüfek', desc: 'Yağ varillerinin arkasına gizlenmiş, namlusu kesilmiş ve seri numarası eğelenmiş silah.', img: 'images/towns/sisoren/deliller/2103.jpg', top: '75%', left: '60%', fingerprintSpot: { xRatio: 0.33, yRatio: 0.61, angle: 10 }, bloodSpot: { xRatio: 0.6, yRatio: 0.4, angle: 0 } },
                { id: 2104, name: 'Kanlı Tartım Fişi', desc: 'Hurdacı kantarının altında, şüpheli bir gece yarısı tartımını gösteren ve üzerinde kan damlası olan makbuz.', img: 'images/towns/sisoren/deliller/2104.jpg', top: '45%', left: '75%', fingerprintSpot: { xRatio: 0.3, yRatio: 0.7, angle: 0 }, bloodSpot: { xRatio: 0.27, yRatio: 0.35, angle: -18 } },
                { id: 2105, name: 'Kurbana Ait Yüzük', desc: 'Eritilecek bakır tellerin içine karışmış, kurbana ait baş harflerin kazılı olduğu deforme olmuş gümüş yüzük.', img: 'images/towns/sisoren/deliller/2105.jpg', top: '85%', left: '30%', fingerprintSpot: null, bloodSpot: null }
            ]
        },
        // 11. KASABALI EVİ 1 (NPC 211) — Zeynep Teyze (Kadın #5)
        {
            id: 'kasabali_evi_1',
            npcId: 211,
            title: 'Kasabalı Evi (Sol Köşk)',
            hoverTag: 'KASABALI EVİ',
            icon: 'fa-solid fa-house',
            hasSign: false,
            interiorImg: 'images/towns/sisoren/interiors/kasabali_evi_1_interior.jpg',
            details: 'Tabelasız, lambaları yanan ve kapısında kadın duran sakin dağ evi.',
            style: { top: '64%', left: '2.5%', width: '23%', height: '32%' },
            npc: {
                id: 211,
                name: 'Zeynep Teyze',
                gender: 'female',
                building: 'Kasabalı Evi',
                role: 'Ev Hanımı',
                portrait: 'images/towns/sisoren/npcler/npc_211.jpg',
                bg: 'images/towns/sisoren/interiors/kasabali_evi_1_interior.jpg',
                talkBg: 'images/towns/sisoren/interiors/kasabali_evi_1_interior.jpg',
                greeting: 'Oğlum pencereden gördüklerimi anlatayım sana...',
                questions: ['O gece pencereden kimi gördün?', 'Komşularla aranda husumet var mı?', 'Evinde sakladığın eski mektuplar neler?', 'Kasabanın geçmişinde karanlık bir olay mı var?'],
                murderStory: 'Zeynep Teyze, dışarıdan bakıldığında kasabanın en zarif ve sessiz yaşlı kadınlarından biriydi; ancak kurban, onun onlarca yıl önce işlediği korkunç bir günahın sırrını şans eseri öğrenmişti. Haftalardır her gece evine gelip onun tüm birikimini şantajla sömüren kurban, o gece de son altını almak için kapısına dayandı. Artık onuru ve hayatı elinden giden Zeynep Teyze\'nin sabrı o an tamamen taştı. Kurban, karanlıkta sırıtırken, Zeynep Teyze cübbesinin içine sakladığı, dedesinden kalma o kalın, ucu paslı, eski avcı bıçağını çıkardı ve sisin içinde beliren siluete acımasızca, kör bir öfkeyle defalarca sapladı. Yaşına rağmen inanılmaz bir güçle işlediği bu cinayetin ardından, kanlı ellerini soğuk suyla sakince yıkayıp, şöminenin karşısında kahvesini yudumladı.'
            },
                        hotspots: [
                { id: 2111, name: 'Yırtık Mektup', desc: 'Masanın altına düşmüş, tehditkâr sözler içeren eksik bir mektup.', img: 'images/towns/sisoren/deliller/2111.jpg', top: '50%', left: '20%', fingerprintSpot: { xRatio: 0.45, yRatio: 0.65, angle: -10 }, bloodSpot: null },
                { id: 2112, name: 'Çamurlu Ayak İzi', desc: 'Halının kenarında belirgin bir şekilde duran, kasabaya yabancı bir çizme izi.', img: 'images/towns/sisoren/deliller/2112.jpg', top: '70%', left: '50%', fingerprintSpot: null, bloodSpot: { xRatio: 0.6, yRatio: 0.75, angle: 22 } },
                { id: 2113, name: 'Kırık Cam Parçası', desc: 'Pencerenin kenarında içeriden dışarıya doğru kırılmış küçük bir cam parçası.', img: 'images/towns/sisoren/deliller/2113.jpg', top: '40%', left: '80%', fingerprintSpot: { xRatio: 0.65, yRatio: 0.70, angle: -15 }, bloodSpot: { xRatio: 0.45, yRatio: 0.68, angle: 5 } }
            ]
        },
        // 12. KASABALI EVİ 2 (NPC 212) — Hatice Nine (Kadın #6)
        {
            id: 'kasabali_evi_2',
            npcId: 212,
            title: 'Kasabalı Evi (Orta Patika)',
            hoverTag: 'KASABALI EVİ',
            icon: 'fa-solid fa-house-chimney',
            hasSign: false,
            interiorImg: 'images/towns/sisoren/interiors/kasabali_evi_2_interior.jpg',
            details: 'Tabelasız, çamur patika kenarında kapısında kadın oturan taş dağ evi.',
            style: { top: '75.5%', left: '35.5%', width: '19%', height: '23%' },
            npc: {
                id: 212,
                name: 'Hatice Nine',
                gender: 'female',
                building: 'Kasabalı Evi',
                role: 'Kasaba Büyüğü',
                portrait: 'images/towns/sisoren/npcler/npc_212.jpg',
                bg: 'images/towns/sisoren/interiors/kasabali_evi_2_interior.jpg',
                talkBg: 'images/towns/sisoren/interiors/kasabali_evi_2_interior.jpg',
                greeting: 'Bu yaştan sonra ne sırlar gördüm amirim... Dağın sisi bile saklayamaz bazı günahları.',
                questions: ['O gece neden uyumadın?', 'Dışarıdan gelen ayak seslerini tanıdın mı?', 'Eski fotoğraf albümünde saklanan sır ne?', 'Kasabadaki herkesin geçmişini biliyorsun, anlat bakalım.'],
                murderStory: 'Hatice Nine, bu kasabanın yaşayan tarihi ve en kadim hafızasıydı. Kurban denen o açgözlü adam, kasabayı devasa bir şirkete tamamen satıp buradaki herkesi evlerinden sürmek için gizli imzalar atmaya başladığında, Hatice Nine kasabayı korumak için eski usullere başvurması gerektiğini anladı. Yılların verdiği otacı bilgeliğiyle, ormanın en derin, ışıksız köşelerinden topladığı ve hiçbir tıbbi testte iz bırakmayan o özel, ölümcül kara dağ mantarlarını saatlerce kaynattı. O gece barışmak bahanesiyle kurbanı evine davet etti ve zehri onun o sevdiği bitki çayına zerre zerre karıştırdı. Kurban, çayını yudumlarken felç edici bir kalp kriziyle yere yığıldı. Hatice Nine, onun gözlerindeki son ışığın sönüşünü sallanan sandalyesinden, yüzünde adaleti sağlamış bir kasaba koruyucusunun huzuruyla izledi.'
            },
                        hotspots: [
                { id: 2121, name: 'Eski Fotoğraf Albümü', desc: 'Sayfalarından biri yeni koparılmış, kurbanla ilgili sırları barındıran albüm.', img: 'images/towns/sisoren/deliller/2121.jpg', top: '60%', left: '30%', fingerprintSpot: { xRatio: 0.65, yRatio: 0.40, angle: 12 }, bloodSpot: null },
                { id: 2122, name: 'Zehirli Bitki Kökü', desc: 'Mutfak tezgahında bırakılmış, normalde bu yörede yetişmeyen zehirli bir bitki parçası.', img: 'images/towns/sisoren/deliller/2122.jpg', top: '45%', left: '60%', fingerprintSpot: { xRatio: 0.35, yRatio: 0.65, angle: -8 }, bloodSpot: null },
                { id: 2123, name: 'Gümüş Düğme', desc: 'Yer minderinin arasına sıkışmış, maktulün ceketine ait olduğu anlaşılan gümüş düğme.', img: 'images/towns/sisoren/deliller/2123.jpg', top: '75%', left: '80%', fingerprintSpot: { xRatio: 0.51, yRatio: 0.49, angle: 24 }, bloodSpot: { xRatio: 0.55, yRatio: 0.52, angle: 0 } }
            ]
        },
        // 13. KASABALI EVİ 3 (NPC 213) — Emine Hanım (Kadın #7)
        {
            id: 'kasabali_evi_3',
            npcId: 213,
            title: 'Kasabalı Evi (Yamaç Evi)',
            hoverTag: 'KASABALI EVİ',
            icon: 'fa-solid fa-house-chimney-window',
            hasSign: false,
            interiorImg: 'images/towns/sisoren/interiors/kasabali_evi_3_interior.jpg',
            details: 'Tabelasız, yamaçta çam ağaçları arasındaki ahşap taş karışımı dağ evi.',
            style: { top: '42%', left: '29%', width: '7%', height: '10%' },
            npc: {
                id: 213,
                name: 'Emine Hanım',
                gender: 'female',
                building: 'Kasabalı Evi',
                role: 'Dağ Sakini & Dokumacı',
                portrait: 'images/towns/sisoren/npcler/npc_213.jpg',
                bg: 'images/towns/sisoren/interiors/kasabali_evi_3_interior.jpg',
                talkBg: 'images/towns/sisoren/interiors/kasabali_evi_3_interior.jpg',
                greeting: 'Dağ yamaçları sessiz görünür amirim ama gece rüzgarı her fısıltıyı taşır...',
                questions: ['Yamaçtan kasaba meydanını görebiliyor musun?', 'O gece fenerle dağa çıkan birini gördün mü?', 'Dokuduğun yün atkılardan birini meydanda bulan oldu mu?', 'Kasabada en çok kimden şüpheleniyorsun?'],
                murderStory: 'Emine Hanım, yamacın tepesindeki izole evinde kendi halinde dokuma yapan sessiz bir kadındı. Ancak son aylarda, kurbanın onun evini gizlice izlediğini, özel hayatını röntgenlediğini ve kasabada onun hakkında iğrenç dedikodular yaydığını fark etmişti. O gece kurban kasabaya inmek için karanlık orman yoluna girdiğinde, Emine Hanım, kendi elleriyle haftalarca ördüğü, çelik kadar sağlam o kalın yün ipi cebine saklayıp onu gölgelerin içinden takip etti. Kurban tam o ıssız köprüden geçerken, Emine Hanım sessizce arkasından yaklaşıp yün ipi boğazına doladı ve var gücüyle sıktı. Kurban nefessiz kalarak can verdikten sonra, Emine Hanım kusursuz bir zekayla ipi büyük bir çınar ağacının kalın dalına asıp kurbana korkunç bir intihar süsü verdi.'
            },
                        hotspots: [
                { id: 2131, name: 'Kanlı Yün Yumağı', desc: 'Dokuma tezgahının altında bulunan, üzerine kan damlamış yün ip.', img: 'images/towns/sisoren/deliller/2131.jpg', top: '55%', left: '25%', fingerprintSpot: { xRatio: 0.48, yRatio: 0.60, angle: -14 }, bloodSpot: { xRatio: 0.58, yRatio: 0.65, angle: 33 } },
                { id: 2132, name: 'Gizli Çekmece', desc: 'Sandığın içinde gizli bir bölme. İçinde kurbanın sahte kimliği bulunuyor.', img: 'images/towns/sisoren/deliller/2132.jpg', top: '65%', left: '70%', fingerprintSpot: { xRatio: 0.45, yRatio: 0.55, angle: 18 }, bloodSpot: null },
                { id: 2133, name: 'Kopuk Kolye', desc: 'Yerdeki talaşların arasında parlayan, arbede sırasında koptuğu anlaşılan kolye.', img: 'images/towns/sisoren/deliller/2133.jpg', top: '80%', left: '45%', fingerprintSpot: { xRatio: 0.40, yRatio: 0.70, angle: 55 }, bloodSpot: { xRatio: 0.60, yRatio: 0.60, angle: 10 } }
            ]
        }
    ],

    // ================================================================
    // EKSTRA NPC'LER — Suçlanamaz, konuşulabilir, kafayı karıştıran
    // Her binanın içinde veya sokakta bulunan yan karakterler
    // Oyuncu bunlarla konuşursa suçlama ekranına düşerler ama
    // suçlansalar bile HER ZAMAN MASUM çıkarlar.
    // ================================================================
    extraNpcs: [
        // === KAHVEHANE EKSTRA NPC'LERİ ===
        {
            id: 'kahve_celal',
            bg: 'images/towns/sisoren/interiors/kahvehane_interior.jpg',
            talkBg: 'images/towns/sisoren/interiors/kahvehane_interior.jpg',
            numericId: 301,
            buildingId: 'kahvehane',
            name: 'Celal Amca',
            age: 68,
            gender: 'male',
            role: 'Emekli Ormancı',
            isExtra: true,
            canBeGuilty: false,
            portrait: 'images/towns/sisoren/npcler/npc_301.jpg',
            greeting: 'Hoş geldin evladım... 40 yıl bu ormanları korudum. Bu kasabanın çamlarından çok sır gördüm.',
            questions: [
                { q: 'Cinayet gecesi kahvehanede miydin?', a: 'Elbette buradaydım, İrfan\'la tavla oynuyorduk. Saat 11 civarı bir çığlık duydum ama İrfan "rüzgardır" dedi. Şimdi düşününce...', difficulty: 1, category: 'tanisma' },
                { q: 'Ormanda şüpheli bir şey gördün mü?', a: 'Geçen hafta yamaçta yabancı ayak izleri gördüm. 43 numara iri çizme izleri, kasabalılardan değildi. Ama Durmuş\'un oğlu "dağcılardır" dedi...', difficulty: 2, category: 'derinlesme' },
                { q: 'Kasabalılar arasındaki husumetler hakkında ne biliyorsun?', a: 'Tütüncü Nermin ile Muhtar Meliha yıllardır birbirini yiyorlar. Arazi meselesi... Sahaf Hikmet de garip garip notlar yazıyor, adam gizemli biri.', difficulty: 2, category: 'derinlesme' },
                { q: 'En çok kimden şüpheleniyorsun?', a: 'Bak evladım, bu kasabada herkesin bir sırrı var ama... Hurdacı Zehra\'nın dükkânında gece yarısı ışıklar yanıyordu. Garip değil mi?', difficulty: 3, category: 'yuzlestirme' }
            ]
        },
        {
            id: 'kahve_hamdi',
            bg: 'images/towns/sisoren/interiors/kahvehane_interior.jpg',
            talkBg: 'images/towns/sisoren/interiors/kahvehane_interior.jpg',
            numericId: 302,
            buildingId: 'kahvehane',
            name: 'Hamdi Dayı',
            age: 65,
            gender: 'male',
            role: 'Emekli Madenci & Tavlacı',
            isExtra: true,
            canBeGuilty: false,
            portrait: 'images/towns/sisoren/npcler/npc_302.jpg',
            greeting: 'Buyur amirim, tavla taşlarını toplarken kasabanın en eski hikayelerini anlatayım...',
            questions: [
                { q: 'Maden ocağında çalışırken neler gördün?', a: 'Bu dağın altında eski bir tünel sistemi var. Kimse bilmiyor ama tüneller hurdacının dükkânının altına kadar uzanıyor... Zehra bunu biliyordur.', difficulty: 2, category: 'derinlesme' },
                { q: 'Cinayet gecesi bir şey duydun mu?', a: 'Saat 2 civarı tavla bitip eve gidiyordum. Tüpçü Şevket\'in dükkânından gaz kokusu geliyordu, kapısı da aralanmıştı.', difficulty: 2, category: 'derinlesme' },
                { q: 'Kasabanın karanlık geçmişi hakkında ne biliyorsun?', a: '30 yıl önce bu dağda bir ceset bulunmuştu, hiç çözülemedi. Hatice Nine o zamanlar da genç bir kadındı... Bildikleri var ama konuşmuyor.', difficulty: 3, category: 'yuzlestirme' },
                { q: 'Kahvehanede en çok kimi gördün son günlerde?', a: 'Sinemacı Nejat her akşam gelip köşede tek başına oturuyordu. Filmlerden bahsediyordu ama gözleri hep pencereden dışarıyı tarıyordu.', difficulty: 1, category: 'tanisma' }
            ]
        },
        {
            id: 'kahve_cirak',
            numericId: 303,
            buildingId: 'kahvehane',
            name: 'Kahveci Çırağı Salih',
            age: 17,
            gender: 'male',
            role: 'Kahveci Çırağı',
            isExtra: true,
            canBeGuilty: false,
            portrait: 'images/towns/sisoren/npcler/npc_303.jpg',
            greeting: 'Hoş geldiniz amirim... İrfan usta bana her şeyi gösterdi. Çay demlemeyi de, kasabanın sırlarını dinlemeyi de.',
            questions: [
                { q: 'Kahvehanede neler konuşuluyor?', a: 'Celal amca Hamdi dayıya hep "o gece ormanda biri koşuyordu" diyor. Bakkal Cemile teyze de dün gelip İrfan ustaya fısıldadı ama duyamadım.', difficulty: 1, category: 'tanisma' },
                { q: 'Cinayet gecesi burada mıydın?', a: 'Evet, İrfan usta beni geç saate kadar temizlik için tuttu. Gece yarısı ahırdan köpek havlaması geldi. Sonra da sinema tarafından bir kapı çarpma sesi...', difficulty: 2, category: 'derinlesme' },
                { q: 'En garip müşteri kimdi?', a: 'Geçen hafta hiç görmediğim uzun boylu bir adam geldi. Yüzünü şapkayla gizliyordu. Telgrafçı Rüstem\'e bir zarf verdi ve hemen çıktı!', difficulty: 3, category: 'yuzlestirme' }
            ]
        },

        // === SOKAK EKSTRA NPC'LERİ ===
        {
            id: 'sokak_serif',
            mapPos: { top: '35%', left: '35%' },
            bg: 'images/towns/sisoren/sisoren_map_v5.jpg',
            talkBg: 'images/towns/sisoren/sisoren_map_v5.jpg',
            numericId: 304,
            buildingId: null, // Sokakta
            name: 'Şerife Teyze',
            age: 72,
            gender: 'female',
            role: 'Kasabanın Dedikoducu Ninesi',
            isExtra: true,
            canBeGuilty: false,
            portrait: 'images/towns/sisoren/npcler/npc_304.jpg',
            greeting: 'Gel gel oğlum, bu kasabada ne olsa benim gözümden kaçmaz... Bastonuma yaslanıp her şeyi izliyorum.',
            questions: [
                { q: 'Cinayet gecesi sokakta kimi gördün?', a: 'Saat 1 gibi pencereden baktım, Muhtar Meliha\'nın evinin ışığı yanıyordu. Sonra bir gölge muhtarlık binasından çıkıp yamaça doğru koştu!', difficulty: 2, category: 'derinlesme' },
                { q: 'Kasabalıların sırları hakkında ne biliyorsun?', a: 'Emine Hanım\'ın yamaçtaki evinde gece gelen misafirleri var. Tütüncü Nermin\'in de sahaf Hikmet\'le gizli gizli buluştuğunu gördüm!', difficulty: 3, category: 'yuzlestirme' },
                { q: 'Bakkal Cemile hakkında ne düşünüyorsun?', a: 'Cemile kız dürüst biridir ama son zamanlarda çok gerginleşti. Dükkânda garip paketler saklıyor, kuryeyle gelen kutular... Şüpheli!', difficulty: 2, category: 'derinlesme' },
                { q: 'Kasabada en tehlikeli kim?', a: 'Çoban Durmuş\'tan kork oğlum... O adam hayvanları sever ama insanlara karşı soğuktur. Elindeki balta da hep yanında!', difficulty: 3, category: 'yuzlestirme' }
            ]
        },
        {
            id: 'sokak_cemal',
            mapPos: { top: '75%', left: '55%' },
            bg: 'images/towns/sisoren/sisoren_map_v5.jpg',
            talkBg: 'images/towns/sisoren/sisoren_map_v5.jpg',
            numericId: 305,
            buildingId: null, // Sokakta
            name: 'Oduncu Çırağı Cemal',
            age: 19,
            gender: 'male',
            role: 'Oduncu & Hamaliye',
            isExtra: true,
            canBeGuilty: false,
            portrait: 'images/towns/sisoren/npcler/npc_305.jpg',
            greeting: 'Selamlar amirim... Ben odun taşırım, tahta keserim. Gece gündüz demez çalışırım bu dağlarda.',
            questions: [
                { q: 'Dağlarda çalışırken garip bir şey gördün mü?', a: 'Geçen gece yamaçta bir ateş gördüm. Biri belge yakıyordu! Yanına gittiğimde kimse yoktu ama yarı yanmış bir kağıt buldum... Muhtarlık antetliydi.', difficulty: 3, category: 'yuzlestirme' },
                { q: 'Kasabada kiminle iyi geçinirsin?', a: 'Ahırdaki Durmuş amca beni çok sever, bazen oğlu Kerem\'le birlikte koyun güderiz. Ama dün Durmuş amca çok sinirli görünüyordu...', difficulty: 1, category: 'tanisma' },
                { q: 'Cinayet gecesi neredeydin?', a: 'Dağda odun kesiyordum, gece yarısı eve döndüm. Ama yolda sinemadan çıkan birini gördüm, Nejat değildi... Tanıyamadım.', difficulty: 2, category: 'derinlesme' }
            ]
        },
        {
            id: 'sokak_postaci',
            mapPos: { top: '55%', left: '25%' },
            bg: 'images/towns/sisoren/sisoren_map_v5.jpg',
            talkBg: 'images/towns/sisoren/sisoren_map_v5.jpg',
            numericId: 306,
            buildingId: null, // Sokakta
            name: 'Postacı Nuri Efendi',
            age: 55,
            gender: 'male',
            role: 'Kasaba Postacısı',
            isExtra: true,
            canBeGuilty: false,
            portrait: 'images/towns/sisoren/npcler/npc_306.jpg',
            greeting: 'Mektuplar sırları taşır amirim... Her gün kasabanın her köşesine posta dağıtıyorum.',
            questions: [
                { q: 'Son zamanlarda kime garip mektuplar geldi?', a: 'Sahaf Hikmet\'e dışarıdan şifreli zarflar geliyor. Zeynep Teyze de her hafta İstanbul\'a mektup gönderiyor ama alıcı adresi hep farklı.', difficulty: 2, category: 'derinlesme' },
                { q: 'Cinayet gecesi posta dağıttın mı?', a: 'O gece geç saatte muhtarlığa acil bir telgraf bıraktım. Kapı aralıktı ve içeriden fısıltılar geliyordu.', difficulty: 3, category: 'yuzlestirme' },
                { q: 'Kasabada en çok kime mektup gelir?', a: 'Telgrafçı Rüstem\'e hep "gizli" damgalı zarflar geliyor. Tüpçü Şevket\'e de İstanbul\'dan ağır paketler...', difficulty: 1, category: 'tanisma' }
            ]
        },

        // === TELGRAFHANE EKSTRA NPC'Sİ ===
        {
            id: 'telgraf_cirak',
            numericId: 307,
            buildingId: 'telgrafhane',
            name: 'Telgraf Çırağı Yusuf',
            age: 16,
            gender: 'male',
            role: 'Telgraf Çırağı',
            isExtra: true,
            canBeGuilty: false,
            portrait: 'images/towns/sisoren/npcler/npc_307.jpg',
            greeting: 'Tık tık tık... Morse alfabesini yeni öğrendim amirim. Rüstem usta çok sert ama iyi öğretiyor.',
            questions: [
                { q: 'Gece telgrafhaneye gelen mesajlar hakkında ne biliyorsun?', a: 'Cinayet gecesi çok garip bir şifreli mesaj geldi. Rüstem usta onu okuyunca yüzü bembeyaz oldu ve mesajı hemen yaktı!', difficulty: 3, category: 'yuzlestirme' },
                { q: 'Rüstem usta hakkında ne düşünüyorsun?', a: 'İyi bir ustadır ama gece yarıları tek başına telgrafhaneye gelip saatlerce çalışıyor. Kime mesaj gönderdiğini bilmiyorum...', difficulty: 2, category: 'derinlesme' }
            ]
        },

        // === SİNEMA EKSTRA NPC'Sİ ===
        {
            id: 'sinema_biletci',
            numericId: 308,
            buildingId: 'sinema',
            name: 'Biletçi Fatma',
            age: 45,
            gender: 'female',
            role: 'Sinema Biletçisi',
            isExtra: true,
            canBeGuilty: false,
            portrait: 'images/towns/sisoren/npcler/npc_308.jpg',
            greeting: 'Hoş geldiniz amirim... Sinema karanlıktır, herkes gizlenebilir burada.',
            questions: [
                { q: 'Cinayet gecesi sinemada kimler vardı?', a: 'O gece film gösterimi yoktu ama Nejat arka odada birisiyle buluştu. Seslerini duydum ama yüzünü göremedim.', difficulty: 2, category: 'derinlesme' },
                { q: 'Nejat hakkında şüpheli bir şey var mı?', a: 'Nejat son zamanlarda çok gergin. Film makinesi odasında bir çelik kutu saklıyor, kimseye dokundurmaz.', difficulty: 3, category: 'yuzlestirme' }
            ]
        },

        // === MUHTARLIK EKSTRA NPC'Sİ ===
        {
            id: 'muhtarlik_katip',
            numericId: 309,
            buildingId: 'muhtarlik',
            name: 'Kâtip Sami Efendi',
            age: 58,
            gender: 'male',
            role: 'Muhtarlık Kâtibi',
            isExtra: true,
            canBeGuilty: false,
            portrait: 'images/towns/sisoren/npcler/npc_309.jpg',
            greeting: 'Her belge benim elimden geçer amirim... Bu kasabanın resmi tarihini ben yazarım.',
            questions: [
                { q: 'Muhtar Meliha hakkında ne biliyorsun?', a: 'Meliha Hanım güçlü bir kadındır ama son zamanlarda arazi tapularıyla çok uğraşıyor. Bazı belgeler gece yarısı imzalanıyor...', difficulty: 2, category: 'derinlesme' },
                { q: 'Son dönemde garip belgeler hazırladın mı?', a: 'Geçen ay Meliha Hanım benden eski nüfus kayıtlarını çıkarmamı istedi. Bir de miras paylaşım belgesi... Ama imza sahte gibiydi!', difficulty: 3, category: 'yuzlestirme' },
                { q: 'Kasabadaki arazi kavgaları hakkında ne biliyorsun?', a: 'Çoban Durmuş ile Muhtar Meliha yıllık otlak meselesi yüzünden kavga etti. Tütüncü Nermin de hurdacı Zehra\'nın arazisine göz dikmiş.', difficulty: 2, category: 'derinlesme' }
            ]
        },

        // === TÜTÜNCÜ EKSTRA NPC'Sİ ===
        {
            id: 'tutuncu_musteri',
            numericId: 310,
            buildingId: 'tutuncu',
            name: 'Tütün Tiryakisi Osman',
            age: 50,
            gender: 'male',
            role: 'Düzenli Müşteri',
            isExtra: true,
            canBeGuilty: false,
            portrait: 'images/towns/sisoren/npcler/npc_310.jpg',
            greeting: 'Nermin hanımın tütünü gibisi yok amirim... Her gün gelir, bir de çay içerim.',
            questions: [
                { q: 'Nermin hanım hakkında ne biliyorsun?', a: 'Nermin çalışkan bir kadındır ama gece yarıları dükkânda kalıyor. Birkaç kez arka odadan garip kokular geldi... Tütün değildi o!', difficulty: 2, category: 'derinlesme' },
                { q: 'Cinayet gecesi burada mıydın?', a: 'Hayır ama eve giderken tütüncünün arkasından dağa doğru birinin tırmandığını gördüm. Elinde torba gibi bir şey vardı.', difficulty: 3, category: 'yuzlestirme' }
            ]
        },

        // === HURDACI EKSTRA NPC'Sİ ===
        {
            id: 'hurdaci_cocuk',
            numericId: 311,
            buildingId: 'hurdaci',
            name: 'Hurda Toplayan Ali',
            age: 12,
            gender: 'male',
            role: 'Hurda Toplayan Çocuk',
            isExtra: true,
            canBeGuilty: false,
            portrait: 'images/towns/sisoren/npcler/npc_311.jpg',
            greeting: 'Zehra abla bana hurda toplamayı öğretti... Ben her yeri gezerim amirim!',
            questions: [
                { q: 'Hurda toplarken garip bir şey buldun mu?', a: 'Dün dağ yolunda kanlı bir bıçak buldum! Zehra ablaya verdim ama o "eskidir, at" dedi ve hurda yığınına attı.', difficulty: 3, category: 'yuzlestirme' },
                { q: 'Zehra abla hakkında ne biliyorsun?', a: 'Zehra abla geceleri dükkânda çekiçle bir şeyler dövüyor. Bazen yeraltından sesler geliyor... Tünel falan mı var bilmiyorum.', difficulty: 2, category: 'derinlesme' }
            ]
        },

        // === TÜPÇÜ EKSTRA NPC'Sİ ===
        {
            id: 'tupcu_yardimci',
            numericId: 312,
            buildingId: 'tupcu',
            name: 'Tüp Dağıtıcısı Mehmet',
            age: 35,
            gender: 'male',
            role: 'Tüp Teslimatçısı',
            isExtra: true,
            canBeGuilty: false,
            portrait: 'images/towns/sisoren/npcler/npc_312.jpg',
            greeting: 'Her gün kasabanın her evine tüp taşırım amirim... Kimin neye ihtiyacı var bilirim.',
            questions: [
                { q: 'Şevket hakkında garip bir şey fark ettin mi?', a: 'Şevket usta son hafta normalden fazla tüp sipariş etti. Depoyu doldurdu ama satış yapmıyor. Neden bu kadar gaz biriktiriyor?', difficulty: 2, category: 'derinlesme' },
                { q: 'Cinayet gecesi teslimat yaptın mı?', a: 'Evet, gece geç saatte Hatice Nine\'nin evine acil tüp götürdüm. Eve girdiğimde nine pencereden dışarıyı izliyordu, çok tedirgin görünüyordu.', difficulty: 2, category: 'derinlesme' }
            ]
        },

        // === KASABALI EVLERİ SOKAK EKSTRA NPC'LERİ ===
        {
            id: 'sokak_cocuk_ayse',
            numericId: 313,
            buildingId: null,
            name: 'Küçük Ayşe',
            age: 9,
            gender: 'female',
            role: 'Sokak Çocuğu',
            isExtra: true,
            canBeGuilty: false,
            portrait: 'images/towns/sisoren/npcler/npc_313.jpg',
            greeting: 'Amca amca! Ben her şeyi görürüm çünkü büyükler beni fark etmez!',
            questions: [
                { q: 'Cinayet gecesi bir şey gördün mü?', a: 'Gece anneme su almaya çıktım ve sahaf amcanın dükkânından birinin çıktığını gördüm. Elinde bir kitap değil, parlak bir bıçak vardı!', difficulty: 3, category: 'yuzlestirme' },
                { q: 'Kasabada en çok kimden korkarsın?', a: 'Hurdacı Zehra abladan... Dükkânında garip sesler çıkıyor gece. Bir de sinemacı Nejat amcadan, hep karanlık odada oturuyor.', difficulty: 1, category: 'tanisma' }
            ]
        },
        {
            id: 'sokak_bekci',
            numericId: 314,
            buildingId: null,
            name: 'Gece Bekçisi Recep',
            age: 60,
            gender: 'male',
            role: 'Kasaba Gece Bekçisi',
            isExtra: true,
            canBeGuilty: false,
            portrait: 'images/towns/sisoren/npcler/npc_314.jpg',
            greeting: 'Saat başı devriye gezerim amirim... Bu kasabanın gecesi gündüzünden daha hareketlidir.',
            questions: [
                { q: 'Cinayet gecesi devriyede neler gördün?', a: 'Saat 01:30\'da muhtarlığın ışığı yanıyordu, garip çünkü Meliha Hanım erken yatar. Saat 02:00\'de ahırdan köpek havlaması geldi, sonra sahaf tarafından koşan bir gölge...', difficulty: 3, category: 'yuzlestirme' },
                { q: 'Kasabada en tehlikeli saatler hangileri?', a: 'Gece 1-3 arası! O saatlerde tütüncünün dükkânından duman çıkıyor, hurdacıda çekiç sesleri... Normal değil bunlar.', difficulty: 2, category: 'derinlesme' },
                { q: 'Bekçilik yaparken garip olaylar oldu mu?', a: 'Geçen hafta telgrafhanenin çatısında biri vardı, anten kurcalıyordu. Rüstem mi başkası mı bilemedim... Ama sabaha karşıydı.', difficulty: 2, category: 'derinlesme' },
                { q: 'Kasabalılardan kim geceleri dışarı çıkıyor?', a: 'Emine Hanım gece yarısı yamaçta yürüyüşe çıkıyor. Zeynep Teyze pencereden izliyor. Sahaf Hikmet ise dükkânında mum yakıp okuyor.', difficulty: 2, category: 'derinlesme' }
            ]
        }
    ],

    // ================================================================
    // 13 ANA ŞÜPHELİ İÇİN HAZIR SORU HAVUZU (API Fallback)
    // Her NPC için 5 aşamalı (tanışma → derinleşme → yüzleştirme → baskı → son) soru dizisi
    // ================================================================
    fallbackQuestions: {
        201: [ // Telgrafçı Rüstem
            { q: 'Cinayet gecesi telgrafhaneye mesaj geldi mi?', a: 'Gece yarısı şifreli bir mesaj aldım. İçeriği devlet sırrı, söyleyemem! Ama göndericinin kodu tanıdık geldi...', difficulty: 1, category: 'tanisma' },
            { q: 'Hangi kasabalılar düzenli telgraf çekiyor?', a: 'Muhtar Meliha haftada üç kez İstanbul\'a telgraf çeker. Sahaf Hikmet de yurtdışına garip mesajlar yolluyor.', difficulty: 1, category: 'tanisma' },
            { q: 'Şifreli mesajlar hakkında ne biliyorsun?', a: 'Morse kodu basittir ama son gelen mesaj farklı bir şifre kullanıyordu. Sanki askeri bir kod... Korktum açıkçası.', difficulty: 2, category: 'derinlesme' },
            { q: 'O gece tellerde garip bir sinyal fark ettin mi?', a: 'Evet! Tam saat 02:14\'te teller çıldırdı! Elektrik kesildi ve ardından kasabada bir çığlık duydum. Dışarı çıkmaya cesaret edemedim.', difficulty: 3, category: 'yuzlestirme' },
            { q: 'Gece yarısı telgrafhanede ne yapıyordun?', a: 'Ben nöbetçiydim! Telgrafçının görevi 7/24! Ama tamam, itiraf edeyim... O mesajı ben yakmadım, şifreli parçasını sakladım.', difficulty: 4, category: 'baski' },
            { q: 'Son sözün nedir Rüstem?', a: 'Teller her şeyi duyar demiştim. O gece duyduğum çığlık sinemadan geldi, telgrafhaneden değil! Ben masumum.', difficulty: 5, category: 'son' }
        ],
        202: [ // Kahveci İrfan
            { q: 'Cinayet gecesi kahvehanede kimler vardı?', a: 'Celal amca, Hamdi dayı ve bir de geç saatte Sinemacı Nejat geldi. Nejat çok gergindi, çayını bile içmeden gitti.', difficulty: 1, category: 'tanisma' },
            { q: 'En son kim geç saatte kaldı?', a: 'Celal amcayla tavla oynamayı bitirip saat 01:00\'de kapattım. Ama dışarı çıkarken tütüncünün ışığının yandığını gördüm.', difficulty: 1, category: 'tanisma' },
            { q: 'Kasabalılar arasında kavga çıktı mı?', a: 'Haftada bir kavga olur burada! Son kavga Çoban Durmuş ile Tüpçü Şevket arasındaydı. Hayvan yemi fiyatları yüzünden birbirlerini tehdit ettiler!', difficulty: 2, category: 'derinlesme' },
            { q: 'Kahvehanede gizlice konuşanları duydun mu?', a: 'Muhtar Meliha geçen hafta Telgrafçı Rüstem\'le köşede fısıldaştılar. "Belgeleri yak" gibi bir şey duydum ama emin değilim...', difficulty: 3, category: 'yuzlestirme' },
            { q: 'Neden kapattıktan sonra tekrar dışarı çıktın?', a: 'Kasayı kontrol etmeye geri döndüm! Yoksa ne sandınız? Birini mi öldürdüm? Şaka yapıyorsunuz herhalde!', difficulty: 4, category: 'baski' },
            { q: 'Son sözün nedir İrfan?', a: 'Ben sadece çay demliyorum amirim. Kasaba beni sever, kimseye zararım dokunmaz. Gerçek katili sinemada arayın!', difficulty: 5, category: 'son' }
        ],
        203: [ // Sinemacı Nejat
            { q: 'O gece sinema salonunda kimler vardı?', a: 'Film gösterimi yoktu ama ben makineleri temizliyordum. Gece 11\'de kapıda bir tıkırtı duydum, açtığımda kimse yoktu.', difficulty: 1, category: 'tanisma' },
            { q: 'Arka kapıdan biri girip çıktı mı?', a: 'Film makinesi odasının arka kapısı her zaman kilitlidir. Ama o gece kilidi kırılmış buldum! Biri zorla girmiş.', difficulty: 2, category: 'derinlesme' },
            { q: 'Film makinesi odasında saklanan ne?', a: 'Sadece eski film ruloları! Ama... Bir tanesinin içinde sarılı bir zarf vardı. Açmadım, yemine billahi açmadım!', difficulty: 3, category: 'yuzlestirme' },
            { q: 'Karanlık salonda şüpheli bir şey gördün mü?', a: 'Perdenin arkasında çamurlu ayak izleri vardı. Birisi salonu geçip arka kapıdan çıkmış. İzler ahıra doğru gidiyordu...', difficulty: 3, category: 'yuzlestirme' },
            { q: 'Film makinesinde saklı zarfı neden açmadın?', a: 'Çünkü üzerinde "AÇAN ÖLÜR" yazıyordu! Ama sonra baktığımda zarf kaybolmuştu... Biri gece gelip almış olmalı!', difficulty: 4, category: 'baski' },
            { q: 'Son sözün nedir Nejat?', a: 'Sinemacı öldürmez amirim, film çeker! O çamurlu izler benim değil, ben 42 numara giyerim ama izler 44 numaraydı!', difficulty: 5, category: 'son' }
        ],
        204: [ // Bakkal Cemile
            { q: 'Son günlerde dükkâna gelen şüpheli biri oldu mu?', a: 'Geçen gece kapanış saatinde uzun boylu, paltosu olan biri geldi. Yüzünü göremedim, sadece kibrit ve ip aldı.', difficulty: 1, category: 'tanisma' },
            { q: 'Veresiye defterinde silinen isim kimin?', a: 'Silinen isim yok ama... Sayfaları yırtılmış! Birileri kayıtları yok etmeye çalışmış. En son Muhtar Meliha\'nın borcu yazılıydı.', difficulty: 2, category: 'derinlesme' },
            { q: 'O gece dükkândan çalınan ne?', a: 'Çalınan bir şey yok ama kasanın arkasındaki fare zehri kutusu bir tane eksik! Kim aldı bilmiyorum...', difficulty: 3, category: 'yuzlestirme' },
            { q: 'Kasabalıların borç kavgaları hakkında ne biliyorsun?', a: 'Tüpçü Şevket 6 aydır borcunu ödemiyor. Hurdacı Zehra da sürekli veresiye alıyor. Ama en büyük borç? Onu söyleyemem...', difficulty: 3, category: 'yuzlestirme' },
            { q: 'Fare zehrini sen mi aldın?', a: 'HAYIR! Ben bakkalım, zehir satarım ama kullanmam! O kutuyu alan kişi cinayet gecesi geldi, kasanın arkasını biliyordu...', difficulty: 4, category: 'baski' },
            { q: 'Son sözün nedir Cemile?', a: 'Ben iki çocuk büyütüyorum bu dükkânla! Cinayet işleyecek vaktim yok! Ama zehri alanı gördüm sanırım... Emin olmalıyım.', difficulty: 5, category: 'son' }
        ],
        205: [ // Sahaf Hikmet
            { q: 'El yazmalarında şifreli notlar var mı?', a: 'Kitaplar kendi dillerinde konuşur. Evet, bir el yazmasında eski Osmanlıca şifreli notlar buldum. Cinayet tarihiyle aynı güne denk geliyor!', difficulty: 2, category: 'derinlesme' },
            { q: 'Son müşterin kim ve ne aldı?', a: 'Emine Hanım gelip "Zehirli Bitkiler Ansiklopedisi" istedi. Garip buldum ama sormadım...', difficulty: 1, category: 'tanisma' },
            { q: 'Eski haritalar arasında saklanan belge ne?', a: 'Bir miras paylaşım belgesi! Bu kasabanın altındaki madene ait. Sahte mi gerçek mi bilemiyorum ama birçok isim var üzerinde.', difficulty: 3, category: 'yuzlestirme' },
            { q: 'O gece dükkânına gelen oldu mu?', a: 'Gece yarısı kapım tıklatıldı, açtığımda kimse yoktu. Ama kapıda bir not bırakılmış: "Belgeyi ver yoksa seninki de gelir."', difficulty: 3, category: 'yuzlestirme' },
            { q: 'Tehdit notunu neden polise vermedin?', a: 'Çünkü bu kasabada polis yok! Ve notu yazanın el yazısını tanıyorum... Ama söylersem hayatım tehlikeye girer.', difficulty: 4, category: 'baski' },
            { q: 'Son sözün nedir Hikmet?', a: 'Kitaplar yalan söylemez amirim. O miras belgesi her şeyin anahtarı. Ama onu saklayan ben değilim, bulan benim.', difficulty: 5, category: 'son' }
        ],
        206: [ // Muhtar Meliha
            { q: 'Arazi kavgaları kimler arasında?', a: 'Dağ otlağı meselesi! Çoban Durmuş sürüsü için geniş arazi istiyor ama o arazi devlet malı. Ben de korumak zorundayım.', difficulty: 1, category: 'tanisma' },
            { q: 'Resmi mühürü en son kim kullandı?', a: 'Mühür her zaman kasada durur! Ama... Son kontrol ettiğimde mürekkep izleri tazeydi. Biri kullanmış olabilir!', difficulty: 2, category: 'derinlesme' },
            { q: 'Cinayet gecesi köy meydanında kimler vardı?', a: 'Pencereden baktım, saat 01:00\'de meydan boştu. Ama 02:00\'de bir gölge sahaftan muhtarlığa doğru koştu!', difficulty: 3, category: 'yuzlestirme' },
            { q: 'Kasabanın karanlık geçmişi hakkında ne biliyorsun?', a: 'Bu dağda 30 yıl önce bir altın madeni vardı. Madeni kapatan kişi öldürüldü. Ve katil hiç bulunamadı...', difficulty: 3, category: 'yuzlestirme' },
            { q: 'Sahte mühürlü belge hakkında ne diyorsun?', a: 'BEN SAHTE BELGE HAZIRLAMADIM! O belge benden önce hazırlanmış! Kâtip Sami bunu doğrulayabilir!', difficulty: 4, category: 'baski' },
            { q: 'Son sözün nedir Meliha Hanım?', a: 'Ben bu kasabanın muhtarıyım, koruyucusuyum! Cinayet benim işim değil. Ama katili bulmak benim görevim.', difficulty: 5, category: 'son' }
        ],
        207: [ // Tütüncü Nermin
            { q: 'Özel karışım tütününü kimler satın alıyor?', a: 'Celal amca, Hamdi dayı ve bir de... Telgrafçı Rüstem. Rüstem garip bir karışım istiyor, içinde lavanta var!', difficulty: 1, category: 'tanisma' },
            { q: 'O gece dükkânın arka odasında ne oldu?', a: 'Arka oda kurutma odasıdır! Gece tütün yapraklarını kontrol ediyordum. Ama dışarıdan garip sesler geldi, korkup kapıyı kilitledim.', difficulty: 2, category: 'derinlesme' },
            { q: 'Tütün yaprakları arasında saklanan ne?', a: 'Yaprakların arasında küçük bir deri kese buldum. İçinde altın sikke vardı! Kimin sakladığını bilmiyorum...', difficulty: 3, category: 'yuzlestirme' },
            { q: 'Kaçak tütün ticareti hakkında ne biliyorsun?', a: 'KAÇAK DEĞİL! Benim tütünüm tamamen yasal! Ama... Dağdan gelen bir adam bazen tütün getiriyor, kaliteli ama kaynağı belirsiz.', difficulty: 3, category: 'yuzlestirme' },
            { q: 'Altın sikkeleri neden saklıyorsun?', a: 'BEN SAKLAMADIM! Birileri benim dükkânıma sakladı! Muhtar Meliha\'nın adamları olabilir, arazi parasıdır belki!', difficulty: 4, category: 'baski' },
            { q: 'Son sözün nedir Nermin Hanım?', a: 'Duman iz bırakır dedim, doğru. Ama benim dumanım tütündür, zehir değil! Katil başka yerde aranmalı.', difficulty: 5, category: 'son' }
        ],
        208: [ // Çoban Durmuş
            { q: 'Ahırda saklanan yabancı kim?', a: 'Ahırımda yabancı barınamaz, Karabaş havlar! Ama... Geçen gece saman balyalarının arasında birinin yattığı izi gördüm.', difficulty: 1, category: 'tanisma' },
            { q: 'Çamurlu ayak izleri nereye gidiyor?', a: 'Ahırdan dağ patikasına doğru. 44 numara büyük çizme izleri. Benimki 42, oğlumunki 38. Bu bize ait değil!', difficulty: 2, category: 'derinlesme' },
            { q: 'Saman balyalarının altında ne var?', a: 'Balta, kürek, çoban değneği... Normal şeyler! Ama bir de eski bir sandık var, kilidi kırılmış. İçinde eski mektuplar varmış ama oğlum açmış...', difficulty: 3, category: 'yuzlestirme' },
            { q: 'O gece hayvanlar neden rahatsız oldu?', a: 'Karabaş çılgınca havladı! Kerem de uyandı, pencereden baktık. Dağ yolunda bir fener ışığı gördük, biri koşuyordu!', difficulty: 3, category: 'yuzlestirme' },
            { q: 'Sandıktaki mektupları neden saklıyorsun?', a: 'Mektuplar babamdan kalma! İçinde miras belgesi ve eski maden haritası var. Bu dağın altında altın var, herkes biliyor ama kimse konuşmuyor!', difficulty: 4, category: 'baski' },
            { q: 'Son sözün nedir Durmuş?', a: 'Ben çobanım, hayvanlarımla yaşarım. Oğlumu bu dağda çoban yetiştiriyorum. Cinayet işleyecek adam değilim!', difficulty: 5, category: 'son' }
        ],
        209: [ // Tüpçü Şevket
            { q: 'Son tüp teslimatını kime yaptın?', a: 'Hatice Nine\'ye. Gece geç saatte acil tüp istedi. Eve gittiğimde nine çok tedirgin görünüyordu, perdeler kapalıydı.', difficulty: 1, category: 'tanisma' },
            { q: 'Depo kayıtlarında eksik tüp var mı?', a: 'Bir tüp eksik! Ama satış kaydı yok. Birileri gece depoya girmiş olabilir, kilit eski ve kolayca açılır.', difficulty: 2, category: 'derinlesme' },
            { q: 'O gece dükkânına gelen oldu mu?', a: 'Gece 01:00\'de kapım tıklatıldı. Açtığımda Çoban Durmuş\'un oğlu Kerem duruyordu, "babam tüp istiyor" dedi ama Durmuş bunu yalanladı!', difficulty: 3, category: 'yuzlestirme' },
            { q: 'Gaz kokusu nereden geliyor?', a: 'Eski bir tüp sızıntı yapıyor, tamirciye vereceğim. Ama o gece gaz kokusu dükkândan değil, dağ yolundan geliyordu!', difficulty: 2, category: 'derinlesme' },
            { q: 'Eksik tüpü sen mi aldın?', a: 'HAYIR! Depomun anahtarı bende ama yedek anahtar muhtarlıkta. Meliha Hanım her kapının anahtarını alır!', difficulty: 4, category: 'baski' },
            { q: 'Son sözün nedir Şevket?', a: 'Gaz tehlikelidir ama ben güvenlik kurallarına uyarım! O eksik tüpü bulan katili de bulur!', difficulty: 5, category: 'son' }
        ],
        210: [ // Hurdacı Zehra
            { q: 'Son getirilen hurda parçalar nereden geldi?', a: 'Dağdan bir adam getirdi, eski maden ekipmanları. Paslanmış kazma, kırık fener ve bir de... kanlı paçavra!', difficulty: 2, category: 'derinlesme' },
            { q: 'Paslı bıçak kime ait?', a: 'Dükkânımda yüzlerce bıçak var! Ama o özel bıçak... Sapında "D" harfi kazılı. Durmuş\'un mu, bilmiyorum ama öyle geliyor.', difficulty: 3, category: 'yuzlestirme' },
            { q: 'Hurda yığınları arasında saklanan ne?', a: 'Hiçbir şey saklamıyorum! Ama... Ali oğlan dün dağ yolunda kanlı bir bıçak bulup getirdi. Hurda yığınına attım... Belki bakmalıyız?', difficulty: 3, category: 'yuzlestirme' },
            { q: 'O gece dükkânından garip sesler duyuldu mu?', a: 'Geceleri çekiçle çalışırım, eski metalleri düzletirim. Normal sesler! Ama o gece yeraltından garip bir gürültü geldi, tünel gibi...', difficulty: 2, category: 'derinlesme' },
            { q: 'Yeraltı tünelleri hakkında ne biliyorsun?', a: 'Eski maden tünelleri! Babam madenci idi, tünellerin dükkânımın altından geçtiğini söylerdi. Ama girmedim hiç... Korkuyorum.', difficulty: 4, category: 'baski' },
            { q: 'Son sözün nedir Zehra?', a: 'Hurda satarak geçinirim, kimseye zararım yok! Ama o kanlı bıçağı ve tünelleri araştırın, gerçek katil orada gizleniyor!', difficulty: 5, category: 'son' }
        ],
        211: [ // Zeynep Teyze
            { q: 'O gece pencereden kimi gördün?', a: 'Gece yarısı pencereden baktım, muhtarlığın ışığı yanıyordu. Sonra bir gölge dağ yoluna doğru koştu, pelerinli biriydi!', difficulty: 2, category: 'derinlesme' },
            { q: 'Komşularla aranda husumet var mı?', a: 'Hatice Nine ile aramız iyidir ama Emine Hanım\'la arazi sınırı yüzünden kavga ettik. Dağ yamacındaki bahçe kimin?', difficulty: 1, category: 'tanisma' },
            { q: 'Evinde sakladığın eski mektuplar neler?', a: 'Kocamdan kalan mektuplar! Ama bir tanesi... Farklı. Sahaf Hikmet\'ten gelen uyarı mektubu: "Belgeleri kimseye gösterme!"', difficulty: 3, category: 'yuzlestirme' },
            { q: 'Kasabanın geçmişinde karanlık bir olay mı var?', a: '30 yıl önce maden kazası diye kapatılan olay aslında cinayet miydi? Herkes biliyor ama kimse konuşmuyor!', difficulty: 3, category: 'yuzlestirme' },
            { q: 'Sahaf Hikmet sana neden uyarı gönderdi?', a: 'Çünkü kocam eski maden kayıtlarını bana bıraktı! O kayıtlarda kimlerin maden hissesi olduğu yazıyor. Öldürülme sebebi bu!', difficulty: 4, category: 'baski' },
            { q: 'Son sözün nedir Zeynep Teyze?', a: 'Oğlum ben yaşlı bir kadınım, pencereden izlemekten başka ne yapabilirim? Ama gördüklerimi unuttuysan söyleyeyim: katil pelerinli biriydi!', difficulty: 5, category: 'son' }
        ],
        212: [ // Hatice Nine
            { q: 'O gece neden uyumadın?', a: 'Bu yaşta uyku gelmez evladım... Pencereden dışarıyı izliyordum. Dağ yolunda bir fener ışığı gördüm, biri koşuyordu!', difficulty: 1, category: 'tanisma' },
            { q: 'Dışarıdan gelen ayak seslerini tanıdın mı?', a: 'Tanıdım! Ağır, kararlı adımlar. 60 yıllık tecrübemle söylüyorum, o adımlar bir erkeğe ait ve çizme giyiyordu.', difficulty: 2, category: 'derinlesme' },
            { q: 'Eski fotoğraf albümünde saklanan sır ne?', a: 'Albümde 30 yıl önceki maden işçilerinin fotoğrafı var. Ve fotoğrafta ölen adamın yanında duranlardan biri bu kasabada yaşıyor!', difficulty: 3, category: 'yuzlestirme' },
            { q: 'Kasabadaki herkesin geçmişini biliyorsun, anlat bakalım.', a: 'Muhtar Meliha\'nın babası eski madenci idi. Çoban Durmuş\'un ailesi madeni kapatan kişilerle akraba. Sahaf Hikmet buraya taşınmadan önce hakim yardımcısıydı!', difficulty: 3, category: 'yuzlestirme' },
            { q: 'Fotoğraftaki kişi kim?', a: 'Söylersem canım tehlikeye girer! Ama ipucu vereyim: o kişi hâlâ bu kasabada ve cinayet gecesi de dışarıdaydı!', difficulty: 4, category: 'baski' },
            { q: 'Son sözün nedir Hatice Nine?', a: 'Torunlarım kadar yaşındasınız amirim... Bu kasabanın sırları benimle mezara gidecekti ama katil yakalanmalı. Fotoğrafa iyi bakın!', difficulty: 5, category: 'son' }
        ],
        213: [ // Emine Hanım
            { q: 'Yamaçtan kasaba meydanını görebiliyor musun?', a: 'Evet, evim yüksekte olduğu için tüm kasabayı görebiliyorum. Gece ışıkları, gölgeleri, her şeyi izleyebiliyorum.', difficulty: 1, category: 'tanisma' },
            { q: 'O gece fenerle dağa çıkan birini gördün mü?', a: 'GÖRDÜM! Saat 02:00 civarı ahır tarafından dağ yoluna çıkan biri vardı. Fener ışığı sallanıyordu, acele ediyordu!', difficulty: 2, category: 'derinlesme' },
            { q: 'Dokuduğun yün atkılardan birini meydanda bulan oldu mu?', a: 'Evet! Olay yerinde kırmızı-yeşil yün parçası bulunmuş. O benim işim ama... Atkıyı geçen hafta bakkal Cemile\'ye sattım!', difficulty: 3, category: 'yuzlestirme' },
            { q: 'Kasabada en çok kimden şüpheleniyorsun?', a: 'Hurdacı Zehra! Gece yarıları dükkânında çalışıyor, yeraltından sesler geliyor. Bir de Sahaf Hikmet... O adam çok gizemli.', difficulty: 2, category: 'derinlesme' },
            { q: 'Gece yürüyüşlerinde ne yapıyorsun?', a: 'Doğa beni rahatlatıyor! Ama son yürüyüşte dağ yolunda taze kazılmış bir çukur gördüm. İçinde bir şey gömülüydü!', difficulty: 4, category: 'baski' },
            { q: 'Son sözün nedir Emine Hanım?', a: 'Ben yamaçta huzur arıyorum amirim. Ama bu kasabada huzur yok. O gömülen şeyi kazın, katil oradadır!', difficulty: 5, category: 'son' }
        ]
    }
};

// Bina içi ekstra NPC konuşma balonu konumları (görseldeki karakterlerin üzerinde)
window.SISOREN_INTERIOR_POSITIONS = {
    // Kahvehane - 3 farklı konumda karakterler
    kahve_celal: { top: '35%', left: '25%' },
    kahve_hamdi: { top: '55%', left: '60%' },
    kahve_cirak: { top: '70%', left: '45%' },
    // Bakkal - gofret alan kız
    bakkal_cocuk_1: { top: '65%', left: '35%' },
    // Sahaf - 2 çocuk
    sahaf_cocuk_1: { top: '45%', left: '30%' },
    sahaf_cocuk_2: { top: '55%', left: '70%' },
    // Ahır - çoban çırağı
    ahir_cocuk_1: { top: '60%', left: '50%' },
    // Telgrafhane - çırak
    telgraf_cirak: { top: '40%', left: '55%' },
    // Sinema - biletçi
    sinema_biletci: { top: '35%', left: '40%' },
    // Muhtarlık - kâtip
    muhtarlik_katip: { top: '50%', left: '60%' },
    // Tütüncü - müşteri
    tutuncu_musteri: { top: '55%', left: '35%' },
    // Hurdacı - çocuk
    hurdaci_cocuk: { top: '65%', left: '40%' },
    // Tüpçü - yardımcı
    tupcu_yardimci: { top: '45%', left: '50%' }
};

// Her görselin içindeki karakterlere göre normalize edilmiş tıklama noktaları.
// Noktalar sahne yüzdesidir; görsel laptopta yeniden ölçeklense bile karakterle birlikte kalır.
window.SISOREN_INTERIOR_LAYOUTS = {
    kahvehane: {
        primary: { top: '56%', left: '68%' },
        kahve_celal: { top: '54%', left: '22%' },
        kahve_hamdi: { top: '65%', left: '42%' },
        kahve_cirak: { top: '40%', left: '72%' }
    },
    bakkal: {
        primary: { top: '58%', left: '70%' },
        bakkal_cocuk_1: { top: '58%', left: '38%' }
    },
    sahaf: {
        primary: { top: '58%', left: '70%' },
        sahaf_cocuk_1: { top: '74%', left: '30%' },
        sahaf_cocuk_2: { top: '74%', left: '46%' }
    },
    ahir: {
        primary: { top: '58%', left: '30%' },
        ahir_cocuk_1: { top: '54%', left: '54%' }
    }
};

// Sisören'in mevcut iç mekân görsellerini bütün binalara güvenli biçimde bağla.
// Yeni görseller eklendiğinde bu eşleme yalnızca ilgili bina için değiştirilebilir.
(function normalizeSisorenPresentation() {
    const config = window.SISOREN_CONFIG;
    const availableInteriors = [
        'images/towns/sisoren/interiors/kahvehane_interior.jpg',
        'images/towns/sisoren/interiors/bakkal_interior.jpg',
        'images/towns/sisoren/interiors/sahaf_interior.jpg',
        'images/towns/sisoren/interiors/ahir_interior.jpg'
    ];
    const primaryPositions = [
        { top: '55%', left: '30%' }, { top: '58%', left: '58%' },
        { top: '56%', left: '76%' }, { top: '60%', left: '24%' },
        { top: '58%', left: '72%' }, { top: '54%', left: '62%' },
        { top: '58%', left: '42%' }, { top: '55%', left: '66%' },
        { top: '60%', left: '34%' }, { top: '58%', left: '72%' },
        { top: '62%', left: '30%' }, { top: '62%', left: '62%' },
        { top: '55%', left: '48%' }
    ];

    config.buildings.forEach((building, index) => {
        building.interiorImg = building.interiorImg || availableInteriors[index % availableInteriors.length];
        if (building.npc) {
            building.npc.bg = building.interiorImg;
            building.npc.talkBg = building.npc.portrait;
        }
        const layout = window.SISOREN_INTERIOR_LAYOUTS[building.id];
        building.interiorLayout = layout || {};
        building.primaryNpcPos = (layout && layout.primary) || building.primaryNpcPos || primaryPositions[index];
        if (!building.hotspots || building.hotspots.length === 0) {
            building.hotspots = [1, 2, 3, 4].map((slot, slotIndex) => ({
                id: building.npcId * 10 + slotIndex + 1,
                name: `${building.title} inceleme noktası ${slot}`,
                desc: `${building.title} içindeki bu bölümde soruşturmayla bağlantılı bir iz aranabilir.`,
                top: `${28 + slotIndex * 16}%`,
                left: `${22 + (slotIndex % 2) * 52}%`,
                img: building.interiorImg,
                relatedNPCId: building.npcId,
                fingerprintSpot: null,
                bloodSpot: null
            }));
        }
    });

    Object.keys(config.fallbackQuestions).forEach(npcId => {
        const pool = config.fallbackQuestions[npcId];
        const categories = ['tanisma', 'derinlesme', 'yuzlestirme', 'baski', 'son'];
        let index = 0;
        while (pool.length < 20) {
            const source = pool[index % pool.length];
            const round = Math.floor(pool.length / 4) + 1;
            pool.push({
                ...source,
                q: `${source.q} Başka hangi ayrıntıyı hatırlıyorsun? (Soru ${pool.length + 1})`,
                a: `${source.a} Bu konuda hatırladığım ek ayrıntı şu: olayın zamanı ve yeri konusunda dikkatli düşününce başka bir iz daha ortaya çıkıyor.`,
                category: categories[Math.floor(pool.length / 4) % categories.length],
                difficulty: Math.min(5, Math.max(1, source.difficulty || round))
            });
            index++;
        }
    });
})();
