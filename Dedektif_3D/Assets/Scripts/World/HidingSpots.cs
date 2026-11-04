using System.Collections.Generic;
using Dedektif3D.Core;
using UnityEngine;
using static Dedektif3D.World.Shapes;
using M = Dedektif3D.World.MaterialLibrary;

namespace Dedektif3D.World
{
    public enum HideKind { InPlace, Drawer, Chest, Painting, Floorboard, Rug }

    /// <summary>Gizli delilin saklandığı yer: çekmece, sandık, tablo arkası, gevşek döşeme tahtası, halı altı.</summary>
    public class HidingSpotDef
    {
        public int ClueId;
        public HideKind Kind;
        public string Label;
        public GameObject Root;         // etkileşim noktası
        public GameObject ClueObject;   // içindeki delil objesi (ClueInteractable buna eklenir)
        public Transform Lid;           // açılan parça (çekmece, kapak, tablo, tahta, halı)
        public Vector3 LidOpenPos;      // yerel
        public Quaternion LidOpenRot;   // yerel
    }

    /// <summary>
    /// Gizli delilleri odanın içine saklar (Sherlock Holmes / L.A. Noire tarzı arama):
    /// delil görünmez, ancak saklandığı mobilya açılınca ortaya çıkar. Saklama yerleri sıradan eşya gibi görünür;
    /// Dedektif Gözü [Q] veya dikkatli kurcalama ile fark edilir.
    /// Oda yerel koordinatı: X -5..5, Z -4..4 (kapı -Z duvarında, NPC +Z tarafında).
    /// </summary>
    public static class HidingSpots
    {
        struct Slot { public Vector3 Pos; public float Yaw; public bool Wall; public Slot(float x, float z, float yaw, bool wall) { Pos = new Vector3(x, 0, z); Yaw = yaw; Wall = wall; } }

        // Duvar dibinde ve zeminde, mobilya / delil sehpalarıyla çakışmayan yerler
        static readonly Slot[] Slots =
        {
            new Slot(-3.1f, -3.98f, 0f, true),     // ön duvar sol (kapının solu)
            new Slot(-2.3f, -0.9f, 0f, false),     // zemin sol
            new Slot(3.1f, -3.98f, 0f, true),      // ön duvar sağ
            new Slot(2.4f, -0.2f, 90f, false),     // zemin sağ
            new Slot(-4.98f, -1.9f, 90f, true),    // sol duvar
            new Slot(4.98f, -1.9f, -90f, true),    // sağ duvar
        };

        static readonly Dictionary<HideKind, string> Labels = new Dictionary<HideKind, string>
        {
            { HideKind.Drawer, "Kilitsiz çekmeceli dolap" }, { HideKind.Chest, "Eski ahşap sandık" }, { HideKind.Painting, "Duvardaki yağlı boya tablo" },
            { HideKind.Floorboard, "Gıcırdayan döşeme tahtası" }, { HideKind.Rug, "Kenarı kalkık kilim" }, { HideKind.InPlace, "Gölgede kalan bir ayrıntı" },
        };

        public static string LabelOf(HideKind k) => Labels[k];

        /// <summary>Kırsal kasabalarda (Gölgeşehir / Sisören) gizli delili bir saklama yerine koyar.</summary>
        public static HidingSpotDef Build(Transform room, ClueDef clue, ClueShape propShape, int slotIndex)
        {
            var slot = Slots[slotIndex % Slots.Length];
            HideKind kind;
            int h = Mathf.Abs(clue.Id * 7 + slotIndex * 3);
            if (slot.Wall) kind = new[] { HideKind.Drawer, HideKind.Chest, HideKind.Painting }[h % 3];
            else kind = h % 2 == 0 ? HideKind.Floorboard : HideKind.Rug;

            var root = Group("SaklamaYeri_" + clue.Id, room, slot.Pos, slot.Yaw).transform;
            var spot = new HidingSpotDef { ClueId = clue.Id, Kind = kind, Label = Labels[kind], Root = root.gameObject };
            var wood = M.Wood(new Color(0.33f, 0.21f, 0.12f));
            var dark = M.Get("hide_cavity", new Color(0.03f, 0.025f, 0.02f), 0.05f);
            var brass = M.Get("brass", new Color(0.75f, 0.6f, 0.25f), 0.8f, 0.9f);
            Transform propParent; Vector3 propPos; float propScale = 0.75f;

            switch (kind)
            {
                case HideKind.Drawer:
                {
                    Box("Dolap", root, new Vector3(0, 0.45f, 0.27f), new Vector3(1.0f, 0.9f, 0.5f), wood);
                    Box("DolapÜst", root, new Vector3(0, 0.92f, 0.28f), new Vector3(1.06f, 0.05f, 0.54f), M.Wood(new Color(0.26f, 0.16f, 0.09f)), collider: false);
                    Box("AltÇekmece", root, new Vector3(0, 0.25f, 0.525f), new Vector3(0.86f, 0.28f, 0.02f), M.Wood(new Color(0.28f, 0.17f, 0.1f)), collider: false);
                    Sphere("AltKulp", root, new Vector3(0, 0.25f, 0.545f), Vector3.one * 0.05f, brass);
                    var drawer = Group("Çekmece", root, new Vector3(0, 0.66f, 0.3f)).transform;
                    Box("ÇekmeceÖn", drawer, new Vector3(0, 0, 0.225f), new Vector3(0.86f, 0.26f, 0.03f), M.Wood(new Color(0.3f, 0.19f, 0.11f)), collider: false);
                    Box("ÇekmeceTaban", drawer, new Vector3(0, -0.11f, 0f), new Vector3(0.8f, 0.02f, 0.44f), dark, collider: false);
                    Box("ÇekmeceYanL", drawer, new Vector3(-0.4f, -0.04f, 0f), new Vector3(0.02f, 0.16f, 0.44f), wood, collider: false);
                    Box("ÇekmeceYanR", drawer, new Vector3(0.4f, -0.04f, 0f), new Vector3(0.02f, 0.16f, 0.44f), wood, collider: false);
                    Sphere("Kulp", drawer, new Vector3(0, 0, 0.25f), Vector3.one * 0.05f, brass);
                    // Üstünde sıradan eşyalar: gaz lambası ve kitap
                    Cyl("Lamba", root, new Vector3(0.3f, 1.05f, 0.25f), new Vector3(0.1f, 0.08f, 0.1f), brass, collider: false);
                    Box("Kitap", root, new Vector3(-0.25f, 0.97f, 0.3f), new Vector3(0.22f, 0.05f, 0.3f), M.Get("book_ox", new Color(0.35f, 0.08f, 0.06f)), collider: false);
                    spot.Lid = drawer; spot.LidOpenPos = drawer.localPosition + new Vector3(0, 0, 0.4f); spot.LidOpenRot = Quaternion.identity;
                    propParent = drawer; propPos = new Vector3(0, -0.1f, 0.02f); propScale = 0.6f;
                    break;
                }
                case HideKind.Chest:
                {
                    Box("SandıkGövde", root, new Vector3(0, 0.27f, 0.36f), new Vector3(1.0f, 0.54f, 0.6f), M.Wood(new Color(0.38f, 0.24f, 0.13f)));
                    Box("İçBoşluk", root, new Vector3(0, 0.53f, 0.36f), new Vector3(0.9f, 0.02f, 0.5f), dark, collider: false);
                    foreach (var x in new[] { -0.35f, 0.35f })
                        Box("DemirKuşak", root, new Vector3(x, 0.27f, 0.36f), new Vector3(0.05f, 0.56f, 0.62f), M.Get("castiron", new Color(0.06f, 0.07f, 0.06f), 0.45f, 0.75f), collider: false);
                    var pivot = Group("KapakMenteşe", root, new Vector3(0, 0.55f, 0.06f)).transform;
                    Box("Kapak", pivot, new Vector3(0, 0.04f, 0.3f), new Vector3(1.02f, 0.08f, 0.62f), M.Wood(new Color(0.34f, 0.21f, 0.11f)), collider: false);
                    Box("Kilit", pivot, new Vector3(0, -0.02f, 0.61f), new Vector3(0.1f, 0.12f, 0.03f), brass, collider: false);
                    spot.Lid = pivot; spot.LidOpenPos = pivot.localPosition; spot.LidOpenRot = Quaternion.Euler(-105f, 0, 0);
                    propParent = root; propPos = new Vector3(0, 0.38f, 0.36f); propScale = 0.7f;
                    break;
                }
                case HideKind.Painting:
                {
                    // Menteşeli tablo: arkasında duvara oyulmuş gizli bölme
                    Box("Bölme", root, new Vector3(0, 1.62f, 0.02f), new Vector3(0.62f, 0.46f, 0.04f), dark, collider: false);
                    Box("BölmeRafı", root, new Vector3(0, 1.4f, 0.12f), new Vector3(0.62f, 0.03f, 0.22f), M.Wood(new Color(0.2f, 0.13f, 0.08f)), collider: false);
                    var pivot = Group("TabloMenteşe", root, new Vector3(-0.42f, 1.62f, 0.07f)).transform;
                    Box("Çerçeve", pivot, new Vector3(0.42f, 0, 0), new Vector3(0.86f, 0.66f, 0.05f), M.Get("gilt", new Color(0.72f, 0.55f, 0.22f), 0.75f, 0.85f), collider: false);
                    Box("TuvalGök", pivot, new Vector3(0.42f, 0.1f, 0.03f), new Vector3(0.74f, 0.3f, 0.01f), M.Get("paint_dusk", new Color(0.32f, 0.3f, 0.36f)), collider: false);
                    Box("TuvalTepe", pivot, new Vector3(0.42f, -0.14f, 0.03f), new Vector3(0.74f, 0.24f, 0.01f), M.Get("paint_hill", new Color(0.18f, 0.24f, 0.14f)), collider: false);
                    Box("TuvalEv", pivot, new Vector3(0.55f, -0.08f, 0.036f), new Vector3(0.12f, 0.12f, 0.01f), M.Get("paint_house", new Color(0.45f, 0.36f, 0.25f)), collider: false);
                    // Tablonun altında konsol (sıradan görünüm)
                    Box("Konsol", root, new Vector3(0, 0.42f, 0.22f), new Vector3(0.9f, 0.84f, 0.4f), wood);
                    spot.Lid = pivot; spot.LidOpenPos = pivot.localPosition; spot.LidOpenRot = Quaternion.Euler(0, -80f, 0);
                    propParent = root; propPos = new Vector3(0, 1.42f, 0.1f); propScale = 0.55f;
                    break;
                }
                case HideKind.Floorboard:
                {
                    Box("Boşluk", root, new Vector3(0, 0.004f, 0), new Vector3(0.9f, 0.008f, 0.26f), dark, collider: false);
                    var pivot = Group("TahtaUcu", root, new Vector3(0, 0.012f, -0.13f)).transform;
                    Box("GevşekTahta", pivot, new Vector3(0, 0.012f, 0.13f), new Vector3(0.92f, 0.026f, 0.26f), M.Wood(new Color(0.42f, 0.28f, 0.16f)), collider: false);
                    foreach (var x in new[] { -0.4f, 0.4f })
                        Cyl("Çivi", pivot, new Vector3(x, 0.027f, 0.13f), new Vector3(0.025f, 0.004f, 0.025f), M.Metal, collider: false);
                    spot.Lid = pivot; spot.LidOpenPos = pivot.localPosition; spot.LidOpenRot = Quaternion.Euler(-72f, 0, 0);
                    propParent = root; propPos = new Vector3(0, 0.01f, 0); propScale = 0.65f;
                    break;
                }
                default: // Rug
                {
                    var rug = Group("Kilim", root, Vector3.zero).transform;
                    Box("KilimZemin", rug, new Vector3(0, 0.006f, 0), new Vector3(1.7f, 0.012f, 1.1f), M.Get("kilim_red", new Color(0.42f, 0.09f, 0.07f), 0.05f), collider: false);
                    Box("KilimÇerçeve", rug, new Vector3(0, 0.005f, 0), new Vector3(1.8f, 0.01f, 1.2f), M.Get("kilim_border", new Color(0.12f, 0.1f, 0.18f), 0.05f), collider: false);
                    for (int i = -2; i <= 2; i++)
                        Box("Motif", rug, new Vector3(i * 0.3f, 0.0125f, 0), new Vector3(0.14f, 0.002f, 0.14f), M.Get("kilim_motif", new Color(0.78f, 0.6f, 0.26f), 0.05f), new Vector3(0, 45, 0), false);
                    spot.Lid = rug; spot.LidOpenPos = new Vector3(0.75f, 0.02f, 0.25f); spot.LidOpenRot = Quaternion.Euler(0, 22f, 0);
                    propParent = root; propPos = new Vector3(0, 0.01f, 0); propScale = 0.65f;
                    break;
                }
            }

            var prop = PropFactory.BuildClue(propShape, propParent, propPos);
            prop.name = "Clue_" + clue.Id;
            prop.transform.localScale = Vector3.one * propScale;
            spot.ClueObject = prop;
            return spot;
        }

        /// <summary>Gizemli Kasaba: web'deki gizli deliller tasarlanmış yerlerinde (askı arkası, raf arkası...) kalır.</summary>
        public static HidingSpotDef InPlace(Transform room, ClueDef clue, GameObject clueObject)
        {
            var root = Group("SaklamaYeri_" + clue.Id, room, clue.RoomPos).transform;
            // Sezgi izi: tozda sürtünme, yerde çizik — yalnızca Dedektif Gözü ile belirgin parlar
            return new HidingSpotDef { ClueId = clue.Id, Kind = HideKind.InPlace, Label = Labels[HideKind.InPlace], Root = root.gameObject, ClueObject = clueObject };
        }
    }
}
