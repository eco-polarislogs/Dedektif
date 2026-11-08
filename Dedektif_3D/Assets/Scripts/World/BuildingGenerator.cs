using Dedektif3D.Core;
using UnityEngine;
using static Dedektif3D.World.Shapes;
using M = Dedektif3D.World.MaterialLibrary;

namespace Dedektif3D.World
{
    /// <summary>
    /// Kasabanın 7 dükkânını (5 soruşturma binası + Kahvehane + Bakkal) town_map_generated.png ve town_bg.png
    /// görsellerindeki mimariye göre inşa eder: taş kaide, köşe taşları, ahşap karkas, öne taşan cumba katı,
    /// saçaklı beşik/kırma çatı, bacalar, kepenkli pencereler ve neon tabelalar. Ön cephe yerel -Z yönüne bakar.
    /// </summary>
    public static class BuildingGenerator
    {
        public class BuildingInstance
        {
            public BuildingDef Def;
            public Transform Root;
            public Transform DoorOutside;   // Binadan çıkınca oyuncunun duracağı nokta
            public Vector3 DoorWorldPos;    // Kapı etkileşim noktası
            public TextMesh StatusLabel;    // "İNCELEME TAMAMLANDI" / "MASUM" yazısı (web: building-hover-tag)
        }

        const float Plinth = 0.45f;

        public static BuildingInstance Build(BuildingDef def, Transform parent, Vector3 worldPos, float yRot)
        {
            var root = Group("Bina_" + def.DisplayName, parent, worldPos, yRot).transform;
            var inst = new BuildingInstance { Def = def, Root = root };
            float doorX = 0f;

            switch (def.Style)
            {
                case BuildingStyle.Eczane: doorX = BuildEczane(root, def); break;
                case BuildingStyle.Kasap: doorX = BuildKasap(root, def); break;
                case BuildingStyle.Terzi: doorX = BuildTerzi(root, def); break;
                case BuildingStyle.Muhtarlik: doorX = BuildMuhtarlik(root, def); break;
                case BuildingStyle.Karakol: doorX = BuildKarakol(root, def); break;
                case BuildingStyle.Kahvehane: doorX = BuildKahvehane(root, def); break;
                case BuildingStyle.Bakkal: doorX = BuildBakkal(root, def); break;
            }

            float fz = -def.Size.z / 2f;
            var outside = Group("DoorOutside", root, new Vector3(doorX, 0, fz - 2.2f), 180f).transform;
            inst.DoorOutside = outside;
            inst.DoorWorldPos = root.TransformPoint(new Vector3(doorX, 1f, fz - 1.0f));

            // Kapı önü renkli neon yansıması (web: glow-kasap, glow-eczane...)
            var glow = PointLight("KapıFeneri", root, new Vector3(doorX, 2.6f, fz - 1.6f), new Color(1f, 0.62f, 0.3f), 3.2f, 7f);
            root.gameObject.AddComponent<LightFlicker>().Init(glow, 0.1f);

            if (def.Enterable)
            {
                var label = Text3D("DurumYazısı", root, new Vector3(doorX, 3.0f, fz - 1.2f), "", new Color(1f, 0.85f, 0.4f), 0.03f);
                inst.StatusLabel = label;
            }
            return inst;
        }

        // =================================================================
        //  ORTAK MİMARİ PARÇALAR
        // =================================================================
        static void Base(Transform t, Vector3 s)
        {
            Box("TaşKaide", t, new Vector3(0, Plinth / 2, 0), new Vector3(s.x + 0.3f, Plinth, s.z + 0.3f), M.Get("plinth", new Color(0.32f, 0.31f, 0.3f), 0.35f, 0f, M.NoiseTexture), texTiling: 0.6f);
        }

        static void WallBlock(Transform t, string name, Vector3 center, Vector3 size, Material mat, bool collider = true)
            => Box(name, t, center, size, mat, collider: collider, texTiling: 0.55f);

        /// <summary>Köşe taşları (quoin): köşelerde dönüşümlü taş bloklar.</summary>
        static void Quoins(Transform t, Vector3 s, float y0, float h, float frontZ)
        {
            var stone = M.Get("quoin", new Color(0.55f, 0.53f, 0.5f), 0.2f, 0f, M.NoiseTexture);
            int n = Mathf.FloorToInt(h / 0.5f);
            foreach (var sx in new[] { -1, 1 })
                for (int i = 0; i < n; i++)
                {
                    bool wide = i % 2 == 0;
                    Box("KöşeTaşı", t, new Vector3(sx * (s.x / 2 - (wide ? 0.28f : 0.18f)), y0 + 0.25f + i * 0.5f, frontZ - 0.04f),
                        new Vector3(wide ? 0.6f : 0.4f, 0.44f, 0.1f), stone, collider: false);
                }
        }

        /// <summary>Yarı ahşap karkas: cephede yatay/dikey kirişler ve çaprazlar (town_map'teki evler).</summary>
        static void TimberFrame(Transform t, float width, float y0, float h, float faceZ, Material beam, bool diagonals = true)
        {
            Box("KirişAlt", t, new Vector3(0, y0 + 0.08f, faceZ - 0.04f), new Vector3(width, 0.16f, 0.1f), beam, collider: false);
            Box("KirişÜst", t, new Vector3(0, y0 + h - 0.08f, faceZ - 0.04f), new Vector3(width, 0.16f, 0.1f), beam, collider: false);
            int posts = Mathf.Max(2, Mathf.RoundToInt(width / 1.6f) + 1);
            for (int i = 0; i < posts; i++)
            {
                float x = -width / 2 + 0.08f + i * (width - 0.16f) / (posts - 1);
                Box("Dikme", t, new Vector3(x, y0 + h / 2, faceZ - 0.04f), new Vector3(0.14f, h, 0.1f), beam, collider: false);
            }
            if (!diagonals) return;
            float seg = (width - 0.16f) / (posts - 1);
            float diag = Mathf.Sqrt(seg * seg + h * h) * 0.92f;
            float ang = Mathf.Atan2(seg, h) * Mathf.Rad2Deg;
            Box("Çapraz", t, new Vector3(-width / 2 + 0.08f + seg / 2, y0 + h / 2, faceZ - 0.045f), new Vector3(0.11f, diag, 0.09f), beam, new Vector3(0, 0, -ang), false);
            Box("Çapraz", t, new Vector3(width / 2 - 0.08f - seg / 2, y0 + h / 2, faceZ - 0.045f), new Vector3(0.11f, diag, 0.09f), beam, new Vector3(0, 0, ang), false);
        }

        /// <summary>Cumba/çıkma altındaki eğik ahşap konsollar.</summary>
        static void Corbels(Transform t, float width, float y, float faceZ, float jut, Material beam)
        {
            int n = Mathf.Max(2, Mathf.RoundToInt(width / 1.4f) + 1);
            for (int i = 0; i < n; i++)
            {
                float x = -width / 2 + 0.2f + i * (width - 0.4f) / (n - 1);
                Box("Konsol", t, new Vector3(x, y - 0.35f, faceZ - jut * 0.5f), new Vector3(0.14f, 0.12f, jut * 1.2f), beam, new Vector3(-35, 0, 0), false);
            }
        }

        /// <summary>Çerçeveli, pervazlı, (isteğe bağlı) kepenkli pencere. Cam ya sıcak ışıkla yanar ya karanlıktır.</summary>
        static void Window(Transform t, float faceZ, float x, float y, float w, float h, bool lit, bool shutters = true, Color? shutterColor = null, bool arch = false)
        {
            var frame = M.Wood(new Color(0.18f, 0.12f, 0.08f));
            var g = Group("Pencere", t, new Vector3(x, y, faceZ)).transform;
            Box("Çerçeve", g, new Vector3(0, 0, -0.05f), new Vector3(w + 0.18f, h + 0.18f, 0.1f), frame, collider: false);
            Box("Cam", g, new Vector3(0, 0, -0.09f), new Vector3(w, h, 0.03f), lit ? M.WarmWindow : M.Glass, collider: false);
            Box("KayıtD", g, new Vector3(0, 0, -0.115f), new Vector3(0.05f, h, 0.03f), frame, collider: false);
            Box("KayıtY", g, new Vector3(0, h * 0.15f, -0.115f), new Vector3(w, 0.05f, 0.03f), frame, collider: false);
            Box("DenizlikAlt", g, new Vector3(0, -h / 2 - 0.1f, -0.15f), new Vector3(w + 0.35f, 0.07f, 0.26f), M.Stone, collider: false);
            Box("Lento", g, new Vector3(0, h / 2 + 0.12f, -0.1f), new Vector3(w + 0.3f, 0.12f, 0.16f), M.Stone, collider: false);
            if (arch) Cyl("Kemer", g, new Vector3(0, h / 2 + 0.05f, -0.08f), new Vector3(w + 0.1f, 0.05f, w * 0.5f), frame, new Vector3(90, 0, 0), false);
            if (shutters)
            {
                var sm = M.Wood(shutterColor ?? new Color(0.2f, 0.27f, 0.22f));
                Box("KepenkSol", g, new Vector3(-w / 2 - 0.27f, 0, -0.07f), new Vector3(0.42f, h + 0.08f, 0.05f), sm, collider: false);
                Box("KepenkSağ", g, new Vector3(w / 2 + 0.27f, 0, -0.07f), new Vector3(0.42f, h + 0.08f, 0.05f), sm, collider: false);
                for (int i = 0; i < 4; i++)
                {
                    float yy = -h / 2 + 0.2f + i * (h - 0.4f) / 3f;
                    Box("Panjur", g, new Vector3(-w / 2 - 0.27f, yy, -0.1f), new Vector3(0.36f, 0.04f, 0.02f), frame, collider: false);
                    Box("Panjur", g, new Vector3(w / 2 + 0.27f, yy, -0.1f), new Vector3(0.36f, 0.04f, 0.02f), frame, collider: false);
                }
            }
        }

        static void Door(Transform t, float faceZ, float x, Color doorColor, float w = 1.25f, float h = 2.3f, bool glass = false)
        {
            var g = Group("Kapı", t, new Vector3(x, Plinth, faceZ)).transform;
            var frame = M.Get("doorframe", new Color(0.42f, 0.4f, 0.37f), 0.25f, 0f, M.NoiseTexture);
            Box("PervazSol", g, new Vector3(-w / 2 - 0.12f, h / 2, -0.07f), new Vector3(0.22f, h + 0.1f, 0.18f), frame, collider: false);
            Box("PervazSağ", g, new Vector3(w / 2 + 0.12f, h / 2, -0.07f), new Vector3(0.22f, h + 0.1f, 0.18f), frame, collider: false);
            Box("Lento", g, new Vector3(0, h + 0.14f, -0.07f), new Vector3(w + 0.55f, 0.28f, 0.22f), frame, collider: false);
            Box("Kanat", g, new Vector3(0, h / 2, -0.03f), new Vector3(w, h, 0.07f), M.Wood(doorColor), collider: false);
            if (glass) Box("KapıCamı", g, new Vector3(0, h * 0.68f, -0.075f), new Vector3(w * 0.7f, h * 0.38f, 0.02f), M.WarmWindow, collider: false);
            else Box("PanoÜst", g, new Vector3(0, h * 0.7f, -0.075f), new Vector3(w * 0.7f, h * 0.32f, 0.02f), M.Wood(doorColor * 0.78f), collider: false);
            Box("PanoAlt", g, new Vector3(0, h * 0.26f, -0.075f), new Vector3(w * 0.7f, h * 0.3f, 0.02f), M.Wood(doorColor * 0.78f), collider: false);
            Sphere("Tokmak", g, new Vector3(w * 0.36f, h * 0.48f, -0.1f), Vector3.one * 0.08f, M.Get("brass", new Color(0.75f, 0.6f, 0.25f), 0.8f, 0.9f));
            Box("Basamak", g, new Vector3(0, -Plinth / 2, -0.55f), new Vector3(w + 0.9f, Plinth, 0.8f), M.Stone);
            // Kapı üstü duvar feneri
            WallLamp(g, new Vector3(w / 2 + 0.45f, h + 0.1f, -0.05f));
        }

        static void WallLamp(Transform t, Vector3 pos)
        {
            var g = Group("DuvarFeneri", t, pos).transform;
            var iron = M.Get("iron", new Color(0.08f, 0.08f, 0.09f), 0.5f, 0.7f);
            Box("Kol", g, new Vector3(0, 0, -0.18f), new Vector3(0.05f, 0.05f, 0.36f), iron, collider: false);
            Box("Kafes", g, new Vector3(0, -0.12f, -0.38f), new Vector3(0.2f, 0.28f, 0.2f), iron, collider: false);
            Box("Alev", g, new Vector3(0, -0.12f, -0.38f), new Vector3(0.15f, 0.22f, 0.15f), M.Emissive("lampglow_warm", new Color(1f, 0.72f, 0.4f), 3.5f), collider: false);
            Prism("Şapka", g, new Vector3(0, 0.02f, -0.38f), new Vector3(0.28f, 0.14f, 0.28f), iron);
            var l = PointLight("FenerIşığı", g, new Vector3(0, -0.2f, -0.6f), new Color(1f, 0.72f, 0.42f), 2.2f, 6f);
            g.gameObject.AddComponent<LightFlicker>().Init(l, 0.08f);
        }

        /// <summary>Neon tabela: koyu kutu + parlayan çerçeve + 3D yazı (web: KASAP, ECZANE... neonları).</summary>
        static void NeonSign(Transform t, string text, Color neon, Vector3 pos, float width, float height = 0.8f, float textSize = 0.07f, float yRot = 0f)
        {
            // 1800'ler: boyalı ahşap tabela, yaldız çerçeve, krem rengi harfler; üstünde gaz feneri (neon yok)
            var g = Group("Tabela", t, pos, yRot).transform;
            var paint = Color.Lerp(neon, new Color(0.08f, 0.07f, 0.06f), 0.78f);
            Box("Pano", g, Vector3.zero, new Vector3(width, height, 0.14f), M.Get("signpaint_" + ColorUtility.ToHtmlStringRGB(paint), paint, 0.35f), collider: false);
            var gilt = M.Get("gilt", new Color(0.72f, 0.55f, 0.22f), 0.75f, 0.85f);
            Box("YaldızÜst", g, new Vector3(0, height / 2 - 0.04f, -0.08f), new Vector3(width - 0.06f, 0.05f, 0.03f), gilt, collider: false);
            Box("YaldızAlt", g, new Vector3(0, -height / 2 + 0.04f, -0.08f), new Vector3(width - 0.06f, 0.05f, 0.03f), gilt, collider: false);
            Box("YaldızSol", g, new Vector3(-width / 2 + 0.04f, 0, -0.08f), new Vector3(0.05f, height - 0.06f, 0.03f), gilt, collider: false);
            Box("YaldızSağ", g, new Vector3(width / 2 - 0.04f, 0, -0.08f), new Vector3(0.05f, height - 0.06f, 0.03f), gilt, collider: false);
            Text3D("Yazı", g, new Vector3(0, 0, -0.1f), text, new Color(0.95f, 0.86f, 0.62f), textSize);
            var iron = M.Get("castiron", new Color(0.06f, 0.07f, 0.06f), 0.45f, 0.75f);
            Box("FenerKolu", g, new Vector3(0, height / 2 + 0.25f, -0.25f), new Vector3(0.04f, 0.04f, 0.5f), iron, collider: false);
            Box("Fener", g, new Vector3(0, height / 2 + 0.08f, -0.5f), new Vector3(0.18f, 0.26f, 0.18f), M.Emissive("gaslamp_glass", new Color(1f, 0.62f, 0.26f), 2.6f), collider: false);
            Prism("FenerŞapka", g, new Vector3(0, height / 2 + 0.22f, -0.5f), new Vector3(0.24f, 0.14f, 0.24f), iron);
            var l = PointLight("TabelaFeneri", g, new Vector3(0, height / 2 + 0.05f, -0.75f), new Color(1f, 0.62f, 0.28f), 3f, 6.5f);
            g.gameObject.AddComponent<LightFlicker>().Init(l, 0.12f);
        }

        /// <summary>Duvardan dik çıkan bayrak tabela (kasaptaki biftek, terzideki makas, karakoldaki yıldız).</summary>
        static Transform BladeSign(Transform t, Vector3 wallPos, Color neon)
        {
            var iron = M.Get("iron", new Color(0.08f, 0.08f, 0.09f), 0.5f, 0.7f);
            Box("TabelaKolu", t, wallPos + new Vector3(0.45f, 0.45f, 0), new Vector3(0.9f, 0.05f, 0.05f), iron, collider: false);
            var g = Group("BayrakTabela", t, wallPos + new Vector3(0.75f, 0, 0), 90f).transform;
            Box("Pano", g, Vector3.zero, new Vector3(0.75f, 0.75f, 0.06f), M.Get("signboard", new Color(0.06f, 0.06f, 0.07f), 0.4f), collider: false);
            PointLight("İkonIşığı", g, new Vector3(0, 0, 0), neon, 1.6f, 3.5f);
            return g;
        }

        static void Awning(Transform t, Vector3 pos, float width, Color a, Color b, float depth = 1.3f, float tilt = -22f)
        {
            var g = Group("Tente", t, pos).transform;
            int n = Mathf.Max(2, Mathf.RoundToInt(width / 0.5f));
            float sw = width / n;
            for (int i = 0; i < n; i++)
            {
                var c = i % 2 == 0 ? a : b;
                var mat = M.Get("awn_" + ColorUtility.ToHtmlStringRGB(c), c, 0.2f);
                Box("Şerit", g, new Vector3(-width / 2 + sw * (i + 0.5f), 0, -depth / 2), new Vector3(sw, 0.04f, depth), mat, new Vector3(tilt, 0, 0), false);
                Box("Saçak", g, new Vector3(-width / 2 + sw * (i + 0.5f), -0.48f, -depth * 0.95f), new Vector3(sw * 0.96f, 0.22f, 0.03f), mat, collider: false);
            }
        }

        static void Chimney(Transform t, Vector3 pos, float height, bool smoke)
        {
            Box("Baca", t, pos + Vector3.up * height / 2, new Vector3(0.65f, height, 0.65f), M.Brick(new Color(0.42f, 0.24f, 0.18f)), collider: false, texTiling: 1f);
            Box("BacaŞapka", t, pos + Vector3.up * (height + 0.06f), new Vector3(0.85f, 0.12f, 0.85f), M.Stone, collider: false);
            if (smoke) Effects.ChimneySmoke(t, pos + Vector3.up * (height + 0.2f));
        }

        static void Gutter(Transform t, Vector3 s, float topY, float frontZ)
        {
            var zinc = M.Get("zinc", new Color(0.3f, 0.32f, 0.34f), 0.6f, 0.7f);
            Box("Oluk", t, new Vector3(0, topY, frontZ - 0.55f), new Vector3(s.x + 0.9f, 0.1f, 0.12f), zinc, collider: false);
            Cyl("İnişBorusu", t, new Vector3(s.x / 2 + 0.05f, topY / 2, frontZ - 0.1f), new Vector3(0.09f, topY / 2, 0.09f), zinc, collider: false);
        }

        /// <summary>Saçaklı beşik çatı (mahya X boyunca) + alın tahtası.</summary>
        static void GableRoof(Transform t, float width, float depth, float baseY, float height, Material mat, float overhang = 0.55f)
        {
            Box("Saçak", t, new Vector3(0, baseY - 0.06f, 0), new Vector3(width + overhang * 2, 0.12f, depth + overhang * 2), M.Wood(new Color(0.2f, 0.13f, 0.08f)), collider: false);
            Prism("Çatı", t, new Vector3(0, baseY, 0), new Vector3(depth + overhang * 2, height, width + overhang * 2), mat, 90f, 0.9f);
            Box("Mahya", t, new Vector3(0, baseY + height, 0), new Vector3(width + overhang * 2 + 0.1f, 0.14f, 0.18f), M.RoofTiles(new Color(0.22f, 0.15f, 0.12f)), collider: false);
        }

        static void FlatRoof(Transform t, Vector3 s, float topY, Material parapet)
        {
            Box("Çatı", t, new Vector3(0, topY + 0.05f, 0), new Vector3(s.x, 0.1f, s.z), M.Get("roof_tar", new Color(0.13f, 0.13f, 0.14f), 0.5f));
            Box("KorkulukÖn", t, new Vector3(0, topY + 0.4f, -s.z / 2 + 0.12f), new Vector3(s.x + 0.2f, 0.8f, 0.24f), parapet, collider: false);
            Box("KorkulukArka", t, new Vector3(0, topY + 0.4f, s.z / 2 - 0.12f), new Vector3(s.x + 0.2f, 0.8f, 0.24f), parapet, collider: false);
            Box("KorkulukSol", t, new Vector3(-s.x / 2 + 0.12f, topY + 0.4f, 0), new Vector3(0.24f, 0.8f, s.z), parapet, collider: false);
            Box("KorkulukSağ", t, new Vector3(s.x / 2 - 0.12f, topY + 0.4f, 0), new Vector3(0.24f, 0.8f, s.z), parapet, collider: false);
            Box("Kornis", t, new Vector3(0, topY + 0.82f, -s.z / 2 + 0.1f), new Vector3(s.x + 0.45f, 0.1f, 0.4f), M.Stone, collider: false);
        }

        // =================================================================
        //  ECZANE — iki katlı, düz çatılı, yeşil sıvalı; yeşil neon haç (town_map)
        // =================================================================
        static float BuildEczane(Transform t, BuildingDef d)
        {
            var s = d.Size; float fz = -s.z / 2f; float h = s.y;
            var wall = M.Plaster(new Color(0.36f, 0.5f, 0.46f));
            Base(t, s);
            WallBlock(t, "Duvar", new Vector3(0, Plinth + h / 2, 0), new Vector3(s.x, h, s.z), wall);
            Quoins(t, s, Plinth, h, fz);
            Box("KatBandı", t, new Vector3(0, Plinth + 3.2f, fz - 0.06f), new Vector3(s.x + 0.1f, 0.22f, 0.16f), M.Stone, collider: false);
            FlatRoof(t, s, Plinth + h, M.Plaster(new Color(0.3f, 0.42f, 0.38f)));
            // Çatıda ahşap yağmur suyu fıçısı (1800'ler — klima/metal depo yok)
            Cyl("SuFıçısı", t, new Vector3(-2.5f, Plinth + h + 0.6f, 1.5f), new Vector3(1.0f, 0.55f, 1.0f), M.Wood(new Color(0.32f, 0.22f, 0.14f)), collider: false);
            Cyl("FıçıÇemberi", t, new Vector3(-2.5f, Plinth + h + 0.85f, 1.5f), new Vector3(1.04f, 0.04f, 1.04f), M.Get("iron", new Color(0.08f, 0.08f, 0.09f), 0.5f, 0.7f), collider: false);
            Chimney(t, new Vector3(3.3f, Plinth + h, -0.5f), 1.2f, false);

            // Zemin: geniş vitrin + cam kapı
            Window(t, fz, -2.2f, Plinth + 1.5f, 3.4f, 2.0f, true, false);
            Box("VitrinRafı", t, new Vector3(-2.2f, Plinth + 0.75f, fz - 0.2f), new Vector3(3.6f, 0.08f, 0.35f), M.Wood(new Color(0.3f, 0.2f, 0.12f)), collider: false);
            Door(t, fz, 2.2f, new Color(0.12f, 0.3f, 0.22f), 1.3f, 2.4f, true);
            for (int i = 0; i < 3; i++) Window(t, fz, -3f + i * 3f, Plinth + 4.9f, 1.1f, 1.45f, i != 1, true, new Color(0.13f, 0.3f, 0.22f));

            Awning(t, new Vector3(-2.2f, Plinth + 2.85f, fz - 0.05f), 3.8f, new Color(0.08f, 0.42f, 0.28f), new Color(0.88f, 0.9f, 0.86f));
            NeonSign(t, "ECZANE", d.Neon, new Vector3(0, Plinth + 3.75f, fz - 0.12f), 4.2f, 0.8f, 0.07f);

            // Köşeden çıkan dönen yeşil neon haç
            Box("HaçKolu", t, new Vector3(s.x / 2 + 0.35f, Plinth + 4.3f, fz + 0.9f), new Vector3(0.7f, 0.08f, 0.08f), M.Metal, collider: false);
            var cross = Group("YeşilHaç", t, new Vector3(s.x / 2 + 0.75f, Plinth + 4.3f, fz + 0.9f), 90f).transform;
            var crossMat = M.Emissive("apothecary_cross", new Color(0.2f, 0.55f, 0.3f), 1.1f);
            Box("HaçDikey", cross, Vector3.zero, new Vector3(0.4f, 1.25f, 0.22f), crossMat, collider: false);
            Box("HaçYatay", cross, Vector3.zero, new Vector3(1.25f, 0.4f, 0.22f), crossMat, collider: false);
            PointLight("HaçIşığı", cross, Vector3.zero, d.Neon, 3f, 7f);

            foreach (var x in new[] { 0.9f, 3.5f })
            {
                Cyl("Saksı", t, new Vector3(x, 0.3f, fz - 0.7f), new Vector3(0.5f, 0.3f, 0.5f), M.Get("terracotta", new Color(0.55f, 0.28f, 0.17f)), collider: false);
                Sphere("Çalı", t, new Vector3(x, 0.8f, fz - 0.7f), new Vector3(0.7f, 0.6f, 0.7f), M.Get("leaves", new Color(0.08f, 0.17f, 0.09f)));
            }
            Gutter(t, s, Plinth + h, fz + 0.45f);
            return 2.2f;
        }

        // =================================================================
        //  KASAP — tuğla zemin + ahşap karkaslı çıkma üst kat, dik beşik çatı, kırmızı neon + biftek tabelası
        // =================================================================
        static float BuildKasap(Transform t, BuildingDef d)
        {
            var s = d.Size; float fz = -s.z / 2f;
            float g = 3.2f, up = s.y - g, jut = 0.55f;
            var beam = M.Wood(new Color(0.17f, 0.11f, 0.07f));
            Base(t, s);
            WallBlock(t, "ZeminKat", new Vector3(0, Plinth + g / 2, 0), new Vector3(s.x, g, s.z), M.Brick(new Color(0.5f, 0.27f, 0.2f)));
            Box("BeyazÇini", t, new Vector3(0, Plinth + 0.6f, fz - 0.04f), new Vector3(s.x - 0.1f, 1.2f, 0.08f), M.Get("tiles_white", new Color(0.78f, 0.78f, 0.74f), 0.6f), collider: false);
            // Çıkma (cumba) üst kat
            WallBlock(t, "ÜstKat", new Vector3(0, Plinth + g + up / 2, -jut / 2), new Vector3(s.x + 0.2f, up, s.z + jut), M.Plaster(new Color(0.62f, 0.56f, 0.46f)));
            TimberFrame(t, s.x + 0.2f, Plinth + g, up, fz - jut, beam);
            Corbels(t, s.x, Plinth + g, fz, jut, beam);
            GableRoof(t, s.x + 0.2f, s.z + jut, Plinth + s.y, 3.0f, M.RoofTiles(new Color(0.38f, 0.17f, 0.12f)));
            Chimney(t, new Vector3(2.4f, Plinth + s.y + 0.6f, 1.6f), 2.6f, true);

            Window(t, fz, -2.1f, Plinth + 1.7f, 2.6f, 1.6f, true, false);
            Door(t, fz, 1.7f, new Color(0.4f, 0.1f, 0.08f));
            Window(t, fz - jut, -2.2f, Plinth + g + 1.5f, 1.1f, 1.3f, true, true, new Color(0.42f, 0.12f, 0.1f));
            Window(t, fz - jut, 2.2f, Plinth + g + 1.5f, 1.1f, 1.3f, false, true, new Color(0.42f, 0.12f, 0.1f));

            Awning(t, new Vector3(-0.6f, Plinth + 2.95f, fz - 0.05f), 5.6f, new Color(0.62f, 0.07f, 0.09f), new Color(0.88f, 0.88f, 0.85f));
            NeonSign(t, "KASAP", d.Neon, new Vector3(0, Plinth + g + 0.25f, fz - jut - 0.12f), 3.2f, 0.75f, 0.068f);
            var blade = BladeSign(t, new Vector3(s.x / 2, Plinth + 2.6f, fz - 0.3f), d.Neon);
            var red = M.Get("paint_steak", new Color(0.55f, 0.1f, 0.08f), 0.4f);
            Sphere("Biftek", blade, new Vector3(-0.05f, 0, -0.05f), new Vector3(0.42f, 0.32f, 0.05f), red);
            Cyl("Kemik", blade, new Vector3(0.22f, -0.08f, -0.06f), new Vector3(0.08f, 0.14f, 0.04f), M.Get("sign_bone", new Color(0.86f, 0.82f, 0.72f), 0.25f), new Vector3(0, 0, 60), false);

            Cyl("Kütük", t, new Vector3(-3.3f, 0.45f, fz - 1.3f), new Vector3(0.8f, 0.45f, 0.8f), M.Wood(new Color(0.42f, 0.28f, 0.15f)));
            PropFactory.Barrel(t, new Vector3(3.6f, 0, fz - 1.2f));
            PropFactory.Crate(t, new Vector3(3.1f, 0, fz - 2.1f), 0.7f);
            Gutter(t, s, Plinth + s.y, fz - jut + 0.45f);
            return 1.7f;
        }

        // =================================================================
        //  TERZİ — dar ve yüksek, hardal sıvalı, çıkmalı, sivri beşik çatı; sarı neon + makas tabelası
        // =================================================================
        static float BuildTerzi(Transform t, BuildingDef d)
        {
            var s = d.Size; float fz = -s.z / 2f;
            float g = 3.3f, up = s.y - g, jut = 0.6f;
            var beam = M.Wood(new Color(0.2f, 0.13f, 0.08f));
            Base(t, s);
            WallBlock(t, "ZeminKat", new Vector3(0, Plinth + g / 2, 0), new Vector3(s.x, g, s.z), M.Get("stonewall", new Color(0.44f, 0.4f, 0.36f), 0.15f, 0f, M.BrickTexture));
            WallBlock(t, "ÜstKat", new Vector3(0, Plinth + g + up / 2, -jut / 2), new Vector3(s.x + 0.2f, up, s.z + jut), M.Plaster(new Color(0.66f, 0.55f, 0.34f)));
            TimberFrame(t, s.x + 0.2f, Plinth + g, up, fz - jut, beam, false);
            Corbels(t, s.x, Plinth + g, fz, jut, beam);
            GableRoof(t, s.x + 0.2f, s.z + jut, Plinth + s.y, 3.6f, M.RoofTiles(new Color(0.24f, 0.22f, 0.24f)));
            Chimney(t, new Vector3(-2f, Plinth + s.y + 0.8f, 1.8f), 2.8f, true);

            Window(t, fz, -1.5f, Plinth + 1.6f, 2.4f, 2.0f, true, false);
            var mq = Group("VitrinManken", t, new Vector3(-1.5f, Plinth, fz + 0.55f)).transform;
            Cyl("Ayak", mq, new Vector3(0, 0.5f, 0), new Vector3(0.06f, 0.5f, 0.06f), M.Metal, collider: false);
            Capsule("Gövde", mq, new Vector3(0, 1.45f, 0), new Vector3(0.52f, 0.48f, 0.34f), M.Get("suit", new Color(0.2f, 0.18f, 0.3f)));
            Door(t, fz, 1.9f, new Color(0.32f, 0.22f, 0.36f), 1.15f, 2.3f, true);
            Window(t, fz - jut, -1.7f, Plinth + g + 1.5f, 1.0f, 1.4f, true, true, new Color(0.45f, 0.32f, 0.12f));
            Window(t, fz - jut, 1.7f, Plinth + g + 1.5f, 1.0f, 1.4f, true, true, new Color(0.45f, 0.32f, 0.12f));

            NeonSign(t, "TERZİ", d.Neon, new Vector3(0, Plinth + g + 0.25f, fz - jut - 0.12f), 3.0f, 0.72f, 0.065f);
            var blade = BladeSign(t, new Vector3(s.x / 2, Plinth + 2.5f, fz - 0.3f), d.Neon);
            var ym = M.Get("gilt", new Color(0.72f, 0.55f, 0.22f), 0.75f, 0.85f);
            Box("MakasA", blade, new Vector3(0, 0, -0.05f), new Vector3(0.06f, 0.55f, 0.03f), ym, new Vector3(0, 0, 25), false);
            Box("MakasB", blade, new Vector3(0, 0, -0.05f), new Vector3(0.06f, 0.55f, 0.03f), ym, new Vector3(0, 0, -25), false);
            Cyl("HalkaA", blade, new Vector3(-0.12f, -0.27f, -0.05f), new Vector3(0.14f, 0.01f, 0.14f), ym, new Vector3(90, 0, 0), false);
            Cyl("HalkaB", blade, new Vector3(0.12f, -0.27f, -0.05f), new Vector3(0.14f, 0.01f, 0.14f), ym, new Vector3(90, 0, 0), false);
            PropFactory.Crate(t, new Vector3(-3f, 0, fz - 1.1f), 0.6f);
            Gutter(t, s, Plinth + s.y, fz - jut + 0.45f);
            return 1.9f;
        }

        // =================================================================
        //  MUHTARLIK — taş zeminli, iki katlı, kırma çatılı resmi bina; beyaz tabela, saat, Türk bayrağı
        // =================================================================
        static float BuildMuhtarlik(Transform t, BuildingDef d)
        {
            var s = d.Size; float fz = -s.z / 2f; float h = s.y;
            Base(t, s);
            WallBlock(t, "ZeminKat", new Vector3(0, Plinth + 1.7f, 0), new Vector3(s.x, 3.4f, s.z), M.Get("ashlar", new Color(0.5f, 0.47f, 0.42f), 0.15f, 0f, M.BrickTexture));
            WallBlock(t, "ÜstKat", new Vector3(0, Plinth + 3.4f + (h - 3.4f) / 2, 0), new Vector3(s.x, h - 3.4f, s.z), M.Plaster(new Color(0.68f, 0.62f, 0.52f)));
            Quoins(t, s, Plinth + 3.4f, h - 3.4f, fz);
            Box("KatKornişi", t, new Vector3(0, Plinth + 3.45f, fz - 0.1f), new Vector3(s.x + 0.3f, 0.25f, 0.25f), M.Stone, collider: false);
            Box("Saçak", t, new Vector3(0, Plinth + h - 0.05f, 0), new Vector3(s.x + 1.1f, 0.14f, s.z + 1.1f), M.Wood(new Color(0.2f, 0.13f, 0.08f)), collider: false);
            HipRoof("Çatı", t, new Vector3(0, Plinth + h, 0), new Vector3(s.x + 1.1f, 2.6f, s.z + 1.1f), M.RoofTiles(new Color(0.3f, 0.2f, 0.17f)), 0.45f);
            Chimney(t, new Vector3(-2.8f, Plinth + h + 0.4f, 1.2f), 2.2f, true);
            Chimney(t, new Vector3(3.0f, Plinth + h + 0.4f, 1.2f), 2.2f, false);

            // Giriş: merdiven + sundurma
            for (int i = 0; i < 2; i++)
                Box("Merdiven", t, new Vector3(0, 0.12f + i * 0.15f, fz - 1.3f + i * 0.4f), new Vector3(3.2f - i * 0.4f, 0.24f + i * 0.0f, 0.6f), M.Stone);
            Door(t, fz, 0, new Color(0.3f, 0.18f, 0.09f), 1.6f, 2.6f, true);
            Box("SundurmaÇatı", t, new Vector3(0, Plinth + 3.0f, fz - 0.8f), new Vector3(3.0f, 0.14f, 1.6f), M.Wood(new Color(0.22f, 0.14f, 0.09f)), new Vector3(-8, 0, 0), false);
            foreach (var x in new[] { -1.3f, 1.3f })
                Box("SundurmaDikme", t, new Vector3(x, Plinth + 1.45f, fz - 1.45f), new Vector3(0.14f, 2.9f, 0.14f), M.Wood(new Color(0.22f, 0.14f, 0.09f)));

            for (int i = 0; i < 2; i++)
            {
                float x = i == 0 ? -3.2f : 3.2f;
                Window(t, fz, x, Plinth + 1.7f, 1.2f, 1.7f, true, false, null, true);
                Window(t, fz, x, Plinth + 4.9f, 1.1f, 1.4f, i == 0, true, new Color(0.25f, 0.2f, 0.15f));
            }
            Window(t, fz, 0, Plinth + 4.9f, 1.1f, 1.4f, true, true, new Color(0.25f, 0.2f, 0.15f));

            // Beyaz resmi tabela (haritada beyaz "MUHTARLIK" levhası) + saat
            var plate = Group("Levha", t, new Vector3(0, Plinth + 3.85f, fz - 0.1f)).transform;
            Box("LevhaZemin", plate, Vector3.zero, new Vector3(3.6f, 0.6f, 0.08f), M.Emissive("plate_white", new Color(0.95f, 0.93f, 0.86f), 0.5f), collider: false);
            Box("LevhaÇerçeve", plate, new Vector3(0, 0, 0.02f), new Vector3(3.75f, 0.72f, 0.06f), M.Get("brass", new Color(0.75f, 0.6f, 0.25f), 0.8f, 0.9f), collider: false);
            Text3D("LevhaYazı", plate, new Vector3(0, 0, -0.05f), "MUHTARLIK", new Color(0.12f, 0.1f, 0.08f), 0.055f);
            PointLight("LevhaIşığı", plate, new Vector3(0, 0.3f, -0.8f), d.Neon, 2.2f, 5f);

            var clock = Group("Saat", t, new Vector3(0, Plinth + h + 1.1f, fz - 0.15f)).transform;
            Cyl("Kadran", clock, Vector3.zero, new Vector3(0.9f, 0.04f, 0.9f), M.Emissive("clockface", new Color(1f, 0.95f, 0.8f), 0.9f), new Vector3(90, 0, 0), false);
            Cyl("Çember", clock, new Vector3(0, 0, 0.02f), new Vector3(1.0f, 0.03f, 1.0f), M.Get("brass", new Color(0.75f, 0.6f, 0.25f), 0.8f, 0.9f), new Vector3(90, 0, 0), false);
            var hour = Group("Akrep", clock, new Vector3(0, 0, -0.05f)).transform;
            Box("İbre", hour, new Vector3(0, 0.14f, 0), new Vector3(0.05f, 0.28f, 0.02f), M.Get("dark", new Color(0.05f, 0.05f, 0.05f)), collider: false);
            hour.localRotation = Quaternion.Euler(0, 0, 25);
            var minute = Group("Yelkovan", clock, new Vector3(0, 0, -0.06f)).transform;
            Box("İbre", minute, new Vector3(0, 0.19f, 0), new Vector3(0.035f, 0.38f, 0.02f), M.Get("dark", new Color(0.05f, 0.05f, 0.05f)), collider: false);
            var sp = minute.gameObject.AddComponent<Spinner>(); sp.axis = Vector3.forward; sp.speed = -6f;
            Box("SaatAlınlığı", t, new Vector3(0, Plinth + h + 0.75f, fz - 0.05f), new Vector3(1.5f, 1.5f, 0.2f), M.Plaster(new Color(0.68f, 0.62f, 0.52f)), collider: false);

            // Bayrak direği + ay-yıldızlı bayrak
            var pole = Group("BayrakDireği", t, new Vector3(-s.x / 2 - 1.0f, 0, fz - 1.2f)).transform;
            Cyl("Kaide", pole, new Vector3(0, 0.2f, 0), new Vector3(0.6f, 0.2f, 0.6f), M.Stone);
            Cyl("Direk", pole, new Vector3(0, 4.5f, 0), new Vector3(0.1f, 4.5f, 0.1f), M.Get("silver", new Color(0.8f, 0.8f, 0.85f), 0.9f, 1f));
            Sphere("Alem", pole, new Vector3(0, 9.05f, 0), Vector3.one * 0.2f, M.Get("brass", new Color(0.75f, 0.6f, 0.25f), 0.8f, 0.9f));
            var flag = Group("Bayrak", pole, new Vector3(0.05f, 8.2f, 0)).transform;
            flag.gameObject.AddComponent<FlagWave>();
            var flagRed = M.Get("flagred", new Color(0.8f, 0.04f, 0.08f), 0.2f);
            var flagWhite = M.Get("flagwhite", Color.white);
            Box("Kumaş", flag, new Vector3(1.1f, 0, 0), new Vector3(2.2f, 1.45f, 0.03f), flagRed, collider: false);
            Cyl("HilalBeyaz", flag, new Vector3(0.75f, 0, -0.025f), new Vector3(0.75f, 0.01f, 0.75f), flagWhite, new Vector3(90, 0, 0), false);
            Cyl("HilalKırmızı", flag, new Vector3(0.88f, 0, -0.03f), new Vector3(0.6f, 0.01f, 0.6f), flagRed, new Vector3(90, 0, 0), false);
            Box("Yıldız", flag, new Vector3(1.38f, 0, -0.03f), new Vector3(0.22f, 0.22f, 0.01f), flagWhite, new Vector3(0, 0, 45), false);
            Box("Yıldız2", flag, new Vector3(1.38f, 0, -0.031f), new Vector3(0.22f, 0.22f, 0.01f), flagWhite, Vector3.zero, false);
            PointLight("BayrakSpotu", pole, new Vector3(0.8f, 6.5f, -0.8f), new Color(1f, 0.9f, 0.75f), 2f, 5f);
            return 0f;
        }

        // =================================================================
        //  KARAKOL — iki katlı, düz çatılı kesme taş zaptiye karakolu; boyalı tabela + yıldız rozeti, gaz fenerleri, zaptiye arabası
        // =================================================================
        static float BuildKarakol(Transform t, BuildingDef d)
        {
            var s = d.Size; float fz = -s.z / 2f; float h = s.y;
            var concrete = M.Get("karakol_stone", new Color(0.5f, 0.5f, 0.5f), 0.2f, 0f, M.BrickTexture);   // kesme taş (beton 1800'lerde yok)
            Base(t, s);
            WallBlock(t, "Duvar", new Vector3(0, Plinth + h / 2, 0), new Vector3(s.x, h, s.z), concrete);
            var blue = M.Get("paint_navy", new Color(0.1f, 0.14f, 0.26f), 0.4f);
            Box("MaviŞerit", t, new Vector3(0, Plinth + 3.3f, fz - 0.06f), new Vector3(s.x + 0.12f, 0.3f, 0.12f), blue, collider: false);
            Box("MaviŞeritSol", t, new Vector3(-s.x / 2 - 0.06f, Plinth + 3.3f, 0), new Vector3(0.12f, 0.3f, s.z), blue, collider: false);
            Box("MaviŞeritSağ", t, new Vector3(s.x / 2 + 0.06f, Plinth + 3.3f, 0), new Vector3(0.12f, 0.3f, s.z), blue, collider: false);
            FlatRoof(t, s, Plinth + h, M.Get("karakol_stone_dark", new Color(0.36f, 0.35f, 0.34f), 0.2f, 0f, M.BrickTexture));

            // Kapı önünde ahşap sundurma (dökme demir sütunlar)
            Box("Saçak", t, new Vector3(0, Plinth + 2.9f, fz - 1.1f), new Vector3(3.8f, 0.2f, 2.2f), M.Wood(new Color(0.2f, 0.13f, 0.08f)));
            foreach (var x in new[] { -1.7f, 1.7f })
                Cyl("SaçakDireği", t, new Vector3(x, Plinth + 1.45f, fz - 2.05f), new Vector3(0.2f, 1.45f, 0.2f), M.Get("castiron", new Color(0.06f, 0.07f, 0.06f), 0.45f, 0.75f));
            Door(t, fz, 0, new Color(0.1f, 0.15f, 0.28f), 1.6f, 2.4f, true);
            Text3D("PolisYazısı", t, new Vector3(0, Plinth + 3.1f, fz - 2.22f), "ZAPTİYE", new Color(0.95f, 0.86f, 0.62f), 0.045f);

            for (int floor = 0; floor < 2; floor++)
                for (int i = 0; i < 4; i++)
                {
                    float x = -4.2f + i * 2.8f;
                    if (floor == 0 && Mathf.Abs(x) < 2.2f) continue;
                    float y = Plinth + (floor == 0 ? 1.7f : 5.0f);
                    Window(t, fz, x, y, 1.3f, 1.35f, (i + floor) % 2 == 0, false);
                    for (int b = 0; b < 4; b++)
                        Box("Parmaklık", t, new Vector3(x - 0.48f + b * 0.32f, y, fz - 0.19f), new Vector3(0.04f, 1.35f, 0.04f), M.Metal, collider: false);
                }

            NeonSign(t, "KARAKOL", d.Neon, new Vector3(0, Plinth + 4.0f, fz - 0.12f), 4.2f, 0.85f, 0.07f);
            var blade = BladeSign(t, new Vector3(s.x / 2, Plinth + 2.6f, fz - 0.3f), d.Neon);
            Cyl("Rozet", blade, new Vector3(0, 0, -0.05f), new Vector3(0.55f, 0.02f, 0.55f), M.Emissive("badge_gold", new Color(1f, 0.8f, 0.25f), 1.8f), new Vector3(90, 0, 0), false);
            Box("YıldızA", blade, new Vector3(0, 0, -0.08f), new Vector3(0.3f, 0.3f, 0.02f), M.Get("paint_navy", new Color(0.1f, 0.14f, 0.26f), 0.4f), new Vector3(0, 0, 45), false);
            Box("YıldızB", blade, new Vector3(0, 0, -0.08f), new Vector3(0.3f, 0.3f, 0.02f), M.Get("paint_navy", new Color(0.1f, 0.14f, 0.26f), 0.4f), Vector3.zero, false);

            // Çatıda iki gaz feneri (1800'ler karakolu — çakar yok)
            foreach (var x in new[] { -2f, 2f }) PropFactory.StreetLamp(t, new Vector3(x, Plinth + h + 0.1f, fz + 0.6f), new Color(1f, 0.6f, 0.26f), x < 0);
            TownProps.PoliceCar(t, new Vector3(-s.x / 2 + 0.8f, 0, fz - 4.3f), 75f, false);
            // Ahşap sehpa barikat (1800'ler)
            var bw = M.Wood(new Color(0.3f, 0.2f, 0.12f));
            Box("Bariyer", t, new Vector3(4.2f, 0.85f, fz - 3.3f), new Vector3(2.5f, 0.2f, 0.12f), bw, new Vector3(0, 10, 0));
            foreach (var sx in new[] { -1f, 1f })
                Box("BariyerAyak", t, new Vector3(4.2f + sx * 1.0f, 0.42f, fz - 3.3f + sx * 0.17f), new Vector3(0.1f, 0.9f, 0.5f), bw, new Vector3(0, 10, 0), false);
            Gutter(t, s, Plinth + h, fz + 0.45f);
            return 0f;
        }

        static void AddSpot(Transform parent, Quaternion rot, Color c)
        {
            var go = new GameObject("Spot"); go.transform.SetParent(parent, false); go.transform.localRotation = rot;
            var l = go.AddComponent<Light>(); l.type = LightType.Spot; l.color = c; l.intensity = 9f; l.range = 30f; l.spotAngle = 50f;
        }

        // =================================================================
        //  KAHVEHANE — (town_bg.png) kırmızı neon, tenteli, önünde masa-sandalye
        // =================================================================
        static float BuildKahvehane(Transform t, BuildingDef d)
        {
            var s = d.Size; float fz = -s.z / 2f;
            float g = 3.2f, up = s.y - g, jut = 0.6f;
            var beam = M.Wood(new Color(0.18f, 0.11f, 0.07f));
            Base(t, s);
            WallBlock(t, "ZeminKat", new Vector3(0, Plinth + g / 2, 0), new Vector3(s.x, g, s.z), M.Get("stonewall_dark", new Color(0.36f, 0.33f, 0.3f), 0.2f, 0f, M.BrickTexture));
            WallBlock(t, "ÜstKat", new Vector3(0, Plinth + g + up / 2, -jut / 2), new Vector3(s.x + 0.2f, up, s.z + jut), M.Wood(new Color(0.34f, 0.23f, 0.15f)));
            TimberFrame(t, s.x + 0.2f, Plinth + g, up, fz - jut, beam, false);
            Corbels(t, s.x, Plinth + g, fz, jut, beam);
            Box("Saçak", t, new Vector3(0, Plinth + s.y - 0.05f, -jut / 2), new Vector3(s.x + 1.2f, 0.14f, s.z + jut + 1.1f), beam, collider: false);
            HipRoof("Çatı", t, new Vector3(0, Plinth + s.y, -jut / 2), new Vector3(s.x + 1.2f, 2.4f, s.z + jut + 1.1f), M.RoofTiles(new Color(0.33f, 0.2f, 0.16f)), 0.35f);
            Chimney(t, new Vector3(2.6f, Plinth + s.y + 0.4f, 1.4f), 2f, true);

            Window(t, fz, -2.6f, Plinth + 1.6f, 2.4f, 1.9f, true, false);
            Window(t, fz, 2.7f, Plinth + 1.6f, 2.0f, 1.9f, true, false);
            Door(t, fz, 0.2f, new Color(0.3f, 0.16f, 0.1f), 1.3f, 2.4f, true);
            for (int i = 0; i < 3; i++) Window(t, fz - jut, -3f + i * 3f, Plinth + g + 1.4f, 1.0f, 1.3f, i != 2, true, new Color(0.3f, 0.18f, 0.12f));

            Awning(t, new Vector3(0, Plinth + 2.95f, fz - 0.05f), s.x - 0.4f, new Color(0.55f, 0.08f, 0.1f), new Color(0.55f, 0.08f, 0.1f) * 0.8f, 1.6f);
            NeonSign(t, "KAHVEHANE", d.Neon, new Vector3(0, Plinth + g + 0.3f, fz - jut - 0.12f), 4.2f, 0.7f, 0.06f);
            // Dış masa ve sandalyeler
            for (int i = 0; i < 2; i++)
                TownProps.CafeTable(t, new Vector3(-2.6f + i * 5.2f, 0, fz - 2.2f));
            return 0.2f;
        }

        // =================================================================
        //  BAKKAL — (town_bg.png) "BAKKAL" + "GECE AÇIK" neonu, meyve kasaları
        // =================================================================
        static float BuildBakkal(Transform t, BuildingDef d)
        {
            var s = d.Size; float fz = -s.z / 2f; float h = s.y;
            Base(t, s);
            WallBlock(t, "Duvar", new Vector3(0, Plinth + h / 2, 0), new Vector3(s.x, h, s.z), M.Get("stonewall_b", new Color(0.42f, 0.38f, 0.34f), 0.2f, 0f, M.BrickTexture));
            Quoins(t, s, Plinth, h, fz);
            Box("Saçak", t, new Vector3(0, Plinth + h - 0.05f, 0), new Vector3(s.x + 1.0f, 0.14f, s.z + 1.0f), M.Wood(new Color(0.2f, 0.13f, 0.08f)), collider: false);
            HipRoof("Çatı", t, new Vector3(0, Plinth + h, 0), new Vector3(s.x + 1.0f, 2.2f, s.z + 1.0f), M.RoofTiles(new Color(0.28f, 0.24f, 0.24f)), 0.3f);
            Chimney(t, new Vector3(-2.4f, Plinth + h + 0.3f, 1.2f), 1.8f, false);

            Window(t, fz, -1.8f, Plinth + 1.5f, 3.0f, 1.9f, true, false);
            Door(t, fz, 2.3f, new Color(0.2f, 0.14f, 0.1f), 1.2f, 2.3f, true);
            Window(t, fz, -2f, Plinth + 4.1f, 1.0f, 1.2f, false, true, new Color(0.3f, 0.12f, 0.1f));
            Window(t, fz, 2f, Plinth + 4.1f, 1.0f, 1.2f, true, true, new Color(0.3f, 0.12f, 0.1f));
            Awning(t, new Vector3(-0.3f, Plinth + 2.9f, fz - 0.05f), s.x - 0.6f, new Color(0.45f, 0.08f, 0.1f), new Color(0.82f, 0.8f, 0.74f), 1.4f);
            NeonSign(t, "BAKKAL", new Color(0.95f, 0.95f, 0.9f), new Vector3(0, Plinth + 3.55f, fz - 0.12f), 3.4f, 0.7f, 0.062f);
            NeonSign(t, "GECE AÇIK", d.Neon, new Vector3(2.3f, Plinth + 2.65f, fz - 0.6f), 1.5f, 0.42f, 0.03f);
            // Meyve-sebze kasaları
            Color[] fruit = { new Color(0.8f, 0.25f, 0.1f), new Color(0.9f, 0.6f, 0.1f), new Color(0.35f, 0.55f, 0.15f) };
            for (int i = 0; i < 3; i++)
            {
                var p = new Vector3(-3.0f + i * 1.0f, 0, fz - 1.0f);
                PropFactory.Crate(t, p, 0.6f);
                for (int k = 0; k < 5; k++)
                    Sphere("Meyve", t, p + new Vector3(-0.18f + (k % 3) * 0.18f, 0.65f, -0.1f + (k / 3) * 0.2f), Vector3.one * 0.16f, M.Get("fruit" + i, fruit[i], 0.5f));
            }
            return 2.3f;
        }
    }

    /// <summary>Bayrağın rüzgârda dalgalanması.</summary>
    public class FlagWave : MonoBehaviour
    {
        void Update() => transform.localRotation = Quaternion.Euler(0, Mathf.Sin(Time.time * 1.7f) * 12f, Mathf.Sin(Time.time * 2.3f) * 2f);
    }
}
