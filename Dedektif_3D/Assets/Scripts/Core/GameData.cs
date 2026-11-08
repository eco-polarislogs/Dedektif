using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dedektif3D.Core
{
    public enum BuildingStyle { Eczane, Kasap, Terzi, Muhtarlik, Karakol, Kahvehane, Bakkal, Rural }

    public enum ClueShape { Cleaver, Notebook, Apron, Bottle, Prescription, Leaf, Letter, Glasses, Safe, Badge, FileFolder, Button, Spool, Fabric, Lining, Card }

    public class BuildingDef
    {
        public int NpcId;              // 0 = girilemeyen dekor bina
        public string Key;             // web: building.id (oduncu, demirci, telgrafhane...)
        public string DisplayName;     // Tabelada yazan (web: hoverTag)
        public string WebName;         // Web'deki bina adı — Çetin mesajları için
        public string Details;         // Web: building.details (mimari tarifi)
        public BuildingStyle Style;
        public Color Neon;
        public Vector3 Position;       // Dünya konumu (kasaba haritasından)
        public float Yaw;              // Ön cephe yönü (Rural için)
        public Vector3 Size;
        public Vector2 MapCenter;      // Kasaba haritasında yüzde konum (minimap ve yerleşim için)
        public bool Enterable => NpcId > 0;
    }

    public class NpcDef
    {
        public int Id;
        public string Name, Role, Building, Secret, MurderStory, Portrait, Greeting;
        public bool Female;
        public Color Clothes, Accent;
    }

    public class ClueDef
    {
        public int Id;
        public int BuildingNpcId;
        public string Name, Description, Image;
        public bool Hidden;
        public bool HasBlood, HasPrint;
        public Vector2 BloodSpot, PrintSpot;
        public ClueShape Shape;
        public Vector3 RoomPos;
        public string Document => GameData.Documents.TryGetValue(Id, out var d) ? d : null;
    }

    public class WebQuestion
    {
        public int NpcId;
        public string Q, A, Category;
        public int Difficulty;
        public int[] RelatedClues = new int[0];
        public string[] Guilty;
        /// <summary>Gölgeşehir / Sisören: bu şüpheli katilse verdiği cevap (Resources/Web/dialogue.json).</summary>
        public string G;
        public bool HasLocalAnswer => !string.IsNullOrEmpty(A);
        /// <summary>guiltyResponse: suçluya göre değişen cevap.</summary>
        public string AnswerFor(int guiltyId)
        {
            if (!string.IsNullOrEmpty(G) && guiltyId == NpcId) return G;
            if (Guilty != null && guiltyId >= 1 && guiltyId <= Guilty.Length && !string.IsNullOrEmpty(Guilty[guiltyId - 1])) return Guilty[guiltyId - 1];
            return A;
        }
    }

    public class RegionDef
    {
        public string Id, Name;
        public Rect MapRect;  // Bölge haritasında yüzde (left, top, width, height)
        public int Level;     // 0 = oynanamaz (kilitli bölge)
    }

    /// <summary>Bir kasabanın tüm verisi + atmosferi (web haritası görsellerinden).</summary>
    public class TownData
    {
        public string Id, Name, Victim, CaseTitle, StoryText, MapImage;
        public int Level, ReqBuildings, ReqLabs, MaxBag, Seed;
        public readonly List<BuildingDef> Buildings = new List<BuildingDef>();
        public readonly List<NpcDef> Npcs = new List<NpcDef>();
        public readonly List<ClueDef> Clues = new List<ClueDef>();
        public readonly List<WebQuestion> Questions = new List<WebQuestion>();
        public readonly Dictionary<string, string> HelperFallback = new Dictionary<string, string>();
        public readonly Dictionary<string, string> BuildingHelper = new Dictionary<string, string>();
        public readonly List<KeyValuePair<string, string>> Intro = new List<KeyValuePair<string, string>>(); // (konuşmacı, metin)
        public string LocalHelperName, LocalHelperPortrait;

        // Atmosfer
        public Color SkyTint, Fog, AmbientSky, AmbientEquator, SunColor;
        public float FogDensity, SunIntensity, SunPitch = 32f, SunYaw = -38f, RainIntensity = 1f;

        // ---- Kasaba teması (kullanıcı tarifi): Gizemli Kasaba MAVİ, Gölgeşehir SARI, Sisören YEŞİL ----
        /// <summary>Arayüz vurguları, harita ışıkları ve başlık kartlarının tema rengi.</summary>
        public Color ThemeColor = new Color(0.83f, 0.65f, 0.35f);
        /// <summary>Gündüz sis / ufuk rengi (gece Fog'a döner).</summary>
        public Color DayFog = new Color(0.64f, 0.66f, 0.69f);
        /// <summary>Gündüz sis yoğunluğu çarpanı (Sisören'de gündüz de sis kalkmaz).</summary>
        public float DayFogFactor = 0.55f;
        /// <summary>Güneş ışığı çarpanı (Gölgeşehir karanlık).</summary>
        public float DayLight = 1f;
        /// <summary>Yağmur aralığı (0 = hiç, 1 = sağanak); gün içinde Perlin gürültüsüyle dolaşır.</summary>
        public float RainMin = 0.25f, RainMax = 1f;
        public bool Thunder = true;
        /// <summary>Rüzgâr uğultusu ve uzak uluma (Sisören'in ürkütücü sessizliği).</summary>
        public float Wind;
        // Renk düzeni (post-process)
        public Vector4 GradeShadows = new Vector4(0.9f, 0.95f, 1.1f, -0.05f), GradeHighlights = new Vector4(1.1f, 1.0f, 0.85f, 0f);
        public Color GradeFilter = new Color(1f, 0.95f, 0.88f);
        public float GradeSaturation = -10f, GradeExposure = 0.9f, GradeContrast = 12f;
        /// <summary>Hikâye girişinin kısa atmosfer tarifi (iniş sahnesi alt yazısı).</summary>
        public string Mood = "gece yarısı, sağanak yağmur";

        static readonly System.Globalization.CultureInfo TrUpper = new System.Globalization.CultureInfo("tr-TR");
        public string DisplayName => Name.ToUpper(TrUpper); // "Gölgeşehir" → "GÖLGEŞEHİR" (Türkçe büyük İ)
    }

    /// <summary>
    /// Üç kasabanın verisi. Metinler web'den çıkarılan Resources/Web/*.json dosyalarından yüklenir:
    ///  gizemli_data.json (app.js), golge_sehir_data.json (GolgeSehirConfig.js), sisoren_data.json (SisorenConfig.js).
    /// Level sırası (web: markTownSolved): Gizemli Kasaba (1) → Gölgeşehir (2) → Sisören (3).
    /// </summary>
    public static class GameData
    {
        public const float AutopsyCountdown = 60f;
        public const float JailSeconds = 3f;
        public const int MaxQuestions = 5;
        static readonly System.Globalization.CultureInfo Tr = new System.Globalization.CultureInfo("tr-TR");
        public static readonly string[] Categories = { "tanisma", "derinlesme", "yuzlestirme", "baski", "son" };

        public static readonly List<TownData> Towns = new List<TownData>();
        public static TownData Current { get; private set; }
        public static TownData Town(string id) => Towns.Find(t => t.Id == id);
        public static void SetCurrent(string id) { var t = Town(id); if (t != null) Current = t; }

        // ---- Aktif kasabaya yönlendirilen kısayollar ----
        public static string TownId => Current.Id;
        public static string TownName => Current.DisplayName;
        public static string CaseTitle => Current.CaseTitle;
        public static string StoryText => Current.StoryText;
        public static int MaxBagSize => Current.MaxBag;
        public static int ReqBuildings => Current.ReqBuildings;
        public static int ReqLabs => Current.ReqLabs;
        public static List<BuildingDef> Buildings => Current.Buildings;
        public static List<NpcDef> Npcs => Current.Npcs;
        public static List<ClueDef> Clues => Current.Clues;
        public static List<WebQuestion> Questions => Current.Questions;
        public static bool WebDataLoaded { get; private set; }

        public static NpcDef Npc(int id) => Npcs.Find(n => n.Id == id);
        public static BuildingDef BuildingOf(int npcId) => Buildings.Find(b => b.NpcId == npcId);
        public static ClueDef Clue(int id) => Clues.Find(c => c.Id == id);
        public static IEnumerable<WebQuestion> QuestionsOf(int npcId) => Questions.Where(q => q.NpcId == npcId);

        public static string HelperMessage(string context, string building = null)
        {
            if (!string.IsNullOrEmpty(building) && Current.BuildingHelper.TryGetValue(building, out var b)) return b;
            if (Current.HelperFallback.TryGetValue(context, out var m)) return m;
            return Town("gizemli").HelperFallback.TryGetValue(context, out var g) ? g.Replace("Gizemli Kasaba", Current.Name) : "Amirim, soruşturmaya devam edin!";
        }

        // ------------------------------------------------------------------
        //  BÖLGE HARİTASI (index.html #world-map-screen)
        // ------------------------------------------------------------------
        public static readonly List<RegionDef> Regions = new List<RegionDef>
        {
            new RegionDef { Id = "gizemli",     Name = "GİZEMLİ KASABA", MapRect = new Rect(36, 44, 25, 25), Level = 1 },
            new RegionDef { Id = "golge_sehir", Name = "GÖLGEŞEHİR",     MapRect = new Rect(10, 18, 20, 20), Level = 2 },
            new RegionDef { Id = "sisoren",     Name = "SİSÖREN",        MapRect = new Rect(42, 9, 20, 18),  Level = 3 },
            new RegionDef { Id = "yikikkoy",    Name = "YIKIKKÖY",       MapRect = new Rect(70, 18, 22, 20), Level = 0 },
            new RegionDef { Id = "karaorman",   Name = "KARAORMAN",      MapRect = new Rect(70, 68, 22, 23), Level = 0 },
        };

        [Serializable] class DocItem { public int id; public string text = null; }
        [Serializable] class DocFile { public DocItem[] docs = null; }
        static Dictionary<int, string> _documents;

        /// <summary>"İçeriği Oku / İncele" metinleri: üç kasabanın okunabilir delilleri (Resources/Web/documents.json).</summary>
        public static Dictionary<int, string> Documents
        {
            get
            {
                if (_documents != null) return _documents;
                _documents = new Dictionary<int, string>();
                var asset = Resources.Load<TextAsset>("Web/documents");
                if (asset == null)
                {
                    Debug.LogError("[Dedektif3D] Resources/Web/documents.json bulunamadı: okunabilir deliller açılamaz.");
                    return _documents;
                }
                var file = JsonUtility.FromJson<DocFile>(asset.text);
                if (file?.docs != null)
                    foreach (var d in file.docs) _documents[d.id] = d.text;
                return _documents;
            }
        }

        /// <summary>Serbest soruya sunucu cevap veremezse (app.js npcFallbacks).</summary>
        public static readonly Dictionary<int, string> FreeQuestionFallbacks = new Dictionary<int, string>
        {
            { 1, "*Satırını tezgaha bırakır* Amirim, kasap dükkânımda her gün yüzlerce et parçası doğrarım. Cinayet soruşturmasıyla ilgili net bir soru sorarsanız size yardımcı olurum." },
            { 2, "*İlaç şişelerini düzenler* Eczanemde her türlü ilacın kaydı var amirim. Osman Bey vakasıyla ilgili ne öğrenmek istiyorsanız açıkça sorun." },
            { 3, "*Evrakları düzeltir* Muhtarlık makamındayım dedektif. Kasabanın huzuru ve davayla ilgili ne soracaksanız buyurun." },
            { 4, "*Rozetini düzeltir* Karakoldayız amirim. Soruşturmayla ilgili tutanaklarımız meydanda, ne bilmek istiyorsanız sorun." },
            { 5, "*İğnesini kumaşa batırır* Yaşlı bir terziyim evladım. Cinayet gecesi ve şüpheliler hakkında ne merak ediyorsanız dinliyorum." },
            { 101, "*Baltasını omzuna alır* Ormandan gelen sesler ve Ekrem Bey cinayeti hakkında ne soracaksanız sorun amirim." },
            { 102, "*Tezgahı siler* Manav Ayşe olarak Ekrem Bey vakası hakkında bildiklerimi anlatmaya hazırım amirim." },
            { 103, "*Çekici örse vurur* Demirci Kâzım'ım ben. Ekrem'in çelik kasası veya cinayet hakkında ne soracaksınız?" },
            { 104, "*Veresiye defterini kapatır* Bakkal Naciye her şeyi duyar amirim. Ekrem Bey hakkında ne bilmek istiyorsunuz?" },
            { 105, "*Tıp kitabını kapatır* Hekim olarak tıbbi sorularınıza ve cinayet şüphelerinize yanıt verebilirim amirim." },
            { 106, "*Masasına yaslanır* Muhtar Cevdet olarak kasaba halkı ve Ekrem Bey hakkında ne soracaksanız buyurun." },
            { 107, "*Gözlüğünü düzeltir* Muallim Fehmi olarak cinayet gecesi duyduklarım ve kasaba tarihiyle ilgili ne sormak istersiniz?" },
            { 108, "*Mumlu ipi çeker* Kunduracı Rasim olarak çamurlu çizmeler ve göl yolundaki izler hakkında ne öğrenmek istiyorsunuz?" },
        };

        /// <summary>Hazır soruları biten Gölgeşehir / Sisören şüphelileri için genel sorular (web: getGolgeFallbackQuestions).</summary>
        static readonly string[] GenericQuestions =
        {
            "Cinayet gecesi tam olarak neredeydin?", "Kurbanla aranızda ne tür bir husumet vardı?", "Kasabada kimden şüpheleniyorsun?",
            "O gece garip bir ses ya da hareket fark ettin mi?", "Kurbanı en son ne zaman gördün?", "Bana söylemediğin bir şey var mı?",
            "Delillerin senin dükkânında ne işi var?", "Son sözün nedir?"
        };

        static Color C(string hex) { ColorUtility.TryParseHtmlString(hex, out var c); return c; }

        // ------------------------------------------------------------------
        //  YÜKLEME
        // ------------------------------------------------------------------
        [Serializable] class JNpc { public int id; public string name, building, role, secret, murderStory, portrait, greeting; public bool female; }
        [Serializable] class JClue { public int id, npcId; public string name, desc, img; public bool hidden, blood, print; public float bx, by, px, py, left, top; }
        [Serializable] class JQuestion { public int npcId, difficulty; public string q, a, category; public int[] relatedClues; public string[] guilty; }
        [Serializable] class JKV { public string key, value; }
        [Serializable] class JBuilding { public string key, title, tag, details, interior; public int npcId; public float left, top, width, height; }
        [Serializable] class JData
        {
            public string story, townId, townName, victim, map; public int reqBuildings, reqLabs;
            public JNpc[] npcs; public JClue[] clues; public JQuestion[] questions; public JKV[] helperFallback, buildingHelper, intro; public JBuilding[] buildings;
        }

        static GameData()
        {
            Towns.Add(BuildGizemli());
            Towns.Add(BuildRural("golge_sehir"));
            Towns.Add(BuildRural("sisoren"));
            Current = Towns[0];
        }

        static JData Load(string file)
        {
            var ta = Resources.Load<TextAsset>("Web/" + file);
            if (ta == null) { Debug.LogWarning($"[Dedektif3D] Resources/Web/{file}.json bulunamadı."); return null; }
            try { return JsonUtility.FromJson<JData>(ta.text); }
            catch (Exception e) { Debug.LogWarning($"[Dedektif3D] {file}.json okunamadı: {e.Message}"); return null; }
        }

        // ------------------------------ LEVEL 1: GİZEMLİ KASABA ------------------------------
        static TownData BuildGizemli()
        {
            var t = new TownData
            {
                Id = "gizemli", Name = "Gizemli Kasaba", Level = 1, Victim = "Tüccar Osman Bey", CaseTitle = "VAKA DOSYASI #104", MapImage = "town_map_generated",
                ReqBuildings = 3, ReqLabs = 3, MaxBag = 5, Seed = 104,
                StoryText = "Yağmurlu bir sonbahar gecesi... Kasabanın meydanında bir ceset bulundu. Kurban, herkesin tanıdığı tüccar Osman Bey'di.",
                SkyTint = new Color(0.2f, 0.24f, 0.32f), Fog = new Color(0.14f, 0.16f, 0.2f), FogDensity = 0.016f, SunPitch = 24f,
                AmbientSky = new Color(0.24f, 0.27f, 0.34f), AmbientEquator = new Color(0.15f, 0.15f, 0.18f), SunColor = new Color(0.62f, 0.7f, 0.9f), SunIntensity = 0.42f,
            };
            // LEVEL 1 — öğrenme kasabası: MAVİ tema, yalnızca hafif yağmur sesi ve kasvetli hava (gök gürültüsü yok)
            t.ThemeColor = new Color(0.42f, 0.62f, 0.95f);
            t.SkyTint = new Color(0.2f, 0.27f, 0.4f); t.Fog = new Color(0.12f, 0.15f, 0.22f); t.FogDensity = 0.014f;
            t.AmbientSky = new Color(0.25f, 0.3f, 0.42f); t.AmbientEquator = new Color(0.14f, 0.16f, 0.22f); t.SunColor = new Color(0.62f, 0.72f, 0.96f);
            t.DayFog = new Color(0.56f, 0.62f, 0.72f); t.DayLight = 0.85f;
            t.RainMin = 0.2f; t.RainMax = 0.5f; t.Thunder = false; t.Wind = 0f;
            t.GradeShadows = new Vector4(0.86f, 0.94f, 1.14f, -0.02f); t.GradeHighlights = new Vector4(0.97f, 1.0f, 1.07f, 0f);
            t.GradeFilter = new Color(0.9f, 0.95f, 1.05f); t.GradeSaturation = -12f; t.GradeExposure = 0.95f; t.GradeContrast = 10f;
            t.Mood = "yağmurlu bir sonbahar gecesi, kasvetli mavi sokaklar";
            t.Buildings.AddRange(new[]
            {
                new BuildingDef { NpcId = 2, Key = "eczane",    DisplayName = "ECZANE",    WebName = "Eczane",    Style = BuildingStyle.Eczane,    Neon = C("#00ff88"), Position = new Vector3(-24f, 0, 7f),   Size = new Vector3(9, 6.6f, 8) },
                new BuildingDef { NpcId = 1, Key = "kasap",     DisplayName = "KASAP",     WebName = "Kasap",     Style = BuildingStyle.Kasap,     Neon = C("#ff3344"), Position = new Vector3(-11f, 0, 19f),  Size = new Vector3(8, 6.4f, 8) },
                new BuildingDef { NpcId = 5, Key = "terzi",     DisplayName = "TERZİ",     WebName = "Terzi",     Style = BuildingStyle.Terzi,     Neon = C("#ffcc00"), Position = new Vector3(9f, 0, 20f),    Size = new Vector3(7.5f, 6.8f, 8) },
                new BuildingDef { NpcId = 3, Key = "muhtarlik", DisplayName = "MUHTARLIK", WebName = "Muhtarlık", Style = BuildingStyle.Muhtarlik, Neon = C("#ffaa33"), Position = new Vector3(22f, 0, 13f),   Size = new Vector3(10, 6.6f, 8.5f) },
                new BuildingDef { NpcId = 4, Key = "karakol",   DisplayName = "KARAKOL",   WebName = "Karakol",   Style = BuildingStyle.Karakol,   Neon = C("#2288ff"), Position = new Vector3(25f, 0, -6f),   Size = new Vector3(11, 7f, 9) },
                new BuildingDef { NpcId = 0, Key = "kahvehane", DisplayName = "KAHVEHANE", WebName = "Kahvehane", Style = BuildingStyle.Kahvehane, Neon = C("#ff4455"), Position = new Vector3(-10f, 0, -21f), Size = new Vector3(9, 6f, 7.5f) },
                new BuildingDef { NpcId = 0, Key = "bakkal",    DisplayName = "BAKKAL",    WebName = "Bakkal",    Style = BuildingStyle.Bakkal,    Neon = C("#ff2a6d"), Position = new Vector3(10f, 0, -22f),  Size = new Vector3(8.5f, 5.6f, 7.5f) },
            });
            t.Npcs.AddRange(new[]
            {
                new NpcDef { Id = 1, Name = "Kasap Hasan",   Role = "Kasabadaki eski kasap", Building = "Kasap",     Clothes = C("#2a2420"), Accent = C("#5a3a26"), Portrait = "hasan" },
                new NpcDef { Id = 2, Name = "Eczacı Selma",  Role = "Eczane sahibi",         Building = "Eczane",    Female = true, Clothes = C("#e8f6ef"), Accent = C("#1f7a52"), Portrait = "selma" },
                new NpcDef { Id = 3, Name = "Muhtar Kemal",  Role = "Kasabanın muhtarı",     Building = "Muhtarlık", Clothes = C("#3b3226"), Accent = C("#c08a2e"), Portrait = "kemal" },
                new NpcDef { Id = 4, Name = "Komiser Güneş", Role = "Kadın polis komiseri",  Building = "Karakol",   Female = true, Clothes = C("#1d2a44"), Accent = C("#c9a227"), Portrait = "gunes" },
                new NpcDef { Id = 5, Name = "Terzi Yahya",   Role = "Kasabanın terzisi",     Building = "Terzi",     Clothes = C("#5a4a6b"), Accent = C("#e2c044"), Portrait = "yahya" },
            });
            var shapes = new[] { ClueShape.Cleaver, ClueShape.Notebook, ClueShape.Apron, ClueShape.Bottle, ClueShape.Prescription, ClueShape.Leaf, ClueShape.Letter, ClueShape.Glasses, ClueShape.Safe, ClueShape.Badge, ClueShape.FileFolder, ClueShape.Button, ClueShape.Spool, ClueShape.Fabric, ClueShape.Lining };
            var pos = new[] { new Vector3(0.6f, 1.05f, 1.6f), new Vector3(-1.2f, 1.05f, 1.6f), new Vector3(-4.82f, 1.5f, -1.5f), new Vector3(3.7f, 1.38f, 3.6f), new Vector3(-0.8f, 1.05f, 1.6f), new Vector3(1.4f, 0.08f, 2.4f), new Vector3(-0.6f, 1.05f, 1.6f), new Vector3(0.8f, 1.05f, 1.6f), new Vector3(4f, 0.5f, 3.5f), new Vector3(-0.7f, 1.05f, 1.6f), new Vector3(4.2f, 1.35f, 0.5f), new Vector3(1.2f, 0.03f, -1.8f), new Vector3(0.7f, 1.05f, 1.6f), new Vector3(-1.5f, 0.03f, -1.2f), new Vector3(-3.5f, 1.35f, 2.32f) };
            int[] owner = { 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4, 5, 5, 5 };
            for (int i = 0; i < 15; i++) t.Clues.Add(new ClueDef { Id = i + 1, BuildingNpcId = owner[i], Name = "Delil " + (i + 1), Shape = shapes[i], RoomPos = pos[i] });
            t.HelperFallback["splash"] = "Hoş geldin Amirim! Ben Yardımcı Dedektif Çetin. Bu karanlık davada sana yardımcı olacağım. Hazır olduğunda dosyayı aç ve soruşturmaya başlayalım!";

            var d = Load("gizemli_data");
            if (d != null)
            {
                if (!string.IsNullOrEmpty(d.story)) t.StoryText = d.story;
                foreach (var j in d.npcs ?? new JNpc[0])
                {
                    var n = t.Npcs.Find(x => x.Id == j.id); if (n == null) continue;
                    n.Name = j.name; n.Role = j.role; n.Building = j.building; n.Secret = j.secret; n.MurderStory = j.murderStory;
                    if (!string.IsNullOrEmpty(j.portrait)) n.Portrait = j.portrait;
                }
                foreach (var j in d.clues ?? new JClue[0])
                {
                    var c = t.Clues.Find(x => x.Id == j.id); if (c == null) continue;
                    ApplyClue(c, j);
                }
                foreach (var j in d.questions ?? new JQuestion[0])
                    t.Questions.Add(new WebQuestion { NpcId = j.npcId, Q = j.q, A = j.a, Category = j.category, Difficulty = j.difficulty, RelatedClues = j.relatedClues ?? new int[0], Guilty = j.guilty });
                foreach (var kv in d.helperFallback ?? new JKV[0]) t.HelperFallback[kv.key] = kv.value;
                foreach (var kv in d.buildingHelper ?? new JKV[0]) t.BuildingHelper[kv.key] = kv.value;
                WebDataLoaded = true;
            }
            return t;
        }

        static void ApplyClue(ClueDef c, JClue j)
        {
            c.Name = j.name; c.Description = j.desc; c.Image = j.img; c.Hidden = j.hidden;
            c.HasBlood = j.blood; c.HasPrint = j.print;
            c.BloodSpot = new Vector2(j.bx, j.by); c.PrintSpot = new Vector2(j.px, j.py);
        }

        // ------------------------------ LEVEL 2-3: GÖLGEŞEHİR / SİSÖREN ------------------------------
        static TownData BuildRural(string id)
        {
            bool golge = id == "golge_sehir";
            var t = new TownData
            {
                Id = id, Level = golge ? 2 : 3, MaxBag = golge ? 5 : 8, Seed = golge ? 201 : 301,
                Name = golge ? "Gölgeşehir" : "Sisören", Victim = golge ? "Tüccar Ekrem Bey" : "Kurban",
                CaseTitle = golge ? "VAKA DOSYASI #201 — GÖLGEŞEHİR" : "VAKA DOSYASI #301 — SİSÖREN",
                // golge_sehir_map.png: çam ormanı içinde koyu yeşil-gri sisli köy / sisoren_map_v5.jpg: yeşilimsi sisli dağ yamacı
                SkyTint = golge ? new Color(0.13f, 0.14f, 0.16f) : new Color(0.42f, 0.5f, 0.46f),
                Fog = golge ? new Color(0.08f, 0.09f, 0.1f) : new Color(0.2f, 0.26f, 0.23f),
                FogDensity = golge ? 0.021f : 0.034f,
                AmbientSky = golge ? new Color(0.21f, 0.23f, 0.27f) : new Color(0.36f, 0.43f, 0.39f),
                AmbientEquator = golge ? new Color(0.09f, 0.09f, 0.1f) : new Color(0.24f, 0.28f, 0.25f),
                SunColor = golge ? new Color(0.68f, 0.74f, 0.82f) : new Color(0.84f, 0.92f, 0.88f),
                SunIntensity = golge ? 0.3f : 0.55f, SunPitch = golge ? 28f : 36f, SunYaw = golge ? -60f : 20f, RainIntensity = golge ? 1.1f : 1.3f,
                LocalHelperName = golge ? "Bekçi Rıfat" : "Tüccar İlyas",
                // Gölgeşehir: SARI tema — daha karanlık, soğuk, sağanak ve gök gürültülü; gaz lambalarının sarısı gölgelere karşı
                // Sisören: YEŞİL tema — sisli dağ yamacında aydınlık ama ürkütücü; yağmur yok, rüzgâr ve uzak uluma
                ThemeColor = golge ? new Color(0.95f, 0.76f, 0.28f) : new Color(0.48f, 0.78f, 0.5f),
                DayFog = golge ? new Color(0.4f, 0.41f, 0.42f) : new Color(0.7f, 0.77f, 0.72f),
                DayFogFactor = golge ? 0.6f : 1.05f,
                DayLight = golge ? 0.55f : 1.05f,
                RainMin = golge ? 0.6f : 0f, RainMax = golge ? 1f : 0f, Thunder = golge, Wind = golge ? 0.12f : 0.4f,
                GradeShadows = golge ? new Vector4(0.88f, 0.97f, 1.12f, -0.07f) : new Vector4(0.93f, 1.05f, 0.96f, 0.01f),
                GradeHighlights = golge ? new Vector4(1.16f, 1.02f, 0.66f, 0.02f) : new Vector4(0.98f, 1.05f, 0.98f, 0.04f),
                GradeFilter = golge ? new Color(1f, 0.92f, 0.76f) : new Color(0.93f, 1.02f, 0.94f),
                GradeSaturation = golge ? -6f : -24f, GradeExposure = golge ? 0.74f : 1.18f, GradeContrast = golge ? 18f : 5f,
                Mood = golge ? "soğuk, sağanak bir gece; gaz lambalarının sarısı karanlığa direniyor" : "dağ yamacında sis; gün ışığı var ama hiçbir şey net değil",
                LocalHelperPortrait = golge ? "images/towns/golge_sehir/npcler/bekci_hasan_helper.png" : "images/towns/sisoren/npcler/tuccar_ilyas_helper.png",
            };
            var d = Load(id + "_data");
            if (d == null) { t.Name += " (veri yok)"; return t; }
            if (!string.IsNullOrEmpty(d.townName)) t.Name = d.townName.Replace("Gölge Şehir", "Gölgeşehir");
            if (!string.IsNullOrEmpty(d.victim)) t.Victim = d.victim;
            t.StoryText = d.story; t.MapImage = d.map;
            t.ReqBuildings = d.reqBuildings > 0 ? d.reqBuildings : 4;
            t.ReqLabs = d.reqLabs > 0 ? d.reqLabs : 4;

            var palette = new[] { C("#3a3128"), C("#2c3a2e"), C("#463b2c"), C("#2e2f3a"), C("#3d2b2b"), C("#4a4232"), C("#28323a"), C("#3a2f3f") };
            int k = 0;
            foreach (var b in d.buildings ?? new JBuilding[0])
            {
                var def = new BuildingDef
                {
                    NpcId = b.npcId, Key = b.key, DisplayName = b.tag, WebName = b.title, Details = b.details, Style = BuildingStyle.Rural,
                    Neon = b.key == "sinema" ? C("#ffe27a") : new Color(1f, 0.72f, 0.42f),
                    MapCenter = new Vector2(b.left + b.width / 2f, b.top + b.height / 2f),
                };
                t.Buildings.Add(def);
            }
            foreach (var j in d.npcs ?? new JNpc[0])
            {
                t.Npcs.Add(new NpcDef
                {
                    Id = j.id, Name = j.name, Role = j.role, Building = j.building, Female = j.female, Secret = j.secret, MurderStory = j.murderStory,
                    Portrait = j.portrait, Greeting = j.greeting, Clothes = palette[k % palette.Length], Accent = palette[(k + 3) % palette.Length] * 1.4f
                });
                k++;
            }
            foreach (var j in d.clues ?? new JClue[0])
            {
                var c = new ClueDef { Id = j.id, BuildingNpcId = j.npcId, Shape = ClueShape.Card };
                ApplyClue(c, j);
                // Web'deki iç mekan görselinde (left%, top%) konum → oda yerel konumu (X -5..5, Z -4..4).
                // Duvar dibindeki 1.4 m'lik şerit mobilyalara ayrılır; delil sehpaları odanın ortasında kalır.
                float x = Mathf.Lerp(-3.4f, 3.4f, Mathf.Clamp01(j.left / 100f));
                float z = Mathf.Lerp(2.9f, -2.3f, Mathf.Clamp01(j.top / 100f));
                if (Mathf.Abs(x) < 1.6f && z > 1.0f && z < 2.4f) z = 0.4f; // tezgah/NPC önünü boş bırak
                if (Mathf.Abs(x) < 1.1f && z < -1.6f) x = x < 0 ? -1.4f : 1.4f; // kapı ile giriş yolu boş kalsın
                c.RoomPos = new Vector3(x, 0f, z);
                t.Clues.Add(c);
            }
            EnsureHiddenClues(t, id == "sisoren" ? 2 : 1);
            foreach (var j in d.questions ?? new JQuestion[0])
                if (!string.IsNullOrEmpty(j.q))
                    t.Questions.Add(new WebQuestion { NpcId = j.npcId, Q = j.q, A = j.a, Category = j.category ?? "", Difficulty = Mathf.Max(1, j.difficulty), RelatedClues = j.relatedClues ?? new int[0] });
            foreach (var n in t.Npcs)
                for (int i = 0; i < GenericQuestions.Length; i++)
                    t.Questions.Add(new WebQuestion { NpcId = n.Id, Q = GenericQuestions[i], A = "", Category = "", Difficulty = 2 + i / 3 });
            ApplyDialogue(t);
            foreach (var kv in d.intro ?? new JKV[0]) t.Intro.Add(new KeyValuePair<string, string>(kv.key, kv.value));
            t.HelperFallback["map_enter"] = $"İşte {t.Name} Amirim! {t.Buildings.Count} binayı gezip delilleri toplayın; en az {t.ReqBuildings} bina ve {t.ReqLabs} laboratuvar gönderimi otopsi raporunu başlatır.";
            t.HelperFallback["story_end"] = $"{t.Name}'de {t.Npcs.Count} şüpheli var Amirim. Katili bulduğunuzda 'BULDUM!' [F] ile suçlamanızı yapın!";
            t.HelperFallback["building_enter"] = "Fareyi odada gezdirin Amirim; delilin üstüne gelince parlar. Saklanmış olanlar için çekmece, sandık, tablo arkası ve döşeme altlarının üstüne gelin ya da Dedektif Gözü'nü [Q] açın!";
            return t;
        }

        /// <summary>
        /// "Gizli delilleri sakla": her binada en az <paramref name="perBuilding"/> delil bir saklama yerine gizlenir
        /// (web verisinde Sisören'in hiç gizli delili yoktu). Seçim deterministiktir — her oyunda aynı delil saklanır.
        /// </summary>
        static void EnsureHiddenClues(TownData t, int perBuilding)
        {
            foreach (var g in t.Clues.GroupBy(c => c.BuildingNpcId))
            {
                var list = g.OrderBy(c => c.Id).ToList();
                int hidden = list.Count(c => c.Hidden);
                // En "suç kokan" deliller saklanır: kanlı/izli olanlar önce, sonra listenin sonundakiler
                foreach (var c in list.OrderByDescending(c => (c.HasBlood ? 2 : 0) + (c.HasPrint ? 1 : 0)).ThenByDescending(c => c.Id))
                {
                    if (hidden >= perBuilding || hidden >= list.Count - 2) break;
                    if (c.Hidden) continue;
                    c.Hidden = true; hidden++;
                }
            }
        }

        // ------------------------------------------------------------------
        //  ÇEVRİMDIŞI KONUŞMALAR (Resources/Web/dialogue.json)
        // ------------------------------------------------------------------
        [Serializable] class JDialogueQA { public string q = null, a = null, g = null; }
        [Serializable] class JDialogueNpc { public int id = 0; public string autopsy = null; public string[] alibi = null, grudge = null, suspect = null, heard = null, secret = null; public JDialogueQA[] qa = null; }
        [Serializable] class JDialogueFile { public JDialogueNpc[] npcs = null; }
        static Dictionary<int, JDialogueNpc> _dialogue;

        static Dictionary<int, JDialogueNpc> Dialogue
        {
            get
            {
                if (_dialogue != null) return _dialogue;
                _dialogue = new Dictionary<int, JDialogueNpc>();
                var asset = Resources.Load<TextAsset>("Web/dialogue");
                if (asset == null)
                {
                    Debug.LogError("[Dedektif3D] Resources/Web/dialogue.json bulunamadı: şüpheliler genel cevaplar verecek.");
                    return _dialogue;
                }
                var file = JsonUtility.FromJson<JDialogueFile>(asset.text);
                if (file?.npcs != null)
                    foreach (var n in file.npcs) _dialogue[n.id] = n;
                return _dialogue;
            }
        }

        /// <summary>Hazır soruların cevaplarını kişiye özel metinle doldurur (masum ve katil hâli).</summary>
        static void ApplyDialogue(TownData t)
        {
            foreach (var q in t.Questions)
            {
                if (!Dialogue.TryGetValue(q.NpcId, out var d)) continue;
                int generic = Array.IndexOf(GenericQuestions, q.Q);
                string[] pair = generic < 0 ? null : LinesFor(d, generic);
                if (pair != null) { q.A = pair[0]; q.G = pair[1]; continue; }
                if (generic >= 0) { q.A = CommonLine(generic, q.NpcId, false, t); q.G = CommonLine(generic, q.NpcId, true, t); continue; }
                var qa = d.qa != null ? Array.Find(d.qa, x => x.q == q.Q) : null;
                if (qa != null) { q.A = qa.a; q.G = qa.g; }
            }
        }

        static string[] LinesFor(JDialogueNpc d, int genericIndex)
        {
            switch (genericIndex)
            {
                case 0: return d.alibi;
                case 1: return d.grudge;
                case 2: return d.suspect;
                case 3: return d.heard;
                case 5: return d.secret;
                default: return null;
            }
        }

        /// <summary>Herkesin kendi diliyle söyleyebileceği kısa cevaplar: "en son ne zaman", "deliller", "son sözün".</summary>
        static string CommonLine(int genericIndex, int npcId, bool guilty, TownData t)
        {
            string v = t.Victim;
            string[] list;
            switch (genericIndex)
            {
                case 4:
                    list = guilty
                        ? new[] { $"*Yutkunur* {v}'i en son... birkaç gün önce miydi? Hatırlamıyorum. Neden bu kadar üstüme geliyorsunuz?",
                                  "O gece değil! Yani o geceden önce. Kimse sizinle aksini iddia edemez, bakın kimseye sormayın.",
                                  $"{v}... Hatırlamaya çalışıyorum amirim. Hafızam böyle sıkışınca çalışmıyor." }
                        : new[] { $"Cinayetten önceki gün gördüm amirim. Her zamanki gibi burnu havadaydı, selam bile vermedi.",
                                  $"{v}'i önceki akşam meydanda gördüm, birine bağırıyordu. Kime, seçemedim.",
                                  "Birkaç gün önceydi. Yanından geçerken göz bile değdirmezdi, alışkındık." };
                    break;
                case 6:
                    list = guilty
                        ? new[] { "*Gözleri deliller ile kapı arasında gidip gelir* Onları kim oraya koydu bilmem. Birisi beni tuzağa düşürüyor!",
                                  "Dükkânımda buldunuz diye benim mi oldu? Herkes girip çıkıyor buraya. Bunları götürmenize de gerek yok, sadece eşya işte.",
                                  "O eşyaların hiçbiriyle ilgim yok. Yani ilgim var ama... İşimle ilgili! Sadece işimle." }
                        : new[] { "Dükkânımda bulduklarınız mesleğimin sıradan eşyaları amirim. Laboratuvarınız ne buluyorsa bulsun, korkmuyorum.",
                                  "Burada her gün yüzlerce el değer eşyalara. Birini bağlayacaksa da beni değil, buraya gireni bağlar.",
                                  "Alın, bakın, inceleyin. Saklayacak bir şeyim olsa bunları ortada bırakmazdım." };
                    break;
                default:
                    list = guilty
                        ? new[] { "*Uzun bir sessizlik* Söyleyecek başka bir şeyim yok. Vekilimle konuşun.",
                                  "Son söz mü? Son söz sizde değil amirim. Kasaba yarın unutacak zaten. *Sesi titrer*",
                                  "Beni rahat bırakın. Ne yaptıysam... ne yapmadıysam, Allah bilir." }
                        : new[] { "Son sözüm şu: Katil hâlâ dışarıda, siz burada vakit kaybediyorsunuz.",
                                  "Vicdanım rahat amirim. Dilerim siz de doğru kişiyi bulunca rahat edersiniz.",
                                  "Size yardım etmek isterim. Ama yanlış adamın peşindesiniz, bunu da bilin." };
                    break;
            }
            return list[Mathf.Abs(npcId) % list.Length];
        }

        /// <summary>Katile göre değişen otopsi bulgusu: ölüm şekli delillerle eşleştirilebilsin diye (Gölgeşehir / Sisören).</summary>
        public static string AutopsyFinding(int guiltyId) => Dialogue.TryGetValue(guiltyId, out var d) ? d.autopsy : null;

        static readonly string[] AskStopwords = { "bana", "benim", "senin", "neden", "nasıl", "için", "hakkında", "biliyor", "musun", "misin", "kadar", "peki", "ama", "şey", "acaba", "söyle", "anlat", "bunu", "şunu", "olan", "gibi" };

        /// <summary>
        /// Serbest yazılan soru (sunucu yok ya da cevap veremedi): şüphelinin hazır sorularından en yakınına, o da yoksa konusuna göre cevap verir.
        /// Türkçe ekler yüzünden sözcükler ilk 4 harfle karşılaştırılır.
        /// </summary>
        public static string AnswerFree(NpcDef n, string text, int guiltyId)
        {
            var words = Words(text);
            WebQuestion best = null;
            float bestScore = 0f;
            foreach (var q in QuestionsOf(n.Id))
            {
                if (!q.HasLocalAnswer) continue;
                var qw = Words(q.Q);
                int common = words.Count(w => qw.Any(x => x.StartsWith(Stem(w))));
                float score = words.Count == 0 ? 0f : common / (float)words.Count;
                if (score > bestScore) { bestScore = score; best = q; }
            }
            if (best != null && bestScore >= 0.4f) return best.AnswerFor(guiltyId);

            int topic = -1;
            string all = text.ToLower(Tr);
            bool Has(params string[] keys) { foreach (var k in keys) if (all.Contains(k)) return true; return false; }
            if (Has("nerede", "neredeydi", "gece ne", "saat", "uyu", "evde")) topic = 0;
            else if (Has("kavga", "husumet", "borç", "düşman", "sevmez", "tartış")) topic = 1;
            else if (Has("kim", "şüphelen", "katil", "suçlu")) topic = 2;
            else if (Has("ses", "duy", "gör", "gürültü", "çığlık")) topic = 3;
            else if (Has("en son", "son kez", "ne zaman")) topic = 4;
            else if (Has("sakla", "gizli", "yalan", "sır", "itiraf")) topic = 5;
            else if (Has("delil", "dükkân", "dükkan", "buldu", "çanta")) topic = 6;
            if (topic >= 0)
            {
                var generic = QuestionsOf(n.Id).FirstOrDefault(q => q.Q == GenericQuestions[topic] && q.HasLocalAnswer);
                if (generic != null) return generic.AnswerFor(guiltyId);
            }
            return FreeQuestionFallbacks.TryGetValue(n.Id, out var fb) ? fb
                : guiltyId == n.Id ? "*Gözlerini kaçırır* Bunun cinayetle ne ilgisi var amirim? Beni başka şeylerle meşgul etmeyin."
                : "Soruyu tam anlamadım amirim. Cinayet gecesini, şüphelileri ya da delilleri sorun, bildiğimi anlatırım.";
        }

        static string Stem(string w) => w.Length <= 4 ? w : w.Substring(0, 4);

        static List<string> Words(string text)
        {
            var list = new List<string>();
            foreach (var w in System.Text.RegularExpressions.Regex.Split((text ?? "").ToLower(Tr), @"[^\p{L}\p{N}]+"))
                if (w.Length >= 3 && Array.IndexOf(AskStopwords, w) < 0) list.Add(w);
            return list;
        }

        /// <summary>Son çare cevabı (hazır metni olmayan soru ve kapalı sunucu).</summary>
        public static string OfflineAnswer(NpcDef n, string question, bool guilty, int askedIndex)
        {
            if (FreeQuestionFallbacks.TryGetValue(n.Id, out var fb) && askedIndex == 0) return fb;
            string[] innocent =
            {
                $"{n.Greeting} Ama inanın, o gece olanlarla benim bir ilgim yok amirim.",
                "O gece erkenden kapattım, hava o kadar kötüydü ki elimi göremiyordum.",
                $"{Current.Victim} herkesle kavgalıydı amirim. Benden başka birini sorgulamanız lazım.",
                "Bildiklerimi anlattım. Gerisini kasabaya sorun."
            };
            string[] guiltyLines =
            {
                $"{n.Greeting} *Gözlerini kaçırır* Neden bana soruyorsunuz ki?",
                "*Terler* Ben... o gece evdeydim. Kimse görmedi ama... evdeydim işte!",
                $"{Current.Victim} mı? Ona borcum yoktu! ...Yani, pek yoktu.",
                "*Sesi titrer* Bu soruların amacı ne amirim? Beni suçlu çıkarmaya mı çalışıyorsunuz?"
            };
            var list = guilty ? guiltyLines : innocent;
            return list[Mathf.Abs(question.GetHashCode() + askedIndex) % list.Length];
        }
    }
}
