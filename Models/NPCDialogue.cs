namespace DedektiflikRPG.Models;

public class NPCDialogue
{
    public int DialogueId { get; set; }
    public int NPCId { get; set; }
    public int Difficulty { get; set; } = 1;
    public string Category { get; set; } = "tanisma";
    public int ButtonIndex { get; set; } = 0;
    public string PlayerText { get; set; } = "";
    public string NPCResponse { get; set; } = "";
    public string? GuiltyResponses { get; set; }
    public string? RelatedClueIds { get; set; }
    public bool IsAccusatory { get; set; } = false;
}
