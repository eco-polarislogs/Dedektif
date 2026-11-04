using Dedektif3D.Core;
using UnityEngine;
using static Dedektif3D.World.Shapes;
using M = Dedektif3D.World.MaterialLibrary;

namespace Dedektif3D.World
{
    /// <summary>
    /// İç mekanları "boş bina" olmaktan çıkarır — wwwroot/images iç mekan görsellerindeki 1800'ler dekoru:
    /// yağmurlu pencere, ahşap kirişler, gaz lambaları, işleyen duvar saati, döküm soba, kilim, toz zerrecikleri
    /// ve her dükkâna özgü eşyalar (telgraf masası, semaver, sinema koltukları, kitap yığınları, saman balyaları...).
    /// Oda yerel koordinatı: X -5..5, Z -4..4, Y 0..4.2. Kapı -Z duvarının ortasında, tezgah Z=1.6'da.
    /// Saklama yerleri (HidingSpots) ön duvarda X=±3.1, yan duvarlarda Z=-1.9 ve zeminde (-2.3,-0.9) / (2.4,-0.2) — buralar boş bırakılır.
    /// </summary>
    public static class InteriorDressing
    {
        const float W = InteriorGenerator.W, D = InteriorGenerator.D, H = InteriorGenerator.H;

        static Material DarkWood => M.Wood(new Color(0.24f, 0.15f, 0.09f));
        static Material MidWood => M.Wood(new Color(0.36f, 0.23f, 0.13f));
        static Material Brass => M.Get("brass", new Color(0.75f, 0.6f, 0.25f), 0.8f, 0.9f);
        static Material Iron => M.Get("castiron", new Color(0.06f, 0.07f, 0.06f), 0.45f, 0.75f);
        static Material Paper => M.Get("paper", new Color(0.9f, 0.86f, 0.74f), 0.05f);
        static Material Flame => M.Emissive("oil_flame", new Color(1f, 0.62f, 0.25f), 4f);

        // =================================================================
        //  ORTAK ATMOSFER (tüm iç mekanlar)
        // =================================================================
        public static void Atmosphere(Transform t, BuildingDef d)
        {
            // Pencere: yağmurlu sokağa bakan, perdeli; camın ışığı gün/gece döngüsüne göre değişir
            switch (d.Style)
            {
                case BuildingStyle.Muhtarlik: Window(t, true, 1.5f); break;
                case BuildingStyle.Karakol: Window(t, true, -1.8f); break;
                case BuildingStyle.Terzi: Window(t, false, 0.4f); break;
                case BuildingStyle.Eczane: Window(t, false, 1.0f); break;
                case BuildingStyle.Kasap: Window(t, false, 1.2f); break;
                default: Window(t, false, -0.1f); break;   // kırsal: yan duvar mobilyaları Z≥1.2'de başlar
            }
            // Tavan kirişleri
            foreach (var z in new[] { -2.4f, 0.4f, 3.0f })
                Box("Kiriş", t, new Vector3(0, H - 0.12f, z), new Vector3(W, 0.22f, 0.26f), DarkWood, collider: false);
            Box("AnaKiriş", t, new Vector3(0, H - 0.3f, 0.4f), new Vector3(0.28f, 0.16f, D), DarkWood, collider: false);
            // Kapının üstünde işleyen duvar saati (oyun saatini gösterir)
            WallClockProp(t, new Vector3(0, 3.62f, -D / 2 + 0.03f));
            // Kapının iki yanında gaz aplikleri
            Sconce(t, new Vector3(-1.45f, 2.3f, -D / 2 + 0.02f));
            Sconce(t, new Vector3(1.45f, 2.3f, -D / 2 + 0.02f));
            // Ön sol köşe: paltoluk (fötr şapka + şemsiye); ön sağ köşe: döküm soba
            CoatStand(t, new Vector3(-4.45f, 0, -3.55f));
            Stove(t, new Vector3(4.3f, 0, -3.4f));
            // Kilim (Muhtarlık'ta zaten var)
            if (d.Style != BuildingStyle.Muhtarlik) Kilim(t, new Vector3(0, 0.003f, -0.7f), new Vector2(3f, 1.8f), d.NpcId);
            DustMotes(t);
        }

        static void Window(Transform t, bool rightWall, float z)
        {
            float x = rightWall ? W / 2 - 0.02f : -W / 2 + 0.02f;
            float yaw = rightWall ? -90f : 90f;
            var g = Group("Pencere", t, new Vector3(x, 0, z), yaw).transform;    // yerel +Z odanın içine bakar
            var glass = M.Emissive("interior_window", new Color(0.3f, 0.36f, 0.45f), 1f);
            Box("Cam", g, new Vector3(0, 2.0f, 0.01f), new Vector3(1.5f, 1.4f, 0.02f), glass, collider: false);
            Box("KasaÜst", g, new Vector3(0, 2.74f, 0.05f), new Vector3(1.7f, 0.1f, 0.1f), DarkWood, collider: false);
            Box("KasaAlt", g, new Vector3(0, 1.26f, 0.09f), new Vector3(1.8f, 0.08f, 0.2f), DarkWood, collider: false);
            Box("KasaSol", g, new Vector3(-0.8f, 2.0f, 0.05f), new Vector3(0.1f, 1.5f, 0.1f), DarkWood, collider: false);
            Box("KasaSağ", g, new Vector3(0.8f, 2.0f, 0.05f), new Vector3(0.1f, 1.5f, 0.1f), DarkWood, collider: false);
            Box("KayıtDikey", g, new Vector3(0, 2.0f, 0.04f), new Vector3(0.05f, 1.4f, 0.05f), DarkWood, collider: false);
            Box("KayıtYatay", g, new Vector3(0, 2.0f, 0.04f), new Vector3(1.5f, 0.05f, 0.05f), DarkWood, collider: false);
            var curtain = M.Get("curtain_wine", new Color(0.3f, 0.06f, 0.06f), 0.15f);
            Box("PerdeSol", g, new Vector3(-1.05f, 1.95f, 0.12f), new Vector3(0.4f, 1.9f, 0.05f), curtain, collider: false);
            Box("PerdeSağ", g, new Vector3(1.05f, 1.95f, 0.12f), new Vector3(0.4f, 1.9f, 0.05f), curtain, collider: false);
            Cyl("Korniş", g, new Vector3(0, 2.95f, 0.14f), new Vector3(0.03f, 1.3f, 0.03f), Brass, new Vector3(0, 0, 90), false);
            // Pencere önünde gaz lambası ve kırmızı fincan (görsellerdeki pencere pervazı)
            OilLamp(t, t.InverseTransformPoint(g.TransformPoint(new Vector3(0.45f, 1.3f, 0.12f))), 0.9f);
        }

        public static void WallClockProp(Transform t, Vector3 pos)
        {
            var g = Group("DuvarSaati", t, pos).transform;
            Cyl("Kasa", g, Vector3.zero, new Vector3(0.62f, 0.04f, 0.62f), DarkWood, new Vector3(90, 0, 0), false);
            Cyl("Kadran", g, new Vector3(0, 0, 0.03f), new Vector3(0.52f, 0.01f, 0.52f), M.Get("clockface", new Color(0.92f, 0.88f, 0.76f), 0.3f), new Vector3(90, 0, 0), false);
            for (int i = 0; i < 12; i++)
            {
                float a = i / 12f * Mathf.PI * 2f;
                Box("Çentik", g, new Vector3(Mathf.Sin(a) * 0.22f, Mathf.Cos(a) * 0.22f, 0.04f), new Vector3(0.015f, i % 3 == 0 ? 0.05f : 0.03f, 0.005f), Iron, new Vector3(0, 0, -i * 30f), false);
            }
            var hp = Group("Akrep", g, new Vector3(0, 0, 0.045f)).transform;
            Box("AkrepÇubuk", hp, new Vector3(0, 0.07f, 0), new Vector3(0.025f, 0.14f, 0.006f), Iron, collider: false);
            var mp = Group("Yelkovan", g, new Vector3(0, 0, 0.052f)).transform;
            Box("YelkovanÇubuk", mp, new Vector3(0, 0.1f, 0), new Vector3(0.015f, 0.2f, 0.006f), Iron, collider: false);
            Sphere("Pim", g, new Vector3(0, 0, 0.058f), Vector3.one * 0.03f, Brass);
            var clock = g.gameObject.AddComponent<WallClock>();
            clock.hourHand = hp; clock.minuteHand = mp;
        }

        static void Sconce(Transform t, Vector3 pos)
        {
            var g = Group("GazApliği", t, pos).transform;
            Box("Taban", g, new Vector3(0, 0, 0.02f), new Vector3(0.12f, 0.2f, 0.04f), Brass, collider: false);
            Box("Kol", g, new Vector3(0, 0, 0.14f), new Vector3(0.03f, 0.03f, 0.22f), Brass, collider: false);
            Cyl("Şişe", g, new Vector3(0, 0.1f, 0.25f), new Vector3(0.1f, 0.1f, 0.1f), M.Emissive("sconce_glass", new Color(1f, 0.7f, 0.35f), 2.2f), collider: false);
            var l = PointLight("AplikIşığı", g, new Vector3(0, 0.15f, 0.35f), new Color(1f, 0.66f, 0.36f), 2.4f, 5.5f);
            l.gameObject.AddComponent<LightFlicker>().Init(l, 0.12f);
        }

        /// <summary>Tavandan zincirle sarkan pirinç gaz lambası.</summary>
        public static void HangingLantern(Transform t, Vector3 ceilingPos, float intensity, bool shadows = false)
        {
            var g = Group("AsılıGazLambası", t, ceilingPos).transform;
            Cyl("Zincir", g, new Vector3(0, -0.35f, 0), new Vector3(0.015f, 0.35f, 0.015f), Iron, collider: false);
            Cyl("Kapak", g, new Vector3(0, -0.72f, 0), new Vector3(0.36f, 0.04f, 0.36f), Brass, collider: false);
            Cyl("Fanus", g, new Vector3(0, -0.88f, 0), new Vector3(0.22f, 0.13f, 0.22f), M.Emissive("lantern_glass_in", new Color(1f, 0.72f, 0.4f), 2.4f), collider: false);
            Cyl("Hazne", g, new Vector3(0, -1.04f, 0), new Vector3(0.18f, 0.04f, 0.18f), Brass, collider: false);
            var l = PointLight("GazLambası", g, new Vector3(0, -1.0f, 0), new Color(1f, 0.68f, 0.38f), intensity, 12f, shadows);
            l.gameObject.AddComponent<LightFlicker>().Init(l, 0.08f);
        }

        /// <summary>Masa üstü gaz lambası (camlı, titreşen alev).</summary>
        public static void OilLamp(Transform t, Vector3 pos, float intensity = 1.2f)
        {
            var g = Group("GazLambası", t, pos).transform;
            Cyl("Gövde", g, new Vector3(0, 0.05f, 0), new Vector3(0.12f, 0.05f, 0.12f), Brass, collider: false);
            Cyl("Şişe", g, new Vector3(0, 0.18f, 0), new Vector3(0.08f, 0.08f, 0.08f), M.Get("lampglass", new Color(0.9f, 0.85f, 0.7f), 0.95f), collider: false);
            Sphere("Alev", g, new Vector3(0, 0.17f, 0), new Vector3(0.03f, 0.06f, 0.03f), Flame);
            var l = PointLight("LambaAlevi", g, new Vector3(0, 0.25f, 0), new Color(1f, 0.64f, 0.32f), intensity, 3.5f);
            l.gameObject.AddComponent<LightFlicker>().Init(l, 0.18f);
        }

        static void CoatStand(Transform t, Vector3 pos)
        {
            var g = Group("Paltoluk", t, pos).transform;
            Cyl("Direk", g, new Vector3(0, 0.95f, 0), new Vector3(0.06f, 0.95f, 0.06f), DarkWood);
            Cyl("Ayak", g, new Vector3(0, 0.03f, 0), new Vector3(0.45f, 0.03f, 0.45f), DarkWood, collider: false);
            for (int i = 0; i < 4; i++)
                Box("Kanca", g, Quaternion.Euler(0, i * 90f, 0) * new Vector3(0, 1.8f, 0.12f), new Vector3(0.03f, 0.03f, 0.18f), DarkWood, new Vector3(0, i * 90f, 0), false);
            Capsule("Palto", g, new Vector3(0.12f, 1.35f, 0.08f), new Vector3(0.32f, 0.45f, 0.2f), M.Get("coat_hung", new Color(0.16f, 0.15f, 0.14f), 0.2f));
            var hat = M.Get("fedora", new Color(0.12f, 0.11f, 0.1f), 0.25f);
            Cyl("ŞapkaKenar", g, new Vector3(0, 1.93f, 0), new Vector3(0.42f, 0.012f, 0.42f), hat, collider: false);
            Cyl("ŞapkaTepe", g, new Vector3(0, 2.0f, 0), new Vector3(0.25f, 0.07f, 0.25f), hat, collider: false);
            Cyl("Şemsiye", g, new Vector3(-0.15f, 0.45f, 0.1f), new Vector3(0.06f, 0.42f, 0.06f), M.Get("umbrella_0", new Color(0.05f, 0.05f, 0.06f), 0.85f), new Vector3(8, 0, 6), false);
        }

        static void Stove(Transform t, Vector3 pos)
        {
            var g = Group("DökümSoba", t, pos).transform;
            Cyl("Gövde", g, new Vector3(0, 0.5f, 0), new Vector3(0.6f, 0.45f, 0.6f), Iron);
            Cyl("Kapak", g, new Vector3(0, 0.98f, 0), new Vector3(0.66f, 0.03f, 0.66f), Iron, collider: false);
            Box("AteşPenceresi", g, new Vector3(-0.2f, 0.45f, 0.18f), new Vector3(0.18f, 0.14f, 0.02f), M.Emissive("embers", new Color(1f, 0.38f, 0.08f), 6f), new Vector3(0, -45, 0), false);
            Cyl("Boru", g, new Vector3(0, 2.6f, 0), new Vector3(0.14f, 1.6f, 0.14f), Iron, collider: false);
            Cyl("Çaydanlık", g, new Vector3(0.1f, 1.08f, 0.05f), new Vector3(0.18f, 0.08f, 0.18f), M.Get("copper", new Color(0.62f, 0.32f, 0.18f), 0.7f, 0.8f), collider: false);
            var l = PointLight("SobaAteşi", g, new Vector3(-0.35f, 0.5f, 0.35f), new Color(1f, 0.42f, 0.14f), 1.6f, 3.8f);
            l.gameObject.AddComponent<LightFlicker>().Init(l, 0.3f);
        }

        public static void Kilim(Transform t, Vector3 pos, Vector2 size, int seed)
        {
            var g = Group("Kilim", t, pos).transform;
            Color[] reds = { new Color(0.42f, 0.09f, 0.07f), new Color(0.32f, 0.1f, 0.12f), new Color(0.38f, 0.16f, 0.06f) };
            var baseCol = reds[Mathf.Abs(seed) % reds.Length];
            Box("Zemin", g, Vector3.zero, new Vector3(size.x, 0.008f, size.y), M.Get("kilim_" + ColorUtility.ToHtmlStringRGB(baseCol), baseCol, 0.05f), collider: false);
            Box("Çerçeve", g, new Vector3(0, -0.001f, 0), new Vector3(size.x + 0.16f, 0.008f, size.y + 0.16f), M.Get("kilim_border", new Color(0.12f, 0.1f, 0.18f), 0.05f), collider: false);
            for (int i = -2; i <= 2; i++)
                Box("Motif", g, new Vector3(i * size.x / 5.5f, 0.005f, 0), new Vector3(0.22f, 0.002f, 0.22f), M.Get("kilim_motif", new Color(0.78f, 0.6f, 0.26f), 0.05f), new Vector3(0, 45, 0), false);
        }

        static void DustMotes(Transform t)
        {
            var go = new GameObject("TozZerrecikleri");
            go.transform.SetParent(t, false);
            go.transform.localPosition = new Vector3(0, 2f, 0);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.loop = true; main.startLifetime = 9f; main.startSpeed = 0.03f; main.startSize = new ParticleSystem.MinMaxCurve(0.012f, 0.03f);
            main.startColor = new Color(1f, 0.88f, 0.7f, 0.35f); main.maxParticles = 160; main.simulationSpace = ParticleSystemSimulationSpace.Local;
            var em = ps.emission; em.rateOverTime = 16f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(W - 1f, 3f, D - 1f);
            var noise = ps.noise; noise.enabled = true; noise.strength = 0.05f; noise.frequency = 0.2f;
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = Effects.TransparentMaterial(new Color(1f, 0.9f, 0.75f, 0.5f), true);
            ps.Play();
        }

        // =================================================================
        //  YARDIMCI MOBİLYALAR
        // =================================================================
        static void Table(Transform t, Vector3 pos, Vector2 size, float yaw = 0f, float h = 0.78f)
        {
            var g = Group("Masa", t, pos, yaw).transform;
            Box("Tabla", g, new Vector3(0, h, 0), new Vector3(size.x, 0.05f, size.y), MidWood);
            foreach (var sx in new[] { -1, 1 }) foreach (var sz in new[] { -1, 1 })
                Box("Ayak", g, new Vector3(sx * (size.x / 2 - 0.06f), h / 2, sz * (size.y / 2 - 0.06f)), new Vector3(0.06f, h, 0.06f), DarkWood, collider: false);
        }

        static void Chair(Transform t, Vector3 pos, float yaw)
        {
            var g = Group("Sandalye", t, pos, yaw).transform;
            Box("Oturak", g, new Vector3(0, 0.45f, 0), new Vector3(0.42f, 0.04f, 0.42f), MidWood, collider: false);
            Box("Arkalık", g, new Vector3(0, 0.75f, -0.19f), new Vector3(0.42f, 0.55f, 0.04f), DarkWood, collider: false);
            foreach (var sx in new[] { -1, 1 }) foreach (var sz in new[] { -1, 1 })
                Box("Ayak", g, new Vector3(sx * 0.18f, 0.22f, sz * 0.18f), new Vector3(0.04f, 0.45f, 0.04f), DarkWood, collider: false);
        }

        /// <summary>Kitap / kavanoz dolu yüksek raf (duvara dayalı, yerel +Z odaya bakar).</summary>
        static void Bookcase(Transform t, Vector3 pos, float yaw, float width, int levels = 5, float height = 2.4f, bool jars = false)
        {
            var g = Group("Kitaplık", t, pos, yaw).transform;
            Box("Gövde", g, new Vector3(0, height / 2, -0.02f), new Vector3(width, height, 0.04f), DarkWood);
            Box("YanL", g, new Vector3(-width / 2, height / 2, 0.18f), new Vector3(0.05f, height, 0.4f), DarkWood, collider: false);
            Box("YanR", g, new Vector3(width / 2, height / 2, 0.18f), new Vector3(0.05f, height, 0.4f), DarkWood, collider: false);
            var col = g.gameObject.AddComponent<BoxCollider>(); col.center = new Vector3(0, height / 2, 0.18f); col.size = new Vector3(width, height, 0.4f);
            for (int i = 0; i < levels; i++)
            {
                float y = 0.15f + i * (height - 0.25f) / (levels - 1);
                Box("Raf", g, new Vector3(0, y, 0.18f), new Vector3(width, 0.04f, 0.4f), MidWood, collider: false);
                if (i == levels - 1) break;
                float x = -width / 2 + 0.08f;
                int n = 0;
                while (x < width / 2 - 0.1f)
                {
                    float w = jars ? 0.16f : 0.05f + ((n * 37 + i * 11) % 5) * 0.015f;
                    float bh = jars ? 0.2f : 0.2f + ((n * 13 + i * 7) % 4) * 0.04f;
                    var c = jars ? Color.HSVToRGB(((n * 0.17f + i * 0.31f) % 1f), 0.45f, 0.45f) : Color.HSVToRGB(((n * 0.13f + i * 0.29f) % 1f), 0.5f, 0.28f);
                    var mat = M.Get((jars ? "jar_" : "book_") + ColorUtility.ToHtmlStringRGB(c), c, jars ? 0.85f : 0.2f);
                    if (jars) Cyl("Kavanoz", g, new Vector3(x + w / 2, y + 0.02f + bh / 2, 0.2f), new Vector3(w * 0.9f, bh / 2, w * 0.9f), mat, collider: false);
                    else if ((n + i) % 9 != 4) Box("Kitap", g, new Vector3(x + w / 2, y + 0.02f + bh / 2, 0.2f), new Vector3(w, bh, 0.26f), mat, collider: false);
                    x += w + (jars ? 0.04f : 0.004f); n++;
                }
            }
        }

        static void Sack(Transform t, Vector3 pos, float s = 1f)
        {
            Capsule("Çuval", t, pos + Vector3.up * 0.32f * s, new Vector3(0.45f, 0.34f, 0.4f) * s, M.Get("burlap", new Color(0.55f, 0.45f, 0.3f), 0.05f, 0f, M.NoiseTexture));
        }

        static void HayBale(Transform t, Vector3 pos, float yaw)
        {
            Box("SamanBalyası", t, pos + Vector3.up * 0.25f, new Vector3(1.1f, 0.5f, 0.55f), M.Get("hay", new Color(0.62f, 0.52f, 0.28f), 0.05f, 0f, M.NoiseTexture), new Vector3(0, yaw, 0));
        }

        public static void Telegraph(Transform t, Vector3 pos)
        {
            var g = Group("TelgrafMakinesi", t, pos).transform;
            Box("Taban", g, new Vector3(0, 0.015f, 0), new Vector3(0.3f, 0.03f, 0.18f), MidWood, collider: false);
            Box("Kol", g, new Vector3(0, 0.05f, 0), new Vector3(0.2f, 0.015f, 0.03f), Brass, new Vector3(0, 0, 6), false);
            Cyl("Topuz", g, new Vector3(0.09f, 0.07f, 0), new Vector3(0.04f, 0.015f, 0.04f), M.Get("blackknob", new Color(0.03f, 0.03f, 0.03f), 0.7f), collider: false);
            Cyl("Bobin", g, new Vector3(-0.08f, 0.06f, 0.05f), new Vector3(0.05f, 0.04f, 0.05f), M.Get("copper", new Color(0.62f, 0.32f, 0.18f), 0.7f, 0.8f), collider: false);
        }

        static void HerbBunches(Transform t, float z, float xFrom, float xTo, Color c, string key)
        {
            var mat = M.Get(key, c, 0.05f);
            for (float x = xFrom; x <= xTo; x += 0.45f)
            {
                Cyl("İp", t, new Vector3(x, H - 0.45f, z), new Vector3(0.01f, 0.2f, 0.01f), Iron, collider: false);
                Capsule("Demet", t, new Vector3(x, H - 0.85f, z), new Vector3(0.18f, 0.24f, 0.14f), mat);
            }
        }

        static void Fireplace(Transform t, bool rightWall, float z)
        {
            float x = rightWall ? W / 2 - 0.35f : -W / 2 + 0.35f;
            var g = Group("Şömine", t, new Vector3(x, 0, z), rightWall ? -90f : 90f).transform;
            var stone = M.Get("stonewall_rural", new Color(0.42f, 0.41f, 0.39f), 0.2f, 0f, M.BrickTexture);
            Box("TaşSol", g, new Vector3(-0.6f, 0.6f, 0), new Vector3(0.3f, 1.2f, 0.6f), stone);
            Box("TaşSağ", g, new Vector3(0.6f, 0.6f, 0), new Vector3(0.3f, 1.2f, 0.6f), stone);
            Box("Lento", g, new Vector3(0, 1.3f, 0), new Vector3(1.6f, 0.25f, 0.65f), stone);
            Box("Baca", g, new Vector3(0, 2.8f, -0.12f), new Vector3(1.2f, 2.8f, 0.4f), stone, collider: false);
            Box("İç", g, new Vector3(0, 0.6f, -0.2f), new Vector3(0.9f, 1.2f, 0.2f), M.Get("soot", new Color(0.03f, 0.025f, 0.02f)), collider: false);
            foreach (var dx in new[] { -0.18f, 0.18f })
                Cyl("Odun", g, new Vector3(dx, 0.12f, 0.05f), new Vector3(0.1f, 0.3f, 0.1f), M.Wood(new Color(0.3f, 0.2f, 0.12f)), new Vector3(0, 0, 90), false);
            Box("Kor", g, new Vector3(0, 0.22f, 0.05f), new Vector3(0.6f, 0.14f, 0.3f), M.Emissive("fire_core", new Color(1f, 0.45f, 0.1f), 7f), collider: false);
            Box("RafTahtası", g, new Vector3(0, 1.47f, 0.08f), new Vector3(1.8f, 0.06f, 0.4f), DarkWood, collider: false);
            Cyl("Şamdan", g, new Vector3(-0.6f, 1.58f, 0.08f), new Vector3(0.05f, 0.08f, 0.05f), Brass, collider: false);
            var l = PointLight("ŞömineAteşi", g, new Vector3(0, 0.5f, 0.6f), new Color(1f, 0.48f, 0.18f), 3.2f, 6.5f, true);
            l.gameObject.AddComponent<LightFlicker>().Init(l, 0.35f);
        }

        // =================================================================
        //  DÜKKÂNA ÖZGÜ DEKOR (Gölgeşehir / Sisören — iç mekan görsellerinden)
        // =================================================================
        public static void Furnish(Transform t, BuildingDef d)
        {
            string key = d.Key ?? "";
            if (key.StartsWith("kasabali_evi")) { Home(t, key); return; }
            switch (key)
            {
                case "telgrafhane":
                    Table(t, new Vector3(-4.35f, 0, 1.2f), new Vector2(0.9f, 2.2f));
                    Telegraph(t, new Vector3(-4.35f, 0.81f, 0.7f));
                    Telegraph(t, new Vector3(-4.35f, 0.81f, 1.5f));
                    for (int i = 0; i < 4; i++) Box("TelgrafŞeridi", t, new Vector3(-4.2f + i * 0.04f, 0.82f, 2.0f + i * 0.08f), new Vector3(0.05f, 0.005f, 0.4f), Paper, new Vector3(0, 20 * i, 0), false);
                    Chair(t, new Vector3(-3.6f, 0, 1.2f), -90f);
                    for (int i = 0; i < 5; i++) Cyl("İzolatör", t, new Vector3(4.6f, 1.2f + (i % 2) * 0.3f, 0.2f + i * 0.5f), new Vector3(0.08f, 0.07f, 0.08f), M.Get("insulator", new Color(0.6f, 0.75f, 0.7f), 0.95f), collider: false);
                    Bookcase(t, new Vector3(4.92f, 0, 2.4f), -90f, 1.6f, 4, 2f);
                    for (int i = 0; i < 6; i++) Box("Kablo", t, new Vector3(-4.9f, 2.8f, -0.5f + i * 0.8f), new Vector3(0.02f, 0.02f, 0.6f), Iron, collider: false);
                    OilLamp(t, new Vector3(-4.5f, 0.81f, 2.1f), 1.4f);
                    break;
                case "kahvehane":
                    foreach (var p in new[] { new Vector3(-3.6f, 0, 0.4f), new Vector3(3.6f, 0, 0.6f), new Vector3(-3.6f, 0, 2.8f) })
                    {
                        Table(t, p, new Vector2(0.9f, 0.9f));
                        Chair(t, p + new Vector3(0.65f, 0, 0), -90f); Chair(t, p + new Vector3(-0.65f, 0, 0), 90f);
                        for (int i = 0; i < 3; i++) Cyl("İnceBelliBardak", t, p + new Vector3(-0.2f + i * 0.2f, 0.85f, 0.1f), new Vector3(0.05f, 0.05f, 0.05f), M.Get("tea", new Color(0.55f, 0.14f, 0.05f), 0.9f), collider: false);
                        Box("Tavla", t, p + new Vector3(0, 0.82f, -0.18f), new Vector3(0.4f, 0.03f, 0.28f), M.Wood(new Color(0.5f, 0.32f, 0.16f)), collider: false);
                    }
                    var sam = Group("Semaver", t, new Vector3(1.2f, 1.02f, 1.7f)).transform;
                    Cyl("Gövde", sam, new Vector3(0, 0.22f, 0), new Vector3(0.28f, 0.2f, 0.28f), M.Get("copper", new Color(0.62f, 0.32f, 0.18f), 0.7f, 0.8f), collider: false);
                    Cyl("Demlik", sam, new Vector3(0, 0.5f, 0), new Vector3(0.14f, 0.08f, 0.14f), M.Get("porcelain", new Color(0.85f, 0.82f, 0.75f), 0.6f), collider: false);
                    Bookcase(t, new Vector3(4.92f, 0, 2.6f), -90f, 1.6f, 4, 1.9f, true);
                    break;
                case "sinema":
                    for (int row = 0; row < 2; row++)
                        for (int i = 0; i < 3; i++)
                            foreach (var side in new[] { -1, 1 })
                            {
                                var p = new Vector3(side * (3.2f + i * 0.55f) + (side < 0 ? 0.4f : -0.4f), 0, 0.2f + row * 1.2f);
                                var s = Group("Koltuk", t, p, 180f).transform;
                                var velvet = M.Get("velvet", new Color(0.42f, 0.05f, 0.08f), 0.3f);
                                Box("Oturak", s, new Vector3(0, 0.42f, 0), new Vector3(0.5f, 0.14f, 0.5f), velvet);
                                Box("Arkalık", s, new Vector3(0, 0.8f, -0.24f), new Vector3(0.5f, 0.7f, 0.1f), velvet, collider: false);
                            }
                    var proj = Group("Projektör", t, new Vector3(-2.0f, 0, -3.3f)).transform;
                    Box("Sehpa", proj, new Vector3(0, 0.55f, 0), new Vector3(0.6f, 1.1f, 0.6f), DarkWood);
                    Box("Makine", proj, new Vector3(0, 1.3f, 0), new Vector3(0.45f, 0.4f, 0.6f), Iron, collider: false);
                    foreach (var z in new[] { -0.25f, 0.25f }) Cyl("Makara", proj, new Vector3(0, 1.75f, z), new Vector3(0.45f, 0.03f, 0.45f), Iron, new Vector3(0, 0, 90), false);
                    var beam = PointLight("ProjektörIşığı", proj, new Vector3(0, 1.3f, 0.5f), new Color(0.9f, 0.92f, 1f), 1.5f, 4f);
                    beam.gameObject.AddComponent<LightFlicker>().Init(beam, 0.25f);
                    for (int i = 0; i < 2; i++)
                        Box("Afiş", t, new Vector3(-4.94f, 1.8f, 1.7f + i * 1.2f), new Vector3(0.02f, 1.1f, 0.75f), M.Get("poster_" + i, i == 0 ? new Color(0.55f, 0.35f, 0.2f) : new Color(0.2f, 0.25f, 0.35f), 0.1f), collider: false);
                    break;
                case "bakkal":
                    Bookcase(t, new Vector3(-4.92f, 0, 2.4f), 90f, 2.2f, 5, 2.4f, true);
                    Bookcase(t, new Vector3(4.92f, 0, 2.2f), -90f, 2.4f, 5, 2.4f, true);
                    foreach (var p in new[] { new Vector3(-4.3f, 0, 0.2f), new Vector3(-3.8f, 0, 0.5f), new Vector3(4.3f, 0, 0.3f), new Vector3(1.8f, 0, -3.55f) }) Sack(t, p);
                    Box("Terazi", t, new Vector3(-1.2f, 1.08f, 1.7f), new Vector3(0.35f, 0.12f, 0.25f), Brass, collider: false);
                    PropFactory.Barrel(t, new Vector3(-1.9f, 0, -3.55f));
                    break;
                case "sahaf":
                    Bookcase(t, new Vector3(-4.92f, 0, 2.4f), 90f, 2.4f, 6, 3.2f);
                    Bookcase(t, new Vector3(4.92f, 0, 1.6f), -90f, 3.6f, 6, 3.2f);
                    for (int s = 0; s < 4; s++)
                        for (int i = 0; i < 5; i++)
                        {
                            var c = Color.HSVToRGB((s * 0.21f + i * 0.13f) % 1f, 0.4f, 0.3f);
                            Box("KitapYığını", t, new Vector3(-1.9f + (s % 2) * 3.8f, 0.04f + i * 0.08f, -3.5f + (s / 2) * 0.4f), new Vector3(0.32f, 0.075f, 0.24f), M.Get("book_" + ColorUtility.ToHtmlStringRGB(c), c), new Vector3(0, i * 9f, 0), false);
                        }
                    Sphere("Küre", t, new Vector3(1.4f, 1.25f, 1.7f), Vector3.one * 0.32f, M.Get("globe", new Color(0.55f, 0.48f, 0.3f), 0.5f));
                    OilLamp(t, new Vector3(-1.3f, 1.02f, 1.7f), 1.5f);
                    break;
                case "muhtar": case "muhtarlik":
                    Table(t, new Vector3(-4.3f, 0, 2.6f), new Vector2(0.9f, 1.6f));
                    for (int i = 0; i < 3; i++) Box("Evrak", t, new Vector3(-4.3f, 0.82f + i * 0.02f, 2.4f + i * 0.15f), new Vector3(0.3f, 0.015f, 0.4f), Paper, new Vector3(0, i * 12f, 0), false);
                    Box("Dolap", t, new Vector3(4.6f, 0.9f, 2.4f), new Vector3(0.6f, 1.8f, 1.2f), DarkWood);
                    Box("Portre", t, new Vector3(4.94f, 2.3f, 0.4f), new Vector3(0.03f, 0.9f, 0.7f), M.Get("gilt", new Color(0.72f, 0.55f, 0.22f), 0.75f, 0.85f), collider: false);
                    Box("PortreTuval", t, new Vector3(4.92f, 2.3f, 0.4f), new Vector3(0.02f, 0.75f, 0.55f), M.Get("portrait_dark", new Color(0.16f, 0.13f, 0.1f)), collider: false);
                    Box("Bayrak", t, new Vector3(-4.94f, 2.4f, 1.9f), new Vector3(0.02f, 0.7f, 1.0f), M.Get("flagred", new Color(0.8f, 0.04f, 0.08f), 0.2f), collider: false);
                    Box("Harita", t, new Vector3(1.6f, 2.6f, -D / 2 + 0.03f), new Vector3(1.0f, 0.7f, 0.02f), M.Get("oldmap", new Color(0.75f, 0.66f, 0.45f), 0.05f), collider: false);
                    OilLamp(t, new Vector3(1.3f, 1.02f, 1.7f), 1.5f);
                    break;
                case "tutuncu":
                    HerbBunches(t, 2.6f, -4.3f, -2.5f, new Color(0.45f, 0.32f, 0.14f), "tobacco_leaf");
                    HerbBunches(t, 2.6f, 2.5f, 4.3f, new Color(0.4f, 0.3f, 0.12f), "tobacco_leaf2");
                    Bookcase(t, new Vector3(-4.92f, 0, 2.5f), 90f, 2.0f, 5, 2.2f, true);
                    Table(t, new Vector3(4.3f, 0, 0.8f), new Vector2(0.9f, 1.6f));
                    for (int i = 0; i < 4; i++) Box("TütünKutusu", t, new Vector3(4.3f, 0.86f, 0.3f + i * 0.33f), new Vector3(0.25f, 0.12f, 0.2f), M.Get("tin_" + i, Color.HSVToRGB(i * 0.2f, 0.4f, 0.4f), 0.6f, 0.5f), collider: false);
                    Box("Pipo", t, new Vector3(0.9f, 1.04f, 1.7f), new Vector3(0.18f, 0.03f, 0.04f), M.Wood(new Color(0.3f, 0.16f, 0.08f)), collider: false);
                    break;
                case "ahir":
                    foreach (var p in new[] { new Vector3(-4.0f, 0, 0.2f), new Vector3(-4.0f, 0.5f, 0.3f), new Vector3(4.0f, 0, 0.6f), new Vector3(3.9f, 0, 2.6f) }) HayBale(t, p, 90f);
                    for (int i = 0; i < 3; i++) Box("BölmeÇiti", t, new Vector3(-2.8f + i * 0.02f, 0.5f + i * 0.35f, 2.8f), new Vector3(0.08f, 0.08f, 1.8f), MidWood, collider: false);
                    Box("Yemlik", t, new Vector3(-4.4f, 0.35f, 2.6f), new Vector3(0.7f, 0.4f, 1.6f), DarkWood);
                    Box("Saman", t, new Vector3(0, 0.01f, 0.4f), new Vector3(6f, 0.02f, 3f), M.Get("hay", new Color(0.62f, 0.52f, 0.28f), 0.05f, 0f, M.NoiseTexture), collider: false);
                    for (int i = 0; i < 3; i++) Box("Alet", t, new Vector3(4.94f, 1.6f, 1.6f + i * 0.4f), new Vector3(0.03f, 1.4f, 0.05f), M.Wood(new Color(0.45f, 0.33f, 0.2f)), new Vector3(8, 0, 0), false);
                    break;
                case "tupcu":
                    for (int i = 0; i < 9; i++)
                    {
                        var c = i % 3 == 0 ? new Color(0.62f, 0.33f, 0.12f) : i % 3 == 1 ? new Color(0.25f, 0.35f, 0.42f) : new Color(0.3f, 0.38f, 0.26f);
                        var x = i < 5 ? -4.5f + (i % 2) * 0.35f : 4.5f - (i % 2) * 0.35f;
                        Cyl("Tüp", t, new Vector3(x, 0.45f, -0.4f + (i % 5) * 0.75f), new Vector3(0.3f, 0.45f, 0.3f), M.Get("cylinder_" + (i % 3), c, 0.5f, 0.6f));
                    }
                    Table(t, new Vector3(-1.8f, 0, -3.5f), new Vector2(1.2f, 0.6f));
                    Box("Defter", t, new Vector3(-1.8f, 0.82f, -3.5f), new Vector3(0.3f, 0.03f, 0.22f), M.Get("book_ox", new Color(0.35f, 0.08f, 0.06f)), collider: false);
                    break;
                case "hurdaci":
                    for (int i = 0; i < 10; i++)
                    {
                        var p = new Vector3((i % 2 == 0 ? -1 : 1) * (3.9f + (i % 3) * 0.3f), 0, -0.4f + (i / 2) * 0.8f);
                        if (i % 3 == 0) Cyl("Tekerlek", t, p + Vector3.up * 0.5f, new Vector3(1f, 0.05f, 1f), M.Wood(new Color(0.3f, 0.2f, 0.12f)), new Vector3(0, 0, 80), false);
                        else if (i % 3 == 1) PropFactory.Crate(t, p, 0.6f);
                        else Cyl("Kazan", t, p + Vector3.up * 0.25f, new Vector3(0.5f, 0.25f, 0.5f), M.Get("copper", new Color(0.62f, 0.32f, 0.18f), 0.7f, 0.8f), collider: false);
                    }
                    for (int i = 0; i < 6; i++) Box("Hurda", t, new Vector3(-1.9f + (i % 3) * 0.2f, 0.06f + (i / 3) * 0.1f, -3.5f), new Vector3(0.5f, 0.08f, 0.3f), Iron, new Vector3(0, i * 25f, i * 6f), false);
                    break;
                case "oduncu":
                    for (int i = 0; i < 12; i++)
                        Cyl("Kütük", t, new Vector3(-4.5f + (i % 2) * 0.1f, 0.15f + (i / 3) * 0.28f, -0.3f + (i % 3) * 0.32f), new Vector3(0.28f, 0.45f, 0.28f), M.Wood(new Color(0.42f, 0.3f, 0.18f)), new Vector3(0, 0, 90), false);
                    Fireplace(t, true, 1.6f);
                    Box("Testere", t, new Vector3(-4.94f, 2.0f, 2.4f), new Vector3(0.02f, 0.25f, 1.0f), M.Get("steel", new Color(0.55f, 0.55f, 0.58f), 0.75f, 0.9f), collider: false);
                    Box("Balta", t, new Vector3(-4.94f, 1.7f, 1.4f), new Vector3(0.03f, 0.8f, 0.05f), M.Wood(new Color(0.45f, 0.33f, 0.2f)), new Vector3(15, 0, 0), false);
                    Box("Talaş", t, new Vector3(-3.6f, 0.005f, 0.3f), new Vector3(1.2f, 0.01f, 1.2f), M.Get("sawdust", new Color(0.7f, 0.58f, 0.38f), 0.05f), collider: false);
                    break;
                case "manav":
                    for (int i = 0; i < 6; i++)
                    {
                        var p = new Vector3(i < 3 ? -4.3f : 4.3f, 0, 0.2f + (i % 3) * 1.0f);
                        PropFactory.Crate(t, p, 0.65f);
                        var fruit = M.Get("fruit_" + i, Color.HSVToRGB(new[] { 0.02f, 0.08f, 0.15f, 0.25f, 0.05f, 0.12f }[i], 0.75f, 0.55f), 0.4f);
                        for (int f = 0; f < 6; f++) Sphere("Meyve", t, p + new Vector3(-0.18f + (f % 3) * 0.18f, 0.7f, -0.1f + (f / 3) * 0.2f), Vector3.one * 0.13f, fruit);
                    }
                    HerbBunches(t, 2.9f, -1.5f, 1.5f, new Color(0.25f, 0.3f, 0.12f), "herb_green");
                    break;
                case "demirci":
                    var stone = M.Get("stonewall_rural", new Color(0.42f, 0.41f, 0.39f), 0.2f, 0f, M.BrickTexture);
                    Box("Ocak", t, new Vector3(-4.2f, 0.6f, 0.8f), new Vector3(1.3f, 1.2f, 1.4f), stone);
                    Box("Kor", t, new Vector3(-4.2f, 1.22f, 0.8f), new Vector3(0.9f, 0.06f, 1f), M.Emissive("embers", new Color(1f, 0.38f, 0.08f), 6f), collider: false);
                    var fire = PointLight("OcakAteşi", t, new Vector3(-4.0f, 1.8f, 0.8f), new Color(1f, 0.45f, 0.15f), 4f, 7f);
                    fire.gameObject.AddComponent<LightFlicker>().Init(fire, 0.35f);
                    var anvil = Group("Örs", t, new Vector3(-2.6f, 0, 0.3f)).transform;
                    Cyl("Kütük", anvil, new Vector3(0, 0.3f, 0), new Vector3(0.5f, 0.3f, 0.5f), M.Wood(new Color(0.35f, 0.24f, 0.14f)));
                    Box("ÖrsGövde", anvil, new Vector3(0, 0.72f, 0), new Vector3(0.6f, 0.22f, 0.25f), Iron, collider: false);
                    Box("Çekiç", anvil, new Vector3(0.1f, 0.86f, 0.05f), new Vector3(0.3f, 0.05f, 0.05f), M.Wood(new Color(0.45f, 0.33f, 0.2f)), collider: false);
                    for (int i = 0; i < 6; i++) Box("Maşa", t, new Vector3(4.94f, 1.5f + (i % 2) * 0.2f, 0.3f + i * 0.45f), new Vector3(0.03f, 0.7f, 0.04f), Iron, new Vector3(0, 0, i * 4f), false);
                    break;
                case "hekim":
                    Bookcase(t, new Vector3(-4.92f, 0, 2.4f), 90f, 2.4f, 6, 2.8f, true);
                    var bed = Group("MuayeneSedir", t, new Vector3(4.35f, 0, 1.6f)).transform;
                    Box("Sedir", bed, new Vector3(0, 0.45f, 0), new Vector3(0.8f, 0.12f, 2.0f), M.Get("leather_dark", new Color(0.3f, 0.15f, 0.08f), 0.5f));
                    Box("Çarşaf", bed, new Vector3(0, 0.52f, 0.2f), new Vector3(0.78f, 0.02f, 1.4f), M.Get("bed_white", new Color(0.9f, 0.9f, 0.88f)), collider: false);
                    Box("DoktorÇantası", t, new Vector3(-1.2f, 1.1f, 1.7f), new Vector3(0.4f, 0.2f, 0.2f), M.Get("leather_dark", new Color(0.3f, 0.15f, 0.08f), 0.5f), collider: false);
                    OilLamp(t, new Vector3(1.3f, 1.02f, 1.7f), 1.4f);
                    break;
                case "ayakkabici":
                    Bookcase(t, new Vector3(-4.92f, 0, 2.4f), 90f, 2.4f, 5, 2.4f);
                    for (int i = 0; i < 8; i++)
                        Box("Ayakkabı", t, new Vector3(4.6f, 0.08f + (i / 4) * 0.45f, 0.2f + (i % 4) * 0.5f), new Vector3(0.12f, 0.12f, 0.3f), M.Get("leather_shoe", new Color(0.28f, 0.16f, 0.08f), 0.6f), collider: false);
                    Box("Raf", t, new Vector3(4.6f, 0.5f, 0.95f), new Vector3(0.4f, 0.04f, 2.1f), MidWood, collider: false);
                    Box("Kalıp", t, new Vector3(1.2f, 1.08f, 1.7f), new Vector3(0.12f, 0.12f, 0.28f), M.Wood(new Color(0.55f, 0.4f, 0.25f)), collider: false);
                    OilLamp(t, new Vector3(-1.3f, 1.02f, 1.7f), 1.4f);
                    break;
                case "kasabali_evi":
                    Home(t, key);
                    break;
                default:
                    PropFactory.Barrel(t, new Vector3(-4.3f, 0, 0.4f));
                    PropFactory.Crate(t, new Vector3(4.3f, 0, 0.4f), 0.7f);
                    break;
            }
        }

        /// <summary>Kasabalı evleri / Fehmi Bey'in evi: şömine, karyola, dolap, dokuma tezgahı, kitaplık.</summary>
        static void Home(Transform t, string key)
        {
            int v = key.EndsWith("_2") ? 2 : key.EndsWith("_3") ? 3 : key.EndsWith("_1") ? 1 : 0;
            Fireplace(t, true, 1.4f);
            if (v == 2)
            {
                var bed = Group("Karyola", t, new Vector3(-4.1f, 0, 1.8f)).transform;
                Box("Çerçeve", bed, new Vector3(0, 0.3f, 0), new Vector3(1.3f, 0.3f, 2.1f), DarkWood);
                Box("Yorgan", bed, new Vector3(0, 0.5f, 0.1f), new Vector3(1.25f, 0.15f, 1.8f), M.Get("quilt", new Color(0.45f, 0.35f, 0.3f), 0.05f), collider: false);
                Box("BaşTahtası", bed, new Vector3(0, 0.8f, 1.02f), new Vector3(1.3f, 1.0f, 0.08f), DarkWood, collider: false);
            }
            else if (v == 3)
            {
                var loom = Group("DokumaTezgahı", t, new Vector3(-4.1f, 0, 2.3f)).transform;
                Box("Çerçeve", loom, new Vector3(0, 0.9f, 0), new Vector3(0.9f, 1.8f, 1.4f), DarkWood, collider: false);
                Box("Çözgü", loom, new Vector3(0, 0.95f, 0), new Vector3(0.05f, 1.1f, 1.2f), M.Get("wool", new Color(0.55f, 0.15f, 0.12f), 0.05f), collider: false);
                HerbBunches(t, 2.8f, -2.4f, 2.4f, new Color(0.3f, 0.33f, 0.18f), "herb_dry");
            }
            else
            {
                Bookcase(t, new Vector3(-4.92f, 0, 2.4f), 90f, 2.4f, 6, 2.6f);
                var arm = Group("Berjer", t, new Vector3(-3.6f, 0, 0.2f), 60f).transform;
                var fabric = M.Get("armchair", new Color(0.4f, 0.25f, 0.15f), 0.2f);
                Box("Oturak", arm, new Vector3(0, 0.4f, 0), new Vector3(0.8f, 0.3f, 0.75f), fabric);
                Box("Arkalık", arm, new Vector3(0, 0.85f, -0.32f), new Vector3(0.8f, 0.8f, 0.15f), fabric, collider: false);
            }
            Table(t, new Vector3(4.3f, 0, -0.2f), new Vector2(0.8f, 0.8f), 0f, 0.7f);
            OilLamp(t, new Vector3(4.3f, 0.73f, -0.2f), 1.2f);
        }
    }
}
