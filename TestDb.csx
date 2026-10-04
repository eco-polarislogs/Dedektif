#r "nuget: Npgsql, 8.0.2"
using System;
using Npgsql;

try {
    using var conn = new NpgsqlConnection("Host=localhost;Port=5432;Database=dedektiflik_rpg_v2;Username=postgres;Password=EcReN12854700456.");
    conn.Open();
    using var cmd = new NpgsqlCommand("SELECT column_name, data_type FROM information_schema.columns WHERE table_name = 'sisorennpcs'", conn);
    using var reader = cmd.ExecuteReader();
    Console.WriteLine("--- TABLE SCHEMA ---");
    while (reader.Read())
    {
        Console.WriteLine($"{reader[0]} - {reader[1]}");
    }
}
catch (Exception e) {
    Console.WriteLine(e);
}
