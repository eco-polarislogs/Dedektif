using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DedektiflikRPG.Core.Interfaces;
using DedektiflikRPG.Models;
using DedektiflikRPG.Services;

namespace DedektiflikRPG.Data;

/// <summary>
/// Oyuncuya özel durumu (katil, güven/korku, diyalog geçmişi, çanta) <see cref="PlayerSession"/> içinde tutar;
/// statik veriyi (NPC/ipucu/diyalog havuzları) ortak veritabanından okur.
/// Böylece farklı tarayıcılardaki oyuncular birbirini etkilemez.
/// </summary>
public class SessionGameRepository : IGameRepository
{
    private readonly IGameRepository _inner;
    private readonly PlayerSession _session;

    public SessionGameRepository(IGameRepository inner, PlayerSession session)
    {
        _inner = inner;
        _session = session;
    }

    // ---- Statik / ortak ----
    public Task<bool> TestConnectionAsync() => _inner.TestConnectionAsync();
    public Task<bool> TablesExistAsync() => _inner.TablesExistAsync();
    public Task SeedDataAsync() => _inner.SeedDataAsync();
    public Task EnsureHelperTablesAsync() => _inner.EnsureHelperTablesAsync();
    public Task SeedHelperMessagesAsync() => _inner.SeedHelperMessagesAsync();
    public Task EnsureGolgeSehirTablesAsync() => _inner.EnsureGolgeSehirTablesAsync();
    public Task EnsureSisorenTablesAsync() => _inner.EnsureSisorenTablesAsync();
    public Task<IEnumerable<NPCDialogue>> GetLocalAIPoolAsync(int npcId) => _inner.GetLocalAIPoolAsync(npcId);
    public Task<IEnumerable<HelperMessage>> GetHelperMessagesAsync(string context, string? building = null) => _inner.GetHelperMessagesAsync(context, building);
    public Task<IEnumerable<HelperMessage>> GetGolgeSehirHelperMessagesAsync(string context, string? building = null) => _inner.GetGolgeSehirHelperMessagesAsync(context, building);
    public Task<IEnumerable<NPCDialogue>> GetGolgeSehirDialoguesAsync(int npcId, string? category = null) => _inner.GetGolgeSehirDialoguesAsync(npcId, category);
    public Task<IEnumerable<NPCDialogue>> GetSisorenDialoguesAsync(int npcId, string? category = null) => _inner.GetSisorenDialoguesAsync(npcId, category);
    public Task<string> AnalyzeCluesForHelperAsync(List<int> clueIds, int guiltyNpcId) => _inner.AnalyzeCluesForHelperAsync(clueIds, guiltyNpcId);
    public Task EndGameSessionAsync(int sessionId, string result, int? accusedNpcId, int totalQuestions, int cluesCollected) => _inner.EndGameSessionAsync(sessionId, result, accusedNpcId, totalQuestions, cluesCollected);
    public Task LogPlayerActionAsync(int sessionId, string actionType, int? targetId, string? details) => _inner.LogPlayerActionAsync(sessionId, actionType, targetId, details);
    public Task SaveGameStateAsync(int sessionId, string stateData) => _inner.SaveGameStateAsync(sessionId, stateData);
    public Task<string?> LoadGameStateAsync(int sessionId) => _inner.LoadGameStateAsync(sessionId);

    // ---- NPC'ler (oyuncuya özel katil + güven/korku) ----
    private IEnumerable<NPC> Overlay(IEnumerable<NPC> npcs)
    {
        var list = npcs.ToList();
        foreach (var n in list) _session.ApplyOverlay(n);
        return list;
    }

    private NPC? Overlay(NPC? npc)
    {
        if (npc != null) _session.ApplyOverlay(npc);
        return npc;
    }

    public async Task<IEnumerable<NPC>> GetAllNPCsAsync() => Overlay(await _inner.GetAllNPCsAsync());
    public async Task<NPC?> GetNPCByIdAsync(int id) => Overlay(await _inner.GetNPCByIdAsync(id));
    public async Task<IEnumerable<NPC>> GetGolgeSehirNPCsAsync() => Overlay(await _inner.GetGolgeSehirNPCsAsync());
    public async Task<NPC?> GetGolgeSehirNPCByIdAsync(int id) => Overlay(await _inner.GetGolgeSehirNPCByIdAsync(id));
    public async Task<IEnumerable<NPC>> GetSisorenNPCsAsync() => Overlay(await _inner.GetSisorenNPCsAsync());
    public async Task<NPC?> GetSisorenNPCByIdAsync(int id) => Overlay(await _inner.GetSisorenNPCByIdAsync(id));

    public async Task<IEnumerable<NPC>> GetNPCsByTownAsync(string townId)
    {
        if (townId == PlayerSession.TownSisoren) return await GetSisorenNPCsAsync();
        if (townId == PlayerSession.TownGolge) return await GetGolgeSehirNPCsAsync();
        return await GetAllNPCsAsync();
    }

    public Task UpdateNPCAsync(NPC npc)
    {
        _session.SetNpcStats(npc);
        if (npc.IsGuilty) _session.SetGuiltyId(PlayerSession.TownOfNpc(npc.NPCId), npc.NPCId);
        return Task.CompletedTask;
    }

    public async Task UpdateNPCTrustAsync(int npcId, int trustChange)
    {
        // Başlangıç değerini ortak veriden al, değişimi yalnızca bu oyuncuya uygula
        NPC? baseNpc = npcId >= 200 ? await _inner.GetSisorenNPCByIdAsync(npcId)
                     : npcId >= 100 ? await _inner.GetGolgeSehirNPCByIdAsync(npcId)
                     : await _inner.GetNPCByIdAsync(npcId);
        if (baseNpc == null) return;
        _session.ApplyOverlay(baseNpc);
        _session.ChangeTrust(baseNpc, trustChange);
    }

    // ---- İpuçları / çanta (oyuncuya özel durum) ----
    private IEnumerable<Clue> OverlayClues(IEnumerable<Clue> clues)
    {
        var list = clues.ToList();
        foreach (var c in list) c.Status = _session.GetClueStatus(c.ClueId, "Pending");
        return list;
    }

    public async Task<IEnumerable<Clue>> GetAllCluesAsync() => OverlayClues(await _inner.GetAllCluesAsync());
    public async Task<IEnumerable<Clue>> GetGolgeSehirCluesAsync() => OverlayClues(await _inner.GetGolgeSehirCluesAsync());
    public async Task<IEnumerable<Clue>> GetSisorenCluesAsync() => OverlayClues(await _inner.GetSisorenCluesAsync());

    public async Task<IEnumerable<Clue>> GetCluesByTownAsync(string townId)
    {
        if (townId == PlayerSession.TownSisoren) return await GetSisorenCluesAsync();
        if (townId == PlayerSession.TownGolge) return await GetGolgeSehirCluesAsync();
        return await GetAllCluesAsync();
    }

    public async Task<IEnumerable<Clue>> GetCluesInBagAsync()
    {
        var all = new List<Clue>();
        try { all.AddRange(await GetAllCluesAsync()); } catch { }
        try { all.AddRange(await GetGolgeSehirCluesAsync()); } catch { }
        try { all.AddRange(await GetSisorenCluesAsync()); } catch { }
        return all.Where(c => c.Status == "KeptInBag").OrderBy(c => c.ClueId).ToList();
    }

    public Task UpdateClueStatusAsync(int clueId, string status)
    {
        _session.SetClueStatus(clueId, status);
        return Task.CompletedTask;
    }

    // ---- Diyalog geçmişi ----
    public Task LogDialogWithCategoryAsync(int npcId, string playerQuestion, string npcResponse, int difficulty, string category)
    {
        _session.AddDialog(new DialogLog
        {
            NPCId = npcId,
            PlayerQuestion = playerQuestion,
            NPCResponse = npcResponse,
            DetectedEmotion = category ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        });
        return Task.CompletedTask;
    }

    public Task<IEnumerable<DialogLog>> GetRecentDialogLogsAsync(int npcId, int count)
        => Task.FromResult<IEnumerable<DialogLog>>(_session.RecentDialogs(npcId, count));

    public Task ClearAllDialogLogsAsync()
    {
        _session.ClearDialogs(0, int.MaxValue);
        return Task.CompletedTask;
    }

    public Task ClearDialogLogsByTownAsync(int minNpcId, int maxNpcId)
    {
        _session.ClearDialogs(minNpcId, maxNpcId);
        return Task.CompletedTask;
    }

    public Task ClearPlayerInventoryAsync()
    {
        _session.ClearClues(0, int.MaxValue);
        return Task.CompletedTask;
    }

    public Task ClearPlayerInventoryByTownAsync(int minClueId, int maxClueId)
    {
        _session.ClearClues(minClueId, maxClueId);
        return Task.CompletedTask;
    }

    // ---- Oturum sıfırlama (yalnızca bu oyuncu) ----
    public Task ResetSisorenSessionAsync(int guiltyNpcId)
    {
        _session.ResetTown(PlayerSession.TownSisoren);
        _session.SetGuiltyId(PlayerSession.TownSisoren, guiltyNpcId);
        return Task.CompletedTask;
    }

    public Task ResetGolgeSehirSessionAsync(int guiltyNpcId)
    {
        _session.ResetTown(PlayerSession.TownGolge);
        _session.SetGuiltyId(PlayerSession.TownGolge, guiltyNpcId);
        return Task.CompletedTask;
    }

    public async Task<int> CreateGameSessionAsync(int guiltyNpcId)
    {
        if (guiltyNpcId > 0)
        {
            var town = PlayerSession.TownOfNpc(guiltyNpcId);
            // Kasaba sıfırlaması: yeni katil, temiz durum
            _session.ResetTown(town);
            _session.SetGuiltyId(town, guiltyNpcId);
        }
        return await _inner.CreateGameSessionAsync(guiltyNpcId);
    }
}
