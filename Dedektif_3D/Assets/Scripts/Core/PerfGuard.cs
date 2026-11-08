using Dedektif3D.Gameplay;
using UnityEngine;

namespace Dedektif3D.Core
{
    /// <summary>
    /// Zayıf bilgisayarda oyun takılmasın diye: oynanış sırasında kare hızı uzun süre düşük kalırsa grafik kalitesini
    /// bir kademe indirir ve oyuncuya söyler. Oyuncu Ayarlar'dan istediği zaman geri alabilir.
    /// Sinematik ve menülerde ölçüm yapmaz; yükleme takılmaları hesaba girmez.
    /// </summary>
    public class PerfGuard : MonoBehaviour
    {
        const float Window = 10f, LowFps = 24f, SpikeSeconds = 0.5f;

        float _elapsed;
        int _frames;
        float _nextAllowed;

        static bool Disabled
        {
            get
            {
                if (Application.isBatchMode) return true;
                foreach (var a in System.Environment.GetCommandLineArgs())
                    if (a == "-dedektifShots") return true;
                return false;
            }
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (Disabled || gm == null || gm.Phase != GamePhase.Playing || (gm.UI != null && gm.UI.IsModalOpen) || Time.unscaledTime < _nextAllowed
                || (gm.Director != null && gm.Director.Current == CinematicDirector.Mode.Focus))
            {
                _elapsed = 0f; _frames = 0;
                return;
            }
            float dt = Time.unscaledDeltaTime;
            if (dt > SpikeSeconds) return;
            _elapsed += dt;
            _frames++;
            if (_elapsed < Window) return;

            float fps = _frames / _elapsed;
            _elapsed = 0f; _frames = 0;
            if (fps >= LowFps || GameSettings.Quality <= 0) return;

            GameSettings.Quality--;
            GameSettings.Apply();
            GameSettings.Save();
            _nextAllowed = Time.unscaledTime + 20f;
            gm.UI.Toast($"Akıcılık için grafik kalitesi {GameSettings.QualityNames[GameSettings.Quality]} yapıldı (Ayarlar'dan değişir).");
            Debug.Log($"[Dedektif3D] Kare hızı {fps:0} — kalite {GameSettings.QualityNames[GameSettings.Quality]} yapıldı.");
        }
    }
}
