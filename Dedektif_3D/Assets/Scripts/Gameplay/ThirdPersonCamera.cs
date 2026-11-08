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

        /// <summary>Kamerayı arkaya alır. Fareyle aramada oyuncunun baktığı yön korunur (keepView); ışınlanmada force ile sıfırlanır.</summary>
        public void SnapBehind(bool force = false)
        {
            if (target == null) return;
            if (force || !SearchMode)
            {
                _yaw = target.eulerAngles.y;
                _pitch = SearchMode ? 4f : 12f;
            }
            _currentDistance = SearchMode ? 0f : distance;
            LateUpdate();
            var rot0 = Quaternion.Euler(_pitch, _yaw, 0);
            transform.rotation = rot0;
            transform.position = SearchMode
                ? target.position + Vector3.up * SearchEyeHeight
                : target.position + Vector3.up * height + rot0 * Vector3.right * shoulder - rot0 * Vector3.forward * _currentDistance;
        }

        /// <summary>true iken kamerayı sinematik yönetmen (CinematicDirector) kontrol eder.</summary>
        public bool CinematicOverride { get; set; }

        // ---------- Fareyle arama (SceneSearch): dedektifin gözünden, imleç serbest ----------
        const float SearchEyeHeight = 1.62f;
        /// <summary>Ekranın ortasında kamera sabit kalır; imleç kenara yaklaştıkça dönme hızlanır.</summary>
        const float PanDeadX = 0.62f, PanDeadY = 0.7f, PanSpeedX = 95f, PanSpeedY = 42f;
        public bool SearchMode { get; set; }
        /// <summary>0 = omuz üstü, 1 = dedektifin gözü.</summary>
        public float SearchBlend { get; private set; }
        public void ResetSearchBlend() { SearchBlend = 0f; _currentDistance = distance; }

        /// <summary>Otomatik test / ekran görüntüsü: göz hizasından bir noktaya anında bakar.</summary>
        public void DebugLookAt(Vector3 world)
        {
            if (target == null) return;
            var from = target.position + Vector3.up * SearchEyeHeight;
            var d = world - from;
            _yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            _pitch = Mathf.Clamp(-Mathf.Atan2(d.y, new Vector2(d.x, d.z).magnitude) * Mathf.Rad2Deg, -42f, maxPitch);
            SearchBlend = SearchMode ? 1f : SearchBlend;
            _currentDistance = 0f;
            transform.SetPositionAndRotation(from, Quaternion.Euler(_pitch, _yaw, 0));
        }

        void SearchPan(Mouse mouse)
        {
            if (!Application.isFocused) return;
            var search = SceneSearch.Instance;
            if (search != null && search.PanPaused) return;
            var p = mouse.position.ReadValue();
            float nx = Mathf.Clamp(p.x / Mathf.Max(1, Screen.width) * 2f - 1f, -1f, 1f);
            float ny = Mathf.Clamp(p.y / Mathf.Max(1, Screen.height) * 2f - 1f, -1f, 1f);
            float ax = Mathf.Sign(nx) * Mathf.Pow(Mathf.Clamp01((Mathf.Abs(nx) - PanDeadX) / (1f - PanDeadX)), 2f);
            float ay = Mathf.Sign(ny) * Mathf.Pow(Mathf.Clamp01((Mathf.Abs(ny) - PanDeadY) / (1f - PanDeadY)), 2f);
            float k = Core.GameSettings.MouseSensitivity * Time.unscaledDeltaTime;
            _yaw += ax * PanSpeedX * k;
            _pitch -= ay * PanSpeedY * k;
        }

        void LateUpdate()
        {
            if (target == null || CinematicOverride) return;
            SearchBlend = Mathf.MoveTowards(SearchBlend, SearchMode ? 1f : 0f, Time.unscaledDeltaTime * 2.6f);
            float sk = Mathf.SmoothStep(0f, 1f, SearchBlend);
            var mouse = Mouse.current;
            if (LookEnabled && mouse != null)
            {
                if (SearchMode) SearchPan(mouse);
                else if (SearchBlend < 0.9f)   // aramadan çıkarken imleç kilidinin ilk sıçraması kameraya yansımasın
                {
                    var delta = mouse.delta.ReadValue();
                    float sens = sensitivity * Core.GameSettings.MouseSensitivity;
                    _yaw += delta.x * sens;
                    _pitch -= delta.y * sens;
                    float scroll = mouse.scroll.ReadValue().y;
                    if (Mathf.Abs(scroll) > 0.01f) distance = Mathf.Clamp(distance - Mathf.Sign(scroll) * 0.4f, minDistance, maxDistance);
                }
            }
            var gp = Gamepad.current;
            if (LookEnabled && gp != null)
            {
                var rs = gp.rightStick.ReadValue();
                _yaw += rs.x * 140f * Time.deltaTime;
                _pitch -= rs.y * 100f * Time.deltaTime;
            }
            _pitch = Mathf.Clamp(_pitch, Mathf.Lerp(minPitch, -42f, sk), maxPitch);

            var rot = Quaternion.Euler(_pitch, _yaw, 0);
            var pivot = target.position + Vector3.up * Mathf.Lerp(height, SearchEyeHeight, sk) + rot * Vector3.right * Mathf.Lerp(shoulder, 0f, sk);

            // Duvar çarpışması: oyuncu katmanı (2) hariç her şeye spherecast
            float wanted = Mathf.Lerp(distance, 0f, sk);
            int mask = ~((1 << 2) | (1 << World.TownGenerator.NpcBarrierLayer));
            if (wanted > 0.05f && Physics.SphereCast(pivot, 0.25f, -(rot * Vector3.forward), out var hit, wanted, mask, QueryTriggerInteraction.Ignore))
                wanted = Mathf.Max(0.4f * (1f - sk), hit.distance - 0.1f);
            _currentDistance = wanted < _currentDistance ? wanted : Mathf.Lerp(_currentDistance, wanted, Time.deltaTime * 6f);

            // Hafif gecikmeli takip + koşarken FOV açılması + el kamerası nefesi. Aramada oda daha geniş görünür.
            var cc = target.GetComponent<CharacterController>();
            float speed = cc != null ? new Vector3(cc.velocity.x, 0, cc.velocity.z).magnitude : 0f;
            var cam = GetComponent<Camera>();
            if (cam != null) cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, baseFov + sk * 16f + Mathf.Clamp01((speed - 3.5f) / 3f) * 8f, Time.deltaTime * 3f);
            var sway = new Vector3(Mathf.PerlinNoise(Time.time * 0.35f, 1.3f) - 0.5f, Mathf.PerlinNoise(2.1f, Time.time * 0.35f) - 0.5f, 0f) * Mathf.Lerp(0.05f, 0.012f, sk);
            var wantedPos = pivot - rot * Vector3.forward * _currentDistance + rot * sway;
            transform.position = Vector3.Lerp(transform.position, wantedPos, 1f - Mathf.Exp(-Time.deltaTime * 14f));
            transform.rotation = Quaternion.Slerp(transform.rotation, rot * Quaternion.Euler(sway.y * 6f, sway.x * 6f, 0), 1f - Mathf.Exp(-Time.deltaTime * 18f));
        }
    }
}
