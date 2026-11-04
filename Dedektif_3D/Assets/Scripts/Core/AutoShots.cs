using System.Collections;
using System.IO;
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
            File.AppendAllText(Path.Combine(_dir, "log.txt"), "DONE\n");
            Application.Quit();
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
