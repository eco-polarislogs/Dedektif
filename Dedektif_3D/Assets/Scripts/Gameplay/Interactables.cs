using System.Collections.Generic;
using Dedektif3D.Core;
using Dedektif3D.World;
using UnityEngine;

namespace Dedektif3D.Gameplay
{
    /// <summary>
    /// Oyuncunun [E] ile etkileşime girebildiği her şeyin temeli. Oyuncu her karede en yakın uygun
    /// Interactable'ı mesafe + bakış açısıyla seçer (web'deki tıklanabilir hotspot / bina / buton karşılığı).
    /// </summary>
    public abstract class Interactable : MonoBehaviour
    {
        public static readonly List<Interactable> All = new List<Interactable>();

        public float radius = 2.3f;
        public Vector3 offset;

        public Vector3 Point => transform.position + offset;
        public abstract string Prompt { get; }
        public virtual bool CanInteract => true;
        /// <summary>Sıfırdan büyükse [E] odağında başka her şeyin arkasına düşer (yanımızda yürüyen yol arkadaşları).</summary>
        public virtual float FocusPenalty => 0f;
        public abstract void Interact();

        protected virtual void OnEnable() => All.Add(this);
        protected virtual void OnDisable() => All.Remove(this);

        // ---------- Fareyle arama (SceneSearch) ----------
        protected static readonly Color GoldGlow = new Color(2.7f, 1.85f, 0.75f, 1f);
        protected static readonly Color PersonGlow = new Color(0.95f, 1.15f, 1.5f, 1f);

        /// <summary>Fare bu nesnenin üstüne geldiğinde seçilebilir mi (uzaktan da).</summary>
        public virtual bool Searchable => CanInteract;
        /// <summary>Fareyle seçilebileceği en uzak mesafe.</summary>
        public virtual float SearchRange => 13f;
        /// <summary>Nesnenin dünyadaki kutusu: imleç bu kutunun ekrandaki izdüşümünde arandığı için küçük eşyalar da kolay yakalanır.</summary>
        public virtual Bounds HoverBounds => new Bounds(Point, Vector3.one * Mathf.Max(0.6f, radius * 0.5f));
        /// <summary>İmlecin yanındaki etiket.</summary>
        public virtual string HoverTitle => Prompt;
        /// <summary>Etiketin alt satırı (tıklayınca ne olacağı).</summary>
        public virtual string HoverAction => "Sol tık";
        public virtual Color GlowColor => GoldGlow;
        /// <summary>Parlatılacak görseller; boş bırakan nesnelerde (kapı, ceset) ışık ve etiket gösterilir.</summary>
        public virtual void CollectGlowRenderers(List<Renderer> into) { }
        public virtual void OnHoverChanged(bool hovered) { }

        /// <summary>Yazı ve parçacık dışındaki görselleri toplar.</summary>
        protected static void AddMeshRenderers(Transform root, List<Renderer> into, System.Func<Renderer, bool> keep = null)
        {
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                if (!(r is MeshRenderer) && !(r is SkinnedMeshRenderer)) continue;
                if (r.GetComponent<TextMesh>() != null) continue;
                if (keep != null && !keep(r)) continue;
                into.Add(r);
            }
        }

        /// <summary>Görsellerin birleşik kutusu; görsel yoksa fallback. Kapalı görsellerde de geçerlidir.</summary>
        protected static Bounds RendererBounds(List<Renderer> renderers, Bounds fallback, float minExtent = 0.12f)
        {
            bool any = false;
            var b = fallback;
            foreach (var r in renderers)
            {
                if (r == null) continue;
                if (!any) { b = r.bounds; any = true; }
                else b.Encapsulate(r.bounds);
            }
            var e = b.extents;
            b.extents = new Vector3(Mathf.Max(e.x, minExtent), Mathf.Max(e.y, minExtent), Mathf.Max(e.z, minExtent));
            return b;
        }
    }

    /// <summary>
    /// Bina kapısı. Dışarıdan: kapı giriş sahnesi (web: building-entry-modal). İçeriden: çıkış uyarısı
    /// (web: exit-warning-modal). Çıkıldıktan sonra bina kilitlenir: "İNCELEME TAMAMLANDI".
    /// </summary>
    public class DoorInteractable : Interactable
    {
        [System.NonSerialized] public BuildingDef building;
        public bool isExit;

        public override string Prompt
        {
            get
            {
                if (isExit) return "Binadan çık (Kasabaya Dön)";
                var gm = GameManager.Instance;
                if (gm != null && gm.IsVisited(building.NpcId)) return $"{building.DisplayName} — İNCELEME TAMAMLANDI (kilitli)";
                return $"{building.DisplayName} binasına gir";
            }
        }

        public override void Interact()
        {
            var gm = GameManager.Instance;
            if (isExit) gm.RequestExitBuilding();
            else gm.OpenDoorEntry(building);
        }

        public override float SearchRange => isExit ? 13f : 18f;
        public override Bounds HoverBounds => new Bounds(transform.position + Vector3.up * (isExit ? 0.3f : 1.3f), new Vector3(1.8f, 2.8f, 1.8f));
        public override string HoverAction => "Sol tık: " + (isExit ? "çık" : "kapıya git");
    }

    /// <summary>
    /// Olay yerindeki ceset: şerit yalnızca kasabalılara kapalı, dedektif içeri girip örtüyü kaldırabilir.
    /// Kamera cesedin başına çömelir, dedektifin iç sesi gördüklerini anlatır.
    /// </summary>
    public class CorpseInteractable : Interactable
    {
        public override string Prompt => $"Cesedi incele — {GameData.Current.Victim}";
        public override void Interact() => GameManager.Instance.InspectCorpse(transform.position);

        public override float SearchRange => 16f;
        public override Bounds HoverBounds => new Bounds(transform.position + Vector3.up * 0.4f, new Vector3(2.2f, 1f, 2.2f));
        public override string HoverAction => "Sol tık: örtüyü kaldır";
    }

    /// <summary>
    /// Olay yeri delili. Odada sıradan bir eşya gibi durur: halka ya da ışıkla ele vermez; üstüne fareyle gelince
    /// (ya da dibindeyken) parlar. Gizli deliller, saklandıkları yer (çekmece, sandık, tablo, döşeme, halı) açılınca ortaya çıkar.
    /// Çantaya alınan delil odadan kalkar.
    /// </summary>
    public class ClueInteractable : Interactable
    {
        public int clueId;
        public bool revealed;
        GameObject _marker;
        Light _glow;
        float _revealTime = -10f;
        bool _hidden, _hovered, _shownTaken;
        readonly List<Renderer> _props = new List<Renderer>();

        ClueRuntime Runtime => GameManager.Instance != null ? GameManager.Instance.GetClue(clueId) : null;
        bool Taken { get { var c = Runtime; return c != null && c.InBag; } }

        public override string Prompt
        {
            get
            {
                var c = Runtime;
                if (c == null) return "İncele";
                return c.InBag ? $"{c.Def.Name} (Çantada Mevcut)" : $"İncele: {c.Def.Name}";
            }
        }

        public override bool CanInteract => revealed && !Taken;

        public override void Interact() => GameManager.Instance.InspectClueInBuilding(clueId);

        public override string HoverTitle { get { var c = Runtime; return c != null ? c.Def.Name : "Delil"; } }
        public override string HoverAction => "Sol tık: incele";
        public override Bounds HoverBounds => RendererBounds(_props, new Bounds(transform.position + Vector3.up * 0.2f, Vector3.one * 0.4f), 0.14f);
        public override void CollectGlowRenderers(List<Renderer> into) => into.AddRange(_props);
        public override void OnHoverChanged(bool hovered) => _hovered = hovered;

        public void Setup(int id, bool hidden, bool showMarker = true)
        {
            clueId = id;
            _hidden = hidden;
            radius = 2.4f;
            // Görünen eşyalar: halka ve kırsal delil sehpası ("Sehpa", "Örtü") parlatılmaz; delilin kendisi parlar.
            AddMeshRenderers(transform, _props, r => r.name != "Sehpa" && r.name != "Örtü");
            if (showMarker) _marker = Shapes.Cyl("DelilHalkası", transform, Vector3.zero,
                new Vector3(0.55f, 0.004f, 0.55f), MaterialLibrary.Emissive("clue_ring", new Color(1f, 0.74f, 0.36f), 0.9f), null, false);
            _glow = Shapes.PointLight("DelilIşığı", transform, new Vector3(0, 0.4f, 0), new Color(1f, 0.76f, 0.42f), 0f, 1.6f);
            if (_marker != null) InteriorGenerator.SetLayerRecursive(_marker, InteriorGenerator.InteriorLayer);
            SetRevealed(!hidden, false);
        }

        public void ResetClue()
        {
            gameObject.SetActive(true);
            _shownTaken = false;
            SetRevealed(!_hidden, false);
        }

        public void SetRevealed(bool value, bool animate)
        {
            revealed = value;
            foreach (var r in GetComponentsInChildren<Renderer>(true))
                if (_marker == null || r.gameObject != _marker) r.enabled = value;
            foreach (var c in GetComponentsInChildren<Collider>(true)) c.enabled = value;
            if (_marker != null) _marker.SetActive(false);
            if (_glow != null) _glow.enabled = false;
            if (animate) _revealTime = Time.time;
            _shownTaken = false;
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            bool taken = Taken;
            // Alınan delil odadan kalkar; kırsal kasabalarda boş sehpası kalır. Geri bırakılırsa yine görünür.
            if (taken != _shownTaken)
            {
                _shownTaken = taken;
                foreach (var r in _props) if (r != null) r.enabled = revealed && !taken;
            }
            // Halka ve ışık yalnızca üstüne gelince, Dedektif Gözü açıkken ya da saklandığı yerden yeni çıktığında
            var vision = DetectiveVision.Instance;
            float v = vision != null ? vision.Strength : 0f;
            float t = Time.time - _revealTime;
            float burst = t < 1.5f ? (1.5f - t) * 2.4f : 0f;
            bool lit = revealed && !taken && (_hovered || v > 0.01f || burst > 0f);
            if (_marker != null)
            {
                _marker.SetActive(revealed && !taken && (_hovered || v > 0.25f));
                if (_marker.activeSelf) _marker.transform.Rotate(Vector3.up, 30f * Time.deltaTime, Space.World);
            }
            if (_glow != null)
            {
                _glow.enabled = lit;
                if (lit)
                {
                    float pulse = 0.5f + Mathf.Sin(Time.time * 3f) * 0.2f;
                    _glow.intensity = (_hovered ? pulse + 0.5f : v * pulse * 1.4f) + burst;
                    _glow.range = 1.6f + burst * 0.5f;
                }
            }
        }
    }

    /// <summary>Şüpheli NPC: sorgu ekranını açar (web: talk-npc-btn), oyuncuya doğru döner, strese göre titrer.</summary>
    public class NpcInteractable : Interactable
    {
        public int npcId;
        [System.NonSerialized] public PropFactory.PersonRig rig;
        Vector3 _baseScale = Vector3.one;
        int _answered = -1;

        public override string Prompt
        {
            get
            {
                var gm = GameManager.Instance;
                var n = gm.GetNpc(npcId);
                if (n == null) return "Konuş";
                if (gm.IsInnocent(npcId)) return $"{n.Def.Name} (Masum İlan Edildi)";
                return $"{n.Def.Name} ile Konuş";
            }
        }

        public override void Interact() => GameManager.Instance.OpenInterrogation(npcId);

        public override Color GlowColor => PersonGlow;
        public override string HoverTitle { get { var n = GameManager.Instance.GetNpc(npcId); return n != null ? n.Def.Name : "Şüpheli"; } }
        public override string HoverAction => GameManager.Instance.IsInnocent(npcId) ? "Masum ilan edildi" : "Sol tık: sorgula";
        public override Bounds HoverBounds => new Bounds(transform.position + Vector3.up * 0.95f, new Vector3(0.8f, 1.9f, 0.8f));
        public override void CollectGlowRenderers(List<Renderer> into) { if (rig != null) AddMeshRenderers(rig.Root, into); }

        void Start() { if (rig != null) _baseScale = rig.Body.localScale; }

        void Update()
        {
            if (rig == null) return;
            float breathe = 1f + Mathf.Sin(Time.time * 1.6f + npcId) * 0.012f;
            rig.Body.localScale = new Vector3(_baseScale.x, _baseScale.y * breathe, _baseScale.z);

            var gm = GameManager.Instance;
            var player = gm != null ? gm.Player : null;
            if (player == null) return;
            var toPlayer = player.transform.position - rig.Root.position; toPlayer.y = 0;
            if (toPlayer.sqrMagnitude < 64f && toPlayer.sqrMagnitude > 0.01f)
                rig.Root.rotation = Quaternion.Slerp(rig.Root.rotation, Quaternion.LookRotation(toPlayer), Time.deltaTime * 2.5f);
            var npc = gm.GetNpc(npcId);
            if (rig.Human != null)
            {
                // İnsan simülasyonu: dedektife bakar, cevap verirken el-kol hareketiyle konuşur
                rig.Human.LookTarget = toPlayer.sqrMagnitude < 81f && Camera.main != null ? Camera.main.transform : null;
                var last = npc != null && npc.Log.Count > 0 ? npc.Log[npc.Log.Count - 1] : null;
                rig.Human.Talking = gm.UI.IsTalkingWith(npcId) && (npc.Waiting || (last != null && !last.FromPlayer && !last.Typed));
                // Yeni cevap bitince beden dili: gergin şüpheli başını iki yana sallar, sakin olan arada onaylar
                int count = npc != null ? npc.Log.Count : 0;
                if (last != null && !last.FromPlayer && last.Typed && count != _answered)
                {
                    _answered = count;
                    if (npc.Stress > 55) rig.Human.Gesture("no");
                    else if (Random.value < 0.35f) rig.Human.Gesture("yes");
                }
                return;
            }
            if (npc != null && npc.Stress > 60)
                rig.Head.localRotation = Quaternion.Euler(Mathf.Sin(Time.time * 25f) * (npc.Stress - 60) * 0.05f, 0, 0);
        }
    }

    /// <summary>Yol arkadaşıyla konuşma: Çetin (web: helper-detective-widget), yerel rehberler (Bekçi Rıfat, Tüccar İlyas).</summary>
    public class HelperInteractable : Interactable
    {
        [System.NonSerialized] public PropFactory.PersonRig rig;
        public string displayName = "Yardımcı Dedektif Çetin";
        public bool localGuide;

        public override string Prompt => localGuide ? $"{displayName} ile Konuş" : $"{displayName}'den İpucu Al";
        public override float FocusPenalty => 3f;
        public override void Interact()
        {
            if (localGuide) GameManager.Instance.TalkToLocalGuide(displayName);
            else GameManager.Instance.OpenHelperPanel();
        }

        public override Color GlowColor => PersonGlow;
        public override string HoverTitle => displayName;
        public override string HoverAction => localGuide ? "Sol tık: konuş" : "Sol tık: ipucu al";
        public override Bounds HoverBounds => new Bounds(transform.position + Vector3.up * 0.95f, new Vector3(0.8f, 1.9f, 0.8f));
        public override void CollectGlowRenderers(List<Renderer> into) { if (rig != null) AddMeshRenderers(rig.Root, into); }
    }
}
