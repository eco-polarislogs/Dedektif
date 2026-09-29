using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using DedektiflikRPG.Core.Interfaces;
using DedektiflikRPG.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DedektiflikRPG.Controllers;

public static class GameEndpoints
{
    private static readonly System.Threading.SemaphoreSlim _resetSemaphore = new System.Threading.SemaphoreSlim(1, 1);

    public static void MapUnifiedEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/towns/{townId}/npcs", async (string townId, IGameRepository repo) =>
        {
            try
            {
                var npcs = await repo.GetNPCsByTownAsync(townId);
                return Results.Ok(npcs);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        app.MapGet("/api/v1/towns/{townId}/clues", async (string townId, IGameRepository repo) =>
        {
            try
            {
                var clues = await repo.GetCluesByTownAsync(townId);
                return Results.Ok(clues);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        app.MapPost("/api/v1/towns/{townId}/interrogate", async (string townId, InterrogationRequest request, IGameRepository repo, IAIService aiService) =>
        {
            try
            {
                if (request == null || request.NpcId <= 0 || string.IsNullOrWhiteSpace(request.Question))
                    return Results.BadRequest("Geçersiz sorgulama isteği.");

                // townId based logic
                NPC? npc = null;
                var npcs = await repo.GetNPCsByTownAsync(townId);
                var guiltyId = npcs.FirstOrDefault(n => n.IsGuilty)?.NPCId ?? 0;

                if (townId == "sisoren")
                {
                    try { npc = await repo.GetSisorenNPCByIdAsync(request.NpcId); } catch { }
                    npc ??= request.NpcId >= 300 ? GetFallbackSisorenExtraNPC(request.NpcId) : GetFallbackSisorenNPC(request.NpcId);
                    if (npc == null) return Results.NotFound("Sisören şüphelisi bulunamadı.");
                }
                else if (townId == "golge_sehir")
                {
                    try { npc = await repo.GetGolgeSehirNPCByIdAsync(request.NpcId); } catch { }
                    npc ??= GetFallbackGolgeNPC(request.NpcId);
                    if (npc == null) return Results.NotFound("Gölge Şehir şüphelisi bulunamadı.");
                }
                else
                {
                    try { npc = await repo.GetNPCByIdAsync(request.NpcId); } catch { }
                    npc ??= GetFallbackGizemliNPC(request.NpcId);
                    if (npc == null) return Results.NotFound("Şüpheli bulunamadı.");
                }

                IEnumerable<Clue> cluesInBag = new List<Clue>();
                try { cluesInBag = await repo.GetCluesInBagAsync(); } catch { }

                IEnumerable<DialogLog> recentDialogs = new List<DialogLog>();
                try { recentDialogs = await repo.GetRecentDialogLogsAsync(npc.NPCId, 5); } catch { }

                var response = await aiService.GenerateResponseAsync(npc, guiltyId, request.Question, cluesInBag, recentDialogs);

                try
                {
                    await repo.LogDialogWithCategoryAsync(npc.NPCId, request.Question, response.Dialogue, 1, $"{townId}_ai");
                }
                catch { }

                return Results.Ok(new
                {
                    success = true,
                    dialogue = response.Dialogue,
                    emotion = response.Emotion,
                    trustChange = response.TrustChange,
                    stressIncrease = response.StressIncrease,
                    npcId = npc.NPCId,
                    town = townId
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        app.MapPost("/api/v1/towns/{townId}/accuse", async (string townId, AccuseRequest request, IGameRepository repo) =>
        {
            if (request == null || request.NpcId <= 0)
                return Results.BadRequest("Geçersiz suçlama isteği.");

            var npcs = (await repo.GetNPCsByTownAsync(townId)).ToList();
            var guiltyNpc = npcs.FirstOrDefault(n => n.IsGuilty);
            int guiltyId = guiltyNpc?.NPCId ?? 0;

            if (townId == "sisoren" && request.NpcId >= 300)
            {
                return Results.Ok(new 
                { 
                    success = false, 
                    message = "Bu kişi suçlanamayacak bir kasabalı/tanıktır ve tamamen masumdur!", 
                    accusedName = "Kasabalı / Tanık", 
                    guiltyNpcName = GetSisorenNPCName(guiltyId), 
                    guiltyNpcId = guiltyId 
                });
            }

            bool isGuilty = (request.NpcId == guiltyId);
            string accusedName = npcs.FirstOrDefault(n => n.NPCId == request.NpcId)?.Name ?? "Şüpheli";
            string guiltyName = guiltyNpc?.Name ?? "Bilinmeyen Katil";

            if (isGuilty)
            {
                return Results.Ok(new 
                { 
                    success = true, 
                    message = $"Tebrikler! {townId} gerçek katilinin {accusedName} olduğunu kanıtladınız!", 
                    accusedName = accusedName, 
                    guiltyNpcName = guiltyName, 
                    guiltyNpcId = guiltyId 
                });
            }
            else
            {
                return Results.Ok(new 
                { 
                    success = false, 
                    message = $"{accusedName} masum çıktı! {townId} gerçek katili {guiltyName} idi.", 
                    accusedName = accusedName, 
                    guiltyNpcName = guiltyName, 
                    guiltyNpcId = guiltyId 
                });
            }
        });

        app.MapPost("/api/v1/towns/{townId}/reset", async (string townId, IGameRepository repo, IForensicService forensicService) =>
        {
            var rnd = new Random();
            int guiltyId = 0;
            
            if (townId == "sisoren")
            {
                guiltyId = rnd.Next(201, 214);
                await repo.ResetSisorenSessionAsync(guiltyId);
                forensicService.ClearSisorenFindings();
                await repo.ClearDialogLogsByTownAsync(200, 999);
                await repo.ClearPlayerInventoryByTownAsync(2000, 9999);
            }
            else if (townId == "golge_sehir")
            {
                guiltyId = rnd.Next(101, 109);
                await repo.ResetGolgeSehirSessionAsync(guiltyId);
                forensicService.ClearGolgeFindings();
                await repo.ClearDialogLogsByTownAsync(100, 199);
                await repo.ClearPlayerInventoryByTownAsync(1000, 1999);
            }
            else
            {
                guiltyId = rnd.Next(1, 6);
                await repo.CreateGameSessionAsync(guiltyId);
                forensicService.ClearFindings();
                await repo.ClearDialogLogsByTownAsync(1, 99);
                await repo.ClearPlayerInventoryByTownAsync(1, 999);
            }

            return Results.Ok(new 
            { 
                success = true, 
                message = $"{townId} sıfırlandı ve yeni suçlu belirlendi.", 
                guiltyNpcId = guiltyId
            });
        });
    }

    public static void MapGameEndpoints(this IEndpointRouteBuilder app)
    {
        // 1. Şüpheli Listesi
        app.MapGet("/api/game/npcs", async (IGameRepository repo) =>
        {
            try
            {
                var npcs = await repo.GetAllNPCsAsync();
                return Results.Ok(npcs);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        // 2. İpuçları Listesi
        app.MapGet("/api/game/clues", async (IGameRepository repo) =>
        {
            try
            {
                var clues = await repo.GetAllCluesAsync();
                return Results.Ok(clues);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        // 3. İpucu Durumu Güncelle
        app.MapPost("/api/game/clues/{id}/action", async (int id, ClueActionRequest request, IGameRepository repo) =>
        {
            try
            {
                if (request.Status != "KeptInBag" && request.Status != "IgnoredAtScene" && request.Status != "Pending")
                {
                    return Results.BadRequest("Geçersiz durum değeri.");
                }

                await repo.UpdateClueStatusAsync(id, request.Status);
                return Results.Ok(new { success = true, clueId = id, status = request.Status });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        // 4. Sorgulama Yap (Geliştirilmiş Yerel Yapay Zeka Motoru)
        app.MapPost("/api/game/interrogate", async (InterrogationRequest request, IGameRepository repo, IAIService aiService) =>
        {
            try
            {
                // Gölge Şehir NPC'leri (101 - 108)
                if (request.NpcId >= 100 && request.NpcId < 200)
                {
                    NPC? golgeNpc = null;
                    try { golgeNpc = await repo.GetGolgeSehirNPCByIdAsync(request.NpcId); } catch { }
                    golgeNpc ??= GetFallbackGolgeNPC(request.NpcId);
                    if (golgeNpc == null) return Results.NotFound("Gölge Şehir şüphelisi bulunamadı.");

                    // Katil ID'sini sunucu tarafında veritabanından çözümle (güvenlik)
                    var golgeNpcList = (await repo.GetGolgeSehirNPCsAsync()).ToList();
                    int guiltyIdGolge = golgeNpcList.FirstOrDefault(n => n.IsGuilty)?.NPCId ?? 101;

                    IEnumerable<Clue> cluesInBagGolge = new List<Clue>();
                    try { cluesInBagGolge = await repo.GetCluesInBagAsync(); } catch { }

                    IEnumerable<DialogLog> recentDialogsGolge = new List<DialogLog>();
                    try { recentDialogsGolge = await repo.GetRecentDialogLogsAsync(golgeNpc.NPCId, 5); } catch { }

                    var responseGolge = await aiService.GenerateResponseAsync(golgeNpc, guiltyIdGolge, request.Question, cluesInBagGolge, recentDialogsGolge);

                    try
                    {
                        if (responseGolge.TrustChange != 0)
                        {
                            await repo.UpdateNPCTrustAsync(golgeNpc.NPCId, responseGolge.TrustChange);
                            golgeNpc = await repo.GetGolgeSehirNPCByIdAsync(request.NpcId) ?? golgeNpc;
                        }

                        await repo.LogDialogWithCategoryAsync(golgeNpc.NPCId, request.Question, responseGolge.Dialogue, 1, "golge_local_ai");
                    }
                    catch { }

                    return Results.Ok(new
                    {
                        success = true,
                        dialogue = responseGolge.Dialogue,
                        emotion = responseGolge.Emotion,
                        trustChange = responseGolge.TrustChange,
                        stressIncrease = responseGolge.StressIncrease,
                        revealedSecret = responseGolge.RevealedSecret ?? "",
                        updatedNpc = golgeNpc
                    });
                }

                // Gizemli Kasaba NPC'leri (1 - 5)
                NPC? npc = null;
                try { npc = await repo.GetNPCByIdAsync(request.NpcId); } catch { }
                npc ??= GetFallbackGizemliNPC(request.NpcId);
                if (npc == null) return Results.NotFound("Şüpheli bulunamadı.");

                // Katil ID'sini sunucu tarafında veritabanından çözümle (güvenlik)
                var gizemliNpcList = (await repo.GetAllNPCsAsync()).ToList();
                int guiltyId = gizemliNpcList.FirstOrDefault(n => n.IsGuilty)?.NPCId ?? 1;

                IEnumerable<Clue> cluesInBag = new List<Clue>();
                try { cluesInBag = await repo.GetCluesInBagAsync(); } catch { }

                IEnumerable<DialogLog> recentDialogs = new List<DialogLog>();
                try { recentDialogs = await repo.GetRecentDialogLogsAsync(npc.NPCId, 5); } catch { }

                var response = await aiService.GenerateResponseAsync(npc, guiltyId, request.Question, cluesInBag, recentDialogs);

                try
                {
                    if (response.TrustChange != 0)
                    {
                        await repo.UpdateNPCTrustAsync(npc.NPCId, response.TrustChange);
                        npc = await repo.GetNPCByIdAsync(request.NpcId) ?? npc;
                    }

                    await repo.LogDialogWithCategoryAsync(npc.NPCId, request.Question, response.Dialogue, 1, "local_ai");
                }
                catch { }

                    return Results.Ok(new
                    {
                        success = true,
                        dialogue = response.Dialogue,
                        emotion = response.Emotion,
                        trustChange = response.TrustChange,
                        stressIncrease = response.StressIncrease,
                        revealedSecret = response.RevealedSecret ?? "",
                        updatedNpc = npc
                    });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        // 5. Suçlamada Bulun
        app.MapPost("/api/game/accuse", async (AccuseRequest request, IGameRepository repo) =>
        {
            try
            {
                // Gölge Şehir Suçlaması
                if (request.NpcId >= 100)
                {
                    var golgeNpc = await repo.GetGolgeSehirNPCByIdAsync(request.NpcId);
                    if (golgeNpc == null) return Results.NotFound("Gölge Şehir şüphelisi bulunamadı.");

                    var golgeNPCs = (await repo.GetGolgeSehirNPCsAsync()).ToList();
                    var guiltyGolge = golgeNPCs.FirstOrDefault(n => n.IsGuilty);
                    
                    int guiltyIdGolge = guiltyGolge?.NPCId ?? 101;

                    var actualGuiltyNpc = golgeNPCs.FirstOrDefault(n => n.NPCId == guiltyIdGolge) ?? guiltyGolge;
                    var guiltyNameGolge = actualGuiltyNpc?.Name ?? "Bilinmiyor";

                    bool isGuilty = (request.NpcId == guiltyIdGolge) || golgeNpc.IsGuilty;

                    if (isGuilty)
                    {
                        return Results.Ok(new { success = true, message = $"Tebrikler! Gölge Şehir katilinin {golgeNpc.Name} olduğunu kanıtladınız!", accusedName = golgeNpc.Name, guiltyNpcName = guiltyNameGolge, guiltyNpcId = guiltyIdGolge });
                    }
                    else
                    {
                        return Results.Ok(new { success = false, message = $"{golgeNpc.Name} masum çıktı! Gölge Şehir'in gerçek katili {guiltyNameGolge} idi.", accusedName = golgeNpc.Name, guiltyNpcName = guiltyNameGolge, guiltyNpcId = guiltyIdGolge });
                    }
                }

                // Gizemli Kasaba Suçlaması
                var npc = await repo.GetNPCByIdAsync(request.NpcId);
                if (npc == null) return Results.NotFound("Şüpheli bulunamadı.");

                var npcs = (await repo.GetAllNPCsAsync()).ToList();
                var guiltyNpc = npcs.FirstOrDefault(n => n.IsGuilty);

                int guiltyId = guiltyNpc?.NPCId ?? 1;

                var actualGuilty = npcs.FirstOrDefault(n => n.NPCId == guiltyId) ?? guiltyNpc;
                var guiltyName = actualGuilty?.Name ?? "Bilinmiyor";

                bool isGuiltyPerson = (request.NpcId == guiltyId) || npc.IsGuilty;

                if (isGuiltyPerson)
                {
                    return Results.Ok(new { success = true, message = $"Tebrikler! Suçlunun {npc.Name} olduğunu doğru tahmin ettiniz.", accusedName = npc.Name, guiltyNpcName = guiltyName, guiltyNpcId = guiltyId });
                }
                else
                {
                    return Results.Ok(new { success = false, message = $"{npc.Name} masum çıktı! Gerçek katil {guiltyName} idi.", accusedName = npc.Name, guiltyNpcName = guiltyName, guiltyNpcId = guiltyId });
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        // 6. Adli Tıbba Gönder
        app.MapPost("/api/game/forensic/submit", async (ForensicSubmitRequest request, IGameRepository repo, IForensicService forensicService) =>
        {
            try
            {
                if (request == null || request.ClueId <= 0)
                {
                    return Results.Ok(new { success = true });
                }

                bool isSisoren = request.ClueId >= 2000;
                bool isGolge = !isSisoren && request.ClueId >= 1000;
                List<NPC> npcs;
                try
                {
                    if (isSisoren)
                        npcs = (await repo.GetSisorenNPCsAsync()).ToList();
                    else if (isGolge)
                        npcs = (await repo.GetGolgeSehirNPCsAsync()).ToList();
                    else
                        npcs = (await repo.GetAllNPCsAsync()).ToList();
                }
                catch
                {
                    npcs = new List<NPC>();
                }

                var guiltyNpc = npcs.FirstOrDefault(n => n.IsGuilty);
                int guiltyId = guiltyNpc?.NPCId ?? (isSisoren ? 201 : (isGolge ? 101 : 1));

                forensicService.SubmitFinding(request.ClueId, request.ClueName ?? "", request.FindingText ?? "", npcs, guiltyId);
                return Results.Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return Results.Ok(new { success = true, warning = ex.Message });
            }
        });

        // 7. Otopsi Raporu Getir
        app.MapGet("/api/game/autopsy", async (string? town, IGameRepository repo, IForensicService forensicService) =>
        {
            try
            {
                bool isGolge = town == "golge_sehir";
                bool isSisoren = town == "sisoren";
                List<NPC> npcs;
                try
                {
                    if (isSisoren)
                    {
                        npcs = (await repo.GetSisorenNPCsAsync()).ToList();
                        if (npcs.Count == 0) npcs = Enumerable.Range(201, 13).Select(id => GetFallbackSisorenNPC(id)).Where(n => n != null).Cast<NPC>().ToList();
                    }
                    else if (isGolge)
                    {
                        npcs = (await repo.GetGolgeSehirNPCsAsync()).ToList();
                    }
                    else
                    {
                        npcs = (await repo.GetAllNPCsAsync()).ToList();
                    }
                }
                catch
                {
                    npcs = new List<NPC>();
                }

                int guiltyId;
                if (isSisoren)
                {
                    var guiltyNpc = npcs.FirstOrDefault(n => n.IsGuilty);
                    guiltyId = guiltyNpc?.NPCId ?? 201;
                }
                else
                {
                    var guiltyNpc = npcs.FirstOrDefault(n => n.IsGuilty);
                    guiltyId = guiltyNpc?.NPCId ?? (isGolge ? 101 : 1);
                }

                string reportHtml = await forensicService.GenerateAutopsyReportAsync(npcs, guiltyId);
                return Results.Ok(new { success = true, report = reportHtml, guiltyId });
            }
            catch (Exception ex)
            {
                return Results.Ok(new { success = false, error = ex.Message });
            }
        });

        // 8. Adli Lab State
        app.MapGet("/api/game/forensic-state", async (string? town, IGameRepository repo, IForensicService forensicService) =>
        {
            try
            {
                bool isGolge = town == "golge_sehir";
                bool isSisoren = town == "sisoren";
                List<NPC> npcs;
                try
                {
                    if (isSisoren)
                    {
                        npcs = (await repo.GetSisorenNPCsAsync()).ToList();
                        if (npcs.Count == 0) npcs = Enumerable.Range(201, 13).Select(id => GetFallbackSisorenNPC(id)).Where(n => n != null).Cast<NPC>().ToList();
                    }
                    else if (isGolge)
                    {
                        npcs = (await repo.GetGolgeSehirNPCsAsync()).ToList();
                    }
                    else
                    {
                        npcs = (await repo.GetAllNPCsAsync()).ToList();
                    }
                }
                catch
                {
                    npcs = new List<NPC>();
                }

                int guiltyId;
                if (isSisoren)
                {
                    var guiltyNpc = npcs.FirstOrDefault(n => n.IsGuilty);
                    guiltyId = guiltyNpc?.NPCId ?? 201;
                }
                else
                {
                    var guiltyNpc = npcs.FirstOrDefault(n => n.IsGuilty);
                    guiltyId = guiltyNpc?.NPCId ?? (isGolge ? 101 : 1);
                }

                var state = forensicService.GetForensicState(guiltyId);
                return Results.Ok(state);
            }
            catch (Exception ex)
            {
                return Results.Ok(new { success = true, guiltyId = 1, weaponId = 1, fingerprintId = 2 });
            }
        });

        // 9. Dinamik İpucu Detayı
        app.MapGet("/api/game/clue-detail/{clueId}", async (int clueId, IGameRepository repo, IForensicService forensicService) =>
        {
            try
            {
                bool isGolge = clueId >= 1000;
                var npcs = isGolge
                    ? (await repo.GetGolgeSehirNPCsAsync()).ToList()
                    : (await repo.GetAllNPCsAsync()).ToList();

                var guiltyNpc = npcs.FirstOrDefault(n => n.IsGuilty);
                int guiltyId = guiltyNpc?.NPCId ?? (isGolge ? 101 : 1);

                string detail = forensicService.GetDynamicClueDetail(clueId, guiltyId);
                return Results.Ok(new { success = true, text = detail });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        // 10. Oyunu Sıfırla (Gizemli Kasaba - Farklı Katil Seçimi Garantili)
        app.MapPost("/api/game/reset", async (IGameRepository repo, IForensicService forensicService) =>
        {
            try
            {
                forensicService.ClearGizemliFindings();
                var npcs = (await repo.GetAllNPCsAsync()).ToList();
                var previousGuilty = npcs.FirstOrDefault(n => n.IsGuilty)?.NPCId ?? 0;

                var random = new Random();
                var candidateIds = Enumerable.Range(1, 5).Where(id => id != previousGuilty).ToList();
                int guiltyId = candidateIds.Count > 0 ? candidateIds[random.Next(candidateIds.Count)] : random.Next(1, 6);

                foreach (var npc in npcs)
                {
                    npc.IsGuilty = (npc.NPCId == guiltyId);
                    npc.TrustLevel = 50;
                    npc.FearLevel = 30;
                    await repo.UpdateNPCAsync(npc);
                }

                await repo.ClearDialogLogsByTownAsync(1, 99);
                await repo.ClearPlayerInventoryByTownAsync(1, 999);

                return Results.Ok(new { success = true, message = "Oyun durumu sıfırlandı ve yeni suçlu belirlendi.", guiltyNpcId = guiltyId });
            }
            catch (Exception ex)
            {
                var fallbackGuilty = new Random().Next(1, 6);
                return Results.Ok(new { success = true, message = "Oyun sıfırlandı (offline mod).", guiltyNpcId = fallbackGuilty });
            }
        });

        // 11. Diyalog Soruları Getir
        app.MapGet("/api/game/dialogues", async (int npcId, string category, IGameRepository repo) =>
        {
            try
            {
                if ((npcId >= 201 && npcId <= 213) || (npcId >= 301 && npcId <= 318))
                {
                    var sisorenDialogues = (await repo.GetSisorenDialoguesAsync(npcId, null)).ToList();
                    sisorenDialogues = sisorenDialogues.OrderBy(_ => Random.Shared.Next()).Take(4).ToList();
                    return Results.Ok(new
                    {
                        success = true,
                        dialogues = sisorenDialogues.Select(d => new
                        {
                            q = d.PlayerText,
                            a = d.NPCResponse,
                            response = d.NPCResponse,
                            category = d.Category,
                            difficulty = d.Difficulty
                        })
                    });
                }

                // Gölge Şehir NPC'leri (101 - 108) Veritabanı sorgusu
                if (npcId >= 100)
                {
                    var golgeDialogues = (await repo.GetGolgeSehirDialoguesAsync(npcId, category)).ToList();
                    if (golgeDialogues.Count == 0)
                    {
                        golgeDialogues = (await repo.GetGolgeSehirDialoguesAsync(npcId, null)).ToList();
                    }

                    var rndG = new Random();
                    var selectedGolge = golgeDialogues.OrderBy(x => rndG.Next()).Take(4).Select(d => new {
                        q = d.PlayerText,
                        a = d.NPCResponse,
                        response = d.NPCResponse,
                        type = d.Category,
                        category = d.Category,
                        difficulty = d.Difficulty
                    }).ToList();

                    return Results.Ok(new { success = true, dialogues = selectedGolge });
                }

                // Gizemli Kasaba NPC'leri (1 - 5)
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "dialogues.json");
                if (!File.Exists(filePath)) return Results.NotFound("Diyalog dosyası bulunamadı.");

                var jsonStr = File.ReadAllText(filePath);
                var allDialogues = JsonSerializer.Deserialize<Dictionary<string, List<DialogueNode>>>(jsonStr);

                if (allDialogues != null && allDialogues.TryGetValue(npcId.ToString(), out var npcDialogues))
                {
                    var contextualPool = npcDialogues.Where(d => d.category == category).ToList();
                    var questionsToShow = contextualPool.Count > 0 ? contextualPool : npcDialogues;

                    var rnd = new Random();
                    var count = Math.Min(4, questionsToShow.Count);
                    var selected = questionsToShow.OrderBy(x => rnd.Next()).Take(count).ToList();

                    return Results.Ok(new { success = true, dialogues = selected });
                }

                return Results.NotFound("NPC diyalogları bulunamadı.");
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        // 12. Oturum Başlat
        app.MapPost("/api/game/session/start", async (SessionStartRequest request, IGameRepository repo) =>
        {
            try
            {
                var sessionId = await repo.CreateGameSessionAsync(request.GuiltyNpcId);
                return Results.Ok(new { success = true, sessionId = sessionId });
            }
            catch (Exception ex)
            {
                return Results.Ok(new { success = false, sessionId = 0, error = ex.Message });
            }
        });

        // 13. Oturum Sonlandır
        app.MapPost("/api/game/session/end", async (SessionEndRequest request, IGameRepository repo) =>
        {
            try
            {
                await repo.EndGameSessionAsync(request.SessionId, request.Result, request.AccusedNpcId, request.TotalQuestions, request.CluesCollected);
                return Results.Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return Results.Ok(new { success = false, error = ex.Message });
            }
        });

        // 14. Aksiyon Kaydı
        app.MapPost("/api/game/action/log", async (ActionLogRequest request, IGameRepository repo) =>
        {
            try
            {
                await repo.LogPlayerActionAsync(request.SessionId, request.ActionType, request.TargetId, request.Details);
                return Results.Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return Results.Ok(new { success = false, error = ex.Message });
            }
        });

        // 15. Yardımcı İpucu Getir
        app.MapGet("/api/game/helper/tip", async (string context, string? building, IGameRepository repo) =>
        {
            try
            {
                var messages = (await repo.GetHelperMessagesAsync(context, building)).ToList();
                if (messages.Any())
                {
                    var rnd = new Random();
                    var topMessage = messages[rnd.Next(messages.Count)];
                    return Results.Ok(new { success = true, message = topMessage.Message, context = topMessage.Context, priority = topMessage.Priority });
                }
                return Results.Ok(new { success = false, message = "", context = context, priority = 0 });
            }
            catch (Exception ex)
            {
                var fallbackMessages = new Dictionary<string, string>
                {
                    ["splash"] = "Hoş geldin Amirims! Ben Yardımcı Dedektif Çetin. Bu karanlık davada sana yardımcı olacağım!",
                    ["story_end"] = "Kasabada 5 bina ve 5 şüpheli var. Delilleri dikkatle incele ve çantana al!",
                    ["map_enter"] = "Haritadaki binalara tıklayarak soruşturmana başlayabilirsin.",
                    ["building_enter"] = "Olay yerindeki delilleri inceleyebilir, çantana atabilirsin.",
                    ["bag_open"] = "Çantandaki delilleri 'İncele' butonuyla detaylı inceleyebilirsin.",
                    ["npc_talk"] = "Dikkatli soru sor Amirims!",
                    ["accuse"] = "Son kararını vermeden önce tüm delilleri gözden geçir Amirims."
                };
                var msg = fallbackMessages.GetValueOrDefault(context, "Amirims, soruşturmaya devam edin!");
                return Results.Ok(new { success = true, message = msg, context = context, priority = 1 });
            }
        });

        // 16. Yardımcı Delil Analizi
        app.MapPost("/api/game/helper/analyze-clues", async (AnalyzeCluesRequest request, IGameRepository repo) =>
        {
            try
            {
                var npcs = (await repo.GetAllNPCsAsync()).ToList();
                var guiltyNpc = npcs.FirstOrDefault(n => n.IsGuilty);
                int guiltyId = guiltyNpc?.NPCId ?? 0;

                var analysis = await repo.AnalyzeCluesForHelperAsync(request.ClueIds ?? new List<int>(), guiltyId);
                return Results.Ok(new { success = true, analysis = analysis });
            }
            catch (Exception ex)
            {
                string fallback = request.ClueIds?.Count > 0
                    ? $"Amirims, {request.ClueIds.Count} delil toplamışsınız. Delilleri dikkatlice inceleyin!"
                    : "Amirims, çantanızda henüz delil yok! Binalara girip delilleri toplamalısınız.";
                return Results.Ok(new { success = true, analysis = fallback });
            }
        });

        // 17. Kaydet / Yükle
        app.MapPost("/api/game/state/save", async (GameStateSaveRequest request, IGameRepository repo) =>
        {
            try
            {
                await repo.SaveGameStateAsync(request.SessionId, request.StateData);
                return Results.Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return Results.Ok(new { success = false, error = ex.Message });
            }
        });

        app.MapGet("/api/game/state/load", async (int sessionId, IGameRepository repo) =>
        {
            try
            {
                var stateData = await repo.LoadGameStateAsync(sessionId);
                return Results.Ok(new { success = stateData != null, stateData = stateData ?? "" });
            }
            catch (Exception ex)
            {
                return Results.Ok(new { success = false, stateData = "", error = ex.Message });
            }
        });

        // 18. Diyalog Kaydı
        app.MapPost("/api/game/dialog/log", async (DialogLogRequest request, IGameRepository repo) =>
        {
            try
            {
                await repo.LogDialogWithCategoryAsync(request.NpcId, request.PlayerQuestion, request.NpcResponse, request.Difficulty, request.Category);
                return Results.Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return Results.Ok(new { success = false, error = ex.Message });
            }
        });

        // =============================================
        // GÖLGE ŞEHİR ÖZEL ENDPOINTLERİ
        // =============================================

        // 1. Gölge Şehir Şüpheli Listesi
        app.MapGet("/api/golge-sehir/npcs", async (IGameRepository repo) =>
        {
            try
            {
                var npcs = await repo.GetGolgeSehirNPCsAsync();
                return Results.Ok(npcs);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        // 2. Gölge Şehir İpuçları
        app.MapGet("/api/golge-sehir/clues", async (IGameRepository repo) =>
        {
            try
            {
                var clues = await repo.GetGolgeSehirCluesAsync();
                return Results.Ok(clues);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        // 3. Gölge Şehir Diyalog Havuzu (4 Rastgele Soru)
        app.MapGet("/api/golge-sehir/dialogues", async (int npcId, string? category, IGameRepository repo) =>
        {
            try
            {
                var pool = (await repo.GetGolgeSehirDialoguesAsync(npcId, category)).ToList();
                if (!pool.Any())
                {
                    pool = (await repo.GetGolgeSehirDialoguesAsync(npcId, null)).ToList();
                }

                var rnd = new Random();
                var count = Math.Min(4, pool.Count);
                var selected = pool.OrderBy(x => rnd.Next()).Take(count).ToList();

                return Results.Ok(new { success = true, dialogues = selected });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        // 4. Gölge Şehir Sorgulama (AI / Local Engine)
        app.MapPost("/api/golge-sehir/interrogate", async (InterrogationRequest request, IGameRepository repo, IAIService aiService) =>
        {
            try
            {
                NPC? npc = null;
                try { npc = await repo.GetGolgeSehirNPCByIdAsync(request.NpcId); } catch { }
                npc ??= GetFallbackGolgeNPC(request.NpcId);
                if (npc == null) return Results.NotFound("Gölge Şehir şüphelisi bulunamadı.");

                var npcs = (await repo.GetGolgeSehirNPCsAsync()).ToList();
                var guiltyNpc = npcs.FirstOrDefault(n => n.IsGuilty);

                int guiltyId = guiltyNpc?.NPCId ?? 101;

                IEnumerable<Clue> cluesInBag = new List<Clue>();
                try { cluesInBag = await repo.GetCluesInBagAsync(); } catch { }

                IEnumerable<DialogLog> recentDialogs = new List<DialogLog>();
                try { recentDialogs = await repo.GetRecentDialogLogsAsync(npc.NPCId, 5); } catch { }

                var response = await aiService.GenerateResponseAsync(npc, guiltyId, request.Question, cluesInBag, recentDialogs);

                try
                {
                    if (response.TrustChange != 0)
                    {
                        await repo.UpdateNPCTrustAsync(npc.NPCId, response.TrustChange);
                        npc = await repo.GetGolgeSehirNPCByIdAsync(request.NpcId) ?? npc;
                    }

                    await repo.LogDialogWithCategoryAsync(npc.NPCId, request.Question, response.Dialogue, 1, "golge_ai");
                }
                catch { }

                return Results.Ok(new
                {
                    success = true,
                    dialogue = response.Dialogue,
                    emotion = response.Emotion,
                    trustChange = response.TrustChange,
                    stressIncrease = response.StressIncrease,
                    revealedSecret = response.RevealedSecret ?? "",
                    updatedNpc = npc
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        // 5. Gölge Şehir Suçlama
        app.MapPost("/api/golge-sehir/accuse", async (AccuseRequest request, IGameRepository repo) =>
        {
            try
            {
                var npc = await repo.GetGolgeSehirNPCByIdAsync(request.NpcId);
                if (npc == null) return Results.NotFound("Gölge Şehir şüphelisi bulunamadı.");

                var npcs = (await repo.GetGolgeSehirNPCsAsync()).ToList();
                var guiltyNpc = npcs.FirstOrDefault(n => n.IsGuilty);
                
                int guiltyId = guiltyNpc?.NPCId ?? 101;

                var actualGuilty = npcs.FirstOrDefault(n => n.NPCId == guiltyId) ?? guiltyNpc;
                var guiltyName = actualGuilty?.Name ?? "Bilinmiyor";

                bool isGuiltyPerson = (request.NpcId == guiltyId) || npc.IsGuilty;

                if (isGuiltyPerson)
                {
                    return Results.Ok(new { success = true, message = $"Tebrikler! Gölge Şehir katilinin {npc.Name} olduğunu çözdünüz.", accusedName = npc.Name, guiltyNpcName = guiltyName, guiltyNpcId = guiltyId });
                }
                else
                {
                    return Results.Ok(new { success = false, message = $"{npc.Name} masum çıktı! Gerçek katil {guiltyName} idi.", accusedName = npc.Name, guiltyNpcName = guiltyName, guiltyNpcId = guiltyId });
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        // 6. Gölge Şehir Sıfırla (Rastgele 101-108 Katil Belirle - Farklı Katil Seçimi Garantili)
        app.MapPost("/api/golge-sehir/reset", async (IGameRepository repo, IForensicService forensicService) =>
        {
            await _resetSemaphore.WaitAsync();
            try
            {
                var currentNpcs = (await repo.GetGolgeSehirNPCsAsync()).ToList();
                var previousGuilty = currentNpcs.FirstOrDefault(n => n.IsGuilty)?.NPCId ?? 0;

                var rnd = new Random();
                var candidateIds = Enumerable.Range(101, 8).Where(id => id != previousGuilty).ToList();
                int guiltyId = candidateIds.Count > 0 ? candidateIds[rnd.Next(candidateIds.Count)] : rnd.Next(101, 109);

                await repo.ResetGolgeSehirSessionAsync(guiltyId);
                forensicService.ClearGolgeFindings();
                await repo.ClearDialogLogsByTownAsync(100, 199);
                await repo.ClearPlayerInventoryByTownAsync(1000, 1999);
                return Results.Ok(new { success = true, message = "Gölge Şehir sıfırlandı ve yeni suçlu belirlendi.", guiltyNpcId = guiltyId });
            }
            catch (Exception ex)
            {
                return Results.Ok(new { success = false, message = "Gölge Şehir sıfırlanamadı: " + ex.Message });
            }
            finally
            {
                _resetSemaphore.Release();
            }
        });

        // 7. Gölge Şehir Yardımcı Mesajları (Çetin / Bekçi Rıfat)
        app.MapGet("/api/golge-sehir/helper/tip", async (string context, string? building, IGameRepository repo) =>
        {
            try
            {
                var messages = (await repo.GetGolgeSehirHelperMessagesAsync(context, building)).ToList();
                if (messages.Any())
                {
                    var rnd = new Random();
                    var top = messages[rnd.Next(messages.Count)];
                    return Results.Ok(new { success = true, message = top.Message, speaker = top.Speaker, context = top.Context, priority = top.Priority });
                }
                return Results.Ok(new { success = false, message = "", speaker = "cetin", context = context });
            }
            catch (Exception ex)
            {
                return Results.Ok(new { success = true, message = "Amirims, Gölge Şehir soruşturmasına devam edelim!", speaker = "cetin", context = context });
            }
        });
    }

    // ========================================================================
    // SİSÖREN KASABASI ENDPOINT'LERİ (KATMANLI MİMARİ ENTEGRASYONU)
    // NPC Aralığı: 201-213 & Ekstra NPC'ler (301-318)
    // ========================================================================
    public static void MapSisorenEndpoints(WebApplication app)
    {
        app.MapPost("/api/sisoren/interrogate", async (InterrogationRequest request, IGameRepository repo, IAIService aiService) =>
        {
            try
            {
                if (request == null || !IsSisorenNpc(request.NpcId) ||
                    string.IsNullOrWhiteSpace(request.Question))
                {
                    return Results.BadRequest("Geçersiz Sisören sorgulama isteği.");
                }

                NPC? npc = null;
                try { npc = await repo.GetSisorenNPCByIdAsync(request.NpcId); } catch { }
                npc ??= request.NpcId >= 300 ? GetFallbackSisorenExtraNPC(request.NpcId) : GetFallbackSisorenNPC(request.NpcId);
                if (npc == null) return Results.NotFound("Sisören şüphelisi bulunamadı.");

                var npcs = await repo.GetSisorenNPCsAsync();
                var guiltyId = npcs.FirstOrDefault(n => n.IsGuilty)?.NPCId ?? 201;

                IEnumerable<Clue> cluesInBag = new List<Clue>();
                try { cluesInBag = (await repo.GetCluesInBagAsync()).Where(c => c.ClueId >= 2000); } catch { }

                IEnumerable<DialogLog> recentDialogs = new List<DialogLog>();
                try { recentDialogs = await repo.GetRecentDialogLogsAsync(npc.NPCId, 5); } catch { }

                var response = await aiService.GenerateResponseAsync(npc, guiltyId, request.Question, cluesInBag, recentDialogs);

                try
                {
                    await repo.LogDialogWithCategoryAsync(npc.NPCId, request.Question, response.Dialogue, 1, "sisoren_ai");
                }
                catch { }

                return Results.Ok(new
                {
                    success = true,
                    dialogue = response.Dialogue,
                    emotion = response.Emotion,
                    trustChange = response.TrustChange,
                    stressIncrease = response.StressIncrease,
                    npcId = npc.NPCId,
                    town = "sisoren",
                    guiltyIdUsed = guiltyId
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        // 1. Sisören Suçlama (13 Şüpheli + Ekstra NPC'ler)
        app.MapPost("/api/sisoren/accuse", async (AccuseRequest request, IGameRepository repo) =>
        {
            if (request == null || request.NpcId <= 0)
            {
                return Results.BadRequest("Geçersiz suçlama isteği.");
            }

            // Katil ID'sini sunucu tarafında veritabanından çözümle (güvenlik)
            var sisorenNpcs = (await repo.GetSisorenNPCsAsync()).ToList();
            var guiltyNpcSis = sisorenNpcs.FirstOrDefault(n => n.IsGuilty);
            int guiltyId = guiltyNpcSis?.NPCId ?? 201;

            // Ekstra veya çocuk NPC'ler (301-318) her zaman masumdur
            if (request.NpcId >= 300)
            {
                return Results.Ok(new 
                { 
                    success = false, 
                    message = "Bu kişi suçlanamayacak bir kasabalı/tanıktır ve tamamen masumdur!", 
                    accusedName = "Kasabalı / Tanık", 
                    guiltyNpcName = GetSisorenNPCName(guiltyId), 
                    guiltyNpcId = guiltyId 
                });
            }

            bool isGuilty = (request.NpcId == guiltyId);
            string accusedName = GetSisorenNPCName(request.NpcId);
            string guiltyName = GetSisorenNPCName(guiltyId);

            if (isGuilty)
            {
                return Results.Ok(new 
                { 
                    success = true, 
                    message = $"Tebrikler! Sisören dağ kasabasının gerçek katilinin {accusedName} olduğunu kanıtladınız!", 
                    accusedName = accusedName, 
                    guiltyNpcName = guiltyName, 
                    guiltyNpcId = guiltyId 
                });
            }
            else
            {
                return Results.Ok(new 
                { 
                    success = false, 
                    message = $"{accusedName} masum çıktı! Sisören'in gerçek katili {guiltyName} idi.", 
                    accusedName = accusedName, 
                    guiltyNpcName = guiltyName, 
                    guiltyNpcId = guiltyId 
                });
            }
        });

        // 2. Sisören Sıfırla (201-213 arası rastgele yeni katil belirleme)
        app.MapPost("/api/sisoren/reset", async (IGameRepository repo, IForensicService forensicService) =>
        {
            var rnd = new Random();
            int guiltyId = rnd.Next(201, 214); // 201-213 arası şüpheli
            await repo.ResetSisorenSessionAsync(guiltyId);
            forensicService.ClearSisorenFindings();
            await repo.ClearDialogLogsByTownAsync(200, 999);
            await repo.ClearPlayerInventoryByTownAsync(2000, 9999);
            return Results.Ok(new 
            { 
                success = true, 
                message = "Sisören sıfırlandı ve yeni suçlu belirlendi.", 
                guiltyNpcId = guiltyId,
                guiltyNpcName = GetSisorenNPCName(guiltyId)
            });
        });

        // 3. Sisören NPC Listesi
        app.MapGet("/api/sisoren/npcs", async (IGameRepository repo) =>
        {
            try
            {
                var npcs = await repo.GetSisorenNPCsAsync();
                if (npcs != null && npcs.Any())
                {
                    return Results.Ok(new { success = true, npcs = npcs.Select(n => new { npcId = n.NPCId, name = n.Name, role = n.Role, building = "" }) });
                }
            }
            catch { }

            // Fallback
            var fallbackNpcs = new[]
            {
                new { npcId = 201, name = "Telgrafçı Rüstem", role = "Telgrafçı", building = "Telgrafhane" },
                new { npcId = 202, name = "Kahveci İrfan", role = "Kahveci", building = "Kahvehane" },
                new { npcId = 203, name = "Sinemacı Nejat", role = "Sinemacı", building = "Sinema" },
                new { npcId = 204, name = "Bakkal Cemile", role = "Bakkal", building = "Bakkal" },
                new { npcId = 205, name = "Sahaf Hikmet", role = "Sahaf", building = "Sahaf" },
                new { npcId = 206, name = "Muhtar Meliha Hanım", role = "Köy Muhtarı", building = "Muhtarlık" },
                new { npcId = 207, name = "Tütüncü Nermin Hanım", role = "Tütüncü", building = "Tütüncü" },
                new { npcId = 208, name = "Çoban Durmuş", role = "Çiftçi & Çoban", building = "Ahır" },
                new { npcId = 209, name = "Tüpçü Şevket", role = "Tüpçü", building = "Tüpçü" },
                new { npcId = 210, name = "Hurdacı Zehra", role = "Hurdacı", building = "Hurdacı" },
                new { npcId = 211, name = "Zeynep Teyze", role = "Ev Hanımı", building = "Kasabalı Evi" },
                new { npcId = 212, name = "Hatice Nine", role = "Kasaba Büyüğü", building = "Kasabalı Evi" },
                new { npcId = 213, name = "Emine Hanım", role = "Dağ Sakini", building = "Kasabalı Evi" }
            };
            return Results.Ok(new { success = true, npcs = fallbackNpcs });
        });

        // 4. Sisören Yardımcı Mesajları
        app.MapGet("/api/sisoren/helper/tip", (string context, string? building) =>
        {
            var rnd = new Random();
            string message = "Dağların sisi gerçeği saklar ama benim katırlarım her dedikoduyu duyar! Şüphelilerin anlattıklarına dikkat et.";
            
            if (context == "building_enter")
            {
                switch (building?.ToLower())
                {
                    case "telgrafhane":
                        var telgraf = new[] { "Telgrafçı Rüstem o gece çok tuhaftı amirim. Mesajlarda bir şeyler saklıyor olabilir.", "Rüstem'in çektiği son telgrafları kontrol etmeliyiz amirim, yalan söylüyor olabilir." };
                        message = telgraf[rnd.Next(telgraf.Length)]; break;
                    case "kahvehane":
                        var kahve = new[] { "Kahveci İrfan sürekli kulak misafiri olur. Fincanların arasına sakladığı sırlar var.", "İrfan amca çok şey bilir ama susar. Onu konuşturmanın bir yolunu bulmalıyız amirim." };
                        message = kahve[rnd.Next(kahve.Length)]; break;
                    case "sinema":
                        var sinema = new[] { "Sinemacı Nejat makine dairesinde neler saklıyor bir baksanız iyi olur.", "O karanlık salonda sadece filmler dönmüyor amirim, Nejat'a dikkat et." };
                        message = sinema[rnd.Next(sinema.Length)]; break;
                    case "bakkal":
                        var bakkal = new[] { "Bakkal Cemile'nin veresiye defteri hiç de masum görünmüyor amirim.", "Cemile abla güler yüzlüdür ama hesapları çok karışıktır, dikkatlice inceleyelim." };
                        message = bakkal[rnd.Next(bakkal.Length)]; break;
                    case "sahaf":
                        var sahaf = new[] { "Sahaf Hikmet eski kitapların arasında zehir tarifleri okuyor, dikkat edin.", "Hikmet'in o tozlu raflarında ne sırlar gizli bir bilsen amirim..." };
                        message = sahaf[rnd.Next(sahaf.Length)]; break;
                    case "muhtarlık":
                        var muhtar = new[] { "Muhtar Meliha'nın sahte mühürleri olduğunu duydum amirim. Kendisine güven olmaz.", "Meliha Hanım kasabayı demir yumrukla yönetir, onun izni olmadan burada kuş uçmaz." };
                        message = muhtar[rnd.Next(muhtar.Length)]; break;
                    case "tütüncü":
                        var tutun = new[] { "Tütüncü Nermin o gece çok gerginmiş, birileriyle kavga etmiş diyorlar.", "Nermin ablanın sardığı tütünlerin arasında başka şeyler de olabilir amirim." };
                        message = tutun[rnd.Next(tutun.Length)]; break;
                    case "ahır":
                        var ahir = new[] { "Çoban Durmuş dağlarda sadece koyun gütmüyor amirim, gece işleri de var.", "Durmuş'un ahırında sakladığı şeyler sadece hayvanlar değil gibi görünüyor." };
                        message = ahir[rnd.Next(ahir.Length)]; break;
                    case "tüpçü":
                        var tup = new[] { "Tüpçü Şevket'in anahtarı çok ağırdır amirim. Kimseye acımaz.", "Şevket sinirlendiğinde gözü hiçbir şeyi görmez, temkinli yaklaşalım amirim." };
                        message = tup[rnd.Next(tup.Length)]; break;
                    case "hurdacı":
                        var hurda = new[] { "Hurdacı Zehra kasadaki paraları nereye saklıyor sormak lazım.", "Zehra'nın hurdalığında kaybolan çok insan var derler, dikkatli olalım." };
                        message = hurda[rnd.Next(hurda.Length)]; break;
                    case "kasabalı evi 1":
                        var ev1 = new[] { "Zeynep Teyze'nin maden kayıtlarında usulsüzlük var amirim.", "Zeynep Teyze'nin kapısı hep kilitlidir, içeri girmek zor olacak." };
                        message = ev1[rnd.Next(ev1.Length)]; break;
                    case "kasabalı evi 2":
                        var ev2 = new[] { "Hatice Nine'nin sandığında neler gizli bir bilseniz...", "Hatice Nine kasabanın en eskilerindendir, çok şey görmüş geçirmiştir." };
                        message = ev2[rnd.Next(ev2.Length)]; break;
                    case "kasabalı evi 3":
                        var ev3 = new[] { "Emine Hanım otlarla sadece şifa dağıtmıyor amirim, bazen zehir de yapıyor.", "Emine'nin demlediği çaylara dikkat et amirim, içinde ne olduğu belli olmaz." };
                        message = ev3[rnd.Next(ev3.Length)]; break;
                }
            }
            else if (context == "map_enter")
            {
                var mapEnter = new[] { "Sisören'e hoş geldin amirim. Buranın soğuğu da insanı da adamı dondurur. Dikkatli ol.", "Amirim, bu dağların sisi gerçekleri gizlemek için birebir. Gözünü dört aç.", "Buralarda yabancıları sevmezler amirim, adımlarımıza dikkat edelim." };
                message = mapEnter[rnd.Next(mapEnter.Length)];
            }
            else if (context == "clue_found")
            {
                var clue = new[] { "Amirim, bu dağlarda hiçbir şey tesadüf değildir. Bulduğun şeye iyi bak!", "İşte bu delil bizi katile bir adım daha yaklaştıracak amirim!", "Bunu saklamak istemişler ama bizden kaçmaz amirim!" };
                message = clue[rnd.Next(clue.Length)];
            }
            else if (context == "accuse")
            {
                var accuse = new[] { "Zaman geldi çattı amirim. Umarız suçluyu doğru seçersin, yoksa kan davaları başlar bu dağlarda.", "Son kararını vermeden önce tüm delilleri gözden geçir amirim, burada hata affedilmez." };
                message = accuse[rnd.Next(accuse.Length)];
            }

            return Results.Ok(new 
            { 
                success = true, 
                message = message, 
                speaker = "ilyas", 
                context = context 
            });
        });
    }

    private static string GetSisorenNPCName(int npcId)
    {
        return npcId switch
        {
            201 => "Telgrafçı Rüstem",
            202 => "Kahveci İrfan",
            203 => "Sinemacı Nejat",
            204 => "Bakkal Cemile",
            205 => "Sahaf Hikmet",
            206 => "Muhtar Meliha Hanım",
            207 => "Tütüncü Nermin Hanım",
            208 => "Çoban Durmuş",
            209 => "Tüpçü Şevket",
            210 => "Hurdacı Zehra",
            211 => "Zeynep Teyze",
            212 => "Hatice Nine",
            213 => "Emine Hanım",
            _ => "Bilinmeyen Şüpheli"
        };
    }

    private static NPC? GetFallbackGizemliNPC(int npcId)
    {
        return npcId switch
        {
            1 => new NPC { NPCId = 1, Name = "Kasap Hasan", Role = "Kasap", SecretInfo = "Cinayet gecesi dükkânında gizlice muhtara et sattı." },
            2 => new NPC { NPCId = 2, Name = "Eczacı Selma", Role = "Eczacı", SecretInfo = "Kurbanın zehirlendiğini biliyordu ama gizledi." },
            3 => new NPC { NPCId = 3, Name = "Muhtar Kemal", Role = "Muhtar", SecretInfo = "Kurbanla arazi anlaşmazlığı vardı." },
            4 => new NPC { NPCId = 4, Name = "Komiser Güneş", Role = "Komiser", SecretInfo = "Olay yerindeki delilleri sakladı." },
            5 => new NPC { NPCId = 5, Name = "Terzi Yahya", Role = "Terzi", SecretInfo = "Kurbana gizli cepli ceket dikti, son gören kişi." },
            _ => null
        };
    }

    private static NPC? GetFallbackGolgeNPC(int npcId)
    {
        return npcId switch
        {
            101 => new NPC { NPCId = 101, Name = "Oduncu Tahsin", Role = "Oduncu", SecretInfo = "Ormanda kaçak gece kesimi yapıyordu." },
            102 => new NPC { NPCId = 102, Name = "Manav Ayşe", Role = "Manav", SecretInfo = "Dükkânı Ekrem'e ipotekliydi." },
            103 => new NPC { NPCId = 103, Name = "Demirci Kazım", Role = "Demirci", SecretInfo = "Ekrem'e özel kilitli çelik kasa yapmıştı." },
            104 => new NPC { NPCId = 104, Name = "Bakkal Naciye", Role = "Bakkal", SecretInfo = "Veresiye defterindeki sayfayı yırttı." },
            105 => new NPC { NPCId = 105, Name = "Hekim Sevgi", Role = "Hekim", SecretInfo = "Banotu zehrini saklıyordu." },
            106 => new NPC { NPCId = 106, Name = "Muhtar Cevdet", Role = "Muhtar", SecretInfo = "Sahte orman tapuları çıkarmıştı." },
            107 => new NPC { NPCId = 107, Name = "Fehmi Bey", Role = "Emekli Muallim", SecretInfo = "Köstekli saatini Ekrem zorla almıştı." },
            108 => new NPC { NPCId = 108, Name = "Kunduracı Rasim", Role = "Kunduracı", SecretInfo = "Kaçak deri ticaretinde Ekrem'e borçluydu." },
            _ => null
        };
    }

    private static NPC? GetFallbackSisorenNPC(int npcId)
    {
        return npcId switch
        {
            201 => new NPC { NPCId = 201, Name = "Telgrafçı Rüstem", Role = "Telgrafçı", SecretInfo = "Cinayet gecesi dağ hattından gelen şifreli mesajı sakladı." },
            202 => new NPC { NPCId = 202, Name = "Kahveci İrfan", Role = "Kahveci", SecretInfo = "Gece yarısı kahvehanede duyduğu tartışmayı gizliyor." },
            203 => new NPC { NPCId = 203, Name = "Sinemacı Nejat", Role = "Sinemacı", SecretInfo = "Film makinesinin saat kaydını değiştirdi." },
            204 => new NPC { NPCId = 204, Name = "Bakkal Cemile", Role = "Bakkal", SecretInfo = "Kırmızı-yeşil yün ipliğini cinayet gecesi sattı." },
            205 => new NPC { NPCId = 205, Name = "Sahaf Hikmet", Role = "Sahaf", SecretInfo = "Eski tapu defterindeki bir sayfayı kopardı." },
            206 => new NPC { NPCId = 206, Name = "Muhtar Meliha Hanım", Role = "Köy Muhtarı", SecretInfo = "Maden yolu için yapılan gizli anlaşmayı biliyor." },
            207 => new NPC { NPCId = 207, Name = "Tütüncü Nermin Hanım", Role = "Tütüncü", SecretInfo = "Olay gecesi dükkânına gelen kişiyi saklıyor." },
            208 => new NPC { NPCId = 208, Name = "Çoban Durmuş", Role = "Çiftçi ve Çoban", SecretInfo = "Dağ yolundaki taze kazılmış çukuru gördü." },
            209 => new NPC { NPCId = 209, Name = "Tüpçü Şevket", Role = "Tüpçü", SecretInfo = "Gece taşınan ağır sandığın izlerini fark etti." },
            210 => new NPC { NPCId = 210, Name = "Hurdacı Zehra", Role = "Hurdacı", SecretInfo = "Yeraltı tüneline açılan kapağın anahtarını taşıyor." },
            211 => new NPC { NPCId = 211, Name = "Zeynep Teyze", Role = "Ev Hanımı", SecretInfo = "Meydandaki fener ışığını penceresinden gördü." },
            212 => new NPC { NPCId = 212, Name = "Hatice Nine", Role = "Kasaba Büyüğü", SecretInfo = "Kasabanın eski maden sırrını biliyor." },
            213 => new NPC { NPCId = 213, Name = "Emine Hanım", Role = "Dağ Sakini", SecretInfo = "Dağ yolunda gömülü bir nesne buldu." },
            _ => null
        };
    }

    private static bool IsSisorenNpc(int npcId) =>
        (npcId >= 201 && npcId <= 213) || (npcId >= 301 && npcId <= 318);

    private static bool IsSisorenWitness(int npcId, int guiltyId) =>
        ((npcId * 17) + guiltyId) % 5 < 2;

    private static NPC? GetFallbackSisorenExtraNPC(int npcId)
    {
        return npcId switch
        {
            301 => new NPC { NPCId = 301, Name = "Celal Amca", Role = "senior" },
            302 => new NPC { NPCId = 302, Name = "Hamdi Dayı", Role = "senior" },
            303 => new NPC { NPCId = 303, Name = "Kahveci Çırağı Salih", Role = "young" },
            304 => new NPC { NPCId = 304, Name = "Şerife Teyze", Role = "senior" },
            305 => new NPC { NPCId = 305, Name = "Oduncu Çırağı Cemal", Role = "young" },
            306 => new NPC { NPCId = 306, Name = "Postacı Nuri Efendi", Role = "adult" },
            307 => new NPC { NPCId = 307, Name = "Telgraf Çırağı Yusuf", Role = "young" },
            308 => new NPC { NPCId = 308, Name = "Biletçi Fatma", Role = "adult" },
            309 => new NPC { NPCId = 309, Name = "Kâtip Sami Efendi", Role = "adult" },
            310 => new NPC { NPCId = 310, Name = "Tütün Tiryakisi Osman", Role = "adult" },
            311 => new NPC { NPCId = 311, Name = "Hurda Toplayan Ali", Role = "child" },
            312 => new NPC { NPCId = 312, Name = "Tüp Dağıtıcısı Mehmet", Role = "adult" },
            313 => new NPC { NPCId = 313, Name = "Küçük Ayşe", Role = "child" },
            314 => new NPC { NPCId = 314, Name = "Gece Bekçisi Recep", Role = "adult" },
            315 => new NPC { NPCId = 315, Name = "Küçük Elif", Role = "child" },
            316 => new NPC { NPCId = 316, Name = "Can", Role = "child" },
            317 => new NPC { NPCId = 317, Name = "Selin", Role = "child" },
            318 => new NPC { NPCId = 318, Name = "Kerem", Role = "child" },
            _ => null
        };
    }
}
