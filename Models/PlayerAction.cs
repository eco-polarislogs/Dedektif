namespace DedektiflikRPG.Models;

/// <summary>
/// Oyuncu aksiyonlarını temsil eder.
/// Bina girme, delil toplama, soru sorma, suçlama gibi aksiyonlar kaydedilir.
/// </summary>
public class PlayerAction
{
    public int ActionId { get; set; }
    public int SessionId { get; set; }
    public string ActionType { get; set; } = string.Empty; // "enter_building", "collect_clue", "ask_question", "accuse", "open_bag", "inspect_clue"
    public int? TargetId { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
