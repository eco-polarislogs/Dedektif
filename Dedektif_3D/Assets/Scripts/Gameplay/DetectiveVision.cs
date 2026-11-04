using System.Collections;
using System.Collections.Generic;
using Dedektif3D.Core;
using Dedektif3D.World;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Dedektif3D.Gameplay
{
    /// <summary>
    /// DEDEKTİF GÖZÜ [Q] — dedektifin sezgisi (Sherlock Holmes "Konsantrasyon", L.A. Noire ipucu sezgisi).
    /// Birkaç saniyeliğine dünya sepya-griye döner, kalp atışı duyulur ve yakındaki saklama yerleri altın bir pırıltıyla belli olur.
    /// Saklanmış deliller ancak bulundukları çekmece / sandık / tablo / döşeme / halı açılınca ortaya çıkar.
    /// </summary>
    public class DetectiveVision : MonoBehaviour
    {
        public static DetectiveVision Instance { get; private set; }
        public static ColorAdjustments Color;   // GameBootstrap post-process profilinden
        public static Vignette Vignette;

        public const float Duration = 6f;
        public const float Cooldown = 4f;
        public const float SenseRadius = 7.5f;

        public bool Active => Time.time < _until;
        public float Strength { get; private set; }   // 0..1 geçişli
        public float CooldownLeft => Mathf.Max(0f, _readyAt - Time.time);

        float _until = -10f, _readyAt, _nextBeat;
        float _baseSat, _baseVig; UnityEngine.Color _baseFilter; bool _baseCaptured;

        void Awake() => Instance = this;

        public void Activate()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Phase != GamePhase.Playing) return;
            if (Time.time < _readyAt) { gm.UI.Toast($"Zihnin hâlâ yorgun... ({CooldownLeft:0} sn)"); return; }
            _until = Time.time + Duration;
            _readyAt = _until + Cooldown;
            _nextBeat = Time.time;

            int newly = 0;
            var pos = gm.Player.transform.position;
            foreach (var spot in HidingSpotInteractable.All)
            {
                if (spot == null || spot.Opened || !spot.isActiveAndEnabled) continue;
                var d = spot.transform.position - pos; d.y = 0;
                if (d.magnitude <= SenseRadius && !spot.Sensed) { spot.Sense(); newly++; }
            }
            if (newly > 0)
            {
                AudioManager.Instance?.Shimmer();
                gm.UI.Toast(newly == 1 ? "Sezgilerin bir şeyin saklandığını fısıldıyor... Altın pırıltıya yaklaş." : $"Bu odada {newly} yer dikkatini çekiyor... Pırıltılara yaklaş ve ara.");
                gm.Api.LogAction("detective_vision", newly, gm.CurrentBuilding != null ? gm.CurrentBuilding.WebName : GameData.TownName);
            }
            else if (gm.InsideBuilding)
            {
                bool anyLeft = false, sensedWaiting = false;
                foreach (var s in HidingSpotInteractable.All)
                {
                    if (s == null || s.Opened || !s.isActiveAndEnabled) continue;
                    var d = s.transform.position - pos; d.y = 0;
                    if (d.magnitude > 30f) continue;   // başka binanın odası
                    anyLeft = true;
                    if (s.Sensed) sensedWaiting = true;
                }
                gm.UI.Toast(sensedWaiting ? "Pırıltılı yeri henüz aramadın... Yaklaş ve [E] ile ara." :
                            anyLeft ? "Buralarda değil... Odanın başka bir köşesinde dene." : "Bu odada saklanmış başka bir şey sezmiyorsun.");
            }
            else gm.UI.Toast("Islak parke taşlarında yalnızca yağmur var. Saklanan izler binaların içinde.");
        }

        void Update()
        {
            float target = Active ? 1f : 0f;
            Strength = Mathf.MoveTowards(Strength, target, Time.deltaTime * (Active ? 2.5f : 1.5f));
            if (Active && Time.time >= _nextBeat) { _nextBeat = Time.time + 1.05f; AudioManager.Instance?.Heartbeat(); }
            ApplyPost();
        }

        /// <summary>Kasaba teması değişince temel renk değerlerini yeniden al.</summary>
        public void RebaseGrade() => _baseCaptured = false;

        void ApplyPost()
        {
            if (Color == null) return;
            if (!_baseCaptured)
            {
                _baseCaptured = true;
                _baseSat = Color.saturation.value; _baseFilter = Color.colorFilter.value;
                if (Vignette != null) _baseVig = Vignette.intensity.value;
            }
            float k = Strength;
            Color.saturation.Override(Mathf.Lerp(_baseSat, -88f, k));
            Color.colorFilter.Override(UnityEngine.Color.Lerp(_baseFilter, new UnityEngine.Color(1f, 0.86f, 0.62f), k));
            if (Vignette != null) Vignette.intensity.Override(Mathf.Lerp(_baseVig, 0.6f, k) + (Active ? Mathf.Sin(Time.time * 6f) * 0.02f : 0f));
        }

        void OnDestroy() { if (Instance == this) Instance = null; }
    }

    /// <summary>
    /// Saklama yeri: sıradan bir mobilya gibi durur. Dedektif Gözü ile sezilince altın pırıltı belirir;
    /// ya da oyuncu dibine kadar gelirse "kurcalanabilir". Açılınca içindeki gizli delil ortaya çıkar.
    /// </summary>
    public class HidingSpotInteractable : Interactable
    {
        public static new readonly List<HidingSpotInteractable> All = new List<HidingSpotInteractable>();

        [System.NonSerialized] public HidingSpotDef def;
        public bool Sensed { get; private set; }
        public bool Opened { get; private set; }

        Vector3 _lidClosedPos; Quaternion _lidClosedRot;
        Light _glint; GameObject _spark;
        float _lingerTime;

        public override string Prompt => Sensed ? $"Ara: {def.Label}" : $"Kurcala: {def.Label}";
        public override bool CanInteract => !Opened && (Sensed || PlayerClose(1.6f));

        public void Setup(HidingSpotDef d)
        {
            def = d;
            radius = 2.0f;
            offset = def.Kind == HideKind.Painting ? Vector3.up * 1.4f : def.Kind == HideKind.Floorboard || def.Kind == HideKind.Rug ? Vector3.up * 0.3f : Vector3.up * 0.7f;
            if (def.Lid != null) { _lidClosedPos = def.Lid.localPosition; _lidClosedRot = def.Lid.localRotation; }
            _glint = Shapes.PointLight("SezgiPırıltısı", transform, offset + Vector3.forward * 0.3f, new UnityEngine.Color(1f, 0.78f, 0.38f), 0f, 2.2f);
            _spark = Shapes.Sphere("Pırıltı", transform, offset + Vector3.forward * 0.35f, Vector3.one * 0.06f,
                MaterialLibrary.Emissive("intuition_spark", new UnityEngine.Color(1f, 0.8f, 0.42f), 6f));
            var col = _spark.GetComponent<Collider>(); if (col != null) Destroy(col);
            InteriorGenerator.SetLayerRecursive(_spark, InteriorGenerator.InteriorLayer);
            _spark.SetActive(false);
        }

        protected override void OnEnable() { base.OnEnable(); All.Add(this); }
        protected override void OnDisable() { base.OnDisable(); All.Remove(this); }

        bool PlayerClose(float r)
        {
            var p = GameManager.Instance != null ? GameManager.Instance.Player : null;
            if (p == null) return false;
            var d = p.transform.position - transform.position; d.y = 0;
            return d.magnitude < r;
        }

        public void Sense()
        {
            if (Sensed || Opened) return;
            Sensed = true;
            if (_spark != null) _spark.SetActive(true);
        }

        public void ResetSpot()
        {
            StopAllCoroutines();
            Sensed = false; Opened = false; _lingerTime = 0f;
            if (def?.Lid != null) { def.Lid.localPosition = _lidClosedPos; def.Lid.localRotation = _lidClosedRot; }
            if (_spark != null) _spark.SetActive(false);
            if (_glint != null) _glint.intensity = 0f;
        }

        public override void Interact()
        {
            if (Opened) return;
            Opened = true;
            if (_spark != null) _spark.SetActive(false);
            AudioManager.Instance?.DoorOpen();
            StartCoroutine(OpenRoutine());
        }

        IEnumerator OpenRoutine()
        {
            var gm = GameManager.Instance;
            // Delil, kapak açılırken görünür hâle gelir
            if (def.Kind != HideKind.InPlace) gm.RevealHiddenClue(def.ClueId, def.Label);
            if (def.Lid != null)
            {
                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime / 0.8f;
                    float k = Mathf.SmoothStep(0, 1, t);
                    def.Lid.localPosition = Vector3.Lerp(_lidClosedPos, def.LidOpenPos, k);
                    def.Lid.localRotation = Quaternion.Slerp(_lidClosedRot, def.LidOpenRot, k);
                    yield return null;
                }
            }
            if (def.Kind == HideKind.InPlace) gm.RevealHiddenClue(def.ClueId, def.Label);
        }

        void Update()
        {
            if (Opened) { if (_glint != null) _glint.intensity = 0f; return; }
            // Uzun süre dibinde durmak da sezgiyi tetikler (L.A. Noire'daki ince ipucu sezgisi)
            if (!Sensed && PlayerClose(1.3f))
            {
                _lingerTime += Time.deltaTime;
                if (_lingerTime > 1.6f)
                {
                    Sense();
                    AudioManager.Instance?.Shimmer();
                    GameManager.Instance?.UI.Toast($"{def.Label}... Bir şey tuhaf görünüyor.");
                }
            }
            else _lingerTime = 0f;

            var vision = DetectiveVision.Instance;
            float v = vision != null ? vision.Strength : 0f;
            if (_glint != null) _glint.intensity = Sensed ? 0.6f + v * 2.2f + Mathf.Sin(Time.time * 4f) * 0.25f : v * 0.4f;
            if (_spark != null && _spark.activeSelf)
            {
                float s = 0.05f + Mathf.Abs(Mathf.Sin(Time.time * 3f)) * 0.03f + v * 0.04f;
                _spark.transform.localScale = Vector3.one * s;
            }
        }
    }
}
