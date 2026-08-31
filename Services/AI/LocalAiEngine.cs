using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DedektiflikRPG.Core.Interfaces;
using DedektiflikRPG.Data;
using DedektiflikRPG.Models;

namespace DedektiflikRPG.Services.AI;

/// <summary>
/// %100 Türkçeye Duyarlı, Gelişmiş Senaryo, Psikoloji ve Her Soruya Uygun Cevap Üreten Türkçe Yapay Zeka Motoru v5.0.
/// 
/// KESİN KURALLAR:
/// 1. Hiçbir NPC doğrudan "Ben suçluyum" veya "Cinayeti ben işledim" demez; katil kıvırır, delil ister, panikler veya hedef saptırır.
/// 2. Hiçbir masum NPC suç kabul etmez, iftiraya öfkelenir ve masumiyetini onurla savunur.
/// 3. Kullanıcı cinayet dışı, genel veya tamamen alakasız (hava durumu, futbol, yemek, aile, memleket, felsefe, din, siyaset, şaka, para vb.) 
///    ne sorarsa sorsun, NPC doğrudan sorulan o konuya kendi karakterine ve mesleğine uygun, son derece mantıklı bir cevap verir.
/// 4. Devrik cümleler ("katil kim", "kim katil", "sen misin katil", "sen öldürdün değil mi") eksiksiz çözümlenir.
/// 5. Selamlamalar ("selam", "merhaba", "kolay gelsin", "naber", "hayırlı işler") tekrara düşmeden zengin varyasyonlarla karşılanır.
/// </summary>
public class LocalAiEngine : IAIService
{
    private static readonly Random _random = new Random();
    private static readonly CultureInfo _cultureTr = new CultureInfo("tr-TR");
    private readonly DatabaseRepository? _repository;
    private readonly AntigravityAiService? _geminiService;

    public LocalAiEngine(DatabaseRepository? repository = null, string geminiApiKey = "", string geminiModel = "gemini-2.0-flash")
    {
        _repository = repository;
        if (!string.IsNullOrWhiteSpace(geminiApiKey) && geminiApiKey != "YOUR_GEMINI_API_KEY")
        {
            _geminiService = new AntigravityAiService(geminiApiKey, geminiModel);
        }
    }

    public async Task<AIInteractionResponse> GenerateResponseAsync(
        NPC npc,
        int guiltyNpcId,
        string userQuestion,
        IEnumerable<Clue>? cluesInBag = null,
        IEnumerable<DialogLog>? recentDialogs = null)
    {
        if (string.IsNullOrWhiteSpace(userQuestion))
        {
            return new AIInteractionResponse
            {
                Dialogue = "*Sessizce yüzünüzü süzüyor.* Söyleyecek bir şeyiniz yoksa zamanımı çalmayın amirim.",
                Emotion = "Sakin",
                TrustChange = 0
            };
        }

        bool isGuilty = (npc.NPCId == guiltyNpcId);
        var clues = cluesInBag?.ToList() ?? new List<Clue>();
        int npcCluesInBagCount = clues.Count(c => c.RelatedNPCId == npc.NPCId);
        var history = recentDialogs?.ToList() ?? new List<DialogLog>();

        // 1. ÖNCE GELİŞMİŞ LLM (GEMINI) İLE YANIT ALMAYI DENE (Canlı, her kelimeyi kavrayan yapay zeka)
        if (_geminiService != null)
        {
            try
            {
                var geminiResponse = await _geminiService.GetNPCResponseAsync(npc, userQuestion, clues, isGuilty, history);
                if (geminiResponse != null && 
                    !string.IsNullOrWhiteSpace(geminiResponse.Dialogue) && 
                    !geminiResponse.Dialogue.Contains("[AI Bağlantı Hatası") &&
                    !geminiResponse.Dialogue.Contains("[Zaman Aşımı]") &&
                    !geminiResponse.Dialogue.Contains("[Hata]"))
                {
                    return geminiResponse;
                }
            }
            catch { }
        }

        // 2. YEREL DERİN TÜRKÇE ZEKA MOTORU (KOTA DOLDUĞUNDA VEYA OFFLINE OLUNDUĞUNDA SIFIR KOPMA İLE DEVREYE GİRER)
        string rawTrLower = userQuestion.ToLower(_cultureTr).Trim();
        string normalizedAscii = TurkishTextEngine.NormalizeToAscii(rawTrLower);

        // 0. GELİŞMİŞ ÇOK TURLU BAĞLAM HAFIZASI (3-Turn Deep Memory & Context Engine)
        var lastLog = history.FirstOrDefault();
        var prevLog = history.Skip(1).FirstOrDefault();
        var thirdLog = history.Skip(2).FirstOrDefault();

        string lastPlayerQuestion = lastLog != null ? lastLog.PlayerQuestion.ToLower(_cultureTr) : "";
        string lastPlayerAscii = lastLog != null ? TurkishTextEngine.NormalizeToAscii(lastPlayerQuestion) : "";
        string lastNpcResponse = lastLog != null ? lastLog.NPCResponse.ToLower(_cultureTr) : "";

        string prevPlayerQuestion = prevLog != null ? prevLog.PlayerQuestion.ToLower(_cultureTr) : "";
        string prevPlayerAscii = prevLog != null ? TurkishTextEngine.NormalizeToAscii(prevPlayerQuestion) : "";
        string prevNpcResponse = prevLog != null ? prevLog.NPCResponse.ToLower(_cultureTr) : "";

        string thirdPlayerQuestion = thirdLog != null ? thirdLog.PlayerQuestion.ToLower(_cultureTr) : "";
        string thirdPlayerAscii = thirdLog != null ? TurkishTextEngine.NormalizeToAscii(thirdPlayerQuestion) : "";

        // Bağlam Tipi Belirleme (Son 3 turdaki konuların toplamı)
        string combinedRecentContext = $"{lastPlayerQuestion} {lastNpcResponse} {prevPlayerQuestion} {prevNpcResponse} {thirdPlayerQuestion}";
        string combinedRecentAscii = TurkishTextEngine.NormalizeToAscii(combinedRecentContext);

        bool lastWasAlibi = TurkishTextEngine.ContainsAnyConcept(combinedRecentContext, combinedRecentAscii, "nere", "saat", "gece", "neredeydin", "evde", "dukkan", "ne yap", "zaman", "ne isin vardi", "yalniz", "tek basina");
        bool lastWasWeapon = TurkishTextEngine.ContainsAnyConcept(combinedRecentContext, combinedRecentAscii, "satir", "bicak", "zehir", "sise", "gozluk", "rozet", "iplik", "balta", "kumas", "delil", "kanit", "esya", "saat", "silah", "canta", "parmak izi", "kan lekesi");
        bool lastWasMotive = TurkishTextEngine.ContainsAnyConcept(combinedRecentContext, combinedRecentAscii, "borc", "para", "tapu", "tehdit", "santaj", "kavga", "neden", "niye", "sebep", "husumet", "alacak", "hesap", "arazi");
        bool lastWasPerson = TurkishTextEngine.ContainsAnyConcept(combinedRecentContext, combinedRecentAscii, "hasan", "selma", "kemal", "gunes", "yahya", "tahsin", "ayse", "kazim", "naciye", "sevgi", "cevdet", "fehmi", "rasim", "ekrem", "osman");

        // Takip / Devam Sorusu Kalıpları
        bool isFollowUpWhy = TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "neden peki", "niye peki", "nicin peki", "neden boyle", "sebebi ne", "neden yaptin", "neden gittin", "niye gittin", "neden", "nicin", "niye ki");
        bool isFollowUpWho = TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "kim vardi", "kim gordu", "yaninda kim", "yalniz miydin", "tek miydin", "baska kimse", "sahidin var mi", "goren oldu mu", "kiminleydin", "biri var miydi");
        bool isFollowUpProof = TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "kanitla", "ispatla", "nasil inanayim", "nereden bileyim", "inandirici gelmedi", "kanitin var mi", "belgesi var mi", "nasil kanitlayacaksin", "nasil ispatlayacaksin", "nasil guveneyim", "nerden bileyim", "yalan soyluyorsun");
        bool isFollowUpTime = TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "saat kacta", "tam olarak ne zaman", "kac gibiydi", "ne kadar surdu", "kacta ciktin", "kacta dondun", "hangi saatte");
        bool isFollowUpDetail = TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "nasil yani", "ne demek bu", "ne gibi", "hangi sesler", "hangi belgeler", "ne konustunuz", "ne oldu sonra", "sonra ne yaptin", "daha detayli", "acikla", "anlat bakalim", "tam olarak");
        bool isFollowUpWhat = TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "ne okuyordun", "hangi kitap", "ne kitabi", "ne yapiyordun", "ne dogruyordun", "hangi ilac", "ne ilaci", "hangi evrak", "ne evraki", "ne dikiyordun", "hangi kumas", "ne ariyordun", "ne inceliyordun", "ne yaziyordun", "ne konusuyordun", "ne icindeydin");

        // 0.1 Olumlama / İsim isteme kontrolü
        bool isAffirmative = TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "evet", "tabi", "isim", "soyle", "istiyorum", "ver", "kim", "dinliyorum", "anlat");
        if (isAffirmative && lastNpcResponse.Contains("isim mi duymak"))
        {
            return new AIInteractionResponse
            {
                Dialogue = (npc.NPCId >= 100) ? GetRandomGolgeSuspectOpinion(npc.NPCId, guiltyNpcId) : GetRandomSuspectOpinion(npc.NPCId, guiltyNpcId),
                Emotion = "Ciddi",
                TrustChange = 2
            };
        }

        // 0.2 AKILLI TAKİP VE BAĞLAM YANITI
        if (lastLog != null && (isFollowUpWhy || isFollowUpWho || isFollowUpProof || isFollowUpTime || isFollowUpDetail || isFollowUpWhat))
        {
            var followUpResult = GenerateContextualFollowUpResponse(
                npc, isGuilty, rawTrLower, normalizedAscii,
                lastWasAlibi, lastWasWeapon, lastWasMotive, lastWasPerson,
                lastPlayerQuestion, lastNpcResponse, prevPlayerQuestion,
                isFollowUpWhy, isFollowUpWho, isFollowUpProof, isFollowUpTime, isFollowUpDetail, isFollowUpWhat);

            if (followUpResult != null)
            {
                return followUpResult;
            }
        }

        // 1. Türkçe Anlamsal / Niyet Analizi (Intent & Concept Detection)
        string processedSentence = TurkishTextEngine.PreprocessSentence(normalizedAscii);

        // DOĞRUDAN SUÇLAMA TESPİTİ (Tüm devrik ve ek kombinasyonları)
        bool isDirectAccusation = TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii,
            "katil sen", "katilsin", "suclu sen", "suclusun", "sen yaptin", "sen yapmissin", "sen oldurdun", 
            "sen kiy", "sen vurdun", "sen kestin", "sen bogdun", "sen zehirledin", "itiraf et", "itiraf eyle",
            "sen misin katil", "katil sen misin", "katil sensin", "senin parmagin var", "senin isin bu", "katil oldugunu biliyorum",
            "sen mi isledin", "sen mi yaptin", "sen mi oldurdun", "cinayeti sen mi", "zehri sen mi", "sen mi zehirledin", "sen mi kestin", "sen mi vurdun")
            || TurkishTextEngine.ContainsAnyConcept(rawTrLower, processedSentence,
            "katil sen", "suclu sen", "sen oldur", "itiraf et", "katil sen", "sen katil", "sen isle", "sen yap");

        // GENEL ŞÜPHELİ VE FİKİR SORGUSU (Devrik cümleler dahil)
        bool isOpinionQuery = !isDirectAccusation && TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii,
            "katil kim", "kim katil", "katil kimdir", "kimdir katil", "katil kim sence", "sence katil kim", "kim sence katil", "kim yapti", "fail kim",
            "supheli kim", "kimden suphe", "sence kim yapti", "sence kim", "fikrin ne", "ne dusunuyorsun", "biri var mi suphelendigin", "kimi sucluyorsun", "kim olabilir");

        // ÇELİŞKİ / YALAN SUÇLAMASI
        bool isLieAccusation = TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii,
            "yalan", "yalan soyluyorsun", "celiski", "demin oyle demedin", "az once", "baska sey soyledin", "farkli soyledin", "inkar etme", "dogruyu soyle", "gercegi anlat");

        // ALİBİ / MEKAN SORGUSU
        bool isAlibiQuery = !isDirectAccusation && !isOpinionQuery && TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii,
            "neredeydin", "nerdeydin", "o gece neredeydin", "o saatte ne isin vardi", "ne yapiyordun", "dukkaninda miydin", "evde miydin", "gece 2", "gece 11", "olay aninda neredeydin", "alibin ne", "sahidin kim");

        // DELİL / SİLAH SORGUSU
        bool isWeaponQuery = !isDirectAccusation && TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii,
            "satir", "bicak", "zehir", "sise", "mektup", "gozluk", "kasa", "rozet", "dugme", "iplik",
            "kumas", "usb", "cep", "defter", "delil", "kanit", "esya", "kanli", "kirik", "balta", "cizme", "ip", "silah", "ceset", "otopsi");

        // MOTİF / BORÇ / HUSUMET SORGUSU
        bool isMotiveQuery = !isDirectAccusation && TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii,
            "borc", "para", "tapu", "arazi", "rusvet", "tehdit", "santaj", "kavga", "tartisma",
            "neden olduruldu", "niye olduruldu", "sebep ne", "husumet", "alacak", "hesap", "dusman", "aralari nasildi");

        // SELAMLAMA VE HAL-HATIR KONTROLÜ (Tekrara düşmeyen zengin varyasyonlar)
        bool isGreeting = !isDirectAccusation && !isLieAccusation && !isWeaponQuery && !isMotiveQuery && !isAlibiQuery && !isOpinionQuery &&
            (TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii,
            "merhaba", "selam", "selamlar", "merhabalar", "gunaydin", "iyi gunler", "iyi aksamlar", "kolay gelsin", "nasilsin", "nasilsiniz", "hos bulduk", "tesekkur", "saol", "sagol", "meraba", "naber", "nbr", "hayirli isler", "hayirli gunler", "bereketli olsun", "ne haber")
            || TurkishTextEngine.ContainsAnyConcept(rawTrLower, processedSentence,
            "merhaba", "selam", "gunaydin", "iyi gunler", "iyi aksamlar", "kolay gelsin", "nasilsiniz", "hos bulduk", "tesekkur", "saol", "sagol", "nasilsin"));

        if (isGreeting)
        {
            var greetingResponse = GenerateDynamicGreeting(npc, rawTrLower, history.Count);
            return new AIInteractionResponse
            {
                Dialogue = greetingResponse,
                Emotion = "Sakin",
                TrustChange = 1,
                StressIncrease = 0
            };
        }

        // 1.5 DİĞER ŞÜPHELİLERİ SORMA KONTROLÜ
        bool mentionsHasan = (npc.NPCId != 1) && TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "hasan", "kasap");
        bool mentionsSelma = (npc.NPCId != 2) && TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "selma", "eczaci");
        bool mentionsKemal = (npc.NPCId != 3) && TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "kemal", "muhtar");
        bool mentionsGunes = (npc.NPCId != 4) && TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "gunes", "komiser", "polis");
        bool mentionsYahya = (npc.NPCId != 5) && TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "yahya", "terzi");

        bool mentionsTahsin = (npc.NPCId != 101) && TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "tahsin", "oduncu");
        bool mentionsAyse = (npc.NPCId != 102) && TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "ayse", "manav");
        bool mentionsKazim = (npc.NPCId != 103) && TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "kazim", "demirci");
        bool mentionsNaciye = (npc.NPCId != 104) && TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "naciye", "bakkal");
        bool mentionsSevgi = (npc.NPCId != 105) && TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "sevgi", "hekim", "doktor");
        bool mentionsCevdet = (npc.NPCId != 106) && TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "cevdet", "muhtar");
        bool mentionsFehmi = (npc.NPCId != 107) && TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "fehmi", "muallim", "ogretmen");
        bool mentionsRasim = (npc.NPCId != 108) && TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "rasim", "kunduraci", "ayakkabici");
        bool mentionsEkrem = TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "ekrem", "tuccar", "kurban", "maktul");
        bool mentionsOsman = TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "osman", "maktul", "kurban");

        string currentIntent = "none";
        if (isDirectAccusation) currentIntent = "local_ai_accusation";
        else if (isLieAccusation) currentIntent = "local_ai_lie";
        else if (isMotiveQuery) currentIntent = "local_ai_motive";
        else if (isWeaponQuery) currentIntent = "local_ai_weapon";
        else if (isAlibiQuery) currentIntent = "local_ai_alibi";

        int annoyanceLevel = 0;
        if (currentIntent != "none" && history.Any())
        {
            int repeated = 0;
            foreach (var log in history)
            {
                string logText = log.PlayerQuestion.ToLower(_cultureTr);
                string logAscii = TurkishTextEngine.NormalizeToAscii(logText);

                bool logAcc = TurkishTextEngine.ContainsAnyConcept(logText, logAscii, "sen yaptin", "katil sensin", "itiraf et", "sen oldurdun", "sen misin katil", "katilsin");
                bool logAlibi = TurkishTextEngine.ContainsAnyConcept(logText, logAscii, "neredeydin", "o gece", "evde miydin", "ne yapiyordun");
                bool logWeapon = TurkishTextEngine.ContainsAnyConcept(logText, logAscii, "satir", "bicak", "zehir", "sise", "gozluk", "rozet", "iplik", "balta", "cizme", "defter", "delil");
                bool logMotive = TurkishTextEngine.ContainsAnyConcept(logText, logAscii, "borc", "para", "tapu", "tehdit", "husumet", "sebep", "santaj");

                string logIntent = "none";
                if (logAcc) logIntent = "local_ai_accusation";
                else if (logAlibi) logIntent = "local_ai_alibi";
                else if (logWeapon) logIntent = "local_ai_weapon";
                else if (logMotive) logIntent = "local_ai_motive";

                if (logIntent == currentIntent) repeated++;
            }
            annoyanceLevel = Math.Min(3, repeated);
        }

        string responseText = "";
        string emotion = "Sakin";
        int trustChange = 0;
        int stressIncrease = 0;
        string? revealedSecret = null;

        // 1.6 ÇELİŞKİ / YALAN YAKALAMA TEPKİSİ
        if (isLieAccusation)
        {
            if (isGuilty)
            {
                stressIncrease = 30;
                emotion = "Panik";
                trustChange = -5;
                responseText = (npc.NPCId >= 101)
                    ? "*Yüzü bembeyaz kesilir ve elleri titrer* B-ben öyle demek istemedim amirim! Olay gecesinin şokuyla dilim sürçtü sadece... Beni köşeye sıkıştırmaya çalışmayın!"
                    : "*Gözlerini kaçırarak kekeler* Ş-şey... hafızam beni yanıltıyor olabilir amirim! O gece çok karanlıktı, kafam karışıktı diyorum size!";
            }
            else
            {
                stressIncrease = 5;
                emotion = "Ciddi";
                responseText = (npc.NPCId >= 101)
                    ? "Benim sözümde çelişki falan yok amirim. Ne gördüysem, ne yaşadıysam onu söyledim. İfademin arkasındayım."
                    : "Ben ne söylediysem dürüstçe söyledim amirim. Lafımı çarpıtmayın lütfen, ben doğruyu konuşuyorum.";
            }

            return new AIInteractionResponse
            {
                Dialogue = responseText,
                Emotion = emotion,
                TrustChange = trustChange,
                StressIncrease = stressIncrease
            };
        }

        // 2. DOĞRUDAN SUÇLAMA TEPKİSİ (STRICT RULES: NO CONFESSION EVER, INNOCENT MAINTAINS INNOCENCE)
        if (isDirectAccusation)
        {
            if (isGuilty)
            {
                stressIncrease = 25;
                if (clues.Count >= 2 && annoyanceLevel >= 2)
                {
                    emotion = "Gergin";
                    responseText = GetGuiltyEvadedResponse(npc.NPCId);
                }
                else
                {
                    emotion = "Savunmacı";
                    responseText = GetGuiltyDefensiveResponse(npc.NPCId);
                }
            }
            else
            {
                stressIncrease = 10;
                emotion = "Öfkeli";
                responseText = GetInnocentAccusationResponse(npc.NPCId);
            }

            return new AIInteractionResponse
            {
                Dialogue = responseText,
                Emotion = emotion,
                TrustChange = isGuilty ? -5 : -3,
                StressIncrease = stressIncrease
            };
        }

        // 3. KURBANLA İLİŞKİ SORGUSU
        bool isVictimRelationQuery = TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii,
            "kurbani tani", "maktulu tani", "osmani tani", "ekremi tani", "kurbani biliyor", "maktulu biliyor",
            "kurbanla arani", "maktulle arani", "kurbanla iliski", "maktulle iliski", "osmanla iliski", "ekremle iliski",
            "kurbani sor", "maktul hakkinda", "osman bey kim", "ekrem bey kim", "kurban kimdi", "maktul kimdi", "onu taniyor musun", "taniyor musun");

        if (isVictimRelationQuery)
        {
            string victimName = (npc.NPCId >= 100) ? "Ekrem Bey" : "Osman Bey";
            string victimRelationResponse = (npc.NPCId, isGuilty) switch
            {
                (1, true) => "*Satırı elinde sıkarak huzursuzca kıpırdanır* Osman Bey dükkânımın sürekli müşterisiydi... Et alırdı, veresiye yazdırırdı. Aramızda ufak tefek borç meseleleri dışında bir şey yoktu diyorum amirim!",
                (1, false) => "Osman Bey'i yıllardır tanırım amirim. Kasabamızın tanınmış simalarındandı. Haftada iki gün et almaya uğrar, kasaba dedikodularından konuşurduk. Mekânı cennet olsun.",

                (2, true) => "*Gözlerini ilaç şişelerine kaçırır, sesi titrer* Osman Bey mi? Kalp ve tansiyon ilaçlarını benden alırdı... Son zamanlarda sık sık eczaneye gelip özel karışımlar soruyordu. Başka da bir hukukumuz yoktu!",
                (2, false) => "Osman Bey düzenli olarak tansiyon ve kalp damar ilaçlarını eczanemden temin ederdi amirim. Saygılı bir beyefendiydi, son zamanlarda biraz dalgın ve stresli görünüyordu.",

                (3, true) => "*Evrakları sertçe kapatır, terini siler* Osman Bey kasabanın zenginlerindendi. Belediye arazileri ve imar işleri için sık sık makamıma gelirdi... Aramızda resmi işlemler dışında hiçbir husumet yoktu!",
                (3, false) => "Osman Bey kasabamızın köklü ailelerindendi. Muhtarlıkta arazi tescil ve vergi işlerini yürütürdük. Kasaba halkı için acı bir kayıp oldu.",

                (4, true) => "*Elini telsizine götürür, sert bir tavır takınır* Osman Bey'i karakola yaptığı resmi başvurulardan tanırım. Kasabadaki bazı esnafla husumeti vardı, bizden koruma talep etmişti... Hepsi bu kadar!",
                (4, false) => "Osman Bey karakolumuza sık sık uğrar, kasaba asayişi hakkında görüş bildirirdi. Vefatından önce tedirgin olduğunu fark etmiştim.",

                (5, true) => "*İğneyi telaşla parmağına batırır* Osman Bey benden takım elbiseler diktirirdi... En son ceketinin astarlarına kadar özenle dikmiştim. Benim gibi yaşlı bir terzinin onunla ne hesabı olabilir ki?!",
                (5, false) => "Osman Bey ceketlerini ve kışlık paltolarını bana diktirirdi amirim. Kumaş seçiminde çok titizdi, nur içinde yatsın.",

                // Gölge Şehir
                (101, true) => "*Baltanın sapını sıkar* Ekrem Bey orman arazilerinden kereste alırdı. Sürekli fiyat kırıp beni zarara sokmaya çalışırdı ama öldürecek değildim ya!",
                (101, false) => "Ekrem Bey orman kütüklerini toptan alıp şehre satardı. Ticari bir ilişkimiz vardı, dürüst bir tüccardı.",

                (102, true) => "*Önlüğünün kenarını büker* Ekrem Bey dükkânımın mülk sahibiydi... Kirayı geciktirince biraz sert konuşurdu ama aramızı düzeltmiştik amirim!",
                (102, false) => "Ekrem Bey her sabah manava uğrar, taze meyvelerini alırdı. Kasabanın can damarıydı, çok üzgünüz.",

                (103, true) => "*Körüğü hızlıca çeker* Ekrem Bey benden özel çelik kasalar ve dayanıklı kilitler sipariş ederdi. Sırlarını kasalarda saklamayı severdi...",
                (103, false) => "Ekrem Bey işyerinin ve evinin kilit sistemlerini bana yaptırırdı. Sağlam demir işçiliğine önem verirdi.",

                (104, true) => "*Veresiye defterini arkasına gizler* Ekrem Bey bakkaldan alışveriş yapmazdı ama bana faizle borç para vermişti... O parayı ödeyecektim amirim!",
                (104, false) => "Ekrem Bey haftalık gazetesini ve tütününü benden alırdı. Kasaba esnafına sahip çıkan biriydi.",

                (105, true) => "*Tıp kodeksini kapatır, gerginleşir* Ekrem Bey kronik rahatsızlıkları için muayenehaneme gelirdi. Benden ağrı kesici ve uyku tentürleri talep ederdi...",
                (105, false) => "Ekrem Bey'in aile hekimiydim. Mide ve karaciğer rahatsızlıkları için hazırladığım bitkisel kürleri kullanıyordu.",

                (106, true) => "*Mührü masaya sertçe bırakır* Ekrem Bey Gölge Şehir meclisinde söz sahibi bir tüccardı. İmar projelerinde fikir ayrılıklarımız olurdu ama saygılıydı!",
                (106, false) => "Ekrem Bey kasabamızın kalkınma projelerine maddi destek sağlayan saygın bir hayırseverdi.",

                (107, true) => "*Köstekli saatini yeleğine saklar* Ekrem Bey eski talebelerimdendi... Ama büyüyünce çok acımasız ve paragöz bir adama dönüştü evladım.",
                (107, false) => "Ekrem Bey benim yıllar önce ilkokulda okuttuğum kıymetli bir talebemdi. Aramızdaki saygı ve sevgi hiç bitmemişti.",

                (108, true) => "*Deri bıçağını tezgaha saplar* Ekrem Bey bana toptan manda derisi temin ederdi. Son sevkiyatta eksik mal gönderince tartıştık ama cinayetle ilgim yok!",
                (108, false) => "Ekrem Bey özel av çizmelerini sadece bana diktirirdi. Ayak ölçüsünü ve zevkini ezbere bilirdim.",

                _ => $"{victimName}'i kasabadaki herkes gibi ben de yakından tanırdım amirim."
            };

            return new AIInteractionResponse
            {
                Dialogue = victimRelationResponse,
                Emotion = isGuilty ? "Tedirgin" : "Düşünceli",
                TrustChange = isGuilty ? -1 : 1,
                StressIncrease = isGuilty ? 10 : 0
            };
        }

        // 4. ŞÜPHELİ FİKİRLERİ VE GÖRÜŞLER
        if (npc.NPCId >= 101 && (mentionsTahsin || mentionsAyse || mentionsKazim || mentionsNaciye || mentionsSevgi || mentionsCevdet || mentionsFehmi || mentionsRasim || mentionsEkrem))
        {
            responseText = GetGolgeNpcOpinion(npc.NPCId, mentionsTahsin, mentionsAyse, mentionsKazim, mentionsNaciye, mentionsSevgi, mentionsCevdet, mentionsFehmi, mentionsRasim, mentionsEkrem, guiltyNpcId);
            emotion = "Düşünceli";
        }
        else if (mentionsHasan || mentionsSelma || mentionsKemal || mentionsGunes || mentionsYahya || mentionsOsman)
        {
            responseText = GetOtherNpcOpinion(npc.NPCId, mentionsHasan, mentionsSelma, mentionsKemal, mentionsGunes, mentionsYahya, guiltyNpcId);
            emotion = "Düşünceli";
        }
        else if (isOpinionQuery)
        {
            responseText = (npc.NPCId >= 101) 
                ? GetRandomGolgeSuspectOpinion(npc.NPCId, guiltyNpcId)
                : GetOtherNpcOpinion(npc.NPCId, false, false, false, false, false, guiltyNpcId);
            emotion = "Düşünceli";
        }
        else if (isAlibiQuery)
        {
            responseText = isGuilty ? GetGuiltyAlibiResponse(npc.NPCId) : GetInnocentAlibiResponse(npc.NPCId);
            emotion = isGuilty ? "Tedirgin" : "Sakin";
        }
        else if (isWeaponQuery)
        {
            responseText = isGuilty ? GetGuiltyWeaponResponse(npc.NPCId, rawTrLower) : GetInnocentWeaponResponse(npc.NPCId, rawTrLower);
            emotion = isGuilty ? "Gergin" : "Düşünceli";
        }
        else if (isMotiveQuery)
        {
            responseText = isGuilty ? GetGuiltyMotiveResponse(npc.NPCId) : GetInnocentMotiveResponse(npc.NPCId);
            emotion = isGuilty ? "Savunmacı" : "Sakin";
        }
        else
        {
            // 5. VERİTABANI KONTROLÜ VEYA DERİN ALAKASIZ/GENEL KONU MOTORU
            var dbPool = new List<NPCDialogue>();
            if (_repository != null)
            {
                try
                {
                    dbPool = (await _repository.GetLocalAIPoolAsync(npc.NPCId)).ToList();
                }
                catch { dbPool = new List<NPCDialogue>(); }
            }
            bool matchedDb = false;

            if (dbPool.Any())
            {
                var usedResponses = history.Select(h => h.NPCResponse).ToHashSet();
                NPCDialogue? bestMatch = null;
                double highestScore = -1;

                foreach (var dialogue in dbPool)
                {
                    bool isUsed = usedResponses.Contains(dialogue.NPCResponse);
                    double score = CalculateSemanticScore(normalizedAscii, dialogue.PlayerText, dialogue.Category, currentIntent, npc.NPCId);

                    if (isUsed) score -= 50;

                    if (score > highestScore)
                    {
                        highestScore = score;
                        bestMatch = dialogue;
                    }
                }

                // Sadece yüksek semantik örtüşme varsa DB yanıtı kullan
                if (highestScore >= 35 && bestMatch != null)
                {
                    matchedDb = true;
                    if (isGuilty && !string.IsNullOrEmpty(bestMatch.GuiltyResponses))
                    {
                        try
                        {
                            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(bestMatch.GuiltyResponses);
                            if (dict != null && dict.TryGetValue(npc.NPCId.ToString(), out var gResp))
                            {
                                responseText = gResp;
                                emotion = "Tedirgin";
                            }
                        }
                        catch { responseText = bestMatch.NPCResponse; }
                    }

                    if (string.IsNullOrEmpty(responseText))
                    {
                        responseText = bestMatch.NPCResponse;
                        emotion = "Sakin";
                    }
                }
            }

            // DB'de yoksa veya alakasız/genel bir konuysa: 30+ Kategoriye Sahip Dinamik Konu Motoru Devreye Girer
            if (!matchedDb || string.IsNullOrEmpty(responseText))
            {
                responseText = GenerateSmartOffTopicOrPersonaResponse(rawTrLower, normalizedAscii, npc, isGuilty);
                emotion = isGuilty ? "Düşünceli" : "Sakin";
            }
        }

        // Dinamik Stres ve Psikolojik Tepki Hesaplama
        if (isGuilty)
        {
            if (isDirectAccusation) stressIncrease = Math.Max(stressIncrease, 32);
            else if (isLieAccusation) stressIncrease = Math.Max(stressIncrease, 35);
            else if (isWeaponQuery) stressIncrease = Math.Max(stressIncrease, 24);
            else if (isMotiveQuery) stressIncrease = Math.Max(stressIncrease, 20);
            else if (isAlibiQuery) stressIncrease = Math.Max(stressIncrease, 16);
            else stressIncrease = Math.Max(stressIncrease, 6);

            if (rawTrLower.Contains("yalan") || rawTrLower.Contains("saklıyorsun") || rawTrLower.Contains("itiraf") || rawTrLower.Contains("katil"))
            {
                stressIncrease += 12;
                emotion = "Panik";
            }
        }
        else
        {
            // Masum NPC
            if (isDirectAccusation) { stressIncrease = Math.Max(stressIncrease, 15); emotion = "Öfkeli"; }
            else if (isLieAccusation) { stressIncrease = Math.Max(stressIncrease, 10); emotion = "Ciddi"; }
            else if (isMotiveQuery || isWeaponQuery) { stressIncrease = Math.Max(stressIncrease, 6); }
            else { stressIncrease = Math.Max(stressIncrease, 2); }
        }

        if (isGuilty && npcCluesInBagCount > 0 && _random.Next(100) < 40 && revealedSecret == null)
        {
            revealedSecret = $"Amirims, Çetin olarak söylüyorum: {npc.Name} konuşurken gözlerini kaçırıyor ve aşırı terliyor. Şu konuyla ilgisi var: '{npc.SecretInfo}'";
        }

        return new AIInteractionResponse
        {
            Dialogue = responseText,
            Emotion = emotion,
            TrustChange = trustChange,
            RevealedSecret = revealedSecret,
            StressIncrease = stressIncrease
        };
    }

    private static string GenerateDynamicGreeting(NPC npc, string rawTrLower, int historyCount)
    {
        int variant = (historyCount + _random.Next(5)) % 5;
        bool isHowAreYou = TurkishTextEngine.ContainsAnyConcept(rawTrLower, TurkishTextEngine.NormalizeToAscii(rawTrLower), "nasilsin", "naber", "nbr", "ne haber", "nasilsiniz", "napiyorsun");

        if (isHowAreYou)
        {
            return npc.NPCId switch
            {
                1 => "Hamdolsun amirim, et doğrayıp müşterilere yetiştirmeye çalışıyoruz. Bu cinayet olayı kasabayı gerdi tabii, keyfimiz kaçık.",
                2 => "Nasıl olalım amirim... Reçeteleri hazırlıyor, kasabanın tansiyonunu yatıştırmaya çalışıyorum. Bu cinayet hepimizin huzurunu kaçırdı.",
                3 => "Görev başındayız dedektif. Kasaba meclisi, belediye evrakları derken başımızı kaşıyacak vakit yok. Siz nasılsınız?",
                4 => "Görevin başındayız meslektaşım. Asayiş berkemal görünse de bu cinayet varken rahat nefes alamıyoruz. Soruşturma nasıl gidiyor?",
                5 => "İhtiyarlık işte amirim... Gözler zor seçiyor ama iğne ipliği bırakmıyoruz. Kasabada böyle bir felaket varken nasıl iyi olalım?",
                101 => "Ormanın ayazı çarptı biraz ama iyiyiz çok şükür. Odunları yarıp ocağı yakıyoruz amirim.",
                102 => "Meyvelerin tozunu alıyorum dedektif bey. Ekrem Bey cinayeti yüzünden kasabanın tadı tuzu kalmadı ama ayaktayız.",
                103 => "Körüğün başındayız dedektif. Demir dövmekten kollar yoruldu ama halimize şükür. Buyurun.",
                104 => "Aaa nasıl olayım amirim, bakkal dükkânında müşteri bekliyorum. Herkes tedirgin, kimse sokağa çıkmıyor.",
                105 => "Hekimlik koşturmacası amirim... Hastalar, bitkisel tentürler... Kasabadaki bu gerginlik herkesin sağlığını bozdu.",
                106 => "Gölge Şehir'in asayişi ve huzuru için evrakların başındayım dedektif. Teşekkür ederim.",
                107 => "Gaz lambasının ışığında kitaplarımla baş başayım evladım. İyiyim, hatıralarla avunuyorum.",
                108 => "Kösele dikiyorum amirim! Çamurlu sokaklar sayesinde işimiz çok ama bu cinayet canımızı sıktı.",
                _ => "İyiyim amirim, görevin başındayız. Buyurun dinliyorum."
            };
        }

        return (npc.NPCId, variant) switch
        {
            (1, 0) => "Aleykümselam amirim, kasap dükkânıma hoş geldiniz. Buyurun, cinayet soruşturmasında size nasıl yardımcı olabilirim?",
            (1, 1) => "Selamlar dedektif. Tezgâhın başındayım, buyurun sorunuzu dinliyorum.",
            (1, 2) => "Hoş geldiniz amirim. Etleri doğrarken kulaklarım sizde, ne sormak istiyorsanız çekinmeden sorun.",
            (1, 3) => "Kolay gelsin amirim, buyurun kasap dükkânım emrinizde.",
            (1, _) => "Selamınızı aldım amirim. Cinayetle ilgili sorunuza geçebiliriz.",

            (2, 0) => "Merhaba amirim, şifa dükkânıma hoş geldiniz. İnşallah bu acı olayı kısa sürede aydınlatırsınız. Dinliyorum amirim.",
            (2, 1) => "Merhaba dedektif bey. Eczanede her şey emrinizde, nasıl yardımcı olabilirim?",
            (2, 2) => "Size de hayırlı günler amirim. İlaç şişelerini düzenliyorum, buyurun sizi dinliyorum.",
            (2, 3) => "Selamlar dedektif. Reçetelere bakıyordum, buyurun sizi dinliyorum.",
            (2, _) => "Hoş geldiniz amirim. Şifalı bitkilerimiz gibi soruşturmanız da ferahlık getirsin kasabaya.",

            (3, 0) => "Selamlar amirim, muhtarlık makamımıza safalar getirdiniz. Kasabamızın huzuru için ne gerekiyorsa sormaktan çekinmeyin.",
            (3, 1) => "Aleykümselam dedektif. Kasabanın muhtarı olarak her türlü sorunuza açığım.",
            (3, 2) => "Yine merhaba amirim. Kasaba meydanındaki sükuneti sağlamaya çalışıyoruz, buyurun.",
            (3, 3) => "Hayırlı günler amirim. Resmi makamımızda sizi dinliyorum.",
            (3, _) => "Hoş geldiniz dedektif bey. Kasabanın selameti için buyurun sorun.",

            (4, 0) => "Merhaba amirim, kolay gelsin. Karakolumuz ve tüm imkânlarımız emrinizdedir, buyurun.",
            (4, 1) => "Selamlar meslektaşım. Karakol arşivimiz ve tüm tutanaklar hazır, ne öğrenmek istiyorsunuz?",
            (4, 2) => "Merhaba amirim. Nöbetçi polislerimiz teyakkuzda, buyurun dinliyorum.",
            (4, 3) => "Hayırlı görevler meslektaşım. Soruşturmanın detaylarını konuşalım.",
            (4, _) => "Selamlar amirim. Tutanakları masaya koydum, dinliyorum.",

            (5, 0) => "Hoş geldiniz amirim, sefalar getirdiniz. Şöyle oturun, bir sıcak çayımı için... Sorularınızı dinliyorum.",
            (5, 1) => "Aleykümselam amirim. Terzi dükkanımda çayım her zaman sıcaktır, buyurun.",
            (5, 2) => "Merhaba evladım. İğne ipliği bıraktım, sizi dinliyorum.",
            (5, 3) => "Selamlar dedektif bey. Kumaşları kenara çektim, buyurun.",
            (5, _) => "Hayırlı işler evladım, yaşlı terzinin kapısı her zaman açıktır.",

            // Gölge Şehir NPC'leri (101 - 108)
            (101, 0) => "Ormandan gelen taze çam kokusu gibisi yoktur amirim... Ama bu gece orman bir garip sesler çıkarıyordu. Buyurun, ne sormak istiyorsanız sorun.",
            (101, 1) => "Selam dedektif! Odunları yarmayı bitirdim, Ekrem Bey cinayeti hakkında ne bilmek istiyorsunuz?",
            (101, 2) => "Aleykümselam amirim. Baltam tezgâhta durur, orman hakkında istediğinizi sorun.",
            (101, 3) => "Hayırlı günler dedektif. Ormanın sesini dinliyordum, buyurun.",
            (101, _) => "Selamlar amirim. Oduncu Tahsin dinliyor sizi.",

            (102, 0) => "Hoş geldiniz amirim, taze meyvelerim gibi temiz bir kasabayız aslında! Ekrem Bey vakası hepimizi sarstı, buyurun ne öğrenmek istersiniz?",
            (102, 1) => "Merhaba dedektif bey! Tezgâhın başındayım, kasabadaki dedikoduları mı yoksa cinayet gecesini mi soracaksınız?",
            (102, 2) => "Selamlar amirim. Buyurun çekinmeyin, Manav Ayşe'ye her şeyi sorabilirsiniz.",
            (102, 3) => "Hayırlı günler dedektifim! Elma kasalarını diziyordum, buyurun dinliyorum.",
            (102, _) => "Aleykümselam amirim. Manav tezgâhı emrinizde.",

            (103, 0) => "Kızgın demir döverken laf dinlemek zordur amirim... Sorunuzu çabuk sorun, ocağın ateşi sönmesin.",
            (103, 1) => "Selam. Demir tavında dövülür dedektif, sorunuz neyse söyleyin çabucak.",
            (103, 2) => "Aleykümselam. Çekiç sesinden rahatsız olmazsanız buyurun dinliyorum.",
            (103, 3) => "Hayırlı işler dedektif bey. Örsümün başında sizi dinliyorum.",
            (103, _) => "Selamlar. Demirci Kâzım'a ne sormak istiyorsanız açıkça sorun.",

            (104, 0) => "Aaa amirim hoş geldiniz sefalar getirdiniz! Gölge Şehir'in tüm havadisleri bakkaldan geçer, Ekrem Bey meselesini de dinleyin benden!",
            (104, 1) => "Merhaba dedektifim! Bakkal defterini kapattım, buyurun ne sormak istiyorsanız sorun.",
            (104, 2) => "Selamlar amirim! Bakkal dükkanım emrinize amadedir, buyurun.",
            (104, 3) => "Hayırlı günler amirim! Bir paket tütün ya da bir soru... Hangisini isterseniz buyurun!",
            (104, _) => "Aleykümselam dedektif bey. Bakkal Naciye kulak kesildi.",

            (105, 0) => "Hekimlik yeminim sır saklamayı gerektirir amirim... Ama bu cinayet kasabanın dengesini bozdu. Tıbbi veya şahsi ne sormak istersiniz?",
            (105, 1) => "Merhaba dedektif. Şifalı bitkilerimi ayıklıyordum, Ekrem Bey'in zehirlenme şüphesi mi var?",
            (105, 2) => "Selam amirim. Tıbbi açıdan merak ettiğiniz bir konu varsa yanıtlayabilirim.",
            (105, 3) => "Hayırlı günler dedektif. Reçete defterini kapattım, buyurun.",
            (105, _) => "Aleykümselam amirim. Muayenehanemde sorularınızı bekliyorum.",

            (106, 0) => "Gölge Şehir sakin bir yerdir dedektif bey. Bu talihsiz olayı çabuk çözüp kasabanın adını lekelemeden kapatmalıyız. Sorun bakalım.",
            (106, 1) => "Selamlar amirim. Muhtarlık evraklarını inceliyordum, kasabayla ilgili ne öğrenmek istersiniz?",
            (106, 2) => "Merhaba dedektif. Kasaba halkının güvenliği için buradayım, buyurun.",
            (106, 3) => "Hayırlı mesailer dedektif bey. Muhtar Cevdet olarak sorularınıza açığım.",
            (106, _) => "Aleykümselam amirim. Gölge Şehir muhtarlığına hoş geldiniz.",

            (107, 0) => "Penceremin önünde oturup gaz lambasında kitap okurdum evladım... O gece ayak sesleri tam penceremin altından geçti. Dinliyorum sizi.",
            (107, 1) => "Merhaba evladım... Emekli bir muallim olarak kasabanın geçmişini iyi bilirim. Ne sormak istersin?",
            (107, 2) => "Aleykümselam dedektif bey. Gözlüğümü taktım, buyurun sizi dinliyorum.",
            (107, 3) => "Hayırlı günler evladım. İhtiyar muallimin evine hoş geldin.",
            (107, _) => "Selamlar evladım. Gaz lambasının ışığında seni dinliyorum.",

            (108, 0) => "Ayakkabı çamurundan insanın nereye gittiğini anlarım amirim... O gece gelen çizmelerdeki çamur göl kenarındandı! Ne öğrenmek istiyorsunuz?",
            (108, 1) => "Selam dedektif. Köseleleri dikiyorum, çamurlu ayakkabı izlerini mi soracaksınız?",
            (108, 2) => "Aleykümselam amirim. Kundura atölyemde sorularınızı dinliyorum.",
            (108, 3) => "Hayırlı işler dedektif. Mumlu ipliği çekerken kulaklarım sizde.",
            (108, _) => "Selam amirim. Kunduracı Rasim usta dinliyor.",

            _ => "Merhaba amirim, hoş geldiniz. Dinliyorum sizi."
        };
    }

    /// <summary>
    /// Kullanıcının sorduğu 30+ genel, alakasız veya absürt konuyu (hava, futbol, yemek, aile, memleket, para, şaka, felsefe, din, siyaset vb.)
    /// NPC'nin mesleği, karakteri ve katil/masum psikolojisiyle doğrudan harmanlayıp cevaplayan akıllı motor.
    /// </summary>
    private static string GenerateSmartOffTopicOrPersonaResponse(string rawTrLower, string normalizedAscii, NPC npc, bool isGuilty)
    {
        bool isGolge = npc.NPCId >= 100;
        string victimName = isGolge ? "Ekrem Bey" : "Osman Bey";

        // 1. HAVA DURUMU / YAĞMUR / FIRTINA / SOĞUK / GÜNEŞ
        if (TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "hava", "yagmur", "firtina", "soguk", "sicak", "ruzgar", "sis", "kar", "gunesli", "bulut"))
        {
            return npc.NPCId switch
            {
                1 => isGuilty 
                    ? "*Satırı tezgaha vurur* Hava çok kasvetli amirim! Cinayet gecesi de öyleydi, bardaktan boşanırcasına yağıyordu... İnsanın aklını başından alacak cinsten bir hava!"
                    : "Kasabada hava fena bozdu amirim. Cinayet gecesi yağan o sağanak yağmur sokaktaki tüm izleri sildi süpürdü.",
                2 => isGuilty 
                    ? "*Pencereye tedirgin bakar* Bu rutubet ve kasvet ilaç dolaplarına bile vuruyor... Gece hava öyle karanlıktı ki insanın gözü hiçbir şey görmüyordu!"
                    : "Havadaki bu ani soğuma romatizma ve grip şikayetlerini artırdı amirim. Cinayet gecesi de hava oldukça fırtınalı ve ayazdı.",
                3 => "Muhtarlık olarak kasabanın drenaj kanallarını kontrol ettiriyorum. Cinayet gecesi yağan yağmur kasabanın sokaklarını çamur deryasına çevirmişti.",
                4 => "Hava koşulları devriye görevini zorlaştırıyor meslektaşım. O geceki yoğun sis ve yağmur görüş mesafesini sıfıra indirmişti.",
                5 => "Hava buz kesti evladım... İhtiyar kemiklerim sızlıyor. Cinayet gecesi de rüzgâr terzi dükkânımın pencerelerini zangır zangır titretiyordu.",
                101 => isGuilty
                    ? "*Baltasını sıkar* Ormanın sisi adamı delirtir amirim! O gece ağaçların arasındaki o uğultu ve fırtına hâlâ kulaklarımda çınlıyor!"
                    : "Ormanda hava her zaman kasabadan beş derece soğuktur. Cinayet gecesi de orman patikaları yoğun çamur ve fırtına altındaydı.",
                102 => "Meyveler dondan ve yağmurdan çürüyecek diye korkuyorum dedektif bey. O gece hava öyle pusluydu ki pelerinime sarılıp güç bela yürüyebildim.",
                103 => "Ocağın karşısında hava hep sıcaktır dedektif. Ama dışarıdaki fırtına demir sesini bile bastırıyordu o gece.",
                104 => "Aaa hava çok ayaz amirim! Sobanın arkasından çıkamıyoruz. O geceki fırtınada bakkalın tabelası yerinden uçacaktı neredeyse!",
                105 => "Mevsim geçişleri ve bu nemli hava enfeksiyonları tetikliyor amirim. O geceki yağmurda sokakta kalan hasta olurdu.",
                106 => "Gölge Şehir'in mikro-kliması böyledir dedektif; bir anda fırtına kopar, bir anda don olur. Cinayet gecesi de tam bir doğa felaketi gibiydi.",
                107 => "Penceremin camına vuran yağmur damlalarını dinlerdim evladım... Cinayet gecesi gök gürültüsü o ayak seslerini bile bastırıyordu.",
                108 => "Çamur diz boyu amirim! Yağan yağmur sayesinde milletin çizmesi parçalanıyor, işimiz artıyor ama kasabanın havası çok boğucu.",
                _ => $"Hava şartları oldukça sert amirim. {victimName} cinayetinin işlendiği gece de sokaklar fırtınalıydı."
            };
        }

        // 2. YEMEK / İÇMEK / AÇLIK / ÇAY / KAHVE / ET / EKMEK
        if (TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "yemek", "aciktin", "aciktim", "cay", "kahve", "corba", "ekmek", "kahvalti", "aksam yemegi", "lokanta", "kebap", "et"))
        {
            return npc.NPCId switch
            {
                1 => "*Bıçağını biler* Ben kasabım amirim, dükkânda taze dana ve kuzu eti bulunur. Bir pirzola pişireyim desem aklımız cinayette, boğazımızdan lokma geçmiyor!",
                2 => "Ben şifalı adaçayı ve ıhlamur kaynatırım amirim. Midem kazındı ama bu gerginlikte insanın canı ne çay ne yemek istiyor.",
                3 => "Muhtarlıkta her zaman taze demli Rize çayımız bulunur dedektif. Buyurun bir bardak doldurayım, bu cinayet meselesi herkesin iştahını kapattı.",
                4 => "Karakol nöbetinde demli çay ve simit bizim vazgeçilmezimizdir meslektaşım. Ama Osman Bey dosyasını çözmeden boğazımızdan bir şey geçmez.",
                5 => "Sobanın üstünde güğümüm her daim kaynar evladım. Bir bardak sıcak çayımı iç, yanında da kuru çörek ikram edeyim.",
                101 => "Kulübede közde demlenen çay ve kuru ekmek-peynir bana yeter amirim. Orman adamı lüks yemek bilmez.",
                102 => "Tezgâhtan taze bir Amasya elması veya sulu şeftali ikram edeyim amirim! Vitamin olur, zihninizi açar.",
                103 => "Kızgın demirin yanında bir demlik çay içeriz dedektif. Yemek saatini unuttuk bu cinayet yüzünden.",
                104 => "Bakkal Naciye'de taze kaşar, sıcak çay, helva ne ararsan var amirim! Buyurun atıştırın, dedektiflik aç karnına yapılmaz.",
                105 => "Şifalı nane, melisa ve papatya çayı öneririm dedektif. Sindirimi rahatlatır, sinirleri yatıştırır.",
                106 => "Muhtarlıkta ikramımız eksik olmaz. Gölge Şehir'in meşhur cevizli pidesinden göndereyim size.",
                107 => "Ihlamur kaynatırım evladım... İhtiyarlıkta hafif çorba ve ıhlamurdan başka bir şey gitmiyor boğazımdan.",
                108 => "Bir bardak tavşan kanı çay içeriz amirim. Kösele dikerken çaysız durulmaz.",
                _ => "Boğazımızdan lokma zor geçiyor amirim, bu cinayet hepimizin huzurunu ve iştahını kaçırdı."
            };
        }

        // 3. FUTBOL / MAÇ / SPOR / OYUN / EĞLENCE
        if (TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "futbol", "mac", "spor", "takim", "derbi", "gol", "sampiyon", "fenerbahce", "galatasaray", "besiktas", "trabzon", "oyun", "kumar", "eglence", "kahvehane"))
        {
            return isGuilty
                ? $"*Sinirle yüzünü buruşturur* Amirim memlekette cinayet işlenmiş, siz maçtan, oyundan bahsediyorsunuz! Benim kafam yerinde değil, futbol düşünecek halim mi var?!"
                : $"Kasabanın kahvesinde millet toplanıp maç izler, kağıt oynardı amirim... Ama bu {victimName} cinayetinden sonra kasabada ne futbol hevesi kaldı ne de eğlence.";
        }

        // 4. AİLE / ÇOCUKLAR / EVLİLİK / AKRABA / EŞ / ANNE / BABA
        if (TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "aile", "cocuk", "evli", "bekar", "kadin", "koca", "anne", "baba", "kardes", "ogul", "kiz", "akraba", "torun"))
        {
            return npc.NPCId switch
            {
                1 => "Benim çoluk çocuğumun rızkı bu kasap tezgâhından çıkar amirim. Ailemin başını öne eğdirecek hiçbir kanlı işe bulaşmam ben!",
                2 => "Yalnız yaşayan bir kadınım amirim... Eczanem benim yuvam, ilaçlarım da evlatlarım gibi oldu. Kimsem yok bu kasabada.",
                3 => "Geniş bir ailemiz var, köklerimiz üç nesildir bu kasabada. Ailemin adını lekeleyecek bir şey yapmam.",
                4 => "Polis ailesi olmak zordur meslektaşım. Eşim ve çocuklarım her gece sağ salim eve dönmem için dua eder.",
                5 => "Hanımı yıllar önce toprağa verdim evladım... Bir başıma bu dikiş makinesinin başında ömrümü tüketiyorum.",
                101 => "Orman kulübesinde tek başımayım amirim. Kurtlar, kuşlar ve ağaçlar dışında bir ailem yok.",
                102 => "İki yetimimi bu manav tezgâhından doyuruyorum dedektif bey! Onların geleceği için gece gündüz çalışırım.",
                103 => "Demirci ocağı babamdan, dedemden mirastır bana. Aile şerefimiz demirden daha serttir.",
                104 => "Bakkal dükkânı bana rahmetli kocamdan kaldı amirim. Tek başıma dimdik ayakta durmaya çalışıyorum.",
                105 => "Hekimlik mesleğim yüzünden aile kurmaya vakit bulamadım dedektif. Kasaba halkının sağlığı benim ailem oldu.",
                106 => "Gölge Şehir'in saygın ailelerindeniz. Muhtarlık mührü dedemin zamanından beri bu masadadır.",
                107 => "Tüm talebelerim benim birer evladım sayılır evladım... Kendi çocuğum olmadı ama yüzlerce çocuk yetiştirdim.",
                108 => "Yalnız bir kunduracıyım amirim. Huysuzluğum da bu kimsesizliktendir belki.",
                _ => "Ailemizin şerefi ve namusu her şeyden önce gelir amirim."
            };
        }

        // 5. PARA / ZENGİNLİK / FAKİRLİK / EKONOMİ / DOLAR / ALTIN / TİCARET
        if (TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "para", "zengin", "fakir", "ekonomi", "enflasyon", "dolar", "altin", "kazanc", "ticaret", "isler", "piyasa", "paha", "fiyat"))
        {
            return isGuilty
                ? $"*Huzursuzca yutkunur* Para bu dünyadaki tüm kötülüklerin anasıdır amirim! İnsanı borca sokarlar, tehdit ederler, sonra da suçlu çıkarırlar!"
                : $"Bu devirde esnaflık zor amirim, para kazanmak aslanın ağzında. Ama {victimName} parasıyla herkesi ezmeye çalışan biriydi, parası da canını kurtarmaya yetmedi.";
        }

        // 6. DİN / GÜNAH / SEVAP / CENNET / CEHENNEM / ALLAH / VİCDAN / DUA
        if (TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "din", "gunah", "sevap", "cennet", "cehennem", "allah", "tanri", "dua", "namaz", "ahiret", "vicdan", "helal", "haram", "tovbe"))
        {
            return isGuilty
                ? "*Gözlerini kaçırarak elleri titrer* Vicdan mı dediniz amirim?! Allah her şeyi görür, herkesin günahı da sevabı da kendine! Bana vaaz vermeyin!"
                : "Bir cana kıymak en büyük günahtır amirim. Kul hakkı yiyen, masum bir can alan ahirette de bu dünyada da cezasını çeker. Bizim vicdanımız çok şükür tertemiz.";
        }

        // 7. MEMLEKET / NERELİSİN / DOĞMA BÜYÜME / KASABA / ŞEHİR / İSTANBUL / ANKARA
        if (TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "memleket", "nerelisin", "dogma", "nereden geldin", "koy", "sehir", "istanbul", "ankara", "izmir", "buralimisin", "yerli"))
        {
            return (npc.NPCId >= 100)
                ? $"Ben doğma büyüme Gölge Şehirliyim dedektif. Bu kasabanın taşını toprağını, kimin kiminle ne hesabı olduğunu adım gibi bilirim."
                : $"Ben bu kasabanın yerlisiyim amirim. Dedelerimizden beri bu topraklardayız, yabancı insanı hemen gözünden tanırız.";
        }

        // 8. YAŞ / KAÇ YAŞINDASIN / EMEKLİLİK / GENÇLİK / İHTİYARLIK
        if (TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "kac yasindasin", "yasin kac", "yaslisin", "gencsin", "emekli", "ihtiyar", "delikanli", "yas"))
        {
            return npc.NPCId switch
            {
                1 => "45 yaşındayım amirim, 25 yıldır da bu satırı sallıyorum.",
                2 => "38 yaşındayım dedektif. Eczacılık fakültesinden mezun olduğumdan beri bu kasabadayım.",
                3 => "52 yaşındayım. Kasabanın yarım asırlık tarihine şahidim.",
                4 => "40 yaşındayım amirim, 18 yılım emniyet teşkilatında geçti.",
                5 => "72 yaşıma bastım evladım... Dikiş dikmekten gözlerimin feri söndü.",
                101 => "48 yaşındayım. Ormanda geçen ömür insanı çabuk yıpratır ama çam gibi dimdik tutar.",
                102 => "36 yaşındayım dedektif bey. Genç yaşta hayatın yükü bindi omuzlarımıza.",
                103 => "55 yaşındayım. Örsün sıcağı saçlarımızı ağarttı.",
                104 => "50 yaşındayım amirim. Bakkal tezgahında bir ömür tükettik.",
                105 => "42 yaşındayım. Tıp tahsilimi tamamlayıp bu kasabaya şifa dağıtmaya geldim.",
                106 => "58 yaşındayım dedektif. Muhtarlık koltuğunda saç ağarttık.",
                107 => "68 yaşındayım evladım. 40 yılım muallimlikle, talebe okutmakla geçti.",
                108 => "60 yaşındayım amirim. Kösele dikmekten parmaklarım nasır tuttu.",
                _ => "Yaşımızı başımızı aldık amirim, önemli olan kalan ömrü onurla yaşamak."
            };
        }

        // 9. ŞAKA / ESPRİ / KOMİK / GÜLMEK / FIKRA
        if (TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "saka", "espri", "komik", "gul", "fikra", "eglen", "dalga"))
        {
            return isGuilty
                ? "*Öfkeyle bakar* Amirim siz benimle dalga mı geçiyorsunuz?! Ortada bir cinayet soruşturması var, bana fıkra mı anlatacaksınız?!"
                : "Şaka kaldıracak günlerde değiliz amirim... Kasabada bir cinayet işlenmiş, herkesin içi kan ağlıyor. Neşemiz kalmadı.";
        }

        // 10. SAĞLIK / HASTALIK / DOKTOR / HASTANE / İLAÇ / BAŞ AĞRISI
        if (TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "saglik", "hasta", "hastalik", "basim agriyor", "agri", "sanci", "ilac", "hastane", "doktor", "grip", "tedavi"))
        {
            return (npc.NPCId == 2 || npc.NPCId == 105)
                ? "Tıbbi bir şikayetiniz varsa tansiyonunuza bakalım veya şifalı bir bitki tentürü hazırlayayım amirim. Ama soruşturma stresi baş ağrısı yapabilir, dikkat edin."
                : "Sağlık her şeyin başı amirim. Kasabadaki bu cinayet gerginliği herkesin tansiyonunu fırlattı, sağlık ocakları dolup taşıyor.";
        }

        // 11. HAYVANLAR / KÖPEK / KEDİ / KURT / AT / KUŞ
        if (TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "kopek", "kedi", "kurt", "hayvan", "kus", "at", "inek", "koyun", "av"))
        {
            return (npc.NPCId == 101)
                ? "Ormandaki kurtlar ve yaban domuzları insanlardan daha dürüsttür amirim. O gece kurtlar bile bir terslik olduğunu anlayıp uluyordu."
                : "Cinayet gecesi sokak köpekleri sabaha kadar durmaksızın havladı amirim, sokakta yabancı bir hareketlilik hissettikleri belliydi.";
        }

        // 12. KİTAP / OKUMAK / ŞİİR / YAZI / EDEBİYAT / GAZETE
        if (TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "kitap", "siir", "oku", "yazi", "edebiyat", "gazete", "roman", "sair"))
        {
            return (npc.NPCId == 107)
                ? "Kitaplar insanın en sadık dostudur evladım. Fuzuli Divanı ve eski kasaba hatıratlarını okurum geceleri... O gece de gaz lambasında sayfaları çeviriyordum."
                : "Gazetelerde her gün cinayet haberleri okurduk amirim ama başımıza geleceğini hiç düşünmezdik. Kasaba halkı bu acı vaka ile sarsıldı.";
        }

        // 13. SİYASET / SEÇİM / DEVLET / BELEDİYE / HÜKÜMET / KANUN
        if (TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "siyaset", "secim", "devlet", "belediye", "hukumet", "kanun", "baskan", "parti", "adalet"))
        {
            return (npc.NPCId == 3 || npc.NPCId == 106)
                ? "Devletin kanunu ve adaleti her şeyin üstündedir dedektif. Makamımızda kanunlara ve kasabanın huzuruna hizmet ediyoruz."
                : "Biz siyasetten anlamayız amirim, kendi ekmeğimizin peşindeyiz. Yeter ki devletimiz katili bulup adaleti sağlasın.";
        }

        // 14. DEDEKTİFİN KENDİSİ / SEN KİMSİN / BEN KİMİM / BİZ KİMİZ / ÇETİN / RIFAT
        if (TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "sen kimsin", "ben kimim", "cetin kim", "cetin", "rifat kim", "rifat", "dedektif", "polis misin", "gorevin ne", "amirim"))
        {
            return (npc.NPCId >= 100)
                ? $"Siz bu cinayeti aydınlatmak için merkezin gönderdiği başkomisersiniz, yanınızdaki Çetin dedektifiniz, sokakta da Bekçi Rıfat feneriyle size rehberlik ediyor. Ben de {npc.Name} olarak dürüstçe ifademi veriyorum."
                : $"Siz bu cinayeti çözmek için görevlendirilen başkomisersiniz amirim, yanınızdaki Çetin de yardımcınız. Bizler de ifademizi dürüstçe veren kasaba halkıyız.";
        }

        // 15. ARABA / TRAFİK / TREN / AT ARABASI / YOLCULUK / KAÇIŞ
        if (TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "araba", "otomobil", "tren", "kamyonet", "traktor", "at arabasi", "yolculuk", "kacis", "otobus", "motor"))
        {
            return isGuilty
                ? "*Yutkunarak kapıya bakar* Ne arabası amirim?! O gece kasabada motor sesi duymadım! Kimse bir yere kaçmıyor, buradayım işte!"
                : $"O gece fırtınada yollar çamurdan kapalıydı amirim. Kasabadan araçla kaçmak imkansızdı, ancak orman patikalarından yürüyerek gidilebilirdi.";
        }

        // 16. MÜZİK / ŞARKI / SANAT / RADYO / PLAK / ÇALGI
        if (TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "muzik", "sarki", "turku", "radyo", "plak", "sanat", "baglama", "keman", "nota", "cal"))
        {
            return isGuilty
                ? "*Kulaklarını tıkar gibi yapar* Amirim cinayet masasında müzikten, türküden bahsedecek havada mıyız Allah aşkına?!"
                : $"Akşamları radyoda Türk Sanat Müziği ve eski türküleri dinlerdik amirim... Ama bu cinayetten sonra radyoyu açmaya bile elimiz varmıyor.";
        }

        // 17. UYKU / RÜYA / KABUS / GECE / UYKUSUZLUK
        if (TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "uyku", "ruya", "kabus", "uyudun", "uyuyamadin", "uykusuz", "yatak", "gece yarisi"))
        {
            return isGuilty
                ? "*Gözlerinin altındaki morlukları ovar* Gözüme bir damla uyku girmiyor amirim! Her gözümü kapattığımda o geceki bağırışlar kafamda zonkluyor!"
                : $"Bu acı olaydan sonra tüm kasabanın uykusu kaçtı amirim. Herkes kapısını iki kez kilitliyor, kimse huzurla uyuyamıyor.";
        }

        // 18. KORKU / ENDİŞE / TEHLİKE / TEKİN / CESARET
        if (TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "korkuyor musun", "korktun mu", "korku", "endise", "tehlike", "tekinsiz", "cesur", "panik"))
        {
            return isGuilty
                ? "*Titreyen sesini gizlemeye çalışır* Korkmak mı?! Benim kimseden korkum yok amirim! Masum adam neden korksun?!"
                : $"Kasabada bir katil serbestçe gezerken kim korkmaz ki amirim? Herkes birbirine şüpheyle bakıyor, hava çok tekinsiz.";
        }

        // 19. AŞK / SEVGİ / KISKANÇLIK / İHANET / GÖNÜL İLİŞKİSİ
        if (TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "ask", "sevgi", "sevgili", "kiskanc", "ihanet", "gonul", "evlilik", "aldatma", "iliski"))
        {
            return isGuilty
                ? "*Gözlerini devirir* Kıskançlık da aşk da insanı delirtir amirim... Ama benim öyle işlerle alakam yok!"
                : $"Gönül işleri ve kıskançlık bazen insanın gözünü kör eder amirim. Ama bu cinayetin arkasında aşktan ziyade para ve şantaj hırsı yatıyor.";
        }

        // 20. GELECEK / PLANLAR / YARIN / KASABANIN DURUMU
        if (TurkishTextEngine.ContainsAnyConcept(rawTrLower, normalizedAscii, "gelecek", "planin ne", "yarin ne yapacaksin", "bundan sonra", "kurtulacak miyiz", "ne olacak"))
        {
            return isGuilty
                ? "*Huzursuzca kıpırdanır* Tek planım bu soruşturmanın bir an önce bitmesi amirim! İşimizin gücümüzün başına dönmek istiyoruz!"
                : $"Yeter ki siz adaleti sağlayın ve gerçek katili hapse atın amirim. Kasabamız ancak o zaman eski huzurlu günlerine döner.";
        }

        // 21. MESLEK VE KARAKTER KALKANI (Kalan her türlü soru için mesleki & psikolojik derin yanıt)
        return (npc.NPCId, isGuilty) switch
        {
            (1, true) => "*Satırın sapını sıkarak kaşlarını çatar* Amirim, ben kasap dükkânımda et doğruyorum! Bu konuştuklarımızla Osman cinayetinin ne ilgisi var?! Ne soracaksanız açıkça sorun!",
            (1, false) => "*Satırı tezgaha bırakır* Kasap Hasan olarak dediğinizi anlıyorum amirim. Ama kasabamızda bir cinayet varken aklımız fikrimiz bu soruşturmada, buyurun dinliyorum.",

            (2, true) => "*İlaç şişelerini telaşla düzeltir* Ben bir eczacıyım amirim... Bu bahsettiğiniz konuyla Osman Bey'in zehirlenmesi arasında nasıl bir bağ kuruyorsunuz anlamadım!",
            (2, false) => "*Gözlüğünü düzeltir* Eczanemdeyim amirim. İnsan sağlığı ve ilaçlarla ilgili merak ettiğiniz bir şey varsa veya cinayet dosyasını soracaksanız yanıtlayayım.",

            (3, true) => "*Masadaki evrakları kapatır* Ben bu kasabanın muhtarıyım! Soruşturmayı başka konulara çekmeyin, Osman vakası hakkında ne öğrenmek istiyorsanız onu sorun!",
            (3, false) => "*Resmi bir ciddiyetle bakar* Muhtarlık makamındayız dedektif. Kasabanın huzuru ve Osman Bey vakası için ne gerekiyorsa açıkça konuşabiliriz.",

            (4, true) => "*Elini beylik tabancasına götürür* Komiser Güneş olarak buradayım amirim! Konuyu dağıtmayalım, cinayet soruşturmasına odaklanalım!",
            (4, false) => "*Rozetini gösterir* Meslektaşım, karakol tutanaklarımız ve deliller meydanda. Soruşturmanın selameti için net sorularınızı bekliyorum.",

            (5, true) => "*İğneyi kumaşa sertçe batırır* Yaşlı bir terziyim amirim, dikişimin başında kafamı bulandırmayın! Osman'ın ölümüyle ilgili sorunuz varsa söyleyin!",
            (5, false) => "*Gözlüğünün üstünden bakar* Terzi Yahya olarak dükkânımdayım amirim. Cinayet gecesi kumaşlarım veya şüpheliler hakkında ne bilmek istiyorsanız çekinmeden sorun.",

            // Gölge Şehir
            (101, true) => "*Baltasını tezgaha dayar* Ormanın bekçisiyim amirim, boş lakırdıyı sevmem! Ekrem'in cinayetiyle ilgili ne soracaksanız çabuk sorun!",
            (101, false) => "*Odunları istifler* Oduncu Tahsin olarak ormandayım amirim. Bahsettiğiniz konuyu anladım ama Ekrem Bey cinayeti hakkında ne öğrenmek istiyorsanız dinliyorum.",

            (102, true) => "*Pelerinini sıkar* Manavda borç harç içindeyim amirim! Ekrem'in ölümü ve borç senetleri dışında laf dinleyecek mecalim yok!",
            (102, false) => "*Meyve kasasını siler* Manav Ayşe olarak buradayım amirim. Ekrem Bey vakası veya kasabadaki durumla ilgili ne sormak istiyorsanız buyurun.",

            (103, true) => "*Çekici örse vurur* Demir tavında dövülür dedektif! Ekrem'in çelik kasası ve cinayet hakkında konuşacaksanız konuşun, ocağımın ateşi sönmesin!",
            (103, false) => "*Körüğü çeker* Demirci Kâzım'ım ben. O bahsettiğiniz konu bir yana, cinayet gecesi etraftaki sesleri mi soracaksınız?",

            (104, true) => "*Veresiye defterini büker* Bakkal dükkânımda dedikodu yapacak vaktim yok amirim! Ekrem'in borçları hakkında ne soracaksınız sorun!",
            (104, false) => "*Tezgaha yaslanır* Bakkal Naciye her şeyi duyar dedektifim. Ekrem Bey cinayeti hakkında aklınıza ne takıldıysa açıkça sorun.",

            (105, true) => "*Zehir şişelerini saklar* Hekim olarak tıbbi formüllerimle uğraşıyorum! Ekrem'in zehirlenmesiyle ilgili ne soracaksanız sorun!",
            (105, false) => "*Tıp kodeksini kapatır* Hekim Sevgi olarak kasabanın sağlığından sorumluyum. Ekrem Bey'in otopsisi veya zehir şüphesi hakkında ne sormak istersiniz?",

            (106, true) => "*Mührü masaya vurur* Muhtar Cevdet olarak vaktim kıymetlidir dedektif! Gölge Şehir'deki bu cinayet hakkında net konuşun!",
            (106, false) => "*Resmi evrakları düzenler* Muhtar olarak soruşturmanıza tam destek veriyorum. Ekrem vakası hakkında ne öğrenmek istiyorsanız çekinmeden sorun.",

            (107, true) => "*Köstekli saatini yeleğine saklar* Yaşlı bir muallimim evladım... Ekrem'in ölümü ve o geceki ayak sesleri hakkında ne soracaksan sor!",
            (107, false) => "*Kitabını kapatır* Muallim Fehmi olarak penceremdeyim evladım. Bahsettiğin konuyu anladım ama 02:14'te duyduğum sesler ve cinayet hakkında ne bilmek istersin?",

            (108, true) => "*Deri bıçağını kaldırır* Kunduracıyım ben amirim! Çamurlu çizmeler veya Ekrem cinayeti dışında laf kalabalığı yapmayın!",
            (108, false) => "*Mumlu ipi çeker* Kunduracı Rasim olarak atölyemdeyim. Cinayet gecesi göl yolundaki ayak izleri hakkında ne bilmek istiyorsanız sorun.",

            _ => $"*Gözlerini kısarak sizi süzer* {victimName} cinayeti hakkında net bir soru sorarsanız size yardımcı olabilirim amirim."
        };
    }

    private static AIInteractionResponse? GenerateContextualFollowUpResponse(
        NPC npc, bool isGuilty, string rawTrLower, string normalizedAscii,
        bool lastWasAlibi, bool lastWasWeapon, bool lastWasMotive, bool lastWasPerson,
        string lastQuestion, string lastResponse, string prevQuestion,
        bool isWhy, bool isWho, bool isProof, bool isTime, bool isDetail, bool isWhat)
    {
        // 1. ALİBİ / MEKAN TAKİP SORULARI (Örn: "Yalnız mıydın?", "Saat kaçta çıktın?", "Şahidin var mı?")
        if (lastWasAlibi || lastResponse.Contains("dükkân") || lastResponse.Contains("evde") || lastResponse.Contains("kulübe") || lastResponse.Contains("nöbet"))
        {
            if (isWho) // "Yanında kim vardı? / Şahidin var mı?"
            {
                string text = (npc.NPCId, isGuilty) switch
                {
                    (1, true) => "*Gözlerini kaçırır* Yalnızdım amirim... Çırak erkenden çıkmıştı. Fırtına vardı zaten sokakta in cin top oynuyordu.",
                    (1, false) => "Dükkanda tektim ama fırıncı Ahmet usta kepenkleri kapatırken dükkânın ışığını gördüğünü söyleyebilir.",
                    (2, true) => "*Elleri titrer* Nöbetçi bendim ama... gece yarısı kimse gelmedi işte! Şahidim yok diye suçlu mu oldum?!",
                    (2, false) => "Nöbetçi bendim amirim, gece 23:30 sularında bekçi Rıza amca tansiyon ilacı almaya uğramıştı, kayıt defterinde de var.",
                    (3, true) => "*Sinirle masayı tıkırdatır* Muhtarlık binasında gece vakti kim olacak amirim? Yalnızdım tabii ki!",
                    (3, false) => "Makamda evrak inceliyordum, çaycı Remzi çıkarken bana çay tazeleyip çıktı. Sorabilirsiniz.",
                    (4, true) => "*Ciddileşmeye çalışır* Karakolda nöbetçi bendim... devriye turuna yalnız çıktım. Bu bir prosedür amirim!",
                    (4, false) => "Nöbetçi polis memurları karakol giriş çıkış defterine saatimi kaydettiler amirim.",
                    (5, true) => "*İğneyi kumaşa batırır* Yaşlı bir terziyim amirim, yanımda kim olsun? Yalnız başıma kumaş biçiyordum.",
                    (5, false) => "Karşı bakkal dükkânının ışığını söndürürken benim dikiş makinesinin tıkırtısını duyduğunu sabah söyledi.",
                    
                    // Gölge Şehir
                    (101, true) => "*Baltanın sapını sıkar* Orman kulübesinde tek başımaydım amirim... Kurtlardan başka şahidim mi olacak?!",
                    (101, false) => "Kulübede yalnızdım ama gece bekçisi traktörün sesini orman yolunda duymuştur.",
                    (102, true) => "*Tereddüt eder* Manavda yalnızdım... ama hava almak için çıktığımda sokak bomboştu!",
                    (102, false) => "Dükkânı kapatırken Naciye hanım komşum kapıdaydı, selamlaştık.",
                    (103, true) => "*Çekici indirir* Demirci ocağında tek başınasındır amirim... O saatte kimse ocağa uğramaz.",
                    (103, false) => "Ocağı söndürürken çırağım Mehmet dükkânı süpürüyordu, ona sorabilirsiniz.",
                    (104, true) => "*Panikle yutkunur* Bakkalın arkasındaki odamdaydım amirim... Tek başımaydım işte!",
                    (104, false) => "Gece 01:00 gibi Muallim Fehmi Bey gaz lambası yağı almaya uğramıştı, şahidimdir.",
                    (105, true) => "*Gözlüğünü düzeltir* Laboratuvarımda yalnızdım... Tıbbi formüller gizlidir, kimseyi yanıma almam!",
                    (105, false) => "Hemşire hanım çıkışta hastanın dosyasını bıraktı, o saatte oradaydım.",
                    (106, true) => "*Makam koltuğunu düzeltir* Muhtar olarak gece çalışırken yanımda şahit tutacak değilim!",
                    (106, false) => "Azalarımızdan Mustafa Bey gece 01:30'a kadar orman tahsis raporunu benimle inceledi.",
                    (107, true) => "*Hüzünle bakar* Penceremde tek başıma oturuyordum evladım... Yalnız bir ihtiyarım ben.",
                    (107, false) => "Pencerede otururken karşı kaldırımdan geçen kunduracının fenerini gördüm, o da lambamı görmüştür.",
                    (108, true) => "*Homurdanır* Atölyemde yalnızdım! Çizme dikerken laf atan sevmem ben!",
                    (108, false) => "Siparişini yetiştirdiğim çizmeleri teslim ettiğim köylü Hasan o saatte atölyedeydi.",
                    _ => isGuilty ? "O saatte yanımda kimse yoktu amirim." : "O saatte çevredekiler beni görmüş olabilir amirim."
                };

                return new AIInteractionResponse
                {
                    Dialogue = text,
                    Emotion = isGuilty ? "Tedirgin" : "Sakin",
                    TrustChange = isGuilty ? -2 : 1,
                    StressIncrease = isGuilty ? 15 : 0
                };
            }

            if (isTime) // "Saat kaçta çıktın? / Ne kadar sürdü?"
            {
                string text = (npc.NPCId, isGuilty) switch
                {
                    (1, true) => "*Terler* Gece 23:45 gibiydi sanırım... Belki biraz daha geçti. Yağmur şiddetlenince fazla oyalanmadan döndüm!",
                    (1, false) => "Gece 22:30'da kepenkleri indirdim, 23:00'te de evde yatağımdaydım amirim.",
                    (2, true) => "*Yutkunur* Gece tam 00:15 gibiydi... Sadece 15-20 dakika dışarı çıktım, hemen döndüm!",
                    (2, false) => "Eczaneyi nöbet bittiğinde, yani tam 01:00'de kilitledim ve doğruca üst kattaki evime çıktım.",
                    (3, true) => "*Kaşlarını çatar* Saat 23:30 ile 00:30 arası belediye evraklarıyla uğraştım, sonra çıktım!",
                    (3, false) => "Gece yarısı saat 00:00'da belediye binasını kilitledim, nöbetçi bekçi saatimi not etti.",
                    (4, true) => "*Sertçe yutkunur* Devriye turum 00:00 ile 01:00 arasındaydı... Olay yerine kaçta vardığımı hatırlamıyorum!",
                    (4, false) => "Karakol telsiz kayıtlarında mevcuttur; 23:45 devriye başlangıcı, 00:45 dönüş saatimdir.",
                    (5, true) => "*Gözlüğünü çıkarır* Gece saat 00:30 gibiydi... İplik bitince hava almak için çıktım.",
                    (5, false) => "Dükkânımın lambasını saat 22:00'de söndürdüm ve uyudum.",

                    // Gölge Şehir
                    (101, true) => "*Reçine bulaşık ellerini ovuşturur* Gece 02:00 sularıydı... Orman yoluna çıktım ama 02:30'da kulübedeydim!",
                    (101, false) => "Gece saat 21:00'de sobayı yakıp yattım amirim, sabaha kadar orman kulübesinden adım atmadım.",
                    (102, true) => "*Telaşla* Gece 01:45 gibi... Sadece göl kenarına kadar gidip geldim!",
                    (102, false) => "Akşam 20:00'de manavı kilitledim. Gece hiç çıkmadım.",
                    (103, true) => "*Ocağa bakar* Gece 02:10 gibiydi, fenerin gazı bitmişti... Kısa sürdü.",
                    (103, false) => "Akşam 21:30'da körüğü kapattım, gece boyunca ateşim sönecekti zaten.",
                    (104, true) => "*Elleri titrer* Saat 02:00 gibi tütün almak için arkadaki depoya geçtim.",
                    (104, false) => "Saat 22:00'de kepengi çektim, sabah 06:00'ya kadar kapı açmadım.",
                    (105, true) => "*Hızlı hızlı konuşur* Saat 02:15 civarıydı, bir hastanın nöbeti vardı... Yarım saatte döndüm!",
                    (105, false) => "Gece yarısı 00:30'da ilaç şişelerini raflara dizmeyi bitirip yattım.",
                    (106, true) => "*Masadaki saate bakar* Gece 02:00'de muhtarlıktan çıktım, kasaba meydanında 20 dakika turladım.",
                    (106, false) => "Akşam 23:00'te tüm evrakları mühürleyip evime geçtim.",
                    (107, true) => "*Saatini saklar* Penceremde 01:30'dan 02:30'a kadar oturdum... Tam 02:14'te o ayak sesini duydum.",
                    (107, false) => "02:14'te duyduğum o ayak sesini ömrümce unutmam. Köstekli saatim o an tam 02:14'ü gösteriyordu.",
                    (108, true) => "*Deriyi keser* Gece 02:00 ile 02:40 arası dışarıdaydım... Çamurlu yolda yürüdüm.",
                    (108, false) => "Akşam 21:00'den sonra dükkândan dışarı adım atan namerttir amirim.",
                    _ => isGuilty ? "O gece saatler biraz karışıktı amirim..." : "Saatlerim tamamen nettir amirim."
                };

                return new AIInteractionResponse
                {
                    Dialogue = text,
                    Emotion = isGuilty ? "Gergin" : "Ciddi",
                    TrustChange = isGuilty ? -1 : 1,
                    StressIncrease = isGuilty ? 10 : 0
                };
            }

            if (isProof) // "Bunu nasıl kanıtlayacaksın?"
            {
                string text = isGuilty
                    ? "*Köşeye sıkışmış gibi bakar* Kanıt mı?! Gece yarısı yağan yağmurda elimde şahit kağıdıyla mı dolaşacaktım amirim?! Siz bana inanmıyorsunuz!"
                    : "Amirim, çevredeki dükkân komşularına, sokaktaki lambalara veya karakol kayıtlarına bakabilirsiniz. Masum adamın saklayacak bir şeyi olmaz.";

                return new AIInteractionResponse
                {
                    Dialogue = text,
                    Emotion = isGuilty ? "Öfkeli" : "Sakin",
                    TrustChange = isGuilty ? -3 : 2,
                    StressIncrease = isGuilty ? 20 : 0
                };
            }
        }

        // 2. MOTİF / BORÇ / KAVGA TAKİP SORULARI
        if (lastWasMotive || lastResponse.Contains("borç") || lastResponse.Contains("tapu") || lastResponse.Contains("şantaj") || lastResponse.Contains("para") || lastResponse.Contains("tehdit"))
        {
            if (isWhy || isDetail) // "Neden peki? / Ne konuştunuz?"
            {
                string text = (npc.NPCId, isGuilty) switch
                {
                    (1, true) => "*Dişlerini sıkar* Bana olan 50 bin liralık et borcunu inkâr etti! 'Bir kuruş bile alamazsın' dedi yüzüme! Gözüm döndü amirim... yani bağırdım çıktım!",
                    (1, false) => "Veresiye defterindeki et parasını konuştuk. 'Haftaya hallederiz Hasan usta' dedi, tatlıya bağladık. Öldürecek bir durum yoktu.",
                    (2, true) => "*Ağlamaklı olur* Eczanemin ruhsatıyla tehdit etti beni! 'Elimdeki belgeleri savcıya veririm' dedi! Beni mahvedecekti!",
                    (2, false) => "Kalp ilaçlarının dozu hakkında konuştuk amirim. Fazla doz almaması konusunda onu uyardım.",
                    (3, true) => "*Yumruğunu sıkar* Belediye arazisini ucuza kapatıp zengin olacaktı! 'Kasabayı sana dar ederim' dedi bana!",
                    (3, false) => "Meydandaki arsa imarı hakkında konuştuk. Kanunlara uygun olarak dilekçesini aldım, hepsi bu.",
                    (4, true) => "*Terler* Eski bir rüşvet kasetini savcılığa vermekle tehdit etti! Meslek hayatımı bitirecekti!",
                    (4, false) => "Karakola gelip çevre esnafı hakkında şikayette bulunmuştu. Tutanak tuttuk ve gönderdik.",
                    (5, true) => "*Gözlerini ovalar* Diktiğim ceketteki gizli USB belleği istedi! İçinde ortaklık paralarımız vardı!",
                    (5, false) => "Yeni diktiğim kabanın provasını yaptık. Gayet memnundu, teşekkür edip gitti.",

                    // Gölge Şehir
                    (101, true) => "*Baltasını yere vurur* Kaçak kestiğim meşe ağaçlarını orman müdürlüğüne ihbar edecekti! Ekmeğimle oynuyordu!",
                    (101, false) => "Kereste fiyatları üzerine konuştuk. Biraz pazarlık ettik ama helalleştik.",
                    (102, true) => "*Gözyaşını siler* Dükkânımı elimden alıp manavı kumarhaneye çevirecekti! Nereye giderdim ben?!",
                    (102, false) => "Kasa kasa elma ve nar siparişi vermişti, avansını ödedi.",
                    (103, true) => "*Örse vurur* Yaptığım özel şifreli kasanın gizli bölmesine sahte senetler koymuştu, beni de ortak gösterecekti!",
                    (103, false) => "Çelik kasa kilidinin yedeğini istedi, teslim edip parasını aldım.",
                    (104, true) => "*Önlüğünü büker* Veresiye borcunu isteyince dükkânımı yakmakla tehdit etti! Canıma tak etmişti!",
                    (104, false) => "Eski veresiye hesabını kapattı, kahvesini içip gitti.",
                    (105, true) => "*Hekim önlüğünü sıkar* Eski bir tıbbi hatamı öğrenmiş! Şantajla benden zehirli banotu özü hazırlamamı istedi!",
                    (105, false) => "Midesi için şifalı papatya ve nane çayı hazırlamamı istemişti, verdim.",
                    (106, true) => "*Sinirlenir* Çam ormanı arazisini kendi üstüne geçirip beni rezil edecekti! Gölge Şehir'in itibarını ayaklar altına alamazdım!",
                    (106, false) => "Kasabanın su şebekesi ihalesini konuştuk, resmi toplantı tutanaklarımız var.",
                    (107, true) => "*Sesi titrer* Rahmetli babamın altın köstekli saatini borç karşılığı elimden zorla aldı! Geri vermiyordu zalim!",
                    (107, false) => "Eski bir divan edebiyatı kitabı getirdi bana, saatlerce o şiirleri okuyup çay içtik.",
                    (108, true) => "*Deri kayışını büker* Kaçak getirdiğim manda derilerini ihbar edecekti! Beni hapse attıracaktı!",
                    (108, false) => "Av çizmelerinin tabanını diktirdi, teslim edip ücretini ödedi.",
                    _ => isGuilty ? "Aramızdaki mesele çok derindi amirim..." : "Aramızda ölümcül bir husumet yoktu amirim."
                };

                return new AIInteractionResponse
                {
                    Dialogue = text,
                    Emotion = isGuilty ? "Savunmacı" : "Sakin",
                    TrustChange = isGuilty ? -2 : 1,
                    StressIncrease = isGuilty ? 20 : 0
                };
            }
        }

        // 3. EYLEM / KRONOLOJİ TAKİP SORULARI ("Peki sonra ne yaptın?", "Sonra nereye gittin?", "Ardından ne oldu?")
        if (isDetail || rawTrLower.Contains("sonra") || normalizedAscii.Contains("sonra") || rawTrLower.Contains("ardından") || normalizedAscii.Contains("ardindan"))
        {
            string text = (npc.NPCId, isGuilty) switch
            {
                (1, true) => "*Gözlerini kaçırır* Sonra mı? Dükkânın kepenklerini indirdim... Yağmur çok şiddetliydi, arka sokaktan doğruca evime geçtim diyorum!",
                (1, false) => "Sonrasında dükkânı kilitleyip evime gittim amirim. Sıcak çorbamı içip erkenden uyudum.",

                (2, true) => "*Ellerini ovuşturur* Sonra tezgahı toplayıp üst kata çıktım... Gece pencereden sokak lambasına baktım ama dışarı çıkmadım!",
                (2, false) => "Ardından eczaneyi kilitledim amirim. Üst kattaki daireme çıkıp tıp dergimi okumaya devam ettim.",

                (3, true) => "*Evrakları düzeltir* Sonrasında muhtarlık kapısını kilitledim, gece bekçisine iyi geceler dileyip eve geçtim.",
                (3, false) => "İşlerimi bitirince lambayı söndürüp çıktım. Gece boyu evimde dinlendim.",

                (4, true) => "*Telsizini kemerine takar* Devriye turunu tamamlayıp karakol nöbet odasına döndüm meslektaşım.",
                (4, false) => "Devriyeden sonra karakola döndüm, gece raporumu imzalayıp masama geçtim.",

                (5, true) => "*İpliği makaraya sarar* Sonra dikiş makinesini örtüp atölyemi kapattım. Doğruca evime gittim.",
                (5, false) => "Paltoları askıya asıp dükkânımı kilitledim. Evime çekilip dinlendim.",

                // Gölge Şehir
                (101, true) => "*Baltasını siler* Sonra kulübeme döndüm, ateşi tazeleyip yattım. Dışarıda ne olduysa ben uyurken olmuş!",
                (101, false) => "Kütükleri dizdikten sonra sobayı harlayıp yatağıma yattım.",

                (102, true) => "*Meyveleri düzeltir* Tezgâhı örtüp pelerinime sarıldım, hızlı adımlarla evime geçtim.",
                (102, false) => "Manavın brandasını çekip evime gittim, sıcacık ıhlamurumu içtim.",

                (103, true) => "*Körüğü bırakır* Ocağı söndürdüm, kapıya asma kilidi vurup evime yollandım.",
                (103, false) => "Demirleri soğutup dükkânımı kapattım, gece boyu evimdeydim.",

                (104, true) => "*Tezgaha yaslanır* Kepengi kilitledim, arka odadaki sedirime uzanıp uyudum.",
                (104, false) => "Günün hasılatını kasaya koyup dükkânımı kilitledim ve yattım.",

                (105, true) => "*Önlüğünü çıkarır* Muayenehanemin ışığını söndürüp arka kapıdan evime geçtim.",
                (105, false) => "İlaç dolaplarını kilitleyip evime çıktım, gece boyu uyudum.",

                (106, true) => "*Koltuğuna oturur* Muhtarlık binasını mühürleyip resmi aracımla evime gittim.",
                (106, false) => "Evrak çantamı alıp çıktım, eve gidip ailemle vakit geçirdim.",

                (107, true) => "*Pencereyi kapatır* Gaz lambasını kısıp yatağıma geçtim evladım.",
                (107, false) => "Kitabımı masaya bırakıp lambayı söndürdüm ve uyudum.",

                (108, true) => "*Bıçağını kınına sokar* Dükkânı kilitledim, çamurlu sokaktan evime yürüdüm.",
                (108, false) => "Atölyemi kilitledim, doğruca evime gidip istirahat ettim.",

                _ => isGuilty ? "Sonrasında doğruca mekânıma döndüm amirim." : "Sonrasında evime gidip istirahat ettim amirim."
            };

            return new AIInteractionResponse
            {
                Dialogue = text,
                Emotion = isGuilty ? "Gergin" : "Sakin",
                TrustChange = isGuilty ? -1 : 1,
                StressIncrease = isGuilty ? 10 : 0
            };
        }

        // 4. DELİL / SİLAH TAKİP SORULARI
        if (lastWasWeapon || lastResponse.Contains("satır") || lastResponse.Contains("zehir") || lastResponse.Contains("balta") || lastResponse.Contains("kumaş") || lastResponse.Contains("ip"))
        {
            if (isDetail || isProof || isWhy)
            {
                string text = (npc.NPCId, isGuilty) switch
                {
                    (1, true) => "*Satıra korkuyla bakar* O satırdaki kan... dana kanı diyorum! Adli Tıbba gönderin de görün! *terler*",
                    (1, false) => "Ben kasabım amirim, dükkânda 10 tane satır var. Her birinin faturası da yeri de bellidir.",
                    (2, true) => "*Zehir şişesini saklar* O şişeyi ben hazırlamadım diyorum! Biri tezgahımdan çalmış olmalı!",
                    (2, false) => "Eczanedeki tüm zehirli bileşenler kilitli dolaptadır ve Sağlık Bakanlığı envanterine kayıtlıdır.",
                    (101, true) => "*Baltayı saklar* Baltamdaki koyu leke meşe reçinesidir! Kan falan değil!",
                    (101, false) => "Oduncunun baltasında reçine de olur talaş da. Adli tıp incelesin amirim.",
                    (105, true) => "*Banotu şişesine bakar* O zehir tıbbi amaçlıdır amirim! Cinayet aleti olduğunu kim söyledi?!",
                    (105, false) => "Banotu zehirli bir ottur ama doğru dozda anesteziktir. Laboratuvar kayıtlarım açıktır.",
                    (108, true) => "*Mumlu ipe bakar* O iple taban dikiyorum amirim! Boğulma aleti nereden çıktı?!",
                    (108, false) => "Mumlu saraç ipidir o, kopmaz. Her kunduracıda bulunur.",
                    _ => isGuilty ? "O delille benim alakam yok amirim!" : "Delili adli tıpta inceletmeniz en doğrusu amirim."
                };

                return new AIInteractionResponse
                {
                    Dialogue = text,
                    Emotion = isGuilty ? "Panik" : "Ciddi",
                    TrustChange = isGuilty ? -3 : 1,
                    StressIncrease = isGuilty ? 25 : 0
                };
            }
        }

        // 5. EYLEM / NESNE / KİTAP / İLAÇ / EVRAK TAKİP SORULARI
        if (isWhat || rawTrLower.Contains("oku") || rawTrLower.Contains("kitap") || rawTrLower.Contains("ilac") || rawTrLower.Contains("evrak") || rawTrLower.Contains("dik") || rawTrLower.Contains("dogra") || rawTrLower.Contains("balta") || rawTrLower.Contains("yazi"))
        {
            string text = (npc.NPCId, isGuilty) switch
            {
                (1, true) => "*Satırın sapını sıkar* Dana karkası doğruyordum dedim ya amirim! Ama aklım Osman'ın bana borcunu ödemeyeceğini söyleyişindeydi... Satır elimde donup kalmıştı!",
                (1, false) => "Ertesi gün köye gidecek dana karkasını satırla parçalara ayırıyor, veresiye defterine de teslimat notu düşüyordum amirim.",
                
                (2, true) => "*Gözlerini kırpıştırarak fısıldar* Tezgahta eski bir farmakoloji ve toksikoloji (zehir bilim) rehberini okuyordum amirim... Kalp yetmezliği olanlarda ölümcül dozun ne kadar olduğunu inceliyordum... Yani sadece mesleki merak!",
                (2, false) => "Tezgahta şifalı bitkiler ve farmakoloji ansiklopedisini okuyordum amirim. Kalp hastalarına verilen dijitalis ve nane özü karışımlarının dozajlarını inceliyordum.",
                
                (3, true) => "*Evrakları telaşla kapatır* Kasaba tapu kütüğünü ve Osman'ın üzerindeki hacizli arazilerin listesini inceliyordum! O araziler belediyeye kalacaktı!",
                (3, false) => "Gelecek ayın belediye bütçe taslağını ve kasaba çeşmesinin onarım ihale evraklarını inceliyordum amirim. Hepsi resmi kayıtlıdır.",
                
                (4, true) => "*Nöbet defterini kapatır* Karakolun eski rüşvet soruşturması tutanaklarını ve Osman'ın bana yaptığı tehdit mektuplarını inceliyordum amirim!",
                (4, false) => "O geceki devriye nöbet çizelgesini ve kasaba giriş-çıkış kontrol tutanaklarını inceliyordum meslektaşım.",
                
                (5, true) => "*İpliği parmağına dolar* Osman Bey'in sipariş ettiği paltonun iç astarını dikiyordum... Astarın içine o gizli cebi yerleştiriyordum amirim!",
                (5, false) => "Kış için sipariş edilen kaşe paltonun kol astarını teyelliyordum amirim. Dikiş makinesinin başında sabahladım.",

                // Gölge Şehir
                (101, true) => "*Baltanın sapını ovuşturur* Çam tomruklarını yarıyordum amirim... Ama kafamda Ekrem'in şantajları yankılanıyordu. Odun yerine sanki...",
                (101, false) => "Sobalık meşe kütüklerini yarıp kışlık yakacak destesi yapıyordum amirim.",

                (102, true) => "*Kasaları titreyerek düzeltir* Manavın borç defterine bakıyordum! Ekrem'e olan borcum öyle büyümüştü ki dükkânımı elimden alacaktı!",
                (102, false) => "Göl kenarından gelen taze elma kasalarını sayıyor, çürükleri ayıklıyordum amirim.",

                (103, true) => "*Körüğe bakar* Ekrem'in benden istediği özel şifreli çelik kasanın kilit mekanizmasını dövüyordum amirim. O kilidin tek sırrı bendeydi!",
                (103, false) => "Atölyede saban demirlerini tavında dövüp su veriyordum amirim.",

                (104, true) => "*Bakkal defterini büker* Bakkalın kara kaplı veresiye defterini inceliyordum... Ekrem'in kasabaya taktığı borçların üzerini kırmızı kalemle çiziyordum!",
                (104, false) => "Haftalık gaz yağı, un ve şeker stok kayıtlarımı tutuyordum dedektifim.",

                (105, true) => "*Havan elini sıkar* Eski bir zehirli bitkiler el kitabını okuyordum... Baldıran otunun ve banotunun kanda iz bırakmayan formüllerini hazırlıyordum!",
                (105, false) => "Şifalı bitkiler kodeksini okuyup hastalar için papatya, kantaron ve melisa tentürü hazırlıyordum amirim.",

                (106, true) => "*Mührü masaya vurur* Çam ormanı arazisinin imar tahsis dosyasını okuyordum. Ekrem arazileri üstüne geçirip beni hiçe sayacaktı!",
                (106, false) => "Gölge Şehir'in yıllık vergi matrahlarını ve su şebekesi haritasını inceliyordum dedektif bey.",

                (107, true) => "*Gözlüğünü siler* Gaz lambasında eski bir mahkeme kararını okuyordum evladım... Ekrem'in babamın saatine nasıl el koyduğunu anlatan o acı kararı...",
                (107, false) => "Gaz lambasının ışığında Fuzuli Divanı ve Gölge Şehir'in eski hatıratlarını okuyordum evladım. Geceleri kitap bana yoldaş olur.",

                (108, true) => "*Deri bıçağını sallar* Av çizmelerinin kösele tabanını dikiyordum amirim... O gece göle gidecek adamın çizmesini hazırlıyordum!",
                (108, false) => "Manda derisinden sağlam kışlık av çizmeleri dikiyordum amirim. Mumlu iplikle tabanları çift kat geçtim.",

                _ => isGuilty ? "*Huzursuzca kıpırdanır* O an neyle uğraştığımı tam hatırlamıyorum amirim..." : "Masamda günlük rutin işlerimi toparlıyordum amirim."
            };

            return new AIInteractionResponse
            {
                Dialogue = text,
                Emotion = isGuilty ? "Gergin" : "Sakin",
                TrustChange = isGuilty ? -2 : 1,
                StressIncrease = isGuilty ? 18 : 2
            };
        }

        // 6. DİĞER ŞÜPHELİ TAKİP SORUSU
        if (lastWasPerson || lastResponse.Contains("dikkat et") || lastResponse.Contains("duydum") || lastResponse.Contains("şüpheli"))
        {
            string text = (npc.NPCId >= 100)
                ? "Gölge Şehir'de herkes bir diğerinin açığını kollar amirim. Söylediğim kişinin o geceki hareketlerini ve Adli Tıp raporunu karşılaştırın, gerçeği göreceksiniz."
                : "Kasabada herkes birbiriyle akraba ya da alacaklı amirim. Bahsettiğim kişinin dükkânına gidip bizzat o geceyi sorun, ne kadar kıvırdığını göreceksiniz.";

            return new AIInteractionResponse
            {
                Dialogue = text,
                Emotion = "Düşünceli",
                TrustChange = 1,
                StressIncrease = 0
            };
        }

        return null;
    }

    private double CalculateSemanticScore(string userQuestion, string playerText, string category, string currentIntent, int npcId)
    {
        double score = 0;
        if (category == currentIntent) score += 100;

        string processedUser = TurkishTextEngine.PreprocessSentence(userQuestion);
        string processedPlayer = TurkishTextEngine.PreprocessSentence(playerText);

        var userWords = processedUser.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var playerWords = processedPlayer.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var pWord in playerWords)
        {
            foreach (var uWord in userWords)
            {
                if (uWord == pWord) score += 20;
                else if (uWord.Contains(pWord) || pWord.Contains(uWord)) score += 10;
                else if (TurkishTextEngine.LevenshteinDistance(uWord, pWord) <= 1) score += 5;
            }
        }

        score += _random.NextDouble() * 5;
        return score;
    }

    private static string GetGuiltyEvadedResponse(int npcId)
    {
        return npcId switch
        {
            1 => "*Satırı tezgaha vurur* Sen ne diyorsun amirim?! O gece dükkândaydım diyorum! Kurbanla sorunumuz vardı ama katillik başka şey! Elinizde ne kanıt var ki beni suçluyorsunuz?!",
            2 => "*Gözlerini kaçırır* Ben bir eczacıyım amirim! Kurbanın ilacındaki sorunla benim ne ilgim olabilir? Başka birinin dükkânıma girip girmediğini araştırmalısınız!",
            3 => "*Masayı yumruklar* Beni katillikle mi itham ediyorsunuz?! Arazi anlaşmazlığımız vardı diye cinayeti bana yıkamazsınız! Kanıtınız yoksa muhtarlıktan çıkın!",
            4 => "*Resmi tavrını takınmaya çalışır ama sesi hafif titrer* Ben bu kasabanın komiseriyim amirim! Kanıtınız olmadan bir polise iftira atamazsınız!",
            5 => "*Gözlüklerini siler gibi yapar* Ben dikiş diken yaşlı bir terziyim amirim... Osman ile gizli işlerimiz vardı ama onu öldürmek benim harcım değil!",
            
            // Gölge Şehir Suçlu Kıvırma Tepkileri
            101 => "*Baltasını sıkar ve sertçe bakar* Ne diyorsun amirim sen?! Ormanda kaçak kereste kestim diye katil mi oldum? Elinizde kanıtınız yoksa baltamın yanından uzak durun!",
            102 => "*Neşesi birden kaybolur* Hahaha... amirim siz de şakacısınız! Borcum vardı diye adam mı öldürülür? Kasaların altına bakın, meyveden başka ne bulacaksınız?!",
            103 => "*Örsün üzerine çekici fırlatır* Ağzınızdan çıkanı kulağınız duysun amirim! Çelik kilit dövdüm diye katil mi ilan edildim? Kanıtınız varsa konuşun!",
            104 => "*Panikle ellerini ovuşturur* Aaa tövbeler olsun amirim! Bakkal Naciye bir karıncayı bile incitmez! Veresiye borcunu ödemedi diye fare zehriyle adam öldürülür mü hiç?!",
            105 => "*Sinir krizi eşiğinde titrer* Ben bir tıp adamıyım amirim! Banotu reçetelerim hastaları iyileştirmek içindir! Bana iftira atacağınıza sokaktaki serserileri arayın!",
            106 => "*Masasından kalkıp kurnazca gülümser* Sayın dedektif, makamıma saygısızlık ediyorsunuz. Sahte tapu iddialarınızla cinayeti bana yıkamazsınız!",
            107 => "*Köstekli saatini saklayıp hüzünle bakar* Ben 40 yıllık öğretmenim evladım... Saatimi geri istedim sadece, Ekrem'in ölümünü bana nasıl yakıştırırsınız?",
            108 => "*Çizme bıçağını tezgaha saplar* Huysuzum diye katil mi oldum amirim?! Çamurlu ayak izi kasabanın yarısında var! Kanıtınız yoksa dükkânımı terk edin!",
            
            _ => "Bu ithamı kesinlikle kabul etmiyorum amirim!"
        };
    }

    private static string GetGuiltyDefensiveResponse(int npcId)
    {
        return npcId switch
        {
            1 => "Ha! Kim söylemiş benim katil olduğumu?! Benim alacağım vardı Osman'dan, canlısı işime yarardı! Boş iddialarla dükkânımı meşgul etmeyin!",
            2 => "Bana katil demeden önce bir durun amirim! Elimde ne bir kanıt var ne bir şahit. Ben insan iyileştiririm, can almam!",
            3 => "Dedektif efendi, muhtarınızla doğru konuşun! Siyasi rakiplerimin uydurmasıyla karşıma çıkıp beni suçlayamazsınız!",
            4 => "Bu resmi bir soruşturma mı yoksa şahsi bir itham mı? Kanıtın varsa getir, yoksa karakolumdan dışarı çık!",
            5 => "Ceket dikmekten başka bir şey yapmadım ben. Yaşlı adama iftira atmak kolay tabii...",
            
            101 => "Oduncuyum ben, cinayetle işim olmaz amirim. Boş iddialarla beni oyalamayın!",
            102 => "Manav dükkânında katil aramak da yeni moda oldu galiba amirim!",
            103 => "Demir döverim, laf dövmem. Suçlamalarınız havada kalıyor.",
            104 => "Bakkal Naciye'yi tüm Gölge Şehir tanır! Bana katil demek büyük günahtır amirim!",
            105 => "Tıbbi teşhislerim kanuna uygundur. Boşuna şüphe üretmeyin!",
            106 => "Gölge Şehir Muhtarı olarak bu kasabada adaleti ben temsil ederim dedektif bey!",
            107 => "Yaşlı bir öğretmene iftira atmak hiç yakışmıyor amirim...",
            108 => "Kunduracı deriyi keser, canı değil! Lafınızı bilin de konuşun!",
            
            _ => "İddialarınız tamamen asılsız amirim!"
        };
    }

    private static string GetInnocentAccusationResponse(int npcId)
    {
        return npcId switch
        {
            1 => "Beni katillikle mi suçluyorsun amirim?! Saçmalama! Ben rızkında bir kasabım. Müşterimi niye öldüreyim? Alacağımı kim ödeyecek o zaman?!",
            2 => "Bana bu iftirayı atamazsınız! Yıllardır bu kasabada şifa dağıtıyorum. Katil arıyorsanız gidin Muhtarın kasasına bakın!",
            3 => "Haddinizi bilin amirim! Ben bu kasabanın seçilmiş muhtarıyım! Elinizde hiçbir kanıt yokken bana çamur atamazsınız!",
            4 => "Bir polise katil demek ağır bir iddiadır amirim! Kanıtın olmadan konuşma, resmi soruşturmayı engellemekten hakkında işlem yaparım!",
            5 => "Ben 70 yaşında dikiş diken bir adamım... Kıymayın bana amirim, günahımı almayın!",
            
            101 => "Ben masum bir oduncuyum amirim! Ekrem Bey'le aram iyi değildi ama ona kıymadım! Ormana gidin, asıl katil orada saklanıyor!",
            102 => "Bana iftira atmayın amirim! Ben dükkânımda neşeyle çalışan bir manavım. Gidin hekimin zehirli şişelerine bakın!",
            103 => "Masum insanlara çamur atmayın amirim. Örsümün başındaydım o gece. Katil arıyorsanız muhtarlığa gidin!",
            104 => "Aman amirim tövbe deyin! Bakkal adam katil olur mu? Kunduracının çizmelerine bakın, göl çamuru orada!",
            105 => "Bana bu hakareti edemezsiniz! Ben hayat kurtaran bir hekimim. Katil demircinin örsünden çıkan bıçağı kullandı!",
            106 => "Haddinizi aşmayın dedektif! Gölge Şehir halkının oylarıyla seçilmiş muhtara iftira atamazsınız!",
            107 => "Evladım ben emekli muallimim, tüm kasaba benim talebem sayılır. Bana bu kötülüğü nasıl kondurursunuz?",
            108 => "Yahu huysuzuz dediysek katil mi olduk?! Masum insanları darlamayı bırakın da gerçek katili bulun!",
            
            _ => "Masum insanlara çamur atmayı bırakın da gerçek faili bulun!"
        };
    }

    private static string GetGuiltyAlibiResponse(int npcId)
    {
        return npcId switch
        {
            1 => "Dükkândaydım diyorum! Et doğruyordum... *gözlerini kaçırır* Yağmur bardaktan boşalıyordu. Yani Osman'ın evine sadece borç konuşmaya gittim, o kadar!",
            2 => "Eczanede envanter sayıyordum. Dışarı çıkmadım... *titrer* Şey, gece yarısı sadece hava almak için Osman'ın sokağına doğru yürümüş olabilirim.",
            3 => "Evimdeydim, evrak inceliyordum! ...Gece saat 11 gibi yürüyüşe çıktım. Kurbanın evinin önünden geçtim ama içeri girmedim diyorum!",
            4 => "Karakoldaydım nöbette! ...Olay yerine ihbardan ÖNCE gittiğim yalan! Ben sadece devriye turundaydım!",
            5 => "Atölyemde dikiş dikiyordum. Makine sesi vardı... Bir anlığına sigara içmeye çıktım ama kurbanın evine kadar gitmedim!",
            
            101 => "Kulübede kereste istifliyordum... *terler* Sadece bir ara feneri alıp orman patikasına çıktım, Ekrem'in evine kadar gitmedim!",
            102 => "Dükkânı erken kapattım... *gözlerini kaçırır* Gece yarısı pelerinimi alıp sadece hava almak için göl kenarına yürümüştüm.",
            103 => "Ocakta demir dövüyordum... *sessizleşir* Gece yarısı fener sönünce dükkândan kısa süreliğine ayrıldım.",
            104 => "Erken uyudum amirim... *elleri titrer* Dükkânın arkasında lambayı yaktım ama sadece tütün sarıyordum!",
            105 => "Muayenehanede ilaç hazırlıyordum... *sinirle* Gece Ekrem'in sokağından geçmiş olabilirim ama sadece hastaya gidiyordum!",
            106 => "Ofiste tapu inceliyordum... *kurnazca* Gece 2 gibi kısa bir teftiş yürüyüşü yaptım, hepsi bu.",
            107 => "Penceremde kitap okuyordum... *hüzünle* Saat 02:14'te kapıya çıktım ama sadece temiz hava için.",
            108 => "Dükkânda çizme dikiyordum... *homurdanır* Çamurlu çizmeleri giyip dışarı çıktım ama göl kenarına uğramadım!",
            
            _ => "O gece kendi yerimdeydim."
        };
    }

    private static string GetInnocentAlibiResponse(int npcId)
    {
        return npcId switch
        {
            1 => "O gece dükkânımı geç kapattım amirim. Tezgâhta dana karkası doğruyor ve veresiye defterini kontrol ediyordum. Dışarıdaki yağmurdan başka ses duymadım.",
            2 => "Gece yarısına kadar dükkânım açıktı amirim. Nöbetçi eczaneydim ama hava yağmurlu olduğu için kimse gelmedi. Tezgah arkasında oturmuş şifalı bitkiler ve farmakoloji kitabı okuyordum.",
            3 => "Muhtarlık binasındaydım, belediye bütçe evraklarını ve arazi kayıtlarını inceliyordum. Cinayet saatinde kasaba meydanı tamamen sakindi.",
            4 => "Devriye gezisindeydim meslektaşım. Nöbet tutanağını doldurup sokak kontrollerine çıktım, karakoldaki nöbetçi memurlar da saatimi onaylar.",
            5 => "Dükkânımda kışlık paltonun astarlarını dikiyordum. İpliğim bitene kadar dikiş makinesinin başındaydım, sonra lambayı söndürüp dinlendim.",
            
            101 => "Orman kulübemdeydim amirim. Yağmur başlamadan önce sobalık kütükleri yarıp istifledim ve erkenden yattım.",
            102 => "Akşam üzeri manavı kapattım, elma kasalarını saydıktan sonra komşum Naciye ile biraz laflayıp evime çekildim.",
            103 => "Demirci ocağını akşam söndürdüm, örs başında saban demirlerini dövmüştüm. Yorgunluktan erkenden uyuyakalmışım.",
            104 => "Bakkalın kepengini indirip veresiye defterindeki hesapları kapattım. Gece bekçisi Rıfat amca da beni pencerede görmüştür.",
            105 => "Muayenehanemde şifalı bitki tentürleri hazırlıyor ve tıp kodeksini inceliyordum. Gece boyunca kapım çalmadı.",
            106 => "Muhtarlık makamında köy meclisi kararlarını ve su şebekesi haritasını inceliyordum. Işığım gece boyu yanıktı.",
            107 => "Penceremin kenarında gaz lambasının ışığında divan şiirleri ve kasaba hatıratını okuyordum. 02:14'teki ayak seslerini de o yüzden net duydum.",
            108 => "Kundura tezgâhımda av çizmelerinin kösele tabanını mumlu iple dikiyordum. Gece dışarı adım atmadım.",
            
            _ => "Kendi mekânımdaydım amirim."
        };
    }

    private static string GetGuiltyWeaponResponse(int npcId, string rawTrLower)
    {
        return npcId switch
        {
            1 => "*Tezgahtaki satıra bakıp terler* O satır... dükkânımdan çalınmıştı diyorum size! Birisi benim satırımı alıp Osman'a vurmuş, beni yakmak istiyorlar!",
            2 => "*Zehirli şişeyi görünce elleri titrer* O ilaç reçeteliydi! Şişenin boş olması kurbanın ilacı aşırı dozda içtiğini gösterir, benim suçum ne?!",
            3 => "*Gözlük ve tapuları görünce kızarır* Sahte tapular bir projedir! Kırık gözlük ise kurban bana saldırınca düştü!",
            4 => "*Kopan polis rozetine bakar* O rozet karakoldan çalınmıştı! Olay yerine ben düşürmedim, beni tuzağa düşürüyorlar!",
            5 => "*İplik makarasını cebine saklar* O iplik sağlamdır evet... Ben terziyim amirim, dükkânımda bin tane makara var!",
            
            101 => "*Baltasını arkasına saklar* Baltamdaki lekeler çam reçinesidir amirim! Kanla reçineyi ayırt edemiyor musunuz?!",
            102 => "*Pelerin parçasını görünce yutkunur* O kumaş her tezgâhta var! Pelerinimi çiviler yırttı, olay yeriyle ilgisi yok!",
            103 => "*Örsteki kilit ve bıçağa bakar* O bıçak benim örsümden çıktı ama ben satmadım! Çalınmış olabilir!",
            104 => "*Veresiye defterini kapatır* Yırtık sayfada sadece borç notları vardı! Zehir formülü falan yoktu!",
            105 => "*Mor şişeyi titreyen ellerle tutar* Bu şişedeki banotu özü tıbbi deneyler içindi! Kurbana ben içirmedim!",
            106 => "*Sahte tapuları masanın altına iter* Bu belgeler resmi taslaklardı! Kırık altın gözlük ise bana hediye gelmişti!",
            107 => "*Köstekli saate bakar* Saat babamın emanetiydi... Olay yerinde düştüyse Ekrem çalmıştı demektir!",
            108 => "*Mumlu ipi çeker* Bu ip sadece taban dikmek içindir! Boğulma izleriyle eşleşmesi tamamen tesadüf!",
            
            _ => "O bahsettiğiniz nesneyle benim ilgim yok!"
        };
    }

    private static string GetInnocentWeaponResponse(int npcId, string rawTrLower)
    {
        return npcId switch
        {
            1 => "O delil şüpheli görünüyor ama benim dükkânımla ilgisi yok. Kasabada herkes et yer, herkes bıçak kullanır.",
            2 => "Tıbbi malzemeler ve bitkiler uzmanlık alanımdır. Eğer zehirlenme varsa kurbanın ne içtiğini adli tıp raporu açıklar.",
            3 => "Resmi belgeler ve tapular belediye arşivindedir. Delil dedikleriniz sahtekârların işi olabilir.",
            4 => "Polis delil toplar, karartmaz. O nesne adli laboratuvara gönderilmeli.",
            5 => "O dikiş malzemesi her terzinin tezgahında bulunur. Önemli olan o malzemeyi kimin kullandığıdır.",
            
            101 => "Ormandaki balta ve aletler iş gereğidir. Olay yerindeki delilleri adli laboratuvarda inceleyin amirim.",
            102 => "Meyve kasaları ve kantar normal dükkân eşyası. Şüpheli bir şey varsa araştırın tabii.",
            103 => "Örsümdeki ay damgasını herkes bilir. Bıçak yaparım ama kime satıldığını defterim yazar.",
            104 => "Veresiye defterim herkese açıktır. Delil arıyorsanız dükkânımı didik didik edebilirsiniz.",
            105 => "Banotu zehirli bir bitkidir evet, ama ben hekimim. Zehirle cinayet işleyecek kadar gözü dönmüş biri değilim.",
            106 => "Muhtarlık mührü resmi evraklara basılır. Sahte evrak varsa arkasındaki çeteyi ortaya çıkarın.",
            107 => "Köstekli saatim eski bir antika. Cinayet aletiyle uzaktan yakından ilgisi olamaz evladım.",
            108 => "Mumlu ip ve kundura kalıbı her ayakkabıcıda bulunur. Katili kalıbın numarasından bulabilirsiniz.",
            
            _ => "Bu delili dikkatle incelemenizi tavsiye ederim amirim."
        };
    }

    private static string GetGuiltyMotiveResponse(int npcId)
    {
        return npcId switch
        {
            1 => "Osman bana 50.000 TL borçluydu! Yıllardır emeğimi sömürdü! 'Yarın öderim' deyip dalga geçti! Hangi insan dayanabilir buna?!",
            2 => "Osman beni geçmişimle tehdit ediyordu! Her ay benden şantajla para alıyordu... Artık dayanacak gücüm kalmamıştı!",
            3 => "O arsa belediyenin geleceğiydi! Osman bencillik yapıp vermiyordu. Kasabanın kalkınmasını engelliyordu!",
            4 => "Beni rüşvet almakla suçlayıp savcılığa gidecekti. Şantaj yapıyordu bana! 15 yıllık şerefimi karartacaktı!",
            5 => "O gizli cebe koyduğu USB bellekte tüm ortaklık sırları vardı. Osman beni saf dışı bırakıp servetime el koyacaktı!",
            
            101 => "Ekrem ormandaki kaçak kereste işimi öğrendi! Beni ihbar etmekle tehdit edip haraç istiyordu!",
            102 => "Tüm dükkânımı borç karşılığı elimden alacaktı! Çocuklarımın rızkını o tefeciye yediremezdim!",
            103 => "Gizli çelik kasanın sırrını bana yıktı! İşlediği kaçakçılığın faturasını benim ocağıma çıkaracaktı!",
            104 => "Veresiye borcu 5 bin lirayı bulmuştu! 'Dükkânını yakarım' diye tehdit etti beni!",
            105 => "Geçmişteki tıbbi hatamı kullanarak beni şantajla zehir üretmeye zorluyordu! Dayanamadım artık!",
            106 => "Çam ormanı arazisini ucuza kapatıp beni makamımdan edecekti! Gölge Şehir'in geleceğini ona bırakamazdım!",
            107 => "Babamdan kalan tek hatırayı, o altın köstekli saati zorla elimden aldı! Gururumla oynadı!",
            108 => "Kaçak deri sevkiyatımı öğrenip beni polise vermekle tehdit etti! Yaşlı kunduracıyı köle yapacaktı!",
            
            _ => "Herkesin kendine göre nedenleri vardır."
        };
    }

    private static string GetInnocentMotiveResponse(int npcId)
    {
        return npcId switch
        {
            1 => "Aramızda ticaret vardı, veresiye borcu vardı evet. Ama borçlu adam öldürülür mü amirim? Öldürürsem param hepten batar!",
            2 => "Osman Bey müşterimdi. Aramızda husumet yoktu. Sağlık sorunları dışında kendisiyle özel bir diyalogum olmadı.",
            3 => "Siyasette herkesle anlaşamazsınız. Osman'la fikir ayrılıklarımız oldu ama ben kanunlara inanan bir muhtarım.",
            4 => "Polis ile vatandaş arasındaki ilişki neyse bizimki de oydu. Görevimi yaptım, husumetim yoktu.",
            5 => "Osman iyi bir müşterimdi. Diktirdiğim kıyafetlerin parasını zamanında öderdi. Neden ona kıyayım?",
            
            101 => "Ekrem Bey kereste alırdı benden. Fiyat konusunda pazarlık ederdik ama cinayet sebebi olacak bir husumetimiz yoktu.",
            102 => "Meyve sebze alırdı, parasını da gecikmeli olsa öderdi. Müşterimi neden öldüreyim amirim?",
            103 => "Kasa kilidi siparişi vermişti, parasını da peşin ödedi. Aramızda husumet yoktu.",
            104 => "Veresiye yazdırırdı ama zengin adamdı, eninde sonunda kapatırdı. Husumetim yoktu.",
            105 => "Hastamdı, kalp ilacı yazardım. Hekim hastasına düşmanlık beslemez amirim.",
            106 => "Kasabanın önde gelen tüccarıydı. Fikir ayrılıklarımız oldu ama hepsi resmi çerçevedeydi.",
            107 => "Kitap koleksiyoncusuydu, eski romanları tartışırdık. Kültürlü bir adamdı, aramız iyiydi.",
            108 => "Çizmelerini bana tamir ettirirdi. Huysuzluğum ona özel değildi, herkese karşı böyleyim.",
            
            _ => "Benim kimseyle husumetim yok amirim."
        };
    }

    private static string GetOtherNpcOpinion(int currentNpcId, bool hasan, bool selma, bool kemal, bool gunes, bool yahya, int guiltyId)
    {
        if (hasan && currentNpcId != 1)
            return "Kasap Hasan öfkeli bir adamdır. O gece dükkânında ışık yanıyordu. Öfkesine yenik düşüp satıra sarılmış olabilir.";
        if (selma && currentNpcId != 2)
            return "Eczacı Selma çok sessizdir ama sessiz sudan korkacaksın. Tezgah altında zehirli sarmaşıklar yetiştirdiğini duymuştum.";
        if (kemal && currentNpcId != 3)
            return "Muhtar Kemal kasabayı parmağında oynatır. Osman ile arazi tapuları yüzünden şiddetli kavgaya tutuştuklarını biliyorum.";
        if (gunes && currentNpcId != 4)
            return "Komiser Güneş... Olay yerini çabucak kapatmaya çalıştı sanki. Karakoldaki gizli dosyada bir şeyler saklıyor.";
        if (yahya && currentNpcId != 5)
            return "Terzi Yahya yaşlı görünür ama Osman'la gizli işler çevirirdi. Ceketlerin astarına gizli cep dikerdi.";

        return "Bu kasabada herkes bir şeyler gizliyor amirim. Kimseye gözü kapalı güvenmeyin.";
    }

    private static string GetGolgeNpcOpinion(int currentNpcId, bool tahsin, bool ayse, bool kazim, bool naciye, bool sevgi, bool cevdet, bool fehmi, bool rasim, bool ekrem, int guiltyId)
    {
        if (tahsin && currentNpcId != 101)
            return "Oduncu Tahsin ormanın derinliklerinde kaçak işler çevirirdi. Ekrem Bey ile kereste konusunda şiddetli kavga ettiklerini duydum.";
        if (ayse && currentNpcId != 102)
            return "Manav Ayşe'nin dükkânı Ekrem'e ipotekliydi. Cinayet gecesi peleriniyle telaş içinde koştuğunu görenler olmuş.";
        if (kazim && currentNpcId != 103)
            return "Demirci Kazım çok az konuşur ama Ekrem için özel şifreli çelik kasa kilidi dövmüştü. Ocağın arkasında sırlar saklıyor olabilir.";
        if (naciye && currentNpcId != 104)
            return "Bakkal Naciye kasabadaki tüm borç ve para trafiğini bilir. Ekrem'in veresiye sayfasını yırttığı söyleniyor.";
        if (sevgi && currentNpcId != 105)
            return "Hekim Sevgi şifalı otlar hazırlar ama banotu gibi ölümcül zehirleri de çok iyi bilir. Ekrem'in vücudundaki lekeler şüpheli.";
        if (cevdet && currentNpcId != 106)
            return "Muhtar Cevdet çam ormanı arazisini ele geçirmek için sahte tapu düzenletmişti. Ekrem bunu ifşa etmekle tehdit ediyordu.";
        if (fehmi && currentNpcId != 107)
            return "Fehmi Bey emekli muallimdir, Ekrem onun babasından kalan değerli köstekli saatini gasp etmişti. O gece penceresinden sesler duymuş.";
        if (rasim && currentNpcId != 108)
            return "Kunduracı Rasim kaçak deri işinde Ekrem'e borçluydu. Göl kenarındaki 42 numara çamurlu çizme izleri doğrudan onun atölyesine çıkıyor.";
        if (ekrem)
            return "Tüccar Ekrem Bey kasabanın en zenginiydi ama herkesi borçla ve şantajla köşeye sıkıştırırdı. Sonunda birinin canına tak etti...";

        return "Gölge Şehir'de herkesin Ekrem Bey ile karanlık bir hesabı vardı amirim. Kimseye gözü kapalı güvenmeyin.";
    }

    private static string GetRandomSuspectOpinion(int currentNpcId, int guiltyId)
    {
        var possibleIds = Enumerable.Range(1, 5).Where(id => id != currentNpcId).ToList();
        int targetId = possibleIds[_random.Next(possibleIds.Count)];

        return targetId switch
        {
            1 => "Kasap Hasan'a dikkat et amirim. O satırı sadece et kesmek için kullanmıyor. Öfke kontrolü sıfırdır.",
            2 => "Eczacı Selma'nın tezgahının altındaki şişeleri incelediniz mi? Çok sessiz bir kadındır ama sessiz sudan korkacaksın.",
            3 => "Muhtar Kemal... Kasabada her taşın altından o çıkar. Siyasi gücünü kullanarak herkesi eziyor.",
            4 => "Komiser Güneş'in üniformasına güvenmeyin amirim. Kendi karakolunda karanlık işler çeviriyor.",
            5 => "Terzi Yahya... İhtiyar göründüğüne bakmayın, o terzi dükkanı kasabanın tüm dedikodularının merkezidir.",
            _ => "Herkes şüpheli amirim, gözünüzü açık tutun."
        };
    }

    private static string GetRandomGolgeSuspectOpinion(int currentNpcId, int guiltyId)
    {
        var possibleIds = Enumerable.Range(101, 8).Where(id => id != currentNpcId).ToList();
        int targetId = possibleIds[_random.Next(possibleIds.Count)];

        return targetId switch
        {
            101 => "Oduncu Tahsin'in baltasındaki reçine ve koyu lekelere dikkat edin amirim. Ormanda kaçak işler çevirirdi.",
            102 => "Manav Ayşe dükkânını Ekrem'e kaptırmamak için her şeyi yapabilirdi. Pelerinli hali o gece sokaktaydı.",
            103 => "Demirci Kazım usta Ekrem'in gizli kasasının kilidini yapan adamdır. Ocağının arkasını iyi araştırın.",
            104 => "Bakkal Naciye'nin veresiye defterindeki yırtık sayfaya ve sakladığı fare zehirlerine bakın amirim.",
            105 => "Hekim Sevgi'nin serasındaki mor banotu zehirlerini inceleyin. Ekrem'in tırnaklarındaki morluklar tesadüf değil.",
            106 => "Muhtar Cevdet'in kasasındaki sahte çam ormanı tapusuna ve resmi mühürlü tehdit mektubuna bakın.",
            107 => "Fehmi Bey'in durmuş köstekli saatini ve o gece 02:14'te duyduğu sesleri sorgulayın.",
            108 => "Kunduracı Rasim'in atölyesindeki mumlu ayakkabı iplerini ve 42 numara çamurlu çizmelerini kontrol edin.",
            _ => "Gölge Şehir'de 8 şüphelinin her biri potansiyel katildir amirim. İpuçlarını birleştirin."
        };
    }
}
