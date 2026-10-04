#r "nuget: Npgsql, 8.0.2"
using System;
using Npgsql;

try {
    using var conn = new NpgsqlConnection("Host=localhost;Port=5432;Database=dedektiflik_rpg_v2;Username=postgres;Password=EcReN12854700456.");
    conn.Open();
    using var cmd = new NpgsqlCommand(@"
        ALTER TABLE SisorenNPCs ADD COLUMN IF NOT EXISTS IsGuilty BOOLEAN NOT NULL DEFAULT FALSE;
        ALTER TABLE SisorenNPCs ADD COLUMN IF NOT EXISTS TrustLevel INTEGER NOT NULL DEFAULT 50;
        ALTER TABLE SisorenNPCs ADD COLUMN IF NOT EXISTS StressLevel INTEGER NOT NULL DEFAULT 30;
        ALTER TABLE SisorenNPCs ADD COLUMN IF NOT EXISTS FearLevel INTEGER NOT NULL DEFAULT 30;
    ", conn);
    cmd.ExecuteNonQuery();
    Console.WriteLine("Added columns to PostgreSQL SisorenNPCs table!");
}
catch (Exception e) {
    Console.WriteLine(e);
}
