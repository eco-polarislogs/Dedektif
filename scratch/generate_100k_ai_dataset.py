# -*- coding: utf-8 -*-
import sqlite3
import json
import os
import sys

def main():
    db_path = os.path.join(os.path.dirname(__file__), "..", "Data", "dedektiflik.db")
    db_path = os.path.abspath(db_path)
    
    print(f"Connecting to database: {db_path}")
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()

    # Create table if not exists with high-speed indexes
    cursor.execute("""
    CREATE TABLE IF NOT EXISTS NPCDialogues (
        DialogueId INTEGER PRIMARY KEY AUTOINCREMENT,
        NPCId INTEGER NOT NULL,
        Category TEXT NOT NULL,
        PlayerText TEXT NOT NULL,
        NPCResponse TEXT NOT NULL,
        Difficulty INTEGER DEFAULT 1,
        GuiltyResponses TEXT DEFAULT '',
        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
    );
    """)
    cursor.execute("CREATE INDEX IF NOT EXISTS idx_dialogue_npc_cat ON NPCDialogues(NPCId, Category);")
    cursor.execute("CREATE INDEX IF NOT EXISTS idx_dialogue_player ON NPCDialogues(PlayerText);")
    conn.commit()

    # Clear existing to cleanly seed 100,000+ records
    cursor.execute("DELETE FROM NPCDialogues;")
    conn.commit()

    print("Generating 100,000+ High-Precision Contextual NLP Dialogues for 13 NPCs...")

    # NPC Master Definitions
    npcs = [
        # GİZEMLİ KASABA (1-5)
        {
            "id": 1, "name": "Kasap Hasan", "job": "Kasap", "victim": "Osman Bey",
            "alibi_innocent": "O gece dükkânımı geç kapattım amirim. Tezgâhta dana karkası doğruyor ve veresiye defterini kontrol ediyordum.",
            "alibi_guilty": "*Satırın sapını sıkarak terler* Dükkândaydım dedim ya! Et doğruyordum... Sadece Osman'ın evine borcunu istemeye gittim, hemen çıktım!",
            "weapon_innocent": "Dükkânda 10 tane satır var amirim. Hepsi et kesmek içindir, kayıtları bellidir.",
            "weapon_guilty": "*Satıra korkuyla bakar* O satırdaki kan dana kanıdır diyorum! Biri beni tuzağa düşürmek için tezgahımdan çalmış!",
            "motive_innocent": "Osman Bey'den 50 bin liralık et alacağım vardı ama helalleşmiştik, cinayet işleyecek adam mıyım ben!",
            "motive_guilty": "*Dişlerini sıkar* Yüzüme karşı borcunu ödemeyeceğini söyledi, alay etti! Gözüm döndü amirim!",
            "action_innocent": "Sabah köye gönderilecek dana etlerini satırla kemiğinden ayırıyordum.",
            "action_guilty": "*Elleri titrer* Dana doğruyordum ama kafamda Osman'ın alaycı sesi yankılanıyordu!",
            "person_opinion": "Muhtar Kemal ile Eczacı Selma'nın son günlerde gizli gizli fısıldaştığını duydum amirim."
        },
        {
            "id": 2, "name": "Eczacı Selma", "job": "Eczacı", "victim": "Osman Bey",
            "alibi_innocent": "Gece yarısına kadar dükkânım açıktı amirim. Nöbetçi eczaneydim, tezgah arkasında şifalı bitkiler ve farmakoloji kitabı okuyordum.",
            "alibi_guilty": "*Gözlerini kaçırara fısıldar* Nöbetçi bendim... Sadece ilaç teslimi için 15 dakika dışarı çıktım, cinayetle alakam yok!",
            "weapon_innocent": "Eczanedeki tüm toksik bileşenler kilitli çelik dolaptadır ve bakanlık denetimindedir.",
            "weapon_guilty": "*Zehir şişesini saklamaya çalışır* O boş şişe reçeteli bir kalp ilacıydı! Kurban aşırı doz içmişse benim suçum ne?!",
            "motive_innocent": "Osman Bey kalp hastasıydı, sadece tansiyon ve kalp haplarını düzenli almasını tembihlerdim.",
            "motive_guilty": "*Ağlamaklı olur* Eczanemin ruhsatıyla beni tehdit etti! Mesleğimi elimden alacaktı!",
            "action_innocent": "Tezgahta şifalı bitkiler farmakoloji ansiklopedisini okuyor, dijitalis dozajlarını inceliyordum.",
            "action_guilty": "*Gözlerini kırpıştırır* Zehirli bitkilerin kalp yetmezliği üzerindeki ölümcül etkilerini okuyordum... sadece mesleki merak!",
            "person_opinion": "Kasap Hasan'ın satırını bilerken çok öfkeli olduğunu gördüm, Osman Bey ile büyük kavgası vardı."
        },
        {
            "id": 3, "name": "Muhtar Kemal", "job": "Muhtar", "victim": "Osman Bey",
            "alibi_innocent": "Muhtarlık binasındaydım amirim. Gece boyunca belediye bütçe taslağını ve arazi tahsis evraklarını inceliyordum.",
            "alibi_guilty": "*Masayı tıkırdatır* Makamda evrak düzenliyordum dedim ya! Gece 12 gibi hava almak için meydanda yürüdüm sadece!",
            "weapon_innocent": "Masadaki kırık okuma gözlüğü eski bir kazadan kalmadır amirim.",
            "weapon_guilty": "*Kırık gözlüğe bakar* Kurban bana makamda saldırınca gözlüğüm yere düştü! Nefsi müdafaaydı!",
            "motive_innocent": "Meydandaki arsa imarı hakkında resmi dilekçesini aldım, her şey kanuna uygundu.",
            "motive_guilty": "*Yumruğunu sıkar* Kasabanın en verimli arazilerine el koyup beni rezil etmekle tehdit ediyordu!",
            "action_innocent": "Kasaba su kanalı ihale dosyalarını ve vergi kayıtlarını mühürlüyordum.",
            "action_guilty": "*Evrakları telaşla kapatır* Osman'ın hacizli tapularını ve arazi devir senetlerini inceliyordum!",
            "person_opinion": "Komiser Güneş'in son zamanlarda karakol kasasından şüpheli evraklar çıkardığını duydum."
        },
        {
            "id": 4, "name": "Komiser Güneş", "job": "Polis Komiseri", "victim": "Osman Bey",
            "alibi_innocent": "Devriye gezisindeydim meslektaşım. Nöbet tutanağını doldurup sokak kontrollerine çıktım.",
            "alibi_guilty": "*Rozetini sıkar* Devriye turundaydım amirim! Osman'ın evinin önünden geçmiş olmam katil olduğumu göstermez!",
            "weapon_innocent": "Polis rozetim ve beylik tabancam her zaman envanterime zimmetlidir.",
            "weapon_guilty": "*Rozete bakar* Olay yerinde bulunan rozet bana ait değil! Çalınmış rozetle bana kumpas kuruyorlar!",
            "motive_innocent": "Karakola gelip çevre esnafı hakkında şikayette bulunmuştu, görevimizi yaptık.",
            "motive_guilty": "*Terler* Eski bir rüşvet dosyasını savcılığa vermekle beni tehdit etti! Mesleğim bitecekti!",
            "action_innocent": "Karakol devriye nöbet defterini ve kasaba giriş-çıkış tutanaklarını inceliyordum.",
            "action_guilty": "*Defteri kapatır* Osman'ın hakkımdaki şantaj mektuplarını ve gizli ifadeleri inceliyordum!",
            "person_opinion": "Terzi Yahya'nın dükkânında gizli bölmeler olduğunu ve kaçak kumaş sakladığını biliyorum."
        },
        {
            "id": 5, "name": "Terzi Yahya", "job": "Terzi", "victim": "Osman Bey",
            "alibi_innocent": "Dükkânımda kışlık paltonun astarlarını dikiyordum. İpliğim bitene kadar tezgâhtaydım amirim.",
            "alibi_guilty": "*İğneyi kumaşa batırır* Dükkânda kumaş biçiyordum amirim... Gece yarısı hava almak için çıktım sadece!",
            "weapon_innocent": "Kullandığım iplikler İngiliz malı dayanıklı terzi ipliğidir, her terzide bulunur.",
            "weapon_guilty": "*İplik makarasını cebine iter* O iplik sağlamdır evet... Boğulma aleti nereden çıktı amirim?!",
            "motive_innocent": "Yeni diktiğim kaşe kabanın provasını yaptık, çok memnun kaldı.",
            "motive_guilty": "*Gözlerini siler* Paltonun içine diktiğim gizli cebi ve ortaklık parasını zorla benden alacaktı!",
            "action_innocent": "Kış için sipariş edilen kaşe paltonun kol astarını ve yakasını teyelliyordum.",
            "action_guilty": "*İpliği parmağına dolar* Osman'ın istediği gizli cepli paltonun astarını dikiyordum!",
            "person_opinion": "Kasap Hasan'ın satırından kan damladığını o gece pencereden görenler olmuş."
        },

        # GÖLGE ŞEHİR (101-108)
        {
            "id": 101, "name": "Oduncu Tahsin", "job": "Oduncu", "victim": "Ekrem Bey",
            "alibi_innocent": "Orman kulübemdeydim amirim. Yağmur başlamadan önce sobalık kütükleri yarıp istifledim ve yattım.",
            "alibi_guilty": "*Baltanın sapını sıkar* Kulübedeydim diyorum! Feneri alıp orman patikasına çıktım ama cinayetle alakam yok!",
            "weapon_innocent": "Baltamdaki koyu lekeler çam ve meşe reçinesidir, adli tıp incelesin.",
            "weapon_guilty": "*Baltayı arkasına saklar* Baltamdaki izler reçinedir! Kanla reçineyi ayırt edemiyor musunuz?!",
            "motive_innocent": "Kereste fiyatları üzerine pazarlık ettik ama helalleşip ayrıldık.",
            "motive_guilty": "*Baltayı yere vurur* Kaçak kestiğim ağaçları orman müdürlüğüne ihbar edip ekmeğimle oynayacaktı!",
            "action_innocent": "Sobalık meşe kütüklerini yarıp kışlık yakacak destesi yapıyordum.",
            "action_guilty": "*Sapı ovuşturur* Çam tomruklarını yarıyordum ama kafamda Ekrem'in şantajları yankılanıyordu!",
            "person_opinion": "Manav Ayşe'nin Ekrem Bey ile yüklü miktarda borç senedi olduğunu kasabada bilmeyen yok."
        },
        {
            "id": 102, "name": "Manav Ayşe", "job": "Manav", "victim": "Ekrem Bey",
            "alibi_innocent": "Akşam üzeri manavı kapattım, elma kasalarını saydıktan sonra evime çekildim.",
            "alibi_guilty": "*Pelerinini sıkar* Dükkânı kapattım... Gece sadece hava almak için göl kenarına yürümüştüm!",
            "weapon_innocent": "Dükkândaki meyve bıçakları sebze kasalarını açmak içindir.",
            "weapon_guilty": "*Yırtık pelerinine bakar* O kumaş parçası çivili kasaya takıldı! Olay yeriyle ilgisi yok!",
            "motive_innocent": "Kasa kasa nar ve elma siparişi vermişti, avansını ödedi.",
            "motive_guilty": "*Gözyaşı döker* Dükkânımı elimden alıp kumarhaneye çevirecekti, sokakta kalacaktım!",
            "action_innocent": "Göl kenarından gelen taze elma kasalarını sayıyor, çürükleri ayıklıyordum.",
            "action_guilty": "*Kasaları titreyerek düzeltir* Manavın borç defterine bakıyordum, Ekrem'in faizlerini hesaplıyordum!",
            "person_opinion": "Demirci Kâzım'ın gece yarısı örs başında garip kilitler dövdüğünü duydum."
        },
        {
            "id": 103, "name": "Demirci Kâzım", "job": "Demirci", "victim": "Ekrem Bey",
            "alibi_innocent": "Demirci ocağını akşam söndürdüm, örs başında saban demirlerini dövmüştüm. Yorgunluktan uyuyakalmışım.",
            "alibi_guilty": "*Çekici indirir* Ocakta demir dövüyordum... Gece yarısı fener sönünce dışarı çıktım kısa süreliğine!",
            "weapon_innocent": "Özel çelik kilitler ve saban uçları döverim, hepsi el emeğimdir.",
            "weapon_guilty": "*Örse bakar* O bıçak benim ocağımdan çıkmış olabilir ama ben satmadım! Çalınmış!",
            "motive_innocent": "Çelik kasanın yedek anahtarını teslim ettim, parasını aldım.",
            "motive_guilty": "*Örse sertçe vurur* Yaptığım kasanın gizli bölmesine sahte senetler koyup beni de hırsızlığa ortak edecekti!",
            "action_innocent": "Atölyede saban demirlerini tavında dövüp su veriyordum amirim.",
            "action_guilty": "*Körüğe bakar* Ekrem'in şifreli çelik kasasının kilit mekanizmasını dövüyordum!",
            "person_opinion": "Bakkal Naciye'nin zehirli tütün sattığını ve kasabayla gizli hesapları olduğunu bilirim."
        },
        {
            "id": 104, "name": "Bakkal Naciye", "job": "Bakkal", "victim": "Ekrem Bey",
            "alibi_innocent": "Bakkalın kepengini indirip veresiye defterindeki hesapları kapattım. Gece boyu evdeydim.",
            "alibi_guilty": "*Önlüğünü büker* Bakkalın arkasındaki depodaydım amirim! Sadece tütün sarıyordum!",
            "weapon_innocent": "Bakkal terazisi ve pirinç ağırlıklar dışında dükkânda kesici alet bulunmaz.",
            "weapon_guilty": "*Veresiye defterini kapatır* Yırtık sayfada zehir falan yoktu, sadece alacak listesi vardı!",
            "motive_innocent": "Eski veresiye borcunu kapattı, kahvesini içip gitti.",
            "motive_guilty": "*Dişlerini sıkar* Borcumu isteyince bakkalı yakmakla tehdit etti, canıma tak etmişti!",
            "action_innocent": "Haftalık gaz yağı, un ve şeker stok kayıtlarımı tutuyordum dedektifim.",
            "action_guilty": "*Defteri büker* Ekrem'in kasabaya taktığı borçların üzerini kırmızı kalemle çiziyordum!",
            "person_opinion": "Hekim Sevgi'nin gece yarısı göl kenarından zehirli otlar topladığını herkes bilir."
        },
        {
            "id": 105, "name": "Hekim Sevgi", "job": "Hekim", "victim": "Ekrem Bey",
            "alibi_innocent": "Muayenehanemde şifalı bitki tentürleri hazırlıyor ve tıp kodeksini inceliyordum. Gece kapım çalmadı.",
            "alibi_guilty": "*Havan elini sıkar* Muayenehanedeydim... Gece Ekrem'in sokağından geçtim ama sadece acil hastaya gidiyordum!",
            "weapon_innocent": "Tıbbi amaçlı baldıran otu ve anestezi tentürleri kilitli ecza dolabındadır.",
            "weapon_guilty": "*Mor şişeyi titreyerek tutar* O banotu özü tıbbi deneyler içindi! Kurbana ben içirmedim!",
            "motive_innocent": "Midesi için şifalı nane ve papatya çayı hazırlamamı istemişti.",
            "motive_guilty": "*Gözlüğü titrer* Eski bir tıbbi hatamı öğrenmiş, şantajla benden zehir hazırlamamı istiyordu!",
            "action_innocent": "Şifalı bitkiler kodeksini okuyup hastalar için papatya ve kantaron merhemi hazırlıyordum.",
            "action_guilty": "*Havanı sıkar* Baldıran otunun ve banotunun kanda iz bırakmayan formüllerini inceliyordum!",
            "person_opinion": "Muhtar Cevdet'in orman arazilerini sahte tapularla sattığını duymayan kalmadı."
        },
        {
            "id": 106, "name": "Muhtar Cevdet", "job": "Muhtar", "victim": "Ekrem Bey",
            "alibi_innocent": "Muhtarlık makamında köy meclisi kararlarını ve su şebekesi haritasını inceliyordum. Işığım yanıktı.",
            "alibi_guilty": "*Mührü sıkar* Ofisteydim dedim ya! Gece 2 gibi sadece kısa bir teftiş yürüyüşü yaptım!",
            "weapon_innocent": "Makam masamda resmi evrak mührü ve dolma kalemden başka bir şey bulunmaz.",
            "weapon_guilty": "*Sahte tapuları saklar* Bu belgeler resmi taslaklardı! Olay yeriyle ilgisi yok!",
            "motive_innocent": "Kasabanın su şebekesi ihalesini konuştuk, resmi toplantı tutanaklarımız var.",
            "motive_guilty": "*Sinirle bağırır* Çam ormanı arazisini kendi üstüne geçirip beni rezil edecekti!",
            "action_innocent": "Gölge Şehir'in yıllık vergi matrahlarını ve su şebekesi haritasını inceliyordum.",
            "action_guilty": "*Mührü vurur* Çam ormanı arazisinin imar tahsis dosyasını ve devir evraklarını okuyordum!",
            "person_opinion": "Muallim Fehmi'nin pencerelerden gizli gizli sokakları gözetlediğini bilirim."
        },
        {
            "id": 107, "name": "Muallim Fehmi", "job": "Muallim", "victim": "Ekrem Bey",
            "alibi_innocent": "Penceremin kenarında gaz lambasının ışığında divan şiirleri ve kasaba hatıratını okuyordum. 02:14'te sesleri duydum.",
            "alibi_guilty": "*Köstekli saatini saklar* Penceremdeydim evladım... Saat 02:14'te kapıya çıktım ama sadece temiz hava için!",
            "weapon_innocent": "Köstekli saatim rahmetli babamdan yadigârdır, hep yeleğimin cebindedir.",
            "weapon_guilty": "*Saatini tutar* Saat babamın emanetiydi! Olay yerinde düştüyse Ekrem çalmıştı demektir!",
            "motive_innocent": "Eski bir divan edebiyatı kitabı getirdi bana, saatlerce şiir okuduk.",
            "motive_guilty": "*Sesi titrer* Babamın altın saatini borç karşılığı elimden zorla aldı, geri vermiyordu!",
            "action_innocent": "Gaz lambasının ışığında Fuzuli Divanı ve Gölge Şehir'in eski hatıratlarını okuyordum evladım.",
            "action_guilty": "*Gözlüğünü siler* Mahkeme kararını okuyordum evladım... Ekrem'in saatime nasıl el koyduğunu anlatan kararı...",
            "person_opinion": "Kunduracı Rasim'in o gece çamurlu av çizmeleriyle gölden döndüğünü bizzat gördüm."
        },
        {
            "id": 108, "name": "Kunduracı Rasim", "job": "Kunduracı", "victim": "Ekrem Bey",
            "alibi_innocent": "Kundura tezgâhımda av çizmelerinin kösele tabanını mumlu iple dikiyordum. Gece dışarı adım atmadım.",
            "alibi_guilty": "*Deri bıçağını sıkar* Dükkândaydım! Çamurlu çizmeleri giyip dışarı çıktım ama göl kenarına gitmedim!",
            "weapon_innocent": "Mumlu saraç ipi ve kundura bıçağı her ayakkabıcıda bulunur amirim.",
            "weapon_guilty": "*Mumlu ipi çeker* Bu ip sadece taban dikmek içindir! Boğulma izleriyle eşleşmesi tesadüf!",
            "motive_innocent": "Av çizmelerinin tabanını diktirdi, teslim edip parasını ödedi.",
            "motive_guilty": "*Deri kayışını büker* Kaçak getirdiğim manda derilerini ihbar edip beni hapse attıracaktı!",
            "action_innocent": "Manda derisinden sağlam kışlık av çizmeleri dikiyordum amirim.",
            "action_guilty": "*Deri bıçağını sallar* Av çizmelerinin kösele tabanını dikiyordum, o gece göle gidecek çizmesini hazırlıyordum!",
            "person_opinion": "Oduncu Tahsin'in baltasında kan izi olduğunu ormandaki köylüler konuşuyor."
        }
    ]

    # Category and Question Intent Archetypes
    categories = [
        ("tanisma", ["merhaba", "selam", "kolay gelsin", "iyi günler", "selamlar amirim", "merhaba dedektif", "naber", "nasılsın", "hayırlı işler"]),
        ("alibi", ["cinayet gecesi neredeydin", "o saatte ne yapıyordun", "neredeydin o gece", "alibin ne", "saat kaçta neredeydin", "evde miydin", "dükkanda mıydın", "o gece neredeydin sen"]),
        ("eylem_detay", ["ne okuyordun", "hangi kitap", "ne yapıyordun", "ne doğruyordun", "hangi ilaç", "hangi evrak", "ne dikiyordun", "ne yazıyordun", "hangi kumaş", "hangi dosya"]),
        ("saat_zaman", ["saat kaçta çıktın", "ne kadar sürdü", "saat kaç gibiydi", "tam saat kaçta", "kaçta dükkandaydın", "saat kaçta eve döndün"]),
        ("sahid_tanik", ["şahidin var mı", "yanında kim vardı", "seni gören oldu mu", "yalnız mıydın", "tek başına mıydın", "bunu kim kanıtlar"]),
        ("delil_silah", ["bu satır ne", "bu zehirli şişe kimin", "bu rozet senin mi", "bu kırık gözlük kimin", "bu balta kime ait", "bu kumaş parçası ne", "deliller hakkında ne diyorsun"]),
        ("motif_borc", ["arandaki husumet neydi", "ne kadar borcu vardı", "neden tartıştınız", "tehdit ettin mi", "şantaj yaptın mı", "kurbanla aranız nasıldı"]),
        ("suclama_direkt", ["katil sensin itiraf et", "sen öldürdün değil mi", "suçlu sensin", "yalan söyleme sen yaptın", "cinayeti sen işledin", "sen vurdun"]),
        ("celiski_yalan", ["demin başka söyledin yalan söylüyorsun", "ifadende çelişki var", "doğruyu anlat yalanı bırak", "sözlerin birbirini tutmuyor"]),
        ("devrik_soru", ["sen misin katil", "kim sence vurdu", "nerdeydin o saatte sen", "ne saklıyorsun bizden sen", "öldürdün mü sen kurbanı", "kitap mı okuyordun sen"]),
        ("argo_ve_sacma", ["noldu la cinayet noldu", "katili bulamadın mı hala", "sacmalama anlat", "dogruyu de bana", "hacı sen mi vurdun", "yok ya inanmam", "hadi ordan"])
    ]

    # Modifiers and Variations to multiply into 100,000+ realistic sentences
    prefixes = ["", "bakar mısın ", "amirim soruyor ", "söyle bakalım ", "açık konuş ", "şüpheli ", "hey ", "lütfen söyle ", "dedektif olarak soruyorum ", "bana bak "]
    suffixes = ["", " peki?", " hemen cevap ver!", " doğruyu söyle.", " amirim bekliyor.", " saklama.", " şimdi açıkla.", " net konuş.", " ya sen?", " doğru mu?"]
    slang_typos = ["", " ", " ya", " be", " yahu", " acaba", " tam olarak", " dürüstçe"]

    records = []
    total_generated = 0

    print("Synthesizing multi-permutation dataset...")

    for npc in npcs:
        npc_id = npc["id"]
        guilty_dict = {
            str(npc_id): npc["alibi_guilty"]
        }
        guilty_json = json.dumps(guilty_dict, ensure_ascii=False)

        for cat_name, base_questions in categories:
            # Response selection based on category
            if cat_name in ["alibi", "devrik_soru", "argo_ve_sacma"]:
                resp = npc["alibi_innocent"]
            elif cat_name == "eylem_detay":
                resp = npc["action_innocent"]
            elif cat_name == "delil_silah":
                resp = npc["weapon_innocent"]
            elif cat_name == "motif_borc":
                resp = npc["motive_innocent"]
            elif cat_name == "suclama_direkt":
                resp = "Ben masumum amirim! İftiralarınızla dükkânımı meşgul etmeyin, katil dışarıda!"
            elif cat_name == "celiski_yalan":
                resp = "Ben ne söylediysem dürüstçe söyledim amirim. Lafımı çarpıtmayın, ifadem nettir."
            elif cat_name == "saat_zaman":
                resp = "Saatlerim tamamen nettir amirim, dükkânımın giriş çıkış saati bellidir."
            elif cat_name == "sahid_tanik":
                resp = "O saatte çevredekiler ve komşular beni görmüş olabilir amirim, saklayacak bir şeyim yok."
            else:
                resp = f"Merhaba dedektif bey. {npc['job']} olarak cinayet soruşturmasında size nasıl yardımcı olabilirim?"

            for base_q in base_questions:
                for pre in prefixes:
                    for suf in suffixes:
                        for sl in slang_typos:
                            full_q = f"{pre}{base_q}{sl}{suf}".strip()
                            if not full_q:
                                continue

                            # Generate a unique variation
                            records.append((
                                npc_id,
                                cat_name,
                                full_q,
                                resp,
                                2 if "suclama" in cat_name or "celiski" in cat_name else 1,
                                guilty_json
                            ))
                            total_generated += 1

                            # Bulk insert in batches of 10,000 for high performance
                            if len(records) >= 10000:
                                cursor.executemany("""
                                INSERT INTO NPCDialogues (NPCId, Category, PlayerText, NPCResponse, Difficulty, GuiltyResponses)
                                VALUES (?, ?, ?, ?, ?, ?);
                                """, records)
                                conn.commit()
                                print(f"  -> Inserted {total_generated} records...")
                                records = []

    if records:
        cursor.executemany("""
        INSERT INTO NPCDialogues (NPCId, Category, PlayerText, NPCResponse, Difficulty, GuiltyResponses)
        VALUES (?, ?, ?, ?, ?, ?);
        """, records)
        conn.commit()
        records = []

    cursor.execute("SELECT count(*) FROM NPCDialogues;")
    count = cursor.fetchone()[0]
    print(f"\n✅ SUCCESS! Total NPCDialogues in Database: {count} rows!")
    conn.close()

if __name__ == "__main__":
    main()
