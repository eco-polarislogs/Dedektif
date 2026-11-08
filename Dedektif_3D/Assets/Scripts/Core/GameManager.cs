using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dedektif3D.API;
using Dedektif3D.Gameplay;
using Dedektif3D.UI;
using Dedektif3D.World;
using UnityEngine;

namespace Dedektif3D.Core
{
    /// <summary>Web ekran sırası: splash-screen → world-map-screen → story-intro-screen → (kasabaya iniş) → town-map/interior → result.</summary>
    public enum GamePhase { Splash, WorldMap, Story, Descent, Playing, Result }

    public class ClueRuntime
    {
        public ClueDef Def;
        public int Id => Def.Id;
        public bool InBag;
        public bool BloodFound, PrintFound, BloodChecked, PrintChecked;
        public bool BloodSent, PrintSent;
        public ClueInteractable View;
    }

    public class DialogueLine
    {
        public bool FromPlayer;
        public string Speaker, Text;
        public float StartTime;           // Daktilo efekti başlangıcı
        public const float CharsPerSecond = 45f;
        public bool Typed => FromPlayer || (Time.time - StartTime) * CharsPerSecond >= Text.Length;
        public string Visible => FromPlayer ? Text : Text.Substring(0, Mathf.Clamp((int)((Time.time - StartTime) * CharsPerSecond), 0, Text.Length));
    }

    public class NpcRuntime
    {
        public NpcDef Def;
        public int Id => Def.Id;
        public int Stress;
        public int Asked;
        public bool Waiting;
        public bool WokenUp;   // gece yarısı uyandırıldı
        public readonly List<DialogueLine> Log = new List<DialogueLine>();
        public readonly HashSet<string> AskedQuestions = new HashSet<string>();
        public List<WebQuestion> Suggestions = new List<WebQuestion>();
        public bool LeftInterrogation => Stress >= 100;
        public bool Finished => Asked >= GameData.MaxQuestions;
        public int Bpm => Mathf.RoundToInt(68 + Stress * 0.75f);
        public string Symptom => Stress >= 85 ? "Aşırı Panik / Terleme" : Stress >= 60 ? "Göz Kaçırma / Titreme" : Stress >= 35 ? "Tedirgin / Savunmacı" : "Sakin & Kontrollü";
    }

    public class HelperMessage { public string Text, Speaker; public float StartTime; public const float CharsPerSecond = 55f; }

    /// <summary>
    /// Oyunun beyni — wwwroot/js/app.js akışının 3D karşılığı. Backend kapalıysa aynı kurallarla yerel modda oynanır.
    /// </summary>
    public partial class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GamePhase Phase { get; private set; } = GamePhase.Splash;
        public PlayerController Player { get; private set; }
        public TownGenerator Town { get; private set; }
        public GameAPIManager Api { get; private set; }
        public GameUI UI { get; private set; }
        public CinematicDirector Director { get; private set; }
        public BuildingDef CurrentBuilding { get; private set; }

        // Otopsi (checkAutopsyConditions / start60SecAutopsyCountdown)
        public bool AutopsyTimerStarted { get; private set; }
        public bool AutopsyReady { get; private set; }
        public float AutopsyTimeLeft { get; private set; } = GameData.AutopsyCountdown;
        public string AutopsyReport { get; private set; }

        public int LabCount { get; private set; }
        public int GuiltyId { get; private set; }
        public string DetectiveNotes = "";

        // Sonuç ekranı
        public bool ResultWin { get; private set; }
        public string ResultTitle { get; private set; }
        public string ResultVerdict { get; private set; }
        public string ResultStory { get; private set; }
        public string ResultButton { get; private set; }
        public string UnlockedMessage { get; private set; }

        // =================================================================
        //  LEVEL SİSTEMİ (web: markTownSolved — Gizemli 1 → Gölgeşehir 2 → Sisören 3)
        // =================================================================
        public bool UnlockAllForTesting { get; set; }
        public bool IsSolved(string townId) => PlayerPrefs.GetInt("dedektif_solved_" + townId, 0) == 1;
        public bool IsTownUnlocked(RegionDef r)
        {
            if (r.Level <= 0) return false;
            if (r.Level == 1 || UnlockAllForTesting) return true;
            var prev = GameData.Regions.Find(x => x.Level == r.Level - 1);
            return prev != null && IsSolved(prev.Id);
        }
        void MarkSolved(string townId)
        {
            bool first = !IsSolved(townId);
            PlayerPrefs.SetInt("dedektif_solved_" + townId, 1);
            PlayerPrefs.Save();
            var cur = GameData.Regions.Find(x => x.Id == townId);
            var next = cur != null ? GameData.Regions.Find(x => x.Level == cur.Level + 1) : null;
            UnlockedMessage = !first ? null : next != null && next.Level > 0
                ? $"YENİ KASABA AÇILDI: Level {next.Level} — {next.Name}"
                : "Üç kasabanın da dosyası kapandı. Anadolu bu gece biraz daha rahat uyuyacak, Amirim.";
        }
        public void ResetProgress()
        {
            foreach (var r in GameData.Regions) PlayerPrefs.DeleteKey("dedektif_solved_" + r.Id);
            PlayerPrefs.Save();
            SaveSystem.DeleteAll();
        }

        readonly Dictionary<int, ClueRuntime> _clues = new Dictionary<int, ClueRuntime>();
        readonly Dictionary<int, NpcRuntime> _npcs = new Dictionary<int, NpcRuntime>();
        readonly HashSet<int> _visited = new HashSet<int>();
        readonly HashSet<int> _innocent = new HashSet<int>();
        readonly List<int> _bagOrder = new List<int>();
        readonly HashSet<string> _shownHelperContexts = new HashSet<string>();
        public readonly List<HelperMessage> HelperHistory = new List<HelperMessage>();

        GameObject _rain;
        public GameObject RainObject => _rain;
        LightningController _lightning;
        readonly List<HidingSpotInteractable> _spots = new List<HidingSpotInteractable>();
        public DetectiveVision Vision { get; private set; }
        public SceneSearch Search { get; private set; }
        int _caseToken; // eski gecikmeli çağrıları iptal etmek için

        public IEnumerable<ClueRuntime> Clues => _clues.Values.OrderBy(c => c.Id);
        public IEnumerable<NpcRuntime> Npcs => _npcs.Values.OrderBy(n => n.Id);
        public IEnumerable<ClueRuntime> Bag => _bagOrder.Select(id => _clues[id]);
        public int BagCount => _bagOrder.Count;
        public int VisitedCount => _visited.Count;
        public ClueRuntime GetClue(int id) => _clues.TryGetValue(id, out var c) ? c : null;
        public NpcRuntime GetNpc(int id) => _npcs.TryGetValue(id, out var n) ? n : null;
        public bool IsVisited(int npcId) => _visited.Contains(npcId);
        public bool IsInnocent(int npcId) => _innocent.Contains(npcId);
        public bool InsideBuilding => CurrentBuilding != null;

        public string AutopsyStatus
        {
            get
            {
                if (AutopsyReady) return "✓ OTOPSİ RAPORU HAZIR! [R]";
                if (AutopsyTimerStarted) { int s = Mathf.CeilToInt(AutopsyTimeLeft); return $"OTOPSİ: {s / 60:00}:{s % 60:00}"; }
                return $"OTOPSİ: BİNA {VisitedCount}/{GameData.ReqBuildings} | LAB {LabCount}/{GameData.ReqLabs}";
            }
        }

        public string LabStatus => LabCount == 0 ? $"0/{GameData.ReqLabs} LAB GEREKLİ" : LabCount < GameData.ReqLabs ? $"{LabCount}/{GameData.ReqLabs} LAB GÖNDERİLDİ" : $"✓ {LabCount} LAB GÖNDERİLDİ";

        // =================================================================
        //  KURULUM
        // =================================================================
        public void Initialize(TownGenerator town, PlayerController player, GameAPIManager api, GameUI ui, LightningController lightning)
        {
            Instance = this;
            Town = town; Player = player; Api = api; UI = ui; _lightning = lightning;

            RebuildRuntime();
            WireInteractables();
            Vision = gameObject.AddComponent<DetectiveVision>();
            Search = gameObject.AddComponent<SceneSearch>();
            Search.Init(this);
            DayNightCycle.Instance?.Rescan(Town.transform);

            if (Player.Cam != null)
            {
                _rain = Effects.Rain(Player.Cam.transform);
                if (_rain != null) _rain.transform.SetParent(Town.transform, true);
                Director = Player.Cam.gameObject.AddComponent<CinematicDirector>();
                Director.Init(Player.Cam.GetComponent<Camera>(), Player.Cam, Town.CrimeScene + Vector3.up * 3f);
            }

            Api.ConnectionChanged += online => UI.Toast(online ? "Sunucu bağlantısı kuruldu." : "Sunucuya ulaşılamıyor — yerel modda devam ediliyor.");
            Api.CheckConnection(_ => { });
            GoToSplash();
        }

        void RebuildRuntime()
        {
            _npcs.Clear(); _clues.Clear(); _bagOrder.Clear();
            foreach (var n in GameData.Npcs) _npcs[n.Id] = new NpcRuntime { Def = n };
            foreach (var c in GameData.Clues) _clues[c.Id] = new ClueRuntime { Def = c };
        }

        /// <summary>Bölge haritasında seçilen kasabanın 3D dünyasını (binalar, iç mekanlar, NPC'ler, deliller) yükler.</summary>
        void LoadTown(string townId)
        {
            Api.townId = townId;
            if (Town.Data != null && Town.Data.Id == townId) return;
            GameData.SetCurrent(townId);
            Town.Generate(GameData.Current);
            RebuildRuntime();
            WireInteractables();
            GameBootstrap.ApplyAtmosphere(GameData.Current);
            DayNightCycle.Instance?.Rescan(Town.transform);
            _lightning?.Rebase();
            Director?.Init(Player.Cam.GetComponent<Camera>(), Player.Cam, Town.CrimeScene + Vector3.up * 3f);
            Player.Teleport(Town.PlayerSpawn, Town.PlayerSpawnRotation);
            Debug.Log($"[Dedektif3D] Level {GameData.Current.Level}: {GameData.Current.Name} yüklendi — {Town.Buildings.Count} bina, {GameData.Npcs.Count} şüpheli, {GameData.Clues.Count} delil.");
        }

        void WireInteractables()
        {
            _spots.Clear();
            foreach (var b in Town.Buildings.Values)
            {
                var door = new GameObject("Kapı_" + b.Def.Key);
                door.transform.SetParent(b.Root, true);
                door.transform.position = b.DoorWorldPos;
                var di = door.AddComponent<DoorInteractable>();
                di.building = b.Def; di.radius = 2.8f;
                if (b.StatusLabel != null) b.StatusLabel.gameObject.AddComponent<Billboard>();
            }
            foreach (var inst in Town.Interiors.Values)
            {
                var exit = new GameObject("ÇıkışKapısı");
                exit.transform.SetParent(inst.Root, true);
                exit.transform.position = inst.ExitPoint;
                var ei = exit.AddComponent<DoorInteractable>();
                ei.building = inst.Def; ei.isExit = true; ei.radius = 2f;

                var ni = inst.Npc.Root.gameObject.AddComponent<NpcInteractable>();
                ni.npcId = inst.Def.NpcId; ni.rig = inst.Npc; ni.radius = 3.2f; ni.offset = Vector3.up * 1.2f;

                var inContainer = new HashSet<int>();
                foreach (var spot in inst.Spots)
                {
                    var hs = spot.Root.AddComponent<HidingSpotInteractable>();
                    hs.Setup(spot);
                    _spots.Add(hs);
                    if (spot.Kind != HideKind.InPlace) inContainer.Add(spot.ClueId);
                }
                foreach (var kv in inst.ClueObjects)
                {
                    var ci = kv.Value.AddComponent<ClueInteractable>();
                    var def = GameData.Clue(kv.Key);
                    bool wallMounted = def.Shape == ClueShape.Apron || def.Shape == ClueShape.Lining || def.Shape == ClueShape.Safe;
                    ci.Setup(kv.Key, def.Hidden, !wallMounted && !inContainer.Contains(kv.Key));
                    _clues[kv.Key].View = ci;
                }
            }
            // Ceset: dedektif şeridin içine girip inceleyebilir
            var corpse = new GameObject("Ceset");
            corpse.transform.SetParent(Town.transform, false);
            corpse.transform.position = Town.CrimeScene;
            var ci2 = corpse.AddComponent<CorpseInteractable>(); ci2.radius = 2.2f; ci2.offset = Vector3.up * 0.3f;
            Town.Track(corpse);

            // Yol arkadaşları dedektifin iki yanında, omuz hizasının biraz önünde yürür: omuz üstü kamera (0.68 sağ, 2.7 geri) ile dedektifin arasına girmezler.
            // Çetin her kasabada solda; Rıfat (Gölgeşehir) ve İlyas (Sisören) kendi kasabalarında sağda
            SpawnCompanion("Yardımcı Dedektif Çetin", "dedektif_helper", false, new Vector2(-1.3f, 0.25f));
            var guide = GameData.Current != null ? GameData.Current.LocalHelperName : null;
            if (!string.IsNullOrEmpty(guide))
                SpawnCompanion(guide, GameData.Current.Id == "golge_sehir" ? "bekci_hasan_helper" : "tuccar_ilyas_helper", true, new Vector2(2.1f, 0.35f));
        }

        /// <summary>Dedektifin yanından ayrılmayan bir yol arkadaşı kurar; bir kasabanın parçasıdır, kasaba değişince silinir.</summary>
        void SpawnCompanion(string displayName, string faceKey, bool localGuide, Vector2 slot)
        {
            var rig = PropFactory.BuildPerson(displayName, Town.transform, Town.HelperPosition,
                new Color(0.16f, 0.15f, 0.14f), new Color(0.7f, 0.66f, 0.58f), false, new Color(0.1f, 0.1f, 0.11f), faceKey);
            Town.Track(rig.Root.gameObject);
            var go = rig.Root.gameObject;
            go.layer = 2;   // kamera ve görüş testleri içinden geçer, dedektif gibi
            InteriorGenerator.SetLayerRecursive(go, 2);
            go.AddComponent<CharacterController>();
            var hi = go.AddComponent<HelperInteractable>();
            hi.rig = rig; hi.displayName = displayName; hi.localGuide = localGuide; hi.radius = 1.9f; hi.offset = Vector3.up;
            var companion = go.AddComponent<HelperCompanion>();
            companion.rig = rig; companion.slot = slot;
        }

        int _guideLine;

        /// <summary>Yerel rehberle konuşma: kasabayı anlatır; bina içindeyse şüpheli hakkında bildiğini söyler.</summary>
        public void TalkToLocalGuide(string guide)
        {
            string line;
            if (InsideBuilding && CurrentBuilding != null)
            {
                var npc = GameData.Npc(CurrentBuilding.NpcId);
                line = GameData.Current.Id == "golge_sehir"
                    ? $"{npc.Name}'i yıllardır bilirim Amirim. Burada gözünüzü dört açın; bu taşlar çok şey gördü, insanlar ise pek az şey söyler."
                    : $"{npc.Name} ile çok alışverişim oldu Amirim. Sisin içinde herkes birbirini tanır ama kimse kimseye tam güvenmez. Sorularınızı dikkatli seçin.";
            }
            else
            {
                var lines = GameData.Current.Id == "golge_sehir" ? GuideLinesRifat : GuideLinesIlyas;
                line = lines[_guideLine++ % lines.Length];
            }
            ShowHelper(line, guide);
        }

        static readonly string[] GuideLinesRifat =
        {
            "Kırk yıldır bu taşlardayım Amirim. Gece fenerimi tutmadığım köşe kalmadı; yine de her köşede bir şey saklı.",
            "Çam ormanının kenarındaki patikalara dikkat edin. Bu kasabada kimse o yoldan gündüz geçmez, geceleyin geçeni de kimse görmemiş olur.",
            "Yağmur izleri siler ama çamur tutar. Kapı önlerine, eşiklere bakın; dükkân sahipleri bunu unutur.",
            "Fenerim yanarken yanımdan ayrılmayın. Gölgeşehir'de sokak lambaları herkese yetmez.",
        };

        static readonly string[] GuideLinesIlyas =
        {
            "Bu yamaçta on yıldır ticaret yaparım Amirim. Kim kime borçlu, kim kimden kaçıyor, hepsi benim defterimde bir şekilde yazılı.",
            "Sis kalkmaz buralarda. Teraslar arasındaki taş merdivenleri bilmeyen kaybolur; siz benim peşimden gelin.",
            "Kasabanın iki yüzü var: gündüz selam verenler, gece kapısını sürgüler. Dükkânlara girince kimin neyi sakladığına bakın.",
            "Telgraf hattı buraya haber taşır ama dedikodu ondan önce gelir. Kahvehanede duyduklarınızı hafife almayın.",
        };

        void SetPhase(GamePhase p)
        {
            Phase = p;
            if (p != GamePhase.Playing) Search?.ForceOff();
            UI.OnPhaseChanged(p);
            ApplyInputState();
        }

        void ApplyInputState()
        {
            bool playing = Phase == GamePhase.Playing && !UI.IsModalOpen;
            Player.InputEnabled = playing;
            if (Player.Cam != null) Player.Cam.LookEnabled = playing;
        }

        IEnumerator After(float seconds, Action a)
        {
            int token = _caseToken;
            yield return new WaitForSeconds(seconds);
            if (token == _caseToken) a?.Invoke();
        }
        void Delay(float s, Action a) => StartCoroutine(After(s, a));

        // =================================================================
        //  1. SPLASH → BÖLGE HARİTASI → HİKAYE → KASABAYA İNİŞ
        // =================================================================
        /// <summary>3D bölge haritasını açar: kasaba yağmuru / şimşeği / gün döngüsü duraklar, kamerayı harita sürer.</summary>
        void EnterMap()
        {
            var map = WorldMapDiorama.Instance;
            if (map == null || Player.Cam == null) return;
            if (_rain != null) _rain.SetActive(false);
            if (_lightning != null) _lightning.enabled = false;
            if (DayNightCycle.Instance != null) DayNightCycle.Instance.Suspended = true;
            Director?.PlayExternal();
            map.Show(Player.Cam.GetComponent<Camera>());
            GameBootstrap.ApplyMapAtmosphere();
        }

        /// <summary>Haritadan kasabaya / açılış ekranına dönüş: kasabanın havası ve renk düzeni geri gelir.</summary>
        void ExitMap()
        {
            var map = WorldMapDiorama.Instance;
            if (map == null || !map.Active) return;
            map.Hide();
            if (_rain != null) _rain.SetActive(true);
            if (_lightning != null) _lightning.enabled = true;
            if (DayNightCycle.Instance != null) DayNightCycle.Instance.Suspended = false;
            GameBootstrap.ApplyAtmosphere(GameData.Current);
        }

        void GoToSplash()
        {
            ExitMap();
            DayNightCycle.Instance?.MenuTime();
            Player.SetVisible(false);
            Director?.PlayIdle();
            SetPhase(GamePhase.Splash);
        }

        /// <summary>"DOSYAYI AÇ VE BAŞLA" (start-btn) → Bölge haritası.</summary>
        public void StartFromSplash()
        {
            AudioManager.Instance?.StartRain();
            UI.FadeTransition(null, Color.black, () =>
            {
                SetPhase(GamePhase.WorldMap);
                EnterMap();
                Delay(4.2f, () =>
                {
                    if (Phase != GamePhase.WorldMap) return;
                    if (IsSolved("sisoren")) ShowHelper("Üç kasabanın da karanlığını aydınlattık Amirim. Dosyalar kapandı; yine de istersen birini yeniden açabiliriz, her soruşturmada katil değişiyor.");
                    else if (IsSolved("golge_sehir")) ShowHelper("Harika bir iş çıkardık Amirim! Gizemli Kasaba'yı da Gölgeşehir'i de aydınlattık. Sırada Sisören'in sisli dağları var!");
                    else if (IsSolved("gizemli")) ShowHelper("Tebrikler Amirim! Gizemli Kasaba davasını çözdük ve Gölgeşehir'in kilidini açtık. Hazır olduğunuzda Level 2'ye geçelim!");
                    else TriggerHelper("splash");
                });
            });
        }

        /// <summary>Bölge haritasında "Ana Menü" (world-back-btn).</summary>
        public void BackToSplash() => UI.FadeTransition(null, Color.black, () =>
        {
            LeaveCase();
            GoToSplash();
        });

        /// <summary>Kasabadan menüye dönüş: davayı kaydeder, bekleyen gecikmeli olayları iptal eder.</summary>
        void LeaveCase()
        {
            SaveCaseNow();
            _caseToken++;
            Search?.ForceOff();
            if (InsideBuilding) { CurrentBuilding = null; SetIndoor(false); }
        }

        /// <summary>Bölge haritasında kasaba tıklaması.</summary>
        public void SelectRegion(RegionDef r)
        {
            if (r.Level <= 0)
            {
                UI.MessageBox($"{r.Name} - KİLİTLİ BÖLGE", $"{r.Name} kasabasında yol kapalı ve henüz soruşturma izni verilmedi. Gelecek güncellemelerde açılacaktır!", true);
                return;
            }
            if (!IsTownUnlocked(r))
            {
                var prev = GameData.Regions.Find(x => x.Level == r.Level - 1);
                UI.MessageBox($"LEVEL {r.Level} — {r.Name} KİLİTLİ", $"Bu soruşturmaya başlamak için önce Level {prev?.Level}: {prev?.Name} davasını çözmelisiniz, Amirim.", true);
                return;
            }
            if (SaveSystem.Exists(r.Id))
            {
                UI.Choose($"{r.Name} — YARIM KALAN SORUŞTURMA",
                    "Bu kasabada süren bir soruşturmanız var. Kaldığınız yerden devam edebilir ya da dosyayı baştan açabilirsiniz.",
                    "Kaldığım Yerden Devam", "Yeni Dava Aç",
                    () => ResumeCase(r.Id),
                    () => UI.Confirm("YENİ DAVA", "Mevcut soruşturmanın kaydı silinecek ve bu işlem geri alınamaz. Emin misiniz?", () => StartNewCase(r)));
                return;
            }
            StartNewCase(r);
        }

        void StartNewCase(RegionDef r)
        {
            SaveSystem.Delete(r.Id);
            UI.FadeTransition(null, Color.black, () =>
            {
                ExitMap();
                LoadTown(r.Id);
                InitCase();
                Director?.PlayStory();
                SetPhase(GamePhase.Story);
            });
        }

        /// <summary>"SORUŞTURMAYI BAŞLAT" (story-continue-btn) → kasabaya sinematik iniş.</summary>
        public void FinishStory()
        {
            UI.FadeTransition(null, Color.black, () =>
            {
                CurrentBuilding = null;
                SetIndoor(false);
                Player.Teleport(Town.PlayerSpawn, Town.PlayerSpawnRotation);
                Player.SetVisible(true);
                SetPhase(GamePhase.Descent);
                if (Director != null && Town.IntroPath.Count > 1)
                    Director.PlayDescent(Town.IntroPath, Town.IntroLook, 10f, BeginInvestigation);
                else BeginInvestigation();
            });
        }

        public void SkipDescent() => Director?.Skip();

        int _corpseLook;
        /// <summary>Dedektif cesedi en az bir kez inceledi mi (Defter'in ilk adımı).</summary>
        public bool CorpseInspected => _corpseLook > 0;

        static readonly string[] BuildingThoughts =
        {
            "{0}. {1} burada yaşıyor. Her eşya bir tanık — dikkatle bakmalıyım.",
            "{0}... Kapı arkamdan kapanınca yağmurun sesi kesildi. Şimdi yalnızca bu odanın anlattıkları var.",
            "{0}. {1} beni bekliyor muydu, yoksa birini mi bekliyordu?",
            "İçerisi sıcak ama hava ağır. {1}, bir şey saklayan biri gibi hareket ediyor.",
        };
        static readonly string[] ClueThoughts =
        {
            "{0}. Sıradan bir eşya gibi duruyor — ama burada olmaması gerekiyordu.",
            "{0}... Bunu kim, neden saklamaya çalıştı?",
            "Bir delil daha: {0}. Parçalar yavaş yavaş yerine oturuyor.",
            "{0}. Laboratuvar bunun üzerindeki izleri okuyabilirse, birinin yalanı çöker.",
        };

        /// <summary>Olay yerinde cesedin başına çömelme: sinematik yakın çekim + dedektifin iç sesi.</summary>
        public void InspectCorpse(Vector3 body)
        {
            var me = Player.transform.position; var dir = body - me; dir.y = 0;
            if (dir.sqrMagnitude < 0.01f) dir = Vector3.forward;
            dir.Normalize();
            var side = Vector3.Cross(Vector3.up, dir);
            Director?.FocusShot(body - dir * 1.1f + side * 0.5f + Vector3.up * 0.75f, body + Vector3.up * 0.1f, 1.4f);
            var t = GameData.Current;
            string[] lines =
            {
                $"Örtüyü yavaşça kaldırıyorum. {t.Victim}... Yağmur kanı sulandırmış, ama yaralar hâlâ konuşuyor.",
                $"Burada ne olduysa, bu kasabanın sakinlerinden biri biliyor. Herkes bir şey saklıyor — dükkânları tek tek dolaşmalıyım.",
                $"Ölüm saati ve sebebi için otopsi şart. En az {t.ReqBuildings} binayı inceleyip {t.ReqLabs} delili laboratuvara göndermeliyim.",
            };
            UI.Monologue(lines[_corpseLook % lines.Length]);
            _corpseLook++;
            Api.LogAction("inspect_corpse", 0, t.Victim);
            Delay(7f, EndCinematicShot);
        }

        /// <summary>Delil / sorgu paneli kapanınca kamera oyuncunun omzuna geri döner.</summary>
        public void EndCinematicShot()
        {
            if (Director != null && Director.Current == CinematicDirector.Mode.Focus) Director.Stop();
        }

        void BeginInvestigation()
        {
            if (Phase != GamePhase.Descent) return;
            Director?.Stop();
            SetPhase(GamePhase.Playing);
            Api.LogAction("enter_town", 0, GameData.TownName);
            // Kara film anlatıcısı: kasabaya ilk adım
            string opening;
            switch (GameData.Current.Id)
            {
                case "gizemli": opening = $"Yağmur, kasabanın bütün izlerini silmeye çalışıyor. Ama ben buradayım. {GameData.Current.Victim}'i kim öldürdüyse, şu an bu sokaklardan birinde nefes alıyor."; break;
                case "golge_sehir": opening = $"Gölgeşehir... Fenerler yanıyor ama karanlık yine de kazanıyor. {GameData.Current.Victim}'in düşmanı çoktu; içlerinden biri bu gece cesaretini bulmuş."; break;
                case "sisoren": opening = "Sisören'de sis hiç kalkmazmış. İnsanlar da öyle: yüzlerini göstermiyorlar. Bu kasabanın sırrını sisin içinden çekip çıkaracağım."; break;
                default: opening = "Yeni bir kasaba, yeni bir ölü. Ve her zamanki gibi, herkes bir şey saklıyor."; break;
            }
            Delay(0.6f, () => UI.Monologue(opening, 9f));
            var intro = GameData.Current.Intro;
            if (intro.Count > 0)
            {
                // Web: Bekçi Rıfat / Tüccar İlyas karşılama diyaloğu (introDialogue)
                float t = 1.2f;
                foreach (var line in intro)
                {
                    var l = line;
                    Delay(t, () => ShowHelper(l.Value, l.Key));
                    t += Mathf.Clamp(l.Value.Length / 22f, 4f, 9f);
                }
                Delay(t, () => TriggerHelper("story_end"));
            }
            else
                Delay(1.2f, () =>
                {
                    TriggerHelper("story_end");
                    Delay(10f, () => { if (Phase == GamePhase.Playing && !InsideBuilding) TriggerHelper("map_enter"); });
                });
        }

        /// <summary>Kasabadan bölge haritasına dönüş (exit-game-btn, onaylı).</summary>
        public void RequestExitToWorldMap()
        {
            UI.Confirm("BÖLGE HARİTASINA DÖNÜŞ",
                $"Bölge haritasına dönmek istediğinize emin misiniz? Soruşturmanız kaydedilecek; {GameData.TownName} kasabasını tekrar seçtiğinizde kaldığınız yerden devam edebilirsiniz.",
                () => UI.FadeTransition(null, Color.black, ReturnToWorldMap));
        }

        void ReturnToWorldMap()
        {
            LeaveCase();
            DayNightCycle.Instance?.MenuTime();
            Player.SetVisible(false);
            Director?.PlayIdle();
            SetPhase(GamePhase.WorldMap);
            EnterMap();
        }

        /// <summary>initGame(): davayı sıfırla, sunucudan suçluyu al, oturum başlat.</summary>
        void InitCase()
        {
            ResetCaseState();
            var ids = GameData.Npcs.Select(x => x.Id).ToList();
            GuiltyId = ids.Count > 0 ? ids[UnityEngine.Random.Range(0, ids.Count)] : 1;
            Api.ResetTown(r =>
            {
                if (r != null && GetNpc(r.guiltyNpcId) != null) GuiltyId = r.guiltyNpcId;
                Api.StartSession(GuiltyId);
            }, _ => { });
        }

        /// <summary>Davanın bütün çalışma zamanı durumunu temizler (yeni dava ve kayıttan devam ortak kullanır).</summary>
        void ResetCaseState()
        {
            _caseToken++;
            _caseClosed = false;
            _resumed = false;
            _saveDirty = false;
            _corpseLook = 0;
            CurrentBuilding = null;
            foreach (var c in _clues.Values)
            {
                c.InBag = false; c.BloodFound = c.PrintFound = c.BloodChecked = c.PrintChecked = c.BloodSent = c.PrintSent = false;
                c.View?.ResetClue();
            }
            foreach (var s in _spots) if (s != null) s.ResetSpot();
            DayNightCycle.Instance?.ResetDay();
            foreach (var n in _npcs.Values)
            {
                n.Stress = 0; n.Asked = 0; n.Waiting = false; n.WokenUp = false; n.Log.Clear(); n.AskedQuestions.Clear(); n.Suggestions.Clear();
            }
            _bagOrder.Clear(); _visited.Clear(); _innocent.Clear(); _shownHelperContexts.Clear(); HelperHistory.Clear();
            LabCount = 0; DetectiveNotes = "";
            AutopsyTimerStarted = false; AutopsyReady = false; AutopsyTimeLeft = GameData.AutopsyCountdown; AutopsyReport = null;
            foreach (var b in Town.Buildings.Values) if (b.StatusLabel != null) b.StatusLabel.text = "";
        }

        // =================================================================
        //  2. BİNALAR (kapı girişi, iç mekan, çıkış uyarısı, kilit)
        // =================================================================
        public void OpenDoorEntry(BuildingDef def)
        {
            if (IsVisited(def.NpcId)) { UI.Toast($"{def.DisplayName}: İnceleme tamamlandı. Bu binaya tekrar giremezsiniz."); return; }
            UI.ShowDoorEntry(def);
        }

        /// <summary>Kapı sahnesinde "KAPIYI ÇAL / İÇERİ GİR".</summary>
        public void EnterBuilding(BuildingDef def)
        {
            if (!Town.Interiors.TryGetValue(def.NpcId, out var inst)) return;
            AudioManager.Instance?.DoorOpen();
            UI.FadeTransition(def.DisplayName, def.Neon, () =>
            {
                CurrentBuilding = def;
                Player.Teleport(inst.Spawn.position, inst.Spawn.rotation);
                SetIndoor(true);
                Search?.OnEnterRoom();
                MarkDirty();
                AudioManager.Instance?.DoorClose();
                Api.LogAction("enter_building", def.NpcId, def.WebName);
                // İnsan simülasyonu: gece yarısı kapı çalınırsa şüpheli uykudan uyandırılmıştır — daha gergin ve huysuz
                var dn = DayNightCycle.Instance;
                var npc = GetNpc(def.NpcId);
                if (dn != null && dn.DeepNight && npc != null && !npc.WokenUp)
                {
                    npc.WokenUp = true;
                    npc.Stress = Mathf.Min(95, npc.Stress + 12);
                    UI.Toast($"Saat {dn.Clock}. Kapıyı uzun uzun çaldın; {npc.Def.Name} uykulu gözlerle, gaz lambasıyla kapıyı açtı. Pek memnun değil.");
                }
                else if (dn != null && dn.HourInt >= 6 && dn.HourInt < 9 && npc != null)
                    UI.Toast($"Sabahın erken saati — {npc.Def.Name} dükkânını yeni açıyor.");
                Delay(1.5f, () => TriggerHelper("building_enter", def.WebName, false));
                if (npc != null) Delay(0.7f, () => UI.Monologue(string.Format(BuildingThoughts[(def.NpcId + VisitedCount) % BuildingThoughts.Length], def.DisplayName, npc.Def.Name)));
            });
        }

        void SetIndoor(bool indoor)
        {
            if (_rain != null) _rain.SetActive(!indoor);
            RenderSettings.fogDensity = indoor ? GameBootstrap.IndoorFog : GameBootstrap.CurrentOutdoorFog;
            if (_lightning != null) _lightning.Indoor = indoor;
            AudioManager.Instance?.SetIndoor(indoor);
        }

        /// <summary>leave-building-btn: uyarı modalı (alınmamış delil / konuşulmamış şüpheli).</summary>
        public void RequestExitBuilding()
        {
            var def = CurrentBuilding;
            if (def == null) return;
            int uncollected = _clues.Values.Count(c => c.Def.BuildingNpcId == def.NpcId && !c.InBag && c.View != null && c.View.revealed);
            int stillHidden = _clues.Values.Count(c => c.Def.BuildingNpcId == def.NpcId && c.View != null && !c.View.revealed);
            bool talked = GetNpc(def.NpcId)?.Log.Count > 0;
            string text = "Bu binadan çıkarsanız inceleme tamamlanacak ve kapı kilitlenecektir.";
            if (uncollected > 0) text += $"\n\n<color=#c0392b><b>(!) Olay yerinde henüz almadığınız {uncollected} delil bulunuyor!</b></color>";
            if (stillHidden > 0) text += $"\n<color=#d4a65a><b>(?) Sezgileriniz bu odada hâlâ saklanmış bir şey olduğunu söylüyor... [Q] Dedektif Gözü</b></color>";
            if (!talked) text += "\n<color=#f59e0b><b>(!) Bu şüpheli ile henüz hiç konuşmadınız!</b></color>";
            UI.ShowExitWarning(text);
        }

        public void ConfirmExitBuilding()
        {
            var def = CurrentBuilding;
            if (def == null || !Town.Buildings.TryGetValue(def.NpcId, out var b)) return;
            _visited.Add(def.NpcId);
            if (b.StatusLabel != null && !IsInnocent(def.NpcId)) b.StatusLabel.text = "İNCELEME TAMAMLANDI";
            AudioManager.Instance?.DoorOpen();
            UI.FadeTransition(GameData.TownName, new Color(0.27f, 0.95f, 1f), () =>
            {
                CurrentBuilding = null;
                Search?.OnLeaveRoom();
                Player.Teleport(b.DoorOutside.position, b.DoorOutside.rotation);
                SetIndoor(false);
                CheckAutopsyConditions();
                MarkDirty();
            });
        }

        // =================================================================
        //  3. DELİLLER (bina içi kart, çanta, adli laboratuvar)
        // =================================================================
        public void InspectClueInBuilding(int id)
        {
            var c = GetClue(id);
            if (c == null) return;
            UI.ShowBuildingClue(c);
            // Film sahnesi: kamera delile yakın plan iner (letterbox + alan derinliği)
            if (c.View != null && Director != null)
            {
                bool onStand = c.View.transform.Find("Sehpa") != null;   // kırsal delil sehpası; saklama yerindekiler sehpasız
                var target = c.View.transform.position + Vector3.up * (onStand ? 0.9f : c.Def.Shape == ClueShape.Card ? 0.1f : 0.25f);
                var from = Player.transform.position + Vector3.up * 1.4f;
                var dir = (target - from); dir.y = 0;
                var camPos = target - dir.normalized * 1.15f + Vector3.up * 0.55f + Vector3.Cross(Vector3.up, dir.normalized) * 0.35f;
                Director.FocusShot(camPos, target, 0.9f);
            }
            Api.LogAction("inspect_clue", id, c.Def.Name);
        }

        public void RevealHiddenClue(int id, string where = null)
        {
            var c = GetClue(id);
            if (c?.View == null || c.View.revealed) return;
            c.View.SetRevealed(true, true);
            MarkDirty();
            UI.Toast(string.IsNullOrEmpty(where) ? $"Gizli bir delil buldun: {c.Def.Name}!" : $"{where} — saklanmış bir delil: {c.Def.Name}!");
            AudioManager.Instance?.Ding();
            Api.LogAction("reveal_hidden_clue", id, c.Def.Name);
        }

        /// <summary>[Q] Dedektif Gözü: yakındaki saklama yerlerini sezer (delili ortaya çıkarmak için o yer açılmalıdır).</summary>
        public void SearchWithMagnifier(Vector3 pos)
        {
            if (Phase != GamePhase.Playing) return;
            if (Vision != null) Vision.Activate();
        }

        /// <summary>building-clue-take-btn — çanta limiti 5 (getMaxBagSize).</summary>
        public void TakeClue(int id)
        {
            var c = GetClue(id);
            if (c == null) return;
            if (c.InBag) { UI.MessageBox("Bilgi", "Bu delil zaten çantanızda bulunuyor.", false); return; }
            if (BagCount >= GameData.MaxBagSize)
            {
                ShowHelper($"Amirim! Çantamız doldu. Maksimum {GameData.MaxBagSize} delil taşıyabiliriz. Önce Adli Tıbba göndermeliyiz!");
                return;
            }
            AddToBag(c);
            UI.Toast($"'{c.Def.Name}' çantaya eklendi.");
            AudioManager.Instance?.Sting(0.45f);
            UI.Monologue(string.Format(ClueThoughts[(id + BagCount) % ClueThoughts.Length], c.Def.Name));
            Api.LogAction("collect_clue", id, c.Def.Name);
            if (BagCount == 1) Delay(0.6f, () => TriggerHelper("bag_open"));
        }

        void AddToBag(ClueRuntime c)
        {
            c.InBag = true;
            if (!_bagOrder.Contains(c.Id)) _bagOrder.Add(c.Id);
            MarkDirty();
            Api.KeepClue(c.Id);
            CheckAutopsyConditions();
        }

        /// <summary>
        /// Çantadan delil çıkarma: çanta dolunca oyuncu takılmasın. Laboratuvara gönderilen bulgular sayılmaya devam eder;
        /// binası açıksa delil odadaki yerine döner, kilitliyse dosyadan çıkar.
        /// </summary>
        public void RemoveFromBag(int id)
        {
            var c = GetClue(id);
            if (c == null || !c.InBag) return;
            c.InBag = false;
            _bagOrder.Remove(id);
            MarkDirty();
            bool locked = IsVisited(c.Def.BuildingNpcId);
            UI.Toast(locked ? $"'{c.Def.Name}' dosyadan çıkarıldı." : $"'{c.Def.Name}' geri bırakıldı; bulunduğu odaya dönünce yine alabilirsin.");
            Api.LogAction("drop_clue", id, c.Def.Name);
        }

        /// <summary>Çantadan delil incele (openClueInspect fromBag) — bina içindeyken laboratuvar incelemesi yapılamaz.</summary>
        public void OpenLabInspect(int id)
        {
            var c = GetClue(id);
            if (c == null) return;
            if (InsideBuilding)
            {
                UI.MessageBox("İnceleme Hatası", "Bina içerisindeyken detaylı laboratuvar incelemesi yapamazsınız! Önce dışarıya (kasabaya) çıkmalısınız.", true);
                ShowHelper("Amirim! Detaylı laboratuvar incelemesi yapmak için binalardan çıkıp kasabaya dönmeliyiz.");
                return;
            }
            UI.ShowLab(c);
            Delay(0.5f, () => TriggerHelper("clue_inspect"));
        }

        /// <summary>UV ışığı / tozlama fırçası sonucu (triggerForensicFinding).</summary>
        public void ReportForensicScan(int id, bool blood)
        {
            var c = GetClue(id);
            if (c == null) return;
            if (blood) { c.BloodChecked = true; c.BloodFound = c.Def.HasBlood; }
            else { c.PrintChecked = true; c.PrintFound = c.Def.HasPrint; }
            MarkDirty();
            AudioManager.Instance?.Ding();
        }

        public static string FindingText(bool blood, bool found) => blood
            ? (found ? "KAN LEKESİ TESPİT EDİLDİ! (Ayıraç koyu maviye döndü; kan örneği ayrıldı)" : "TEMİZ: Kan ayıracı tepki vermedi; nesnede kan lekesi bulunmamaktadır.")
            : (found ? "PARMAK İZİ BULUNDU! (Yüzey üstü daktilografik iz numunesi izole edildi)" : "TEMİZ: Nesne yüzeyinde adli incelemeye elverişli bir parmak izi bulunmamaktadır.");

        /// <summary>send-blood-btn / send-fingerprint-btn → /api/game/forensic/submit, lab sayacı.</summary>
        public void SendToForensics(int id, bool blood)
        {
            var c = GetClue(id);
            if (c == null) return;
            if (blood ? (c.BloodSent || !c.BloodFound) : (c.PrintSent || !c.PrintFound)) return;
            if (blood) c.BloodSent = true; else c.PrintSent = true;
            LabCount++;
            MarkDirty();
            Api.SubmitForensic(id, c.Def.Name, FindingText(blood, true));
            ShowHelper($"Harika Amirim! '{c.Def.Name}' üzerindeki {(blood ? "KAN LEKESİ" : "PARMAK İZİ")} bulgusunu Adli Tıp Merkezi'ne ilettim. Otopsi raporuna yeni detaylar eklendi!");
            CheckAutopsyConditions();
        }

        // =================================================================
        //  4. OTOPSİ (3 bina + 3 lab → 60 sn sayaç → rapor)
        // =================================================================
        void CheckAutopsyConditions()
        {
            if (AutopsyReady || AutopsyTimerStarted) return;
            if (VisitedCount >= GameData.ReqBuildings && LabCount >= GameData.ReqLabs)
            {
                AutopsyTimerStarted = true;
                AutopsyTimeLeft = GameData.AutopsyCountdown;
                UI.MessageBox("BİLGİ", $"OTOPSİ RAPORU HAZIRLANIYOR!\n\nEn az {GameData.ReqBuildings} bina incelendi ve {GameData.ReqLabs} delil adli tıbba gönderildi. Adli Tıp Kurumu otopsi raporunu hazırlamaya başladı. Rapor 60 saniye içinde hazır olacak.", false);
                ShowHelper($"Amirim! {GameData.ReqBuildings} bina incelemesi ve {GameData.ReqLabs} lab gönderimi tamamlandı. Adli Tıp otopsi raporu hazırlanıyor, ekrandaki 60 saniyelik sayacı takip edebilirsiniz!");
            }
        }

        void Update()
        {
            if (Phase == GamePhase.Playing || Phase == GamePhase.Descent) ApplyInputState();
            if (Phase != GamePhase.Playing) return;
            if (AutopsyTimerStarted && !AutopsyReady)
            {
                AutopsyTimeLeft -= Time.deltaTime;
                if (AutopsyTimeLeft <= 0f) { AutopsyTimeLeft = 0f; FinishAutopsy(); MarkDirty(); }
            }
            TickAutosave();
        }

        void FinishAutopsy()
        {
            AutopsyReady = true;
            AutopsyReport = "Rapor Adli Tıp Merkezi'nden alınıyor...";
            UI.MessageBox("YENİ BİLGİ!", "Adli Tıp Merkezi'nden Otopsi Raporu geldi! [R] tuşuna ya da üstteki otopsi butonuna basarak inceleyebilirsiniz.", false);
            TriggerHelper("autopsy_ready", null, false);
            UI.Monologue("Rapor geldi. Ölüler yalan söylemez; yalan söyleyen, yaşayanlardır.");
            if (Api.IsOnline)
                Api.GetAutopsy(r => AutopsyReport = r != null && r.success && !string.IsNullOrEmpty(r.report) ? StripHtml(r.report) : OfflineAutopsy(), _ => AutopsyReport = OfflineAutopsy());
            else AutopsyReport = OfflineAutopsy();
        }

        string OfflineAutopsy()
        {
            var g = GetNpc(GuiltyId);
            string cause;
            switch (GuiltyId)
            {
                case 1: cause = "Boyun bölgesinde ağır, tırtıklı kesici alet yarası. Tırnak altında siyah deri parçacıkları."; break;
                case 2: cause = "Kanda bitkisel kökenli kardiyotoksik madde ve yüksek doz sakinleştirici. Kalp krizi benzeri tablo."; break;
                case 3: cause = "Şakakta ağır künt travma, beyin kanaması. Olay yerinde kırık gözlük camı parçaları."; break;
                case 4: cause = "Yakın mesafeden uygulanan şiddet. Elinde kopmuş pahalı palto düğmesi izi."; break;
                default: cause = "Boğulma bulguları. Ceket astarında yırtılmış gizli cep, ince iplik lifleri."; break;
            }
            if (GameData.TownId != "gizemli") cause = GameData.AutopsyFinding(GuiltyId) ?? "Ağır darbe ve boğuşma izleri. Tırnak altında yabancı kumaş lifleri, giysilerde çamur ve çam iğneleri.";
            string hour = GameData.TownId == "golge_sehir" ? "Gece 01:30 – 02:30 arası." : "Gece 23:30 – 00:30 arası.";
            return $"ADLİ TIP OTOPSİ VAKA DOSYASI — Maktul: {GameData.Current.Victim}\n\n" +
                   $"Ölüm saati: {hour}\n\n" +
                   $"Bulgular: {cause}\n\n" +
                   "Hekimin notu: Bulgular, topladığınız delillerle ve şüphelilerin anlattıklarıyla karşılaştırılmalıdır.\n\n" +
                   $"Adli laboratuvara gönderilen bulgu sayısı: {LabCount}";
        }

        static string StripHtml(string html)
        {
            var s = Regex.Replace(html, @"<(br|BR)\s*/?>|</(p|div|h\d|li|tr)>", "\n");
            s = Regex.Replace(s, "<[^>]+>", "");
            s = System.Net.WebUtility.HtmlDecode(s);
            s = Regex.Replace(s, @"[ \t]+", " ");
            s = Regex.Replace(s, @"\n\s*\n\s*\n+", "\n\n");
            return s.Trim();
        }

        // =================================================================
        //  5. SORGU (5 soru hakkı, kategori sırası, stres, ifadeden delil düşmesi, serbest soru)
        // =================================================================
        public void OpenInterrogation(int npcId)
        {
            var n = GetNpc(npcId);
            if (n == null) return;
            if (IsInnocent(npcId))
            {
                UI.MessageBox("MASUM KARAKTER", $"{n.Def.Name} masum ilan edilmiştir ve dava dosyasından çıkarılmıştır. Kendisiyle tekrar sorgu yapılamaz!", true);
                return;
            }
            UI.OpenInterrogation(n);
            // Film sahnesi: dedektifin omzunun üstünden şüphelinin yüzüne (over-the-shoulder)
            if (Director != null && InsideBuilding && Town.Interiors.TryGetValue(npcId, out var inst))
            {
                // Noir sorgu yakın planı: kamera tezgahın üstünden şüphelinin portre yüzüne, hafif alt açıdan
                var face = inst.Npc.Head.position;
                var me = Player.transform.position + Vector3.up * 1.6f;
                var dir = face - me; dir.y = 0; dir.Normalize();
                var side = Vector3.Cross(Vector3.up, dir);
                Player.transform.rotation = Quaternion.LookRotation(dir);
                inst.Npc.Root.rotation = Quaternion.LookRotation(-dir);
                var camPos = face - dir * 1.35f + side * 0.42f + Vector3.down * 0.06f;
                Director.FocusShot(camPos, face + Vector3.down * 0.02f, 1.1f);
            }
            LoadQuestions(n);
            Delay(0.8f, () => TriggerHelper("npc_talk", null, true));
            Api.LogAction("ask_question", npcId, n.Def.Name);
        }

        /// <summary>loadContextualQuestions: soru sayısına göre kategori (tanışma → derinleşme → yüzleştirme → baskı → son), 4 seçenek.</summary>
        void LoadQuestions(NpcRuntime n)
        {
            n.Suggestions = new List<WebQuestion>();
            if (n.LeftInterrogation || n.Finished) return;
            string category = GameData.Categories[Mathf.Clamp(n.Asked, 0, GameData.Categories.Length - 1)];
            var local = LocalQuestions(n, category);
            n.Suggestions = local;
            if (!Api.IsOnline) return;
            Api.GetDialogues(n.Id, category, r =>
            {
                if (r?.dialogues == null || r.dialogues.Length == 0 || n.Finished || n.LeftInterrogation) return;
                var pool = GameData.QuestionsOf(n.Id).ToList();
                var list = new List<WebQuestion>();
                foreach (var d in r.dialogues)
                {
                    if (string.IsNullOrEmpty(d.q) || n.AskedQuestions.Contains(d.q)) continue;
                    var match = pool.Find(p => p.Q == d.q);
                    list.Add(match ?? new WebQuestion { NpcId = n.Id, Q = d.q, A = d.a, Category = d.category ?? category, Difficulty = Mathf.Max(1, d.difficulty), RelatedClues = new int[0] });
                }
                if (list.Count > 0) n.Suggestions = EnsureFour(list, n);
            });
        }

        List<WebQuestion> LocalQuestions(NpcRuntime n, string category)
        {
            var pool = GameData.QuestionsOf(n.Id).Where(q => !n.AskedQuestions.Contains(q.Q)).ToList();
            var inCat = pool.Where(q => q.Category == category).OrderBy(_ => UnityEngine.Random.value).Take(4).ToList();
            return EnsureFour(inCat, n);
        }

        List<WebQuestion> EnsureFour(List<WebQuestion> list, NpcRuntime n)
        {
            if (list.Count >= 4) return list.Take(4).ToList();
            var extra = GameData.QuestionsOf(n.Id).Where(q => !n.AskedQuestions.Contains(q.Q) && !list.Contains(q)).OrderBy(q => q.Difficulty).Take(4 - list.Count);
            return list.Concat(extra).ToList();
        }

        /// <summary>askQuestionBackend: hazır soru butonu.</summary>
        public void AskQuestion(int npcId, WebQuestion q)
        {
            var n = GetNpc(npcId);
            if (n == null || q == null || n.Waiting || n.Finished || n.LeftInterrogation) return;
            n.Asked++;
            n.AskedQuestions.Add(q.Q);
            n.Suggestions = new List<WebQuestion>();
            n.Log.Add(new DialogueLine { FromPlayer = true, Speaker = "Dedektif", Text = q.Q });
            n.Waiting = true;
            int askedIndex = n.Asked - 1;
            if (q.HasLocalAnswer) Delay(0.8f, () => Reply(q.AnswerFor(GuiltyId)));
            else if (Api.IsOnline)
                Api.Interrogate(n.Id, q.Q, r => Reply(!string.IsNullOrWhiteSpace(r?.dialogue) ? r.dialogue : GameData.OfflineAnswer(n.Def, q.Q, n.Id == GuiltyId, askedIndex)),
                    _ => Reply(GameData.OfflineAnswer(n.Def, q.Q, n.Id == GuiltyId, askedIndex)));
            else Delay(0.8f, () => Reply(GameData.OfflineAnswer(n.Def, q.Q, n.Id == GuiltyId, askedIndex)));

            void Reply(string answer)
            {
                if (string.IsNullOrWhiteSpace(answer)) answer = "Söyleyecek bir şeyim yok amirim.";
                var line = new DialogueLine { Speaker = n.Def.Name, Text = answer, StartTime = Time.time };
                n.Log.Add(line);
                n.Waiting = false;
                Delay(answer.Length / DialogueLine.CharsPerSecond + 0.2f, () =>
                {
                    if (q.RelatedClues != null && q.RelatedClues.Length > 0) DropClues(n.Id, q.RelatedClues);
                    int stress = q.Category == "baski" || q.Difficulty >= 4 ? 25 : q.Category == "yuzlestirme" || q.Difficulty == 3 ? 15 : q.Category == "derinlesme" || q.Difficulty == 2 ? 8 : 3;
                    if (GuiltyId == n.Id) stress = Mathf.RoundToInt(stress * 1.4f);
                    n.Stress = Mathf.Min(100, n.Stress + stress);
                    MarkDirty();
                    Api.LogAction("dialog", n.Id, q.Q);
                    LoadQuestions(n);
                });
            }
        }

        /// <summary>askFreeAiQuestion: serbest Türkçe soru → yapay zekâ sorgusu (backend), yoksa karakterin yedek cevabı.</summary>
        public void AskFree(int npcId, string text)
        {
            var n = GetNpc(npcId);
            if (n == null || string.IsNullOrWhiteSpace(text) || n.Waiting || n.LeftInterrogation) return;
            if (n.Finished) { UI.MessageBox("Uyarı", $"Bu NPC ile konuşma hakkınız doldu ({GameData.MaxQuestions}/{GameData.MaxQuestions}). Artık soru soramazsınız!", true); return; }
            text = text.Trim();
            n.Asked++;
            n.Suggestions = new List<WebQuestion>();
            n.Log.Add(new DialogueLine { FromPlayer = true, Speaker = "Dedektif", Text = text });
            n.Waiting = true;

            void Answer(string ans, int stressInc)
            {
                if (string.IsNullOrWhiteSpace(ans)) ans = GameData.AnswerFree(n.Def, text, GuiltyId);
                n.Log.Add(new DialogueLine { Speaker = n.Def.Name, Text = ans, StartTime = Time.time });
                n.Waiting = false;
                Delay(ans.Length / DialogueLine.CharsPerSecond + 0.2f, () =>
                {
                    n.Stress = Mathf.Clamp(n.Stress + stressInc, 0, 100);
                    MarkDirty();
                    LoadQuestions(n);
                });
            }

            if (Api.IsOnline)
                Api.Interrogate(n.Id, text, r => Answer(r?.dialogue, r != null ? Mathf.Max(3, r.stressIncrease) : 5), _ => Answer(null, 3));
            else
                Delay(0.8f, () => Answer(null, 3));
        }

        /// <summary>checkAndDropClues: ifadede geçen deliller çantaya eklenir (yer varsa).</summary>
        void DropClues(int npcId, int[] ids)
        {
            bool any = false;
            foreach (var id in ids)
            {
                if (BagCount >= GameData.MaxBagSize) break;
                var c = GetClue(id);
                if (c == null || c.InBag) continue;
                AddToBag(c);
                c.View?.SetRevealed(true, false);
                any = true;
            }
            if (any)
            {
                UI.Toast("Şüphelinin ifadesi yeni bir delile işaret etti — çantaya eklendi!");
                TriggerHelper("clue_found", null, false);
            }
        }

        // =================================================================
        //  6. YARDIMCI DEDEKTİF ÇETİN (sinematik ipucu kutusu)
        // =================================================================
        public void ShowHelper(string msg, string speaker = null)
        {
            if (string.IsNullOrEmpty(msg)) return;
            HelperHistory.Add(new HelperMessage { Text = msg, Speaker = speaker, StartTime = Time.time });
            UI.ShowCinematicHelper();
        }

        /// <summary>triggerHelperMessage: /api/game/helper/tip, yoksa web'deki yedek mesajlar.</summary>
        public void TriggerHelper(string context, string building = null, bool oneTime = true)
        {
            string key = building != null ? context + "_" + building : context;
            if (oneTime && _shownHelperContexts.Contains(key)) return;
            _shownHelperContexts.Add(key);
            if (context == "clue_found") { ShowHelper("Amirim! Şüphelinin anlattıkları yeni bir delile işaret ediyor. Delili çantaya ekledim, lab'a göndermeyi unutmayın!"); return; }
            if (!Api.IsOnline) { ShowHelper(GameData.HelperMessage(context, building)); return; }
            Api.GetHelperTip(context, building, r =>
            {
                ShowHelper(r != null && r.success && !string.IsNullOrEmpty(r.message) ? r.message : GameData.HelperMessage(context, building));
            }, _ => ShowHelper(GameData.HelperMessage(context, building)));
        }

        /// <summary>provideBuildingSpecificHint — "Başka İpucu Söyle".</summary>
        public void ProvideBuildingHint()
        {
            if (CurrentBuilding == null) { ShowHelper("Amirim, mekan içerisindeki delilleri ve ipuçlarını öğrenmek için binalardan birine girmeliyiz!"); return; }
            var npc = GameData.Npc(CurrentBuilding.NpcId);
            var names = string.Join(", ", GameData.Clues.Where(c => c.BuildingNpcId == npc.Id).Select(c => $"'{c.Name}'"));
            string extra;
            switch (npc.Id)
            {
                case 1: extra = "Tezgaha saplanan Kanlı Satır'daki kan izlerine, Kara Kaplı Defter'e ve Yırtık Önlük'e dikkat et."; break;
                case 2: extra = "Tezgah altındaki Zehirli Sarmaşık'a, Boş İlaç Şişesi'ne ve Reçete Defteri'ne odaklan."; break;
                case 3: extra = "Masadaki Tehdit Mektubu'na, köşedeki Gizli Kasa'ya ve Kırık Gözlük'e dikkat et."; break;
                case 4: extra = "Dolaptaki 'GİZLİ' damgalı dosyaya, Polis Rozeti'ne ve paltonun Kopmuş Düğmesi'ne odaklan."; break;
                case 5: extra = "Kanlı İplik Makarası'na, Yırtık Kumaş parçasına ve ceketin astarındaki Gizli Cep'e dikkat et."; break;
                default: extra = "Tezgahın çevresini, rafları ve karanlık köşeleri dikkatle tara; çekmeceler, sandıklar, tablolar ve gevşek tahtalar bir şey saklıyor olabilir — Dedektif Gözü [Q]!"; break;
            }
            string secret = string.IsNullOrEmpty(npc.Secret) ? "" : $" Şüpheli sırrı: \"{npc.Secret}\"";
            ShowHelper($"Amirim, {npc.Building} binasındayız! Odada incelenebilecek deliller: {names}. {extra}{secret}");
        }

        /// <summary>handleHelperAnalyzeClick — "Delilleri İncele / Deliller Hakkında Ne Düşünüyorsun?".</summary>
        public void AnalyzeBag()
        {
            var ids = _bagOrder.ToList();
            if (ids.Count == 0) { ShowHelper("Amirim, çantanızda henüz delil yok! Binalara girip delilleri toplamalısınız."); return; }
            if (Api.IsOnline)
                Api.AnalyzeClues(ids, r => ShowHelper(r != null && !string.IsNullOrEmpty(r.analysis) ? r.analysis : LocalAnalysis()), _ => ShowHelper(LocalAnalysis()));
            else ShowHelper(LocalAnalysis());
        }

        string LocalAnalysis()
        {
            var byNpc = Bag.GroupBy(c => c.Def.BuildingNpcId).OrderByDescending(g => g.Count()).First();
            int stained = Bag.Count(c => c.BloodFound || c.PrintFound);
            return $"Amirim, çantada {BagCount} delil var; en çok delil {GameData.Npc(byNpc.Key).Name} ile ilişkili görünüyor. " +
                   $"{stained} delilde adli bulgu tespit ettik. Laboratuvara {LabCount}/{GameData.ReqLabs} bulgu gönderdik. Şüphelilerin stres tepkilerini de karşılaştırın!";
        }

        /// <summary>Çetin'e tıklama (reopenHelperSpeechBubble).</summary>
        public void OpenHelperPanel()
        {
            if (InsideBuilding) ProvideBuildingHint();
            else if (BagCount > 0) AnalyzeBag();
            else TriggerHelper("map_enter", null, false);
        }

        // =================================================================
        //  7. SUÇLAMA → HAPİS → SONUÇ
        // =================================================================
        public void OpenAccuse()
        {
            if (Phase != GamePhase.Playing) return;
            UI.ShowFound();
            Delay(0.8f, () => TriggerHelper("accuse"));
            Api.LogAction("accuse_screen_open", 0, null);
        }

        /// <summary>window.innocentNpc — şüpheliyi masum ilan et.</summary>
        public void DeclareInnocent(int npcId)
        {
            var n = GetNpc(npcId);
            if (n == null || IsInnocent(npcId)) return;
            _innocent.Add(npcId);
            MarkDirty();
            if (Town.Buildings.TryGetValue(npcId, out var b) && b.StatusLabel != null) b.StatusLabel.text = $"{n.Def.Name.ToUpperInvariant()} MASUM";
            UI.MessageBox("MASUM İLAN EDİLDİ", $"{n.Def.Name} soruşturma dosyasında masum olarak arşivlendi. Yeni bir vaka açılana kadar bu şüpheli masum kalacaktır ve sorgulanamaz.", false);
            Api.LogAction("mark_innocent", npcId, n.Def.Name);
        }

        /// <summary>window.accuseNpc — hakim tokmağı, hapis sahnesi (3 sn), ardından sonuç.</summary>
        public void Accuse(int npcId)
        {
            var accused = GetNpc(npcId);
            if (accused == null) return;
            _caseClosed = true;
            AudioManager.Instance?.GavelTriple();
            UI.ShowJail(accused);
            int token = _caseToken;

            void Finish(bool correct, int realKillerId)
            {
                if (token != _caseToken) return;
                var killer = GetNpc(realKillerId) ?? GetNpc(GuiltyId);
                ResultWin = correct;
                ResultTitle = correct ? "TEBRİKLER! KAZANDINIZ!" : "KAYBETTİNİZ!";
                ResultVerdict = correct
                    ? $"Sizin Seçiminiz: <b>{accused.Def.Name}</b>\nGerçek Katil: <b>{killer.Def.Name}</b>\n<color=#2ea44f><b>✓ DOĞRU TAHMİN!</b></color>"
                    : $"Sizin Seçiminiz: <b>{accused.Def.Name}</b> <color=#cf222e>(MASUM)</color>\nGerçek Katil: <b>{killer.Def.Name}</b>\n<color=#cf222e><b>✗ YANLIŞ TAHMİN!</b></color>";
                ResultStory = killer.Def.MurderStory;
                ResultButton = correct ? "Haritaya Dön" : "Haritaya Dön ve Tekrar Dene";
                UnlockedMessage = null;
                if (correct) MarkSolved(GameData.TownId);
                SaveSystem.Delete(GameData.TownId);
                Api.EndSession(correct ? "Won" : "Lost", npcId, Npcs.Sum(x => x.Asked), BagCount);
                Delay(GameData.JailSeconds, () => SetPhase(GamePhase.Result));
            }

            if (Api.IsOnline && !_resumed)
                Api.Accuse(npcId, r =>
                {
                    if (r == null) { Finish(npcId == GuiltyId, GuiltyId); return; }
                    int killer = GetNpc(r.guiltyNpcId) != null ? r.guiltyNpcId : GuiltyId;
                    Finish(r.success || npcId == killer, killer);
                }, _ => Finish(npcId == GuiltyId, GuiltyId));
            else Finish(npcId == GuiltyId, GuiltyId);
        }

        /// <summary>result-retry-btn: bölge haritasına dön (kaybedince dava sıfırlanır — web ile aynı).</summary>
        public void ResultContinue() => UI.FadeTransition(null, Color.black, ReturnToWorldMap);
    }

    /// <summary>3D yazının her zaman kameraya dönmesi.</summary>
    public class Billboard : MonoBehaviour
    {
        void LateUpdate()
        {
            var cam = Camera.main;
            if (cam != null) transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
        }
    }
}
