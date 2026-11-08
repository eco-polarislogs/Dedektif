using System.Collections.Generic;
using UnityEngine;
using static Dedektif3D.World.Shapes;
using M = Dedektif3D.World.MaterialLibrary;

namespace Dedektif3D.World
{
    /// <summary>
    /// Kara film kıyafetlerini doğrudan iskelete deri olarak diker: gömlek, ceket / yelek / kazak, trençkot, pantolon ve kollar.
    /// Modelin fantastik gövde parçaları takılmaz: baş, eller ve ayakkabı kitten gelir, geri kalan her şey burada üretilir.
    /// Ölçüler iskeletten alınır, tüm parçalar kemik ağırlıklıdır: yürürken, otururken ve diz çökerken kumaş bedenle birlikte hareket eder.
    /// </summary>
    public static class Tailoring
    {
        class Body
        {
            public Transform Root, Hips, Spine, Chest, Neck, LThigh, RThigh, LCalf, RCalf, LFoot, RFoot, LArm, RArm, LFore, RFore, LHand, RHand;
            public Vector3 Fwd, Right, Center;
            public float HipsY, KneeY, AnkleY, ChestY, NeckY, ArmY, WaistY, ShoulderW, K, Girth;
        }

        struct Ring
        {
            public float Y, Rx, Rz, Open, Shift;
            public Ring(float y, float rx, float rz, float openDeg = 0f, float shift = 0f) { Y = y; Rx = rx; Rz = rz; Open = openDeg * Mathf.Deg2Rad; Shift = shift; }
        }

        delegate void Weigh(Body b, float y, float side, List<(Transform, float)> w);

        static Body Measure(Animator a, Transform root, bool female)
        {
            Transform B(HumanBodyBones x) => a.GetBoneTransform(x);
            var b = new Body
            {
                Root = root, Hips = B(HumanBodyBones.Hips), Spine = B(HumanBodyBones.Spine), Chest = B(HumanBodyBones.UpperChest) ?? B(HumanBodyBones.Chest),
                Neck = B(HumanBodyBones.Neck) ?? B(HumanBodyBones.Head), LThigh = B(HumanBodyBones.LeftUpperLeg), RThigh = B(HumanBodyBones.RightUpperLeg),
                LCalf = B(HumanBodyBones.LeftLowerLeg), RCalf = B(HumanBodyBones.RightLowerLeg), LFoot = B(HumanBodyBones.LeftFoot), RFoot = B(HumanBodyBones.RightFoot),
                LArm = B(HumanBodyBones.LeftUpperArm), RArm = B(HumanBodyBones.RightUpperArm), LFore = B(HumanBodyBones.LeftLowerArm), RFore = B(HumanBodyBones.RightLowerArm),
                LHand = B(HumanBodyBones.LeftHand), RHand = B(HumanBodyBones.RightHand),
            };
            if (b.Hips == null || b.LThigh == null || b.RThigh == null || b.LCalf == null || b.RCalf == null || b.LFoot == null || b.RFoot == null) return null;
            if (b.LArm == null || b.RArm == null || b.LFore == null || b.RFore == null || b.LHand == null || b.RHand == null) return null;
            if (b.Chest == null) b.Chest = b.Spine ?? b.Hips;
            if (b.Spine == null) b.Spine = b.Hips;
            var right = b.RThigh.position - b.LThigh.position; right.y = 0;
            b.Right = right.sqrMagnitude > 1e-6f ? right.normalized : Vector3.right;
            b.Fwd = Vector3.Cross(b.Right, Vector3.up);
            b.Center = b.Hips.position;
            b.HipsY = b.Hips.position.y;
            b.KneeY = (b.LCalf.position.y + b.RCalf.position.y) * 0.5f;
            b.AnkleY = (b.LFoot.position.y + b.RFoot.position.y) * 0.5f;
            b.ChestY = b.Chest.position.y;
            b.NeckY = b.Neck != null ? b.Neck.position.y : b.ChestY + 0.2f;
            b.ArmY = (b.LArm.position.y + b.RArm.position.y) * 0.5f;
            b.ShoulderW = Vector3.Distance(b.LArm.position, b.RArm.position);
            b.K = Mathf.Max(0.3f, b.NeckY - b.AnkleY) / 1.38f;
            b.Girth = b.K * (female ? 0.84f : 1f);
            b.WaistY = b.HipsY + (b.ChestY - b.HipsY) * 0.42f;
            return b;
        }

        /// <summary>Kumaş: ince dokuma desenli, iki yüzlü. Islak trençkot için pürüzsüzlük yüksek tutulur.</summary>
        public static Material Cloth(Color c, float smooth = 0.14f)
        {
            string key = "tailor_" + ColorUtility.ToHtmlStringRGB(c) + "_" + Mathf.RoundToInt(smooth * 20);
            var m = M.Cached(key);
            if (m != null) return m;
            m = M.Get(key, c, smooth, 0f, ProceduralTextures.Weave);
            if (m.HasProperty("_Cull")) m.SetFloat("_Cull", 0f);
            return m;
        }

        /// <summary>Tarife göre bütün kıyafeti diker. Model bağlama pozunda olmalı. Dikilemezse false döner.</summary>
        public static bool Dress(Animator a, Transform root, CharLook l)
        {
            if (a == null || l == null) return false;
            var b = Measure(a, root, l.Female);
            if (b == null) return false;

            var pants = Cloth(l.Pants);
            var shirt = Cloth(l.Shirt, 0.1f);
            bool longCoat = l.Coat || l.LabCoat;
            bool closedKnit = !longCoat && l.Body != "ranger" && l.Sleeves.HasValue && Approximately(l.Sleeves.Value, l.Top);

            // Pantolon (kadında etek altında yine ince pantolon yerine uzun çorap görevi görür)
            Leg(b, b.LThigh, b.LCalf, b.LFoot, pants, "PaçaSol", l.Female && l.Skirt);
            Leg(b, b.RThigh, b.RCalf, b.RFoot, pants, "PaçaSağ", l.Female && l.Skirt);
            Pelvis(b, pants);

            // Gömlek: ceketin V yakasından görünür
            Torso(b, "Gömlek", shirt, b.WaistY - 0.06f * b.K, -0.018f, 0f, closed: true, sleeveless: true);
            ShirtCollar(b, shirt);

            if (l.Skirt) Skirt(b, Cloth(l.SkirtColor));

            if (longCoat)
            {
                bool trench = l.Trench;
                var coatColor = l.LabCoat ? new Color(0.88f, 0.89f, 0.88f) : l.CoatColor;
                var coat = Cloth(coatColor, trench ? 0.42f : 0.16f);
                // Paltonun altında takım ceketi (dedektif.png: trençkot açık, altında koyu ceket, gömlek, kravat)
                if (!l.LabCoat)
                {
                    var inner = Cloth(l.Top, 0.14f);
                    Torso(b, "İçCeket", inner, b.HipsY - 0.12f * b.K, 0.006f, 19f);
                }
                float hem = l.LabCoat ? b.KneeY - 0.02f : l.Skirt ? b.AnkleY + 0.28f * b.K : trench ? b.KneeY - 0.24f * b.K : b.KneeY - 0.16f * b.K;
                Torso(b, trench ? "Trençkot" : "Palto", coat, hem, 0.02f, trench ? 31f : 24f, flare: 1.18f);
                Arm(b, b.LArm, b.LFore, b.LHand, coat, "KolSol", 1.14f, trench);
                Arm(b, b.RArm, b.RFore, b.RHand, coat, "KolSağ", 1.14f, trench);
                Lapels(b, Cloth(coatColor * 0.86f, trench ? 0.42f : 0.16f), trench ? 1.16f : 1f, trench ? 31f : 24f);
                if (trench)
                {
                    Belt(b, Cloth(coatColor * 0.72f, 0.42f));
                    CollarUp(b, coat);
                }
            }
            else if (closedKnit)
            {
                var knit = Cloth(l.Top, 0.05f);
                Torso(b, "Kazak", knit, b.HipsY - 0.03f * b.K, 0.004f, 0f, closed: true);
                Arm(b, b.LArm, b.LFore, b.LHand, knit, "KolSol", 1.02f, false);
                Arm(b, b.RArm, b.RFore, b.RHand, knit, "KolSağ", 1.02f, false);
            }
            else if (l.Body == "ranger")
            {
                // Takım ceketi
                var jacket = Cloth(l.Top, 0.15f);
                Torso(b, "Ceket", jacket, b.HipsY - 0.16f * b.K, 0.008f, 20f);
                Arm(b, b.LArm, b.LFore, b.LHand, jacket, "KolSol", 1.06f, false);
                Arm(b, b.RArm, b.RFore, b.RHand, jacket, "KolSağ", 1.06f, false);
                Lapels(b, Cloth(l.Top * 0.82f, 0.2f), 0.9f, 20f);
            }
            else
            {
                // Yelek + gömlek kolları
                var vest = Cloth(l.Top, 0.14f);
                Torso(b, "Yelek", vest, b.HipsY + 0.02f * b.K, 0.002f, 15f, sleeveless: true);
                var sleeve = Cloth(l.Sleeves ?? l.Shirt, 0.1f);
                Arm(b, b.LArm, b.LFore, b.LHand, sleeve, "KolSol", 1f, false);
                Arm(b, b.RArm, b.RFore, b.RHand, sleeve, "KolSağ", 1f, false);
            }

            if (l.Apron || l.BibApron) Apron(b, l.BibApron, Cloth(l.ApronColor, 0.22f));
            if (l.Shawl) Shawl(b, Cloth(l.ShawlColor));
            if (l.Scarf) Scarf(b, Cloth(l.ScarfColor, 0.06f));
            return true;
        }

        static bool Approximately(Color a, Color b) => Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b) < 0.06f;

        // =================================================================
        //  GÖVDE
        // =================================================================
        /// <summary>Boyundan etek ucuna kadar gövde kabuğu. grow: bedenden dışa kalınlık, openDeg: yaka açıklığı (0 = kapalı), flare: etek ucu genişliği.</summary>
        static void Torso(Body b, string name, Material mat, float hemY, float grow, float openDeg, bool closed = false, bool sleeveless = false, float flare = 1f)
        {
            float K = b.Girth, g = grow * b.K;
            float shoulder = b.ShoulderW * 0.5f;
            float open = closed ? 0f : openDeg;
            var rings = new List<Ring>
            {
                new Ring(b.NeckY + 0.005f * b.K, 0.066f * b.K + g, 0.066f * b.K + g, open * 1.5f),
                new Ring(b.ArmY + 0.055f * b.K, (shoulder * 0.62f) + g, 0.082f * K + g, open * 1.4f),
                new Ring(b.ArmY + 0.018f * b.K, shoulder + (sleeveless ? 0.012f : 0.05f) * b.K + g, 0.108f * K + g, open * 1.15f),
                new Ring(b.ArmY - 0.06f * b.K, shoulder + (sleeveless ? 0.0f : 0.04f) * b.K + g, 0.128f * K + g, open * 0.95f),
                new Ring(b.ChestY - 0.03f * b.K, 0.178f * K + g, 0.128f * K + g, open * 0.62f),
                new Ring(Mathf.Lerp(b.ChestY, b.WaistY, 0.55f), 0.166f * K + g, 0.116f * K + g, open * 0.2f),
                new Ring(b.WaistY, 0.156f * K + g, 0.108f * K + g),
            };
            if (hemY < b.WaistY - 0.01f)
            {
                float hipY = Mathf.Max(hemY, b.HipsY - 0.05f * b.K);
                rings.Add(new Ring(hipY, 0.172f * K + g, 0.122f * K + g));
                if (hemY < hipY - 0.02f)
                {
                    float span = hipY - hemY;
                    float topFlare = 0.18f * K + g, botFlare = (0.19f + 0.1f * Mathf.Clamp01(span / 0.8f)) * K * flare + g;
                    float slit = closed || openDeg <= 0f ? 0f : openDeg * 0.34f;
                    int n = Mathf.Clamp(Mathf.RoundToInt(span / 0.075f), 2, 9);
                    for (int i = 1; i <= n; i++)
                    {
                        float t = i / (float)n;
                        rings.Add(new Ring(hipY - span * t, Mathf.Lerp(topFlare, botFlare, t), Mathf.Lerp(0.13f * K + g, botFlare * 0.82f, t), slit * t));
                    }
                }
            }
            Loft(b, name, rings, mat, WeighTorso, 30, folds: true);
        }

        static void WeighTorso(Body b, float y, float side, List<(Transform, float)> w)
        {
            if (y >= b.ChestY) { w.Add((b.Chest, 1f)); return; }
            if (y >= b.WaistY) { float k = Mathf.InverseLerp(b.WaistY, b.ChestY, y); w.Add((b.Chest, k)); w.Add((b.Spine, 1f - k)); return; }
            if (y >= b.HipsY) { float k = Mathf.InverseLerp(b.HipsY, b.WaistY, y); w.Add((b.Spine, k)); w.Add((b.Hips, 1f - k)); return; }
            float down = Mathf.Clamp01((b.HipsY - y) / Mathf.Max(0.05f, b.HipsY - b.KneeY));
            float hips = Mathf.Lerp(1f, 0.3f, down);
            float r = Mathf.Clamp01(0.5f + side * 0.7f);
            w.Add((b.Hips, hips)); w.Add((b.RThigh, (1f - hips) * r)); w.Add((b.LThigh, (1f - hips) * (1f - r)));
        }

        static void Pelvis(Body b, Material mat)
        {
            float K = b.Girth;
            var rings = new List<Ring>
            {
                new Ring(b.WaistY - 0.01f * b.K, 0.146f * K, 0.1f * K),
                new Ring(b.HipsY + 0.02f * b.K, 0.164f * K, 0.118f * K),
                new Ring(b.HipsY - 0.05f * b.K, 0.158f * K, 0.118f * K),
                new Ring(b.HipsY - 0.105f * b.K, 0.1f * K, 0.085f * K),
            };
            Loft(b, "Kalça", rings, mat, WeighTorso, 18, capBottom: true);
        }

        static void Skirt(Body b, Material mat)
        {
            float K = b.Girth;
            float bottom = b.AnkleY + 0.1f * b.K;
            var rings = new List<Ring>();
            rings.Add(new Ring(b.WaistY, 0.15f * K, 0.11f * K));
            rings.Add(new Ring(b.HipsY, 0.185f * K, 0.14f * K));
            int n = 7;
            for (int i = 1; i <= n; i++)
            {
                float t = i / (float)n;
                rings.Add(new Ring(Mathf.Lerp(b.HipsY, bottom, t), Mathf.Lerp(0.185f, 0.285f, t) * K, Mathf.Lerp(0.15f, 0.25f, t) * K));
            }
            Loft(b, "Etek", rings, mat, WeighTorso, 28, folds: true);
        }

        // =================================================================
        //  KOL VE BACAK
        // =================================================================
        static void Arm(Body b, Transform upper, Transform fore, Transform hand, Material mat, string name, float thick, bool strap)
        {
            float g = b.K * thick * (b.Girth / b.K > 0.9f ? 1f : 0.88f);
            Vector3 s = upper.position, e = fore.position, w = hand.position;
            var pts = new List<(Vector3 p, float r, float elbow)>
            {
                (Vector3.Lerp(s, b.Chest.position, 0.14f) + Vector3.up * 0.005f * b.K, 0.06f * g, 0f),
                (s, 0.074f * g, 0f),
                (Vector3.Lerp(s, e, 0.35f), 0.068f * g, 0f),
                (Vector3.Lerp(s, e, 0.75f), 0.058f * g, 0.08f),
                (e, 0.054f * g, 0.5f),
                (Vector3.Lerp(e, w, 0.25f), 0.053f * g, 0.92f),
                (Vector3.Lerp(e, w, 0.65f), 0.046f * g, 1f),
                (Vector3.Lerp(e, w, 0.93f), 0.042f * g, 1f),
                (Vector3.Lerp(e, w, 1.0f), 0.05f * g, 1f),
            };
            Sweep(b, name, pts, mat, (t, list) =>
            {
                if (t.elbow <= 0f) { list.Add((upper, t.isStart ? 0.65f : 1f)); if (t.isStart) list.Add((b.Chest, 0.35f)); }
                else { list.Add((upper, 1f - t.elbow)); list.Add((fore, t.elbow)); }
            });
            if (strap)
            {
                // Trençkot kol kayışı
                var cuff = new List<(Vector3 p, float r, float elbow)> { (Vector3.Lerp(e, w, 0.8f), 0.05f * g, 1f), (Vector3.Lerp(e, w, 0.88f), 0.05f * g, 1f) };
                Sweep(b, name + "Kayış", cuff, mat, (t, list) => list.Add((fore, 1f)), 12);
            }
        }

        static void Leg(Body b, Transform thigh, Transform calf, Transform foot, Material mat, string name, bool slim)
        {
            float g = b.K * (slim ? 0.72f : 1f) * (b.Girth / b.K > 0.9f ? 1f : 0.92f);
            Vector3 h = thigh.position, k = calf.position, a = foot.position;
            var pts = new List<(Vector3 p, float r, float elbow)>
            {
                (h + Vector3.up * 0.06f * b.K, 0.092f * g, 0f),
                (Vector3.Lerp(h, k, 0.3f), 0.094f * g, 0f),
                (Vector3.Lerp(h, k, 0.72f), 0.076f * g, 0.08f),
                (k, 0.07f * g, 0.5f),
                (Vector3.Lerp(k, a, 0.28f), 0.071f * g, 0.92f),
                (Vector3.Lerp(k, a, 0.68f), 0.062f * g, 1f),
                (a + Vector3.down * 0.01f * b.K, 0.067f * g, 1f),
            };
            Sweep(b, name, pts, mat, (t, list) =>
            {
                if (t.elbow <= 0f) { list.Add((thigh, t.isStart ? 0.6f : 1f)); if (t.isStart) list.Add((b.Hips, 0.4f)); }
                else { list.Add((thigh, 1f - t.elbow)); list.Add((calf, t.elbow)); }
            }, 14);
        }

        // =================================================================
        //  YAKA, KEMER, ÖNLÜK
        // =================================================================
        static void ShirtCollar(Body b, Material shirt)
        {
            float k = b.K;
            var rings = new List<Ring>
            {
                new Ring(b.NeckY + 0.055f * k, 0.066f * k, 0.066f * k, 12f),
                new Ring(b.NeckY + 0.005f * k, 0.074f * k, 0.074f * k, 14f),
            };
            Loft(b, "GömlekYakası", rings, shirt, WeighNeck, 16);
        }

        static void CollarUp(Body b, Material mat)
        {
            float k = b.K;
            var rings = new List<Ring>
            {
                new Ring(b.NeckY + 0.085f * k, 0.098f * k, 0.09f * k, 38f, -0.012f * k),
                new Ring(b.NeckY + 0.03f * k, 0.09f * k, 0.088f * k, 34f),
                new Ring(b.NeckY - 0.02f * k, 0.104f * k, 0.098f * k, 34f),
            };
            Loft(b, "KalkıkYaka", rings, mat, WeighNeck, 18);
        }

        static void WeighNeck(Body b, float y, float side, List<(Transform, float)> w)
        {
            w.Add((b.Neck ?? b.Chest, 0.5f)); w.Add((b.Chest, 0.5f));
        }

        /// <summary>Ceket yakası: V açıklığı boyunca iki katlı şerit.</summary>
        static void Lapels(Body b, Material mat, float width, float openDeg)
        {
            foreach (var sgn in new[] { -1f, 1f })
            {
                var verts = new List<Vector3>(); var uvs = new List<Vector2>(); var weights = new List<List<(Transform, float)>>();
                var tris = new List<int>();
                float top = b.ArmY + 0.05f * b.K, bottom = Mathf.Lerp(b.ChestY, b.WaistY, 0.55f);
                int n = 6;
                for (int i = 0; i <= n; i++)
                {
                    float t = i / (float)n;
                    float y = Mathf.Lerp(top, bottom, t);
                    float open = Mathf.Lerp(openDeg * 1.25f, openDeg * 0.2f, t) * Mathf.Deg2Rad;
                    float rx = Mathf.Lerp(0.15f, 0.168f, t) * b.Girth + 0.024f * b.K, rz = Mathf.Lerp(0.118f, 0.128f, t) * b.Girth + 0.024f * b.K;
                    float wd = Mathf.Sin(Mathf.Clamp01(t * 1.6f + 0.15f) * Mathf.PI * 0.72f) * 0.062f * b.K * width;
                    Vector3 Edge(float a) => b.Center + b.Right * (sgn * Mathf.Sin(a) * rx) + b.Fwd * (Mathf.Cos(a) * rz);
                    var inner = Edge(open); inner.y = y;
                    var outer = Edge(open + wd / Mathf.Max(0.05f, rx)); outer.y = y + 0.012f * b.K;
                    verts.Add(inner); verts.Add(outer);
                    uvs.Add(new Vector2(0, t)); uvs.Add(new Vector2(1, t));
                    for (int v = 0; v < 2; v++) { var w = new List<(Transform, float)>(); WeighTorso(b, y, sgn * 0.3f, w); weights.Add(w); }
                    if (i < n)
                    {
                        int a = i * 2;
                        tris.AddRange(new[] { a, a + 2, a + 1, a + 1, a + 2, a + 3 });
                    }
                }
                Build(b, sgn < 0 ? "YakaSol" : "YakaSağ", verts, uvs, tris, weights, mat, flatOutward: true);
            }
        }

        static void Belt(Body b, Material mat)
        {
            float K = b.Girth, y = b.WaistY - 0.005f * b.K, g = 0.024f * b.K;
            var rings = new List<Ring>
            {
                new Ring(y + 0.026f * b.K, 0.156f * K + g, 0.108f * K + g),
                new Ring(y - 0.026f * b.K, 0.16f * K + g, 0.112f * K + g),
            };
            Loft(b, "Kemer", rings, mat, WeighTorso, 28);
        }

        static void Apron(Body b, bool bib, Material mat)
        {
            float top = bib ? b.ChestY + (b.NeckY - b.ChestY) * 0.16f : b.WaistY;
            float bottom = b.KneeY - 0.02f * b.K;
            var verts = new List<Vector3>(); var uvs = new List<Vector2>(); var weights = new List<List<(Transform, float)>>();
            int rows = 9, cols = 7;
            for (int i = 0; i < rows; i++)
            {
                float t = i / (float)(rows - 1);
                float y = Mathf.Lerp(top, bottom, t);
                bool chest = y > b.WaistY;
                float halfW = chest ? 0.11f * b.K : Mathf.Lerp(0.17f, 0.21f, Mathf.InverseLerp(b.WaistY, bottom, y)) * b.Girth;
                float depth = chest ? Mathf.Lerp(0.136f, 0.15f, Mathf.InverseLerp(top, b.ChestY, y)) * b.Girth : Mathf.Lerp(0.135f, 0.2f, Mathf.InverseLerp(b.WaistY, bottom, y)) * b.Girth;
                for (int c = 0; c < cols; c++)
                {
                    float u = c / (float)(cols - 1) * 2f - 1f;
                    var p = b.Center + b.Right * (u * halfW) + b.Fwd * (depth - Mathf.Abs(u) * 0.02f);
                    p.y = y;
                    verts.Add(p); uvs.Add(new Vector2((u + 1f) * 0.5f, 1f - t));
                    var w = new List<(Transform, float)>(); WeighTorso(b, y, u * 0.6f, w); weights.Add(w);
                }
            }
            var tris = new List<int>();
            for (int i = 0; i < rows - 1; i++)
                for (int c = 0; c < cols - 1; c++)
                {
                    int a0 = i * cols + c, b0 = a0 + cols;
                    tris.AddRange(new[] { a0, a0 + 1, b0, a0 + 1, b0 + 1, b0 });
                }
            Build(b, "Önlük", verts, uvs, tris, weights, mat, flatOutward: true);
        }

        static void Shawl(Body b, Material mat)
        {
            var rings = new List<Ring>
            {
                new Ring(b.NeckY + 0.01f * b.K, 0.085f * b.K, 0.08f * b.K),
                new Ring(b.ArmY + 0.02f * b.K, b.ShoulderW * 0.5f + 0.075f * b.K, 0.13f * b.Girth),
                new Ring(b.ChestY - 0.05f * b.K, b.ShoulderW * 0.5f + 0.09f * b.K, 0.16f * b.Girth),
                new Ring(b.WaistY + 0.02f * b.K, b.ShoulderW * 0.5f + 0.08f * b.K, 0.15f * b.Girth),
            };
            Loft(b, "Şal", rings, mat, WeighTorso, 22, folds: true);
        }

        static void Scarf(Body b, Material mat)
        {
            var rings = new List<Ring>
            {
                new Ring(b.NeckY + 0.05f * b.K, 0.082f * b.K, 0.082f * b.K),
                new Ring(b.NeckY, 0.102f * b.K, 0.1f * b.K),
                new Ring(b.NeckY - 0.045f * b.K, 0.11f * b.K, 0.105f * b.K),
            };
            Loft(b, "Atkı", rings, mat, WeighNeck, 16);
        }

        // =================================================================
        //  MESH ÜRETİMİ
        // =================================================================
        /// <summary>Yatay halkalar arasında yüzey örer. Dikiş ön ortadadır; halkanın Open değeri kadar ön açık kalır.</summary>
        static void Loft(Body b, string name, List<Ring> rings, Material mat, Weigh weigh, int seg, bool folds = false, bool capBottom = false)
        {
            var verts = new List<Vector3>(); var uvs = new List<Vector2>(); var weights = new List<List<(Transform, float)>>(); var tris = new List<int>();
            float len = 0f;
            for (int i = 0; i < rings.Count; i++)
            {
                var r = rings[i];
                if (i > 0) len += Mathf.Abs(rings[i - 1].Y - r.Y);
                float yt = i / (float)Mathf.Max(1, rings.Count - 1);
                for (int s = 0; s <= seg; s++)
                {
                    float u = s / (float)seg;
                    float a = r.Open + u * (Mathf.PI * 2f - r.Open * 2f);
                    float side = Mathf.Sin(a), front = Mathf.Cos(a);
                    float foldAmp = folds ? Mathf.Lerp(0.006f, 0.012f, yt) * b.K : 0f;
                    float fold = foldAmp * (Mathf.Sin(a * 5f + yt * 5f) + 0.6f * Mathf.Sin(a * 11f - yt * 8f));
                    // Yuvarlatılmış dikdörtgen kesit: sırt ve göğüs düz, yanlar yumuşak
                    float sx = Mathf.Sign(side) * Mathf.Pow(Mathf.Abs(side), 0.82f), fz = Mathf.Sign(front) * Mathf.Pow(Mathf.Abs(front), 0.9f);
                    var p = b.Center + b.Right * (sx * (r.Rx + fold)) + b.Fwd * (fz * (r.Rz + fold) + r.Shift);
                    p.y = r.Y;
                    verts.Add(p);
                    uvs.Add(new Vector2(u * (r.Rx + r.Rz) * 6f, len * 2.2f));
                    var w = new List<(Transform, float)>(); weigh(b, r.Y, side, w); weights.Add(w);
                }
            }
            for (int i = 0; i < rings.Count - 1; i++)
                for (int s = 0; s < seg; s++)
                {
                    int a0 = i * (seg + 1) + s, b0 = a0 + seg + 1;
                    tris.AddRange(new[] { a0, b0, a0 + 1, a0 + 1, b0, b0 + 1 });
                }
            if (capBottom)
            {
                var last = rings[rings.Count - 1];
                int c = verts.Count;
                var center = b.Center + b.Fwd * last.Shift; center.y = last.Y;
                verts.Add(center); uvs.Add(Vector2.zero);
                var cw = new List<(Transform, float)>(); weigh(b, last.Y, 0f, cw); weights.Add(cw);
                int start = (rings.Count - 1) * (seg + 1);
                for (int s = 0; s < seg; s++) tris.AddRange(new[] { start + s, c, start + s + 1 });
            }
            Build(b, name, verts, uvs, tris, weights, mat);
        }

        struct SweepInfo { public float elbow; public bool isStart; }

        /// <summary>Kemik zinciri boyunca boru (kol, paça). elbow: 0 üst kemik, 1 alt kemik.</summary>
        static void Sweep(Body b, string name, List<(Vector3 p, float r, float elbow)> pts, Material mat, System.Action<SweepInfo, List<(Transform, float)>> weigh, int seg = 12)
        {
            var verts = new List<Vector3>(); var uvs = new List<Vector2>(); var weights = new List<List<(Transform, float)>>(); var tris = new List<int>();
            float len = 0f;
            var reference = Vector3.Cross((pts[pts.Count - 1].p - pts[0].p).normalized, Vector3.forward);
            if (reference.sqrMagnitude < 0.01f) reference = Vector3.Cross((pts[pts.Count - 1].p - pts[0].p).normalized, Vector3.up);
            reference.Normalize();
            for (int i = 0; i < pts.Count; i++)
            {
                var prev = pts[Mathf.Max(0, i - 1)].p; var next = pts[Mathf.Min(pts.Count - 1, i + 1)].p;
                var axis = (next - prev).normalized;
                var x = Vector3.Cross(axis, reference).normalized;
                var y = Vector3.Cross(axis, x).normalized;
                if (i > 0) len += Vector3.Distance(pts[i - 1].p, pts[i].p);
                for (int s = 0; s <= seg; s++)
                {
                    float u = s / (float)seg, a = u * Mathf.PI * 2f;
                    verts.Add(pts[i].p + x * (Mathf.Cos(a) * pts[i].r) + y * (Mathf.Sin(a) * pts[i].r));
                    uvs.Add(new Vector2(u * 1.4f, len * 2.2f));
                    var w = new List<(Transform, float)>();
                    weigh(new SweepInfo { elbow = pts[i].elbow, isStart = i == 0 }, w);
                    weights.Add(w);
                }
            }
            for (int i = 0; i < pts.Count - 1; i++)
                for (int s = 0; s < seg; s++)
                {
                    int a0 = i * (seg + 1) + s, b0 = a0 + seg + 1;
                    tris.AddRange(new[] { a0, b0, a0 + 1, a0 + 1, b0, b0 + 1 });
                }
            Build(b, name, verts, uvs, tris, weights, mat, sweepAxis: true, pts: pts);
        }

        static void Build(Body b, string name, List<Vector3> worldVerts, List<Vector2> uvs, List<int> tris, List<List<(Transform, float)>> weights, Material mat,
            bool flatOutward = false, bool sweepAxis = false, List<(Vector3 p, float r, float elbow)> pts = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(b.Root, false);
            var smrT = go.transform;
            var bones = new List<Transform>();
            int Idx(Transform t) { int i = bones.IndexOf(t); if (i < 0) { bones.Add(t); i = bones.Count - 1; } return i; }
            var bw = new BoneWeight[worldVerts.Count];
            var local = new Vector3[worldVerts.Count];
            for (int v = 0; v < worldVerts.Count; v++)
            {
                local[v] = smrT.InverseTransformPoint(worldVerts[v]);
                var w = weights[v];
                w.RemoveAll(x => x.Item1 == null || x.Item2 <= 0.001f);
                w.Sort((x, y) => y.Item2.CompareTo(x.Item2));
                if (w.Count == 0) w.Add((b.Hips, 1f));
                if (w.Count > 4) w.RemoveRange(4, w.Count - 4);
                float sum = 0f; foreach (var x in w) sum += x.Item2;
                var bo = new BoneWeight();
                bo.boneIndex0 = Idx(w[0].Item1); bo.weight0 = w[0].Item2 / sum;
                if (w.Count > 1) { bo.boneIndex1 = Idx(w[1].Item1); bo.weight1 = w[1].Item2 / sum; }
                if (w.Count > 2) { bo.boneIndex2 = Idx(w[2].Item1); bo.weight2 = w[2].Item2 / sum; }
                if (w.Count > 3) { bo.boneIndex3 = Idx(w[3].Item1); bo.weight3 = w[3].Item2 / sum; }
                bw[v] = bo;
            }
            var mesh = new Mesh { name = name, vertices = local, uv = uvs.ToArray(), triangles = tris.ToArray(), boneWeights = bw };
            var bp = new Matrix4x4[bones.Count];
            for (int i = 0; i < bones.Count; i++) bp[i] = bones[i].worldToLocalMatrix * smrT.localToWorldMatrix;
            mesh.bindposes = bp;
            mesh.RecalculateNormals();

            // Normaller dışa baksın: üçgenlerin sarım yönü ne olursa olsun tek karar
            var n = mesh.normals;
            int outward = 0, total = local.Length;
            var centerWorld = b.Center;
            for (int i = 0; i < total; i++)
            {
                var wp = worldVerts[i];
                Vector3 d;
                if (flatOutward) d = b.Fwd;
                else if (sweepAxis && pts != null)
                {
                    int ring = i / 13 < pts.Count ? i / 13 : pts.Count - 1;
                    d = Vector3.zero;
                    float best = float.MaxValue;
                    foreach (var q in pts) { float dd = (q.p - wp).sqrMagnitude; if (dd < best) { best = dd; d = wp - q.p; } }
                }
                else { d = wp - centerWorld; d.y = 0; }
                var nw = smrT.TransformDirection(n[i]);
                if (Vector3.Dot(nw, d) > 0) outward++;
            }
            if (outward < total / 2)
            {
                var t = mesh.triangles;
                for (int i = 0; i < t.Length; i += 3) { var x = t[i + 1]; t[i + 1] = t[i + 2]; t[i + 2] = x; }
                mesh.triangles = t;
                mesh.RecalculateNormals();
            }
            mesh.RecalculateBounds();
            var smr = go.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = mesh; smr.bones = bones.ToArray(); smr.rootBone = null; smr.sharedMaterial = mat;
            smr.localBounds = mesh.bounds;
            smr.updateWhenOffscreen = false;
        }

        // =================================================================
        //  SERT AKSESUARLAR (göğüs çerçevesine): kemer tokası, düğmeler, apolet
        // =================================================================
        public static void Accessorize(PropFactory.PersonRig rig, CharLook l)
        {
            var h = rig.Human;
            if (h == null || h.ChestAnchor == null || !l.Trench) return;
            var ch = h.ChestAnchor;
            var button = M.Get("coat_button", new Color(0.05f, 0.045f, 0.04f), 0.55f);
            var leather = M.Get("buckle_leather", l.CoatColor * 0.6f, 0.45f);
            // Kruvaze düğme sırası, açık önün iki yanında
            foreach (var x in new[] { -0.105f, 0.105f })
                for (int i = 0; i < 3; i++)
                    Cyl("Düğme", ch, new Vector3(x, -0.05f - i * 0.095f, 0.155f - i * 0.004f), new Vector3(0.024f, 0.004f, 0.024f), button, new Vector3(90, 0, 0), false);
            // Omuz apoletleri
            foreach (var x in new[] { -0.15f, 0.15f })
                Box("Apolet", ch, new Vector3(x, 0.135f, 0.0f), new Vector3(0.11f, 0.008f, 0.042f), leather, new Vector3(0, 0, x > 0 ? -12f : 12f), false);
            // Fırtına kapağı (sağ göğüs)
            Box("FırtınaKapağı", ch, new Vector3(0.14f, 0.0f, 0.128f), new Vector3(0.1f, 0.13f, 0.008f), Cloth(l.CoatColor * 0.9f, 0.42f), new Vector3(-8, 0, -5), false);
        }
    }
}
