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
                if (request.NpcId >= 100)
                {
                    var golgeNpc = await repo.GetGolgeSehirNPCByIdAsync(request.NpcId);
                    if (golgeNpc == null) return Results.NotFound("Gölge Şehir şüphelisi bulunamadı.");

                    var golgeNPCs = (await repo.GetGolgeSehirNPCsAsync()).ToList();
                    var guiltyGolge = golgeNPCs.FirstOrDefault(n => n.IsGuilty);
                    int guiltyIdGolge = guiltyGolge?.NPCId ?? 101;

                    var cluesInBagGolge = await repo.GetCluesInBagAsync();
                    var recentDialogsGolge = await repo.GetRecentDialogLogsAsync(golgeNpc.NPCId, 5);

                    var responseGolge = await aiService.GenerateResponseAsync(golgeNpc, guiltyIdGolge, request.Question, cluesInBagGolge, recentDialogsGolge);

                    if (responseGolge.TrustChange != 0)
                    {
                        await repo.UpdateNPCTrustAsync(golgeNpc.NPCId, responseGolge.TrustChange);
                    }

                    await repo.LogDialogWithCategoryAsync(golgeNpc.NPCId, request.Question, responseGolge.Dialogue, 1, "golge_local_ai");

                    return Results.Ok(new
                    {
                        success = true,
                        dialogue = responseGolge.Dialogue,
                        emotion = responseGolge.Emotion,
                        trustChange = responseGolge.TrustChange,
                        revealedSecret = responseGolge.RevealedSecret ?? "",
                        updatedNpc = golgeNpc,
                        guiltyIdUsed = guiltyIdGolge
                    });
                }

                // Gizemli Kasaba NPC'leri (1 - 5)
                var npc = await repo.GetNPCByIdAsync(request.NpcId);
                if (npc == null) return Results.NotFound("Şüpheli bulunamadı.");

                var npcs = (await repo.GetAllNPCsAsync()).ToList();
                var guiltyNpc = npcs.FirstOrDefault(n => n.IsGuilty);

                int guiltyId = (request.GuiltyNpcId.HasValue && request.GuiltyNpcId.Value > 0)
                    ? request.GuiltyNpcId.Value
                    : (guiltyNpc?.NPCId ?? 1);

                var cluesInBag = await repo.GetCluesInBagAsync();
                var recentDialogs = await repo.GetRecentDialogLogsAsync(npc.NPCId, 5);

                var response = await aiService.GenerateResponseAsync(npc, guiltyId, request.Question, cluesInBag, recentDialogs);

                if (response.TrustChange != 0)
                {
                    await repo.UpdateNPCTrustAsync(npc.NPCId, response.TrustChange);
                }

                await repo.LogDialogWithCategoryAsync(npc.NPCId, request.Question, response.Dialogue, 1, "local_ai");

                return Results.Ok(new
                {
                    success = true,
                    dialogue = response.Dialogue,
                    emotion = response.Emotion,
                    trustChange = response.TrustChange,
                    revealedSecret = response.RevealedSecret ?? "",
                    updatedNpc = npc,
                    guiltyIdUsed = guiltyId
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
                    var guiltyNameGolge = guiltyGolge?.Name ?? "Bilinmiyor";
                    var guiltyIdGolge = guiltyGolge?.NPCId ?? 101;

                    if (golgeNpc.IsGuilty)
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
                var guiltyName = guiltyNpc?.Name ?? "Bilinmiyor";
                var guiltyId = guiltyNpc?.NPCId ?? 0;

                if (npc.IsGuilty)
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
                bool isGolge = request.ClueId >= 1000;
                var npcs = isGolge
                    ? (await repo.GetGolgeSehirNPCsAsync()).ToList()
                    : (await repo.GetAllNPCsAsync()).ToList();

                var guiltyNpc = npcs.FirstOrDefault(n => n.IsGuilty);
                int guiltyId = guiltyNpc?.NPCId ?? (isGolge ? 101 : 1);

                forensicService.SubmitFinding(request.ClueId, request.ClueName, request.FindingText, npcs, guiltyId);
                return Results.Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        // 7. Otopsi Raporu Getir
        app.MapGet("/api/game/autopsy", async (string? town, IGameRepository repo, IForensicService forensicService) =>
        {
            try
            {
                bool isGolge = town == "golge_sehir";
                var npcs = isGolge
                    ? (await repo.GetGolgeSehirNPCsAsync()).ToList()
                    : (await repo.GetAllNPCsAsync()).ToList();

                var guiltyNpc = npcs.FirstOrDefault(n => n.IsGuilty);
                int guiltyId = guiltyNpc?.NPCId ?? (isGolge ? 101 : 1);

                string reportHtml = await forensicService.GenerateAutopsyReportAsync(npcs, guiltyId);
                return Results.Ok(new { success = true, report = reportHtml, guiltyId });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        // 8. Adli Lab State
        app.MapGet("/api/game/forensic-state", async (string? town, IGameRepository repo, IForensicService forensicService) =>
        {
            try
            {
                bool isGolge = town == "golge_sehir";
                var npcs = isGolge
                    ? (await repo.GetGolgeSehirNPCsAsync()).ToList()
                    : (await repo.GetAllNPCsAsync()).ToList();

                var guiltyNpc = npcs.FirstOrDefault(n => n.IsGuilty);
                int guiltyId = guiltyNpc?.NPCId ?? (isGolge ? 101 : 1);

                var state = forensicService.GetForensicState(guiltyId);
                return Results.Ok(state);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
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

        // 10. Oyunu Sıfırla
        app.MapPost("/api/game/reset", async (IGameRepository repo, IForensicService forensicService) =>
        {
            try
            {
                forensicService.ClearGizemliFindings();
                var random = new Random();
                int guiltyId = random.Next(1, 6);

                var npcs = (await repo.GetAllNPCsAsync()).ToList();
                foreach (var npc in npcs)
                {
                    npc.IsGuilty = (npc.NPCId == guiltyId);
                    npc.TrustLevel = 50;
                    npc.FearLevel = 30;
                    await repo.UpdateNPCAsync(npc);
                }

                await repo.ClearAllDialogLogsAsync();
                await repo.ClearPlayerInventoryAsync();

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
                var topMessage = messages.FirstOrDefault();
                if (topMessage != null)
                {
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
                var npc = await repo.GetGolgeSehirNPCByIdAsync(request.NpcId);
                if (npc == null) return Results.NotFound("Gölge Şehir şüphelisi bulunamadı.");

                var npcs = (await repo.GetGolgeSehirNPCsAsync()).ToList();
                var guiltyNpc = npcs.FirstOrDefault(n => n.IsGuilty);

                int guiltyId = (request.GuiltyNpcId.HasValue && request.GuiltyNpcId.Value > 0)
                    ? request.GuiltyNpcId.Value
                    : (guiltyNpc?.NPCId ?? 101);

                var cluesInBag = await repo.GetCluesInBagAsync();
                var recentDialogs = await repo.GetRecentDialogLogsAsync(npc.NPCId, 5);

                var response = await aiService.GenerateResponseAsync(npc, guiltyId, request.Question, cluesInBag, recentDialogs);

                await repo.LogDialogWithCategoryAsync(npc.NPCId, request.Question, response.Dialogue, 1, "golge_ai");

                return Results.Ok(new
                {
                    success = true,
                    dialogue = response.Dialogue,
                    emotion = response.Emotion,
                    trustChange = response.TrustChange,
                    revealedSecret = response.RevealedSecret ?? "",
                    updatedNpc = npc,
                    guiltyIdUsed = guiltyId
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
                var guiltyName = guiltyNpc?.Name ?? "Bilinmiyor";
                var guiltyId = guiltyNpc?.NPCId ?? 101;

                if (npc.IsGuilty)
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

        // 6. Gölge Şehir Sıfırla (Rastgele 101-108 Katil Belirle)
        app.MapPost("/api/golge-sehir/reset", async (IGameRepository repo, IForensicService forensicService) =>
        {
            await _resetSemaphore.WaitAsync();
            try
            {
                var rnd = new Random();
                int guiltyId = rnd.Next(101, 109);
                await repo.ResetGolgeSehirSessionAsync(guiltyId);
                forensicService.ClearGolgeFindings();
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
                var top = messages.FirstOrDefault();
                if (top != null)
                {
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
}
