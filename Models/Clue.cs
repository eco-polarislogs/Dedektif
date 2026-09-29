namespace DedektiflikRPG.Models;

/// <summary>
/// Oyuncu tarafından keşfedilen ipuçlarını temsil eder.
/// Her ipucu bir NPC ile ilişkilendirilebilir.
/// </summary>
public class Clue
{
    public int ClueId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? RelatedNPCId { get; set; }
    public string Status { get; set; } = "Pending"; // "Pending", "KeptInBag", "IgnoredAtScene"
    public string Location { get; set; } = "Olay Yeri";
}
