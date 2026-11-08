using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Dedektif3D.Core
{
    /// <summary>
    /// Oyuncu ayarları (Ayarlar menüsü): parlaklık, ses kanalları, grafik kalitesi, tam ekran, film efektleri,
    /// fare hassasiyeti ve altyazı hızı. PlayerPrefs'te saklanır; değişiklikler anında uygulanır.
    /// </summary>
    public static class GameSettings
    {
        public const float DefaultBrightness = 0.55f;

        /// <summary>Pozlama farkı (EV). Karanlık sahneler için artırılır.</summary>
        public static float Brightness = DefaultBrightness;
        public static float Master = 0.9f, Music = 0.55f, Effects = 0.85f, Ambience = 0.8f;
        /// <summary>0 = Düşük, 1 = Orta, 2 = Yüksek.</summary>
        public static int Quality = 2;
        public static bool Fullscreen = true, FilmGrain = true, DepthOfField = true, CameraShake = true;
        public static float MouseSensitivity = 1f;
        public static float TextSpeed = 1f;
        /// <summary>Binaya girince fareyle arama kendiliğinden açılsın (web'deki gibi).</summary>
        public static bool AutoSearch = true;

        public static readonly string[] QualityNames = { "DÜŞÜK", "ORTA", "YÜKSEK" };

        /// <summary>Kaliteye göre gölge mesafesi (3D harita açıkken geçici olarak kendi değerini kullanır).</summary>
        public static float ShadowDistance => Quality == 0 ? 25f : Quality == 1 ? 38f : 50f;

        static bool _loaded;

        public static void Load()
        {
            if (_loaded) return;
            _loaded = true;
            if (!PlayerPrefs.HasKey("ayar_surum")) return;     // ilk açılış: varsayılanlar
            Brightness = PlayerPrefs.GetFloat("ayar_parlaklik", Brightness);
            Master = PlayerPrefs.GetFloat("ayar_ses_ana", Master);
            Music = PlayerPrefs.GetFloat("ayar_ses_muzik", Music);
            Effects = PlayerPrefs.GetFloat("ayar_ses_efekt", Effects);
            Ambience = PlayerPrefs.GetFloat("ayar_ses_ortam", Ambience);
            Quality = Mathf.Clamp(PlayerPrefs.GetInt("ayar_kalite", Quality), 0, 2);
            Fullscreen = PlayerPrefs.GetInt("ayar_tamekran", 1) == 1;
            FilmGrain = PlayerPrefs.GetInt("ayar_gren", 1) == 1;
            DepthOfField = PlayerPrefs.GetInt("ayar_alanderinligi", 1) == 1;
            CameraShake = PlayerPrefs.GetInt("ayar_sarsinti", 1) == 1;
            MouseSensitivity = PlayerPrefs.GetFloat("ayar_fare", MouseSensitivity);
            TextSpeed = PlayerPrefs.GetFloat("ayar_metin", TextSpeed);
            AutoSearch = PlayerPrefs.GetInt("ayar_farearama", 1) == 1;
        }

        public static void Save()
        {
            PlayerPrefs.SetInt("ayar_surum", 1);
            PlayerPrefs.SetFloat("ayar_parlaklik", Brightness);
            PlayerPrefs.SetFloat("ayar_ses_ana", Master);
            PlayerPrefs.SetFloat("ayar_ses_muzik", Music);
            PlayerPrefs.SetFloat("ayar_ses_efekt", Effects);
            PlayerPrefs.SetFloat("ayar_ses_ortam", Ambience);
            PlayerPrefs.SetInt("ayar_kalite", Quality);
            PlayerPrefs.SetInt("ayar_tamekran", Fullscreen ? 1 : 0);
            PlayerPrefs.SetInt("ayar_gren", FilmGrain ? 1 : 0);
            PlayerPrefs.SetInt("ayar_alanderinligi", DepthOfField ? 1 : 0);
            PlayerPrefs.SetInt("ayar_sarsinti", CameraShake ? 1 : 0);
            PlayerPrefs.SetFloat("ayar_fare", MouseSensitivity);
            PlayerPrefs.SetFloat("ayar_metin", TextSpeed);
            PlayerPrefs.SetInt("ayar_farearama", AutoSearch ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void ResetDefaults()
        {
            Brightness = DefaultBrightness; Master = 0.9f; Music = 0.55f; Effects = 0.85f; Ambience = 0.8f;
            Quality = 2; FilmGrain = true; DepthOfField = true; CameraShake = true; MouseSensitivity = 1f; TextSpeed = 1f; AutoSearch = true;
        }

        /// <summary>Tüm ayarları uygular. screen=true ise pencere / tam ekran modu da ayarlanır.</summary>
        public static void Apply(bool screen = false)
        {
            var am = AudioManager.Instance;
            AudioListener.volume = am != null && am.Muted ? 0f : Master;
            am?.ApplyVolumes();
            GameBootstrap.RefreshPost();
            Gameplay.CameraFx.Enabled = CameraShake;
            ApplyQuality();
            ApplyFrameCap();
            if (screen) ApplyScreen();
        }

        /// <summary>
        /// Kare hızı sınırı: ekran kartı gereksiz yere tam güçte çalışmaz (dizüstünde ısı ve güç sıçramalarını azaltır).
        /// 60 Hz ekranda 60 kare; 100 Hz ve üstünde her iki yenilemede bir kare.
        /// </summary>
        public static void ApplyFrameCap()
        {
            double hz = Screen.currentResolution.refreshRateRatio.value;
            QualitySettings.vSyncCount = hz >= 100.0 ? 2 : 1;
            Application.targetFrameRate = 60;
        }

        public static void ApplyQuality()
        {
            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            var map = World.WorldMapDiorama.Instance;
            if (urp != null)
            {
                if (map == null || !map.Active) urp.shadowDistance = ShadowDistance;
                urp.renderScale = Quality == 0 ? 0.8f : 1f;
            }
            QualitySettings.anisotropicFiltering = Quality == 0 ? AnisotropicFiltering.Disable : AnisotropicFiltering.ForceEnable;
            QualitySettings.lodBias = Quality == 0 ? 0.7f : Quality == 1 ? 1f : 1.5f;
            var cam = Camera.main;
            if (cam != null)
            {
                var data = cam.GetUniversalAdditionalCameraData();
                if (data != null)
                {
                    data.antialiasing = Quality == 0 ? AntialiasingMode.None : AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    data.antialiasingQuality = Quality == 2 ? AntialiasingQuality.High : AntialiasingQuality.Medium;
                }
            }
        }

        /// <summary>Komut satırında pencere/çözünürlük verilmişse (test çalıştırmaları) ekran moduna dokunulmaz.</summary>
        public static bool ScreenOverriddenByCommandLine
        {
            get
            {
                foreach (var a in System.Environment.GetCommandLineArgs())
                    if (a == "-screen-fullscreen" || a == "-screen-width" || a == "-dedektifShots" || a == "-popupwindow") return true;
                return false;
            }
        }

        public static void ApplyScreen()
        {
            if (Application.isEditor || ScreenOverriddenByCommandLine) return;
            int w = Display.main.systemWidth, h = Display.main.systemHeight;
            if (w <= 0 || h <= 0) { w = Screen.currentResolution.width; h = Screen.currentResolution.height; }
            if (Fullscreen) Screen.SetResolution(w, h, FullScreenMode.FullScreenWindow);
            else Screen.SetResolution(Mathf.RoundToInt(w * 0.8f), Mathf.RoundToInt(h * 0.8f), FullScreenMode.Windowed);
        }
    }
}
