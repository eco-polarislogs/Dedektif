// =============================================================
// 🔍 DEDEKTİFLİK RPG - TAM OYUN MOTORU v2.0
// 5 NPC, Kademesiz Karışık Diyalog, Rastgele Suçlu, Ses Sistemi
// =============================================================

// === DOM ELEMENTS ===
const splashScreen = document.getElementById('splash-screen');
const worldMapScreen = document.getElementById('world-map-screen');
const storyIntroScreen = document.getElementById('story-intro-screen');
const townMapScreen = document.getElementById('town-map-screen');
const interiorScreen = document.getElementById('interior-screen');
const clueInspectModal = document.getElementById('clue-inspect-modal');
const npcTalkModal = document.getElementById('npc-talk-modal');
const bagModal = document.getElementById('bag-modal');
const foundModal = document.getElementById('found-modal');
const npcHistoryModal = document.getElementById('npc-history-modal');
const jailOverlay = document.getElementById('jail-overlay');
const resultModal = document.getElementById('result-modal');
const exitWarningModal = document.getElementById('exit-warning-modal');
const townLockedModal = document.getElementById('town-locked-modal');
const transitionOverlay = document.getElementById('transition-overlay');

// === AUDIO ELEMENTS ===
const bgMusic = document.getElementById('bg-music');
const rainSound = document.getElementById('rain-sound');
const thunderSound = document.getElementById('thunder-sound');
const chatterSound = document.getElementById('chatter-sound');
const doorCreak = document.getElementById('door-creak');
const doorClose = document.getElementById('door-close');
const mumbleMale = document.getElementById('mumble-male');
const mumbleFemale = document.getElementById('mumble-female');
const typewriterSound = document.getElementById('typewriter-sound');
const gavelSound = document.getElementById('gavel-sound');

// === GAME STATE ===
let currentBag = [];
window.currentBag = currentBag;
const MAX_BAG_SIZE = 5;
let activeNpcId = null;
let currentSessionId = 0;
let currentPendingObject = null;
let visitedBuildings = new Set();
window.visitedBuildings = visitedBuildings;
let dialogHistory = {}; // { npcId: [{player, npc}] }
let guiltyNpcId = null;
let npcTalkCompleted = {}; // { npcId: true/false }
let isMuted = false;
let npcQuestionPools = {}; // { npcId: [remaining questions] }
let askedQuestionCount = {}; // { npcId: count }
let npcStressLevels = {}; // { npcId: stressPercent }
var shownCinematicContexts = new Set(); // Bir kez gösterilen bağlamları takip et
var cinematicTypewriterTimeout = null;
var helperMessageHistory = []; // { text: string }
var currentHelperHistoryIndex = -1;
var isHelperTyping = false;
var currentHelperMessageText = '';

// === GAME DATA ===
const NPC_DATA = {
    1: {
        id: 1, name: 'Kasap Hasan', building: 'Kasap', role: 'Kasabadaki eski kasap', img: 'images/hasan.png', bg: 'images/butcher_interior.png', talkBg: 'images/hasan.png', clipPath: 'ellipse(22% 35% at 50% 55%)',
        secret: 'Cinayet gecesi dükkânında gizlice muhtara et sattı.',
        murderStory: 'Yağmurlu bir sonbahar gecesiydi. Kasap Hasan, dükkânını kapattıktan sonra doğruca Osman Bey\'in evine yürüdü. Yıllardır biriken veresiye borcu artık dayanılmaz bir hal almıştı — Osman Bey her seferinde ödemeyi erteliyordu. O gece Hasan son kez parasını istemeye gitti. Kapıyı Osman Bey açtığında, Hasan\'ın gözlerindeki öfkeyi fark edemedi. "Paran yarın gelecek" dedi alaycı bir gülümsemeyle. "Yarın mı? Yıllardır yarın diyorsun!" diye kükredi Hasan. Tartışma kızıştıkça Hasan\'ın eli yanında getirdiği satıra gitti. Bir anlık öfke krizinde, o ağır satırı kurbanın boyun bölgesine indirdi. Derin, tırtıklı yara — ancak bir kasabın elinden çıkabilecek bir darbeydi. Osman Bey son çırpınışlarında Hasan\'ın siyah deri önlüğünden parçalar kopartmaya çalıştı, tırnakları arasında kalan o küçük parçalar, son nefesinde bile savaştığının kanıtıydı. Hasan panikle satırı dükkânına götürüp tezgaha sapladı. Kanlı önlüğünü bir köşeye fırlattı, kara kaplı veresiye defterindeki Osman\'ın adını kırmızı kalemle çizdi. Ama karanlıkta ne kadar temizlerse temizlesin, kanın izi her yere sinmişti.'
    },
    2: {
        id: 2, name: 'Eczacı Selma', building: 'Eczane', role: 'Eczane sahibi', img: 'images/selma.png', bg: 'images/eczane_final.png', talkBg: 'images/eczane_ic_mekan.png', clipPath: 'ellipse(20% 35% at 50% 60%)',
        secret: 'Kurbanın zehirlendiğini biliyordu ama gizledi.',
        murderStory: 'Eczacı Selma, yıllardır kasabada sessiz sedasız çalışan, herkesin güvendiği bir kadındı. Ama bu sessizliğin ardında derin bir nefret gizliydi. Osman Bey, Selma\'nın geçmişine dair bir sır keşfetmiş ve onu aylardır bununla tehdit ediyordu — sessiz kalmasının karşılığında düzenli para talep ediyordu. O gece Selma, planını uygulamaya koydu. Eczanesinin tezgahı altında yetiştirdiği ölümcül bir sarmaşık türünün özünü, son derece dikkatli bir şekilde Osman Bey\'in her gün kullandığı kalp ilacına karıştırdı. Dozajı mükemmel hesaplamıştı — ne çok az, ne çok fazla. İlacı o gün Osman\'ın eline bizzat verdi, gülümseyerek. "Geçmiş olsun Osman Bey, bu ilacı düzenli alın" dedi. Gece yarısı, Osman Bey yatmadan önce ilacını içti. Birkaç dakika içinde kalbinde keskin bir ağrı hissetti. Kalp krizi geçiriyormuş gibi kıvrandı, nefes almaya çalıştı ama zehir çoktan damarlarına yayılmıştı. Selma, o sırada eczanesinin karanlık köşesinde, yağmurun sesini dinleyerek bekledi. Boş ilaç şişesini masanın altına sakladı, parmak izlerini titizlikle sildi. Reçete defterinin son sayfalarını — Osman\'ın gerçek teşhisini ve zehirlenme belirtilerini içeren notları — aceleyle yırtıp attı. Ama her ne kadar profesyonel davranmış olsa da, zehirli sarmaşık tezgahın altında kurumaya bırakılmış halde duruyordu.'
    },
    3: {
        id: 3, name: 'Muhtar Kemal', building: 'Muhtarlık', role: 'Kasabanın muhtarı', img: 'images/kemal.png', bg: 'images/muhtarlik_wide.png', talkBg: 'images/muhtar_final.png', clipPath: 'ellipse(22% 35% at 50% 60%)',
        secret: 'Kurbanla arazi anlaşmazlığı vardı.',
        murderStory: 'Muhtar Kemal, kasabanın en güçlü adamıydı — herkesin sırrını biliyor, her kapıyı açıyordu. Ama yaklaşan belediye seçimleri için büyük bir arazi projesine ihtiyacı vardı ve o arazinin sahibi Osman Bey\'di. Haftalardır Osman\'ı arazisini satması için ikna etmeye çalışmıştı ama Osman direndi. "Bu arazi babamdan kalma, satmam" dedi her seferinde. O gece Kemal, tüm diplomatik maskesini çıkardı. Gece yarısı Osman\'ın evine sızdı — muhtarlık kasasındaki yedek anahtarlarla kapıyı açmak çocuk oyuncağıydı. İçeri girdiğinde Osman masasında oturmuş, belgelerini inceliyordu. "Sen de mi Kemal?" dedi Osman, şaşkınlıkla. Kemal sahte tapu belgelerini masaya fırlattı. "Bunu imzalayacaksın, ya da..." Osman belgeleri yırtmaya başladı. Kemal kontrolünü kaybetti. Masadaki ağır bronz mühürü kaptığı gibi Osman\'ın yüzüne indirdi. Şiddetli bir boğuşma başladı — mobilyalar devrildi, Osman\'ın gözlüğü yere düşüp kırıldı. Kemal, son darbeyi kurbanın şakağına indirdiğinde, Osman\'ın gözleri kararıp yere yığıldı. Ölüm sebebi: ağır darbe sonucu beyin kanaması. Kemal panikle evi terk etti ama aceleyle çıkarken yırtılmış tapu belgelerini ve kırık gözlüğü olduğu yerde bıraktı. Ofisine döndüğünde, titreyerek gizli kasasını açıp sahte belgelerin kopyalarını içine kilitledi.'
    },
    4: {
        id: 4, name: 'Komiser Güneş', building: 'Karakol', role: 'Kadın polis komiseri', img: 'images/gunes.png', bg: 'images/karakol_final.png', talkBg: 'images/karakol_ic_mekan.png', clipPath: 'ellipse(20% 35% at 50% 60%)',
        secret: 'Olay yerindeki delilleri sakladı.',
        murderStory: 'Komiser Güneş, kasabanın adalet sembolüydü — ya da öyle görünüyordu. Gerçekte yıllardır Osman Bey\'den düzenli rüşvet alıyordu. Osman, kasabadaki yasadışı arazi işlemlerini ve kaçak ticaret yollarını biliyordu; Güneş ise bu bilgilerin gün yüzüne çıkmaması için olayları kapatıyor, dosyaları kaybediyordu. Ama Osman artık bu düzenden bıkmıştı ve Güneş\'i ihbar etmekle tehdit etti. "Yarın sabah savcılığa gidiyorum" dedi telefonda, sesi kararlıydı. Güneş o gece üniformasını giydi, polis copunu beline taktı ve Osman\'ın evine gitti. Kapıyı açan Osman, komiserin yüzündeki soğuk ifadeyi gördüğünde anladı ama çok geçti. Güneş ilk darbeyi polis copuyla Osman\'ın karnına indirdi. Osman ikiye katlanırken, Güneş onu yere devirdi. Boğuşma sırasında Osman savunma yaraları aldı — kollarında, ellerinde darbe izleri oluştu. Güneş yakın mesafeden copla art arda vurdu. Son darbe şakağına geldiğinde Osman hareketsiz kaldı. Havasız kalma ve travmatik darbeler — bir polisin eğitimli şiddetiyle uyumlu izler. Güneş, bir polis olarak olay yerini profesyonelce temizlemeye çalıştı — parmak izlerini sildi, kan lekelerini temizledi. Ama boğuşma sırasında paltosunun pirinç düğmesi kopmuş, rozeti yere düşmüştü. Karanlıkta bunları fark edemedi. Karakola döndüğünde, "GİZLİ" damgalı dosyaya Osman\'ın ihbar dilekçesini kilitledi ve anahtarı çekmecesinin derinliklerine gömdü.'
    },
    5: {
        id: 5, name: 'Terzi Yahya', building: 'Terzi', role: 'Kasabanın terzisi', img: 'images/yahya.png', bg: 'images/terzi_final.png', talkBg: 'images/terzi_ic_mekan.png', clipPath: 'ellipse(22% 35% at 50% 60%)',
        secret: 'Kurbana gizli cepli ceket dikti, son gören kişi.',
        murderStory: 'Terzi Yahya, kasabanın en yaşlı ve en saygın ustasıydı. Ama bu saygın cephenin arkasında karanlık bir ortaklık vardı — Yahya, yıllardır Osman Bey\'in gizli işlerinin sessiz ortağıydı. Para aklama, belge saklama, hatta kaçak mal transferi... Osman\'ın son diktirdiği ceketin astarına gizli bir cep dikmişti ve bu cepte, tüm yasadışı işlemlerin kaydını içeren bir USB bellek saklanıyordu. Ama Osman, ortaklığı bitirmeye ve Yahya\'yı saf dışı bırakmaya karar vermişti. O gece Yahya, payını almak için Osman\'ın evine gitti. "Param nerede Osman?" diye sordu titreyen bir sesle. Osman güldü. "Senin paran mı? Bu işte sen artık yoksun yaşlı adam. O USB\'yi de sana vermeyeceğim." Yılların birikimi bir anda patladı. Yahya, meslek hayatının en sadık aleti olan iplik makarasını cebinden çıkardı. Kalın, dayanıklı, kopması imkansız terzi ipliğini Osman\'ın boynuna doladı ve tüm gücüyle sıktı. Osman çırpındı, direndi — bu sırada Yahya\'nın diktiği ceketinden kumaş parçaları yırtıldı. Ama Yahya bırakmadı. İplik boyun bölgesinde derin izler bırakarak, Osman\'ın son nefesini de aldı. Yahya titreyerek ayağa kalktı. Kanlı iplik makarasını cebine koydu, yırtılan kumaş parçalarını toplamaya çalıştı ama hepsini bulamadı. Dükkânına döndüğünde, o gece diktiği son ceketin gizli cebindeki not hâlâ duruyordu: "Bu gece gel, konuşalım."'
    }
};

const SCENE_OBJECTS = {
    1: [ // Kasap Hasan
        {
            id: 1, name: 'Satır', desc: 'Tezgaha sertçe saplanmış paslı bir satır.', top: '75%', left: '45%', img: 'images/bloody_cleaver.png',
            fingerprintSpot: { xRatio: 0.22, yRatio: 0.82, angle: 0 }, // Sap kısmında
            bloodSpot: { xRatio: 0.78, yRatio: 0.22, angle: 0 }        // Keskin ağız ucunda
        },
        {
            id: 2, name: 'Kara Kaplı Defter', desc: 'Veresiye listesinde kurbanın isminin üzeri kırmızı kalemle çizilmiş.', top: '65%', left: '30%', img: 'images/black_notebook.png',
            fingerprintSpot: { xRatio: 0.18, yRatio: 0.22, angle: 0 }, // Sol üst köşe
            bloodSpot: null
        },
        {
            id: 3, name: 'Kasap Önlüğü', desc: 'Askının arkasında asılı duran kasap önlüğü.', top: '40%', left: '10%', img: 'images/torn_apron.png', isHidden: true,
            fingerprintSpot: null,
            bloodSpot: { xRatio: 0.72, yRatio: 0.68, angle: 0 }        // Alt sağ cep bölgesi
        }
    ],
    2: [ // Eczane (Selma)
        {
            id: 4, name: 'İlaç Şişesi', desc: 'İlaç raflarının arkasına konulmuş boş cam şişe.', top: '55%', left: '75%', img: 'images/empty_medicine_bottle.png', isHidden: true,
            fingerprintSpot: { xRatio: 0.68, yRatio: 0.38, angle: 0 }, // Camın sağ omuz eğrisi
            bloodSpot: null
        },
        {
            id: 5, name: 'Reçete Defteri', desc: 'Sayfalarında eksikler bulunan reçete kayıt defteri.', top: '75%', left: '40%', img: 'images/prescription_notebook.png',
            fingerprintSpot: { xRatio: 0.75, yRatio: 0.78, angle: 0 }, // Sayfa alt çevirme köşesi
            bloodSpot: null
        },
        {
            id: 6, name: 'Bitki Yaprağı', desc: 'Tezgah altında kurumaya bırakılmış yabani bir bitki türü.', top: '15%', left: '45%', img: 'images/poison_ivy.png',
            fingerprintSpot: null, bloodSpot: null
        }
    ],
    3: [ // Muhtarlık (Kemal)
        {
            id: 7, name: 'Tehdit Mektubu', desc: 'Muhtarın masa üstünde kurbana yazılmış, henüz gönderilmemiş mektup.', top: '56%', left: '42%', img: 'images/threat_letter.png',
            fingerprintSpot: { xRatio: 0.82, yRatio: 0.18, angle: 0 }, // Üst sağ mühür kenarı
            bloodSpot: null
        },
        {
            id: 8, name: 'Okuma Gözlüğü', desc: 'Masa üzerinde bulunmuş okuma gözlüğü.', top: '58%', left: '54%', img: 'images/broken_glasses.png',
            fingerprintSpot: { xRatio: 0.22, yRatio: 0.45, angle: 0 }, // Sol gözlük sapı
            bloodSpot: { xRatio: 0.76, yRatio: 0.35, angle: 0 }        // Kırık sağ cam çatlağı
        },
        {
            id: 9, name: 'Demir Kasa', desc: 'Arka köşede duran demir kasa.', top: '44%', left: '72%', img: 'images/hidden_safe.png', isHidden: true,
            fingerprintSpot: { xRatio: 0.35, yRatio: 0.62, angle: 0 }, // Şifre çarkının altı
            bloodSpot: null
        }
    ],
    4: [ // Karakol (Komiser Güneş)
        {
            id: 10, name: 'Polis Rozeti', desc: 'Olay yerinde bulunan, numarası kazınmış bir polis rozeti.', top: '65%', left: '40%', img: 'images/police_badge.png',
            fingerprintSpot: { xRatio: 0.65, yRatio: 0.28, angle: 0 }, // Yıldızın sağ ucu
            bloodSpot: null
        },
        {
            id: 11, name: 'Evrak Dosyası', desc: 'Evrak dolabında bulunan resmi bir dosya.', top: '45%', left: '80%', img: 'images/evidence_file.png', isHidden: true,
            fingerprintSpot: { xRatio: 0.82, yRatio: 0.20, angle: 0 }, // Dosya kapağı tutamağı
            bloodSpot: null
        },
        {
            id: 12, name: 'Kayıp Düğme', desc: 'Pahalı bir paltonun kopmuş düğmesi.', top: '85%', left: '60%', img: 'images/missing_button.png',
            fingerprintSpot: { xRatio: 0.28, yRatio: 0.32, angle: 0 }, // Kenar çentik
            bloodSpot: { xRatio: 0.72, yRatio: 0.68, angle: 0 }        // İplik deliği kenarı
        }
    ],
    5: [ // Terzi (Yahya)
        {
            id: 13, name: 'İplik Makarası', desc: 'Tezgahta bulunan sıradan bir iplik makarası.', top: '75%', left: '50%', img: 'images/thread_spool.png',
            fingerprintSpot: { xRatio: 0.25, yRatio: 0.28, angle: 0 }, // Ahşap makara başlığı
            bloodSpot: { xRatio: 0.75, yRatio: 0.65, angle: 0 }        // İpliğin alt katmanı
        },
        {
            id: 14, name: 'Kumaş Parçası', desc: 'Yere düşmüş ufak bir kumaş parçası.', top: '80%', left: '40%', img: 'images/torn_fabric.png',
            fingerprintSpot: null,
            bloodSpot: { xRatio: 0.78, yRatio: 0.28, angle: 0 }        // Yırtık uç köşesi
        },
        {
            id: 15, name: 'Ceket Astarı', desc: 'Mankendeki ceketin iç kısmındaki astar dikişi.', top: '45%', left: '35%', img: 'images/hidden_pocket.png', isHidden: true,
            fingerprintSpot: { xRatio: 0.65, yRatio: 0.28, angle: 0 }, // Gizli cep kapağı
            bloodSpot: null
        }
    ]
};

window.NPC_DATA = NPC_DATA;
window.SCENE_OBJECTS = SCENE_OBJECTS;


// =============================================================
// KADEMESIZ DIYALOG HAVUZU — 20 SORU HER NPC İÇİN (KARIŞIK)
// Her soru difficulty (1-5) ve category ile etiketli
// Suçlu NPC'ye göre dinamik cevaplar: guiltyResponse
// =============================================================

const NPC_ALL_QUESTIONS = {
    1: [ // Kasap Hasan — TÜM 20 SORU TEK HAVUZDA
        // Eski Kademe 1 (Tanışma - kolay)
        { q: 'Cinayet gecesi neredeydin Hasan?', a: 'Buradaydım, dükkânımda. Gece geç saate kadar et doğruyordum. Kimsecikler yoktu ortalıkta, yağmur bardaktan boşalırcasına yağıyordu.', difficulty: 1, category: 'tanisma', relatedClues: [] },
        { q: 'Kurbanı ne kadar iyi tanıyordun?', a: 'Osman Bey mi? Herkes tanır onu. İyi müşterimdi, her hafta gelirdi. Ama son zamanlarda arası bazılarıyla açılmıştı...', difficulty: 1, category: 'tanisma', relatedClues: [] },
        { q: 'Kasabada düşmanı olan var mıydı?', a: 'Düşman mı? Ha, bir sürü... Muhtar Kemal\'le arazi meselesinden dolayı birbirlerine giriyorlardı. Eczacı Selma da ondan pek hazzetmezdi.', difficulty: 1, category: 'tanisma', relatedClues: [7, 8] },
        { q: 'Dükkânında şüpheli bir şey gördün mü?', a: 'Şüpheli mi? Ben sadece kasabım dedektif bey. Ama... o gece garip sesler duydum sokaktan.', difficulty: 1, category: 'tanisma', relatedClues: [] },
        // Eski Kademe 2 (Derinleşme)
        { q: 'O gece duyduğun garip sesler neydi?', a: 'Bağrışma gibiydi... Ama yağmurdan net duyamadım. Saat gece yarısı civarıydı. Sonra bir araba kapısı çarpma sesi... Sonra sessizlik.', difficulty: 2, category: 'derinlesme', relatedClues: [] },
        { q: 'Dükkânına gelen şüpheli biri oldu mu?', a: 'Cinayet gecesi muhtar Kemal geldi aslında. Gece vakti et istedi. Aceleyle aldı gitti. Garip buldum ama sormadım.', difficulty: 2, category: 'derinlesme', relatedClues: [7] },
        { q: 'Kurbanla son ne zaman konuştun?', a: 'Cinayet gününden bir gün önce geldi. "Yarın büyük bir para gelecek" dedi. Bir daha göremedim...', difficulty: 2, category: 'derinlesme', relatedClues: [] },
        { q: 'Seni şüpheli görüyorlar, biliyor musun?', a: 'Ha! Beni mi? Ben niye öldüreyim müşterimi? Borcunu ödeyecekti, öldürsem para gider! Aklını kullan dedektif...', difficulty: 2, category: 'derinlesme', relatedClues: [1, 2] },
        // Eski Kademe 3 (Yüzleştirme - orta)
        { q: 'Bu kanlı satır senin tezgahından çıktı!', a: 'O... o satır çalınmıştı! Bir hafta önce kayboldu, polise söyledim ama kimse ciddiye almadı! Birisi beni suçlu göstermek istiyor!', difficulty: 3, category: 'yuzlestirme', relatedClues: [1], guiltyResponse: { 1: '*Yüzü bembeyaz olur* O... o satır... Evet, benim ama çalınmıştı! İnanın bana, birisi... birisi çerçeveliyor!', 2: 'Eczacıya sorun! O kadın her şeyi biliyor, zehirler, ilaçlar... Satırı çalan da o olabilir!', 3: 'Muhtar\'ın adamları çalmış olmalı! O gece dükkâna gelen Kemal\'di, belki de satırı o aldı!', 4: 'Komiser Güneş bunu neden araştırmadı? Kayıp bildirimi yaptım ama dosya kayboldu!', 5: 'Terzi Yahya bir bıçak kılıfı diktirdi benden... Acaba o kılıf bu satır için miydi?' } },
        { q: 'Kara defterdeki kurbanın ismi neden çizili?', a: 'Veresiye borcunu ödeyeceğini söyledi diye çizdim! O kadar! Herkes veresiye defteri tutar!', difficulty: 3, category: 'yuzlestirme', relatedClues: [2] },
        { q: 'Yırtık önlüğündeki anahtar neyin anahtarı?', a: '*Terler* O... o anahtar arka odanın anahtarı. Soğuk hava deposu. İçinde sadece etler var...', difficulty: 3, category: 'yuzlestirme', relatedClues: [3] },
        { q: 'Muhtar cinayet gecesi sana geldiğini inkâr ediyor.', a: 'Yalancı! O gece buraya geldi, gözleri dönmüştü! Eğer inkâr ediyorsa gizleyecek bir şeyi var demektir!', difficulty: 3, category: 'yuzlestirme', relatedClues: [7] },
        // Eski Kademe 4 (Baskı - zor)
        { q: 'Soğuk hava deposunda sadece et mi var?', a: '... İyi tamam. Orada eski belgeler de var. Kurbanın bazı evrakları... O bana emanet bırakmıştı.', difficulty: 4, category: 'baski', relatedClues: [3, 9], guiltyResponse: { 1: '*Çok terler* Tamam... orada... sadece belgeler değil... Kurbanla son tartışmamızda... İşler kontrolden çıktı. Ama yemin ederim kaza oldu!', 2: 'O belgeler Selma\'nın zehir siparişlerini gösteriyor! Korktum, sakladım!', 3: 'Muhtar\'ın sahte tapuları orada! Kurban bana emanet bıraktı!', 4: 'Komiser\'in kapatmaya çalıştığı dosyanın kopyası orada... Delil karartıyor!', 5: 'Yahya\'nın diktiği ceketteki USB\'nin kopyası orada...' } },
        { q: 'Kurbanın sana emanet bıraktığı şey neydi?', a: 'Bir zarf... İçinde arazi tapuları vardı. Muhtarın üzerine kayıtlı arazilerin aslında kurbana ait olduğunu gösteren belgeler.', difficulty: 4, category: 'baski', relatedClues: [9, 7] },
        { q: 'Neden bu belgeleri polise vermedin?', a: 'Korktum! Muhtar bu kasabada herkesin efendisi! Komiser Güneş zaten muhtarın adamı, kime güveneyim?', difficulty: 4, category: 'baski', relatedClues: [10, 11] },
        { q: 'Terziden kurbanın ceketini aldığını biliyoruz.', a: 'Hayır! Ben terziye hiç gitmedim! ... Tamam, Yahya\'dan bir bıçak kılıfı diktirdim ama kurbanla alakası yok!', difficulty: 4, category: 'baski', relatedClues: [13, 14] },
        // Eski Kademe 5 (Son - çok zor)
        { q: 'Son sözün nedir Hasan?', a: 'Ben masum bir kasabım! Evet, korkağım, belgeleri sakladım, ama kimseyi öldürmedim! Gerçek katili bulun!', difficulty: 5, category: 'son', relatedClues: [], guiltyResponse: { 1: '*Gözyaşları* Ben... ben sadece kızdım. O gece Osman borcunu ödemeyeceğini söyledi, tartıştık... Satır elimdeydi... Bir anlık öfke... Ama ben katil değilim, bu bir kazaydı!', 2: 'Selma\'nın elindeki zehir şişesini sorun! Ben değilim!', 3: 'Muhtar\'ın tapuları sahte! O öldürdü!', 4: 'Komiser delilleri saklıyor, gerçek katil o!', 5: 'Yahya\'daki USB\'yi açın, her şey orada!' } },
        { q: 'Katil kim sence?', a: 'Muhtar Kemal! Arazi meselesi yüzünden... Ama komiser de bu işin içinde olabilir. O gece karanlıkta bir kadın silueti gördüm...', difficulty: 5, category: 'son', relatedClues: [7, 10] },
        { q: 'Söylemediklerin var mı hâlâ?', a: '*Uzun sessizlik* Eczacı Selma... O gece dükkânını geç kapattı. Pencereden ışık gördüm. Elinde bir şişe vardı...', difficulty: 5, category: 'son', relatedClues: [4, 6] },
        { q: 'Masum olduğunu kanıtlayamazsan...', a: 'Emanet zarfı açın! İçindeki belgeler her şeyi anlatır! Ben sadece bir kasabım, korkak bir kasap...', difficulty: 5, category: 'son', relatedClues: [9] }
    ],
    2: [ // Eczacı Selma — TÜM 20 SORU
        { q: 'Cinayet gecesi eczaneniz açık mıydı?', a: 'Gece yarısına kadar açıktı. Envanter sayımı yapıyordum... Dışarıda yağmur yağıyordu, içeri müşteri gelmedi.', difficulty: 1, category: 'tanisma', relatedClues: [] },
        { q: 'Kurbanla ilişkiniz nasıldı?', a: 'Sadece müşterimdi. Düzenli ilaç alırdı, kronik bir rahatsızlığı vardı. Son zamanlarda daha sık geliyordu...', difficulty: 1, category: 'tanisma', relatedClues: [4] },
        { q: 'Kasabada zehirlenme vakaları olduğunu duydunuz mu?', a: 'Ne zehirlenmesi? Ben eczacıyım, ilaç satarım! Zehir değil! Böyle iftiralar atılmasına tahammülüm yok!', difficulty: 1, category: 'tanisma', relatedClues: [6] },
        { q: 'Kurbanın sağlık durumu hakkında bilginiz var mı?', a: 'Hasta bir adamdı. Kalp ilacı kullanıyordu. Ama son haftalarda reçetesiz bir ilaç daha istemeye başladı...', difficulty: 1, category: 'tanisma', relatedClues: [4, 5] },
        { q: 'Kurban reçetesiz hangi ilacı istedi?', a: 'Güçlü bir uyku ilacı. Uykusuzluk çektiğini söyledi ama... O dozda kalp hastası için çok tehlikeli olurdu.', difficulty: 2, category: 'derinlesme', relatedClues: [4] },
        { q: 'Gece yarısına kadar neden açıktınız?', a: '... Birini bekliyordum tamam mı? Muhtar Kemal aradı, "Acil ilaç lazım, geç geleceğim" dedi. Ama gelmedi.', difficulty: 2, category: 'derinlesme', relatedClues: [7], guiltyResponse: { 1: 'Kasap Hasan o gece geldi, çok gergindi. Ellerinde kan lekesi vardı, "Kesildim" dedi ama inanmadım...', 2: '*Titrer* Tamam... Muhtar\'ı beklemiyordum. Ben... kurbanı bekliyordum. Son ilaç dozunu verecektim ama... dozajı yanlış hesaplamış olabilirim.', 3: 'Muhtar aradı evet, ama sesinde panik vardı. "Selma, beni örtbas et" dedi. Ne demek istediğini anlayamadım...', 4: 'Komiser o gece geldi dükkâna. Defterimi istedi, "Bunu bana ver" dedi. Sayfaları o yırttı!', 5: 'Yahya\'nın o gece paketini gördüm... Kurbanın evine gidiyordu. İçinde ne vardı bilmiyorum ama ağırdı.' } },
        { q: 'Komiser Güneş sizi cinayet gecesi gördüğünü söylüyor.', a: 'Nerede görmüş? Ben dükkânımdan çıkmadım! Eğer öyle diyorsa yalan söylüyor...', difficulty: 2, category: 'derinlesme', relatedClues: [10, 11] },
        { q: 'Kurbanın ölüm sebebi zehirlenme olabilir mi?', a: '*Yüzü solar* Zehirlenme mi? Bu... bu çok kötü. Ben hiçbir şey satmadım, yemin ederim!', difficulty: 2, category: 'derinlesme', relatedClues: [4, 6] },
        { q: 'Bu boş ilaç şişesindeki zehri kime sattın?', a: 'O... o ilacı ben kimseye satmadım! Şişe çalınmış olmalı! Belki biri gece dükkâna girdi ve aldı...', difficulty: 3, category: 'yuzlestirme', relatedClues: [4], guiltyResponse: { 1: 'Kasap\'a sorun! O gece dükkânıma girip çıkan birini gördüm, iri yarı biriydi!', 2: '*Yıkılır* O şişe... benim. Kurbanın ilacına karıştırdım ama öldürmek için değil! Acısını dindirmek için! O çok acı çekiyordu, yalvardı bana!', 3: 'Muhtar istedi o ilacı! "Birisi için lazım" dedi. Kim için olduğunu sormadım...', 4: 'O şişeyi komiser aldı! Benden zorla aldı, "Delil" dedi ama rapora yazmadı!', 5: 'Yahya\'nın gizli cepte sakladığı not... O notta bu ilaçtan bahsediliyor olabilir.' } },
        { q: 'Reçete defterinin son sayfasını neden yırttın?', a: '*Titreyerek* Orada önemli bir not vardı. Kurbanın gerçek teşhisi... Mesleki sorumluluğum...', difficulty: 3, category: 'yuzlestirme', relatedClues: [5] },
        { q: 'Tezgah altındaki zehirli sarmaşık ne için?', a: 'Tıbbi araştırma! Geleneksel tıpta kullanılır! Ben onu ilaç yapmak için yetiştiriyorum, zehir olarak değil!', difficulty: 3, category: 'yuzlestirme', relatedClues: [6] },
        { q: 'Kurbanın gerçek teşhisi neydi?', a: '*Uzun sessizlik* Osman Bey zehirleniyordu... Yavaş yavaş. Ama ben yapmadım! Birisi ona düzenli olarak küçük dozlarda zehir veriyordu.', difficulty: 3, category: 'yuzlestirme', relatedClues: [4, 5, 6] },
        { q: 'Neden polise söylemedin zehirlenme şüpheni?', a: 'Komiser Güneş\'e söyledim! Ama ciddiye almadı. Defteri gösterdim. Sonra... bazı sayfaları kayboldu.', difficulty: 4, category: 'baski', relatedClues: [5, 10, 11] },
        { q: 'Komiser defterin sayfalarını mı aldı?', a: 'Bilmiyorum! Ama o gün komiserin gelişinden sonra sayfalar yoktu. Belki tesadüftür... belki değildir.', difficulty: 4, category: 'baski', relatedClues: [11] },
        { q: 'Muhtar neden seni arayıp ilaç istedi o gece?', a: 'Stres ilacı istedi. "Çok gerginim, uyuyamıyorum" dedi. Ama sesinde korku vardı... Normal değildi.', difficulty: 4, category: 'baski', relatedClues: [7] },
        { q: 'Terzi Yahya\'yla ilişkin nedir?', a: 'Yahya mı? Komşuyuz, bazen çay içeriz. Ama... Yahya kurbanın son günlerinde onu çok sık ziyaret etti.', difficulty: 4, category: 'baski', relatedClues: [13, 14, 15] },
        { q: 'Son sözün nedir Selma?', a: 'Ben eczacıyım, insanları iyileştirmek için çalışıyorum! Evet, şüphemi sakladım ama korktum!', difficulty: 5, category: 'son', relatedClues: [] },
        { q: 'Katil kim sence?', a: 'Muhtar ve komiser arasında bir bağ var. Cinayet gecesi muhtar et almaya gitti, komiser olay yerini geç inceledi...', difficulty: 5, category: 'son', relatedClues: [7, 10] },
        { q: 'Sakladığın başka bir şey var mı?', a: 'Cinayet gecesi... Pencereden Terzi Yahya\'yı gördüm. Elinde bir paket vardı ve kurbanın evine doğru gidiyordu.', difficulty: 5, category: 'son', relatedClues: [13, 15] },
        { q: 'Bu zehirli bitkiyi kurban için mi kullandın?', a: 'HAYIR! O bitki deneysel ilaç çalışmam için! Ben birini zehirlemek istesem... şey, yani teorik olarak...', difficulty: 5, category: 'son', relatedClues: [6] }
    ],
    3: [ // Muhtar Kemal — TÜM 20 SORU
        { q: 'Muhtar bey, cinayet gecesi neredeydiniz?', a: 'Evimdeydim tabii ki. Televizyon izledim, sonra uyudum. Muhtarın gece sokakta ne işi olur?', difficulty: 1, category: 'tanisma', relatedClues: [] },
        { q: 'Kurbanla aranızdaki ilişki nasıldı?', a: 'Normal komşuluk. Bazen anlaşamadığımız konular oldu ama siyasette düşman olmak cinayet sebebi değildir.', difficulty: 1, category: 'tanisma', relatedClues: [7] },
        { q: 'Kasabada gerginliğin sebebi nedir?', a: 'Arazi meseleleri... Belediye yeni yol geçirecek, bazı araziler kamulaştırılacak. Herkes pay kapmaya çalışıyor.', difficulty: 1, category: 'tanisma', relatedClues: [9] },
        { q: 'Kurbanın ölümü sana yaradı diyorlar.', a: 'Kim diyor? Kimin diline düşmüşüm? Osman\'ın ölümü bana hiçbir şey kazandırmadı!', difficulty: 1, category: 'tanisma', relatedClues: [7, 9] },
        { q: 'Arazi meselesi hakkında detay ver.', a: 'Kurbanın arazisi yolun tam üzerinde. Kamulaştırma bedeli çok yüksek olacaktı. Osman satmak istemiyordu...', difficulty: 2, category: 'derinlesme', relatedClues: [9] },
        { q: 'Cinayet gecesi kasaba gidip et aldığın doğru mu?', a: '*Duraksır* Kim söyledi? Kasap Hasan mı? Sadece kısa bir yürüyüşe çıktım. Kasabın önünden geçtim ama et almadım!', difficulty: 2, category: 'derinlesme', relatedClues: [1, 2], guiltyResponse: { 1: 'Hasan yalancı! O gece kasabın önünden bile geçmedim! ... Tamam, geçtim ama et almak için değil.', 2: 'Selma\'yı aradığımı itiraf ediyorum. Stres ilacı lazımdı. Ama gitmediğimi yemin ederim!', 3: '*Sinirlenir* Tamam! Et aldım! Ama cinayet saatinden ÇOK önce! Saat 10\'da gidip geldim. Sonra eve döndüm. Sonra... yürüyüşe çıktım tekrar. Kurbanın evinin önünden geçtim. Ama ona dokunmadım! ...En azından öyle hatırlıyorum.', 4: 'Komiser beni uyardı, "Evinde kal" dedi. Ama dinlemedim, merak ettim ne oluyor diye...', 5: 'Yahya\'yı o gece sokakta gördüm! O da dışarıdaydı!' } },
        { q: 'Eczacı Selma seni aradığını söylüyor o gece.', a: 'Hayır! Ben kimseyi aramadım! Selma yanlış hatırlıyor... Ya da bilerek yalan söylüyor.', difficulty: 2, category: 'derinlesme', relatedClues: [4, 5] },
        { q: 'Kurbanla son görüşmeniz ne zamandı?', a: 'Cinayet gününden iki gün önce. Bağırdı çağırdı. "Arazimi vermeyeceğim, mahkemeye giderim!" dedi.', difficulty: 2, category: 'derinlesme', relatedClues: [7] },
        { q: 'Bu tehdit mektubunu sen yazdın, çekmecende bulduk!', a: '*Yüzü kızarır* Bunu sinirle yazdım! Göndermeyecektim! Herkes kızgınken bir şeyler yazar!', difficulty: 3, category: 'yuzlestirme', relatedClues: [7] },
        { q: 'Kurbanın kırık gözlüğü senin odanda ne işi var?', a: 'Kavga ettiğimizde düştü! Kırdım evet, ama sonra pişman oldum. Geri verecektim...', difficulty: 3, category: 'yuzlestirme', relatedClues: [8] },
        { q: 'Kasadaki sahte belgeler neyin nesi?', a: '*Terler* Onlar... eski belediye evrakları. Bazen prosedürler hızlansın diye bazı belgeler... düzenler.', difficulty: 3, category: 'yuzlestirme', relatedClues: [9] },
        { q: 'Kasap Hasan cinayet gecesi geldiğini kanıtlayabilir.', a: 'Tamam! Evet, kasaba gittim! Et aldım! Ama bu beni katil yapmaz!', difficulty: 3, category: 'yuzlestirme', relatedClues: [1, 2] },
        { q: 'Gece yarısı et almak için mi çıktın gerçekten?', a: '... Tamam, et bahaneydi. Kurbanın evinin önünden geçmek istedim. Tehdit mektubu yazdığım için vicdan azabı çekiyordum.', difficulty: 4, category: 'baski', relatedClues: [7], guiltyResponse: { 1: 'Kasap\'ın söylediklerine inanmayın! O adam yalancının teki!', 2: 'Selma beni çerçevelemeye çalışıyor! O zehirci kadın!', 3: '*Masayı yumruklar* TAMAM! Kurbanın evine gittim! Konuşmak istedim, barışmak... Ama o beni kovdu, aşağıladı. "Sahte tapularını biliyorum" dedi. Tartıştık... ve ben... kontrol... *susar*', 4: 'Komiser Güneş her şeyi biliyor! O gece beni aradı, "Muhtar sakin ol" dedi. Neden böyle dedi?', 5: 'Yahya\'nın elinde bir paket vardı o gece, onu araştırın!' } },
        { q: 'Kurbanın evinde ne gördün?', a: 'Işıklar yanıyordu. Bir gölge gördüm pencerede... Kurban değildi. Başka birisi vardı orada.', difficulty: 4, category: 'baski', relatedClues: [] },
        { q: 'Komiser Güneş\'le ilişkin nedir?', a: 'Resmi ilişkimiz var... *duraksır* Bazen bazı dosyaların kapanması konusunda ortak çalışırız. Hepsi bu.', difficulty: 4, category: 'baski', relatedClues: [10, 11] },
        { q: 'Arazi tapuları aslında kurbanın üzerineymiş.', a: '*Şok olur* Ne?! Tapular... Kim söyledi bunu?! O araziler yasal olarak belediyeye aittir!', difficulty: 4, category: 'baski', relatedClues: [9] },
        { q: 'Son sözün nedir Kemal?', a: 'Ben bu kasabanın muhtarıyım! 20 yıldır hizmet ediyorum. Evet hatalarım oldu ama kimseyi öldürmedim!', difficulty: 5, category: 'son', relatedClues: [] },
        { q: 'Katil kim sence?', a: 'Kasap Hasan! O satırla... Ama belki eczacı. O kadının ne zehirler bildiğini düşünün! Ya da terzi...', difficulty: 5, category: 'son', relatedClues: [1, 4, 13] },
        { q: 'Söylemediklerin var mı?', a: '*İç çeker* Komiser Güneş... O gece beni aradı. "Muhtar, evinde kal" dedi. Neden böyle dediğini hiç sormadım...', difficulty: 5, category: 'son', relatedClues: [10, 11] },
        { q: 'Kurbanın evindeki gölge kim olabilir?', a: 'Uzun boylu biriydi... Terzi Yahya\'nın boyu uzundur... O gece herkes bir yerlere gidiyordu bu kasabada.', difficulty: 5, category: 'son', relatedClues: [13, 14, 15] }
    ],
    4: [ // Komiser Güneş — TÜM 20 SORU
        { q: 'Komiser, olay yerine ilk gelen siz miydiniz?', a: 'Evet, saat 02:30 civarında ihbar aldık. 10 dakika içinde oradaydım. Ceset meydanda yatıyordu, yağmur izleri siliyordu.', difficulty: 1, category: 'tanisma', relatedClues: [] },
        { q: 'Kurban hakkında ne biliyorsunuz?', a: 'Osman Bey, 58 yaşında, tüccar. Kasabada tanınan bir isim. Arazi anlaşmazlıkları dışında bilinen düşmanı yoktu...', difficulty: 1, category: 'tanisma', relatedClues: [] },
        { q: 'Cinayet gecesi siz neredeydiniz?', a: 'Karakolda nöbetteydim. Evrak işleriyle uğraşıyordum. Yağmurlu gecelerde genelde sakin olur kasaba...', difficulty: 1, category: 'tanisma', relatedClues: [10] },
        { q: 'İlk bulgular neler?', a: 'Kafa travması. Sert bir cisimle vurulmuş. Ölüm saati 23:00-01:00 arası. Olay yerinde az sayıda fiziksel delil vardı.', difficulty: 1, category: 'tanisma', relatedClues: [1, 12] },
        { q: 'Olay yerinde hangi delilleri buldunuz?', a: 'Bir düğme, bazı ayak izleri... Yağmur çoğunu sildi. Standart prosedür uyguladık.', difficulty: 2, category: 'derinlesme', relatedClues: [12] },
        { q: 'Neden dış dedektif çağrıldı?', a: '*Rahatsız olur* Üst makamın kararı. "Tarafsız göz lazım" dediler. Kasabada herkes birbirini tanıyor.', difficulty: 2, category: 'derinlesme', relatedClues: [] },
        { q: 'Muhtar Kemal\'le ilişkiniz profesyonel mi?', a: 'Tabii ki profesyonel! Muhtar resmi makam, ben de polis. Başka bir ilişki yok!', difficulty: 2, category: 'derinlesme', relatedClues: [7, 9], guiltyResponse: { 1: 'Kasap Hasan\'ın dükkânında şüpheli evraklar bulundu, onu soruşturuyorum!', 2: 'Eczacı Selma\'nın zehir stoku var, bunu raporladım ama üst makam ilgilenmedi.', 3: 'Muhtar Kemal... *duraksır* Evet, bazen dosyaları birlikte kapattık. Ama bu normal prosedür!', 4: '*Sertleşir* Ben 15 yıllık polisim! Muhtar\'la profesyonel ilişkimiz var, ama cinayet gecesi... tamam, onu aradım. Uyardım. Çünkü bilgi aldım, ihbar vardı. Ama bu bilgiyi... resmi kanaldan almadım. Bir kaynak... söyleyemem.', 5: 'Terzinin dosyasında adı geçiyor ama bu gizli bir soruşturma!' } },
        { q: 'Eczacı Selma zehirlenme şüphesini bildirmiş miydi?', a: '*Duraksır* Selma mı söyledi? Bir keresinde "kan değerleri garip" gibi bir şey demişti ama somut kanıt yoktu.', difficulty: 2, category: 'derinlesme', relatedClues: [4, 5] },
        { q: 'Bu polis rozeti olay yerinde bulundu!', a: '*Yüzü değişir* Kayıp olarak rapor edilmişti. Bir ay önce karakoldan çalındı. Kim aldıysa olay yerine bırakmış.', difficulty: 3, category: 'yuzlestirme', relatedClues: [10] },
        { q: 'Gizli dosyadaki bilgiler neden rapor edilmedi?', a: 'O dosya devam eden bir soruşturmanın parçası! Her şeyi kamuoyuyla paylaşamam. Prosedür gereği gizli!', difficulty: 3, category: 'yuzlestirme', relatedClues: [11] },
        { q: 'Bu düğme terzi Yahya\'nın diktiği paltoya ait.', a: 'İlginç... Yahya\'dan düğmenin kime ait olduğunu sorduk ama net cevap vermedi.', difficulty: 3, category: 'yuzlestirme', relatedClues: [12, 14] },
        { q: 'Eczacının defterinden sayfaları siz mi aldınız?', a: 'NE?! Ben kimsenin defterinden sayfa almadım! Bu çok ciddi bir itham! Kanıtınız var mı?', difficulty: 3, category: 'yuzlestirme', relatedClues: [5] },
        { q: 'O gece muhtarı neden arayıp evinde kalmasını söylediniz?', a: '*Şaşırır* Muhtar bunu mu söyledi? Ben onu güvenlik için uyardım! İhbar gelmişti!', difficulty: 4, category: 'baski', relatedClues: [7] },
        { q: 'İhbar gelmeden ÖNCE muhtarı aradığınız kanıtlandı.', a: '*Uzun sessizlik* ... Birisi beni aradı. Tanımadığım numara. "Meydanda bir şey olacak" dedi. Ciddiye aldım.', difficulty: 4, category: 'baski', relatedClues: [10, 11], guiltyResponse: { 1: 'Kasap\'ın soğuk hava deposunu araştırın!', 2: 'Eczacının tezgah altındaki bitkileri inceleyin!', 3: 'Muhtar\'ın kasasındaki sahte belgeleri bulun!', 4: '*Masaya yumruğunu vurur* TAMAM! Evet, muhtarı ihbardan önce aradım. Çünkü BEN o ihbarı aldım... kendi kendime. Anonim aramanın kaynağını biliyordum çünkü... o ses benim tanıdığımdı. Ve ben... olay yerine gittiğimde delilleri... düzenledim. Ama öldürmedim! Birisi bana talimat verdi!', 5: 'Yahya\'nın USB belleğinde her şey var!' } },
        { q: 'Delilleri karartma şüpheniz var.', a: 'Karartma mı?! 15 yıllık polisim! Evet, bazı bilgileri gizli tuttum ama prosedür gereği!', difficulty: 4, category: 'baski', relatedClues: [10, 11] },
        { q: 'Olay yerinden rapora yazmadığınız neler var?', a: '*Masaya bakar* Bir mektup... Kurbanın cebinden. "Beni takip ediyorlar, bu gece her şeyi açıklayacağım" yazıyordu...', difficulty: 4, category: 'baski', relatedClues: [15] },
        { q: 'Son sözünüz nedir Komiser?', a: 'Ben görevimi yaptım! Prosedür hataları oldu ama tarafsız kalmak zor... Adaletin yanındayım.', difficulty: 5, category: 'son', relatedClues: [] },
        { q: 'Katil kim sizce?', a: 'Kanıtlar muhtarı gösteriyor... Arazi meselesi, tehdit mektubu, o gece dışarı çıkması. Ama kasap da şüpheli.', difficulty: 5, category: 'son', relatedClues: [1, 7] },
        { q: 'Sakladığınız başka bilgi var mı?', a: 'Kurbanın cebindeki mektupta: "Yahya her şeyi biliyor" yazıyordu. Ne anlama geliyor bilmiyorum.', difficulty: 5, category: 'son', relatedClues: [15] },
        { q: 'Sizi de şüpheli listesine ekliyorum.', a: '*Sertleşir* Bu sizin hakkınız. Ama gerçek katili bulun, o zaman masumiyetimi görürsünüz.', difficulty: 5, category: 'son', relatedClues: [10, 11] }
    ],
    5: [ // Terzi Yahya — TÜM 20 SORU
        { q: 'Yahya usta, cinayet gecesi neredeydin?', a: 'Dükkânımdaydım, gece geç saate kadar dikiş dikiyordum. Sipariş yetişmesi lazımdı. Makinenin sesinden başka bir şey duymadım.', difficulty: 1, category: 'tanisma', relatedClues: [] },
        { q: 'Kurbanı tanıyor muydun?', a: 'Eski müşterimdi. Son birkaç haftadır sık geliyordu. Özel bir ceket sipariş etmişti... Teslim edemedim.', difficulty: 1, category: 'tanisma', relatedClues: [14, 15] },
        { q: 'Kasabadaki gerginliklerden haberin var mı?', a: 'Ben terziyim, kumaşla uğraşırım. İnsanların kavgalarına karışmam. Ama... son zamanlarda herkes gergindi.', difficulty: 1, category: 'tanisma', relatedClues: [] },
        { q: 'Dükkânında ilginç bir şey fark ettin mi?', a: 'İlginç mi? Hayır, her şey normal... Dikişler, kumaşlar, müşteriler. *Gözlerini kaçırır*', difficulty: 1, category: 'tanisma', relatedClues: [13] },
        { q: 'Kurbanın sipariş ettiği ceket nasıl bir ceketti?', a: 'Özel bir ceket... Gizli cepleri olan, astarı kalın. "İçine belgeler koyacağım" demişti.', difficulty: 2, category: 'derinlesme', relatedClues: [14, 15] },
        { q: 'Kurban sana belge saklatmak mı istedi?', a: 'Hayır! Sadece cekete cep dikeyim dedi. Belgeleri kendisi koyacaktı. Ben sadece terziyim!', difficulty: 2, category: 'derinlesme', relatedClues: [15] },
        { q: 'Cinayet gecesi dükkânından çıktın mı?', a: '*Duraksır* Bir kez çıktım. Sigara içmeye. Ama hemen döndüm. 10 dakika bile sürmedi.', difficulty: 2, category: 'derinlesme', relatedClues: [], guiltyResponse: { 1: 'Kasap\'ın dükkânında ışık yanıyordu ama girmedim oraya!', 2: 'Eczane\'nin ışığı da açıktı. Selma pencereden baktı ama beni görmemiş olmalı... *yutkunur*', 3: 'Muhtar\'ın sokakta olduğunu gördüm, ama yüzünü seçemedim.', 4: 'Komiser\'in devriye aracı geçti sokaktan... Beni görmüş olabilir.', 5: '*Elleri titrer* Tamam... 10 dakikadan fazla dışarıdaydım. Kurbanın evine gittim. Notu ben yazmıştım, "Bu gece gel konuşalım" diye. Gittim. Kapı açıktı. İçeri girdim. Osman... Osman yerde yatıyordu. Ama ben varmadan ölmüştü! ...Yoksa ölmemiş miydi?' } },
        { q: 'Eczacı Selma kurbanın evine giderken gördü.', a: 'Selma mı?! Yanılmış olmalı! Sadece sigara içmeye çıktım! *Elleri titrer*', difficulty: 2, category: 'derinlesme', relatedClues: [] },
        { q: 'Eczacı Selma kurbanın evine giderken gördü.', a: 'Selma mı?! Yanılmış olmalı! Sadece sigara içmeye çıktım! *Elleri titrer*', difficulty: 2, category: 'derinlesme', relatedClues: [] },
        { q: 'Bu kanlı iplik makarası senin dükkânından!', a: 'Kan mı?! İmkânsız! Kumaş keserken elimi keserim, o benim kanım olabilir!', difficulty: 3, category: 'yuzlestirme', relatedClues: [13] },
        { q: 'Bu yırtık kumaş kurbanın ceketinden kopmuş.', a: '*Yutkunur* O kumaş bende vardı evet. Ceketi dikerken artan parça. Her terzi artık kumaş saklar!', difficulty: 3, category: 'yuzlestirme', relatedClues: [14] },
        { q: 'Gizli cepteki not "Bu gece gel, konuşalım" yazıyor. Senin el yazın!', a: '*Terler* Ben yazdım evet. Kurban benimle konuşmak istedi! Gizli bir şey anlatacaktı ama... varamadım!', difficulty: 3, category: 'yuzlestirme', relatedClues: [15], guiltyResponse: { 1: 'Kasap Hasan\'ın o satırla bir ilgisi olabilir! Benden bıçak kılıfı istedi!', 2: 'Selma\'nın zehirleri var, onu araştırın! Ben sadece not yazdım!', 3: 'Muhtar\'ın tehdit mektubu daha tehlikeli bir delil!', 4: 'Komiser bu notu biliyordu! Dosyasında yazıyor!', 5: '*Titrer* Evet, o notu ben yazdım. Kurbanla buluşacaktık. Ama gittiğimde... kapı açıktı. Girdim. Tartıştık. O belgeler yüzünden... USB\'deki bilgiler... Ben sadece korumak istiyordum ama Osman vermek istemedi. Çekiştirdik ve...' } },
        { q: 'Neden varamadın? Yola çıktığını biliyoruz.', a: 'Çıktım evet! Ama yarı yolda döndüm! Korktum... Gölge gibi bir figür gördüm. Korkup geri döndüm!', difficulty: 3, category: 'yuzlestirme', relatedClues: [] },
        { q: 'Gördüğün gölge kim olabilir?', a: 'Bilmiyorum! Karanlıktı, yağmur yağıyordu... Uzun bir palto giyiyordu. Belki muhtarın paltosu.', difficulty: 4, category: 'baski', relatedClues: [12] },
        { q: 'Kasap Hasan\'a bıçak kılıfı diktin mi?', a: '*Gözleri büyür* Kim söyledi?! Evet diktim. Normal bir sipariş! Kasap bıçak kılıfı kullanır!', difficulty: 4, category: 'baski', relatedClues: [1, 3] },
        { q: 'Kurbanın sana anlattığı gizli şey neydi?', a: 'Tam anlatmadı. "Bu kasabada herkes bir şeyler gizliyor, dikkat et Yahya" dedi. Tapulardan, belgelerden bahsetti.', difficulty: 4, category: 'baski', relatedClues: [9, 15] },
        { q: 'Komiserin gizli dosyasında senin adın geçiyor.', a: 'BENİM ADIM MI?! Ne yazıyor?! Ben hiçbir suç işlemedim! Sadece elbise dikerim! *Panik yapar*', difficulty: 4, category: 'baski', relatedClues: [11] },
        { q: 'Son sözün nedir Yahya?', a: 'Ben masum bir terziyim! Kurbanla görüşmeye çalıştım ama varamadım! O gece herkes sokaktaydı...', difficulty: 5, category: 'son', relatedClues: [] },
        { q: 'Katil kim sence?', a: 'Muhtar Kemal! Arazi meselesi yüzünden... Ya da komiser. O kadın bir şeyler saklıyor, gözlerinden belli.', difficulty: 5, category: 'son', relatedClues: [7, 10] },
        { q: 'Sakladığın son bir şey var mı?', a: '*Gözyaşları* Kurbanın ceketi... Bitmiş haliyle duvarda asılı. İçindeki gizli cepte bir USB bellek var. Kimseye vermedim.', difficulty: 5, category: 'son', relatedClues: [15] },
        { q: 'USB bellekte ne var?', a: 'Bilmiyorum! Açmadım! Kurban "Başıma bir şey gelirse bunu doğru kişiye ver" demişti. Doğru kişi kim?', difficulty: 5, category: 'son', relatedClues: [15, 9] }
    ]
};

// =============================================================
// SES SİSTEMİ
// =============================================================

// Web Audio API for Character Voices (Undertale / Animal Crossing style)
const audioCtx = new (window.AudioContext || window.webkitAudioContext)();

function playSynthVoice(isFemale) {
    if (isMuted) return;
    try {
        if (audioCtx.state === 'suspended') {
            audioCtx.resume();
        }
        const osc = audioCtx.createOscillator();
        const gainNode = audioCtx.createGain();

        osc.connect(gainNode);
        gainNode.connect(audioCtx.destination);

        // Kadınlar için ince (sine), Erkekler için tok (triangle/sawtooth) ses dalgası
        osc.type = isFemale ? 'sine' : 'triangle';

        // Rastgele tonlama ile gerçekçi konuşma/mırıldanma hissi
        const baseFreq = isFemale ? (400 + Math.random() * 150) : (120 + Math.random() * 60);
        osc.frequency.setValueAtTime(baseFreq, audioCtx.currentTime);

        // Sesin şiddeti ve süresi
        gainNode.gain.setValueAtTime(0.15, audioCtx.currentTime);
        gainNode.gain.exponentialRampToValueAtTime(0.01, audioCtx.currentTime + 0.06);

        osc.start();
        osc.stop(audioCtx.currentTime + 0.06);
    } catch (e) { console.log('Web Audio Hatası:', e); }
}

function playJudgeGavelTripleStrike() {
    if (isMuted || window.isMuted) return;
    try {
        if (audioCtx.state === 'suspended') {
            audioCtx.resume();
        }
    } catch (e) { }

    // 3 Kere Tokmak Vuruşu: Tok... Tok... Tok! (Ağır Mahkeme Kürsüsü Vuruşları: 0.0s, 0.45s, 0.9s)
    [0, 450, 900].forEach((delay, strikeIndex) => {
        setTimeout(() => {
            if (isMuted || window.isMuted) return;

            // HTML5 Ses Çalımı (Hafif arka plan katkısı)
            const gavelEl = document.getElementById('gavel-sound');
            if (gavelEl) {
                try {
                    const clone = gavelEl.cloneNode();
                    clone.volume = 0.5;
                    clone.play().catch(() => { });
                    clone.onended = () => clone.remove();
                } catch (e) { }
            }

            // Web Audio API ile Ağır ve Olgun Masif Ahşap Hakim Tokmağı Sentezi
            try {
                if (audioCtx.state === 'suspended') audioCtx.resume();
                const now = audioCtx.currentTime;

                // 1. KATMAN: Derin Ahşap Gövde & Masa Titreşimi (Derin Sub & Low-Mid Bas Vuruşu)
                const oscLow = audioCtx.createOscillator();
                const gainLow = audioCtx.createGain();
                const filterLow = audioCtx.createBiquadFilter();

                oscLow.type = 'sine';
                // Tok başlangıç frekansı 95Hz'den 28Hz'e inerek ağır masa titreşimi verir
                oscLow.frequency.setValueAtTime(95, now);
                oscLow.frequency.exponentialRampToValueAtTime(28, now + 0.32);

                filterLow.type = 'lowpass';
                filterLow.frequency.setValueAtTime(220, now);

                gainLow.gain.setValueAtTime(0.95, now);
                gainLow.gain.exponentialRampToValueAtTime(0.001, now + 0.32);

                oscLow.connect(filterLow);
                filterLow.connect(gainLow);
                gainLow.connect(audioCtx.destination);

                oscLow.start(now);
                oscLow.stop(now + 0.32);

                // 2. KATMAN: Tok Ahşap Tak Sesi (Ağır Meşe Ağacı Rezonansı)
                const oscMid = audioCtx.createOscillator();
                const gainMid = audioCtx.createGain();
                const filterMid = audioCtx.createBiquadFilter();

                oscMid.type = 'triangle';
                oscMid.frequency.setValueAtTime(210, now);
                oscMid.frequency.exponentialRampToValueAtTime(55, now + 0.16);

                filterMid.type = 'bandpass';
                filterMid.frequency.setValueAtTime(320, now);
                filterMid.Q.setValueAtTime(2.5, now);

                gainMid.gain.setValueAtTime(0.85, now);
                gainMid.gain.exponentialRampToValueAtTime(0.001, now + 0.16);

                oscMid.connect(filterMid);
                filterMid.connect(gainMid);
                gainMid.connect(audioCtx.destination);

                oscMid.start(now);
                oscMid.stop(now + 0.16);

                // 3. KATMAN: Anlık Tok Vuruş Klak Sesi (Keskin ama Asla Tiz Olmayan Sert Temas)
                const bufferSize = audioCtx.sampleRate * 0.04;
                const buffer = audioCtx.createBuffer(1, bufferSize, audioCtx.sampleRate);
                const data = buffer.getChannelData(0);
                for (let i = 0; i < bufferSize; i++) {
                    data[i] = (Math.random() * 2 - 1) * Math.exp(-i / (audioCtx.sampleRate * 0.008));
                }
                const noise = audioCtx.createBufferSource();
                noise.buffer = buffer;

                const noiseFilter = audioCtx.createBiquadFilter();
                noiseFilter.type = 'lowpass';
                noiseFilter.frequency.setValueAtTime(650, now); // Tizliği kırılmış tok ahşap darbesi

                const noiseGain = audioCtx.createGain();
                noiseGain.gain.setValueAtTime(0.5, now);
                noiseGain.gain.exponentialRampToValueAtTime(0.001, now + 0.04);

                noise.connect(noiseFilter);
                noiseFilter.connect(noiseGain);
                noiseGain.connect(audioCtx.destination);

                noise.start(now);
                noise.stop(now + 0.04);

            } catch (err) {
                console.log('Gavel synthesis error:', err);
            }
        }, delay);
    });
}
window.playJudgeGavelTripleStrike = playJudgeGavelTripleStrike;

function playSound(audioEl, volume = 0.5) {
    if (!audioEl || isMuted || window.isMuted) return;
    try {
        audioEl.volume = Math.min(1, Math.max(0, volume));
        audioEl.currentTime = 0;
        audioEl.play().catch(e => console.log('Ses hatası:', e));
    } catch (e) { console.log('Ses hatası:', e); }
}
window.playSound = playSound;
window.stopSound = stopSound;

function playMumbleSound(audioEl, volume = 0.5) {
    if (!audioEl || isMuted) return;
    try {
        const clone = audioEl.cloneNode();
        clone.volume = Math.min(1, Math.max(0, volume));
        clone.play().catch(e => { });
        clone.onended = () => { clone.remove(); };
    } catch (e) { }
}

function playLoopSound(audioEl, volume = 0.3) {
    if (!audioEl || isMuted || window.isMuted) return;
    try {
        audioEl.volume = Math.min(1, Math.max(0, volume));
        audioEl.loop = true;
        const p = audioEl.play();
        if (p !== undefined) {
            p.catch(e => {
                // Tarayıcı ilk tıklamada otomatik başlatmayı bekleyebilir
                const resumeOnInteraction = () => {
                    if (!isMuted && !window.isMuted) audioEl.play().catch(() => { });
                    document.removeEventListener('click', resumeOnInteraction);
                };
                document.addEventListener('click', resumeOnInteraction, { once: true });
            });
        }
    } catch (e) { console.log('Ses hatası:', e); }
}
window.playLoopSound = playLoopSound;

function stopSound(audioEl) {
    if (!audioEl) return;
    try {
        audioEl.pause();
        audioEl.currentTime = 0;
    } catch (e) { }
}

function stopAllSounds() {
    [bgMusic, rainSound, thunderSound, chatterSound, doorCreak, doorClose, mumbleMale, mumbleFemale, typewriterSound].forEach(a => {
        if (a) { a.pause(); a.currentTime = 0; }
    });
    const doorVideo = document.getElementById('fehmi-door-video');
    if (doorVideo) {
        doorVideo.muted = true;
    }
}

function toggleMute() {
    isMuted = !isMuted;
    window.isMuted = isMuted;
    localStorage.setItem('gameMuted', isMuted);

    const btn = document.getElementById('mute-toggle-btn');
    if (!btn) return;

    const doorVideo = document.getElementById('fehmi-door-video');

    if (isMuted) {
        btn.classList.add('muted');
        btn.innerHTML = '<i class="fa-solid fa-volume-xmark"></i>';
        stopAllSounds();
        if (doorVideo) doorVideo.muted = true;
    } else {
        btn.classList.remove('muted');
        btn.innerHTML = '<i class="fa-solid fa-volume-high"></i>';
        if (doorVideo) doorVideo.muted = true; // Video sesi her zaman sessiz (kapı foley sesleri harici çalınır)
        // Arka plan müziğini ve yağmuru tekrar başlat (eğer oyun başladıysa)
        if (!splashScreen || splashScreen.classList.contains('hidden')) {
            playLoopSound(bgMusic, 0.3);
            playLoopSound(rainSound, 0.5);
            playLoopSound(chatterSound, 0.2);
        }
    }
}

// OTOPSİ VE ADLİ TIP SİSTEMİ (EN AZ 3/4 BİNA + EN AZ 3/4 LAB → 60 SANİYE KÖŞE SAYACI)
let autopsyTimer = null;
let autopsyTimeLeft = 60; // 1 dakika (60 saniye)
let isAutopsyReady = false;
let isAutopsyTimerStarted = false;
window.isAutopsyReady = false;
window.isAutopsyTimerStarted = false;

// KASABALARA ÖZEL OTOPSİ VE LAB DURUM BAYRAKLARI
window.isAutopsyReadyGizemli = false;
window.isAutopsyTimerStartedGizemli = false;
window.isAutopsyReadyGolge = false;
window.isAutopsyTimerStartedGolge = false;
window.isAutopsyReadySisoren = false;
window.isAutopsyTimerStartedSisoren = false;

// KASABALARA ÖZEL LAB SAYACLARI
window.submittedForensicCountGizemli = window.submittedForensicCountGizemli || 0;
window.submittedForensicCountGolge = window.submittedForensicCountGolge || 0;
window.submittedForensicCountSisoren = window.submittedForensicCountSisoren || 0;

function checkAutopsyConditions() {
    const isGolge = (window.currentActiveTown === 'golge_sehir');
    const isSisoren = (window.currentActiveTown === 'sisoren');
    const reqBuildings = isSisoren ? 5 : (isGolge ? 4 : 3);
    const reqLabs = isSisoren ? 5 : (isGolge ? 4 : 3);

    // Aktif kasabanın otopsi hazır/sayaç durumlarını senkronize et
    let currentIsReady, currentIsTimerStarted;
    if (isSisoren) {
        currentIsReady = window.isAutopsyReadySisoren;
        currentIsTimerStarted = window.isAutopsyTimerStartedSisoren;
    } else if (isGolge) {
        currentIsReady = window.isAutopsyReadyGolge;
        currentIsTimerStarted = window.isAutopsyTimerStartedGolge;
    } else {
        currentIsReady = window.isAutopsyReadyGizemli;
        currentIsTimerStarted = window.isAutopsyTimerStartedGizemli;
    }
    isAutopsyReady = currentIsReady || false;
    window.isAutopsyReady = isAutopsyReady;
    isAutopsyTimerStarted = currentIsTimerStarted || false;
    window.isAutopsyTimerStarted = isAutopsyTimerStarted;

    // Aktif kasabaya göre uygun lab sayacını al
    let currentLabCount;
    if (isSisoren) {
        currentLabCount = window.submittedForensicCountSisoren || 0;
    } else if (isGolge) {
        currentLabCount = window.submittedForensicCountGolge || 0;
    } else {
        currentLabCount = window.submittedForensicCountGizemli || 0;
    }
    window.submittedForensicCount = currentLabCount;

    // Aktif kasabaya göre girilmiş bina sayısını al
    let buildingCount = 0;
    if (isSisoren) {
        let uniqueSisorenVisited = new Set();
        if (window.SisorenEngine && window.SisorenEngine.visitedSisorenBuildings) {
            window.SisorenEngine.visitedSisorenBuildings.forEach(id => uniqueSisorenVisited.add(id));
        }
        if (window.visitedBuildings) {
            window.visitedBuildings.forEach(id => { if (id >= 201 && id <= 213) uniqueSisorenVisited.add(id); });
        }
        buildingCount = uniqueSisorenVisited.size;
    } else if (isGolge) {
        let uniqueGolgeVisited = new Set();
        if (window.GolgeSehirEngine && window.GolgeSehirEngine.visitedGolgeBuildings) {
            window.GolgeSehirEngine.visitedGolgeBuildings.forEach(id => uniqueGolgeVisited.add(id));
        }
        if (window.visitedBuildings) {
            window.visitedBuildings.forEach(id => { if (id >= 101 && id <= 199) uniqueGolgeVisited.add(id); });
        }
        buildingCount = uniqueGolgeVisited.size;
    } else {
        let uniqueGizemliVisited = new Set();
        if (window.visitedBuildings) {
            window.visitedBuildings.forEach(id => { if (id < 100) uniqueGizemliVisited.add(id); });
        }
        buildingCount = uniqueGizemliVisited.size;
    }

    const labCount = currentLabCount;
    const container = document.getElementById('autopsy-timer-container');

    updateForensicBadge();

    if (isGolge && window.GolgeSehirEngine) {
        window.GolgeSehirEngine.updateGolgeSehirAutopsyUI();
    }
    if (isSisoren && window.SisorenEngine) {
        window.SisorenEngine.updateSisorenAutopsyUI();
    }

    if (!container) return;

    if (isAutopsyReady || window.isAutopsyReady) {
        container.classList.remove('hidden', 'pending', 'active-timer');
        container.classList.add('ready');
        container.innerHTML = '<i class="fa-solid fa-file-signature"></i> \u2713 OTOPSİ RAPORU HAZIR! (TIKLA)';
        return;
    }

    if (isAutopsyTimerStarted || window.isAutopsyTimerStarted) return;

    // Durumu ekrandaki göstergede göster
    container.classList.remove('hidden', 'ready', 'active-timer');
    container.classList.add('pending');
    container.innerHTML = `<i class="fa-solid fa-clock-rotate-left"></i> OTOPSİ: BİNA ${buildingCount}/${reqBuildings} | LAB ${labCount}/${reqLabs}`;

    // En az gerekli sayıda binaya girilmiş VE gerekli sayıda lab gönderimi yapılmışsa 60 saniyelik GERİ SAYIM BAŞLAR!
    if (buildingCount >= reqBuildings && labCount >= reqLabs) {
        start60SecAutopsyCountdown();
    }
}

function start60SecAutopsyCountdown() {
    const isGolge = (window.currentActiveTown === 'golge_sehir');
    const isSisoren = (window.currentActiveTown === 'sisoren');
    let currentIsReady, currentIsTimerStarted;
    if (isSisoren) {
        currentIsReady = window.isAutopsyReadySisoren;
        currentIsTimerStarted = window.isAutopsyTimerStartedSisoren;
    } else if (isGolge) {
        currentIsReady = window.isAutopsyReadyGolge;
        currentIsTimerStarted = window.isAutopsyTimerStartedGolge;
    } else {
        currentIsReady = window.isAutopsyReadyGizemli;
        currentIsTimerStarted = window.isAutopsyTimerStartedGizemli;
    }

    if (currentIsTimerStarted || currentIsReady) return;

    if (isSisoren) {
        window.isAutopsyTimerStartedSisoren = true;
    } else if (isGolge) {
        window.isAutopsyTimerStartedGolge = true;
    } else {
        window.isAutopsyTimerStartedGizemli = true;
    }
    isAutopsyTimerStarted = true;
    window.isAutopsyTimerStarted = true;
    autopsyTimeLeft = 60;

    const reqBuildings = isSisoren ? 5 : (isGolge ? 4 : 3);
    const reqLabs = isSisoren ? 5 : (isGolge ? 4 : 3);

    const container = document.getElementById('autopsy-timer-container');
    if (container) {
        container.classList.remove('hidden', 'pending', 'ready');
        container.classList.add('active-timer');
        container.innerHTML = `<i class="fa-solid fa-hourglass-half fa-spin"></i> OTOPSİ: 01:00`;
    }

    showGlobalNotification("BİLGİ", `📋 OTOPSİ RAPORU HAZIRLANIYOR!\n\nEn az ${reqBuildings} bina incelendi ve ${reqLabs} delil adli tıbba gönderildi. Adli Tıp Kurumu otopsi raporunu hazırlamaya başladı! Ekrandaki 60 saniyelik sayacı takip ediniz.`, false);
    showCinematicHelper(`Amirims! ${reqBuildings} bina incelemesi ve ${reqLabs} lab gönderimi tamamlandı. Adli Tıp otopsi raporu hazırlanıyor, ekrandaki 60 saniyelik sayacı takip edebilirsiniz!`, false);

    if (autopsyTimer) clearInterval(autopsyTimer);

    autopsyTimer = setInterval(() => {
        autopsyTimeLeft--;

        const mins = Math.floor(autopsyTimeLeft / 60);
        const secs = autopsyTimeLeft % 60;
        const formattedTime = `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;

        if (container) {
            container.innerHTML = `<i class="fa-solid fa-hourglass-half fa-spin"></i> OTOPSİ: ${formattedTime}`;
        }

        if (autopsyTimeLeft <= 0) {
            clearInterval(autopsyTimer);
            autopsyTimer = null;
            isAutopsyReady = true;
            window.isAutopsyReady = true;

            if (window.currentActiveTown === 'sisoren') {
                window.isAutopsyReadySisoren = true;
                window.isAutopsyTimerStartedSisoren = false;
            } else if (window.currentActiveTown === 'golge_sehir') {
                window.isAutopsyReadyGolge = true;
                window.isAutopsyTimerStartedGolge = false;
            } else {
                window.isAutopsyReadyGizemli = true;
                window.isAutopsyTimerStartedGizemli = false;
            }

            if (container) {
                container.classList.remove('active-timer', 'pending');
                container.classList.add('ready');
                container.innerHTML = '<i class="fa-solid fa-file-signature"></i> ✓ OTOPSİ RAPORU HAZIR! (TIKLA)';
            }

            showGlobalNotification("BİLGİ", "📋 OTOPSİ RAPORU GELDİ!\n\nAdli Tıp Kurumu'ndan beklenen detaylı otopsi raporu ve laboratuvar analizleri merkeze ulaştı. Haritadaki 'OTOPSİ RAPORU HAZIR' butonuna tıklayarak hemen inceleyebilirsiniz.", false);
            showCinematicHelper("Amirims! Adli Tıp otopsi raporu hazır! Haritadaki 'OTOPSİ RAPORU HAZIR' butonuna tıklayarak hemen inceleyin!", false);
        }
    }, 1000);
}

function startAutopsyTimer() {
    const isGolge = (window.currentActiveTown === 'golge_sehir');
    if (isGolge) {
        window.isAutopsyTimerStartedGolge = false;
        window.isAutopsyReadyGolge = false;
    } else {
        window.isAutopsyTimerStartedGizemli = false;
        window.isAutopsyReadyGizemli = false;
    }
    isAutopsyTimerStarted = false;
    window.isAutopsyTimerStarted = false;
    isAutopsyReady = false;
    window.isAutopsyReady = false;
    autopsyTimeLeft = 60;
    if (autopsyTimer) clearInterval(autopsyTimer);
    autopsyTimer = null;
    checkAutopsyConditions();
}

let currentMessageBoxConfirm = null;
let currentMessageBoxCancel = null;

function showGameMessageBox({ title, message, type = 'info', onConfirm = null, onCancel = null, showCancel = false, confirmText = 'Anladım', cancelText = 'İptal' }) {
    const notifModal = document.getElementById('global-notification-modal');
    const notifText = document.getElementById('global-notification-text');
    const notifTitle = document.getElementById('global-notification-title');
    const notifIcon = document.getElementById('game-messagebox-icon');
    const okBtn = document.getElementById('notification-ok-btn');
    const cancelBtn = document.getElementById('messagebox-cancel-btn');

    if (notifTitle) notifTitle.textContent = title;
    if (notifText) notifText.textContent = message;

    if (notifIcon) {
        if (type === 'warning' || type === 'danger') {
            notifIcon.innerHTML = '<i class="fa-solid fa-triangle-exclamation"></i>';
            notifIcon.style.color = '#f59e0b';
        } else if (type === 'error') {
            notifIcon.innerHTML = '<i class="fa-solid fa-skull-crossbones"></i>';
            notifIcon.style.color = '#ef4444';
        } else {
            notifIcon.innerHTML = '<i class="fa-solid fa-envelope-open-text"></i>';
            notifIcon.style.color = '#38bdf8';
        }
    }

    if (okBtn) {
        okBtn.innerHTML = `<i class="fa-solid fa-check"></i> ${confirmText}`;
    }

    if (cancelBtn) {
        if (showCancel) {
            cancelBtn.classList.remove('hidden');
            cancelBtn.innerHTML = `<i class="fa-solid fa-xmark"></i> ${cancelText}`;
        } else {
            cancelBtn.classList.add('hidden');
        }
    }

    currentMessageBoxConfirm = onConfirm;
    currentMessageBoxCancel = onCancel;

    if (notifModal) notifModal.classList.remove('hidden');
}
window.showGameMessageBox = showGameMessageBox;

function showGlobalNotification(title, text, isWarning = false) {
    showGameMessageBox({
        title: title,
        message: text,
        type: isWarning ? 'warning' : 'info',
        showCancel: false,
        confirmText: 'Anladım'
    });
}

// Global notification modal close and confirm events
document.getElementById('notification-ok-btn')?.addEventListener('click', () => {
    document.getElementById('global-notification-modal').classList.add('hidden');
    if (typeof currentMessageBoxConfirm === 'function') {
        const cb = currentMessageBoxConfirm;
        currentMessageBoxConfirm = null;
        currentMessageBoxCancel = null;
        cb();
    }
});

document.getElementById('messagebox-cancel-btn')?.addEventListener('click', () => {
    document.getElementById('global-notification-modal').classList.add('hidden');
    if (typeof currentMessageBoxCancel === 'function') {
        const cb = currentMessageBoxCancel;
        currentMessageBoxConfirm = null;
        currentMessageBoxCancel = null;
        cb();
    }
});

function updateForensicBadge() {
    const badge = document.getElementById('forensic-pending-badge');
    const badgeText = document.getElementById('forensic-badge-text');
    if (!badge || !badgeText) return;

    const isSisoren = (window.currentActiveTown === 'sisoren' || document.body.classList.contains('sisoren-theme'));
    const isGolge = (window.currentActiveTown === 'golge_sehir' || document.body.classList.contains('golge-sehir-theme'));
    const reqLabs = isSisoren ? 5 : (isGolge ? 4 : 3);

    const labCount = isSisoren 
        ? (window.submittedForensicCountSisoren || 0)
        : (isGolge ? (window.submittedForensicCountGolge || 0) : submittedForensicCount);

    if (labCount === 0) {
        badge.className = 'forensic-status-badge badge-pending';
        badgeText.textContent = `0/${reqLabs} LAB GEREKLİ`;
    } else if (labCount < reqLabs) {
        badge.className = 'forensic-status-badge badge-pending';
        badgeText.textContent = `${labCount}/${reqLabs} LAB GÖNDERİLDİ`;
    } else {
        badge.className = 'forensic-status-badge badge-ready';
        badgeText.textContent = `✓ ${labCount} LAB GÖNDERİLDİ`;
    }
}

document.getElementById('forensic-pending-badge')?.addEventListener('click', () => {
    const isSisoren = (window.currentActiveTown === 'sisoren' || document.body.classList.contains('sisoren-theme'));
    const isGolge = (window.currentActiveTown === 'golge_sehir' || document.body.classList.contains('golge-sehir-theme'));
    const reqLabs = isSisoren ? 5 : (isGolge ? 4 : 3);
    const labCount = isSisoren 
        ? (window.submittedForensicCountSisoren || 0)
        : (isGolge ? (window.submittedForensicCountGolge || 0) : submittedForensicCount);
    showGlobalNotification("BİLGİ", `Adli Tıp Kurumu'na şu ana kadar ${labCount} adet delil bulgusu iletildi. (Otopsi raporunun başlaması için en az ${reqLabs} lab gönderimi gereklidir).`, false);
});

// Otopsi Tıklama Olayı
document.getElementById('autopsy-timer-container').addEventListener('click', () => {
    const isSisoren = (window.currentActiveTown === 'sisoren' || document.body.classList.contains('sisoren-theme'));
    const isGolge = (window.currentActiveTown === 'golge_sehir' || document.body.classList.contains('golge-sehir-theme'));
    const reqBuildings = isSisoren ? 5 : (isGolge ? 4 : 3);
    const reqLabs = isSisoren ? 5 : (isGolge ? 4 : 3);

    let buildingCount = 0;
    if (isSisoren) {
        let uniqueSisorenVisited = new Set();
        if (window.SisorenEngine && window.SisorenEngine.visitedSisorenBuildings) {
            window.SisorenEngine.visitedSisorenBuildings.forEach(id => uniqueSisorenVisited.add(id));
        }
        if (window.visitedBuildings) {
            window.visitedBuildings.forEach(id => { if (id >= 201 && id <= 213) uniqueSisorenVisited.add(id); });
        }
        buildingCount = uniqueSisorenVisited.size;
    } else if (isGolge) {
        let uniqueGolgeVisited = new Set();
        if (window.GolgeSehirEngine && window.GolgeSehirEngine.visitedGolgeBuildings) {
            window.GolgeSehirEngine.visitedGolgeBuildings.forEach(id => uniqueGolgeVisited.add(id));
        }
        if (window.visitedBuildings) {
            window.visitedBuildings.forEach(id => { if (id >= 101 && id <= 199) uniqueGolgeVisited.add(id); });
        }
        buildingCount = uniqueGolgeVisited.size;
    } else {
        let uniqueGizemliVisited = new Set();
        if (window.visitedBuildings) {
            window.visitedBuildings.forEach(id => { if (id < 100) uniqueGizemliVisited.add(id); });
        }
        buildingCount = uniqueGizemliVisited.size;
    }

    const labCount = isSisoren 
        ? (window.submittedForensicCountSisoren || 0)
        : (isGolge ? (window.submittedForensicCountGolge || 0) : submittedForensicCount);

    if (!isAutopsyTimerStarted && !isAutopsyReady) {
        showGlobalNotification("UYARI", `Otopsi raporunun hazırlanmaya başlaması için en az ${reqBuildings} bina incelenmeli (Şu an: ${buildingCount}/${reqBuildings}) ve en az ${reqLabs} delil adli tıbba gönderilmelidir (Şu an: ${labCount}/${reqLabs})!`, true);
        showCinematicHelper(`Amirims! Otopsi raporunun başlaması için en az ${reqBuildings} bina gezilmeli (${buildingCount}/${reqBuildings}) ve ${reqLabs} delil adli tıbba gönderilmeli (${labCount}/${reqLabs}).`, false);
        return;
    }

    if (isAutopsyTimerStarted && !isAutopsyReady) {
        showGlobalNotification("BİLGİ", `Adli Tıp Kurumu otopsi raporunu hazırlıyor! Raporun tamamlanmasına kalan süre: ${autopsyTimeLeft} saniye.`, false);
        showCinematicHelper(`Amirims! Adli Tıp otopsi raporunu hazırlıyor. Kalan süre: ${autopsyTimeLeft} saniye.`, false);
        return;
    }

    // Backend'den otopsi raporunu çek
    fetch('/api/game/autopsy?town=' + encodeURIComponent(window.currentActiveTown || 'gizemli_kasaba'))
        .then(async res => {
            if (!res.ok) {
                const errText = await res.text();
                throw new Error(errText);
            }
            return res.json();
        })
        .then(data => {
            if (data.success) {
                if (data.guiltyId) guiltyNpcId = data.guiltyId;
                document.getElementById('autopsy-text').innerHTML = data.report;
                document.getElementById('autopsy-modal').classList.remove('hidden');
            }
        })
        .catch(err => {
            console.error("Otopsi hatası:", err);
            document.getElementById('autopsy-text').textContent = "Otopsi raporu alınamadı: " + err.message;
            document.getElementById('autopsy-modal').classList.remove('hidden');
        });
});

document.getElementById('close-autopsy').addEventListener('click', () => {
    document.getElementById('autopsy-modal').classList.add('hidden');
});

// Mute butonunu başlat
function initMuteButton() {
    const btn = document.getElementById('mute-toggle-btn');
    if (!btn) return;

    btn.addEventListener('click', toggleMute);

    // Kayıtlı durumu uygula
    if (isMuted) {
        btn.classList.add('muted');
        btn.innerHTML = '<i class="fa-solid fa-volume-xmark"></i>';
    }
}

initMuteButton();

// =============================================================
// GAME INITIALIZATION
// =============================================================

function initGame() {
    currentBag = [];
    visitedBuildings.clear();
    dialogHistory = {};
    npcTalkCompleted = {};
    activeNpcId = null;
    npcQuestionPools = {};
    askedQuestionCount = {};
    npcStressLevels = {};

    // Otopsi Zamanlayıcısını Tamamen Sıfırla
    if (autopsyTimer) clearInterval(autopsyTimer);
    autopsyTimer = null;
    isAutopsyReady = false;
    autopsyTimeLeft = 240;
    const autopsyContainer = document.getElementById('autopsy-timer-container');
    if (autopsyContainer) {
        autopsyContainer.classList.add('hidden');
        autopsyContainer.classList.remove('ready');
        autopsyContainer.innerHTML = '<i class="fa-solid fa-clock"></i> <span id="autopsy-status-text">Otopsi Raporu Hazırlanıyor...</span>';
    }

    // Her NPC için soru havuzunu sıfırla
    for (let id = 1; id <= 5; id++) {
        npcQuestionPools[id] = [...(NPC_ALL_QUESTIONS[id] || [])];
        askedQuestionCount[id] = 0;
        npcStressLevels[id] = 0;
    }
    // Gölge Şehir (101 - 108) NPC'lerini sıfırla
    for (let id = 101; id <= 108; id++) {
        askedQuestionCount[id] = 0;
        npcStressLevels[id] = 0;
    }

    // Suçluyu backend API'den sıfırla
    const isGolge = (window.currentActiveTown === 'golge_sehir');
    const resetUrl = isGolge ? '/api/golge-sehir/reset' : '/api/game/reset';
    fetch(resetUrl, { method: 'POST' })
        .then(res => res.json())
        .then(data => {
            console.log('🔍 API:', data.message);
            // Suçlu NPC ID'sini al
            if (data.guiltyNpcId) {
                guiltyNpcId = data.guiltyNpcId;
                console.log('🔍 Suçlu NPC:', guiltyNpcId);
                // Backend'de oturum başlat
                fetch('/api/game/session/start', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ GuiltyNpcId: guiltyNpcId })
                })
                    .then(r => r.json())
                    .then(sData => {
                        if (sData.success) currentSessionId = sData.sessionId;
                    })
                    .catch(() => { });
            }
        })
        .catch(err => console.error('API Error:', err));

    // Sinematik bağlamları sıfırla
    shownCinematicContexts = new Set();

    // Haritadaki binaları resetle
    visitedBuildings.clear();
    const buildingIcons = {
        1: '<i class="fa-solid fa-drumstick-bite"></i> KASAP',
        2: '<i class="fa-solid fa-prescription-bottle-medical"></i> ECZANE',
        3: '<i class="fa-solid fa-building-flag"></i> MUHTARLIK',
        4: '<i class="fa-solid fa-building-shield"></i> KARAKOL',
        5: '<i class="fa-solid fa-scissors"></i> TERZİ'
    };
    document.querySelectorAll('.map-building').forEach(b => {
        b.classList.remove('visited');
        const npcId = parseInt(b.getAttribute('data-npc-id'));
        const tag = b.querySelector('.building-hover-tag');
        if (tag && buildingIcons[npcId]) {
            tag.innerHTML = buildingIcons[npcId];
        }
    });
}

initGame();

// =============================================================
// 1. SPLASH → WORLD MAP → STORY INTRO
// =============================================================

// Splash → Bölge Haritası
document.getElementById('start-btn').addEventListener('click', () => {
    playLoopSound(bgMusic, 0.3);
    playLoopSound(rainSound, 0.5);

    triggerTransition(() => {
        splashScreen.classList.add('hidden');
        worldMapScreen.classList.remove('hidden');
    });

    // Çetin bölge haritasında konuşsun (Geçmiş tecrübeyi ve çözülen kasabaları hatırlar)
    setTimeout(() => {
        if (window.gizemliSolved && window.golgeSolved) {
            showCinematicHelper("Harika bir iş çıkardık Amirims! Hem Gizemli Kasaba'yı hem de Gölge Şehir'i aydınlattık! Şimdi diğer kasabalardaki yeni gizemlere odaklanalım!", false, 'world_map_both_solved');
        } else if (window.gizemliSolved) {
            showCinematicHelper("Tebrikler Amirims! Gizemli Kasaba davasını başarıyla çözdük ve Gölge Şehir'in kilidini açtık. Hazır olduğunuzda yeni kasabaya geçelim!", false, 'world_map_gizemli_solved');
        } else {
            triggerHelperMessage('splash');
        }
    }, 1000);
});

// Bölge Haritası → Ana Menü
document.getElementById('exit-game-btn').addEventListener('click', () => {
    showGameMessageBox({
        title: "KASABADAN AYRILMA",
        message: "Ana haritaya dönerseniz bu kasabadaki tüm ilerlemeniz (topladığınız deliller, konuşma geçmişi ve mevcut katil) tamamen sıfırlanacak ve yeni bir rastgele katil belirlenecektir. Çıkmak istediğinize emin misiniz?",
        type: "warning",
        showCancel: true,
        confirmText: "Evet, Haritaya Dön",
        cancelText: "Kasabada Kal",
        onConfirm: async () => {
            try {
                let endpoint = '/api/game/reset';
                if (window.currentActiveTown === 'golge_sehir') {
                    endpoint = '/api/golge-sehir/reset';
                }
                const res = await fetch(endpoint, { method: 'POST' });
                const resetData = await res.json();
                if (resetData && resetData.guiltyNpcId) {
                    guiltyNpcId = resetData.guiltyNpcId;
                    console.log("🔄 Yeni Rastgele Katil Belirlendi:", guiltyNpcId);
                }

                triggerTransition(() => {
                    townMapScreen.classList.add('hidden');
                    if (interiorScreen) interiorScreen.classList.add('hidden');
                    worldMapScreen.classList.remove('hidden');

                    // UI & Envanter & Ziyaret & Masumiyet Sıfırlama (Yeni Oyun)
                    visitedBuildings.clear();
                    innocentNpcIds.clear();
                    if (window.GolgeSehirEngine) {
                        window.GolgeSehirEngine.visitedGolgeBuildings.clear();
                        window.GolgeSehirEngine.clearGolgeSehirMap();
                    }
                    currentBag = [];
                    submittedBloodClueIds.clear();
                    submittedPrintClueIds.clear();
                    submittedForensicCount = 0;
                    submittedForensicCountGizemli = 0;
                    submittedForensicCountGolge = 0;
                    dialogHistory = {};
                    npcTalkCompleted = {};
                    activeNpcId = null;

                    updateForensicBadge();
                    checkAutopsyConditions();

                    // Kapı kilitlerini sıfırla
                    document.querySelectorAll('.map-building').forEach(b => {
                        b.classList.remove('visited');
                    });
                });
                stopAllSounds();
            } catch (err) {
                console.error('Reset hatası:', err);
            }
        }
    });
});

document.getElementById('world-back-btn')?.addEventListener('click', () => {
    triggerTransition(() => {
        worldMapScreen.classList.add('hidden');
        splashScreen.classList.remove('hidden');
    });
});

// Bölge Haritasındaki Kasaba Tıklamaları & Seviye İlerleme Kilidi (1. Seviye: Gizemli Kasaba, 2. Seviye: Gölge Şehir)
document.querySelectorAll('.region-town').forEach(townEl => {
    townEl.addEventListener('click', (e) => {
        const townId = townEl.getAttribute('data-town-id');
        const townName = townEl.getAttribute('data-town-name');

        if (townId === 'golge_sehir' || townName === 'Gölgeşehir') {
            // Kilit Kontrolü: Gizemli Kasaba çözülmeden Gölge Şehir açılamaz
            if (townEl.classList.contains('town-locked') && !window.gizemliSolved) {
                document.getElementById('locked-town-title').textContent = "GÖLGE ŞEHİR - KİLİTLİ BÖLGE";
                document.getElementById('locked-town-desc').textContent = "Gölge Şehir soruşturmasına geçmek için önce Gizemli Kasaba cinayet vakasını başarıyla çözmeli ve katili bulmalısınız!";
                if (townLockedModal) townLockedModal.classList.remove('hidden');
                return;
            }

            window.currentActiveTown = 'golge_sehir';
            document.body.classList.add('golge-sehir-theme');
            // Gölge Şehir delil havuzunu izole et (Gizemli Kasaba eşyalarını tamamen ayır)
            currentBag = currentBag.filter(c => c.id >= 1000);
            window.currentBag = currentBag;

            // Her yeni girişte yeni bir Gölge Şehir vaka oturumu başlat
            if (window.GolgeSehirEngine) {
                window.GolgeSehirEngine.showSuccessModalBeforeStory();
            }
        } else if (townId === 'gizemli') {
            // Gizemli Kasaba (1. Seviye) → Osman Bey Hikaye Ekranı
            window.currentActiveTown = 'gizemli';
            document.body.classList.remove('golge-sehir-theme');
            // Gizemli Kasaba delil havuzunu izole et (Gölge Şehir eşyalarını tamamen ayır)
            currentBag = currentBag.filter(c => c.id < 1000);
            window.currentBag = currentBag;

            if (window.GolgeSehirEngine) {
                window.GolgeSehirEngine.resetGolgeState();
            }

            const storyBadge = document.querySelector('.story-badge');
            if (storyBadge) {
                storyBadge.innerHTML = '<i class="fa-solid fa-scroll"></i> VAKA DOSYASI #104';
            }
            triggerTransition(() => {
                worldMapScreen.classList.add('hidden');
                storyIntroScreen.classList.remove('hidden');
                initGame();
                startTypewriter();
            });
        } else if (townId === 'sisoren' || townName === 'Sisören') {
            // Sisören (3. Seviye Dağ Kasabası)
            if (townEl.classList.contains('town-locked') && (!window.gizemliSolved || !window.golgeSolved)) {
                document.getElementById('locked-town-title').textContent = "SİSÖREN - KİLİTLİ BÖLGE";
                document.getElementById('locked-town-desc').textContent = "Sisören dağ kasabasına giden patikalar yoğun sis altında kapalı. Önce Gizemli Kasaba ve Gölge Şehir vakalarını aydınlatmalısınız!";
                if (townLockedModal) townLockedModal.classList.remove('hidden');
                return;
            }

            if (window.SisorenEngine) {
                window.SisorenEngine.showSisorenStoryIntro();
            }
        } else {
            // Diğer Kilitli Kasabalar (4, 5: Yıkıkköy, Karaorman)
            document.getElementById('locked-town-title').textContent = `${(townName || '').toUpperCase()} - KİLİTLİ BÖLGE`;
            document.getElementById('locked-town-desc').textContent = `${townName} kasabasında yol kapalı ve henüz soruşturma izni verilmedi. Önceki vakaları sırasıyla çözmelisiniz!`;
            if (townLockedModal) townLockedModal.classList.remove('hidden');
        }
    });
});

// Kilitli Modal Kapatma
document.getElementById('close-locked-modal-btn')?.addEventListener('click', () => {
    townLockedModal.classList.add('hidden');
});

const STORY_TEXT = 'Yağmurlu bir sonbahar gecesi... Kasabanın meydanında bir ceset bulundu. Kurban, herkesin tanıdığı tüccar Osman Bey\'di. Parke taşların üzerinde yatan cansız beden, yağmurun altında solgun bir ışıkla aydınlanıyordu. Polis şeridinin arkasında toplanan kalabalık, birbirlerine şüpheyle bakıyordu. Kasabanın en deneyimli dedektifi olarak bu davayı çözmek için buraya çağrıldınız. Beş şüpheli, beş bina, sayısız sır... Gerçeği ortaya çıkarabilecek misiniz?';

let typewriterTimeout = null;
let isTypewriterActive = false;

function startTypewriter() {
    const el = document.getElementById('typewriter-text');
    const continueBtn = document.getElementById('story-continue-btn');
    const skipBtn = document.getElementById('skip-story-btn');
    const cursor = document.querySelector('.story-cursor');

    if (skipBtn) skipBtn.classList.remove('hidden');
    if (continueBtn) continueBtn.classList.add('hidden');
    if (cursor) cursor.style.display = 'inline-block';

    playLoopSound(bgMusic, 0.3);
    playLoopSound(rainSound, 0.5);
    playLoopSound(typewriterSound, 0.4);

    if (el) el.textContent = '';
    let i = 0;
    const speed = 35;
    isTypewriterActive = true;

    function type() {
        if (!isTypewriterActive) return;
        if (i < STORY_TEXT.length) {
            if (el) el.textContent += STORY_TEXT.charAt(i);
            i++;
            typewriterTimeout = setTimeout(type, speed);
        } else {
            finishTypewriter();
        }
    }
    type();
}

function finishTypewriter() {
    isTypewriterActive = false;
    if (typewriterTimeout) clearTimeout(typewriterTimeout);
    stopSound(typewriterSound);

    const el = document.getElementById('typewriter-text');
    const continueBtn = document.getElementById('story-continue-btn');
    const skipBtn = document.getElementById('skip-story-btn');
    const cursor = document.querySelector('.story-cursor');

    if (el) el.textContent = STORY_TEXT;
    if (cursor) cursor.style.display = 'none';
    if (skipBtn) skipBtn.classList.add('hidden');
    if (continueBtn) {
        continueBtn.classList.remove('hidden');
        continueBtn.innerHTML = '<i class="fa-solid fa-magnifying-glass"></i> SORUŞTURMAYI BAŞLAT';
    }
}

// Hikayeyi Geç Butonu
document.getElementById('skip-story-btn')?.addEventListener('click', () => {
    if (window.currentActiveTown === 'golge_sehir') {
        if (window.GolgeSehirEngine) {
            // Gölge şehir için daktilo atla
            const el = document.getElementById('typewriter-text');
            const continueBtn = document.getElementById('story-continue-btn');
            const skipBtn = document.getElementById('skip-story-btn');
            const cursor = document.querySelector('.story-cursor');
            if (window.golgeTypewriterTimer) clearInterval(window.golgeTypewriterTimer);
            if (el && window.GOLGE_SEHIR_CONFIG) el.textContent = window.GOLGE_SEHIR_CONFIG.storyIntroText;
            if (cursor) cursor.style.display = 'none';
            if (skipBtn) skipBtn.classList.add('hidden');
            if (continueBtn) {
                continueBtn.classList.remove('hidden');
                continueBtn.innerHTML = '<i class="fa-solid fa-folder-open"></i> DOSYAYI AÇ VE SORUŞTUR';
            }
        }
    } else if (window.currentActiveTown === 'sisoren') {
        // Sisören için daktilo atla
        const el = document.getElementById('typewriter-text');
        const continueBtn = document.getElementById('story-continue-btn');
        const skipBtn = document.getElementById('skip-story-btn');
        const cursor = document.querySelector('.story-cursor');
        if (window.sisorenTypewriterTimer) clearTimeout(window.sisorenTypewriterTimer);
        if (typeof window.stopSound === 'function' && window.typewriterSound) {
            window.stopSound(window.typewriterSound);
        }
        if (el && window.SISOREN_CONFIG) el.textContent = window.SISOREN_CONFIG.storyIntroText;
        if (cursor) cursor.style.display = 'none';
        if (skipBtn) skipBtn.classList.add('hidden');
        if (continueBtn) {
            continueBtn.classList.remove('hidden');
            continueBtn.innerHTML = '<i class="fa-solid fa-magnifying-glass"></i> SORUŞTURMAYI BAŞLAT';
        }
    } else {
        finishTypewriter();
    }
});

// Story → Town Map (Kasaba Haritası Geçişi)
document.getElementById('story-continue-btn').addEventListener('click', () => {
    if (window.currentActiveTown === 'golge_sehir') {
        storyIntroScreen.classList.add('hidden');
        if (window.GolgeSehirEngine) {
            window.GolgeSehirEngine.playBekciWalkAnimation();
        }
        return;
    }

    if (window.currentActiveTown === 'sisoren') {
        triggerTransition(() => {
            storyIntroScreen.classList.add('hidden');
            townMapScreen.classList.remove('hidden');
            if (window.SisorenEngine) {
                window.SisorenEngine.loadSisorenMap();
            }
        });
        return;
    }

    triggerTransition(() => {
        storyIntroScreen.classList.add('hidden');
        townMapScreen.classList.remove('hidden');
        playLoopSound(bgMusic, 0.3);
        playLoopSound(rainSound, 0.5);
        startAutopsyTimer(); // Oyuna (Haritaya) geçildiğinde sayacı başlat
    });
    // Çetin harita girişinde konuşsun
    setTimeout(() => {
        triggerHelperMessage('story_end');
        setTimeout(() => {
            if (!townMapScreen.classList.contains('hidden')) {
                triggerHelperMessage('map_enter');
            }
        }, 10000);
    }, 1200);
});

// Kasaba Haritası → Bölge Haritası

// =============================================================
// 2. TOWN MAP — BUILDING CLICKS & DOOR ANIMATION
// =============================================================

const MASTER_VIDEO_PATH = 'images/master_building_transitions.mp4';

const BUILDING_THEMES = {
    1: { name: 'Kasap', video: MASTER_VIDEO_PATH, start: 4.0, end: 6.0, glow: 'glow-kasap', icon: 'fa-solid fa-drumstick-bite', color: '#ff3344' },
    2: { name: 'Eczane', video: MASTER_VIDEO_PATH, start: 0.0, end: 2.0, glow: 'glow-eczane', icon: 'fa-solid fa-prescription-bottle-medical', color: '#00ff88' },
    3: { name: 'Muhtarlık', video: MASTER_VIDEO_PATH, start: 2.0, end: 4.0, glow: 'glow-muhtarlik', icon: 'fa-solid fa-building-flag', color: '#ffaa33' },
    4: { name: 'Karakol', video: MASTER_VIDEO_PATH, start: 8.0, end: 10.0, glow: 'glow-karakol', icon: 'fa-solid fa-building-shield', color: '#2288ff' },
    5: { name: 'Terzi', video: MASTER_VIDEO_PATH, start: 6.0, end: 8.0, glow: 'glow-terzi', icon: 'fa-solid fa-scissors', color: '#ffcc00' },

    // Gölge Şehir (101 - 108) - Altın/Sarı Neon Kapılar
    101: { name: 'Oduncu', video: 'images/towns/golge_sehir/Görseldeki_yazılar_da_türkçe_o.mp4', start: 5.7, end: 7.0, glow: 'glow-golge', icon: 'fa-solid fa-axe', color: '#f59e0b' },
    102: { name: 'Manav', video: 'images/towns/golge_sehir/Görseldeki_yazılar_da_türkçe_o.mp4', start: 2.8, end: 4.1, glow: 'glow-golge', icon: 'fa-solid fa-carrot', color: '#f59e0b' },
    103: { name: 'Demirci', video: 'images/towns/golge_sehir/Görseldeki_yazılar_da_türkçe_o.mp4', start: 4.2, end: 5.6, glow: 'glow-golge', icon: 'fa-solid fa-anvil', color: '#f59e0b' },
    104: { name: 'Bakkal', video: 'images/towns/golge_sehir/Görseldeki_yazılar_da_türkçe_o.mp4', start: 1.4, end: 2.7, glow: 'glow-golge', icon: 'fa-solid fa-store', color: '#f59e0b' },
    105: { name: 'Hekim', video: 'images/towns/golge_sehir/Görseldeki_yazılar_da_türkçe_o.mp4', start: 0.0, end: 1.3, glow: 'glow-golge', icon: 'fa-solid fa-notes-medical', color: '#f59e0b' },
    106: { name: 'Muhtarlık', video: 'images/towns/golge_sehir/Görseldeki_yazılar_da_türkçe_o.mp4', start: 8.5, end: 9.9, glow: 'glow-golge', icon: 'fa-solid fa-landmark', color: '#f59e0b' },
    107: { name: 'Fehmi Bey Ev', video: MASTER_VIDEO_PATH, start: 6.0, end: 8.0, glow: 'glow-golge', icon: 'fa-solid fa-house-user', color: '#f59e0b' },
    108: { name: 'Kunduracı', video: 'images/towns/golge_sehir/Görseldeki_yazılar_da_türkçe_o.mp4', start: 7.1, end: 8.4, glow: 'glow-golge', icon: 'fa-solid fa-shoe-prints', color: '#f59e0b' }
};

let pendingBuildingNpcId = null;

const buildingEntryModal = document.getElementById('building-entry-modal');
const doorInteractiveFrame = document.getElementById('door-interactive-frame');
const buildingTransitionVideo = document.getElementById('building-transition-video');
const doorHandlePrompt = document.getElementById('door-handle-prompt');
const cancelEntryBtn = document.getElementById('cancel-entry-btn');

let currentVideoCheckTimer = null;

document.querySelectorAll('.map-building').forEach(b => {
    b.addEventListener('click', () => {
        if (b.classList.contains('visited')) return;
        const npcId = parseInt(b.getAttribute('data-npc-id'));
        openDoorTransitionModal(npcId);
    });
});

function openDoorTransitionModal(npcId) {
    pendingBuildingNpcId = npcId;
    isVideoPlaying = false;
    if (currentVideoCheckTimer) clearInterval(currentVideoCheckTimer);

    const npc = NPC_DATA[npcId];
    const theme = BUILDING_THEMES[npcId];
    if (!npc || !theme) return;

    const ambientGlow = document.getElementById('door-ambient-glow');
    const neonIcon = document.getElementById('door-neon-icon');
    const topHeader = document.querySelector('.door-top-header');

    if (ambientGlow) {
        ambientGlow.className = `door-ambient-glow ${theme.glow}`;
    }
    if (neonIcon) {
        neonIcon.className = theme.icon;
        neonIcon.style.color = theme.color;
    }

    document.getElementById('door-building-title').textContent = `${npc.building.toUpperCase()} - ${npc.name.toUpperCase()}`;
    document.getElementById('door-building-desc').textContent = `${npc.building} binasına girmek için kapıyı tıklayın`;

    // Kapı modalının ve arka planının Kasap görsel standardında yüklenmesi
    const bgUrl = `url('${npc.bg}?v=${Date.now()}')`;
    if (buildingEntryModal) {
        buildingEntryModal.style.backgroundImage = bgUrl;
    }
    const doorBgImg = document.getElementById('door-bg-image');
    if (doorBgImg) {
        doorBgImg.style.backgroundImage = bgUrl;
        doorBgImg.style.backgroundSize = 'cover';
        doorBgImg.style.backgroundPosition = 'center center';
    }

    if (topHeader) topHeader.style.opacity = '1';
    if (doorHandlePrompt) doorHandlePrompt.style.opacity = '1';

    buildingEntryModal.classList.remove('hidden');

    // Binaya tıklandığı an geçiş animasyon videosunu anında sesli ve görüntülü oynat!
    startVideoPlayback();
}

// Kapıya tıklama / Binaya basıldığında animasyon oynatma
let isVideoPlaying = false;

function startVideoPlayback() {
    if (!pendingBuildingNpcId || isVideoPlaying) return;

    isVideoPlaying = true;
    const theme = BUILDING_THEMES[pendingBuildingNpcId];
    const topHeader = document.querySelector('.door-top-header');

    if (topHeader) topHeader.style.opacity = '0';
    if (doorHandlePrompt) doorHandlePrompt.style.opacity = '0';
    playSound(doorCreak, 0.85);

    // Ana müzik ve yağmur sesini kıs (Ducking)
    if (bgMusic && !bgMusic.paused) bgMusic.volume = 0.05;
    if (rainSound && !rainSound.paused) rainSound.volume = 0.1;

    if (buildingTransitionVideo && theme) {
        buildingTransitionVideo.style.display = 'block';
        buildingTransitionVideo.style.opacity = '1';
        buildingTransitionVideo.style.zIndex = '50';
        buildingTransitionVideo.muted = isMuted;
        buildingTransitionVideo.volume = 0.85;

        try {
            if (!buildingTransitionVideo.src.includes(theme.video)) {
                buildingTransitionVideo.src = theme.video;
            }
            buildingTransitionVideo.currentTime = theme.start || 0;
        } catch (e) { }

        const attemptPlay = () => {
            try { buildingTransitionVideo.currentTime = theme.start || 0; } catch (e) { }
            const playPromise = buildingTransitionVideo.play();
            if (playPromise !== undefined) {
                playPromise.then(() => {
                    if (!isMuted) {
                        if (bgMusic) bgMusic.volume = 0.1;
                        if (rainSound) rainSound.volume = 0.2;
                        if (chatterSound) chatterSound.volume = 0.05;
                    }
                }).catch(e => {
                    console.warn('Video sessiz modda deneniyor:', e);
                    buildingTransitionVideo.muted = true;
                    buildingTransitionVideo.play().catch(() => finishVideoTransition());
                });
            }
        };

        buildingTransitionVideo.onloadedmetadata = attemptPlay;
        attemptPlay();

        buildingTransitionVideo.onended = () => {
            finishVideoTransition();
        };

        if (currentVideoCheckTimer) clearInterval(currentVideoCheckTimer);
        currentVideoCheckTimer = setInterval(() => {
            if (!isVideoPlaying) {
                clearInterval(currentVideoCheckTimer);
                return;
            }
            if (buildingTransitionVideo.currentTime >= (theme.end - 0.08) || buildingTransitionVideo.ended) {
                clearInterval(currentVideoCheckTimer);
                buildingTransitionVideo.pause();
                finishVideoTransition();
            }
        }, 30);

        setTimeout(() => {
            if (isVideoPlaying) {
                if (currentVideoCheckTimer) clearInterval(currentVideoCheckTimer);
                finishVideoTransition();
            }
        }, 1900);
    } else {
        finishVideoTransition();
    }
}

doorInteractiveFrame?.addEventListener('click', startVideoPlayback);
buildingTransitionVideo?.addEventListener('click', startVideoPlayback);

function finishVideoTransition() {
    if (currentVideoCheckTimer) clearInterval(currentVideoCheckTimer);
    isVideoPlaying = false;
    playSound(doorClose, 0.6);

    // Ana müzik ve yağmur sesini orijinal seviyelerine geri yükle
    if (!isMuted) {
        if (bgMusic && !bgMusic.paused) bgMusic.volume = 0.3;
        if (rainSound && !rainSound.paused) rainSound.volume = 0.5;
        if (chatterSound && !chatterSound.paused) chatterSound.volume = 0.2;
    }

    buildingEntryModal.classList.add('hidden');
    openBuilding(pendingBuildingNpcId);
}

cancelEntryBtn?.addEventListener('click', (e) => {
    e.stopPropagation();
    if (currentVideoCheckTimer) clearInterval(currentVideoCheckTimer);
    if (buildingTransitionVideo) buildingTransitionVideo.pause();
    buildingEntryModal.classList.add('hidden');

    // Ana müziği geri yükle
    if (!isMuted) {
        if (bgMusic && !bgMusic.paused) bgMusic.volume = 0.3;
        if (rainSound && !rainSound.paused) rainSound.volume = 0.5;
        if (chatterSound && !chatterSound.paused) chatterSound.volume = 0.2;
    }

    pendingBuildingNpcId = null;
    isVideoPlaying = false;
});

function openBuilding(npcId) {
    activeNpcId = npcId;

    // Gölge Şehir Binaları
    if (npcId >= 100 && window.GolgeSehirEngine && window.GOLGE_SEHIR_CONFIG) {
        const bld = window.GOLGE_SEHIR_CONFIG.buildings.find(b => b.npcId === npcId);
        if (bld) {
            window.GolgeSehirEngine.onBuildingClick(bld);
            const buildingName = bld.npc.building || bld.title;
            setTimeout(() => triggerHelperMessage('building_enter', buildingName, false), 1500);
            logAction('enter_building', npcId, buildingName);
            return;
        }
    }

    const npc = NPC_DATA[npcId];
    if (!npc) return;

    // Geniş panoramik birleştirici devam görselleri
    const WIDE_PANORAMAS = {
        1: 'images/kasap_wide.png',
        2: 'images/eczane_wide.png',
        3: 'images/muhtarlik_wide.png',
        4: 'images/karakol_wide.png',
        5: 'images/terzi_wide.png'
    };

    const wideImgUrl = `url('${(WIDE_PANORAMAS[npcId] || npc.bg)}?v=${Date.now()}')`;
    const mainBgUrl = `url('${npc.bg}?v=${Date.now()}')`;

    const stageCanvas = document.getElementById('interior-stage-canvas');
    const intScreen = document.getElementById('interior-screen');
    const sideLeft = document.getElementById('interior-side-left');
    const sideRight = document.getElementById('interior-side-right');

    // Katman 1: Ana kapsayıcı arkaplanı
    if (intScreen) {
        intScreen.setAttribute('data-npc-id', npcId);
        intScreen.style.backgroundImage = wideImgUrl;
        intScreen.style.backgroundSize = 'cover';
        intScreen.style.backgroundPosition = 'center center';
        intScreen.style.backgroundRepeat = 'no-repeat';
    }

    // Katman 2: SADECE Muhtarlık (ID 3) için çift katman çakışması olmaması adına canvas arkaplanı temizlenir
    // Diğer binalar (Kasap, Eczane, Karakol, Terzi) orijinal çift katmanlı yapısında kalır
    if (stageCanvas) {
        if (npcId === 3) {
            stageCanvas.style.backgroundImage = 'none';
            stageCanvas.style.backgroundColor = 'transparent';
        } else {
            stageCanvas.style.backgroundImage = mainBgUrl;
            stageCanvas.style.backgroundSize = 'contain';
            stageCanvas.style.backgroundPosition = 'center center';
            stageCanvas.style.backgroundRepeat = 'no-repeat';
            stageCanvas.style.backgroundColor = 'transparent';
        }
    }

    // Siyah boşluk ve dikiş izi yaratmaması için yan paneller temizlendi
    if (sideLeft && sideRight) {
        sideLeft.style.display = 'none';
        sideRight.style.display = 'none';
    }

    const isNpcInnocent = (window.innocentNpcIds && window.innocentNpcIds.has(npcId)) || innocentNpcIds.has(npcId);
    const talkBtn = document.getElementById('talk-npc-btn');
    const talkNameEl = document.getElementById('talk-npc-name');

    if (talkNameEl) {
        talkNameEl.innerText = isNpcInnocent ? `${npc.name} (Masum İlan Edildi)` : `${npc.name} ile Konuş`;
    }
    if (talkBtn) {
        if (isNpcInnocent) {
            talkBtn.style.opacity = '0.5';
            talkBtn.style.cursor = 'not-allowed';
            talkBtn.title = 'Bu karakter masum ilan edildiği için sorgulanamaz.';
        } else {
            talkBtn.style.opacity = '1';
            talkBtn.style.cursor = 'pointer';
            talkBtn.title = '';
        }
    }

    // Load Hotspots (Sadece Gizemli Kasaba için hotspots-container'a yükle; Gölge Şehir'i GolgeSehirEngine yönetecek)
    const container = document.getElementById('hotspots-container');
    if (container) container.innerHTML = '';

    if (window.currentActiveTown !== 'golge_sehir' && npcId < 100) {
        const objects = SCENE_OBJECTS[npcId] || [];
        objects.forEach(obj => {
            const wrapper = document.createElement('div');
            wrapper.className = 'hotspot-wrapper';
            wrapper.style.position = 'absolute';
            wrapper.style.top = obj.top;
            wrapper.style.left = obj.left;

            const img = document.createElement('img');
            img.src = obj.img;
            img.className = 'hotspot-img';
            img.alt = obj.name;

            if (obj.isHidden) {
                wrapper.classList.add('hidden-clue');
            }

            const label = document.createElement('span');
            label.className = 'hotspot-label';
            label.textContent = obj.name;

            wrapper.appendChild(img);
            wrapper.appendChild(label);
            wrapper.addEventListener('click', () => openBuildingClueModal(obj, npcId));
            if (container) container.appendChild(wrapper);
        });
    }

    // Kapı gıcırtısı + kapanma sesi
    playSound(doorCreak, 0.7);
    setTimeout(() => {
        playSound(doorClose, 0.5);
    }, 600);

    // No double transition since video provides it
    townMapScreen.classList.add('hidden');
    interiorScreen.classList.remove('hidden');

    // Çetin binaya girişte konuşsun
    const buildingName = npc.building;
    setTimeout(() => triggerHelperMessage('building_enter', buildingName, false), 1500);
    logAction('enter_building', npcId, npc.building);
}

// =============================================================
// 3. CLUE INSPECTION (BİNA İÇİ KÜÇÜK MODAL & ADLİ LAB EKRANI)
// =============================================================

const buildingClueModal = document.getElementById('building-clue-modal');

// Bina İçindeki Küçük İnceleme Modalı (Scene Object Click inside Building)
function openBuildingClueModal(obj, npcId) {
    currentPendingObject = obj;

    document.getElementById('building-clue-title').textContent = obj.name;
    document.getElementById('building-clue-desc').textContent = obj.desc;
    document.getElementById('building-clue-img').src = obj.img;

    const takeBtn = document.getElementById('building-clue-take-btn');
    const isAlreadyInBag = currentBag.some(b => b.id === obj.id);
    if (takeBtn) {
        if (isAlreadyInBag) {
            takeBtn.disabled = true;
            takeBtn.innerHTML = '<i class="fa-solid fa-check"></i> Çantada Mevcut';
            takeBtn.style.opacity = '0.6';
            takeBtn.style.cursor = 'not-allowed';
        } else {
            takeBtn.disabled = false;
            takeBtn.innerHTML = '<i class="fa-solid fa-briefcase"></i> Çantaya Al';
            takeBtn.style.opacity = '1';
            takeBtn.style.cursor = 'pointer';
        }
    }

    if (buildingClueModal) buildingClueModal.classList.remove('hidden');
}

document.getElementById('building-clue-take-btn')?.addEventListener('click', () => {
    if (!currentPendingObject) return;

    // Kasaba delil kontrolü (Gölge Şehir'de sadece 1000+, Gizemli Kasaba'da <1000)
    const isGolge = (window.currentActiveTown === 'golge_sehir');
    if (isGolge && currentPendingObject.id < 1000) {
        showGlobalNotification('Uyarı', 'Bu delil başka bir kasabaya aittir.', true);
        buildingClueModal.classList.add('hidden');
        return;
    } else if (!isGolge && currentPendingObject.id >= 1000) {
        showGlobalNotification('Uyarı', 'Bu delil başka bir kasabaya aittir.', true);
        buildingClueModal.classList.add('hidden');
        return;
    }

    if (currentBag.some(b => b.id === currentPendingObject.id)) {
        showGlobalNotification('Bilgi', 'Bu delil zaten çantanızda bulunuyor.', false);
        buildingClueModal.classList.add('hidden');
        return;
    }

    if (currentBag.length >= MAX_BAG_SIZE) {
        showCinematicHelper('Amirim! Çantamız doldu. Maksimum 5 delil taşıyabiliriz. Önce Adli Tıbba göndermeliyiz!', false);
    } else {
        currentBag.push(currentPendingObject);
        window.currentBag = currentBag;
        logAction('collect_clue', currentPendingObject.id, currentPendingObject.name);
        saveGameState();
        checkAutopsyConditions();
        showCinematicHelper(`Harika Amirim! '${currentPendingObject.name}' delilini çantaya attık! (${currentBag.length}/${MAX_BAG_SIZE} delil).`, false);
    }
    if (buildingClueModal) buildingClueModal.classList.add('hidden');
});

document.getElementById('building-clue-leave-btn')?.addEventListener('click', () => {
    if (buildingClueModal) buildingClueModal.classList.add('hidden');
});
document.getElementById('building-clue-close-btn')?.addEventListener('click', () => {
    if (buildingClueModal) buildingClueModal.classList.add('hidden');
});

let isInspectStartedFromBuilding = false;

// Ana İnceleme Ekranı (Haritadan/Çantadan Erişilen Gerçek Büyüklükteki Lab İnceleme)
function openClueInspect(obj, npcId, fromBag = false) {
    // Gerçek bina içinde olma şartı: interiorScreen görünür VE townMapScreen gizli olmalıdır
    const isInsideBuilding = (interiorScreen && !interiorScreen.classList.contains('hidden')) && (townMapScreen && townMapScreen.classList.contains('hidden'));
    isInspectStartedFromBuilding = isInsideBuilding;

    if (isInsideBuilding && fromBag) {
        showGlobalNotification("İnceleme Hatası", "Bina içerisindeyken detaylı laboratuvar incelemesi yapamazsınız! Önce dışarıya (kasaba haritasına) çıkmalısınız.", true);
        showCinematicHelper("Amirim! Detaylı laboratuvar incelemesi yapmak için binalardan çıkıp kasaba haritasına dönmeliyiz.", false);
        return;
    }

    // Harita ekranındaysak (dışarıdaysak) activeNpcId'yi temizle ki kilitlenme/bug olmasın
    if (!isInsideBuilding) {
        activeNpcId = null;
        if (interiorScreen) interiorScreen.classList.add('hidden');
        if (townMapScreen) townMapScreen.classList.remove('hidden');
    }

    // Bina içindeyken sahnedeki nesneye tıklanırsa küçük bina içi modal açılır
    if (isInsideBuilding && !fromBag) {
        openBuildingClueModal(obj, npcId);
        return;
    }

    currentPendingObject = obj;
    let actualNpcId = npcId;
    if (!actualNpcId || (obj.id >= 1000 && actualNpcId < 100)) {
        actualNpcId = obj.id >= 1000 ? Math.floor(obj.id / 10) : (Math.floor((obj.id - 1) / 3) + 1);
    }
    const npc = NPC_DATA[actualNpcId] || (obj.id >= 1000 ? (NPC_DATA[101] || NPC_DATA[1]) : NPC_DATA[1]);

    document.getElementById('clue-inspect-title').textContent = obj.name;
    document.getElementById('clue-inspect-desc').textContent = obj.desc;
    document.getElementById('clue-inspect-img').src = obj.img;
    const bgImage = npc ? (npc.talkBg || npc.bg) : '';
    document.getElementById('clue-inspect-bg').style.backgroundImage = bgImage ? `url('${bgImage}')` : 'none';

    // Okuma Modu Butonunu Göster/Gizle
    const readBtn = document.getElementById('clue-read-btn');
    if (readBtn) {
        if ([2, 5, 7, 9, 11, 15, 1012, 1024, 1041, 1052, 1061, 1062, 1072, 1074].includes(obj.id)) {
            readBtn.classList.remove('hidden');
        } else {
            readBtn.classList.add('hidden');
        }
    }

    // Yeşil İpucu Mesaj Kutusu ve Ayrı Ayrı Adli Tıbba Gönder Butonlarını Sıfırla
    const findingBox = document.getElementById('forensic-finding-box');
    const bloodItem = document.getElementById('forensic-blood-item');
    const printItem = document.getElementById('forensic-fingerprint-item');
    const sendBloodBtn = document.getElementById('send-blood-btn');
    const sendPrintBtn = document.getElementById('send-fingerprint-btn');

    if (findingBox) findingBox.classList.add('hidden');
    if (bloodItem) bloodItem.classList.add('hidden');
    if (printItem) printItem.classList.add('hidden');

    if (sendBloodBtn) {
        if (submittedBloodClueIds.has(obj.id)) {
            sendBloodBtn.disabled = true;
            sendBloodBtn.innerHTML = '<i class="fa-solid fa-check"></i> KAN LEKESİ ADLİ TIBBA GÖNDERİLDİ';
            sendBloodBtn.style.opacity = '0.6';
            sendBloodBtn.style.cursor = 'not-allowed';
        } else {
            sendBloodBtn.disabled = false;
            sendBloodBtn.innerHTML = '<i class="fa-solid fa-paper-plane"></i> KAN LEKESİNİ ADLİ TIBBA GÖNDER';
            sendBloodBtn.style.opacity = '1';
            sendBloodBtn.style.cursor = 'pointer';
        }
    }
    if (sendPrintBtn) {
        if (submittedPrintClueIds.has(obj.id)) {
            sendPrintBtn.disabled = true;
            sendPrintBtn.innerHTML = '<i class="fa-solid fa-check"></i> PARMAK İZİ ADLİ TIBBA GÖNDERİLDİ';
            sendPrintBtn.style.opacity = '0.6';
            sendPrintBtn.style.cursor = 'not-allowed';
        } else {
            sendPrintBtn.disabled = false;
            sendPrintBtn.innerHTML = '<i class="fa-solid fa-paper-plane"></i> PARMAK İZİNİ ADLİ TIBBA GÖNDER';
            sendPrintBtn.style.opacity = '1';
            sendPrintBtn.style.cursor = 'pointer';
        }
    }

    window.currentBloodFindingText = "";
    window.currentFingerprintFindingText = "";

    // Fetch dynamic forensic state
    fetch('/api/game/forensic-state')
        .then(res => res.json())
        .then(data => {
            if (data.success) window.currentForensicState = data;
        }).catch(err => console.error("Forensic state error:", err));

    // Araçları sıfırla
    if (window.resetForensicTools) window.resetForensicTools();

    const takeBtn = document.getElementById('clue-take-btn');
    const isAlreadyInBag = currentBag.some(b => b.id === obj.id);
    if (takeBtn) {
        if (isAlreadyInBag) {
            takeBtn.disabled = true;
            takeBtn.innerHTML = '<i class="fa-solid fa-check"></i> Çantada Mevcut';
            takeBtn.style.opacity = '0.6';
            takeBtn.style.cursor = 'not-allowed';
        } else {
            takeBtn.disabled = false;
            takeBtn.innerHTML = '<i class="fa-solid fa-briefcase"></i> Çantaya Al';
            takeBtn.style.opacity = '1';
            takeBtn.style.cursor = 'pointer';
        }
    }

    const forensicTools = document.querySelector('.forensic-tools');
    const thumbsContainer = document.getElementById('clue-thumbnails');
    if (forensicTools) {
        forensicTools.style.display = 'flex';
    }
    if (thumbsContainer && window.populateThumbnails) {
        window.populateThumbnails();
    }

    clueInspectModal.classList.remove('hidden');
}

document.getElementById('clue-take-btn').addEventListener('click', () => {
    if (!currentPendingObject) return;

    // Kasaba delil kontrolü (Gölge Şehir'de sadece 1000+, Gizemli Kasaba'da <1000)
    const isGolge = (window.currentActiveTown === 'golge_sehir');
    if (isGolge && currentPendingObject.id < 1000) {
        showGlobalNotification('Uyarı', 'Bu delil başka bir kasabaya aittir.', true);
        clueInspectModal.classList.add('hidden');
        return;
    } else if (!isGolge && currentPendingObject.id >= 1000) {
        showGlobalNotification('Uyarı', 'Bu delil başka bir kasabaya aittir.', true);
        clueInspectModal.classList.add('hidden');
        return;
    }

    if (currentBag.some(b => b.id === currentPendingObject.id)) {
        showGlobalNotification('Bilgi', 'Bu delil zaten çantanızda bulunuyor.', false);
        clueInspectModal.classList.add('hidden');
        return;
    }

    if (currentBag.length >= MAX_BAG_SIZE) {
        showCinematicHelper('Amirim! Çantamız doldu. Maksimum 5 delil taşıyabiliriz. Önce Adli Tıbba göndermeliyiz!', false);
    } else {
        currentBag.push(currentPendingObject);
        window.currentBag = currentBag;
        logAction('collect_clue', currentPendingObject.id, currentPendingObject.name);
        saveGameState();
        checkAutopsyConditions();
        showCinematicHelper(`Harika Amirim! '${currentPendingObject.name}' delilini çantaya attık! (${currentBag.length}/${MAX_BAG_SIZE} delil). Şüphelileri sorgularken bu delili ipucu olarak kullanabiliriz.`, false);
    }
    closeClueInspectAndReturn();
});

function closeClueInspectAndReturn() {
    if (clueInspectModal) clueInspectModal.classList.add('hidden');
    document.getElementById('document-reader-overlay')?.classList.add('hidden');
    if (window.resetForensicTools) window.resetForensicTools();

    if (!isInspectStartedFromBuilding) {
        activeNpcId = null;
        if (interiorScreen) interiorScreen.classList.add('hidden');
        if (townMapScreen) townMapScreen.classList.remove('hidden');
    } else {
        if (interiorScreen) interiorScreen.classList.remove('hidden');
    }
}

document.getElementById('clue-leave-btn').addEventListener('click', () => {
    closeClueInspectAndReturn();
});

document.getElementById('clue-read-btn')?.addEventListener('click', () => {
    if (!currentPendingObject) return;
    const overlay = document.getElementById('document-reader-overlay');
    const content = document.getElementById('document-reader-content');
    if (!overlay || !content) return;

    let text = "";
    content.className = "document-reader-content"; // reset class

    switch (currentPendingObject.id) {
        case 7: // Tehdit Mektubu
            content.classList.add('threat-letter');
            content.innerHTML = `
                <div class="letter-header">T.C. KASABA MUHTARLIĞI RESMİ NOTU</div>
                <div class="letter-body">
                    <p>Osman Bey,</p>
                    <p>Karanlık sırlar sonsuza dek gizli kalmaz. Kasaba arazilerini devretmeyi reddetmenin ağır bir bedeli olacak.</p>
                    <p>Zamanın doldu. Bu gece yaptıklarının hesabını vereceksin.</p>
                    <div class="letter-signature">Ecelin...</div>
                </div>
            `;
            break;
        case 15: // Gizli Cep Notu (Başlıksız Gizli Not)
            content.classList.add('secret-note');
            content.innerHTML = `
                <p>Osman, bugün hava kararınca dükkânıma gel konuşalım. Kimseye görünme, eski defterleri kapatacağız.</p>
                <div class="secret-note-sig">- Y.</div>
            `;
            break;
        case 2: // Kara Kaplı Defter (Açık İki Sayfalı Veresiye Defteri)
            content.classList.add('black-book');
            content.innerHTML = `
                <div class="open-notebook-wrapper">
                    <div class="notebook-spine"></div>
                    <div class="notebook-page left-page">
                        <div class="notebook-title">VERESİYE & HESAPLAR</div>
                        <div class="notebook-date">1994 Güz Tahsilatları</div>
                        <div class="ledger-row">
                            <span class="ledger-name">Bakkal Rıza:</span>
                            <span class="ledger-val">15.000 TL</span>
                        </div>
                        <div class="ledger-row">
                            <span class="ledger-name">Manav Hasan:</span>
                            <span class="ledger-val">4.200 TL</span>
                        </div>
                        <div class="ledger-row">
                            <span class="ledger-name">Terzi Yahya:</span>
                            <span class="ledger-val">-20.000 TL (Borçlu)</span>
                        </div>
                        <div class="ledger-row strike-row">
                            <span class="ledger-name victim-strike">Osman Bey:</span>
                            <span class="ledger-val victim-strike-val">150.000 TL</span>
                            <div class="hand-strike-line"></div>
                        </div>
                        <div class="ledger-row">
                            <span class="ledger-name">Eczacı Selma:</span>
                            <span class="ledger-val">8.500 TL</span>
                        </div>
                    </div>
                    <div class="notebook-page right-page">
                        <div class="notebook-title">SÖZLÜ ANLAŞMALAR</div>
                        <div class="handwritten-note">• Osman Bey borcunu her soruşumda erteleyip alay ediyor. Sabrım kalmadı.</div>
                        <div class="handwritten-note">• Bu borç tahsil edilmezse dükkânı batıracağım. O gece son kez hesaba kapatmaya gideceğim.</div>
                        <div class="notebook-stamp">TAHSİLAT BEKLİYOR</div>
                    </div>
                </div>
            `;
            break;
        case 5: // Reçete Defteri (Yırtık Kağıt Görseli Üzerinde Kesik Yazı)
            content.classList.add('prescription');
            content.innerHTML = `
                <div class="prescription-header">
                    <h3>SAĞLIK OCAĞI REÇETE VE İLAÇ KAYDI</h3>
                    <p>Tarih: 12 Kasım 1994 | Protokol No: #904</p>
                </div>
                <div class="prescription-body">
                    <p><strong>Hasta Adı:</strong> Osman Bey (Yaş: 54)</p>
                    <p><strong>Teşhis:</strong> Şiddetli Uykusuzluk & Göğüs Darlığı</p>
                    <hr>
                    <p><strong>Verilen İlaçlar:</strong></p>
                    <p>1. Diazepam 5mg — Günde 1 Adet (Gece)</p>
                    <p class="torn-edge-text">2. Bitkisel <span class="faded-torn-text">... [Yırtılmış / Silinmiş]</span></p>
                </div>
            `;
            break;
        case 9: // Tapu Senedi
            content.classList.add('title-deed');
            content.innerHTML = `
                <div class="deed-header">T.C. TAPU VE KADASTRO GENEL MÜDÜRLÜĞÜ</div>
                <div class="deed-title">ARAZİ DEVİR SENEDİ</div>
                <p><strong>Ada/Parsel:</strong> #404 - Kasaba Meydan Çarşısı</p>
                <p><strong>Mülk Sahibi:</strong> Osman Bey</p>
                <p><strong>Devredilen Taraf:</strong> Kasaba Muhtarlığı</p>
                <div class="deed-warning">[SAHTE İMZA — HÜKÜMSÜZDÜR]</div>
            `;
            break;
        case 11: // Gizli Dosya (Karakol Emniyet Dosyası)
            content.classList.add('confidential-file');
            content.innerHTML = `
                <div class="police-file-header">EMNİYET AMİRLİĞİ SORGULAMA DOSYASI #88</div>
                <p><strong>ŞÜPHELİ:</strong> Osman Bey</p>
                <p><strong>SUÇLAMA:</strong> Yasa Dışı İhbar & İrtikap</p>
                <p><strong>DURUM:</strong> Tahkikat komiser emriyle GİZLİ kategorisine alınmış ve durdurulmuştur.</p>
                <div class="police-file-sig">İmza: Komiser Güneş</div>
            `;
            break;
        case 1012: // Gece Kesim Defteri
            content.classList.add('golge-timber-book');
            content.innerHTML = `
                <div class="letter-header">T.C. ORMAN İŞLETMELERİ MÜDÜRLÜĞÜ GECE KAYDI</div>
                <div class="letter-body" style="font-family: var(--font-typewriter); padding: 15px;">
                    <p><strong>Tarih:</strong> 14 Kasım 1948 | <strong>Kayıt Sahibi:</strong> T. Tahsin</p>
                    <hr style="border-color: #5c3a21; margin: 12px 0;">
                    <p>• Ekrem Bey için gece vakti 15 metreküp özel reçineli çam kereste kesimi yapıldı.</p>
                    <p style="color: #8b0000; font-weight: bold; margin-top: 10px;">• Not: Ekrem Bey ormandaki yasa dışı ağaç kesimlerimi ihbar etmekle tehdit ediyor. Eğer savcılığa giderse hayatımı karartacak. Bu gece çam ormanının girişinde buluşup bu işi sonsuza dek bitireceğiz.</p>
                </div>
            `;
            break;
        case 1024: // Pirinç Kantar Notu
            content.classList.add('threat-letter');
            content.innerHTML = `
                <div class="letter-header">ALACAK VE İPOTEK SENEDİ #92</div>
                <div class="letter-body" style="font-family: var(--font-typewriter); padding: 10px;">
                    <p><strong>Alacaklı:</strong> Tüccar Ekrem Bey | <strong>Borçlu:</strong> Manav Ayşe</p>
                    <hr>
                    <p>Manav dükkânına karşılık verilen 15.000 Liralık borcun ödeme vadesi geçmiştir. 15 Kasım gününe kadar nakit ödeme yapılmadığı takdirde dükkân binası ve tezgâhlar haczedilerek mülkiyeti Ekrem Bey'e devredilecektir.</p>
                    <div style="font-style: italic; margin-top: 15px;">"Bu dükkân benim her şeyim, Ekrem'in burayı almasına asla izin vermeyeceğim..." - Ayşe'nin el yazısı notu.</div>
                </div>
            `;
            break;
        case 1041: // Kırmızı Çizgili Veresiye Defteri
            content.classList.add('black-book');
            content.innerHTML = `
                <div class="open-notebook-wrapper">
                    <div class="notebook-spine"></div>
                    <div class="notebook-page left-page">
                        <div class="notebook-title">GÖLGE BAKKAL VERESİYE</div>
                        <div class="notebook-date">1948 Kasım Kayıtları</div>
                        <div class="ledger-row">
                            <span class="ledger-name">Demirci Kazım:</span>
                            <span class="ledger-val">250 Lira</span>
                        </div>
                        <div class="ledger-row">
                            <span class="ledger-name">Kunduracı Rasim:</span>
                            <span class="ledger-val">420 Lira</span>
                        </div>
                        <div class="ledger-row strike-row">
                            <span class="ledger-name victim-strike" style="color:#ef4444;">Tüccar Ekrem Bey:</span>
                            <span class="ledger-val victim-strike-val">35.000 Lira</span>
                            <div class="hand-strike-line"></div>
                        </div>
                    </div>
                    <div class="notebook-page right-page">
                        <div class="notebook-title">SÖZLÜ ANLAŞMALAR</div>
                        <div class="handwritten-note">• Ekrem borcunu ödemediği gibi 'Beni borçla mı korkutacaksın Naciye, yarın zabıtayı kapına yollarım' diye beni tehdit etti.</div>
                        <div class="handwritten-note">• Bakkalımı batırmasına izin vermeyeceğim. Hekimden aldığım fare zehrini Ekrem'in tütününe karıştıracağım. Hak ettiğini bulacak.</div>
                        <div class="notebook-stamp" style="border-color:#ef4444; color:#ef4444;">KARA LİSTE</div>
                    </div>
                </div>
            `;
            break;
        case 1052: // Yırtık Reçete Sayfası
            content.classList.add('prescription');
            content.innerHTML = `
                <div class="prescription-header">
                    <h3>GÖLGE ŞEHİR HEKİMLİĞİ REÇETE KAYDI</h3>
                    <p>Tarih: 14 Kasım 1948 | Protokol No: #201</p>
                </div>
                <div class="prescription-body">
                    <p><strong>Hasta Adı:</strong> Ekrem Bey (Yaş: 58)</p>
                    <p><strong>Teşhis:</strong> İleri Derece Kalp Spazmı & Uykusuzluk</p>
                    <hr>
                    <p><strong>Verilen İlaçlar:</strong></p>
                    <p>1. Diazepam 5mg — Günde 1 Adet (Gece)</p>
                    <p class="torn-edge-text" style="color:#ef4444; font-weight:bold;">2. Özel İksir: Banotu Kökü Özü (3cc Lethal Doz) ... [YIRTILMIŞ / KESİLMİŞ]</p>
                </div>
            `;
            break;
        case 1061: // Resmi Kasaba Mührü ve Gizli Mektup
            content.classList.add('threat-letter');
            content.innerHTML = `
                <div class="letter-header">T.C. GÖLGE ŞEHİR MUHTARLIĞI MÜHÜRLÜ NOTU</div>
                <div class="letter-body" style="font-family: var(--font-typewriter); padding: 10px;">
                    <p>Ekrem Bey,</p>
                    <p>Çam Ormanı arazilerine ait tapu devir senetlerinin sahte olduğunu bildiğinizi biliyorum. Bu konuyu savcılığa taşırsanız muhtarlık makamımı ve kasabadaki tüm nüfuzumu kaybederim.</p>
                    <p>Bu gece yarısı ofisime gelin, bu konuyu kendi yöntemlerimizle tatlıya bağlayalım. Aksi takdirde sonuçlarına katlanırsınız.</p>
                    <div class="letter-signature">Muhtar Cevdet</div>
                </div>
            `;
            break;
        case 1062: // Sahte Gölge Şehir Tapusu
            content.classList.add('title-deed');
            content.innerHTML = `
                <div class="deed-header">T.C. TAPU VE KADASTRO GENEL MÜDÜRLÜĞÜ</div>
                <div class="deed-title">ARAZİ DEVİR SENEDİ</div>
                <p><strong>Bölge:</strong> #201 - Gölge Şehir Çam Ormanı Kuzey Parseli</p>
                <p><strong>Mülk Sahibi:</strong> Tüccar Ekrem Bey</p>
                <p><strong>Devredilen Taraf:</strong> Muhtar Cevdet (Gölge Şehir Muhtarlığı)</p>
                <div class="deed-warning" style="border-color:#ef4444; color:#ef4444;">[SAHTE İMZA VE YALAN BEYAN — GEÇERSİZDİR]</div>
            `;
            break;
        case 1072: // Tehdit İçerikli Eski Mektup
            content.classList.add('threat-letter');
            content.innerHTML = `
                <div class="letter-header">KİŞİSEL UYARI NOTU</div>
                <div class="letter-body" style="font-family: var(--font-typewriter); padding: 10px;">
                    <p>Fehmi Muallim,</p>
                    <p>Babanızdan kalan o antika altın köstekli saati bir daha asla göremeyeceksiniz. O saat artık borcunuza karşılık benim kasamda kilitli duruyor. Boşuna jandarmaya gidip şikayet etmeyin, bölge komutanı benim tarafımda.</p>
                    <div class="letter-signature">Ekrem</div>
                </div>
            `;
            break;
        case 1074: // Altı Çizili Eski Kitap
            content.classList.add('confidential-file');
            content.innerHTML = `
                <div class="police-file-header" style="background:#5b21b6;">"GECENİN SIRLARI" ROMAN SAYFASI</div>
                <div class="letter-body" style="font-family: var(--font-typewriter); padding: 15px;">
                    <p style="font-style: italic; line-height: 1.8;">"...ve saat tam 02:14'ü gösterdiğinde, fırtınanın ve gök gürültüsünün ortasında, göl kenarındaki o eski evden boğuk bir çığlık duyuldu. Katil, parmak uçlarında yürüyerek gecenin karanlığına karıştı..."</p>
                    <hr>
                    <p style="color:#f59e0b; font-weight:bold; margin-top: 15px;">[Fehmi Bey'in kurşun kalemle düştüğü el yazısı not]:</p>
                    <p style="font-style: italic; color:#fef3c7;">"O gece tam saat 02:14'te penceremin hemen altından geçen o sert çizme seslerini duydum. Adımlar göl kenarındaki deri atölyesine doğru ilerliyordu."</p>
                </div>
            `;
            break;
    }
    overlay.classList.remove('hidden');
});

document.getElementById('document-reader-close')?.addEventListener('click', () => {
    document.getElementById('document-reader-overlay')?.classList.add('hidden');
});

// =============================================================
// 3.5 FORENSIC TOOLS & 3D TILT (BÜYÜTEÇ, UV, TOZ VE ADLİ TIP)
// =============================================================

let activeForensicTool = null;
let isDrawing = false;
let lastX = 0, lastY = 0;


const submittedBloodClueIds = new Set();
const submittedPrintClueIds = new Set();

function triggerForensicFinding(findingText, findingType) {
    const findingBox = document.getElementById('forensic-finding-box');
    if (!findingBox) return;

    findingBox.classList.remove('hidden');

    if (findingType === 'blood') {
        window.currentBloodFindingText = findingText;
        const bloodItem = document.getElementById('forensic-blood-item');
        const bloodText = document.getElementById('forensic-blood-text');
        const sendBloodBtn = document.getElementById('send-blood-btn');
        if (bloodText) bloodText.innerHTML = `<i class="fa-solid fa-droplet text-red"></i> ${findingText}`;
        if (sendBloodBtn) sendBloodBtn.classList.remove('hidden');
        if (bloodItem) bloodItem.classList.remove('hidden');
    } else if (findingType === 'blood_clean') {
        const bloodItem = document.getElementById('forensic-blood-item');
        const bloodText = document.getElementById('forensic-blood-text');
        const sendBloodBtn = document.getElementById('send-blood-btn');
        if (bloodText) bloodText.innerHTML = `<i class="fa-solid fa-shield-halved" style="color: #10b981;"></i> ${findingText}`;
        if (sendBloodBtn) sendBloodBtn.classList.add('hidden');
        if (bloodItem) bloodItem.classList.remove('hidden');
    } else if (findingType === 'fingerprint') {
        window.currentFingerprintFindingText = findingText;
        const printItem = document.getElementById('forensic-fingerprint-item');
        const printText = document.getElementById('forensic-fingerprint-text');
        const sendPrintBtn = document.getElementById('send-fingerprint-btn');
        if (printText) printText.innerHTML = `<i class="fa-solid fa-fingerprint text-cyan"></i> ${findingText}`;
        if (sendPrintBtn) sendPrintBtn.classList.remove('hidden');
        if (printItem) printItem.classList.remove('hidden');
    } else if (findingType === 'fingerprint_clean') {
        const printItem = document.getElementById('forensic-fingerprint-item');
        const printText = document.getElementById('forensic-fingerprint-text');
        const sendPrintBtn = document.getElementById('send-fingerprint-btn');
        if (printText) printText.innerHTML = `<i class="fa-solid fa-shield-halved" style="color: #10b981;"></i> ${findingText}`;
        if (sendPrintBtn) sendPrintBtn.classList.add('hidden');
        if (printItem) printItem.classList.remove('hidden');
    }
}

// Adli Tıbba Gönder Buton Dinleyicileri
document.addEventListener('DOMContentLoaded', () => {
    const sendBloodBtn = document.getElementById('send-blood-btn');
    const sendPrintBtn = document.getElementById('send-fingerprint-btn');

    if (sendBloodBtn) {
        sendBloodBtn.addEventListener('click', () => {
            if (!currentPendingObject || !window.currentBloodFindingText) return;
            if (submittedBloodClueIds.has(currentPendingObject.id)) return;

            fetch('/api/game/forensic/submit', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    ClueId: currentPendingObject.id,
                    ClueName: currentPendingObject.name,
                    FindingText: window.currentBloodFindingText
                })
            })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        submittedBloodClueIds.add(currentPendingObject.id);
                        sendBloodBtn.disabled = true;
                        sendBloodBtn.innerHTML = '<i class="fa-solid fa-check"></i> KAN LEKESİ ADLİ TIBBA GÖNDERİLDİ';
                        sendBloodBtn.style.opacity = '0.6';
                        if (window.currentActiveTown === 'golge_sehir') {
                            window.submittedForensicCountGolge++;
                        } else {
                            window.submittedForensicCountGizemli++;
                        }
                        checkAutopsyConditions();
                        showCinematicHelper(`Harika Amirim! '${currentPendingObject.name}' üzerindeki KAN LEKESİ bulgusunu Adli Tıp Merkezi'ne ilettim. Otopsi raporuna yeni detaylar eklendi!`, false);
                    }
                })
                .catch(err => console.error("Adli Tıp gönderme hatası:", err));
        });
    }

    if (sendPrintBtn) {
        sendPrintBtn.addEventListener('click', () => {
            if (!currentPendingObject || !window.currentFingerprintFindingText) return;
            if (submittedPrintClueIds.has(currentPendingObject.id)) return;

            fetch('/api/game/forensic/submit', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    ClueId: currentPendingObject.id,
                    ClueName: currentPendingObject.name,
                    FindingText: window.currentFingerprintFindingText
                })
            })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        submittedPrintClueIds.add(currentPendingObject.id);
                        sendPrintBtn.disabled = true;
                        sendPrintBtn.innerHTML = '<i class="fa-solid fa-check"></i> PARMAK İZİ ADLİ TIBBA GÖNDERİLDİ';
                        sendPrintBtn.style.opacity = '0.6';
                        if (window.currentActiveTown === 'golge_sehir') {
                            window.submittedForensicCountGolge++;
                        } else {
                            window.submittedForensicCountGizemli++;
                        }
                        checkAutopsyConditions();
                        showCinematicHelper(`Harika Amirim! '${currentPendingObject.name}' üzerindeki PARMAK İZİ bulgusunu Adli Tıp Merkezi'ne ilettim. Otopsi raporuna yeni detaylar eklendi!`, false);
                    }
                })
                .catch(err => console.error("Adli Tıp gönderme hatası:", err));
        });
    }
});

let dustAnimationFrame;

const realFingerprint = new Image();
realFingerprint.src = 'images/real_fingerprint.png?v=' + Date.now();

const realBloodStain = new Image();
realBloodStain.src = 'images/real_blood_stain.png?v=' + Date.now();

function setupForensicTools() {
    const uvBtn = document.getElementById('tool-uv-btn');
    const dustBtn = document.getElementById('tool-dust-btn');
    const magBtn = document.getElementById('tool-mag-btn');
    const uvCanvas = document.getElementById('clue-uv-canvas');
    const dustCanvas = document.getElementById('clue-dust-canvas');
    const visualArea = document.getElementById('clue-inspect-visual');
    const wrapper = document.getElementById('clue-3d-wrapper');
    const magnifierLens = document.getElementById('magnifier-lens');
    const imgEl = document.getElementById('clue-inspect-img');
    if (!uvBtn) return;

    let currentRotationIndex = 0;
    const angles = ['', '_right', '_back', '_left'];

    const syncCanvasSize = () => {
        const w = wrapper.offsetWidth || 400;
        const h = wrapper.offsetHeight || 400;
        [uvCanvas, dustCanvas].forEach(c => {
            if (c) {
                c.width = w;
                c.height = h;
            }
        });
    };

    window.resetForensicTools = function () {
        activeForensicTool = null;
        uvBtn.classList.remove('active');
        dustBtn.classList.remove('active');
        if (magBtn) magBtn.classList.remove('active');
        uvCanvas.classList.add('hidden');
        dustCanvas.classList.add('hidden');
        uvCanvas.classList.remove('active');
        dustCanvas.classList.remove('active');
        visualArea.style.cursor = 'default';
        visualArea.classList.remove('cursor-uv', 'cursor-dust', 'cursor-magnify');
        wrapper.style.transform = 'none';
        currentRotationIndex = 0;
        if (magnifierLens) magnifierLens.style.display = 'none';

        const rotateControls = document.getElementById('clue-rotate-controls');
        if (rotateControls) {
            rotateControls.classList.remove('hidden');
        }
    };

    const updateRotationImage = () => {
        if (!currentPendingObject) return;
        wrapper.style.transform = 'none';

        const imgPath = currentPendingObject.img;
        const ext = imgPath.toLowerCase().endsWith('.jpg') ? '.jpg' : '.png';
        const baseSrc = imgPath.substring(0, imgPath.length - ext.length);
        const suffix = angles[currentRotationIndex];

        imgEl.src = `${baseSrc}${suffix}${ext}`;

        syncCanvasSize();
        uvCanvas.getContext('2d').clearRect(0, 0, uvCanvas.width, uvCanvas.height);
        dustCanvas.getContext('2d').clearRect(0, 0, dustCanvas.width, dustCanvas.height);
        if (activeForensicTool === 'uv') uvBtn.click();
    };

    window.populateThumbnails = () => {
        const thumbsContainer = document.getElementById('clue-thumbnails');
        if (!thumbsContainer || !currentPendingObject) return;
        thumbsContainer.innerHTML = '';
        thumbsContainer.classList.remove('hidden');

        const imgPath = currentPendingObject.img;
        const ext = imgPath.toLowerCase().endsWith('.jpg') ? '.jpg' : '.png';
        const baseSrc = imgPath.substring(0, imgPath.length - ext.length);

        angles.forEach((suffix, index) => {
            const thumbImg = document.createElement('img');
            thumbImg.src = `${baseSrc}${suffix}${ext}`;
            thumbImg.className = 'clue-thumbnail';

            if (index === currentRotationIndex) thumbImg.classList.add('active');

            thumbImg.addEventListener('click', () => {
                currentRotationIndex = index;
                updateRotationImage();
                document.querySelectorAll('.clue-thumbnail').forEach(t => t.classList.remove('active'));
                thumbImg.classList.add('active');
            });

            thumbsContainer.appendChild(thumbImg);
        });
    };

    uvBtn.addEventListener('click', () => {
        resetForensicTools();
        activeForensicTool = 'uv';
        uvBtn.classList.add('active');
        uvCanvas.classList.remove('hidden');
        uvCanvas.classList.add('active');
        visualArea.classList.add('cursor-uv');

        realBloodStain.src = 'images/real_blood_stain.png?v=' + Date.now();

        syncCanvasSize();
        const ctx = uvCanvas.getContext('2d');
        ctx.fillStyle = 'rgba(8, 5, 20, 0.92)';
        ctx.fillRect(0, 0, uvCanvas.width, uvCanvas.height);
    });

    dustBtn.addEventListener('click', () => {
        resetForensicTools();
        activeForensicTool = 'dust';
        dustBtn.classList.add('active');
        dustCanvas.classList.remove('hidden');
        dustCanvas.classList.add('active');
        visualArea.classList.add('cursor-dust');

        realFingerprint.src = 'images/real_fingerprint.png?v=' + Date.now();

        syncCanvasSize();
        const ctx = dustCanvas.getContext('2d');
        ctx.clearRect(0, 0, dustCanvas.width, dustCanvas.height);
    });

    if (magBtn) {
        magBtn.addEventListener('click', () => {
            resetForensicTools();
            activeForensicTool = 'magnify';
            magBtn.classList.add('active');
            visualArea.classList.add('cursor-magnify');
        });
    }

    // Interactive handling for tools
    const handleToolInteraction = (e) => {
        if (!activeForensicTool) return;

        const wrapperRect = wrapper.getBoundingClientRect();
        const x = e.clientX - wrapperRect.left;
        const y = e.clientY - wrapperRect.top;

        if (activeForensicTool === 'magnify') {
            const imgRect = imgEl.getBoundingClientRect();
            const containerRect = visualArea.getBoundingClientRect();
            const relX = e.clientX - imgRect.left;
            const relY = e.clientY - imgRect.top;

            if (magnifierLens && relX >= -30 && relX <= imgRect.width + 30 && relY >= -30 && relY <= imgRect.height + 30) {
                magnifierLens.style.display = 'block';
                const zoom = 2.4;
                const lensSize = 220;
                magnifierLens.style.width = lensSize + 'px';
                magnifierLens.style.height = lensSize + 'px';

                magnifierLens.style.left = (e.clientX - containerRect.left - lensSize / 2) + 'px';
                magnifierLens.style.top = (e.clientY - containerRect.top - lensSize / 2) + 'px';

                magnifierLens.style.backgroundImage = `url('${imgEl.src}')`;
                magnifierLens.style.backgroundSize = `${imgRect.width * zoom}px ${imgRect.height * zoom}px`;
                magnifierLens.style.backgroundPosition = `-${relX * zoom - lensSize / 2}px -${relY * zoom - lensSize / 2}px`;
            } else if (magnifierLens) {
                magnifierLens.style.display = 'none';
            }
        } else if (activeForensicTool === 'uv') {
            const ctx = uvCanvas.getContext('2d');
            ctx.globalCompositeOperation = 'source-over';
            ctx.fillStyle = 'rgba(8, 5, 20, 0.92)';
            ctx.fillRect(0, 0, uvCanvas.width, uvCanvas.height);

            // Spotlight hole
            ctx.globalCompositeOperation = 'destination-out';
            const grad = ctx.createRadialGradient(x, y, 15, x, y, 130);
            grad.addColorStop(0, 'rgba(0,0,0,1)');
            grad.addColorStop(1, 'rgba(0,0,0,0)');
            ctx.fillStyle = grad;
            ctx.beginPath();
            ctx.arc(x, y, 130, 0, Math.PI * 2);
            ctx.fill();

            // Neon purple ring
            ctx.globalCompositeOperation = 'source-over';
            const purpleGrad = ctx.createRadialGradient(x, y, 15, x, y, 130);
            purpleGrad.addColorStop(0, 'rgba(192, 38, 211, 0.55)');
            purpleGrad.addColorStop(1, 'rgba(192, 38, 211, 0)');
            ctx.fillStyle = purpleGrad;
            ctx.beginPath();
            ctx.arc(x, y, 130, 0, Math.PI * 2);
            ctx.fill();

            // Kan lekesi gösterimi — SADECE UV ışığının fener hüzmesi (130px) lekenin TAM üstüne gelirse göster!
            if (currentPendingObject) {
                if (currentPendingObject.bloodSpot) {
                    const spot = currentPendingObject.bloodSpot;
                    const targetAngle = spot.angle !== undefined ? spot.angle : 0;

                    if (currentRotationIndex === targetAngle) {
                        const imgRect = imgEl.getBoundingClientRect();
                        const imgOffsetLeft = imgRect.left - wrapperRect.left;
                        const imgOffsetTop = imgRect.top - wrapperRect.top;
                        const cx = imgOffsetLeft + imgRect.width * spot.xRatio;
                        const cy = imgOffsetTop + imgRect.height * spot.yRatio;

                        const dist = Math.hypot(x - cx, y - cy);
                        if (dist < 130) {
                            // Nesne boyutuyla tam orantılı dinamik leke boyutu
                            const bloodSize = Math.min(72, Math.max(38, Math.min(imgRect.width, imgRect.height) * 0.24));
                            const halfBlood = bloodSize / 2;

                            ctx.save();
                            ctx.globalCompositeOperation = 'source-over';
                            ctx.globalAlpha = Math.min(1.0, (130 - dist) / 60 + 0.4);
                            ctx.shadowColor = 'transparent';
                            ctx.shadowBlur = 0;
                            ctx.drawImage(realBloodStain, cx - halfBlood, cy - halfBlood, bloodSize, bloodSize);
                            ctx.restore();

                            triggerForensicFinding(`KAN LEKESİ TESPİT EDİLDİ! (Biyolojik kan lekesi örneği izole edildi)`, 'blood');
                        }
                    }
                } else {
                    triggerForensicFinding(`TEMİZ: Nesne üzerinde UV altında tespit edilen bir kan veya biyolojik leke bulunmamaktadır.`, 'blood_clean');
                }
            }
        } else if (activeForensicTool === 'dust') {
            const ctx = dustCanvas.getContext('2d');
            ctx.fillStyle = 'rgba(25, 25, 30, 0.85)';
            ctx.shadowColor = 'transparent';
            ctx.shadowBlur = 0;

            for (let i = 0; i < 18; i++) {
                const rx = (Math.random() - 0.5) * 36;
                const ry = (Math.random() - 0.5) * 36;
                ctx.beginPath();
                ctx.arc(x + rx, y + ry, Math.random() * 2.5 + 1, 0, Math.PI * 2);
                ctx.fill();
            }

            // Parmak izi gösterimi — SADECE tozlama fırçası TAM lekenin üstüne sürülürse göster!
            if (currentPendingObject) {
                if (currentPendingObject.fingerprintSpot) {
                    const spot = currentPendingObject.fingerprintSpot;
                    const targetAngle = spot.angle !== undefined ? spot.angle : 0;

                    if (currentRotationIndex === targetAngle) {
                        const imgRect = imgEl.getBoundingClientRect();
                        const imgOffsetLeft = imgRect.left - wrapperRect.left;
                        const imgOffsetTop = imgRect.top - wrapperRect.top;
                        const cx = imgOffsetLeft + imgRect.width * spot.xRatio;
                        const cy = imgOffsetTop + imgRect.height * spot.yRatio;

                        const dist = Math.hypot(x - cx, y - cy);
                        if (dist < 110) {
                            // Nesne boyutuyla tam orantılı dinamik parmak izi boyutu
                            const printSize = Math.min(60, Math.max(32, Math.min(imgRect.width, imgRect.height) * 0.20));
                            const halfPrint = printSize / 2;

                            ctx.save();
                            ctx.globalCompositeOperation = 'source-over';
                            ctx.globalAlpha = 0.95;
                            ctx.shadowColor = 'transparent';
                            ctx.shadowBlur = 0;
                            ctx.drawImage(realFingerprint, cx - halfPrint, cy - halfPrint, printSize, printSize);
                            ctx.restore();
                            triggerForensicFinding(`PARMAK İZİ BULUNDU! (Yüzey üstü daktilografik iz numunesi izole edildi)`, 'fingerprint');
                        }
                    }
                } else {
                    triggerForensicFinding(`TEMİZ: Nesne yüzeyinde adli incelemeye elverişli bir parmak izi bulunmamaktadır.`, 'fingerprint_clean');
                }
            }
        }
    };

    visualArea.addEventListener('mousemove', handleToolInteraction);
    visualArea.addEventListener('mousedown', (e) => {
        isDrawing = true;
        handleToolInteraction(e);
    });
    visualArea.addEventListener('mouseup', () => isDrawing = false);
}
document.addEventListener('DOMContentLoaded', setupForensicTools);

// =============================================================
// 4. LEAVE BUILDING (WARNING)
// =============================================================

document.getElementById('leave-building-btn').addEventListener('click', () => {
    const currentNpcId = parseInt(window.activeNpcId || activeNpcId || interiorScreen?.getAttribute('data-npc-id'), 10);
    const objects = SCENE_OBJECTS[currentNpcId] || [];
    const collectedInBag = currentBag.map(c => c.id);
    const uncollectedCount = objects.filter(o => !collectedInBag.includes(o.id)).length;
    const hasTalked = dialogHistory[currentNpcId] && dialogHistory[currentNpcId].length > 0;

    const warningTextEl = document.querySelector('.exit-warning-card p');
    if (warningTextEl) {
        let details = "Bu binadan çıkarsanız inceleme tamamlanacak ve kapı kilitlenecektir.";
        if (uncollectedCount > 0) {
            details += `<br><span style="color:#ef4444; font-weight:bold;"><i class="fa-solid fa-triangle-exclamation"></i> Olay yerinde henüz almadığınız ${uncollectedCount} delil bulunuyor!</span>`;
        }
        if (!hasTalked) {
            details += `<br><span style="color:#f59e0b; font-weight:bold;"><i class="fa-solid fa-comment-slash"></i> Bu şüpheli ile henüz hiç konuşmadınız!</span>`;
        }
        warningTextEl.innerHTML = details;
    }
    exitWarningModal.classList.remove('hidden');
});

document.getElementById('exit-cancel-btn').addEventListener('click', () => {
    exitWarningModal.classList.add('hidden');
});

document.getElementById('exit-confirm-btn').addEventListener('click', () => {
    exitWarningModal.classList.add('hidden');

    // Aktif NPC ID'yi hem global değişkenlerden hem de DOM attribute'tan garantiye al
    const currentNpcId = parseInt(window.activeNpcId || activeNpcId || interiorScreen?.getAttribute('data-npc-id'), 10);

    // Binayı kilitle
    if (!isNaN(currentNpcId) && currentNpcId > 0) {
        visitedBuildings.add(currentNpcId);
        if (currentNpcId >= 100 && window.GolgeSehirEngine) {
            window.GolgeSehirEngine.visitedGolgeBuildings.add(currentNpcId);
            window.GolgeSehirEngine.updateGolgeSehirAutopsyUI();
        }

        const buildingEl = document.querySelector(`.map-building[data-npc-id="${currentNpcId}"]`);
        if (buildingEl) {
            buildingEl.classList.add('visited');
            const tag = buildingEl.querySelector('.building-hover-tag');
            if (tag) {
                tag.innerHTML = `<i class="fa-solid fa-lock"></i> İNCELEME TAMAMLANDI`;
            }
        }
    }
    triggerTransition(() => {
        interiorScreen.classList.add('hidden');
        if (window.currentActiveTown === 'golge_sehir' && window.GolgeSehirEngine) {
            const mapScreen = document.getElementById('town-map-screen');
            if (mapScreen) mapScreen.classList.remove('hidden');
            window.GolgeSehirEngine.applyGolgeSehirState();
            checkAutopsyConditions();
        } else if (window.currentActiveTown === 'sisoren' && window.SisorenEngine) {
            if (typeof window.SisorenEngine.clearInteriorMarkers === 'function') {
                window.SisorenEngine.clearInteriorMarkers();
            }
            const mapScreen = document.getElementById('town-map-screen');
            if (mapScreen) mapScreen.classList.remove('hidden');
            window.SisorenEngine.applySisorenState();
            if (typeof window.checkAutopsyConditions === 'function') {
                window.checkAutopsyConditions();
            }
        } else {
            townMapScreen.classList.remove('hidden');
            checkAutopsyConditions();
        }
        activeNpcId = null;
        window.activeNpcId = null;
    });
});

// =============================================================
// 5. NPC TALK — KADEMESİZ KARIŞIK SİSTEM
// =============================================================

document.getElementById('talk-npc-btn').addEventListener('click', () => {
    const currentNpcId = window.activeNpcId || activeNpcId;
    if (!currentNpcId) return;
    openNpcTalk(currentNpcId);
});

function openNpcTalk(npcId) {
    activeNpcId = npcId;
    window.activeNpcId = npcId;
    let npc = (window.NPC_DATA && window.NPC_DATA[npcId]) || NPC_DATA[npcId];
    if (!npc && npcId >= 100 && window.GOLGE_SEHIR_CONFIG && window.GOLGE_SEHIR_CONFIG.buildings) {
        const bld = window.GOLGE_SEHIR_CONFIG.buildings.find(b => b.npcId === npcId);
        if (bld) npc = bld.npc;
    }
    // Sisören ana NPC'leri
    if (!npc && npcId >= 200 && window.SISOREN_CONFIG && window.SISOREN_CONFIG.buildings) {
        const bld = window.SISOREN_CONFIG.buildings.find(b => b.npcId === npcId);
        if (bld) npc = bld.npc;
    }
    // Sisören ekstra NPC'leri (numericId ile)
    if (!npc && npcId >= 300 && window.SISOREN_CONFIG) {
        if (window.SISOREN_CONFIG.extraNpcs) {
            const extra = window.SISOREN_CONFIG.extraNpcs.find(e => e.numericId === npcId);
            if (extra) {
                npc = {
                    id: extra.numericId,
                    name: extra.name,
                    gender: extra.gender,
                    building: extra.buildingId || 'Sokak',
                    role: extra.role,
                    portrait: extra.portrait,
                    bg: null,
                    talkBg: null,
                    greeting: extra.greeting,
                    questions: extra.questions || [],
                    isExtra: true,
                    canBeGuilty: false
                };
                window.NPC_DATA[npcId] = npc;
            }
        }
        // Çocuk NPC'leri (315-318)
        if (!npc && window.SISOREN_CONFIG.buildings) {
            for (const bld of window.SISOREN_CONFIG.buildings) {
                if (bld.children) {
                    const ch = bld.children.find(c => c.numericId === npcId || c.id === npcId);
                    if (ch) {
                        npc = {
                            id: ch.numericId || ch.id,
                            name: ch.name,
                            gender: ch.gender,
                            building: bld.title,
                            role: ch.role,
                            portrait: ch.portrait || null,
                            bg: null,
                            talkBg: null,
                            greeting: ch.greeting,
                            questions: ch.questions || [],
                            isExtra: true,
                            canBeGuilty: false,
                            isChild: true
                        };
                        window.NPC_DATA[npcId] = npc;
                        break;
                    }
                }
            }
        }
    }
    if (!npc) return;

    // Ekstra NPC konuşulduğunda suçlama ekranına ekle
    if (npc.isExtra && npcId >= 300) {
        if (!window.sisorenTalkedExtraNpcs) window.sisorenTalkedExtraNpcs = new Set();
        window.sisorenTalkedExtraNpcs.add(npcId);
    }

    // Kural: Masum işaretlenen karakter sorgulanamaz/işlem yapılamaz
    if ((window.innocentNpcIds && window.innocentNpcIds.has(npcId)) || innocentNpcIds.has(npcId)) {
        showGlobalNotification("MASUM KARAKTER", `${npc.name} masum ilan edilmiştir ve dava dosyasından çıkarılmıştır. Kendisiyle tekrar sorgu yapılamaz!`, true);
        return;
    }

    // Konuşma ekranında NPC portresi kullanılır; bina iç sahnesi bu katmana taşınmaz.
    const talkBgImage = npc.talkBg || npc.bg;
    const talkContainer = document.querySelector('.npc-talk-container');
    const characterLayer = document.getElementById('npc-talk-character-layer');

    if (talkContainer) {
        talkContainer.style.backgroundImage = 'none';
        talkContainer.style.backgroundSize = 'contain';
        talkContainer.style.backgroundPosition = 'center';
        talkContainer.className = 'npc-talk-container';
    }

    if (characterLayer) {
        characterLayer.style.backgroundImage = (window.currentActiveTown === 'sisoren' && (npc.portrait || talkBgImage))
            ? `url('${npc.portrait || talkBgImage}?v=${Date.now()}')`
            : 'none';
        characterLayer.style.backgroundSize = 'contain';
        characterLayer.style.backgroundPosition = 'center';
        characterLayer.style.backgroundRepeat = 'no-repeat';
        characterLayer.className = 'npc-talk-character-layer';
    }

    const portraitImg = document.getElementById('npc-talk-portrait-img');
    if (portraitImg) {
        portraitImg.src = `${npc.portrait || talkBgImage || ''}?v=${Date.now()}`;
        portraitImg.alt = npc.name;
        portraitImg.style.display = npc.portrait || talkBgImage ? 'block' : 'none';
    }

    // Chat alanını temizle
    const chatArea = document.getElementById('npc-talk-chat');
    if (chatArea) chatArea.innerHTML = '';

    // NPC mırıltı sesi çal
    let mumble = (npcId == 2 || npcId == 4) ? mumbleFemale : mumbleMale;
    if (mumble) {
        mumble.currentTime = 0;
        playSound(mumble, 0.3);
        setTimeout(() => stopSound(mumble), 2000);
    }

    // Kalan soru sayısını güncelle
    updateQuestionIndicator(npcId);

    // Sıradaki mantıksal soruları yükle
    loadContextualQuestions(npcId);

    // Modal Elementini Görünür Yap (Garantili açılma için getElementById)
    const talkModal = document.getElementById('npc-talk-modal');
    if (talkModal) {
        talkModal.classList.remove('hidden');
        talkModal.style.display = 'block';
    }

    // Çetin NPC konuşma başlangıcında söylesin
    setTimeout(() => triggerHelperMessage('npc_talk', null, false), 800);
    logAction('ask_question', npcId, npc.name);
}

// Konuşma Ekranı Kapat Butonu
document.getElementById('npc-talk-close')?.addEventListener('click', () => {
    const talkModal = document.getElementById('npc-talk-modal');
    if (talkModal) {
        talkModal.classList.add('hidden');
        talkModal.style.display = 'none';
    }
});

function updateQuestionIndicator(npcId) {
    const asked = askedQuestionCount[npcId] || 0;
    const maxQuestions = 5;
    const remainingLimit = Math.max(0, maxQuestions - asked);
    const stageIndicator = document.getElementById('npc-talk-stage');
    if (stageIndicator) stageIndicator.textContent = `Kalan Soru Hakkı: ${remainingLimit}/${maxQuestions}`;

    const aiSection = document.querySelector('.npc-talk-ai-section');
    const btnContainer = document.getElementById('npc-talk-buttons');

    // Stres, Nabız ve Semptom Monitörü
    const stress = Math.min(100, Math.max(0, npcStressLevels[npcId] || 0));
    const stressFill = document.getElementById('npc-stress-fill');
    const stressPct = document.getElementById('npc-stress-pct');
    const bpmVal = document.getElementById('npc-bpm-val');
    const symptomTag = document.getElementById('npc-symptom-tag');
    const monitorContainer = document.getElementById('npc-vitals-monitor');
    const heartIcon = document.querySelector('.vitals-heart-icon');
    const ecgSvg = document.querySelector('.vitals-ecg-svg');
    const ecgLine = document.querySelector('.vitals-ecg-line');

    if (stressFill) {
        stressFill.style.width = stress + '%';
        stressFill.className = 'npc-stress-fill';
        if (stress >= 70) stressFill.classList.add('stress-fill-high');
        else if (stress >= 40) stressFill.classList.add('stress-fill-med');
    }
    if (stressPct) stressPct.textContent = stress + '%';

    // Dinamik BPM ve Nabız Hesabı (65 - 145 BPM arası)
    const currentBpm = Math.round(68 + (stress * 0.75));
    if (bpmVal) bpmVal.textContent = currentBpm;

    // Semptom ve EKG Animasyon Durumu
    if (monitorContainer) {
        monitorContainer.className = 'npc-stress-container';
        if (stress >= 75) monitorContainer.classList.add('stress-panic');
        else if (stress >= 40) monitorContainer.classList.add('stress-high');
    }

    if (heartIcon) {
        heartIcon.className = 'fa-solid fa-heart-pulse vitals-heart-icon';
        if (stress >= 75) heartIcon.classList.add('pulse-panic');
        else if (stress >= 40) heartIcon.classList.add('pulse-fast');
    }

    if (ecgSvg && ecgLine) {
        ecgSvg.className = 'vitals-ecg-svg';
        ecgLine.className = 'vitals-ecg-line';
        if (stress >= 75) {
            ecgSvg.classList.add('ecg-panic');
            ecgLine.classList.add('line-panic');
        } else if (stress >= 40) {
            ecgSvg.classList.add('ecg-fast');
            ecgLine.classList.add('line-nervous');
        }
    }

    if (symptomTag) {
        symptomTag.className = 'vitals-symptom-tag';
        if (stress >= 85) {
            symptomTag.classList.add('symptom-panic');
            symptomTag.innerHTML = '<i class="fa-solid fa-triangle-exclamation"></i> <span>Aşırı Panik / Terleme</span>';
        } else if (stress >= 60) {
            symptomTag.classList.add('symptom-panic');
            symptomTag.innerHTML = '<i class="fa-solid fa-droplet"></i> <span>Göz Kaçırma / Titreme</span>';
        } else if (stress >= 35) {
            symptomTag.classList.add('symptom-nervous');
            symptomTag.innerHTML = '<i class="fa-solid fa-face-frown-open"></i> <span>Tedirgin / Savunmacı</span>';
        } else {
            symptomTag.innerHTML = '<i class="fa-solid fa-face-meh"></i> <span>Sakin & Kontrollü</span>';
        }
    }

    if (stress >= 100) {
        if (aiSection) aiSection.style.display = 'none';
        if (btnContainer) {
            btnContainer.innerHTML = '<div class="npc-talk-end-msg" style="color:var(--danger);"><i class="fa-solid fa-triangle-exclamation"></i> Karakter aşırı stres ve panikten sorguyu terk etti! Artık onunla konuşamazsınız.</div>';
        }
        return;
    }

    if (remainingLimit <= 0) {
        if (aiSection) aiSection.style.display = 'none';
        if (btnContainer) {
            btnContainer.innerHTML = '<div class="npc-talk-end-msg"><i class="fa-solid fa-check-circle"></i> Sorgu tamamlandı. Bu NPC\'ye sorabileceğiniz soru kalmadı. (5/5)</div>';
        }
    } else {
        if (aiSection) aiSection.style.display = 'flex';
    }
}

function loadContextualQuestions(npcId) {
    const container = document.getElementById('npc-talk-buttons');
    if (!container) return;

    const stress = npcStressLevels[npcId] || 0;
    const askedCount = askedQuestionCount[npcId] || 0;
    const aiSection = document.querySelector('.npc-talk-ai-section');

    if (stress >= 100) {
        if (aiSection) aiSection.style.display = 'none';
        container.innerHTML = '<div class="npc-talk-end-msg" style="color:var(--danger);"><i class="fa-solid fa-triangle-exclamation"></i> Karakter öfkelendi ve sorguyu terk etti! Artık onunla konuşamazsınız.</div>';
        return;
    }

    const maxQuestions = 5;
    if (askedCount >= maxQuestions) {
        if (aiSection) aiSection.style.display = 'none';
        container.innerHTML = '<div class="npc-talk-end-msg"><i class="fa-solid fa-check-circle"></i> Sorgu tamamlandı. Bu NPC\'ye sorabileceğiniz soru kalmadı. (5/5)</div>';
        npcTalkCompleted[npcId] = true;
        return;
    }

    container.innerHTML = '<div style="color:var(--text-muted); text-align:center;">Diyaloglar yükleniyor...</div>';

    const categories = ['tanisma', 'derinlesme', 'yuzlestirme', 'baski', 'son'];
    const currentCategory = categories[askedCount] || 'son';

    // C# API'den diyalogları çek
    fetch(`/api/game/dialogues?npcId=${npcId}&category=${currentCategory}`)
        .then(res => res.json())
        .then(data => {
            // Asenkron istek dönerken stres veya soru limiti dolmuş olabilir
            const currentStress = npcStressLevels[npcId] || 0;
            const currentAsked = askedQuestionCount[npcId] || 0;
            const maxQuestions = 5;
            if (currentStress >= 100 || currentAsked >= maxQuestions) {
                updateQuestionIndicator(npcId);
                return;
            }

            let questionsToShow = [];
            if (data && data.success && Array.isArray(data.dialogues) && data.dialogues.length > 0) {
                questionsToShow = data.dialogues;
            } else if (npcId >= 200) {
                // Sisören Frontend Fallback (SISOREN_CONFIG içinden hazır soru havuzu)
                questionsToShow = getSisorenFallbackQuestions(npcId, askedCount);
            } else if (npcId >= 100) {
                // Gölge Şehir Frontend Fallback (GOLGE_SEHIR_CONFIG içinden 4'lü hazır soru havuzu)
                questionsToShow = getGolgeFallbackQuestions(npcId, askedCount);
            } else if (NPC_QUESTIONS[npcId]) {
                const pool = NPC_QUESTIONS[npcId];
                questionsToShow = pool.slice(0, 4);
            }

            if (questionsToShow && questionsToShow.length > 0) {
                if (npcId >= 200) questionsToShow = ensureFourSisorenQuestions(questionsToShow, npcId, askedCount);
                container.innerHTML = '';
                questionsToShow.forEach((q, index) => {
                    const btn = document.createElement('button');
                    btn.className = 'npc-talk-btn';
                    btn.innerHTML = `<i class="fa-regular fa-comment-dots"></i> ${q.q}`;

                    btn.dataset.question = JSON.stringify(q);
                    btn.onclick = () => { askQuestionBackend(npcId, q); };
                    container.appendChild(btn);
                });
            } else {
                container.innerHTML = '<div style="color:var(--text-muted); text-align:center;">Diyalog bulunamadı.</div>';
            }
        })
        .catch(err => {
            console.warn("Diyalog API uyarısı, yerel havuz kullanılıyor:", err);
            container.innerHTML = '';
            let fallbackQuestions = [];
            if (npcId >= 200) {
                fallbackQuestions = getSisorenFallbackQuestions(npcId, askedCount);
            } else if (npcId >= 100) {
                fallbackQuestions = getGolgeFallbackQuestions(npcId, askedCount);
            } else if (NPC_QUESTIONS[npcId]) {
                const pool = NPC_QUESTIONS[npcId];
                fallbackQuestions = pool.slice(0, 4);
            }

            if (fallbackQuestions && fallbackQuestions.length > 0) {
                if (npcId >= 200) fallbackQuestions = ensureFourSisorenQuestions(fallbackQuestions, npcId, askedCount);
                fallbackQuestions.forEach((q) => {
                    const btn = document.createElement('button');
                    btn.className = 'npc-talk-btn';
                    btn.innerHTML = `<i class="fa-regular fa-comment-dots"></i> ${q.q}`;
                    btn.dataset.question = JSON.stringify(q);
                    btn.onclick = () => { askQuestionBackend(npcId, q); };
                    container.appendChild(btn);
                });
            } else {
                container.innerHTML = '<div style="color:red;">Diyaloglar yüklenemedi!</div>';
            }

        });
}

function ensureFourSisorenQuestions(questions, npcId, askedCount) {
    const result = [...questions];
    const npc = window.NPC_DATA && window.NPC_DATA[npcId];
    while (result.length < 4) {
        const source = result[result.length % Math.max(1, result.length)] || {
            q: 'O gece başka hangi ayrıntıyı hatırlıyorsun?',
            a: 'Bu ayrıntıyı tam hatırlamıyorum amirim.',
            difficulty: 2,
            category: 'derinlesme'
        };
        result.push({
            ...source,
            q: `${source.q} (Ek ayrıntı ${askedCount + result.length + 1})`,
            a: source.a || `${npc ? npc.name : 'Tanık'} bu konuda kesin konuşmaktan kaçınıyor.`
        });
    }
    return result.slice(0, 4);
}

// Sisören için her aşamada zengin hazır soru üreten yardımcı fonksiyon
function getSisorenFallbackQuestions(npcId, askedCount) {
    // Ekstra NPC'ler (300+) için config'den soruları çek
    if (npcId >= 300 && window.SISOREN_CONFIG) {
        if (window.SISOREN_CONFIG.extraNpcs) {
            const extra = window.SISOREN_CONFIG.extraNpcs.find(e => e.numericId === npcId);
            if (extra && extra.questions) {
                const registered = window.NPC_DATA && window.NPC_DATA[npcId];
                const questions = registered && Array.isArray(registered.questions)
                    ? registered.questions
                    : extra.questions.map((q, index) => ({
                        q: typeof q === 'string' ? q : q.q,
                        a: typeof q === 'string'
                            ? `${extra.name} bu ayrıntıyı hatırlıyor: ${q}`
                            : q.a,
                        difficulty: index > 2 ? 2 : 1,
                        category: 'tanisma'
                    }));
                return questions.slice(askedCount, askedCount + 4);
            }
        }
        // Çocuk NPC'ler
        if (window.SISOREN_CONFIG.buildings) {
            for (const bld of window.SISOREN_CONFIG.buildings) {
                if (bld.children) {
                    const ch = bld.children.find(c => c.numericId === npcId || c.id === npcId);
                    if (ch && ch.questions) {
                        const registered = window.NPC_DATA && window.NPC_DATA[npcId];
                        const questions = registered && Array.isArray(registered.questions)
                            ? registered.questions
                            : ch.questions.map(qText => (typeof qText === 'string' ? {
                                q: qText,
                                a: ch.greeting || 'Bilmiyorum dedektif amca...',
                                difficulty: 1,
                                category: 'tanisma'
                            } : qText));
                        return questions.slice(askedCount, askedCount + 4);
                    }
                }
            }
        }
    }

    // Ana 13 şüpheli (201-213) için fallbackQuestions'dan çek
    if (window.SISOREN_CONFIG && window.SISOREN_CONFIG.fallbackQuestions && window.SISOREN_CONFIG.fallbackQuestions[npcId]) {
        const pool = window.SISOREN_CONFIG.fallbackQuestions[npcId];
        const categories = ['tanisma', 'derinlesme', 'yuzlestirme', 'baski', 'son'];
        const currentCategory = categories[askedCount] || 'tanisma';
        // Önce mevcut kategorideki soruları filtrele
        let filtered = pool.filter(q => q.category === currentCategory);
        if (filtered.length === 0) filtered = pool.slice(askedCount * 2, askedCount * 2 + 4);
        if (filtered.length === 0) filtered = pool.slice(0, 4);
        return filtered.slice(0, 4);
    }

    // SISOREN_CONFIG buildings içindeki npc.questions'dan fallback
    if (window.SISOREN_CONFIG && window.SISOREN_CONFIG.buildings) {
        const bld = window.SISOREN_CONFIG.buildings.find(b => b.npcId === npcId);
        if (bld && bld.npc && bld.npc.questions) {
            return bld.npc.questions.slice(askedCount, askedCount + 4).map(q => ({
                q: q,
                a: bld.npc.greeting || 'Sorunuzun cevabını düşünüp size döneceğim amirim...',
                difficulty: 1 + askedCount,
                category: 'tanisma'
            }));
        }
    }

    return [];
}

// Gölge Şehir için her aşamada zengin 4'lü hazır soru üreten yardımcı fonksiyon
function getGolgeFallbackQuestions(npcId, askedCount) {
    const categories = ['tanisma', 'derinlesme', 'yuzlestirme', 'baski', 'son'];
    const currentCategory = categories[askedCount] || 'tanisma';

    const golgePools = {
        101: [ // Oduncu Tahsin
            { q: 'Cinayet gecesi ormanda kimi gördün?', a: 'Gece yarısı orman yolu zifiri karanlıktı. Fener ışığı gibi bir parıltı gördüm, biri telaşla göl patikasına doğru koşuyordu.', difficulty: 1, category: 'tanisma', relatedClues: [1011] },
            { q: 'Baltanı en son nerede kullandın?', a: 'Dün akşamüstü tomrukları yontmak için kullandım. Sonra kulübenin kapısına dayadım.', difficulty: 1, category: 'tanisma', relatedClues: [1011] },
            { q: 'Ekrem Bey ile arandaki borç meselesi nedir?', a: 'Benden kestiğim keresteleri yok pahasına almaya çalışıyordu. Aramızda ticari bir sürtüşme vardı ama bu cinayet sebebi değil!', difficulty: 1, category: 'tanisma', relatedClues: [1012] },
            { q: 'Ormanda duyduğun sesler kime aitti?', a: 'Saat tam 02:00 gibiydi... Bir dal kırılma sesi ve ardından hızlı ayak sesleri duydum.', difficulty: 1, category: 'tanisma', relatedClues: [] },
            { q: 'Baltanın üzerindeki koyu lekeler nedir?', a: 'Reçinedir o dedektif! Çam reçinesi kuruyunca kararır, aklınıza hemen kan getirmeyin!', difficulty: 2, category: 'derinlesme', relatedClues: [1011] },
            { q: 'Kereste defterindeki son sipariş kime aitti?', a: 'Ekrem Bey özel av köşkü için kaliteli kereste ayırtmıştı. O gece teslimatı konuşacaktık.', difficulty: 2, category: 'derinlesme', relatedClues: [1012] },
            { q: 'Muhtar Cevdet ile orman arazisini görüştünüz mü?', a: 'Muhtar arazileri Ekrem\'e satmaya çalışıyordu. Ben de hakkımı yedirmem dedim.', difficulty: 3, category: 'yuzlestirme', relatedClues: [] },
            { q: 'Son sözün nedir Tahsin?', a: 'Ben dürüst bir oduncuyum amirim! Ekrem ile kavgamız oldu ama ona kıymadım!', difficulty: 4, category: 'son', relatedClues: [] }
        ],
        102: [ // Manav Ayşe
            { q: 'Cinayet gecesi saat kaçta uyudun?', a: 'Gece 01:00\'e kadar kasaları düzenledim amirim. Yağmur başlayınca kepengi çekip içeri girdim.', difficulty: 1, category: 'tanisma', relatedClues: [1021] },
            { q: 'Ekrem Bey senden ne istiyordu?', a: 'Dükkânımın tapusu onun elindeydi... İpotek borcumu ödeyemezsem beni kapının önüne koymakla tehdit ediyordu!', difficulty: 1, category: 'tanisma', relatedClues: [1022] },
            { q: 'Dükkânın etrafında dolaşan yabancı kimdi?', a: 'Pelerinli ve uzun boylu biriydi... Yüzünü göremedim ama adımları çok hızlıydı.', difficulty: 1, category: 'tanisma', relatedClues: [] },
            { q: 'Çamur izleri hakkında ne biliyorsun?', a: 'Gece kapımın önünden göl kenarına doğru 42 numara iri çizme izleri gidiyordu.', difficulty: 1, category: 'tanisma', relatedClues: [1023] },
            { q: 'Kasanın arkasındaki zehirli elma neyin nesi?', a: 'Kurtlanmayı önlemek için tarım ilacı dökmüştüm amirim, kimseye yedirecek değildim!', difficulty: 2, category: 'derinlesme', relatedClues: [1021] },
            { q: 'İpotek mektubundaki tehditler kime aitti?', a: 'Ekrem Bey yazmıştı! \'Ya parayı getir ya dükkânı boşalt\' diyordu.', difficulty: 3, category: 'yuzlestirme', relatedClues: [1022] },
            { q: 'Cinayet saatinde dışarı çıktın mı?', a: 'Sadece hava almak için kapıyı araladım, hepsi bu!', difficulty: 4, category: 'baski', relatedClues: [] },
            { q: 'Son sözün nedir Ayşe?', a: 'Ben iki çocuk annesiyim, dükkânım için yalvardım ama katil olmadım!', difficulty: 5, category: 'son', relatedClues: [] }
        ],
        103: [ // Demirci Kazım
            { q: 'Bu özel çelik kilidi kimin için yaptın?', a: 'Ekrem Bey sipariş etmişti. Çok gizli evraklarını koyacağı çelik bir kasa için özel üç tırnaklı kilit istedi.', difficulty: 1, category: 'tanisma', relatedClues: [1031] },
            { q: 'Gece ocağı ne zaman söndürdün?', a: 'Gece 01:30 gibi körüğü kapattım. Külleri ıslatıp arkadaki kulübeme geçtim.', difficulty: 1, category: 'tanisma', relatedClues: [] },
            { q: 'Bıçaktaki özel damga senin işin mi?', a: 'Evet, benim çelik damgam. Ama ben o bıçaklardan kasabada isteyen herkese döverim.', difficulty: 1, category: 'tanisma', relatedClues: [1032] },
            { q: 'Demir tozları neden her yerde?', a: 'Ocak burası amirim! Gece gündüz çekiç vuruyoruz, her yer demir tozu olur elbet.', difficulty: 1, category: 'tanisma', relatedClues: [1033] },
            { q: 'Çelik kasanın şifresini başka kim biliyordu?', a: 'Sadece Ekrem Bey ve ben... Başka kimseye söylemedim yemin ederim!', difficulty: 2, category: 'derinlesme', relatedClues: [1031] },
            { q: 'Ocağın arkasındaki kanlı bez parçası kime ait?', a: 'Elimi demir çapağı kestiğinde sardığım bezdir o! Hemen cinayete bağlamayın!', difficulty: 3, category: 'yuzlestirme', relatedClues: [] },
            { q: 'Ekrem Bey ile tartıştınız mı?', a: 'Kasa parasını eksik verince biraz bağırdık çağırdık, sonra helalleştik.', difficulty: 4, category: 'baski', relatedClues: [] },
            { q: 'Son sözün nedir Kazım Usta?', a: 'Ben demiri bükerim ama insanın canına kıymam dedektif bey!', difficulty: 5, category: 'son', relatedClues: [] }
        ],
        104: [ // Bakkal Naciye
            { q: 'Veresiye defterindeki isim neden karalandı?', a: 'Ekrem Bey borcunu nakit ödediğini söyleyince çizdim. Yanlış bir şey mi var?', difficulty: 1, category: 'tanisma', relatedClues: [1041] },
            { q: 'Zehri kime sattığını hatırlıyor musun?', a: 'Fare zehirlerini Hekim Sevgi Hanım ve Manav Ayşe Hanım alırdı depodaki haşereler için.', difficulty: 1, category: 'tanisma', relatedClues: [1042] },
            { q: 'Ekrem Bey dükkânda ne kadar kaldı?', a: 'Cinayet akşamı saat 20:00 gibi uğradı, kibrit ve tütün alıp 5 dakikada çıktı.', difficulty: 1, category: 'tanisma', relatedClues: [] },
            { q: 'Gece dükkânın arkasından gelen sesler neydi?', a: 'Kedilerdir ya da rüzgardır amirim... Fırtına vardı zaten.', difficulty: 1, category: 'tanisma', relatedClues: [1043] },
            { q: 'Yırtılan defter sayfasında ne yazıyordu?', a: 'Kasabanın önde gelenlerinin gizli borç listesiydi... Korktum, yırttım attım!', difficulty: 2, category: 'derinlesme', relatedClues: [1041] },
            { q: 'Hekim Sevgi ile ne konuştunuz?', a: 'Zehir şişelerini teslim ettim, \'Dikkatli kullan\' dedi.', difficulty: 3, category: 'yuzlestirme', relatedClues: [1042] },
            { q: 'Son sözün nedir Naciye Teyze?', a: 'Ben yaşlı bir bakkalım, kasabanın tüm sırları bendedir ama elim kana bulanmaz!', difficulty: 5, category: 'son', relatedClues: [] }
        ],
        105: [ // Hekim Sevgi
            { q: 'Bu mor sızıntı hangi bitkiden elde ediliyor?', a: 'Banotu ve güzelavrat otu özütü. Ağrı kesici tentürlerde mikro dozlarda kullanırım.', difficulty: 1, category: 'tanisma', relatedClues: [1051] },
            { q: 'Muayenehaneye son gelen hasta kimdi?', a: 'Gece yarısına doğru Fehmi Bey geldi, tansiyonu fırlamıştı. İlaç verip gönderdim.', difficulty: 1, category: 'tanisma', relatedClues: [] },
            { q: 'Ekrem Bey sağlık sorunları için mi geldi?', a: 'Kalp çarpıntısı ve mide krampları vardı. Ona yatıştırıcı çay hazırlamıştım.', difficulty: 1, category: 'tanisma', relatedClues: [1052] },
            { q: 'Neşterin üzerindeki izler ne anlama geliyor?', a: 'Tıbbi operasyonlarda kullanılmış steril izlerdir amirim, cinayet aleti değildir.', difficulty: 1, category: 'tanisma', relatedClues: [1053] },
            { q: 'Banotu zehri ölümcül müdür?', a: 'Yüksek dozda kalbi 10 dakikada durdurur... Ama ben hekimim, can almam can kurtarırım!', difficulty: 2, category: 'derinlesme', relatedClues: [1051] },
            { q: 'Ekrem Bey\'in tırnaklarındaki morluklar zehir izi mi?', a: 'Olabilir... Otopsi raporu çıkınca gerçek ortaya çıkar zaten.', difficulty: 3, category: 'yuzlestirme', relatedClues: [] },
            { q: 'Son sözünüz nedir Hekim Hanım?', a: 'Hipokrat yemini ettim ben! Ekrem\'in düşmanları çoktu, beni alet etmeyin!', difficulty: 5, category: 'son', relatedClues: [] }
        ],
        106: [ // Muhtar Cevdet
            { q: 'Orman arazisiyle ilgili tapu kimde?', a: 'Resmi tapular muhtarlık kasasındadır. Ekrem Bey arazileri ucuza kapatmak istiyordu.', difficulty: 1, category: 'tanisma', relatedClues: [1061] },
            { q: 'Olay gecesi köy meydanında kimler vardı?', a: 'Gece 01:00\'de meydana çıktım, Bekçi Rıfat fenerle turluyordu, başka kimseyi görmedim.', difficulty: 1, category: 'tanisma', relatedClues: [] },
            { q: 'Ekrem Bey ile tartıştığınız doğru mu?', a: 'Kasabanın ortak merası için tartıştık. Muhtar olarak halkın hakkını korumak zorundaydım.', difficulty: 1, category: 'tanisma', relatedClues: [1062] },
            { q: 'Masandaki mühürlü mektup kime gidecekti?', a: 'Kaymakamlığa resmi şikayet dilekçesiydi. Ekrem\'in usulsüzlüklerini bildiriyordum.', difficulty: 1, category: 'tanisma', relatedClues: [1063] },
            { q: 'Sahte mühür iddialarına ne diyorsunuz?', a: 'İftira! Ben 15 yıllık muhtarım, mührüm de imzam da tertemizdir!', difficulty: 2, category: 'derinlesme', relatedClues: [1061] },
            { q: 'Tehdit mektubunu siz mi yazdınız?', a: 'Sert bir dille uyardım evet, ama ölüm tehdidi asla yapmadım!', difficulty: 3, category: 'yuzlestirme', relatedClues: [1062] },
            { q: 'Son sözünüz nedir Muhtar Bey?', a: 'Gölge Şehir\'in düzenini sağlamak benim görevim. Katili bulun, adalete teslim edin!', difficulty: 5, category: 'son', relatedClues: [] }
        ],
        107: [ // Muallim Fehmi Bey
            { q: 'Saatiniz neden tam cinayet saatinde durdu?', a: '02:14\'te durdu evladım... Dışarıdan gelen korkunç bir feryatla elimdeki köstekli saat yere düştü.', difficulty: 1, category: 'tanisma', relatedClues: [1071] },
            { q: 'Pencerenizin altından geçenler kimdi?', a: 'Gölge gibi iki karaltı gördüm. Biri diğerini çam ağaçlarının arasına doğru sürüklüyordu.', difficulty: 1, category: 'tanisma', relatedClues: [] },
            { q: 'Kitaptaki notun cinayetle ilgisi nedir?', a: 'Eski bir intikam şiiri... Ekrem Bey yıllar önce babamın tüm mirasını hileyle elimizden almıştı.', difficulty: 1, category: 'tanisma', relatedClues: [1072] },
            { q: 'Gece dışarı çıkıp feneri neden kullandınız?', a: 'Sesleri duyunca pencereden baktım, göremedim. Kapının önüne kadar çıktım ama korkup geri döndüm.', difficulty: 1, category: 'tanisma', relatedClues: [1073] },
            { q: 'Ekrem Bey\'den nefret ediyor muydunuz?', a: 'Nefret değil evladım, derin bir keder... Ama Allah\'a havale etmiştim onu.', difficulty: 2, category: 'derinlesme', relatedClues: [1072] },
            { q: '02:14\'teki feryat kime aitti?', a: 'Ekrem\'in boğuk sesiydi... \'Yapma, dur!\' diye yalvarıyordu.', difficulty: 3, category: 'yuzlestirme', relatedClues: [1071] },
            { q: 'Son sözünüz nedir Hocam?', a: 'Ömrüm talebe yetiştirmekle geçti. Kan dökmek benim kitabımda yazmaz evladım.', difficulty: 5, category: 'son', relatedClues: [] }
        ],
        108: [ // Kunduracı Rasim
            { q: 'Göl kenarındaki çamurlu çizme izleri sana mı ait?', a: 'Ben o çizmeleri sipariş üzerine kasabadaki pek çok kişiye diktim amirim. Taban kalıbı benimdir ama izi bırakan ben değilim.', difficulty: 1, category: 'tanisma', relatedClues: [1081] },
            { q: 'Ekrem Bey sana kaçak deri getirmiş miydi?', a: 'Ekrem Bey ucuza getirdiği manda derilerini bana satmaya çalıştı. Kabul etmeyince aramız açıldı.', difficulty: 1, category: 'tanisma', relatedClues: [1082] },
            { q: 'Gece dükkânda ışık neden yanıktı?', a: 'Göl yoluna gidecek bir köylünün acil çizme siparişini yetiştiriyordum. Gece 02:00\'ye kadar mumlu iple diktim.', difficulty: 1, category: 'tanisma', relatedClues: [] },
            { q: 'Tezgâhındaki mumlu ip cinayette kullanılabilir mi?', a: 'Bu ip kalın saraç ipidir, kopmaz. Ama ben kundura dikerim, cana kıymam.', difficulty: 1, category: 'tanisma', relatedClues: [1083] },
            { q: 'Manda derisindeki gizli bölmede ne vardı?', a: 'Sahte senetler ve tapu evrakları saklamıştı Ekrem! Ben görünce öfkeyle üzerime yürüdü!', difficulty: 2, category: 'derinlesme', relatedClues: [1082] },
            { q: 'Gece 02:00\'den sonra göl kenarına gittin mi?', a: 'Sadece kapımın önündeki çamurları süpürdüm amirim, göle falan gitmedim!', difficulty: 3, category: 'yuzlestirme', relatedClues: [1081] },
            { q: 'Mumlu iplikle boğulma izleri uyuşuyor mu?', a: 'Ben ip satmam, ayakkabı dikerim! Biri benden ip çaldıysa benim suçum ne?!', difficulty: 4, category: 'baski', relatedClues: [1083] },
            { q: 'Son sözün nedir Rasim Usta?', a: 'Ekrem\'e 5 çift çizme borcum vardı, öldürsem borcum mu silinecekti? Ben yapmadım!', difficulty: 5, category: 'son', relatedClues: [] }
        ]
    };

    const allNpcQuestions = golgePools[npcId] || [];
    if (allNpcQuestions.length === 0) return [];

    // Kategoriye veya sıraya göre 4'lü paket seç
    const startIdx = (askedCount * 2) % Math.max(1, allNpcQuestions.length - 3);
    const selected = allNpcQuestions.slice(startIdx, startIdx + 4);
    return selected.length >= 4 ? selected : allNpcQuestions.slice(0, 4);
}

let currentTypewriterTimeout = null;
let isTyping = false;

function typeWriter(element, text, i, onComplete) {
    if (i < text.length) {
        isTyping = true;
        element.innerHTML = text.substring(0, i + 1) + '<span class="typewriter-cursor"></span>';

        // Ses efekti (Her 3 harfte bir mırıldanma/ses tonu)
        if (i % 3 === 0) {
            // Eczacı Selma (2) ve Komiser Güneş (4) kadın
            if (activeNpcId == 2 || activeNpcId == 4) {
                playSynthVoice(true); // Kadın sesi
            }
            // Kasap Hasan (1), Muhtar Kemal (3), Terzi Yahya (5) erkek
            else if (activeNpcId == 1 || activeNpcId == 3 || activeNpcId == 5) {
                playSynthVoice(false); // Erkek sesi
            }
        }

        currentTypewriterTimeout = setTimeout(() => typeWriter(element, text, i + 1, onComplete), 20); // Daktilo hızı
    } else {
        isTyping = false;
        element.innerHTML = text; // İmleci kaldır
        if (onComplete) onComplete();
    }
}

async function askQuestionBackend(npcId, question) {
    const npc = NPC_DATA[npcId];
    const chatArea = document.getElementById('npc-talk-chat');

    askedQuestionCount[npcId] = (askedQuestionCount[npcId] || 0) + 1;

    // Önceki yazma işlemini iptal et
    if (currentTypewriterTimeout) {
        clearTimeout(currentTypewriterTimeout);
        isTyping = false;
    }

    // Oyuncu mesajı
    const playerMsg = document.createElement('div');
    playerMsg.className = 'npc-talk-message player';
    playerMsg.innerHTML = `<div class="speaker">Dedektif</div><div class="msg-text">${question.q}</div>`;
    chatArea.appendChild(playerMsg);
    chatArea.scrollTop = chatArea.scrollHeight;

    // Butonları gizle (yazma bitene kadar)
    const btnContainer = document.getElementById('npc-talk-buttons');
    if (btnContainer) btnContainer.style.display = 'none';

    // Duygu durumuna göre CSS sınıfı belirle ve sadece karakter katmanına uygula
    const characterLayer = document.getElementById('npc-talk-character-layer');
    if (characterLayer) {
        characterLayer.className = 'npc-talk-character-layer'; // Sıfırla
        if (question.difficulty > 3 || question.category === 'baski') {
            characterLayer.classList.add('emotion-angry');
        } else if (question.category === 'yuzlestirme') {
            characterLayer.classList.add('emotion-nervous');
        }
    }

    // NPC mırıltı sesi
    let mumble = (npcId == 2 || npcId == 4) ? mumbleFemale : mumbleMale;
    if (mumble) {
        mumble.currentTime = 0;
        playSound(mumble, 0.25);
        setTimeout(() => stopSound(mumble), 1500);
    }

    // NPC cevabını belirle
    let answer = question.a || question.response || question.NPCResponse || question.responseText || "Söyleyecek bir şeyim yok amirim.";
    if ((npcId >= 201 && npcId <= 213) || (npcId >= 301 && npcId <= 318)) {
        try {
            const sisorenResponse = await fetch('/api/sisoren/interrogate', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    NpcId: npcId,
                    Question: question.q,
                    GuiltyNpcId: guiltyNpcId
                })
            });
            if (sisorenResponse.ok) {
                const data = await sisorenResponse.json();
                if (data.dialogue) answer = data.dialogue;
            }
        } catch (error) {
            console.warn('Sisören kayıtlı diyalog yanıtı alınamadı, yerel cevap kullanılacak.', error);
        }
    }
    if (question.guiltyResponse && guiltyNpcId && question.guiltyResponse[guiltyNpcId]) {
        answer = question.guiltyResponse[guiltyNpcId];
    }

    // NPC cevabı (gecikmeli başlat)
    setTimeout(() => {
        const npcMsg = document.createElement('div');
        npcMsg.className = 'npc-talk-message';

        const speakerDiv = document.createElement('div');
        speakerDiv.className = 'speaker';
        speakerDiv.textContent = npc.name;

        const textDiv = document.createElement('div');
        textDiv.className = 'msg-text';

        npcMsg.appendChild(speakerDiv);
        npcMsg.appendChild(textDiv);
        chatArea.appendChild(npcMsg);

        // Yazma sırasında scroll'u aşağıda tutmak için bir interval
        const scrollInterval = setInterval(() => {
            if (isTyping) chatArea.scrollTop = chatArea.scrollHeight;
            else clearInterval(scrollInterval);
        }, 100);

        // Daktilo efektiyle yaz
        typeWriter(textDiv, answer, 0, () => {
            // İpuçlarını kontrol et ve çantaya ekle
            if (question.relatedClues && question.relatedClues.length > 0) {
                checkAndDropClues(npcId, question.relatedClues);
            }

            // Konuşma geçmişine kaydet
            if (!dialogHistory[npcId]) dialogHistory[npcId] = [];
            dialogHistory[npcId].push({
                player: question.q,
                npc: answer,
                npcName: npc.name,
                difficulty: question.difficulty
            });

            // Soru kategorisine ve zorluğuna göre stres artışı
            let autoStress = 0;
            if (question.category === 'baski' || question.difficulty >= 4) autoStress = 25;
            else if (question.category === 'yuzlestirme' || question.difficulty === 3) autoStress = 15;
            else if (question.category === 'derinlesme' || question.difficulty === 2) autoStress = 8;
            else autoStress = 3;

            if (guiltyNpcId === npcId) autoStress = Math.round(autoStress * 1.4); // Suçlu daha hızlı strese girer
            npcStressLevels[npcId] = Math.min(100, (npcStressLevels[npcId] || 0) + autoStress);

            // Kalan soru sayısını ve stres monitörünü güncelle
            updateQuestionIndicator(npcId);
            saveGameState();

            if (btnContainer) btnContainer.style.display = 'grid';
            loadContextualQuestions(npcId);
        });

    }, 800);
}

// YEREL YAPAY ZEKA SERBEST SORU SORMA SİSTEMİ
function askFreeAiQuestion() {
    const currentNpcId = window.activeNpcId || activeNpcId || parseInt(document.getElementById('interior-screen')?.getAttribute('data-npc-id'), 10);
    if (!currentNpcId) {
        console.warn("Soru sorulacak aktif NPC belirlenemedi.");
        return;
    }
    activeNpcId = currentNpcId;
    window.activeNpcId = currentNpcId;

    const inputEl = document.getElementById('npc-ai-input');
    if (!inputEl) return;
    const questionText = inputEl.value.trim();
    if (!questionText) return;

    const askedCount = askedQuestionCount[currentNpcId] || 0;
    const maxQuestions = 5;
    if (askedCount >= maxQuestions) {
        showGlobalNotification('Uyarı', `Bu NPC ile konuşma hakkınız doldu (${maxQuestions}/${maxQuestions}). Artık soru soramazsınız!`, true);
        return;
    }

    inputEl.value = ''; // Temizle
    askedQuestionCount[currentNpcId] = askedCount + 1;
    updateQuestionIndicator(currentNpcId); // Anında kalan soru sayısını güncelle

    let npc = (window.NPC_DATA && window.NPC_DATA[currentNpcId]) || (typeof NPC_DATA !== 'undefined' ? NPC_DATA[currentNpcId] : null);
    if (!npc && currentNpcId >= 100 && window.GOLGE_SEHIR_CONFIG && window.GOLGE_SEHIR_CONFIG.buildings) {
        const bld = window.GOLGE_SEHIR_CONFIG.buildings.find(b => b.npcId == currentNpcId);
        if (bld) npc = bld.npc;
    }
    const chatArea = document.getElementById('npc-talk-chat');

    // Önceki daktilo yazma işlemini iptal et
    if (currentTypewriterTimeout) {
        clearTimeout(currentTypewriterTimeout);
        isTyping = false;
    }

    // Oyuncu mesajını chat alanına ekle
    const playerMsg = document.createElement('div');
    playerMsg.className = 'npc-talk-message player';
    playerMsg.innerHTML = `<div class="speaker">Dedektif</div><div class="msg-text">${questionText}</div>`;
    chatArea.appendChild(playerMsg);
    chatArea.scrollTop = chatArea.scrollHeight;

    // NPC mırıltı sesi
    let mumble = (activeNpcId == 2 || activeNpcId == 4) ? mumbleFemale : mumbleMale;
    if (mumble) {
        mumble.currentTime = 0;
        playSound(mumble, 0.25);
        setTimeout(() => stopSound(mumble), 1500);
    }

    // Backend Yerel Yapay Zeka Motoruna İstek Gönder (Gölge Şehir veya Gizemli Kasaba)
    const isSisoren = window.currentActiveTown === 'sisoren' && activeNpcId >= 200;
    const isGolge = !isSisoren && (activeNpcId >= 100);
    const targetEndpoint = isSisoren ? '/api/sisoren/interrogate' : (isGolge ? '/api/golge-sehir/interrogate' : '/api/game/interrogate');

    fetch(targetEndpoint, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ NpcId: activeNpcId, Question: questionText, GuiltyNpcId: guiltyNpcId })
    })
        .then(res => res.json())
        .then(data => {
            // Zengin Karakter-Bazlı Offline Fallback (Hiçbir zaman boş/anlamsız yanıt gösterilmez)
            let answer = data.dialogue;
            if (!answer || answer.trim() === '') {
                const npcFallbacks = {
                    1: "*Satırını tezgaha bırakır* Amirim, kasap dükkânımda her gün yüzlerce et parçası doğrarım. Cinayet soruşturmasıyla ilgili net bir soru sorarsanız size yardımcı olmaya çalışırım.",
                    2: "*İlaç şişelerini düzenler* Eczanemde her türlü ilacın kaydı var amirim. Osman Bey vakasıyla ilgili ne öğrenmek istiyorsanız açıkça sorun.",
                    3: "*Evrakları düzeltir* Muhtarlık makamındayım dedektif. Kasabanın huzuru ve davayla ilgili ne soracaksanız buyurun.",
                    4: "*Rozetini düzeltir* Karakoldayız amirim. Soruşturmayla ilgili tutanaklarımız meydanda, ne bilmek istiyorsanız sorun.",
                    5: "*İğnesini kumaşa batırır* Yaşlı bir terziyim evladım. Cinayet gecesi ve şüpheliler hakkında ne merak ediyorsanız dinliyorum.",
                    101: "*Baltasını omzuna alır* Ormandan gelen sesler ve Ekrem Bey cinayeti hakkında ne soracaksanız sorun amirim.",
                    102: "*Tezgahı siler* Manav Ayşe olarak Ekrem Bey vakası hakkında bildiklerimi anlatmaya hazırım amirim.",
                    103: "*Çekici örse vurur* Demirci Kâzım'ım ben. Ekrem'in çelik kasası veya cinayet hakkında ne soracaksınız?",
                    104: "*Veresiye defterini kapatır* Bakkal Naciye her şeyi duyar amirim. Ekrem Bey hakkında ne bilmek istiyorsunuz?",
                    105: "*Tıp kitabını kapatır* Hekim olarak tıbbi sorularınıza ve cinayet şüphelerinize yanıt verebilirim amirim.",
                    106: "*Masasına yaslanır* Muhtar Cevdet olarak kasaba halkı ve Ekrem Bey hakkında ne soracaksanız buyurun.",
                    107: "*Gözlüğünü düzeltir* Muallim Fehmi olarak cinayet gecesi duyduklarım ve kasaba tarihiyle ilgili ne sormak istersiniz?",
                    108: "*Mumlu ipi çeker* Kunduracı Rasim olarak çamurlu çizmeler ve göl yolundaki izler hakkında ne öğrenmek istiyorsunuz?"
                };
                answer = npcFallbacks[activeNpcId] || "*Gözlerini kısarak sizi süzer* Cinayet soruşturması hakkında net bir soru sorarsanız size yardımcı olabilirim amirim.";
            }
            let emotion = data.emotion || "Sakin";

            // Karakter duygu stili
            const characterLayer = document.getElementById('npc-talk-character-layer');
            if (characterLayer) {
                characterLayer.className = 'npc-talk-character-layer';
                if (emotion === 'Panik' || emotion === 'Sinirli') {
                    characterLayer.classList.add('emotion-angry');
                } else if (emotion === 'Tedirgin' || emotion === 'Gergin') {
                    characterLayer.classList.add('emotion-nervous');
                }
            }

            const npcMsg = document.createElement('div');
            npcMsg.className = 'npc-talk-message';
            const speakerDiv = document.createElement('div');
            speakerDiv.className = 'speaker';
            speakerDiv.textContent = npc.name;
            const textDiv = document.createElement('div');
            textDiv.className = 'msg-text';

            npcMsg.appendChild(speakerDiv);
            npcMsg.appendChild(textDiv);
            chatArea.appendChild(npcMsg);
            typeWriter(textDiv, answer, 0, () => {
                const stressInc = data.stressIncrease || 0;
                if (stressInc > 0) {
                    npcStressLevels[activeNpcId] = Math.min(100, (npcStressLevels[activeNpcId] || 0) + stressInc);
                }

                if (data.revealedSecret) {
                    showCinematicHelper(data.revealedSecret, false);
                }
                if (!dialogHistory[activeNpcId]) dialogHistory[activeNpcId] = [];
                dialogHistory[activeNpcId].push({
                    player: questionText,
                    npc: answer,
                    npcName: npc.name,
                    difficulty: 3
                });
                logDialog(activeNpcId, questionText, answer, 3, 'serbest_ai');
                updateQuestionIndicator(activeNpcId);
                saveGameState();
                loadContextualQuestions(activeNpcId);
            });
        })
        .catch(err => {
            console.error("Yerel AI hatası:", err);
            const npcMsg = document.createElement('div');
            npcMsg.className = 'npc-talk-message';
            npcMsg.innerHTML = `<div class="speaker">${npc.name}</div><div class="msg-text">*Sessizce süzüyor* Ne demek istiyorsun dedektif?</div>`;
            chatArea.appendChild(npcMsg);
            updateQuestionIndicator(activeNpcId);
        });
}

document.getElementById('npc-ai-send-btn')?.addEventListener('click', askFreeAiQuestion);
document.getElementById('npc-ai-input')?.addEventListener('keypress', (e) => {
    if (e.key === 'Enter') {
        askFreeAiQuestion();
    }
});

document.getElementById('npc-talk-close').addEventListener('click', () => {
    npcTalkModal.classList.add('hidden');
    stopSound(mumbleMale);
    stopSound(mumbleFemale);
});
// Removed duplicate startThunder function

// =============================================================
// 6. BAG (ÇANTA) VE DETAYLI İNCELEME
// =============================================================

const detailedClueModal = document.getElementById('detailed-clue-modal');

function inspectClue(clueId) {
    const clue = currentBag.find(c => c.id === clueId);
    if (!clue) return;

    // İşaretle: İncelendi
    clue.inspected = true;
    openBag();

    // NPC ID'yi bul (Arkaplan için)
    let clueNpcId = activeNpcId || 1;
    for (const [npcIdStr, items] of Object.entries(SCENE_OBJECTS)) {
        if (items.some(i => i.id === clue.id)) {
            clueNpcId = parseInt(npcIdStr);
            break;
        }
    }

    // Yeni 4D Laboratuvar Ekranını Aç (fromBag = true)
    openClueInspect(clue, clueNpcId, true);

    const detailedText = document.getElementById('clue-inspect-desc');
    detailedText.innerHTML = '<span style="opacity:0.5;">Yükleniyor...</span>';

    // Backend'den dinamik ipucu detayını çek
    fetch(`/api/game/clue-detail/${clue.id}`)
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                // Typewriter effect for detailed text
                detailedText.innerHTML = '';
                let i = 0;
                let text = data.text;
                function type() {
                    if (i < text.length) {
                        detailedText.innerHTML += text.charAt(i);
                        i++;
                        setTimeout(type, 30);
                    }
                }
                type();
            } else {
                detailedText.textContent = clue.desc; // Fallback
            }
        })
        .catch(err => {
            console.error(err);
            detailedText.textContent = clue.desc;
        });
}

function removeClue(clueId) {
    const idx = currentBag.findIndex(c => c.id === clueId);
    if (idx > -1) {
        if (currentBag[idx].inspected) {
            showGlobalNotification("Uyarı", "İncelenen deliller çantadan çıkarılamaz!", true);
            return;
        }
        currentBag.splice(idx, 1);
        checkAutopsyConditions();
        openBag();
    }
}

document.getElementById('close-detailed-clue-btn')?.addEventListener('click', () => {
    detailedClueModal.classList.add('hidden');
});

function openBag() {
    const bagList = document.getElementById('bag-items-list');
    if (currentBag.length === 0) {
        bagList.innerHTML = '<p style="color: var(--text-muted); text-align:center; padding:30px;">Çanta boş. Binalardaki ipuçlarını toplayın.</p>';
    } else {
        bagList.innerHTML = currentBag.map(b => `
            <div class="bag-item">
                <div class="bag-item-left">
                    <img src="${b.img}" alt="${b.name}">
                    <span>${b.name}</span>
                </div>
                <div class="bag-item-actions">
                    <button class="btn btn-primary" onclick="inspectClue(${b.id})">
                        <i class="fa-solid fa-magnifying-glass"></i> İncele
                    </button>
                    <button class="btn btn-danger" onclick="removeClue(${b.id})" ${b.inspected ? 'disabled style="opacity:0.5;" title="İncelendi, çıkarılamaz"' : ''}>
                        <i class="fa-solid fa-trash"></i> ${b.inspected ? 'Çıkarılamaz' : 'Çıkar'}
                    </button>
                </div>
            </div>
        `).join('');
    }

    // Not Defterini Yükle
    const notebook = document.getElementById('detective-notes');
    if (notebook) {
        notebook.value = localStorage.getItem('detectiveNotes') || '';
    }

    bagModal.classList.remove('hidden');

    // Çetin çanta açıldığında tam sayı ve delil isimleri versin
    if (currentBag.length === 0) {
        showCinematicHelper("Amirims, çantamız henüz bomboş (0/5 delil)! Binalardaki nesnelere tıklayarak delilleri toplayın.", false);
    } else {
        const itemNames = currentBag.map(c => `'${c.name}'`).join(', ');
        showCinematicHelper(`Amirims, çantamızda şu an tam ${currentBag.length} delil var: ${itemNames}. Şüphelileri sorgularken bu deliller büyük kozumuz!`, false);
    }
    logAction('open_bag', null, `${currentBag.length} delil`);
}

// Notları kaydetme
document.getElementById('detective-notes')?.addEventListener('input', (e) => {
    localStorage.setItem('detectiveNotes', e.target.value);
});

document.getElementById('open-bag-btn')?.addEventListener('click', openBag);
document.getElementById('interior-bag-btn')?.addEventListener('click', openBag);
document.getElementById('close-bag-btn')?.addEventListener('click', () => bagModal.classList.add('hidden'));
document.getElementById('close-bag-x-btn')?.addEventListener('click', () => bagModal.classList.add('hidden'));

// =============================================================
// 7. BULDUM! (SUÇLAMA SİSTEMİ) — 8 ŞÜPHELİ MASA KARTLARI
// =============================================================

document.getElementById('found-btn').addEventListener('click', () => {
    renderFoundScreen();
    foundModal.classList.remove('hidden');
    // Çetin suçlama ekranında uyarsın
    setTimeout(() => triggerHelperMessage('accuse'), 800);
    logAction('accuse_screen_open');
});

document.getElementById('found-close-btn').addEventListener('click', () => {
    foundModal.classList.add('hidden');
});

function renderFoundScreen() {
    const isGolge = (window.currentActiveTown === 'golge_sehir' || document.body.classList.contains('golge-sehir-theme'));
    const isSisoren = (window.currentActiveTown === 'sisoren' || document.body.classList.contains('sisoren-theme'));
    const grid = document.getElementById('found-npc-cards');
    if (!grid) return;
    grid.innerHTML = '';

    // Verileri yükle
    if (isGolge && window.GolgeSehirEngine && typeof window.GolgeSehirEngine.registerGolgeSehirData === 'function') {
        window.GolgeSehirEngine.registerGolgeSehirData();
    }
    if (isSisoren && window.SisorenEngine && typeof window.SisorenEngine.registerSisorenData === 'function') {
        window.SisorenEngine.registerSisorenData();
    }

    // KASABAYA GÖRE GRID DÜZENİNİ AYIR
    grid.classList.remove('desk-photos-grid', 'golge-sehir-grid', 'sisoren-grid');
    if (isSisoren) {
        grid.classList.add('sisoren-grid');
    } else if (isGolge) {
        grid.classList.add('golge-sehir-grid');
    } else {
        grid.classList.add('desk-photos-grid');
    }

    let startId, endId;
    if (isSisoren) {
        startId = 201; endId = 213;
    } else if (isGolge) {
        startId = 101; endId = 108;
    } else {
        startId = 1; endId = 5;
    }

    for (let id = startId; id <= endId; id++) {
        let npc = (window.NPC_DATA && window.NPC_DATA[id]) || (typeof NPC_DATA !== 'undefined' ? NPC_DATA[id] : null);

        // Config'lerden çek
        if (!npc && isGolge && window.GOLGE_SEHIR_CONFIG && window.GOLGE_SEHIR_CONFIG.buildings) {
            const bld = window.GOLGE_SEHIR_CONFIG.buildings.find(b => b.npcId === id);
            if (bld) npc = bld.npc;
        }
        if (!npc && isSisoren && window.SISOREN_CONFIG && window.SISOREN_CONFIG.buildings) {
            const bld = window.SISOREN_CONFIG.buildings.find(b => b.npcId === id);
            if (bld) npc = bld.npc;
        }

        if (!npc) continue;
        const hasHistory = dialogHistory[id] && dialogHistory[id].length > 0;
        const askedCount = askedQuestionCount[id] || 0;
        
        let baseImg;
        if (isSisoren) {
            baseImg = npc.portrait || '';
        } else if (id === 107) {
            baseImg = 'images/towns/golge_sehir/npcler/npc_107_talk.jpg';
        } else {
            baseImg = npc.portrait || npc.talkBg || npc.img || 'images/dedektif.png';
        }
        const npcImg = baseImg ? `${baseImg.split('?')[0]}?v=${Date.now()}` : '';

        const isAlreadyInnocent = (window.innocentNpcIds && window.innocentNpcIds.has(id)) || innocentNpcIds.has(id);
        const card = document.createElement('div');
        card.className = `found-npc-card ${isAlreadyInnocent ? 'innocent-marked' : ''} ${isSisoren ? 'sisoren-card' : ''}`;

        const photoHtml = (isSisoren && npc.portrait)
            ? `<div class="found-npc-photo-wrap">
                   <img src="${npcImg}" alt="${npc.name}" class="found-npc-img" data-npc-id="${id}" title="Konuşma geçmişini görüntüle" onerror="this.src='images/dedektif.png'">
               </div>`
            : (isSisoren
                ? `<div class="found-npc-photo-wrap sisoren-placeholder-photo" data-npc-id="${id}" title="Şüpheli #${id}">
                       <i class="fa-solid fa-user-secret sisoren-placeholder-icon"></i>
                       <span class="sisoren-placeholder-text">ŞÜPHELİ #${id - 200}</span>
                   </div>`
                : `<div class="found-npc-photo-wrap">
                       <img src="${npcImg}" alt="${npc.name}" class="found-npc-img" data-npc-id="${id}" title="Konuşma geçmişini görüntüle" onerror="this.src='images/dedektif.png'">
                   </div>`);

        const nameHtml = (isSisoren && npc.name)
            ? `<div class="found-npc-name" style="color: #6ee7b7;">${npc.name}</div>`
            : (isSisoren
                ? `<div class="found-npc-name sisoren-empty-name">---</div>`
                : `<div class="found-npc-name">${npc.name}</div>`);

        const roleHtml = `<div class="found-npc-role">${npc.building} (${isSisoren ? (npc.gender === 'female' ? 'Kadın' : 'Erkek') : npc.role})</div>`;

        card.innerHTML = `
            ${isAlreadyInnocent ? '<div class="innocent-stamp">MASUM</div>' : ''}
            ${photoHtml}
            ${nameHtml}
            ${roleHtml}
            ${hasHistory ? `<div style="font-size:0.8rem; color:${isSisoren ? '#34d399' : '#f59e0b'}; font-weight:bold; margin-bottom:6px;"><i class="fa-solid fa-comment-check"></i> ${askedCount} Soru Soruldu</div>` : '<div style="font-size:0.8rem; color:#888; margin-bottom:6px;">Henüz konuşulmadı</div>'}
            <div class="found-npc-actions">
                <button class="btn btn-outline" style="width:100%; margin-bottom:6px;" onclick="window.showNpcHistory(${id})"><i class="fa-solid fa-comments"></i> Notlar</button>
            </div>
            <div class="found-npc-actions" style="display:flex; gap:6px; width:100%;">
                ${isAlreadyInnocent 
                    ? '<button class="btn btn-success" style="width:100%; pointer-events:none;" disabled><i class="fa-solid fa-shield-check"></i> MASUM İLAN EDİLDİ</button>'
                    : `<button class="btn btn-danger" style="flex:1;" onclick="if(typeof window.playJudgeGavelTripleStrike==='function')window.playJudgeGavelTripleStrike(); window.accuseNpc(${id});"><i class="fa-solid fa-handcuffs"></i> Suçlu</button>
                       <button class="btn btn-success" style="flex:1;" onclick="window.innocentNpc(${id})"><i class="fa-solid fa-shield-halved"></i> Masum</button>`
                }
            </div>
        `;
        grid.appendChild(card);
    }

    // === SİSÖREN EKSTRA NPC'LERİ (Konuşulmuş olanlar) ===
    if (isSisoren && window.sisorenTalkedExtraNpcs && window.sisorenTalkedExtraNpcs.size > 0) {
        // Ayırıcı başlık
        const separator = document.createElement('div');
        separator.style.cssText = 'grid-column: 1 / -1; text-align: center; padding: 12px; border-top: 1px solid rgba(52, 211, 153, 0.3); margin-top: 10px;';
        separator.innerHTML = '<span style="color: #6ee7b7; font-size: 0.85rem; font-weight: 700; letter-spacing: 1px;"><i class="fa-solid fa-users"></i> KONUŞULAN KASABALILAR (Yan Şüpheliler)</span>';
        grid.appendChild(separator);

        window.sisorenTalkedExtraNpcs.forEach(extraId => {
            const extraNpc = window.NPC_DATA[extraId];
            if (!extraNpc) return;

            const hasHistory = dialogHistory[extraId] && dialogHistory[extraId].length > 0;
            const askedCount = askedQuestionCount[extraId] || 0;
            const isAlreadyInnocent = (window.innocentNpcIds && window.innocentNpcIds.has(extraId)) || innocentNpcIds.has(extraId);

            const card = document.createElement('div');
            card.className = `found-npc-card sisoren-card sisoren-extra-card ${isAlreadyInnocent ? 'innocent-marked' : ''}`;

            card.innerHTML = `
                ${isAlreadyInnocent ? '<div class="innocent-stamp">MASUM</div>' : ''}
                <div class="found-npc-photo-wrap sisoren-placeholder-photo sisoren-extra-photo" data-npc-id="${extraId}" title="${extraNpc.name}">
                    <i class="fa-solid fa-user sisoren-placeholder-icon" style="color: #6ee7b7;"></i>
                    <span class="sisoren-placeholder-text" style="font-size: 0.65rem;">${extraNpc.role || 'Kasabalı'}</span>
                </div>
                <div class="found-npc-name" style="color: #6ee7b7; font-size: 0.8rem;">${extraNpc.name}</div>
                <div class="found-npc-role">${extraNpc.building || 'Sokak'} (${extraNpc.gender === 'female' ? 'Kadın' : 'Erkek'})</div>
                ${hasHistory ? `<div style="font-size:0.75rem; color:#34d399; font-weight:bold; margin-bottom:6px;"><i class="fa-solid fa-comment-check"></i> ${askedCount} Soru</div>` : ''}
                <div class="found-npc-actions" style="display:flex; gap:6px; width:100%;">
                    ${isAlreadyInnocent 
                        ? '<button class="btn btn-success" style="width:100%; pointer-events:none; font-size:0.75rem;" disabled><i class="fa-solid fa-shield-check"></i> MASUM</button>'
                        : `<button class="btn btn-danger" style="flex:1; font-size:0.75rem;" onclick="window.accuseNpc(${extraId})"><i class="fa-solid fa-handcuffs"></i> Suçla</button>
                           <button class="btn btn-success" style="flex:1; font-size:0.75rem;" onclick="window.innocentNpc(${extraId})"><i class="fa-solid fa-shield-halved"></i> Masum</button>`
                    }
                </div>
            `;
            grid.appendChild(card);
        });
    }

    // NPC görsel tıklanınca konuşma geçmişi
    grid.querySelectorAll('.found-npc-img').forEach(img => {
        img.addEventListener('click', () => {
            const npcId = parseInt(img.getAttribute('data-npc-id'));
            showNpcHistory(npcId);
        });
    });
}

window.showNpcHistory = function (npcId) {
    const npc = NPC_DATA[npcId];
    const history = dialogHistory[npcId] || [];

    document.getElementById('npc-history-title').innerHTML = `<i class="fa-solid fa-comments"></i> ${npc.name} \u2014 Konu\u015Fma Ge\u00E7mi\u015Fi`;

    const content = document.getElementById('npc-history-content');
    if (history.length === 0) {
        content.innerHTML = '<p style="color: var(--text-muted); text-align:center; padding:30px;">Bu NPC ile hen\u00FCz konu\u015Fulmad\u0131.</p>';
    } else {
        content.innerHTML = history.map(h => `
            <div class="history-msg player-msg">
                <div class="h-speaker">Dedektif</div>
                <div>${h.player}</div>
            </div>
            <div class="history-msg npc-msg">
                <div class="h-speaker">${h.npcName}</div>
                <div>${h.npc}</div>
            </div>
        `).join('');
    }

    npcHistoryModal.classList.remove('hidden');
}

document.getElementById('npc-history-close').addEventListener('click', () => {
    npcHistoryModal.classList.add('hidden');
});

// === ACCUSE NPC (BACKEND CHECK) — YENİ HAPİS ANİMASYONU ===
window.accuseNpc = function (accusedId) {
    const npc = NPC_DATA[accusedId];
    if (!npc) return;

    // === EKSTRA NPC SUÇLAMASI: HER ZAMAN MASUM ===
    if (npc.isExtra || npc.canBeGuilty === false || accusedId >= 300) {
        foundModal.classList.add('hidden');
        const resultIcon = document.getElementById('result-icon');
        const resultTitle = document.getElementById('result-title');
        const resultMessage = document.getElementById('result-message');
        const retryBtn = document.getElementById('result-retry-btn');
        const resultModal = document.getElementById('result-modal');

        if (resultIcon) {
            resultIcon.className = 'result-icon fail';
            resultIcon.innerHTML = '<i class="fa-solid fa-shield-halved" style="color: #10b981;"></i>';
        }
        if (resultTitle) {
            resultTitle.textContent = 'BU KİŞİ MASUMDUR!';
            resultTitle.style.color = '#10b981';
        }
        if (resultMessage) {
            resultMessage.innerHTML = `
                <div class="result-verdict">
                    <div class="result-verdict-line"><i class="fa-solid fa-user-check" style="color:#10b981"></i> Suçlanan: <strong>${npc.name}</strong></div>
                    <div class="result-verdict-line" style="color:#6ee7b7;"><i class="fa-solid fa-shield-check"></i> Durum: <strong>KESİNLİKLE MASUM</strong></div>
                    <div class="result-verdict-correct" style="background: rgba(16, 185, 129, 0.2); color: #34d399; border-color: #059669;">
                        <i class="fa-solid fa-circle-info"></i> Bu kişi cinayet gecesi başka bir yerdeydi ve kesinlikle masumdur!
                    </div>
                </div>
                <div class="result-story">
                    <div class="result-story-header"><i class="fa-solid fa-user-shield"></i> Masumiyet Kanıtı</div>
                    <div class="result-story-text">${npc.name} (${npc.role || 'Kasabalı'}), cinayet gecesi birden fazla tanık tarafından farklı bir konumda görülmüştür. Bu kişi asıl şüphelilerden biri değildir. Soruşturmayı asıl 13 şühpeliye yoğunlaştırın!</div>
                </div>`;
        }
        if (retryBtn) {
            retryBtn.innerHTML = '<i class="fa-solid fa-rotate-right"></i> Suçlama Ekranına Dön';
            retryBtn.classList.remove('hidden');
            retryBtn.onclick = () => {
                resultModal.classList.add('hidden');
                renderFoundScreen();
                foundModal.classList.remove('hidden');
            };
        }
        if (resultModal) resultModal.classList.remove('hidden');

        // Masum olarak işaretle
        if (window.innocentNpcIds) window.innocentNpcIds.add(accusedId);
        innocentNpcIds.add(accusedId);
        return;
    }

    foundModal.classList.add('hidden');

    // YENİ HAPİS GÖRSELLERİNİ KULLAN (GİZEMLİ KASABA & GÖLGE ŞEHİR)
    const jailImages = {
        1: 'images/jail_hasan.png',
        2: 'images/jail_selma.png',
        3: 'images/jail_kemal.png',
        4: 'images/jail_gunes.png',
        5: 'images/jail_yahya.png',
        101: 'images/towns/golge_sehir/npcler/jail_npc_101.png',
        102: 'images/towns/golge_sehir/npcler/jail_npc_102.png',
        103: 'images/towns/golge_sehir/npcler/jail_npc_103.png',
        104: 'images/towns/golge_sehir/npcler/jail_npc_104.png',
        105: 'images/towns/golge_sehir/npcler/jail_npc_105.png',
        106: 'images/towns/golge_sehir/npcler/jail_npc_106.png',
        107: 'images/towns/golge_sehir/npcler/jail_npc_107.png',
        108: 'images/towns/golge_sehir/npcler/jail_npc_108.png'
    };

    // Yeni hapis animasyonu — NPC tam boy parmaklıkların arkasında
    const jailNpcFull = document.getElementById('jail-npc-full');
    const jailNpcName = document.getElementById('jail-npc-name');
    const jailArrestedText = document.getElementById('jail-arrested-text');

    if (jailNpcFull) jailNpcFull.src = jailImages[npc.id] || npc.img;
    if (jailNpcName) jailNpcName.textContent = npc.name;

    // Animasyonu sıfırla
    if (jailNpcFull) {
        jailNpcFull.style.animation = 'none';
        jailNpcFull.offsetHeight; // Force reflow
        jailNpcFull.style.animation = '';
    }
    if (jailArrestedText) {
        jailArrestedText.style.animation = 'none';
        jailArrestedText.offsetHeight;
        jailArrestedText.style.animation = '';
    }

    // Jail bars animasyonu da sıfırla
    const jailBars = document.querySelector('.jail-bars');
    if (jailBars) {
        jailBars.style.animation = 'none';
        jailBars.offsetHeight;
        jailBars.style.animation = '';
    }

    jailOverlay.classList.remove('hidden');

    // Hakim Tokmak Vuruşları (Tak... Tak... Tak!)
    if (typeof playJudgeGavelTripleStrike === 'function') {
        playJudgeGavelTripleStrike();
    }

    // Animasyon sırasında API'ye sor (Sisören, Gölge Şehir veya Gizemli Kasaba)
    let accuseEndpoint;
    if (accusedId >= 201 || window.currentActiveTown === 'sisoren') {
        accuseEndpoint = '/api/sisoren/accuse';
    } else if (accusedId >= 100 || window.currentActiveTown === 'golge_sehir') {
        accuseEndpoint = '/api/golge-sehir/accuse';
    } else {
        accuseEndpoint = '/api/game/accuse';
    }

    fetch(accuseEndpoint, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ 
            NpcId: accusedId,
            GuiltyNpcId: (typeof guiltyNpcId !== 'undefined' && guiltyNpcId) ? guiltyNpcId : (window.guiltyNpcId || null)
        })
    })
        .then(res => res.json())
        .then(data => {
            setTimeout(() => {
                jailOverlay.classList.add('hidden');

                const resultIcon = document.getElementById('result-icon');
                const resultTitle = document.getElementById('result-title');
                const resultMessage = document.getElementById('result-message');
                const retryBtn = document.getElementById('result-retry-btn');

                // Backend'den dönen resmi katil kimliğini ve başarı sonucunu esas al
                const serverGuiltyId = data.guiltyNpcId || guiltyNpcId;
                guiltyNpcId = serverGuiltyId;

                const realKiller = NPC_DATA[serverGuiltyId] || NPC_DATA[1];
                const accusedNpc = NPC_DATA[accusedId] || NPC_DATA[1];

                // SUÇLU DOĞRU MU KONTROLÜ (API sonucu veya ID eşleşmesi)
                const isCorrect = (data.success === true) || (accusedId === serverGuiltyId);

                if (isCorrect) {
                    if (window.currentActiveTown === 'gizemli' || accusedId < 100) {
                        window.gizemliSolved = true;
                        const golgeTownBtn = document.querySelector('.region-town[data-town-name="Gölgeşehir"]') || document.querySelector('.region-town[data-town-id="golge_sehir"]');
                        if (golgeTownBtn) {
                            golgeTownBtn.classList.remove('town-locked');
                            golgeTownBtn.classList.add('town-active');
                            const tagEl = golgeTownBtn.querySelector('.region-town-tag');
                            if (tagEl) {
                                tagEl.classList.remove('tag-locked');
                                tagEl.classList.add('tag-active');
                                tagEl.innerHTML = '<i class="fa-solid fa-magnifying-glass"></i> GÖLGEŞEHİR <span class="status-badge status-open" style="background:#f59e0b; color:#111;">Soruşturmaya Açık</span>';
                            }
                        }
                    }

                    resultIcon.className = 'result-icon success';
                    resultIcon.innerHTML = '<i class="fa-solid fa-trophy"></i>';
                    resultTitle.textContent = 'TEBRİKLER! KAZANDINIZ!';
                    resultTitle.style.color = 'var(--success)';
                    resultMessage.innerHTML = `
                    <div class="result-verdict">
                        <div class="result-verdict-line"><i class="fa-solid fa-user-check" style="color:var(--success)"></i> Sizin Seçiminiz: <strong>${accusedNpc.name}</strong></div>
                        <div class="result-verdict-line"><i class="fa-solid fa-handcuffs" style="color:var(--accent)"></i> Gerçek Katil: <strong>${realKiller.name}</strong></div>
                        <div class="result-verdict-correct"><i class="fa-solid fa-circle-check"></i> DOĞRU TAHMİN!</div>
                    </div>
                    <div class="result-story">
                        <div class="result-story-header"><i class="fa-solid fa-book-skull"></i> O Gecenin Hikayesi</div>
                        <div class="result-story-text">${realKiller.murderStory}</div>
                    </div>`;
                    retryBtn.innerHTML = '<i class="fa-solid fa-map-location-dot"></i> Haritaya Dön';
                    retryBtn.classList.remove('hidden');

                    if (window.currentActiveTown === 'golge_sehir' || accusedId > 100) {
                        window.golgeSolved = true;
                        // 3. Bölge: Sisören Kilidini Aç
                        const sisorenTownBtn = document.querySelector('.region-town[data-town-name="Sisören"]') || document.querySelector('.region-town[data-town-id="sisoren"]');
                        if (sisorenTownBtn) {
                            sisorenTownBtn.classList.remove('town-locked');
                            sisorenTownBtn.classList.add('town-active', 'town-sisoren-active');
                            const tagEl = sisorenTownBtn.querySelector('.region-town-tag');
                            if (tagEl) {
                                tagEl.classList.remove('tag-locked');
                                tagEl.classList.add('tag-active');
                                tagEl.innerHTML = '<i class="fa-solid fa-magnifying-glass"></i> SİSÖREN <span class="status-badge status-open" style="background:#10b981; color:#064e3b; font-weight:bold;">Soruşturmaya Açık</span>';
                            }
                        }
                    }

                    // Haritaya dönüş için event listener
                    retryBtn.onclick = () => {
                        resultModal.classList.add('hidden');
                        triggerTransition(() => {
                            document.getElementById('town-map-screen')?.classList.add('hidden');
                            if (interiorScreen) interiorScreen.classList.add('hidden');
                            if (npcTalkModal) npcTalkModal.classList.add('hidden');
                            if (foundModal) foundModal.classList.add('hidden');
                            document.getElementById('world-map-screen')?.classList.remove('hidden');

                            // Kasaba durumunu temizle
                            window.currentActiveTown = 'gizemli';
                            document.body.classList.remove('golge-sehir-theme');
                            visitedBuildings.clear();
                            if (window.GolgeSehirEngine) {
                                window.GolgeSehirEngine.visitedGolgeBuildings.clear();
                                window.GolgeSehirEngine.clearGolgeSehirMap();
                                window.GolgeSehirEngine.resetGolgeState();
                            }
                            currentBag = [];
                            submittedBloodClueIds.clear();
                            submittedPrintClueIds.clear();
                            submittedForensicCount = 0;
                            submittedForensicCountGizemli = 0;
                            submittedForensicCountGolge = 0;
                            isAutopsyTimerStarted = false;
                            isAutopsyReady = false;
                            autopsyTimeLeft = 60;
                            if (autopsyTimer) clearInterval(autopsyTimer);
                            dialogHistory = {};
                            npcTalkCompleted = {};
                            activeNpcId = null;
                            updateForensicBadge();
                            checkAutopsyConditions();
                        });
                        stopAllSounds();

                        // Çetin bölge haritasında durumu kutlar
                        setTimeout(() => {
                            if (window.gizemliSolved && window.golgeSolved) {
                                showCinematicHelper("Harika bir iş çıkardık Amirims! Hem Gizemli Kasaba'yı hem de Gölge Şehir'i aydınlattık! Şimdi diğer kasabalardaki yeni gizemlere odaklanalım!", false, 'world_map_both_solved');
                            } else if (window.gizemliSolved) {
                                showCinematicHelper("Tebrikler Amirims! Gizemli Kasaba davasını başarıyla çözdük ve Gölge Şehir'in kilidini açtık. Hazır olduğunuzda yeni kasabaya geçelim!", false, 'world_map_gizemli_solved');
                            }
                        }, 800);
                    };

                    resultModal.classList.remove('hidden');
                } else {
                    resultIcon.className = 'result-icon fail';
                    resultIcon.innerHTML = '<i class="fa-solid fa-skull-crossbones"></i>';
                    resultTitle.textContent = 'KAYBETTİNİZ!';
                    resultTitle.style.color = 'var(--danger)';
                    resultMessage.innerHTML = `
                    <div class="result-verdict">
                        <div class="result-verdict-line"><i class="fa-solid fa-user-xmark" style="color:var(--danger)"></i> Sizin Seçiminiz: <strong>${accusedNpc.name}</strong> <span style="color:var(--danger)">(MASUM)</span></div>
                        <div class="result-verdict-line"><i class="fa-solid fa-handcuffs" style="color:var(--accent)"></i> Gerçek Katil: <strong>${realKiller.name}</strong></div>
                        <div class="result-verdict-wrong"><i class="fa-solid fa-circle-xmark"></i> YANLIŞ TAHMİN!</div>
                    </div>
                    <div class="result-story">
                        <div class="result-story-header"><i class="fa-solid fa-book-skull"></i> O Gecenin Hikayesi</div>
                        <div class="result-story-text">${realKiller.murderStory}</div>
                    </div>`;
                    retryBtn.innerHTML = '<i class="fa-solid fa-rotate-right"></i> Haritaya Dön ve Tekrar Dene';
                    retryBtn.classList.remove('hidden');

                    retryBtn.onclick = () => {
                        resultModal.classList.add('hidden');
                        triggerTransition(() => {
                            document.getElementById('town-map-screen')?.classList.add('hidden');
                            if (interiorScreen) interiorScreen.classList.add('hidden');
                            if (npcTalkModal) npcTalkModal.classList.add('hidden');
                            if (foundModal) foundModal.classList.add('hidden');
                            document.getElementById('world-map-screen')?.classList.remove('hidden');

                            // Kasaba durumunu temizle
                            window.currentActiveTown = 'gizemli';
                            document.body.classList.remove('golge-sehir-theme');
                            visitedBuildings.clear();
                            if (window.GolgeSehirEngine) {
                                window.GolgeSehirEngine.visitedGolgeBuildings.clear();
                                window.GolgeSehirEngine.clearGolgeSehirMap();
                                window.GolgeSehirEngine.resetGolgeState();
                            }
                            currentBag = [];
                            submittedBloodClueIds.clear();
                            submittedPrintClueIds.clear();
                            submittedForensicCount = 0;
                            submittedForensicCountGizemli = 0;
                            submittedForensicCountGolge = 0;
                            isAutopsyTimerStarted = false;
                            isAutopsyReady = false;
                            autopsyTimeLeft = 60;
                            if (autopsyTimer) clearInterval(autopsyTimer);
                            dialogHistory = {};
                            npcTalkCompleted = {};
                            activeNpcId = null;
                            updateForensicBadge();
                            checkAutopsyConditions();
                        });
                        stopAllSounds();
                    };

                    resultModal.classList.remove('hidden');
                }
            }, 3000);
        })
        .catch(err => {
            console.error('Accuse Error:', err);
            jailOverlay.classList.add('hidden');
            showGlobalNotification("HATA", "Sunucu bağlantısı sırasında bir hata oluştu.", true);
        });
};

// Masum seçilen NPC'lerin oturum boyunca kalıcı tutulduğu küme
let innocentNpcIds = new Set();
window.innocentNpcIds = innocentNpcIds;

window.innocentNpc = function (npcId) {
    if (!npcId) return;
    innocentNpcIds.add(npcId);

    const npc = (window.NPC_DATA && window.NPC_DATA[npcId]) || NPC_DATA[npcId];
    const npcName = npc ? npc.name : `Şüpheli #${npcId}`;

    // Ekrandaki kartı anında güncelle
    const card = document.querySelector(`.found-npc-card .found-npc-img[data-npc-id="${npcId}"]`)?.closest('.found-npc-card');
    if (card) {
        card.classList.add('innocent-marked');
        if (!card.querySelector('.innocent-stamp')) {
            const stamp = document.createElement('div');
            stamp.className = 'innocent-stamp';
            stamp.textContent = 'MASUM';
            card.appendChild(stamp);
        }
        const successBtn = card.querySelector('.btn-success');
        if (successBtn) {
            successBtn.innerHTML = '<i class="fa-solid fa-shield-check"></i> MASUM İLAN EDİLDİ';
        }
    }

    // Bina üzerindeki etiketi güncelle
    const bldDiv = document.querySelector(`.map-building[data-npc-id="${npcId}"]`);
    if (bldDiv) {
        const hoverTag = bldDiv.querySelector('.building-hover-tag');
        if (hoverTag) {
            hoverTag.innerHTML = `<i class="fa-solid fa-shield-halved" style="color:#22c55e;"></i> ${npcName.toUpperCase()} MASUM`;
        }
    }

    showGlobalNotification("MASUM İLAN EDİLDİ", `${npcName} soruşturma dosyasında masum olarak arşivlendi. Yeni bir vaka açılana kadar bu şüpheli masum kalacaktır ve sorgulanamaz.`, false);
    logAction('mark_innocent', npcId, npcName);
};

// === GLOBAL RETRY / RETURN TO MAP ===
document.getElementById('result-retry-btn')?.addEventListener('click', () => {
    resultModal.classList.add('hidden');
    triggerTransition(() => {
        townMapScreen.classList.add('hidden');
        interiorScreen.classList.add('hidden');
        npcTalkModal.classList.add('hidden');
        foundModal.classList.add('hidden');
        worldMapScreen.classList.remove('hidden');
    });
    stopAllSounds();
});

// =============================================================
// 8. EXIT TO MAIN MENU
// =============================================================



// =============================================================
// 9. YARDIMCI DEDEKTİF (ÇETİN) — SİNEMATİK BAĞLAM SİSTEMİ
// =============================================================

const HELPER_TIPS = [
    "Amirims! Kasap Hasan'ın dükkânındaki kara kaplı veresiye defterine dikkat ettiniz mi? Kurbanın adı çizilmiş!",
    "Eczacı Selma sarmaşıklardan ilaç yaptığını söylüyor ama tezgâh altındaki şişe zehir barındırıyor olabilir amirims.",
    "Muhtar Kemal arazi anlaşmazlıklarını reddediyor ama kasasında sahte tapu evrakları bulduk!",
    "Komiser Güneş olay yerindeki delilleri gizli tutmaya çalışıyor. Polis rozetini sorgulayın amirims!",
    "Terzi Yahya ceketlerin astarına gizli cepler diker. Kurbanın son ceketindeki notu ve USB'yi arayın!",
    "Amirims, çantanıza aldığınız ipuçlarını 'İncele' butonuna basarak detaylı okuyabilirsiniz. Şifreler orada gizli!"
];

let currentTipIndex = 0;


// Fallback mesajlar (API çalışmazsa)
const HELPER_FALLBACK_MESSAGES = {
    'splash': 'Hoş geldin Amirims! Ben Yardımcı Dedektif Çetin. Bu karanlık davada sana yardımcı olacağım. Hazır olduğunda dosyayı aç ve soruşturmaya başlayalım!',
    'story_end': 'Soruşturmaya başlamadan önce şunu bil Amirims: Kasabada binaları gezmeli, delilleri toplamalı ve şüphelileri sorgulamalısınız. Katili kesin olarak belirlediğinizde, ana haritanın sağ altında sabit duran kırmızı \'BULDUM!\' butonuna basarak mahkemeye sevk edebilirsiniz!',
    'map_enter': 'İşte kasaba haritası Amirims! Binalara girip delilleri toplayın ve şüphelileri sorgulayın. Katili tespit ettiğinizde haritanın sağ alt köşesinde sabit yer alan kırmızı \'BULDUM!\' butonuyla suçlamanızı yapabilirsiniz!',
    'building_enter': 'Olay yerindeki delilleri inceleyebilir, çantana atabilirsin. Karakterle konuşmak için sağ alttaki konuşma butonuna tıklayabilirsin!',
    'bag_open': 'Çantandaki delilleri İncele butonuyla detaylı inceleyebilirsin Amirims. Suçluyu bulmak için ipuçlarını birleştir!',
    'clue_inspect': 'Bu delili dikkatle incele Amirims. Suçluya ait olabilecek izler görebilirsin!',
    'npc_talk': 'Dikkatli soru sor Amirims, şüphelinin tepkilerini iyi analiz et!',
    'autopsy_ready': 'Amirims! Adli Tıp Merkezi\'nden otopsi raporu geldi! Haritadaki otopsi butonuna basıp hemen inceleyin!',
    'accuse': 'Son kararını vermeden önce tüm delilleri gözden geçir Amirims. Yanlış suçlama kasaba için felaket olur!'
};

// Bina-bazlı fallback mesajlar
const BUILDING_HELPER_MESSAGES = {
    'Kasap': 'Burası Kasap Hasan\'ın dükkânı Amirims. Tezgahtaki satıra, deftere ve yırtık önlüğe dikkat et. Hasan sert bir adam, ama gözlerinde korku var...',
    'Eczane': 'Eczacı Selma\'nın dükkânındayız Amirims. Tezgah altına, reçete defterine ve ilaç şişelerine dikkat et. Bu kadın zehirler konusunda uzman...',
    'Muhtarlık': 'Muhtar Kemal\'in ofisindeyiz Amirims. Çekmecesindeki mektuplara, kırık gözlüğe ve kasasına dikkat et. Bu adam her şeyi kontrol etmek istiyor...',
    'Karakol': 'Komiser Güneş\'in karakolundayız Amirims. Polis rozetine, gizli dosyaya ve kayıp düğmeye dikkat et. Bir polis neden delilleri saklasın ki?',
    'Terzi': 'Terzi Yahya\'nın atölyesindeyiz Amirims. İplik makarasına, yırtık kumaşa ve gizli cebe dikkat et. Bu yaşlı adam bildiklerinden fazlasını saklıyor...'
};

/**
 * Sinematik diyalog kutusunu gösterir — daktilo efektiyle ve çift yardımcı (Çetin / Bekçi) desteğiyle
 * @param {string} message - Gösterilecek mesaj
 * @param {boolean} isOneTime - Sadece bir kez mi gösterilecek
 * @param {string} contextKey - Bağlam anahtarı (tekrar gösterimi engellemek için)
 * @param {boolean} skipHistory - Geçmişe ekleme atlanacak mı
 * @param {object} speakerInfo - Konuşmacı bilgisi { speaker, speakerName, avatar, theme }
 */
function showCinematicHelper(message, isOneTime = true, contextKey = '', skipHistory = false, speakerInfo = null) {
    if (isOneTime && contextKey && shownCinematicContexts.has(contextKey)) return;
    if (isOneTime && contextKey) shownCinematicContexts.add(contextKey);

    const box = document.getElementById('cinematic-helper-box');
    const textEl = document.getElementById('cinematic-helper-text');
    const avatarImg = document.querySelector('.cinematic-helper-avatar img');
    const nameEl = document.querySelector('.cinematic-helper-name');

    if (!box || !textEl) return;

    // Apply speaker details if provided
    if (speakerInfo) {
        if (speakerInfo.theme) {
            box.classList.remove('rifat-speaking', 'cetin-speaking');
            box.classList.add(speakerInfo.theme);
        }
        if (speakerInfo.avatar && avatarImg) {
            avatarImg.src = speakerInfo.avatar;
        }
        if (speakerInfo.speakerName && nameEl) {
            nameEl.textContent = speakerInfo.speakerName;
        }
    } else {
        // Default to Çetin
        box.classList.remove('rifat-speaking');
        box.classList.add('cetin-speaking');
        if (avatarImg) avatarImg.src = 'images/dedektif_helper.png';
        if (nameEl) nameEl.textContent = 'YARDIMCI DEDEKTİF ÇETİN';
    }

    if (!skipHistory) {
        const historyItem = {
            text: message,
            speakerInfo: speakerInfo ? { ...speakerInfo } : {
                speaker: 'cetin',
                speakerName: nameEl ? nameEl.textContent : 'YARDIMCI DEDEKTİF ÇETİN',
                avatar: avatarImg ? avatarImg.src : 'images/dedektif_helper.png',
                theme: box.classList.contains('rifat-speaking') ? 'rifat-speaking' : 'cetin-speaking'
            }
        };
        helperMessageHistory.push(historyItem);
        currentHelperHistoryIndex = helperMessageHistory.length - 1;
    }

    // Update prev button visibility
    const prevBtn = document.getElementById('cinematic-prev-btn');
    if (prevBtn) {
        prevBtn.style.display = currentHelperHistoryIndex > 0 ? 'inline-block' : 'none';
    }

    // Clear existing
    if (cinematicTypewriterTimeout) clearTimeout(cinematicTypewriterTimeout);
    textEl.textContent = '';
    textEl.classList.remove('typing-done');
    currentHelperMessageText = message;
    isHelperTyping = true;

    box.classList.remove('hidden');
    document.body.classList.add('helper-box-active');
    const talkBtn = document.getElementById('talk-npc-btn');
    if (talkBtn) {
        talkBtn.classList.add('floating-above-helper');
        // Kutu yüksekliğini ölçüp CSS değişkenine ata
        setTimeout(() => {
            const h = box.offsetHeight || 175;
            document.documentElement.style.setProperty('--helper-box-height', `${h}px`);
        }, 50);
    }

    document.getElementById('interior-helper-btn')?.classList.add('hidden');
    document.getElementById('town-helper-btn')?.classList.add('hidden');

    let i = 0;
    const speed = 26;
    function typeChar() {
        if (!isHelperTyping) return; // if skipped
        if (i < message.length) {
            textEl.textContent += message.charAt(i);
            i++;
            cinematicTypewriterTimeout = setTimeout(typeChar, speed);
        } else {
            isHelperTyping = false;
            textEl.classList.add('typing-done');
        }
    }
    typeChar();
}

/**
 * Bağlama göre Çetin'in mesajını API'den çeker ve sinematik kutuyu gösterir
 */
function triggerHelperMessage(context, building = null, isOneTime = true) {
    const contextKey = building ? `${context}_${building}` : context;
    if (isOneTime && shownCinematicContexts.has(contextKey)) return;

    const params = new URLSearchParams({ context });
    if (building) params.append('building', building);

    fetch(`/api/game/helper/tip?${params}`)
        .then(res => {
            if (!res.ok) throw new Error('API hatası');
            return res.json();
        })
        .then(data => {
            if (data.success && data.message) {
                showCinematicHelper(data.message, isOneTime, contextKey);
            } else {
                // API yanıt verdi ama mesaj yok — fallback kullan
                let msg = '';
                if (building && BUILDING_HELPER_MESSAGES[building]) {
                    msg = BUILDING_HELPER_MESSAGES[building];
                } else {
                    msg = HELPER_FALLBACK_MESSAGES[context] || 'Amirims, soruşturmaya devam edin!';
                }
                showCinematicHelper(msg, isOneTime, contextKey);
            }
        })
        .catch(() => {
            let msg = '';
            if (building && BUILDING_HELPER_MESSAGES[building]) {
                msg = BUILDING_HELPER_MESSAGES[building];
            } else {
                msg = HELPER_FALLBACK_MESSAGES[context] || 'Amirims, soruşturmaya devam edin!';
            }
            showCinematicHelper(msg, isOneTime, contextKey);
        });
}

// Helper Butonları & Akıllı Mekan İpucu Sistem
function provideBuildingSpecificHint(npcId = activeNpcId) {
    if (!npcId || !NPC_DATA[npcId]) {
        showCinematicHelper('Amirims, mekan içerisindeki delilleri ve ipuçlarını öğrenmek için binalardan birine girmeliyiz!', false);
        return;
    }
    const npc = NPC_DATA[npcId];
    const objects = SCENE_OBJECTS[npcId] || [];
    const clueNames = objects.map(o => `'${o.name}'`).join(', ');

    let hintMsg = `Amirims, ${npc.building} binasındayız! Odada incelenebilecek deliller: ${clueNames}. `;
    if (npcId === 1) { // Kasap
        hintMsg += `Tezgaha saplanan Kanlı Satır'daki kan izlerine, Kara Kaplı Defter'e ve Yırtık Önlük'e dikkat et. Şüpheli sırrı: "${npc.secret}"`;
    } else if (npcId === 2) { // Eczane
        hintMsg += `Tezgah altındaki Zehirli Sarmaşık'a, Boş İlaç Şişesi'ne ve Reçete Defteri'ne odaklan. Şüpheli sırrı: "${npc.secret}"`;
    } else if (npcId === 3) { // Muhtarlık
        hintMsg += `Çekmecedeki Tehdit Mektubu'na, Tablonun arkasındaki Gizli Kasa'ya ve yerdeki Kırık Gözlük'e dikkat et. Şüpheli sırrı: "${npc.secret}"`;
    } else if (npcId === 4) { // Karakol
        hintMsg += `Masadaki 'GİZLİ' damgalı dosyaya, Polis Rozeti'ne ve paltonun Kopmuş Düğmesi'ne odaklan. Şüpheli sırrı: "${npc.secret}"`;
    } else if (npcId === 5) { // Terzi
        hintMsg += `Kanlı İplik Makarası'na, Yırtık Kumaş parçasına ve ceketin astarındaki Gizli Cep'e dikkat et. Şüpheli sırrı: "${npc.secret}"`;
    }
    showCinematicHelper(hintMsg, false);
}

// Balon Kapatma
document.getElementById('cinematic-helper-close')?.addEventListener('click', () => {
    const box = document.getElementById('cinematic-helper-box');
    if (box) box.classList.add('hidden');
    document.body.classList.remove('helper-box-active');
    const talkBtn = document.getElementById('talk-npc-btn');
    if (talkBtn) talkBtn.classList.remove('floating-above-helper');

    document.getElementById('interior-helper-btn')?.classList.remove('hidden');
    document.getElementById('town-helper-btn')?.classList.remove('hidden');
    isHelperTyping = false;
    if (cinematicTypewriterTimeout) clearTimeout(cinematicTypewriterTimeout);
});
document.getElementById('close-helper-btn')?.addEventListener('click', () => {
    const modal = document.getElementById('helper-modal');
    if (modal) modal.classList.add('hidden');
});

// Avatar Tıklaması ile Balonu Tekrar Açma ve Daktilo Yazısını Tetikleme
function reopenHelperSpeechBubble() {
    const box = document.getElementById('cinematic-helper-box');
    if (box) {
        box.classList.remove('hidden');
        document.getElementById('interior-helper-btn')?.classList.add('hidden');
        document.getElementById('town-helper-btn')?.classList.add('hidden');

        isHelperTyping = false;
        if (cinematicTypewriterTimeout) clearTimeout(cinematicTypewriterTimeout);

        if (currentHelperMessageText) {
            showCinematicHelper(currentHelperMessageText, false, '', true);
        } else if (activeNpcId) {
            provideBuildingSpecificHint(activeNpcId);
        } else {
            triggerHelperMessage('map_enter', null, false);
        }
    }
}
document.getElementById('interior-helper-btn')?.addEventListener('click', reopenHelperSpeechBubble);
document.getElementById('town-helper-btn')?.addEventListener('click', reopenHelperSpeechBubble);
document.querySelector('.cinematic-helper-avatar')?.addEventListener('click', reopenHelperSpeechBubble);

// Önceki Mesaj
document.getElementById('cinematic-prev-btn')?.addEventListener('click', () => {
    if (currentHelperHistoryIndex > 0) {
        currentHelperHistoryIndex--;
        const item = helperMessageHistory[currentHelperHistoryIndex];
        if (item && typeof item === 'object' && item.text) {
            showCinematicHelper(item.text, false, '', true, item.speakerInfo);
        } else {
            showCinematicHelper(item, false, '', true);
        }
    }
});

// Yazı Geçme
document.getElementById('cinematic-skip-btn')?.addEventListener('click', () => {
    if (isHelperTyping) {
        isHelperTyping = false;
        if (cinematicTypewriterTimeout) clearTimeout(cinematicTypewriterTimeout);
        const textEl = document.getElementById('cinematic-helper-text');
        if (textEl) {
            textEl.textContent = currentHelperMessageText;
            textEl.classList.add('typing-done');
        }
    } else if (currentHelperHistoryIndex < helperMessageHistory.length - 1) {
        currentHelperHistoryIndex++;
        const item = helperMessageHistory[currentHelperHistoryIndex];
        if (item && typeof item === 'object' && item.text) {
            showCinematicHelper(item.text, false, '', true, item.speakerInfo);
        } else {
            showCinematicHelper(item, false, '', true);
        }
    }
});

// İpucu Butonları (Sinematik Balon & Modal)
document.getElementById('cinematic-tip-btn')?.addEventListener('click', () => provideBuildingSpecificHint(activeNpcId));
document.getElementById('helper-tip-btn')?.addEventListener('click', () => provideBuildingSpecificHint(activeNpcId));

// Çanta Butonları (Modal)
function handleHelperBagClick() {
    openBag();
    if (currentBag.length === 0) {
        showCinematicHelper(`Amirims, çantamız henüz bomboş (0/${MAX_BAG_SIZE} delil)! Binalara girip nesnelere tıklayarak delilleri toplayın. Hatırlatırım, çantanıza yalnızca ${MAX_BAG_SIZE} delil alabilirsiniz, dikkatli seçin!`, false);
    } else {
        const names = currentBag.map(c => `'${c.name}'`).join(', ');
        const inspectedCount = currentBag.filter(c => c.inspected).length;
        let msg = `Amirims, çantamızda ${currentBag.length}/${MAX_BAG_SIZE} delil var: ${names}. `;
        if (inspectedCount > 0) {
            msg += `${inspectedCount} tanesi zaten incelendi. `;
        }
        if (inspectedCount < currentBag.length) {
            msg += `Henüz incelenmemiş deliller var, 'İncele' butonuyla detaylı incelemeyi unutmayın!`;
        } else {
            msg += `Tüm deliller incelendi, artık şüphelileri sorgulayıp katili bulmaya odaklanmalıyız!`;
        }
        showCinematicHelper(msg, false);
    }
}
// cinematic-open-bag-btn kaldırıldı, sadece modal butonundan çalışır
document.getElementById('helper-open-bag-btn')?.addEventListener('click', handleHelperBagClick);

// Delil Analiz Butonları (Sinematik Balon & Modal)
// FIX: Her zaman çantadaki (currentBag) delilleri analiz et, SCENE_OBJECTS değil
function handleHelperAnalyzeClick() {
    const textEl = document.getElementById('cinematic-helper-text');
    if (textEl) textEl.textContent = 'Deliller analiz ediliyor...';

    // Her zaman çantadaki gerçek delilleri kullan
    const clueIds = currentBag.map(c => c.id);

    // Çantada hiç delil yoksa direkt bildir
    if (clueIds.length === 0) {
        showCinematicHelper('Amirims, çantamızda henüz hiçbir delil yok! Binalara girin, nesnelere tıklayın ve delilleri çantaya alın. Analiz edecek bir şeyimiz olsun önce!', false);
        return;
    }

    // Çantadaki delillerin isimlerini listele
    const clueNames = currentBag.map(c => `'${c.name}'`).join(', ');

    fetch('/api/game/helper/analyze-clues', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ClueIds: clueIds })
    })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                showCinematicHelper(data.analysis, false);
            } else {
                showCinematicHelper(`Amirims, çantamızdaki ${currentBag.length} delili inceledim: ${clueNames}. Bunları şüphelilerin ifadeleriyle karşılaştırmanızı öneririm!`, false);
            }
        })
        .catch(() => {
            // Backend erişilemezse bile çantadaki bilgiyi göster
            const inspectedCount = currentBag.filter(c => c.inspected).length;
            let msg = `Amirims, çantamızda toplam ${currentBag.length}/${MAX_BAG_SIZE} delil var: ${clueNames}. `;
            if (inspectedCount > 0) {
                msg += `Bunlardan ${inspectedCount} tanesi incelendi. `;
            }
            if (currentBag.length < 3) {
                msg += 'Daha fazla delil toplamak için diğer binaları da ziyaret edin!';
            } else {
                msg += 'Deliller katile işaret ediyor olabilir, şüphelilerin ifadeleriyle karşılaştırın!';
            }
            showCinematicHelper(msg, false);
        });
}
document.getElementById('cinematic-analyze-btn')?.addEventListener('click', handleHelperAnalyzeClick);
document.getElementById('helper-analyze-btn')?.addEventListener('click', handleHelperAnalyzeClick);

// =============================================================
// 10. BACKEND API ENTEGRASYONU — OTURUM & AKSİYON KAYDI
// =============================================================

/**
 * Backend'e oyuncu aksiyonu kaydeder (fire-and-forget)
 */
function logAction(actionType, targetId = null, details = null) {
    if (!currentSessionId) return;
    fetch('/api/game/action/log', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ SessionId: currentSessionId, ActionType: actionType, TargetId: targetId, Details: details })
    }).catch(() => { }); // Sessizce hata yut
}

/**
 * Backend'e NPC diyalog kaydı yazar
 */
function logDialog(npcId, playerQuestion, npcResponse, difficulty = 1, category = 'tanisma') {
    fetch('/api/game/dialog/log', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ NpcId: npcId, PlayerQuestion: playerQuestion, NpcResponse: npcResponse, Difficulty: difficulty, Category: category })
    }).catch(() => { });
}

/**
 * Oyun durumunu kaydet
 */
function saveGameState() {
    if (!currentSessionId) return;
    const state = {
        bag: currentBag.map(c => c.id),
        visitedBuildings: [...visitedBuildings],
        dialogHistory: dialogHistory,
        askedQuestionCount: askedQuestionCount,
        npcStressLevels: npcStressLevels,
        guiltyNpcId: guiltyNpcId
    };
    fetch('/api/game/state/save', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ SessionId: currentSessionId, StateData: JSON.stringify(state) })
    }).catch(() => { });
}

// =============================================================
// UTILITY: TRANSITION
// =============================================================

function triggerTransition(callback) {
    transitionOverlay.classList.add('flash');
    setTimeout(() => {
        callback();
        setTimeout(() => transitionOverlay.classList.remove('flash'), 300);
    }, 500);
}

// =============================================================
// 11. DEDEKTİF ÇANTASI (ENVANTER) VE KANITLAR
// =============================================================

function checkAndDropClues(npcId, clueIds) {
    let newCluesFound = false;
    const npcSceneObjects = SCENE_OBJECTS[npcId];
    if (!npcSceneObjects) return;

    clueIds.forEach(clueId => {
        // Çanta limiti kontrolü (Maksimum MAX_BAG_SIZE / 5)
        if (currentBag.length >= MAX_BAG_SIZE) return;

        // Zaten çantada var mı?
        if (currentBag.some(c => c.id === clueId)) return;

        // Clue bilgisini SCENE_OBJECTS içinden bul
        const clueInfo = npcSceneObjects.find(obj => obj.id === clueId);
        if (clueInfo) {
            currentBag.push(clueInfo);
            window.currentBag = currentBag;
            newCluesFound = true;
        }
    });

    if (newCluesFound) {
        showClueBadge();
        saveGameState();
        checkAutopsyConditions();
        // Sinematik bildirim
        triggerHelperMessage('clue_found', null, false);
    }
}

function saveTestimonyToBag(npcName, testimonyText) {
    if (currentBag.length >= MAX_BAG_SIZE) {
        showCinematicHelper("Amirim! Çantamız dolu. Önce çantadaki delilleri Adli Tıp laboratuvarına incelemeye göndermelisiniz.", false);
        return;
    }

    // Generate a unique ID for the testimony
    const testimonyId = 1000 + currentBag.length;
    const testimonyClue = {
        id: testimonyId,
        name: `${npcName}'nin İfadesi`,
        desc: `"${testimonyText}"`,
        img: 'images/helper_avatar.png', // Temporary generic icon or a custom testimony icon
        scene: activeNpcId
    };

    currentBag.push(testimonyClue);
    window.currentBag = currentBag;
    showClueBadge();
    triggerHelperMessage('testimony_saved', `Amirims, ${npcName} karakterinin bu sözünü kaydettim! Diğer şüphelilere bu ifadeyi kanıt olarak sunabilirsiniz.`, true);
    saveGameState();
}

function showClueBadge() {
    const badge = document.getElementById('clue-badge');
    if (badge) {
        badge.classList.remove('hidden');
    }
}

function hideClueBadge() {
    const badge = document.getElementById('clue-badge');
    if (badge) {
        badge.classList.add('hidden');
    }
}

// Çanta butonunu dinle
document.getElementById('open-bag-floating-btn')?.addEventListener('click', openBagModal);
document.getElementById('close-bag-x-btn')?.addEventListener('click', closeBagModal);
document.getElementById('close-bag-btn')?.addEventListener('click', closeBagModal);

function openBagModal() {
    hideClueBadge();
    const bagModal = document.getElementById('bag-modal');
    if (!bagModal) return;

    renderBagItems();
    bagModal.classList.remove('hidden');
}

function closeBagModal() {
    const bagModal = document.getElementById('bag-modal');
    if (bagModal) bagModal.classList.add('hidden');
}

function renderBagItems() {
    const container = document.getElementById('bag-items-list');
    if (!container) return;

    container.innerHTML = '';

    if (currentBag.length === 0) {
        container.innerHTML = '<p style="color:var(--text-muted); text-align:center;">Henüz hiçbir fiziksel kanıt bulamadınız.</p>';
        return;
    }

    currentBag.forEach(item => {
        const itemEl = document.createElement('div');
        itemEl.className = 'bag-item';
        itemEl.style.display = 'flex';
        itemEl.style.alignItems = 'center';
        itemEl.style.gap = '15px';
        itemEl.style.padding = '10px';
        itemEl.style.border = '1px solid var(--border-color)';
        itemEl.style.borderRadius = '8px';
        itemEl.style.marginBottom = '10px';
        itemEl.style.cursor = 'pointer';
        itemEl.style.background = 'rgba(255,255,255,0.05)';

        itemEl.innerHTML = `
            <img src="${item.img}" style="width: 50px; height: 50px; object-fit: cover; border-radius: 4px;" alt="Kanıt">
            <div>
                <div style="font-weight: bold; color: var(--accent); font-family: var(--font-heading);">${item.name}</div>
                <div style="font-size: 0.8rem; color: var(--text-muted);">${item.desc.substring(0, 40)}...</div>
            </div>
        `;

        itemEl.addEventListener('click', () => showDetailedClue(item));
        container.appendChild(itemEl);
    });
}

function showDetailedClue(item) {
    const itemNpcId = item.scene || item.relatedNpcId || 1;
    openClueInspect(item, itemNpcId, true);
}

document.getElementById('close-detailed-clue-btn')?.addEventListener('click', () => {
    document.getElementById('detailed-clue-modal')?.classList.add('hidden');
});

// === THUNDER EFFECT ===
function startThunder() {
    setInterval(() => {
        if (!isMuted && thunderSound) {
            playSound(thunderSound, Math.random() * 0.4 + 0.3); // 0.3 - 0.7 volume
            // Flash effect for immersion
            const canvas = document.getElementById('town-map-canvas');
            if (canvas && !townMapScreen.classList.contains('hidden')) {
                const overlay = document.createElement('div');
                overlay.style.position = 'absolute';
                overlay.style.top = 0; overlay.style.left = 0; overlay.style.right = 0; overlay.style.bottom = 0;
                overlay.style.backgroundColor = 'rgba(255,255,255,0.4)';
                overlay.style.pointerEvents = 'none';
                overlay.style.zIndex = 9999;
                overlay.style.transition = 'opacity 0.2s';
                canvas.appendChild(overlay);
                setTimeout(() => { overlay.style.opacity = 0; }, 100);
                setTimeout(() => { overlay.remove(); }, 300);
            }
        }
    }, Math.random() * 20000 + 10000); // 10-30 seconds
}

// Start thunder immediately when game starts
startThunder();
