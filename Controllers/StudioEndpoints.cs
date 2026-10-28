using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DedektiflikRPG.Controllers;

public static class StudioEndpoints
{
    private static readonly HttpClient _httpClient = new HttpClient();
    private static Process? _bananaProcess;

    public static void MapStudioEndpoints(this IEndpointRouteBuilder app)
    {
        // 1. Stüdyo Durumu
        app.MapGet("/api/studio/status", () =>
        {
            var baseDir = Directory.GetCurrentDirectory();
            var bananaDir = Path.Combine(baseDir, "tools", "banana-studio");
            var nashiExe = Path.Combine(baseDir, "tools", "nashi-app", "windows", "Nashi.Windows", "bin", "Release", "net10.0-windows10.0.19041.0", "win-x64", "Nashi.exe");

            return Results.Ok(new
            {
                bananaInstalled = Directory.Exists(bananaDir),
                bananaRunning = _bananaProcess != null && !_bananaProcess.HasExited,
                bananaUrl = "http://localhost:5173",
                nashiInstalled = File.Exists(nashiExe),
                nashiPath = nashiExe
            });
        });

        // 2. Banana Studio'yu Başlat
        app.MapPost("/api/studio/launch-banana", () =>
        {
            try
            {
                var baseDir = Directory.GetCurrentDirectory();
                var bananaDir = Path.Combine(baseDir, "tools", "banana-studio");

                if (!Directory.Exists(bananaDir))
                    return Results.BadRequest(new { error = "Banana Studio dizini bulunamadı." });

                if (_bananaProcess == null || _bananaProcess.HasExited)
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c npm run dev",
                        WorkingDirectory = bananaDir,
                        UseShellExecute = true,
                        CreateNoWindow = false
                    };
                    _bananaProcess = Process.Start(psi);
                }

                // Tarayıcıda aç
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "http://localhost:5173",
                        UseShellExecute = true
                    });
                }
                catch { }

                return Results.Ok(new { success = true, message = "Banana Studio başlatıldı!", url = "http://localhost:5173" });
            }
            catch (Exception ex)
            {
                return Results.Problem("Banana Studio başlatılamadı: " + ex.Message);
            }
        });

        // 3. Nashi'yi Başlat
        app.MapPost("/api/studio/launch-nashi", () =>
        {
            try
            {
                var baseDir = Directory.GetCurrentDirectory();
                var nashiExe = Path.Combine(baseDir, "tools", "nashi-app", "windows", "Nashi.Windows", "bin", "Release", "net10.0-windows10.0.19041.0", "win-x64", "Nashi.exe");

                if (!File.Exists(nashiExe))
                    return Results.BadRequest(new { error = "Nashi.exe bulunamadı. Lütfen önce derleyin." });

                Process.Start(new ProcessStartInfo
                {
                    FileName = nashiExe,
                    WorkingDirectory = Path.GetDirectoryName(nashiExe)!,
                    UseShellExecute = true
                });

                return Results.Ok(new { success = true, message = "Nashi Medya Aracı açıldı!" });
            }
            catch (Exception ex)
            {
                return Results.Problem("Nashi başlatılamadı: " + ex.Message);
            }
        });

        // 4. Prompt Enhance (Banana Studio Tarzı Gemini Destekli Sinematik Prompt İyileştirici)
        app.MapPost("/api/studio/enhance-prompt", async (EnhancePromptRequest req, IServiceProvider sp) =>
        {
            if (string.IsNullOrWhiteSpace(req.Prompt))
                return Results.BadRequest(new { error = "Prompt boş olamaz." });

            try
            {
                var configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
                string apiKey = "";
                if (File.Exists(configPath))
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(configPath);
                        using var doc = JsonDocument.Parse(json);
                        if (doc.RootElement.TryGetProperty("GeminiApiKey", out var keyElem))
                            apiKey = keyElem.GetString() ?? "";
                    }
                    catch { }
                }

                if (string.IsNullOrWhiteSpace(apiKey))
                    apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "";

                string systemInstruction = "Sen Banana Studio için dedektiflik ve sinematik noir görsel/video yönetmenisin. " +
                    "Kullanıcının verdiği kısa dedektiflik sahnesini, şüpheli portresini veya delil tanımını " +
                    "yüksek kaliteli, detaylı bir yapay zeka görsel/video üretim promptuna dönüştür. " +
                    "Prompt İngilizce olmalı; 1940'lar noir atmosferi, sinematik ışıklandırma, sis, dramatik gölgeler, 8k çözünürlük, fotogerçekçi detaylar içermelidir. " +
                    "YALNIZCA nihai prompt metnini döndür, açıklama veya selamlaşma yazma.";

                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    var geminiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}";
                    var payload = new
                    {
                        contents = new[]
                        {
                            new
                            {
                                role = "user",
                                parts = new[]
                                {
                                    new { text = $"{systemInstruction}\n\nKullanıcı Taslağı: {req.Prompt}\nKategori: {req.Category ?? "general"}" }
                                }
                            }
                        }
                    };

                    var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                    var res = await _httpClient.PostAsync(geminiUrl, content);
                    if (res.IsSuccessStatusCode)
                    {
                        var resJson = await res.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(resJson);
                        var enhanced = doc.RootElement
                            .GetProperty("candidates")[0]
                            .GetProperty("content")
                            .GetProperty("parts")[0]
                            .GetProperty("text")
                            .GetString()?.Trim();

                        if (!string.IsNullOrEmpty(enhanced))
                        {
                            return Results.Ok(new { enhancedPrompt = enhanced });
                        }
                    }
                }

                // Fallback Enhance
                var fallback = $"Cinematic 1940s film noir style, {req.Prompt}, dramatic volumetric fog, dim amber streetlamp lighting, highly detailed textures, photorealistic 8k, moody detective mystery atmosphere, 35mm photograph.";
                return Results.Ok(new { enhancedPrompt = fallback });
            }
            catch (Exception ex)
            {
                return Results.Ok(new { enhancedPrompt = $"Cinematic noir: {req.Prompt}, highly detailed, dramatic lighting, 8k." });
            }
        });

        // 5. Doğrudan Hızlı Görsel Üretimi ve Oyuna Kaydetme
        app.MapPost("/api/studio/generate-image", async (GenerateImageRequest req) =>
        {
            if (string.IsNullOrWhiteSpace(req.Prompt))
                return Results.BadRequest(new { error = "Prompt boş olamaz." });

            try
            {
                string encodedPrompt = Uri.EscapeDataString(req.Prompt);
                int width = req.Width > 0 ? req.Width : 1024;
                int height = req.Height > 0 ? req.Height : 768;
                string imageUrl = $"https://image.pollinations.ai/prompt/{encodedPrompt}?width={width}&height={height}&nologo=true&seed={Random.Shared.Next(1, 999999)}";

                // Görseli indirip wwwroot/images/studio/ altına kaydet
                var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
                var studioDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "studio");
                if (!Directory.Exists(studioDir)) Directory.CreateDirectory(studioDir);

                string fileName = $"studio_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Random.Shared.Next(100, 999)}.jpg";
                string fullPath = Path.Combine(studioDir, fileName);
                await File.WriteAllBytesAsync(fullPath, imageBytes);

                string relativeUrl = $"/images/studio/{fileName}";

                return Results.Ok(new
                {
                    success = true,
                    imageUrl = relativeUrl,
                    fullUrl = imageUrl,
                    fileName = fileName
                });
            }
            catch (Exception ex)
            {
                return Results.Problem("Görsel üretilemedi: " + ex.Message);
            }
        });

        // 6. Üretilen Stüdyo Medyalarını Listele
        app.MapGet("/api/studio/gallery", () =>
        {
            var studioDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "studio");
            if (!Directory.Exists(studioDir))
                return Results.Ok(Array.Empty<object>());

            var files = Directory.GetFiles(studioDir, "*.*")
                .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".webp", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(f => File.GetCreationTimeUtc(f))
                .Select(f => new
                {
                    fileName = Path.GetFileName(f),
                    url = $"/images/studio/{Path.GetFileName(f)}",
                    isVideo = f.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase),
                    created = File.GetCreationTimeUtc(f)
                });

            return Results.Ok(files);
        });
    }
}

public class EnhancePromptRequest
{
    public string Prompt { get; set; } = string.Empty;
    public string? Category { get; set; }
}

public class GenerateImageRequest
{
    public string Prompt { get; set; } = string.Empty;
    public int Width { get; set; } = 1024;
    public int Height { get; set; } = 768;
    public string? TargetCategory { get; set; } // character, location, clue, cinematic
}
