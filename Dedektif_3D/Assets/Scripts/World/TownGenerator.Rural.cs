using System.Collections.Generic;
using System.Linq;
using Dedektif3D.Core;
using UnityEngine;
using static Dedektif3D.World.Shapes;
using M = Dedektif3D.World.MaterialLibrary;

namespace Dedektif3D.World
{
    /// <summary>
    /// Level 2 — Gölgeşehir (golge_sehir_map.png): çam ormanının ortasında, çapraz uzanan parke ana caddeli sisli köy.
    /// Level 3 — Sisören (sisoren_map_v5.jpg): dik yamaca üç teras halinde kurulmuş, kayalıklarla çevrili dağ kasabası.
    /// Bina konumları web haritasındaki yüzde konumlardan (style.left/top) türetilir.
    /// </summary>
    public partial class TownGenerator
    {
        // =================================================================
        //  ORTAK
        // =================================================================
        /// <summary>Ev yanında istif odun: üç sıra kütük (çarpışma tek kutu, kütükler süs).</summary>
        void Woodpile(Transform parent, Vector3 pos, float yaw)
        {
            var g = Group("OdunYığını", parent, pos, yaw).transform;
            var log = M.Wood(new Color(0.34f, 0.25f, 0.16f));
            Box("OdunÇarpışma", g, new Vector3(0, 0.45f, 0), new Vector3(1.9f, 0.9f, 0.7f), log).GetComponent<Renderer>().enabled = false;
            for (int row = 0; row < 3; row++)
                for (int i = 0; i < 6 - row; i++)
                    Cyl("Kütük", g, new Vector3(-0.8f + row * 0.15f + i * 0.31f, 0.16f + row * 0.27f, 0), new Vector3(0.28f, 0.34f, 0.28f), log, new Vector3(90, 0, 0), false);
        }

        void RuralGround(Transform t, Color grass, Color mud)
        {
            var g = Box("Zemin", t, new Vector3(0, -0.5f, 0), new Vector3(1400, 1, 1400), M.Get("grass_" + ColorUtility.ToHtmlStringRGB(grass), grass, 0.15f, 0f, M.NoiseTexture));
            SetTiling(g.GetComponent<Renderer>(), new Vector2(230, 230));
            var m = Cyl("Çamur", t, new Vector3(0, 0.008f, 0), new Vector3(150, 0.008f, 120), M.Get("mud_" + ColorUtility.ToHtmlStringRGB(mud), mud, 0.26f, 0f, M.NoiseTexture), collider: false);
            SetTiling(m.GetComponent<Renderer>(), new Vector2(30, 24));
        }

        void RuralBoundary(Transform t, float halfX, float zMin, float zMax)
        {
            void Wall(Vector3 c, Vector3 s)
            {
                var go = new GameObject("GörünmezDuvar"); go.transform.SetParent(t, false);
                go.transform.localPosition = c; go.AddComponent<BoxCollider>().size = s;
            }
            float cz = (zMin + zMax) / 2f, lz = zMax - zMin;
            Wall(new Vector3(-halfX, 10, cz), new Vector3(1, 30, lz));
            Wall(new Vector3(halfX, 10, cz), new Vector3(1, 30, lz));
            Wall(new Vector3(0, 10, zMax), new Vector3(halfX * 2, 30, 1));
            Wall(new Vector3(0, 10, zMin), new Vector3(halfX * 2, 30, 1));
        }

        /// <summary>Kırsal olay yeri: ceset örtüsü, delil numaraları, polis şeridi, yerde fenerler.</summary>
        void RuralCrimeScene(Transform t, Vector3 center, float yaw, string localHelper, string localHelperPortraitHint)
        {
            CrimeScene = center;
            var cs = Group("OlayYeri", t, center, yaw).transform;
            var sheet = M.Get("sheet", new Color(0.28f, 0.3f, 0.32f), 0.45f);
            Box("CesetÖrtüsü", cs, new Vector3(0, 0.12f, 0), new Vector3(0.75f, 0.24f, 1.85f), sheet, collider: false);
            Sphere("ÖrtüBaş", cs, new Vector3(0, 0.16f, 0.95f), new Vector3(0.38f, 0.26f, 0.36f), sheet);
            Box("ÖrtüKenar", cs, new Vector3(0, 0.02f, 0), new Vector3(1.15f, 0.03f, 2.3f), sheet, collider: false);
            Box("KanLekesi", cs, new Vector3(0.45f, 0.005f, 0.6f), new Vector3(0.9f, 0.006f, 0.6f), M.Get("blood_wet", new Color(0.35f, 0.02f, 0.03f), 0.9f), new Vector3(0, 30, 0), false);
            var cards = new[] { new Vector3(-0.9f, 0, 0.4f), new Vector3(1.0f, 0, -0.3f), new Vector3(0.3f, 0, 1.6f) };
            for (int i = 0; i < 3; i++)
            {
                var card = Group("DelilKartı" + (i + 1), cs, cards[i], Random.Range(-30f, 30f)).transform;
                Prism("Kart", card, Vector3.zero, new Vector3(0.22f, 0.2f, 0.16f), M.Emissive("evidencecard", new Color(1f, 0.82f, 0.1f), 0.4f), 0);
                Text3D("No", card, new Vector3(0, 0.08f, -0.06f), (i + 1).ToString(), Color.black, 0.02f);
            }
            var c = new[] { new Vector3(-2.6f, 0, -2.8f), new Vector3(2.6f, 0, -2.8f), new Vector3(2.6f, 0, 2.8f), new Vector3(-2.6f, 0, 2.8f) };
            for (int i = 0; i < 4; i++) PropFactory.PoliceTapeLine(cs, c[i], c[(i + 1) % 4]);
            var block = new GameObject("OlayYeriEngeli"); block.transform.SetParent(cs, false); block.layer = NpcBarrierLayer;
            block.transform.localPosition = new Vector3(0, 0.6f, 0);
            block.AddComponent<BoxCollider>().size = new Vector3(5.2f, 1.2f, 5.6f);
            foreach (var p in new[] { new Vector3(-2.9f, 0, -3f), new Vector3(2.9f, 0, 3f) })
            {
                Box("YerFeneri", cs, p + Vector3.up * 0.18f, new Vector3(0.22f, 0.36f, 0.22f), M.Emissive("lantern_rural", new Color(1f, 0.7f, 0.35f), 3f), collider: false);
                var l = PointLight("FenerIşığı", cs, p + Vector3.up * 0.5f, new Color(1f, 0.7f, 0.4f), 6f, 11f);
                l.gameObject.AddComponent<LightFlicker>().Init(l, 0.08f);
            }
            // Yerel rehber (Bekçi Rıfat / Tüccar İlyas) olay yerinde beklemez: GameManager onu yol arkadaşı olarak kurar
            HelperPosition = center + Quaternion.Euler(0, yaw, 0) * new Vector3(-3.8f, 0, -2.6f);
        }

        void SetIntro(Vector3 crime, Vector3 farStart)
        {
            IntroPath.Clear(); IntroLook.Clear();
            IntroPath.Add(farStart); IntroLook.Add(crime + new Vector3(0, 0, 10));
            IntroPath.Add(Vector3.Lerp(farStart, crime, 0.45f) + Vector3.up * 14f); IntroLook.Add(crime);
            IntroPath.Add(crime + new Vector3(-6, 9, -10)); IntroLook.Add(crime);
            IntroPath.Add(crime + new Vector3(-5, 3.5f, -6)); IntroLook.Add(crime);
            var behind = PlayerSpawn - PlayerSpawnRotation * Vector3.forward * 4.2f + Vector3.up * 2.2f;
            IntroPath.Add(behind); IntroLook.Add(PlayerSpawn + Vector3.up * 1.6f + PlayerSpawnRotation * Vector3.forward * 3f);
        }

        static float YawToward(Vector3 from, Vector3 to)
        {
            var d = to - from; d.y = 0;
            if (d.sqrMagnitude < 0.01f) return 0f;
            d.Normalize();
            return Mathf.Atan2(-d.x, -d.z) * Mathf.Rad2Deg; // yerel -Z (ön cephe) hedefe bakar
        }

        // =================================================================
        //  LEVEL 2 — GÖLGEŞEHİR
        // =================================================================
        void BuildGolgeSehir(Transform town, TownData data)
        {
            const float S = 1.1f;
            RuralGround(town, new Color(0.12f, 0.17f, 0.12f), new Color(0.17f, 0.15f, 0.12f));
            var cobble = M.Get("cobble_wet", new Color(0.42f, 0.42f, 0.45f), 0.72f, 0.05f, M.CobbleTexture);
            var dirt = M.Get("dirtroad", new Color(0.24f, 0.2f, 0.16f), 0.26f, 0f, M.NoiseTexture);

            // Çapraz parke ana cadde (haritada sol-ortadan sağ-alta)
            Vector3 A = new Vector3(-62f, 0, 18f), B = new Vector3(62f, 0, -36f);
            var street = new[] { A, Vector3.Lerp(A, B, 0.33f) + new Vector3(0, 0, 1.5f), Vector3.Lerp(A, B, 0.66f) - new Vector3(0, 0, 1.5f), B };
            Polyline(town, street, 7f, cobble, false, null);
            for (float k = 0; k <= 1f; k += 0.02f) _occupied.Add(Vector3.Lerp(A, B, k));
            Polyline(town, new[] { new Vector3(-2f, 0, -8f), new Vector3(2f, 0, 12f), new Vector3(10f, 0, 28f), new Vector3(24f, 0, 40f) }, 4f, dirt, false, null);
            Polyline(town, new[] { new Vector3(-30f, 0, 5f), new Vector3(-34f, 0, -12f), new Vector3(-30f, 0, -30f) }, 3.5f, dirt, false, null);

            Vector3 Nearest(Vector3 p)
            {
                var ab = B - A; float k = Mathf.Clamp01(Vector3.Dot(p - A, ab) / ab.sqrMagnitude);
                return A + ab * k;
            }

            var parent = Group("Binalar", town, Vector3.zero).transform;
            foreach (var def in data.Buildings)
            {
                def.Size = RuralBuildings.SizeFor(def.Key);
                var pos = new Vector3((def.MapCenter.x - 50f) * S, 0, (50f - def.MapCenter.y) * S);
                var near = Nearest(pos);
                var away = pos - near; away.y = 0;
                float need = def.Size.z / 2f + 5f;
                if (away.magnitude < need) pos = near + (away.sqrMagnitude > 0.01f ? away.normalized : Vector3.forward) * need;
                def.Position = pos;
                def.Yaw = YawToward(pos, Nearest(pos));
                var inst = RuralBuildings.Build(def, parent, pos, def.Yaw);
                if (def.Enterable) Buildings[def.NpcId] = inst; else DecorBuildings.Add(inst);
                _occupied.Add(pos);
                Woodpile(town, pos + Quaternion.Euler(0, def.Yaw, 0) * new Vector3(def.Size.x / 2f + 1.1f, 0, 0.5f), def.Yaw + 90f);
            }
            // Toprak yollar da boş kalsın: ikinci sıra evler üstlerine gelmez
            foreach (var path in new[] { (new Vector3(-2f, 0, -8f), new Vector3(24f, 0, 40f)), (new Vector3(-30f, 0, 5f), new Vector3(-30f, 0, -30f)) })
                for (float k = 0; k <= 1f; k += 0.08f) _occupied.Add(Vector3.Lerp(path.Item1, path.Item2, k));
            var fieldCenters = new[] { new Vector3(-36, 0, -40), new Vector3(40, 0, 36) };
            var crimeArea = new Vector3(-48f, 0, 4f);

            void DecorHouse(Vector3 p, float extraYaw)
            {
                var def = new BuildingDef { Key = "kasabali_evi", NpcId = 0, DisplayName = "", Style = BuildingStyle.Rural, Neon = new Color(1f, 0.72f, 0.42f) };
                def.Size = RuralBuildings.SizeFor(def.Key);
                float yaw = YawToward(p, Nearest(p)) + extraYaw;
                DecorBuildings.Add(RuralBuildings.Build(def, parent, p, yaw));
                _occupied.Add(p);
                Woodpile(town, p + Quaternion.Euler(0, yaw, 0) * new Vector3(def.Size.x / 2f + 1.1f, 0, 0.5f), yaw + 90f);
            }

            // Haritadaki isimsiz kütük evler
            var extra = new[] { new Vector3(30, 0, 22), new Vector3(42, 0, 14), new Vector3(52, 0, 4), new Vector3(-52, 0, -4), new Vector3(-8, 0, -34), new Vector3(14, 0, -44), new Vector3(-24, 0, 32) };
            int e = 0;
            foreach (var p in extra)
                if (Free(p, 9f)) DecorHouse(p, e++ % 2 == 0 ? 0 : 15);

            // Haritadaki sık doku: caddenin iki yanında iki sıra ev. Olay yeri, tarlalar ve kasaba sınırı boş kalır.
            var roadSide = Vector3.Cross(Vector3.up, (B - A).normalized);
            int added = 0;
            for (int k = 0; k < 11 && added < 12; k++)
                foreach (var side in new[] { -1f, 1f })
                    foreach (var row in new[] { 14f, 27f })
                    {
                        var p = Vector3.Lerp(A, B, 0.06f + k * 0.085f) + roadSide * side * (row + (k % 3) * 1.5f);
                        if (added >= 12 || Mathf.Abs(p.x) > 62f || p.z < -50f || p.z > 42f) continue;
                        if (Vector3.Distance(p, crimeArea) < 20f || Vector3.Distance(p, fieldCenters[0]) < 17f || Vector3.Distance(p, fieldCenters[1]) < 17f) continue;
                        if (!Free(p, 9.5f)) continue;
                        DecorHouse(p, k % 2 == 0 ? 0 : -12);
                        added++;
                    }

            // Tarlalar (güneybatı + kuzeydoğu) ve çitler
            foreach (var f in new[] { (c: new Vector3(-36, 0, -40), yaw: 20f), (c: new Vector3(40, 0, 36), yaw: -15f) })
            {
                var g = Group("Tarla", town, f.c, f.yaw).transform;
                for (int r = 0; r < 9; r++)
                    Box("TarlaSırası", g, new Vector3(0, 0.08f, -7f + r * 1.7f), new Vector3(22f, 0.16f, 0.7f), M.Get("field", new Color(0.18f, 0.14f, 0.1f), 0.3f, 0f, M.NoiseTexture), collider: false);
                PropFactory.FencePost(g, new Vector3(0, 0, -8.5f), 0, 23f);
                PropFactory.FencePost(g, new Vector3(-11.5f, 0, 0), 90, 17f);
                _occupied.Add(f.c);
            }
            RuralBuildings.Cart(town, new Vector3(-18, 0, -22), 70f);

            // Cadde fenerleri
            for (int i = 1; i < 9; i++)
            {
                var p = Vector3.Lerp(A, B, i / 9f);
                var side = Vector3.Cross(Vector3.up, (B - A).normalized) * (i % 2 == 0 ? 4.6f : -4.6f);
                PropFactory.StreetLamp(town, p + side, new Color(1f, 0.7f, 0.4f));
                WalkPoints.Add(p + side * 0.5f + Vector3.up * 0.05f);
            }

            // Olay yeri: çam ormanı girişi (hikaye: "çam ormanı girişinde bir ceset bulundu")
            var crime = new Vector3(-48f, 0.05f, 4f);
            RuralCrimeScene(town, crime, 15f, data.LocalHelperName ?? "Bekçi Rıfat", null);
            PlayerSpawn = new Vector3(-38f, 0.1f, 9.5f);
            PlayerSpawnRotation = Quaternion.LookRotation(crime - PlayerSpawn);

            // Yoğun çam ormanı (haritanın kuzeyi ve kenarları)
            var forest = Group("ÇamOrmanı", town, Vector3.zero).transform;
            for (int i = 0; i < 420; i++)
            {
                var p = new Vector3(Random.Range(-140f, 140f), 0, Random.Range(-120f, 140f));
                float r = new Vector2(p.x, p.z * 1.2f).magnitude;
                bool north = p.z > 34f && p.x < 20f;
                if (r < 62f && !north) continue;
                if (!Free(p, 5f)) continue;
                PropFactory.PineTree(forest, p, Random.Range(1.5f, 2.9f));
            }
            for (int i = 0; i < 25; i++)
            {
                var p = new Vector3(Random.Range(-55f, 55f), 0, Random.Range(-45f, 30f));
                if (!Free(p, 8f)) continue;
                PropFactory.Tree(forest, p, Random.Range(0.9f, 1.3f));
                _occupied.Add(p);
            }
            Mountains(town, new Color(0.13f, 0.16f, 0.15f), 230f, 280f, 40f, 70f);
            for (int i = 0; i < 18; i++) TownProps.Puddle(town, Vector3.Lerp(A, B, Random.value) + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f)), Random.Range(0.8f, 2f));
            SpawnRuralWalkers(town, WalkPoints, 6);
            Effects.GroundFog(town, new Vector3(0, 0.6f, 0), new Vector3(110, 0.5f, 90), 70);
            RuralBoundary(town, 70f, -58f, 50f);
            SetIntro(crime, new Vector3(55f, 42f, -85f));
        }

        // =================================================================
        //  LEVEL 3 — SİSÖREN
        // =================================================================
        void BuildSisoren(Transform town, TownData data)
        {
            const float SX = 1.15f;
            const float Mid = 4.5f, Up = 9f;
            RuralGround(town, new Color(0.13f, 0.18f, 0.15f), new Color(0.18f, 0.16f, 0.13f));
            var rock = M.Get("cliff_rock", new Color(0.24f, 0.27f, 0.25f), 0.15f, 0f, M.NoiseTexture);
            var dirt = M.Get("dirtroad", new Color(0.24f, 0.2f, 0.16f), 0.26f, 0f, M.NoiseTexture);

            // Haritadaki üç sıra → üç teras seviyesi
            float LevelOf(Vector2 mc) => mc.y < 32f ? Up : mc.y < 60f ? Mid : 0f;
            foreach (var def in data.Buildings)
            {
                def.Size = RuralBuildings.SizeFor(def.Key);
                float y = LevelOf(def.MapCenter);
                float z = (50f - def.MapCenter.y) * 1.0f;
                if (y == Up) z = Mathf.Max(z, 24.5f); // kapı önü (DoorOutside) teras kenarının içinde kalsın else if (y == Mid) z = Mathf.Clamp(z, -4f, 12f); else z = Mathf.Min(z, -18f);
                def.Position = new Vector3((def.MapCenter.x - 50f) * SX, y, z);
            }
            // Yamaç aralarını aç: aynı seviyede üst üste binen binaları yana kaydır
            foreach (var lvl in new[] { 0f, Mid, Up })
            {
                var row = data.Buildings.Where(b => Mathf.Approximately(b.Position.y, lvl)).OrderBy(b => b.Position.x).ToList();
                for (int i = 1; i < row.Count; i++)
                {
                    float minGap = (row[i - 1].Size.x + row[i].Size.x) / 2f + 3f;
                    if (row[i].Position.x - row[i - 1].Position.x < minGap && Mathf.Abs(row[i].Position.z - row[i - 1].Position.z) < 9f)
                        row[i].Position = new Vector3(row[i - 1].Position.x + minGap, lvl, row[i].Position.z);
                }
            }

            float RampX(float lvl, float zMin, float zMax)
            {
                float best = 0f, bestScore = -1f;
                for (float x = -30f; x <= 30f; x += 1f)
                {
                    float score = 999f;
                    foreach (var b in data.Buildings)
                    {
                        if (b.Position.z < zMin - 6f || b.Position.z > zMax + 6f) continue;
                        score = Mathf.Min(score, Mathf.Abs(b.Position.x - x) - b.Size.x / 2f);
                    }
                    score -= Mathf.Abs(x) * 0.05f;
                    if (score > bestScore) { bestScore = score; best = x; }
                }
                return best;
            }
            float midFront = -12f, upFront = 17f, slope = 21f;
            float L1 = Mid / Mathf.Tan(slope * Mathf.Deg2Rad), L2 = (Up - Mid) / Mathf.Tan(slope * Mathf.Deg2Rad);
            float rampLow = RampX(0f, midFront - L1, midFront);
            float rampHigh = RampX(Mid, upFront - L2, upFront);

            void Platform(string name, float top, float zMin, float zMax, float extraX)
            {
                var bs = data.Buildings.Where(b => b.Position.y >= top - 0.01f).ToList();
                float minX = Mathf.Min(bs.Count > 0 ? bs.Min(b => b.Position.x - b.Size.x / 2f) : -20f, extraX) - 8f;
                float maxX = Mathf.Max(bs.Count > 0 ? bs.Max(b => b.Position.x + b.Size.x / 2f) : 20f, extraX) + 8f;
                var p = Box(name, town, new Vector3((minX + maxX) / 2f, top / 2f, (zMin + zMax) / 2f), new Vector3(maxX - minX, top, zMax - zMin), rock, texTiling: 0.15f);
                Box(name + "Toprak", town, new Vector3((minX + maxX) / 2f, top + 0.02f, (zMin + zMax) / 2f), new Vector3(maxX - minX - 0.4f, 0.04f, zMax - zMin - 0.4f), M.Get("terrace_grass", new Color(0.16f, 0.2f, 0.15f), 0.15f, 0f, M.NoiseTexture), collider: false, texTiling: 0.3f);
                // Uçurum kenarı kayaları + korkuluk
                for (float x = minX + 1f; x < maxX; x += Random.Range(2.5f, 4.5f))
                {
                    Sphere("Kaya", town, new Vector3(x, Random.Range(0.3f, top * 0.4f), zMin - Random.Range(0.4f, 1.4f)), new Vector3(Random.Range(1.5f, 3.5f), Random.Range(1.2f, 2.8f), Random.Range(1.2f, 2.4f)), rock, true);
                    if (Mathf.Abs(x - (top < Up ? rampLow : rampHigh)) > 4f && Random.value > 0.25f)
                        Box("Korkuluk", town, new Vector3(x, top + 0.5f, zMin + 0.3f), new Vector3(2.6f, 0.08f, 0.08f), M.Wood(new Color(0.3f, 0.2f, 0.12f)), collider: false);
                }
            }
            Platform("OrtaTeras", Mid, midFront, 60f, rampLow);
            Platform("ÜstTeras", Up, upFront, 60f, rampHigh);

            void Ramp(float x, float y0, float y1, float zEnd, float len)
            {
                float rise = y1 - y0; float hyp = Mathf.Sqrt(len * len + rise * rise);
                float ang = Mathf.Atan2(rise, len) * Mathf.Rad2Deg;
                var r = Box("Rampa", town, new Vector3(x, y0 + rise / 2f - 0.15f, zEnd - len / 2f), new Vector3(5f, 0.4f, hyp), dirt, new Vector3(-ang, 0, 0));
                SetTiling(r.GetComponent<Renderer>(), new Vector2(1.3f, hyp / 4f));
                foreach (var sx in new[] { -2.6f, 2.6f })
                    Box("RampaKorkuluk", town, new Vector3(x + sx, y0 + rise / 2f + 0.55f, zEnd - len / 2f), new Vector3(0.08f, 0.08f, hyp), M.Wood(new Color(0.3f, 0.2f, 0.12f)), new Vector3(-ang, 0, 0), false);
            }
            Ramp(rampLow, 0f, Mid, midFront, L1);
            Ramp(rampHigh, Mid, Up, upFront, L2);

            // Binalar
            var parent = Group("Binalar", town, Vector3.zero).transform;
            foreach (var def in data.Buildings)
            {
                def.Yaw = Random.Range(-8f, 8f);
                var inst = RuralBuildings.Build(def, parent, def.Position, def.Yaw);
                if (def.Enterable) Buildings[def.NpcId] = inst; else DecorBuildings.Add(inst);
                _occupied.Add(def.Position);
            }

            // Üst terasta telgraf direkleri ve teller
            var poles = new List<Vector3>();
            for (float x = -20f; x <= 40f; x += 10f) poles.Add(new Vector3(x, Up, upFront + 2.5f));
            for (int i = 0; i < poles.Count; i++)
            {
                var p = poles[i];
                Cyl("TelgrafDireği", town, p + Vector3.up * 3.5f, new Vector3(0.18f, 3.5f, 0.18f), M.Wood(new Color(0.25f, 0.18f, 0.12f)));
                Box("Travers", town, p + Vector3.up * 6.6f, new Vector3(1.4f, 0.1f, 0.1f), M.Wood(new Color(0.25f, 0.18f, 0.12f)), collider: false);
                if (i > 0)
                    foreach (var dx in new[] { -0.6f, 0.6f })
                    {
                        var a = poles[i - 1] + new Vector3(dx, 6.65f, 0); var b = p + new Vector3(dx, 6.65f, 0);
                        Box("Tel", town, (a + b) / 2f - Vector3.up * 0.25f, new Vector3(Vector3.Distance(a, b), 0.02f, 0.02f), Iron(), collider: false);
                    }
            }

            // Fenerler (haritadaki yeşilimsi lambalar) + yürüyüş güzergahları
            var lampCol = new Color(0.75f, 1f, 0.62f);
            var lower = new List<Vector3>(); var middle = new List<Vector3>();
            for (float x = -40f; x <= 40f; x += 13f)
            {
                PropFactory.StreetLamp(town, new Vector3(x, 0, -14f), lampCol, ((int)x / 13) % 2 == 0);
                PropFactory.StreetLamp(town, new Vector3(x + 6f, Mid, midFront + 1.5f), lampCol, ((int)x / 13) % 2 != 0);
                lower.Add(new Vector3(x, 0.1f, -12f + Mathf.Sin(x) * 2f));
                middle.Add(new Vector3(x * 0.8f, Mid + 0.1f, midFront + 4f));
            }
            for (int i = middle.Count - 1; i >= 0; i--) middle.Add(middle[i] + new Vector3(0, 0, 3f));
            for (int i = lower.Count - 1; i >= 0; i--) lower.Add(lower[i] + new Vector3(0, 0, -6f));
            WalkPoints.AddRange(lower);
            Polyline(town, new[] { new Vector3(-50, 0, -15), new Vector3(0, 0, -13), new Vector3(50, 0, -15) }, 5f, dirt, false, null);
            Polyline(town, new[] { new Vector3(rampLow, 0, midFront - L1), new Vector3(rampLow - 2f, 0, -30f), new Vector3(0, 0, -55f) }, 4f, dirt, false, null);

            // Olay yeri: orta terasta muhtarlık önü
            var muh = data.Buildings.Find(b => b.Key == "muhtarlik") ?? data.Buildings[0];
            var crime = new Vector3(Mathf.Clamp(muh.Position.x - 6f, -25f, 25f), Mid + 0.05f, midFront + 6f);
            RuralCrimeScene(town, crime, -10f, data.LocalHelperName ?? "Tüccar İlyas", null);
            PlayerSpawn = new Vector3(rampLow, 0.1f, midFront - L1 - 6f);
            PlayerSpawnRotation = Quaternion.LookRotation(new Vector3(0, 0, 1));

            // Arka plan: sarp kayalıklar, yamaçta çamlar, sisli dağlar
            for (int i = 0; i < 14; i++)
            {
                float x = -70f + i * 11f + Random.Range(-3f, 3f);
                HipRoof("Kayalık", town, new Vector3(x, 0, 66f + Random.Range(0f, 14f)), new Vector3(Random.Range(28f, 40f), Random.Range(16f, 30f), Random.Range(22f, 30f)), rock, Random.Range(0.25f, 0.55f), Random.Range(-20f, 20f), 0f);
            }
            foreach (var sx in new[] { -1f, 1f })
                for (int i = 0; i < 6; i++)
                    HipRoof("YanKayalık", town, new Vector3(sx * Random.Range(62f, 75f), 0, -40f + i * 18f), new Vector3(26f, Random.Range(14f, 26f), 26f), rock, 0.45f, Random.Range(0f, 90f), 0f);
            var forest = Group("Yamaç", town, Vector3.zero).transform;
            for (int i = 0; i < 260; i++)
            {
                var p = new Vector3(Random.Range(-150f, 150f), 0, Random.Range(-130f, 150f));
                if (Mathf.Abs(p.x) < 60f && p.z > -50f && p.z < 64f) continue;
                PropFactory.PineTree(forest, p, Random.Range(1.6f, 3f));
            }
            for (int i = 0; i < 30; i++)
            {
                var p = new Vector3(Random.Range(-55f, 55f), Up, Random.Range(upFront + 6f, 58f));
                if (!Free(p, 6f)) continue;
                PropFactory.PineTree(forest, p, Random.Range(1.2f, 2f));
            }
            Mountains(town, new Color(0.15f, 0.19f, 0.18f), 180f, 240f, 70f, 120f);
            SpawnRuralWalkers(town, lower, 5);
            SpawnRuralWalkers(town, middle, 4);
            Effects.GroundFog(town, new Vector3(0, 2f, 10f), new Vector3(110, 6f, 90), 80);
            RuralBoundary(town, 64f, -58f, 62f);
            SetIntro(crime, new Vector3(-10f, 30f, -110f));
        }

        static Material Iron() => M.Get("iron", new Color(0.08f, 0.08f, 0.09f), 0.5f, 0.7f);

        void Mountains(Transform t, Color col, float rMin, float rMax, float hMin, float hMax)
        {
            var rock = M.Get("mountain_" + ColorUtility.ToHtmlStringRGB(col), col, 0.1f, 0f, M.NoiseTexture);
            for (int i = 0; i < 16; i++)
            {
                float a = (i / 16f) * Mathf.PI * 2f + Random.value * 0.2f;
                var p = new Vector3(Mathf.Sin(a), -1f, Mathf.Cos(a)) * Random.Range(rMin, rMax);
                float w = Random.Range(90f, 140f);
                HipRoof("Dağ", t, p, new Vector3(w, Random.Range(hMin, hMax), w * 0.8f), rock, Random.Range(0f, 0.3f), Random.Range(0, 360f), 0f);
            }
        }

        void SpawnRuralWalkers(Transform t, List<Vector3> route, int count)
        {
            if (route == null || route.Count < 2) return;
            var parent = Group("Kasabalılar", t, Vector3.zero).transform;
            for (int i = 0; i < count; i++) Townsfolk.CreateWalker(parent, route, i * route.Count / count);
        }
    }
}
