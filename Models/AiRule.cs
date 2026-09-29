using System.Collections.Generic;

namespace DedektiflikRPG.Models;

public class AiRuleSet
{
    public Dictionary<int, NpcAiRules> NPCs { get; set; } = new();
}

public class NpcAiRules
{
    public List<string> Greetings { get; set; } = new();
    public string VictimRelationGuilty { get; set; } = string.Empty;
    public string VictimRelationInnocent { get; set; } = string.Empty;
    public string GeneralInquiryGuilty { get; set; } = string.Empty;
    public string GeneralInquiryInnocent { get; set; } = string.Empty;
    public string AlibiGuilty { get; set; } = string.Empty;
    public string AlibiInnocent { get; set; } = string.Empty;
    public string AccusationGuilty { get; set; } = string.Empty;
    public string AccusationInnocent { get; set; } = string.Empty;
    public string SecretReaction { get; set; } = string.Empty;
}
