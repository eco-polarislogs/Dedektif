namespace DedektiflikRPG.Models;

/// <summary>
/// Oyuncu ile NPC arasındaki diyalog kaydını temsil eder.
/// Her sorgulama AI tarafından üretilen cevap, duygu ve güven değişimiyle birlikte kaydedilir.
/// </summary>
public class DialogLog
{
    public int LogId { get; set; }
    public int NPCId { get; set; }
    public string PlayerQuestion { get; set; } = string.Empty;
    public string NPCResponse { get; set; } = string.Empty;
    public string DetectedEmotion { get; set; } = string.Empty;
    public int TrustChange { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
