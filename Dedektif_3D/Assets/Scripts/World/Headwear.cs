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
        public enum Kind { None, Fez, FlatCap, Bowler, TopHat, Fedora, Headscarf, Beanie, PostCap, PoliceCap, HeadWrap }

        /// <summary>Kasabalı için anahtardan sabit başlık seçimi (erkekte fes ağırlıklı, kadında başörtüsü).</summary>
        public static Kind PickFor(int hash, bool female, bool child, bool indoors)
        {
            int r = (int)((uint)(hash * 747796405u + 2891336453u) >> 9) % 100;
            if (female) return !child && r < (indoors ? 30 : 45) ? Kind.Headscarf : Kind.None;
            if (child) return r < 30 ? Kind.FlatCap : Kind.None;
            if (indoors) return r < 35 ? Kind.Fez : Kind.None;   // içeride şapka çıkarılır, fes kalır
            return r < 32 ? Kind.Fez : r < 52 ? Kind.FlatCap : r < 64 ? Kind.Bowler : r < 69 ? Kind.TopHat : Kind.None;
        }

        public static void Put(Transform head, Kind k, int hash = 0, Color? color = null)
        {
            if (head == null) return;
            switch (k)
            {
                case Kind.Fez: Fez(head); break;
                case Kind.FlatCap: FlatCap(head, hash, color); break;
                case Kind.Bowler: Bowler(head); break;
                case Kind.TopHat: TownProps.TopHat(head); break;
                case Kind.Fedora: Fedora(head, color); break;
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
            Sphere("Bere", head, new Vector3(0, 0.1f, -0.02f), new Vector3(0.41f, 0.36f, 0.42f), mat);
            Cyl("BereKenarı", head, new Vector3(0, 0.06f, -0.02f), new Vector3(0.415f, 0.035f, 0.425f), mat, null, false);
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
            Sphere("SarıkTepe", head, new Vector3(0, 0.11f, -0.03f), new Vector3(0.41f, 0.34f, 0.42f), mat);
            Cyl("Sargı", head, new Vector3(0, 0.08f, -0.02f), new Vector3(0.43f, 0.06f, 0.44f), mat, new Vector3(-8, 0, 0), false);
        }

        public static void Fez(Transform head)
        {
            Cyl("Fes", head, new Vector3(0, 0.2f, -0.01f), new Vector3(0.27f, 0.1f, 0.27f), M.Get("fez", new Color(0.55f, 0.06f, 0.06f), 0.3f), null, false);
            Box("Püskül", head, new Vector3(0.1f, 0.22f, -0.09f), new Vector3(0.02f, 0.15f, 0.02f), M.Get("tassel", new Color(0.05f, 0.05f, 0.05f)), null, false);
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

        public static void Fedora(Transform head, Color? color = null)
        {
            var hc = color ?? new Color(0.12f, 0.11f, 0.1f);
            var hatMat = M.Get("fedora_" + ColorUtility.ToHtmlStringRGB(hc), hc, 0.25f);
            Cyl("HatBrim", head, new Vector3(0, 0.13f, 0), new Vector3(0.5f, 0.015f, 0.5f), hatMat, null, false);
            Cyl("HatCrown", head, new Vector3(0, 0.21f, 0), new Vector3(0.3f, 0.08f, 0.3f), hatMat, null, false);
            Cyl("HatBand", head, new Vector3(0, 0.155f, 0), new Vector3(0.305f, 0.02f, 0.305f), M.Get("hatband", new Color(0.4f, 0.1f, 0.1f)), null, false);
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
