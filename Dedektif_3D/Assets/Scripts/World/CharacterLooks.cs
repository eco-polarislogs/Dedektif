using System.Collections.Generic;
using UnityEngine;

namespace Dedektif3D.World
{
    /// <summary>
    /// Bir karakterin 3D görünüm tarifi — wwwroot/images portrelerine bakılarak elle çıkarıldı:
    /// yaş, saç / sakal, başlık, kıyafet katmanları (gömlek, yelek / ceket, palto, önlük, etek, şal, atkı),
    /// renkler, gözlük, kravat ve elindeki eşya. HumanLibrary bu tarifle modüler insan modelini montajlar.
    /// </summary>
    public class CharLook
    {
        public bool Female;
        public int Age = 40;
        /// <summary>Gövde / kol / bacak / ayak parçası tarzı: "peasant" (gömlek, kısa / kıvrık kol) ya da "ranger" (uzun kollu ceket, çizme).</summary>
        public string Body = "peasant", Arms, Legs = "peasant", Feet = "peasant";
        /// <summary>Koyu kumaş (yelek / ceket / kazak), açık kumaş (gömlek / bluz), kollar, pantolon, ayakkabı.</summary>
        public Color Top = Hex("#3a3128"), Shirt = Hex("#d8d0bf"), Pants = Hex("#2b2723"), Shoes = Hex("#1c1714");
        public Color? Sleeves;
        /// <summary>"short", "parted", "long", "buns", "bald".</summary>
        public string Hair = "parted";
        public Color? HairColor;
        /// <summary>null, "full", "stubble", "mustache".</summary>
        public string Beard;
        public Headwear.Kind Hat = Headwear.Kind.None;
        public Color? HatColor;
        public Color? Skin;
        // Kodla dikilen katmanlar
        public bool Coat, LabCoat, Skirt, Apron, Shawl, Scarf, Tie, Glasses, BibApron;
        public Color CoatColor = Hex("#2a2622"), SkirtColor = Hex("#3a2e2a"), ApronColor = Hex("#5a4632"), ShawlColor = Hex("#3c3a34"), ScarfColor = Hex("#5a3a2a"), TieColor = Hex("#141212");
        /// <summary>Elindeki eşya: cane, lantern, cleaver, scissors, book, axe, hammer, bottle, paper, pipe, crook, bag, tray, glass.</summary>
        public string Prop;
        /// <summary>Bekleme duruşu: null (rastgele), "idle_arms" (kollar kavuşturulmuş), "idle_1".</summary>
        public string Idle;
        public float Height;

        public bool Child => Age < 14;
        public bool Old => Age >= 60;
        public Color SleeveColor => Coat || LabCoat ? (LabCoat ? Hex("#e6e8e6") : CoatColor) : Sleeves ?? ((Arms ?? Body) == "ranger" ? Top : Shirt);
        public Color TopColor => Coat ? CoatColor : LabCoat ? Hex("#e6e8e6") : Top;

        public CharLook Clone() => (CharLook)MemberwiseClone();

        public static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }

    public static class CharacterLooks
    {
        static Color H(string h) => CharLook.Hex(h);
        static readonly Color Grey = H("#9a958e"), White = H("#d9d6d0"), Black = H("#151311"), DarkBrown = H("#2e2219"), Brown = H("#4a3424"), Salt = H("#8a8680");

        static Dictionary<string, CharLook> _t;

        /// <summary>Portre anahtarına (hasan, npc_101_talk, npc_201, dedektif ...) göre tarif; yoksa null.</summary>
        public static CharLook For(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            if (_t == null) Build();
            key = key.Split('#')[0];
            return _t.TryGetValue(key, out var l) ? l : null;
        }

        static void Add(string key, CharLook l) => _t[key] = l;

        static void Build()
        {
            _t = new Dictionary<string, CharLook>();
            // ---------------- DEDEKTİF & YARDIMCILAR ----------------
            // dedektif.png: 35 yaşında, koyu deri trençkot, beyaz gömlek, siyah kravat, ıslak fötr, hafif sakal
            Add("dedektif", new CharLook { Age = 35, Body = "ranger", Legs = "ranger", Feet = "ranger", Coat = true, CoatColor = H("#2b241f"), Top = H("#24201c"), Shirt = H("#dcd8cf"),
                Pants = H("#26262a"), Shoes = Black, Hair = "parted", HairColor = H("#1e1611"), Beard = "stubble", Hat = Headwear.Kind.Fedora, Tie = true, TieColor = H("#101012"), Height = 1.8f });
            // dedektif_helper.png (Yardımcı Çetin): siyah trençkot, beyaz gömlek, çizgili kravat, fötr, kirli sakal, elde harita
            Add("dedektif_helper", new CharLook { Age = 42, Body = "ranger", Legs = "ranger", Feet = "ranger", Coat = true, CoatColor = H("#1d1c1c"), Top = H("#1d1c1c"), Shirt = H("#e2e0dc"),
                Pants = H("#202022"), Shoes = Black, Hair = "short", HairColor = H("#2a2420"), Beard = "stubble", Hat = Headwear.Kind.Fedora, HatColor = H("#2a2a2c"), Tie = true, TieColor = H("#2c2c30"), Prop = "paper", Height = 1.78f });
            // bekci_hasan_helper.png (Bekçi Rıfat): yaşlı, bembeyaz sakal, bere, uzun kahve palto, fener, baston
            Add("bekci_hasan_helper", new CharLook { Age = 72, Body = "ranger", Feet = "ranger", Coat = true, CoatColor = H("#3b2c20"), Top = H("#3b2c20"), Shirt = H("#8a7a62"), Pants = H("#2a2420"),
                Hair = "short", HairColor = White, Beard = "full", Hat = Headwear.Kind.Beanie, HatColor = H("#2a2622"), Prop = "lantern", Height = 1.72f });
            // bekci_ihsan.png: yaşlı, beyaz kirli sakal, siyah bere, siyah palto, atkı, fener
            Add("bekci_ihsan", new CharLook { Age = 75, Body = "ranger", Feet = "ranger", Coat = true, CoatColor = H("#1e1d1d"), Top = H("#1e1d1d"), Pants = H("#1c1a18"), Hair = "short", HairColor = White,
                Beard = "stubble", Hat = Headwear.Kind.Beanie, HatColor = H("#1a1a1a"), Scarf = true, ScarfColor = H("#4a3a2a"), Prop = "lantern", Height = 1.7f });
            // tuccar_ilyas_helper.png: siyah sarık, beyaz gömlek, desenli koyu yelek, çapraz çanta kayışı, fener
            Add("tuccar_ilyas_helper", new CharLook { Age = 40, Body = "peasant", Top = H("#2a201a"), Shirt = H("#d6d0c4"), Pants = H("#2a2622"), Feet = "ranger", Hair = "short", HairColor = Black,
                Beard = "stubble", Hat = Headwear.Kind.HeadWrap, HatColor = H("#161414"), Prop = "lantern", Height = 1.77f });

            // ---------------- GİZEMLİ KASABA (Vaka #104) ----------------
            // hasan.png: ~55, kır dağınık saç, kır sakal, koyu gri kazak (kollar sıvalı), kirli deri önlük, satır
            Add("hasan", new CharLook { Age = 56, Body = "peasant", Top = H("#2f2d2b"), Shirt = H("#3a3734"), Sleeves = H("#2f2d2b"), Pants = H("#26221e"), Hair = "short", HairColor = Salt,
                Beard = "full", BibApron = true, ApronColor = H("#4a3220"), Prop = "cleaver", Height = 1.78f });
            // selma.png: ~45, koyu dalgalı omuz boyu saç, gözlük, koyu gri ceket, siyah bluz, ilaç şişesi
            Add("selma", new CharLook { Female = true, Age = 45, Body = "ranger", Top = H("#2c2a28"), Shirt = H("#1a1818"), Hair = "long", HairColor = H("#1e1612"), Glasses = true,
                Skirt = true, SkirtColor = H("#1f1d1c"), Prop = "bottle", Height = 1.65f });
            // kemal.png: ~58, kır saç yana taranmış, kısa kır sakal, koyu takım elbise, yelek, beyaz gömlek, koyu kravat
            Add("kemal", new CharLook { Age = 58, Body = "ranger", Legs = "ranger", Top = H("#262628"), Shirt = H("#dedcd8"), Pants = H("#262628"), Shoes = Black, Hair = "parted", HairColor = Grey,
                Beard = "stubble", Tie = true, TieColor = H("#1a1c24"), Prop = "paper", Height = 1.76f });
            // gunes.png: ~30, sarı toplu saç, lacivert üniforma, kollar kavuşturulmuş
            Add("gunes", new CharLook { Female = true, Age = 30, Body = "ranger", Legs = "ranger", Feet = "ranger", Top = H("#1b2338"), Shirt = H("#1b2338"), Pants = H("#1b2338"), Shoes = Black,
                Hair = "buns", HairColor = H("#b89a60"), Hat = Headwear.Kind.PoliceCap, Idle = "idle_arms", Height = 1.68f });
            // yahya.png: ~55, koyu kır saç, kirli sakal, açık gri gömlek, koyu yelek, kravat, boyunda mezura, makas
            Add("yahya", new CharLook { Age = 55, Body = "peasant", Top = H("#22201e"), Shirt = H("#9a9790"), Sleeves = H("#9a9790"), Pants = H("#262422"), Hair = "parted", HairColor = H("#3e3832"),
                Beard = "stubble", Tie = true, TieColor = H("#1a1818"), Prop = "scissors", Height = 1.74f });

            // ---------------- GÖLGEŞEHİR (Vaka #201) ----------------
            // npc_101 Oduncu Tahsin: bere, kır sakal, kırmızı ekose gömlek, siyah deri yelek, balta
            Add("npc_101_talk", new CharLook { Age = 52, Body = "peasant", Top = H("#1e1a17"), Shirt = H("#7a2620"), Sleeves = H("#7a2620"), Pants = H("#2f3026"), Feet = "ranger", Hair = "short", HairColor = Salt,
                Beard = "full", Hat = Headwear.Kind.Beanie, HatColor = H("#4a4638"), Prop = "axe", Height = 1.8f });
            // npc_102 Manav Ayşe: kahve dalgalı uzun saç, kahve örgü kazak, yeşil önlük
            Add("npc_102_talk", new CharLook { Female = true, Age = 34, Body = "ranger", Top = H("#6a4528"), Shirt = H("#6a4528"), Hair = "long", HairColor = H("#3a2618"), Apron = true, ApronColor = H("#46503a"),
                Skirt = true, SkirtColor = H("#3a2c22"), Height = 1.64f });
            // npc_103 Demirci Kâzım: siyah saç, siyah sakal, gri tişört, deri önlük, çekiç, iri yapılı
            Add("npc_103_talk", new CharLook { Age = 40, Body = "peasant", Top = H("#3a3a38"), Shirt = H("#4a4a46"), Sleeves = H("#4a4a46"), Pants = H("#24221f"), Feet = "ranger", Hair = "short", HairColor = Black,
                Beard = "full", BibApron = true, ApronColor = H("#2e241c"), Prop = "hammer", Height = 1.84f });
            // npc_104 Bakkal Naciye: kısa koyu saç, desenli kahve gömlek, koyu haki yelek
            Add("npc_104_talk", new CharLook { Female = true, Age = 50, Body = "peasant", Top = H("#3a3828"), Shirt = H("#6a4434"), Sleeves = H("#6a4434"), Hair = "buns", HairColor = H("#1e1814"),
                Skirt = true, SkirtColor = H("#2e2620"), Height = 1.62f });
            // npc_105 Hekim Sevgi: siyah topuz saç, beyaz önlük, koyu gömlek, stetoskop
            Add("npc_105_talk", new CharLook { Female = true, Age = 34, Body = "ranger", LabCoat = true, Shirt = H("#22262c"), Top = H("#e6e8e6"), Hair = "buns", HairColor = H("#0e0c0c"),
                Skirt = true, SkirtColor = H("#24262a"), Height = 1.66f });
            // npc_106 Muhtar Cevdet: kır saç, kır bıyık, gri takım, çizgili kravat, mühür
            Add("npc_106_talk", new CharLook { Age = 62, Body = "ranger", Legs = "ranger", Top = H("#34363a"), Shirt = H("#dcdad6"), Pants = H("#34363a"), Shoes = Black, Hair = "parted", HairColor = Grey,
                Beard = "mustache", Tie = true, TieColor = H("#3a3e4a"), Prop = "paper", Height = 1.75f });
            // npc_107 Fehmi Bey: koyu kasket, kır sakal, beyaz gömlek, yıpranmış kahve yelek, baston
            Add("npc_107_talk", new CharLook { Age = 63, Body = "peasant", Top = H("#4a3a2a"), Shirt = H("#cfc8b8"), Sleeves = H("#cfc8b8"), Pants = H("#2a2622"), Hair = "short", HairColor = Salt,
                Beard = "full", Hat = Headwear.Kind.FlatCap, HatColor = H("#2a2a2a"), Prop = "cane", Height = 1.72f });
            Add("npc_107", For("npc_107_talk"));
            // npc_108 Kunduracı Rasim: ~75, beyaz saç, gözlük, beyaz kirli sakal, mavi ekose gömlek, deri önlük
            Add("npc_108_talk", new CharLook { Age = 76, Body = "peasant", Top = H("#2e3a52"), Shirt = H("#3e4a64"), Sleeves = H("#3e4a64"), Pants = H("#2a2a2a"), Hair = "short", HairColor = White,
                Beard = "stubble", Glasses = true, BibApron = true, ApronColor = H("#3a2c22"), Prop = "hammer", Height = 1.68f });

            // ---------------- SİSÖREN (Vaka #301) ----------------
            // npc_201 Telgrafçı Rüstem: dağınık kır saç, yuvarlak gözlük, kır kirli sakal, çizgili gömlek, koyu yelek
            Add("npc_201", new CharLook { Age = 66, Body = "peasant", Top = H("#2a2e28"), Shirt = H("#8a8676"), Sleeves = H("#8a8676"), Pants = H("#2a2824"), Hair = "short", HairColor = Grey,
                Beard = "stubble", Glasses = true, Height = 1.72f });
            // npc_202 Kahveci İrfan: kır saç, gür kır bıyık, koyu ekose gömlek, gri önlük, çay bardağı
            Add("npc_202", new CharLook { Age = 62, Body = "peasant", Top = H("#2c3036"), Shirt = H("#3a4048"), Sleeves = H("#3a4048"), Pants = H("#26241f"), Hair = "short", HairColor = Grey,
                Beard = "mustache", Apron = true, ApronColor = H("#5c5a52"), Prop = "glass", Height = 1.73f });
            // npc_203 Sinemacı Nejat: geriye taranmış kır-siyah saç, koyu tüvit ceket, siyah gömlek
            Add("npc_203", new CharLook { Age = 56, Body = "ranger", Legs = "ranger", Top = H("#2c2e2c"), Shirt = H("#141414"), Pants = H("#222222"), Shoes = Black, Hair = "parted", HairColor = H("#3e3c3a"),
                Height = 1.77f });
            // npc_204 Bakkal Cemile: açık başörtüsü, yeşil hırka, çiçekli elbise, bej önlük
            Add("npc_204", new CharLook { Female = true, Age = 60, Body = "ranger", Top = H("#2e4632"), Shirt = H("#2e4632"), Hair = "buns", HairColor = Grey, Hat = Headwear.Kind.Headscarf, HatColor = H("#b8b2a6"),
                Apron = true, ApronColor = H("#9a8e78"), Skirt = true, SkirtColor = H("#3a2a2a"), Height = 1.6f });
            // npc_205 Sahaf Hikmet: beyaz saç, beyaz sakal, gözlük, krem gömlek, kahve tüvit yelek, kitap
            Add("npc_205", new CharLook { Age = 72, Body = "peasant", Top = H("#4a3e2e"), Shirt = H("#d8ceb6"), Sleeves = H("#d8ceb6"), Pants = H("#2e2a24"), Hair = "parted", HairColor = White,
                Beard = "full", Glasses = true, Prop = "book", Height = 1.7f });
            // npc_206 Muhtar Meliha: kır topuz saç, koyu petrol mavisi palto, desenli eşarp, belge
            Add("npc_206", new CharLook { Female = true, Age = 62, Body = "ranger", Coat = true, CoatColor = H("#1e3238"), Top = H("#1e3238"), Shirt = H("#22262a"), Hair = "buns", HairColor = Grey,
                Scarf = true, ScarfColor = H("#3a3a40"), Skirt = true, SkirtColor = H("#1c2226"), Prop = "paper", Height = 1.64f });
            // npc_207 Tütüncü Nermin: kır-siyah topuz, işlemeli koyu yelek, çiçekli gömlek, pipo
            Add("npc_207", new CharLook { Female = true, Age = 56, Body = "peasant", Top = H("#2a201c"), Shirt = H("#6a4a42"), Sleeves = H("#6a4a42"), Hair = "buns", HairColor = H("#4a4642"),
                Skirt = true, SkirtColor = H("#2e2420"), Prop = "pipe", Height = 1.62f });
            // npc_208 Çoban Durmuş: örgü bere, kır sakal, kuzu postu yelek, koyu kazak, değnek
            Add("npc_208", new CharLook { Age = 66, Body = "ranger", Top = H("#6a5a46"), Shirt = H("#2a2a2a"), Sleeves = H("#2a2a2a"), Pants = H("#2a2622"), Feet = "ranger", Hair = "short", HairColor = Salt,
                Beard = "full", Hat = Headwear.Kind.Beanie, HatColor = H("#4a4a48"), Prop = "crook", Height = 1.74f });
            // npc_209 Tüpçü: kısa kır saç, kirli sakal, mavi iş ceketi, iş eldiveni
            Add("npc_209", new CharLook { Age = 56, Body = "ranger", Legs = "ranger", Feet = "ranger", Top = H("#26344a"), Shirt = H("#26344a"), Pants = H("#222a36"), Hair = "short", HairColor = Salt,
                Beard = "stubble", Height = 1.76f });
            // npc_210 Hurdacı kadın: koyu başörtüsü (sarık gibi bağlı), kır saç, siyah deri ceket, eldiven
            Add("npc_210", new CharLook { Female = true, Age = 50, Body = "ranger", Legs = "ranger", Feet = "ranger", Top = H("#1c1a18"), Shirt = H("#1c1a18"), Pants = H("#24221e"), Hair = "buns", HairColor = Salt,
                Hat = Headwear.Kind.HeadWrap, HatColor = H("#2a2826"), Height = 1.64f });
            // npc_211: ~75, beyaz desenli başörtüsü, bordo hırka, çiçekli etek, tespih
            Add("npc_211", new CharLook { Female = true, Age = 76, Body = "ranger", Top = H("#5a1e24"), Shirt = H("#5a1e24"), Hair = "buns", HairColor = White, Hat = Headwear.Kind.Headscarf, HatColor = H("#d8d2c6"),
                Skirt = true, SkirtColor = H("#3a2a30"), Height = 1.55f });
            // npc_212: ~80, siyah başörtüsü, gri yün şal
            Add("npc_212", new CharLook { Female = true, Age = 82, Body = "ranger", Top = H("#3a3a36"), Shirt = H("#3a3a36"), Hair = "buns", HairColor = White, Hat = Headwear.Kind.Headscarf, HatColor = H("#141414"),
                Shawl = true, ShawlColor = H("#4a4a42"), Skirt = true, SkirtColor = H("#2a2826"), Prop = "cane", Height = 1.52f });
            // npc_213 Dokumacı: kır-siyah örgü saç, yeşil-gri ekose şal
            Add("npc_213", new CharLook { Female = true, Age = 46, Body = "ranger", Top = H("#2a2a28"), Shirt = H("#2a2a28"), Hair = "long", HairColor = H("#3a3634"), Shawl = true, ShawlColor = H("#2e4236"),
                Skirt = true, SkirtColor = H("#2a2624"), Height = 1.63f });

            // Sisören kasabalıları (npc_301-318)
            Add("npc_301", new CharLook { Age = 74, Body = "ranger", Coat = true, CoatColor = H("#3c4a38"), Top = H("#3c4a38"), Pants = H("#2a2824"), Hair = "short", HairColor = White, Beard = "mustache",
                Hat = Headwear.Kind.FlatCap, HatColor = H("#4a4a48"), Scarf = true, ScarfColor = H("#5a4630"), Prop = "cane", Height = 1.68f });
            Add("npc_302", new CharLook { Age = 68, Body = "peasant", Top = H("#6a5236"), Shirt = H("#6a3a2e"), Sleeves = H("#6a3a2e"), Pants = H("#2e2a24"), Hair = "short", HairColor = Grey, Beard = "mustache",
                Hat = Headwear.Kind.FlatCap, HatColor = H("#5a4632"), Prop = "glass", Height = 1.7f });
            Add("npc_303", new CharLook { Age = 18, Body = "peasant", Top = H("#2a3450"), Shirt = H("#d0ccc4"), Sleeves = H("#c8c6c2"), Pants = H("#26241f"), Hair = "short", HairColor = Black,
                Apron = true, ApronColor = H("#5a4632"), Prop = "tray", Height = 1.74f });
            Add("npc_304", new CharLook { Female = true, Age = 80, Body = "ranger", Top = H("#5a4632"), Shirt = H("#5a4632"), Hair = "buns", HairColor = White, Hat = Headwear.Kind.Headscarf, HatColor = H("#2a3e7a"),
                Skirt = true, SkirtColor = H("#3a2a22"), Prop = "cane", Height = 1.5f });
            Add("npc_305", new CharLook { Age = 22, Body = "peasant", Top = H("#2e4430"), Shirt = H("#6a2a24"), Sleeves = H("#6a2a24"), Pants = H("#4a3a2a"), Feet = "ranger", Hair = "short", HairColor = Black,
                Prop = "logs", Height = 1.78f });
            Add("npc_306", new CharLook { Age = 66, Body = "ranger", Legs = "ranger", Top = H("#2e3e32"), Shirt = H("#2e3e32"), Pants = H("#2a3028"), Shoes = Black, Hair = "short", HairColor = Grey, Beard = "mustache",
                Hat = Headwear.Kind.PostCap, HatColor = H("#3a4a3a"), Prop = "bag", Height = 1.72f });
            Add("npc_307", new CharLook { Age = 16, Body = "ranger", Top = H("#2e4a3a"), Shirt = H("#2e4a3a"), Pants = H("#2a2a2a"), Hair = "short", HairColor = Black, Height = 1.7f });
            Add("npc_308", new CharLook { Female = true, Age = 52, Body = "ranger", Top = H("#264030"), Shirt = H("#264030"), Hair = "long", HairColor = H("#1e1814"), Skirt = true, SkirtColor = H("#2a2422"), Height = 1.62f });
            Add("npc_309", new CharLook { Age = 68, Body = "ranger", Legs = "ranger", Top = H("#26382c"), Shirt = H("#d0ccc0"), Pants = H("#2a2a26"), Hair = "parted", HairColor = Grey, Beard = "stubble",
                Tie = true, TieColor = H("#2a2620"), Prop = "paper", Height = 1.71f });
            Add("npc_310", new CharLook { Age = 19, Body = "peasant", Top = H("#2e2e2c"), Shirt = H("#3a3a36"), Sleeves = H("#3a3a36"), Pants = H("#26241f"), Hair = "long", HairColor = Black,
                Apron = true, ApronColor = H("#2a2a2a"), Prop = "tray", Height = 1.73f });
            Add("npc_311", new CharLook { Age = 18, Body = "peasant", Top = H("#3a3a34"), Shirt = H("#4a4434"), Sleeves = H("#4a4434"), Pants = H("#26241f"), Hair = "short", HairColor = Black,
                Apron = true, ApronColor = H("#2e4a36"), Height = 1.72f });
            Add("npc_312", new CharLook { Age = 19, Body = "ranger", Top = H("#2e4632"), Shirt = H("#2e4632"), Pants = H("#2a2a2a"), Hair = "parted", HairColor = Black, Apron = true, ApronColor = H("#3a4a3a"),
                Prop = "book", Height = 1.72f });
            Add("npc_313", new CharLook { Age = 20, Body = "ranger", Legs = "ranger", Feet = "ranger", Top = H("#2a3a5a"), Shirt = H("#2a3a5a"), Pants = H("#2a2a2a"), Hair = "long", HairColor = Black, Height = 1.74f });
            Add("npc_314", new CharLook { Age = 20, Body = "ranger", Top = H("#3e4430"), Shirt = H("#3e4430"), Pants = H("#2a2824"), Feet = "ranger", Hair = "parted", HairColor = H("#3a2a1e"),
                Apron = true, ApronColor = H("#5a5446"), Height = 1.75f });
            Add("npc_315", new CharLook { Female = true, Age = 9, Body = "ranger", Top = H("#7a2a26"), Shirt = H("#7a2a26"), Hair = "long", HairColor = H("#1a1410"), Scarf = true, ScarfColor = H("#6a4a30"),
                Skirt = true, SkirtColor = H("#2a2a3a"), Height = 1.3f });
            Add("npc_316", new CharLook { Age = 10, Body = "ranger", Top = H("#7a4a2a"), Shirt = H("#7a4a2a"), Pants = H("#22242a"), Hair = "short", HairColor = H("#2a1e16"), Prop = "book", Height = 1.36f });
            Add("npc_317", new CharLook { Female = true, Age = 9, Body = "ranger", Top = H("#9a8a74"), Shirt = H("#9a8a74"), Hair = "long", HairColor = H("#1a1410"), Skirt = true, SkirtColor = H("#3a2e2a"), Height = 1.3f });
            Add("npc_318", new CharLook { Age = 11, Body = "peasant", Top = H("#bfb29a"), Shirt = H("#2a2a2a"), Sleeves = H("#2a2a2a"), Pants = H("#262a30"), Feet = "ranger", Hair = "short", HairColor = H("#2a1e16"),
                Hat = Headwear.Kind.Beanie, HatColor = H("#262626"), Prop = "crook", Height = 1.4f });
        }

        // =================================================================
        //  İSİMSİZ KASABALILAR — kasabanın temasına uygun, anahtardan sabit
        // =================================================================
        static readonly string[] MenTops = { "#3a3128", "#2e2a26", "#4a3a2a", "#2a2e34", "#33382e", "#3c2e2a", "#2a2622", "#4a4234" };
        static readonly string[] Shirts = { "#d8d0bf", "#c8bea8", "#b8b0a0", "#9a9284", "#d6d2c8", "#7a6a56" };
        static readonly string[] WomenTops = { "#4a2e2a", "#2e3a4a", "#3a4232", "#5a4632", "#3a2e3a", "#2e2a26", "#6a5a46" };
        static readonly string[] Skirts = { "#2e2622", "#3a2a2a", "#22262e", "#2a2e26", "#3a3228", "#1e1c1a" };
        static readonly string[] Scarves = { "#b8b2a6", "#3a2a2a", "#2a3e5a", "#4a3a2a", "#141414", "#5a2a2a", "#6a6a5a" };

        /// <summary>Portresi olmayan kasabalı. Tema: gizemli (gri-mavi), golge_sehir (kahve-hardal), sisoren (yeşil-haki).</summary>
        public static CharLook Generic(int seed, string townId, bool? female = null, bool allowChild = true)
        {
            var rnd = new System.Random(seed);
            int R(int n) => rnd.Next(n);
            Color Pick(string[] a) => H(a[R(a.Length)]);
            Color Theme(Color c)
            {
                // Kasaba renk teması kıyafetlere hafifçe siner
                var t = townId == "golge_sehir" ? H("#7a6430") : townId == "sisoren" ? H("#3e5a3a") : H("#34425a");
                return Color.Lerp(c, t, 0.22f);
            }
            bool fem = female ?? R(100) < 42;
            int age = allowChild && R(100) < 12 ? 7 + R(6) : 18 + R(60);
            var l = new CharLook { Female = fem, Age = age };
            if (fem)
            {
                l.Body = "ranger"; l.Top = Theme(Pick(WomenTops)); l.Shirt = l.Top; l.Skirt = true; l.SkirtColor = Theme(Pick(Skirts));
                l.Hair = R(2) == 0 ? "buns" : "long";
                if (age >= 16 && R(100) < 60) { l.Hat = Headwear.Kind.Headscarf; l.HatColor = Pick(Scarves); l.Hair = "buns"; }
                if (age >= 50 && R(100) < 45) { l.Shawl = true; l.ShawlColor = Theme(Pick(MenTops)); }
                if (R(100) < 18) { l.Apron = true; l.ApronColor = H("#9a8e78"); }
                if (age >= 70 && R(100) < 50) l.Prop = "cane";
            }
            else
            {
                l.Body = R(100) < 55 ? "peasant" : "ranger";
                l.Legs = R(100) < 70 ? "peasant" : "ranger"; l.Feet = R(100) < 50 ? "ranger" : "peasant";
                l.Top = Theme(Pick(MenTops)); l.Shirt = Pick(Shirts); l.Pants = Theme(H("#2a2622")) * (0.8f + R(40) / 100f); l.Pants.a = 1f;
                if (l.Body == "peasant" && R(100) < 50) l.Sleeves = l.Shirt;
                int r = R(100);
                l.Hair = r < 40 ? "short" : r < 85 ? "parted" : "long";
                if (age >= 25) { int b = R(100); l.Beard = b < 30 ? "full" : b < 55 ? "mustache" : b < 70 ? "stubble" : null; }
                if (age >= 14)
                {
                    int h = R(100);
                    l.Hat = h < 34 ? Headwear.Kind.Fez : h < 54 ? Headwear.Kind.FlatCap : h < 62 ? Headwear.Kind.Bowler : h < 66 ? Headwear.Kind.TopHat : h < 72 ? Headwear.Kind.Beanie : Headwear.Kind.None;
                }
                else if (R(100) < 35) l.Hat = Headwear.Kind.FlatCap;
                if (age >= 40 && R(100) < 22) { l.Coat = true; l.CoatColor = Theme(Pick(MenTops)); }
                if (age >= 18 && R(100) < 12) l.Scarf = true;
                if (age >= 68 && R(100) < 55) l.Prop = "cane";
            }
            if (age >= 58) l.HairColor = age >= 70 ? White : Grey;
            l.Height = l.Child ? 1.1f + (age - 6) * 0.06f : (fem ? 1.58f : 1.70f) + R(10) / 100f - (age > 70 ? 0.04f : 0f);
            return l;
        }
    }
}
