using UnityEngine;
using UnityEngine.InputSystem;

namespace Dedektif3D.Gameplay
{
    /// <summary>
    /// Omuz üstü takip kamerası: fare ile döner, tekerlek ile yakınlaşır, duvarlara çarpınca içeri girer.
    /// </summary>
    public class ThirdPersonCamera : MonoBehaviour
    {
        public Transform target;
        // Sinematik omuz üstü (over-the-shoulder): yakın, sağ omza kayık, göz hizasında
        public float distance = 2.7f;
        public float minDistance = 1.0f, maxDistance = 5f;
        public float height = 1.62f;
        public float shoulder = 0.68f;
        public float baseFov = 52f;
        public float sensitivity = 0.12f;
        public float minPitch = -25f, maxPitch = 65f;
        public bool LookEnabled { get; set; } = true;

        float _yaw, _pitch = 12f;
        float _currentDistance;

        /// <summary>Kameranın yere izdüşen ileri yönü (oyuncu hareketi için).</summary>
        public Vector3 Forward => Quaternion.Euler(0, _yaw, 0) * Vector3.forward;

        public void SnapBehind()
        {
            if (target == null) return;
            _yaw = target.eulerAngles.y;
            _pitch = 12f;
            _currentDistance = distance;
            LateUpdate();
            var rot0 = Quaternion.Euler(_pitch, _yaw, 0);
            transform.rotation = rot0;
            transform.position = target.position + Vector3.up * height + rot0 * Vector3.right * shoulder - rot0 * Vector3.forward * _currentDistance;
        }

        /// <summary>true iken kamerayı sinematik yönetmen (CinematicDirector) kontrol eder.</summary>
        public bool CinematicOverride { get; set; }

        void LateUpdate()
        {
            if (target == null || CinematicOverride) return;
            var mouse = Mouse.current;
            if (LookEnabled && mouse != null)
            {
                var delta = mouse.delta.ReadValue();
                float sens = sensitivity * Core.GameSettings.MouseSensitivity;
                _yaw += delta.x * sens;
                _pitch -= delta.y * sens;
                float scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f) distance = Mathf.Clamp(distance - Mathf.Sign(scroll) * 0.4f, minDistance, maxDistance);
            }
            var gp = Gamepad.current;
            if (LookEnabled && gp != null)
            {
                var rs = gp.rightStick.ReadValue();
                _yaw += rs.x * 140f * Time.deltaTime;
                _pitch -= rs.y * 100f * Time.deltaTime;
            }
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

            var rot = Quaternion.Euler(_pitch, _yaw, 0);
            var pivot = target.position + Vector3.up * height + rot * Vector3.right * shoulder;

            // Duvar çarpışması: oyuncu katmanı (2) hariç her şeye spherecast
            float wanted = distance;
            int mask = ~((1 << 2) | (1 << World.TownGenerator.NpcBarrierLayer));
            if (Physics.SphereCast(pivot, 0.25f, -(rot * Vector3.forward), out var hit, wanted, mask, QueryTriggerInteraction.Ignore))
                wanted = Mathf.Max(0.4f, hit.distance - 0.1f);
            _currentDistance = wanted < _currentDistance ? wanted : Mathf.Lerp(_currentDistance, wanted, Time.deltaTime * 6f);

            // Hafif gecikmeli takip + koşarken FOV açılması + el kamerası nefesi
            var cc = target.GetComponent<CharacterController>();
            float speed = cc != null ? new Vector3(cc.velocity.x, 0, cc.velocity.z).magnitude : 0f;
            var cam = GetComponent<Camera>();
            if (cam != null) cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, baseFov + Mathf.Clamp01((speed - 3.5f) / 3f) * 8f, Time.deltaTime * 3f);
            var sway = new Vector3(Mathf.PerlinNoise(Time.time * 0.35f, 1.3f) - 0.5f, Mathf.PerlinNoise(2.1f, Time.time * 0.35f) - 0.5f, 0f) * 0.05f;
            var wantedPos = pivot - rot * Vector3.forward * _currentDistance + rot * sway;
            transform.position = Vector3.Lerp(transform.position, wantedPos, 1f - Mathf.Exp(-Time.deltaTime * 14f));
            transform.rotation = Quaternion.Slerp(transform.rotation, rot * Quaternion.Euler(sway.y * 6f, sway.x * 6f, 0), 1f - Mathf.Exp(-Time.deltaTime * 18f));
        }
    }
}
