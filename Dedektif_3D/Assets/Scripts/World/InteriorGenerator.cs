using System.Collections.Generic;
using Dedektif3D.Core;
using UnityEngine;
using static Dedektif3D.World.Shapes;
using M = Dedektif3D.World.MaterialLibrary;

namespace Dedektif3D.World
{
    /// <summary>
    /// Her binanın iç mekanını (oda, mobilya, NPC, deliller) primitive'lerle üretir.
    /// Odalar kasabadan uzakta ayrı bir bölgede durur; kapıdan girince oyuncu buraya ışınlanır.
    /// Oda yerel koordinatı: X -5..5, Z -4..4, Y 0..4.2. Giriş kapısı -Z duvarında.
    /// </summary>
    public static class InteriorGenerator
    {
        public const int InteriorLayer = 9;
        public const float W = 10f, D = 8f, H = 4.2f;

        public class InteriorInstance
        {
            public BuildingDef Def;
            public Transform Root;
            public Transform Spawn;
            public Vector3 ExitPoint;
            public PropFactory.PersonRig Npc;
            public readonly Dictionary<int, GameObject> ClueObjects = new Dictionary<int, GameObject>();
            /// <summary>Gizli delillerin saklandığı yerler (çekmece, sandık, tablo arkası, döşeme, halı).</summary>
            public readonly List<HidingSpotDef> Spots = new List<HidingSpotDef>();
        }

        public static InteriorInstance Build(BuildingDef def, Transform parent, Vector3 worldPos)
        {
            var root = Group("İç_" + def.DisplayName, parent, worldPos).transform;
            var inst = new InteriorInstance { Def = def, Root = root };

            BuildShell(root, def);
            switch (def.Style)
            {
                case BuildingStyle.Kasap: FurnishKasap(root, def); break;
                case BuildingStyle.Eczane: FurnishEczane(root, def); break;
                case BuildingStyle.Muhtarlik: FurnishMuhtarlik(root, def); break;
                case BuildingStyle.Karakol: FurnishKarakol(root, def); break;
                case BuildingStyle.Terzi: FurnishTerzi(root, def); break;
                case BuildingStyle.Rural: FurnishRural(root, def); break;
            }

            // NPC (tezgahın arkasında, kapıya bakar)
            var npcDef = GameData.Npc(def.NpcId);
            // Yüz ve kıyafet renkleri portreden (images/...): şüpheli 3D'de de portresiyle aynı kişidir
            var faceKey = FaceLibrary.KeyFromPortrait(npcDef.Portrait);
            var look = FaceLibrary.Info(faceKey);
            var clothes = look != null ? FaceLibrary.Readable(look.Coat) : npcDef.Clothes;
            var accent = look != null ? FaceLibrary.Readable(look.Accent, 0.2f, 0.85f) : npcDef.Accent;
            var rig = PropFactory.BuildPerson("NPC_" + npcDef.Name, root, new Vector3(0, 0, 2.8f), clothes, accent, npcDef.Female, null, faceKey);
            rig.Root.localRotation = Quaternion.Euler(0, 180, 0);
            if (def.Style == BuildingStyle.Rural) RuralBuildings.Dress(rig, def.Key); else PropFactory.Dress(rig, npcDef.Id);
            inst.Npc = rig;

            // Deliller — gizli olanlar bir saklama yerine konur (çekmece, sandık, tablo arkası, döşeme altı, halı altı)
            int slot = 0;
            foreach (var clue in GameData.Clues)
            {
                if (clue.BuildingNpcId != def.NpcId) continue;
                GameObject go;
                if (clue.Hidden && clue.Shape == ClueShape.Card)
                {
                    var spot = HidingSpots.Build(root, clue, ShapeFor(clue.Name), slot++);
                    inst.Spots.Add(spot);
                    go = spot.ClueObject;
                }
                else
                {
                    go = clue.Shape == ClueShape.Card ? ClueCard(root, clue) : PropFactory.BuildClue(clue.Shape, root, clue.RoomPos);
                    if (clue.Shape == ClueShape.Apron) go.transform.localRotation = Quaternion.Euler(0, 90, 0);
                    if (clue.Shape == ClueShape.Lining) go.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    if (clue.Shape == ClueShape.Cleaver) go.transform.localRotation = Quaternion.Euler(0, 70, 0);
                    if (clue.Hidden) inst.Spots.Add(HidingSpots.InPlace(root, clue, go));
                }
                inst.ClueObjects[clue.Id] = go;
            }

            var spawn = Group("Spawn", root, new Vector3(0, 0, -2.6f)).transform;
            inst.Spawn = spawn;
            inst.ExitPoint = root.TransformPoint(new Vector3(0, 1f, -3.5f));

            SetLayerRecursive(root.gameObject, InteriorLayer);
            return inst;
        }

        // =================================================================
        //  KIRSAL İÇ MEKAN (Gölgeşehir / Sisören)
        // =================================================================
        static void FurnishRural(Transform t, BuildingDef d)
        {
            var wood = new Color(0.4f, 0.27f, 0.16f);
            Counter(t, wood);
            // Dükkân adı: duvara çakılı küçük boyalı levha (ışıklı dev yazı değil)
            Box("TabelaLevha", t, new Vector3(0, 3.42f, D / 2 - 0.06f), new Vector3(2.1f, 0.5f, 0.04f), M.Get("detail_signboard", new Color(0.07f, 0.13f, 0.09f), 0.35f), collider: false);
            Text3D("Tabela", t, new Vector3(0, 3.42f, D / 2 - 0.09f), d.DisplayName, new Color(0.72f, 0.62f, 0.4f), 0.022f);
            RoomDetail.Rural(t, d);
            InteriorDressing.Furnish(t, d);
        }

        /// <summary>
        /// Gölgeşehir / Sisören delilleri: web görseli yapıştırılmaz; delilin adından türetilen 3D obje
        /// (mektup, şişe, bıçak, anahtar, kese, kablo...) küçük bir delil sehpasının üstüne konur.
        /// </summary>
        static GameObject ClueCard(Transform root, ClueDef clue)
        {
            var g = Group("Clue_" + clue.Id, root, clue.RoomPos);
            var t = g.transform;
            Box("Sehpa", t, new Vector3(0, 0.38f, 0), new Vector3(0.62f, 0.76f, 0.5f), M.Wood(new Color(0.3f, 0.2f, 0.12f)));
            Box("Örtü", t, new Vector3(0, 0.77f, 0), new Vector3(0.66f, 0.02f, 0.54f), M.Get("felt_green", new Color(0.08f, 0.18f, 0.12f), 0.1f), collider: false);
            var prop = PropFactory.BuildClue(ShapeFor(clue.Name), t, new Vector3(0, 0.79f, 0));
            // Kasa gibi iri delil sehpayı ve odayı kapatmasın
            prop.transform.localScale = Vector3.one * (ShapeFor(clue.Name) == ClueShape.Safe ? 0.42f : 0.9f);
            return g;
        }

        static ClueShape ShapeFor(string name)
        {
            string n = (name ?? "").ToLowerInvariant();
            bool Has(params string[] k) { foreach (var x in k) if (n.Contains(x)) return true; return false; }
            if (Has("şişe", "ilaç", "zehir", "şurup", "kavanoz", "flakon")) return ClueShape.Bottle;
            if (Has("bıçak", "satır", "balta", "çekiç", "kama", "makas", "keski", "neşter")) return ClueShape.Cleaver;
            if (Has("mektup", "zarf", "telgraf", "mesaj", "afiş", "bilet")) return ClueShape.Letter;
            if (Has("defter", "kitap", "ansiklopedi", "albüm", "kütük")) return ClueShape.Notebook;
            if (Has("reçete", "senet", "tapu", "vasiyet", "liste", "fiş", "kayıt", "sayfa", "not", "kâğıt", "kağıt", "belge")) return ClueShape.Prescription;
            if (Has("dosya", "evrak")) return ClueShape.FileFolder;
            if (Has("gözlük")) return ClueShape.Glasses;
            if (Has("kasa", "sandık", "kutu", "çekmece")) return ClueShape.Safe;
            if (Has("rozet", "madalya", "mühür", "yüzük", "kolye", "saat", "kese", "para", "altın", "anahtar")) return ClueShape.Badge;
            if (Has("düğme", "toka")) return ClueShape.Button;
            if (Has("iplik", "kablo", "tel", "ip", "halat")) return ClueShape.Spool;
            if (Has("kumaş", "mendil", "eşarp", "astar", "önlük", "eldiven", "palto", "ceket", "atkı")) return ClueShape.Fabric;
            if (Has("yaprak", "bitki", "çiçek", "ot")) return ClueShape.Leaf;
            return ClueShape.FileFolder;
        }

        public static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform c in go.transform) SetLayerRecursive(c.gameObject, layer);
        }

        // =================================================================
        //  ODA KABUĞU
        // =================================================================
        static void BuildShell(Transform t, BuildingDef def)
        {
            Material wall;
            switch (def.Style)
            {
                // Koyu tonlar: web iç mekân görselleri loş, gaz lambasıyla aydınlanan odalar gösteriyor
                case BuildingStyle.Kasap: wall = M.Brick(new Color(0.42f, 0.41f, 0.38f)); break;
                case BuildingStyle.Eczane: wall = M.Plaster(new Color(0.3f, 0.32f, 0.28f)); break;
                case BuildingStyle.Muhtarlik: wall = M.Plaster(new Color(0.34f, 0.28f, 0.2f)); break;
                case BuildingStyle.Karakol: wall = M.Plaster(new Color(0.26f, 0.3f, 0.33f)); break;
                case BuildingStyle.Rural: wall = M.Wood(new Color(0.32f, 0.23f, 0.15f)); break;
                default: wall = M.Plaster(new Color(0.34f, 0.29f, 0.24f)); break;
            }
            // Kasap ve karakolun zemini fayans, diğerleri ahşap (web iç mekân görselleri)
            bool tiled = def.Style == BuildingStyle.Kasap || def.Style == BuildingStyle.Karakol;
            var floorMat = !tiled ? M.Floorboards : RoomDetail.TileFloor(def.Style == BuildingStyle.Kasap ? new Color(0.42f, 0.4f, 0.36f) : new Color(0.3f, 0.35f, 0.37f));
            var floor = Box("Floor", t, new Vector3(0, -0.1f, 0), new Vector3(W, 0.2f, D), floorMat);
            SetTiling(floor.GetComponent<Renderer>(), tiled ? new Vector2(5, 4) : new Vector2(3, 2.5f));
            Box("Ceiling", t, new Vector3(0, H + 0.1f, 0), new Vector3(W, 0.2f, D), M.Plaster(new Color(0.5f, 0.48f, 0.45f)));
            Box("WallBack", t, new Vector3(0, H / 2, D / 2 + 0.1f), new Vector3(W, H, 0.2f), wall, texTiling: 0.5f);
            Box("WallLeft", t, new Vector3(-W / 2 - 0.1f, H / 2, 0), new Vector3(0.2f, H, D), wall, texTiling: 0.5f);
            Box("WallRight", t, new Vector3(W / 2 + 0.1f, H / 2, 0), new Vector3(0.2f, H, D), wall, texTiling: 0.5f);
            // Ön duvar (kapı boşluklu)
            Box("WallFrontL", t, new Vector3(-2.9f, H / 2, -D / 2 - 0.1f), new Vector3(4.2f, H, 0.2f), wall, texTiling: 0.5f);
            Box("WallFrontR", t, new Vector3(2.9f, H / 2, -D / 2 - 0.1f), new Vector3(4.2f, H, 0.2f), wall, texTiling: 0.5f);
            Box("WallFrontTop", t, new Vector3(0, H - 0.8f, -D / 2 - 0.1f), new Vector3(1.6f, 1.6f, 0.2f), wall);
            Box("ExitDoor", t, new Vector3(0, 1.3f, -D / 2 - 0.05f), new Vector3(1.6f, 2.6f, 0.12f), M.Wood(new Color(0.3f, 0.18f, 0.1f)));
            Sphere("ExitKnob", t, new Vector3(0.55f, 1.2f, -D / 2 + 0.05f), Vector3.one * 0.09f, M.Get("brass", new Color(0.75f, 0.6f, 0.25f), 0.8f, 0.9f));
            Text3D("ExitSign", t, new Vector3(0, 2.85f, -D / 2 + 0.02f), "ÇIKIŞ", new Color(0.86f, 0.74f, 0.5f), 0.035f, 180f);
            // Süpürgelik
            var trim = M.Wood(new Color(0.25f, 0.15f, 0.09f));
            Box("TrimBack", t, new Vector3(0, 0.08f, D / 2 - 0.02f), new Vector3(W, 0.16f, 0.06f), trim, collider: false);
            Box("TrimL", t, new Vector3(-W / 2 + 0.02f, 0.08f, 0), new Vector3(0.06f, 0.16f, D), trim, collider: false);
            Box("TrimR", t, new Vector3(W / 2 - 0.02f, 0.08f, 0), new Vector3(0.06f, 0.16f, D), trim, collider: false);

            // 1800'ler: tavandan zincirle sarkan pirinç gaz lambaları (titreşen sıcak alev)
            InteriorDressing.HangingLantern(t, new Vector3(0, H, -1.2f), 9f, false);
            InteriorDressing.HangingLantern(t, new Vector3(0, H, 2f), 10f, true);
            // Binanın tabela renginden çok hafif bir dolgu ışığı (tamamen kararmasın)
            PointLight("MoodLight", t, new Vector3(-4f, 2.5f, -3f), Color.Lerp(def.Neon, new Color(1f, 0.7f, 0.45f), 0.6f), 2.2f, 9f);
            // Ortak atmosfer: yağmurlu pencere, kirişler, duvar saati, gaz aplikleri, kilim, toz zerrecikleri
            InteriorDressing.Atmosphere(t, def);
        }

        static void Counter(Transform t, Color wood, float width = 4f)
        {
            Box("CounterBase", t, new Vector3(0, 0.48f, 1.6f), new Vector3(width, 0.96f, 0.8f), M.Wood(wood));
            Box("CounterTop", t, new Vector3(0, 0.99f, 1.6f), new Vector3(width + 0.2f, 0.06f, 0.95f), M.Get("countertop", new Color(0.3f, 0.22f, 0.15f), 0.6f));
            Box("CounterPanel", t, new Vector3(0, 0.48f, 1.19f), new Vector3(width - 0.3f, 0.7f, 0.02f), M.Wood(wood * 0.8f), collider: false);
        }

        static void Shelf(Transform t, Vector3 pos, float yRot, float width, int levels, Color wood)
        {
            var g = Group("Shelf", t, pos, yRot).transform;
            var mat = M.Wood(wood);
            Box("SideL", g, new Vector3(-width / 2, 1.25f, 0), new Vector3(0.06f, 2.5f, 0.45f), mat);
            Box("SideR", g, new Vector3(width / 2, 1.25f, 0), new Vector3(0.06f, 2.5f, 0.45f), mat);
            Box("BackPanel", g, new Vector3(0, 1.25f, 0.21f), new Vector3(width, 2.5f, 0.03f), M.Wood(wood * 0.7f), collider: false);
            for (int i = 0; i < levels; i++)
                Box("Board" + i, g, new Vector3(0, 0.3f + i * (2.1f / (levels - 1)), 0), new Vector3(width, 0.05f, 0.45f), mat, collider: false);
        }

        // =================================================================
        //  KASAP İÇİ
        // =================================================================
        static void FurnishKasap(Transform t, BuildingDef d)
        {
            Counter(t, new Color(0.55f, 0.35f, 0.2f));
            // Cam vitrin (tezgahın önünde et sergisi)
            Box("DisplayGlass", t, new Vector3(0, 1.25f, 1.25f), new Vector3(3.6f, 0.45f, 0.03f), M.Glass, new Vector3(-20, 0, 0), false);
            // Asılı karkaslar (tavan rayı + çengeller)
            Box("HookRail", t, new Vector3(0, H - 0.42f, 3.6f), new Vector3(9f, 0.08f, 0.08f), M.Metal, collider: false);
            foreach (var x in new[] { -4f, -3f, -2f, 2f, 3f, 4f })
                RoomDetail.Carcass(t, new Vector3(x, H - 0.42f, 3.6f), 1.05f);
            Cyl("ChopBlock", t, new Vector3(3.2f, 0.45f, -1f), new Vector3(0.9f, 0.45f, 0.9f), M.Wood(new Color(0.45f, 0.3f, 0.16f)));
            Box("BloodPuddle", t, new Vector3(3.2f, 0.005f, -0.2f), new Vector3(0.7f, 0.01f, 0.4f), M.Get("blood", new Color(0.45f, 0.02f, 0.03f), 0.8f), new Vector3(0, 25, 0), false);
            // Soğuk hava deposu kapısı
            Box("ColdRoomDoor", t, new Vector3(3.6f, 1.2f, D / 2 - 0.05f), new Vector3(1.5f, 2.4f, 0.12f), M.Get("steel", new Color(0.55f, 0.55f, 0.58f), 0.75f, 0.9f), collider: false);
            Box("ColdRoomHandle", t, new Vector3(3.1f, 1.2f, D / 2 - 0.15f), new Vector3(0.06f, 0.5f, 0.08f), M.Metal, collider: false);
            // Önlük askısı (sol duvar)
            Box("CoatRack", t, new Vector3(-W / 2 + 0.05f, 1.95f, -1.5f), new Vector3(0.08f, 0.12f, 1.5f), M.Wood(new Color(0.3f, 0.2f, 0.1f)), collider: false);
            for (int i = 0; i < 3; i++)
                Cyl("Peg" + i, t, new Vector3(-W / 2 + 0.15f, 1.95f, -2f + i * 0.5f), new Vector3(0.04f, 0.1f, 0.04f), M.Metal, new Vector3(0, 0, 90), false);
            Box("Scale", t, new Vector3(-0.2f, 1.1f, 1.7f), new Vector3(0.3f, 0.15f, 0.3f), M.Get("silver", new Color(0.8f, 0.8f, 0.85f), 0.9f, 1f), collider: false);
            PropFactory.Barrel(t, new Vector3(-4f, 0, 3f));
            RoomDetail.Kasap(t);
        }

        // =================================================================
        //  ECZANE İÇİ
        // =================================================================
        static void FurnishEczane(Transform t, BuildingDef d)
        {
            var wood = new Color(0.2f, 0.125f, 0.07f);
            Counter(t, wood);
            // Duvardaki boyalı yeşil haç (ışıklı değil: 1887)
            var crossMat = M.Get("apothecary_paint", new Color(0.1f, 0.42f, 0.24f), 0.2f);
            Box("CrossV", t, new Vector3(0, 3.3f, D / 2 - 0.05f), new Vector3(0.3f, 0.9f, 0.08f), crossMat, collider: false);
            Box("CrossH", t, new Vector3(0, 3.3f, D / 2 - 0.05f), new Vector3(0.9f, 0.3f, 0.08f), crossMat, collider: false);
            Box("Register", t, new Vector3(1.4f, 1.15f, 1.7f), new Vector3(0.45f, 0.3f, 0.4f), M.Get("register", new Color(0.2f, 0.2f, 0.22f), 0.6f, 0.5f), collider: false);
            Box("Mortar", t, new Vector3(0.4f, 1.08f, 1.8f), new Vector3(0.18f, 0.12f, 0.18f), M.Stone, collider: false);
            Cyl("PlantPot", t, new Vector3(-4.2f, 0.3f, -3f), new Vector3(0.6f, 0.3f, 0.6f), M.Get("terracotta", new Color(0.6f, 0.3f, 0.18f)));
            Sphere("Plant", t, new Vector3(-4.2f, 1f, -3f), new Vector3(0.9f, 1f, 0.9f), M.Get("leaves", new Color(0.08f, 0.17f, 0.09f)));
            Box("ScaleBench", t, new Vector3(-4.4f, 0.25f, 1.0f), new Vector3(0.9f, 0.5f, 1.5f), M.Wood(wood));
            RoomDetail.Eczane(t);
        }

        // =================================================================
        //  MUHTARLIK İÇİ
        // =================================================================
        static void FurnishMuhtarlik(Transform t, BuildingDef d)
        {
            var wood = new Color(0.35f, 0.2f, 0.1f);
            // Makam masası
            Box("DeskTop", t, new Vector3(0, 0.98f, 1.6f), new Vector3(3.4f, 0.08f, 1.2f), M.Wood(wood));
            Box("DeskPedL", t, new Vector3(-1.3f, 0.47f, 1.6f), new Vector3(0.7f, 0.94f, 1.1f), M.Wood(wood));
            Box("DeskPedR", t, new Vector3(1.3f, 0.47f, 1.6f), new Vector3(0.7f, 0.94f, 1.1f), M.Wood(wood));
            Box("DeskFront", t, new Vector3(0, 0.6f, 1.05f), new Vector3(1.9f, 0.7f, 0.04f), M.Wood(wood * 0.8f), collider: false);
            Box("NamePlate", t, new Vector3(0, 1.08f, 1.15f), new Vector3(0.6f, 0.12f, 0.05f), M.Get("brass", new Color(0.75f, 0.6f, 0.25f), 0.8f, 0.9f), new Vector3(-20, 0, 0), false);
            Text3D("NameText", t, new Vector3(0, 1.085f, 1.12f), "MUHTAR", Color.black, 0.012f);
            Box("Stamp", t, new Vector3(-1.3f, 1.08f, 1.8f), new Vector3(0.12f, 0.12f, 0.12f), M.Wood(new Color(0.2f, 0.1f, 0.05f)), collider: false);
            // Makam koltuğu
            Box("ChairSeat", t, new Vector3(0, 0.55f, 3.2f), new Vector3(0.9f, 0.12f, 0.8f), M.Get("leather", new Color(0.3f, 0.08f, 0.06f), 0.6f), collider: false);
            Box("ChairBack", t, new Vector3(0, 1.3f, 3.6f), new Vector3(0.9f, 1.4f, 0.15f), M.Get("leather", new Color(0.3f, 0.08f, 0.06f), 0.6f));
            // Kitaplık
            Shelf(t, new Vector3(-W / 2 + 0.3f, 0, 1.5f), -90, 3.5f, 5, wood);
            for (int lvl = 1; lvl < 5; lvl++)
                for (int i = 0; i < 9; i++)
                {
                    var c = Color.HSVToRGB((i * 0.13f + lvl * 0.3f) % 1f, 0.5f, 0.35f);
                    Box("Book", t, new Vector3(-W / 2 + 0.35f, 0.3f + lvl * 0.525f + 0.17f, 0.0f + i * 0.36f), new Vector3(0.3f, 0.34f, 0.08f + (i % 3) * 0.03f),
                        M.Get("book_" + ColorUtility.ToHtmlStringRGB(c), c), collider: false);
                }
            // Bayrak standı
            Cyl("FlagStand", t, new Vector3(-3.2f, 1.2f, 3.4f), new Vector3(0.05f, 1.2f, 0.05f), M.Get("brass", new Color(0.75f, 0.6f, 0.25f), 0.8f, 0.9f), collider: false);
            Box("FlagCloth", t, new Vector3(-2.75f, 2.05f, 3.4f), new Vector3(0.9f, 0.6f, 0.02f), M.Get("flagred", new Color(0.8f, 0.04f, 0.08f), 0.2f), collider: false);
            Cyl("FlagCrescent", t, new Vector3(-2.9f, 2.05f, 3.38f), new Vector3(0.3f, 0.005f, 0.3f), M.Get("flagwhite", Color.white), new Vector3(90, 0, 0), false);
            Cyl("FlagCrescentCut", t, new Vector3(-2.85f, 2.05f, 3.375f), new Vector3(0.24f, 0.005f, 0.24f), M.Get("flagred", new Color(0.8f, 0.04f, 0.08f), 0.2f), new Vector3(90, 0, 0), false);
            // Duvar tablosu (manzara resmi)
            Box("FrameGold", t, new Vector3(0, 2.8f, D / 2 - 0.04f), new Vector3(1.6f, 1.1f, 0.05f), M.Get("brass", new Color(0.75f, 0.6f, 0.25f), 0.8f, 0.9f), collider: false);
            Box("PaintingSky", t, new Vector3(0, 2.95f, D / 2 - 0.07f), new Vector3(1.4f, 0.5f, 0.02f), M.Get("paintsky", new Color(0.25f, 0.35f, 0.5f)), collider: false);
            Box("PaintingHill", t, new Vector3(0, 2.55f, D / 2 - 0.07f), new Vector3(1.4f, 0.4f, 0.02f), M.Get("painthill", new Color(0.2f, 0.35f, 0.15f)), collider: false);
            // Dosya dolabı
            Box("FilingCabinet", t, new Vector3(4.4f, 0.75f, -1.5f), new Vector3(0.8f, 1.5f, 0.7f), M.Get("cabinet", new Color(0.3f, 0.35f, 0.3f), 0.4f, 0.6f));
            Box("Rug", t, new Vector3(0, 0.005f, -0.5f), new Vector3(4f, 0.01f, 2.6f), M.Get("rug", new Color(0.45f, 0.1f, 0.1f), 0.05f), collider: false);
            Box("RugBorder", t, new Vector3(0, 0.004f, -0.5f), new Vector3(4.3f, 0.01f, 2.9f), M.Get("rugborder", new Color(0.7f, 0.55f, 0.25f), 0.05f), collider: false);
            RoomDetail.Muhtarlik(t);
        }

        // =================================================================
        //  KARAKOL İÇİ
        // =================================================================
        static void FurnishKarakol(Transform t, BuildingDef d)
        {
            var metal = M.Get("deskmetal", new Color(0.35f, 0.38f, 0.42f), 0.4f, 0.6f);
            Box("DeskTop", t, new Vector3(0, 0.98f, 1.6f), new Vector3(3.2f, 0.06f, 1.1f), M.Get("desktop", new Color(0.2f, 0.25f, 0.2f), 0.4f));
            Box("DeskBody", t, new Vector3(0, 0.48f, 1.6f), new Vector3(3.0f, 0.94f, 1.0f), metal);
            Box("Typewriter", t, new Vector3(1.0f, 1.1f, 1.8f), new Vector3(0.5f, 0.18f, 0.4f), M.Get("typewriter", new Color(0.1f, 0.1f, 0.1f), 0.6f, 0.4f), collider: false);
            InteriorDressing.Telegraph(t, new Vector3(-1.2f, 1.01f, 1.85f));
            Cyl("Hokka", t, new Vector3(-0.5f, 1.05f, 1.8f), new Vector3(0.07f, 0.04f, 0.07f), M.Get("inkwell", new Color(0.05f, 0.06f, 0.1f), 0.9f), collider: false);
            // Nezarethane (sol tarafta parmaklıklar)
            var bars = M.Get("bars", new Color(0.15f, 0.15f, 0.17f), 0.6f, 0.9f);
            for (int i = 0; i < 14; i++)
                Cyl("Bar" + i, t, new Vector3(-2.8f, H / 2, 0.6f + i * 0.25f), new Vector3(0.05f, H / 2, 0.05f), bars);
            for (int i = 0; i < 9; i++)
                Cyl("BarF" + i, t, new Vector3(-2.8f - i * 0.25f, H / 2, 0.5f), new Vector3(0.05f, H / 2, 0.05f), bars);
            Box("CellBench", t, new Vector3(-4.4f, 0.4f, 2.5f), new Vector3(0.9f, 0.1f, 2.5f), M.Wood(new Color(0.3f, 0.2f, 0.1f)));
            // Dosya dolabı (delil dosyası üstünde)
            Box("FileCabinet", t, new Vector3(4.4f, 0.65f, 0.5f), new Vector3(0.9f, 1.3f, 0.8f), metal);
            for (int i = 0; i < 3; i++)
                Box("Drawer" + i, t, new Vector3(3.94f, 0.25f + i * 0.4f, 0.5f), new Vector3(0.02f, 0.3f, 0.7f), M.Get("drawer", new Color(0.3f, 0.32f, 0.36f), 0.5f, 0.6f), collider: false);
            // Mantar pano: ipliklerle bağlı şüpheli kartları
            Box("CorkBoard", t, new Vector3(2.6f, 2.4f, D / 2 - 0.04f), new Vector3(3f, 1.6f, 0.05f), M.Get("cork", new Color(0.6f, 0.45f, 0.28f)), collider: false);
            var pins = new[] { new Vector2(1.6f, 2.8f), new Vector2(2.4f, 2.2f), new Vector2(3.4f, 2.9f), new Vector2(3.2f, 2.0f), new Vector2(2.0f, 1.9f) };
            foreach (var p in pins)
                Box("Card", t, new Vector3(p.x, p.y, D / 2 - 0.07f), new Vector3(0.35f, 0.45f, 0.01f), M.Get("paperwhite", new Color(0.95f, 0.93f, 0.88f)), collider: false);
            for (int i = 0; i < pins.Length; i++)
            {
                var a = pins[i]; var b = pins[(i + 2) % pins.Length];
                var mid = (a + b) / 2; var dir = b - a;
                Box("String", t, new Vector3(mid.x, mid.y, D / 2 - 0.08f), new Vector3(dir.magnitude, 0.015f, 0.005f),
                    M.Emissive("redstring", new Color(0.9f, 0.05f, 0.05f), 0.5f), new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg), false);
            }
            Box("WantedBoard", t, new Vector3(-0.5f, 2.5f, D / 2 - 0.04f), new Vector3(1f, 1.3f, 0.04f), M.Get("paper", new Color(0.9f, 0.86f, 0.74f)), collider: false);
            Text3D("Wanted", t, new Vector3(-0.5f, 2.95f, D / 2 - 0.07f), "ARANIYOR", new Color(0.4f, 0.05f, 0.05f), 0.018f);
            // Zaptiye karakolu: nezarethanenin önünde asılı gaz feneri, duvarda kelepçe ve tüfek askısı
            InteriorDressing.HangingLantern(t, new Vector3(-2.2f, H, 0.2f), 3.2f);
            Box("TüfekAskısı", t, new Vector3(W / 2 - 0.06f, 2.2f, 2.4f), new Vector3(0.06f, 0.08f, 1.4f), M.Wood(new Color(0.3f, 0.18f, 0.1f)), collider: false);
            foreach (var z in new[] { 2.0f, 2.4f, 2.8f })
                Cyl("Tüfek", t, new Vector3(W / 2 - 0.12f, 1.7f, z), new Vector3(0.04f, 0.6f, 0.04f), M.Get("rifle", new Color(0.22f, 0.13f, 0.07f), 0.5f), collider: false);
            RoomDetail.Karakol(t);
        }

        // =================================================================
        //  TERZİ İÇİ
        // =================================================================
        static void FurnishTerzi(Transform t, BuildingDef d)
        {
            Counter(t, new Color(0.26f, 0.17f, 0.11f));
            // Dikiş makinesi
            var mach = Group("SewingMachine", t, new Vector3(-1.2f, 1.02f, 1.65f)).transform;
            var black = M.Get("machine", new Color(0.05f, 0.05f, 0.06f), 0.8f, 0.3f);
            Box("Base", mach, new Vector3(0, 0.05f, 0), new Vector3(0.6f, 0.1f, 0.3f), black, collider: false);
            Box("Pillar", mach, new Vector3(0.22f, 0.2f, 0), new Vector3(0.12f, 0.3f, 0.2f), black, collider: false);
            Box("Arm", mach, new Vector3(0, 0.38f, 0), new Vector3(0.55f, 0.1f, 0.18f), black, collider: false);
            Cyl("Wheel", mach, new Vector3(0.3f, 0.3f, 0), new Vector3(0.18f, 0.02f, 0.18f), M.Get("brass", new Color(0.75f, 0.6f, 0.25f), 0.8f, 0.9f), new Vector3(0, 0, 90), false);
            // Mankenler
            foreach (var pos in new[] { new Vector3(-3.5f, 0, 2.5f), new Vector3(-3.5f, 0, -1.8f), new Vector3(3.6f, 0, 2.6f) })
            {
                var mq = Group("Mannequin", t, pos).transform;
                Cyl("Stand", mq, new Vector3(0, 0.55f, 0), new Vector3(0.06f, 0.55f, 0.06f), M.Metal);
                Cyl("Foot", mq, new Vector3(0, 0.02f, 0), new Vector3(0.5f, 0.02f, 0.5f), M.Metal, collider: false);
                RoomDetail.DressForm(mq, Tailoring.Cloth(pos.z > 2f ? new Color(0.62f, 0.55f, 0.42f) : new Color(0.16f, 0.17f, 0.22f), 0.1f));
            }
            // Kumaş topları rafı
            var wood = new Color(0.4f, 0.28f, 0.2f);
            Shelf(t, new Vector3(1.5f, 0, D / 2 - 0.3f), 0, 5f, 4, wood);
            for (int lvl = 1; lvl < 4; lvl++)
                for (int i = 0; i < 6; i++)
                {
                    // Görseldeki gibi koyu takım kumaşları: lacivert, bordo, füme, kahve
                    Color[] rolls = { new Color(0.09f, 0.13f, 0.27f), new Color(0.32f, 0.06f, 0.08f), new Color(0.22f, 0.22f, 0.24f), new Color(0.3f, 0.25f, 0.17f), new Color(0.08f, 0.2f, 0.15f), new Color(0.12f, 0.1f, 0.1f) };
                    Cyl("FabricRoll", t, new Vector3(-0.6f + i * 0.8f, 0.3f + lvl * 0.7f + 0.2f, D / 2 - 0.35f), new Vector3(0.35f, 0.2f, 0.35f),
                        Tailoring.Cloth(rolls[(i * 2 + lvl) % rolls.Length], 0.12f), new Vector3(0, 0, 90), false);
                }
            // Boy aynası
            Box("MirrorFrame", t, new Vector3(W / 2 - 0.05f, 1.3f, -1f), new Vector3(0.06f, 2.2f, 1f), M.Wood(new Color(0.5f, 0.35f, 0.15f)), collider: false);
            Box("Mirror", t, new Vector3(W / 2 - 0.09f, 1.3f, -1f), new Vector3(0.02f, 2f, 0.85f), M.Get("mirror", new Color(0.7f, 0.75f, 0.8f), 1f, 1f), collider: false);
            Cyl("Stool", t, new Vector3(2.4f, 0.3f, -1.5f), new Vector3(0.5f, 0.3f, 0.5f), M.Wood(new Color(0.35f, 0.22f, 0.12f)));
            RoomDetail.Terzi(t);
        }
    }
}
