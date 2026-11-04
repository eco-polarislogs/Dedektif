#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using Dedektif3D.Core;
using Dedektif3D.Gameplay;
using Dedektif3D.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>GEÇİCİ doğrulama aracı: batch modda oyunu oynatıp kareler alır (DEDEKTIF_MODE = towns, map, materials birleşimi).</summary>
public static class DedektifShots
{
    static string OutDir => System.Environment.GetEnvironmentVariable("DEDEKTIF_SHOTS") ?? Path.GetFullPath("Temp/shots");
    static string Mode => System.Environment.GetEnvironmentVariable("DEDEKTIF_MODE") ?? "towns";

    public static void Run()
    {
        Directory.CreateDirectory(OutDir);
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        EditorApplication.playModeStateChanged += s => { if (s == PlayModeStateChange.EnteredPlayMode) new GameObject("ShotRunner").AddComponent<ShotRunner>(); };
        EditorApplication.isPlaying = true;
    }

    public class ShotRunner : MonoBehaviour
    {
        IEnumerator Start()
        {
            float t0 = Time.realtimeSinceStartup;
            while (GameManager.Instance == null && Time.realtimeSinceStartup - t0 < 60f) yield return null;
            var gm = GameManager.Instance;
            yield return new WaitForSeconds(1f);
            var cam = Camera.main;
            if (Mode.Contains("map"))
            {
                var map = WorldMapDiorama.Instance;
                gm.StartFromSplash(); yield return new WaitForSeconds(6.5f);
                RenderCurrent(cam, "map_overview");
                map.Select(map.Sites.Find(x => x.Region.Id == "gizemli")); yield return new WaitForSeconds(2.6f);
                RenderCurrent(cam, "map_gizemli");
                gm.BackToSplash(); yield return new WaitForSeconds(2.5f);
            }
            if (Mode.Contains("towns"))
            {
                gm.UnlockAllForTesting = true;
                foreach (var reg in GameData.Regions.Where(r => r.Level > 0))
                {
                    gm.StartFromSplash(); yield return new WaitForSeconds(2f);
                    gm.SelectRegion(reg); yield return new WaitForSeconds(3f);
                    gm.FinishStory(); yield return new WaitForSeconds(2f);
                    gm.SkipDescent(); yield return new WaitForSeconds(2.5f);
                    var town = gm.Town; var cs = town.CrimeScene;
                    foreach (var an in FindObjectsByType<Animator>(FindObjectsSortMode.None)) an.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    gm.Player.SetVisible(false);
                    foreach (var hour in new[] { 21.5f, 12.5f })
                    {
                        DayNightCycle.Instance.ResetDay(hour); yield return new WaitForSeconds(0.6f);
                        string tag = reg.Id + (hour > 20 ? "_gece" : "_gun");
                        Render(cam, cs + new Vector3(-14f, 14f, -26f), cs + new Vector3(0, 1f, 4f), tag + "_1genel");
                        var sp = town.PlayerSpawn; var f = town.PlayerSpawnRotation * Vector3.forward;
                        Render(cam, sp - f * 1.5f + Vector3.up * 1.7f, sp + f * 12f + Vector3.up * 1.4f, tag + "_2sokak");
                        int bi = 0;
                        foreach (var b in town.Buildings.Values.Take(3))
                        {
                            var fwd = -b.Root.forward;   // ön cephe yerel -Z
                            var door = b.Root.position;
                            Render(cam, door + fwd * 11f + Vector3.up * 2.2f + b.Root.right * 2.5f, door + Vector3.up * 3.6f, tag + "_3bina" + bi);
                            bi++;
                        }
                    }
                    gm.BackToSplash(); yield return new WaitForSeconds(2.5f);
                }
            }
            if (Mode.Contains("materials")) Log("materials saved: " + DedektifBuild.SnapshotMaterials());
            Log("DONE");
            EditorApplication.Exit(0);
        }

        static void Log(string s) { Debug.Log("[SHOTS] " + s); File.AppendAllText(Path.Combine(OutDir, "log.txt"), s + "\n"); }

        static void RenderCurrent(Camera cam, string name, int w = 1600, int h = 900)
        {
            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0); tex.Apply();
            File.WriteAllBytes(Path.Combine(OutDir, name + ".jpg"), tex.EncodeToJPG(90));
            RenderTexture.active = null; cam.targetTexture = null; rt.Release();
        }

        static void Render(Camera cam, Vector3 pos, Vector3 look, string name, int w = 1280, int h = 720)
        {
            var tpc = cam.GetComponent<ThirdPersonCamera>(); var dir = cam.GetComponent<CinematicDirector>(); var fx = cam.GetComponent<CameraFx>();
            if (tpc) tpc.enabled = false; if (dir) dir.enabled = false; if (fx) fx.enabled = false;
            cam.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(look - pos));
            RenderCurrent(cam, name, w, h);
            if (tpc) tpc.enabled = true; if (dir) dir.enabled = true; if (fx) fx.enabled = true;
        }
    }
}
#endif
