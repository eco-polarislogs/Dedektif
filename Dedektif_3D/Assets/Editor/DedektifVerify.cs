#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Dedektif3D.Core;
using Dedektif3D.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Yayın doğrulaması: batch modda oyunu oynatıp okunabilir delilleri, görsel yüklemeyi, kayıt sistemini,
/// fareyle aramayı ve üç kasabanın baştan sona oynanışını sınar.
/// Çalıştırma: Unity -batchmode -executeMethod DedektifVerify.Run (DEDEKTIF_SHOTS = çıktı klasörü). Unity kapalı olmalıdır.
/// Sonuç: çıktı klasöründe verify.txt; çıkış kodu 0 = hepsi geçti, 1 = başarısız, 2 = zaman aşımı.
/// </summary>
public static class DedektifVerify
{
    static string OutDir => System.Environment.GetEnvironmentVariable("DEDEKTIF_SHOTS") ?? Path.GetFullPath("Temp/verify");

    public static void Run()
    {
        Directory.CreateDirectory(OutDir);
        File.WriteAllText(Path.Combine(OutDir, "verify.txt"), "");
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        EditorApplication.playModeStateChanged += s =>
        {
            if (s == PlayModeStateChange.EnteredPlayMode) new GameObject("VerifyRunner").AddComponent<Runner>();
        };
        EditorApplication.isPlaying = true;
    }

    public class Runner : MonoBehaviour
    {
        const float TimeLimit = 1500f;
        const string Town = "gizemli";

        int _fails, _checks;
        float _started;
        readonly List<string> _errors = new List<string>();

        static void Log(string s)
        {
            Debug.Log("[VERIFY] " + s);
            File.AppendAllText(Path.Combine(OutDir, "verify.txt"), s + "\n");
        }

        void Check(string name, bool ok)
        {
            _checks++;
            if (!ok) _fails++;
            Log((ok ? "GEÇTİ  " : "KALDI  ") + name);
        }

        static WaitForSeconds Wait(float s) => new WaitForSeconds(s);

        void Update()
        {
            if (Time.realtimeSinceStartup - _started < TimeLimit) return;
            Log($"ZAMAN AŞIMI: {TimeLimit} sn içinde bitmedi. Son hatalar: {string.Join(" | ", _errors.Take(5))}");
            EditorApplication.Exit(2);
        }

        IEnumerator Start()
        {
            _started = Time.realtimeSinceStartup;
            Application.logMessageReceived += (msg, _, type) =>
            {
                if (type == LogType.Exception || type == LogType.Error) _errors.Add(msg.Length > 240 ? msg.Substring(0, 240) : msg);
            };
            while (GameManager.Instance == null && Time.realtimeSinceStartup - _started < 90f) yield return null;
            var gm = GameManager.Instance;
            if (gm == null) { Log("KALDI  GameManager oluşmadı"); EditorApplication.Exit(1); yield break; }
            yield return Wait(1f);

            var solvedBefore = GameData.Regions.ToDictionary(r => r.Id, r => PlayerPrefs.GetInt("dedektif_solved_" + r.Id, 0));

            CheckDocuments(gm);
            CheckDialogue();
            yield return CheckImages();
            yield return CheckSaves(gm);

            string saves = Path.Combine(OutDir, "kayit_oynanis");
            if (Directory.Exists(saves)) Directory.Delete(saves, true);
            SaveSystem.UseDirectory(saves);
            gm.UnlockAllForTesting = true;
            Time.timeScale = 3f;
            foreach (var r in GameData.Regions.Where(x => x.Level > 0).OrderBy(x => x.Level))
                yield return PlayTown(gm, r);
            Time.timeScale = 1f;

            foreach (var kv in solvedBefore)
            {
                if (kv.Value == 0) PlayerPrefs.DeleteKey("dedektif_solved_" + kv.Key);
                else PlayerPrefs.SetInt("dedektif_solved_" + kv.Key, kv.Value);
            }
            PlayerPrefs.Save();

            Check($"oyun sırasında hata veya istisna yok ({_errors.Count})", _errors.Count == 0);
            foreach (var e in _errors.Distinct().Take(12)) Log("  HATA: " + e);
            Log($"SONUÇ: {_checks - _fails}/{_checks} geçti");
            EditorApplication.Exit(_fails == 0 ? 0 : 1);
        }

        void CheckDocuments(GameManager gm)
        {
            var docs = GameData.Documents;
            Check($"35 okunabilir belge yüklendi ({docs.Count})", docs.Count == 35);
            Check("hiçbir belge boş değil", docs.Values.All(v => !string.IsNullOrWhiteSpace(v)));
            string all = string.Join("\n", docs.Values);
            Check("belgelerde T.C., 20. yüzyıl tarihi veya Diazepam yok", !Regex.IsMatch(all, @"T\.C\.|\b19\d\d\b|Diazepam"));

            string current = gm.Town.Data.Id;
            int mapped = 0;
            foreach (var r in GameData.Regions.Where(x => x.Level > 0))
            {
                GameData.SetCurrent(r.Id);
                mapped += GameData.Clues.Count(c => c.Document != null);
            }
            GameData.SetCurrent(current);
            Check($"her belge oyundaki bir delile bağlı ({mapped}/35)", mapped == 35);
        }

        /// <summary>Çevrimdışı konuşmalar: her şüphelinin her sorusuna gerçek cevap, katilse farklı cevap ve otopsi bulgusu.</summary>
        void CheckDialogue()
        {
            foreach (var t in GameData.Towns)
            {
                int total = 0, answered = 0, guiltyDiffers = 0;
                foreach (var q in t.Questions)
                {
                    total++;
                    if (q.HasLocalAnswer) answered++;
                    if (q.G != null && q.G != q.A) guiltyDiffers++;
                }
                Check($"{t.Name}: bütün sorular cevaplı ({answered}/{total})", total > 0 && answered == total);
                Check($"{t.Name}: şüphelilerin hikâyesi ve kurban adı tam", t.Npcs.All(n => !string.IsNullOrWhiteSpace(n.MurderStory)) && t.Victim != "Kurban");
                if (t.Id == "gizemli") continue;
                Check($"{t.Name}: katil soruları farklı cevaplıyor ({guiltyDiffers})", guiltyDiffers >= t.Npcs.Count * 8);
                Check($"{t.Name}: her şüpheliye özel otopsi bulgusu var", t.Npcs.All(n => !string.IsNullOrEmpty(GameData.AutopsyFinding(n.Id))));
                string text = string.Join(" ", t.Questions.Select(q => q.A + " " + q.G));
                var era = Regex.Matches(text, @"\b(sinema|polis|telefon|tüpçü|plaka|televizyon|Amirims)\b", RegexOptions.IgnoreCase).Cast<Match>().Select(m => m.Value).Distinct().ToList();
                Check($"{t.Name}: konuşmalarda dönem dışı sözcük yok [{string.Join(",", era)}]", era.Count == 0);
                // Yanıtlardan cinayetin Vahap/Ekrem dışındaki kasabaya sızması
                string wrong = t.Id == "sisoren" ? "Ekrem" : "Vahap";
                Check($"{t.Name}: başka kasabanın maktulü geçmiyor", !text.Contains(wrong));
            }
            var n1 = GameData.Town("golge_sehir").Npcs.First();
            string prev = GameData.Current.Id;
            GameData.SetCurrent("golge_sehir");
            string alibi = GameData.AnswerFree(n1, "o gece neredeydin?", n1.Id + 1);
            string guilty = GameData.AnswerFree(n1, "o gece neredeydin?", n1.Id);
            Check("serbest soru konuya göre cevaplanıyor", alibi.Contains("Kulübemdeydim") && guilty != alibi);
            Check("serbest soru hazır soruyla eşleşiyor", GameData.AnswerFree(n1, "Baltanı nerede kullandın", n1.Id + 1).Contains("kuzey yamaçta"));
            GameData.SetCurrent(prev);
        }

        IEnumerator CheckImages()
        {
            var paths = new HashSet<string>();
            foreach (var name in new[] { "golge_sehir_data", "sisoren_data" })
            {
                var asset = Resources.Load<TextAsset>("Web/" + name);
                Check($"{name} bulundu", asset != null);
                if (asset == null) continue;
                foreach (Match m in Regex.Matches(asset.text.Replace("\\/", "/"), @"images/[^""\\]+?\.(?:jpe?g|png|webp)"))
                    paths.Add(m.Value);
            }
            int ok = 0, bad = 0, pending = paths.Count;
            foreach (var p in paths)
                WebTextures.Load(p, t => { if (t != null) ok++; else bad++; pending--; });
            float t0 = Time.realtimeSinceStartup;
            while (pending > 0 && Time.realtimeSinceStartup - t0 < 120f) yield return null;
            Check($"Gölgeşehir ve Sisören görselleri sunucusuz yüklendi ({ok}/{paths.Count}, hatalı {bad})", paths.Count > 0 && ok == paths.Count);
        }

        IEnumerator CheckSaves(GameManager gm)
        {
            string dir = Path.Combine(OutDir, "kayit_testi");
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
            SaveSystem.UseDirectory(dir);
            string mainFile = Path.Combine(dir, $"dava_{Town}.json");
            var region = GameData.Regions.First(r => r.Id == Town);

            // Yeni dava
            gm.StartFromSplash(); yield return Wait(2f);
            gm.SelectRegion(region); yield return Wait(3f);
            gm.FinishStory(); yield return Wait(2f);
            gm.SkipDescent(); yield return Wait(2.5f);
            Check("yeni dava oynanabilir duruma geldi", gm.Phase == GamePhase.Playing);

            // Önemli olayda otomatik kayıt
            var clueIds = GameData.Clues.Take(2).Select(c => c.Id).ToList();
            SaveSystem.Delete(Town);
            gm.TakeClue(clueIds[0]); yield return Wait(3f);
            Check("delil alınınca otomatik kayıt yazıldı", SaveSystem.Exists(Town));
            gm.TakeClue(clueIds[1]);
            gm.ReportForensicScan(clueIds[0], true);

            int guilty = gm.GuiltyId;
            var others = gm.Npcs.Where(n => n.Id != guilty).ToList();
            int innocentId = others[0].Id;
            gm.DeclareInnocent(innocentId);
            gm.DetectiveNotes = "doğrulama notu";

            var asked = gm.Npcs.First(n => n.Id != innocentId);
            gm.AskQuestion(asked.Id, GameData.QuestionsOf(asked.Id).First());
            yield return Wait(10f);
            int askedCount = asked.Asked, stress = asked.Stress, logCount = asked.Log.Count;
            Check("soru soruldu ve cevap alındı", askedCount == 1 && logCount >= 2);

            var spawnRot = gm.Town.PlayerSpawnRotation;
            gm.Player.Teleport(gm.Town.PlayerSpawn + spawnRot * Vector3.forward * 3f, spawnRot);
            yield return Wait(1f);
            var savedPos = gm.Player.transform.position;
            float savedHour = DayNightCycle.Instance.Hour;
            var savedBag = gm.Bag.Select(c => c.Id).ToList();

            // Menüye dönüp kayıttan devam
            gm.BackToSplash(); yield return Wait(2.5f);
            Check("ana menüye dönüldü", gm.Phase == GamePhase.Splash);
            Check("ana menü kaydı görüyor", SaveSystem.LatestTown() == Town);

            gm.ResumeCase(Town); yield return Wait(2.5f);
            Check("kayıttan doğrudan oyuna girildi", gm.Phase == GamePhase.Playing);
            Check("katil aynı kaldı", gm.GuiltyId == guilty);
            Check($"çanta aynı ve aynı sırada ({savedBag.Count})", gm.Bag.Select(c => c.Id).SequenceEqual(savedBag));
            Check("adli inceleme durumu geri geldi", gm.GetClue(clueIds[0]).BloodChecked);
            Check("masum ilanı geri geldi", gm.IsInnocent(innocentId));
            Check("dedektif notu geri geldi", gm.DetectiveNotes == "doğrulama notu");
            var restored = gm.GetNpc(asked.Id);
            Check("sorgu hakkı ve stres geri geldi", restored.Asked == askedCount && restored.Stress == stress);
            Check("konuşma geçmişi yeniden yazılmadan geri geldi", restored.Log.Count == logCount && restored.Log.All(l => l.Typed));
            var a = gm.Player.transform.position; a.y = 0;
            var b = savedPos; b.y = 0;
            Check($"oyuncu konumu geri geldi ({Vector3.Distance(a, b):0.00} m)", Vector3.Distance(a, b) < 1f);
            Check($"oyun saati geri geldi ({savedHour:0.00} → {DayNightCycle.Instance.Hour:0.00})", Mathf.Abs(DayNightCycle.Instance.Hour - savedHour) < 0.5f);
            if (gm.Town.Buildings.TryGetValue(innocentId, out var innocentBuilding) && innocentBuilding.StatusLabel != null)
                Check("masum tabelası geri geldi", innocentBuilding.StatusLabel.text.Contains("MASUM"));

            // Bina içindeyken kayıt
            var def = gm.Town.Buildings[asked.Id].Def;
            gm.EnterBuilding(def); yield return Wait(2.5f);
            Check("binaya girildi", gm.InsideBuilding);
            var spot = HidingSpotInteractable.All.FirstOrDefault(s => s != null && s.def != null && GameData.Clue(s.def.ClueId).BuildingNpcId == def.NpcId);
            int spotClue = spot != null ? spot.def.ClueId : 0;
            if (spot != null) { spot.Interact(); yield return Wait(1.5f); }

            gm.BackToSplash(); yield return Wait(2.5f);
            Check("menüde bina durumu temizlendi", !gm.InsideBuilding);
            Check("menüde fareyle arama kapandı", !gm.Search.Active);
            gm.ResumeCase(Town); yield return Wait(2.5f);
            Check("içeride kaydedilen oyun aynı binada açıldı", gm.CurrentBuilding != null && gm.CurrentBuilding.NpcId == def.NpcId);
            Check("kayıttan binaya dönünce fareyle arama açıldı", gm.Search.Active);
            var inst = gm.Town.Interiors[def.NpcId];
            Check("oyuncu odanın girişinde başladı", Vector3.Distance(gm.Player.transform.position, inst.Spawn.position) < 1.5f);
            if (spot != null)
            {
                var again = HidingSpotInteractable.All.First(s => s != null && s.def != null && s.def.ClueId == spotClue);
                Check("açılan saklama yeri açık kaldı", again.Opened);
                Check("bulunan gizli delil görünür kaldı", gm.GetClue(spotClue).View.revealed);
            }
            else Log("  BİLGİ: bu binada saklama yeri yok, o adım atlandı");

            gm.ConfirmExitBuilding(); yield return Wait(2f);
            Check("binadan çıkıldı", !gm.InsideBuilding && gm.IsVisited(def.NpcId));
            Check("dışarıda fareyle arama kapandı", !gm.Search.Active);
            gm.BackToSplash(); yield return Wait(2.5f);
            gm.ResumeCase(Town); yield return Wait(2.5f);
            Check("incelenen bina kilitli kaldı", gm.IsVisited(def.NpcId));

            // Elektrik kesintisi benzetimi: asıl dosya yarım kalmış, yedek sağlam
            gm.BackToSplash(); yield return Wait(2.5f);
            string goodJson = File.ReadAllText(mainFile);
            File.WriteAllText(mainFile, goodJson.Substring(0, goodJson.Length / 2));
            var fallback = SaveSystem.Read(Town);
            Check("bozuk kayıt yerine yedek okundu", fallback != null && fallback.guiltyId == guilty);
            File.WriteAllText(mainFile, goodJson);

            // Suçlama kaydı siler ve sonuç ekranından sonra kayıt geri oluşmaz
            gm.ResumeCase(Town); yield return Wait(2.5f);
            gm.Accuse(gm.GuiltyId); yield return Wait(GameData.JailSeconds + 2f);
            Check("doğru suçlama kazandırdı", gm.Phase == GamePhase.Result && gm.ResultWin);
            Check("dava bitince kayıt silindi", !SaveSystem.Exists(Town));
            Check("ana menü artık kayıt görmüyor", SaveSystem.LatestTown() == null);
            yield return Wait(3f);
            Check("dava kapandıktan sonra kayıt geri oluşmadı", !SaveSystem.Exists(Town));

            // Asıl dosya ve yedek bozuksa oyun çökmeden uyarır
            Directory.CreateDirectory(dir);
            File.WriteAllText(mainFile, "{bozuk");
            File.WriteAllText(mainFile + ".bak", "{bozuk");
            Check("iki dosya da bozuksa kayıt okunmaz", SaveSystem.Read(Town) == null);
            gm.ResumeCase(Town); yield return Wait(0.5f);
            Check("okunamayan kayıt temizlendi", !SaveSystem.Exists(Town));
            gm.BackToSplash(); yield return Wait(2.5f);
        }

        // =================================================================
        //  BAŞTAN SONA: harita → hikâye → iniş → olay yeri → binalar → lab → otopsi → suçlama → sonuç
        // =================================================================
        IEnumerator PlayTown(GameManager gm, RegionDef region)
        {
            Log($"--- {region.Name} baştan sona");
            gm.StartFromSplash(); yield return Wait(2f);
            Check($"{region.Name}: harita açıldı", gm.Phase == GamePhase.WorldMap);
            gm.SelectRegion(region); yield return Wait(3f);
            Check($"{region.Name}: kasaba ve hikâye yüklendi", gm.Phase == GamePhase.Story && gm.Town.Data.Id == region.Id);
            Check($"{region.Name}: hikâye metni var", !string.IsNullOrWhiteSpace(GameData.StoryText));
            gm.FinishStory(); yield return Wait(2f);
            Check($"{region.Name}: iniş sinematiği", gm.Phase == GamePhase.Descent);
            gm.SkipDescent(); yield return Wait(2.5f);
            Check($"{region.Name}: soruşturma başladı", gm.Phase == GamePhase.Playing);

            var town = GameData.Current;
            // Olay yeri: fareyle ceset
            gm.Search.SetActive(true);
            var corpse = Interactable.All.OfType<CorpseInteractable>().FirstOrDefault();
            Check($"{region.Name}: ceset fareyle seçilebilir nesne", corpse != null && corpse.Searchable);
            if (corpse != null)
            {
                gm.Search.DebugForceHover(corpse); yield return null; yield return null;
                gm.Search.ClickHovered();
                gm.Search.DebugForceHover(null);
                yield return Wait(8f);
            }
            Check($"{region.Name}: ceset incelendi", gm.CorpseInspected);
            gm.Search.SetActive(false);

            // Binalar: her odada delil seçimi; gerekli sayıda binayı oyna
            var order = gm.Town.Interiors.Values.OrderBy(i => i.Def.NpcId).ToList();
            int need = town.ReqBuildings;
            for (int i = 0; i < order.Count; i++)
                yield return VisitRoom(gm, order[i], i < need, region.Name);

            Check($"{region.Name}: gerekli binalar incelendi ({gm.VisitedCount}/{town.ReqBuildings})", gm.VisitedCount >= town.ReqBuildings);
            Check($"{region.Name}: çantada delil var ({gm.BagCount})", gm.BagCount > 0);

            // Adli laboratuvar (dışarıda)
            foreach (var c in gm.Bag.ToList())
            {
                if (gm.LabCount >= town.ReqLabs) break;
                gm.ReportForensicScan(c.Id, true);
                gm.ReportForensicScan(c.Id, false);
                if (c.BloodFound) gm.SendToForensics(c.Id, true);
                if (c.PrintFound) gm.SendToForensics(c.Id, false);
            }
            yield return null;
            // Çanta dolunca delil değiştirilebilir: yetmediyse çıkarıp başkasını deneriz
            if (gm.LabCount < town.ReqLabs) Log($"  BİLGİ: {region.Name} ilk delillerle {gm.LabCount}/{town.ReqLabs} bulgu");
            Check($"{region.Name}: laboratuvar eşiği aşıldı ({gm.LabCount}/{town.ReqLabs})", gm.LabCount >= town.ReqLabs);

            // Sayaç 60 oyun saniyesi
            float t0 = Time.realtimeSinceStartup;
            while (gm.AutopsyTimerStarted && !gm.AutopsyReady && Time.realtimeSinceStartup - t0 < 60f) yield return null;
            Check($"{region.Name}: otopsi hazır", gm.AutopsyReady);
            string finding = GameData.AutopsyFinding(gm.GuiltyId);
            if (town.Id != "gizemli") Check($"{region.Name}: otopsi katile göre yazıldı", gm.AutopsyReport != null && finding != null && gm.AutopsyReport.Contains(finding));
            else Check($"{region.Name}: otopsi raporu dolu", !string.IsNullOrWhiteSpace(gm.AutopsyReport));

            gm.UI.DebugCloseModal();
            gm.OpenAccuse(); yield return Wait(1f);
            int wrong = gm.Npcs.First(n => n.Id != gm.GuiltyId).Id;
            gm.Accuse(gm.GuiltyId); yield return Wait(GameData.JailSeconds + 2f);
            Check($"{region.Name}: doğru suçlama kazandırdı", gm.Phase == GamePhase.Result && gm.ResultWin);
            Check($"{region.Name}: finalde cinayetin hikâyesi var", !string.IsNullOrWhiteSpace(gm.ResultStory));
            Check($"{region.Name}: kasaba çözüldü kaydı", gm.IsSolved(region.Id));
            gm.ResultContinue(); yield return Wait(2f);
            Check($"{region.Name}: finalden haritaya dönüldü", gm.Phase == GamePhase.WorldMap);
            gm.BackToSplash(); yield return Wait(2.5f);
        }

        IEnumerator VisitRoom(GameManager gm, Dedektif3D.World.InteriorGenerator.InteriorInstance inst, bool playRoom, string townName)
        {
            var def = inst.Def;
            string room = $"{townName}/{def.DisplayName}";
            gm.EnterBuilding(def); yield return Wait(2.2f);
            if (!gm.InsideBuilding) { Check($"{room}: girildi", false); yield break; }
            Check($"{room}: fareyle arama açık", gm.Search.Active);
            yield return Wait(0.6f);

            var clues = GameData.Clues.Where(c => c.BuildingNpcId == def.NpcId).ToList();

            // Saklama yerleri: üstüne gelip tıklayınca açılır, içindeki delil görünür olur
            // Odalar yan yana durur: yalnızca bu odanın içindekiler (komşu odanınkiler duvarın ardında kalır)
            bool InThisRoom(Vector3 p) { var l = inst.Root.InverseTransformPoint(p); return Mathf.Abs(l.x) < 5.6f && Mathf.Abs(l.z) < 4.6f && Mathf.Abs(l.y) < 6f; }
            var spots = HidingSpotInteractable.All.Where(s => s != null && s.def != null && InThisRoom(s.transform.position)).ToList();
            foreach (var s in spots)
            {
                if (s.Opened) continue;
                bool seen = LookFromAnyVantage(gm, inst, s);
                if (!seen) { Check($"{room}: saklama yeri görülemiyor: {s.def.Label}", false); }
                gm.Search.DebugForceHover(s); yield return null; yield return null;
                if (gm.Search.Hovered == s) gm.Search.ClickHovered();
                gm.Search.DebugForceHover(null);
                yield return Wait(1.1f);
            }
            int hiddenLeft = clues.Count(c => c.Hidden && (gm.GetClue(c.Id).View == null || !gm.GetClue(c.Id).View.revealed));
            Check($"{room}: gizli deliller açığa çıktı (kalan {hiddenLeft})", hiddenLeft == 0);

            // Her delil: en az bir köşeden görülür, imleçle seçilir, parlar
            int selectable = 0, glowOk = 0;
            foreach (var cd in clues)
            {
                var view = gm.GetClue(cd.Id).View;
                if (view == null) continue;
                if (LookFromAnyVantage(gm, inst, view)) selectable++;
                else Log($"  UYARI: {room}: '{cd.Name}' imleçle seçilemedi");
                gm.Search.DebugForceHover(view); yield return null; yield return null;
                if (gm.Search.Hovered == view && GlowApplied(view)) glowOk++;
                gm.Search.DebugForceHover(null); yield return null;
                if (!GlowApplied(view)) continue;
                Check($"{room}: parıltı temizlenmedi: {cd.Name}", false);
            }
            Check($"{room}: bütün deliller imleçle seçilebilir ({selectable}/{clues.Count})", selectable == clues.Count);
            Check($"{room}: bütün deliller parlıyor ({glowOk}/{clues.Count})", glowOk == clues.Count);
            gm.Player.Teleport(inst.Spawn.position, inst.Spawn.rotation);
            yield return null;

            if (!playRoom)
            {
                gm.Search.OnLeaveRoom();
                gm.ConfirmExitBuilding(); yield return Wait(1.6f);
                yield break;
            }

            // Fareyle tıklayıp delil al
            foreach (var cd in clues.OrderByDescending(c => (c.HasBlood ? 1 : 0) + (c.HasPrint ? 1 : 0)).Take(2))
            {
                if (gm.BagCount >= GameData.MaxBagSize) break;
                var view = gm.GetClue(cd.Id).View;
                gm.Search.DebugForceHover(view); yield return null; yield return null;
                gm.Search.ClickHovered();
                gm.Search.DebugForceHover(null);
                yield return null;
                Check($"{room}: tıklayınca delil kartı açıldı: {cd.Name}", gm.UI.ShowingClueCard);
                gm.TakeClue(cd.Id);
                gm.UI.DebugCloseModal();
                yield return Wait(0.3f);
                Check($"{room}: delil odadan kalktı: {cd.Name}", gm.GetClue(cd.Id).InBag && !view.Searchable);
            }

            // Sorgu: hazır soru, cevabı, serbest soru
            var npc = gm.GetNpc(def.NpcId);
            gm.OpenInterrogation(npc.Id); yield return null;
            Check($"{room}: sorgu açıldı", gm.UI.ShowingInterrogation);
            var firstQ = npc.Suggestions.FirstOrDefault();
            Check($"{room}: soru seçenekleri var ({npc.Suggestions.Count})", firstQ != null && npc.Suggestions.Count >= 4);
            if (firstQ != null)
            {
                gm.AskQuestion(npc.Id, firstQ);
                float t0 = Time.realtimeSinceStartup;
                while ((npc.Waiting || !npc.Log.All(l => l.Typed)) && Time.realtimeSinceStartup - t0 < 30f) yield return null;
                var ans = npc.Log.LastOrDefault(l => !l.FromPlayer);
                Check($"{room}: cevap geldi", ans != null && !string.IsNullOrWhiteSpace(ans.Text));
                yield return Wait(1.2f);
                gm.AskFree(npc.Id, "O gece neredeydin?");
                t0 = Time.realtimeSinceStartup;
                while ((npc.Waiting || !npc.Log.All(l => l.Typed)) && Time.realtimeSinceStartup - t0 < 30f) yield return null;
                var free = npc.Log.LastOrDefault(l => !l.FromPlayer);
                Check($"{room}: serbest soruya cevap", free != null && free != ans && !string.IsNullOrWhiteSpace(free.Text));
                yield return Wait(0.6f);
            }
            gm.UI.DebugCloseModal(); yield return null;
            gm.ProvideBuildingHint(); gm.AnalyzeBag(); gm.Vision.Activate();
            yield return Wait(0.2f);

            gm.RequestExitBuilding(); yield return null;
            gm.UI.DebugCloseModal();
            gm.ConfirmExitBuilding(); yield return Wait(1.6f);
            Check($"{room}: çıkış ve kilit", !gm.InsideBuilding && gm.IsVisited(def.NpcId));
        }

        static bool GlowApplied(Interactable it)
        {
            var list = new List<Renderer>();
            it.CollectGlowRenderers(list);
            return list.Any(r => r != null && r.sharedMaterials.Any(m => m != null && m.shader != null && m.shader.name == "Dedektif/HoverGlow"));
        }

        /// <summary>Oyuncu kapıdan, odanın ortasından ve iki yanından bakar; imleç nesnenin ekrandaki merkezindeyken seçiliyor mu?</summary>
        static bool LookFromAnyVantage(GameManager gm, Dedektif3D.World.InteriorGenerator.InteriorInstance inst, Interactable it)
        {
            var vantage = new[] { new Vector3(0, 0, -2.6f), new Vector3(0, 0, 0.3f), new Vector3(-2.8f, 0, -0.6f), new Vector3(2.8f, 0, -0.6f), new Vector3(-3.6f, 0, 0.9f), new Vector3(3.6f, 0, 0.9f), new Vector3(3.2f, 0, 2.6f), new Vector3(-3.2f, 0, 2.6f) };   // son ikisi: tezgâhın arkası
            var cam = gm.Player.Cam;
            string last;
            var trail = new System.Text.StringBuilder();
            foreach (var v in vantage)
            {
                var pos = inst.Root.TransformPoint(v);
                gm.Player.Teleport(pos, inst.Root.rotation);
                cam.DebugLookAt(it.HoverBounds.center);
                Physics.SyncTransforms();
                if (gm.Search.ScreenCenter(it, out var px))
                {
                    var picked = gm.Search.Pick(px);
                    if (picked == it) return true;
                    var from = cam.transform.position;
                    var aim = it.HoverBounds.center + Vector3.up * (it.HoverBounds.extents.y + 0.25f);
                    int mask = ~((1 << 2) | (1 << Dedektif3D.World.TownGenerator.NpcBarrierLayer));
                    string blocker = Physics.Raycast(from, (aim - from).normalized, out var hit, (aim - from).magnitude, mask, QueryTriggerInteraction.Ignore)
                        ? $"{hit.collider.name}[{hit.collider.transform.parent?.name}]" : "boş";
                    last = $"köşe {v}: seçilen={(picked != null ? picked.name : "yok")}, görüş={gm.Search.CanSee(it)}, engel={blocker}";
                }
                else last = $"köşe {v}: ekranda değil";
                trail.Append(" | ").Append(last);
            }
            Log($"  AYRINTI: {it.name}:{trail}");
            return false;
        }
    }
}
#endif
