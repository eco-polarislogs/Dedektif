using Dedektif3D.Core;
using UnityEngine;
using static Dedektif3D.World.Shapes;
using M = Dedektif3D.World.MaterialLibrary;

namespace Dedektif3D.World
{
    /// <summary>Karakterler, delil objeleri ve kasaba dekorlarını primitive'lerden üretir.</summary>
    public static class PropFactory
    {
        static readonly Color Skin = new Color(0.86f, 0.68f, 0.55f);
        static readonly Color Hair = new Color(0.12f, 0.08f, 0.06f);

        // =================================================================
        //  KARAKTERLER
        // =================================================================
        public class PersonRig
        {
            public Transform Root, Body, Head, LegL, LegR, ArmL, ArmR;
            /// <summary>Gerçek insan modeli kullanılıyor (primitive aksesuarlar eklenmez).</summary>
            public bool HasModel;
            public bool HasFace => HasModel;
            public string FaceKey;
            /// <summary>Elde taşınan eşyaların (fener, defter, şemsiye) bağlandığı sağ el.</summary>
            public Transform HandR;
            public HumanAnimator Human;
            /// <summary>Portreden çıkarılmış görünüm tarifi (gerçek modelde).</summary>
            public CharLook Look;
        }

        /// <summary>Hacimli insan figürü: bacak/kol pivotları (yürüme animasyonu için), gövde, kafa, saç, göz.</summary>
        public static PersonRig BuildPerson(string name, Transform parent, Vector3 pos, Color clothes, Color accent,
            bool female, Color? pants = null, string faceKey = null, string pose = null)
        {
            var rig = new PersonRig();
            var face = FaceLibrary.Info(faceKey);   // portreden ten / saç / kıyafet renkleri
            var root = Group(name, parent, pos).transform;
            rig.Root = root;
            rig.FaceKey = faceKey;

            // ---- GERÇEK İNSAN MODELİ (Resources/Humans) ----
            if (HumanLibrary.Available)
            {
                var bodyG = Group("Body", root, Vector3.zero).transform;
                // Kasabalı yüzleri tekrar edebilir: kıyafet/saç kombinasyonu kişiye özel olsun
                bool named = name.StartsWith("NPC_") || name == "DetectiveModel" || name.StartsWith("Yardımcı");
                string key = faceKey ?? name;
                if (!named) key += "#" + Mathf.Abs((pos + parent.position).GetHashCode() ^ name.GetHashCode());
                int hash = HumanLibrary.Hash(key);
                var spec = SpecFor(name, faceKey, face, female, hash);
                rig.Look = spec;
                var look = new HumanLibrary.Look
                {
                    Clothes = clothes,
                    Skin = face != null ? face.Skin : (Color?)null,
                    Hair = face != null ? face.Hair : (Color?)null,
                    Spec = spec,
                };
                var ha = HumanLibrary.Spawn(bodyG, key, spec.Female, spec.Child, pose, look);
                if (ha != null)
                {
                    rig.HasModel = true; rig.Human = ha; rig.Body = bodyG;
                    rig.Head = ha.HeadAnchor != null ? ha.HeadAnchor : ha.Head != null ? ha.Head : bodyG;
                    rig.HandR = ha.HandAnchor != null ? ha.HandAnchor : ha.HandR != null ? ha.HandR : bodyG;
                    // Eski kodun döndürdüğü uzuv pivotları: modelde etkisiz vekiller
                    rig.LegL = Group("LegL", bodyG, new Vector3(-0.13f, 0.9f, 0)).transform;
                    rig.LegR = Group("LegR", bodyG, new Vector3(0.13f, 0.9f, 0)).transform;
                    rig.ArmL = Group("ArmL", bodyG, new Vector3(-0.31f, 1.52f, 0)).transform;
                    rig.ArmR = Group("ArmR", bodyG, new Vector3(0.31f, 1.52f, 0)).transform;
                    Headwear.Put(rig.Head, spec.Hat, hash, spec.HatColor);
                    Garments.Accessorize(rig, spec);
                    return rig;
                }
                Object.Destroy(bodyG.gameObject);
            }
            var matClothes = M.Get("cloth_" + ColorUtility.ToHtmlStringRGB(clothes), clothes, 0.15f);
            var matAccent = M.Get("cloth_" + ColorUtility.ToHtmlStringRGB(accent), accent, 0.2f);
            var pantsColor = pants ?? new Color(0.15f, 0.15f, 0.18f);
            var matPants = M.Get("cloth_" + ColorUtility.ToHtmlStringRGB(pantsColor), pantsColor, 0.15f);
            // Ten ve saç rengi portreden örneklenir
            var skinCol = face != null ? face.Skin : Skin;
            var hairCol = face != null ? FaceLibrary.Readable(face.Hair, 0.05f, 0.75f) : Hair;
            var matSkin = face != null ? M.Get("skin_" + ColorUtility.ToHtmlStringRGB(skinCol), skinCol, 0.3f) : M.Get("skin", Skin, 0.35f);
            var matHair = face != null ? M.Get("hair_" + ColorUtility.ToHtmlStringRGB(hairCol), hairCol, 0.3f) : M.Get("hair", Hair, 0.3f);
            var matShoe = M.Get("shoe", new Color(0.08f, 0.06f, 0.05f), 0.6f);
            var matEye = M.Get("eye", Color.black, 0.9f);

            var body = Group("Body", root, Vector3.zero).transform;
            rig.Body = body;

            // Bacaklar (pivot kalçada)
            rig.LegL = Group("LegL", body, new Vector3(-0.13f, 0.9f, 0)).transform;
            rig.LegR = Group("LegR", body, new Vector3(0.13f, 0.9f, 0)).transform;
            foreach (var leg in new[] { rig.LegL, rig.LegR })
            {
                Capsule("Leg", leg, new Vector3(0, -0.45f, 0), new Vector3(0.18f, 0.45f, 0.18f), matPants);
                Box("Shoe", leg, new Vector3(0, -0.86f, 0.05f), new Vector3(0.18f, 0.1f, 0.3f), matShoe, collider: false);
            }

            // Gövde
            Capsule("Torso", body, new Vector3(0, 1.25f, 0), new Vector3(0.48f, 0.42f, 0.3f), matClothes);
            if (female)
                Cyl("Skirt", body, new Vector3(0, 0.82f, 0), new Vector3(0.5f, 0.2f, 0.4f), matClothes, collider: false);
            Box("Belt", body, new Vector3(0, 0.95f, 0), new Vector3(0.46f, 0.06f, 0.3f), matAccent, collider: false);
            Box("Collar", body, new Vector3(0, 1.58f, 0.08f), new Vector3(0.22f, 0.1f, 0.15f), matAccent, collider: false);

            // Kollar (pivot omuzda)
            rig.ArmL = Group("ArmL", body, new Vector3(-0.31f, 1.52f, 0)).transform;
            rig.ArmR = Group("ArmR", body, new Vector3(0.31f, 1.52f, 0)).transform;
            foreach (var arm in new[] { rig.ArmL, rig.ArmR })
            {
                Capsule("Arm", arm, new Vector3(0, -0.32f, 0), new Vector3(0.13f, 0.33f, 0.13f), matClothes);
                Sphere("Hand", arm, new Vector3(0, -0.66f, 0), Vector3.one * 0.12f, matSkin);
            }

            // Kafa
            var head = Group("Head", body, new Vector3(0, 1.82f, 0)).transform;
            rig.Head = head;
            Cyl("Neck", body, new Vector3(0, 1.66f, 0), new Vector3(0.12f, 0.06f, 0.12f), matSkin, collider: false);
            Sphere("Skull", head, Vector3.zero, new Vector3(0.3f, 0.34f, 0.3f), matSkin);
            {
                Sphere("Nose", head, new Vector3(0, -0.02f, 0.15f), new Vector3(0.05f, 0.07f, 0.06f), matSkin);
                Sphere("EyeL", head, new Vector3(-0.065f, 0.03f, 0.13f), Vector3.one * 0.04f, matEye);
                Sphere("EyeR", head, new Vector3(0.065f, 0.03f, 0.13f), Vector3.one * 0.04f, matEye);
                Box("Mouth", head, new Vector3(0, -0.08f, 0.135f), new Vector3(0.08f, 0.012f, 0.01f), M.Get("mouth", new Color(0.45f, 0.2f, 0.2f)), collider: false);
                Sphere("Hair", head, new Vector3(0, 0.07f, -0.02f), new Vector3(0.32f, 0.27f, 0.32f), matHair);
                if (female)
                    Capsule("LongHair", head, new Vector3(0, -0.12f, -0.1f), new Vector3(0.3f, 0.22f, 0.15f), matHair);
            }

            rig.HandR = Group("HandR", rig.ArmR, new Vector3(0, -0.66f, 0)).transform;
            return rig;
        }

        /// <summary>
        /// Kişinin görünüm tarifi: portresi olan herkes (şüpheliler, dedektif, yardımcılar, Sisören kasabalıları) portresine göre;
        /// portresiz kasabalılar kasabanın renk temasına göre rastgele ama sabit. Rol kıyafetleri (zaptiye, arabacı) burada giydirilir.
        /// </summary>
        static CharLook SpecFor(string name, string faceKey, FaceLibrary.FaceInfo face, bool female, int hash)
        {
            string town = GameData.Current != null ? GameData.Current.Id : "gizemli";
            CharLook spec = null;
            bool townsfolkPortrait = faceKey != null && (faceKey.StartsWith("npc_3") || faceKey == "bekci_ihsan");
            // Sisören kasabalı portreleri yalnızca Sisören'in sokaklarında; diğer kasabalarda kendi halkı
            if (!townsfolkPortrait || town == "sisoren") spec = CharacterLooks.For(faceKey)?.Clone();
            if (name.StartsWith("PolisMemuru"))
            {
                // Polis memuru (town_bg.png): lacivert kuşaklı yağmurluk, koyu pantolon, kep (kepi TownGenerator takar)
                spec = CharacterLooks.Generic(hash, town, false, false);
                spec.Body = "ranger"; spec.Legs = "ranger"; spec.Feet = "ranger"; spec.Coat = true; spec.Trench = true; spec.Scarf = false; spec.Prop = null;
                spec.CoatColor = CharLook.Hex("#141a2c"); spec.Top = CharLook.Hex("#1d2438"); spec.Shirt = CharLook.Hex("#c8c8c0"); spec.Pants = CharLook.Hex("#141826"); spec.Shoes = CharLook.Hex("#141414");
                spec.Tie = true; spec.TieColor = CharLook.Hex("#0e1018");
                spec.Hat = Headwear.Kind.None; spec.Beard = "mustache"; spec.Age = 30 + hash % 20; spec.HairColor = null;
                return spec;
            }
            if (spec == null && faceKey != null && faceKey.StartsWith("cocuk_"))
            {
                // Sokakta oynayan çocuklar
                spec = CharacterLooks.Generic(hash, town, female, false);
                spec.Age = 7 + hash % 5; spec.Height = 1.12f + (spec.Age - 7) * 0.06f; spec.Coat = false; spec.Prop = null; spec.Beard = null; spec.Shawl = false;
                spec.Hat = !female && hash % 3 == 0 ? Headwear.Kind.FlatCap : Headwear.Kind.None; spec.HairColor = null;
                return spec;
            }
            if (spec == null) spec = CharacterLooks.Generic(hash, town, face != null ? face.Female : (bool?)null, name == "Kasabalı");
            if (name == "Arabacı" || name == "OlayYeriUzmanı" || name == "Müşteri")
            {
                // Erkek rolleri: oturan arabacı / müşteri, diz çöken olay yeri uzmanı (etek ve el eşyası pozla çakışmasın)
                if (spec.Female || spec.Child) spec = CharacterLooks.Generic(hash + 7, town, false, false);
                spec.Prop = null; spec.Skirt = false;
                if (name != "Müşteri") spec.Hat = Headwear.Kind.None;
            }
            return spec;
        }

        /// <summary>NPC'ye mesleğine özel aksesuar ekler.</summary>
        public static void Dress(PersonRig rig, int npcId)
        {
            if (rig.HasModel) return;   // gerçek modelin kıyafeti / aksesuarı portre tarifinden (CharacterLooks)
            if (rig.HasModel)
            {
                // Gerçek modelin kıyafeti kendinde; yalnızca rol işaretleri eklenir
                if (npcId == 4)
                {
                    Cyl("PoliceCap", rig.Head, new Vector3(0, 0.17f, 0), new Vector3(0.36f, 0.07f, 0.36f), M.Get("policecap", new Color(0.08f, 0.12f, 0.22f)), collider: false);
                    Box("CapVisor", rig.Head, new Vector3(0, 0.12f, 0.17f), new Vector3(0.24f, 0.02f, 0.1f), M.Get("shoe", Color.black), collider: false);
                    Sphere("CapBadge", rig.Head, new Vector3(0, 0.19f, 0.18f), Vector3.one * 0.05f, M.Emissive("gold", new Color(1f, 0.8f, 0.2f), 0.6f));
                }
                if (npcId == 1 && rig.Human != null && rig.Human.ChestAnchor != null)
                {
                    // Kasap önlüğü (göğüs kemiğine bağlı, gövdeyle eğilir)
                    var ap = rig.Human.ChestAnchor;
                    Box("Apron", ap, new Vector3(0, -0.36f, 0.15f), new Vector3(0.36f, 0.62f, 0.015f), M.Get("apron", new Color(0.92f, 0.92f, 0.9f)), collider: false);
                    Box("Stain", ap, new Vector3(0.07f, -0.45f, 0.16f), new Vector3(0.1f, 0.09f, 0.01f), M.Get("blood", new Color(0.45f, 0.02f, 0.03f), 0.6f), collider: false);
                }
                return;
            }
            var body = rig.Body;
            switch (npcId)
            {
                case 1: // Kasap Hasan — kanlı önlük + bıyık
                    Box("Apron", body, new Vector3(0, 1.05f, 0.155f), new Vector3(0.42f, 0.7f, 0.02f), M.Get("apron", new Color(0.92f, 0.92f, 0.9f)), collider: false);
                    Box("Stain", body, new Vector3(0.08f, 0.95f, 0.168f), new Vector3(0.12f, 0.1f, 0.01f), M.Get("blood", new Color(0.45f, 0.02f, 0.03f), 0.6f), collider: false);
                    if (!rig.HasFace) Box("Mustache", rig.Head, new Vector3(0, -0.05f, 0.145f), new Vector3(0.14f, 0.03f, 0.03f), M.Get("hair", Hair), collider: false);
                    break;
                case 2: // Eczacı Selma — beyaz önlük + gözlük
                    Box("CoatFront", body, new Vector3(0, 1.0f, 0.15f), new Vector3(0.5f, 0.9f, 0.02f), M.Get("labcoat", new Color(0.96f, 0.97f, 0.98f)), collider: false);
                    if (!rig.HasFace) Box("Glasses", rig.Head, new Vector3(0, 0.03f, 0.15f), new Vector3(0.2f, 0.035f, 0.01f), M.Metal, collider: false);
                    break;
                case 3: // Muhtar Kemal — kravat + şapka (kasket)
                    Box("Tie", body, new Vector3(0, 1.32f, 0.16f), new Vector3(0.07f, 0.4f, 0.02f), M.Get("tie", new Color(0.55f, 0.08f, 0.1f)), collider: false);
                    if (rig.HasFace) break; // portrede başı açık
                    Cyl("Cap", rig.Head, new Vector3(0, 0.16f, 0.02f), new Vector3(0.34f, 0.05f, 0.36f), M.Get("cap", new Color(0.25f, 0.22f, 0.2f)), collider: false);
                    Box("CapBrim", rig.Head, new Vector3(0, 0.12f, 0.17f), new Vector3(0.22f, 0.02f, 0.1f), M.Get("cap", new Color(0.25f, 0.22f, 0.2f)), collider: false);
                    break;
                case 4: // Komiser Güneş — polis şapkası + rozet
                    Cyl("PoliceCap", rig.Head, new Vector3(0, 0.17f, 0), new Vector3(0.36f, 0.07f, 0.36f), M.Get("policecap", new Color(0.08f, 0.12f, 0.22f)), collider: false);
                    Box("CapVisor", rig.Head, new Vector3(0, 0.12f, 0.17f), new Vector3(0.24f, 0.02f, 0.1f), M.Get("shoe", Color.black), collider: false);
                    Sphere("CapBadge", rig.Head, new Vector3(0, 0.19f, 0.18f), Vector3.one * 0.05f, M.Emissive("gold", new Color(1f, 0.8f, 0.2f), 0.6f));
                    Box("ChestBadge", body, new Vector3(-0.12f, 1.38f, 0.16f), new Vector3(0.07f, 0.08f, 0.02f), M.Emissive("gold", new Color(1f, 0.8f, 0.2f), 0.6f), collider: false);
                    break;
                case 5: // Terzi Yahya — boyunda mezura + yelek
                    Box("TapeL", body, new Vector3(-0.1f, 1.3f, 0.155f), new Vector3(0.03f, 0.5f, 0.01f), M.Get("tape", new Color(0.95f, 0.85f, 0.2f)), collider: false);
                    Box("TapeR", body, new Vector3(0.1f, 1.3f, 0.155f), new Vector3(0.03f, 0.5f, 0.01f), M.Get("tape", new Color(0.95f, 0.85f, 0.2f)), collider: false);
                    if (!rig.HasFace) Box("Glasses", rig.Head, new Vector3(0, 0.03f, 0.15f), new Vector3(0.18f, 0.03f, 0.01f), M.Metal, collider: false);
                    break;
            }
        }

        /// <summary>Oyuncu dedektif: trençkot + fötr şapka.</summary>
        public static PersonRig BuildDetective(Transform parent)
        {
            // dedektif.png: koyu yağmurluk, beyaz gömlek, siyah kravat, ıslak fötr şapka
            var coat = new Color(0.2f, 0.19f, 0.17f);
            var rig = BuildPerson("DetectiveModel", parent, Vector3.zero, coat, new Color(0.82f, 0.8f, 0.76f), false, new Color(0.1f, 0.1f, 0.11f), "dedektif");
            if (rig.HasModel) return rig;   // trençkot, fötr ve kravat portre tarifinden
            Box("Kravat", rig.Body, new Vector3(0, 1.36f, 0.158f), new Vector3(0.06f, 0.36f, 0.02f), M.Get("tie_black", new Color(0.04f, 0.04f, 0.05f), 0.4f), collider: false);
            var matCoat = M.Get("trench", coat, 0.2f);
            Cyl("CoatTail", rig.Body, new Vector3(0, 0.78f, 0), new Vector3(0.5f, 0.22f, 0.36f), matCoat, collider: false);
            var hatMat = M.Get("fedora", new Color(0.12f, 0.11f, 0.1f), 0.25f);
            Cyl("HatBrim", rig.Head, new Vector3(0, 0.13f, 0), new Vector3(0.5f, 0.015f, 0.5f), hatMat, collider: false);
            Cyl("HatCrown", rig.Head, new Vector3(0, 0.21f, 0), new Vector3(0.3f, 0.08f, 0.3f), hatMat, collider: false);
            Cyl("HatBand", rig.Head, new Vector3(0, 0.155f, 0), new Vector3(0.305f, 0.02f, 0.305f), M.Get("hatband", new Color(0.4f, 0.1f, 0.1f)), collider: false);
            return rig;
        }

        // =================================================================
        //  DELİL OBJELERİ (hepsi hacimli primitive birleşimleri)
        // =================================================================
        public static GameObject BuildClue(ClueShape shape, Transform parent, Vector3 pos)
        {
            var g = Group("Clue_" + shape, parent, pos);
            var t = g.transform;
            var paper = M.Get("paper", new Color(0.9f, 0.86f, 0.74f), 0.05f);
            var blood = M.Get("blood", new Color(0.45f, 0.02f, 0.03f), 0.6f);
            switch (shape)
            {
                case ClueShape.Cleaver:
                    Box("Blade", t, new Vector3(0, 0.22f, 0), new Vector3(0.03f, 0.3f, 0.36f), M.Get("steel", new Color(0.55f, 0.55f, 0.58f), 0.75f, 0.9f), new Vector3(0, 0, 0), false);
                    Box("BloodEdge", t, new Vector3(0, 0.1f, 0.02f), new Vector3(0.035f, 0.08f, 0.3f), blood, collider: false);
                    Cyl("Handle", t, new Vector3(0, 0.3f, -0.3f), new Vector3(0.06f, 0.13f, 0.06f), M.Wood(new Color(0.35f, 0.2f, 0.1f)), new Vector3(90, 0, 0), false);
                    break;
                case ClueShape.Notebook:
                    Box("Cover", t, new Vector3(0, 0.025f, 0), new Vector3(0.32f, 0.05f, 0.42f), M.Get("blackleather", new Color(0.06f, 0.06f, 0.06f), 0.4f), collider: false);
                    Box("Pages", t, new Vector3(0.005f, 0.025f, 0), new Vector3(0.3f, 0.04f, 0.4f), paper, collider: false);
                    Box("RedMark", t, new Vector3(0, 0.052f, 0.05f), new Vector3(0.2f, 0.005f, 0.02f), blood, collider: false);
                    break;
                case ClueShape.Apron:
                    Box("Cloth", t, new Vector3(0, 0, 0), new Vector3(0.5f, 0.75f, 0.03f), M.Get("apron", new Color(0.92f, 0.92f, 0.9f)), collider: false);
                    Box("Tear", t, new Vector3(0.15f, -0.25f, 0.02f), new Vector3(0.12f, 0.2f, 0.01f), M.Get("dark", new Color(0.05f, 0.05f, 0.05f)), collider: false);
                    Box("Stain", t, new Vector3(-0.1f, 0.05f, 0.02f), new Vector3(0.18f, 0.15f, 0.01f), blood, collider: false);
                    Sphere("Key", t, new Vector3(0.12f, -0.1f, 0.03f), new Vector3(0.05f, 0.08f, 0.02f), M.Get("brass", new Color(0.75f, 0.6f, 0.25f), 0.8f, 0.9f));
                    break;
                case ClueShape.Bottle:
                    Cyl("Glass", t, new Vector3(0, 0.12f, 0), new Vector3(0.12f, 0.12f, 0.12f), M.Get("brownglass", new Color(0.35f, 0.18f, 0.06f), 0.95f, 0.1f), collider: false);
                    Cyl("Neck", t, new Vector3(0, 0.28f, 0), new Vector3(0.05f, 0.05f, 0.05f), M.Get("brownglass", new Color(0.35f, 0.18f, 0.06f), 0.95f, 0.1f), collider: false);
                    Box("Label", t, new Vector3(0, 0.12f, 0.06f), new Vector3(0.09f, 0.08f, 0.01f), paper, collider: false);
                    break;
                case ClueShape.Prescription:
                    Box("Book", t, new Vector3(0, 0.03f, 0), new Vector3(0.3f, 0.06f, 0.4f), M.Get("greenbook", new Color(0.12f, 0.3f, 0.2f)), collider: false);
                    Box("TornPage", t, new Vector3(0.05f, 0.065f, 0.05f), new Vector3(0.22f, 0.01f, 0.2f), paper, new Vector3(0, 15, 0), false);
                    break;
                case ClueShape.Leaf:
                    for (int i = 0; i < 3; i++)
                        Sphere("Leaf" + i, t, new Vector3((i - 1) * 0.1f, 0.02f, 0), new Vector3(0.1f, 0.02f, 0.22f), M.Get("ivy", new Color(0.2f, 0.45f, 0.12f), 0.4f));
                    Cyl("Stem", t, new Vector3(0, 0.02f, -0.12f), new Vector3(0.015f, 0.08f, 0.015f), M.Get("stem", new Color(0.3f, 0.25f, 0.1f)), new Vector3(90, 0, 0), false);
                    break;
                case ClueShape.Letter:
                    Box("Envelope", t, new Vector3(0, 0.01f, 0), new Vector3(0.36f, 0.015f, 0.24f), paper, collider: false);
                    Box("Letter", t, new Vector3(0.05f, 0.022f, 0.04f), new Vector3(0.3f, 0.008f, 0.38f), M.Get("paperwhite", new Color(0.95f, 0.93f, 0.88f)), new Vector3(0, -20, 0), false);
                    Cyl("WaxSeal", t, new Vector3(0, 0.025f, -0.05f), new Vector3(0.06f, 0.01f, 0.06f), blood, collider: false);
                    break;
                case ClueShape.Glasses:
                    var metal = M.Metal;
                    Cyl("LensL", t, new Vector3(-0.07f, 0.04f, 0), new Vector3(0.11f, 0.008f, 0.11f), M.Glass, new Vector3(90, 0, 0), false);
                    Cyl("LensR", t, new Vector3(0.07f, 0.04f, 0), new Vector3(0.11f, 0.008f, 0.11f), M.Glass, new Vector3(90, 0, 0), false);
                    Box("Bridge", t, new Vector3(0, 0.05f, 0), new Vector3(0.04f, 0.01f, 0.01f), metal, collider: false);
                    Box("ArmL", t, new Vector3(-0.13f, 0.04f, -0.07f), new Vector3(0.008f, 0.01f, 0.15f), metal, collider: false);
                    Box("ArmR", t, new Vector3(0.13f, 0.04f, -0.07f), new Vector3(0.008f, 0.01f, 0.15f), metal, new Vector3(0, -25, 0), false);
                    Box("Crack", t, new Vector3(0.07f, 0.04f, -0.006f), new Vector3(0.08f, 0.005f, 0.003f), M.Unlit("crack", Color.white), new Vector3(0, 0, 35), false);
                    break;
                case ClueShape.Safe:
                    var safeMat = M.Get("safe", new Color(0.2f, 0.22f, 0.22f), 0.55f, 0.8f);
                    Box("Body", t, Vector3.zero, new Vector3(0.9f, 1f, 0.8f), safeMat);
                    Box("Door", t, new Vector3(0, 0, -0.41f), new Vector3(0.75f, 0.85f, 0.03f), M.Get("safedoor", new Color(0.25f, 0.27f, 0.27f), 0.6f, 0.85f), collider: false);
                    Cyl("Dial", t, new Vector3(0.15f, 0.1f, -0.44f), new Vector3(0.18f, 0.02f, 0.18f), M.Get("brass", new Color(0.75f, 0.6f, 0.25f), 0.8f, 0.9f), new Vector3(90, 0, 0), false);
                    Box("Handle", t, new Vector3(-0.2f, 0, -0.45f), new Vector3(0.04f, 0.25f, 0.04f), M.Metal, collider: false);
                    break;
                case ClueShape.Badge:
                    var gold = M.Get("brass", new Color(0.75f, 0.6f, 0.25f), 0.8f, 0.9f);
                    Cyl("Shield", t, new Vector3(0, 0.015f, 0), new Vector3(0.16f, 0.015f, 0.2f), gold, collider: false);
                    Cyl("Star", t, new Vector3(0, 0.03f, 0), new Vector3(0.08f, 0.01f, 0.08f), M.Get("silver", new Color(0.8f, 0.8f, 0.85f), 0.9f, 1f), collider: false);
                    Box("Scratch", t, new Vector3(0, 0.032f, -0.06f), new Vector3(0.1f, 0.004f, 0.01f), M.Get("dark", new Color(0.05f, 0.05f, 0.05f)), new Vector3(0, 20, 0), false);
                    break;
                case ClueShape.FileFolder:
                    Box("Folder", t, new Vector3(0, 0.02f, 0), new Vector3(0.32f, 0.04f, 0.42f), M.Get("manila", new Color(0.8f, 0.65f, 0.38f)), collider: false);
                    Box("Stamp", t, new Vector3(0.05f, 0.042f, 0.1f), new Vector3(0.12f, 0.004f, 0.05f), blood, new Vector3(0, -12, 0), false);
                    Box("Papers", t, new Vector3(-0.02f, 0.03f, 0.01f), new Vector3(0.3f, 0.02f, 0.4f), paper, new Vector3(0, 4, 0), false);
                    break;
                case ClueShape.Button:
                    Cyl("Button", t, new Vector3(0, 0.02f, 0), new Vector3(0.09f, 0.015f, 0.09f), M.Get("hornbtn", new Color(0.3f, 0.18f, 0.1f), 0.7f), collider: false);
                    for (int i = 0; i < 4; i++)
                        Cyl("Hole" + i, t, new Vector3((i % 2 - 0.5f) * 0.03f, 0.03f, (i / 2 - 0.5f) * 0.03f), new Vector3(0.012f, 0.005f, 0.012f), M.Get("dark", new Color(0.05f, 0.05f, 0.05f)), collider: false);
                    break;
                case ClueShape.Spool:
                    var spoolWood = M.Wood(new Color(0.7f, 0.5f, 0.3f));
                    Cyl("Top", t, new Vector3(0, 0.2f, 0), new Vector3(0.14f, 0.015f, 0.14f), spoolWood, collider: false);
                    Cyl("Bottom", t, new Vector3(0, 0.01f, 0), new Vector3(0.14f, 0.015f, 0.14f), spoolWood, collider: false);
                    Cyl("Thread", t, new Vector3(0, 0.105f, 0), new Vector3(0.11f, 0.09f, 0.11f), M.Get("thread", new Color(0.6f, 0.08f, 0.1f), 0.3f), collider: false);
                    break;
                case ClueShape.Fabric:
                    Box("Fabric", t, new Vector3(0, 0.01f, 0), new Vector3(0.28f, 0.01f, 0.2f), M.Get("tweed", new Color(0.3f, 0.25f, 0.35f)), new Vector3(0, 30, 0), false);
                    Box("Fabric2", t, new Vector3(0.05f, 0.018f, 0.03f), new Vector3(0.12f, 0.01f, 0.1f), M.Get("tweed", new Color(0.3f, 0.25f, 0.35f)), new Vector3(0, -15, 0), false);
                    break;
                case ClueShape.Lining:
                    Box("Lining", t, new Vector3(0, 0, -0.02f), new Vector3(0.25f, 0.3f, 0.02f), M.Get("silk", new Color(0.55f, 0.1f, 0.15f), 0.7f), collider: false);
                    Box("USB", t, new Vector3(0.03f, -0.04f, -0.035f), new Vector3(0.05f, 0.1f, 0.02f), M.Metal, collider: false);
                    break;
            }
            return g;
        }

        // =================================================================
        //  KASABA DEKORLARI
        // =================================================================
        static int _lampCount;

        /// <summary>1800'ler gaz lambası: oluklu döküm demir ayak, merdiven kolu, dört camlı fener, piramit şapka + tepelik.</summary>
        public static GameObject StreetLamp(Transform parent, Vector3 pos, Color lightColor, bool withLight = true)
        {
            var g = Group("GazLambası", parent, pos);
            var t = g.transform;
            var iron = M.Get("castiron", new Color(0.06f, 0.07f, 0.06f), 0.45f, 0.75f);
            Cyl("Kaide", t, new Vector3(0, 0.18f, 0), new Vector3(0.5f, 0.18f, 0.5f), iron);
            Cyl("KaideBoğum", t, new Vector3(0, 0.5f, 0), new Vector3(0.3f, 0.16f, 0.3f), iron, collider: false);
            for (int i = 0; i < 6; i++)
                Box("Oluk", t, new Vector3(Mathf.Sin(i * 1.047f) * 0.11f, 1.3f, Mathf.Cos(i * 1.047f) * 0.11f), new Vector3(0.04f, 1.3f, 0.04f), iron, null, false);
            Cyl("Direk", t, new Vector3(0, 2.0f, 0), new Vector3(0.14f, 2.0f, 0.14f), iron);
            Sphere("Boğum", t, new Vector3(0, 2.7f, 0), Vector3.one * 0.22f, iron);
            Box("MerdivenKolu", t, new Vector3(0, 3.35f, 0), new Vector3(0.9f, 0.05f, 0.05f), iron, collider: false);
            Sphere("KolUçSol", t, new Vector3(-0.45f, 3.35f, 0), Vector3.one * 0.08f, iron);
            Sphere("KolUçSağ", t, new Vector3(0.45f, 3.35f, 0), Vector3.one * 0.08f, iron);
            Cyl("FenerAltı", t, new Vector3(0, 3.62f, 0), new Vector3(0.36f, 0.05f, 0.36f), iron, collider: false);
            var glass = M.Emissive("gaslamp_glass", new Color(1f, 0.62f, 0.26f), 2.6f);
            Box("FenerCamı", t, new Vector3(0, 3.95f, 0), new Vector3(0.36f, 0.55f, 0.36f), glass, collider: false);
            foreach (var c in new[] { new Vector3(-0.18f, 0, -0.18f), new Vector3(0.18f, 0, -0.18f), new Vector3(-0.18f, 0, 0.18f), new Vector3(0.18f, 0, 0.18f) })
                Box("FenerÇubuğu", t, c + new Vector3(0, 3.95f, 0), new Vector3(0.035f, 0.6f, 0.035f), iron, null, false);
            Box("FenerTavanı", t, new Vector3(0, 4.26f, 0), new Vector3(0.48f, 0.05f, 0.48f), iron, collider: false);
            Prism("FenerŞapka", t, new Vector3(0, 4.28f, 0), new Vector3(0.46f, 0.32f, 0.46f), iron);
            Prism("FenerŞapka2", t, new Vector3(0, 4.28f, 0), new Vector3(0.46f, 0.32f, 0.46f), iron, 90f);
            Sphere("Tepelik", t, new Vector3(0, 4.66f, 0), Vector3.one * 0.08f, iron);
            if (withLight)
            {
                var warm = new Color(1f, 0.6f, 0.26f);
                var l = PointLight("GazAlevi", t, new Vector3(0, 3.9f, 0), warm, 20f, 21f, (_lampCount++ % 3) == 0);
                g.AddComponent<LightFlicker>().Init(l, 0.06f);
            }
            return g;
        }

        public static GameObject Tree(Transform parent, Vector3 pos, float scale = 1f)
        {
            var g = Group("Tree", parent, pos, Random.Range(0, 360f));
            var t = g.transform;
            t.localScale = Vector3.one * scale;
            var bark = M.Bark(new Color(0.27f, 0.22f, 0.16f));
            Cyl("Trunk", t, new Vector3(0, 1.5f, 0), new Vector3(0.42f, 1.5f, 0.42f), bark);
            Cyl("Dal0", t, new Vector3(0.45f, 2.9f, 0.1f), new Vector3(0.14f, 0.75f, 0.14f), bark, new Vector3(0, 0, -38f), false);
            Cyl("Dal1", t, new Vector3(-0.42f, 3.1f, -0.15f), new Vector3(0.12f, 0.7f, 0.12f), bark, new Vector3(12f, 0, 34f), false);
            // Anadolu sonbaharı: yeşil ve hardal yapraklar
            var inner = M.Foliage(new Color(0.17f, 0.21f, 0.09f));
            var cards = M.FoliageCards(new Color(0.34f, 0.39f, 0.16f));
            var cards2 = M.FoliageCards(new Color(0.5f, 0.41f, 0.14f));
            int v = NatureMeshes.VariantOf(pos);
            void Lobe(string name, Vector3 at, Vector3 size, int variant, Material outer, float yaw)
            {
                // İçteki koyu kütle boşlukları kapatır, dıştaki kartlar yaprak silueti verir
                NatureMeshes.Place(name + "Ic", t, at, size * 0.8f, NatureMeshes.LeafCanopy(variant), inner, yaw);
                NatureMeshes.Place(name, t, at, size, NatureMeshes.LeafCards(variant), outer, yaw);
            }
            Lobe("Canopy0", new Vector3(0, 4.1f, 0), new Vector3(3.8f, 3.2f, 3.8f), v, cards, 0f);
            Lobe("Canopy1", new Vector3(1f, 3.5f, 0.6f), new Vector3(2.7f, 2.3f, 2.7f), v + 1, cards2, 70f);
            Lobe("Canopy2", new Vector3(-0.9f, 4.7f, -0.5f), new Vector3(2.5f, 2.2f, 2.5f), v + 2, cards, 140f);
            return g;
        }

        public static GameObject PineTree(Transform parent, Vector3 pos, float scale = 1f)
        {
            var g = Group("Pine", parent, pos, Random.Range(0, 360f));
            var t = g.transform;
            t.localScale = Vector3.one * scale;
            Cyl("Trunk", t, new Vector3(0, 1.2f, 0), new Vector3(0.3f, 1.2f, 0.3f), M.Bark(new Color(0.24f, 0.18f, 0.13f)));
            NatureMeshes.Place("Tac", t, new Vector3(0, 0.75f, 0), new Vector3(3.3f, 5.6f, 3.3f), NatureMeshes.PineCanopy(NatureMeshes.VariantOf(pos)), M.PineNeedles(new Color(0.17f, 0.3f, 0.2f)));
            return g;
        }

        public static GameObject Bench(Transform parent, Vector3 pos, float yRot)
        {
            var g = Group("Bench", parent, pos, yRot);
            var t = g.transform;
            var wood = M.Wood(new Color(0.4f, 0.25f, 0.14f));
            var iron = M.Get("iron", new Color(0.08f, 0.08f, 0.09f), 0.5f, 0.7f);
            for (int i = 0; i < 3; i++)
                Box("Slat" + i, t, new Vector3(0, 0.5f, -0.15f + i * 0.15f), new Vector3(1.8f, 0.05f, 0.12f), wood, collider: i == 1);
            Box("Back0", t, new Vector3(0, 0.8f, 0.25f), new Vector3(1.8f, 0.12f, 0.04f), wood, collider: false);
            Box("Back1", t, new Vector3(0, 1.0f, 0.25f), new Vector3(1.8f, 0.12f, 0.04f), wood, collider: false);
            foreach (var x in new[] { -0.8f, 0.8f })
            {
                Box("Leg", t, new Vector3(x, 0.25f, 0), new Vector3(0.06f, 0.5f, 0.5f), iron, collider: false);
                Box("BackLeg", t, new Vector3(x, 0.75f, 0.27f), new Vector3(0.06f, 0.55f, 0.04f), iron, collider: false);
            }
            return g;
        }

        public static GameObject Barrel(Transform parent, Vector3 pos)
        {
            var g = Group("Barrel", parent, pos, Random.Range(0, 360f));
            var t = g.transform;
            Cyl("Body", t, new Vector3(0, 0.5f, 0), new Vector3(0.7f, 0.5f, 0.7f), M.Wood(new Color(0.4f, 0.26f, 0.13f)));
            var iron = M.Get("iron", new Color(0.08f, 0.08f, 0.09f), 0.5f, 0.7f);
            Cyl("Ring0", t, new Vector3(0, 0.2f, 0), new Vector3(0.72f, 0.03f, 0.72f), iron, collider: false);
            Cyl("Ring1", t, new Vector3(0, 0.8f, 0), new Vector3(0.72f, 0.03f, 0.72f), iron, collider: false);
            return g;
        }

        public static GameObject Crate(Transform parent, Vector3 pos, float size = 0.8f)
        {
            var g = Group("Crate", parent, pos, Random.Range(-20f, 20f));
            var t = g.transform;
            var wood = M.Wood(new Color(0.5f, 0.36f, 0.2f));
            Box("Body", t, new Vector3(0, size / 2, 0), Vector3.one * size, wood);
            Box("BandA", t, new Vector3(0, size / 2, 0), new Vector3(size + 0.02f, 0.08f, size + 0.02f), M.Wood(new Color(0.35f, 0.24f, 0.13f)), collider: false);
            return g;
        }

        public static GameObject FencePost(Transform parent, Vector3 pos, float yRot, float length)
        {
            var g = Group("Fence", parent, pos, yRot);
            var t = g.transform;
            var wood = M.Wood(new Color(0.3f, 0.2f, 0.12f));
            int posts = Mathf.Max(2, Mathf.RoundToInt(length / 2f) + 1);
            for (int i = 0; i < posts; i++)
            {
                float x = -length / 2 + length * i / (posts - 1);
                Box("Post" + i, t, new Vector3(x, 0.6f, 0), new Vector3(0.15f, 1.2f, 0.15f), wood, collider: false);
            }
            Box("RailTop", t, new Vector3(0, 1.0f, 0), new Vector3(length, 0.1f, 0.06f), wood, texTiling: 0.5f);
            Box("RailBottom", t, new Vector3(0, 0.5f, 0), new Vector3(length, 0.1f, 0.06f), wood, collider: false);
            return g;
        }

        public static GameObject PoliceTapeLine(Transform parent, Vector3 a, Vector3 b)
        {
            var g = new GameObject("PoliceTape");
            g.transform.SetParent(parent, false);
            var tapeMat = M.Emissive("tape_yellow", new Color(1f, 0.85f, 0.1f), 0.5f);
            var postMat = M.Get("iron", new Color(0.08f, 0.08f, 0.09f), 0.5f, 0.7f);
            Cyl("PostA", g.transform, a + Vector3.up * 0.5f, new Vector3(0.08f, 0.5f, 0.08f), postMat, collider: false);
            Cyl("PostB", g.transform, b + Vector3.up * 0.5f, new Vector3(0.08f, 0.5f, 0.08f), postMat, collider: false);
            var mid = (a + b) / 2 + Vector3.up * 0.9f;
            var dir = b - a;
            float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            Box("Tape", g.transform, mid, new Vector3(0.02f, 0.08f, dir.magnitude), tapeMat, new Vector3(0, yaw, 0), false);
            return g;
        }
    }

    /// <summary>Sokak lambaları ve neonlar için hafif titreme.</summary>
    public class LightFlicker : MonoBehaviour
    {
        Light _light; float _base; float _amount; float _seed;
        /// <summary>Gün/gece çarpanı (gündüz sokak fenerleri söner).</summary>
        public float Multiplier = 1f;
        public void Init(Light l, float amount) { _light = l; _base = l.intensity; _amount = amount; _seed = Random.value * 100f; }
        void Update()
        {
            if (_light == null) return;
            float n = Mathf.PerlinNoise(_seed, Time.time * 3f);
            _light.intensity = _base * Multiplier * (1f - _amount + n * _amount * 2f);
        }
    }

    /// <summary>Basit sürekli döndürme (polis çakarı, saat, vb.).</summary>
    public class Spinner : MonoBehaviour
    {
        public Vector3 axis = Vector3.up; public float speed = 90f;
        void Update() => transform.Rotate(axis, speed * Time.deltaTime, Space.Self);
    }

    /// <summary>Delillerin üstünde yüzer şekilde süzülmesi ve parlaması.</summary>
    public class Bobber : MonoBehaviour
    {
        public float amplitude = 0.04f, speed = 2f;
        Vector3 _start; float _seed;
        void Start() { _start = transform.localPosition; _seed = Random.value * 10f; }
        void Update() => transform.localPosition = _start + Vector3.up * (Mathf.Sin(Time.time * speed + _seed) * amplitude);
    }
}
