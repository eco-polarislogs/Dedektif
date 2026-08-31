# -*- coding: utf-8 -*-
import sqlite3
import psycopg2
from psycopg2.extras import execute_batch
import os
import sys

def migrate_to_postgres():
    pg_host = "localhost"
    pg_port = 5432
    pg_user = "postgres"
    pg_pass = "EcReN12854700456."
    target_db = "dedektiflik_rpg"

    print("1. Connecting to PostgreSQL default database 'postgres' to create 'dedektiflik_rpg'...")
    try:
        conn = psycopg2.connect(
            host=pg_host,
            port=pg_port,
            user=pg_user,
            password=pg_pass,
            dbname="postgres"
        )
        conn.autocommit = True
        cur = conn.cursor()
        
        cur.execute(f"SELECT 1 FROM pg_catalog.pg_database WHERE datname = '{target_db}'")
        exists = cur.fetchone()
        if not exists:
            print(f"Creating database {target_db}...")
            cur.execute(f"CREATE DATABASE {target_db};")
            print(f"Database {target_db} created successfully!")
        else:
            print(f"Database {target_db} already exists.")
        
        cur.close()
        conn.close()
    except Exception as e:
        print(f"Error creating database: {e}")
        return

    print("\n2. Connecting to target database 'dedektiflik_rpg'...")
    pg_conn = psycopg2.connect(
        host=pg_host,
        port=pg_port,
        user=pg_user,
        password=pg_pass,
        dbname=target_db
    )
    pg_cur = pg_conn.cursor()

    # Create Tables
    print("3. Creating schema and indexes...")
    pg_cur.execute("""
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

    CREATE TABLE IF NOT EXISTS "NPCs" (
        "NPCId" INTEGER PRIMARY KEY,
        "Name" VARCHAR(100) NOT NULL,
        "Role" TEXT NOT NULL,
        "TrustLevel" INTEGER DEFAULT 50,
        "FearLevel" INTEGER DEFAULT 0,
        "IsGuilty" BOOLEAN DEFAULT FALSE,
        "SecretInfo" TEXT NOT NULL
    );

    CREATE TABLE IF NOT EXISTS "GolgeSehirNPCs" (
        "NPCId" INTEGER PRIMARY KEY,
        "Name" VARCHAR(100) NOT NULL,
        "Role" TEXT NOT NULL,
        "TrustLevel" INTEGER DEFAULT 50,
        "FearLevel" INTEGER DEFAULT 0,
        "IsGuilty" BOOLEAN DEFAULT FALSE,
        "SecretInfo" TEXT NOT NULL
    );
    """)
    pg_conn.commit()

    # Read from SQLite and copy to PostgreSQL
    sqlite_db = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "Data", "dedektiflik.db"))
    print(f"4. Reading dialogues from SQLite: {sqlite_db}...")
    sq_conn = sqlite3.connect(sqlite_db)
    sq_cur = sq_conn.cursor()

    # Migrate NPCs
    sq_cur.execute("SELECT NPCId, Name, Role, TrustLevel, FearLevel, IsGuilty, SecretInfo FROM NPCs")
    raw_npcs = sq_cur.fetchall()
    npcs = [(r[0], r[1], r[2], r[3], r[4], bool(r[5]), r[6]) for r in raw_npcs]
    if npcs:
        pg_cur.execute("DELETE FROM \"NPCs\";")
        execute_batch(pg_cur, """
        INSERT INTO "NPCs" ("NPCId", "Name", "Role", "TrustLevel", "FearLevel", "IsGuilty", "SecretInfo")
        VALUES (%s, %s, %s, %s, %s, %s, %s)
        ON CONFLICT ("NPCId") DO UPDATE SET "Name"=EXCLUDED."Name", "Role"=EXCLUDED."Role";
        """, npcs)
        pg_conn.commit()
        print(f"Migrated {len(npcs)} Gizemli Kasaba NPCs.")

    # Migrate NPCDialogues (780,000 records)
    pg_cur.execute("SELECT count(*) FROM \"NPCDialogues\";")
    current_pg_count = pg_cur.fetchone()[0]
    print(f"Current dialogues in PostgreSQL: {current_pg_count}")

    if current_pg_count < 100000:
        print("Transferring 780,000+ AI dialogues to PostgreSQL in high-speed batches...")
        sq_cur.execute("SELECT NPCId, Category, PlayerText, NPCResponse, Difficulty, GuiltyResponses FROM NPCDialogues")
        
        batch = []
        total_migrated = 0
        while True:
            rows = sq_cur.fetchmany(10000)
            if not rows:
                break
            execute_batch(pg_cur, """
            INSERT INTO "NPCDialogues" ("NPCId", "Category", "PlayerText", "NPCResponse", "Difficulty", "GuiltyResponses")
            VALUES (%s, %s, %s, %s, %s, %s);
            """, rows)
            pg_conn.commit()
            total_migrated += len(rows)
            print(f"  -> Migrated {total_migrated} dialogues into PostgreSQL...")

    pg_cur.execute("SELECT count(*) FROM \"NPCDialogues\";")
    final_count = pg_cur.fetchone()[0]
    print(f"\nALL DONE! PostgreSQL 'dedektiflik_rpg' has {final_count} NPCDialogues records!")

    sq_conn.close()
    pg_cur.close()
    pg_conn.close()

if __name__ == "__main__":
    migrate_to_postgres()
