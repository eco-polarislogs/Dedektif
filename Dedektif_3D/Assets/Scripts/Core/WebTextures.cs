using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Dedektif3D.API;
using UnityEngine;
using UnityEngine.Networking;

namespace Dedektif3D.Core
{
    /// <summary>
    /// Web görselleri: Gizemli Kasaba görselleri projede (Resources/Web) gömülüdür; Gölgeşehir ve Sisören'in
    /// görselleri (portreler, deliller, haritalar) sırayla şu kaynaklardan okunur:
    ///   1) Oyunla birlikte dağıtılan kopya: StreamingAssets/Web/images/... (build sırasında wwwroot'tan kopyalanır)
    ///   2) Yalnızca editörde: depodaki ../wwwroot/images/... (aynı dosyaların iki kopyası tutulmaz)
    ///   3) Son çare: backend (baseUrl). Oyuncunun sunucu çalıştırması gerekmez.
    /// </summary>
    public class WebTextures : MonoBehaviour
    {
        static WebTextures _instance;
        static readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();
        static readonly Dictionary<string, List<Action<Texture2D>>> Pending = new Dictionary<string, List<Action<Texture2D>>>();
        static readonly HashSet<string> Failed = new HashSet<string>();

        static WebTextures Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("WebTextures");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<WebTextures>();
                }
                return _instance;
            }
        }

        /// <summary>Hazırsa dokuyu döndürür; değilse indirmeyi başlatır ve null döner (IMGUI bir sonraki karede tekrar sorar).</summary>
        public static Texture2D Get(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (Cache.TryGetValue(path, out var t)) return t;
            Load(path, null);
            return null;
        }

        public static void Load(string path, Action<Texture2D> done)
        {
            if (string.IsNullOrEmpty(path)) { done?.Invoke(null); return; }
            if (Cache.TryGetValue(path, out var t)) { done?.Invoke(t); return; }
            if (!path.Contains("/"))
            {
                t = Resources.Load<Texture2D>("Web/" + path);
                Cache[path] = t;
                done?.Invoke(t);
                return;
            }
            if (Failed.Contains(path)) { done?.Invoke(null); return; }
            if (Pending.TryGetValue(path, out var list)) { if (done != null) list.Add(done); return; }
            Pending[path] = new List<Action<Texture2D>>();
            if (done != null) Pending[path].Add(done);
            Instance.StartCoroutine(Download(path));
        }

        /// <summary>Görselin diskteki yerel kopyası; yoksa null.</summary>
        static string LocalFile(string path)
        {
            string rel = path.Replace('\\', '/').TrimStart('/');
            string packaged = Path.Combine(Application.streamingAssetsPath, "Web", rel);
            if (File.Exists(packaged)) return packaged;
#if UNITY_EDITOR
            string dev = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "wwwroot", rel));
            if (File.Exists(dev)) return dev;
#endif
            return null;
        }

        static string ResolveUrl(string path)
        {
            string local = LocalFile(path);
            if (local != null) return new Uri(local).AbsoluteUri;
            string baseUrl = GameAPIManager.Instance != null ? GameAPIManager.Instance.baseUrl : null;
            if (string.IsNullOrEmpty(baseUrl)) return null; // yerel kopya yok ve sunucu ayarlanmamış
            return baseUrl.TrimEnd('/') + "/" + string.Join("/", Array.ConvertAll(path.TrimStart('/').Split('/'), Uri.EscapeDataString));
        }

        static void FailWithoutRequest(string path)
        {
            Failed.Add(path);
            Debug.LogWarning($"[WebTextures] Görsel bulunamadı: {path}");
            if (Pending.TryGetValue(path, out var cbs))
            {
                Pending.Remove(path);
                foreach (var cb in cbs) cb?.Invoke(null);
            }
        }

        static IEnumerator Download(string path)
        {
            string url = ResolveUrl(path);
            if (url == null)
            {
                FailWithoutRequest(path);
                yield break;
            }
            using (var req = UnityWebRequestTexture.GetTexture(url))
            {
                req.timeout = 20;
                yield return req.SendWebRequest();
                Texture2D tex = null;
                if (req.result == UnityWebRequest.Result.Success)
                {
                    tex = EraFilter(path, DownloadHandlerTexture.GetContent(req));
                    tex.wrapMode = TextureWrapMode.Clamp;
                    Cache[path] = tex;
                }
                else
                {
                    Failed.Add(path); // dosya yok ve sunucu kapalı: oyun görselsiz devam eder, tekrar tekrar denemez
                    Debug.LogWarning($"[WebTextures] Görsel yüklenemedi: {path} ({req.error})");
                }
                if (Pending.TryGetValue(path, out var cbs))
                {
                    Pending.Remove(path);
                    foreach (var cb in cbs) cb?.Invoke(tex);
                }
            }
        }

        // ------------------------------------------------------------------
        //  DÖNEM SÜZGECİ: Sisören görsellerinde 1887'ye uymayan ayrıntılar
        // ------------------------------------------------------------------
        /// <summary>Kırpma (0-1, sol üstten) + silinecek bölgeler. Görseller değişmeden oyunda sepya tabak fotoğraf gibi gösterilir.</summary>
        class EraFix { public Rect Crop = new Rect(0, 0, 1, 1); public Rect[] Smudge = new Rect[0]; }

        static readonly Dictionary<string, EraFix> EraFixes = new Dictionary<string, EraFix>
        {
            // Arkadaki sinema makinesi ve neon "TÜPÇÜ" tabelası kadrajdan çıkar
            { "npcler/npc_203", new EraFix { Crop = new Rect(0.1f, 0.3f, 0.58f, 0.58f) } },
            { "npcler/npc_209", new EraFix { Crop = new Rect(0.3f, 0.13f, 0.33f, 0.33f) } },
            // Film makarası etiketi kadraj dışı; levha ve kayıt üzerindeki modern yazılar eğelenmiş gibi silinir
            { "deliller/2031", new EraFix { Crop = new Rect(0.02f, 0.44f, 0.56f, 0.56f) } },
            { "deliller/2102", new EraFix { Smudge = new[] { new Rect(0.2f, 0.28f, 0.66f, 0.42f) } } },
            { "deliller/2033", new EraFix { Smudge = new[] { new Rect(0.43f, 0.6f, 0.2f, 0.11f) } } },
            { "deliller/2093", new EraFix { Smudge = new[] { new Rect(0.58f, 0.55f, 0.34f, 0.12f) } } },
            { "deliller/2014", new EraFix { Smudge = new[] { new Rect(0.58f, 0.68f, 0.3f, 0.2f) } } },
        };

        static Texture2D EraFilter(string path, Texture2D src)
        {
            string p = path.Replace('\\', '/');
            if (!p.Contains("towns/sisoren/")) return src;
            foreach (var kv in EraFixes)
                if (p.Contains(kv.Key + ".")) return ApplyEraFix(src, kv.Value);
            return src;
        }

        static Texture2D ApplyEraFix(Texture2D src, EraFix fix)
        {
            if (!src.isReadable) return src;
            int w = src.width, h = src.height;
            int cx = Mathf.RoundToInt(fix.Crop.x * w), cw = Mathf.Clamp(Mathf.RoundToInt(fix.Crop.width * w), 8, w - cx);
            int ch = Mathf.Clamp(Mathf.RoundToInt(fix.Crop.height * h), 8, h);
            int cy = Mathf.Clamp(h - Mathf.RoundToInt(fix.Crop.y * h) - ch, 0, h - ch);
            var px = src.GetPixels(cx, cy, cw, ch);

            // Silinecek bölge: yatay çizgilerle ovulmuş metal görünümü
            foreach (var s in fix.Smudge)
            {
                int x0 = Mathf.RoundToInt((s.x - fix.Crop.x) / fix.Crop.width * cw), x1 = Mathf.RoundToInt((s.xMax - fix.Crop.x) / fix.Crop.width * cw);
                int yTop = Mathf.RoundToInt((s.y - fix.Crop.y) / fix.Crop.height * ch), yBot = Mathf.RoundToInt((s.yMax - fix.Crop.y) / fix.Crop.height * ch);
                x0 = Mathf.Clamp(x0, 0, cw - 1); x1 = Mathf.Clamp(x1, x0 + 1, cw);
                int y0 = Mathf.Clamp(ch - yBot, 0, ch - 1), y1 = Mathf.Clamp(ch - yTop, y0 + 1, ch);
                int radius = Mathf.Max(6, (x1 - x0) / 10);
                var copy = (Color[])px.Clone();
                for (int y = y0; y < y1; y++)
                    for (int x = x0; x < x1; x++)
                    {
                        Color sum = Color.black;
                        int n = 0;
                        for (int i = -radius; i <= radius; i += 2)
                        {
                            sum += copy[y * cw + Mathf.Clamp(x + i, 0, cw - 1)];
                            n++;
                        }
                        sum /= n;
                        float streak = Mathf.PerlinNoise(x * 0.012f, y * 0.35f) * 0.28f - 0.14f;
                        // Kenarlarda yumuşak geçiş
                        float ex = Mathf.Min(x - x0, x1 - 1 - x) / (float)Mathf.Max(1, (x1 - x0) / 8);
                        float ey = Mathf.Min(y - y0, y1 - 1 - y) / (float)Mathf.Max(1, (y1 - y0) / 6);
                        float k = Mathf.Clamp01(Mathf.Min(ex, ey));
                        var smeared = new Color(sum.r + streak, sum.g + streak, sum.b + streak, 1f);
                        px[y * cw + x] = Color.Lerp(copy[y * cw + x], smeared, k);
                    }
            }

            // Sepya: renkli ayrıntılar bastırılır, görsel eski bir fotoğraf gibi okunur
            for (int i = 0; i < px.Length; i++)
            {
                var c = px[i];
                float l = c.r * 0.3f + c.g * 0.59f + c.b * 0.11f;
                var sepia = new Color(Mathf.Clamp01(l * 1.07f + 0.03f), Mathf.Clamp01(l * 0.93f + 0.015f), Mathf.Clamp01(l * 0.72f), 1f);
                px[i] = Color.Lerp(c, sepia, 0.85f);
            }
            var dst = new Texture2D(cw, ch, TextureFormat.RGB24, false) { filterMode = FilterMode.Bilinear };
            dst.SetPixels(px);
            dst.Apply(false, false);
            UnityEngine.Object.Destroy(src);
            return dst;
        }

        /// <summary>Sunucu sonradan açılırsa başarısız indirmeleri yeniden dene.</summary>
        public static void RetryFailed() => Failed.Clear();
    }
}
