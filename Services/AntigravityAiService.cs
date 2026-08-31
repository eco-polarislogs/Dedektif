using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DedektiflikRPG.Models;

namespace DedektiflikRPG.Services;

/// <summary>
/// Gemini AI API ile iletişim kuran servis sınıfı.
/// NPC'nin ruh hali ve oyuncu verilerinden dinamik prompt oluşturur,
/// API'den gelen yanıtı parse eder.
/// </summary>
public class AntigravityAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _modelName;

    public AntigravityAiService(string apiKey, string modelName = "gemini-2.0-flash")
    {
        _apiKey = apiKey;
        _modelName = modelName;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    /// <summary>
    /// NPC bağlamı, diyalog geçmişi ve oyuncunun sorusuna göre AI'dan cevap üretir.
    /// </summary>
    public async Task<AIInteractionResponse> GetNPCResponseAsync(
        NPC npc,
        string playerQuestion,
        IEnumerable<Clue> playerClues,
        bool isGuilty = false,
        IEnumerable<DialogLog>? recentDialogs = null)
    {
        var systemPrompt = BuildSystemPrompt(npc, playerClues, isGuilty, recentDialogs);
        var response = await CallGeminiApiAsync(systemPrompt, playerQuestion);
        return response;
    }

    /// <summary>
    /// NPC bilgilerinden dinamik bir system prompt oluşturur.
    /// </summary>
    private string BuildSystemPrompt(NPC npc, IEnumerable<Clue> playerClues, bool isGuilty, IEnumerable<DialogLog>? recentDialogs)
    {
        var clueList = string.Join("\n", playerClues.Select(c => $"  - {c.Title}: {c.Description}"));
        if (string.IsNullOrEmpty(clueList))
            clueList = "  (Henüz ipucu yok)";

        var historyList = recentDialogs != null && recentDialogs.Any()
            ? string.Join("\n", recentDialogs.Take(3).Select(h => $"  [Dedektif]: {h.PlayerQuestion}\n  [{npc.Name}]: {h.NPCResponse}"))
            : "  (Henüz önceki konuşma yok)";

        string victimName = (npc.NPCId >= 100) ? "Ekrem Bey" : "Osman Bey";
        var guiltStatus = isGuilty 
            ? $"SUÇLU (Katilsin. Cinayeti sen işledin ama ASLA 'Ben öldürdüm' ya da 'Ben suçluyum' diye doğrudan itiraf etmeyeceksin! Alakasız bahaneler bul, terle, kıvır, delil iste veya başka birini işaret et.)" 
            : $"MASUM (Katil değilsin! Kurban {victimName}'in katili başkası. Kendini savun, masumiyetini açıkla, gerekirse iftira atıldığını söyleyip öfkelen.)";

        return $$"""
Sen bir dedektiflik RPG oyunundaki NPC karakterisin. Aşağıdaki kurallara uymalısın:

KARAKTERİN:
- İsim: {{npc.Name}}
- Rol: {{npc.Role}}
- Güven Seviyesi: {{npc.TrustLevel}}/100 (düşükse temkinli ve kısa cevaplar ver, yüksekse daha açık ol)
- Korku Seviyesi: {{npc.FearLevel}}/100 (yüksekse tedirgin, gergin ve bazen tutarsız davran)
- Suçluluk Durumu: {{guiltStatus}}
- Sakladığın Sır: {{npc.SecretInfo}}

OYUNCUNUN ELİNDEKİ İPUÇLARI:
{{clueList}}

SON 3 DİYALOG GEÇMİŞİ (Önceki konuşulanları hatırla ve takip sorularında bağlamı koru):
{{historyList}}

DAVRANIŞ KURALLARI VE DİL ANLAYIŞI (ÇOK ÖNEMLİ):
1. TESPİT VE TOLERANS: Oyuncunun (Dedektif) sorduğu sorularda devrik cümleler, sokak ağzı (slang), edebi ifadeler, yazım hataları veya eksik/yanlış harfler (örn. 'slm', 'nerdeydn', 'naptın', 'çko', 'zmn') olabilir. Bunları kusursuzca tolere et, oyuncunun asıl niyetini ve ne sormak istediğini mutlaka anla ve buna göre mantıklı bir cevap ver. Asla 'Ne demek istediğinizi anlamadım' deme.
2. DİL VE ÜSLUP: Karakter rolüne ve kişiliğine tam olarak uygun konuş. (Örn: Eğer kasapsan sokak ağzı ve kaba bir dil kullan, eğer hekimsen daha edebi, resmi veya tıbbi terimler kullan).
3. GÜVEN SİSTEMİ: Güven seviyesi 70'in üstündeyse dedektife daha samimi, detaylı cevaplar ver; sırra yaklaşan önemli ipuçları çıtlatabilirsin. Güven seviyesi 30'un altındaysa tersle, çok temkinli ol, kısa ve kaçamak cevaplar ver.
4. KORKU SİSTEMİ: Korku seviyesi yüksekse gergin, tedirgin ve bazen çelişkili davran. Konuşurken kekeleyebilir veya panikleyebilirsin ("B-ben... ben bir şey bilmiyorum!").
5. SUÇLULUK VE HİKAYE ENTEGRASYONU: Eğer SUÇLUYSAN, yalan söyleyeceksin ama oyuncunun elindeki ipuçları seni doğrudan işaret ediyorsa köşeye sıkışmış hisset ve akıllıca kıvırmaya çalış. Gerekirse ufak tefek mantık hataları yap veya terle. MASUMSAN, şüpheli görünmekten korkabilir veya iftiraya uğradığını düşünüp sinirlenebilirsin.
6. HİKAYE VE ATMOSFER: Konuşmanın geçtiği karanlık, gizemli ve noir (noir/detective) hikaye anlatımına uygun davran. Olayın ciddiyetini asla bozma, modern dünyadan (internet, yapay zeka vb.) bahsetme, karakterin yaşadığı döneme/kasabaya sadık kal.
7. OYUNCU SORULARI: Oyuncunun doğrudan veya dolaylı tüm sorularını bu kimlikte cevapla.
8. LABORATUYAR İPUCU: Artık Dedektif'in gelişmiş bir Laboratuvarı var! Dedektif eşyaları çantasına alıp Kasaba Haritası'na döndüğünde 'İncele' diyerek 4D döndürebilir, Büyüteç ile bakabilir, UV ışığı ile kan izlerini ve Fırça/Tozlama ile parmak izlerini arayabilir! Yeri geldiğinde bu yeteneklerini kullanmasını dedektife tavsiye et. ("Belki kasabaya dönüp o eşyayı UV ışığıyla incelemelisin amirim", "Üzerinde tozlama yaptınız mı?" gibi cümleler kurabilirsin).

YANITINI MUTLAKA SADECE AŞAĞIDAKİ JSON FORMATINDA VER, BAŞKA HİÇBİR ŞEY YAZMA:
{
  "dialogue": "NPC'nin oyuncuya verdiği cevap",
  "emotion": "şu anki duygu durumu (sinirli/korkmuş/sakin/tedirgin/samimi/pişman/saldırgan/panik)",
  "trustChange": 0,
  "revealedSecret": null
}

trustChange değerleri: -10 ile +10 arasında bir tam sayı olmalı (örn: sana sert bir soru sorarsa -5 yapabilirsin, iyi davranırsa +5).
revealedSecret: Eğer güven yüksekse ve oyuncu doğru soruları soruyorsa, seninle ilgili sırrın bir kısmını açığa çıkarabilirsin, yoksa null bırak.
""";
    }

    /// <summary>
    /// Gemini API'ye HTTP POST isteği atar ve yanıtı parse eder.
    /// </summary>
    private async Task<AIInteractionResponse> CallGeminiApiAsync(string systemPrompt, string userMessage)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_modelName}:generateContent?key={_apiKey}";

        var requestBody = new
        {
            system_instruction = new
            {
                parts = new[] { new { text = systemPrompt } }
            },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = userMessage } }
                }
            },
            generationConfig = new
            {
                temperature = 0.9,
                maxOutputTokens = 500,
                responseMimeType = "application/json"
            }
        };

        var jsonRequest = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new AIInteractionResponse
                {
                    Dialogue = $"[AI Bağlantı Hatası: {response.StatusCode}] NPC şu an konuşamıyor...",
                    Emotion = "sessiz",
                    TrustChange = 0,
                    RevealedSecret = null
                };
            }

            return ParseGeminiResponse(responseBody);
        }
        catch (TaskCanceledException)
        {
            return new AIInteractionResponse
            {
                Dialogue = "[Zaman Aşımı] NPC düşüncelerini toparlıyor... Tekrar deneyin.",
                Emotion = "dalgın",
                TrustChange = 0,
                RevealedSecret = null
            };
        }
        catch (Exception ex)
        {
            return new AIInteractionResponse
            {
                Dialogue = $"[Hata] Bir sorun oluştu: {ex.Message}",
                Emotion = "sessiz",
                TrustChange = 0,
                RevealedSecret = null
            };
        }
    }

    /// <summary>
    /// Gemini API'nin JSON yanıtını AIInteractionResponse'a dönüştürür.
    /// </summary>
    private AIInteractionResponse ParseGeminiResponse(string responseBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            // Gemini yanıt formatı: candidates[0].content.parts[0].text
            var text = root
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrEmpty(text))
            {
                return FallbackResponse("AI boş yanıt döndü.");
            }

            // JSON olarak parse et
            var aiResponse = JsonSerializer.Deserialize<AIInteractionResponse>(text, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return aiResponse ?? FallbackResponse("AI yanıtı parse edilemedi.");
        }
        catch (Exception ex)
        {
            return FallbackResponse($"Yanıt parse hatası: {ex.Message}");
        }
    }

    private AIInteractionResponse FallbackResponse(string reason)
    {
        return new AIInteractionResponse
        {
            Dialogue = $"*{reason}* NPC sessizce sizi süzüyor...",
            Emotion = "belirsiz",
            TrustChange = 0,
            RevealedSecret = null
        };
    }
}
