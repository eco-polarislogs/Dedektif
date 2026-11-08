using System.Collections.Generic;
using Dedektif3D.Core;
using Dedektif3D.World;
using UnityEngine;

namespace Dedektif3D.Gameplay
{
    /// <summary>
    /// Dedektifin yol arkadaşı (Çetin; Gölgeşehir'de Bekçi Rıfat, Sisören'de Tüccar İlyas): dedektifin yanında yürür, o koşunca koşar,
    /// durunca döner bakar, binalara ve sokaklara onunla birlikte girer.
    /// Yol bulma: görüş açıksa omuz başındaki yerine doğrudan gider; bir şey engelliyorsa dedektifin geçtiği izleri (ekmek kırıntısı) takip eder.
    /// İz de tıkanırsa kameranın görmediği bir anda yanına geçer. Dedektif ışınlanınca (bina kapısı) karartma altında onunla gelir.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class HelperCompanion : MonoBehaviour
    {
        public static readonly List<HelperCompanion> All = new List<HelperCompanion>();

        public PropFactory.PersonRig rig;
        /// <summary>Dedektife göre yeri: x sağ (+) / sol (-), y ileri (+) / geri (-).</summary>
        public Vector2 slot = new Vector2(-1.3f, 0.25f);

        /// <summary>Kamera bu mesafeden yakına gelince figür gizlenir (dar sokakta, kamera döndürülünce, arama modunda görüşü kapatmasın).</summary>
        const float CameraHideDist = 1.2f, CameraShowDist = 1.5f;
        Renderer[] _renderers;
        bool _hiddenNearCamera;

        const float CrumbStep = 0.7f, Radius = 0.3f, Height = 1.75f;
        const int MaxCrumbs = 90;
        const float WalkMin = 1.5f, RunMax = 6.9f, Arrive = 0.4f;
        static int ObstacleMask => ~((1 << 2) | (1 << TownGenerator.NpcBarrierLayer));

        CharacterController _cc;
        readonly List<Vector3> _trail = new List<Vector3>();
        Vector3 _heading = Vector3.forward, _vel, _goal, _lastProgressPos;
        float _vy, _replan, _stuck, _progressT, _idleT, _nextIdleAction, _lookT, _stepT, _ignoreT;
        bool _hasGoal;

        void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _cc.height = Height; _cc.radius = Radius; _cc.center = new Vector3(0, Height * 0.5f + 0.02f, 0);
            _cc.stepOffset = 0.4f; _cc.slopeLimit = 50f; _cc.skinWidth = 0.04f;
            _cc.detectCollisions = false;   // dedektifi ve kasabalıları itmesin
        }

        void OnEnable()
        {
            All.Add(this);
            PlayerController.Teleported += WarpToPlayer;
        }

        void OnDisable()
        {
            All.Remove(this);
            PlayerController.Teleported -= WarpToPlayer;
        }

        void Start()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
            WarpToPlayer();
        }

        /// <summary>Figür kameraya çok yaklaşınca ekranı kaplamasın diye çizilmez; uzaklaşınca geri gelir.</summary>
        void UpdateCameraHide()
        {
            var cam = Camera.main;
            if (cam == null || _renderers == null) return;
            var camPos = cam.transform.position;
            var nearest = transform.position;
            nearest.y = Mathf.Clamp(camPos.y, nearest.y + 0.2f, nearest.y + Height);
            float dist = Vector3.Distance(camPos, nearest);
            bool hide = _hiddenNearCamera ? dist < CameraShowDist : dist < CameraHideDist;
            if (hide == _hiddenNearCamera) return;
            _hiddenNearCamera = hide;
            foreach (var r in _renderers) if (r != null) r.forceRenderingOff = hide;
        }

        void Update()
        {
            var gm = GameManager.Instance;
            var player = gm != null ? gm.Player : null;
            if (player == null) return;

            UpdateCameraHide();

            _ignoreT -= Time.deltaTime;
            if (_ignoreT <= 0f) { _ignoreT = 1f; IgnorePartyCollisions(player); }

            var p = player.transform.position;
            if (_trail.Count == 0 || (p - _trail[_trail.Count - 1]).sqrMagnitude > CrumbStep * CrumbStep)
            {
                _trail.Add(p);
                if (_trail.Count > MaxCrumbs) _trail.RemoveAt(0);
            }

            var cc = player.GetComponent<CharacterController>();
            float playerSpeed = cc != null ? new Vector3(cc.velocity.x, 0, cc.velocity.z).magnitude : 0f;
            if (playerSpeed > 0.3f)
            {
                var f = player.transform.forward; f.y = 0;
                if (f.sqrMagnitude > 0.01f) _heading = Vector3.Slerp(_heading, f.normalized, 1f - Mathf.Exp(-Time.deltaTime * 3f)).normalized;
            }

            if (gm.Phase != GamePhase.Playing && gm.Phase != GamePhase.Descent) { Settle(); return; }

            var pos = transform.position;
            float flatToPlayer = Flat(p - pos).magnitude;
            // Başka bir kattaysa ya da çok gerideyse (bina geçişi, düşme) doğrudan yanına
            if (Mathf.Abs(p.y - pos.y) > 7f || flatToPlayer > 32f) { WarpToPlayer(); return; }

            _replan -= Time.deltaTime;
            if (_replan <= 0f || !_hasGoal)
            {
                _replan = 0.22f;
                _goal = ChooseGoal(p, pos);
                _hasGoal = true;
            }

            var to = Flat(_goal - pos);
            float dist = to.magnitude;
            float want = 0f;
            if (dist > Arrive || playerSpeed > 0.3f)
            {
                want = Mathf.Max(WalkMin, playerSpeed * 0.98f);
                if (dist > 1.1f) want = Mathf.Max(want, Mathf.Lerp(WalkMin, RunMax, Mathf.InverseLerp(1.1f, 6.5f, dist)));
                // Yerine varırken yavaşla
                if (playerSpeed < 0.3f) want = Mathf.Min(want, 0.7f + dist * 2.2f);
            }

            var dir = dist > 0.01f ? to / dist : Vector3.zero;
            dir += Separation(player);
            if (dir.sqrMagnitude > 1f) dir.Normalize();
            _vel = Vector3.MoveTowards(_vel, dir * want, 15f * Time.deltaTime);

            if (_cc.isGrounded) _vy = -2f; else _vy -= 20f * Time.deltaTime;
            if (_cc.enabled) _cc.Move((_vel + Vector3.up * _vy) * Time.deltaTime);

            TrackStuck(want, player);
            Face(player, playerSpeed);
            Footsteps(gm);
        }

        /// <summary>Faz dışında (hikâye, harita) sessizce durur.</summary>
        void Settle()
        {
            _vel = Vector3.zero;
            if (rig != null && rig.Human != null) rig.Human.LookTarget = null;
        }

        // =================================================================
        //  Hedef seçimi
        // =================================================================
        Vector3 ChooseGoal(Vector3 player, Vector3 me)
        {
            // 1) Omuz başındaki yer: dedektiften görülebilen ve durulabilen ilk aday
            var right = Vector3.Cross(Vector3.up, _heading);
            Vector3 best = player;
            bool found = false;
            foreach (var c in SlotCandidates())
            {
                var w = player + right * c.x + _heading * c.y;
                if (!Grounded(w, out var ground)) continue;
                if (!Standable(ground) || !Clear(player, ground)) continue;
                best = ground; found = true;
                break;
            }
            if (found && Clear(me, best)) return best;

            // 2) İz takibi: görüş açık olan en yeni kırıntıya git
            for (int i = _trail.Count - 1; i >= 0; i--)
                if (Clear(me, _trail[i])) return _trail[i];

            // 3) Hiçbiri açık değil: en azından dedektife doğru (takılma sayacı halleder)
            return found ? best : player;
        }

        IEnumerable<Vector2> SlotCandidates()
        {
            yield return slot;
            yield return new Vector2(slot.x * 1.15f, 0.1f);
            yield return new Vector2(slot.x * 0.5f, slot.y * 1.2f);
            yield return new Vector2(-slot.x * 1.15f, 0.1f);
            yield return new Vector2(slot.x * 1.1f, 0.9f);
            yield return new Vector2(-slot.x, slot.y);
            yield return new Vector2(slot.x * 0.4f, -1.3f);   // dar sokak: arkaya geçer, kameraya yaklaşırsa gizlenir
        }

        static bool Grounded(Vector3 at, out Vector3 ground)
        {
            ground = at;
            if (!Physics.Raycast(at + Vector3.up * 2.2f, Vector3.down, out var hit, 5f, ObstacleMask, QueryTriggerInteraction.Ignore)) return false;
            ground = hit.point;
            return true;
        }

        static bool Standable(Vector3 feet)
        {
            var a = feet + Vector3.up * (Radius + 0.42f);
            var b = feet + Vector3.up * (Height - Radius);
            return !Physics.CheckCapsule(a, b, Radius * 0.95f, ObstacleMask, QueryTriggerInteraction.Ignore);
        }

        static bool Clear(Vector3 from, Vector3 to)
        {
            var d = to - from;
            float len = d.magnitude;
            if (len < 0.05f) return true;
            d /= len;
            var a = from + Vector3.up * (Radius + 0.42f);
            var b = from + Vector3.up * (Height - Radius);
            return !Physics.CapsuleCast(a, b, Radius * 0.85f, d, len, ObstacleMask, QueryTriggerInteraction.Ignore);
        }

        Vector3 Separation(PlayerController player)
        {
            Vector3 push = Vector3.zero;
            void Away(Vector3 other, float radius)
            {
                var d = Flat(transform.position - other);
                float m = d.magnitude;
                if (m < radius && m > 0.001f) push += d / m * (1f - m / radius) * 1.4f;
            }
            Away(player.transform.position, 0.95f);
            foreach (var c in All) if (c != this) Away(c.transform.position, 0.9f);
            return push;
        }

        // =================================================================
        //  Takılma ve ışınlanma
        // =================================================================
        void TrackStuck(float want, PlayerController player)
        {
            _progressT += Time.deltaTime;
            if (_progressT < 0.5f) return;
            float moved = Flat(transform.position - _lastProgressPos).magnitude;
            _lastProgressPos = transform.position;
            _stuck = want > 0.8f && moved < want * _progressT * 0.2f ? _stuck + _progressT : 0f;
            _progressT = 0f;
            if (_stuck < 2f) return;
            // Oyuncu görmeden geç; görüyorsa biraz daha bekle
            var cam = Camera.main;
            bool seen = false;
            if (cam != null)
            {
                var vp = cam.WorldToViewportPoint(transform.position + Vector3.up);
                seen = vp.z > 0f && vp.x > 0f && vp.x < 1f && vp.y > 0f && vp.y < 1f;
            }
            if (!seen || _stuck > 5f) WarpToPlayer();
        }

        /// <summary>Dedektifin yanına anında geçer (bina kapısı, kayıt yükleme, takılma).</summary>
        public void WarpToPlayer()
        {
            var gm = GameManager.Instance;
            var player = gm != null ? gm.Player : null;
            if (player == null || _cc == null) return;
            Physics.SyncTransforms();
            var p = player.transform.position;
            var f = player.transform.forward; f.y = 0;
            _heading = f.sqrMagnitude > 0.01f ? f.normalized : Vector3.forward;
            var right = Vector3.Cross(Vector3.up, _heading);

            var spot = p - _heading * 0.8f;
            foreach (var c in SlotCandidates())
            {
                var w = p + right * c.x + _heading * c.y;
                if (Grounded(w, out var ground) && Standable(ground) && Clear(p, ground)) { spot = ground; break; }
            }
            _cc.enabled = false;
            transform.position = spot + Vector3.up * 0.03f;
            transform.rotation = Quaternion.LookRotation(_heading);
            _cc.enabled = true;
            _vel = Vector3.zero; _vy = 0f; _stuck = 0f; _hasGoal = false;
            _lastProgressPos = transform.position;
            _trail.Clear();
            _trail.Add(p);
            _idleT = 0f;
            IgnorePartyCollisions(player);
            // İç mekânda güneş girmesin: görünen beden iç mekân katmanına
            if (rig != null && rig.Body != null) InteriorGenerator.SetLayerRecursive(rig.Body.gameObject, p.y < -100f ? InteriorGenerator.InteriorLayer : 2);
        }

        void IgnorePartyCollisions(PlayerController player)
        {
            if (_cc == null || !_cc.enabled) return;
            var pc = player.GetComponent<CharacterController>();
            if (pc != null && pc.enabled) Physics.IgnoreCollision(_cc, pc, true);
            foreach (var c in All)
                if (c != this && c._cc != null && c._cc.enabled) Physics.IgnoreCollision(_cc, c._cc, true);
        }

        // =================================================================
        //  Davranış: yüz yönü, bakış, küçük hareketler, ayak sesi
        // =================================================================
        void Face(PlayerController player, float playerSpeed)
        {
            var flatVel = Flat(_vel);
            Vector3 look;
            float rate;
            if (flatVel.magnitude > 0.5f) { look = flatVel; rate = 9f; _idleT = 0f; }
            else
            {
                _idleT += Time.deltaTime;
                // Dedektifin incelediği şeye dön; yoksa onun baktığı yöne hafif içeri açılarak
                var focus = player.Focus;
                if (focus != null && !(focus is HelperInteractable)) look = Flat(focus.Point - transform.position);
                else look = _heading + Flat(player.transform.position - transform.position).normalized * 0.45f;
                rate = 3.2f;
            }
            if (look.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look.normalized), 1f - Mathf.Exp(-Time.deltaTime * rate));

            var h = rig != null ? rig.Human : null;
            if (h == null) return;
            _lookT -= Time.deltaTime;
            if (_lookT <= 0f)
            {
                _lookT = Random.Range(2.5f, 7f);
                var pr = player.Rig;
                var cam = Camera.main;
                float roll = Random.value;
                h.LookTarget = roll < 0.55f && pr != null ? pr.Head : roll < 0.7f && cam != null ? cam.transform : null;
            }
            if (_idleT > _nextIdleAction)
            {
                _nextIdleAction = _idleT + Random.Range(9f, 20f);
                if (_idleT > 3f && !h.GestureActive) h.Gesture(Random.value < 0.6f ? "yes" : "interact");
            }
        }

        void Footsteps(GameManager gm)
        {
            float speed = Flat(_vel).magnitude;
            if (speed < 0.6f || !_cc.isGrounded) { _stepT = 0.1f; return; }
            _stepT -= Time.deltaTime;
            if (_stepT > 0f) return;
            _stepT = speed > 4f ? 0.32f : 0.52f;
            AudioManager.Instance?.Footstep(gm.InsideBuilding, speed > 4f ? 0.16f : 0.1f);
        }

        static Vector3 Flat(Vector3 v) { v.y = 0f; return v; }
    }
}
