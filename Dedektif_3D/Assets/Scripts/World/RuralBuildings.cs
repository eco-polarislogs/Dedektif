using Dedektif3D.Core;
using UnityEngine;
using static Dedektif3D.World.Shapes;
using M = Dedektif3D.World.MaterialLibrary;

namespace Dedektif3D.World
{
    /// <summary>
    /// Gölgeşehir (golge_sehir_map.png) ve Sisören (sisoren_map_v5.jpg) binaları: kütük kulübeler, ahşap dükkânlar,
    /// taş binalar; dik beşik çatı, baca dumanı, ahşap tabela + fener; binaya özgü dış mekân detayları
    /// (demirci ocağı, manav tezgâhı, telgraf anteni, sinema ışıklı tabelası, ahır hayvanları, tüp yığını, hurda...).
    /// Ön cephe yerel -Z yönüne bakar.
    /// </summary>
    public static class RuralBuildings
    {
        enum Wall { Log, Plank, Stone, Plaster, Concrete }

        static Material Iron => M.Get("iron", new Color(0.08f, 0.08f, 0.09f), 0.5f, 0.7f);

        public static Vector3 SizeFor(string key)
        {
            switch (Base(key))
            {
                case "muhtar": case "muhtarlik": return new Vector3(10f, 7f, 8f);
                case "sinema": return new Vector3(10f, 6.2f, 9f);
                case "ahir": return new Vector3(9f, 5f, 7f);
                case "demirci": return new Vector3(7f, 4.4f, 7f);
                case "hurdaci": return new Vector3(6f, 3.8f, 5.5f);
                case "oduncu": return new Vector3(8f, 4.6f, 7f);
                default: return new Vector3(7f, 4.6f, 6.5f);
            }
        }

        static string Base(string key) => key != null && key.StartsWith("kasabali_evi") ? "kasabali_evi" : key;

        public static BuildingGenerator.BuildingInstance Build(BuildingDef def, Transform parent, Vector3 pos, float yaw)
        {
            var t = Group("Bina_" + def.DisplayName, parent, pos, yaw).transform;
            var s = def.Size = SizeFor(def.Key);
            float fz = -s.z / 2f;
            string key = Base(def.Key);
            Wall wall;
            switch (key)
            {
                case "oduncu": case "kasabali_evi": case "ahir": wall = key == "kasabali_evi" && def.NpcId % 2 == 0 ? Wall.Plank : Wall.Log; break;
                case "demirci": case "muhtar": case "muhtarlik": case "sinema": wall = Wall.Stone; break;
                case "hekim": wall = Wall.Plaster; break;
                case "tupcu": wall = Wall.Plank; break;   // gazyağcı: ahşap depo
                default: wall = Wall.Plank; break;
            }

            if (key == "ahir") BuildBarn(t, s);
            else
            {
                Box("TaşTemel", t, new Vector3(0, 0.2f, 0), new Vector3(s.x + 0.3f, 0.4f, s.z + 0.3f), M.Get("plinth", new Color(0.32f, 0.31f, 0.3f), 0.35f, 0f, M.NoiseTexture));
                Walls(t, s, wall);
                bool twoStorey = s.y > 6f;
                if (twoStorey)
                {
                    Box("KatBandı", t, new Vector3(0, 3.5f, fz - 0.06f), new Vector3(s.x + 0.1f, 0.2f, 0.14f), M.Stone, collider: false);
                    for (int i = 0; i < 3; i++) Window(t, fz, -s.x / 2 + 1.7f + i * (s.x - 3.4f) / 2f, 4.9f, i != 1 || def.NpcId % 3 != 0);
                }
                // Dik beşik çatı (ahşap padavra)
                var roofCol = wall == Wall.Stone ? new Color(0.24f, 0.24f, 0.26f) : new Color(0.22f, 0.19f, 0.16f);
                Box("Saçak", t, new Vector3(0, s.y + 0.35f, 0), new Vector3(s.x + 1.0f, 0.12f, s.z + 1.0f), M.Wood(new Color(0.16f, 0.11f, 0.07f)), collider: false);
                Prism("Çatı", t, new Vector3(0, s.y + 0.4f, 0), new Vector3(s.z + 1.0f, s.y * 0.55f, s.x + 1.0f), M.RoofTiles(roofCol), 90f, 0.9f);
                if (key == "muhtar" || key == "muhtarlik")
                    foreach (var x in new[] { -2.5f, 2.5f })
                    {
                        // Çatı penceresi (dormer) — golge_sehir_map.png muhtarlık
                        Box("ÇatıPenceresiGövde", t, new Vector3(x, s.y + 1.4f, fz + 0.9f), new Vector3(1.2f, 1.2f, 1.2f), M.Stone, collider: false);
                        Prism("ÇatıPenceresiÇatı", t, new Vector3(x, s.y + 2.0f, fz + 0.9f), new Vector3(1.5f, 0.7f, 1.5f), M.RoofTiles(roofCol), 0f);
                        Box("ÇatıPenceresiCam", t, new Vector3(x, s.y + 1.4f, fz + 0.28f), new Vector3(0.6f, 0.7f, 0.04f), M.WarmWindow, collider: false);
                    }
                Box("Baca", t, new Vector3(s.x * 0.28f, s.y + s.y * 0.4f, s.z * 0.15f), new Vector3(0.6f, 2f, 0.6f), M.Get("stonewall_house", new Color(0.4f, 0.37f, 0.33f), 0.2f, 0f, M.BrickTexture), collider: false);
                Effects.ChimneySmoke(t, new Vector3(s.x * 0.28f, s.y + s.y * 0.4f + 1.1f, s.z * 0.15f));

                // Ön cephe: kapı + pencereler + sundurma + tabela
                Box("Kapı", t, new Vector3(0, 1.45f, fz - 0.06f), new Vector3(1.2f, 2.2f, 0.1f), M.Wood(new Color(0.24f, 0.15f, 0.09f)), collider: false);
                Box("KapıPervazı", t, new Vector3(0, 1.5f, fz - 0.03f), new Vector3(1.45f, 2.45f, 0.06f), M.Wood(new Color(0.14f, 0.09f, 0.06f)), collider: false);
                Window(t, fz, -s.x / 2 + 1.5f, 2.0f, true);
                Window(t, fz, s.x / 2 - 1.5f, 2.0f, def.NpcId % 2 == 1);
                Box("Basamak", t, new Vector3(0, 0.2f, fz - 0.6f), new Vector3(2.2f, 0.4f, 0.9f), M.Stone);
                Box("SundurmaÇatı", t, new Vector3(0, 3.0f, fz - 0.9f), new Vector3(2.8f, 0.1f, 1.8f), M.Wood(new Color(0.2f, 0.13f, 0.08f)), new Vector3(-10, 0, 0), false);
                foreach (var x in new[] { -1.25f, 1.25f })
                    Box("SundurmaDirek", t, new Vector3(x, 1.5f, fz - 1.65f), new Vector3(0.12f, 3f, 0.12f), M.Wood(new Color(0.2f, 0.13f, 0.08f)));
            }

            if (key == "sinema") Marquee(t, s, def);
            else if (!string.IsNullOrEmpty(def.DisplayName)) Sign(t, new Vector3(0, key == "ahir" ? 5.6f : 3.55f, fz - (key == "ahir" ? 0.1f : 0.2f)), def.DisplayName, s.x);

            Extras(t, key, s, def);

            var inst = new BuildingGenerator.BuildingInstance { Def = def, Root = t };
            inst.DoorOutside = Group("DoorOutside", t, new Vector3(0, 0, fz - 2.6f), 180f).transform;
            inst.DoorWorldPos = t.TransformPoint(new Vector3(0, 1f, fz - 1.2f));
            if (def.Enterable) inst.StatusLabel = Text3D("DurumYazısı", t, new Vector3(0, 3.0f, fz - 1.4f), "", new Color(1f, 0.85f, 0.4f), 0.03f);
            var glow = PointLight("KapıIşığı", t, new Vector3(0, 2.6f, fz - 1.8f), def.Neon, 2.2f, 7f);
            t.gameObject.AddComponent<LightFlicker>().Init(glow, 0.12f);
            return inst;
        }

        // ------------------------------------------------------------------ duvarlar
        static void Walls(Transform t, Vector3 s, Wall wall)
        {
            float y0 = 0.4f, h = s.y;
            if (wall == Wall.Log)
            {
                // Kütük ev: üst üste yatay kütükler, köşelerde taşan uçlar
                var log = M.Wood(new Color(0.33f, 0.22f, 0.13f));
                Box("İçDuvar", t, new Vector3(0, y0 + h / 2, 0), new Vector3(s.x - 0.25f, h, s.z - 0.25f), M.Wood(new Color(0.25f, 0.17f, 0.1f)));
                int rows = Mathf.FloorToInt(h / 0.36f);
                for (int i = 0; i < rows; i++)
                {
                    float y = y0 + 0.18f + i * 0.36f;
                    bool alt = i % 2 == 0;
                    Cyl("KütükÖn", t, new Vector3(0, y, -s.z / 2), new Vector3(0.36f, (s.x + (alt ? 0.6f : 0.3f)) / 2f, 0.36f), log, new Vector3(0, 0, 90), false);
                    Cyl("KütükArka", t, new Vector3(0, y, s.z / 2), new Vector3(0.36f, (s.x + (alt ? 0.6f : 0.3f)) / 2f, 0.36f), log, new Vector3(0, 0, 90), false);
                    Cyl("KütükSol", t, new Vector3(-s.x / 2, y + 0.18f, 0), new Vector3(0.36f, (s.z + (alt ? 0.3f : 0.6f)) / 2f, 0.36f), log, new Vector3(90, 0, 0), false);
                    Cyl("KütükSağ", t, new Vector3(s.x / 2, y + 0.18f, 0), new Vector3(0.36f, (s.z + (alt ? 0.3f : 0.6f)) / 2f, 0.36f), log, new Vector3(90, 0, 0), false);
                }
                return;
            }
            Material mat;
            switch (wall)
            {
                case Wall.Stone: mat = M.Get("stonewall_rural", new Color(0.42f, 0.41f, 0.39f), 0.2f, 0f, M.BrickTexture); break;
                case Wall.Plaster: mat = M.Plaster(new Color(0.72f, 0.72f, 0.68f)); break;
                case Wall.Concrete: mat = M.Get("concrete_rural", new Color(0.45f, 0.48f, 0.47f), 0.2f, 0f, M.NoiseTexture); break;
                default: mat = M.Wood(new Color(0.3f, 0.23f, 0.16f)); break;
            }
            Box("Duvar", t, new Vector3(0, y0 + h / 2, 0), new Vector3(s.x, h, s.z), mat, texTiling: 0.55f);
            if (wall == Wall.Plank)
            {
                // Dikey çıtalar (ahşap dükkân cephesi)
                var batten = M.Wood(new Color(0.2f, 0.14f, 0.09f));
                int n = Mathf.RoundToInt(s.x / 0.9f);
                for (int i = 0; i <= n; i++)
                    Box("Çıta", t, new Vector3(-s.x / 2 + i * s.x / n, y0 + h / 2, -s.z / 2 - 0.03f), new Vector3(0.08f, h, 0.05f), batten, collider: false);
            }
        }

        static void Window(Transform t, float fz, float x, float y, bool lit)
        {
            var frame = M.Wood(new Color(0.14f, 0.09f, 0.06f));
            Box("Pencere", t, new Vector3(x, y, fz - 0.07f), new Vector3(0.9f, 1.05f, 0.05f), lit ? M.WarmWindow : M.Glass, collider: false);
            Box("PencereÇerçeve", t, new Vector3(x, y, fz - 0.05f), new Vector3(1.1f, 1.25f, 0.05f), frame, collider: false);
            Box("PencereKayıt", t, new Vector3(x, y, fz - 0.1f), new Vector3(0.05f, 1.05f, 0.03f), frame, collider: false);
            Box("PencereKayıtY", t, new Vector3(x, y, fz - 0.1f), new Vector3(0.9f, 0.05f, 0.03f), frame, collider: false);
            Box("Denizlik", t, new Vector3(x, y - 0.6f, fz - 0.14f), new Vector3(1.2f, 0.07f, 0.22f), frame, collider: false);
        }

        /// <summary>Ahşap tabela + iki fener (haritadaki dükkân tabelaları).</summary>
        static void Sign(Transform t, Vector3 pos, string text, float width)
        {
            var g = Group("Tabela", t, pos).transform;
            float w = Mathf.Min(width - 1f, 1.0f + text.Length * 0.32f);
            Box("Pano", g, Vector3.zero, new Vector3(w, 0.7f, 0.1f), M.Wood(new Color(0.42f, 0.3f, 0.17f)), collider: false);
            Box("Çerçeve", g, new Vector3(0, 0, 0.02f), new Vector3(w + 0.14f, 0.84f, 0.06f), M.Wood(new Color(0.16f, 0.1f, 0.06f)), collider: false);
            Text3D("Yazı", g, new Vector3(0, 0, -0.07f), text, new Color(0.96f, 0.9f, 0.74f), 0.055f);
            foreach (var x in new[] { -w / 2 - 0.35f, w / 2 + 0.35f })
            {
                Box("FenerKolu", g, new Vector3(x, 0.3f, -0.15f), new Vector3(0.04f, 0.04f, 0.3f), Iron, collider: false);
                Box("Fener", g, new Vector3(x, 0.12f, -0.3f), new Vector3(0.18f, 0.26f, 0.18f), M.Emissive("lantern_rural", new Color(1f, 0.7f, 0.35f), 3f), collider: false);
            }
            var l = PointLight("TabelaFeneri", g, new Vector3(0, 0.2f, -0.9f), new Color(1f, 0.72f, 0.42f), 2.2f, 6f);
            g.gameObject.AddComponent<LightFlicker>().Init(l, 0.1f);
        }

        /// <summary>Hayal feneri salonu: 1887 — elle boyanmış tahta tabela, gaz fenerleri, el boyaması gösteri afişleri.</summary>
        static void Marquee(Transform t, Vector3 s, BuildingDef def)
        {
            float fz = -s.z / 2f;
            var g = Group("HayalFeneriTabelası", t, new Vector3(0, 4.1f, fz - 0.35f)).transform;
            Box("Pano", g, Vector3.zero, new Vector3(5f, 1.3f, 0.3f), M.Wood(new Color(0.34f, 0.2f, 0.11f)), collider: false);
            Box("PanoÇerçeve", g, new Vector3(0, 0, 0.02f), new Vector3(5.2f, 1.5f, 0.24f), M.Get("gilt", new Color(0.72f, 0.55f, 0.22f), 0.75f, 0.85f), collider: false);
            Text3D("Yazı", g, new Vector3(0, 0, -0.17f), def.DisplayName, new Color(0.96f, 0.84f, 0.5f), 0.075f);
            var glass = M.Emissive("gaslamp_glass", new Color(1f, 0.62f, 0.26f), 2.6f);
            foreach (var x in new[] { -2.8f, 2.8f })
            {
                Box("FenerKolu", g, new Vector3(x, 0.55f, -0.22f), new Vector3(0.05f, 0.05f, 0.5f), M.Get("iron", new Color(0.08f, 0.08f, 0.09f), 0.5f, 0.7f), collider: false);
                Box("Fener", g, new Vector3(x, 0.3f, -0.44f), new Vector3(0.2f, 0.34f, 0.2f), glass, collider: false);
            }
            var l = PointLight("TabelaFeneriIşığı", g, new Vector3(0, -0.5f, -1.5f), new Color(1f, 0.74f, 0.42f), 4.2f, 10f);
            g.gameObject.AddComponent<LightFlicker>().Init(l, 0.12f);
            Box("Saçak", t, new Vector3(0, 3.3f, fz - 1.1f), new Vector3(6f, 0.2f, 2.2f), M.Wood(new Color(0.24f, 0.15f, 0.09f)), collider: false);
            Box("AfişSol", t, new Vector3(-3.3f, 2.0f, fz - 0.06f), new Vector3(1.0f, 1.5f, 0.05f), M.Get("poster", new Color(0.62f, 0.24f, 0.16f), 0.05f), collider: false);
            Box("AfişSağ", t, new Vector3(3.3f, 2.0f, fz - 0.06f), new Vector3(1.0f, 1.5f, 0.05f), M.Get("poster2", new Color(0.22f, 0.3f, 0.48f), 0.05f), collider: false);
            Box("GişeCamı", t, new Vector3(0, 1.5f, fz - 0.06f), new Vector3(2.2f, 2.2f, 0.05f), M.WarmWindow, collider: false);
        }

        static void BuildBarn(Transform t, Vector3 s)
        {
            var plank = M.Wood(new Color(0.3f, 0.2f, 0.13f));
            Box("Gövde", t, new Vector3(0, s.y / 2, 0), s, plank, texTiling: 0.4f);
            Box("Saçak", t, new Vector3(0, s.y - 0.05f, 0), new Vector3(s.x + 0.8f, 0.12f, s.z + 0.8f), M.Wood(new Color(0.18f, 0.12f, 0.08f)), collider: false);
            Prism("Çatı", t, new Vector3(0, s.y, 0), new Vector3(s.z + 0.8f, 3f, s.x + 0.8f), M.RoofTiles(new Color(0.22f, 0.2f, 0.18f)), 90f, 0.6f);
            Box("AhırKapısı", t, new Vector3(-1.5f, 1.7f, -s.z / 2 - 0.04f), new Vector3(2.8f, 3.4f, 0.08f), M.Get("barn_dark", new Color(0.08f, 0.06f, 0.05f)), collider: false);
            Box("Çapraz1", t, new Vector3(2.4f, 1.7f, -s.z / 2 - 0.06f), new Vector3(0.12f, 3.6f, 0.04f), M.Wood(new Color(0.5f, 0.45f, 0.36f)), new Vector3(0, 0, 35), false);
            Box("Çapraz2", t, new Vector3(2.4f, 1.7f, -s.z / 2 - 0.06f), new Vector3(0.12f, 3.6f, 0.04f), M.Wood(new Color(0.5f, 0.45f, 0.36f)), new Vector3(0, 0, -35), false);
            Box("SamanKapağı", t, new Vector3(0, 4.3f, -s.z / 2 - 0.04f), new Vector3(1.2f, 0.9f, 0.06f), M.WarmWindow, collider: false);
        }

        // ------------------------------------------------------------------ binaya özgü detaylar
        static void Extras(Transform t, string key, Vector3 s, BuildingDef def)
        {
            float fz = -s.z / 2f;
            switch (key)
            {
                case "oduncu":
                    for (int i = 0; i < 9; i++)
                        Cyl("OdunYığını", t, new Vector3(s.x / 2 + 1.4f, 0.25f + (i / 3) * 0.45f, fz + 1f + (i % 3) * 0.5f), new Vector3(0.45f, 1.1f, 0.45f), M.Wood(new Color(0.42f, 0.3f, 0.18f)), new Vector3(0, 0, 90));
                    Cyl("Kütük", t, new Vector3(-s.x / 2 - 1.2f, 0.4f, fz - 1.2f), new Vector3(0.8f, 0.4f, 0.8f), M.Wood(new Color(0.4f, 0.28f, 0.16f)));
                    Box("BaltaSapı", t, new Vector3(-s.x / 2 - 1.2f, 1.1f, fz - 1.2f), new Vector3(0.06f, 0.9f, 0.06f), M.Wood(new Color(0.5f, 0.35f, 0.2f)), new Vector3(0, 0, 20), false);
                    Box("BaltaAğzı", t, new Vector3(-s.x / 2 - 1.36f, 0.75f, fz - 1.2f), new Vector3(0.28f, 0.2f, 0.04f), M.Get("steel", new Color(0.55f, 0.55f, 0.58f), 0.75f, 0.9f), new Vector3(0, 0, 20), false);
                    break;
                case "manav":
                    MarketStall(t, new Vector3(-s.x / 2 - 2.2f, 0, fz - 1.8f), new Color(0.15f, 0.4f, 0.25f));
                    break;
                case "demirci":
                {
                    // Açık demirci ocağı: parlayan kor, örs, kıvılcımlar (golge_sehir_map.png)
                    var forge = Group("DemirciOcağı", t, new Vector3(0, 0, fz - 1.8f)).transform;
                    Box("Ocak", forge, new Vector3(-1.6f, 0.6f, 0), new Vector3(1.4f, 1.2f, 1.2f), M.Get("stonewall_rural", new Color(0.42f, 0.41f, 0.39f), 0.2f, 0f, M.BrickTexture));
                    Box("Kor", forge, new Vector3(-1.6f, 1.22f, 0), new Vector3(1.0f, 0.08f, 0.8f), M.Emissive("embers", new Color(1f, 0.38f, 0.08f), 6f), collider: false);
                    var fire = PointLight("OcakAteşi", forge, new Vector3(-1.6f, 1.6f, -0.3f), new Color(1f, 0.45f, 0.15f), 7f, 10f);
                    forge.gameObject.AddComponent<LightFlicker>().Init(fire, 0.35f);
                    Box("Örs", forge, new Vector3(0.6f, 0.75f, 0.2f), new Vector3(0.8f, 0.25f, 0.35f), Iron);
                    Cyl("ÖrsAyağı", forge, new Vector3(0.6f, 0.32f, 0.2f), new Vector3(0.5f, 0.32f, 0.5f), M.Wood(new Color(0.35f, 0.22f, 0.12f)));
                    Sparks(forge, new Vector3(-1.6f, 1.3f, 0));
                    Box("SundurmaÇatı2", t, new Vector3(0, 2.9f, fz - 1.8f), new Vector3(4.6f, 0.12f, 2.6f), M.Wood(new Color(0.2f, 0.13f, 0.08f)), new Vector3(-12, 0, 0), false);
                    break;
                }
                case "bakkal":
                    for (int i = 0; i < 3; i++) PropFactory.Crate(t, new Vector3(-s.x / 2 + 0.8f + i * 0.9f, 0, fz - 1.3f), 0.6f);
                    PropFactory.Barrel(t, new Vector3(s.x / 2 - 0.6f, 0, fz - 1.3f));
                    break;
                case "hekim":
                    Box("HaçD", t, new Vector3(s.x / 2 - 1.2f, 3.4f, fz - 0.08f), new Vector3(0.22f, 0.7f, 0.05f), M.Emissive("hekim_cross", new Color(0.3f, 1f, 0.55f), 3f), collider: false);
                    Box("HaçY", t, new Vector3(s.x / 2 - 1.2f, 3.4f, fz - 0.08f), new Vector3(0.7f, 0.22f, 0.05f), M.Emissive("hekim_cross", new Color(0.3f, 1f, 0.55f), 3f), collider: false);
                    break;
                case "muhtar":
                case "muhtarlik":
                {
                    var pole = Group("BayrakDireği", t, new Vector3(-s.x / 2 - 0.8f, 0, fz - 1f)).transform;
                    Cyl("Direk", pole, new Vector3(0, 3.5f, 0), new Vector3(0.09f, 3.5f, 0.09f), M.Get("silver", new Color(0.8f, 0.8f, 0.85f), 0.9f, 1f));
                    var flag = Group("Bayrak", pole, new Vector3(0.05f, 6.4f, 0)).transform;
                    flag.gameObject.AddComponent<FlagWave>();
                    Box("Kumaş", flag, new Vector3(0.8f, 0, 0), new Vector3(1.6f, 1.05f, 0.03f), M.Get("flagred", new Color(0.8f, 0.04f, 0.08f), 0.2f), collider: false);
                    Cyl("Hilal", flag, new Vector3(0.55f, 0, -0.025f), new Vector3(0.55f, 0.01f, 0.55f), M.Get("flagwhite", Color.white), new Vector3(90, 0, 0), false);
                    Cyl("HilalKes", flag, new Vector3(0.65f, 0, -0.03f), new Vector3(0.44f, 0.01f, 0.44f), M.Get("flagred", new Color(0.8f, 0.04f, 0.08f), 0.2f), new Vector3(90, 0, 0), false);
                    Box("Yıldız", flag, new Vector3(1.0f, 0, -0.03f), new Vector3(0.16f, 0.16f, 0.01f), M.Get("flagwhite", Color.white), new Vector3(0, 0, 45), false);
                    break;
                }
                case "ayakkabici":
                {
                    var boot = Group("ÇizmeTabelası", t, new Vector3(s.x / 2 + 0.5f, 2.6f, fz + 0.3f)).transform;
                    Box("Kol", t, new Vector3(s.x / 2 + 0.25f, 3.0f, fz + 0.3f), new Vector3(0.5f, 0.05f, 0.05f), Iron, collider: false);
                    Box("Konç", boot, new Vector3(0, 0.1f, 0), new Vector3(0.12f, 0.5f, 0.25f), M.Get("leather", new Color(0.3f, 0.15f, 0.08f), 0.6f), collider: false);
                    Box("Ayak", boot, new Vector3(0, -0.15f, -0.15f), new Vector3(0.12f, 0.18f, 0.45f), M.Get("leather", new Color(0.3f, 0.15f, 0.08f), 0.6f), collider: false);
                    break;
                }
                case "telgrafhane":
                {
                    // Kafes telgraf anteni + elektrik direkleri (sisoren_map.png zirvesi)
                    var tower = Group("TelgrafAnteni", t, new Vector3(s.x / 2 + 2f, 0, s.z / 2 - 1f)).transform;
                    for (int c = 0; c < 4; c++)
                    {
                        var corner = new Vector3((c % 2 - 0.5f) * 1.2f, 0, (c / 2 - 0.5f) * 1.2f);
                        Box("Ayak", tower, corner * 0.5f + Vector3.up * 7f, new Vector3(0.1f, 14f, 0.1f), Iron, new Vector3(-corner.z * 5f, 0, corner.x * 5f), false);
                    }
                    for (int i = 0; i < 7; i++) Box("Kuşak", tower, new Vector3(0, 1.5f + i * 1.9f, 0), new Vector3(1.2f - i * 0.12f, 0.06f, 1.2f - i * 0.12f), Iron, collider: false);
                    Sphere("İkazLambası", tower, new Vector3(0, 14.2f, 0), Vector3.one * 0.3f, M.Emissive("beacon_red", new Color(1f, 0.1f, 0.1f), 4f));
                    PointLight("İkaz", tower, new Vector3(0, 14.2f, 0), new Color(1f, 0.15f, 0.1f), 2f, 8f).gameObject.AddComponent<Blinker>();
                    break;
                }
                case "kahvehane":
                    TownProps.CafeTable(t, new Vector3(-s.x / 2 + 1.2f, 0, fz - 2.2f));
                    TownProps.CafeTable(t, new Vector3(s.x / 2 - 1.2f, 0, fz - 2.2f));
                    Cyl("Semaver", t, new Vector3(1.6f, 0.9f, fz - 0.8f), new Vector3(0.35f, 0.4f, 0.35f), M.Get("brass", new Color(0.75f, 0.6f, 0.25f), 0.8f, 0.9f));
                    Box("SemaverMasası", t, new Vector3(1.6f, 0.25f, fz - 0.8f), new Vector3(0.7f, 0.5f, 0.7f), M.Wood(new Color(0.3f, 0.2f, 0.12f)));
                    Effects.ChimneySmoke(t, new Vector3(1.6f, 1.4f, fz - 0.8f));
                    break;
                case "sinema":
                    for (int i = 0; i < 5; i++) Townsfolk.CreateStanding(t, new Vector3(-2f + i * 1f, 0, fz - 2.4f - (i % 2) * 0.6f), i % 2 == 0).transform.localRotation = Quaternion.Euler(0, 180, 0);
                    break;
                case "sahaf":
                    for (int i = 0; i < 2; i++)
                    {
                        PropFactory.Crate(t, new Vector3(-s.x / 2 + 1f + i, 0, fz - 1.2f), 0.6f);
                        for (int b = 0; b < 6; b++)
                        {
                            var c = Color.HSVToRGB((b * 0.17f + i * 0.3f) % 1f, 0.5f, 0.4f);
                            Box("Kitap", t, new Vector3(-s.x / 2 + 0.8f + i + b * 0.07f, 0.72f, fz - 1.2f), new Vector3(0.06f, 0.25f, 0.18f), M.Get("book_" + ColorUtility.ToHtmlStringRGB(c), c), collider: false);
                        }
                    }
                    break;
                case "tutuncu":
                    // Saçak altında kuruyan tütün yaprakları
                    for (int r = 0; r < 2; r++)
                        for (int i = 0; i < 10; i++)
                            Box("TütünYaprağı", t, new Vector3(-2.2f + i * 0.5f, 2.4f - r * 0.5f, fz - 1.4f - r * 0.2f), new Vector3(0.18f, 0.45f, 0.02f), M.Get("tobacco", new Color(0.55f, 0.38f, 0.12f), 0.2f), new Vector3(0, 0, Random.Range(-10f, 10f)), false);
                    break;
                case "ahir":
                    Animal(t, new Vector3(-3.5f, 0, fz - 2.5f), 20f, new Color(0.35f, 0.22f, 0.12f), true);
                    Animal(t, new Vector3(-1f, 0, fz - 3.2f), -30f, new Color(0.85f, 0.82f, 0.78f), false);
                    Animal(t, new Vector3(1.6f, 0, fz - 2.6f), 70f, new Color(0.2f, 0.15f, 0.1f), false);
                    PropFactory.FencePost(t, new Vector3(-1f, 0, fz - 4.8f), 0, 9f);
                    for (int i = 0; i < 3; i++) Cyl("Saman", t, new Vector3(s.x / 2 + 1.2f, 0.55f, fz + 1f + i * 1.3f), new Vector3(1.1f, 0.6f, 1.1f), M.Get("hay", new Color(0.62f, 0.5f, 0.25f)), new Vector3(0, 0, 90));
                    break;
                case "tupcu":
                    // Gazyağı varilleri ve teneke yığını
                    for (int i = 0; i < 8; i++)
                        PropFactory.Barrel(t, new Vector3(-s.x / 2 - 0.8f + (i % 4) * 0.72f, 0, fz - 0.9f - (i / 4) * 0.72f));
                    for (int i = 0; i < 6; i++)
                        Box("Teneke", t, new Vector3(s.x / 2 + 0.7f + (i % 3) * 0.34f, 0.17f + (i / 3) * 0.34f, fz - 0.9f), new Vector3(0.3f, 0.34f, 0.3f), M.Get("tin_can", new Color(0.42f, 0.4f, 0.34f), 0.5f, 0.6f), new Vector3(0, i * 13f, 0), false);
                    break;
                case "hurdaci":
                    // Eski araba tekerlekleri (lastik 1887'de yok): yığılmış ahşap tekerlek ve demir çember
                    for (int i = 0; i < 12; i++)
                        Cyl("EskiTekerlek", t, new Vector3(s.x / 2 + 1f + Random.Range(-1f, 1.5f), 0.1f + (i % 4) * 0.16f, fz + Random.Range(-1f, 2f)), new Vector3(0.95f, 0.05f, 0.95f), i % 2 == 0 ? M.Wood(new Color(0.3f, 0.2f, 0.12f)) : M.Get("rust", new Color(0.4f, 0.22f, 0.12f), 0.3f, 0.5f), new Vector3(Random.Range(-14, 14), 0, Random.Range(-14, 14)), false);
                    for (int i = 0; i < 6; i++)
                        Box("Hurda", t, new Vector3(-s.x / 2 - 1.2f + Random.Range(-0.8f, 0.8f), 0.3f + i * 0.15f, fz + Random.Range(-1f, 2f)), new Vector3(Random.Range(0.4f, 1.1f), 0.3f, Random.Range(0.4f, 0.9f)), M.Get("rust", new Color(0.4f, 0.22f, 0.12f), 0.3f, 0.5f), new Vector3(0, Random.Range(0, 90), Random.Range(-25, 25)));
                    Cart(t, new Vector3(-s.x / 2 - 2.5f, 0, fz - 1.5f), 30f);
                    break;
                case "kasabali_evi":
                    PropFactory.FencePost(t, new Vector3(0, 0, fz - 3.2f), 0, s.x + 2f);
                    Box("Çamaşırİpi", t, new Vector3(s.x / 2 + 1.5f, 1.8f, fz), new Vector3(0.02f, 0.02f, 3f), Iron, collider: false);
                    for (int i = 0; i < 3; i++) Box("Çamaşır", t, new Vector3(s.x / 2 + 1.5f, 1.5f, fz - 1f + i), new Vector3(0.02f, 0.6f, 0.5f), M.Get("laundry" + i, Color.HSVToRGB(i * 0.3f, 0.3f, 0.7f)), collider: false);
                    break;
            }
        }

        static void MarketStall(Transform parent, Vector3 pos, Color awning)
        {
            var g = Group("ManavTezgahı", parent, pos).transform;
            var wood = M.Wood(new Color(0.38f, 0.26f, 0.15f));
            Box("Tezgah", g, new Vector3(0, 0.8f, 0), new Vector3(2.6f, 0.1f, 1.2f), wood);
            foreach (var x in new[] { -1.2f, 1.2f })
                foreach (var z in new[] { -0.5f, 0.5f })
                    Box("Direk", g, new Vector3(x, 1.1f, z), new Vector3(0.08f, 2.2f, 0.08f), wood, collider: false);
            Box("Tente", g, new Vector3(0, 2.25f, 0), new Vector3(2.9f, 0.05f, 1.6f), M.Get("stall_awning", awning, 0.3f), new Vector3(-12, 0, 0), false);
            Color[] produce = { new Color(0.75f, 0.12f, 0.08f), new Color(0.9f, 0.55f, 0.1f), new Color(0.3f, 0.55f, 0.15f), new Color(0.55f, 0.15f, 0.4f) };
            for (int i = 0; i < 4; i++)
            {
                Box("Kasa", g, new Vector3(-0.95f + i * 0.63f, 0.95f, 0), new Vector3(0.55f, 0.2f, 0.8f), M.Wood(new Color(0.5f, 0.36f, 0.2f)), collider: false);
                for (int k = 0; k < 6; k++)
                    Sphere("Ürün", g, new Vector3(-1.1f + i * 0.63f + (k % 3) * 0.14f, 1.1f, -0.2f + (k / 3) * 0.3f), Vector3.one * 0.14f, M.Get("produce" + i, produce[i], 0.5f));
            }
            PointLight("TezgahFeneri", g, new Vector3(0, 2f, -0.8f), new Color(1f, 0.72f, 0.42f), 1.8f, 5f);
            Cart(parent, pos + new Vector3(3.6f, 0, -0.5f), -20f);
        }

        /// <summary>At arabası (haritalardaki atlı arabalar).</summary>
        public static void Cart(Transform parent, Vector3 pos, float yaw)
        {
            var g = Group("AtArabası", parent, pos, yaw).transform;
            var wood = M.Wood(new Color(0.36f, 0.25f, 0.15f));
            Box("Kasa", g, new Vector3(0, 0.9f, 0), new Vector3(1.4f, 0.5f, 2.4f), wood);
            foreach (var x in new[] { -0.8f, 0.8f })
                Cyl("Tekerlek", g, new Vector3(x, 0.5f, -0.4f), new Vector3(1f, 0.05f, 1f), wood, new Vector3(0, 0, 90), false);
            Box("Ok1", g, new Vector3(-0.35f, 0.75f, 1.9f), new Vector3(0.06f, 0.06f, 1.6f), wood, collider: false);
            Box("Ok2", g, new Vector3(0.35f, 0.75f, 1.9f), new Vector3(0.06f, 0.06f, 1.6f), wood, collider: false);
            Animal(g, new Vector3(0, 0, 3.2f), 0f, new Color(0.3f, 0.2f, 0.12f), true);
        }

        /// <summary>Basit dört ayaklı hayvan (at / inek).</summary>
        public static void Animal(Transform parent, Vector3 pos, float yaw, Color color, bool horse)
        {
            var g = Group(horse ? "At" : "İnek", parent, pos, yaw).transform;
            var mat = M.Get("animal_" + ColorUtility.ToHtmlStringRGB(color), color, 0.3f);
            if (horse) { BuildHorse(g, mat); return; }
            Capsule("Gövde", g, new Vector3(0, 1.15f, 0), new Vector3(0.7f, horse ? 0.85f : 0.8f, 0.7f), mat).transform.localRotation = Quaternion.Euler(90, 0, 0);
            foreach (var x in new[] { -0.22f, 0.22f })
                foreach (var z in new[] { -0.55f, 0.55f })
                    Cyl("Bacak", g, new Vector3(x, 0.4f, z), new Vector3(0.14f, 0.4f, 0.14f), mat, collider: false);
            var head = Group("Baş", g, new Vector3(0, horse ? 1.7f : 1.35f, horse ? 0.95f : 1.0f), 0).transform;
            Box("Boyun", head, new Vector3(0, -0.15f, -0.15f), new Vector3(0.25f, 0.5f, 0.3f), mat, horse ? new Vector3(-35, 0, 0) : Vector3.zero, false);
            Box("Kafa", head, new Vector3(0, 0.05f, 0.15f), new Vector3(0.26f, 0.28f, horse ? 0.6f : 0.42f), mat, new Vector3(horse ? 25 : 10, 0, 0), false);
            if (!horse) { Box("Boynuz", head, new Vector3(0, 0.22f, 0.05f), new Vector3(0.5f, 0.05f, 0.05f), M.Get("horn", new Color(0.85f, 0.8f, 0.7f)), collider: false); }
            else Box("Yele", head, new Vector3(0, 0.1f, -0.25f), new Vector3(0.08f, 0.5f, 0.35f), M.Get("mane", new Color(0.08f, 0.06f, 0.05f)), new Vector3(-35, 0, 0), false);
            g.gameObject.AddComponent<AnimalIdle>().head = head;
        }

        /// <summary>At: gövde, göğüs ve sağrı ayrı kütleler; eklemli bacaklar, toynak, yele, kuyruk ve koşum takımı (siluet at gibi okunsun).</summary>
        static void BuildHorse(Transform g, Material coat)
        {
            var hair = M.Get("mane", new Color(0.05f, 0.04f, 0.03f), 0.25f);
            var hoof = M.Get("hoof", new Color(0.04f, 0.04f, 0.04f), 0.5f);
            var leather = M.Get("harness", new Color(0.08f, 0.05f, 0.03f), 0.55f);

            Sphere("Gövde", g, new Vector3(0, 1.14f, 0), new Vector3(0.5f, 0.58f, 1.25f), coat);
            Sphere("Göğüs", g, new Vector3(0, 1.17f, 0.5f), new Vector3(0.5f, 0.62f, 0.62f), coat);
            Sphere("Sağrı", g, new Vector3(0, 1.2f, -0.52f), new Vector3(0.54f, 0.64f, 0.66f), coat);

            foreach (var x in new[] { -0.17f, 0.17f })
                foreach (var z in new[] { 0.52f, -0.55f })
                {
                    bool hind = z < 0f;
                    Cyl("Uyluk", g, new Vector3(x, 0.72f, z - (hind ? 0.03f : 0f)), new Vector3(hind ? 0.17f : 0.14f, 0.22f, hind ? 0.2f : 0.15f), coat, collider: false);
                    Cyl("Baldır", g, new Vector3(x, 0.3f, z + (hind ? -0.04f : 0.01f)), new Vector3(0.075f, 0.24f, 0.075f), coat, collider: false);
                    Cyl("Toynak", g, new Vector3(x, 0.035f, z + (hind ? -0.03f : 0.02f)), new Vector3(0.11f, 0.035f, 0.12f), hoof, collider: false);
                }

            Capsule("Boyun", g, new Vector3(0, 1.56f, 0.76f), new Vector3(0.24f, 0.42f, 0.36f), coat).transform.localRotation = Quaternion.Euler(36, 0, 0);
            Box("Yele", g, new Vector3(0, 1.66f, 0.62f), new Vector3(0.05f, 0.62f, 0.13f), hair, new Vector3(36, 0, 0), false);

            var head = Group("Baş", g, new Vector3(0, 1.9f, 1.02f), 0).transform;
            Sphere("Kafa", head, new Vector3(0, 0, 0), new Vector3(0.2f, 0.25f, 0.36f), coat);
            Sphere("Burun", head, new Vector3(0, -0.11f, 0.24f), new Vector3(0.15f, 0.16f, 0.3f), coat);
            foreach (var x in new[] { -0.07f, 0.07f })
                Cyl("Kulak", head, new Vector3(x, 0.16f, -0.09f), new Vector3(0.03f, 0.065f, 0.03f), coat, collider: false);
            Box("Yular", head, new Vector3(0, -0.08f, 0.17f), new Vector3(0.17f, 0.03f, 0.04f), leather, new Vector3(0, 0, 0), false);
            Box("Perçem", head, new Vector3(0, 0.11f, 0.03f), new Vector3(0.05f, 0.08f, 0.12f), hair, collider: false);

            Capsule("Kuyruk", g, new Vector3(0, 0.92f, -1.08f), new Vector3(0.1f, 0.42f, 0.1f), hair).transform.localRotation = Quaternion.Euler(-14, 0, 0);
            Cyl("Koşum", g, new Vector3(0, 1.26f, 0.26f), new Vector3(0.55f, 0.04f, 0.68f), leather, new Vector3(90, 0, 0), false);

            g.gameObject.AddComponent<AnimalIdle>().head = head;
        }

        static void Sparks(Transform parent, Vector3 pos)
        {
            var go = new GameObject("Kıvılcımlar");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.8f; main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.5f); main.startSize = 0.04f;
            main.startColor = new Color(1f, 0.6f, 0.2f, 1f); main.gravityModifier = 0.6f; main.maxParticles = 80;
            var em = ps.emission; em.rateOverTime = 25;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 30; sh.radius = 0.2f;
            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = M.Emissive("spark", new Color(1f, 0.55f, 0.15f), 6f);
        }

        /// <summary>Kırsal NPC'lere meslek aksesuarı (önlük, şapka, sakal...).</summary>
        public static void Dress(PropFactory.PersonRig rig, string key)
        {
            if (rig.HasModel) return;
            switch (Base(key))
            {
                case "demirci": case "oduncu":
                    Box("Önlük", rig.Body, new Vector3(0, 1.05f, 0.155f), new Vector3(0.42f, 0.7f, 0.02f), M.Get("leather", new Color(0.3f, 0.15f, 0.08f), 0.6f), null, false);
                    if (!rig.HasFace) Box("Sakal", rig.Head, new Vector3(0, -0.12f, 0.12f), new Vector3(0.2f, 0.14f, 0.08f), M.Get("hair", new Color(0.12f, 0.08f, 0.06f)), null, false);
                    break;
                case "hekim":
                    Box("Önlük", rig.Body, new Vector3(0, 1.0f, 0.15f), new Vector3(0.5f, 0.9f, 0.02f), M.Get("labcoat", new Color(0.96f, 0.97f, 0.98f)), null, false);
                    break;
                case "muhtar": case "muhtarlik": case "telgrafhane":
                    if (rig.HasFace) break; // başlığı portre belirler
                    Cyl("Kasket", rig.Head, new Vector3(0, 0.16f, 0.02f), new Vector3(0.34f, 0.05f, 0.36f), M.Get("cap", new Color(0.25f, 0.22f, 0.2f)), null, false);
                    Box("Siper", rig.Head, new Vector3(0, 0.12f, 0.17f), new Vector3(0.22f, 0.02f, 0.1f), M.Get("cap", new Color(0.25f, 0.22f, 0.2f)), null, false);
                    break;
                case "manav": case "bakkal": case "tutuncu": case "kahvehane":
                    Box("Önlük", rig.Body, new Vector3(0, 1.0f, 0.155f), new Vector3(0.42f, 0.6f, 0.02f), M.Get("apron_rural", new Color(0.75f, 0.72f, 0.62f)), null, false);
                    break;
                default:
                    if (rig.HasFace) break; // başlığı portre belirler
                    Cyl("Şapka", rig.Head, new Vector3(0, 0.14f, 0), new Vector3(0.42f, 0.015f, 0.42f), M.Get("fedora", new Color(0.12f, 0.11f, 0.1f), 0.25f), null, false);
                    Cyl("ŞapkaTepe", rig.Head, new Vector3(0, 0.2f, 0), new Vector3(0.27f, 0.07f, 0.27f), M.Get("fedora", new Color(0.12f, 0.11f, 0.1f), 0.25f), null, false);
                    break;
            }
        }
    }

    public class Blinker : MonoBehaviour
    {
        Light _l;
        void Start() => _l = GetComponent<Light>();
        void Update() { if (_l != null) _l.enabled = Mathf.Repeat(Time.time, 1.6f) < 0.5f; }
    }

    public class AnimalIdle : MonoBehaviour
    {
        public Transform head; float _seed;
        void Start() => _seed = Random.value * 10f;
        void Update() { if (head != null) head.localRotation = Quaternion.Euler(Mathf.Sin(Time.time * 0.4f + _seed) * 12f + 8f, Mathf.Sin(Time.time * 0.3f + _seed) * 20f, 0); }
    }
}
