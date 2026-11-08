using System.Collections.Generic;
using Dedektif3D.Core;
using UnityEngine;
using static Dedektif3D.World.Shapes;
using M = Dedektif3D.World.MaterialLibrary;

namespace Dedektif3D.World
{
    /// <summary>
    /// "Gizemli Kasaba"yı web görsellerine göre Runtime'da inşa eder:
    ///  • town_map_generated.png → parke taşlı meydan, etrafında Eczane / Kasap / Terzi / Muhtarlık / Karakol / Kahvehane / Bakkal,
    ///    kuzeye çıkan parke yol ve çiftlikler (ambar + silo), çevre toprak yolu, yarı ahşap evler, yön tabelaları, sis ve su birikintileri.
    ///  • town_bg.png → yağmurlu Türk kasabası: cumbalı evler, cami silueti, olay yeri (ceset örtüsü, delil numaraları,
    ///    POLİS GİRİLMEZ şeridi), çakarlı polis aracı, taksi, şemsiyeli kalabalık, sarı sokak lambaları.
    ///  • world_map_generated.png → kasabanın kuzeyinde göl, çevresinde çam ormanı, sisli dağlar, dere ve taş köprü.
    /// Meydan merkezi (0,0,0), +Z haritanın kuzeyi.
    /// </summary>
    public partial class TownGenerator : MonoBehaviour
    {
        /// <summary>Olay yeri şeridinin görünmez engeli: kasabalıları durdurur, dedektif (Ignore Raycast katmanı) geçer.</summary>
        public const int NpcBarrierLayer = 12;

        public readonly Dictionary<int, BuildingGenerator.BuildingInstance> Buildings = new Dictionary<int, BuildingGenerator.BuildingInstance>();
        public readonly List<BuildingGenerator.BuildingInstance> DecorBuildings = new List<BuildingGenerator.BuildingInstance>();
        public readonly Dictionary<int, InteriorGenerator.InteriorInstance> Interiors = new Dictionary<int, InteriorGenerator.InteriorInstance>();

        public Vector3 PlayerSpawn { get; private set; }
        public Quaternion PlayerSpawnRotation { get; private set; }
        public Vector3 HelperPosition { get; private set; }
        public Vector3 CrimeScene { get; private set; }

        /// <summary>"Kasabaya İniş" sinematiği: göl üstünden meydana süzülen kamera yolu (konum, bakış hedefi).</summary>
        public readonly List<Vector3> IntroPath = new List<Vector3>();
        public readonly List<Vector3> IntroLook = new List<Vector3>();
        /// <summary>Splash/Bölge haritası/Hikaye sırasında arkada yavaşça dönen kamera için merkez.</summary>
        public Vector3 IdleOrbitCenter => new Vector3(0, 3, 0);

        /// <summary>Kasabalıların yürüdüğü güzergah noktaları.</summary>
        public readonly List<Vector3> WalkPoints = new List<Vector3>();

        static readonly Vector3 InteriorOrigin = new Vector3(0, -300f, 0); // İç mekanlar yerin altında, kasabadan görünmez

        public const float PlazaRadius = 17f;
        public const float RingRadius = 44f;

        readonly List<Vector3> _occupied = new List<Vector3>();

        readonly List<GameObject> _generated = new List<GameObject>();
        public TownData Data { get; private set; }

        /// <summary>Bu kasabaya ait üretilmiş objeyi kaydet (kasaba değişince silinir).</summary>
        public void Track(GameObject go) { if (go != null) _generated.Add(go); }

        /// <summary>Önceki kasabayı tamamen kaldırır (Level değişiminde).</summary>
        public void Clear()
        {
            foreach (var go in _generated) if (go != null) Destroy(go);
            _generated.Clear();
            Buildings.Clear(); DecorBuildings.Clear(); Interiors.Clear();
            IntroPath.Clear(); IntroLook.Clear(); WalkPoints.Clear(); _occupied.Clear();
        }

        public void Generate(TownData data)
        {
            Clear();
            Data = data;
            Random.InitState(data.Seed);
            var world = transform;
            var town = Group("Kasaba", world, Vector3.zero).transform;
            Track(town.gameObject);
            if (data.Id != "gizemli")
            {
                if (data.Id == "golge_sehir") BuildGolgeSehir(town, data); else BuildSisoren(town, data);
                PopulateLife(town, data);
                BuildInteriors(world, data);
                return;
            }

            BuildGround(town);
            BuildPlaza(town);
            BuildBuildings(town);
            BuildRoads(town);
            BuildCrimeScene(town);
            BuildHouses(town);
            BuildFarmsAndLandmarks(town);
            BuildNature(town);
            BuildBoundary(town);
            ScatterPuddles(town);
            SpawnWalkers(town);
            PopulateLife(town, data);
            Effects.GroundFog(town, new Vector3(0, 0.6f, 0), new Vector3(70, 0.5f, 70), 50);

            BuildInteriors(world, data);

            PlayerSpawn = new Vector3(-1.5f, 0.1f, -16f);
            PlayerSpawnRotation = Quaternion.LookRotation(CrimeScene - PlayerSpawn);
            BuildIntroPath();
        }

        void BuildInteriors(Transform world, TownData data)
        {
            var interiors = Group("İçMekanlar", world, InteriorOrigin).transform;
            Track(interiors.gameObject);
            int i = 0;
            foreach (var def in data.Buildings)
            {
                if (!def.Enterable) continue;
                Interiors[def.NpcId] = InteriorGenerator.Build(def, interiors, new Vector3((i - 2) * 30f, 0, 0));
                i++;
            }
        }

        bool Free(Vector3 p, float r)
        {
            foreach (var o in _occupied) if ((new Vector2(o.x - p.x, o.z - p.z)).magnitude < r) return false;
            return true;
        }

        static float FacePlazaYaw(Vector3 pos)
        {
            var d = -new Vector3(pos.x, 0, pos.z).normalized; // ön cephe (-Z yerel) meydana bakmalı
            return Mathf.Atan2(-d.x, -d.z) * Mathf.Rad2Deg;
        }

        // =================================================================
        //  ZEMİN
        // =================================================================
        void BuildGround(Transform t)
        {
            var g = Box("Zemin", t, new Vector3(0, -0.5f, 0), new Vector3(1400, 1, 1400), M.Get("grass_dark", new Color(0.15f, 0.2f, 0.14f), 0.15f, 0f, M.NoiseTexture));
            SetTiling(g.GetComponent<Renderer>(), new Vector2(230, 230));
            var dirt = Cyl("KasabaToprağı", t, new Vector3(0, 0.008f, 4f), new Vector3(120, 0.008f, 120), M.Get("mud", new Color(0.2f, 0.18f, 0.15f), 0.55f, 0f, M.NoiseTexture), collider: false);
            SetTiling(dirt.GetComponent<Renderer>(), new Vector2(26, 26));
        }

        // =================================================================
        //  MEYDAN: ıslak parke, bordür, saat kulesi, banklar, lambalar
        // =================================================================
        void BuildPlaza(Transform t)
        {
            var plaza = Group("Meydan", t, Vector3.zero).transform;
            var cobble = M.Get("cobble_wet", new Color(0.42f, 0.42f, 0.45f), 0.72f, 0.05f, M.CobbleTexture);
            var floor = Cyl("ParkeMeydan", plaza, new Vector3(0, 0.03f, 0), new Vector3(PlazaRadius * 2, 0.03f, PlazaRadius * 2), cobble);
            SetTiling(floor.GetComponent<Renderer>(), new Vector2(9, 9));
            // Binalara doğru taşan parke cepler (düzensiz meydan silueti)
            foreach (var b in GameData.Buildings)
            {
                var dir = new Vector3(b.Position.x, 0, b.Position.z);
                var mid = dir.normalized * (PlazaRadius + (dir.magnitude - PlazaRadius) * 0.45f);
                float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
                var pocket = Box("ParkeCep", plaza, new Vector3(mid.x, 0.035f, mid.z), new Vector3(b.Size.x + 3f, 0.04f, dir.magnitude - PlazaRadius + 4f), cobble, new Vector3(0, yaw, 0), false);
                SetTiling(pocket.GetComponent<Renderer>(), new Vector2((b.Size.x + 3f) / 3.8f, (dir.magnitude - PlazaRadius + 4f) / 3.8f));
            }
            // Taş bordür halkası (kaldırım kenarı)
            var kerb = M.Get("kerb", new Color(0.5f, 0.49f, 0.47f), 0.4f, 0f, M.NoiseTexture);
            for (int i = 0; i < 48; i++)
            {
                float a = i / 48f * Mathf.PI * 2f;
                var p = new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a)) * (PlazaRadius + 0.15f);
                if (NearRoadOrBuildingFront(p)) continue;
                Box("Bordür", plaza, p + Vector3.up * 0.08f, new Vector3(2.3f, 0.16f, 0.3f), kerb, new Vector3(0, a * Mathf.Rad2Deg + 90f, 0), false);
            }

            TownProps.ClockTower(plaza, new Vector3(0, 0.06f, 3f));
            _occupied.Add(new Vector3(0, 0, 3f));

            // Lambalar: meydan çevresinde
            for (int i = 0; i < 10; i++)
            {
                float a = (i / 10f) * Mathf.PI * 2f + 0.2f;
                var p = new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a)) * (PlazaRadius - 1.2f);
                PropFactory.StreetLamp(plaza, p, new Color(1f, 0.72f, 0.42f));
            }
            // Banklar saat kulesinin etrafında
            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI / 2f + Mathf.PI / 4f;
                var p = new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a)) * 5.5f + new Vector3(0, 0, 3f);
                PropFactory.Bench(plaza, p, a * Mathf.Rad2Deg + 180f);
            }
        }

        static bool NearRoadOrBuildingFront(Vector3 p)
        {
            if (Mathf.Abs(p.x) < 3.5f) return true; // kuzey/güney yolu ağzı
            foreach (var b in GameData.Buildings)
            {
                var dir = new Vector3(b.Position.x, 0, b.Position.z).normalized;
                if (Vector3.Angle(dir, p) < 12f) return true;
            }
            return false;
        }

        // =================================================================
        //  BİNALAR (harita konumlarında, ön cephe meydana dönük)
        // =================================================================
        void BuildBuildings(Transform t)
        {
            var parent = Group("Dükkânlar", t, Vector3.zero).transform;
            foreach (var def in GameData.Buildings)
            {
                var inst = BuildingGenerator.Build(def, parent, def.Position, FacePlazaYaw(def.Position));
                if (def.Enterable) Buildings[def.NpcId] = inst; else DecorBuildings.Add(inst);
                _occupied.Add(def.Position);
            }
        }

        // =================================================================
        //  YOLLAR: kuzey parke yolu, güney giriş yolu + köprü, çevre toprak yolu, radyal patikalar
        // =================================================================
        void BuildRoads(Transform t)
        {
            var roads = Group("Yollar", t, Vector3.zero).transform;
            var cobble = M.Get("cobble_wet", new Color(0.42f, 0.42f, 0.45f), 0.72f, 0.05f, M.CobbleTexture);
            var dirt = M.Get("dirtroad", new Color(0.24f, 0.2f, 0.16f), 0.6f, 0f, M.NoiseTexture);
            var kerb = M.Get("kerb", new Color(0.5f, 0.49f, 0.47f), 0.4f, 0f, M.NoiseTexture);

            // Kuzey yolu: hafif kıvrımlı parke (haritanın ortasından çiftliklere)
            var north = new[] { new Vector3(0, 0, 16), new Vector3(1.5f, 0, 30), new Vector3(-1f, 0, 44), new Vector3(2f, 0, 60), new Vector3(0, 0, 82) };
            Polyline(roads, north, 6f, cobble, true, kerb);
            // Güney yolu: meydandan kasaba girişine (dereyi köprüyle geçer)
            var south = new[] { new Vector3(0, 0, -16), new Vector3(-1f, 0, -32), new Vector3(0, 0, -52), new Vector3(0, 0, -59) };
            Polyline(roads, south, 6.5f, cobble, true, kerb);
            Polyline(roads, new[] { new Vector3(0, 0, -65), new Vector3(1.5f, 0, -85), new Vector3(0, 0, -110) }, 6f, dirt, false, null);
            TownProps.Bridge(roads, new Vector3(0, 0, -62), 0, 9f);
            // Bina önleri ile meydan arası kaldırım lambaları
            for (int i = 0; i < north.Length - 1; i++)
            {
                var p = Vector3.Lerp(north[i], north[i + 1], 0.5f);
                PropFactory.StreetLamp(roads, p + new Vector3(i % 2 == 0 ? 4.2f : -4.2f, 0, 0), new Color(1f, 0.72f, 0.42f));
            }
            for (int i = 0; i < south.Length - 1; i++)
            {
                var p = Vector3.Lerp(south[i], south[i + 1], 0.5f);
                PropFactory.StreetLamp(roads, p + new Vector3(i % 2 == 0 ? -4.4f : 4.4f, 0, 0), new Color(1f, 0.72f, 0.42f));
            }
            for (float z = 16; z < 82; z += 2) _occupied.Add(new Vector3(0, 0, z));
            for (float z = -16; z > -110; z -= 2) _occupied.Add(new Vector3(0, 0, z));

            // Çevre yolu (toprak, kasabayı halka gibi sarar)
            var ring = new List<Vector3>();
            for (int i = 0; i <= 40; i++)
            {
                float a = i / 40f * Mathf.PI * 2f;
                float r = RingRadius + Mathf.Sin(a * 3f) * 3f;
                ring.Add(new Vector3(Mathf.Sin(a) * r, 0, Mathf.Cos(a) * r * 1.05f + 4f));
            }
            Polyline(roads, ring.ToArray(), 4.5f, dirt, false, null);
            foreach (var p in ring) _occupied.Add(p);
            for (int i = 0; i < ring.Count; i += 2) WalkPoints.Add(ring[i] + Vector3.up * 0.05f);

            // Meydandan çevre yoluna radyal patikalar + yön tabelaları (town_map'teki tabelalar)
            var spokes = new[]
            {
                (yaw: -50f, name: new[] { "GÖLGEŞEHİR", "DARSOKAK" }),
                (yaw: 45f,  name: new[] { "YIKIKKÖY", "SİSÖREN" }),
                (yaw: -135f, name: new[] { "DARSOKAK", "GÖLGEŞEHİR" }),
                (yaw: 132f, name: new[] { "KARAORMAN", "YIKIKKÖY" }),
            };
            foreach (var s in spokes)
            {
                var dir = Quaternion.Euler(0, s.yaw, 0) * Vector3.forward;
                var a = dir * (PlazaRadius + 1f);
                var b = dir * (RingRadius + 1f) + new Vector3(0, 0, 4f);
                Polyline(roads, new[] { a, Vector3.Lerp(a, b, 0.5f) + Vector3.Cross(Vector3.up, dir) * 2f, b }, 3.2f, dirt, false, null);
                for (float k = 0; k <= 1; k += 0.1f) _occupied.Add(Vector3.Lerp(a, b, k));
                TownProps.Signpost(roads, b + Vector3.Cross(Vector3.up, dir) * 3.2f, s.yaw, s.name);
                PropFactory.StreetLamp(roads, Vector3.Lerp(a, b, 0.5f) - Vector3.Cross(Vector3.up, dir) * 2.4f, new Color(1f, 0.7f, 0.4f));
            }
            TownProps.Signpost(roads, new Vector3(4.5f, 0, 74f), 0, "SİSÖREN", "GÖL YOLU");
            TownProps.Signpost(roads, new Vector3(-4.8f, 0, -44f), 180, "GİZEMLİ KASABA", "MEYDAN");

            // Dere (güneyde doğu-batı akan) + kıyı taşları
            var water = M.Get("river", new Color(0.05f, 0.08f, 0.11f), 0.96f, 0.3f);
            Box("Dere", t, new Vector3(0, -0.15f, -62f), new Vector3(300f, 0.1f, 6.5f), water, collider: false);
            Box("DereYatağı", t, new Vector3(0, -0.4f, -62f), new Vector3(300f, 0.2f, 7f), M.Get("riverbed", new Color(0.12f, 0.12f, 0.1f)), collider: false);
            Box("DereEngelK", t, new Vector3(0, 0.5f, -58.6f), new Vector3(300f, 1f, 0.3f), null, collider: true).GetComponent<Renderer>().enabled = false;
            Box("DereEngelG", t, new Vector3(0, 0.5f, -65.4f), new Vector3(300f, 1f, 0.3f), null, collider: true).GetComponent<Renderer>().enabled = false;
            for (int i = 0; i < 70; i++)
            {
                float x = Random.Range(-140f, 140f);
                if (Mathf.Abs(x) < 4f) continue;
                Sphere("KıyıTaşı", t, new Vector3(x, 0, -62f + (Random.value > 0.5f ? 3.4f : -3.4f)), new Vector3(Random.Range(0.6f, 1.4f), 0.5f, Random.Range(0.6f, 1.4f)), M.Stone);
            }
        }

        /// <summary>Noktalar boyunca yol şeridi (+ isteğe bağlı iki yanda kaldırım bordürü).</summary>
        void Polyline(Transform parent, Vector3[] pts, float width, Material mat, bool sidewalks, Material kerb)
        {
            for (int i = 0; i < pts.Length - 1; i++)
            {
                var a = pts[i]; var b = pts[i + 1];
                var d = b - a; float len = d.magnitude;
                var mid = (a + b) / 2f;
                float yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
                var seg = Box("Yol", parent, new Vector3(mid.x, 0.03f + i * 0.0004f, mid.z), new Vector3(width, 0.04f, len + width * 0.6f), mat, new Vector3(0, yaw, 0), false);
                SetTiling(seg.GetComponent<Renderer>(), new Vector2(width / 3.8f, (len + width * 0.6f) / 3.8f));
                if (sidewalks)
                {
                    var side = Vector3.Cross(Vector3.up, d.normalized);
                    foreach (var sgn in new[] { -1f, 1f })
                    {
                        var c = mid + side * sgn * (width / 2 + 0.75f);
                        Box("Kaldırım", parent, new Vector3(c.x, 0.1f, c.z), new Vector3(1.5f, 0.2f, len + 0.4f), M.Get("sidewalk", new Color(0.38f, 0.37f, 0.36f), 0.55f, 0f, M.NoiseTexture), new Vector3(0, yaw, 0), false, 0.5f);
                        Box("Bordür", parent, new Vector3(mid.x + side.x * sgn * width / 2, 0.11f, mid.z + side.z * sgn * width / 2), new Vector3(0.18f, 0.22f, len + 0.4f), kerb, new Vector3(0, yaw, 0), false);
                    }
                }
            }
        }

        // =================================================================
        //  OLAY YERİ (town_bg.png): ceset örtüsü, delil numaraları, POLİS GİRİLMEZ şeridi, polis, kalabalık
        // =================================================================
        void BuildCrimeScene(Transform t)
        {
            CrimeScene = new Vector3(4.5f, 0.07f, -8f);
            var cs = Group("OlayYeri", t, CrimeScene, -25f).transform;
            _occupied.Add(CrimeScene);
            var chalk = M.Unlit("chalk", new Color(0.92f, 0.92f, 0.9f));
            // Kurbanın üzerindeki gri örtü
            var sheet = M.Get("sheet", new Color(0.28f, 0.3f, 0.32f), 0.45f);
            Box("CesetÖrtüsü", cs, new Vector3(0, 0.12f, 0), new Vector3(0.75f, 0.24f, 1.85f), sheet, collider: false);
            Sphere("ÖrtüBaş", cs, new Vector3(0, 0.16f, 0.95f), new Vector3(0.38f, 0.26f, 0.36f), sheet);
            Sphere("ÖrtüAyak", cs, new Vector3(0, 0.12f, -0.95f), new Vector3(0.55f, 0.2f, 0.3f), sheet);
            Box("ÖrtüKenar", cs, new Vector3(0, 0.02f, 0), new Vector3(1.15f, 0.03f, 2.3f), sheet, collider: false);
            Box("Ayakkabı", cs, new Vector3(0.18f, 0.08f, -1.25f), new Vector3(0.12f, 0.1f, 0.26f), M.Get("shoe", new Color(0.08f, 0.06f, 0.05f), 0.6f), collider: false);
            Box("KanLekesi", cs, new Vector3(0.45f, 0.005f, 0.6f), new Vector3(0.9f, 0.006f, 0.6f), M.Get("blood_wet", new Color(0.35f, 0.02f, 0.03f), 0.9f), new Vector3(0, 30, 0), false);
            // Tebeşir çerçeve
            var outline = new[] { new Vector3(-0.7f, 0, -1.4f), new Vector3(0.7f, 0, -1.4f), new Vector3(0.75f, 0, 1.3f), new Vector3(-0.75f, 0, 1.3f), new Vector3(-0.7f, 0, -1.4f) };
            for (int i = 0; i < outline.Length - 1; i++)
            {
                var a = outline[i]; var b = outline[i + 1]; var d = b - a;
                Box("Tebeşir", cs, (a + b) / 2 + Vector3.up * 0.004f, new Vector3(0.05f, 0.005f, d.magnitude), chalk, new Vector3(0, Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg, 0), false);
            }
            // Delil numara kartları 1-2-3 (sarı)
            var cardPos = new[] { new Vector3(-0.9f, 0, 0.4f), new Vector3(1.0f, 0, -0.3f), new Vector3(0.3f, 0, 1.6f) };
            for (int i = 0; i < 3; i++)
            {
                var card = Group("DelilKartı" + (i + 1), cs, cardPos[i], Random.Range(-30f, 30f)).transform;
                Prism("Kart", card, Vector3.zero, new Vector3(0.22f, 0.2f, 0.16f), M.Emissive("evidencecard", new Color(1f, 0.82f, 0.1f), 0.4f), 0);
                Text3D("No", card, new Vector3(0, 0.08f, -0.06f), (i + 1).ToString(), Color.black, 0.02f);
            }
            // POLİS GİRİLMEZ şeridi
            var c = new[] { new Vector3(-3f, 0, -3.2f), new Vector3(3f, 0, -3.2f), new Vector3(3f, 0, 3.2f), new Vector3(-3f, 0, 3.2f) };
            for (int i = 0; i < 4; i++)
            {
                PropFactory.PoliceTapeLine(cs, c[i], c[(i + 1) % 4]);
                var mid = (c[i] + c[(i + 1) % 4]) / 2 + Vector3.up * 0.9f;
                var d = c[(i + 1) % 4] - c[i];
                float yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg - 90f;
                Text3D("ŞeritYazı", cs, mid + Quaternion.Euler(0, yaw, 0) * new Vector3(0, 0, -0.02f), "ZAPTİYE - GİRİLMEZ   ZAPTİYE - GİRİLMEZ", new Color(0.05f, 0.05f, 0.05f), 0.007f, yaw);
                Text3D("ŞeritYazıArka", cs, mid + Quaternion.Euler(0, yaw, 0) * new Vector3(0, 0, 0.02f), "ZAPTİYE - GİRİLMEZ   ZAPTİYE - GİRİLMEZ", new Color(0.05f, 0.05f, 0.05f), 0.007f, yaw + 180f);
            }
            // Görünmez engel: yalnızca kasabalılar ve NPC'ler olay yerine giremez — dedektif şeridin altından geçip inceleyebilir
            var block = new GameObject("OlayYeriEngeli"); block.transform.SetParent(cs, false); block.layer = NpcBarrierLayer;
            block.transform.localPosition = new Vector3(0, 0.6f, 0);
            block.AddComponent<BoxCollider>().size = new Vector3(6f, 1.2f, 6.4f);

            // Polis memurları + olay yeri inceleme uzmanı (fenerle)
            var officerA = PropFactory.BuildPerson("PolisMemuru", cs, new Vector3(-3.6f, 0, -1.5f), new Color(0.1f, 0.12f, 0.18f), new Color(0.75f, 0.65f, 0.2f), false, new Color(0.1f, 0.12f, 0.18f), FaceLibrary.NextTownsfolkFace());
            PoliceCap(officerA); officerA.Root.localRotation = Quaternion.Euler(0, 80, 0);
            var officerB = PropFactory.BuildPerson("PolisMemuru2", cs, new Vector3(-3.4f, 0, 1.2f), new Color(0.1f, 0.12f, 0.18f), new Color(0.75f, 0.65f, 0.2f), false, new Color(0.1f, 0.12f, 0.18f), FaceLibrary.NextTownsfolkFace());
            PoliceCap(officerB); officerB.Root.localRotation = Quaternion.Euler(0, 110, 0);
            officerB.ArmR.localRotation = Quaternion.Euler(-70, 0, 0);
            var csi = PropFactory.BuildPerson("OlayYeriUzmanı", cs, new Vector3(1.0f, -0.45f, 0.9f), new Color(0.12f, 0.12f, 0.14f), new Color(0.2f, 0.2f, 0.25f), false, null, FaceLibrary.NextTownsfolkFace(), "kneel");
            if (csi.HasModel) csi.Root.localPosition = new Vector3(1.0f, 0f, 0.9f);
            csi.Root.localRotation = Quaternion.Euler(0, -120, 0);
            csi.LegL.localRotation = Quaternion.Euler(-90, 0, 0); csi.LegR.localRotation = Quaternion.Euler(-30, 0, 0);
            if (!csi.HasModel) csi.Body.localRotation = Quaternion.Euler(20, 0, 0);
            csi.ArmR.localRotation = Quaternion.Euler(-70, 0, 0);
            Cyl("Fener", csi.HandR, new Vector3(0, -0.04f, 0.05f), new Vector3(0.06f, 0.1f, 0.06f), M.Metal, null, false);
            var fl = new GameObject("FenerIşığı"); fl.transform.SetParent(csi.HandR, false);
            fl.transform.localPosition = new Vector3(0, -0.14f, 0.1f);
            fl.transform.rotation = Quaternion.LookRotation(cs.position - fl.transform.position + Vector3.down * 0.5f);
            var spot = fl.AddComponent<Light>(); spot.type = LightType.Spot; spot.color = new Color(0.9f, 0.95f, 1f); spot.intensity = 8f; spot.range = 6f; spot.spotAngle = 35f;
            csi.Root.gameObject.AddComponent<FlashlightSweep>().arm = csi.ArmR;

            // Çakarlı polis aracı ve taksi (town_bg)
            TownProps.PoliceCar(t, CrimeScene + new Vector3(-6.5f, -0.07f, -2.5f), 60f, true);
            TownProps.Taxi(t, new Vector3(11.5f, 0.05f, -13.5f), -20f);
            _occupied.Add(CrimeScene + new Vector3(-6.5f, 0, -2.5f));

            // Şeridin arkasında şemsiyeli meraklı kalabalık (olay yerinin doğu tarafında, meydanı kapatmayacak mesafede)
            var crowd = new[] { new Vector3(10.2f, 0.04f, -4.2f), new Vector3(11.0f, 0.04f, -6.5f), new Vector3(10.5f, 0.04f, -9.2f), new Vector3(11.4f, 0.04f, -11.2f) };
            foreach (var p in crowd)
            {
                var person = Townsfolk.CreateStanding(t, p, Random.value > 0.4f);
                var look = CrimeScene - p; look.y = 0;
                person.transform.rotation = Quaternion.LookRotation(look);
            }
            HelperPosition = CrimeScene + new Vector3(-4.2f, -0.07f, -6.2f);
        }

        static void PoliceCap(PropFactory.PersonRig rig)
        {
            // Memur zaptiye kepi (fes kaldırıldı)
            Headwear.PoliceCap(rig.Head);
        }

        // =================================================================
        //  EVLER: çevre yolunun iki yanında cumbalı ve yarı ahşap evler
        // =================================================================
        void BuildHouses(Transform t)
        {
            var parent = Group("Evler", t, Vector3.zero).transform;
            // Meydan ile çevre yolu arası (iç halka) — binaların arasındaki boşluklar
            PlaceRing(parent, 31f, 26, 9f);
            // Çevre yolunun dışı
            PlaceRing(parent, 56f, 30, 8.5f);
            PlaceRing(parent, 70f, 24, 10f, northOnly: false, skipNorth: true);
        }

        void PlaceRing(Transform parent, float radius, int count, float minGap, bool northOnly = false, bool skipNorth = false)
        {
            for (int i = 0; i < count; i++)
            {
                float a = (i + Random.value * 0.5f) / count * Mathf.PI * 2f;
                var pos = new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a)) * (radius + Random.Range(-2.5f, 2.5f)) + new Vector3(0, 0, 4f);
                if (skipNorth && pos.z > 50f) continue;
                if (pos.z < -56f) continue;               // dere
                if (!Free(pos, minGap)) continue;
                var size = new Vector3(Random.Range(5.5f, 8f), Random.Range(4.8f, 6.8f), Random.Range(5.5f, 7f));
                // Ön cephe en yakın yola (merkeze) baksın
                float yaw = FacePlazaYaw(pos) + Random.Range(-10f, 10f);
                if (Random.value > 0.45f) TownProps.OttomanHouse(parent, pos, yaw, size, true);
                else TownProps.TimberHouse(parent, pos, yaw, size, true);
                _occupied.Add(pos);
                if (Random.value > 0.6f)
                {
                    // Bahçe çiti
                    var fwd = Quaternion.Euler(0, yaw, 0) * Vector3.back;
                    PropFactory.FencePost(parent, pos + fwd * (size.z / 2 + 1.6f), yaw, size.x + 1.5f);
                }
            }
        }

        // =================================================================
        //  ÇİFTLİKLER, CAMİ, GÖL
        // =================================================================
        void BuildFarmsAndLandmarks(Transform t)
        {
            var parent = Group("Çevre", t, Vector3.zero).transform;
            TownProps.Barn(parent, new Vector3(-24f, 0, 70f), 160f);
            TownProps.Barn(parent, new Vector3(22f, 0, 76f), -165f);
            _occupied.Add(new Vector3(-24f, 0, 70f)); _occupied.Add(new Vector3(22f, 0, 76f));
            _occupied.Add(new Vector3(-17f, 0, 72f)); _occupied.Add(new Vector3(28f, 0, 78f));
            // Tarla çitleri ve sürülmüş tarla şeritleri
            foreach (var cx in new[] { -26f, 24f })
            {
                for (int r = 0; r < 7; r++)
                    Box("TarlaSırası", parent, new Vector3(cx, 0.06f, 86f + r * 1.6f), new Vector3(16f, 0.12f, 0.6f), M.Get("field", new Color(0.18f, 0.14f, 0.1f), 0.3f, 0f, M.NoiseTexture), collider: false);
                PropFactory.FencePost(parent, new Vector3(cx, 0, 82f), 0, 18f);
                PropFactory.FencePost(parent, new Vector3(cx - 9f, 0, 90f), 90, 16f);
            }
            TownProps.Mosque(parent, new Vector3(44f, 0, 54f), -140f);
            _occupied.Add(new Vector3(44f, 0, 54f)); _occupied.Add(new Vector3(36f, 0, 46f)); _occupied.Add(new Vector3(52f, 0, 60f));

            // Göl (kasabanın kuzeyi — bölge haritası)
            var lake = M.Get("lake", new Color(0.04f, 0.07f, 0.1f), 0.97f, 0.3f);
            Cyl("Göl", parent, new Vector3(0, 0.02f, 135f), new Vector3(150f, 0.02f, 60f), lake, collider: false);
            Cyl("GölKıyısı", parent, new Vector3(0, 0.012f, 135f), new Vector3(158f, 0.012f, 68f), M.Get("shore", new Color(0.2f, 0.19f, 0.16f), 0.4f), collider: false);
            PropFactory.StreetLamp(parent, new Vector3(3.5f, 0, 100f), new Color(1f, 0.7f, 0.4f));
            Box("İskele", parent, new Vector3(0, 0.25f, 108f), new Vector3(2.5f, 0.15f, 9f), M.Wood(new Color(0.32f, 0.22f, 0.14f)));
        }

        // =================================================================
        //  DOĞA: çam ormanı, ağaçlar, sisli dağlar
        // =================================================================
        void BuildNature(Transform t)
        {
            var parent = Group("Doğa", t, Vector3.zero).transform;
            // Kasaba içi ağaçlar
            for (int i = 0; i < 40; i++)
            {
                float a = Random.value * Mathf.PI * 2f;
                var p = new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a)) * Random.Range(24f, 66f) + new Vector3(0, 0, 4f);
                if (p.z < -56f || !Free(p, 5f)) continue;
                if (Random.value > 0.5f) PropFactory.Tree(parent, p, Random.Range(0.9f, 1.3f));
                else PropFactory.PineTree(parent, p, Random.Range(1.1f, 1.6f));
                _occupied.Add(p);
            }
            // Dış çam ormanı (gölün etrafı hariç)
            for (int i = 0; i < 220; i++)
            {
                float a = Random.value * Mathf.PI * 2f;
                var p = new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a)) * Random.Range(84f, 150f);
                if (p.z > 85f && Mathf.Abs(p.x) < 80f && p.z < 170f) continue; // göl
                if (Mathf.Abs(p.x) < 6f && p.z < 0) continue;                   // güney yolu
                if (Mathf.Abs(p.z + 62f) < 5f) continue;                         // dere
                PropFactory.PineTree(parent, p, Random.Range(1.6f, 2.8f));
            }
            // Sisli dağlar (piramit kırma çatı mesh'i ile)
            var rock = M.Get("mountain", new Color(0.16f, 0.18f, 0.22f), 0.1f, 0f, M.NoiseTexture);
            var snow = M.Get("mountain_snow", new Color(0.55f, 0.58f, 0.64f), 0.2f);
            for (int i = 0; i < 16; i++)
            {
                float a = (i / 16f) * Mathf.PI * 2f + Random.value * 0.2f;
                var p = new Vector3(Mathf.Sin(a), -1f, Mathf.Cos(a)) * Random.Range(210f, 260f);
                float h = Random.Range(45f, 85f) * (p.z > 0 ? 1.25f : 0.9f);
                var w = Random.Range(90f, 140f);
                HipRoof("Dağ", parent, p, new Vector3(w, h, w * 0.8f), rock, Random.Range(0f, 0.3f), Random.Range(0, 360f), 0f);
                HipRoof("DağKarı", parent, p + Vector3.up * h * 0.72f, new Vector3(w * 0.28f, h * 0.28f, w * 0.8f * 0.28f), snow, 0f, Random.Range(0, 360f), 0f);
            }
        }

        void BuildBoundary(Transform t)
        {
            var parent = Group("Sınır", t, Vector3.zero).transform;
            void Wall(Vector3 c, Vector3 s)
            {
                var go = new GameObject("GörünmezDuvar"); go.transform.SetParent(parent, false);
                go.transform.localPosition = c; go.AddComponent<BoxCollider>().size = s;
            }
            Wall(new Vector3(-80, 5, 10), new Vector3(1, 10, 200));
            Wall(new Vector3(80, 5, 10), new Vector3(1, 10, 200));
            Wall(new Vector3(0, 5, 100), new Vector3(160, 10, 1));
            Wall(new Vector3(0, 5, -57.5f), new Vector3(160, 10, 1)); // dere kıyısı (köprüden geçilmez — kasaba sınırı)
        }

        /// <summary>Meydan kenarı + kuzey/güney yolu + çevre yolu boyunca dolaşan şemsiyeli kasabalılar.</summary>
        void SpawnWalkers(Transform t)
        {
            var parent = Group("Kasabalılar", t, Vector3.zero).transform;
            var plazaLoop = new List<Vector3>();
            for (int i = 0; i < 12; i++)
            {
                float a = i / 12f * Mathf.PI * 2f;
                plazaLoop.Add(new Vector3(Mathf.Sin(a) * 13.5f, 0.1f, Mathf.Cos(a) * 13.5f));
            }
            var northSouth = new List<Vector3> { new Vector3(-2, 0.1f, 70), new Vector3(-2, 0.1f, 40), new Vector3(-2, 0.1f, 18), new Vector3(-12, 0.1f, 8), new Vector3(-10, 0.1f, -10), new Vector3(-2, 0.1f, -20), new Vector3(-2, 0.1f, -50), new Vector3(2, 0.1f, -50), new Vector3(2, 0.1f, -20), new Vector3(12, 0.1f, 4), new Vector3(2, 0.1f, 18), new Vector3(2, 0.1f, 70) };
            for (int i = 0; i < 6; i++) Townsfolk.CreateWalker(parent, plazaLoop, i * 2);
            for (int i = 0; i < 4; i++) Townsfolk.CreateWalker(parent, northSouth, i * 3);
            if (WalkPoints.Count > 4) for (int i = 0; i < 5; i++) Townsfolk.CreateWalker(parent, WalkPoints, i * WalkPoints.Count / 5);
        }

        void ScatterPuddles(Transform t)
        {
            var parent = Group("SuBirikintileri", t, Vector3.zero).transform;
            for (int i = 0; i < 26; i++)
            {
                var p = Random.insideUnitCircle * (PlazaRadius - 1.5f);
                TownProps.Puddle(parent, new Vector3(p.x, 0, p.y), Random.Range(0.8f, 2.4f));
            }
            for (int i = 0; i < 14; i++) TownProps.Puddle(parent, new Vector3(Random.Range(-2f, 2f), 0, Random.Range(18f, 75f)), Random.Range(0.8f, 1.8f));
            for (int i = 0; i < 10; i++) TownProps.Puddle(parent, new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-18f, -54f)), Random.Range(0.8f, 1.8f));
        }

        // =================================================================
        //  KASABAYA İNİŞ: göl üstünden meydana sinematik kamera yolu
        // =================================================================
        void BuildIntroPath()
        {
            IntroPath.Clear(); IntroLook.Clear();
            IntroPath.Add(new Vector3(-20, 55, 150)); IntroLook.Add(new Vector3(0, 0, 40));
            IntroPath.Add(new Vector3(-6, 38, 95));   IntroLook.Add(new Vector3(0, 0, 10));
            IntroPath.Add(new Vector3(4, 22, 52));    IntroLook.Add(new Vector3(0, 2, 0));
            IntroPath.Add(new Vector3(-8, 10, 18));   IntroLook.Add(CrimeScene);
            IntroPath.Add(new Vector3(-6, 4.5f, -6)); IntroLook.Add(CrimeScene);
            var behind = PlayerSpawn - PlayerSpawnRotation * Vector3.forward * 4.2f + Vector3.up * 2.2f;
            IntroPath.Add(behind); IntroLook.Add(PlayerSpawn + Vector3.up * 1.6f + PlayerSpawnRotation * Vector3.forward * 3f);
        }
    }

    /// <summary>Olay yeri uzmanının fenerini yavaşça sağa sola gezdirmesi.</summary>
    public class FlashlightSweep : MonoBehaviour
    {
        public Transform arm;
        void Update() { if (arm != null) arm.localRotation = Quaternion.Euler(-70 + Mathf.Sin(Time.time * 0.7f) * 10f, Mathf.Sin(Time.time * 0.5f) * 25f, 0); }
    }
}
