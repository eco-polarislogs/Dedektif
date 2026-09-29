using System.Collections.Generic;
using System.Threading.Tasks;
using DedektiflikRPG.Models;

namespace DedektiflikRPG.Core.Interfaces;

public interface IForensicService
{
    void SubmitFinding(int clueId, string clueName, string findingText, List<NPC> npcs, int guiltyId);
    Task<string> GenerateAutopsyReportAsync(List<NPC> npcs, int guiltyId);
    object GetForensicState(int guiltyId);
    string GetDynamicClueDetail(int clueId, int guiltyId);
    void ClearFindings();
    void ClearGolgeFindings();
    void ClearGizemliFindings();
    void ClearSisorenFindings();
}
