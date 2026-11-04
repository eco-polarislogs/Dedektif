using System;
using System.Collections;
using System.Collections.Generic;
using Dedektif3D.API;
using UnityEngine;
using UnityEngine.Networking;

namespace Dedektif3D.Core
{
    /// <summary>
    /// Web görselleri: Gizemli Kasaba görselleri projede (Resources/Web) gömülüdür; Gölgeşehir ve Sisören'in
    /// 140+ görseli (portreler, deliller, haritalar) wwwroot'u sunan backend'den (http://localhost:5000/images/...)
    /// çalışma anında indirilip önbelleğe alınır — aynı dosyaların iki kopyası tutulmaz.
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

        static IEnumerator Download(string path)
        {
            string baseUrl = GameAPIManager.Instance != null ? GameAPIManager.Instance.baseUrl : "http://localhost:5000";
            string url = baseUrl.TrimEnd('/') + "/" + string.Join("/", Array.ConvertAll(path.Split('/'), Uri.EscapeDataString));
            using (var req = UnityWebRequestTexture.GetTexture(url))
            {
                req.timeout = 20;
                yield return req.SendWebRequest();
                Texture2D tex = null;
                if (req.result == UnityWebRequest.Result.Success)
                {
                    tex = DownloadHandlerTexture.GetContent(req);
                    tex.wrapMode = TextureWrapMode.Clamp;
                    Cache[path] = tex;
                }
                else Failed.Add(path); // sunucu kapalı: oyun görselsiz devam eder, tekrar tekrar denemez
                if (Pending.TryGetValue(path, out var cbs))
                {
                    Pending.Remove(path);
                    foreach (var cb in cbs) cb?.Invoke(tex);
                }
            }
        }

        /// <summary>Sunucu sonradan açılırsa başarısız indirmeleri yeniden dene.</summary>
        public static void RetryFailed() => Failed.Clear();
    }
}
