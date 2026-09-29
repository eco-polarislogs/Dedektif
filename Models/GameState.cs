namespace DedektiflikRPG.Models;

/// <summary>
/// Oyun durumunu kaydetmek/yüklemek için kullanılan model.
/// Tüm oyun state'i JSON olarak saklanır.
/// </summary>
public class GameState
{
    public int StateId { get; set; }
    public int SessionId { get; set; }
    public string StateData { get; set; } = string.Empty; // JSON formatında tüm durum
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}
