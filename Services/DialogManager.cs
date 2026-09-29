using DedektiflikRPG.Data;
using DedektiflikRPG.Models;

namespace DedektiflikRPG.Services;

/// <summary>
/// Oyuncu ile NPC arasındaki diyaloğu yöneten servis.
/// Veritabanından veri çeker, AI'a gönderir, sonuçları kaydeder.
/// </summary>
public class DialogManager
{
    private readonly DatabaseRepository _repository;
    private readonly AntigravityAiService _aiService;

    public DialogManager(DatabaseRepository repository, AntigravityAiService aiService)
    {
        _repository = repository;
        _aiService = aiService;
    }

    /// <summary>
    /// Oyuncunun sorusunu belirtilen NPC'ye yönlendirir ve tüm süreci yönetir.
    /// 1. NPC ve ipuçlarını veritabanından çeker
    /// 2. AI'dan yanıt alır
    /// 3. NPC güven seviyesini günceller
    /// 4. Diyalog kaydını veritabanına yazar
    /// </summary>
    public async Task<(AIInteractionResponse Response, NPC UpdatedNPC)?> ProcessQuestionAsync(
        int npcId,
        string playerQuestion)
    {
        // NPC'yi getir
        var npc = await _repository.GetNPCByIdAsync(npcId);
        if (npc == null)
            return null;

        // Oyuncunun elindeki tüm ipuçlarını getir
        var allClues = await _repository.GetAllCluesAsync();

        // AI'dan cevap al
        var aiResponse = await _aiService.GetNPCResponseAsync(npc, playerQuestion, allClues);

        // Güven seviyesini güncelle
        await _repository.UpdateNPCTrustAsync(npcId, aiResponse.TrustChange);

        // Diyalog kaydını veritabanına yaz
        var dialogLog = new DialogLog
        {
            NPCId = npcId,
            PlayerQuestion = playerQuestion,
            NPCResponse = aiResponse.Dialogue,
            DetectedEmotion = aiResponse.Emotion,
            TrustChange = aiResponse.TrustChange,
            CreatedAt = DateTime.UtcNow
        };
        await _repository.AddDialogLogAsync(dialogLog);

        // Güncellenmiş NPC'yi getir
        var updatedNPC = await _repository.GetNPCByIdAsync(npcId);

        return (aiResponse, updatedNPC!);
    }

    /// <summary>
    /// Belirli bir NPC ile yapılan geçmiş diyalogları getirir.
    /// </summary>
    public async Task<IEnumerable<DialogLog>> GetDialogHistoryAsync(int npcId)
    {
        return await _repository.GetDialogLogsByNPCIdAsync(npcId);
    }
}
