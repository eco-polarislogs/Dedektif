using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dedektif3D.Core
{
    /// <summary>
    /// Portre bilgisi: wwwroot/images portrelerinden örneklenen ten / saç / kıyafet renkleri, cinsiyet ve çocuk bilgisi
    /// (Resources/Faces/faces.json — tools_faces/extract_faces.py). Gerçek insan modeli seçimi (HumanLibrary) ve
    /// model yoksa primitive figürlerin renkleri bunu kullanır. Yüz resmi karaktere yapıştırılmaz.
    /// </summary>
    public static class FaceLibrary
    {
        public class FaceInfo
        {
            public string Key;
            public Color Skin, Hair, Coat, Accent;
            public bool Female, Child;
        }

        [Serializable] class JFace { public string key; public bool detected; public float[] skin, hair, coat, accent; }
        [Serializable] class JFaces { public JFace[] faces; }

        static Dictionary<string, FaceInfo> _infos;

        /// <summary>Sokaktaki kasabalıların yüzleri (Sisören 301-318 kasabalı portreleri + Bekçi İhsan).</summary>
        public static readonly string[] TownsfolkPool =
        {
            "npc_301", "npc_302", "npc_303", "npc_304", "npc_305", "npc_306", "npc_307", "npc_308", "npc_309",
            "npc_310", "npc_311", "npc_312", "npc_313", "npc_314", "npc_315", "npc_316", "npc_317", "npc_318", "bekci_ihsan"
        };
        static readonly HashSet<string> FemaleKeys = new HashSet<string>
        {
            "selma", "gunes", "npc_102_talk", "npc_104_talk", "npc_105_talk", "npc_204", "npc_206", "npc_207", "npc_210",
            "npc_211", "npc_212", "npc_213", "npc_304", "npc_308", "npc_315", "npc_317"
        };
        static readonly HashSet<string> ChildKeys = new HashSet<string> { "npc_307", "npc_315", "npc_316", "npc_317", "npc_318" };

        static void EnsureLoaded()
        {
            if (_infos != null) return;
            _infos = new Dictionary<string, FaceInfo>();
            var ta = Resources.Load<TextAsset>("Faces/faces");
            if (ta == null) { Debug.LogWarning("[Dedektif3D] Resources/Faces/faces.json yok — NPC'ler portre yüzü olmadan çizilecek."); return; }
            JFaces data;
            try { data = JsonUtility.FromJson<JFaces>(ta.text); }
            catch (Exception e) { Debug.LogWarning("[Dedektif3D] faces.json okunamadı: " + e.Message); return; }
            foreach (var f in data.faces ?? new JFace[0])
            {
                if (string.IsNullOrEmpty(f.key)) continue;
                _infos[f.key] = new FaceInfo
                {
                    Key = f.key, Skin = C(f.skin), Hair = C(f.hair), Coat = C(f.coat), Accent = C(f.accent),
                    Female = FemaleKeys.Contains(f.key), Child = ChildKeys.Contains(f.key)
                };
            }
        }

        static Color C(float[] v) => v != null && v.Length >= 3 ? new Color(v[0], v[1], v[2]) : new Color(0.3f, 0.3f, 0.3f);

        /// <summary>"images/towns/sisoren/npcler/npc_201.jpg" / "hasan" → "npc_201" / "hasan".</summary>
        public static string KeyFromPortrait(string portrait)
        {
            if (string.IsNullOrEmpty(portrait)) return null;
            var s = portrait.Replace('\\', '/');
            int slash = s.LastIndexOf('/');
            if (slash >= 0) s = s.Substring(slash + 1);
            int dot = s.LastIndexOf('.');
            if (dot > 0) s = s.Substring(0, dot);
            // Gölgeşehir'de 107 için "npc_107.png" de var; yüz dokusu _talk portresinden çıkarıldı
            if (s.StartsWith("npc_1") && !s.EndsWith("_talk")) s += "_talk";
            return s;
        }

        public static FaceInfo Info(string key)
        {
            EnsureLoaded();
            return key != null && _infos.TryGetValue(key, out var i) ? i : null;
        }

        public static bool Has(string key) => Info(key) != null;

        /// <summary>Kıyafet rengi: karanlık portrelerden örneklenen renkleri gece sahnesinde okunur kılar.</summary>
        public static Color Readable(Color c, float minValue = 0.16f, float maxValue = 0.7f)
        {
            Color.RGBToHSV(c, out var h, out var s, out var v);
            return Color.HSVToRGB(h, Mathf.Min(s, 0.6f), Mathf.Clamp(v, minValue, maxValue));
        }

        /// <summary>Rastgele kasabalı yüzü (kullanılanları mümkün olduğunca tekrar etmeden).</summary>
        static readonly List<string> _bag = new List<string>();
        public static string NextTownsfolkFace()
        {
            if (_bag.Count == 0) _bag.AddRange(TownsfolkPool);
            int i = UnityEngine.Random.Range(0, _bag.Count);
            var k = _bag[i]; _bag.RemoveAt(i);
            return Has(k) ? k : null;
        }
    }
}
