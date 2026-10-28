using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DedektiflikRPG.Core.Interfaces;
using DedektiflikRPG.Models;
using DedektiflikRPG.Services.Forensic;

namespace DedektiflikRPG.Services;

/// <summary>
/// Tek bir oyuncuya (tarayıcıya) ait tüm oyun durumu. Başka oyuncularla ASLA paylaşılmaz.
/// </summary>
public class PlayerSession
{
    public const string TownGizemli = "gizemli";
    public const string TownGolge = "golge_sehir";
    public const string TownSisoren = "sisoren";

    private readonly object _lock = new();
    private readonly Dictionary<string, int> _guilty = new();
    private readonly Dictionary<int, (int Trust, int Fear, int Stress)> _npcStats = new();
    private readonly Dictionary<int, string> _clueStatus = new();
    private readonly List<DialogLog> _dialogs = new();

    public string Id { get; }
    public DateTime LastSeenUtc { get; set; } = DateTime.UtcNow;
    public IForensicService Forensic { get; } = new ForensicService();

    public PlayerSession(string id) { Id = id; }

    public static string TownOfNpc(int npcId) => npcId >= 200 ? TownSisoren : (npcId >= 100 ? TownGolge : TownGizemli);
    public static string TownOfClue(int clueId) => clueId >= 2000 ? TownSisoren : (clueId >= 1000 ? TownGolge : TownGizemli);

    private static int RandomGuilty(string town) => town switch
    {
        TownSisoren => Random.Shared.Next(201, 214),
        TownGolge => Random.Shared.Next(101, 109),
        _ => Random.Shared.Next(1, 6)
    };

    public int GetGuiltyId(string town)
    {
        lock (_lock)
        {
            if (!_guilty.TryGetValue(town, out var id))
            {
                id = RandomGuilty(town);
                _guilty[town] = id;
            }
            return id;
        }
    }

    public void SetGuiltyId(string town, int npcId)
    {
        lock (_lock) { _guilty[town] = npcId; }
    }

    // ---- NPC istatistikleri ----
    public void ApplyOverlay(NPC npc)
    {
        npc.IsGuilty = npc.NPCId == GetGuiltyId(TownOfNpc(npc.NPCId));
        lock (_lock)
        {
            if (_npcStats.TryGetValue(npc.NPCId, out var s))
            {
                npc.TrustLevel = s.Trust;
                npc.FearLevel = s.Fear;
                npc.StressLevel = s.Stress;
            }
        }
    }

    public void SetNpcStats(NPC npc)
    {
        lock (_lock) { _npcStats[npc.NPCId] = (npc.TrustLevel, npc.FearLevel, npc.StressLevel); }
    }

    public void ChangeTrust(NPC baseNpc, int change)
    {
        lock (_lock)
        {
            var cur = _npcStats.TryGetValue(baseNpc.NPCId, out var s) ? s : (baseNpc.TrustLevel, baseNpc.FearLevel, baseNpc.StressLevel);
            _npcStats[baseNpc.NPCId] = (Math.Clamp(cur.Item1 + change, 0, 100), cur.Item2, cur.Item3);
        }
    }

    public void ResetTown(string town)
    {
        lock (_lock)
        {
            foreach (var id in _npcStats.Keys.Where(k => TownOfNpc(k) == town).ToList()) _npcStats.Remove(id);
            foreach (var id in _clueStatus.Keys.Where(k => TownOfClue(k) == town).ToList()) _clueStatus.Remove(id);
            _dialogs.RemoveAll(d => TownOfNpc(d.NPCId) == town);
        }
    }

    // ---- İpuçları ----
    public string GetClueStatus(int clueId, string fallback)
    {
        lock (_lock) { return _clueStatus.TryGetValue(clueId, out var s) ? s : (fallback ?? "Pending"); }
    }

    public void SetClueStatus(int clueId, string status)
    {
        lock (_lock) { _clueStatus[clueId] = status; }
    }

    public void ClearClues(int minId, int maxId)
    {
        lock (_lock)
        {
            foreach (var id in _clueStatus.Keys.Where(k => k >= minId && k <= maxId).ToList()) _clueStatus.Remove(id);
        }
    }

    // ---- Diyaloglar ----
    public void AddDialog(DialogLog log)
    {
        lock (_lock) { _dialogs.Add(log); }
    }

    public List<DialogLog> RecentDialogs(int npcId, int count)
    {
        lock (_lock)
        {
            return _dialogs.Where(d => d.NPCId == npcId).OrderByDescending(d => d.CreatedAt).Take(count).ToList();
        }
    }

    public void ClearDialogs(int minNpcId, int maxNpcId)
    {
        lock (_lock) { _dialogs.RemoveAll(d => d.NPCId >= minNpcId && d.NPCId <= maxNpcId); }
    }
}

/// <summary>Tüm oyuncu oturumlarını tutan singleton. Boşta kalan oturumlar otomatik temizlenir.</summary>
public class PlayerSessionStore
{
    private static readonly TimeSpan Idle = TimeSpan.FromHours(6);
    private readonly ConcurrentDictionary<string, PlayerSession> _sessions = new();
    private DateTime _lastSweep = DateTime.UtcNow;

    public PlayerSession GetOrCreate(string? sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId) || sessionId.Length > 64) sessionId = "anonymous";
        Sweep();
        var s = _sessions.GetOrAdd(sessionId, id => new PlayerSession(id));
        s.LastSeenUtc = DateTime.UtcNow;
        return s;
    }

    private void Sweep()
    {
        var now = DateTime.UtcNow;
        if (now - _lastSweep < TimeSpan.FromMinutes(10)) return;
        _lastSweep = now;
        foreach (var kv in _sessions.Where(kv => now - kv.Value.LastSeenUtc > Idle).ToList())
            _sessions.TryRemove(kv.Key, out _);
    }
}
