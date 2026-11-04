using Dedektif3D.Core;
using Dedektif3D.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dedektif3D.Gameplay
{
    /// <summary>
    /// Üçüncü şahıs dedektif kontrolcüsü (CharacterController + yeni Input System).
    /// WASD hareket, Shift koşma, Space zıplama, fare ile kamera, E etkileşim, Q Dedektif Gözü.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public float walkSpeed = 3.2f;
        public float runSpeed = 6.2f;
        public float jumpHeight = 0.9f;
        public float gravity = -20f;
        public float turnSpeed = 12f;

        public ThirdPersonCamera Cam { get; private set; }
        public Interactable Focus { get; private set; }
        public bool InputEnabled { get; set; } = true;

        CharacterController _cc;
        PropFactory.PersonRig _rig;
        float _vy;
        float _walkCycle;
        float _stepTimer;

        public static PlayerController Create(Vector3 pos, Quaternion rot)
        {
            var go = new GameObject("Dedektif (Oyuncu)");
            go.layer = 2; // Ignore Raycast: kamera çarpışma testleri oyuncuyu görmesin
            go.transform.SetPositionAndRotation(pos, rot);
            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.85f; cc.radius = 0.35f; cc.center = new Vector3(0, 0.93f, 0);
            cc.stepOffset = 0.4f; cc.slopeLimit = 50f;
            var pc = go.AddComponent<PlayerController>();
            pc._rig = PropFactory.BuildDetective(go.transform);
            InteriorGenerator.SetLayerRecursive(pc._rig.Root.gameObject, 2);
            return pc;
        }

        void Awake() => _cc = GetComponent<CharacterController>();

        /// <summary>Splash/hikaye sırasında dedektif görünmez; kasabaya inişte belirir.</summary>
        public void SetVisible(bool visible)
        {
            foreach (var r in GetComponentsInChildren<Renderer>(true)) r.enabled = visible;
        }

        public void AttachCamera(Camera cam)
        {
            Cam = cam.gameObject.GetComponent<ThirdPersonCamera>();
            if (Cam == null) Cam = cam.gameObject.AddComponent<ThirdPersonCamera>();
            Cam.target = transform;
            Cam.SnapBehind();
        }

        public void Teleport(Vector3 pos, Quaternion rot)
        {
            _cc.enabled = false;
            transform.SetPositionAndRotation(pos, rot);
            _cc.enabled = true;
            _vy = 0;
            if (Cam != null) Cam.SnapBehind();
        }

        void Update()
        {
            var kb = Keyboard.current;
            Vector2 move = Vector2.zero;
            bool run = false, jump = false;

            if (InputEnabled && kb != null)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) move.y += 1;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) move.y -= 1;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) move.x += 1;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) move.x -= 1;
                run = kb.leftShiftKey.isPressed;
                jump = kb.spaceKey.wasPressedThisFrame;
            }
            var gp = Gamepad.current;
            if (InputEnabled && gp != null)
            {
                var stick = gp.leftStick.ReadValue();
                if (stick.sqrMagnitude > 0.04f) move = stick;
                run |= gp.leftStickButton.isPressed;
                jump |= gp.buttonSouth.wasPressedThisFrame;
            }
            move = Vector2.ClampMagnitude(move, 1f);

            // Kamera yönüne göre hareket
            Vector3 fwd = Cam != null ? Cam.Forward : Vector3.forward;
            Vector3 right = Vector3.Cross(Vector3.up, fwd);
            Vector3 dir = fwd * move.y + right * move.x;
            float speed = run ? runSpeed : walkSpeed;

            if (dir.sqrMagnitude > 0.001f)
            {
                var target = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * turnSpeed);
            }

            if (_cc.isGrounded)
            {
                _vy = -2f;
                if (jump) _vy = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            _vy += gravity * Time.deltaTime;

            var velocity = dir * speed + Vector3.up * _vy;
            _cc.Move(velocity * Time.deltaTime);

            AnimateRig(dir.magnitude * speed);
            // Ayak sesleri (zemine göre)
            float moving = dir.magnitude * speed;
            if (_cc.isGrounded && moving > 0.5f)
            {
                _stepTimer -= Time.deltaTime;
                if (_stepTimer <= 0f)
                {
                    _stepTimer = run ? 0.3f : 0.5f;
                    var gm = GameManager.Instance;
                    AudioManager.Instance?.Footstep(gm != null && gm.InsideBuilding, run ? 0.45f : 0.3f);
                }
            }
            else _stepTimer = 0.1f;
            UpdateFocus();

            if (InputEnabled && kb != null)
            {
                if ((kb.eKey.wasPressedThisFrame || (gp != null && gp.buttonWest.wasPressedThisFrame)) && Focus != null)
                    Focus.Interact();
                if (kb.qKey.wasPressedThisFrame || (gp != null && gp.buttonNorth.wasPressedThisFrame))
                    GameManager.Instance.SearchWithMagnifier(transform.position);
            }
        }

        /// <summary>Prosedürel yürüyüş animasyonu: bacak ve kol salınımı.</summary>
        void AnimateRig(float speed)
        {
            if (_rig == null || _rig.HasModel) return;   // gerçek model: HumanAnimator yürür/koşar
            float amount = Mathf.Clamp01(speed / runSpeed);
            _walkCycle += Time.deltaTime * (4f + speed * 1.6f);
            float swing = Mathf.Sin(_walkCycle) * 38f * amount;
            _rig.LegL.localRotation = Quaternion.Euler(swing, 0, 0);
            _rig.LegR.localRotation = Quaternion.Euler(-swing, 0, 0);
            _rig.ArmL.localRotation = Quaternion.Euler(-swing * 0.8f, 0, 0);
            _rig.ArmR.localRotation = Quaternion.Euler(swing * 0.8f, 0, 0);
            float bob = Mathf.Abs(Mathf.Sin(_walkCycle)) * 0.05f * amount;
            _rig.Body.localPosition = new Vector3(0, bob, 0);
        }

        /// <summary>En yakın etkileşimli objeyi seç (önüne bakılan tercih edilir).</summary>
        void UpdateFocus()
        {
            Interactable best = null;
            float bestScore = float.MaxValue;
            var pos = transform.position + Vector3.up * 1f;
            foreach (var it in Interactable.All)
            {
                if (it == null || !it.isActiveAndEnabled || !it.CanInteract) continue;
                var d = it.Point - pos;
                var flat = new Vector3(d.x, 0, d.z);
                float dist = flat.magnitude;
                if (dist > it.radius || Mathf.Abs(d.y) > 2.2f) continue;
                float facing = dist > 0.01f ? Vector3.Dot(transform.forward, flat / dist) : 1f;
                float score = dist - facing * 0.8f;
                if (score < bestScore) { bestScore = score; best = it; }
            }
            Focus = best;
        }
    }
}
