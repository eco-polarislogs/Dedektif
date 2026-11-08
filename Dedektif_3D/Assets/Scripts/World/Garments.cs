using System.Collections.Generic;
using UnityEngine;
using static Dedektif3D.World.Shapes;
using M = Dedektif3D.World.MaterialLibrary;

namespace Dedektif3D.World
{
    /// <summary>
    /// Kodla dikilen, iskelete deri (skin) olarak bağlı giysiler: uzun etek, palto / önlük eteği, göğüslü önlük, şal, atkı.
    /// Model bağlama pozundayken (montajdan hemen sonra) kurulur; ağırlıklar kalça / uyluk / göğüs kemiklerine dağıtılır,
    /// böylece yürürken etek bacaklarla salınır, otururken uylukla birlikte öne katlanır.
    /// Ayrıca kafa / göğüs / el çerçevelerine bağlı sert aksesuarlar (gözlük, bıyık, kravat, el eşyaları).
    /// </summary>
    public static class Garments
    {
        class Frame
        {
            public Transform Root, Hips, Spine, Chest, Neck, LThigh, RThigh, LCalf, RCalf, LArm, RArm;
            public Vector3 Fwd, Right, Center;
            public float HipsY, KneeY, AnkleY, ChestY, NeckY, ShoulderW;
        }

        static Frame Measure(Animator a, Transform root)
        {
            Transform B(HumanBodyBones b) => a.GetBoneTransform(b);
            var f = new Frame
            {
                Root = root, Hips = B(HumanBodyBones.Hips), Spine = B(HumanBodyBones.Spine), Chest = B(HumanBodyBones.UpperChest) ?? B(HumanBodyBones.Chest),
                Neck = B(HumanBodyBones.Neck) ?? B(HumanBodyBones.Head), LThigh = B(HumanBodyBones.LeftUpperLeg), RThigh = B(HumanBodyBones.RightUpperLeg),
                LCalf = B(HumanBodyBones.LeftLowerLeg), RCalf = B(HumanBodyBones.RightLowerLeg), LArm = B(HumanBodyBones.LeftUpperArm), RArm = B(HumanBodyBones.RightUpperArm),
            };
            if (f.Hips == null || f.LThigh == null || f.RThigh == null || f.LCalf == null) return null;
            if (f.Chest == null) f.Chest = f.Spine ?? f.Hips;
            if (f.Spine == null) f.Spine = f.Hips;
            var right = f.RThigh.position - f.LThigh.position; right.y = 0;
            f.Right = right.sqrMagnitude > 1e-6f ? right.normalized : Vector3.right;
            f.Fwd = Vector3.Cross(f.Right, Vector3.up);
            f.Center = f.Hips.position;
            f.HipsY = f.Hips.position.y;
            f.KneeY = (f.LCalf.position.y + f.RCalf.position.y) * 0.5f;
            var lf = a.GetBoneTransform(HumanBodyBones.LeftFoot);
            f.AnkleY = lf != null ? lf.position.y : f.KneeY - (f.HipsY - f.KneeY);
            f.ChestY = f.Chest.position.y;
            f.NeckY = f.Neck != null ? f.Neck.position.y : f.ChestY + 0.2f;
            f.ShoulderW = f.LArm != null && f.RArm != null ? Vector3.Distance(f.LArm.position, f.RArm.position) : 0.36f;
            return f;
        }

        static Material Cloth(Color c, float smooth = 0.12f)
        {
            var m = M.Get("garment_" + ColorUtility.ToHtmlStringRGB(c), c, smooth);
            if (m.HasProperty("_Cull")) m.SetFloat("_Cull", 0f);   // iki yüzlü kumaş
            return m;
        }

        /// <summary>Tarife göre tüm kumaş katmanlarını dikip modele ekler. Model bağlama pozunda olmalı.</summary>
        public static void Dress(Animator a, Transform root, CharLook l)
        {
            if (a == null || l == null) return;
            if (Tailoring.Dress(a, root, l)) return;   // baştan dikilen kara film kıyafetleri (hazır model ve eski parça yolu yedek)
            var f = Measure(a, root);
            if (f == null) return;
            float leg = Mathf.Max(0.3f, f.HipsY - f.AnkleY);
            float waistY = f.HipsY + leg * 0.08f;
            if (l.Skirt)
                Tube(f, "Etek", waistY, f.AnkleY + leg * 0.08f, new Vector2(0.17f, 0.13f), new Vector2(0.3f, 0.27f), Cloth(l.SkirtColor), SkirtWeight);
            if (l.Coat || l.LabCoat)
            {
                var c = l.LabCoat ? new Color(0.9f, 0.91f, 0.9f) : l.CoatColor;
                float hem = l.Skirt ? f.AnkleY + leg * 0.3f : f.KneeY - leg * 0.12f;
                // Etek varsa palto eteği eteğin dışında kalsın (iç içe geçmesin)
                var botR = l.Skirt ? new Vector2(0.33f, 0.3f) : new Vector2(0.27f, 0.23f);
                Tube(f, l.LabCoat ? "DoktorÖnlüğü" : "PaltoEteği", waistY + leg * 0.05f, hem, new Vector2(0.21f, 0.16f), botR, Cloth(c, 0.18f), SkirtWeight, frontSlit: true);
            }
            if (l.Apron || l.BibApron)
                Apron(f, l.BibApron ? f.ChestY + (f.NeckY - f.ChestY) * 0.1f : waistY + leg * 0.03f, f.KneeY + leg * (l.BibApron ? 0.02f : 0.12f), Cloth(l.ApronColor, 0.2f));
            if (l.Shawl)
                Tube(f, "Şal", f.NeckY - 0.03f, f.HipsY + (f.ChestY - f.HipsY) * 0.3f, new Vector2(0.11f, 0.1f), new Vector2(f.ShoulderW * 0.68f, 0.2f), Cloth(l.ShawlColor), ShawlWeight, rings: 6);
            if (l.Scarf)
                Tube(f, "Atkı", f.NeckY + 0.035f, f.NeckY - 0.05f, new Vector2(0.075f, 0.072f), new Vector2(0.105f, 0.1f), Cloth(l.ScarfColor), ScarfWeight, rings: 3);
        }

        delegate void Weigh(Frame f, float t, float side, float front, List<(Transform, float)> w);

        static void SkirtWeight(Frame f, float t, float side, float front, List<(Transform, float)> w)
        {
            float hips = Mathf.Lerp(1f, 0.35f, Mathf.Clamp01(t * 1.3f));
            float rest = 1f - hips;
            float r = Mathf.Clamp01(0.5f + side * 0.75f);
            w.Add((f.Hips, hips)); w.Add((f.RThigh, rest * r)); w.Add((f.LThigh, rest * (1f - r)));
        }

        static void ShawlWeight(Frame f, float t, float side, float front, List<(Transform, float)> w)
        {
            float arm = Mathf.Clamp01((Mathf.Abs(side) - 0.55f) * 2f) * t * 0.6f;
            var armB = side > 0 ? f.RArm : f.LArm;
            w.Add((f.Chest, 1f - arm)); if (armB != null && arm > 0) w.Add((armB, arm));
        }

        static void ScarfWeight(Frame f, float t, float side, float front, List<(Transform, float)> w)
        {
            w.Add((f.Neck ?? f.Chest, 1f - t * 0.5f)); w.Add((f.Chest, t * 0.5f));
        }

        /// <summary>Gövde çevresinde elips kesitli boru (etek, palto eteği, şal, atkı). t: üstten (0) alta (1).</summary>
        static void Tube(Frame f, string name, float topY, float botY, Vector2 topR, Vector2 botR, Material mat, Weigh weigh, int rings = 9, int seg = 22, bool frontSlit = false)
        {
            var verts = new List<Vector3>(); var uvs = new List<Vector2>();
            var weights = new List<List<(Transform, float)>>();
            for (int i = 0; i < rings; i++)
            {
                float t = i / (float)(rings - 1);
                float y = Mathf.Lerp(topY, botY, t);
                // Hafif çan eğrisi: üstte dar, altta açılır
                float k = Mathf.SmoothStep(0f, 1f, t);
                var r = Vector2.Lerp(topR, botR, k);
                for (int s = 0; s <= seg; s++)
                {
                    float a = s / (float)seg * Mathf.PI * 2f;
                    float side = Mathf.Sin(a), front = Mathf.Cos(a);
                    // Palto önü açık: önde küçük yarık (iki etek kanadı hafif ayrık)
                    float open = frontSlit && front > 0.96f ? 0.02f * t : 0f;
                    var p = f.Center + f.Right * (side * r.x) + f.Fwd * (front * r.y - 0.01f - open);
                    p.y = y;
                    verts.Add(p); uvs.Add(new Vector2(s / (float)seg, 1f - t));
                    var w = new List<(Transform, float)>(); weigh(f, t, side, front, w); weights.Add(w);
                }
            }
            var tris = new List<int>();
            for (int i = 0; i < rings - 1; i++)
                for (int s = 0; s < seg; s++)
                {
                    if (frontSlit && (s == 0 || s == seg - 1) && i > rings / 3) continue;   // yarık
                    int a0 = i * (seg + 1) + s, b0 = a0 + seg + 1;
                    tris.Add(a0); tris.Add(b0); tris.Add(a0 + 1);
                    tris.Add(a0 + 1); tris.Add(b0); tris.Add(b0 + 1);
                }
            Build(f, name, verts, uvs, tris, weights, mat);
        }

        /// <summary>Önden sarkan önlük paneli (göğüslü ya da bel önlüğü).</summary>
        static void Apron(Frame f, float topY, float botY, Material mat)
        {
            var verts = new List<Vector3>(); var uvs = new List<Vector2>(); var weights = new List<List<(Transform, float)>>();
            int rows = 8, cols = 6;
            float waist = f.HipsY + (f.ChestY - f.HipsY) * 0.25f;
            for (int i = 0; i < rows; i++)
            {
                float t = i / (float)(rows - 1);
                float y = Mathf.Lerp(topY, botY, t);
                float halfW = y > waist ? 0.13f : Mathf.Lerp(0.17f, 0.2f, Mathf.InverseLerp(waist, botY, y));
                // Göğüs / karın önünde, aşağıda bacakların önüne doğru biraz öne açılır
                float depth = y > waist ? Mathf.Lerp(0.15f, 0.13f, Mathf.InverseLerp(waist, topY, y)) : Mathf.Lerp(0.15f, 0.2f, Mathf.InverseLerp(waist, botY, y));
                for (int c = 0; c < cols; c++)
                {
                    float u = c / (float)(cols - 1) * 2f - 1f;
                    var p = f.Center + f.Right * (u * halfW) + f.Fwd * (depth - Mathf.Abs(u) * 0.03f);
                    p.y = y;
                    verts.Add(p); uvs.Add(new Vector2((u + 1) / 2, 1 - t));
                    var w = new List<(Transform, float)>();
                    if (y > f.HipsY + 0.05f) { float k = Mathf.InverseLerp(f.HipsY, f.ChestY, y); w.Add((f.Chest, k)); w.Add((f.Spine, 1f - k)); }
                    else
                    {
                        float tt = Mathf.InverseLerp(f.HipsY, botY, y);
                        float hips = Mathf.Lerp(1f, 0.45f, tt); float r = Mathf.Clamp01(0.5f + u * 0.6f);
                        w.Add((f.Hips, hips)); w.Add((f.RThigh, (1 - hips) * r)); w.Add((f.LThigh, (1 - hips) * (1 - r)));
                    }
                    weights.Add(w);
                }
            }
            var tris = new List<int>();
            for (int i = 0; i < rows - 1; i++)
                for (int c = 0; c < cols - 1; c++)
                {
                    int a0 = i * cols + c, b0 = a0 + cols;
                    tris.Add(a0); tris.Add(a0 + 1); tris.Add(b0);
                    tris.Add(a0 + 1); tris.Add(b0 + 1); tris.Add(b0);
                }
            Build(f, "Önlük", verts, uvs, tris, weights, mat);
        }

        static void Build(Frame f, string name, List<Vector3> worldVerts, List<Vector2> uvs, List<int> tris, List<List<(Transform, float)>> weights, Material mat)
        {
            var go = new GameObject(name);
            go.transform.SetParent(f.Root, false);
            var smrT = go.transform;
            var bones = new List<Transform>();
            var bw = new BoneWeight[worldVerts.Count];
            var local = new Vector3[worldVerts.Count];
            for (int v = 0; v < worldVerts.Count; v++)
            {
                local[v] = smrT.InverseTransformPoint(worldVerts[v]);
                var w = weights[v];
                w.RemoveAll(x => x.Item1 == null || x.Item2 <= 0.001f);
                w.Sort((x, y) => y.Item2.CompareTo(x.Item2));
                if (w.Count == 0) w.Add((f.Hips, 1f));
                if (w.Count > 4) w.RemoveRange(4, w.Count - 4);
                float sum = 0; foreach (var x in w) sum += x.Item2;
                int Idx(Transform t) { int i = bones.IndexOf(t); if (i < 0) { bones.Add(t); i = bones.Count - 1; } return i; }
                var b = new BoneWeight();
                b.boneIndex0 = Idx(w[0].Item1); b.weight0 = w[0].Item2 / sum;
                if (w.Count > 1) { b.boneIndex1 = Idx(w[1].Item1); b.weight1 = w[1].Item2 / sum; }
                if (w.Count > 2) { b.boneIndex2 = Idx(w[2].Item1); b.weight2 = w[2].Item2 / sum; }
                if (w.Count > 3) { b.boneIndex3 = Idx(w[3].Item1); b.weight3 = w[3].Item2 / sum; }
                bw[v] = b;
            }
            var mesh = new Mesh { name = name };
            mesh.vertices = local; mesh.uv = uvs.ToArray(); mesh.triangles = tris.ToArray();
            mesh.boneWeights = bw;
            var bp = new Matrix4x4[bones.Count];
            for (int i = 0; i < bones.Count; i++) bp[i] = bones[i].worldToLocalMatrix * smrT.localToWorldMatrix;
            mesh.bindposes = bp;
            mesh.RecalculateNormals();
            // Normaller dışa baksın (sarım yönü ne olursa olsun)
            var n = mesh.normals; var cLocal = smrT.InverseTransformPoint(f.Center);
            int outward = 0;
            for (int i = 0; i < local.Length; i++) { var d = local[i] - cLocal; d.y = 0; if (Vector3.Dot(n[i], d) > 0) outward++; }
            if (outward < local.Length / 2)
            {
                var t = mesh.triangles; for (int i = 0; i < t.Length; i += 3) { var x = t[i + 1]; t[i + 1] = t[i + 2]; t[i + 2] = x; }
                mesh.triangles = t; mesh.RecalculateNormals();
            }
            mesh.RecalculateBounds();
            var smr = go.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = mesh; smr.bones = bones.ToArray(); smr.rootBone = null; smr.sharedMaterial = mat;
            smr.localBounds = mesh.bounds;
            smr.updateWhenOffscreen = false;
        }

        // =================================================================
        //  SERT AKSESUARLAR (kafa / göğüs / el çerçevelerine)
        // =================================================================
        public static void Accessorize(PropFactory.PersonRig rig, CharLook l)
        {
            var h = rig.Human;
            if (h == null || l == null) return;
            var head = rig.Head;
            if (l.Glasses && head != null)
            {
                var frame = M.Get("glasses_frame", new Color(0.06f, 0.05f, 0.05f), 0.6f, 0.4f);
                var glass = M.Get("glasses_lens", new Color(0.6f, 0.65f, 0.7f), 0.95f, 0.1f);
                foreach (float x in new[] { -0.068f, 0.068f })
                {
                    Cyl("GözlükÇerçeve", head, new Vector3(x, 0.03f, 0.19f), new Vector3(0.085f, 0.004f, 0.085f), frame, new Vector3(90, 0, 0), false);
                    Cyl("GözlükCam", head, new Vector3(x, 0.03f, 0.193f), new Vector3(0.072f, 0.002f, 0.072f), glass, new Vector3(90, 0, 0), false);
                }
                Box("GözlükKöprü", head, new Vector3(0, 0.035f, 0.195f), new Vector3(0.05f, 0.008f, 0.008f), frame, null, false);
            }
            if (l.Beard == "mustache" && head != null)
            {
                var hc = l.HairColor ?? new Color(0.1f, 0.08f, 0.06f);
                Box("Bıyık", head, new Vector3(0, -0.065f, 0.185f), new Vector3(0.105f, 0.022f, 0.03f), M.Get("mustache_" + ColorUtility.ToHtmlStringRGB(hc), hc * 0.85f, 0.2f), new Vector3(-8, 0, 0), false);
            }
            if (l.Tie && h.ChestAnchor != null)
            {
                var tie = M.Get("tie_" + ColorUtility.ToHtmlStringRGB(l.TieColor), l.TieColor, 0.45f);
                var ch = h.ChestAnchor;
                Box("KravatDüğüm", ch, new Vector3(0, 0.15f, 0.125f), new Vector3(0.035f, 0.03f, 0.02f), tie, null, false);
                Box("Kravat", ch, new Vector3(0, 0.0f, 0.13f), new Vector3(0.05f, 0.28f, 0.012f), tie, new Vector3(-6, 0, 0), false);
            }
            if (!string.IsNullOrEmpty(l.Prop) && h.HandAnchor != null) Prop(rig, l.Prop);
            Tailoring.Accessorize(rig, l);
        }

        static void Prop(PropFactory.PersonRig rig, string prop)
        {
            var h = rig.Human; var hand = h.HandAnchor;
            Transform Hang() { var g = new GameObject("ElEşyası_" + prop).transform; g.SetParent(hand, false); g.gameObject.AddComponent<KeepUpright>().facing = rig.Root; return g; }
            var wood = M.Wood(new Color(0.32f, 0.2f, 0.11f));
            var steel = M.Get("steel", new Color(0.55f, 0.55f, 0.58f), 0.75f, 0.9f);
            switch (prop)
            {
                case "cane":
                {
                    var g = Hang();
                    Cyl("Baston", g, new Vector3(0.02f, -0.42f, 0.04f), new Vector3(0.025f, 0.43f, 0.025f), wood, null, false);
                    Box("BastonSapı", g, new Vector3(0.02f, 0.0f, 0.0f), new Vector3(0.03f, 0.03f, 0.1f), wood, null, false);
                    break;
                }
                case "crook":
                {
                    var g = Hang();
                    Cyl("Değnek", g, new Vector3(0.02f, -0.2f, 0.04f), new Vector3(0.025f, 0.75f, 0.025f), wood, null, false);
                    break;
                }
                case "lantern":
                {
                    if (!h.HoldWithRightArm("lantern")) break;
                    var g = Hang();
                    var iron = M.Get("lantern_iron", new Color(0.08f, 0.08f, 0.08f), 0.5f, 0.8f);
                    Box("Kulp", g, new Vector3(0, -0.03f, 0), new Vector3(0.012f, 0.07f, 0.012f), iron, null, false);
                    Cyl("Kapak", g, new Vector3(0, -0.08f, 0), new Vector3(0.11f, 0.02f, 0.11f), iron, null, false);
                    Cyl("Cam", g, new Vector3(0, -0.17f, 0), new Vector3(0.09f, 0.07f, 0.09f), M.Emissive("lantern_glow", new Color(1f, 0.7f, 0.35f), 2.2f), null, false);
                    Cyl("Taban", g, new Vector3(0, -0.25f, 0), new Vector3(0.11f, 0.015f, 0.11f), iron, null, false);
                    var li = PointLight("FenerIşığı", g, new Vector3(0, -0.17f, 0.05f), new Color(1f, 0.68f, 0.34f), 2.2f, 6f);
                    li.gameObject.AddComponent<LightFlicker>().Init(li, 0.12f);
                    break;
                }
                case "cleaver":
                {
                    var g = Hang();
                    Box("SatırSap", g, new Vector3(0, -0.04f, 0.02f), new Vector3(0.03f, 0.12f, 0.035f), wood, null, false);
                    Box("SatırAğzı", g, new Vector3(0, -0.2f, 0.06f), new Vector3(0.012f, 0.2f, 0.12f), steel, null, false);
                    break;
                }
                case "axe":
                {
                    var g = Hang();
                    Cyl("BaltaSap", g, new Vector3(0, -0.3f, 0.02f), new Vector3(0.035f, 0.33f, 0.035f), wood, null, false);
                    Box("BaltaAğzı", g, new Vector3(0, -0.6f, 0.08f), new Vector3(0.02f, 0.12f, 0.16f), steel, null, false);
                    break;
                }
                case "hammer":
                {
                    var g = Hang();
                    Cyl("ÇekiçSap", g, new Vector3(0, -0.12f, 0.02f), new Vector3(0.025f, 0.15f, 0.025f), wood, null, false);
                    Box("ÇekiçBaşı", g, new Vector3(0, -0.28f, 0.02f), new Vector3(0.05f, 0.05f, 0.13f), M.Get("iron_dark", new Color(0.12f, 0.12f, 0.13f), 0.5f, 0.9f), null, false);
                    break;
                }
                case "scissors":
                {
                    var g = Hang();
                    Box("Makas1", g, new Vector3(0.01f, -0.1f, 0.03f), new Vector3(0.012f, 0.18f, 0.02f), steel, new Vector3(0, 0, 8), false);
                    Box("Makas2", g, new Vector3(-0.01f, -0.1f, 0.03f), new Vector3(0.012f, 0.18f, 0.02f), steel, new Vector3(0, 0, -8), false);
                    break;
                }
                case "book":
                {
                    var g = Hang();
                    Box("Kitap", g, new Vector3(0, -0.08f, 0.04f), new Vector3(0.04f, 0.22f, 0.16f), M.Get("book_leather", new Color(0.35f, 0.14f, 0.1f), 0.3f), null, false);
                    break;
                }
                case "bottle":
                {
                    var g = Hang();
                    Cyl("Şişe", g, new Vector3(0, -0.07f, 0.04f), new Vector3(0.06f, 0.07f, 0.06f), M.Get("brownglass", new Color(0.35f, 0.18f, 0.06f), 0.95f, 0.1f), null, false);
                    break;
                }
                case "paper":
                {
                    var g = Hang();
                    Box("Kağıt", g, new Vector3(0, -0.12f, 0.05f), new Vector3(0.005f, 0.26f, 0.19f), M.Get("paper", new Color(0.9f, 0.86f, 0.74f)), null, false);
                    break;
                }
                case "glass":
                {
                    var g = Hang();
                    Cyl("ÇayBardağı", g, new Vector3(0, -0.03f, 0.05f), new Vector3(0.045f, 0.04f, 0.045f), M.Get("tea", new Color(0.6f, 0.15f, 0.05f), 0.9f), null, false);
                    break;
                }
                case "pipe":
                {
                    var g = Hang();
                    Cyl("Pipo", g, new Vector3(0, -0.02f, 0.06f), new Vector3(0.012f, 0.07f, 0.012f), M.Wood(new Color(0.25f, 0.14f, 0.08f)), new Vector3(90, 0, 0), false);
                    Cyl("PipoHaznesi", g, new Vector3(0, 0.0f, 0.13f), new Vector3(0.035f, 0.03f, 0.035f), M.Wood(new Color(0.25f, 0.14f, 0.08f)), null, false);
                    break;
                }
                case "bag":
                {
                    // Postacı çantası: kalçada, göğüs çerçevesinden çapraz kayış
                    if (h.ChestAnchor == null) break;
                    var leather = M.Get("bag_leather", new Color(0.3f, 0.2f, 0.12f), 0.35f);
                    Box("ÇantaKayışı", h.ChestAnchor, new Vector3(0, -0.05f, 0.135f), new Vector3(0.04f, 0.5f, 0.01f), leather, new Vector3(0, 0, 38), false);
                    Box("Çanta", h.ChestAnchor, new Vector3(0.2f, -0.42f, 0.06f), new Vector3(0.08f, 0.22f, 0.28f), leather, null, false);
                    break;
                }
                case "tray":
                {
                    var g = Hang();
                    Cyl("Tepsi", g, new Vector3(0, -0.02f, 0.14f), new Vector3(0.3f, 0.01f, 0.3f), M.Get("brass", new Color(0.75f, 0.6f, 0.25f), 0.8f, 0.9f), null, false);
                    break;
                }
                case "logs":
                {
                    if (h.ChestAnchor == null) break;
                    for (int i = 0; i < 4; i++)
                        Cyl("Odun", h.ChestAnchor, new Vector3(0.18f + (i % 2) * 0.06f, 0.22f + i * 0.04f, -0.02f + (i / 2) * 0.06f), new Vector3(0.07f, 0.28f, 0.07f), wood, new Vector3(90, 30, 0), false);
                    break;
                }
            }
        }
    }
}
