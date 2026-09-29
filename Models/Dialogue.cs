namespace DedektiflikRPG.Models
{
    public class Dialogue
    {
        public int DialogueId { get; set; }
        public int NPCId { get; set; }
        public int? ParentId { get; set; }
        public string PlayerText { get; set; } = "";
        public string NPCText { get; set; } = "";
        public int TrustChange { get; set; }
        public int FearChange { get; set; }
        public int? RequiredObjectId { get; set; }
    }
}
