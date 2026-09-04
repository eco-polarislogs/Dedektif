using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DedektiflikRPG.Models;
using System.Text.Json;

namespace DedektiflikRPG.Data;

public class AISeeder
{
    private readonly DatabaseRepository _repository;

    public AISeeder(DatabaseRepository repository)
    {
        _repository = repository;
    }

    public async Task Seed1000PlusDialoguesAsync()
    {
        using var db = _repository.CreateConnection();
        db.Open();
        
        int count = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM NPCDialogues WHERE Category LIKE 'local_ai_%'");
        if (count >= 1200) return; // Zaten seedlenmiş

        // Eğer veritabanı daha önce seedlendiyse (Eski jenerik verileri) temizle
        await db.ExecuteAsync("DELETE FROM NPCDialogues WHERE Category LIKE 'local_ai_%'");

        Console.WriteLine("Yapay Zeka İçin 1250+ Senaryo Bazlı Cümle Veritabanına Yükleniyor...");

        var allNPCs = await db.QueryAsync<NPC>("SELECT * FROM NPCs");
        
        string[] alibiQuestions = { "neredeydin", "o gece", "cinayet saati nerede idin", "evde miydin", "ne yapiyordun", "nerdeydin", "saat kacta" };
        string[] motiveQuestions = { "borc", "para", "tapu", "tehdit", "neden", "husumet", "ilişki", "sebep", "santaj", "kavga", "anlasma", "saat" };
        string[] weaponQuestions = { "satir", "bicak", "zehir", "sise", "gozluk", "rozet", "iplik", "delil", "balta", "kasa", "cizme", "defter", "tapu", "ip", "mum" };
        string[] accusationQuestions = { "sen yaptin", "katil sensin", "itiraf et", "suclu sensin", "sen öldürdün", "sen kiy", "sen misin katil" };

        var insertList = new List<NPCDialogue>();

        // 13 NPC (5 Gizemli + 8 Gölge) * 4 Kategori * 60 varyasyon = 3120+ Satır
        foreach (var npc in allNPCs)
        {
            // 1. Alibi Üretimi
            for (int i = 0; i < 60; i++)
            {
                insertList.Add(new NPCDialogue
                {
                    NPCId = npc.NPCId,
                    Category = "local_ai_alibi",
                    PlayerText = alibiQuestions[i % alibiQuestions.Length],
                    Difficulty = (i % 3) + 1,
                    NPCResponse = GenerateInnocentAlibi(npc.NPCId, i),
                    GuiltyResponses = JsonSerializer.Serialize(new Dictionary<string, string> { { npc.NPCId.ToString(), GenerateGuiltyAlibi(npc.NPCId, i) } }),
                    IsAccusatory = false
                });
            }

            // 2. Motif Üretimi
            for (int i = 0; i < 60; i++)
            {
                insertList.Add(new NPCDialogue
                {
                    NPCId = npc.NPCId,
                    Category = "local_ai_motive",
                    PlayerText = motiveQuestions[i % motiveQuestions.Length],
                    Difficulty = (i % 3) + 1,
                    NPCResponse = GenerateInnocentMotive(npc.NPCId, i),
                    GuiltyResponses = JsonSerializer.Serialize(new Dictionary<string, string> { { npc.NPCId.ToString(), GenerateGuiltyMotive(npc.NPCId, i) } }),
                    IsAccusatory = false
                });
            }

            // 3. Silah/Delil Üretimi
            for (int i = 0; i < 60; i++)
            {
                insertList.Add(new NPCDialogue
                {
                    NPCId = npc.NPCId,
                    Category = "local_ai_weapon",
                    PlayerText = weaponQuestions[i % weaponQuestions.Length],
                    Difficulty = (i % 3) + 1,
                    NPCResponse = GenerateInnocentWeapon(npc.NPCId, i),
                    GuiltyResponses = JsonSerializer.Serialize(new Dictionary<string, string> { { npc.NPCId.ToString(), GenerateGuiltyWeapon(npc.NPCId, i) } }),
                    IsAccusatory = false
                });
            }

            // 4. Doğrudan Suçlama Üretimi
            for (int i = 0; i < 70; i++)
            {
                insertList.Add(new NPCDialogue
                {
                    NPCId = npc.NPCId,
                    Category = "local_ai_accusation",
                    PlayerText = accusationQuestions[i % accusationQuestions.Length],
                    Difficulty = (i % 3) + 2, 
                    NPCResponse = GenerateInnocentAccusation(npc.NPCId, i),
                    GuiltyResponses = JsonSerializer.Serialize(new Dictionary<string, string> { { npc.NPCId.ToString(), GenerateGuiltyAccusation(npc.NPCId, i) } }),
                    IsAccusatory = true
                });
            }
        }

        using var transaction = db.BeginTransaction();
        try
        {
            var sql = @"
                INSERT INTO NPCDialogues (NPCId, Difficulty, Category, PlayerText, NPCResponse, GuiltyResponses, IsAccusatory)
                VALUES (@NPCId, @Difficulty, @Category, @PlayerText, @NPCResponse, @GuiltyResponses, @IsAccusatory)";
            
            await db.ExecuteAsync(sql, insertList.Select(x => new {
                x.NPCId,
                x.Difficulty,
                x.Category,
                x.PlayerText,
                x.NPCResponse,
                x.GuiltyResponses,
                IsAccusatory = x.IsAccusatory ? 1 : 0
            }), transaction);
            transaction.Commit();
            Console.WriteLine($"Başarıyla {insertList.Count} adet senaryo bazlı AI diyalog varyasyonu eklendi!");
        }
        catch(Exception ex)
        {
            transaction.Rollback();
            Console.WriteLine("AI Seed Hatası: " + ex.Message);
        }
    }

    private string GenerateInnocentAlibi(int npcId, int i)
    {
        string[] bases = npcId switch {
            1 => new[] { "Gece dükkandaydım, et doğruyordum.", "Soğuk hava deposunu temizliyordum.", "Veresiye defterimi kontrol ediyordum." },
            2 => new[] { "Eczanede nöbetçiydim, envanter sayıyordum.", "Arka odada tıbbi bitkilerimi kurutuyordum.", "Dükkandan hiç çıkmadım, pencereden sokağı izledim." },
            3 => new[] { "Evimdeydim, ertesi günün toplantısına hazırlanıyordum.", "Muhtarlık ofisinde geç saate kadar evrak imzaladım.", "Televizyon izliyordum, muhtarın sokakta işi olmaz." },
            4 => new[] { "Karakolda nöbetteydim, telsiz dinliyordum.", "Gece devriyesinden dönmüş, çay içiyordum.", "Olay raporlarını düzenliyordum amirim." },
            5 => new[] { "Atölyede dikiş dikiyordum, gözlerim yorulana kadar çalıştım.", "Yeni bir siparişi bitirmeye çalışıyordum.", "İhtiyar bir terzinin gece evinden başka nerede işi olur?" },
            101 => new[] { "Gece orman kulübemdeydim, odun kesiyordum.", "Keresteleri dizmekle meşguldüm.", "Fırtına vardı, dışarı çıkmadım." },
            102 => new[] { "Manav dükkanımı kapatıp evime geçtim.", "Meyveleri çürüklerinden ayırıyordum.", "Akşam ezanından sonra evimdeydim." },
            103 => new[] { "Ocağı söndürüp uykuya daldım.", "Gece boyu demirhanemde değildim, evimde dinleniyordum.", "Akşamdan sonra çekiç sallamam ben." },
            104 => new[] { "Bakkalımı erkenden kapatıp evime geçtim.", "Veresiye defterimi temize çekiyordum evimde.", "Dükkanı kilitledikten sonra sokağa çıkmadım." },
            105 => new[] { "Muayenehanemde tıp kitapları okuyordum.", "Bitkilerimi kurutmakla meşguldüm.", "Gece boyu kimse kapımı çalmadı, içerideydim." },
            106 => new[] { "Muhtarlık binasında evrak işleriyle uğraşıyordum.", "Makamımda oturmuş kasabanın sorunlarını düşünüyordum.", "Gece dışarıda ne işim olur, ofisimdeydim." },
            107 => new[] { "Gaz lambasının ışığında kitabımı okuyordum.", "Penceremin önünde oturup dışarıyı izledim.", "Evimdeydim, yaşlı adam gece ne yapsın." },
            108 => new[] { "Kundura tezgahımda deri kesiyordum.", "Siparişleri yetiştirmeye çalışıyordum.", "Atölyemden dışarı adım atmadım." },
            _ => new[] { "Evimdeydim.", "Çalışıyordum.", "Dinleniyordum." }
        };
        string[] extras = { " Neden sordunuz?", " Yağmur yağıyordu zaten dışarı çıkmadım.", " Kimseyi görmedim.", " Siz de beni mi sorguluyorsunuz?" };
        return bases[i % bases.Length] + extras[(i / bases.Length) % extras.Length];
    }
    private string GenerateGuiltyAlibi(int npcId, int i)
    {
        string[] bases = npcId switch {
            1 => new[] { "Gece... dükkandaydım. Yağmurdan dolayı kapıyı erken kitledim sadece.", "Evime geçmiştim... Evet, kesinlikle evimdeydim.", "Etleri depoya koyup... sokağa adımımı atmadım." },
            2 => new[] { "Eczanedeydim... Belki bir ara hava almak için kapıya çıkmışımdır.", "Reçete defterime bakıyordum, dükkandan çıkmam gerekecek bir durum yoktu.", "Arka tarafta bitkilerle ilgileniyordum... Dışarıdaki gürültüyü duymadım." },
            3 => new[] { "Evimdeydim dedim ya! Belki bir ara camdan dışarı bakmışımdır.", "Ofisteydim... Bazı tapu meselelerini... yani sıradan işleri hallediyordum.", "Sokağa çıkmadım. O saatte yağmurda ne işim olur?" },
            4 => new[] { "Devriyedeydim... Ama kurbanın sokağına hiç uğramadım.", "Karakoldan çıkmadım. Telsizden anons gelene kadar bekledim sadece.", "Bana sormayın, telsiz kayıtlarına bakın." },
            5 => new[] { "Dikiş dikiyordum... Ellimdeki iğne battı, biraz hava almak istedim o kadar.", "Evimdeydim... Yani atölyemde. Sadece birine bir paket teslim etmem gerekiyordu ama gitmedim.", "Ben yaşlı bir adamım, o saatte dışarıda dolaşmam." },
            101 => new[] { "Ormandaydım ama... çam ormanı girişine hiç inmedim.", "Kulübemdeydim! Ekrem'le karşılaşmadım!", "O gece dışarı çıktım ama cinayetle alakam yok!" },
            102 => new[] { "Evimdeydim... Yani, çürük elmaları çöpe atmak için çıkmış olabilirim.", "Dükkanda değildim! Gidin başkasına sorun.", "Benim o saatte orman girişinde ne işim olur?" },
            103 => new[] { "Ocağım tütüyordu ama ben evdeydim... İnanmıyorsanız komşulara sorun.", "Demirhanede yalnızdım, kimse beni görmemiş olabilir.", "Neden bana soruyorsunuz? Evimdeydim!" },
            104 => new[] { "Bakkalın arkasındaydım... Sadece hava alıyordum.", "Evime gitmiştim! Belki bir an dükkana geri dönmüşümdür.", "Beni sıkıştırmayın, uyuyordum diyorum!" },
            105 => new[] { "Muayenehanedeydim... Bir hastam vardı ama adını veremem.", "Otlarımı toplamak için ormana... hayır, bahçeye çıkmıştım.", "Gece boyu içerideydim, beni kimse dışarıda göremez." },
            106 => new[] { "Muhtarlık ofisindeydim! Bazı gizli evraklar için... dışarı çıkmam gerekmiş olabilir.", "Evimdeydim amirim, neden bana inanmıyorsunuz?", "Ben bu kasabanın muhtarıyım, nerede olduğumu size mi hesap vereceğim?" },
            107 => new[] { "Kitap okuyordum... Sadece bir an için pencereden dışarı bakmışımdır.", "Evimdeydim! Ayak sesleri falan duymadım ben.", "Yaşlı bir adamım, beni neden suçluyorsunuz?" },
            108 => new[] { "Tezgahımın başındaydım! Belki çizmeleri denemek için dışarı çıkmışımdır.", "Atölyemden hiç çıkmadım, o çamur izleri bana ait değil!", "Uyuyordum! Gidin asıl katili bulun." },
            _ => new[] { "Evimdeydim diyorum!", "O saatte dışarı çıkmadım!" }
        };
        string[] extras = { " İnanmıyorsanız komşulara sorun.", " O saatte herkes uyur, kimse sizi görmez zaten.", " Sorularınız çok bunaltıcı amirim.", " Eminim gerçek katil dışarıda geziyordu." };
        return bases[i % bases.Length] + extras[(i / bases.Length) % extras.Length];
    }
    
    private string GenerateInnocentMotive(int npcId, int i)
    {
        string[] bases = npcId switch {
            1 => new[] { "Osman iyi müşterimdi, veresiye borcu vardı ama öderdi.", "Kurbanla sadece et ticaretimiz vardı.", "Benim onla bir derdim yok, borcu olan benim derdim olmaz." },
            2 => new[] { "Sadece sıradan bir hastaydı, kalbi için ilaç alırdı.", "Kurbanın sağlık sorunları vardı, ben sadece tedavi etmeye çalışıyordum.", "Arkamızdan konuşanlar olur ama ben kimseye kin tutmam." },
            3 => new[] { "Arazi meseleleri her zaman olur, ama bunlar siyasetin cilvesi.", "Osman ile ara sıra tartışırdık ama kasaba için ortak paydada buluşurduk.", "Ölümünden benim ne çıkarım olabilir? Belediye işleri zaten kanunla yürür." },
            4 => new[] { "Bir polis olarak kasabadaki herkesle mesafeliyimdir. Osman da sadece bir vatandaştı.", "O adamın bazı karanlık işleri vardı ama benim masama hiç düşmedi.", "Bana rüşvet teklif eden çok olur, ben adaletten şaşmam." },
            5 => new[] { "Ben sadece ceketini diken yaşlı bir terziyim, adamla ne alıp veremediğim olabilir?", "Osman bey kıyafetine düşkündü, gizli cepler isterdi ama ne sakladığını hiç sormadım.", "Bizim gibi esnaf, müşterisiyle husumet kurmaz." },
            101 => new[] { "Ekrem Bey ormandan kereste alırdı, pazarlık ederdik ama öldürecek kadar düşman değildik.", "Ağaç kesim sınırları yüzünden mahkemelik olmuştuk ama kan dökecek adam değilim ben.", "Bana borcu yoktu, sadece orman sınırlarında hak iddia ediyordu." },
            102 => new[] { "Manav dükkânımı ipotek ettirmiştim ama vadesi dolmamıştı, borcumu ödeyecektim.", "Ekrem Bey sert adamdı ama meyve sebzesini benden alırdı.", "Borç için insan öldürülmez amirim, dükkanımı haczetse de canına kıymazdım." },
            103 => new[] { "Ekrem Bey özel kasa siparişi vermişti, parasını kuruşu kuruşuna ödedi.", "Aramızda ticaret dışında tek kelime geçmedi, husumetim yoktu.", "Demirci adam çelikle uğraşır, entrikayla değil." },
            104 => new[] { "Veresiye defterinde yüklü borcu vardı ama zengin adamdı, elbet kapatırdı.", "Tehdit falan etmedim, sadece borcunu ne zaman ödeyeceğini sordum.", "Bakkal esnafıyım ben, müşterimi niye öldüreyim?" },
            105 => new[] { "Ekrem Bey hastamdı, kalp rahatsızlığı için şifalı bitkiler hazırlardım.", "Geçmişimdeki sırlarımı bildiği dedikodusu yalan, hekim hastasına zarar vermez.", "Şantaj iddiaları asılsızdır, ona sadece tıbbi yardım sundum." },
            106 => new[] { "Muhtarlık olarak kasabanın orman arazilerini korumak görevim. Ekrem ile resmi temaslarımız oldu.", "Tapu devirlerinde anlaşmazlık çıksa da hukuk yolunu seçtik.", "Ekrem'in ölümü kasabaya sadece zarar getirdi, benim bir çıkarım yok." },
            107 => new[] { "Dedemden kalan köstekli saati borca karşılık rehin almıştı ama geri vereceğine söz vermişti.", "Eski bir muallim olarak adalet duygum intikamdan üstündür.", "Kitaplarımı okur kendi halimde yaşarım, kimseyle düşmanlığım olmaz." },
            108 => new[] { "Kaçak deri alımında ticari anlaşmazlık yaşadık ama hesabımızı kapatmıştık.", "Ekrem Bey sert bir tüccardı ama aramızda kan davası yoktu.", "Çizmelerini tamir ederdim, o kadar." },
            _ => new[] { "Kurbanla ticaretimiz vardı.", "Sadece bir müşterimdi." }
        };
        return bases[i % bases.Length] + " Lütfen beni asılsız şeylerle suçlamayın.";
    }
    private string GenerateGuiltyMotive(int npcId, int i)
    {
        string[] bases = npcId switch {
            1 => new[] { "Osman o borcu asla ödemeyecekti... Bunu herkes biliyordu. Onun gibi kibirli adamlar başkalarını ezmeyi sever.", "Ete para vermez, ama lüks içinde yaşardı... Borcunu istediğimde bana güldü.", "O adamın ne kadar zalim olduğunu kimse bilmiyor. Belki de birisi artık dayanamamıştır." },
            2 => new[] { "Bazı hastalar çok talepkardır... Sürekli tehdit ederler. Osman da haddini aşanlardandı.", "Bana sırrımı ifşa edeceğini söylüyordu... Kendini çok akıllı sanıyordu.", "Eczacı olmak zordur, bazen yanlış reçete yazan doktorların hatalarını bizim temizlememiz beklenir." },
            3 => new[] { "O arsalar kasabanın geleceği için lazımdı! Ama Osman hep engel oluyordu, vizyonsuz bir adamdı.", "Beni şikayet etmekle tehdit ediyordu. Sanki bu kasabayı ben inşa etmemişim gibi!", "Onun inadı yüzünden kasaba kaybediyordu. Birinin onu yoldan çekmesi... yani ikna etmesi lazımdı." },
            4 => new[] { "Osman çok tehlikeli sulara girdi. Benim gibi birine şantaj yapmak akıl karı değil.", "Herkesin sırrı vardır amirim. Osman benim sırrımı öğrenmişti... ve bunu kullanmaya kalktı.", "Adalet bazen kör değildir amirim. Bazen adaleti kendi ellerinle sağlaman gerekir." },
            5 => new[] { "Beni o USB ile köşeye sıkıştırdı. Yılların emeğini bir çırpıda silecekti.", "Benden gizli cepler dikmemi isterdi, ama o ceplerde ne taşıdığını öğrendiğimde beni susturmak istedi.", "Yaşlı bir adamı küçümserseniz, o iğnenin ne kadar derine batacağını tahmin edemezsiniz." },
            101 => new[] { "Ekrem beni ormandaki kaçak kesimlerle savcılığa vermekle tehdit ediyordu! Hayatımı zindana çevirecekti.", "Bütün ormanı kendi tapulu malı sandı, beni aç bırakmaya yemin etmişti.", "O adam laftan anlamazdı, baltanın dilinden anlardı ancak..." },
            102 => new[] { "Dükkânımı elimden alıp beni sokağa atacaktı! İki yetim çocuğumla nereye giderdim?", "Zehirli elmayı hak etti... Beni açlıkla terbiye etmeye çalışıyordu.", "Gözümün içine baka baka alay etti borcumla. Sonunda bedelini ödedi." },
            103 => new[] { "Kasanın içine koyduğu o karanlık anlaşmayı yok etmem gerekiyordu! Beni rüşvete alet etti.", "Demirci Kazım'a emir veremezdi! Haddini çok aştı.", "O çelik kilit onun mezarı oldu..." },
            104 => new[] { "Veresiye borcunu sordum diye bakkalımı zabıtayla kapatmakla tehdit etti!", "Fare zehri onun gibi bir parazit için en doğru ilaçtı.", "Gölge Şehir'in kanını emiyordu, birinin onu durdurması şarttı." },
            105 => new[] { "Tıp diplomamın sahte olduğunu öğrenmişti... Bana şantaj yapıp servetimi istiyordu!", "Banotu ölümcüldür evet ama o adam zaten ruhen çürümüştü.", "Beni hapse attıracaktı, mesleğimi elimden alacaktı... Mecbur kaldım!" },
            106 => new[] { "Sahte tapuları savcıya götürseydi muhtarlık makamım da itibarım da biterdi!", "Bütün kasabayı ben yönetirim, bir tüccar bana şantaj yapamaz!", "Resmi mührümü kendi emellerine alet etmeye kalktı, cezasını kestim." },
            107 => new[] { "Babamdan kalan tek hatırayı, o köstekli saati gasp etti ve bana güldü!", "Onurumu ayaklar altına aldı. Yaşlı bir öğretmene hakaret etmenin bedeli ağırdır.", "O gece saat tam 02:14'te durdu... Tıpkı Ekrem'in kalbi gibi." },
            108 => new[] { "Kaçak deri sevkiyatında bana pusu kurdurup borcumu iki katına çıkardı!", "42 numara çizmelerin altında ezileceğini düşünmemişti.", "Kunduracı Rasim'i dolandırmanın bedelini canıyla ödedi." },
            _ => new[] { "Beni tehdit ediyordu!", "Başka çarem kalmamıştı." }
        };
        return "*Soğukkanlı görünmeye çalışarak* " + bases[i % bases.Length] + " Kim yaptıysa, kasabayı büyük bir yükten kurtarmış.";
    }
    
    private string GenerateInnocentWeapon(int npcId, int i)
    {
        string[] bases = npcId switch {
            1 => new[] { "O kanlı satır çalınmıştı amirim! Polise de bildirdim.", "Önlüğümdeki kan dana kanı, insan değil. Kasabım ben!", "O defteri herkes görebilir, üstünü çizmem borcunu ödediği içindi." },
            2 => new[] { "Boş şişeyi çöpe atmıştım, biri oradan almış olmalı.", "Zehirli sarmaşığı sadece merhemler için kullanıyorum. Asla içilmez.", "Yırtık sayfalar... Onlar sadece yanlış yazdığım reçetelerdi." },
            3 => new[] { "Kırık gözlük mü? Kurban buraya geldiğinde düşürmüştü, geri verecektim.", "O mektup sinirle yazıldı, ama asla gönderilmedi.", "Kasadaki evraklar sadece eski kopyalar. Cinayet silahı sayılamazlar." },
            4 => new[] { "Polis rozetim mi? Onu devriyede düşürmüş olmalıyım.", "O düğme çok sıradan bir düğme, benim paltoma ait olduğunu nereden çıkardınız?", "Dosya kilitliydi, kurbanın geçmişini araştırmak benim görevim." },
            5 => new[] { "İplik makarasındaki kan benim elime ait, dikiş dikerken iğne battı.", "Kurbanın ceketi burada dikildi, kumaş parçası kalması çok doğal.", "Gizli cebi o istedi, içine ne koyduğunu ben bilemem." },
            101 => new[] { "Baltamdaki leke çam reçinesi ve pas amirim, kan değil!", "Kesim defterim orman işletmelerine açıktır, gizlim saklım yok.", "Orman kulübemde yüzlerce balta var, hangisinden şüpheleniyorsunuz?" },
            102 => new[] { "Kırık kasa tezgahımın altındaydı, kaza ile devrildi.", "Zehirli elma mı? Bahçemdeki meyveler organiktir, kimseye zehir vermem.", "Pelerinimi yağmurda herkes giyer, cinayetle ne ilgisi var?" },
            103 => new[] { "Çelik kasanın anahtarını Ekrem Bey'in kendisine teslim etmiştim.", "Demir tozu ocakta çalışan her demircinin üstüne siner.", "Kasa şifresi özel yapımdı, ben sadece mekanizmayı dövdüm." },
            104 => new[] { "Veresiye defterindeki yırtık sayfa eski bir borç kaydıydı, yırttım attım.", "Fare zehirleri bakkalın ambarını farelerden korumak için, insana verilmez.", "Kırmızı çizgi borcun ödendiğini gösterir amirim." },
            105 => new[] { "Banotu özünü anestezik merhem hazırlamak için kullanıyorum, reçetem resmi.", "Yırtık reçete sayfası hatalı dozaj yazdığım için çöpe atılmıştı.", "Şişelerim kilitli dolaptadır, kimse izinsiz alamaz." },
            106 => new[] { "Resmi muhtarlık mührü makamımda kilitlidir, sahtecilik yapmadım.", "Çam ormanı tapusu belediye arşivine aittir.", "Tehdit mektubu bana ait değil, el yazısı incelensin!" },
            107 => new[] { "Köstekli saat babamın hatırasıydı, cinayetle ilgisi yok.", "Eski kitapların altını çizmek öğretmenlik alışkanlığımdır.", "Penceremin altındaki ayak izleri sokaktan geçen herkesin olabilir." },
            108 => new[] { "42 numara çizmeleri kasabanın yarısı giyiyor, sadece bana ait değil.", "Mumlu ayakkabı ipi deri dikmek için kullanılır, boğma teli değil.", "Atölyemdeki çamur izleri ormandan gelen müşterilerden bulaştı." },
            _ => new[] { "O eşya benimle ilgili değil.", "Dikkatli incelerseniz başkasına ait olduğunu anlarsınız." }
        };
        return bases[i % bases.Length];
    }
    private string GenerateGuiltyWeapon(int npcId, int i)
    {
        string[] bases = npcId switch {
            1 => new[] { "Satır benim evet ama... herkesin satırı var! O kan... hayır, o dana kanı olmalı!", "Defterdeki kırmızı çizgi... Sadece sinirle çizdim. Bu beni katil yapmaz ki!", "Önlüğümdeki kan... Ben bütün gün et kesiyorum, ne bekliyordunuz?" },
            2 => new[] { "O zehirli bitkiyi herkes ormandan toplayabilir. Şişeyi de dışarıdan biri almış olabilir.", "Reçete sayfalarını ben yırttım çünkü... ticari sırdı. İlaç dozajıyla ilgisi yok!", "Kurbanın ilacına ben bir şey katmadım. Kendi kalbi dayanamamıştır belki..." },
            3 => new[] { "Gözlük... Evet kavga ettik, itişme oldu ve düştü. Ama onu ben öldürmedim!", "Tehdit mektubu sadece korkutmak içindi, icraata geçmek için değil.", "Sahte tapular benim siyasi kariyerim için, cinayet için değil. Bu deliller alakasız!" },
            4 => new[] { "Rozetim olay yerindeyse, cinayeti incelemeye gittiğim içindir! Olay yerini ilk ben gördüm.", "Düğme... Palto bana ait olabilir ama o düğme çoktan kopmuştu.", "Kurbanı copla mı darp etmişler? Benim copum her zaman belimdedir." },
            5 => new[] { "Kanlı iplik... O ip çok sağlamdır evet ama boğmak için kullanıldığını nereden çıkardınız?", "Ceketteki gizli cepte ne olduğu umurumda değil. Belki de notu o yazmıştır.", "Kumaş benden alınmış olabilir ama herkes benden kumaş alıyor." },
            101 => new[] { "Baltadaki kan lekesi... Sadece bir geyik kestim ormanda! Ekrem'e vurmadım!", "Kesim defterindeki tehdit notunu öfkeyle yazdım ama icraata geçmedim!", "Çam reçinesi kanı gizler sandım... yani hayır, sadece reçinedir o!" },
            102 => new[] { "O zehirli elmayı tezgahta unuttum... Ekrem'in onu yiyeceğini nereden bilebilirdim?", "Pelerinimin çamurlu olması suç mu? Yağmurda koştum sadece!", "Kırık kasadaki izler bana ait olabilir ama cinayetle alakam yok!" },
            103 => new[] { "Kasa kilidini sadece ben açabilirdim evet... Ama içindekileri almak cinayet sayılmaz!", "Çelik parmak izi... O kilit benim elimden çıktı, iz olması çok doğal!", "Demir tozu olay yerindeyse tesadüftür amirim!" },
            104 => new[] { "Fare zehrini tütününe karıştırmadım... Yani öyle bir niyetim yoktu!", "Veresiye defterindeki o sayfayı yırttım çünkü borcunu canıyla ödedi... hayır, yanlış söyledim!", "Kara liste sadece ticari bir uyarıydı!" },
            105 => new[] { "Banotu özünü 3cc ölümcül dozda hazırladığımı nereden biliyorsunuz?", "Reçetedeki yırtık kısım... Ekrem'in kalp ilacına kattığım şey sadece sakinleştiriciydi!", "Tırnaklarındaki morluklar zehir belirtisi değil, kalp krizidir!" },
            106 => new[] { "Sahte tapu ve mühürlü tehdit mektubu... Onu sadece korkutup kasabadan sürmek istedim!", "Resmi mührüm olay yerindeyse birisi çalıp komplo kurmuştur!", "Orman arazisini bana devretmeye mecburdu!" },
            107 => new[] { "Köstekli saat o gece 02:14'te durdu... Çünkü boğuşurken yere düştü! Ama onu öldürmek istemedim!", "Kitaptaki altı çizili satırlar cinayet planı değildi, edebi bir meraktı!", "Penceremin altındaki ayak izleri benim çizmelerime ait olamaz!" },
            108 => new[] { "Mumlu ayakkabı ipi çok dayanıklıdır... Boynuna dolandığında kopmayacağını biliyordum... yani hayır, deri dikiyordum!", "42 numara çamurlu çizmeler olay yerindeyse, herkes o çizmelerden giyiyor!", "Kaçak derileri geri almak için oradaydım sadece!" },
            _ => new[] { "O... O bana ait değil! Hayır, biri tezgahımı karıştırmış!" }
        };
        string[] extras = { " Bunlar sadece tesadüf amirim.", " Delilleri benim aleyhime kullanmaya çalışıyorsunuz.", " Mahkemede bu kanıtlar yetersiz kalır.", " Gerçek bir kanıtınız olsaydı şu an burada konuşmuyor olurduk." };
        return "*Gözlerini hafifçe kısarak* " + bases[i % bases.Length] + extras[(i / bases.Length) % extras.Length];
    }
    
    private string GenerateInnocentAccusation(int npcId, int i)
    {
        string[] bases = { "Haddinizi bilin amirim!", "Beni böyle doğrudan suçlayamazsınız!", "Masum bir insana iftira atmayın!", "Avukatımı çağıracağım!", "Ben bu kasabanın dürüst bir esnafıyım!" };
        return bases[i % bases.Length] + " Gidip gerçek katili bulun.";
    }
    private string GenerateGuiltyAccusation(int npcId, int i)
    {
        string[] bases = npcId switch {
            1 => new[] { "Ben sadece paramı istedim! Üzerime yürüdü, bana hakaret etti. Satır elimdeydi...", "Beni suçlayamazsınız! O adam kasabanın kanını emiyordu!", "Tamam! Tartıştık ama amacım sadece korkutmaktı... Kazaydı diyorum!" },
            2 => new[] { "Beni hapse atamazsınız! Ben hayat kurtarırım! O adamın yaşatılması bir hataydı.", "O zehir acısızdır amirim. Ona bir lütufta bulundum aslında.", "Bana şantaj yapıyordu! Başka ne yapabilirdim? Siz olsanız ne yapardınız?" },
            3 => new[] { "Ben bu kasabanın lideriyim! Osman benim yoluma çıkmamalıydı.", "Beni tutuklarsanız kasaba kaosa sürüklenir. O adamın ölmesi hepimizin iyiliği içindi.", "Evet ben yaptım! Çünkü kimsenin onu durduracak cesareti yoktu!" },
            4 => new[] { "Bana kelepçe takmak sizin haddinize mi? Ben yıllarımı bu mesleğe verdim!", "O beni teşkilata satacaktı. Kariyerimi bir çöp uğruna mahvedemezdim.", "Delilleri istediğim gibi karartırım. Kanıtlayamazsınız... En azından deneyene kadar." },
            5 => new[] { "Yıllarca onun pis işlerini dikip gizledim. Sonunda beni çöpe atamazdı.", "O ip onun boynuna çok yakıştı. Tıpkı diktiğim ceketler gibi kusursuz.", "Ben yaşlı biriyim. Cezaevinde ne kadar yaşarım ki? Ama o... o cezasını buldu." },
            101 => new[] { "Ekrem ormanı da beni de yok edecekti! Baltayı kaldırdığımda gözlerinde o kibir vardı... Hak etti!", "Orman kanunları dedektif! Bana şantaj yapan adam o ormandan sağ çıkamazdı!", "Evet ben yaptım! Gece yarısı ormanın girişinde hesabını kestim!" },
            102 => new[] { "Dükkânımı elimden alacaktı! Zehirli elmayı yerken hiç şüphelenmedi bile...", "Çocuklarımın rızkını bir tefeciye yediremezdim! Pişman değilim!", "Beni sokağa atmanın bedelini canıyla ödedi." },
            103 => new[] { "Kendi ellerimle dövdüğüm kasanın sırrını kimseyle paylaşamazdı! Demir levyeyle kafasına vurdum...", "Karanlık anlaşmalarını ifşa edeceğim diye beni tehdit ediyordu. Ateşle oynadı, yandı!", "Evet! O çelik kasanın önünde can verdi!" },
            104 => new[] { "Bakkal Naciye'ye borç takıp üstüne zabıtayla tehdit etmek neymiş öğrendi! Tütününe fare zehri koydum...", "Kasabanın kanını emen bir keneyi temizledim, bana teşekkür etmelisiniz!", "Veresiye defterindeki son sayfası cehennem oldu!" },
            105 => new[] { "Diplomamı elimden alıp beni süründürecekti! Banotu kökü özü kalbini üç dakikada durdurdu...", "O bir hastaydı evet ama kasaba için ölümcül bir tümördü! Ben sadece ameliyat ettim.", "Bana şantaj yapmanın bedeli zehirli bir kadehtir dedektif!" },
            106 => new[] { "Muhtarlık makamımı bir tüccarın şantajına kurban edemezdim! O gece sahte tapuları alırken boğdum onu...", "Bu kasaba benim eserim! Kimse beni savcılıkla tehdit edemez!", "Resmi mühür adaletin mührüdür dedektif, ben kendi adaletimi sağladım!" },
            107 => new[] { "Babamın köstekli saatini gasp edip bana 'ihtiyar bunak' dedi! O gece 02:14'te bastonumun ucuyla işini bitirdim...", "Yıllarca eğittiğim talebelerime örnek bir adalet dersi verdim!", "O saat benim hakkımdı, canı da cehenneme!" },
            108 => new[] { "Mumlu ayakkabı ipini boynuna doladığımda gözlerindeki o korkuyu unutamıyorum... Kaçak derilerin hesabını kapattık!", "Kunduracı Rasim'i dolandırıp sokakta gezemezdi! 42 numara çizmelerimle göl kenarına taşıdım cesedini!", "Evet ben öldürdüm! Adalet mahkemelerde değil, sokakta tecelli eder!" },
            _ => new[] { "Ben yapmadım! Ben değildim!", "O... O bir kazaydı!" }
        };
        string[] evasions = { 
            "Ama elinizdeki kanıtlar beni içeri tıkmaya yetecek mi?", 
            "Bunu ispatlayamazsınız. Mahkemede her şeyi reddedeceğim.", 
            "Bunu kasaba halkına nasıl açıklayacaksınız? Herkes onu sevmediğimi biliyordu ama katil olduğuma inanmazlar.", 
            "Belki de... Belki de başkası da yardım etmiştir. Ne malum?"
        };
        return bases[i % bases.Length] + " " + evasions[(i / bases.Length) % evasions.Length];
    }
}
