using System.Collections.Generic;
using System.Threading.Tasks;
using DedektiflikRPG.Models;

namespace DedektiflikRPG.Core.Interfaces;

public interface IAIService
{
    Task<AIInteractionResponse> GenerateResponseAsync(
        NPC npc, 
        int guiltyNpcId, 
        string userQuestion, 
        IEnumerable<Clue>? cluesInBag = null, 
        IEnumerable<DialogLog>? recentDialogs = null);
}
