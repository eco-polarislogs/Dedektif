using UnityEngine;
using UnityEngine.InputSystem;

namespace Dedektif3D.Gameplay
{
    /// <summary>
    /// "4D" hissi: oyunda hissedilebilen fiziksel geri bildirim — gök gürültüsünde, kapı çarpınca, tokmak vurunca ve
    /// Dedektif Gözü'nün kalp atışında kamera sarsıntısı + gamepad titreşimi (sol/sağ motor).
    /// Kamera kontrolcüleri (ThirdPersonCamera / CinematicDirector) konumu yazdıktan SONRA ofset eklenir ve
    /// bir sonraki karenin başında geri alınır — kayma birikmez.
    /// </summary>
    [DefaultExecutionOrder(10000)]
    public class CameraFx : MonoBehaviour
    {
        static CameraFx _i;
        float _shakeAmp, _shakeUntil, _shakeStart, _shakeAt = -1f, _pendingAmp, _pendingDur;
        float _rumbleUntil = -1f, _rumbleAt = -1f, _rLow, _rHigh, _rDur;
        Vector3 _appliedPos; Quaternion _appliedRot = Quaternion.identity;
        public static bool Enabled = true;

        public static void Attach(Camera cam) { if (cam != null && cam.GetComponent<CameraFx>() == null) _i = cam.gameObject.AddComponent<CameraFx>(); }

        /// <summary>Sarsıntı: genlik (metre), süre (sn), gecikme (sn — ör. şimşekten sonra gök gürültüsü).</summary>
        public static void Shake(float amplitude, float duration, float delay = 0f)
        {
            if (_i == null || !Enabled) return;
            if (delay > 0f) { _i._shakeAt = Time.time + delay; _i._pendingAmp = amplitude; _i._pendingDur = duration; return; }
            _i.StartShake(amplitude, duration);
        }

        /// <summary>Gamepad titreşimi: düşük frekans (gövde), yüksek frekans (tık), süre.</summary>
        public static void Rumble(float low, float high, float duration, float delay = 0f)
        {
            if (_i == null || !Enabled) return;
            _i._rLow = low; _i._rHigh = high; _i._rDur = duration;
            _i._rumbleAt = Time.time + delay;
        }

        void StartShake(float amp, float dur)
        {
            if (Time.time < _shakeUntil && amp < _shakeAmp) return;
            _shakeAmp = amp; _shakeStart = Time.time; _shakeUntil = Time.time + dur;
        }

        void Update()
        {
            // Önceki karenin ofsetini geri al
            transform.position -= _appliedPos;
            transform.rotation = Quaternion.Inverse(_appliedRot) * transform.rotation;
            _appliedPos = Vector3.zero; _appliedRot = Quaternion.identity;

            if (_shakeAt > 0f && Time.time >= _shakeAt) { _shakeAt = -1f; StartShake(_pendingAmp, _pendingDur); }
            var gp = Gamepad.current;
            if (_rumbleAt > 0f && Time.time >= _rumbleAt)
            {
                _rumbleAt = -1f; _rumbleUntil = Time.time + _rDur;
                gp?.SetMotorSpeeds(_rLow, _rHigh);
            }
            if (_rumbleUntil > 0f && Time.time >= _rumbleUntil) { _rumbleUntil = -1f; gp?.SetMotorSpeeds(0f, 0f); }
        }

        void LateUpdate()
        {
            if (Time.time >= _shakeUntil) return;
            float k = 1f - (Time.time - _shakeStart) / Mathf.Max(0.01f, _shakeUntil - _shakeStart);
            float a = _shakeAmp * k * k;
            float t = Time.time * 28f;
            var off = new Vector3(Mathf.PerlinNoise(t, 0.3f) - 0.5f, Mathf.PerlinNoise(1.7f, t) - 0.5f, 0f) * 2f * a;
            _appliedPos = transform.rotation * off;
            _appliedRot = Quaternion.Euler((Mathf.PerlinNoise(t, 4.2f) - 0.5f) * a * 30f, 0f, (Mathf.PerlinNoise(5.1f, t) - 0.5f) * a * 40f);
            transform.position += _appliedPos;
            transform.rotation = _appliedRot * transform.rotation;
        }

        void OnDisable() { Gamepad.current?.SetMotorSpeeds(0f, 0f); }
    }
}
