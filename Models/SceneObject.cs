namespace DedektiflikRPG.Models
{
    public class SceneObject
    {
        public int ObjectId { get; set; }
        public int NPCId { get; set; }
        public string ObjectName { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImageFile { get; set; } = "";
        public string PosTop { get; set; } = "50%";
        public string PosLeft { get; set; } = "50%";
        public bool IsDiscovered { get; set; }
    }
}
