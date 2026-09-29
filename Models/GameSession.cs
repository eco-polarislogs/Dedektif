namespace DedektiflikRPG.Models;

/// <summary>
/// Oyun oturumunu temsil eder.
/// Her yeni oyun başlangıcında bir oturum oluşturulur ve sonuç kaydedilir.
/// </summary>
public class GameSession
{
    public int SessionId { get; set; }
    public int GuiltyNPCId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public string? Result { get; set; }        // "won", "lost", "abandoned"
    public int? AccusedNPCId { get; set; }
    public int TotalQuestions { get; set; } = 0;
    public int CluesCollected { get; set; } = 0;
}
