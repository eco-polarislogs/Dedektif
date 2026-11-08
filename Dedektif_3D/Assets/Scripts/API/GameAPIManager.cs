using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Dedektif3D.API
{
    // =====================================================================
    //  DTO'lar — ASP.NET Minimal API camelCase JSON çıktısıyla birebir eşleşir
    // =====================================================================
    [Serializable] public class NpcDto
    {
        public int npcId; public string name; public string role; public int trustLevel; public int fearLevel;
        public bool isGuilty; public string secretInfo; public int stressLevel; public string buildingName;
        public bool isActive; public string imageFile;
    }

    [Serializable] public class ClueDto
    {
        public int clueId; public string title; public string description; public int relatedNPCId;
        public string status; public string location;
    }

    [Serializable] public class InterrogateResponse
    {
        public bool success; public string dialogue; public string emotion; public int trustChange;
        public int stressIncrease; public int npcId; public string town;
    }

    [Serializable] public class AccuseResponse
    {
        public bool success; public string message; public string accusedName; public string guiltyNpcName; public int guiltyNpcId;
    }

    [Serializable] public class ResetResponse { public bool success; public string message; public int guiltyNpcId; }
    [Serializable] public class ClueActionResponse { public bool success; public int clueId; public string status; }
    [Serializable] public class AutopsyResponse { public bool success; public string report; public int guiltyId; public string error; }
    [Serializable] public class HelperTipResponse { public bool success; public string message; public string context; public int priority; }
    [Serializable] public class AnalyzeResponse { public bool success; public string analysis; }
    [Serializable] public class SessionStartResponse { public bool success; public int sessionId; public string error; }
    [Serializable] public class SimpleResponse { public bool success; public string error; public string warning; }

    [Serializable] public class DialogueNodeDto { public string q; public string a; public int difficulty; public string category; }
    [Serializable] public class DialoguesResponse { public bool success; public DialogueNodeDto[] dialogues; }

    // İstek gövdeleri
    [Serializable] class InterrogateRequest { public int npcId; public string question; }
    [Serializable] class AccuseRequest { public int npcId; }
    [Serializable] class ClueActionRequest { public string status; }
    [Serializable] class AnalyzeRequest { public int[] clueIds; }
    [Serializable] class ForensicSubmitRequest { public int clueId; public string clueName; public string findingText; }
    [Serializable] class SessionStartRequest { public int guiltyNpcId; }
    [Serializable] class SessionEndRequest { public int sessionId; public string result; public int accusedNpcId; public int totalQuestions; public int cluesCollected; }
    [Serializable] class ActionLogRequest { public int sessionId; public string actionType; public int targetId; public string details; }
    [Serializable] class StateSaveRequest { public int sessionId; public string stateData; }

    [Serializable] class ArrayWrapper<T> { public T[] items; }

    /// <summary>
    /// DedektiflikRPG ASP.NET backend'i ile tüm iletişimi yöneten singleton.
    /// Web istemcisindeki session.js gibi her istekte X-Session-Id başlığı gönderir, böylece
    /// Unity oyuncusu da kendi bağımsız oturumunda (katil, çanta, adli bulgular) oynar.
    ///
    /// Sunucu isteğe bağlıdır: dağıtılan oyun varsayılan olarak çevrimdışı çalışır ve hiç ağ isteği yapmaz.
    /// Adres sırayla şuradan okunur: "-server http://..." komut satırı argümanı, StreamingAssets/server.txt,
    /// yalnızca editörde http://localhost:5000.
    /// </summary>
    public class GameAPIManager : MonoBehaviour
    {
        public static GameAPIManager Instance { get; private set; }

        [Header("Backend")]
        [Tooltip("Backend adresi; boşsa oyun çevrimdışı çalışır. Açılışta ResolveServerUrl ile belirlenir.")]
        public string baseUrl = "";
        public string townId = "gizemli";
        public int timeoutSeconds = 12;
        public bool verboseLogging = true;

        /// <summary>Bir sunucu adresi ayarlanmış mı? Değilse hiçbir ağ isteği yapılmaz.</summary>
        public bool HasServer => !string.IsNullOrEmpty(baseUrl);

        public string SessionId { get; private set; }
        public bool IsOnline { get; private set; }
        public int GameSessionId { get; private set; }
        public string LastError { get; private set; }

        public event Action<bool> ConnectionChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SessionId = Guid.NewGuid().ToString();
            baseUrl = ResolveServerUrl();
            verboseLogging = Application.isEditor;
        }

        static string ResolveServerUrl()
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == "-server") return args[i + 1].Trim().TrimEnd('/');
            try
            {
                string file = System.IO.Path.Combine(Application.streamingAssetsPath, "server.txt");
                if (System.IO.File.Exists(file))
                {
                    string url = System.IO.File.ReadAllText(file).Trim().TrimEnd('/');
                    if (url.Length > 0) return url;
                }
            }
            catch (Exception e) { Debug.LogWarning("[GameAPI] server.txt okunamadı: " + e.Message); }
#if UNITY_EDITOR
            return "http://localhost:5000";
#else
            return "";
#endif
        }

        /// <summary>Kod içinden hiçbir sahne objesi olmadan oluşturmak için.</summary>
        public static GameAPIManager EnsureExists()
        {
            if (Instance != null) return Instance;
            var go = new GameObject("GameAPIManager");
            return go.AddComponent<GameAPIManager>();
        }

        // =================================================================
        //  Kasaba (Unified v1) uç noktaları
        // =================================================================
        public void CheckConnection(Action<bool> done)
        {
            Get($"/api/v1/towns/{townId}/npcs", (ok, _) => { SetOnline(ok); done?.Invoke(ok); });
        }

        public void GetNPCs(Action<List<NpcDto>> onSuccess, Action<string> onError = null)
            => Get($"/api/v1/towns/{townId}/npcs", (ok, body) =>
            {
                if (ok) onSuccess?.Invoke(ParseArray<NpcDto>(body)); else onError?.Invoke(body);
            });

        public void GetClues(Action<List<ClueDto>> onSuccess, Action<string> onError = null)
            => Get($"/api/v1/towns/{townId}/clues", (ok, body) =>
            {
                if (ok) onSuccess?.Invoke(ParseArray<ClueDto>(body)); else onError?.Invoke(body);
            });

        public void Interrogate(int npcId, string question, Action<InterrogateResponse> onSuccess, Action<string> onError = null)
            => Post($"/api/v1/towns/{townId}/interrogate", new InterrogateRequest { npcId = npcId, question = question }, onSuccess, onError);

        public void Accuse(int npcId, Action<AccuseResponse> onSuccess, Action<string> onError = null)
            => Post($"/api/v1/towns/{townId}/accuse", new AccuseRequest { npcId = npcId }, onSuccess, onError);

        public void ResetTown(Action<ResetResponse> onSuccess, Action<string> onError = null)
            // Not: birleşik /api/v1/towns/{id}/reset backend'de HTTP 500 veriyor; web istemcisi (app.js initGame,
            // GolgeSehirEngine, SisorenEngine) kasabaya özgü uç noktaları kullanıyor — aynısını kullanıyoruz.
            => PostRaw(townId == "golge_sehir" ? "/api/golge-sehir/reset" : townId == "sisoren" ? "/api/sisoren/reset" : "/api/game/reset", "{}", onSuccess, onError);

        // =================================================================
        //  Oyun (/api/game) uç noktaları
        // =================================================================
        public void SetClueStatus(int clueId, string status, Action<ClueActionResponse> onSuccess = null, Action<string> onError = null)
            => Post($"/api/game/clues/{clueId}/action", new ClueActionRequest { status = status }, onSuccess, onError);

        public void KeepClue(int clueId, Action<ClueActionResponse> cb = null) => SetClueStatus(clueId, "KeptInBag", cb);
        public void IgnoreClue(int clueId, Action<ClueActionResponse> cb = null) => SetClueStatus(clueId, "IgnoredAtScene", cb);

        public void GetDialogues(int npcId, string category, Action<DialoguesResponse> onSuccess, Action<string> onError = null)
            => Get($"/api/game/dialogues?npcId={npcId}&category={UnityWebRequest.EscapeURL(category)}", (ok, body) =>
            {
                if (ok) onSuccess?.Invoke(Parse<DialoguesResponse>(body)); else onError?.Invoke(body);
            });

        public void GetAutopsy(Action<AutopsyResponse> onSuccess, Action<string> onError = null)
            => Get($"/api/game/autopsy?town={UnityWebRequest.EscapeURL(townId == "gizemli" ? "gizemli_kasaba" : townId)}", (ok, body) =>
            {
                if (ok) onSuccess?.Invoke(Parse<AutopsyResponse>(body)); else onError?.Invoke(body);
            });

        public void SubmitForensic(int clueId, string clueName, string finding, Action<SimpleResponse> onSuccess = null, Action<string> onError = null)
            => Post("/api/game/forensic/submit", new ForensicSubmitRequest { clueId = clueId, clueName = clueName, findingText = finding }, onSuccess, onError);

        public void GetHelperTip(string context, string building, Action<HelperTipResponse> onSuccess, Action<string> onError = null)
        {
            string prefix = townId == "golge_sehir" ? "/api/golge-sehir" : townId == "sisoren" ? "/api/sisoren" : "/api/game";
            string url = $"{prefix}/helper/tip?context={UnityWebRequest.EscapeURL(context)}";
            if (!string.IsNullOrEmpty(building)) url += $"&building={UnityWebRequest.EscapeURL(building)}";
            Get(url, (ok, body) => { if (ok) onSuccess?.Invoke(Parse<HelperTipResponse>(body)); else onError?.Invoke(body); });
        }

        public void AnalyzeClues(List<int> clueIds, Action<AnalyzeResponse> onSuccess, Action<string> onError = null)
            => Post("/api/game/helper/analyze-clues", new AnalyzeRequest { clueIds = clueIds.ToArray() }, onSuccess, onError);

        public void StartSession(int guiltyNpcId, Action<SessionStartResponse> onSuccess = null, Action<string> onError = null)
            => Post<SessionStartRequest, SessionStartResponse>("/api/game/session/start", new SessionStartRequest { guiltyNpcId = guiltyNpcId }, r =>
            {
                if (r != null && r.success) GameSessionId = r.sessionId;
                onSuccess?.Invoke(r);
            }, onError);

        public void EndSession(string result, int accusedNpcId, int totalQuestions, int cluesCollected, Action<SimpleResponse> onSuccess = null)
            => Post("/api/game/session/end", new SessionEndRequest
            {
                sessionId = GameSessionId, result = result, accusedNpcId = accusedNpcId,
                totalQuestions = totalQuestions, cluesCollected = cluesCollected
            }, onSuccess, null);

        public void LogAction(string actionType, int targetId, string details)
        {
            if (!IsOnline) return;
            Post<ActionLogRequest, SimpleResponse>("/api/game/action/log", new ActionLogRequest
            {
                sessionId = GameSessionId, actionType = actionType, targetId = targetId, details = details
            }, null, null);
        }

        public void SaveState(string stateJson, Action<SimpleResponse> onSuccess = null)
            => Post("/api/game/state/save", new StateSaveRequest { sessionId = GameSessionId, stateData = stateJson }, onSuccess, null);

        // =================================================================
        //  HTTP katmanı
        // =================================================================
        void Get(string path, Action<bool, string> done)
        {
            if (!HasServer) { StartCoroutine(Offline(done)); return; }
            StartCoroutine(Send(UnityWebRequest.Get(baseUrl + path), done));
        }

        /// <summary>Sunucu yokken isteği ağa çıkmadan reddeder; çağıranlar yine bir kare sonra yanıt alır.</summary>
        IEnumerator Offline(Action<bool, string> done)
        {
            yield return null;
            done?.Invoke(false, "offline");
        }

        void Post<TReq, TRes>(string path, TReq body, Action<TRes> onSuccess, Action<string> onError)
            => PostRaw(path, JsonUtility.ToJson(body), onSuccess, onError);

        void PostRaw<TRes>(string path, string json, Action<TRes> onSuccess, Action<string> onError)
        {
            if (!HasServer)
            {
                StartCoroutine(Offline((_, body) => onError?.Invoke(body)));
                return;
            }
            var req = new UnityWebRequest(baseUrl + path, UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
            StartCoroutine(Send(req, (ok, body) =>
            {
                if (ok) onSuccess?.Invoke(Parse<TRes>(body));
                else onError?.Invoke(body);
            }));
        }

        IEnumerator Send(UnityWebRequest req, Action<bool, string> done)
        {
            using (req)
            {
                req.timeout = timeoutSeconds;
                req.SetRequestHeader("X-Session-Id", SessionId);
                req.SetRequestHeader("Accept", "application/json");
                yield return req.SendWebRequest();

                bool ok = req.result == UnityWebRequest.Result.Success;
                string body = req.downloadHandler != null ? req.downloadHandler.text : "";

                if (!ok)
                {
                    LastError = $"{req.method} {req.url} → {req.responseCode} {req.error}";
                    if (verboseLogging) Debug.LogWarning("[GameAPI] " + LastError + (string.IsNullOrEmpty(body) ? "" : "\n" + body));
                    // Bağlantı hatası (sunucu kapalı) ise çevrimdışı moda geç
                    if (req.result == UnityWebRequest.Result.ConnectionError) SetOnline(false);
                    done?.Invoke(false, string.IsNullOrEmpty(body) ? req.error : body);
                }
                else
                {
                    SetOnline(true);
                    if (verboseLogging) Debug.Log($"[GameAPI] {req.method} {req.url} ✓");
                    done?.Invoke(true, body);
                }
            }
        }

        void SetOnline(bool value)
        {
            if (IsOnline == value) return;
            IsOnline = value;
            ConnectionChanged?.Invoke(value);
        }

        // =================================================================
        //  JSON yardımcıları (JsonUtility kök dizi desteklemediği için sarmalanır)
        // =================================================================
        public static T Parse<T>(string json)
        {
            if (string.IsNullOrEmpty(json)) return default;
            try { return JsonUtility.FromJson<T>(json); }
            catch (Exception e) { Debug.LogWarning($"[GameAPI] JSON çözümlenemedi ({typeof(T).Name}): {e.Message}\n{json}"); return default; }
        }

        public static List<T> ParseArray<T>(string json)
        {
            if (string.IsNullOrEmpty(json)) return new List<T>();
            try
            {
                var w = JsonUtility.FromJson<ArrayWrapper<T>>("{\"items\":" + json + "}");
                return w?.items != null ? new List<T>(w.items) : new List<T>();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GameAPI] JSON dizi çözümlenemedi ({typeof(T).Name}): {e.Message}");
                return new List<T>();
            }
        }
    }
}
