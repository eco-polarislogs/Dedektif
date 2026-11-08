using System.Collections.Generic;
using UnityEngine;
using static Dedektif3D.World.Shapes;
using M = Dedektif3D.World.MaterialLibrary;

namespace Dedektif3D.World
{
    /// <summary>
    /// 1800'ler Osmanlı kasabası başlıkları. Hepsi "kafa çerçevesi" ölçüsünde çizilir: merkez kafatası ortası,
    /// tepe +0.17 (primitive figürde kafa, gerçek modelde HumanAnimator.HeadAnchor — ölçek kafaya göre ayarlı).
    /// </summary>
    public static class Headwear
    {
        public enum Kind { None, FlatCap, Bowler, TopHat, Fedora, Headscarf, Beanie, PostCap, PoliceCap, HeadWrap, Cloche }

        /// <summary>Kara film atmosferine uygun başlık seçimi: fötr, melon, kasket; kadında küçük şapka ya da başörtüsü.</summary>
        public static Kind PickFor(int hash, bool female, bool child, bool indoors)
        {
            int r = (int)((uint)(hash * 747796405u + 2891336453u) >> 9) % 100;
            if (female) return child ? Kind.None : r < (indoors ? 15 : 30) ? Kind.Cloche : r < (indoors ? 30 : 50) ? Kind.Headscarf : Kind.None;
            if (child) return r < 40 ? Kind.FlatCap : Kind.None;
            if (indoors) return r < 30 ? Kind.Fedora : Kind.None;
            return r < 45 ? Kind.Fedora : r < 75 ? Kind.FlatCap : r < 88 ? Kind.Bowler : Kind.None;
        }

        public static void Put(Transform head, Kind k, int hash = 0, Color? color = null)
        {
            if (head == null) return;
            switch (k)
            {
                case Kind.FlatCap: FlatCap(head, hash, color); break;
                case Kind.Bowler: Bowler(head); break;
                case Kind.TopHat: TownProps.TopHat(head); break;
                case Kind.Fedora: Fedora(head, color); break;
                case Kind.Cloche: Cloche(head, color); break;
                case Kind.Headscarf: Headscarf(head, hash, color); break;
                case Kind.Beanie: Beanie(head, color ?? new Color(0.2f, 0.2f, 0.19f)); break;
                case Kind.PostCap: PostCap(head, color ?? new Color(0.22f, 0.3f, 0.22f)); break;
                case Kind.PoliceCap: PoliceCap(head); break;
                case Kind.HeadWrap: HeadWrap(head, color ?? new Color(0.1f, 0.09f, 0.09f)); break;
            }
        }

        /// <summary>Örgü bere (bekçi, oduncu, çoban): kafatasını saran yuvarlak kep + kıvrık kenar.</summary>
        public static void Beanie(Transform head, Color c)
        {
            var mat = M.Get("beanie_" + ColorUtility.ToHtmlStringRGB(c), c, 0.05f);
            // Kaş hizasının üstünde biter: yüzü kapatmaz
            Sphere("Bere", head, new Vector3(0, 0.155f, -0.02f), new Vector3(0.4f, 0.24f, 0.41f), mat);
            Cyl("BereKenarı", head, new Vector3(0, 0.105f, -0.02f), new Vector3(0.405f, 0.03f, 0.415f), mat, null, false);
        }

        /// <summary>Postacı / memur kasketi: yuvarlak tepe, siperlik, ön rozet.</summary>
        public static void PostCap(Transform head, Color c)
        {
            var mat = M.Get("postcap_" + ColorUtility.ToHtmlStringRGB(c), c, 0.2f);
            Cyl("KasketTepe", head, new Vector3(0, 0.17f, 0), new Vector3(0.36f, 0.06f, 0.37f), mat, null, false);
            Box("KasketSiper", head, new Vector3(0, 0.12f, 0.17f), new Vector3(0.24f, 0.015f, 0.1f), M.Get("shoe", Color.black), null, false);
            Sphere("KasketRozet", head, new Vector3(0, 0.18f, 0.18f), Vector3.one * 0.04f, M.Emissive("gold", new Color(1f, 0.8f, 0.2f), 0.4f));
        }

        public static void PoliceCap(Transform head)
        {
            Cyl("PoliceCap", head, new Vector3(0, 0.17f, 0), new Vector3(0.36f, 0.07f, 0.36f), M.Get("policecap", new Color(0.08f, 0.12f, 0.22f)), null, false);
            Box("CapVisor", head, new Vector3(0, 0.12f, 0.17f), new Vector3(0.24f, 0.02f, 0.1f), M.Get("shoe", Color.black), null, false);
            Sphere("CapBadge", head, new Vector3(0, 0.19f, 0.18f), Vector3.one * 0.05f, M.Emissive("gold", new Color(1f, 0.8f, 0.2f), 0.6f));
        }

        /// <summary>Sarık / bağlı başörtüsü (tüccar, hurdacı kadın): kafayı saran kalın sargı.</summary>
        public static void HeadWrap(Transform head, Color c)
        {
            var mat = M.Get("wrap_" + ColorUtility.ToHtmlStringRGB(c), c, 0.08f);
            // Kaş hizasının üstünde biter: yüzü kapatmaz
            Sphere("SarıkTepe", head, new Vector3(0, 0.16f, -0.03f), new Vector3(0.4f, 0.25f, 0.41f), mat);
            Cyl("Sargı", head, new Vector3(0, 0.115f, -0.02f), new Vector3(0.42f, 0.05f, 0.43f), mat, new Vector3(-8, 0, 0), false);
        }

        /// <summary>Kadın şapkası: başa oturan yuvarlak tepe, dar ve hafif sarkık kenar, ince bant.</summary>
        public static void Cloche(Transform head, Color? color = null)
        {
            var c = color ?? new Color(0.14f, 0.11f, 0.12f);
            var felt = Tailoring.Cloth(c, 0.15f);
            Sphere("ŞapkaTepe", head, new Vector3(0, 0.1f, -0.005f), new Vector3(0.39f, 0.31f, 0.4f), felt);
            Cyl("ŞapkaKenar", head, new Vector3(0, 0.055f, 0.005f), new Vector3(0.44f, 0.012f, 0.46f), felt, new Vector3(-6, 0, 0), false);
            Cyl("ŞapkaBant", head, new Vector3(0, 0.085f, 0), new Vector3(0.395f, 0.016f, 0.405f), M.Get("hatband_dark", new Color(0.03f, 0.03f, 0.03f)), null, false);
        }

        static readonly Color[] CapColors = { new Color(0.22f, 0.2f, 0.18f), new Color(0.16f, 0.16f, 0.17f), new Color(0.28f, 0.22f, 0.16f), new Color(0.18f, 0.2f, 0.24f) };

        public static void FlatCap(Transform head, int hash, Color? color = null)
        {
            var mat = color.HasValue ? M.Get("cap_" + ColorUtility.ToHtmlStringRGB(color.Value), color.Value, 0.2f) : M.Get("cap_" + (hash % CapColors.Length), CapColors[hash % CapColors.Length], 0.2f);
            Cyl("Kasket", head, new Vector3(0, 0.15f, 0.01f), new Vector3(0.34f, 0.05f, 0.36f), mat, null, false);
            Box("KasketSiperi", head, new Vector3(0, 0.12f, 0.17f), new Vector3(0.24f, 0.02f, 0.11f), mat, null, false);
        }

        public static void Bowler(Transform head)
        {
            var felt = M.Get("bowler", new Color(0.07f, 0.065f, 0.06f), 0.3f);
            Sphere("MelonTepe", head, new Vector3(0, 0.17f, 0), new Vector3(0.36f, 0.28f, 0.37f), felt);
            Cyl("MelonKenar", head, new Vector3(0, 0.12f, 0), new Vector3(0.42f, 0.012f, 0.44f), felt, null, false);
            Cyl("MelonBant", head, new Vector3(0, 0.145f, 0), new Vector3(0.315f, 0.02f, 0.325f), M.Get("hatband_dark", new Color(0.03f, 0.03f, 0.03f)), null, false);
        }

        /// <summary>Kara film fötrü: önden kırık kenar, yanları hafif kalkık, tepesi ortadan ezik ve iki yandan tutamlı (dedektif.png).</summary>
        public static void Fedora(Transform head, Color? color = null)
        {
            var hc = color ?? new Color(0.11f, 0.105f, 0.1f);
            var felt = Tailoring.Cloth(hc, 0.3f);
            var hat = new GameObject("FötrŞapka");
            hat.transform.SetParent(head, false);
            hat.AddComponent<MeshFilter>().sharedMesh = FedoraFelt;
            hat.AddComponent<MeshRenderer>().sharedMaterial = felt;
            var band = new GameObject("FötrBant");
            band.transform.SetParent(head, false);
            band.AddComponent<MeshFilter>().sharedMesh = FedoraBand;
            band.AddComponent<MeshRenderer>().sharedMaterial = M.Get("hatband_noir", hc * 0.35f, 0.35f);
        }

        const float HatBase = 0.128f, HatCrown = 0.135f, CrownR = 0.137f, BrimR = 0.272f;
        static Mesh _fedoraFelt, _fedoraBand;

        /// <summary>Kenar yüksekliği: yanlarda kalkık, önde ve arkada aşağı kırık.</summary>
        static float BrimLift(float angle, float s)
        {
            float side = Mathf.Sin(angle), front = Mathf.Cos(angle);
            float lift = 0.03f * side * side * s * s;
            float dip = front > 0f ? 0.034f * front * front * s : 0.016f * front * front * s;
            return lift - dip;
        }

        static float CrownRadius(float angle, float t)
        {
            float a = Mathf.Abs(Mathf.DeltaAngle(0f, angle * Mathf.Rad2Deg));
            float pinch = Mathf.Exp(-Mathf.Pow((a - 38f) / 24f, 2f)) * 0.17f * t * t;
            float oval = Mathf.Lerp(1f, 1.14f, Mathf.Abs(Mathf.Cos(angle)));
            return Mathf.Lerp(CrownR, CrownR * 0.8f, t * t) * oval * (1f - pinch);
        }

        static Mesh FedoraFelt
        {
            get
            {
                if (_fedoraFelt != null) return _fedoraFelt;
                const int seg = 40, brimRings = 7, crownRings = 8, capRings = 6;
                var v = new List<Vector3>(); var uv = new List<Vector2>(); var t = new List<int>();
                void Stitch(int r0, int r1)
                {
                    for (int s = 0; s < seg; s++)
                    {
                        int a0 = r0 * (seg + 1) + s, b0 = r1 * (seg + 1) + s;
                        t.AddRange(new[] { a0, b0, a0 + 1, a0 + 1, b0, b0 + 1 });
                    }
                }
                int ring = 0;
                // Kenar: dıştan içe
                for (int i = 0; i <= brimRings; i++, ring++)
                {
                    float k = 1f - i / (float)brimRings;
                    for (int s = 0; s <= seg; s++)
                    {
                        float a = s / (float)seg * Mathf.PI * 2f;
                        float inner = CrownRadius(a, 0f);
                        float r = Mathf.Lerp(inner, BrimR * Mathf.Lerp(1f, 1.1f, Mathf.Abs(Mathf.Cos(a))), k);
                        v.Add(new Vector3(Mathf.Sin(a) * r, HatBase + BrimLift(a, k), Mathf.Cos(a) * r));
                        uv.Add(new Vector2(s / (float)seg * 4f, k));
                    }
                    if (i > 0) Stitch(ring - 1, ring);
                }
                // Tepe: yukarı doğru
                for (int i = 1; i <= crownRings; i++, ring++)
                {
                    float k = i / (float)crownRings;
                    for (int s = 0; s <= seg; s++)
                    {
                        float a = s / (float)seg * Mathf.PI * 2f;
                        float r = CrownRadius(a, k);
                        // Tepe sırtı önden arkaya alçalır
                        float slope = -0.024f * Mathf.Cos(a) * k * k;
                        v.Add(new Vector3(Mathf.Sin(a) * r, HatBase + HatCrown * k + slope, Mathf.Cos(a) * r));
                        uv.Add(new Vector2(s / (float)seg * 4f, 1f + k));
                    }
                    Stitch(ring - 1, ring);
                }
                // Üst kapak: ortası uzunlamasına ezik
                for (int i = 1; i <= capRings; i++, ring++)
                {
                    float k = 1f - i / (float)capRings;
                    for (int s = 0; s <= seg; s++)
                    {
                        float a = s / (float)seg * Mathf.PI * 2f;
                        float r = CrownRadius(a, 1f) * k;
                        float x = Mathf.Sin(a) * r, z = Mathf.Cos(a) * r;
                        float crease = 0.034f * Mathf.Exp(-(x * x) / (0.042f * 0.042f)) * Mathf.Clamp01(1f - Mathf.Abs(z) * 3.2f + 0.45f);
                        float slope = -0.024f * Mathf.Cos(a) * k;
                        v.Add(new Vector3(x, HatBase + HatCrown + slope + 0.012f * (1f - k) - crease * (1f - k * k * 0.75f), z));
                        uv.Add(new Vector2(s / (float)seg * 4f, 2f + (1f - k)));
                    }
                    Stitch(ring - 1, ring);
                }
                var m = new Mesh { name = "Fötr" };
                m.SetVertices(v); m.SetUVs(0, uv); m.SetTriangles(t, 0);
                m.RecalculateNormals(); m.RecalculateBounds();
                _fedoraFelt = m;
                return m;
            }
        }

        static Mesh FedoraBand
        {
            get
            {
                if (_fedoraBand != null) return _fedoraBand;
                const int seg = 40;
                var v = new List<Vector3>(); var uv = new List<Vector2>(); var t = new List<int>();
                for (int i = 0; i < 2; i++)
                    for (int s = 0; s <= seg; s++)
                    {
                        float a = s / (float)seg * Mathf.PI * 2f;
                        float k = i == 0 ? 0.015f : 0.19f;
                        float r = CrownRadius(a, k) + 0.0035f;
                        v.Add(new Vector3(Mathf.Sin(a) * r, HatBase + HatCrown * k, Mathf.Cos(a) * r));
                        uv.Add(new Vector2(s / (float)seg, i));
                    }
                for (int s = 0; s < seg; s++) t.AddRange(new[] { s, s + seg + 1, s + 1, s + 1, s + seg + 1, s + seg + 2 });
                var m = new Mesh { name = "FötrBant" };
                m.SetVertices(v); m.SetUVs(0, uv); m.SetTriangles(t, 0);
                m.RecalculateNormals(); m.RecalculateBounds();
                _fedoraBand = m;
                return m;
            }
        }

        static readonly Color[] ScarfColors = { new Color(0.3f, 0.12f, 0.1f), new Color(0.15f, 0.18f, 0.28f), new Color(0.75f, 0.72f, 0.65f), new Color(0.2f, 0.24f, 0.16f), new Color(0.12f, 0.1f, 0.1f) };

        public static void Headscarf(Transform head, int hash, Color? color = null)
        {
            var c = color ?? ScarfColors[hash % ScarfColors.Length];
            var mat = M.Get("scarf_" + ColorUtility.ToHtmlStringRGB(c), c, 0.1f);
            Sphere("Başörtüsü", head, new Vector3(0, 0.05f, -0.06f), new Vector3(0.42f, 0.44f, 0.42f), mat);
            Cyl("ÖrtüAlın", head, new Vector3(0, 0.1f, 0.02f), new Vector3(0.4f, 0.05f, 0.36f), mat, new Vector3(-20, 0, 0), false);
            Capsule("ÖrtüEteği", head, new Vector3(0, -0.19f, -0.11f), new Vector3(0.36f, 0.2f, 0.2f), mat);
        }
    }
}
