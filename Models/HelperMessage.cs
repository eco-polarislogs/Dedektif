namespace DedektiflikRPG.Models;

/// <summary>
/// Yardımcı Dedektif Çetin'in bağlam-tabanlı mesajlarını temsil eder.
/// Her mesaj belirli bir oyun bağlamında (binaya giriş, çanta açma vs.) gösterilir.
/// </summary>
public class HelperMessage
{
    public int MessageId { get; set; }
    public string Speaker { get; set; } = "cetin";             // "cetin" veya "rifat"
    public string Context { get; set; } = string.Empty;       // "splash", "story_end", "map_enter", "building_enter", "bag_open", "clue_inspect", "npc_talk", "autopsy_ready", "accuse"
    public string? BuildingName { get; set; }                  // Bina-bazlı mesajlar için (opsiyonel)
    public string Message { get; set; } = string.Empty;
    public int Priority { get; set; } = 1;
    public bool IsOneTime { get; set; } = true;                // Sadece bir kez gösterilsin mi?
}
