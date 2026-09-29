using System.Collections.Generic;

namespace DedektiflikRPG.Models
{
    public class DialogueNode
    {
        public string q { get; set; } = string.Empty;
        public string a { get; set; } = string.Empty;
        public int difficulty { get; set; }
        public string category { get; set; } = string.Empty;
        public List<int> relatedClues { get; set; } = new List<int>();
        public Dictionary<string, string>? guiltyResponse { get; set; }
    }
}
