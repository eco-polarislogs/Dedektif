using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Dedektif3D.Gameplay
{
    /// <summary>
    /// Film yönetmeni kamerası:
    ///  • Idle    : Splash / Bölge haritası arkasında kasabanın üzerinde ağır dönen vinç çekimi.
    ///  • Story   : Hikaye anlatılırken olay yerinin etrafında alçak, yavaş "dolly" çekimi (altyazılı film açılışı).
    ///  • Descent : "Kasabaya İniş" — uzaktan olay yerine, oradan dedektifin omzuna süzülen açılış planı.
    ///  • Focus   : Delil yakın planı / sorguda omuz üstü (over-the-shoulder) şüpheli çekimi.
    ///  • External: Kamerayı başka bir sistem sürer (3D bölge haritası); yönetmen yalnızca minyatür alan derinliğini ayarlar.
    /// Her karede alan derinliği (Depth of Field) netlik mesafesini kameranın baktığı hedefe ayarlar.
    /// </summary>
    public class CinematicDirector : MonoBehaviour
    {
        public enum Mode { None, Idle, Story, Descent, Focus, External }
        public Mode Current { get; private set; }
        public bool Letterbox => Current == Mode.Story || Current == Mode.Descent || Current == Mode.Focus;
        public float DescentProgress => Current == Mode.Descent ? Mathf.Clamp01(_t / _duration) : 0f;

        public static DepthOfField Dof;   // GameBootstrap post-process profilinden

        Camera _cam;
        ThirdPersonCamera _tpc;
        Vector3 _orbitCenter;
        List<Vector3> _path, _look;
        float _t, _duration;
        Action _onDone;
        Vector3 _focusPos, _focusLook, _fromPos; Quaternion _fromRot; float _blend, _blendT;
        float _focusDistance = 6f;

        public void Init(Camera cam, ThirdPersonCamera tpc, Vector3 orbitCenter)
        {
            _cam = cam; _tpc = tpc; _orbitCenter = orbitCenter;
        }

        void Take(Mode m) { Current = m; if (_tpc != null) _tpc.CinematicOverride = true; }

        public void PlayIdle() => Take(Mode.Idle);
        /// <summary>3D bölge haritası kamerayı kendisi sürer (tilt-shift alan derinliği burada kalır).</summary>
        public void PlayExternal() => Take(Mode.External);
        public void PlayStory() { _t = 0f; Take(Mode.Story); }

        public void PlayDescent(List<Vector3> path, List<Vector3> look, float duration, Action onDone)
        {
            _path = path; _look = look; _duration = duration; _t = 0f; _onDone = onDone;
            Take(Mode.Descent);
        }

        /// <summary>Kamerayı yumuşakça sabit bir sinematik açıya taşır (delil / sorgu).</summary>
        public void FocusShot(Vector3 camPos, Vector3 lookAt, float blendSeconds)
        {
            if (_cam == null) return;
            _fromPos = _cam.transform.position; _fromRot = _cam.transform.rotation;
            _focusPos = camPos; _focusLook = lookAt; _blend = Mathf.Max(0.05f, blendSeconds); _blendT = 0f;
            Take(Mode.Focus);
        }

        public void Skip() { if (Current == Mode.Descent) _t = _duration; }

        public void Stop()
        {
            Current = Mode.None;
            // Fareyle aramadaysa oyuncunun baktığı yön korunur; yakın plandan dönünce oda gözünün önünden kaymaz.
            if (_tpc != null) { _tpc.CinematicOverride = false; _tpc.SnapBehind(); }
        }

        void LateUpdate()
        {
            if (_cam == null) return;
            var tr = _cam.transform;
            Vector3 focusTarget = tr.position + tr.forward * 6f;
            switch (Current)
            {
                case Mode.Idle:
                {
                    // Açılış ekranı: yağmurlu kasabanın çatıları üstünde alçak, ağır vinç çekimi
                    float a = Time.time * 0.022f;
                    var pos = _orbitCenter + new Vector3(Mathf.Sin(a) * 42f, 13.5f + Mathf.Sin(Time.time * 0.08f) * 2f, Mathf.Cos(a) * 42f);
                    tr.position = Vector3.Lerp(tr.position, pos, Time.deltaTime * 2f);
                    tr.rotation = Quaternion.Slerp(tr.rotation, Quaternion.LookRotation(_orbitCenter - tr.position), Time.deltaTime * 2f);
                    focusTarget = _orbitCenter;
                    break;
                }
                case Mode.Story:
                {
                    // Olay yerinin etrafında diz hizasından yavaş dolly + hafif el kamerası titreşimi
                    _t += Time.deltaTime;
                    float a = 2.4f + _t * 0.06f;
                    var center = _orbitCenter + Vector3.down * 2.4f;
                    var pos = center + new Vector3(Mathf.Sin(a) * 9f, 1.6f + Mathf.Sin(_t * 0.2f) * 0.4f, Mathf.Cos(a) * 9f);
                    pos += new Vector3(Mathf.PerlinNoise(_t * 0.4f, 0) - 0.5f, Mathf.PerlinNoise(0, _t * 0.4f) - 0.5f, 0) * 0.12f;
                    tr.position = Vector3.Lerp(tr.position, pos, Time.deltaTime * 1.5f);
                    tr.rotation = Quaternion.Slerp(tr.rotation, Quaternion.LookRotation(center + Vector3.up * 0.3f - tr.position), Time.deltaTime * 1.5f);
                    focusTarget = center;
                    break;
                }
                case Mode.Descent:
                {
                    _t += Time.deltaTime;
                    float u = Mathf.Clamp01(_t / _duration);
                    float e = u * u * (3f - 2f * u);
                    tr.position = CatmullRom(_path, e);
                    var target = CatmullRom(_look, e);
                    tr.rotation = Quaternion.LookRotation(target - tr.position);
                    focusTarget = target;
                    if (u >= 1f)
                    {
                        var cb = _onDone; _onDone = null;
                        Stop();
                        cb?.Invoke();
                    }
                    break;
                }
                case Mode.Focus:
                {
                    _blendT += Time.deltaTime;
                    float k = Mathf.SmoothStep(0, 1, Mathf.Clamp01(_blendT / _blend));
                    var breathe = new Vector3(Mathf.Sin(Time.time * 0.6f), Mathf.Sin(Time.time * 0.45f), 0) * 0.015f;
                    tr.position = Vector3.Lerp(_fromPos, _focusPos, k) + breathe;
                    tr.rotation = Quaternion.Slerp(_fromRot, Quaternion.LookRotation(_focusLook - _focusPos), k);
                    focusTarget = _focusLook;
                    break;
                }
                case Mode.External:
                {
                    var map = World.WorldMapDiorama.Instance;
                    focusTarget = map != null && map.Active ? map.FocusPoint : tr.position + tr.forward * 3f;
                    break;
                }
                default:
                    if (_tpc != null && _tpc.target != null) focusTarget = _tpc.target.position + Vector3.up * 1.5f;
                    break;
            }
            UpdateDof(Vector3.Distance(tr.position, focusTarget));
        }

        void UpdateDof(float dist)
        {
            if (Dof == null) return;
            // Oynanışta alan derinliği kapalı (bulanıklık yok); yalnızca hikaye ve yakın plan çekimlerinde sinematik bokeh
            Dof.active = Core.GameSettings.DepthOfField && (Current == Mode.Focus || Current == Mode.Story || Current == Mode.External);
            _focusDistance = Mathf.Lerp(_focusDistance, Mathf.Clamp(dist, 0.4f, 60f), Time.deltaTime * 5f);
            Dof.focusDistance.Override(_focusDistance);
            // Yakın planlarda daha sığ alan derinliği (sinematik bokeh); haritada uzun odaklı "tilt-shift" minyatür etkisi
            Dof.aperture.Override(Current == Mode.Focus ? 2.8f : Current == Mode.External ? 3.2f : 6f);
            Dof.focalLength.Override(Current == Mode.External ? 85f : 55f);
        }

        static Vector3 CatmullRom(List<Vector3> pts, float u)
        {
            if (pts == null || pts.Count == 0) return Vector3.zero;
            if (pts.Count == 1) return pts[0];
            float f = u * (pts.Count - 1);
            int i = Mathf.Min(Mathf.FloorToInt(f), pts.Count - 2);
            float t = f - i;
            var p0 = pts[Mathf.Max(i - 1, 0)]; var p1 = pts[i]; var p2 = pts[i + 1]; var p3 = pts[Mathf.Min(i + 2, pts.Count - 1)];
            float t2 = t * t, t3 = t2 * t;
            return 0.5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
        }
    }
}
