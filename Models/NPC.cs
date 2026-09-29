namespace DedektiflikRPG.Models;

/// <summary>
/// Kasabadaki şüpheli karakterleri temsil eder.
/// Her NPC'nin güven/korku seviyesi, suçluluk durumu ve sakladığı bir sır vardır.
/// </summary>
public class NPC
{
    public int NPCId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int TrustLevel { get; set; } = 50;   // 0-100 arası, 50 başlangıç
    public int FearLevel { get; set; } = 30;     // 0-100 arası
    public bool IsGuilty { get; set; } = false;
    public string SecretInfo { get; set; } = string.Empty;
    
    // YENİ EKLENEN ÖZELLİKLER (V2)
    public int StressLevel { get; set; } = 0; // Sorguda yükselen stres seviyesi (0-100)
    public string SecondarySecret { get; set; } = string.Empty; // Cinayetle alakasız ikinci bir suç/sır
    public string Rivals { get; set; } = string.Empty; // Suçlayacağı veya düşman olduğu diğer NPC'lerin ID'leri virgülle ayrılmış (örn: "2,4")

    public string BuildingName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string ImageFile { get; set; } = string.Empty;
    public string InteriorFile { get; set; } = string.Empty;
}
