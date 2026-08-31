# -*- coding: utf-8 -*-
import sqlite3
import psycopg2
from psycopg2.extras import execute_batch
import os
import sys

def migrate_all_to_postgres():
    pg_host = "localhost"
    pg_port = 5432
    pg_user = "postgres"
    pg_pass = "EcReN12854700456."
    target_db = "dedektiflik_rpg"

    print("Connecting to PostgreSQL 'dedektiflik_rpg'...")
    pg_conn = psycopg2.connect(
        host=pg_host,
        port=pg_port,
        user=pg_user,
        password=pg_pass,
        dbname=target_db
    )
    pg_conn.autocommit = True
    pg_cur = pg_conn.cursor()

    sqlite_db = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "Data", "dedektiflik.db"))
    print(f"Connecting to SQLite: {sqlite_db}...")
    sq_conn = sqlite3.connect(sqlite_db)
    sq_cur = sq_conn.cursor()

    # 1. CREATE ALL SCHEMAS IN POSTGRESQL
    print("1. Creating all database tables in PostgreSQL...")
    pg_cur.execute("""
    -- 1. NPCs (Gizemli Kasaba)
    CREATE TABLE IF NOT EXISTS "NPCs" (
        "NPCId" INTEGER PRIMARY KEY,
        "Name" VARCHAR(100) NOT NULL,
        "Role" TEXT NOT NULL,
        "TrustLevel" INTEGER DEFAULT 50,
        "FearLevel" INTEGER DEFAULT 0,
        "IsGuilty" BOOLEAN DEFAULT FALSE,
        "SecretInfo" TEXT NOT NULL
    );

    -- 2. Clues (Gizemli Kasaba İpuçları)
    CREATE TABLE IF NOT EXISTS "Clues" (
        "ClueId" INTEGER PRIMARY KEY,
        "Title" VARCHAR(150) NOT NULL,
        "Description" TEXT NOT NULL,
        "RelatedNPCId" INTEGER,
        "Status" VARCHAR(50) DEFAULT 'Pending'
    );

    -- 3. NPCDialogues (780,000+ AI Diyalog Veri Seti)
    CREATE TABLE IF NOT EXISTS "NPCDialogues" (
        "DialogueId" SERIAL PRIMARY KEY,
        "NPCId" INTEGER NOT NULL,
        "Category" VARCHAR(50) NOT NULL,
        "PlayerText" TEXT NOT NULL,
        "NPCResponse" TEXT NOT NULL,
        "Difficulty" INTEGER DEFAULT 1,
        "GuiltyResponses" TEXT DEFAULT '',
        "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    );
    CREATE INDEX IF NOT EXISTS idx_dialogue_npc_cat ON "NPCDialogues"("NPCId", "Category");
    CREATE INDEX IF NOT EXISTS idx_dialogue_player ON "NPCDialogues"("PlayerText");

    -- 4. GolgeSehirNPCs (Gölge Şehir Karakterleri 101-108)
    CREATE TABLE IF NOT EXISTS "GolgeSehirNPCs" (
        "NPCId" INTEGER PRIMARY KEY,
        "Name" VARCHAR(100) NOT NULL,
        "Role" TEXT NOT NULL,
        "TrustLevel" INTEGER DEFAULT 50,
        "FearLevel" INTEGER DEFAULT 0,
        "IsGuilty" BOOLEAN DEFAULT FALSE,
        "SecretInfo" TEXT NOT NULL
    );

    -- 5. GolgeSehirClues (Gölge Şehir İpuçları)
    CREATE TABLE IF NOT EXISTS "GolgeSehirClues" (
        "ClueId" INTEGER PRIMARY KEY,
        "Name" VARCHAR(150) NOT NULL,
        "Description" TEXT NOT NULL,
        "NPCId" INTEGER,
        "Status" VARCHAR(50) DEFAULT 'Pending'
    );

    -- 6. GolgeSehirNPCDialogues (Gölge Şehir AI Diyalogları)
    CREATE TABLE IF NOT EXISTS "GolgeSehirNPCDialogues" (
        "DialogueId" SERIAL PRIMARY KEY,
        "NPCId" INTEGER NOT NULL,
        "Difficulty" INTEGER DEFAULT 1,
        "Category" VARCHAR(50) NOT NULL,
        "QuestionText" TEXT NOT NULL,
        "ResponseText" TEXT NOT NULL,
        "GuiltyResponseText" TEXT DEFAULT ''
    );

    -- 7. HelperMessages (Çetin / Yardımcı Dedektif Mesajları)
    CREATE TABLE IF NOT EXISTS "HelperMessages" (
        "MessageId" SERIAL PRIMARY KEY,
        "Context" VARCHAR(50) NOT NULL,
        "BuildingName" VARCHAR(50),
        "Message" TEXT NOT NULL,
        "Priority" INTEGER DEFAULT 1,
        "IsOneTime" BOOLEAN DEFAULT TRUE
    );

    -- 8. GameSessions (Oyun Oturumları)
    CREATE TABLE IF NOT EXISTS "GameSessions" (
        "SessionId" SERIAL PRIMARY KEY,
        "GuiltyNPCId" INTEGER NOT NULL,
        "StartedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
        "EndedAt" TIMESTAMP,
        "Result" VARCHAR(50),
        "AccusedNPCId" INTEGER,
        "TotalQuestions" INTEGER DEFAULT 0,
        "CluesCollected" INTEGER DEFAULT 0
    );

    -- 9. DialogLogs (Konuşma Geçmişi)
    CREATE TABLE IF NOT EXISTS "DialogLogs" (
        "LogId" SERIAL PRIMARY KEY,
        "NPCId" INTEGER NOT NULL,
        "PlayerQuestion" TEXT NOT NULL,
        "NPCResponse" TEXT NOT NULL,
        "Difficulty" INTEGER DEFAULT 1,
        "Category" VARCHAR(50) DEFAULT 'general',
        "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    );

    -- 10. PlayerActions (Oyuncu Eylemleri)
    CREATE TABLE IF NOT EXISTS "PlayerActions" (
        "ActionId" SERIAL PRIMARY KEY,
        "SessionId" INTEGER,
        "ActionType" VARCHAR(50) NOT NULL,
        "TargetId" INTEGER,
        "Details" TEXT,
        "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    );

    -- 11. GameState (Oyun Durumu)
    CREATE TABLE IF NOT EXISTS "GameState" (
        "StateId" SERIAL PRIMARY KEY,
        "SessionId" INTEGER,
        "StateData" TEXT,
        "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    );
    """)
    print("Schema created successfully!")

    # 2. MIGRATE DATA FOR ALL TABLES
    tables_to_migrate = [
        ("NPCs", "SELECT NPCId, Name, Role, TrustLevel, FearLevel, IsGuilty, SecretInfo FROM NPCs", 
         """INSERT INTO "NPCs" ("NPCId", "Name", "Role", "TrustLevel", "FearLevel", "IsGuilty", "SecretInfo") VALUES (%s, %s, %s, %s, %s, %s, %s) ON CONFLICT ("NPCId") DO NOTHING;""", True),

        ("Clues", "SELECT ClueId, Title, Description, RelatedNPCId, Status FROM Clues",
         """INSERT INTO "Clues" ("ClueId", "Title", "Description", "RelatedNPCId", "Status") VALUES (%s, %s, %s, %s, %s) ON CONFLICT ("ClueId") DO NOTHING;""", False),

        ("GolgeSehirNPCs", "SELECT NPCId, Name, Role, TrustLevel, FearLevel, IsGuilty, SecretInfo FROM GolgeSehirNPCs",
         """INSERT INTO "GolgeSehirNPCs" ("NPCId", "Name", "Role", "TrustLevel", "FearLevel", "IsGuilty", "SecretInfo") VALUES (%s, %s, %s, %s, %s, %s, %s) ON CONFLICT ("NPCId") DO NOTHING;""", True),

        ("GolgeSehirClues", "SELECT ClueId, Name, Description, NPCId, Status FROM GolgeSehirClues",
         """INSERT INTO "GolgeSehirClues" ("ClueId", "Name", "Description", "NPCId", "Status") VALUES (%s, %s, %s, %s, %s) ON CONFLICT ("ClueId") DO NOTHING;""", False),

        ("GolgeSehirNPCDialogues", "SELECT NPCId, Difficulty, Category, QuestionText, ResponseText, GuiltyResponseText FROM GolgeSehirNPCDialogues",
         """INSERT INTO "GolgeSehirNPCDialogues" ("NPCId", "Difficulty", "Category", "QuestionText", "ResponseText", "GuiltyResponseText") VALUES (%s, %s, %s, %s, %s, %s);""", False),

        ("HelperMessages", "SELECT Context, BuildingName, Message, Priority, IsOneTime FROM HelperMessages",
         """INSERT INTO "HelperMessages" ("Context", "BuildingName", "Message", "Priority", "IsOneTime") VALUES (%s, %s, %s, %s, %s);""", True)
    ]

    for tbl_name, select_sql, insert_sql, has_bool in tables_to_migrate:
        try:
            sq_cur.execute(select_sql)
            rows = sq_cur.fetchall()
            if not rows:
                print(f"Table {tbl_name} is empty in SQLite.")
                continue

            if has_bool:
                cleaned_rows = []
                for r in rows:
                    row_list = list(r)
                    # Convert boolean integer to bool
                    if "NPCs" in tbl_name:
                        row_list[5] = bool(row_list[5])
                    elif tbl_name == "HelperMessages":
                        row_list[4] = bool(row_list[4])
                    cleaned_rows.append(tuple(row_list))
                rows = cleaned_rows

            pg_cur.execute(f'SELECT count(*) FROM "{tbl_name}"')
            cnt = pg_cur.fetchone()[0]
            if cnt == 0:
                execute_batch(pg_cur, insert_sql, rows)
                print(f"[OK] Migrated {len(rows)} records into PostgreSQL table '{tbl_name}'.")
            else:
                print(f"[INFO] Table '{tbl_name}' already contains {cnt} records in PostgreSQL.")
        except Exception as ex:
            print(f"[ERROR] Error migrating {tbl_name}: {ex}")

    # Summary
    print("\n================== POSTGRESQL TABLES STATUS ==================")
    all_tables = ["NPCs", "Clues", "NPCDialogues", "GolgeSehirNPCs", "GolgeSehirClues", "GolgeSehirNPCDialogues", "HelperMessages", "GameSessions", "DialogLogs", "PlayerActions", "GameState"]
    for t in all_tables:
        pg_cur.execute(f'SELECT count(*) FROM "{t}"')
        count = pg_cur.fetchone()[0]
        print(f"  * Table '{t}': {count} rows")
    print("==============================================================")

    sq_conn.close()
    pg_cur.close()
    pg_conn.close()

if __name__ == "__main__":
    migrate_all_to_postgres()
