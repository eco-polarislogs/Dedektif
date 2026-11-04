using UnityEngine;
using static Dedektif3D.World.Shapes;
using M = Dedektif3D.World.MaterialLibrary;

namespace Dedektif3D.World
{
    /// <summary>
    /// town_bg.png ve town_map_generated.png'deki kasaba dekorları: polis aracı, taksi, kahvehane masaları,
    /// yön tabelaları, cami (kubbe + minareler), ambar ve silo, su birikintileri, ceset örtüsü, Osmanlı cumbalı evleri.
    /// </summary>
    public static class TownProps
    {
        static Material Iron => M.Get("iron", new Color(0.08f, 0.08f, 0.09f), 0.5f, 0.7f);

        // =================================================================
        //  ARAÇLAR
        // =================================================================
        /// <summary>1800'ler: iki fenerli, atlı zaptiye (polis) arabası.</summary>
        public static Transform PoliceCar(Transform parent, Vector3 pos, float yRot, bool flashing)
        {
            var car = Carriage(parent, pos, yRot, M.Get("carriage_black", new Color(0.05f, 0.05f, 0.06f), 0.7f, 0.2f), true);
            car.name = "ZaptiyeArabası";
            Text3D("Yazı", car, new Vector3(-0.8f, 1.2f, -0.6f), "ZAPTİYE", new Color(0.8f, 0.65f, 0.3f), 0.02f, 90f);
            return car;
        }

        /// <summary>1800'ler: fayton (taksi yerine).</summary>
        public static Transform Taxi(Transform parent, Vector3 pos, float yRot)
        {
            var car = Carriage(parent, pos, yRot, M.Get("carriage_wine", new Color(0.25f, 0.06f, 0.07f), 0.6f, 0.2f), false);
            car.name = "Fayton";
            return car;
        }

        static Transform Carriage(Transform parent, Vector3 pos, float yRot, Material body, bool closed)
        {
            var car = Group("Araba", parent, pos, yRot).transform;
            var wood = M.Wood(new Color(0.22f, 0.14f, 0.08f));
            var brass = M.Get("brass", new Color(0.75f, 0.6f, 0.25f), 0.8f, 0.9f);
            Box("Şasi", car, new Vector3(0, 0.75f, -0.4f), new Vector3(1.5f, 0.2f, 2.6f), wood);
            if (closed)
            {
                Box("Kabin", car, new Vector3(0, 1.45f, -0.6f), new Vector3(1.55f, 1.2f, 1.8f), body);
                Box("KabinTavan", car, new Vector3(0, 2.1f, -0.6f), new Vector3(1.7f, 0.1f, 1.95f), body, collider: false);
                foreach (var x in new[] { -0.79f, 0.79f })
                    Box("KabinPenceresi", car, new Vector3(x, 1.55f, -0.6f), new Vector3(0.03f, 0.5f, 0.7f), M.WarmWindow, collider: false);
            }
            else
            {
                Box("Koltuk", car, new Vector3(0, 1.1f, -0.9f), new Vector3(1.4f, 0.5f, 0.9f), body);
                Prism("Körük", car, new Vector3(0, 1.35f, -1.2f), new Vector3(0.9f, 0.9f, 1.5f), M.Get("hood_leather", new Color(0.08f, 0.07f, 0.06f), 0.4f), 90f);
            }
            Box("ArabacıKoltuğu", car, new Vector3(0, 1.25f, 0.75f), new Vector3(1.2f, 0.12f, 0.5f), wood, collider: false);
            foreach (var x in new[] { -0.82f, 0.82f })
            {
                Cyl("ArkaTekerlek", car, new Vector3(x, 0.62f, -1.3f), new Vector3(1.24f, 0.05f, 1.24f), wood, new Vector3(0, 0, 90), false);
                Cyl("ÖnTekerlek", car, new Vector3(x, 0.48f, 0.6f), new Vector3(0.96f, 0.05f, 0.96f), wood, new Vector3(0, 0, 90), false);
                Cyl("Göbek", car, new Vector3(x * 1.05f, 0.62f, -1.3f), new Vector3(0.2f, 0.06f, 0.2f), brass, new Vector3(0, 0, 90), false);
                Box("ArabaFeneri", car, new Vector3(x * 0.95f, 1.6f, 0.95f), new Vector3(0.16f, 0.24f, 0.16f), M.Emissive("gaslamp_glass", new Color(1f, 0.62f, 0.26f), 2.6f), collider: false);
            }
            var lamp = PointLight("ArabaFeneriIşığı", car, new Vector3(0, 1.6f, 1.3f), new Color(1f, 0.62f, 0.28f), 3f, 7f);
            lamp.gameObject.AddComponent<LightFlicker>().Init(lamp, 0.15f);
            Box("Ok1", car, new Vector3(-0.35f, 0.8f, 2.0f), new Vector3(0.06f, 0.06f, 2.2f), wood, collider: false);
            Box("Ok2", car, new Vector3(0.35f, 0.8f, 2.0f), new Vector3(0.06f, 0.06f, 2.2f), wood, collider: false);
            RuralBuildings.Animal(car, new Vector3(0, 0, 2.9f), 0f, new Color(0.12f, 0.09f, 0.07f), true);
            var driver = PropFactory.BuildPerson("Arabacı", car, new Vector3(0, 0.62f, 0.75f), new Color(0.12f, 0.11f, 0.1f), new Color(0.3f, 0.2f, 0.15f), false, null, Core.FaceLibrary.NextTownsfolkFace(), "sit");
            if (driver.HasModel) driver.Root.localPosition = new Vector3(0, 0.17f, 0.75f);
            driver.LegL.localRotation = Quaternion.Euler(-80, 0, 0); driver.LegR.localRotation = Quaternion.Euler(-80, 0, 0);
            TopHat(driver.Head);
            return car;
        }

        public static void TopHat(Transform head)
        {
            var felt = M.Get("tophat", new Color(0.05f, 0.05f, 0.05f), 0.35f);
            Cyl("ŞapkaKenarı", head, new Vector3(0, 0.13f, 0), new Vector3(0.42f, 0.012f, 0.42f), felt, null, false);
            Cyl("Silindir", head, new Vector3(0, 0.28f, 0), new Vector3(0.27f, 0.15f, 0.27f), felt, null, false);
            Cyl("Bant", head, new Vector3(0, 0.16f, 0), new Vector3(0.275f, 0.025f, 0.275f), M.Get("hatband", new Color(0.3f, 0.08f, 0.08f)), null, false);
        }

        // =================================================================
        //  KAHVEHANE MASASI (oturan müşterilerle)
        // =================================================================
        public static void CafeTable(Transform parent, Vector3 pos)
        {
            var g = Group("KahveMasası", parent, pos).transform;
            var wood = M.Wood(new Color(0.35f, 0.22f, 0.12f));
            Cyl("Tabla", g, new Vector3(0, 0.72f, 0), new Vector3(0.8f, 0.03f, 0.8f), wood);
            Cyl("Ayak", g, new Vector3(0, 0.36f, 0), new Vector3(0.08f, 0.36f, 0.08f), Iron, collider: false);
            Cyl("ÇayBardağı", g, new Vector3(0.15f, 0.8f, 0.05f), new Vector3(0.06f, 0.05f, 0.06f), M.Get("tea", new Color(0.6f, 0.15f, 0.05f), 0.9f), collider: false);
            Box("Tavla", g, new Vector3(-0.1f, 0.76f, -0.05f), new Vector3(0.4f, 0.04f, 0.3f), M.Wood(new Color(0.5f, 0.32f, 0.15f)), collider: false);
            for (int i = 0; i < 2; i++)
            {
                float sx = i == 0 ? -0.75f : 0.75f;
                Box("Tabure", g, new Vector3(sx, 0.4f, 0), new Vector3(0.38f, 0.06f, 0.38f), wood, collider: false);
                Box("TabureAyak", g, new Vector3(sx, 0.2f, 0), new Vector3(0.05f, 0.4f, 0.05f), wood, collider: false);
                var p = PropFactory.BuildPerson("Müşteri", g, new Vector3(sx, -0.42f, 0), Random.value > 0.5f ? new Color(0.25f, 0.24f, 0.22f) : new Color(0.2f, 0.25f, 0.32f), new Color(0.3f, 0.2f, 0.15f), false, null, Core.FaceLibrary.NextTownsfolkFace(), "sit");
                if (p.HasModel) p.Root.localPosition = new Vector3(sx, 0f, 0);
                p.Root.localRotation = Quaternion.Euler(0, i == 0 ? 90 : -90, 0);
                p.LegL.localRotation = Quaternion.Euler(-80, 0, 0);
                p.LegR.localRotation = Quaternion.Euler(-80, 0, 0);
                p.ArmL.localRotation = Quaternion.Euler(-45, 0, 0);
                if (i == 0 && !p.HasModel) Cyl("Kasket", p.Head, new Vector3(0, 0.15f, 0.02f), new Vector3(0.34f, 0.05f, 0.36f), M.Get("cap", new Color(0.22f, 0.2f, 0.18f)), collider: false);
            }
        }

        // =================================================================
        //  YÖN TABELASI (town_map: GÖLGEŞEHİR, SİSÖREN, YIKIKKÖY, DARSOKAK, KARAORMAN)
        // =================================================================
        public static void Signpost(Transform parent, Vector3 pos, float yRot, params string[] arrows)
        {
            var g = Group("YönTabelası", parent, pos, yRot).transform;
            var wood = M.Wood(new Color(0.3f, 0.2f, 0.12f));
            Box("Direk", g, new Vector3(0, 1.4f, 0), new Vector3(0.14f, 2.8f, 0.14f), wood);
            for (int i = 0; i < arrows.Length; i++)
            {
                bool right = i % 2 == 0;
                float y = 2.45f - i * 0.45f;
                var board = Group("Ok", g, new Vector3(right ? 0.75f : -0.75f, y, 0), right ? 0 : 180).transform;
                Box("Tahta", board, Vector3.zero, new Vector3(1.4f, 0.32f, 0.05f), M.Wood(new Color(0.45f, 0.32f, 0.18f)), collider: false);
                Prism("Uç", board, new Vector3(0.78f, -0.16f, 0), new Vector3(0.32f, 0.32f, 0.05f), M.Wood(new Color(0.45f, 0.32f, 0.18f)), 0);
                board.Find("Uç").localRotation = Quaternion.Euler(0, 0, -90);
                Text3D("Yazı", board, new Vector3(0, 0, -0.035f), arrows[i], new Color(0.95f, 0.9f, 0.78f), 0.018f);
                Text3D("YazıArka", board, new Vector3(0, 0, 0.035f), arrows[i], new Color(0.95f, 0.9f, 0.78f), 0.018f, 180f);
            }
        }

        // =================================================================
        //  CAMİ — town_bg.png arka planındaki kubbe ve iki minare (ışıklı şerefeler)
        // =================================================================
        public static void Mosque(Transform parent, Vector3 pos, float yRot)
        {
            var g = Group("Cami", parent, pos, yRot).transform;
            var stone = M.Get("mosque_stone", new Color(0.62f, 0.6f, 0.56f), 0.2f, 0f, M.NoiseTexture);
            var lead = M.Get("mosque_lead", new Color(0.3f, 0.34f, 0.38f), 0.55f, 0.5f);
            var glow = M.Emissive("mosque_glow", new Color(1f, 0.78f, 0.45f), 2.2f);
            Box("Gövde", g, new Vector3(0, 4f, 0), new Vector3(16, 8, 16), stone, texTiling: 0.3f);
            Cyl("Kasnak", g, new Vector3(0, 8.8f, 0), new Vector3(11f, 0.8f, 11f), stone);
            Sphere("Kubbe", g, new Vector3(0, 9.6f, 0), new Vector3(11f, 9f, 11f), lead);
            Cyl("Alem", g, new Vector3(0, 14.6f, 0), new Vector3(0.15f, 0.6f, 0.15f), M.Get("brass", new Color(0.75f, 0.6f, 0.25f), 0.8f, 0.9f), collider: false);
            for (int i = 0; i < 4; i++)
            {
                var d = Quaternion.Euler(0, i * 90, 0) * new Vector3(0, 0, 8.01f);
                Sphere("YarımKubbe", g, d * 0.85f + Vector3.up * 8f, new Vector3(5f, 4f, 5f), lead);
                for (int k = -2; k <= 2; k++)
                {
                    var side = Quaternion.Euler(0, i * 90, 0) * new Vector3(k * 2.6f, 4.5f, 8.05f);
                    Box("Pencere", g, side, new Vector3(1.0f, 2.2f, 0.05f), glow, new Vector3(0, i * 90, 0), false);
                }
            }
            for (int i = 0; i < 8; i++)
            {
                var p = Quaternion.Euler(0, i * 45, 0) * new Vector3(0, 9.1f, 5.6f);
                Box("KasnakPenceresi", g, p, new Vector3(0.6f, 0.6f, 0.05f), glow, new Vector3(0, i * 45, 0), false);
            }
            foreach (var x in new[] { -10f, 10f })
            {
                var m = Group("Minare", g, new Vector3(x, 0, 7f)).transform;
                Cyl("Kaide", m, new Vector3(0, 1.5f, 0), new Vector3(2.2f, 1.5f, 2.2f), stone);
                Cyl("Gövde", m, new Vector3(0, 12f, 0), new Vector3(1.4f, 9f, 1.4f), stone);
                Cyl("Şerefe", m, new Vector3(0, 17.5f, 0), new Vector3(2.2f, 0.25f, 2.2f), stone, collider: false);
                Cyl("ŞerefeIşığı", m, new Vector3(0, 17.9f, 0), new Vector3(1.6f, 0.3f, 1.6f), glow, collider: false);
                Cyl("ÜstGövde", m, new Vector3(0, 20.5f, 0), new Vector3(1.1f, 2.5f, 1.1f), stone, collider: false);
                Prism("Külah", m, new Vector3(0, 23f, 0), new Vector3(1.2f, 4.5f, 1.2f), lead);
                Prism("Külah2", m, new Vector3(0, 23f, 0), new Vector3(1.2f, 4.5f, 1.2f), lead, 90f);
                PointLight("ŞerefeLambası", m, new Vector3(0, 18.4f, 0), new Color(1f, 0.75f, 0.45f), 3f, 9f);
            }
            PointLight("CamiAydınlatma", g, new Vector3(0, 6f, 12f), new Color(1f, 0.8f, 0.55f), 4f, 20f);
        }

        // =================================================================
        //  ÇİFTLİK: ambar + silo + çit (town_map kuzeyi)
        // =================================================================
        public static void Barn(Transform parent, Vector3 pos, float yRot)
        {
            var g = Group("Ambar", parent, pos, yRot).transform;
            var plank = M.Wood(new Color(0.36f, 0.16f, 0.12f));
            Box("Gövde", g, new Vector3(0, 2.5f, 0), new Vector3(9, 5, 7), plank, texTiling: 0.4f);
            Box("Saçak", g, new Vector3(0, 4.95f, 0), new Vector3(9.8f, 0.12f, 7.8f), M.Wood(new Color(0.2f, 0.12f, 0.08f)), collider: false);
            Prism("Çatı", g, new Vector3(0, 5f, 0), new Vector3(7.8f, 3f, 9.8f), M.RoofTiles(new Color(0.22f, 0.2f, 0.2f)), 90f, 0.6f);
            Box("BüyükKapı", g, new Vector3(0, 1.8f, -3.52f), new Vector3(3f, 3.6f, 0.08f), M.Wood(new Color(0.42f, 0.3f, 0.18f)), collider: false);
            Box("Çapraz1", g, new Vector3(0, 1.8f, -3.58f), new Vector3(0.14f, 4.4f, 0.04f), M.Wood(new Color(0.6f, 0.55f, 0.45f)), new Vector3(0, 0, 40), false);
            Box("Çapraz2", g, new Vector3(0, 1.8f, -3.58f), new Vector3(0.14f, 4.4f, 0.04f), M.Wood(new Color(0.6f, 0.55f, 0.45f)), new Vector3(0, 0, -40), false);
            Box("Pencere", g, new Vector3(0, 4.6f, -3.52f), new Vector3(1f, 0.8f, 0.05f), M.WarmWindow, collider: false);
            for (int i = 0; i < 3; i++) Cyl("SamanBalyası", g, new Vector3(-5.5f, 0.55f, -2f + i * 1.3f), new Vector3(1.1f, 0.6f, 1.1f), M.Get("hay", new Color(0.62f, 0.5f, 0.25f)), new Vector3(0, 0, 90));
            var silo = Group("Silo", g, new Vector3(6.5f, 0, 1.5f)).transform;
            Cyl("SiloGövde", silo, new Vector3(0, 4.5f, 0), new Vector3(3f, 4.5f, 3f), M.Get("silo", new Color(0.4f, 0.42f, 0.44f), 0.45f, 0.5f));
            Sphere("SiloKubbe", silo, new Vector3(0, 9f, 0), new Vector3(3f, 2f, 3f), M.Get("silo", new Color(0.4f, 0.42f, 0.44f), 0.45f, 0.5f));
            for (int i = 0; i < 4; i++) Cyl("Çember", silo, new Vector3(0, 1.5f + i * 2f, 0), new Vector3(3.05f, 0.05f, 3.05f), M.Metal, collider: false);
        }

        // =================================================================
        //  SU BİRİKİNTİSİ — ıslak parkeye düşen lamba yansımaları için çok parlak koyu disk
        // =================================================================
        public static void Puddle(Transform parent, Vector3 pos, float size)
        {
            // Gökyüzünü ve lambaları yansıtan ince su tabakası (siyah delik gibi görünmesin)
            var mat = M.Get("puddle", new Color(0.22f, 0.25f, 0.3f), 0.97f, 0.85f);
            Cyl("SuBirikintisi", parent, pos + Vector3.up * 0.065f, new Vector3(size, 0.004f, size * Random.Range(0.55f, 0.9f)), mat, new Vector3(0, Random.Range(0, 180f), 0), false);
        }

        // =================================================================
        //  KASABA EVLERİ
        // =================================================================
        // ---- Gerçekçi ev parçaları ----
        static readonly Color[] HousePaints =
        {
            new Color(0.76f, 0.63f, 0.44f),   // aşı boyası / hardal
            new Color(0.74f, 0.58f, 0.52f),   // gül kurusu
            new Color(0.63f, 0.67f, 0.68f),   // solgun mavi-gri
            new Color(0.8f, 0.76f, 0.67f),    // kireç badana
            new Color(0.62f, 0.52f, 0.41f),   // toprak
            new Color(0.7f, 0.68f, 0.55f),    // zeytin badana
        };

        /// <summary>Derinlikli pervaz, kayıtlı cam, taş denizlik, lento; ahşap kepenk ya da demir parmaklık.</summary>
        static void HouseWindow(Transform t, float faceZ, float x, float y, float w, float h, bool lit, Material frame, Material shutter, bool bars = false)
        {
            var g = Group("Pencere", t, new Vector3(x, y, faceZ)).transform;
            Box("Pervaz", g, new Vector3(0, 0, -0.035f), new Vector3(w + 0.16f, h + 0.16f, 0.07f), frame, null, false);
            Box("Cam", g, new Vector3(0, 0, -0.075f), new Vector3(w, h, 0.02f), lit ? M.WarmWindow : M.Glass, null, false);
            Box("KayıtD", g, new Vector3(0, 0, -0.09f), new Vector3(0.04f, h, 0.02f), frame, null, false);
            Box("KayıtY", g, new Vector3(0, h * 0.2f, -0.09f), new Vector3(w, 0.04f, 0.02f), frame, null, false);
            Box("KayıtY2", g, new Vector3(0, -h * 0.18f, -0.09f), new Vector3(w, 0.03f, 0.02f), frame, null, false);
            Box("Denizlik", g, new Vector3(0, -h / 2f - 0.08f, -0.11f), new Vector3(w + 0.3f, 0.06f, 0.22f), M.Stone, null, false);
            Box("Lento", g, new Vector3(0, h / 2f + 0.1f, -0.06f), new Vector3(w + 0.28f, 0.1f, 0.12f), frame, null, false);
            if (bars)
            {
                var iron = M.Get("iron", new Color(0.08f, 0.08f, 0.09f), 0.5f, 0.7f);
                for (int i = 0; i < 4; i++) Cyl("Parmaklık", g, new Vector3(-w / 2f + w * (i + 0.5f) / 4f, 0, -0.13f), new Vector3(0.025f, h / 2f, 0.025f), iron, null, false);
                Box("ParmaklıkY", g, new Vector3(0, h * 0.25f, -0.13f), new Vector3(w, 0.03f, 0.03f), iron, null, false);
                Box("ParmaklıkY", g, new Vector3(0, -h * 0.25f, -0.13f), new Vector3(w, 0.03f, 0.03f), iron, null, false);
            }
            else if (shutter != null)
            {
                float open = Random.Range(0f, 1f);
                foreach (var sx in new[] { -1f, 1f })
                {
                    var leaf = Group("Kepenk", g, new Vector3(sx * (w / 2f + 0.06f), 0, -0.07f), sx * Mathf.Lerp(150f, 95f, open)).transform;
                    Box("Kanat", leaf, new Vector3(0, 0, -w * 0.25f), new Vector3(0.04f, h + 0.06f, w * 0.5f), shutter, null, false);
                    for (int i = 0; i < 5; i++)
                        Box("Panjur", leaf, new Vector3(-0.03f * sx, -h / 2f + 0.15f + i * (h - 0.3f) / 4f, -w * 0.25f), new Vector3(0.02f, 0.035f, w * 0.44f), frame, null, false);
                }
            }
        }

        /// <summary>Taş pervazlı, panelli ahşap kapı, taş basamak ve (isteğe bağlı) küçük sundurma.</summary>
        static void HouseDoor(Transform t, float faceZ, float x, Material frame, Color doorColor, bool canopy)
        {
            var g = Group("Kapı", t, new Vector3(x, 0, faceZ)).transform;
            var stone = M.Stone;
            const float w = 1.05f, h = 2.15f;
            Box("PervazSol", g, new Vector3(-w / 2f - 0.1f, h / 2f + 0.15f, -0.05f), new Vector3(0.18f, h + 0.1f, 0.12f), stone, null, false);
            Box("PervazSağ", g, new Vector3(w / 2f + 0.1f, h / 2f + 0.15f, -0.05f), new Vector3(0.18f, h + 0.1f, 0.12f), stone, null, false);
            Box("Lento", g, new Vector3(0, h + 0.28f, -0.06f), new Vector3(w + 0.5f, 0.22f, 0.16f), stone, null, false);
            Box("Kanat", g, new Vector3(0, h / 2f + 0.15f, -0.02f), new Vector3(w, h, 0.06f), M.Wood(doorColor), null, false);
            Box("PanoÜst", g, new Vector3(0, h * 0.72f + 0.15f, -0.06f), new Vector3(w * 0.7f, h * 0.3f, 0.02f), M.Wood(doorColor * 0.75f), null, false);
            Box("PanoAlt", g, new Vector3(0, h * 0.28f + 0.15f, -0.06f), new Vector3(w * 0.7f, h * 0.3f, 0.02f), M.Wood(doorColor * 0.75f), null, false);
            Sphere("Tokmak", g, new Vector3(w * 0.33f, h * 0.5f + 0.15f, -0.08f), Vector3.one * 0.07f, M.Get("brass", new Color(0.75f, 0.6f, 0.25f), 0.8f, 0.9f));
            Box("Basamak", g, new Vector3(0, 0.08f, -0.35f), new Vector3(w + 0.6f, 0.16f, 0.6f), stone);
            if (!canopy) return;
            Box("SundurmaÇatı", g, new Vector3(0, h + 0.62f, -0.45f), new Vector3(w + 0.9f, 0.08f, 0.95f), frame, new Vector3(-14f, 0, 0), false);
            foreach (var sx in new[] { -1f, 1f })
                Box("SundurmaKonsol", g, new Vector3(sx * (w / 2f + 0.25f), h + 0.35f, -0.35f), new Vector3(0.08f, 0.08f, 0.75f), frame, new Vector3(-40f, 0, 0), false);
        }

        static void Chimney2(Transform h, Vector3 pos, float height)
        {
            Box("Baca", h, pos + Vector3.up * height / 2f, new Vector3(0.55f, height, 0.55f), M.Brick(new Color(0.46f, 0.27f, 0.2f)), null, false, 1f);
            Box("BacaŞapka", h, pos + Vector3.up * (height + 0.05f), new Vector3(0.72f, 0.1f, 0.72f), M.Stone, null, false);
            if (Random.value > 0.5f) Effects.ChimneySmoke(h, pos + Vector3.up * (height + 0.25f));
        }

        /// <summary>
        /// 1800'ler Anadolu konağı: moloz taş zemin kat, boyalı sıvalı ve öne taşan üst kat, ortada konsollu cumba,
        /// dar uzun kepenkli pencereler, parmaklıklı alt pencere, taş pervazlı kapı, kiriş uçlu geniş saçak, kil kiremit.
        /// </summary>
        public static void OttomanHouse(Transform parent, Vector3 pos, float yRot, Vector3 size, bool lit)
        {
            var h = Group("CumbalıEv", parent, pos, yRot).transform;
            float g = 2.9f, up = Mathf.Max(2.6f, size.y - g), jut = 0.75f, fz = -size.z / 2f;
            var paint = M.Plaster(HousePaints[Random.Range(0, HousePaints.Length)] * Random.Range(0.92f, 1.04f));
            var stone = M.Get("stonewall_house", new Color(0.47f, 0.44f, 0.39f), 0.2f, 0f, M.BrickTexture);
            var beam = M.Wood(new Color(0.2f, 0.13f, 0.08f));
            var frame = M.Wood(new Color(0.24f, 0.16f, 0.1f));
            var shutter = M.Wood(Random.value > 0.5f ? new Color(0.22f, 0.31f, 0.26f) : new Color(0.33f, 0.2f, 0.14f));

            Box("TaşZemin", h, new Vector3(0, g / 2f, 0), new Vector3(size.x, g, size.z), stone, texTiling: 0.5f);
            Box("KatKirişi", h, new Vector3(0, g + 0.06f, fz - 0.05f), new Vector3(size.x + 0.12f, 0.18f, 0.12f), beam, null, false);
            Box("ÜstKat", h, new Vector3(0, g + up / 2f, -jut / 2f), new Vector3(size.x + 0.2f, up, size.z + jut), paint, collider: false, texTiling: 0.5f);
            foreach (var sx in new[] { -1f, 1f })
                Box("KöşeDikme", h, new Vector3(sx * (size.x / 2f + 0.06f), g + up / 2f, fz - jut - 0.03f), new Vector3(0.14f, up, 0.12f), beam, null, false);
            Box("SaçakKirişi", h, new Vector3(0, g + up - 0.08f, fz - jut - 0.03f), new Vector3(size.x + 0.3f, 0.16f, 0.12f), beam, null, false);
            int nb = Mathf.Max(3, Mathf.RoundToInt(size.x / 1.25f));
            for (int i = 0; i < nb; i++)
            {
                float x = -size.x / 2f + 0.25f + i * (size.x - 0.5f) / (nb - 1);
                Box("Konsol", h, new Vector3(x, g - 0.3f, fz - jut * 0.45f), new Vector3(0.12f, 0.12f, jut * 1.15f), beam, new Vector3(-38f, 0, 0), false);
            }

            // Cumba (öne taşan pencereli çıkma)
            float cw = Mathf.Min(size.x * 0.46f, 2.6f), cj = 0.55f, ch = up * 0.78f, cy = g + up * 0.5f, cz = fz - jut - cj / 2f;
            Box("Cumba", h, new Vector3(0, cy, cz), new Vector3(cw, ch, cj), paint, null, false);
            Box("CumbaSaçak", h, new Vector3(0, cy + ch / 2f + 0.05f, cz - 0.04f), new Vector3(cw + 0.24f, 0.1f, cj + 0.22f), beam, null, false);
            Box("CumbaAltı", h, new Vector3(0, cy - ch / 2f - 0.05f, cz), new Vector3(cw + 0.1f, 0.1f, cj + 0.08f), beam, null, false);
            foreach (var sx in new[] { -1f, 1f })
                Box("CumbaKonsol", h, new Vector3(sx * (cw / 2f - 0.15f), cy - ch / 2f - 0.32f, cz + 0.05f), new Vector3(0.1f, 0.1f, 0.75f), beam, new Vector3(-42f, 0, 0), false);
            int cn = cw > 2f ? 3 : 2;
            for (int i = 0; i < cn; i++)
            {
                float x = -cw / 2f + cw * (i + 0.5f) / cn;
                HouseWindow(h, fz - jut - cj, x, cy + 0.05f, Mathf.Min(0.52f, cw / cn - 0.2f), ch * 0.6f, lit && Random.value > 0.35f, frame, null);
            }
            // Cumbanın iki yanında dar uzun pencereler
            float side = (size.x - cw) / 2f;
            if (side > 1.0f)
                foreach (var sx in new[] { -1f, 1f })
                    HouseWindow(h, fz - jut, sx * (cw / 2f + side / 2f), g + up * 0.52f, 0.58f, Mathf.Min(1.3f, up * 0.5f), lit && Random.value > 0.45f, frame, shutter);

            // Zemin kat: kapı + parmaklıklı pencere
            float doorX = (Random.value > 0.5f ? -1f : 1f) * size.x * 0.24f;
            HouseDoor(h, fz, doorX, frame, new Color(0.27f, 0.16f, 0.09f) * Random.Range(0.85f, 1.15f), false);
            HouseWindow(h, fz, -doorX, 1.6f, 0.62f, 0.78f, lit && Random.value > 0.6f, frame, null, true);

            // Geniş saçak + kiriş uçları + kil kiremitli kırma çatı
            float roofY = g + up;
            Box("Saçak", h, new Vector3(0, roofY + 0.02f, -jut / 2f), new Vector3(size.x + 1.3f, 0.1f, size.z + jut + 1.3f), beam, null, false);
            int nr = Mathf.Max(4, Mathf.RoundToInt(size.x / 0.5f));
            for (int i = 0; i < nr; i++)
                Box("KirişUcu", h, new Vector3(-size.x / 2f - 0.4f + i * (size.x + 0.8f) / (nr - 1), roofY - 0.06f, fz - jut - 0.42f), new Vector3(0.07f, 0.08f, 0.5f), beam, null, false);
            HipRoof("Çatı", h, new Vector3(0, roofY + 0.07f, -jut / 2f), new Vector3(size.x + 1.3f, Mathf.Clamp(size.y * 0.26f, 1.1f, 1.8f), size.z + jut + 1.3f),
                M.RoofTiles(new Color(0.48f, 0.25f, 0.16f) * Random.Range(0.8f, 1.08f)), Random.Range(0.25f, 0.5f));
            if (Random.value > 0.3f) Chimney2(h, new Vector3(size.x * 0.28f, roofY + 0.6f, 0.4f), 1.4f);
            // Yağmur borusu
            Cyl("İnişBorusu", h, new Vector3(size.x / 2f + 0.05f, roofY / 2f, fz - jut + 0.1f), new Vector3(0.08f, roofY / 2f, 0.08f), M.Get("zinc", new Color(0.3f, 0.32f, 0.34f), 0.6f, 0.7f), null, false);
        }

        /// <summary>Gotik mahya tepeliği (çatı uçlarında sivri döküm süs).</summary>
        public static void Finial(Transform parent, Vector3 pos)
        {
            var iron = M.Get("castiron", new Color(0.06f, 0.07f, 0.06f), 0.45f, 0.75f);
            Cyl("TepelikGövde", parent, pos + Vector3.up * 0.3f, new Vector3(0.06f, 0.3f, 0.06f), iron, null, false);
            Sphere("TepelikTop", parent, pos + Vector3.up * 0.6f, Vector3.one * 0.12f, iron);
            Prism("TepelikUç", parent, pos + Vector3.up * 0.66f, new Vector3(0.1f, 0.35f, 0.1f), iron);
        }

        /// <summary>Yarı ahşap (badanalı sıva + koyu meşe kiriş) dik beşik çatılı ev: taş kaide, çerçeveli pencereler, sundurmalı kapı.</summary>
        public static void TimberHouse(Transform parent, Vector3 pos, float yRot, Vector3 size, bool lit)
        {
            var h = Group("KarkasEv", parent, pos, yRot).transform;
            var plaster = M.Plaster(Color.Lerp(new Color(0.8f, 0.75f, 0.64f), new Color(0.68f, 0.63f, 0.56f), Random.value));
            var beam = M.Wood(new Color(0.16f, 0.1f, 0.06f));
            var frame = M.Wood(new Color(0.22f, 0.15f, 0.09f));
            var shutter = M.Wood(Random.value > 0.5f ? new Color(0.3f, 0.12f, 0.1f) : new Color(0.2f, 0.26f, 0.22f));
            float fz = -size.z / 2f, mid = size.y * 0.5f;
            Box("Kaide", h, new Vector3(0, 0.3f, 0), new Vector3(size.x + 0.14f, 0.6f, size.z + 0.14f), M.Stone, texTiling: 0.6f);
            Box("Gövde", h, new Vector3(0, size.y / 2f, 0), size, plaster, texTiling: 0.5f);
            // Ön cephe karkası
            foreach (var y in new[] { 0.66f, mid, size.y - 0.1f })
                Box("Kiriş", h, new Vector3(0, y, fz - 0.03f), new Vector3(size.x, 0.15f, 0.07f), beam, null, false);
            int posts = 4;
            for (int i = 0; i < posts; i++)
            {
                float x = -size.x / 2f + 0.07f + i * (size.x - 0.14f) / (posts - 1);
                Box("Dikme", h, new Vector3(x, size.y / 2f + 0.3f, fz - 0.03f), new Vector3(0.13f, size.y - 0.6f, 0.07f), beam, null, false);
            }
            float bay = (size.x - 0.14f) / (posts - 1);
            foreach (var (x0, sgn) in new[] { (-size.x / 2f + 0.07f + bay / 2f, 1f), (size.x / 2f - 0.07f - bay / 2f, -1f) })
                foreach (var (y0, hh) in new[] { (0.66f, mid - 0.66f), (mid, size.y - 0.1f - mid) })
                {
                    float diag = Mathf.Sqrt(bay * bay + hh * hh) * 0.95f;
                    float ang = Mathf.Atan2(bay, hh) * Mathf.Rad2Deg * sgn;
                    Box("Çapraz", h, new Vector3(x0, y0 + hh / 2f, fz - 0.035f), new Vector3(0.1f, diag, 0.06f), beam, new Vector3(0, 0, -ang), false);
                }
            // Yan cephe köşe dikmeleri
            foreach (var sx in new[] { -1f, 1f })
                foreach (var sz in new[] { -1f, 1f })
                    Box("KöşeDikme", h, new Vector3(sx * (size.x / 2f + 0.02f), size.y / 2f + 0.3f, sz * (size.z / 2f - 0.07f)), new Vector3(0.07f, size.y - 0.6f, 0.13f), beam, null, false);
            // Saçak + dik beşik çatı + alın tahtaları
            Box("Saçak", h, new Vector3(0, size.y - 0.04f, 0), new Vector3(size.x + 0.9f, 0.1f, size.z + 0.9f), beam, null, false);
            float rh = size.y * 0.8f;
            Prism("Çatı", h, new Vector3(0, size.y, 0), new Vector3(size.z + 0.9f, rh, size.x + 0.9f), M.RoofTiles(new Color(0.42f, 0.24f, 0.17f) * Random.Range(0.8f, 1.15f)), 90f, 0.6f);
            Finial(h, new Vector3(-size.x / 2f - 0.2f, size.y + rh - 0.2f, 0));
            Finial(h, new Vector3(size.x / 2f + 0.2f, size.y + rh - 0.2f, 0));
            // Kapı (sundurmalı) + pencereler
            HouseDoor(h, fz, 0f, frame, new Color(0.3f, 0.18f, 0.1f) * Random.Range(0.85f, 1.15f), Random.value > 0.4f);
            foreach (var x in new[] { -size.x * 0.32f, size.x * 0.32f })
            {
                HouseWindow(h, fz - 0.03f, x, 1.62f, 0.62f, 0.85f, lit && Random.value > 0.4f, frame, shutter);
                HouseWindow(h, fz - 0.03f, x, mid + (size.y - mid) * 0.5f, 0.58f, 0.8f, lit && Random.value > 0.5f, frame, shutter);
            }
            if (Random.value > 0.4f) Chimney2(h, new Vector3(size.x * 0.3f, size.y + rh * 0.35f, 0.6f), 1.5f);
        }

        // =================================================================
        //  MEYDAN SAAT KULESİ (bölge haritasındaki meydan anıtı)
        // =================================================================
        public static void ClockTower(Transform parent, Vector3 pos)
        {
            var g = Group("SaatKulesi", parent, pos).transform;
            var stone = M.Get("tower_stone", new Color(0.5f, 0.48f, 0.44f), 0.2f, 0f, M.BrickTexture);
            Box("Kaide", g, new Vector3(0, 0.5f, 0), new Vector3(2.4f, 1f, 2.4f), M.Stone);
            Box("Gövde", g, new Vector3(0, 3.5f, 0), new Vector3(1.6f, 5f, 1.6f), stone, texTiling: 0.6f);
            Box("SaatOdası", g, new Vector3(0, 6.6f, 0), new Vector3(2f, 1.6f, 2f), stone);
            for (int i = 0; i < 4; i++)
            {
                var rot = Quaternion.Euler(0, i * 90, 0);
                var face = Group("Kadran", g, rot * new Vector3(0, 6.6f, -1.02f), i * 90).transform;
                Cyl("Yüz", face, Vector3.zero, new Vector3(1.2f, 0.03f, 1.2f), M.Emissive("towerclock", new Color(1f, 0.93f, 0.75f), 1.4f), new Vector3(90, 0, 0), false);
                var hand = Group("Yelkovan", face, new Vector3(0, 0, -0.04f)).transform;
                Box("İbre", hand, new Vector3(0, 0.25f, 0), new Vector3(0.05f, 0.5f, 0.02f), M.Get("dark", new Color(0.05f, 0.05f, 0.05f)), collider: false);
                var sp = hand.gameObject.AddComponent<Spinner>(); sp.axis = Vector3.forward; sp.speed = -6f;
                var hr = Group("Akrep", face, new Vector3(0, 0, -0.035f)).transform;
                Box("İbre", hr, new Vector3(0, 0.16f, 0), new Vector3(0.07f, 0.32f, 0.02f), M.Get("dark", new Color(0.05f, 0.05f, 0.05f)), collider: false);
                hr.localRotation = Quaternion.Euler(0, 0, -345); // ~23:45
            }
            HipRoof("Külah", g, new Vector3(0, 7.4f, 0), new Vector3(2.5f, 2.2f, 2.5f), M.Get("mosque_lead", new Color(0.3f, 0.34f, 0.38f), 0.55f, 0.5f), 0f);
            PointLight("KuleIşığı", g, new Vector3(0, 6.6f, 0), new Color(1f, 0.85f, 0.6f), 2.5f, 8f);
        }

        // =================================================================
        //  KÖPRÜ (dere geçişi)
        // =================================================================
        public static void Bridge(Transform parent, Vector3 pos, float yRot, float length)
        {
            var g = Group("TaşKöprü", parent, pos, yRot).transform;
            var stone = M.Get("bridge_stone", new Color(0.42f, 0.4f, 0.37f), 0.25f, 0f, M.BrickTexture);
            Box("Tabliye", g, new Vector3(0, 0.25f, 0), new Vector3(5f, 0.5f, length), stone, texTiling: 0.5f);
            foreach (var x in new[] { -2.4f, 2.4f })
                Box("Korkuluk", g, new Vector3(x, 0.85f, 0), new Vector3(0.3f, 0.7f, length), stone);
            Box("Kemer", g, new Vector3(0, -0.6f, 0), new Vector3(4.6f, 1.2f, 2f), stone, collider: false);
        }
    }

    /// <summary>Polis çakarı: kırmızı/mavi dönüşümlü yanıp söner (town_bg'deki polis aracı).</summary>
    public class PoliceFlasher : MonoBehaviour
    {
        public Light red, blue;
        public Renderer redMesh, blueMesh;
        void Update()
        {
            float t = Time.time * 4f;
            bool phase = Mathf.FloorToInt(t) % 2 == 0;
            float pulse = Mathf.Abs(Mathf.Sin(t * Mathf.PI));
            if (red != null) red.intensity = phase ? 7f * pulse : 0.2f;
            if (blue != null) blue.intensity = !phase ? 7f * pulse : 0.2f;
            if (redMesh != null) redMesh.enabled = phase || pulse < 0.3f;
            if (blueMesh != null) blueMesh.enabled = !phase || pulse < 0.3f;
        }
    }
}
