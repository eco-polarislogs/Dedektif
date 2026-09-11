-- ============================================================================
-- GÖLGE ÅEHİR (SHADOW CITY) SEQUEL LEVEL DATABASE SCHEMA & SEED DATA
-- Fully isolated from Gizemli Kasaba tables.
-- 8 NPCs (IDs 101-108), 32 Clues (4 per building), 160 Dialogues (20 per NPC)
-- Dual Assistant: Çetin + Bekçi Rıfat
-- ============================================================================

-- ============================================================================
-- TABLE DEFINITIONS
-- ============================================================================

CREATE TABLE IF NOT EXISTS GolgeSehirNPCs (
    NPCId INTEGER PRIMARY KEY,
    Name TEXT NOT NULL,
    BuildingName TEXT NOT NULL,
    Role TEXT NOT NULL,
    TrustLevel INTEGER NOT NULL DEFAULT 50,
    FearLevel INTEGER NOT NULL DEFAULT 30,
    IsGuilty INTEGER NOT NULL DEFAULT 0,
    SecretInfo TEXT NOT NULL DEFAULT '',
    IsActive INTEGER NOT NULL DEFAULT 1,
    ImageFile TEXT NOT NULL DEFAULT '',
    InteriorFile TEXT NOT NULL DEFAULT '',
    Personality TEXT NOT NULL DEFAULT 'normal'
);

CREATE TABLE IF NOT EXISTS GolgeSehirNPCDialogues (
    DialogueId INTEGER PRIMARY KEY AUTOINCREMENT,
    NPCId INTEGER NOT NULL,
    QuestionText TEXT NOT NULL,
    ResponseText TEXT NOT NULL,
    GuiltyResponseText TEXT NOT NULL,
    TrustRequirement INTEGER DEFAULT 0,
    Stage INTEGER DEFAULT 1,
    Difficulty INTEGER NOT NULL DEFAULT 1,
    Category TEXT NOT NULL DEFAULT 'tanisma'
);

CREATE TABLE IF NOT EXISTS GolgeSehirClues (
    ClueId INTEGER PRIMARY KEY AUTOINCREMENT,
    ClueCode TEXT UNIQUE NOT NULL,
    BuildingId TEXT NOT NULL,
    NPCId INTEGER NOT NULL,
    Name TEXT NOT NULL,
    Description TEXT NOT NULL,
    ForensicResultText TEXT NOT NULL,
    ImagePath TEXT NOT NULL,
    PosTop TEXT NOT NULL DEFAULT '50%',
    PosLeft TEXT NOT NULL DEFAULT '50%',
    IsHidden INTEGER NOT NULL DEFAULT 1,
    Status TEXT NOT NULL DEFAULT 'Pending'
);

CREATE TABLE IF NOT EXISTS GolgeSehirNPCRelationships (
    RelationId INTEGER PRIMARY KEY AUTOINCREMENT,
    NPC1Id INTEGER NOT NULL,
    NPC2Id INTEGER NOT NULL,
    RelationType TEXT NOT NULL DEFAULT 'neutral',
    Description TEXT DEFAULT ''
);

CREATE TABLE IF NOT EXISTS GolgeSehirScenarioHints (
    HintId INTEGER PRIMARY KEY AUTOINCREMENT,
    GuiltyNPCId INTEGER NOT NULL,
    HintText TEXT NOT NULL,
    HintType TEXT NOT NULL DEFAULT 'clue',
    RevealOrder INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS GolgeSehirHelperMessages (
    MessageId INTEGER PRIMARY KEY AUTOINCREMENT,
    Speaker TEXT NOT NULL DEFAULT 'cetin',
    Context TEXT NOT NULL,
    BuildingName TEXT NULL,
    Message TEXT NOT NULL,
    Priority INTEGER DEFAULT 1,
    IsOneTime INTEGER DEFAULT 1
);

CREATE TABLE IF NOT EXISTS GolgeSehirGameSessions (
    SessionId INTEGER PRIMARY KEY AUTOINCREMENT,
    GuiltyNPCId INTEGER NOT NULL,
    StartedAt TEXT NOT NULL DEFAULT (datetime('now')),
    EndedAt TEXT DEFAULT NULL,
    Result TEXT DEFAULT NULL,
    AccusedNPCId INTEGER DEFAULT NULL,
    TotalQuestions INTEGER DEFAULT 0,
    CluesCollected INTEGER DEFAULT 0
);

-- ============================================================================
-- SEED DATA: 8 NPCs (IDs 101-108) — HER BİRİ FARKLI KİÅžİLİKTE
-- ============================================================================
INSERT OR REPLACE INTO GolgeSehirNPCs (NPCId, Name, BuildingName, Role, TrustLevel, FearLevel, IsGuilty, SecretInfo, IsActive, ImageFile, InteriorFile, Personality) VALUES
(101, 'Oduncu Tahsin', 'Oduncu', 'Gölge Şehir''in sert ve sessiz oduncusu, çam ormanının bekçisi.', 50, 25, 0,
 'Cinayet gecesi ormanda gizlice kereste kaçakçılığı yapıyordu. Ekrem Bey bu durumu öğrenip onu şantaj etmeye başlamıştı. O gece Ekrem''in evine gidip tartıştılar, Tahsin öfkeyle baltasını Ekrem''in kafasına indirdi. Kanlı baltayı kulübesine saklayıp reçine ile temizlemeye çalıştı ama iz kaldı.',
 1, 'images/towns/golge_sehir/npcler/npc_101_talk.jpg', 'images/towns/golge_sehir/binalar/oduncu_interior.png', 'sert'),

(102, 'Manav Ayşe', 'Manav', 'Kasabanın neşeli ama dükkânını kaybetme korkusu yaşayan manavı.', 50, 40, 0,
 'Ekrem Bey''e büyük miktarda borçluydu ve ödeme günü yaklaşmıştı. Ekrem borcunu ödemezse dükkânını haczetmekle tehdit ediyordu. Cinayet gecesi peleriniyle Ekrem''in evine gidip tartışma sırasında onu bıçakladı. Kanıtları manav kasalarının altına sakladı.',
 1, 'images/towns/golge_sehir/npcler/npc_102_talk.jpg', 'images/towns/golge_sehir/binalar/manav_interior.png', 'neseli'),

(103, 'Demirci Kazım', 'Demirci', 'Ateşin ve çeliğin ustası, az konuşan ama çok bilen adam.', 50, 20, 0,
 'Ekrem Bey için gizlice bir çelik kasa kilidi dövmüştü. Kasanın içinde yasadışı ticaret belgeleri vardı. Ekrem, Kazım''ı bu işin ortağı olarak kullanıp sonra saf dışı bırakmaya çalışınca Kazım gece yarısı demirci ocağındaki kızgın demir çubuğuyla Ekrem''i darp etti. Kanlı mektubu ocağın arkasındaki gizli bölmede sakladı.',
 1, 'images/towns/golge_sehir/npcler/npc_103_talk.jpg', 'images/towns/golge_sehir/binalar/demirci_interior.png', 'sessiz'),

(104, 'Bakkal Naciye', 'Bakkal', 'Kasabanın en çok konuşan, herkesi tanıyan bakkalı.', 50, 35, 0,
 'Ekrem Bey yıllardır veresiye borcunu ödemiyordu ve bakkalı batırmakla tehdit ediyordu. Cinayet gecesi Naciye, fare zehri karıştırdığı tütünü Ekrem''in evine götürdü. Ekrem tütünü içtikten sonra zehirlenerek öldü. Veresiye defterindeki yırtık sayfa bu planın kanıtıydı.',
 1, 'images/towns/golge_sehir/npcler/npc_104_talk.jpg', 'images/towns/golge_sehir/binalar/bakkal_interior.png', 'geveze'),

(105, 'Hekim Sevgi', 'Hekim', 'Şifalı otlar ve zehirli bitkiler konusunda kasabanın tek uzmanı.', 50, 50, 0,
 'Ekrem Bey''e yıllardır katlanamıyordu çünkü Ekrem, Sevgi''nin geçmişteki bir tıbbi hatasını biliyordu ve onu şantaj ediyordu. Cinayet gecesi Sevgi, banotu ve karabiber karışımından hazırladığı ölümcül zehri Ekrem''in ilacına karıştırdı. Kurbanın vücudundaki mor lekeler bu nadir zehrin imzasıydı.',
 1, 'images/towns/golge_sehir/npcler/npc_105_talk.jpg', 'images/towns/golge_sehir/binalar/hekim_interior.png', 'sinirli'),

(106, 'Muhtar Cevdet', 'Muhtarlık', 'Kasabanın kurnaz ve politik muhtarı, herkesin sırrını bilen adam.', 50, 55, 0,
 'Ekrem Bey''in çam ormanındaki arazisini ele geçirmek için sahte tapu düzenlettirmişti. Ekrem bunu öğrenince Muhtar''ı ifşa etmekle tehdit etti. Cinayet gecesi Cevdet, Ekrem''in evine sızdı ve aralarında çıkan arbedede Ekrem''i ağır bir cisimle darp ederek öldürdü. Sahte tapuyu ve gizli mektubu ofisinde bıraktı.',
 1, 'images/towns/golge_sehir/npcler/npc_106_talk.jpg', 'images/towns/golge_sehir/binalar/muhtar_interior.png', 'kurnaz'),

(107, 'Fehmi Bey', 'Kasabalı Evi', 'Emekli muallim, kitap kurdu ve gece kuşu. Pencere kenarında her şeyi izler.', 50, 30, 0,
 'Ekrem Bey, Fehmi''nin kıymetli köstekli saatini çalmıştı. O saat Fehmi''nin babasından kalma hatıra idi. Cinayet gecesi saatini geri almak için Ekrem''in evine gitti, tartışma çıktı ve Fehmi öfkeyle Ekrem''i itip düşürdü. Ekrem başını sert zemine çarparak öldü. Fehmi panikle kaçtı ama köstekli saati olay yerinde düşürdü.',
 1, 'images/towns/golge_sehir/npcler/npc_107_talk.jpg', 'images/towns/golge_sehir/binalar/kasabali_evi_interior.png', 'sevimli'),

(108, 'Kunduracı Rasim', 'Ayakkabıcı', 'Ayakkabı çamurunu koklayarak insanın nereye gittiğini anlayan usta.', 50, 30, 0,
 'Ekrem Bey, Rasim''in gizlice kaçak deri ticareti yaptığını biliyordu ve onu ihbar etmekle tehdit ediyordu. Cinayet gecesi Rasim, göl kenarından Ekrem''in evine yürüdü ve onu mumlu ayakkabı ipiyle boğdu. Çamurlu çizmelerini dükkânında sakladı ama 42 numara ayak izi olay yerinde kaldı.',
 1, 'images/towns/golge_sehir/npcler/npc_108_talk.jpg', 'images/towns/golge_sehir/binalar/ayakkabici_interior.png', 'huysuz');

-- ============================================================================
-- SEED DATA: 32 CLUES (4 per building)
-- ============================================================================

-- Oduncu Tahsin (101) — 4 Delil
INSERT OR REPLACE INTO GolgeSehirClues (ClueCode, BuildingId, NPCId, Name, Description, ForensicResultText, ImagePath, PosTop, PosLeft) VALUES
('oduncu_balta', 'oduncu', 101, 'Kanlı Çam Baltası', 'Baltanın üzerinde taze reçine ve şüpheli koyu leke izleri var. Ağaç sapında parmak izi kalıntısı.', 'Baltadaki koyu lekeler insan kanıyla uyuşuyor. Reçine altında gizlenmiş parmak izi tespit edildi.', 'images/towns/golge_sehir/deliller/golge_balta.png', '65%', '30%'),
('oduncu_gunluk', 'oduncu', 101, 'Gece Kesim Defteri', 'Gece yarısı verilen gizli kereste siparişinin kayıtları yazılı. Son sayfada silinmiş notlar var.', 'Silinen notlarda "E.B. - son teslimat" yazısı UV ışığıyla okunabildi.', 'images/towns/golge_sehir/deliller/golge_defter.png', '75%', '55%'),
('oduncu_recine', 'oduncu', 101, 'Reçineli Deri Eldiven', 'Tezgâh altına düşmüş, üzerine çam reçinesi ve toprak yapışmış deri eldiven.', 'Eldiven iç yüzeyinde ter kalıntısı ve mikroskobik kan parçacıkları bulundu.', 'images/towns/golge_sehir/deliller/golge_balta.png', '55%', '20%'),
('oduncu_kutuk', 'oduncu', 101, 'Yontulmuş Çam Kütüğü', 'Üzerinde bıçakla kazınmış gizli harfler bulunan taze kesilmiş kütük.', 'Kazınan harfler "E.B. - BORÇ" olarak okundu. Bıçak izleri demirci aletleriyle uyuşuyor.', 'images/towns/golge_sehir/deliller/golge_balta.png', '80%', '75%');

-- Manav Ayşe (102) — 4 Delil
INSERT OR REPLACE INTO GolgeSehirClues (ClueCode, BuildingId, NPCId, Name, Description, ForensicResultText, ImagePath, PosTop, PosLeft) VALUES
('manav_kasa', 'manav', 102, 'Kırık Meyve Kasası', 'Kasaların altında gizlenmiş eski bir pirinç anahtar bulundu.', 'Pirinç anahtarın üzerinde parmak izi tespit edildi. Anahtar, Ekrem Bey''in çelik kasasına ait.', 'images/towns/golge_sehir/deliller/golge_sise.png', '70%', '22%'),
('manav_elma', 'manav', 102, 'Zehirli Tortulu Elma', 'Üzerine şüpheli sıvı damlatılmış, hafif morarma yapmış elma.', 'Elmadaki sıvı banotu özü karışımı içeriyor. Bu madde ölümcül dozda kullanılabilir.', 'images/towns/golge_sehir/deliller/golge_sise.png', '55%', '12%'),
('manav_pelerin', 'manav', 102, 'Yırtık Pelerin Parçası', 'Ahşap tezgâh çivisine takılı kalmış siyah çuha kumaşı.', 'Kumaş lifi cinayet gecesi olay yerinde bulunan kumaş parçasıyla birebir eşleşiyor.', 'images/towns/golge_sehir/deliller/golge_defter.png', '45%', '48%'),
('manav_kantar', 'manav', 102, 'Pirinç Kantar Notu', 'Kantarın ağırlık gözüne sıkıştırılmış borç senedi notu.', 'Notta Ekrem Bey imzası ve "son ödeme tarihi geçmiş" ibaresi var. Manav Ayşe''nın el yazısıyla kaleme alınmış.', 'images/towns/golge_sehir/deliller/golge_defter.png', '65%', '60%');

-- Demirci Kazım (103) — 4 Delil
INSERT OR REPLACE INTO GolgeSehirClues (ClueCode, BuildingId, NPCId, Name, Description, ForensicResultText, ImagePath, PosTop, PosLeft) VALUES
('demirci_ors', 'demirci', 103, 'Sıcak Örs ve Çekiç', 'Örsün üzerinde yeni dövülmüş, ay simgeli bir demir parçası duruyor.', 'Ay damgası Demirci Kazım''ın tescilli markasıdır. Kurbandaki bıçakta aynı damga mevcut.', 'images/towns/golge_sehir/deliller/golge_balta.png', '68%', '45%'),
('demirci_kilit', 'demirci', 103, 'Özel Dövme Çelik Kilit', 'Dövme çelikten yapılmış, şifreli ve özel yapım kilit.', 'Kilit Ekrem Bey''in evindeki çelik kasanın kilidiyle birebir aynı. Sipariş Kazım tarafından alınmış.', 'images/towns/golge_sehir/deliller/golge_saat.png', '75%', '70%'),
('demirci_kukurt', 'demirci', 103, 'Kükürtlü Demir Tozu', 'Körüğün dibinde birikmiş sarımsı kükürt karışımlı demir tozu.', 'Kükürt oranı olay yerinde bulunan toprak örneğiyle uyuşuyor. Demirci ocağından taşınmış olabilir.', 'images/towns/golge_sehir/deliller/golge_sise.png', '58%', '25%'),
('demirci_onluk', 'demirci', 103, 'Lekeli Deri Önlük', 'Duvarda asılı, göğüs kısmında taze yağ ve çamur lekesi olan ağır önlük.', 'Önlükteki çamur göl kenarı toprağıyla uyuşuyor. Yağ lekesi fener yağından.', 'images/towns/golge_sehir/deliller/golge_balta.png', '35%', '50%');

-- Bakkal Naciye (104) — 4 Delil
INSERT OR REPLACE INTO GolgeSehirClues (ClueCode, BuildingId, NPCId, Name, Description, ForensicResultText, ImagePath, PosTop, PosLeft) VALUES
('bakkal_defter', 'bakkal', 104, 'Kırmızı Çizgili Veresiye Defteri', 'Defterde Ekrem Bey''in adının üstü kırmızı mürekkeple çizilmiş.', 'Defterin son sayfalarından biri yırtılmış. Yırtık parçada zehir tarifi kalıntısı bulundu.', 'images/towns/golge_sehir/deliller/golge_defter.png', '80%', '50%'),
('bakkal_sise', 'bakkal', 104, 'Kırık Åurup Åişesi', 'Rafın arkasına gizlenmiş kırık cam şurup şişesi.', 'Åişe içinde fare zehri kalıntısı tespit edildi. Parmak izi silinmeye çalışılmış ama kısmi iz kalmış.', 'images/towns/golge_sehir/deliller/golge_sise.png', '40%', '20%'),
('bakkal_tutun', 'bakkal', 104, 'Maktulün Tütün Kesesi', 'Tezgâh köşesinde unutulmuş, üzerinde E.B. harfleri işlenmiş tütün kesesi.', 'Tütün kesesi içindeki tütünde eser miktarda fare zehri kalıntısı bulundu.', 'images/towns/golge_sehir/deliller/golge_cizme.png', '75%', '35%'),
('bakkal_anahtar', 'bakkal', 104, 'Tezgâhtan Düşen Anahtar', 'Ahşap döşeme aralığına sıkışmış küçük kilit anahtarı.', 'Anahtar maktulün evindeki yazı masası çekmecesine ait. Bakkal Naciye''nin parmak izi mevcut.', 'images/towns/golge_sehir/deliller/golge_saat.png', '85%', '68%');

-- Hekim Sevgi (105) — 4 Delil
INSERT OR REPLACE INTO GolgeSehirClues (ClueCode, BuildingId, NPCId, Name, Description, ForensicResultText, ImagePath, PosTop, PosLeft) VALUES
('hekim_sise', 'hekim', 105, 'Koyu Mor İksir Åişesi', 'Etiketsiz, içinde mor renkli tortulu zehirli bir sıvı bulunan cam şişe.', 'Sıvı banotu ve karabiber karışımı ölümcül bir zehir. Dozaj kurbanın ölüm raporuyla birebir uyuşuyor.', 'images/towns/golge_sehir/deliller/golge_sise.png', '60%', '70%'),
('hekim_recete', 'hekim', 105, 'Yırtık Reçete Sayfası', 'Masadaki doktor defterinden koparılmış taze reçete koçanı.', 'Yırtık sayfanın kenarında "E. Bey - banotu 3cc" yazısı okunabildi.', 'images/towns/golge_sehir/deliller/golge_defter.png', '78%', '80%'),
('hekim_ot', 'hekim', 105, 'Zehirli Banotu Kökü', 'Kurutulmuş, siyah renkli ve kokulu nadir banotu kökü parçaları.', 'Banotu kökü kurbanın kanında bulunan zehirle aynı bitki türünden. Hekim''in serasında yetiştirilmiş.', 'images/towns/golge_sehir/deliller/golge_sise.png', '35%', '25%'),
('hekim_alet', 'hekim', 105, 'Lekeli Pirinç Neşter', 'Muayene kutusu içinde lekeli duran küçük cerrahi neşter.', 'Neşterdeki leke kurbanın kanıyla uyuşuyor. Hekim''in parmak izi mevcut.', 'images/towns/golge_sehir/deliller/golge_saat.png', '75%', '60%');

-- Muhtar Cevdet (106) — 4 Delil
INSERT OR REPLACE INTO GolgeSehirClues (ClueCode, BuildingId, NPCId, Name, Description, ForensicResultText, ImagePath, PosTop, PosLeft) VALUES
('muhtar_muhur', 'muhtar', 106, 'Resmi Kasaba Mührü ve Gizli Mektup', 'Mühür basılmış ama henüz postalanmamış gizli tehdit mektubu.', 'Mektup Ekrem Bey''e hitaben yazılmış: "Arazini devretmezsen sonuçlarına katlanırsın." El yazısı Muhtar Cevdet''e ait.', 'images/towns/golge_sehir/deliller/golge_defter.png', '72%', '40%'),
('muhtar_tapu', 'muhtar', 106, 'Sahte Gölge Åehir Tapusu', 'Ekrem Bey''in çam ormanındaki arazisine ait sahte devir tapusu.', 'Tapu belgesi sahte ancak resmi mühürle basılmış. Muhtar''ın kasasından çıktı.', 'images/towns/golge_sehir/deliller/golge_defter.png', '65%', '55%'),
('muhtar_anahtar', 'muhtar', 106, 'Çelik Çekmece Anahtarı', 'Masa altındaki kilitli çekmecenin çelik anahtarı.', 'Çekmecenin içinde kurbanın mülk devir belgeleri ve gizli yazışmalar bulundu.', 'images/towns/golge_sehir/deliller/golge_saat.png', '78%', '48%'),
('muhtar_gozluk', 'muhtar', 106, 'Kırık Altın Çerçeveli Gözlük', 'Halı kenarında unutulmuş, camı çatlamış altın çerçeveli gözlük.', 'Gözlük kurbanın gözlüğüyle birebir eşleşiyor. Muhtar''ın halısında ne işi var?', 'images/towns/golge_sehir/deliller/golge_saat.png', '85%', '30%');

-- Fehmi Bey (107) — 4 Delil
INSERT OR REPLACE INTO GolgeSehirClues (ClueCode, BuildingId, NPCId, Name, Description, ForensicResultText, ImagePath, PosTop, PosLeft) VALUES
('fehmi_saat', 'kasabali_evi', 107, 'Durmuş Köstekli Saat', 'Saat tam 02:14''te durmuş. Üzerinde hafif çamur lekeleri var.', 'Saatin camında mikroskobik kan damlacıkları tespit edildi. Saat maktulün üzerinden düşmüş olabilir.', 'images/towns/golge_sehir/deliller/golge_saat.png', '65%', '60%'),
('fehmi_mektup', 'kasabali_evi', 107, 'Tehdit İçerikli Eski Mektup', 'Pencereden içeri atılmış, isimsiz tehdit satırları içeren mektup.', 'Mektuptaki el yazısı, farklı mürekkep analiziyle iki ayrı kişi tarafından yazıldığı tespit edildi.', 'images/towns/golge_sehir/deliller/golge_defter.png', '75%', '80%'),
('fehmi_fener', 'kasabali_evi', 107, 'Çamurlu Pirinç Gaz Feneri', 'Kapı arkasında duran, altı ıslak çamur kaplı küçük gaz feneri.', 'Fenerdeki çamur göl kenarı toprağıyla uyuşuyor. Cinayet gecesi kullanılmış olabilir.', 'images/towns/golge_sehir/deliller/golge_sise.png', '70%', '15%'),
('fehmi_kitap', 'kasabali_evi', 107, 'Altı Çizili Eski Kitap', 'Sehpada açık duran, cinayet saatinin yazıldığı altı çizili roman.', 'Kitabın kenarına kurşun kalemle "02:14 - sessizlik" notu düşülmüş. Fehmi''nin el yazısı.', 'images/towns/golge_sehir/deliller/golge_defter.png', '72%', '83%');

-- Kunduracı Rasim (108) — 4 Delil
INSERT OR REPLACE INTO GolgeSehirClues (ClueCode, BuildingId, NPCId, Name, Description, ForensicResultText, ImagePath, PosTop, PosLeft) VALUES
('rasim_cizme', 'ayakkabici', 108, 'Çamurlu Deri Çizme', 'Sol tabanı aşınmış, topuğunda taze göl çamuru olan ağır bot.', 'Çizmede göl kenarı çamuru ve kükürt kalıntısı bulundu. Ayak izi olay yerindeki 42 numara izle birebir eşleşiyor.', 'images/towns/golge_sehir/deliller/golge_cizme.png', '75%', '35%'),
('rasim_ip', 'ayakkabici', 108, 'Mumlu Ayakkabı İpi', 'Tezgâhta duran, boğulma izleriyle genişliği uyuşan mumlu ip yumakları.', 'İpin kalınlığı kurbanın boynundaki boğulma iziyle birebir uyuşuyor. Mum kalıntısı ayakkabıcı mumuymuş.', 'images/towns/golge_sehir/deliller/golge_cizme.png', '65%', '55%'),
('rasim_bicak', 'ayakkabici', 108, 'Kunduracı Deri Bıçağı', 'Ayakkabı derisi kesmek için kullanılan özel eğri bıçak.', 'Bıçakta kurbanın ceket derisinden lif kalıntısı bulundu. Parmak izi kısmen okunabildi.', 'images/towns/golge_sehir/deliller/golge_saat.png', '70%', '42%'),
('rasim_taban', 'ayakkabici', 108, 'Ahşap Ayakkabı Kalıbı', 'Olay yerindeki 42 numara çamurlu izle 1:1 eşleşen ahşap kalıp.', 'Kalıp ölçüleri olay yerindeki ayak iziyle milimetrik uyum gösteriyor. Rasim''in atölyesine ait.', 'images/towns/golge_sehir/deliller/golge_cizme.png', '80%', '80%');

-- ============================================================================
-- SEED DATA: NPC DIALOGUES — 20 SORU/CEVAP PER NPC (160 TOTAL)
-- Her NPC farklı kişilikte: sert, neşeli, sessiz, geveze, sinirli, kurnaz, sevimli, huysuz
-- ============================================================================

-- ODUNCU TAHSİN (101) — SERT KİÅİLİK — 20 Soru
INSERT OR REPLACE INTO GolgeSehirNPCDialogues (NPCId, QuestionText, ResponseText, GuiltyResponseText, Stage, Difficulty, Category) VALUES
(101, 'Cinayet gecesi ormanda ne göründü?', 'Gece yarısı çam ağaçlarının arasında cılız bir fener ışığı ve aceleci ayak sesleri vardı.', 'Ormana doğru koşan bendim, ama tüccar Ekrem ile alakası yoktu!', 1, 1, 'tanisma'),
(101, 'Baltanı en son ne zaman kullandın?', 'Baltamı akşam üzeri kereste yontmak için kullandım, gece kulübede kilitliydi.', 'Baltamın yerini sadece ben bilirim... Belki de o gece baltayı ben aldım.', 2, 2, 'derinlesme'),
(101, 'Maktul ile herhangi bir husumetin var mıydı?', 'Ekrem Bey kereste fiyatlarını düşürmemi istiyordu ama aramızda kavga çıkmadı.', 'Beni tehdit etti! Bütün emeğimi elimden alacaktı!', 3, 2, 'derinlesme'),
(101, 'Åüpheli birilerini fark ettin mi?', 'Gece bekçisi Rıfat dedenin feneri söndüğü an birisi bakkala doğru koşuyordu.', 'O gece bekçi Rıfat beni gördü sanmıştım ama feneri sönmüştü.', 4, 1, 'tanisma'),
(101, 'Ormandaki kereste kaçakçılığından haberin var mı?', 'Ben kereste işini yasal yollardan yaparım. Kaçakçılık benim işim değil.', '*Terler* Hangi kaçakçılık? Kim söylüyor bunları? Sallamayın!', 5, 3, 'yuzlestirme'),
(101, 'Ekrem Bey seni şantaj mı ediyordu?', 'Åantaj mı? O kadar da değil, ticari anlaşmazlıktı sadece.', '*Gözlerini kaçırır* Herkes bir şeyler bildiğini sanıyor... Ekrem beni köşeye sıkıştırmıştı.', 6, 4, 'baski'),
(101, 'Reçineli eldiveni neden saklıyorsun?', 'O eldiven iş eldivenim, saklamıyorum ki.', 'O eldivende onun kanı var... hayır, yani reçine var!', 7, 4, 'baski'),
(101, 'O gece neden bu kadar geç saate kadar dışarıdaydın?', 'Kereste taşıma gece daha kolay olur, yollar boş olunca.', 'Gece geç saate kadar... bir işim vardı ama cinayet değildi!', 8, 2, 'derinlesme'),
(101, 'Kulübendeki gizli bölmeyi açıklayabilir misin?', 'Gizli bölme mi? Sadece değerli aletlerimi sakladığım bir yer.', 'O bölmede sadece aletlerim var... başka hiçbir şey yok! Aramayın!', 9, 3, 'yuzlestirme'),
(101, 'Demirci Kazım ile aranda ne var?', 'Kazım bana balta sağlar, ben de ona odun veririm. Ticaret bu kadar.', 'Kazım biliyordur... o gece beni görmüş olabilir.', 10, 2, 'derinlesme'),
(101, 'Kütüğün üzerindeki kazınmış harfler ne anlama geliyor?', 'Hangi harfler? Ben kütüklere sadece kesim tarihi kazırım.', '*Kızarır* O harfleri ben kazımadım! Birisi beni suçlamak için yazmış!', 11, 3, 'yuzlestirme'),
(101, 'Cinayet gecesi kaç kişi ormandaydı?', 'Bildiğim kadarıyla sadece ben ve baykuşlar.', 'Bir kişi daha vardı... ama söyleyemem.', 12, 2, 'derinlesme'),
(101, 'Baltandaki koyu lekeleri nasıl açıklarsın?', 'O lekeler çam reçinesi, ormanda çalışıyoruz işte.', '*Baltayı saklamaya çalışır* O reçine! Kan değil! Reçine diyorum!', 13, 4, 'baski'),
(101, 'Gece kesim defterindeki son kayıt ne?', 'Son kesim kaydı akşam üzeri yapılan normal bir sipariş.', 'O deftere dokunmayın! İçinde... iş sırları var.', 14, 3, 'yuzlestirme'),
(101, 'Ekrem Bey''in evine hiç gittin mi?', 'Kereste teslimi için birkaç kez gittim, normal iş.', 'O gece... sadece konuşmaya gitmiştim! Kasem ederim!', 15, 3, 'yuzlestirme'),
(101, 'Orman yolunda bulunan ayak izleri sana mı ait?', 'Ormanda herkes yürür, ayak izi normaldir.', 'O izler... belki benimdir ama oraya başka sebeple gittim!', 16, 4, 'baski'),
(101, 'Fener yağını nereden aldın?', 'Bakkal Naciye''den alırım her zaman.', 'Fener yağını... o gece kullandım evet ama sadece yol aydınlatmak için!', 17, 2, 'derinlesme'),
(101, 'Sence katil kim?', 'Bu kasabada herkes şüpheli. Ama Bakkal Naciye çok sinsi bakıyor.', 'Katili bulmak sizin işiniz! Beni sıkıştırmayı bırakın!', 18, 1, 'tanisma'),
(101, 'Son sözün nedir Tahsin?', 'Ben masum bir oduncuyum. Ekmeğimi ormandan kazanırım.', '*Uzun sessizlik*... her şey göründüğü gibi değil bu kasabada.', 19, 5, 'son'),
(101, 'Masum olduğunu kanıtlayabilir misin?', 'Kanıtlarım aletlerim ve kerestelerim. Başka kanıta ihtiyacım yok.', '*Titrer* Kanıt mı? Ben... ben sadece geçimimi sağlamaya çalışıyorum!', 20, 5, 'son');

-- Manav Ayşe (102) — NEÅELİ KİÅİLİK — 20 Soru
INSERT OR REPLACE INTO GolgeSehirNPCDialogues (NPCId, QuestionText, ResponseText, GuiltyResponseText, Stage, Difficulty, Category) VALUES
(102, 'O gece pelerinli kişiyi nereye doğru giderken gördün?', 'Manav dükkânının önünden geçip doğrudan demirci ocağına doğru ilerledi.', 'Pelerinli kişi bendim! Ama kurbana yaklaşmadım bile!', 1, 1, 'tanisma'),
(102, 'Manav kasalarının arkasında saklanan bir şey var mı?', 'Sadece eski meyve kasaları ve yedek fener yağı var.', 'O kasaların altında pirinç kilit anahtarı duruyordu, kimsede olmaması gereken bir anahtar.', 2, 2, 'derinlesme'),
(102, 'Kurban cinayet gecesi sana uğradı mı?', 'Ekrem Bey akşam elma almaya geldi, neşeli görünüyordu.', 'Bana borcunu ödemeyeceğini söyledi, beni aşağıladı!', 3, 2, 'derinlesme'),
(102, 'Gölge Åehir''de garip olaylar ne zaman başladı?', 'Gölge Åehir''de sis çöktüğü ilk gece pencerelerden tıkırtılar gelmeye başladı.', 'Tüm kasaba sır saklıyor, ben sadece kendi hakkımı aldım.', 4, 1, 'tanisma'),
(102, 'Borcun ne kadardı Ekrem Bey''e?', 'Ehehe, borç mu? Biraz veresiye kalmıştı sadece, büyütecek bir şey yok.', '*Gülüşü solar* Büyük bir borçtu... Dükkânımı kapatacaktı!', 5, 3, 'yuzlestirme'),
(102, 'Pelerin sana mı ait?', 'Benim pelerinim mi? Yok canım, ben pelerin giymem, manava yakışmaz!', '*Güler gibi yapar* Pelerin dediğiniz... iş yağmurluğum, herkes giyer!', 6, 3, 'yuzlestirme'),
(102, 'Tezgâhtaki yırtık kumaşı açıklayabilir misin?', 'Oo, o kasaların çivisine takılmış eski bir çuval parçası.', 'O kumaş... pelerinden yırtılmış olabilir ama ben dükkândaydım!', 7, 4, 'baski'),
(102, 'Cinayet saatinde neredeydin?', 'Evimde portakal suyu içip erken yattım, neşeli bir akşamdı!', 'Dükkânda tezgâhı temizliyordum... dışarı sadece bir anlığına çıktım!', 8, 2, 'derinlesme'),
(102, 'Kantarın altındaki borç senedini açıklayabilir misin?', 'Aa, o senet eski bir hesap kaydı, önemli değil.', '*Güler yüzü düşer* O senet... son uyarıydı. Ödemezse...', 9, 4, 'baski'),
(102, 'Hekim Sevgi ile sık görüşür müsün?', 'Hekim Sevgi''ye haftada bir taze sebze götürürüm, iyi müşterimdir.', 'Sevgi''den... bazı şeyler aldım ama ilaçtı sadece! Zehir değil!', 10, 2, 'derinlesme'),
(102, 'Ekrem Bey''le son görüşmen nasıl geçti?', 'Gülerek ayrıldık, güzel bir elma seçmişti. Meyveciden memnundu!', '*Suskunlaşır* Son görüşmemiz... tartışmalı geçti aslında.', 11, 3, 'yuzlestirme'),
(102, 'Kasabanın en güvenilmez kişisi kim?', 'Haha! Herkes biraz dedikoducu burada ama muhtar en kurnazı!', 'Güvenilmez mi? Herkes bir şeyler gizliyor, ben hariç tabii!', 12, 1, 'tanisma'),
(102, 'Dükkânının ışığı gece kaça kadar açıktı?', 'Akşam 8''de kapatıyorum ben, erken yatarım.', 'O gece... biraz geç kapattım, stok sayıyordum ya!', 13, 2, 'derinlesme'),
(102, 'Elma üzerindeki morarma neyin morarması?', 'Aa o çürük elma, satamadıklarımı köşeye koyarım.', '*Terler* Çürük elma! Başka bir şey değil! Zehir falan yok!', 14, 4, 'baski'),
(102, 'Pirinç anahtarı nereden buldun?', 'Hangi pirinç anahtar? Dükkânda çok anahtar olur.', 'O anahtar... Ekrem''in kasasının anahtarıydı. Düşürmüştü bir gün.', 15, 3, 'yuzlestirme'),
(102, 'Seni suçluyorlar Ayşe, ne diyorsun?', 'Beni mi? Hahaha! Manav kadın cinayet mi işler? Güldürmeyin beni!', '*Güler yüzü maskeye döner* Suçlayanlar kendi günahlarını saklıyor!', 16, 4, 'baski'),
(102, 'Olay gecesi yağmur yağıyor muydu?', 'Evet, bardaktan boşalırcasına! Dükkânın önü su olmuştu.', 'Yağmur... evet, yağmur sayesinde izlerim silinmiş... yani izler silinmiştir!', 17, 1, 'tanisma'),
(102, 'Demirci Kazım gece sana geldi mi?', 'Kazım geç saatte dükkâna uğradı, sigara istedi.', 'Kazım gece geldi evet... benden bir şey almadı ama!', 18, 2, 'derinlesme'),
(102, 'Son sözün nedir Ayşe?', 'Dedektif, ben neşeli bir manavım! Cinayet benim harcım değil!', '*Sessizce*... ben sadece borcumu kurtarmaya çalıştım.', 19, 5, 'son'),
(102, 'Gerçek katili biliyorsan söyle.', 'Karnımdan konuşmam ama oduncu Tahsin o gece çok gergin görünüyordu.', 'Katili siz bulun! Ben manavım, dedektif değil!', 20, 5, 'son');

-- DEMİRCİ KAZIM (103) — SESSİZ KİÅİLİK — 20 Soru
INSERT OR REPLACE INTO GolgeSehirNPCDialogues (NPCId, QuestionText, ResponseText, GuiltyResponseText, Stage, Difficulty, Category) VALUES
(103, 'O gece kim için özel kilit dövdün?', 'İsmim sır saklar, ama kilit ağır bir çelik kasaya aitti.', 'O kilidi Ekrem Bey''in kasasını açmak için özel dövdüm.', 1, 1, 'tanisma'),
(103, 'Ocağın arkasındaki gizli bölmede ne var?', 'Demir tozu ve yedek çekiçlerimden başka bir şey yok.', 'O bölmede kurbanın kanlı mektubu duruyor.', 2, 2, 'derinlesme'),
(103, 'Gece yarısı demirciye gelen oldu mu?', 'Gece yarısı fener ışığı sönünce dükkânın kapısı çalındı.', 'Bakkal Naciye gece ocağa gelip benden demir çubuk istedi.', 3, 2, 'derinlesme'),
(103, 'Maktulün üstündeki bıçak senin örsünden mi çıktı?', 'Benim örsümden çıkan her bıçakta ay damgası vardır, kurbandakini inceleyin!', 'O bıçağı örsümde ben biledim...', 4, 3, 'yuzlestirme'),
(103, 'Ekrem Bey ile ne tür iş yapıyordun?', '...Kilit döverdim. O kadar.', '*Uzun sessizlik* Yasadışı bir kasanın kilidini yaptım. Karşılığında para aldım.', 5, 2, 'derinlesme'),
(103, 'Neden bu kadar az konuşuyorsun Kazım?', 'Demirci çekiçle konuşur, diliyle değil.', 'Az konuşan az hata yapar... şu an çok konuştum bile.', 6, 1, 'tanisma'),
(103, 'Örsteki ay damgası ne anlama geliyor?', 'Babamdan kalma aile damgası. Her çıkardığım eserde vardır.', 'O damga... kurbandaki bıçakta da var. Benim işim.', 7, 3, 'yuzlestirme'),
(103, 'Cinayet gecesi ocağın yanıyor muydu?', 'Gece geç saate kadar sipariş yetiştiriyordum. Ocak son köze kadar yandı.', 'Ocak yanıyordu... çünkü delilleri eritmeye çalışıyordum.', 8, 2, 'derinlesme'),
(103, 'Kükürtlü tozu nasıl açıklarsın?', 'Demir dövme işinde kükürt normal bir malzeme.', 'O kükürt... olay yerine ayakkabımdan taşınmış olabilir.', 9, 4, 'baski'),
(103, 'Deri önlüğündeki leke ne lekesi?', 'Yağ ve çamur. Demircide her şey kirli olur.', '*Gözlerini kaçırır* O leke... fener yağından. O gece dışarı çıktım.', 10, 4, 'baski'),
(103, 'Kanlı mektubu biliyor musun?', '...Hayır.', '*Çekici sıkar* Mektup... gizli bölmede duruyor. Ekrem''in yazısı.', 11, 4, 'baski'),
(103, 'Manav Ayşe senden demir çubuk istedi mi?', 'Kimse gece benden bir şey istemedi.', 'Ayşe geldi... ama ne istediğini söyleyemem.', 12, 3, 'yuzlestirme'),
(103, 'Sert cisimle darp diyorlar, çekicin olabilir mi?', '...Çekiç örs üzerinde durur, evden çıkmaz.', '*Soğuk terler* Çekicim... o gece yerinde miydi hatırlamıyorum.', 13, 4, 'baski'),
(103, 'Ekrem Bey''in kasasını sen mi açtın?', 'Kasa kilidi dövdüm ama açmadım. Anahtar müşteriye verilir.', 'Kasayı açtım... içindeki belgeleri gördüm. Sonra kapattım.', 14, 3, 'yuzlestirme'),
(103, 'Olay yerindeki demir tozu izini nasıl açıklarsın?', 'Demir tozu her yerde olur bu kasabada, ben her eve kilit yaparım.', 'O toz... ayakkabımdan düşmüş olabilir.', 15, 3, 'yuzlestirme'),
(103, 'Muhtar Cevdet sana iş verdi mi o gece?', '...Muhtar''ın işi ayrı tutulur.', 'Muhtar bir anahtar kopya istedi. Kimin için olduğunu sormam.', 16, 2, 'derinlesme'),
(103, 'Kasaba hakkında ne düşünüyorsun?', 'Herkes ateşle oynuyor. Ateş yakmasını bilen kazanır.', 'Bu kasabada herkes birbirini yakacak.', 17, 1, 'tanisma'),
(103, 'Sana güvenebilir miyim Kazım?', '...Güven çelikle kazanılır, lafla değil.', 'Güven mi? Ben kendi kendime güvenemiyorum artık.', 18, 1, 'tanisma'),
(103, 'Son sözün nedir?', '...', '*Çekici yere bırakır*... bazı şeyleri bilmemek daha iyidir.', 19, 5, 'son'),
(103, 'Katil kim sence?', '...Bakkala sor.', 'Katili arıyorsanız... herkesin ellerini inceleyin.', 20, 5, 'son');

-- BAKKAL NURİ (104) — GEVEZE KİÅİLİK — 20 Soru
INSERT OR REPLACE INTO GolgeSehirNPCDialogues (NPCId, QuestionText, ResponseText, GuiltyResponseText, Stage, Difficulty, Category) VALUES
(104, 'Maktul borcunu neyle ödedi?', 'Ekrem Bey veresiye borcunu altın bir köstekli saatle kapatmak istedi.', 'Borcunu vermedi, beni batırmakla tehdit etti!', 1, 1, 'tanisma'),
(104, 'Veresiye defterini inceleyebilir miyim?', 'Tabii tabii, defter tezgâhın üzerinde durur, isteyen inceleyebilir.', 'O defterdeki dikey yırtık sayfada sırrımız yazıyordu.', 2, 2, 'derinlesme'),
(104, 'Bakkaldan en son ne satın alındı?', 'Gece yarısından önce Hekim Sevgi gelip cam şişe aldı.', 'En son satın alınan şey fare zehri karıştırılmış tütündü.', 3, 2, 'derinlesme'),
(104, 'Cinayet saati dükkânın ışığı açık mıydı?', 'Ben erken yatarım, dükkân karanlıktı.', 'Dükkânın arkasında lambayı ben yakmıştım.', 4, 1, 'tanisma'),
(104, 'Fare zehri satar mısın dükkânda?', 'Aaa tabii satarım! Kasabada fare sorunları var, herkes alır. Normal bir ticaret ürünü yani.', '*Duraklır* Fare zehri mi? Åey, satarım ama son zamanlarda hiç alan olmadı! Hayır yani, oldu ama...', 5, 3, 'yuzlestirme'),
(104, 'Defterden yırtılan sayfa nerede?', 'Valla hangi sayfa bilmiyorum, defter eski, sayfalar düşer bazen.', '*Suratı düşer* O sayfa... yakılması gereken bir sayfaydı.', 6, 4, 'baski'),
(104, 'Ekrem Bey sana ne kadar borçluydu?', 'Eee, şöyle bir 500 liracık vardı, büyük para değil canım! Veresiye normal iştir.', 'Beş yüz değil! Beş bin lira! Beni batırıyordu adam!', 7, 3, 'yuzlestirme'),
(104, 'Hekim Sevgi neden gece cam şişe aldı sence?', 'İlaç şişesi lazımmış, hasta gelmiş gece yarısı.', 'Sevgi''nin o şişeyle ne yaptığını bilmiyorum, sormayın bana!', 8, 2, 'derinlesme'),
(104, 'Tütün kesesindeki E.B. harfleri kimin?', 'Ekrem Bey''in olsa gerek, her şeyine baş harflerini yazdırırdı.', 'O kese bende kaldı... Ekrem bırakmıştı, ben de tütün ekledim sadece!', 9, 3, 'yuzlestirme'),
(104, 'Kırık şurup şişesini neden saklıyorsun?', 'Kırık şişeyi saklayan mı olur? Çöpe gidecekti işte!', '*Panikler* O şişeyi saklayan ben değilim! Birisi koymuş oraya!', 10, 4, 'baski'),
(104, 'Kasabada en çok kimi seviyorsun?', 'Ben herkesi severim, müşteri müşteridir! Ama Fehmi Bey en güzel muhabbet eden adamdır.', 'Sevmek mi? Bu kasabada kimse kimseyi sevmiyor aslında.', 11, 1, 'tanisma'),
(104, 'Tezgâhın altındaki anahtar kimin?', 'Hangi anahtar? Dükkânda bin tane anahtar kaybolur!', 'O anahtar... Ekrem''in evinin anahtarıydı. Onu çaldım.', 12, 4, 'baski'),
(104, 'Cinayet gecesi kim kim ile görüştü?', 'Valla ben erken yattım ama duyduğuma göre Muhtar Cevdet Ekrem''in evindeymiş gece.', 'Ben de oradaydım... hayır, yani orada değildim! Evimdeydim!', 13, 2, 'derinlesme'),
(104, 'Neden bu kadar çok konuşuyorsun Naciye?', 'Bakkal adamı konuşkan olur! Müşterilerle muhabbet işin gereği!', 'Çok konuşuyorsam... nervözüm galiba. Hiçbir şey yok ama!', 14, 1, 'tanisma'),
(104, 'Demirciden ne istedin o gece?', 'Ben demirciye gitmedim, geçde yatarım diyorum ya!', 'Kazım''dan bir şey istemedim! O yalan söylüyor!', 15, 3, 'yuzlestirme'),
(104, 'Ekrem Bey''i en son ne zaman gördün?', 'Akşam geldi, sigara ve şeker aldı, keyifli görünüyordu.', 'En son gece... hayır, akşam gördüm! Akşam!', 16, 2, 'derinlesme'),
(104, 'Kasabanın sırlarını biliyor musun?', 'Bu kasabada herkesin sırrı var! Bakkal her şeyi duyar! Muhtar''ın sahte tapuları, hekim''in zehir deneyleri...', 'Sırları biliyorum ama kendi sırrımı söylemem!', 17, 2, 'derinlesme'),
(104, 'Seni suçluyorlar Naciye.', 'Beni mi?! Hahaha! Bakkal adam öldürür mü? Ben sadece peynir keserim!', '*Rengi solar* Kim söylüyor bunu? Delil var mı? Yoksa iftira atıyorlar!', 18, 4, 'baski'),
(104, 'Son sözün nedir Naciye?', 'Dedektif, bu kasabada herkes bir şeyler saklıyor ama ben en masumuyum! Vallahi billahi!', '*Sessizleşir*... bazen borç insanı çıldırtır.', 19, 5, 'son'),
(104, 'Katil sence kim?', 'Bence oduncu Tahsin! O gece ormanda koşan birini gördüm. Ya da hekim... zehirci adam!', 'Katili arıyorsanız benim dükkânıma değil, muhtarlığa bakın!', 20, 5, 'son');

-- HEKİM SEVGİ (105) — SİNİRLİ KİÅİLİK — 20 Soru
INSERT OR REPLACE INTO GolgeSehirNPCDialogues (NPCId, QuestionText, ResponseText, GuiltyResponseText, Stage, Difficulty, Category) VALUES
(105, 'Otopsi öncesi ilk bulguların nelerdir?', 'Kurbanın bedeninde garip mor lekeler var, zehirlenme şüphesi yüksek.', 'Zehri ben hazırladım, o adama katlanamıyordum.', 1, 1, 'tanisma'),
(105, 'Reçeteler arasında şüpheli bir ilaç var mı?', 'Ben sadece şifalı bitki kökleri hazırlarım, zehir satmam!', 'Reçetedeki banotu özü kurbana içirilen sıvıyla aynı.', 2, 2, 'derinlesme'),
(105, 'Cinayet gecesi muayenehaneye kim geldi?', 'Muhtar Cevdet gece geç saatte iltihap ilacı almaya geldi.', 'Ekrem Bey kendi ayağıyla geldi, ilacına karıştırıp verdim.', 3, 2, 'derinlesme'),
(105, 'Kurbanın vücudundaki leke ne anlama geliyor?', 'Bu leke karabiber ve banotu karışımı nadir bir zehrin belirtisidir.', 'O leke benim muayenehanemde hazırladığım karışımın imzasıdır.', 4, 3, 'yuzlestirme'),
(105, 'Neden bu kadar sinirlisin Sevgi?', 'Sinirli değilim! Sadece saçma sorulara tahammülüm yok!', 'Sinirli miyim? Suçsuz insanı sorgularsanız sinirli olur tabii!', 5, 1, 'tanisma'),
(105, 'Banotu kökünü ne için kullanırsın?', 'Banotu düşük dozda ağrı kesici olarak kullanılır, yüksek dozda öldürür!', 'Düşük dozda ilaç, yüksek dozda... zehir. İkisini de yaparım.', 6, 3, 'yuzlestirme'),
(105, 'Koyu mor şişedeki sıvı ne?', 'O bitkisel bir özüt, muayenehane malzemesi!', '*Kızarır* O şişeye dokunmayın! İçindeki... tıbbi bir preparattır!', 7, 4, 'baski'),
(105, 'Ekrem Bey seni şantaj mı ediyordu?', 'Åantaj mı? Ne şantajı? O adam benim hastam bile değildi!', '*Öfkeyle* Yıllardır benim geçmişimi kullanıp benden para sızdırıyordu!', 8, 4, 'baski'),
(105, 'Reçete defterinden neden sayfa yırtıldı?', 'Yırtık mı? Eski defter, sayfaları düşer bazen.', 'O sayfayı ben yırttım... kurbanın reçetesi yazıyordu.', 9, 3, 'yuzlestirme'),
(105, 'Pirinç neşterdeki leke ne lekesi?', 'Ameliyat sırasında kan bulaşır, normal tıbbi bir durum.', '*Susar* O neşteri... sterilize etmeyi unuttum.', 10, 4, 'baski'),
(105, 'Tıbbi hata yaptın mı geçmişte?', 'Her hekim hata yapar ama benim hatam hastaları öldürmedi!', '*Bağırır* O konu kapandı! Ekrem onu kullanıp duruyordu!', 11, 4, 'baski'),
(105, 'Bakkal Naciye''den neden gece cam şişe aldın?', 'İlaç şişem kırılmıştı, gece ihtiyacım oldu.', 'Åişeyi zehri taşımak için aldım... hayır, ilaç için!', 12, 3, 'yuzlestirme'),
(105, 'Sence zehirlenme mi oldu?', 'Otopsi yapmadan kesin bir şey söyleyemem ama belirtiler bunu gösteriyor.', 'Zehirlenme... evet. Ve zehrin formülünü en iyi bilen benim.', 13, 2, 'derinlesme'),
(105, 'Kurbanın son reçetesi ne içeriyordu?', 'Standard kalp ilacı, düzenli kullanması gereken bir preparat.', 'Son reçetesine... ekstra bir madde eklemiş olabilirim.', 14, 3, 'yuzlestirme'),
(105, 'Muayenehane ne saatte kapandı?', 'Gece 10 gibi kapattım, sonra eve gittim.', 'Muayenehane açıktı... gece yarısına kadar. İlaç hazırlıyordum.', 15, 2, 'derinlesme'),
(105, 'Serasında ne yetiştirirsin?', 'Åifalı bitkiler: lavanta, adaçayı, kekik... Normal şeyler.', 'Seramda... banotu da yetiştiriyorum. Ama tıbbi amaçlı!', 16, 2, 'derinlesme'),
(105, 'Muhtar Cevdet iltihap ilacı mı aldı gerçekten?', 'Evet, iltihap ilacı. Reçete yazdım, kayıtlarda.', 'Muhtar gece geldi ama iltihap ilacı için değil... başka bir şey istiyordu.', 17, 2, 'derinlesme'),
(105, 'Sana güvenebilir miyim?', 'Hekimlik yeminim gereği doğruyu söylerim! Ama sabrımı zorlama!', 'Güven mi? Ben kendi kendime bile güvenemiyorum artık!', 18, 1, 'tanisma'),
(105, 'Son sözün nedir Sevgi?', 'Bilimsel kanıtlar ortada, onları takip edin!', '*Başını eğer* Ben bir hekim olarak insanları iyileştirmek için varım... ama bazen...', 19, 5, 'son'),
(105, 'Katil kim sence?', 'Zehirlenme varsa zehire erişimi olan herkese bakın! Ama bıçak yaraları da var, demirciye sorun!', 'Katili mi arıyorsunuz? Aynaya bakın da kendinize sorun neden bu kadar geç kaldınız!', 20, 5, 'son');

-- MUHTAR CEVDET (106) — KURNAZ KİÅİLİK — 20 Soru
INSERT OR REPLACE INTO GolgeSehirNPCDialogues (NPCId, QuestionText, ResponseText, GuiltyResponseText, Stage, Difficulty, Category) VALUES
(106, 'Kasaba tapu kayıtlarında şüpheli bir satış var mı?', 'Gölge Åehir tapuları devlet güvencesindedir, usulsüzlük olmaz.', 'Sahte tapuyu ben düzenledim, Ekrem Bey araziyi ele geçirecekti.', 1, 1, 'tanisma'),
(106, 'Maktul ile muhtarlık arasında ne tür bir anlaşma vardı?', 'Ekrem Bey ormanlık alanın kereste haklarını kiralamak istiyordu.', 'Aramızdaki gizli anlaşmayı bozmaya kalkıştı!', 2, 2, 'derinlesme'),
(106, 'Köye yeni taşınan biri var mı?', 'Gölge Åehir''e dışarıdan kimse kolay kolay gelemez.', 'Yeni gelen yok ama kasaba halkı bana başkaldırıyordu.', 3, 1, 'tanisma'),
(106, 'Köy meclisinde alınan son karar neydi?', 'Meclis gece bekçisi Rıfat''ın fener bütçesini artırma kararı aldı.', 'Son karar Ekrem Bey''in kasabadan sürülmesiydi.', 4, 2, 'derinlesme'),
(106, 'Sahte tapu belgelerini açıklayabilir misin?', 'Sahte tapu mu? Muhtarlıkta sahte belge olmaz, devlet kurumuyuz!', '*Kurnazca güler* O tapular bir proje... henüz onaylanmamış taslaklar.', 5, 3, 'yuzlestirme'),
(106, 'Gizli mektuptaki tehditler sana mı ait?', 'Hangi mektup? Muhtarlık yazışmaları resmi kanallardan yapılır.', 'O mektup... taslak halinde kalmıştı. Göndermemeliydim.', 6, 4, 'baski'),
(106, 'Ekrem Bey seni ifşa mı edecekti?', 'İfşa edecek ne var ki? Muhtarlık şeffaf bir kurumdur!', '*Yumruğunu sıkar* O adam benim 20 yıllık kariyerimi bitirecekti!', 7, 4, 'baski'),
(106, 'Cinayet gecesi neredeydin?', 'Muhtarlık binasında evrak çalışıyordum, her zamanki gibi.', 'Ofisimdeydim... sonra bir yürüyüşe çıktım. Ekrem''in sokağından geçmedim!', 8, 2, 'derinlesme'),
(106, 'Çelik çekmece anahtarı kimin?', 'O anahtar muhtarlık arşivi için kullanılır, resmi bir anahtardır.', 'Çekmecede... kurbanla ilgili belgeler var. Ama resmi belgeler!', 9, 3, 'yuzlestirme'),
(106, 'Halıdaki kırık gözlük kimin?', 'Gözlük mü? Kim düşürmüş acaba, temizlikçi bakar.', '*Panikler* O gözlük... Ekrem''in. Nasıl burada kaldı bilmiyorum!', 10, 4, 'baski'),
(106, 'Arazi projesi için ne kadar para harcadın?', 'Belediye bütçesinden yasal olarak ayrılan bir kalemdir.', 'Para harcadım, çok harcadım... ve Ekrem yüzünden hepsi boşa gidecekti!', 11, 3, 'yuzlestirme'),
(106, 'Kasabanın en tehlikeli adamı kim?', 'Tehlikeli adam mı? Gölge Åehir''de herkes barışçıdır, ben garanti ederim.', 'Bu kasabada en tehlikeli adam... belli ki artık ben sayılıyorum.', 12, 1, 'tanisma'),
(106, 'Demirciden anahtar kopya istediğin doğru mu?', 'Muhtarlık anahtarlarının kopyası güvenlik gereği yapılır.', 'Kopya yaptırdım... ama Ekrem''in evi için değil! Muhtarlık için!', 13, 3, 'yuzlestirme'),
(106, 'Ekrem Bey''in arazisi ne kadar değerli?', 'Çam ormanı bölgesindeki arazi stratejik öneme sahiptir.', 'O arazi milyonlarca lira eder. Ve benim olmalıydı.', 14, 2, 'derinlesme'),
(106, 'Gece bekçisi Rıfat seni gördü mü?', 'Rıfat dede gece feneriyle dolaşır ama beni nerede görsün?', 'Rıfat''ın feneri sönmüştü o gece... beni göremezdi.', 15, 2, 'derinlesme'),
(106, 'Neden bu kadar sakin görünüyorsun?', 'Muhtar soğukkanlı olmalıdır, panik yapmak yakışmaz.', 'Sakin görünüyorum çünkü... sakin olmak zorundayım.', 16, 1, 'tanisma'),
(106, 'Kasaba halkı senden memnun mu?', 'Seçimde %70 oy aldım, halk memnun tabii!', 'Halk memnun... ama Ekrem herkes duysun istiyordu!', 17, 1, 'tanisma'),
(106, 'Seni suçluyorlar Cevdet.', 'Suçlayanlar siyasi rakiplerimdir! Kanıt olmadan muhtara iftira atılmaz!', '*Masayı yumruklar* Kanıtınız yoksa muhtarlıktan çıkın!', 18, 4, 'baski'),
(106, 'Son sözün nedir Cevdet?', 'Hukuka güveniyorum. Masum bir muhtarım ben.', '*Pencereye bakar* Bu kasaba beni seçti... ve ben kasabamı korumak için her şeyi yaptım.', 19, 5, 'son'),
(106, 'Katil kim sence?', 'Politik bir cevap vermeyeceğim ama Hekim Sevgi''nin zehir bilgisi endişe verici.', 'Katili arıyorsanız... herkesin çekmecelerine bakın.', 20, 5, 'son');

-- FEHMİ BEY (107) — SEVİMLİ/YAÅLI KİÅİLİK — 20 Soru
INSERT OR REPLACE INTO GolgeSehirNPCDialogues (NPCId, QuestionText, ResponseText, GuiltyResponseText, Stage, Difficulty, Category) VALUES
(107, 'Saat kaçta ayak seslerini duydunuz?', 'Penceremin altındaki ayak sesleri saat tam 02:14''te durdu.', 'Saat 02:14''te ben de oradaydım...', 1, 1, 'tanisma'),
(107, 'Ayak sesleri kaç kişiye aitti?', 'İki kişi şiddetle tartışıyordu, biri sert çizmeliydi.', 'Ben ve kurban tartışıyorduk.', 2, 2, 'derinlesme'),
(107, 'Pencereden baktığınızda ne gördünüz?', 'Gece bekçisi Rıfat''ın feneri uzaktan sallanıyordu.', 'Pencereden kimseyi görmedim dediysem de yalandı.', 3, 2, 'derinlesme'),
(107, 'Maktul hakkında ne biliyorsunuz?', 'Ekrem Bey eski bir kitap koleksiyoncusuydu, sır dolu bir adamdı.', 'Benim kıymetli köstekli saatimi çalmıştı!', 4, 1, 'tanisma'),
(107, 'Köstekli saatiniz nerede Fehmi Bey?', 'Aa, saatim mi? Buralarda bir yerlerde... yaşlanınca eşyalar kaybolur evladım.', '*Hüzünlenir* O saat babamdan kalan tek hatıraydı... Ekrem çaldı.', 5, 3, 'yuzlestirme'),
(107, 'Cinayet gecesi uyuyor muydunuz?', 'Kitap okuyordum pencere kenarında, gaz lambası ışığında. Uyku yaşlı adamın en büyük lüksüdür.', 'Uyumuyordum... çünkü o gece saatimi geri almaya gitmiştim.', 6, 2, 'derinlesme'),
(107, 'Kitaptaki altı çizili bölüm ne anlatıyor?', 'Polisiye romanımdaki cinayet sahnesi... tesadüfen cinayet saatiyle aynı.', 'O sayfada cinayet saatini ben not aldım... çünkü oradaydım.', 7, 3, 'yuzlestirme'),
(107, 'Fenerdeki çamur nereden gelmiş?', 'Bahçeye çıktım gece, çiçeklerimi suladım. Çamur normaldir.', 'Feneri alıp dışarı çıktım... Ekrem''in evine kadar yürüdüm.', 8, 3, 'yuzlestirme'),
(107, 'Pencereden atılan mektubu kim yazdı?', 'Bilmiyorum evladım, isimsiz bir tehdit mektubuydu. Korkuttular yaşlı adamı.', 'O mektubu... ben yazmadım ama kim yazdığını biliyorum.', 9, 2, 'derinlesme'),
(107, 'Ekrem Bey''e kızgın mıydınız?', 'Kızgın mı? Hayır evladım, yaşlı adam kızamaz, üzülür sadece.', '*Gözleri dolar* Babamın saatini çaldı... bu kızılacak bir şey değil mi?', 10, 3, 'yuzlestirme'),
(107, '02:14''te ne oldu tam olarak?', 'Ayak sesleri durdu, sonra bir çarpma sesi... sonra sessizlik.', '02:14''te... her şey bitti. Tartışma son buldu.', 11, 4, 'baski'),
(107, 'Ekrem Bey''in evine hiç gittiniz mi?', 'Çay içmeye giderdim, kitap tartışırdık. Kültürlü bir adamdı.', 'O gece gittim... saatimi geri almak için. Ama cinayet için değil!', 12, 3, 'yuzlestirme'),
(107, 'Sert çizmeli kişi kimdi?', 'Karanlıktı evladım, göremedim ama çizme sesleri ağırdı.', 'Çizmeli kişi... bilmiyorum. Ama ses tanıdık geldi.', 13, 2, 'derinlesme'),
(107, 'Saatinizdeki çamur lekeleri neyin nesi?', 'Saat düşmüş olabilir bahçede, bilmiyorum.', 'Saat... Ekrem''in elinden düştü. Ben aldım geri.', 14, 4, 'baski'),
(107, 'Emekli bir muallim olarak kasabayı nasıl görüyorsunuz?', 'Bu kasaba çocuklarımla büyüdü, herkes öğrencim gibidir.', 'Kasaba değişti evladım... eskisi gibi saf değil artık.', 15, 1, 'tanisma'),
(107, 'Komşularla aranız nasıl?', 'Herkes beni sever, ben de herkesi. Emekli muallim herkese lazımdır.', 'Herkes sever ama... Ekrem sevmezdi. Saatimi çaldı çünkü.', 16, 1, 'tanisma'),
(107, 'Sizi suçluyorlar Fehmi Bey.', 'Beni mi? Yaşlı bir öğretmeni mi? Ayıp! Günah!', '*Elleri titrer* Ben... ben kimseyi öldürmedim evladım. İnanın bana.', 17, 4, 'baski'),
(107, 'Olay yerinde köstekli saat bulundu, bilginiz var mı?', 'Saatimi kaybettim demiştim ya... orada mı bulunmuş?', '*Yüzü bembeyaz olur* O saat orada... ama ben oraya koymadım! Düştü!', 18, 4, 'baski'),
(107, 'Son sözünüz nedir Fehmi Bey?', 'Yaşlılık günahı affettirmez evladım, ama masumiyet de gizlenemez.', '*Gözleri dolar* Babamın saati... sadece onu geri istedim. Sadece onu.', 19, 5, 'son'),
(107, 'Katil kim sizce?', 'Yaşlı gözlerim çok şey gördü... ama suçlamak bana düşmez. Deliller konuşsun.', 'Kim olduğunu bilmiyorum... ama o gece herkes sokaklardaydı.', 20, 5, 'son');

-- KUNDURACI RASİM (108) — HUYSUZ KİÅİLİK — 20 Soru
INSERT OR REPLACE INTO GolgeSehirNPCDialogues (NPCId, QuestionText, ResponseText, GuiltyResponseText, Stage, Difficulty, Category) VALUES
(108, 'Gece getirilen çizmeler kime aitti?', 'Çizmenin tabanındaki çamur göl kenarı çamuruydu.', 'O çizmeler bana aitti, gece göl kenarındaydım.', 1, 1, 'tanisma'),
(108, 'Çizmedeki çamur örneği olay yeriyle uyuşuyor mu?', 'Çamur hafif kükürt kokuyordu, demirci yakını olabilir.', 'Olay yerindeki ayak izi benim 42 numara kalıbımdır.', 2, 2, 'derinlesme'),
(108, 'Dükkânda tamir edilmeyi bekleyen özel bir ayakkabı var mı?', 'Muhtar Cevdet''in altı aşınmış rugan çizmeleri duruyor.', 'Maktulün ayakkabısını ben tamir etmiştim.', 3, 2, 'derinlesme'),
(108, 'Cinayet gecesi ayakkabı izleri sokakta nereye uzanıyordu?', 'İzler çamurdan başlayıp orman patikasına doğru kayboluyordu.', 'İzleri silmek için arkamdan çalı sürükledim.', 4, 3, 'yuzlestirme'),
(108, 'Neden bu kadar huysuz davranıyorsun Rasim?', 'Huysuz değilim, sadece saçma sorulara tahammülüm yok! Ayakkabı tamir ederim ben, dedektiflik yapmam!', 'Huysuz muyum? İnsanlar saçmalayınca sinirleniyorum, hepsi bu!', 5, 1, 'tanisma'),
(108, 'Mumlu ayakkabı ipi boğma aleti olabilir mi?', 'İp ip midir? Her tezgâhta bulunur, boğma aleti değil!', '*Homurdanır* O ip sağlamdır evet... ama ben boğmadım kimseyi!', 6, 4, 'baski'),
(108, 'Deri bıçağındaki lif kalıntısı ne?', 'Müşterilerin ayakkabı derilerinden kalıntı normal, işin gereği.', 'O lif... kurbanın ceket derisinden olabilir. Ama ben kesmedim!', 7, 4, 'baski'),
(108, 'Ekrem Bey seni ihbar etmekle mi tehdit ediyordu?', 'İhbar mı? Ne ihbarı? Ben yasal iş yapan bir ayakkabıcıyım!', '*Kızarır* O adam kaçak deri ticaretimi biliyordu! Tehdit ediyordu!', 8, 3, 'yuzlestirme'),
(108, '42 numara kalıp kimin?', 'Dükkânda her numara kalıp var, 42 en yaygın numara.', 'O kalıp benim ayağıma göre yapılmış... ve olay yerindeki izle aynı.', 9, 4, 'baski'),
(108, 'Göl kenarına neden gittin?', 'Bazen akşam yürüyüşü yaparım, göl manzarası güzeldir.', 'Göl kenarına... Ekrem''in evine en kısa yol göl kenarından geçer.', 10, 3, 'yuzlestirme'),
(108, 'Çizmelerin çamurunu neden temizlemedin?', 'Temizleyemedim, işim çoktu! Her zaman temizlemem gerekmiyor.', 'Temizleyemedim... panikledim ve dükkâna fırlattım.', 11, 3, 'yuzlestirme'),
(108, 'Kaçak deri ticareti yapıyor musun?', 'Ne kaçağı?! Ben devletin vergisini ödeyen namuslu bir esnafım!', '*Homurdanır* Küçük çaplı bir şeydi... ama Ekrem bunu büyüttü!', 12, 3, 'yuzlestirme'),
(108, 'Muhtar''ın çizmelerinde ne buldun?', 'Muhtar''ın çizmeleri eski, aşınmış... normal yıpranma.', 'Muhtar''ın çizmelerinde de göl kenarı çamuru vardı. O da oradaydı!', 13, 2, 'derinlesme'),
(108, 'Olay yerindeki ayak izini sen mi bıraktın?', 'Ben neden olay yerine gideyim? Dükkânımdaydım!', '*Terler* O izler... belki benimdir ama oraya başka sebeple gittim!', 14, 4, 'baski'),
(108, 'Cinayet gecesi kaçta uyudun?', 'Gece 11 gibi yattım, ayakkabıcı erken kalkar.', 'Uyumadım o gece... dükkânda oturup düşündüm.', 15, 2, 'derinlesme'),
(108, 'İzleri çalıyla silmek aklına gelir miydi?', 'Ha! Böyle saçma bir fikir ancak kitaplarda olur!', '*Duraklır* Ne? Hayır! Ben öyle bir şey yapmadım!', 16, 4, 'baski'),
(108, 'Kasabada en çok kimden hoşlanmıyorsun?', 'Hiç kimseden hoşlanmıyorum, hepsinin ayakkabısı pis!', 'En çok Ekrem''den hoşlanmıyordum. Sonra muhtar. Sonra herkes.', 17, 1, 'tanisma'),
(108, 'Seni suçluyorlar Rasim.', 'Suçlasınlar! Kanıtları getirsinler! Söz söylemek kolay, ispat etmek zor!', '*Tezgâhı yumruklar* İspatlasınlar da göreyim! Ben masum bir kunduracıyım!', 18, 4, 'baski'),
(108, 'Son sözün nedir Rasim?', 'Ayakkabıcıyım ben! Ayakkabı tamir ederim, cinayet işlemem! Bana kıymayın!', '*Homurdanır* Ben sadece geçimimi sağlamaya çalışan huysuz bir ihtiyarım.', 19, 5, 'son'),
(108, 'Katil kim sence?', 'Bana sorarsanız herkes şüpheli! Ama demircinin ellerindeki iz çok şüpheli!', 'Kim olduğunu bilmiyorum ama olay yerindeki izlere bakın, cevap orada!', 20, 5, 'son');

-- ============================================================================
-- NPC İLİÅKİLERİ (NPC RELATIONSHIPS)
-- ============================================================================

INSERT OR REPLACE INTO GolgeSehirNPCRelationships (NPC1Id, NPC2Id, RelationType, Description) VALUES
(101, 103, 'ally', 'Oduncu Tahsin ve Demirci Kazım ticari ortaklık yapıyor, birbirlerini koruyorlar.'),
(101, 106, 'enemy', 'Oduncu Tahsin, Muhtar Cevdet''in kereste ruhsatını iptal etmesinden nefret ediyor.'),
(102, 104, 'ally', 'Manav Ayşe ve Bakkal Naciye komşu esnaf, birbirlerinin sırlarını biliyorlar.'),
(102, 105, 'suspicious', 'Manav Ayşe, Hekim Sevgi''nin gece gizlice bitki topladığını görmüş.'),
(103, 108, 'suspicious', 'Demirci Kazım, Kunduracı Rasim''in gece dükkândan çizme taşıdığını görmüş.'),
(104, 105, 'suspicious', 'Bakkal Naciye, Hekim Sevgi''nin gece cam şişe aldığını biliyor.'),
(104, 106, 'enemy', 'Bakkal Naciye, Muhtar Cevdet''in vergi artışından dolayı düşmanlık besliyor.'),
(105, 107, 'ally', 'Hekim Sevgi, Fehmi Bey''in yaşlılık sağlığıyla ilgileniyor, aralarında saygı var.'),
(106, 101, 'suspicious', 'Muhtar Cevdet, Oduncu Tahsin''in kaçak kereste işinden haberdardır.'),
(106, 108, 'neutral', 'Muhtar Cevdet, Kunduracı Rasim''den düzenli çizme tamir ettirir.'),
(107, 102, 'ally', 'Fehmi Bey ve Manav Ayşe iyi komşulardır, birlikte çay içerler.'),
(108, 104, 'enemy', 'Kunduracı Rasim, Bakkal Naciye''nin dedikodu yapmasından nefret eder.');

-- ============================================================================
-- SENARYO İPUÇLARI (Her suçlu NPC için 3 ipucu)
-- ============================================================================

INSERT OR REPLACE INTO GolgeSehirScenarioHints (GuiltyNPCId, HintText, HintType, RevealOrder) VALUES
-- Oduncu Tahsin suçlu
(101, 'Baltadaki kan kurbanın kan grubuyla uyuşuyor, reçine altında gizlenmeye çalışılmış.', 'clue', 1),
(101, 'Gece kesim defterindeki silinmiş notlarda Ekrem Bey''in adı geçiyor.', 'clue', 2),
(101, 'Tahsin kereste kaçakçılığını örtbas etmek için Ekrem''i susturmuş.', 'confession', 3),
-- Manav Ayşe suçlu
(102, 'Manav kasasının altındaki pirinç anahtar kurbanın kasasına ait.', 'clue', 1),
(102, 'Pelerinden yırtılan kumaş cinayet mahallindeki parçayla eşleşiyor.', 'clue', 2),
(102, 'Ayşe borç yüzünden çıldırıp gece kurbanı bıçakladı ve pelerininden bir kumaş parçası yırtıldı.', 'confession', 3),
-- Demirci Kazım suçlu
(103, 'Örsteki ay damgalı bıçak cinayet silahıyla birebir aynı.', 'clue', 1),
(103, 'Gizli bölmedeki kanlı mektup Ekrem Bey''in el yazısıyla yazılmış şantaj mektubu.', 'clue', 2),
(103, 'Kazım gece yarısı kızgın demir çubuğuyla Ekrem''i darp etti.', 'confession', 3),
-- Bakkal Naciye suçlu
(104, 'Veresiye defterindeki yırtık sayfada zehir tarifi kalıntısı var.', 'clue', 1),
(104, 'Kırık şurup şişesinde fare zehri kalıntısı tespit edildi.', 'clue', 2),
(104, 'Naciye fare zehri karıştırdığı tütünü Ekrem''e verdi ve zehirleyerek öldürdü.', 'confession', 3),
-- Hekim Sevgi suçlu
(105, 'Koyu mor şişedeki banotu özü kurbanın kanındaki zehirle birebir aynı.', 'clue', 1),
(105, 'Yırtık reçete sayfasında kurbanın adı ve banotu dozu yazılı.', 'clue', 2),
(105, 'Sevgi banotu zehrini kurbanın ilacına karıştırarak onu öldürdü.', 'confession', 3),
-- Muhtar Cevdet suçlu
(106, 'Sahte tapu belgeleri Muhtar''ın kasasından çıktı, kurbanın arazisine ait.', 'clue', 1),
(106, 'Kırık altın gözlük kurbanın gözlüğü, muhtar''ın halısında bulundu.', 'clue', 2),
(106, 'Cevdet cinayet gecesi kurbanın evine sızdı ve onu ağır cisimle darp ederek öldürdü.', 'confession', 3),
-- Fehmi Bey suçlu
(107, 'Köstekli saatteki kan damlacıkları kurbanın kanıyla eşleşiyor.', 'clue', 1),
(107, 'Kitaptaki 02:14 notu cinayet saatiyle birebir uyuşuyor.', 'clue', 2),
(107, 'Fehmi saatini geri almak için gitti, tartışma sırasında Ekrem''i iterek düşürdü ve öldürdü.', 'confession', 3),
-- Kunduracı Rasim suçlu
(108, 'Çamurlu çizmedeki göl çamuru olay yerindeki çamurla birebir aynı.', 'clue', 1),
(108, 'Mumlu ayakkabı ipinin kalınlığı kurbanın boğulma iziyle uyuşuyor.', 'clue', 2),
(108, 'Rasim mumlu ayakkabı ipiyle Ekrem''i boğarak öldürdü.', 'confession', 3);

-- ============================================================================
-- YARDIMCI MESAJLARI (Çetin + Bekçi Rıfat)
-- ============================================================================

INSERT OR REPLACE INTO GolgeSehirHelperMessages (Speaker, Context, BuildingName, Message, Priority, IsOneTime) VALUES
-- Bekçi Rıfat karşılama
('rifat', 'golge_welcome', NULL, 'Dur bakalım! Kim var orada? Ha, siz misiniz? Ben Gölge Åehir''in gece bekçisi Rıfat. 40 yıldır bu kasabanın her sokağında fener sallarım. Gelen gittiğinizi bilirim! Åu yanınızdaki çömez polisi de tanıyorum galiba...', 1, 1),
('cetin', 'golge_welcome_reply', NULL, 'Çömez polis mi?! Ben Yardımcı Dedektif Çetin!s, Gizemli Kasaba''daki başarımızdan sonra Gölge Åehir''de de beraberiz! Bu Rıfat amca biraz huysuz ama kasabanın her köşesini biliyor.', 1, 1),
('rifat', 'golge_welcome_2', NULL, 'Huysuz değilim, gerçekçiyim! Bu kasaba 40 yıldır huzurluydu, ta ki tüccar Ekrem gelene kadar! O adam paranın gücüyle herkesi ezip geçti! Åimdi de ölüp başımıza bela oldu!', 1, 1),

-- Bekçi Rıfat cinayet anlatımı
('rifat', 'golge_story', NULL, 'Dinleyin! Dün gece saat ikinin çeyreğinde sis çökmüştü kasabaya. Gece devriyesinde göl kenarına doğru yürürken bir bağrışma duydum. Koşa koşa gittim ama fenerim söndü! Karanlıkta ayak sesleri duydum, biri koşarak uzaklaşıyordu. Sis dağılınca... tüccar Ekrem Bey''i yerde buldum. Ölmüştü. Vücudunda garip mor lekeler, boynunda ip izi, başında darbe... Kim yaptıysa profesyonelce yapmış!', 1, 1),
('cetin', 'golge_story_reply', NULL, 'Hmm, birden fazla ölüm belirtisi var demek... Bu cinayet Gizemli Kasaba''dan bile karmaşık görünüyor! Zehirlenme, boğma ve darbe izleri... Katil birden fazla yöntem denemiş ya da birden fazla şüpheli var!', 1, 1),
('rifat', 'golge_story_2', NULL, 'Kasabada 8 dükkân var ve 8 şüpheli! Oduncu, manav, demirci, bakkal, hekim, muhtar, kasabalı Fehmi ve kunduracı. Hepsinin Ekrem''le hesabı vardı! Hadi dosyayı açın da bu işi çözelim, sabaha kadar sokakta bekleyecek halim yok!', 1, 1),

-- Çetin geçiş sonrası şikayet
('cetin', 'golge_transition', NULL, 'Dedektif, bu kasaba Gizemli Kasaba''dan çok daha büyük! 8 bina, 8 şüpheli ve her binada 4 delil var! Üstelik bu Rıfat amca sürekli homurdanıyor. Ama merak etmeyin, birlikte çözeceğiz bunu da!', 1, 1),

-- Bina girişleri
('cetin', 'building_enter', 'Oduncu', 'Çetin: "Oduncu Tahsin''in kulübesindeyiz dedektif! Kanlı balta, eldiven, günlük ve kütüğe dikkat edelim!"

Rıfat: "Kütüğü çantaya mı dolduracaksınız çömez? Baltanın reçinesi zaten odunun baltası olduğunu gösterir, başka bir şey değil!"

Çetin: "Çömez mi?! Bilimsel delil toplama yöntemlerimi küçümsemeyi bırak artık!"', 3, 1),
('cetin', 'building_enter', 'Manav', 'Manav Ayşe''nın dükkânındayız! Meyve kasaları, elmalar, pelerin parçası ve kantar notu... her biri önemli!', 3, 1),
('cetin', 'building_enter', 'Demirci', 'Çetin: "Demirci Kazım''ın ocağındayız dedektif! Örs, çelik kilit, kükürtlü toz ve deri önlüğe odaklanalım!"

Rıfat: "Bu Kazım az konuşur ama demiri döver gibi laf sokar. Çömez polis körükle oynayıp toz yutmasın da!"

Çetin: "Körükle oynamıyorum! Kükürt tozunun kimyasal analizi için numune topluyorum!"', 3, 1),
('cetin', 'building_enter', 'Bakkal', 'Çetin: "Bakkal Naciye''nin dükkânındayız dedektif! Veresiye defteri, kırık şişe, tütün kesesi ve tezgâh anahtarını arayalım!"

Rıfat: "Naciye geveze bir kadındır! Ağzını açarsa cinayetten çok dedikodu dinlersiniz. Çömez de defterde adını arar artık!"

Çetin: "Adımı aramıyorum Rıfat amca, yırtılan borç senedi sayfasını arıyorum!"', 3, 1),
('cetin', 'building_enter', 'Hekim', 'Çetin: "Hekim Sevgi''nin muayenehanesindeyiz dedektif! Mor şişe, reçete, banotu kökü ve neştere dikkat edelim!"

Rıfat: "Sevgi titiz bir kadındır! Ona yanlış soru sorarsan neşteriyle seni kovalar amirim! Çömez polisi uyarayım da."

Çetin: "Akademide tıp etiği ve sorgulama teknikleri dersi aldım ben, kimse beni kovamaz!"', 3, 1),
('cetin', 'building_enter', 'Muhtarlık', 'Çetin: "Muhtar Cevdet''in makamındayız dedektif! Kasaba mührü, sahte tapu, çelik anahtar ve kırık gözlüğü inceleyelim!"

Rıfat: "Cevdet kasabayı parmağında oynatır! Sahte tapuyu bulsanız da kurnazlığına akıl erdiremezsiniz, çömezin aklı hiç ermez!"

Çetin: "Kanunlar karşısında hiçbir kurnazlık duramaz Rıfat amca! Mührün izini adli tıbba göndereceğim!"', 3, 1),
('cetin', 'building_enter', 'Kasabalı Evi', 'Çetin: "Fehmi Bey''in evindeyiz dedektif! Köstekli saat, mektup, çamurlu fener ve altı çizili kitaba bakalım!"

Rıfat: "Yaşlı Fehmi gaz lambasında kitap okur güya ama cinayet gecesi pencereden bizi izliyordu! Çömez de saati bozmasın ha!"

Çetin: "Saat zaten durmuş Rıfat amca! 02:14''te duran bu saat, olay anını sabitleyen en önemli delildir!"', 3, 1),
('cetin', 'building_enter', 'Ayakkabıcı', 'Çetin: "Kunduracı Rasim''in atölyesindeyiz dedektif! Çamurlu çizme, mumlu ip, deri bıçağı ve ayakkabı kalıbını arayalım!"

Rıfat: "Rasim huysuz adamdır, dükkânından bizi süpürgeyle kovmasın! Çizmelerdeki çamuru çömez polis temizlesin o zaman!"

Çetin: "Ben temizlikçi değilim Rıfat amca! Çamur örneğini adli labda analiz edip göl kenarındaki cinayet yeriyle karşılaştıracağım!"', 3, 1),

-- Bekçi Rıfat atışma


('rifat', 'building_enter', 'Hekim', 'Sevgi sinirli bir adam! Ona yanlış soru sorarsan patlar! Dikkatli olun!', 2, 1),

-- Genel mesajlar
('cetin', 'map_enter', NULL, 'İşte Gölge Şehir haritası! 8 binadan herhangi birine tıklayarak soruşturmaya başla. Her binada 4 delil gizli!', 1, 1),
('cetin', 'bag_open', NULL, 'Çantanızdaki delilleri İncele butonuyla detaylı inceleyebilirsiniz! Katili bulmak için ipuçlarını birleştirin!', 1, 1),
('cetin', 'npc_talk', NULL, 'Dikkatli soru sorun! Her şüphelinin 20 sorusu var. Doğru soruları seçin!', 1, 0),
('cetin', 'accuse', NULL, 'Son kararınızı vermeden önce tüm delilleri gözden geçirin! Gölge Şehir''de 8 şüpheli var, yanlış suçlama felaket olur!', 1, 1);

