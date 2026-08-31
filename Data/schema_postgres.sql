-- PostgreSQL 100.000+ AI Dataset and Game Schema
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
