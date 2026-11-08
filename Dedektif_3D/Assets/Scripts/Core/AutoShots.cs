using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Dedektif3D.Core
{
    /// <summary>
    /// Geliştirici aracı: derlenmiş oyun "-dedektifShots &lt;klasör&gt;" ile başlatılırsa açılış → 3D harita → vaka dosyası →
    /// hikâye → kasabaya iniş → oynanış akışını kendisi oynar, arayüz dahil ekran görüntüleri alır ve kapanır.
    /// Bu bayrak verilmezse hiçbir şey yapmaz (normal oyun etkilenmez).
    /// </summary>
    public class AutoShots : MonoBehaviour
    {
        string _dir;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (Application.isEditor) return;
            var args = System.Environment.GetCommandLineArgs();
            int i = System.Array.IndexOf(args, "-dedektifShots");
            if (i < 0 || i + 1 >= args.Length) return;
            Application.runInBackground = true;   // otomatik test penceresi odakta olmasa da sürsün
            var go = new GameObject("AutoShots");
            DontDestroyOnLoad(go);
            go.AddComponent<AutoShots>()._dir = args[i + 1];
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory(_dir);
            float t0 = Time.realtimeSinceStartup;
            while (GameManager.Instance == null && Time.realtimeSinceStartup - t0 < 90f) yield return null;
            var gm = GameManager.Instance;
            if (gm == null) { Application.Quit(); yield break; }
            File.AppendAllText(Path.Combine(_dir, "log.txt"), $"shaders: lit={Shader.Find("Universal Render Pipeline/Lit") != null} particles={Shader.Find("Universal Render Pipeline/Particles/Unlit") != null} sky={Shader.Find("Skybox/Procedural") != null}\n");
            yield return CheckPackage();
            yield return new WaitForSeconds(5f); yield return Shot("01_acilis");
            gm.StartFromSplash(); yield return new WaitForSeconds(7.5f); yield return Shot("02_harita");
            var map = World.WorldMapDiorama.Instance;
            var gizemli = map != null ? map.Sites.Find(s => s.Region.Id == "gizemli") : null;
            if (map != null) map.Select(gizemli);
            yield return new WaitForSeconds(2.5f); yield return Shot("03_vaka_dosyasi");
            if (map != null) map.Select(map.Sites.Find(s => s.Region.Id == "golge_sehir"));
            yield return new WaitForSeconds(2.5f); yield return Shot("04_kilitli_dosya");
            if (map != null && gizemli != null) map.Dive(gizemli, () => gm.SelectRegion(gizemli.Region));
            else gm.SelectRegion(GameData.Regions[0]);
            yield return new WaitForSeconds(0.9f); yield return Shot("05_dalis");
            yield return new WaitForSeconds(3.4f); yield return Shot("06_bolum_karti");
            yield return new WaitForSeconds(7.5f); yield return Shot("07_hikaye");
            gm.FinishStory(); yield return new WaitForSeconds(5f); yield return Shot("08_inis");
            gm.SkipDescent(); yield return new WaitForSeconds(4f); yield return Shot("09_oynanis");
            gm.UI.DebugOverlay("pause"); yield return new WaitForSecondsRealtime(1f); yield return Shot("10_duraklat");
            gm.UI.DebugOverlay("settings"); yield return new WaitForSecondsRealtime(1f); yield return Shot("11_ayarlar");
            gm.UI.DebugOverlay("board"); yield return new WaitForSecondsRealtime(1f); yield return Shot("12_vaka_panosu");
            gm.UI.DebugOverlay("none"); yield return new WaitForSeconds(0.5f);
            gm.OpenHelperPanel(); yield return new WaitForSeconds(3f); yield return Shot("13_yardimci");
            yield return RoomTour(gm, "14_gizemli", gm.Town.Interiors.Values.First());
            yield return AllRooms(gm, "15_gizemli");

            // Diğer iki kasaba: dış görünüş, oda, parıltı, sorgu
            gm.UnlockAllForTesting = true;
            foreach (var id in new[] { "golge_sehir", "sisoren" })
            {
                gm.BackToSplash(); yield return new WaitForSeconds(3f);
                gm.SelectRegion(GameData.Regions.Find(r => r.Id == id)); yield return new WaitForSeconds(4f);
                gm.FinishStory(); yield return new WaitForSeconds(2.5f);
                gm.SkipDescent(); yield return new WaitForSeconds(4f);
                yield return Shot("20_" + id + "_kasaba");
                gm.UI.DebugOverlay("none");
                var inst = id == "sisoren" ? gm.Town.Interiors.Values.FirstOrDefault(i => i.Def.Key == "tupcu") ?? gm.Town.Interiors.Values.First() : gm.Town.Interiors.Values.First();
                yield return RoomTour(gm, "21_" + id, inst);
                yield return AllRooms(gm, "22_" + id);
            }
            File.AppendAllText(Path.Combine(_dir, "log.txt"), "DONE\n");
            Application.Quit();
        }

        /// <summary>Kasabanın bütün odalarından birer kare: web iç mekân görselleriyle karşılaştırmak için.</summary>
        IEnumerator AllRooms(GameManager gm, string tag)
        {
            foreach (var inst in gm.Town.Interiors.Values.ToList())
            {
                gm.EnterBuilding(inst.Def); yield return new WaitForSeconds(2.6f);
                gm.UI.DebugCloseModal();
                yield return Shot(tag + "_" + inst.Def.Key);
                gm.ConfirmExitBuilding(); yield return new WaitForSeconds(1.8f);
            }
        }

        /// <summary>Bir odayı gezer: fareyle arama, delilin parlaması, delil kartı, sorgu ekranı.</summary>
        IEnumerator RoomTour(GameManager gm, string tag, World.InteriorGenerator.InteriorInstance inst)
        {
            gm.EnterBuilding(inst.Def); yield return new WaitForSeconds(3f);
            yield return Shot(tag + "_oda");
            var search = gm.Search;
            var cam = gm.Player.Cam;
            var clue = GameData.Clues.Where(c => c.BuildingNpcId == inst.Def.NpcId && !c.Hidden).Select(c => gm.GetClue(c.Id).View).FirstOrDefault(v => v != null);
            if (clue != null)
            {
                cam.DebugLookAt(clue.HoverBounds.center);
                search.DebugForceHover(clue);
                if (search.ScreenCenter(clue, out var px)) gm.UI.DebugPointerPx = px;
                yield return new WaitForSeconds(1f); yield return Shot(tag + "_delil_parliyor");
                search.ClickHovered();
                search.DebugForceHover(null);
                gm.UI.DebugPointerPx = null;
                yield return new WaitForSeconds(2f); yield return Shot(tag + "_delil_karti");
                gm.TakeClue(clue.clueId);
                gm.UI.DebugCloseModal();
                yield return new WaitForSeconds(1f);
            }
            bool InRoom(Vector3 p) { var l = inst.Root.InverseTransformPoint(p); return Mathf.Abs(l.x) < 5.6f && Mathf.Abs(l.z) < 4.6f; }
            var spot = Gameplay.HidingSpotInteractable.All.FirstOrDefault(s => s != null && s.def != null && InRoom(s.transform.position) && !s.Opened);
            if (spot != null)
            {
                cam.DebugLookAt(spot.HoverBounds.center);
                search.DebugForceHover(spot);
                if (search.ScreenCenter(spot, out var px)) gm.UI.DebugPointerPx = px;
                yield return new WaitForSeconds(1f); yield return Shot(tag + "_saklama_yeri");
                search.ClickHovered();
                search.DebugForceHover(null);
                gm.UI.DebugPointerPx = null;
                yield return new WaitForSeconds(2f);
            }
            var npc = gm.GetNpc(inst.Def.NpcId);
            gm.OpenInterrogation(npc.Id); yield return new WaitForSeconds(2f);
            var q = npc.Suggestions.FirstOrDefault();
            if (q != null)
            {
                gm.AskQuestion(npc.Id, q);
                yield return new WaitForSeconds(4f);
                yield return Shot(tag + "_sorgu");
                File.AppendAllText(Path.Combine(_dir, "log.txt"), $"{inst.Def.DisplayName} sorgu: {q.Q} => {npc.Log.LastOrDefault(l => !l.FromPlayer)?.Text}\n");
            }
            gm.UI.DebugCloseModal();
            yield return new WaitForSeconds(0.6f);
            gm.ConfirmExitBuilding(); yield return new WaitForSeconds(2f);
        }

        /// <summary>Dağıtılan pakette sunucu ayarlı değil mi ve görseller oyunla birlikte geliyor mu?</summary>
        IEnumerator CheckPackage()
        {
            string log = Path.Combine(_dir, "log.txt");
            var api = API.GameAPIManager.Instance;
            File.AppendAllText(log, $"sunucu ayarli: {(api != null && api.HasServer)}\n");
            File.AppendAllText(log, $"belge: {GameData.Documents.Count}\n");

            var paths = new System.Collections.Generic.HashSet<string>();
            foreach (var name in new[] { "golge_sehir_data", "sisoren_data" })
            {
                var asset = Resources.Load<TextAsset>("Web/" + name);
                if (asset == null) { File.AppendAllText(log, $"veri yok: {name}\n"); continue; }
                foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(asset.text.Replace("\\/", "/"), @"images/[^""\\]+?\.(?:jpe?g|png|webp)"))
                    paths.Add(m.Value);
            }
            int ok = 0, pending = paths.Count;
            foreach (var p in paths)
                WebTextures.Load(p, t => { if (t != null) ok++; pending--; });
            float t0 = Time.realtimeSinceStartup;
            while (pending > 0 && Time.realtimeSinceStartup - t0 < 120f) yield return null;
            File.AppendAllText(log, $"gorsel: {ok}/{paths.Count}\n");
        }

        IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(_dir, name + ".png"));
            yield return null;
            yield return null;
        }
    }
}
