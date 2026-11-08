using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Dedektif3D.Core
{
    [Serializable]
    public class ClueSave
    {
        public int id;
        public bool inBag, revealed;
        public bool bloodFound, printFound, bloodChecked, printChecked, bloodSent, printSent;
    }

    [Serializable]
    public class LineSave
    {
        public bool fromPlayer;
        public string speaker, text;
    }

    [Serializable]
    public class NpcSave
    {
        public int id, stress, asked;
        public bool wokenUp;
        public List<string> askedQuestions = new List<string>();
        public List<LineSave> log = new List<LineSave>();
    }

    [Serializable]
    public class SpotSave
    {
        public int clueId;
        public bool sensed, opened;
    }

    /// <summary>Bir kasabadaki yarım kalmış soruşturmanın tamamı.</summary>
    [Serializable]
    public class CaseSave
    {
        public int version = SaveSystem.Version;
        public string townId;
        public string savedAtUtc;
        public int guiltyId, labCount, corpseLook;
        public string notes;
        public bool autopsyTimerStarted, autopsyReady;
        public float autopsyTimeLeft;
        public string autopsyReport;
        public float hour, elapsedMinutes;
        public int day;
        public int buildingNpcId;
        public Vector3 playerPos;
        public Quaternion playerRot = Quaternion.identity;
        public List<int> bag = new List<int>();
        public List<int> visited = new List<int>();
        public List<int> innocent = new List<int>();
        public List<string> helperContexts = new List<string>();
        public List<ClueSave> clues = new List<ClueSave>();
        public List<NpcSave> npcs = new List<NpcSave>();
        public List<SpotSave> spots = new List<SpotSave>();
    }

    /// <summary>
    /// Soruşturma kayıtları: kasaba başına bir JSON dosyası (Application.persistentDataPath/Kayitlar).
    /// Yazım elektrik kesintisine dayanıklıdır: önce geçici dosyaya yazılıp diske zorlanır, sonra tek adımda
    /// asıl dosyanın yerine geçer ve önceki sürüm .bak olarak saklanır. Asıl dosya okunamazsa yedek denenir.
    /// </summary>
    public static class SaveSystem
    {
        public const int Version = 1;

        static string _latest;
        static bool _latestKnown;

        /// <summary>Otomatik testler oyuncunun kayıtlarını okumaz ve değiştirmez.</summary>
        static readonly bool TestRun =
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEDEKTIF_SHOTS")) ||
            Array.IndexOf(Environment.GetCommandLineArgs(), "-dedektifShots") >= 0;

        static string _dirOverride;
        static bool Disabled => TestRun && _dirOverride == null;

        /// <summary>Doğrulama testleri kendi klasöründe çalışır; oyuncunun kayıtlarına dokunmaz.</summary>
        public static void UseDirectory(string dir) { _dirOverride = dir; _latestKnown = false; }

        static string Dir => _dirOverride ?? Path.Combine(Application.persistentDataPath, "Kayitlar");
        static string FileOf(string townId) => Path.Combine(Dir, $"dava_{townId}.json");

        public static bool Exists(string townId)
        {
            if (Disabled || string.IsNullOrEmpty(townId)) return false;
            string file = FileOf(townId);
            return File.Exists(file) || File.Exists(file + ".bak");
        }

        /// <summary>Başarılı olursa true döner. Hata oyunu durdurmaz; önceki kayıt olduğu gibi kalır.</summary>
        public static bool Write(CaseSave save)
        {
            if (Disabled || save == null || string.IsNullOrEmpty(save.townId)) return false;
            string file = FileOf(save.townId);
            string tmp = file + ".tmp";
            try
            {
                Directory.CreateDirectory(Dir);
                save.version = Version;
                save.savedAtUtc = DateTime.UtcNow.ToString("o");
                byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(save));
                using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Flush(true);
                }
                if (File.Exists(file)) File.Replace(tmp, file, file + ".bak", true);
                else File.Move(tmp, file);
                _latestKnown = false;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Kayıt] Yazılamadı ({save.townId}): {e.Message}");
                return false;
            }
        }

        /// <summary>Kaydı okur; asıl dosya bozuksa yedeği dener. İkisi de geçersizse null döner.</summary>
        public static CaseSave Read(string townId)
        {
            if (Disabled || string.IsNullOrEmpty(townId)) return null;
            string file = FileOf(townId);
            return TryRead(file, townId) ?? TryRead(file + ".bak", townId);
        }

        static CaseSave TryRead(string file, string townId)
        {
            try
            {
                if (!File.Exists(file)) return null;
                var save = JsonUtility.FromJson<CaseSave>(File.ReadAllText(file, Encoding.UTF8));
                if (save == null || save.version != Version || save.townId != townId || save.guiltyId == 0) return null;
                return save;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Kayıt] Okunamadı ({Path.GetFileName(file)}): {e.Message}");
                return null;
            }
        }

        public static void Delete(string townId)
        {
            if (Disabled || string.IsNullOrEmpty(townId)) return;
            string file = FileOf(townId);
            foreach (var f in new[] { file, file + ".bak", file + ".tmp" })
            {
                try { if (File.Exists(f)) File.Delete(f); }
                catch (Exception e) { Debug.LogWarning($"[Kayıt] Silinemedi ({Path.GetFileName(f)}): {e.Message}"); }
            }
            _latestKnown = false;
        }

        public static void DeleteAll()
        {
            foreach (var r in GameData.Regions) Delete(r.Id);
        }

        /// <summary>En son kaydedilen kasaba; kayıt yoksa null. Sonuç önbelleklidir, her IMGUI karesinde çağrılabilir.</summary>
        public static string LatestTown()
        {
            if (_latestKnown) return _latest;
            _latest = null;
            _latestKnown = true;
            if (Disabled) return null;
            DateTime best = DateTime.MinValue;
            foreach (var r in GameData.Regions)
            {
                if (!Exists(r.Id)) continue;
                string file = FileOf(r.Id);
                if (!File.Exists(file)) file += ".bak";
                var when = File.GetLastWriteTimeUtc(file);
                if (when > best) { best = when; _latest = r.Id; }
            }
            return _latest;
        }
    }
}
