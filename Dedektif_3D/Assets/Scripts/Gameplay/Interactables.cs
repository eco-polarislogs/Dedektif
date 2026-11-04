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
        public abstract void Interact();

        protected virtual void OnEnable() => All.Add(this);
        protected virtual void OnDisable() => All.Remove(this);
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
    }

    /// <summary>
    /// Olay yerindeki ceset: şerit yalnızca kasabalılara kapalı, dedektif içeri girip örtüyü kaldırabilir.
    /// Kamera cesedin başına çömelir, dedektifin iç sesi gördüklerini anlatır.
    /// </summary>
    public class CorpseInteractable : Interactable
    {
        public override string Prompt => $"Cesedi incele — {GameData.Current.Victim}";
        public override void Interact() => GameManager.Instance.InspectCorpse(transform.position);
    }

    /// <summary>Olay yeri delili. Gizli deliller, saklandıkları yer (çekmece, sandık, tablo, döşeme, halı) açılınca ortaya çıkar.</summary>
    public class ClueInteractable : Interactable
    {
        public int clueId;
        public bool revealed;
        GameObject _marker;
        Light _glow;
        float _revealTime = -10f;
        bool _hidden;

        public override string Prompt
        {
            get
            {
                var c = GameManager.Instance.GetClue(clueId);
                if (c == null) return "İncele";
                return c.InBag ? $"{c.Def.Name} (Çantada Mevcut)" : $"İncele: {c.Def.Name}";
            }
        }

        public override bool CanInteract => revealed;

        public override void Interact() => GameManager.Instance.InspectClueInBuilding(clueId);

        public void Setup(int id, bool hidden, bool showMarker = true)
        {
            clueId = id;
            _hidden = hidden;
            radius = 2.4f;
            if (showMarker) _marker = Shapes.Cyl("DelilHalkası", transform, Vector3.zero,
                new Vector3(0.55f, 0.004f, 0.55f), MaterialLibrary.Emissive("clue_ring", new Color(1f, 0.74f, 0.36f), 0.9f), null, false);
            _glow = Shapes.PointLight("DelilIşığı", transform, new Vector3(0, 0.4f, 0), new Color(1f, 0.76f, 0.42f), 0.6f, 1.6f);
            if (_marker != null) InteriorGenerator.SetLayerRecursive(_marker, InteriorGenerator.InteriorLayer);
            SetRevealed(!hidden, false);
        }

        public void ResetClue()
        {
            gameObject.SetActive(true);
            SetRevealed(!_hidden, false);
        }

        public void SetRevealed(bool value, bool animate)
        {
            revealed = value;
            foreach (var r in GetComponentsInChildren<Renderer>(true))
                if (_marker == null || r.gameObject != _marker) r.enabled = value;
            foreach (var c in GetComponentsInChildren<Collider>(true)) c.enabled = value;
            if (_marker != null) _marker.SetActive(value);
            if (_glow != null) _glow.enabled = value;
            if (animate) _revealTime = Time.time;
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            // Gizli delil kendiliğinden görünmez: saklandığı yer (HidingSpotInteractable) açılmalıdır.
            var c = gm.GetClue(clueId);
            bool taken = c != null && c.InBag;
            if (_marker != null) _marker.SetActive(revealed && !taken);
            if (_glow != null)
            {
                _glow.enabled = revealed && !taken;
                float t = Time.time - _revealTime;
                float burst = t < 1.5f ? (1.5f - t) * 4f : 0f;
                _glow.intensity = 0.5f + Mathf.Sin(Time.time * 3f) * 0.2f + burst;
                _glow.range = 1.6f + burst * 0.5f;
            }
            if (_marker != null && revealed && !taken)
                _marker.transform.Rotate(Vector3.up, 30f * Time.deltaTime, Space.World);
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

    /// <summary>Yardımcı Dedektif Çetin (web: helper-detective-widget).</summary>
    public class HelperInteractable : Interactable
    {
        [System.NonSerialized] public PropFactory.PersonRig rig;
        public override string Prompt => "Yardımcı Dedektif Çetin'den İpucu Al";
        public override void Interact() => GameManager.Instance.OpenHelperPanel();

        void Update()
        {
            if (rig == null) return;
            rig.ArmR.localRotation = Quaternion.Euler(Mathf.Sin(Time.time * 1.2f) * 6f - 10f, 0, 8f);
            var gm = GameManager.Instance;
            var player = gm != null ? gm.Player : null;
            if (player == null) return;
            var to = player.transform.position - rig.Root.position; to.y = 0;
            if (rig.Human != null) rig.Human.LookTarget = to.sqrMagnitude < 100f && Camera.main != null ? Camera.main.transform : null;
            if (to.sqrMagnitude > 0.01f && to.sqrMagnitude < 144f)
                rig.Root.rotation = Quaternion.Slerp(rig.Root.rotation, Quaternion.LookRotation(to), Time.deltaTime * 2f);
        }
    }
}
