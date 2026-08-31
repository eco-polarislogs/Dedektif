using Dapper;
using DedektiflikRPG.Models;
using DedektiflikRPG.Core.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;
using Npgsql;
using System.Data;
using System.IO;

namespace DedektiflikRPG.Data;

/// <summary>
/// Dapper ile PostgreSQL, SQLite & SQL Server veritabanı işlemlerini yöneten hibrit repository sınıfı.
/// 100.000+ satırlık AI diyalog ve senaryo veri setini yüksek performansla işler.
/// </summary>
public class DatabaseRepository : IGameRepository
{
    private readonly string _connectionString;
    private readonly bool _isSqlite;
    private readonly bool _isPostgres;

    public DatabaseRepository(string connectionString, string? provider = null)
    {
        _connectionString = string.IsNullOrWhiteSpace(connectionString)
            ? "Data Source=Data/dedektiflik.db"
            : connectionString;

        bool requestedPostgres = (provider?.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase) == true) ||
                                  _connectionString.Contains("Host=") || _connectionString.Contains("Port=5432") || _connectionString.Contains("Username=");

        if (requestedPostgres)
        {
            try
            {
                using var testConn = new NpgsqlConnection(_connectionString);
                testConn.Open();
                _isPostgres = true;
                _isSqlite = false;
                EnsurePostgresDatabaseCreated();
                Console.WriteLine("  🐘 PostgreSQL Veritabanına Başarıyla Bağlanıldı!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ⚠️ PostgreSQL bağlantısı kurulamadı ({ex.Message}). Güvenli SQLite motoruna geçildi.");
                _isPostgres = false;
                _isSqlite = true;
                _connectionString = "Data Source=Data/dedektiflik.db";
                EnsureDatabaseCreated();
            }
        }
        else
        {
            _isPostgres = false;
            _isSqlite = true;
            EnsureDatabaseCreated();
        }
    }

    public IDbConnection CreateConnection()
    {
        if (_isPostgres)
        {
            return new NpgsqlConnection(_connectionString);
        }

        if (_isSqlite)
            return new SqliteConnection(_connectionString);

        return new SqlConnection(_connectionString);
    }

    private void EnsurePostgresDatabaseCreated()
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            // Connected successfully to PostgreSQL!
        }
        catch
        {
            // PostgreSQL server not running locally; fallback silently to SQLite
        }
    }

    private void EnsureDatabaseCreated()
    {
        try
        {
            var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "dedektiflik.db");
            var schemaPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "schema.sql");
            
            var dir = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!File.Exists(dbPath) && File.Exists(schemaPath))
            {
                var schemaSql = File.ReadAllText(schemaPath);
                using var conn = new SqliteConnection(_connectionString);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = schemaSql;
                cmd.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠️ SQLite veritabanı başlatma uyarısı: {ex.Message}");
        }
    }

    // =============================================
    // NPC İşlemleri
    // =============================================

    public async Task<IEnumerable<NPC>> GetAllNPCsAsync()
    {
        using var db = CreateConnection();
        return await db.QueryAsync<NPC>("SELECT * FROM NPCs ORDER BY NPCId");
    }

    public async Task<NPC?> GetNPCByIdAsync(int npcId)
    {
        using var db = CreateConnection();
        return await db.QueryFirstOrDefaultAsync<NPC>(
            "SELECT * FROM NPCs WHERE NPCId = @NPCId",
            new { NPCId = npcId });
    }

    public async Task UpdateNPCTrustAsync(int npcId, int trustChange)
    {
        using var db = CreateConnection();
        string tableName = (npcId >= 100) ? "GolgeSehirNPCs" : "NPCs";
        await db.ExecuteAsync($@"
            UPDATE {tableName} 
            SET TrustLevel = 
                CASE 
                    WHEN TrustLevel + @TrustChange > 100 THEN 100
                    WHEN TrustLevel + @TrustChange < 0 THEN 0
                    ELSE TrustLevel + @TrustChange
                END
            WHERE NPCId = @NPCId",
            new { NPCId = npcId, TrustChange = trustChange });
    }

    // =============================================
    // İpucu İşlemleri
    // =============================================

    public async Task<IEnumerable<Clue>> GetAllCluesAsync()
    {
        using var db = CreateConnection();
        try
        {
            return await db.QueryAsync<Clue>("SELECT * FROM Clues ORDER BY ClueId");
        }
        catch
        {
            // Clues yoksa SceneObjects'ten dönüştür
            var objs = await db.QueryAsync<SceneObject>("SELECT * FROM SceneObjects");
            return objs.Select(o => new Clue
            {
                ClueId = o.ObjectId,
                Title = o.ObjectName,
                Description = o.Description,
                RelatedNPCId = o.NPCId,
                Status = "Pending"
            });
        }
    }

    public async Task<IEnumerable<Clue>> GetCluesInBagAsync()
    {
        using var db = CreateConnection();
        try
        {
            return await db.QueryAsync<Clue>("SELECT * FROM Clues WHERE Status = 'KeptInBag' ORDER BY ClueId");
        }
        catch
        {
            return Enumerable.Empty<Clue>();
        }
    }
    public async Task UpdateNPCAsync(NPC npc)
    {
        using var db = CreateConnection();
        await db.ExecuteAsync(@"
            UPDATE NPCs 
            SET TrustLevel = @TrustLevel, FearLevel = @FearLevel, IsGuilty = @IsGuilty 
            WHERE NPCId = @NPCId", 
            npc);
    }

    public async Task UpdateClueStatusAsync(int clueId, string status)
    {
        using var db = CreateConnection();
        try
        {
            await db.ExecuteAsync("UPDATE Clues SET Status = @Status WHERE ClueId = @ClueId", new { ClueId = clueId, Status = status });
        }
        catch { }
    }

    public async Task<IEnumerable<Clue>> GetCluesByNPCIdAsync(int npcId)
    {
        using var db = CreateConnection();
        try
        {
            return await db.QueryAsync<Clue>(
                "SELECT * FROM Clues WHERE RelatedNPCId = @NPCId ORDER BY ClueId",
                new { NPCId = npcId });
        }
        catch
        {
            return Enumerable.Empty<Clue>();
        }
    }

    // =============================================
    // Diyalog Kayıt İşlemleri
    // =============================================

    public async Task AddDialogLogAsync(DialogLog log)
    {
        using var db = CreateConnection();
        await db.ExecuteAsync(@"
            INSERT INTO DialogLogs (NPCId, PlayerQuestion, NPCResponse, DetectedEmotion, TrustChange, CreatedAt)
            VALUES (@NPCId, @PlayerQuestion, @NPCResponse, @DetectedEmotion, @TrustChange, @CreatedAt)",
            log);
    }

    public async Task<IEnumerable<DialogLog>> GetDialogLogsByNPCIdAsync(int npcId)
    {
        using var db = CreateConnection();
        return await db.QueryAsync<DialogLog>(
            "SELECT * FROM DialogLogs WHERE NPCId = @NPCId ORDER BY CreatedAt DESC",
            new { NPCId = npcId });
    }

    public async Task ClearAllDialogLogsAsync()
    {
        using var db = CreateConnection();
        await db.ExecuteAsync("DELETE FROM DialogLogs");
    }

    public async Task ClearPlayerInventoryAsync()
    {
        using var db = CreateConnection();
        await db.ExecuteAsync("DELETE FROM PlayerInventory");
    }

    public async Task<IEnumerable<DialogLog>> GetRecentDialogLogsAsync(int npcId, int count = 10)
    {
        using var db = CreateConnection();
        if (_isSqlite)
        {
            return await db.QueryAsync<DialogLog>(
                "SELECT * FROM DialogLogs WHERE NPCId = @NPCId ORDER BY CreatedAt DESC LIMIT @Count",
                new { NPCId = npcId, Count = count });
        }
        else
        {
            return await db.QueryAsync<DialogLog>(
                "SELECT TOP (@Count) * FROM DialogLogs WHERE NPCId = @NPCId ORDER BY CreatedAt DESC",
                new { NPCId = npcId, Count = count });
        }
    }

    public async Task<IEnumerable<NPCDialogue>> GetLocalAIPoolAsync(int npcId)
    {
        using var db = CreateConnection();
        if (npcId >= 100)
        {
            return await db.QueryAsync<NPCDialogue>(
                "SELECT DialogueId, NPCId, Difficulty, Category, QuestionText AS PlayerText, ResponseText AS NPCResponse, GuiltyResponseText AS GuiltyResponses FROM GolgeSehirNPCDialogues WHERE NPCId = @NPCId",
                new { NPCId = npcId });
        }
        return await db.QueryAsync<NPCDialogue>(
            "SELECT * FROM NPCDialogues WHERE NPCId = @NPCId",
            new { NPCId = npcId });
    }

    // =============================================
    // Seed Data — Varsayılan Veri Yükleme
    // =============================================

    public async Task SeedDataAsync()
    {
        using var db = CreateConnection();
        try
        {
            var npcCount = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM NPCs");

            // schema.sql varsa çalıştır (Eğer tablo yoksa vs.)
            var schemaPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "schema.sql");
            if (File.Exists(schemaPath) && npcCount < 5)
            {
                var sql = File.ReadAllText(schemaPath);
                await db.ExecuteAsync(sql);
            }


            // Gelişmiş yapay zeka hafıza verilerini yükle (1000+ Cümle)
            var aiSeeder = new AISeeder(this);
            await aiSeeder.Seed1000PlusDialoguesAsync();

            // Gölge Şehir Tablolarını ve Verilerini Yükle
            await EnsureGolgeSehirTablesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠️ SeedDataAsync uyarısı: {ex.Message}");
        }
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using var db = CreateConnection();
            db.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> TablesExistAsync()
    {
        try
        {
            using var db = CreateConnection();
            var count = await db.ExecuteScalarAsync<int>(
                _isSqlite 
                    ? "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name IN ('NPCs', 'SceneObjects', 'DialogLogs')"
                    : "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME IN ('NPCs', 'Clues', 'DialogLogs')");
            return count >= 2;
        }
        catch
        {
            return false;
        }
    }

    // =============================================
    // Oyun Oturumu İşlemleri (GameSessions)
    // =============================================

    public async Task<int> CreateGameSessionAsync(int guiltyNpcId)
    {
        using var db = CreateConnection();
        if (_isSqlite)
        {
            await db.ExecuteAsync("INSERT INTO GameSessions (GuiltyNPCId, StartedAt) VALUES (@GuiltyNPCId, datetime('now'))", new { GuiltyNPCId = guiltyNpcId });
            return await db.ExecuteScalarAsync<int>("SELECT last_insert_rowid()");
        }
        else
        {
            return await db.ExecuteScalarAsync<int>(@"
                INSERT INTO GameSessions (GuiltyNPCId, StartedAt)
                VALUES (@GuiltyNPCId, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new { GuiltyNPCId = guiltyNpcId });
        }
    }

    public async Task EndGameSessionAsync(int sessionId, string result, int? accusedNpcId, int totalQuestions, int cluesCollected)
    {
        using var db = CreateConnection();
        string dateFunc = _isSqlite ? "datetime('now')" : "GETDATE()";
        await db.ExecuteAsync($@"
            UPDATE GameSessions 
            SET EndedAt = {dateFunc}, Result = @Result, AccusedNPCId = @AccusedNPCId,
                TotalQuestions = @TotalQuestions, CluesCollected = @CluesCollected
            WHERE SessionId = @SessionId",
            new { SessionId = sessionId, Result = result, AccusedNPCId = accusedNpcId, TotalQuestions = totalQuestions, CluesCollected = cluesCollected });
    }

    public async Task LogPlayerActionAsync(int sessionId, string actionType, int? targetId, string? details)
    {
        using var db = CreateConnection();
        string dateFunc = _isSqlite ? "datetime('now')" : "GETDATE()";
        await db.ExecuteAsync($@"
            INSERT INTO PlayerActions (SessionId, ActionType, TargetId, Details, CreatedAt)
            VALUES (@SessionId, @ActionType, @TargetId, @Details, {dateFunc})",
            new { SessionId = sessionId, ActionType = actionType, TargetId = targetId, Details = details });
    }

    public async Task SaveGameStateAsync(int sessionId, string stateDataJson)
    {
        using var db = CreateConnection();
        string dateFunc = _isSqlite ? "datetime('now')" : "GETDATE()";
        var exists = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM GameStates WHERE SessionId = @SessionId",
            new { SessionId = sessionId });
        
        if (exists > 0)
        {
            await db.ExecuteAsync($@"
                UPDATE GameStates SET StateData = @StateData, SavedAt = {dateFunc}
                WHERE SessionId = @SessionId",
                new { SessionId = sessionId, StateData = stateDataJson });
        }
        else
        {
            await db.ExecuteAsync($@"
                INSERT INTO GameStates (SessionId, StateData, SavedAt)
                VALUES (@SessionId, @StateData, {dateFunc})",
                new { SessionId = sessionId, StateData = stateDataJson });
        }
    }

    public async Task<string?> LoadGameStateAsync(int sessionId)
    {
        using var db = CreateConnection();
        return await db.ExecuteScalarAsync<string?>(
            _isSqlite 
                ? "SELECT StateData FROM GameStates WHERE SessionId = @SessionId ORDER BY SavedAt DESC LIMIT 1"
                : "SELECT TOP 1 StateData FROM GameStates WHERE SessionId = @SessionId ORDER BY SavedAt DESC",
            new { SessionId = sessionId });
    }

    public async Task<IEnumerable<HelperMessage>> GetHelperMessagesAsync(string context, string? buildingName = null)
    {
        using var db = CreateConnection();
        try
        {
            if (!string.IsNullOrEmpty(buildingName))
            {
                return await db.QueryAsync<HelperMessage>(@"
                    SELECT * FROM HelperMessages 
                    WHERE Context = @Context AND (BuildingName = @BuildingName OR BuildingName IS NULL)
                    ORDER BY Priority DESC",
                    new { Context = context, BuildingName = buildingName });
            }
            return await db.QueryAsync<HelperMessage>(@"
                SELECT * FROM HelperMessages WHERE Context = @Context ORDER BY Priority DESC",
                new { Context = context });
        }
        catch
        {
            return Enumerable.Empty<HelperMessage>();
        }
    }

    public async Task<string> AnalyzeCluesForHelperAsync(List<int> clueIds, int guiltyNpcId)
    {
        if (clueIds == null || clueIds.Count == 0)
            return "Amirims, çantanızda henüz delil yok! Binalara girip delilleri toplamalısınız.";

        using var db = CreateConnection();
        try
        {
            var clues = await db.QueryAsync<Clue>(
                "SELECT * FROM Clues WHERE ClueId IN @Ids",
                new { Ids = clueIds });

            var clueList = clues.ToList();
            var relatedToGuilty = clueList.Where(c => c.RelatedNPCId == guiltyNpcId).ToList();

            if (relatedToGuilty.Count >= 2)
                return $"Amirims, çantanızdaki {clueList.Count} delilden bazıları aynı şüpheliyi işaret ediyor gibi görünüyor. Bu izleri dikkatle takip edin!";
            else if (clueList.Count >= 3)
                return $"Amirims, {clueList.Count} delil toplamışsınız. Farklı binalardaki delilleri karşılaştırın, bazıları birbiriyle bağlantılı olabilir!";
            else
                return $"Amirims, henüz {clueList.Count} delil topladınız. Daha fazla bina incelemenizi öneririm. Her binada 3 delil var!";
        }
        catch
        {
            return $"Amirims, çantanızdaki {clueIds.Count} delili dikkatle inceleyin. Katil bu delillerin arasında gizli!";
        }
    }

    public async Task LogDialogWithCategoryAsync(int npcId, string playerQuestion, string npcResponse, int difficulty, string category)
    {
        using var db = CreateConnection();
        string dateFunc = _isSqlite ? "datetime('now')" : "GETDATE()";
        await db.ExecuteAsync($@"
            INSERT INTO DialogLogs (NPCId, PlayerQuestion, NPCResponse, Difficulty, Category, CreatedAt)
            VALUES (@NPCId, @PlayerQuestion, @NPCResponse, @Difficulty, @Category, {dateFunc})",
            new { NPCId = npcId, PlayerQuestion = playerQuestion, NPCResponse = npcResponse, Difficulty = difficulty, Category = category });
    }

    public async Task EnsureHelperTablesAsync()
    {
        using var db = CreateConnection();
        if (_isSqlite)
        {
            await db.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS GameSessions (
                    SessionId INTEGER PRIMARY KEY AUTOINCREMENT,
                    GuiltyNPCId INTEGER NOT NULL,
                    StartedAt TEXT NOT NULL DEFAULT (datetime('now')),
                    EndedAt TEXT NULL,
                    Result TEXT NULL,
                    AccusedNPCId INTEGER NULL,
                    TotalQuestions INTEGER DEFAULT 0,
                    CluesCollected INTEGER DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS PlayerActions (
                    ActionId INTEGER PRIMARY KEY AUTOINCREMENT,
                    SessionId INTEGER NOT NULL,
                    ActionType TEXT NOT NULL,
                    TargetId INTEGER NULL,
                    Details TEXT NULL,
                    CreatedAt TEXT NOT NULL DEFAULT (datetime('now'))
                );

                CREATE TABLE IF NOT EXISTS GameStates (
                    StateId INTEGER PRIMARY KEY AUTOINCREMENT,
                    SessionId INTEGER NOT NULL,
                    StateData TEXT NOT NULL,
                    SavedAt TEXT NOT NULL DEFAULT (datetime('now'))
                );

                CREATE TABLE IF NOT EXISTS HelperMessages (
                    MessageId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Context TEXT NOT NULL,
                    BuildingName TEXT NULL,
                    Message TEXT NOT NULL,
                    Priority INTEGER DEFAULT 1,
                    IsOneTime INTEGER DEFAULT 1
                );
            ");
        }
        else
        {
            await db.ExecuteAsync(@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'GameSessions')
                CREATE TABLE GameSessions (
                    SessionId INT IDENTITY(1,1) PRIMARY KEY,
                    GuiltyNPCId INT NOT NULL,
                    StartedAt DATETIME NOT NULL DEFAULT GETDATE(),
                    EndedAt DATETIME NULL,
                    Result NVARCHAR(50) NULL,
                    AccusedNPCId INT NULL,
                    TotalQuestions INT DEFAULT 0,
                    CluesCollected INT DEFAULT 0
                );

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PlayerActions')
                CREATE TABLE PlayerActions (
                    ActionId INT IDENTITY(1,1) PRIMARY KEY,
                    SessionId INT NOT NULL,
                    ActionType NVARCHAR(100) NOT NULL,
                    TargetId INT NULL,
                    Details NVARCHAR(MAX) NULL,
                    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
                );

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'GameStates')
                CREATE TABLE GameStates (
                    StateId INT IDENTITY(1,1) PRIMARY KEY,
                    SessionId INT NOT NULL,
                    StateData NVARCHAR(MAX) NOT NULL,
                    SavedAt DATETIME NOT NULL DEFAULT GETDATE()
                );

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'HelperMessages')
                CREATE TABLE HelperMessages (
                    MessageId INT IDENTITY(1,1) PRIMARY KEY,
                    Context NVARCHAR(100) NOT NULL,
                    BuildingName NVARCHAR(100) NULL,
                    Message NVARCHAR(MAX) NOT NULL,
                    Priority INT DEFAULT 1,
                    IsOneTime BIT DEFAULT 1
                );
            ");
        }
    }

    public async Task SeedHelperMessagesAsync()
    {
        using var db = CreateConnection();
        try
        {
            var count = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM HelperMessages");
            if (count > 0) return;

            await db.ExecuteAsync(@"
                INSERT INTO HelperMessages (Context, BuildingName, Message, Priority, IsOneTime) VALUES
                ('splash', NULL, 'Hoş geldin Amirims! Ben Yardımcı Dedektif Çetin. Bu karanlık davada sana yardımcı olacağım. Hazır olduğunda dosyayı aç ve soruşturmaya başlayalım!', 1, 1),
                ('story_end', NULL, 'Soruşturmaya başlamadan önce şunu bil Amirims: Kasabada 5 bina ve 5 şüpheli var. Her binada 3 delil bulabilirsin. Delilleri dikkatle incele ve çantana al. Ama dikkat et, çantanda yalnızca 5 delil bulunabilir!', 1, 1),
                ('map_enter', NULL, 'İşte kasaba haritası Amirims! Haritadaki binalara tıklayarak soruşturmana başlayabilirsin. Her binada deliller ve şüpheliler seni bekliyor. Dikkatli ol!', 1, 1),
                ('building_enter', NULL, 'Olay yerindeki delilleri inceleyebilir, çantana atabilirsin. Ama dikkat et, çantanda yalnızca 5 delil bulunabilir!', 2, 0),
                ('building_enter', 'Kasap', 'Burası Kasap Hasan''ın dükkânı Amirims. Tezgahtaki satıra, deftere ve yırtık önlüğe dikkat et. Hasan sert bir adam...', 3, 1),
                ('building_enter', 'Eczane', 'Eczacı Selma''nın dükkânındayız Amirims. Tezgah altına, reçete defterine ve ilaç şişelerine dikkat et...', 3, 1),
                ('building_enter', 'Muhtarlık', 'Muhtar Kemal''in ofisindeyiz Amirims. Çekmecesindeki mektuplara, kırık gözlüğe ve kasasına dikkat et...', 3, 1),
                ('building_enter', 'Karakol', 'Komiser Güneş''in karakolundayız Amirims. Polis rozetine, gizli dosyaya ve kayıp düğmeye dikkat et...', 3, 1),
                ('building_enter', 'Terzi', 'Terzi Yahya''nın atölyesindeyiz Amirims. İplik makarasına, yırtık kumaşa ve gizli cebe dikkat et...', 3, 1),
                ('bag_open', NULL, 'Çantandaki delilleri İncele butonuyla detaylı inceleyebilirsin Amirims. Suçluyu bulmak için ipuçlarını birleştir!', 1, 1),
                ('clue_inspect', NULL, 'Bu delili dikkatle incele Amirims. Suçluya ait olabilecek izler görebilirsin.', 1, 0),
                ('npc_talk', NULL, 'Dikkatli soru sor Amirims, sadece 5 soru hakkın var! Sorularını iyi seç.', 1, 0),
                ('autopsy_ready', NULL, 'Amirims! Adli Tıp Merkezi''nden otopsi raporu geldi! Harita ekranındaki rapora tıklayarak hemen inceleyin!', 1, 1),
                ('accuse', NULL, 'Son kararını vermeden önce tüm delilleri gözden geçir Amirims. Yanlış suçlama kasaba için felaket olur!', 1, 1);
            ");
        }
        catch { }
    }

    // =============================================
    // Backend API Metotları
    // =============================================

    public async Task<int> StartGameSessionAsync(int guiltyNpcId)
    {
        using var db = CreateConnection();
        try
        {
            var query = _isSqlite 
                ? "INSERT INTO GameSessions (GuiltyNPCId, StartedAt) VALUES (@GuiltyNPCId, @StartedAt); SELECT last_insert_rowid();"
                : "INSERT INTO GameSessions (GuiltyNPCId, StartedAt) VALUES (@GuiltyNPCId, @StartedAt); SELECT CAST(SCOPE_IDENTITY() as int);";
            return await db.ExecuteScalarAsync<int>(query, new { GuiltyNPCId = guiltyNpcId, StartedAt = DateTime.UtcNow.ToString("O") });
        }
        catch { return 1; }
    }

    // =============================================
    // Gölge Şehir Özel Metotları
    // =============================================

    public async Task EnsureGolgeSehirTablesAsync()
    {
        using var db = CreateConnection();
        try
        {
            var schemaPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "schema_golge_sehir.sql");
            if (File.Exists(schemaPath))
            {
                var sql = await File.ReadAllTextAsync(schemaPath);
                await db.ExecuteAsync(sql);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠️ EnsureGolgeSehirTablesAsync hatası: {ex.Message}");
        }
    }

    public async Task<IEnumerable<NPC>> GetGolgeSehirNPCsAsync()
    {
        using var db = CreateConnection();
        try
        {
            return await db.QueryAsync<NPC>("SELECT * FROM GolgeSehirNPCs ORDER BY NPCId");
        }
        catch
        {
            return Enumerable.Empty<NPC>();
        }
    }

    public async Task<NPC?> GetGolgeSehirNPCByIdAsync(int id)
    {
        using var db = CreateConnection();
        try
        {
            return await db.QueryFirstOrDefaultAsync<NPC>("SELECT * FROM GolgeSehirNPCs WHERE NPCId = @NPCId", new { NPCId = id });
        }
        catch
        {
            return null;
        }
    }

    public async Task<IEnumerable<Clue>> GetGolgeSehirCluesAsync()
    {
        using var db = CreateConnection();
        try
        {
            var rows = await db.QueryAsync<dynamic>("SELECT * FROM GolgeSehirClues ORDER BY ClueId");
            return rows.Select(r => new Clue
            {
                ClueId = (int)r.ClueId,
                Title = (string)r.Name,
                Description = (string)r.Description,
                RelatedNPCId = (int)r.NPCId,
                Status = (string)(r.Status ?? "Pending")
            });
        }
        catch
        {
            return Enumerable.Empty<Clue>();
        }
    }

    public async Task<IEnumerable<NPCDialogue>> GetGolgeSehirDialoguesAsync(int npcId, string? category = null)
    {
        using var db = CreateConnection();
        try
        {
            if (string.IsNullOrEmpty(category))
            {
                var rows = await db.QueryAsync<dynamic>("SELECT * FROM GolgeSehirNPCDialogues WHERE NPCId = @NPCId", new { NPCId = npcId });
                return rows.Select(r => new NPCDialogue
                {
                    DialogueId = (int)r.DialogueId,
                    NPCId = (int)r.NPCId,
                    PlayerText = (string)r.QuestionText,
                    NPCResponse = (string)r.ResponseText,
                    GuiltyResponses = (string)r.GuiltyResponseText,
                    Difficulty = (int)(r.Difficulty ?? 1),
                    Category = (string)(r.Category ?? "tanisma")
                });
            }
            else
            {
                var rows = await db.QueryAsync<dynamic>("SELECT * FROM GolgeSehirNPCDialogues WHERE NPCId = @NPCId AND Category = @Category", new { NPCId = npcId, Category = category });
                return rows.Select(r => new NPCDialogue
                {
                    DialogueId = (int)r.DialogueId,
                    NPCId = (int)r.NPCId,
                    PlayerText = (string)r.QuestionText,
                    NPCResponse = (string)r.ResponseText,
                    GuiltyResponses = (string)r.GuiltyResponseText,
                    Difficulty = (int)(r.Difficulty ?? 1),
                    Category = (string)(r.Category ?? "tanisma")
                });
            }
        }
        catch
        {
            return Enumerable.Empty<NPCDialogue>();
        }
    }

    public async Task<IEnumerable<HelperMessage>> GetGolgeSehirHelperMessagesAsync(string context, string? building = null)
    {
        using var db = CreateConnection();
        try
        {
            if (!string.IsNullOrEmpty(building))
            {
                return await db.QueryAsync<HelperMessage>(@"
                    SELECT * FROM GolgeSehirHelperMessages 
                    WHERE Context = @Context AND (BuildingName = @BuildingName OR BuildingName IS NULL)
                    ORDER BY Priority DESC",
                    new { Context = context, BuildingName = building });
            }
            return await db.QueryAsync<HelperMessage>(@"
                SELECT * FROM GolgeSehirHelperMessages WHERE Context = @Context ORDER BY Priority DESC",
                new { Context = context });
        }
        catch
        {
            return Enumerable.Empty<HelperMessage>();
        }
    }

    public async Task ResetGolgeSehirSessionAsync(int guiltyNpcId)
    {
        using var db = CreateConnection();
        try
        {
            await db.ExecuteAsync(@"
                UPDATE GolgeSehirNPCs 
                SET IsGuilty = CASE WHEN NPCId = @GuiltyId THEN 1 ELSE 0 END,
                    TrustLevel = 50,
                    FearLevel = 30",
                new { GuiltyId = guiltyNpcId });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠️ ResetGolgeSehirSessionAsync hatası: {ex.Message}");
        }
    }

}
