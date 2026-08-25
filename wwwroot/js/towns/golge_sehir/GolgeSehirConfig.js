/**
 * GÖLGE ŞEHİR (SHADOW CITY) CONFIGURATION MODULE v8.0
 * 8 Binası, 32 Delili, Mantıklı Yerleşim Alanları ve Özgün Soruşturma Akışı
 */

window.GOLGE_SEHIR_CONFIG = {
    townId: 'golge_sehir',
    townName: 'Gölge Şehir',
    victimName: 'Tüccar Ekrem Bey',
    mapImage: 'images/towns/golge_sehir/golge_sehir_map.png',

    storyIntroText: "Sisli ve fırtınalı bir gece... Gölge Şehir'in çam ormanı girişinde bir ceset bulundu. Kurban, kasabanın en zengin ve tekinsiz tüccarı Ekrem Bey'di. Islak çam iğnelerinin üzerinde yatan cansız beden, gaz lambalarının altında solgun bir ışıkla aydınlanıyordu.\n\nOrman sınırında toplanan kasabalılar, birbirlerine derin bir şüpheyle bakıyordu. Başarılı dedektif olarak bu karmaşık davayı çözmek için Gölge Şehir'e çağrıldınız. Sekiz şüpheli, sekiz bina, sayısız karanlık sır... Gerçeği ortaya çıkarabilecek misiniz?",

    assistants: {
        primary: {
            name: 'Yardımcı Dedektif Çetin',
            portrait: 'images/dedektif_helper.png',
            subtitle: 'Olay Yeri & Adli Analiz Uzmanı'
        },
        local: {
            name: 'Bekçi Rıfat (Gölge Şehir Gece Bekçisi)',
            portrait: 'images/towns/golge_sehir/npcler/bekci_hasan_helper.png',
            subtitle: 'Gölge Şehir Yerel İz Sürücüsü & Fenercisi'
        },
        introDialogue: [
            {
                speaker: 'Bekçi Rıfat',
                text: 'Durun bakalım! Kim var orada? Fenerimi gözünüze tutturmayın! Ha... Demek Gizemli Kasaba\'daki cinayeti çözen o meşhur dedektif heyeti sizsiniz! Yanınızdaki de fötr şapkalı çaylak mı?'
            },
            {
                speaker: 'Çetin',
                text: 'Aşk olsun Rıfat amca! Biz amirimle birlikte Gizemli Kasaba davasını tereyağından kıl çeker gibi aydınlattık! Şimdi de Gölge Şehir\'in sırlarını çözmeye geldik. Feneri biraz indirirseniz önümüzü göreceğiz.'
            },
            {
                speaker: 'Bekçi Rıfat',
                text: 'Bana bak mektepli dedektif, senin o laboratuvar kitapların bu sisli sokakları kurtarmaz! 40 yıldır bu taşlardayım. Tüccar Ekrem\'in öldüğü gece tüm kasaba birbirine girdi. Kimin kimle hesabı olduğunu sadece benim fenerim aydınlatır!'
            },
            {
                speaker: 'Çetin',
                text: 'Merak etmeyin Rıfat amca, amirimin dedektiflik tecrübesi ve delil analizimizle katili hemen köşeye sıkıştıracağız! Bu kasabada 8 şüpheli mekân var. En az 4 binayı incelemeli ve 4 delili Adli Tıbba iletmeliyiz.'
            },
            {
                speaker: 'Bekçi Rıfat',
                text: 'Hah! Sen yine mikroskoplarına güven bakalım. Ama Manav Ayşe\'nin ipotek borcunu, Hekim Sevgi\'nin zehirli otlarını, Bakkal Naciye\'nin yırttığı defteri ararken benim dedikodularıma muhtaç olacaksınız!'
            },
            {
                speaker: 'Çetin',
                text: 'O zaman güçlerimizi birleştirelim Rıfat amca! Siz yerel sırları fısıldayın, biz de amirimle bilimsel izleri birleştirelim. Katil bu 8 şüpheliden biri, adaletten kaçamayacak!'
            }
        ]
    },

    buildings: [
        // 1. ODUNCU (ID 101)
        {
            id: 'oduncu',
            npcId: 101,
            title: 'Oduncu',
            icon: 'fa-solid fa-axe',
            hoverTag: 'ODUNCU',
            wideImg: 'images/towns/golge_sehir/binalar/oduncu_interior.png',
            interiorImg: 'images/towns/golge_sehir/binalar/oduncu_interior.png',
            style: { top: '25%', left: '10%', width: '14%', height: '16%' },
            npc: {
                id: 101,
                name: 'Oduncu Tahsin',
                building: 'Oduncu',
                role: 'Oduncu',
                portrait: 'images/towns/golge_sehir/npcler/npc_101_talk.jpg',
                bg: 'images/towns/golge_sehir/binalar/oduncu_interior.png',
                talkBg: 'images/towns/golge_sehir/npcler/npc_101_talk.jpg',
                greeting: 'Ormandan gelen taze çam kokusu gibisi yoktur amirim... Ama bu gece orman bir garip sesler çıkarıyordu.',
                questions: ['Cinayet gecesi ormanda kimi gördün?', 'Baltanı en son nerede kullandın?', 'Ekrem Bey ile arandaki borç meselesi nedir?', 'Ormanda duyduğun sesler kime aitti?'],
                murderStory: 'Yağmurlu ve tekinsiz bir sonbahar gecesiydi. Oduncu Tahsin, Çam Ormanı\'nın girişinde, elinde feneriyle Ekrem Bey\'i bekliyordu. Ekrem Bey\'in elinde Tahsin\'in ormandaki yasadışı kereste kesimlerini belgeleyen evraklar vardı. "Ya arazini bana yoksasına devredersin ya da yarın jandarma kapına dayanır," diyerek gülümsedi Ekrem Bey. Yılların yorgunluğu ve toprağını kaybetme korkusu Tahsin\'in gözünü döndürdü. "Ben bu ormana ömrümü verdim!" diye kükredi Tahsin. Tartışma şiddetlendi. Ekrem arkasını dönüp gitmeye yeltendiği sırada, Tahsin elindeki taze reçineli ağır oduncu baltasını var gücüyle havaya kaldırdı. Baltanın kör tarafı Ekrem\'in kafasının arkasına şiddetle indi. Ekrem tek kelime edemeden çamur birikintisinin içine yığıldı. Tahsin, panik içinde kanlı baltayı kulübesindeki tahtaların arasına sakladı. Baltanın sapındaki kanı silmeye çalıştı ancak ormanın taze çam reçinesi kanla birbirine karışıp kurumuştu. O geceki fırtına sesleri bastırsa da, yerde bıraktığı derin çamurlu ayak izleri ve reçineli balta, işlediği korkunç cinayetin sessiz tanıklarıydı.'
            },
            hotspots: [
                {
                    id: 1011,
                    name: 'Ağır Balta',
                    desc: 'Orman işlerinde kullanılan ağır saplı balta.',
                    img: 'images/towns/golge_sehir/deliller/1011.jpg',
                    top: '72%',
                    left: '18%',
                    fingerprintSpot: { xRatio: 0.5, yRatio: 0.8, angle: 0 }, bloodSpot: { xRatio: 0.45, yRatio: 0.2, angle: 0 }
                },
                {
                    id: 1012,
                    name: 'Kereste Kayıt Defteri',
                    desc: 'Siparişlerin ve günlük kereste kesim adetlerinin tutulduğu eski bir defter.',
                    img: 'images/towns/golge_sehir/deliller/1012.jpg',
                    top: '35%',
                    left: '78%',
                    fingerprintSpot: { xRatio: 0.8, yRatio: 0.5, angle: 0 }, bloodSpot: null
                },
                {
                    id: 1013,
                    isHidden: true,
                    name: 'Ağır İş Eldiveni',
                    desc: 'Soğuk havalarda ve ağır işlerde kullanılan, kalın deriden yapılmış eldiven.',
                    img: 'images/towns/golge_sehir/deliller/1013.jpg',
                    top: '78%',
                    left: '52%',
                    fingerprintSpot: { xRatio: 0.4, yRatio: 0.6, angle: 0 }, bloodSpot: { xRatio: 0.7, yRatio: 0.3, angle: 0 }
                },
                {
                    id: 1014,
                    name: 'Yontulmuş Çam Kütüğü',
                    desc: 'Üzerinde bıçakla kazınmış gizli harfler bulunan taze kesilmiş kütük.',
                    img: 'images/towns/golge_sehir/deliller/1014.jpg',
                    top: '42%',
                    left: '25%',
                    fingerprintSpot: { xRatio: 0.8, yRatio: 0.8, angle: 0 }, bloodSpot: null
                }
            ]
        },

        // 2. MANAV (ID 102)
        {
            id: 'manav',
            npcId: 102,
            title: 'Manav',
            icon: 'fa-solid fa-carrot',
            hoverTag: 'MANAV',
            wideImg: 'images/towns/golge_sehir/binalar/manav_interior.png',
            interiorImg: 'images/towns/golge_sehir/binalar/manav_interior.png',
            style: { top: '33%', left: '25%', width: '14%', height: '16%' },
            npc: {
                id: 102,
                name: 'Manav Ayşe',
                building: 'Manav',
                role: 'Manav',
                portrait: 'images/towns/golge_sehir/npcler/npc_102_talk.jpg',
                bg: 'images/towns/golge_sehir/binalar/manav_interior.png',
                talkBg: 'images/towns/golge_sehir/npcler/npc_102_talk.jpg',
                greeting: 'Hoş geldiniz amirim, taze meyvelerim gibi temiz bir kasabayız aslında!',
                questions: ['Cinayet gecesi saat kaçta uyudun?', 'Ekrem Bey senden ne istiyordu?', 'Dükkanın etrafında dolaşan yabancı kimdi?', 'Çamur izleri hakkında ne biliyorsun?'],
                murderStory: 'Manav Ayşe için o gece, her şeyin bittiği an olacaktı. Ekrem Bey, aylardır ödenmeyen borçlar yüzünden dükkâna haciz getireceğini kesin bir dille bildirmişti. Ayşe, gece yarısı kasabanın ıssız sokaklarında siyah pelerinine sarınarak Ekrem Bey\'in evine gizlice gitti. Amacı sadece biraz daha zaman istemekti. Kapıyı çaldığında Ekrem Bey onu alaycı bir tavırla içeri aldı. "Ağlamaların borcunu ödemez Ayşe, yarın o dükkân benim!" diyerek sırtını döndü. Bütün hayatını verdiği dükkânı kaybetme düşüncesi Ayşe\'yi delirtmişti. Gözü, masanın üzerinde duran uzun, sivri meyve bıçağına takıldı. Bir anlık cinnetle bıçağı kaptığı gibi Ekrem\'in göğsüne sapladı. Ekrem acı içinde yere yığılırken elleriyle Ayşe\'nin pelerinine tutundu ve siyah kumaştan bir parçayı yırttı. Ayşe dehşet içinde geri çekildi, elleri titreyerek dükkânına koştu. Kanlı ellerini yıkadıktan sonra, dükkânın anahtarını meyve kasalarının en dibine sakladı. Kopardığı hayatın bedelini meyvelerin ardına gizleyebileceğini sanıyordu ama kopan pelerin parçası gerçeği haykırıyordu.'
            },
            hotspots: [
                {
                    id: 1021,
                    name: 'Meyve Kasası',
                    desc: 'Tezgahın altında duran eski ahşap kasa.',
                    img: 'images/towns/golge_sehir/deliller/1021.jpg',
                    top: '75%',
                    left: '15%',
                    fingerprintSpot: { xRatio: 0.2, yRatio: 0.4, angle: 0 }, bloodSpot: null
                },
                {
                    id: 1022,
                    name: 'Elma',
                    desc: 'Tezgahta satılmayı bekleyen elma.',
                    img: 'images/towns/golge_sehir/deliller/1022.jpg',
                    top: '40%',
                    left: '32%',
                    fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: { xRatio: 0.3, yRatio: 0.7, angle: 0 }
                },
                {
                    id: 1023,
                    name: 'Siyah Kumaş',
                    desc: 'Ahşap tezgâhta bulunan siyah kumaş parçası.',
                    img: 'images/towns/golge_sehir/deliller/1023.jpg',
                    top: '32%',
                    left: '75%',
                    fingerprintSpot: null, bloodSpot: { xRatio: 0.6, yRatio: 0.3, angle: 0 }
                },
                {
                    id: 1024,
                    isHidden: true,
                    name: 'Kâğıt Parçası',
                    desc: 'Top haline getirilmiş, üzerinde anlaşılmayan yazılar olan bir kağıt parçası.',
                    img: 'images/towns/golge_sehir/deliller/1024.jpg',
                    top: '70%',
                    left: '62%',
                    fingerprintSpot: { xRatio: 0.8, yRatio: 0.8, angle: 0 }, bloodSpot: null
                }
            ]
        },

        // 3. DEMİRCİ (ID 103)
        {
            id: 'demirci',
            npcId: 103,
            title: 'Demirci',
            icon: 'fa-solid fa-anvil',
            hoverTag: 'DEMİRCİ',
            wideImg: 'images/towns/golge_sehir/binalar/demirci_interior.png',
            interiorImg: 'images/towns/golge_sehir/binalar/demirci_interior.png',
            style: { top: '52%', left: '20%', width: '14%', height: '16%' },
            npc: {
                id: 103,
                name: 'Demirci Kazım',
                building: 'Demirci',
                role: 'Demirci',
                portrait: 'images/towns/golge_sehir/npcler/npc_103_talk.jpg',
                bg: 'images/towns/golge_sehir/binalar/demirci_interior.png',
                talkBg: 'images/towns/golge_sehir/npcler/npc_103_talk.jpg',
                greeting: 'Kızgın demir döverken laf dinlemek zordur amirim... Sorunuzu çabuk sorun.',
                questions: ['Bu özel çelik kilidi kimin için yaptın?', 'Gece ocağı ne zaman söndürdün?', 'Bıçaktaki özel damga senin işin mi?', 'Demir tozları neden her yerde?'],
                murderStory: 'Demirci Kazım, kasabanın en suskun ama en maharetli ustasıydı. Ekrem Bey\'in yasadışı evraklarını sakladığı o özel şifreli çelik kasayı bizzat kendi elleriyle yapmıştı. Ancak Ekrem Bey, kasanın sırrının sızmasından korktuğu için Kazım\'ı, "Eğer kasanın varlığı duyulursa kaçakçılık suçunu sana yıkarım" diyerek tehdit etmeye başlamıştı. O gece Kazım, meseleyi sonsuza dek çözmek için Ekrem\'in evine gitti. Tartışma kısa sürede arbedeye dönüştü. Kazım, yanında getirdiği ve kendi özel ay damgasını taşıyan ağır, kükürtlü bir demir çubuğu çıkardı. Ekrem\'in şakağına indirdiği tek bir darbe, tüccarın yere yığılması için yetti. Kurbanın ceketine ve yere, Kazım\'ın ocağından gelen ince kükürtlü demir tozları döküldü. Kazım soğukkanlılıkla evden çıktı, suç aletini kendi dükkânındaki kızgın ocağa atıp eritmeyi düşündü. Ancak aceleyle dükkânın köşesine fırlattığı demir parçası ve olay yerinde bıraktığı ince kükürt tozları, suskun demircinin işlediği bu vahşi cinayeti gün yüzüne çıkarmaya yetecekti.'
            },
            hotspots: [
                {
                    id: 1031,
                    name: 'Ağır Demirci Çekici',
                    desc: 'Sapı kararmış, başı oldukça ağır ve dayanıklı metalden yapılma çekiç.',
                    img: 'images/towns/golge_sehir/deliller/1031.jpg',
                    top: '68%',
                    left: '42%',
                    fingerprintSpot: { xRatio: 0.7, yRatio: 0.2, angle: 0 }, bloodSpot: { xRatio: 0.3, yRatio: 0.7, angle: 0 }
                },
                {
                    id: 1032,
                    isHidden: true,
                    name: 'Ağır Asma Kilit',
                    desc: 'Kapıları kilitlemek için kullanılan büyük ebatlı çelik kilit.',
                    img: 'images/towns/golge_sehir/deliller/1032.jpg',
                    top: '38%',
                    left: '80%',
                    fingerprintSpot: { xRatio: 0.5, yRatio: 0.8, angle: 0 }, bloodSpot: null
                },
                {
                    id: 1033,
                    name: 'Körük Tozu Numunesi',
                    desc: 'Demir atölyesinden alınmış siyah ve gri renkte toz tanecikleri.',
                    img: 'images/towns/golge_sehir/deliller/1033.jpg',
                    top: '78%',
                    left: '18%',
                    fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }, bloodSpot: null
                },
                {
                    id: 1034,
                    name: 'Deri İş Önlüğü',
                    desc: 'Ateşten ve kıvılcımdan korunmak için kullanılan kalın deri atölye önlüğü.',
                    img: 'images/towns/golge_sehir/deliller/1034.jpg',
                    top: '32%',
                    left: '20%',
                    fingerprintSpot: { xRatio: 0.4, yRatio: 0.2, angle: 0 }, bloodSpot: { xRatio: 0.5, yRatio: 0.6, angle: 0 }
                }
            ]
        },

        // 4. BAKKAL (ID 104)
        {
            id: 'bakkal',
            npcId: 104,
            title: 'Bakkal',
            icon: 'fa-solid fa-store',
            hoverTag: 'BAKKAL',
            wideImg: 'images/towns/golge_sehir/binalar/bakkal_interior.png',
            interiorImg: 'images/towns/golge_sehir/binalar/bakkal_interior.png',
            style: { top: '42%', left: '39%', width: '14%', height: '16%' },
            npc: {
                id: 104,
                name: 'Bakkal Naciye',
                building: 'Bakkal',
                role: 'Bakkal',
                portrait: 'images/towns/golge_sehir/npcler/npc_104_talk.jpg',
                bg: 'images/towns/golge_sehir/binalar/bakkal_interior.png',
                talkBg: 'images/towns/golge_sehir/npcler/npc_104_talk.jpg',
                greeting: 'Aaa amirim hoş geldiniz sefalar getirdiniz! Gölge Şehir havadisleri bakkaldan geçer.',
                questions: ['Veresiye defterindeki isim neden karalandı?', 'Zehri kime sattığını hatırlıyor musun?', 'Ekrem Bey dükkanda ne kadar kaldı?', 'Gece dükkanın arkasından gelen sesler neydi?'],
                murderStory: 'Gölge Şehir\'de herkesin sırrını bilen Bakkal Naciye, kendi sırrının altında eziliyordu. Ekrem Bey, kasabanın en zengini olmasına rağmen yıllardır bakkal borcunu ödemiyor, üstelik Naciye\'yi "Dükkânını elinden alırım" diyerek sindiriyordu. Artık tahammülü kalmayan Naciye, o gece ince ve kusursuz bir plan yaptı. Ekrem Bey\'in her akşam içtiği özel tütün kesesinin içine dükkânında sattığı güçlü bir fare zehrini zerk etti. Gece yarısı "Borç hesabı için" diyerek Ekrem Bey\'in evine gitti ve tütün kesesini sinsice masasına bıraktı. Ekrem, Naciye gittikten sonra zehirli tütünden derin bir nefes çekti. Saniyeler içinde boğazı düğümlendi, nefessiz kalarak yere yığıldı ve acı içinde can verdi. Naciye ise o sırada dükkânına dönmüş, elleri titreyerek veresiye defterini açmıştı. Ekrem\'in adının olduğu sayfayı hışımla yırtıp sobaya attı. Borç bitmişti, tehdit ortadan kalkmıştı. Ancak masanın üzerinde unutulan tütün kesesindeki zehir partikülleri ve sobanın kenarında kalan yanık kâğıt parçaları, intikamın kokusunu taşıyordu.'
            },
            hotspots: [
                {
                    id: 1041,
                    name: 'Veresiye Defteri',
                    desc: 'Müşteri borçlarının yazıldığı bakkal defteri.',
                    img: 'images/towns/golge_sehir/deliller/1041.jpg',
                    top: '65%',
                    left: '25%',
                    fingerprintSpot: { xRatio: 0.8, yRatio: 0.5, angle: 0 }, bloodSpot: null
                },
                {
                    id: 1042,
                    name: 'Boş Cam Şişe',
                    desc: 'Yeşil camdan yapılmış, kapağı açık eski bir sıvı şişesi.',
                    img: 'images/towns/golge_sehir/deliller/1042.jpg',
                    top: '32%',
                    left: '72%',
                    fingerprintSpot: { xRatio: 0.5, yRatio: 0.2, angle: 0 }, bloodSpot: { xRatio: 0.5, yRatio: 0.8, angle: 0 }
                },
                {
                    id: 1043,
                    name: 'İşlemeli Tütün Kesesi',
                    desc: 'İçerisinde kurutulmuş tütün yaprakları saklanan, deriden mamul kese.',
                    img: 'images/towns/golge_sehir/deliller/1043.jpg',
                    top: '75%',
                    left: '58%',
                    fingerprintSpot: { xRatio: 0.5, yRatio: 0.1, angle: 0 }, bloodSpot: null
                },
                {
                    id: 1044,
                    isHidden: true,
                    name: 'Küçük Metal Anahtar',
                    desc: 'Küçük bir kilide veya sandığa ait olduğu düşünülen pirinç anahtar.',
                    img: 'images/towns/golge_sehir/deliller/1044.jpg',
                    top: '82%',
                    left: '12%',
                    fingerprintSpot: { xRatio: 0.8, yRatio: 0.2, angle: 0 }, bloodSpot: null
                }
            ]
        },

        // 5. HEKİM (ID 105)
        {
            id: 'hekim',
            npcId: 105,
            title: 'Hekim',
            icon: 'fa-solid fa-notes-medical',
            hoverTag: 'HEKİM',
            wideImg: 'images/towns/golge_sehir/binalar/hekim_interior.png',
            interiorImg: 'images/towns/golge_sehir/binalar/hekim_interior.png',
            style: { top: '48%', left: '51%', width: '14%', height: '16%' },
            npc: {
                id: 105,
                name: 'Hekim Sevgi',
                building: 'Hekim',
                role: 'Hekim',
                portrait: 'images/towns/golge_sehir/npcler/npc_105_talk.jpg',
                bg: 'images/towns/golge_sehir/binalar/hekim_interior.png',
                talkBg: 'images/towns/golge_sehir/npcler/npc_105_talk.jpg',
                greeting: 'Hekimlik yeminim sır saklamayı gerektirir amirim... Ama cinayet kasabayı sarstı.',
                questions: ['Bu mor sızıntı hangi bitkiden elde ediliyor?', 'Muayenehaneye son gelen hasta kimdi?', 'Ekrem Bey sağlık sorunları için mi geldi?', 'Neşterin üzerindeki izler ne anlama geliyor?'],
                murderStory: 'Hekim Sevgi, kasabanın şifa kaynağıydı ancak Ekrem Bey onun en karanlık sırrını, geçmişte yaptığı ve bir hastanın ölümüne neden olan tıbbi hatayı öğrenmişti. Ekrem bu sırrı kullanarak aylardır Sevgi\'den bedava ilaçlar ve haraç alıyor, onu hapse attırmakla tehdit ediyordu. O gece Sevgi\'nin sabrı taştı. Serasında özel olarak yetiştirdiği, son derece zehirli banotu köklerinden koyu mor renkli, ölümcül bir iksir hazırladı. Ekrem Bey\'in rutini olan mide şurubunun içine bu mor iksiri damla damla enjekte etti. Gece yarısı ilacını içen Ekrem, damarlarında dolaşan ateşle kıvranmaya başladı. Vücudu kasılıyor, zehrin etkisiyle tırnak dipleri morarıyordu. O sırada evde olan Sevgi, şantaj belgelerini aramak için Ekrem\'in masasını karıştırıyordu. Ekrem son bir gayretle Sevgi\'nin kolunu yakaladı. Sevgi cebindeki küçük pirinç neşteri çekip Ekrem\'in elini kesti ve onu ölüme terk edip kaçtı. Muayenehanesine dönüp neşteri kutusuna koydu ama üzerinde kalan kan lekesi ve kurbanın morarmış tırnakları tıbbi bir cinayetin tartışmasız kanıtlarıydı.'
            },
            hotspots: [
                {
                    id: 1051,
                    name: 'Cam Şişe',
                    desc: 'Ağzı kapalı duran tıbbi cam şişe.',
                    img: 'images/towns/golge_sehir/deliller/1051.jpg',
                    top: '40%',
                    left: '75%',
                    fingerprintSpot: { xRatio: 0.2, yRatio: 0.5, angle: 0 }, bloodSpot: null
                },
                {
                    id: 1052,
                    name: 'Reçete Sayfası',
                    desc: 'Üzerinde çeşitli tıbbi semboller ve ilaç isimleri bulunan kağıt.',
                    img: 'images/towns/golge_sehir/deliller/1052.jpg',
                    top: '72%',
                    left: '32%',
                    fingerprintSpot: { xRatio: 0.8, yRatio: 0.8, angle: 0 }, bloodSpot: null
                },
                {
                    id: 1053,
                    isHidden: true,
                    name: 'Kurutulmuş Bitki Kökü',
                    desc: 'Tıbbi karışımlarda kullanıldığı düşünülen sert ve kokulu kök parçası.',
                    img: 'images/towns/golge_sehir/deliller/1053.jpg',
                    top: '35%',
                    left: '18%',
                    fingerprintSpot: { xRatio: 0.8, yRatio: 0.8, angle: 0 }, bloodSpot: null
                },
                {
                    id: 1054,
                    name: 'Kesici El Aleti',
                    desc: 'Pirinç saplı, oldukça keskin uca sahip ufak boyutta tıbbi kesici alet.',
                    img: 'images/towns/golge_sehir/deliller/1054.jpg',
                    top: '78%',
                    left: '60%',
                    fingerprintSpot: { xRatio: 0.7, yRatio: 0.7, angle: 0 }, bloodSpot: { xRatio: 0.2, yRatio: 0.2, angle: 0 }
                }
            ]
        },

        // 6. MUHTARLIK (ID 106)
        {
            id: 'muhtar',
            npcId: 106,
            title: 'Muhtarlık',
            icon: 'fa-solid fa-landmark',
            hoverTag: 'MUHTARLIK',
            wideImg: 'images/towns/golge_sehir/binalar/muhtar_interior.png',
            interiorImg: 'images/towns/golge_sehir/binalar/muhtar_interior.png',
            style: { top: '50%', left: '63%', width: '14%', height: '16%' },
            npc: {
                id: 106,
                name: 'Muhtar Cevdet',
                building: 'Muhtarlık',
                role: 'Muhtar',
                portrait: 'images/towns/golge_sehir/npcler/npc_106_talk.jpg',
                bg: 'images/towns/golge_sehir/binalar/muhtar_interior.png',
                talkBg: 'images/towns/golge_sehir/npcler/npc_106_talk.jpg',
                greeting: 'Gölge Şehir sakin bir yerdir dedektif bey. Bu olayı lekelemeden çözmeliyiz.',
                questions: ['Orman arazisiyle ilgili tapu kimde?', 'Olay gecesi köy meydanında kimler vardı?', 'Ekrem Bey ile tartıştığınız doğru mu?', 'Masandaki mühürlü mektup kime gidecekti?'],
                murderStory: 'Kasabanın sözü geçen adamı Muhtar Cevdet, hırsının kurbanı olmuştu. Ekrem Bey\'in çam ormanındaki arazisine gizlice sahte bir devir tapusu hazırlatmış ve satmaya kalkmıştı. Ancak Ekrem bu sahtekârlığı öğrenmiş, "Yarın sabah kaymakamlığa gidip senin bu rezilliğini, o mühürlü sahte belgelerle birlikte ortaya dökeceğim!" diyerek muhtarı köşeye sıkıştırmıştı. İtibarını ve makamını kaybetme korkusuyla dehşete düşen Cevdet, gece 02:00 sularında Ekrem\'in evine gizlice girdi. Masasında oturan Ekrem, karşısında Cevdet\'i görünce bağırmaya yeltendi. Cevdet masanın üzerindeki ağır pirinç şamdana uzandı ve var gücüyle Ekrem\'in başına geçirdi. Arbede sırasında Cevdet\'in yüzünden düşen altın çerçeveli gözlüğünün camı kırıldı. Ekrem yere yığılıp son nefesini verirken Cevdet panikledi. Sahte tapuyu ve resmi mührünü toplayıp koşarak evden çıktı. Ancak kırık altın gözlüğünü halının kenarında unutmuştu. Muhtarlık ofisine gidip çekmecesini çelik anahtarıyla kilitledi ama olay yerinde unuttuğu o tek parça cam, bütün itibarını yerle bir etmeye yetecekti.'
            },
            hotspots: [
                {
                    id: 1061,
                    name: 'Katlanmış Evrak',
                    desc: 'Üzerinde resmi mühür bulunan, katlanmış bir evrak parçası.',
                    img: 'images/towns/golge_sehir/deliller/1061.jpg',
                    top: '68%',
                    left: '28%',
                    fingerprintSpot: { xRatio: 0.8, yRatio: 0.8, angle: 0 }, bloodSpot: null
                },
                {
                    id: 1062,
                    name: 'Arazi Belgesi',
                    desc: 'Çam ormanıyla ilgili bir mülkiyet belgesi.',
                    img: 'images/towns/golge_sehir/deliller/1062.jpg',
                    top: '38%',
                    left: '72%',
                    fingerprintSpot: { xRatio: 0.6, yRatio: 0.8, angle: 0 }, bloodSpot: null
                },
                {
                    id: 1063,
                    isHidden: true,
                    name: 'Uzun Kasa Anahtarı',
                    desc: 'Sağlam bir kilidi veya kasayı açmaya yarayan ağır anahtar.',
                    img: 'images/towns/golge_sehir/deliller/1063.jpg',
                    top: '78%',
                    left: '52%',
                    fingerprintSpot: { xRatio: 0.2, yRatio: 0.5, angle: 0 }, bloodSpot: null
                },
                {
                    id: 1064,
                    name: 'Altın Çerçeveli Gözlük',
                    desc: 'Sol camı hafif çatlamış, zarif tel çerçeveli okuma gözlüğü.',
                    img: 'images/towns/golge_sehir/deliller/1064.jpg',
                    top: '85%',
                    left: '12%',
                    fingerprintSpot: { xRatio: 0.2, yRatio: 0.3, angle: 0 }, bloodSpot: { xRatio: 0.8, yRatio: 0.5, angle: 0 }
                }
            ]
        },

        // 7. FEHMİ BEY (ID 107)
        {
            id: 'kasabali_evi',
            npcId: 107,
            title: 'Fehmi Bey Ev',
            icon: 'fa-solid fa-house-user',
            hoverTag: 'FEHMİ BEY EV',
            wideImg: 'images/towns/golge_sehir/binalar/kasabali_evi_interior.png',
            interiorImg: 'images/towns/golge_sehir/binalar/kasabali_evi_interior.png',
            style: { top: '71%', left: '43%', width: '14%', height: '16%' },
            npc: {
                id: 107,
                name: 'Fehmi Bey',
                building: 'Fehmi Bey Ev',
                role: 'Emekli Öğretmen',
                img: 'images/towns/golge_sehir/npcler/npc_107_talk.jpg',
                portrait: 'images/towns/golge_sehir/npcler/npc_107_talk.jpg',
                bg: 'images/towns/golge_sehir/binalar/kasabali_evi_interior.png',
                talkBg: 'images/towns/golge_sehir/npcler/npc_107_talk.jpg',
                greeting: 'Penceremin önünde oturup gaz lambasında kitap okurdum evladım... Gece 02:14\'te sesler geldi.',
                questions: ['Saatiniz neden tam cinayet saatinde durdu?', 'Pencerenizin altından geçenler kimdi?', 'Kitaptaki notun cinayetle ilgisi nedir?', 'Gece dışarı çıkıp feneri neden kullandınız?'],
                murderStory: 'Emekli öğretmen Fehmi Bey, ömrünü kitaplara ve babasından kalan yadigârlara adamış sakin bir adamdı. Ancak Ekrem Bey, tefecilik oyunlarıyla Fehmi Bey\'in babasından kalma o çok değerli altın köstekli saati gasp etmişti. Saati geri almak için yıllarca yalvardı ama Ekrem her defasında onunla alay etti. O gece yağmur yağarken, Fehmi Bey dayanamayıp Ekrem\'in kapısına dayandı. "Saatimi geri ver, o benim ailemin onuru!" diye bağırdı. Ekrem gülerek onu itip kapıyı kapatmak istedi. Yaşlı bedeni öfkeyle dolan Fehmi Bey, var gücüyle Ekrem\'i göğsünden itti. Ekrem dengesini kaybederek geriye doğru sendeledi ve kafasını şöminenin sivri mermer köşesine inanılmaz bir şiddetle çarptı. Çarpmanın etkisiyle kafatası çatlayan Ekrem oracıkta can verdi. Fehmi Bey dehşet içinde titreyerek kurbanın cebinden köstekli saatini aldı ve koşarak oradan uzaklaştı. Eve vardığında saat 02:14\'ü gösteriyordu. Saatini çekmecesine sakladı ve gaz lambasının ışığında sanki hiçbir şey olmamış gibi roman okumaya başladı. Ancak olay yerinde düşürdüğü bir ceket düğmesi ve o gece sokaktan duyulan sesler, bu trajik kazanın ardındaki faili gizleyemezdi.'
            },
            hotspots: [
                {
                    id: 1071,
                    name: 'Cep Saati',
                    desc: 'Zincirli, eski tip bir cep saati.',
                    img: 'images/towns/golge_sehir/deliller/1071.jpg',
                    top: '65%',
                    left: '58%',
                    fingerprintSpot: { xRatio: 0.5, yRatio: 0.1, angle: 0 }, bloodSpot: null
                },
                {
                    id: 1072,
                    isHidden: true,
                    name: 'Not Kâğıdı',
                    desc: 'Üzerinde silik bir el yazısıyla notlar düşülmüş kağıt.',
                    img: 'images/towns/golge_sehir/deliller/1072.jpg',
                    top: '75%',
                    left: '80%',
                    fingerprintSpot: { xRatio: 0.8, yRatio: 0.8, angle: 0 }, bloodSpot: { xRatio: 0.2, yRatio: 0.2, angle: 0 }
                },
                {
                    id: 1073,
                    name: 'Gaz Feneri',
                    desc: 'Karanlıkta aydınlatma sağlamak için kullanılan saplı metal fener.',
                    img: 'images/towns/golge_sehir/deliller/1073.jpg',
                    top: '70%',
                    left: '15%',
                    fingerprintSpot: { xRatio: 0.5, yRatio: 0.2, angle: 0 }, bloodSpot: null
                },
                {
                    id: 1074,
                    name: 'Kalın Ciltli Roman',
                    desc: 'Bazı sayfaları kıvrılmış, kalın kapaklı eski bir hikaye kitabı.',
                    img: 'images/towns/golge_sehir/deliller/1074.jpg',
                    top: '38%',
                    left: '35%',
                    fingerprintSpot: { xRatio: 0.8, yRatio: 0.5, angle: 0 }, bloodSpot: null
                }
            ]
        },

        // 8. KUNDURACI (ID 108)
        {
            id: 'ayakkabici',
            npcId: 108,
            title: 'Kunduracı',
            icon: 'fa-solid fa-shoe-prints',
            hoverTag: 'KUNDURACI',
            wideImg: 'images/towns/golge_sehir/binalar/ayakkabici_interior.png',
            interiorImg: 'images/towns/golge_sehir/binalar/ayakkabici_interior.png',
            style: { top: '56%', left: '74%', width: '14%', height: '16%' },
            npc: {
                id: 108,
                name: 'Kunduracı Rasim',
                building: 'Kunduracı',
                role: 'Kunduracı',
                portrait: 'images/towns/golge_sehir/npcler/npc_108_talk.jpg',
                bg: 'images/towns/golge_sehir/binalar/ayakkabici_interior.png',
                talkBg: 'images/towns/golge_sehir/npcler/npc_108_talk.jpg',
                greeting: 'Ayakkabı çamurundan insanın nereye gittiğini anlarım amirim... Gece gelen çizmeler göl kenarındandı!',
                questions: ['Çamurlu çizmelerin kime ait olduğunu biliyor musun?', 'Mumlu iplik genelde ne için kullanılır?', 'Deri kesme bıçağını en son ne zaman biledin?', 'Göl kenarında işin neydi?'],
                murderStory: 'Gölge Şehir\'in huysuz kunduracısı Rasim, atölyesinde kaçak ve yasadışı deri işleyerek para kazanıyordu. Ekrem Bey bu sırrı öğrenmiş ve "Bana her ay düzenli pay vermezsen jandarmaya kaçak atölyeni basmasını söylerim" diyerek Rasim\'i haraca bağlamıştı. Yılların emeğini bu açgözlü tüccara yedirmek istemeyen Rasim\'in gözü dönmüştü. Cinayet gecesi dükkânından aldığı kalın, dayanıklı mumlu ayakkabı ipini cebine koydu. Kimseye görünmemek için göl kenarındaki çamurlu kestirme yoldan Ekrem Bey\'in evine ulaştı. Açık pencereden sessizce içeri süzüldü. Ekrem koltuğunda uyuklarken, Rasim arkasından yaklaşıp mumlu ipi boynuna doladı ve kollarındaki bütün güçle sıktı. Ekrem çırpınarak nefes almaya çalıştı ama deri yüzmekten nasırlaşmış o güçlü eller ipi bırakmadı. Saniyeler sonra Ekrem cansız yere yığıldı. Rasim kurbanının öldüğünden emin olunca aynı çamurlu yoldan atölyesine geri döndü. Mumlu ipi diğer iplerin arasına fırlattı, çizmelerini de köşeye attı. Kendi kafasında mükemmel bir cinayet işlemişti. Ancak evin halısında bıraktığı 42 numara çamurlu ayak izleri ve kurbanın boynundaki o karakteristik mumlu ipin dokusu, kunduracının sonunu çoktan hazırlamıştı.'
            },
            hotspots: [
                {
                    id: 1081,
                    name: 'Deri Kışlık Çizme',
                    desc: 'Ağır hava şartları için üretilmiş, sağlam ve dayanıklı deri çizme.',
                    img: 'images/towns/golge_sehir/deliller/1081.jpg',
                    top: '75%',
                    left: '25%',
                    fingerprintSpot: { xRatio: 0.5, yRatio: 0.2, angle: 0 }, bloodSpot: { xRatio: 0.5, yRatio: 0.9, angle: 0 }
                },
                {
                    id: 1082,
                    name: 'Mumlu İp Yumakları',
                    desc: 'Ayakkabı dikiminde kullanılan, kalın ve mum kaplı sağlam ip.',
                    img: 'images/towns/golge_sehir/deliller/1082.jpg',
                    top: '65%',
                    left: '60%',
                    fingerprintSpot: { xRatio: 0.8, yRatio: 0.2, angle: 0 }, bloodSpot: null
                },
                {
                    id: 1083,
                    name: 'Eğri Kesici Alet',
                    desc: 'Keskin ve oval uçlu, kısa saplı el aleti.',
                    img: 'images/towns/golge_sehir/deliller/1083.jpg',
                    top: '38%',
                    left: '32%',
                    fingerprintSpot: { xRatio: 0.8, yRatio: 0.8, angle: 0 }, bloodSpot: { xRatio: 0.2, yRatio: 0.2, angle: 0 }
                },
                {
                    id: 1084,
                    isHidden: true,
                    name: 'Ayakkabı Kalıbı',
                    desc: 'Deri ayakkabıların formunu koruması için içine yerleştirilen tahta blok.',
                    img: 'images/towns/golge_sehir/deliller/1084.jpg',
                    top: '80%',
                    left: '78%',
                    fingerprintSpot: { xRatio: 0.8, yRatio: 0.8, angle: 0 }, bloodSpot: null
                }
            ]
        }
    ]
};
