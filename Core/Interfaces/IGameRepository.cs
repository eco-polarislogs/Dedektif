using System.Collections.Generic;
using System.Threading.Tasks;
using DedektiflikRPG.Models;

namespace DedektiflikRPG.Core.Interfaces;

public interface IGameRepository
{
    Task<bool> TestConnectionAsync();
    Task<bool> TablesExistAsync();
    Task SeedDataAsync();
    Task EnsureHelperTablesAsync();
    Task SeedHelperMessagesAsync();

    Task<IEnumerable<NPC>> GetAllNPCsAsync();
    Task<NPC?> GetNPCByIdAsync(int id);
    Task UpdateNPCAsync(NPC npc);
    Task UpdateNPCTrustAsync(int npcId, int trustChange);

    Task<IEnumerable<Clue>> GetAllCluesAsync();
    Task<IEnumerable<Clue>> GetCluesInBagAsync();
    Task UpdateClueStatusAsync(int clueId, string status);

    Task LogDialogWithCategoryAsync(int npcId, string playerQuestion, string npcResponse, int difficulty, string category);
    Task<IEnumerable<DialogLog>> GetRecentDialogLogsAsync(int npcId, int count);
    Task ClearAllDialogLogsAsync();
    Task ClearPlayerInventoryAsync();

    Task<IEnumerable<NPCDialogue>> GetLocalAIPoolAsync(int npcId);
    Task<IEnumerable<HelperMessage>> GetHelperMessagesAsync(string context, string? building = null);
    Task<string> AnalyzeCluesForHelperAsync(List<int> clueIds, int guiltyNpcId);

    Task<int> CreateGameSessionAsync(int guiltyNpcId);
    Task EndGameSessionAsync(int sessionId, string result, int? accusedNpcId, int totalQuestions, int cluesCollected);
    Task LogPlayerActionAsync(int sessionId, string actionType, int? targetId, string? details);
    Task SaveGameStateAsync(int sessionId, string stateData);
    Task<string?> LoadGameStateAsync(int sessionId);

    // =============================================
    // Gölge Şehir Özel Metotları
    // =============================================
    Task EnsureGolgeSehirTablesAsync();
    Task<IEnumerable<NPC>> GetGolgeSehirNPCsAsync();
    Task<NPC?> GetGolgeSehirNPCByIdAsync(int id);
    Task<IEnumerable<Clue>> GetGolgeSehirCluesAsync();
    Task<IEnumerable<NPCDialogue>> GetGolgeSehirDialoguesAsync(int npcId, string? category = null);
    Task<IEnumerable<HelperMessage>> GetGolgeSehirHelperMessagesAsync(string context, string? building = null);
    Task ResetGolgeSehirSessionAsync(int guiltyNpcId);
    Task EnsureSisorenTablesAsync();
    Task<IEnumerable<NPCDialogue>> GetSisorenDialoguesAsync(int npcId, string? category = null);
}
