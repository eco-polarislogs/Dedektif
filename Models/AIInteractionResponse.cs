namespace DedektiflikRPG.Models;

/// <summary>
/// AI servisinden dönen yanıtı temsil eder.
/// NPC'nin diyaloğu, duygu durumu, güven değişimi ve varsa açığa çıkan sır bilgisini içerir.
/// </summary>
public class AIInteractionResponse
{
    public string Dialogue { get; set; } = string.Empty;
    public string Emotion { get; set; } = string.Empty;
    public int TrustChange { get; set; }
    public string? RevealedSecret { get; set; }
    public int StressIncrease { get; set; }
}
