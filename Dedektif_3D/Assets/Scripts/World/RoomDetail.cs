using System.Collections.Generic;
using Dedektif3D.Core;
using UnityEngine;
using static Dedektif3D.World.Shapes;
using M = Dedektif3D.World.MaterialLibrary;

namespace Dedektif3D.World
{
    /// <summary>
    /// İç mekânların web görsellerine (interiors/*.png, binalar/*) göre dolu hâli: tavana kadar raflar, kavanozlar, karkaslar,
    /// kumaş topları, harita, sütun. Görseller 1887'ye uyarlanır: neon yerine boyalı tabela, ampul yerine gaz lambası.
    /// Random kullanmaz (kasaba üretiminin tohumlu sırası değişmez). Küçük eşyalar çarpışmasızdır ve odada tek mesh'e birleştirilir.
    /// Delil konumları ve yürüme yolu boş bırakılır: orta koridor (|x| &lt; 2) ve tezgâh üstü (|x| &lt; 1.3).
    /// </summary>
    public static class RoomDetail
    {
        const float W = InteriorGenerator.W, D = InteriorGenerator.D, H = InteriorGenerator.H;

        static Material DarkOak => M.Wood(new Color(0.17f, 0.105f, 0.06f));
        static Material MidOak => M.Wood(new Color(0.3f, 0.19f, 0.11f));
        static Material Brass => M.Get("brass", new Color(0.75f, 0.6f, 0.25f), 0.8f, 0.9f);
        static Material Iron => M.Get("detail_iron", new Color(0.07f, 0.07f, 0.08f), 0.5f, 0.8f);
        static Material Steel => M.Get("detail_steel", new Color(0.55f, 0.56f, 0.58f), 0.75f, 0.9f);
        static Material Paper => M.Get("detail_paper", new Color(0.84f, 0.8f, 0.68f), 0.05f);
        static Material Cork => M.Get("detail_cork", new Color(0.55f, 0.42f, 0.27f), 0.05f);
        static Material Leather => M.Get("detail_leather", new Color(0.24f, 0.11f, 0.06f), 0.45f);

        static readonly Color[] JarColors =
        {
            new Color(0.34f, 0.17f, 0.05f), new Color(0.1f, 0.26f, 0.15f), new Color(0.58f, 0.6f, 0.56f),
            new Color(0.09f, 0.14f, 0.34f), new Color(0.72f, 0.68f, 0.58f), new Color(0.42f, 0.26f, 0.1f),
        };
        static readonly Color[] BookColors =
        {
            new Color(0.27f, 0.07f, 0.05f), new Color(0.09f, 0.16f, 0.11f), new Color(0.1f, 0.12f, 0.22f), new Color(0.25f, 0.16f, 0.08f),
            new Color(0.34f, 0.24f, 0.12f), new Color(0.14f, 0.09f, 0.06f), new Color(0.3f, 0.26f, 0.2f), new Color(0.2f, 0.05f, 0.09f),
        };
        static readonly Color[] FabricColors =
        {
            new Color(0.09f, 0.13f, 0.27f), new Color(0.32f, 0.06f, 0.08f), new Color(0.22f, 0.22f, 0.24f), new Color(0.3f, 0.25f, 0.17f),
            new Color(0.08f, 0.2f, 0.15f), new Color(0.42f, 0.36f, 0.27f), new Color(0.12f, 0.1f, 0.1f), new Color(0.38f, 0.22f, 0.1f),
        };

        /// <summary>Tohumsuz, tekrar edilebilir 0-1 değer.</summary>
        static float R(int a, int b = 0)
        {
            float v = Mathf.Sin(a * 12.9898f + b * 78.233f + 1.7f) * 43758.5453f;
            return v - Mathf.Floor(v);
        }

        static GameObject B(Transform p, Vector3 pos, Vector3 s, Material m, Vector3? e = null) => Box("Ayrıntı", p, pos, s, m, e, false);
        static GameObject C(Transform p, Vector3 pos, Vector3 s, Material m, Vector3? e = null) => Cyl("Ayrıntı", p, pos, s, m, e, false);
        static GameObject S(Transform p, Vector3 pos, Vector3 s, Material m) => Sphere("Ayrıntı", p, pos, s, m);
        static GameObject K(Transform p, Vector3 pos, Vector3 s, Material m) => Capsule("Ayrıntı", p, pos, s, m);

        static Material Tint(string key, Color[] colors, int i, float smooth = 0.3f)
        {
            int k = Mathf.Abs(i) % colors.Length;
            return M.Get(key + "_" + k, colors[k], smooth);
        }

        // =================================================================
        //  ZEMİN VE DUVAR KAPLAMASI
        // =================================================================
        public static Material TileFloor(Color tint) => M.Get("tile_floor_" + ColorUtility.ToHtmlStringRGB(tint), tint, 0.45f, 0f, ProceduralTextures.Tile);

        /// <summary>Sol, sağ ve arka duvarın alt kısmına fayans.</summary>
        static void TileBand(Transform det, Color tint, float height)
        {
            var mat = M.Get("tile_wall_" + ColorUtility.ToHtmlStringRGB(tint), tint, 0.62f, 0f, ProceduralTextures.Tile);
            void Panel(Vector3 pos, Vector3 size, float length)
            {
                var go = B(det, pos, size, mat);
                SetTiling(go.GetComponent<Renderer>(), new Vector2(length / 0.6f, height / 0.6f));
            }
            Panel(new Vector3(0, height / 2, D / 2 - 0.015f), new Vector3(W, height, 0.03f), W);
            Panel(new Vector3(-W / 2 + 0.015f, height / 2, 0), new Vector3(0.03f, height, D), D);
            Panel(new Vector3(W / 2 - 0.015f, height / 2, 0), new Vector3(0.03f, height, D), D);
        }

        /// <summary>Sol ve sağ duvarın altına koyu ahşap lambri.</summary>
        static void Wainscot(Transform det, float height)
        {
            B(det, new Vector3(-W / 2 + 0.02f, height / 2, 0), new Vector3(0.04f, height, D), DarkOak);
            B(det, new Vector3(W / 2 - 0.02f, height / 2, 0), new Vector3(0.04f, height, D), DarkOak);
            B(det, new Vector3(-W / 2 + 0.05f, height + 0.02f, 0), new Vector3(0.08f, 0.05f, D), MidOak);
            B(det, new Vector3(W / 2 - 0.05f, height + 0.02f, 0), new Vector3(0.08f, 0.05f, D), MidOak);
        }

        // =================================================================
        //  RAFLAR
        // =================================================================
        /// <summary>
        /// Duvara dayalı açık raf: yerel +Z odaya bakar (sol duvar yaw 90, sağ duvar -90, arka duvar 180).
        /// skip: odadaki konumu verilen eşya atlanır (raftaki delilin yeri boş kalsın diye).
        /// </summary>
        static void Shelves(Transform det, Vector3 pos, float yaw, float width, float y0, float step, int levels, string goods, Material wood, int seed = 0, System.Func<Vector3, bool> skip = null)
        {
            var g = Group("Raf", det, pos, yaw).transform;
            var rot = Quaternion.Euler(0, yaw, 0);
            float top = y0 + step * (levels - 1);
            B(g, new Vector3(0, (top + 0.04f) / 2f, 0.015f), new Vector3(width, top + 0.04f, 0.03f), wood);
            foreach (var sx in new[] { -1f, 1f })
                B(g, new Vector3(sx * width / 2f, (top + 0.04f) / 2f, 0.21f), new Vector3(0.05f, top + 0.04f, 0.42f), wood);
            for (int level = 0; level < levels; level++)
            {
                float y = y0 + level * step;
                B(g, new Vector3(0, y, 0.21f), new Vector3(width, 0.035f, 0.42f), wood);
                if (level == levels - 1) break;
                bool Skipped(float x) => skip != null && skip(pos + rot * new Vector3(x, y + 0.15f, 0.2f));
                Fill(g, goods, width, y, step, seed + level * 31, Skipped);
            }
        }

        static void Fill(Transform g, string goods, float width, float y, float step, int seed, System.Func<float, bool> skipped)
        {
            float x = -width / 2f + 0.1f, end = width / 2f - 0.1f;
            float room = step - 0.08f;
            int n = 0;
            while (x < end)
            {
                float a = R(seed, n), b = R(seed + 7, n), c = R(seed + 13, n);
                float advance;
                bool skip = skipped(x);
                switch (goods)
                {
                    case "books":
                    {
                        float w = 0.035f + 0.05f * a, h = Mathf.Min(room, 0.2f + 0.16f * b);
                        if (!skip && n % 17 != 11) B(g, new Vector3(x, y + 0.02f + h / 2f, 0.2f), new Vector3(w, h, 0.24f), Tint("detail_book", BookColors, (int)(c * 8f), 0.25f));
                        advance = w + 0.004f;
                        break;
                    }
                    case "boots":
                    {
                        var leather = M.Get("detail_boot_" + (int)(a * 3f), Color.Lerp(new Color(0.09f, 0.06f, 0.05f), new Color(0.27f, 0.15f, 0.07f), (int)(a * 3f) / 2f), 0.55f);
                        if (!skip)
                            foreach (var side in new[] { -0.06f, 0.06f })
                            {
                                B(g, new Vector3(x + side, y + 0.02f + Mathf.Min(room, 0.22f) / 2f, 0.16f), new Vector3(0.085f, Mathf.Min(room, 0.22f), 0.1f), leather);
                                B(g, new Vector3(x + side, y + 0.055f, 0.24f), new Vector3(0.085f, 0.07f, 0.2f), leather);
                            }
                        advance = 0.36f;
                        break;
                    }
                    case "folded":
                    {
                        int layers = 3 + (int)(a * 4f);
                        if (!skip)
                            for (int l = 0; l < layers && (l + 1) * 0.05f < room; l++)
                                B(g, new Vector3(x + 0.2f, y + 0.042f + l * 0.05f, 0.21f), new Vector3(0.38f, 0.045f, 0.3f), Tailoring.Cloth(FabricColors[(seed + n * 3 + l) & 7], 0.1f));
                        advance = 0.44f;
                        break;
                    }
                    case "boxes":
                    {
                        float w = 0.26f + 0.1f * a, h = Mathf.Min(room, 0.18f + 0.1f * b);
                        if (!skip)
                        {
                            B(g, new Vector3(x + w / 2f, y + 0.02f + h / 2f, 0.2f), new Vector3(w, h, 0.28f), M.Get("detail_box_" + (int)(c * 3f), Color.Lerp(new Color(0.42f, 0.32f, 0.2f), new Color(0.24f, 0.2f, 0.16f), (int)(c * 3f) / 2f), 0.08f));
                            B(g, new Vector3(x + w / 2f, y + 0.02f + h * 0.6f, 0.345f), new Vector3(w * 0.5f, h * 0.3f, 0.004f), Paper);
                        }
                        advance = w + 0.03f;
                        break;
                    }
                    case "tins":
                    {
                        float h = Mathf.Min(room, 0.16f + 0.08f * b);
                        if (!skip)
                        {
                            C(g, new Vector3(x, y + 0.02f + h / 2f, 0.2f), new Vector3(0.13f, h / 2f, 0.13f), M.Get("detail_tin_" + (int)(a * 3f), Color.Lerp(new Color(0.35f, 0.33f, 0.28f), new Color(0.2f, 0.16f, 0.12f), (int)(a * 3f) / 2f), 0.5f, 0.6f));
                            if (c < 0.6f) B(g, new Vector3(x, y + 0.02f + h / 2f, 0.267f), new Vector3(0.08f, h * 0.5f, 0.004f), Paper);
                        }
                        advance = 0.17f;
                        break;
                    }
                    default:   // jars
                    {
                        float r = 0.09f + 0.05f * a, h = Mathf.Min(room - 0.03f, 0.14f + 0.16f * b);
                        float z = 0.16f + c * 0.1f;
                        if (!skip)
                        {
                            C(g, new Vector3(x, y + 0.02f + h / 2f, z), new Vector3(r, h / 2f, r), Tint("detail_jar", JarColors, (int)(c * 6f) + n, 0.85f));
                            C(g, new Vector3(x, y + 0.03f + h, z), new Vector3(r * 0.72f, 0.014f, r * 0.72f), Cork);
                            if (a < 0.45f) B(g, new Vector3(x, y + 0.02f + h * 0.5f, z + r * 0.5f + 0.002f), new Vector3(r * 0.62f, h * 0.4f, 0.004f), Paper);
                        }
                        advance = r + 0.035f;
                        break;
                    }
                }
                x += advance;
                n++;
            }
        }

        // =================================================================
        //  EŞYALAR
        // =================================================================
        static void BalanceScale(Transform p, Vector3 pos)
        {
            var g = Group("Terazi", p, pos).transform;
            C(g, new Vector3(0, 0.015f, 0), new Vector3(0.2f, 0.015f, 0.2f), DarkOak);
            C(g, new Vector3(0, 0.2f, 0), new Vector3(0.022f, 0.18f, 0.022f), Brass);
            B(g, new Vector3(0, 0.37f, 0), new Vector3(0.42f, 0.012f, 0.018f), Brass);
            foreach (var side in new[] { -0.2f, 0.2f })
            {
                C(g, new Vector3(side, 0.26f, 0), new Vector3(0.004f, 0.1f, 0.004f), Brass);
                C(g, new Vector3(side, 0.155f, 0), new Vector3(0.15f, 0.008f, 0.15f), Brass);
            }
        }

        static void Mortar(Transform p, Vector3 pos)
        {
            var stone = M.Get("detail_marble", new Color(0.66f, 0.64f, 0.6f), 0.4f);
            C(p, pos + Vector3.up * 0.05f, new Vector3(0.15f, 0.05f, 0.15f), stone);
            C(p, pos + new Vector3(0.03f, 0.14f, 0), new Vector3(0.024f, 0.09f, 0.024f), stone, new Vector3(0, 0, -25));
        }

        static void HerbRow(Transform det, float z, float x0, float x1, int seed)
        {
            int i = 0;
            for (float x = x0; x <= x1; x += 0.38f, i++)
            {
                float drop = 0.24f + R(seed, i) * 0.14f;
                var c = Color.Lerp(new Color(0.2f, 0.25f, 0.1f), new Color(0.36f, 0.27f, 0.12f), R(seed + 3, i));
                C(det, new Vector3(x, H - 0.25f - drop / 2f, z), new Vector3(0.008f, drop / 2f, 0.008f), Iron);
                K(det, new Vector3(x, H - 0.46f - drop, z), new Vector3(0.16f, 0.24f, 0.12f), M.Get("detail_herb_" + (i % 3) + "_" + seed, c, 0.05f));
            }
        }

        /// <summary>Çengelde asılı yarım karkas (kasap görselinden).</summary>
        public static void Carcass(Transform p, Vector3 hook, float scale = 1f, float yaw = 0f)
        {
            var meat = M.Get("detail_meat", new Color(0.34f, 0.075f, 0.065f), 0.45f);
            var fat = M.Get("detail_fat", new Color(0.74f, 0.64f, 0.52f), 0.45f);
            var g = Group("Karkas", p, hook, yaw).transform;
            C(g, new Vector3(0, -0.14f, 0), new Vector3(0.022f, 0.14f, 0.022f), Steel);
            // Geniş yarım gövde, üstte incelen but, iki yanda yağ tabakası, ortada kaburgalar (yerel -Z odaya bakar)
            K(g, new Vector3(0, -1.08f * scale, 0), new Vector3(0.66f, 0.62f, 0.2f) * scale, meat);
            K(g, new Vector3(0, -0.46f * scale, 0), new Vector3(0.3f, 0.3f, 0.16f) * scale, meat);
            foreach (var s in new[] { -1f, 1f })
                K(g, new Vector3(s * 0.25f * scale, -1.05f * scale, -0.05f * scale), new Vector3(0.16f, 0.52f, 0.12f) * scale, fat);
            for (int i = 0; i < 7; i++)
                B(g, new Vector3(0, (-0.8f - i * 0.095f) * scale, -0.098f * scale), new Vector3(0.4f, 0.026f, 0.012f) * scale, fat, new Vector3(0, 0, i % 2 == 0 ? -5 : 5));
        }

        static void DrawerChest(Transform det, Vector3 pos, float yaw, float width, float height, int cols, int rows)
        {
            var g = Group("Çekmeceler", det, pos, yaw).transform;
            B(g, new Vector3(0, height / 2f, 0.25f), new Vector3(width, height, 0.5f), DarkOak);
            B(g, new Vector3(0, height + 0.02f, 0.26f), new Vector3(width + 0.06f, 0.04f, 0.55f), MidOak);
            float cw = width / cols, ch = (height - 0.1f) / rows;
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                {
                    var c = new Vector3(-width / 2f + cw * (x + 0.5f), 0.08f + ch * (y + 0.5f), 0.505f);
                    B(g, c, new Vector3(cw - 0.03f, ch - 0.03f, 0.012f), MidOak);
                    B(g, c + new Vector3(0, ch * 0.2f, 0.008f), new Vector3(cw * 0.42f, ch * 0.22f, 0.003f), Paper);
                    S(g, c + new Vector3(0, -ch * 0.14f, 0.018f), Vector3.one * 0.028f, Brass);
                }
        }

        static void Papers(Transform p, Vector3 pos, int sheets, int seed)
        {
            for (int i = 0; i < sheets; i++)
                B(p, pos + new Vector3((R(seed, i) - 0.5f) * 0.05f, 0.006f + i * 0.011f, (R(seed + 2, i) - 0.5f) * 0.05f), new Vector3(0.25f, 0.009f, 0.34f), Paper, new Vector3(0, (R(seed + 4, i) - 0.5f) * 22f, 0));
        }

        static void Scissors(Transform p, Vector3 pos, Vector3 facing)
        {
            var g = Group("Makas", p, pos).transform;
            g.localRotation = Quaternion.Euler(facing);
            B(g, new Vector3(0, 0.1f, 0), new Vector3(0.016f, 0.2f, 0.005f), Steel, new Vector3(0, 0, 7));
            B(g, new Vector3(0, 0.1f, 0), new Vector3(0.016f, 0.2f, 0.005f), Steel, new Vector3(0, 0, -7));
            foreach (var s in new[] { -1f, 1f }) C(g, new Vector3(s * 0.03f, -0.035f, 0), new Vector3(0.05f, 0.004f, 0.05f), Iron, new Vector3(90, 0, 0));
        }

        /// <summary>Kumaş topu: terzi görselindeki gibi dik duran, hafif eğik rulo.</summary>
        static void Bolt(Transform det, Vector3 foot, int i)
        {
            float h = 1.35f + R(i, 5) * 0.55f;
            var tilt = new Vector3((R(i, 7) - 0.5f) * 7f, 0, (R(i, 9) - 0.5f) * 7f);
            var cloth = Tailoring.Cloth(FabricColors[i & 7], 0.12f);
            C(det, foot + Vector3.up * (h / 2f), new Vector3(0.17f, h / 2f, 0.17f), cloth, tilt);
            C(det, foot + Vector3.up * (h + 0.015f), new Vector3(0.05f, 0.02f, 0.05f), MidOak, tilt);
        }

        static void Spools(Transform p, Vector3 corner, int cols, int rows)
        {
            for (int z = 0; z < rows; z++)
                for (int x = 0; x < cols; x++)
                {
                    var pos = corner + new Vector3(x * 0.1f, 0, z * 0.1f);
                    C(p, pos + Vector3.up * 0.045f, new Vector3(0.07f, 0.035f, 0.07f), Tailoring.Cloth(FabricColors[(x * 3 + z * 5) & 7], 0.3f));
                    C(p, pos + Vector3.up * 0.006f, new Vector3(0.082f, 0.006f, 0.082f), MidOak);
                    C(p, pos + Vector3.up * 0.086f, new Vector3(0.082f, 0.006f, 0.082f), MidOak);
                }
        }

        /// <summary>Terzi mankeni: göğüs, bel, kalça ve boyun (delil astarı göğüs hizasında kalır).</summary>
        public static void DressForm(Transform mq, Material cloth)
        {
            S(mq, new Vector3(0, 1.45f, 0), new Vector3(0.46f, 0.44f, 0.32f), cloth);
            S(mq, new Vector3(0, 1.2f, 0), new Vector3(0.36f, 0.42f, 0.27f), cloth);
            S(mq, new Vector3(0, 0.99f, 0), new Vector3(0.46f, 0.36f, 0.33f), cloth);
            C(mq, new Vector3(0, 1.72f, 0), new Vector3(0.11f, 0.06f, 0.11f), cloth);
            C(mq, new Vector3(0, 1.79f, 0), new Vector3(0.14f, 0.012f, 0.14f), MidOak);
            B(mq, new Vector3(0.05f, 1.5f, -0.158f), new Vector3(0.018f, 0.45f, 0.004f), M.Get("detail_tape", new Color(0.72f, 0.62f, 0.3f), 0.1f), new Vector3(0, 0, 8));
        }

        /// <summary>Deri berjer: oturak ve arkalık çarpışır, kollar süstür.</summary>
        static void Armchair(Transform t, Transform det, Vector3 pos, float yaw)
        {
            Box("BerjerÇarpışma", t, pos + Vector3.up * 0.4f, new Vector3(0.82f, 0.8f, 0.82f), Leather, new Vector3(0, yaw, 0)).GetComponent<Renderer>().enabled = false;
            var g = Group("Berjer", det, pos, yaw).transform;
            B(g, new Vector3(0, 0.3f, 0), new Vector3(0.76f, 0.24f, 0.76f), Leather);
            B(g, new Vector3(0, 0.46f, 0.02f), new Vector3(0.62f, 0.1f, 0.66f), Leather);
            B(g, new Vector3(0, 0.75f, -0.34f), new Vector3(0.78f, 0.86f, 0.16f), Leather);
            foreach (var s in new[] { -1f, 1f })
            {
                B(g, new Vector3(s * 0.38f, 0.5f, 0), new Vector3(0.14f, 0.26f, 0.76f), Leather);
                C(g, new Vector3(s * 0.32f, 0.08f, 0.32f), new Vector3(0.05f, 0.08f, 0.05f), DarkOak);
                C(g, new Vector3(s * 0.32f, 0.08f, -0.32f), new Vector3(0.05f, 0.08f, 0.05f), DarkOak);
            }
        }

        // =================================================================
        //  GİZEMLİ KASABA ODALARI
        // =================================================================
        public static void Eczane(Transform t)
        {
            var det = Group("Ayrıntı", t, Vector3.zero).transform;
            const float y0 = 0.25f, step = 0.54f;
            var wood = DarkOak;
            // Arka duvar: tavana kadar kavanoz (delil şişesi x=3.7, y=1.38 boş kalır)
            System.Func<Vector3, bool> clueGap = p => p.x > 3.3f && p.x < 4.1f && p.y > 1.2f && p.y < 1.8f && p.z > 3f;
            Shelves(det, new Vector3(-2.85f, 0, D / 2 - 0.02f), 180f, 4.2f, y0, step, 7, "jars", wood, 3);
            Shelves(det, new Vector3(2.85f, 0, D / 2 - 0.02f), 180f, 4.2f, y0, step, 7, "jars", wood, 41, clueGap);
            // Sağ duvar: kavanoz rafı ve küçük çekmeceli ilaç dolabı
            Shelves(det, new Vector3(W / 2 - 0.02f, 0, -1.0f), -90f, 3.2f, y0, step, 7, "jars", wood, 83);
            DrawerChest(det, new Vector3(W / 2 - 0.02f, 0, 2.1f), -90f, 2.6f, 1.15f, 9, 4);
            Shelves(det, new Vector3(W / 2 - 0.02f, 1.2f, 2.1f), -90f, 2.6f, 0.12f, step, 5, "boxes", wood, 7);
            // Sol duvar: pencerenin iki yanı
            Shelves(det, new Vector3(-W / 2 + 0.02f, 0, -1.55f), 90f, 2.5f, y0, step, 7, "jars", wood, 127);
            Shelves(det, new Vector3(-W / 2 + 0.02f, 0, 2.9f), 90f, 1.9f, y0, step, 7, "jars", wood, 151);
            // Tavandan sarkan kurutulmuş otlar
            HerbRow(det, 3.05f, -3.9f, -0.9f, 1);
            HerbRow(det, 3.05f, 0.9f, 3.9f, 2);
            HerbRow(det, 0.75f, -2.4f, 2.4f, 3);
            // Tezgâh: terazi, havan, küçük şişeler, ilaç paketleri
            BalanceScale(det, new Vector3(1.75f, 1.02f, 1.85f));
            Mortar(det, new Vector3(-1.85f, 1.02f, 1.85f));
            for (int i = 0; i < 6; i++)
            {
                var pos = new Vector3(-1.75f + (i % 3) * 0.13f, 1.02f, 1.4f + (i / 3) * 0.13f);
                C(det, pos + Vector3.up * 0.06f, new Vector3(0.08f, 0.06f, 0.08f), Tint("detail_jar", JarColors, i, 0.85f));
                C(det, pos + Vector3.up * 0.13f, new Vector3(0.05f, 0.012f, 0.05f), Cork);
            }
            for (int i = 0; i < 4; i++)
                B(det, new Vector3(1.55f + i * 0.14f, 1.045f, 1.35f), new Vector3(0.12f, 0.05f, 0.09f), Paper, new Vector3(0, i * 13f, 0));
            InteriorDressing.OilLamp(t, new Vector3(-1.45f, 1.02f, 1.95f), 1.3f);
            Bake(det);
        }

        public static void Kasap(Transform t)
        {
            var det = Group("Ayrıntı", t, Vector3.zero).transform;
            TileBand(det, new Color(0.78f, 0.77f, 0.72f), 2.3f);
            // Sağ duvar rayı ve karkaslar
            B(det, new Vector3(4.55f, H - 0.42f, 1.5f), new Vector3(0.07f, 0.07f, 3.8f), Steel);
            foreach (var z in new[] { 0.25f, 1.25f, 2.3f })
                Carcass(det, new Vector3(4.55f, H - 0.42f, z), 1.1f, 90f);
            // Tezgâh önünde camlı et vitrini: koyu ahşap gövde, açık tepsi, saydam ön cam
            Box("VitrinGövde", t, new Vector3(0, 0.3f, 0.98f), new Vector3(3.9f, 0.6f, 0.42f), DarkOak);
            B(det, new Vector3(0, 0.625f, 0.98f), new Vector3(3.8f, 0.03f, 0.4f), M.Get("detail_tray", new Color(0.42f, 0.41f, 0.38f), 0.65f));
            for (int i = 0; i < 18; i++)
            {
                float x = -1.75f + (i % 9) * 0.44f, z = 0.9f + (i / 9) * 0.18f;
                var mat = i % 3 == 0 ? M.Get("detail_meat_light", new Color(0.62f, 0.2f, 0.18f), 0.6f) : M.Get("detail_meat", new Color(0.42f, 0.07f, 0.07f), 0.55f);
                S(det, new Vector3(x, 0.665f, z), new Vector3(0.36f, 0.08f, 0.16f), mat);
                if (i % 4 == 1) S(det, new Vector3(x + 0.08f, 0.685f, z), new Vector3(0.14f, 0.04f, 0.1f), M.Get("detail_fat", new Color(0.82f, 0.72f, 0.62f), 0.5f));
            }
            B(det, new Vector3(0, 0.84f, 0.79f), new Vector3(3.84f, 0.42f, 0.015f), M.PuddleWater, new Vector3(-16, 0, 0));
            foreach (var x in new[] { -1.93f, 1.93f }) B(det, new Vector3(x, 0.84f, 0.98f), new Vector3(0.03f, 0.44f, 0.42f), Steel);
            // Boyalı tabela (görseldeki neon yerine)
            B(det, new Vector3(0, 3.35f, D / 2 - 0.04f), new Vector3(2.5f, 0.78f, 0.05f), M.Get("detail_signboard", new Color(0.07f, 0.13f, 0.09f), 0.35f));
            B(det, new Vector3(0, 3.35f, D / 2 - 0.02f), new Vector3(2.6f, 0.88f, 0.03f), Brass);
            Text3D("KasapTabela", t, new Vector3(0, 3.46f, D / 2 - 0.08f), "KASAP", new Color(0.85f, 0.72f, 0.4f), 0.04f);
            Text3D("KasapTabelaAlt", t, new Vector3(0, 3.17f, D / 2 - 0.08f), "HASAN USTA · ET VE SAKATAT", new Color(0.7f, 0.62f, 0.42f), 0.011f);
            // Sucuk dizileri (ray altı, kasabın arkasında)
            for (int s = 0; s < 7; s++)
                for (int k = 0; k < 4; k++)
                    K(det, new Vector3(-1.2f + s * 0.4f, H - 0.72f - k * 0.26f, 3.6f), new Vector3(0.07f, 0.13f, 0.07f), M.Get("detail_sausage", new Color(0.3f, 0.09f, 0.06f), 0.5f));
            // Bıçak askısı (arka duvar sol)
            B(det, new Vector3(-3.3f, 1.75f, D / 2 - 0.05f), new Vector3(1.3f, 0.09f, 0.03f), DarkOak);
            for (int i = 0; i < 6; i++)
            {
                float x = -3.85f + i * 0.22f, len = 0.22f + (i % 3) * 0.06f;
                B(det, new Vector3(x, 1.68f - len / 2f, D / 2 - 0.07f), new Vector3(0.05f + (i % 2) * 0.03f, len, 0.006f), Steel);
                B(det, new Vector3(x, 1.72f, D / 2 - 0.07f), new Vector3(0.03f, 0.11f, 0.018f), DarkOak);
            }
            // Zeminde talaş
            foreach (var p in new[] { new Vector3(2.2f, 0.004f, -0.6f), new Vector3(-2.3f, 0.004f, 0.1f), new Vector3(3.5f, 0.004f, 1.1f) })
                B(det, p, new Vector3(1.3f, 0.006f, 0.9f), M.Get("detail_sawdust", new Color(0.52f, 0.42f, 0.27f), 0.03f), new Vector3(0, p.x * 40f, 0));
            Bake(det);
        }

        public static void Terzi(Transform t)
        {
            var det = Group("Ayrıntı", t, Vector3.zero).transform;
            Wainscot(det, 1.0f);
            // Sol duvar: dik kumaş topları (mankenin arkası)
            for (int i = 0; i < 16; i++)
                Bolt(det, new Vector3(-4.78f + (i % 2) * 0.2f, 0.02f, 1.3f + i * 0.155f), i);
            // Sağ duvar: katlanmış kumaş rafı
            Shelves(det, new Vector3(W / 2 - 0.02f, 0, 2.0f), -90f, 3.7f, 0.25f, 0.48f, 7, "folded", DarkOak, 11);
            // Arka duvar sol: delikli pano, kalıplar ve makaslar
            B(det, new Vector3(-3.0f, 2.3f, D / 2 - 0.04f), new Vector3(3.2f, 1.9f, 0.04f), M.Get("detail_pegboard", new Color(0.44f, 0.34f, 0.22f), 0.15f));
            for (int i = 0; i < 6; i++)
                B(det, new Vector3(-4.2f + i * 0.46f, 2.72f - (i % 2) * 0.1f, D / 2 - 0.07f), new Vector3(0.36f, 0.62f, 0.005f), Paper, new Vector3(0, 0, (R(i, 3) - 0.5f) * 8f));
            for (int i = 0; i < 5; i++)
                Scissors(det, new Vector3(-4.1f + i * 0.5f, 1.86f, D / 2 - 0.07f), new Vector3(0, 0, (R(i, 5) - 0.5f) * 16f));
            // Arka duvar sağ üst: kumaş rafının üstünde şapka kutuları
            for (int i = 0; i < 6; i++)
                C(det, new Vector3(-0.6f + i * 0.85f, 2.72f, D / 2 - 0.25f), new Vector3(0.36f, 0.12f, 0.36f), M.Get("detail_hatbox_" + (i % 2), i % 2 == 0 ? new Color(0.32f, 0.24f, 0.17f) : new Color(0.15f, 0.17f, 0.22f), 0.15f));
            // Tezgâh: iplik makaraları, makas, mezura
            Spools(det, new Vector3(1.25f, 1.02f, 1.32f), 7, 5);
            Scissors(det, new Vector3(-1.85f, 1.03f, 1.55f), new Vector3(90, 30, 0));
            Scissors(det, new Vector3(-1.65f, 1.03f, 1.75f), new Vector3(90, -20, 0));
            C(det, new Vector3(-1.8f, 1.045f, 1.3f), new Vector3(0.14f, 0.022f, 0.14f), M.Get("detail_tape", new Color(0.72f, 0.62f, 0.3f), 0.1f));
            // Pedallı dikiş masası (sol ön)
            Box("DikişMasası", t, new Vector3(-4.3f, 0.78f, -1.55f), new Vector3(0.8f, 0.05f, 1.5f), MidOak);
            foreach (var z in new[] { -2.2f, -0.9f })
                B(det, new Vector3(-4.3f, 0.38f, z), new Vector3(0.7f, 0.76f, 0.05f), Iron);
            B(det, new Vector3(-4.3f, 0.1f, -1.55f), new Vector3(0.42f, 0.03f, 0.3f), Iron, new Vector3(0, 0, 12));
            C(det, new Vector3(-4.3f, 0.4f, -1.95f), new Vector3(0.42f, 0.015f, 0.42f), Iron, new Vector3(0, 0, 90));
            B(det, new Vector3(-4.3f, 0.86f, -1.7f), new Vector3(0.28f, 0.1f, 0.5f), Iron);
            B(det, new Vector3(-4.3f, 1.05f, -1.82f), new Vector3(0.14f, 0.32f, 0.14f), Iron);
            B(det, new Vector3(-4.3f, 1.2f, -1.62f), new Vector3(0.13f, 0.11f, 0.48f), Iron);
            C(det, new Vector3(-4.3f, 1.08f, -2.0f), new Vector3(0.2f, 0.018f, 0.2f), Brass, new Vector3(0, 0, 90));
            Spools(det, new Vector3(-4.55f, 0.805f, -1.2f), 4, 2);
            InteriorDressing.OilLamp(t, new Vector3(-4.15f, 0.805f, -1.0f), 1.1f);
            Bake(det);
        }

        public static void Muhtarlik(Transform t)
        {
            var det = Group("Ayrıntı", t, Vector3.zero).transform;
            Wainscot(det, 1.05f);
            // Kasetli tavan
            foreach (var x in new[] { -3.3f, -1.65f, 1.65f, 3.3f })
                B(det, new Vector3(x, H - 0.1f, 0), new Vector3(0.2f, 0.2f, D), DarkOak);
            // Eyalet haritası (arka duvar sağ)
            B(det, new Vector3(2.8f, 2.6f, D / 2 - 0.03f), new Vector3(2.35f, 1.65f, 0.04f), DarkOak);
            B(det, new Vector3(2.8f, 2.6f, D / 2 - 0.06f), new Vector3(2.2f, 1.5f, 0.008f), M.Get("detail_parchment", new Color(0.66f, 0.56f, 0.36f), 0.04f));
            // Harita çizgileri: dağınık çubuklar yerine birbirine bağlı sınır ve kıyı hatları
            var ink = M.Get("detail_ink", new Color(0.14f, 0.09f, 0.06f), 0.1f);
            for (int line = 0; line < 4; line++)
            {
                var a = new Vector2(1.85f + R(line, 11) * 0.5f, 2.0f + R(line, 12) * 1.2f);
                for (int i = 0; i < 9; i++)
                {
                    float angle = (R(line * 17 + i, 13) - 0.5f) * 110f + line * 22f;
                    var next = a + new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * (0.12f + R(line, i) * 0.12f);
                    next.x = Mathf.Clamp(next.x, 1.8f, 3.8f);
                    next.y = Mathf.Clamp(next.y, 1.95f, 3.25f);
                    var mid = (a + next) / 2f; var dir = next - a;
                    if (dir.sqrMagnitude > 0.0004f)
                        B(det, new Vector3(mid.x, mid.y, D / 2 - 0.066f), new Vector3(dir.magnitude + 0.008f, 0.009f, 0.002f), ink, new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg));
                    a = next;
                }
            }
            for (int i = 0; i < 7; i++)
                S(det, new Vector3(1.95f + R(i, 21) * 1.7f, 2.1f + R(i, 22) * 1.0f, D / 2 - 0.066f), new Vector3(0.035f, 0.035f, 0.004f), M.Get("flagred", new Color(0.8f, 0.04f, 0.08f), 0.2f));
            // Masa üstü: evrak yığınları, hokka, lamba
            Papers(det, new Vector3(1.4f, 1.02f, 1.42f), 16, 3);
            Papers(det, new Vector3(-1.5f, 1.02f, 1.42f), 11, 8);
            B(det, new Vector3(1.42f, 1.21f, 1.42f), new Vector3(0.28f, 0.05f, 0.36f), Tint("detail_book", BookColors, 0, 0.25f), new Vector3(0, 12, 0));
            C(det, new Vector3(1.08f, 1.05f, 1.92f), new Vector3(0.07f, 0.035f, 0.07f), Brass);
            C(det, new Vector3(1.12f, 1.14f, 1.92f), new Vector3(0.008f, 0.09f, 0.008f), Paper, new Vector3(0, 0, -25));
            InteriorDressing.OilLamp(t, new Vector3(-1.2f, 1.02f, 1.95f), 1.4f);
            // Kapıya bakan iki berjer ve sağ duvarda kanepe yerine ikinci koltuk
            Armchair(t, det, new Vector3(-2.35f, 0, 0.05f), 25f);
            Armchair(t, det, new Vector3(2.35f, 0, 0.05f), -25f);
            // Dosya dolapları (sağ duvar, sobanın ilerisi) ve küre
            foreach (var z in new[] { -0.6f, 0.25f })
                DrawerChest(det, new Vector3(W / 2 - 0.02f, 0, z), -90f, 0.82f, 1.25f, 2, 4);
            var globe = Group("Küre", det, new Vector3(-4.35f, 0, -1.5f)).transform;
            C(globe, new Vector3(0, 0.45f, 0), new Vector3(0.05f, 0.45f, 0.05f), DarkOak);
            C(globe, new Vector3(0, 0.03f, 0), new Vector3(0.44f, 0.03f, 0.44f), DarkOak);
            S(globe, new Vector3(0, 1.08f, 0), Vector3.one * 0.46f, M.Get("detail_globe", new Color(0.52f, 0.44f, 0.27f), 0.45f));
            C(globe, new Vector3(0, 1.08f, 0), new Vector3(0.52f, 0.008f, 0.52f), Brass, new Vector3(0, 0, 90));
            Bake(det);
        }

        public static void Karakol(Transform t)
        {
            var det = Group("Ayrıntı", t, Vector3.zero).transform;
            TileBand(det, new Color(0.3f, 0.36f, 0.4f), 1.4f);
            // Taşıyıcı sütunlar
            var column = M.Get("detail_column", new Color(0.3f, 0.33f, 0.35f), 0.2f, 0f, M.NoiseTexture);
            foreach (var x in new[] { -2.8f, 2.8f })
            {
                SetTiling(Box("Sütun", t, new Vector3(x, H / 2, 0.2f), new Vector3(0.48f, H, 0.48f), column).GetComponent<Renderer>(), new Vector2(0.5f, 3f));
                B(det, new Vector3(x, 0.16f, 0.2f), new Vector3(0.6f, 0.32f, 0.6f), DarkOak);
            }
            // Kurum tabelası (arka duvar üstü; görseldeki "Emniyet" yerine dönemin zaptiyesi)
            B(det, new Vector3(0.1f, 3.6f, D / 2 - 0.03f), new Vector3(3.6f, 0.5f, 0.04f), M.Get("detail_policeboard", new Color(0.06f, 0.09f, 0.17f), 0.35f));
            Text3D("KarakolTabela", t, new Vector3(0.1f, 3.6f, D / 2 - 0.07f), "ZAPTİYE DAİRESİ · 2. ŞUBE", new Color(0.8f, 0.76f, 0.6f), 0.018f);
            // Masa: dosya yığınları
            Papers(det, new Vector3(0.25f, 1.01f, 1.35f), 12, 5);
            Papers(det, new Vector3(-1.4f, 1.01f, 1.38f), 7, 9);
            for (int i = 0; i < 4; i++)
                B(det, new Vector3(0.68f, 1.03f + i * 0.04f, 1.38f), new Vector3(0.28f, 0.035f, 0.36f), M.Get("detail_folder_" + (i % 2), i % 2 == 0 ? new Color(0.5f, 0.38f, 0.22f) : new Color(0.22f, 0.26f, 0.32f), 0.1f), new Vector3(0, i * 9f - 12f, 0));
            InteriorDressing.OilLamp(t, new Vector3(0.1f, 1.01f, 1.95f), 1.3f);
            // Arşiv rafı (sol ön duvar) ve pencere altında bekleme sırası
            Shelves(det, new Vector3(-W / 2 + 0.02f, 0, -1.4f), 90f, 2.7f, 0.25f, 0.5f, 6, "boxes", DarkOak, 19);
            Box("BeklemeSırası", t, new Vector3(4.62f, 0.46f, -1.8f), new Vector3(0.5f, 0.06f, 2.1f), MidOak);
            foreach (var z in new[] { -2.7f, -0.9f }) B(det, new Vector3(4.62f, 0.22f, z), new Vector3(0.42f, 0.44f, 0.06f), Iron);
            // Hücre: hasır yatak, kova
            B(det, new Vector3(-4.4f, 0.49f, 2.5f), new Vector3(0.82f, 0.08f, 2.3f), M.Get("detail_straw", new Color(0.48f, 0.4f, 0.22f), 0.04f));
            C(det, new Vector3(-3.5f, 0.15f, 3.4f), new Vector3(0.3f, 0.15f, 0.3f), M.Get("detail_bucket", new Color(0.24f, 0.23f, 0.21f), 0.4f, 0.5f));
            Bake(det);
        }

        // =================================================================
        //  KIRSAL ODALAR (Gölgeşehir / Sisören)
        // =================================================================
        static string GoodsFor(string key)
        {
            switch (key)
            {
                case "bakkal": case "manav": case "tutuncu": case "kahvehane": case "hekim": return "jars";
                case "sahaf": case "muhtar": case "muhtarlik": case "telgrafhane": case "sinema": return "books";
                case "ayakkabici": return "boots";
                case "oduncu": case "demirci": case "hurdaci": case "tupcu": case "ahir": return "tins";
                default: return key.StartsWith("kasabali_evi") ? "books" : "boxes";
            }
        }

        /// <summary>Tezgâh arkasındaki iki raf, dükkânın malıyla dolu (renkli blok yerine gerçek eşya).</summary>
        public static void Rural(Transform t, BuildingDef d)
        {
            var det = Group("Ayrıntı", t, Vector3.zero).transform;
            string key = d.Key ?? "";
            string goods = GoodsFor(key);
            Shelves(det, new Vector3(-2.85f, 0, D / 2 - 0.02f), 180f, 3.95f, 0.25f, 0.5f, 6, goods, DarkOak, d.NpcId * 5 + 1);
            Shelves(det, new Vector3(2.85f, 0, D / 2 - 0.02f), 180f, 3.95f, 0.25f, 0.5f, 6, goods, DarkOak, d.NpcId * 5 + 43);

            switch (key)
            {
                case "oduncu":
                    foreach (var x in new[] { -3.2f, 3.2f })   // iki kişilik testereler
                    {
                        B(det, new Vector3(x, 3.2f, D / 2 - 0.05f), new Vector3(1.9f, 0.17f, 0.012f), Steel, new Vector3(0, 0, x * 2f));
                        foreach (var s in new[] { -1f, 1f }) C(det, new Vector3(x + s * 1.0f, 3.2f + s * x * 0.035f, D / 2 - 0.06f), new Vector3(0.05f, 0.14f, 0.05f), MidOak);
                    }
                    for (int i = 0; i < 4; i++)   // balta sırası
                    {
                        float x = -1.2f + i * 0.8f;
                        B(det, new Vector3(x, 2.95f, D / 2 - 0.06f), new Vector3(0.05f, 0.75f, 0.03f), MidOak, new Vector3(0, 0, 6));
                        B(det, new Vector3(x + 0.04f, 3.27f, D / 2 - 0.06f), new Vector3(0.24f, 0.13f, 0.02f), Steel);
                    }
                    break;
                case "demirci":
                    for (int i = 0; i < 8; i++)   // duvarda nallar
                        C(det, new Vector3(-3.6f + i * 1.03f, 3.15f, D / 2 - 0.05f), new Vector3(0.26f, 0.008f, 0.26f), Iron, new Vector3(90, 0, 0));
                    break;
                case "bakkal":
                    BalanceScale(det, new Vector3(1.7f, 1.02f, 1.75f));
                    HerbRow(det, 3.05f, -3.8f, -1f, d.NpcId + 11);
                    break;
                case "manav":
                    for (int i = 0; i < 4; i++)   // tezgâh önünde sebze kasaları
                    {
                        float x = -1.5f + i;
                        Box("SebzeKasası", t, new Vector3(x, 0.17f, 0.98f), new Vector3(0.9f, 0.34f, 0.42f), MidOak);
                        var veg = M.Get("detail_veg_" + i, new[] { new Color(0.22f, 0.36f, 0.1f), new Color(0.65f, 0.28f, 0.05f), new Color(0.32f, 0.42f, 0.14f), new Color(0.6f, 0.12f, 0.06f) }[i], 0.4f);
                        for (int v = 0; v < 12; v++)
                            S(det, new Vector3(x - 0.33f + (v % 4) * 0.22f, 0.38f, 0.86f + (v / 4) * 0.12f), Vector3.one * (i == 0 ? 0.2f : 0.12f), veg);
                    }
                    break;
                case "hekim":
                    B(det, new Vector3(0, 2.75f, D / 2 - 0.04f), new Vector3(0.8f, 1.1f, 0.02f), Paper);   // anatomi levhası
                    S(det, new Vector3(0, 3.05f, D / 2 - 0.06f), new Vector3(0.16f, 0.2f, 0.01f), M.Get("detail_ink", new Color(0.14f, 0.09f, 0.06f), 0.1f));
                    B(det, new Vector3(0, 2.68f, D / 2 - 0.06f), new Vector3(0.28f, 0.5f, 0.005f), M.Get("detail_ink", new Color(0.14f, 0.09f, 0.06f), 0.1f));
                    break;
                case "ayakkabici":
                    Box("TamirTezgahı", t, new Vector3(-4.3f, 0.4f, -0.1f), new Vector3(0.85f, 0.8f, 1.5f), MidOak);
                    C(det, new Vector3(-4.3f, 0.92f, -0.1f), new Vector3(0.05f, 0.12f, 0.05f), Iron);
                    B(det, new Vector3(-4.3f, 1.06f, -0.03f), new Vector3(0.09f, 0.06f, 0.26f), Iron);
                    for (int i = 0; i < 4; i++) Scissors(det, new Vector3(-4.5f, 0.81f, -0.65f + i * 0.13f), new Vector3(90, i * 30f, 0));
                    break;
                case "muhtar": case "muhtarlik":
                    Papers(det, new Vector3(-1.4f, 1.02f, 1.75f), 10, d.NpcId + 21);
                    Armchair(t, det, new Vector3(-2.35f, 0, 0.1f), 25f);
                    Armchair(t, det, new Vector3(2.35f, 0, 0.1f), -25f);
                    break;
            }
            Bake(det);
        }

        // =================================================================
        //  BİRLEŞTİRME
        // =================================================================
        /// <summary>
        /// Gruptaki küçük eşyaları malzeme başına tek mesh'e birleştirir: yüzlerce kavanoz tek çizim olur.
        /// Grupta çarpışması gereken obje bulunmaz; okunamayan mesh (olmamalı) olduğu gibi kalır.
        /// </summary>
        static void Bake(Transform group)
        {
            var toLocal = group.worldToLocalMatrix;
            var byMaterial = new Dictionary<Material, List<CombineInstance>>();
            var vertexCount = new Dictionary<Material, int>();
            var baked = new List<GameObject>();
            foreach (var mf in group.GetComponentsInChildren<MeshFilter>(true))
            {
                var r = mf.GetComponent<MeshRenderer>();
                var mesh = mf.sharedMesh;
                if (r == null || mesh == null || !mesh.isReadable || r.sharedMaterial == null) continue;
                var mat = r.sharedMaterial;
                if (!byMaterial.TryGetValue(mat, out var list)) { list = new List<CombineInstance>(); byMaterial[mat] = list; vertexCount[mat] = 0; }
                list.Add(new CombineInstance { mesh = mesh, transform = toLocal * mf.transform.localToWorldMatrix });
                vertexCount[mat] += mesh.vertexCount;
                baked.Add(mf.gameObject);
            }
            foreach (var kv in byMaterial)
            {
                var combined = new Mesh { name = "Birleşik_" + kv.Key.name };
                if (vertexCount[kv.Key] > 65000) combined.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                combined.CombineMeshes(kv.Value.ToArray(), true, true);
                var go = new GameObject("Birleşik");
                go.transform.SetParent(group, false);
                go.AddComponent<MeshFilter>().sharedMesh = combined;
                go.AddComponent<MeshRenderer>().sharedMaterial = kv.Key;
            }
            foreach (var go in baked)
            {
                go.SetActive(false);
                Object.Destroy(go);
            }
        }
    }
}
