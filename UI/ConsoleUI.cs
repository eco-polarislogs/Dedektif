using DedektiflikRPG.Data;
using DedektiflikRPG.Models;
using DedektiflikRPG.Services;

namespace DedektiflikRPG.UI;

/// <summary>
/// Renkli ve interaktif konsol arayüzü.
/// Oyun döngüsünü, menüleri ve diyalog ekranlarını yönetir.
/// </summary>
public class ConsoleUI
{
    private readonly DatabaseRepository _repository;
    private readonly DialogManager _dialogManager;
    private NPC? _currentNPC;

    // Renk paleti
    private static readonly ConsoleColor TitleColor = ConsoleColor.Cyan;
    private static readonly ConsoleColor MenuColor = ConsoleColor.Yellow;
    private static readonly ConsoleColor NPCColor = ConsoleColor.Green;
    private static readonly ConsoleColor ClueColor = ConsoleColor.Magenta;
    private static readonly ConsoleColor DialogColor = ConsoleColor.White;
    private static readonly ConsoleColor EmotionColor = ConsoleColor.DarkYellow;
    private static readonly ConsoleColor TrustUpColor = ConsoleColor.Green;
    private static readonly ConsoleColor TrustDownColor = ConsoleColor.Red;
    private static readonly ConsoleColor SecretColor = ConsoleColor.Red;
    private static readonly ConsoleColor CommandColor = ConsoleColor.DarkCyan;
    private static readonly ConsoleColor ErrorColor = ConsoleColor.Red;
    private static readonly ConsoleColor InfoColor = ConsoleColor.Gray;

    public ConsoleUI(DatabaseRepository repository, DialogManager dialogManager)
    {
        _repository = repository;
        _dialogManager = dialogManager;
    }

    /// <summary>
    /// Ana oyun döngüsünü başlatır.
    /// </summary>
    public async Task RunAsync()
    {
        try { Console.OutputEncoding = System.Text.Encoding.UTF8; } catch { }
        try { Console.Title = "🔍 Akıllı Dedektiflik RPG"; } catch { }

        ShowSplashScreen();
        await ShowMainMenuAsync();
    }

    // =============================================
    // Splash Screen
    // =============================================

    private void ShowSplashScreen()
    {
        SafeClear();
        WriteLineColored(@"
    ╔══════════════════════════════════════════════════════════╗
    ║                                                          ║
    ║        ██████╗ ███████╗██████╗ ███████╗██╗  ██╗          ║
    ║        ██╔══██╗██╔════╝██╔══██╗██╔════╝██║ ██╔╝          ║
    ║        ██║  ██║█████╗  ██║  ██║█████╗  █████╔╝           ║
    ║        ██║  ██║██╔══╝  ██║  ██║██╔══╝  ██╔═██╗           ║
    ║        ██████╔╝███████╗██████╔╝███████╗██║  ██╗          ║
    ║        ╚═════╝ ╚══════╝╚═════╝ ╚══════╝╚═╝  ╚═╝          ║
    ║                                                          ║
    ║          🔍  A I   D E D E K T İ F L İ K  🔍            ║
    ║               R P G   O Y U N U                          ║
    ║                                                          ║
    ║          Yapay Zeka Destekli Sorgulama Simülasyonu        ║
    ║                                                          ║
    ╚══════════════════════════════════════════════════════════╝
", TitleColor);

        WriteLineColored("    Devam etmek için bir tuşa basın...", InfoColor);
        SafeReadKey();
    }

    // =============================================
    // Ana Menü
    // =============================================

    private async Task ShowMainMenuAsync()
    {
        while (true)
        {
            SafeClear();
            DrawHeader("🏘️  KASABA MERKEZİ");
            Console.WriteLine();

            // NPC listesi
            await ShowNPCListAsync();
            Console.WriteLine();

            // Komutlar
            DrawSeparator();
            WriteLineColored("  📋 KOMUTLAR:", CommandColor);
            WriteLineColored("  ────────────────────────────────────────", CommandColor);
            WriteLineColored("  [1-3]       → Şüpheliyle konuşmak için numara gir", MenuColor);
            WriteLineColored("  ipucular    → Elindeki tüm ipuçlarını göster", MenuColor);
            WriteLineColored("  sorgula     → Bir NPC'nin geçmiş sorgulamalarını göster", MenuColor);
            WriteLineColored("  cikis       → Oyundan çık", MenuColor);
            DrawSeparator();
            Console.WriteLine();

            WriteColored("  ▶ Komutunuz: ", TitleColor);
            var input = Console.ReadLine()?.Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(input))
                continue;

            switch (input)
            {
                case "1":
                case "2":
                case "3":
                    if (int.TryParse(input, out int npcId))
                        await StartInterrogationAsync(npcId);
                    break;

                case "ipucular":
                    await ShowCluesAsync();
                    break;

                case "sorgula":
                    await ShowDialogHistoryMenuAsync();
                    break;

                case "cikis":
                    ShowExitScreen();
                    return;

                default:
                    WriteLineColored($"\n  ⚠️  Bilinmeyen komut: '{input}'", ErrorColor);
                    Thread.Sleep(1500);
                    break;
            }
        }
    }

    // =============================================
    // NPC Listesi
    // =============================================

    private async Task ShowNPCListAsync()
    {
        var npcs = await _repository.GetAllNPCsAsync();

        WriteLineColored("  🎭 ŞÜPHELİLER:", NPCColor);
        WriteLineColored("  ────────────────────────────────────────", NPCColor);

        foreach (var npc in npcs)
        {
            var trustBar = GetProgressBar(npc.TrustLevel, 100, 15);
            var fearBar = GetProgressBar(npc.FearLevel, 100, 15);
            var trustColor = npc.TrustLevel >= 60 ? TrustUpColor : (npc.TrustLevel <= 30 ? TrustDownColor : MenuColor);

            WriteColored($"  [{npc.NPCId}] ", MenuColor);
            WriteColored($"{npc.Name,-15}", NPCColor);
            WriteColored($" │ Güven: ", InfoColor);
            WriteColored($"{trustBar} {npc.TrustLevel,3}%", trustColor);
            WriteColored($" │ Korku: ", InfoColor);
            WriteLineColored($"{fearBar} {npc.FearLevel,3}%", EmotionColor);

            WriteLineColored($"      └─ {npc.Role}", InfoColor);
        }
    }

    // =============================================
    // Sorgulama (İnteraktif Sohbet)
    // =============================================

    private async Task StartInterrogationAsync(int npcId)
    {
        var npc = await _repository.GetNPCByIdAsync(npcId);
        if (npc == null)
        {
            WriteLineColored("\n  ⚠️  Bu ID'ye sahip bir şüpheli bulunamadı.", ErrorColor);
            Thread.Sleep(1500);
            return;
        }

        _currentNPC = npc;

        SafeClear();
        DrawHeader($"🔍 SORGULAMA: {npc.Name.ToUpperInvariant()}");
        Console.WriteLine();
        WriteLineColored($"  📋 Rol: {npc.Role}", InfoColor);
        WriteLineColored($"  💬 Güven: {npc.TrustLevel}% | 😰 Korku: {npc.FearLevel}%", InfoColor);
        DrawSeparator();
        WriteLineColored("  💡 Sorgunuzu yazın. 'degistir' ile ana menüye dönün.", CommandColor);
        DrawSeparator();
        Console.WriteLine();

        // Sohbet döngüsü
        while (true)
        {
            WriteColored("  🕵️ Siz: ", TitleColor);
            var question = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(question))
                continue;

            if (question.ToLowerInvariant() == "degistir")
                return;

            if (question.ToLowerInvariant() == "ipucular")
            {
                await ShowCluesInline();
                continue;
            }

            if (question.ToLowerInvariant() == "cikis")
            {
                ShowExitScreen();
                Environment.Exit(0);
                return;
            }

            // AI'dan cevap al
            WriteLineColored("\n  ⏳ NPC düşünüyor...", InfoColor);

            var result = await _dialogManager.ProcessQuestionAsync(npcId, question);

            if (result == null)
            {
                WriteLineColored("  ⚠️  Sorgulama sırasında bir hata oluştu.", ErrorColor);
                continue;
            }

            var (response, updatedNPC) = result.Value;
            _currentNPC = updatedNPC;

            // Cevabı göster
            Console.WriteLine();
            DrawDialogBox(npc.Name, response, updatedNPC);
            Console.WriteLine();
        }
    }

    // =============================================
    // Diyalog Kutusu
    // =============================================

    private void DrawDialogBox(string npcName, AIInteractionResponse response, NPC updatedNPC)
    {
        var emotionEmoji = GetEmotionEmoji(response.Emotion);
        var trustChangeStr = response.TrustChange > 0
            ? $"+{response.TrustChange}"
            : response.TrustChange.ToString();
        var trustChangeColor = response.TrustChange > 0 ? TrustUpColor
            : response.TrustChange < 0 ? TrustDownColor : InfoColor;

        WriteLineColored("  ┌──────────────────────────────────────────────────┐", DialogColor);
        WriteColored("  │ ", DialogColor);
        WriteColored($"{emotionEmoji} {npcName}", NPCColor);
        WriteColored($"  [{response.Emotion}]", EmotionColor);
        Console.WriteLine();
        WriteLineColored("  ├──────────────────────────────────────────────────┤", DialogColor);

        // Diyalog metnini satırlara böl
        var words = response.Dialogue.Split(' ');
        var line = "  │  ";
        foreach (var word in words)
        {
            if (line.Length + word.Length + 1 > 54)
            {
                WriteLineColored(line.PadRight(54) + "│", DialogColor);
                line = "  │  " + word + " ";
            }
            else
            {
                line += word + " ";
            }
        }
        if (line.Trim().Length > 3)
            WriteLineColored(line.PadRight(54) + "│", DialogColor);

        WriteLineColored("  ├──────────────────────────────────────────────────┤", DialogColor);

        // Güven değişimi
        WriteColored("  │  📊 Güven Değişimi: ", InfoColor);
        WriteColored($"{trustChangeStr}", trustChangeColor);
        WriteColored($"  │  Toplam Güven: ", InfoColor);
        WriteLineColored($"{updatedNPC.TrustLevel}%", trustChangeColor);

        // Sır açığa çıktıysa
        if (!string.IsNullOrEmpty(response.RevealedSecret))
        {
            WriteLineColored("  ├──────────────────────────────────────────────────┤", DialogColor);
            WriteColored("  │  🔓 SIR AÇIĞA ÇIKTI: ", SecretColor);
            WriteLineColored(response.RevealedSecret, SecretColor);
        }

        WriteLineColored("  └──────────────────────────────────────────────────┘", DialogColor);
    }

    // =============================================
    // İpuçları Ekranı
    // =============================================

    private async Task ShowCluesAsync()
    {
        SafeClear();
        DrawHeader("🔎 İPUÇLARI");
        Console.WriteLine();

        var clues = await _repository.GetAllCluesAsync();
        var npcs = await _repository.GetAllNPCsAsync();
        var npcDict = npcs.ToDictionary(n => n.NPCId, n => n.Name);

        foreach (var clue in clues)
        {
            WriteColored($"  📌 [{clue.ClueId}] ", ClueColor);
            WriteLineColored(clue.Title, ConsoleColor.White);

            WriteLineColored($"      {clue.Description}", InfoColor);

            if (clue.RelatedNPCId.HasValue && npcDict.TryGetValue(clue.RelatedNPCId.Value, out var npcName))
            {
                WriteLineColored($"      🔗 İlişkili Şüpheli: {npcName}", NPCColor);
            }
            Console.WriteLine();
        }

        WriteLineColored("  Devam etmek için bir tuşa basın...", InfoColor);
        SafeReadKey();
    }

    private async Task ShowCluesInline()
    {
        var clues = await _repository.GetAllCluesAsync();
        Console.WriteLine();
        WriteLineColored("  ── İPUÇLARI ──────────────────────────────────────", ClueColor);
        foreach (var clue in clues)
        {
            WriteLineColored($"  📌 {clue.Title}: {clue.Description}", ClueColor);
        }
        WriteLineColored("  ──────────────────────────────────────────────────", ClueColor);
        Console.WriteLine();
    }

    // =============================================
    // Sorgulama Geçmişi
    // =============================================

    private async Task ShowDialogHistoryMenuAsync()
    {
        SafeClear();
        DrawHeader("📜 SORGULAMA GEÇMİŞİ");
        Console.WriteLine();

        var npcs = await _repository.GetAllNPCsAsync();
        foreach (var npc in npcs)
        {
            WriteLineColored($"  [{npc.NPCId}] {npc.Name}", NPCColor);
        }

        Console.WriteLine();
        WriteColored("  ▶ Geçmişini görmek istediğiniz şüphelinin numarası: ", TitleColor);
        var input = Console.ReadLine()?.Trim();

        if (int.TryParse(input, out int npcId))
        {
            var npc = await _repository.GetNPCByIdAsync(npcId);
            if (npc == null)
            {
                WriteLineColored("  ⚠️  Şüpheli bulunamadı.", ErrorColor);
                Thread.Sleep(1500);
                return;
            }

            var logs = await _dialogManager.GetDialogHistoryAsync(npcId);
            var logList = logs.ToList();

            SafeClear();
            DrawHeader($"📜 {npc.Name} — SORGULAMA GEÇMİŞİ");
            Console.WriteLine();

            if (!logList.Any())
            {
                WriteLineColored("  Bu şüpheliyle henüz konuşulmamış.", InfoColor);
            }
            else
            {
                foreach (var log in logList)
                {
                    WriteColored($"  [{log.CreatedAt:HH:mm}] ", InfoColor);
                    WriteLineColored($"🕵️ {log.PlayerQuestion}", TitleColor);
                    WriteColored($"          ", InfoColor);
                    WriteColored($"💬 {log.NPCResponse}", DialogColor);
                    WriteColored($"  [{log.DetectedEmotion}]", EmotionColor);
                    var changeStr = log.TrustChange > 0 ? $"+{log.TrustChange}" : log.TrustChange.ToString();
                    WriteLineColored($"  (Güven: {changeStr})", log.TrustChange >= 0 ? TrustUpColor : TrustDownColor);
                    Console.WriteLine();
                }
            }

            WriteLineColored("  Devam etmek için bir tuşa basın...", InfoColor);
            SafeReadKey();
        }
    }

    // =============================================
    // Çıkış Ekranı
    // =============================================

    private void ShowExitScreen()
    {
        SafeClear();
        WriteLineColored(@"
    ╔══════════════════════════════════════════════════════════╗
    ║                                                          ║
    ║        🔍  Soruşturma dosyası kapatıldı.  🔍            ║
    ║                                                          ║
    ║        Oynadığınız için teşekkürler!                      ║
    ║        AI Dedektiflik RPG — v1.0                          ║
    ║                                                          ║
    ╚══════════════════════════════════════════════════════════╝
", TitleColor);
    }

    // =============================================
    // Yardımcı Fonksiyonlar
    // =============================================

    private void DrawHeader(string title)
    {
        var padding = Math.Max(0, (52 - title.Length) / 2);
        var paddedTitle = new string(' ', padding) + title;
        WriteLineColored("  ╔══════════════════════════════════════════════════════╗", TitleColor);
        WriteLineColored($"  ║  {paddedTitle.PadRight(52)}║", TitleColor);
        WriteLineColored("  ╚══════════════════════════════════════════════════════╝", TitleColor);
    }

    private void DrawSeparator()
    {
        WriteLineColored("  ──────────────────────────────────────────────────────", InfoColor);
    }

    private string GetProgressBar(int value, int max, int width)
    {
        var filled = (int)((double)value / max * width);
        filled = Math.Clamp(filled, 0, width);
        var empty = width - filled;
        return $"[{"█".PadRight(filled, '█')}{"░".PadRight(empty, '░')}]";
    }

    private string GetEmotionEmoji(string emotion)
    {
        return emotion.ToLowerInvariant() switch
        {
            "sinirli" or "saldırgan" => "😡",
            "korkmuş" or "tedirgin" => "😰",
            "sakin" => "😐",
            "samimi" => "😊",
            "pişman" => "😢",
            "şüpheli" => "🤨",
            "sessiz" or "dalgın" => "😶",
            _ => "🗣️"
        };
    }

    private void WriteColored(string text, ConsoleColor color)
    {
        var prev = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ForegroundColor = prev;
    }

    private void WriteLineColored(string text, ConsoleColor color)
    {
        var prev = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = prev;
    }

    private static void SafeReadKey()
    {
        if (!Console.IsInputRedirected)
        {
            try { Console.ReadKey(true); } catch { }
        }
    }

    private static void SafeClear()
    {
        if (!Console.IsOutputRedirected)
        {
            try { Console.Clear(); } catch { }
        }
    }
}
