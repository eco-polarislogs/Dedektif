using Dedektif3D.API;
using Dedektif3D.Gameplay;
using Dedektif3D.UI;
using Dedektif3D.World;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Dedektif3D.Core
{
    /// <summary>
    /// PROGRAMMATIC SCENE GENERATOR — giriş noktası.
    /// Play'e basıldığında sahneye hiçbir şey eklemeden çalışır ve sırasıyla kurar:
    ///   ışık + gökyüzü + sis → API → kasaba → kamera + post-process → ses → oyuncu → arayüz → oyun yöneticisi.
    /// Oyun SPLASH ekranında açılır (web: #splash-screen); kasabaya iniş ancak hikaye bittikten sonra yapılır.
    /// Devre dışı bırakmak için sahneye "NoDedektifBootstrap" adlı boş bir obje koymanız yeterli.
    /// </summary>
    public static class GameBootstrap
    {
        public const string RootName = "DedektifDünyası";

        // ---- Atmosfer paleti (town_bg.png / town_map_generated.png'den: çelik mavisi gök, gri-mavi sis, sıcak lamba ışığı) ----
        public static readonly Color SkyTint = new Color(0.46f, 0.52f, 0.62f);
        public static readonly Color HorizonFog = new Color(0.38f, 0.43f, 0.5f);
        public static readonly Color AmbientSky = new Color(0.46f, 0.51f, 0.6f);
        public static readonly Color AmbientEquator = new Color(0.3f, 0.32f, 0.36f);
        public static readonly Color AmbientGround = new Color(0.13f, 0.13f, 0.13f);
        public const float OutdoorFog = 0.0085f;
        public const float IndoorFog = 0.002f;

        public static Light Sun { get; private set; }
        public static DepthOfField Dof { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnGameStart()
        {
            if (GameObject.Find("NoDedektifBootstrap") != null) return;
            if (GameObject.Find(RootName) != null) return;
            Build();
        }

        public static void Build()
        {
            float t0 = Time.realtimeSinceStartup;
            GameSettings.Load();
            GameSettings.ApplyScreen();            // Windows oyunu tam ekran açılır (Ayarlar'dan değiştirilebilir)
            var root = new GameObject(RootName);

            // 1) Işık, gökyüzü, sis
            SetupLighting(root.transform);

            // 2) Backend API
            var api = GameAPIManager.EnsureExists();

            // 3) Dünya
            var town = root.AddComponent<TownGenerator>();
            GameData.SetCurrent("gizemli");
            town.Generate(GameData.Current);
            ApplyAtmosphere(GameData.Current);

            // 4) Kamera + görsel efektler
            var cam = SetupCamera();
            CameraFx.Attach(cam);
            SetupPostProcessing(root.transform);

            // 5) Ses + şimşek
            AudioManager.Create(root.transform);
            var lightning = root.AddComponent<LightningController>();
            lightning.sun = Sun;

            // 5b) Gün hesabı: 24 gerçek dakika = 24 saat (gündüz / gece)
            DayNightCycle.Create(root, Sun);

            // 6) Oyuncu (iniş sinematiğine kadar gizli)
            var player = PlayerController.Create(town.PlayerSpawn, town.PlayerSpawnRotation);
            player.transform.SetParent(root.transform, true);
            player.AttachCamera(cam);

            // 6b) 3D bölge haritası (dedektifin masasındaki minyatür diorama; kasabadan uzakta, gizli)
            WorldMapDiorama.Create();

            // 7) Arayüz + oyun yöneticisi
            var ui = root.AddComponent<GameUI>();
            var gm = root.AddComponent<GameManager>();
            // Olay yeri şeridi yalnızca NPC'lere kapalı: dedektif (katman 2) engelden geçer
            Physics.IgnoreLayerCollision(2, World.TownGenerator.NpcBarrierLayer, true);
            gm.Initialize(town, player, api, ui, lightning);

            GameSettings.Apply();
            Physics.SyncTransforms();
            Debug.Log($"[Dedektif3D] Dünya {(Time.realtimeSinceStartup - t0) * 1000f:0} ms'de inşa edildi. " +
                      $"Bina: {town.Buildings.Count + town.DecorBuildings.Count}, iç mekan: {town.Interiors.Count}, obje: {root.GetComponentsInChildren<Transform>().Length}, " +
                      $"web verisi: {(GameData.WebDataLoaded ? "yüklendi" : "YOK")} ({GameData.Questions.Count} soru)");
        }

        /// <summary>
        /// Yönlü ışık (alçak açılı akşam/ay ışığı), üç tonlu ortam ışığı, açık çelik mavisi prosedürel gökyüzü ve
        /// ufka karışan gri-mavi sis. Görsellerdeki yağmurlu havayı korur ama sahne okunabilir şekilde aydınlıktır.
        /// </summary>
        static void SetupLighting(Transform parent)
        {
            // Sahnede hazır gelen yönlü ışıkları kapat (çift güneş olmasın)
            foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
                foreach (var l in go.GetComponentsInChildren<Light>())
                    if (l.type == LightType.Directional) l.gameObject.SetActive(false);

            var sunGo = new GameObject("Güneş (Akşam Işığı)");
            sunGo.transform.SetParent(parent, false);
            sunGo.transform.rotation = Quaternion.Euler(32f, -38f, 0f);
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(0.82f, 0.86f, 0.96f);
            sun.intensity = 1.25f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.7f;
            sun.shadowBias = 0.04f;
            sun.cullingMask = ~(1 << InteriorGenerator.InteriorLayer); // İç mekanlara güneş girmez
            RenderSettings.sun = sun;
            Sun = sun;

            // Gökyüzü: prosedürel skybox (bulutlu akşam tonu); shader yoksa açık renk düz zemin
            var skyShader = Shader.Find("Skybox/Procedural");
            if (skyShader != null)
            {
                var sky = new Material(skyShader) { name = "M_GizemliGökyüzü" };
                sky.SetFloat("_SunDisk", 1f);
                sky.SetFloat("_SunSize", 0.025f);
                sky.SetFloat("_SunSizeConvergence", 6f);
                sky.SetFloat("_AtmosphereThickness", 1.15f);
                sky.SetColor("_SkyTint", SkyTint);
                sky.SetColor("_GroundColor", HorizonFog); // ufkun altı sisle aynı renk — kuş bakışında siyah bant olmasın
                sky.SetFloat("_Exposure", 0.85f);
                RenderSettings.skybox = sky;
            }
            else RenderSettings.skybox = null;

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = AmbientSky;
            RenderSettings.ambientEquatorColor = AmbientEquator;
            RenderSettings.ambientGroundColor = AmbientGround;
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.reflectionIntensity = 0.6f;

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = OutdoorFog;
            RenderSettings.fogColor = HorizonFog;
        }

        public static float CurrentOutdoorFog { get; private set; } = OutdoorFog;

        /// <summary>Kasabaya özgü atmosfer: Gizemli (çelik mavisi yağmurlu gece), Gölgeşehir (koyu yeşil-gri orman sisi), Sisören (yeşilimsi dağ sisi).</summary>
        static ColorAdjustments Grade;
        static ShadowsMidtonesHighlights Smh;

        public static void ApplyAtmosphere(TownData t)
        {
            if (t == null) return;
            // Kasaba teması: renk düzeni (mavi / sarı / yeşil)
            if (Grade != null)
            {
                Grade.colorFilter.Override(t.GradeFilter);
                Grade.saturation.Override(t.GradeSaturation);
                BaseExposure = t.GradeExposure;
                Grade.postExposure.Override(BaseExposure + GameSettings.Brightness);
                Grade.contrast.Override(t.GradeContrast);
                DetectiveVision.Instance?.RebaseGrade();
            }
            if (Smh != null) { Smh.shadows.Override(t.GradeShadows); Smh.highlights.Override(t.GradeHighlights); }
            AudioManager.Instance?.SetTownAmbience(t);
            if (Sun != null)
            {
                Sun.transform.rotation = Quaternion.Euler(t.SunPitch, t.SunYaw, 0f);
                Sun.color = t.SunColor;
                Sun.intensity = t.SunIntensity;
            }
            if (RenderSettings.skybox != null)
            {
                RenderSettings.skybox.SetColor("_SkyTint", t.SkyTint);
                RenderSettings.skybox.SetColor("_GroundColor", t.Fog);
            }
            RenderSettings.ambientSkyColor = t.AmbientSky;
            RenderSettings.ambientEquatorColor = t.AmbientEquator;
            RenderSettings.fogColor = t.Fog;
            RenderSettings.fogDensity = CurrentOutdoorFog = t.FogDensity;
            if (Camera.main != null) Camera.main.backgroundColor = t.Fog;
        }

        /// <summary>3D bölge haritası: gaz lambası sıcaklığında, hafif soğuk gölgeli nötr renk düzeni.</summary>
        public static void ApplyMapAtmosphere()
        {
            if (Grade != null)
            {
                Grade.colorFilter.Override(new Color(1f, 0.96f, 0.9f));
                Grade.saturation.Override(-6f);
                BaseExposure = 0.85f;
                Grade.postExposure.Override(BaseExposure + GameSettings.Brightness * 0.6f);
                Grade.contrast.Override(14f);
            }
            if (Smh != null)
            {
                Smh.shadows.Override(new Vector4(0.92f, 0.96f, 1.08f, -0.02f));
                Smh.highlights.Override(new Vector4(1.06f, 1.0f, 0.9f, 0f));
            }
        }

        /// <summary>Sahnenin kendi pozlaması (kasaba / harita); oyuncunun parlaklık ayarı bunun üstüne eklenir.</summary>
        public static float BaseExposure = 0.9f;
        static FilmGrain Grain;

        /// <summary>Ayarlar değişince: parlaklık ve film greni.</summary>
        public static void RefreshPost()
        {
            var map = WorldMapDiorama.Instance;
            bool onMap = map != null && map.Active;
            if (Grade != null) Grade.postExposure.Override(BaseExposure + GameSettings.Brightness * (onMap ? 0.6f : 1f));
            if (Grain != null) Grain.active = GameSettings.FilmGrain;
        }

        static Camera SetupCamera()
        {
            Camera cam = Camera.main;
            foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
                foreach (var c in go.GetComponentsInChildren<Camera>())
                    if (c != cam) c.gameObject.SetActive(false);

            if (cam == null)
            {
                var go = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
            }
            cam.clearFlags = RenderSettings.skybox != null ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
            cam.backgroundColor = HorizonFog;
            cam.nearClipPlane = 0.08f;
            cam.farClipPlane = 650f;
            cam.fieldOfView = 58f;

            var data = cam.GetUniversalAdditionalCameraData();
            if (data != null)
            {
                data.renderPostProcessing = true;
                data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                data.antialiasingQuality = AntialiasingQuality.High;
                data.renderShadows = true;
            }
            return cam;
        }

        /// <summary>Neon ve lambaların parlaması için Bloom, noir hissi için hafif vinyet / soğuk renk filtresi / film greni.</summary>
        static void SetupPostProcessing(Transform parent)
        {
            var go = new GameObject("NoirPostProcess");
            go.transform.SetParent(parent, false);
            var volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 50;
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            volume.sharedProfile = profile;

            var bloom = profile.Add<Bloom>(true);
            bloom.threshold.Override(1.0f);
            bloom.intensity.Override(0.8f);
            bloom.scatter.Override(0.6f);
            bloom.highQualityFiltering.Override(true);

            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.26f);
            vignette.smoothness.Override(0.5f);
            DetectiveVision.Vignette = vignette;

            // Sinematik alan derinliği: netlik CinematicDirector tarafından her karede hedefe ayarlanır
            var dof = profile.Add<DepthOfField>(true);
            dof.mode.Override(DepthOfFieldMode.Bokeh);
            dof.focusDistance.Override(6f);
            dof.aperture.Override(5.6f);
            dof.active = false;   // yalnızca sinematik yakın planlarda açılır (oynanışta görüntü net)
            dof.focalLength.Override(55f);
            Dof = dof;
            CinematicDirector.Dof = dof;

            var tone = profile.Add<Tonemapping>(true);
            tone.mode.Override(TonemappingMode.ACES);

            var color = profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(0.9f);
            color.contrast.Override(12f);
            color.saturation.Override(-10f);
            color.colorFilter.Override(new Color(1f, 0.95f, 0.88f)); // eski film sıcaklığı
            DetectiveVision.Color = color;
            Grade = color;

            var grain = profile.Add<FilmGrain>(true);
            grain.type.Override(FilmGrainLookup.Medium3);
            grain.intensity.Override(0.12f);
            grain.response.Override(0.85f);
            Grain = grain;

            var chroma = profile.Add<ChromaticAberration>(true);
            chroma.intensity.Override(0f);
            chroma.active = false;

            var smh = profile.Add<ShadowsMidtonesHighlights>(true);
            smh.shadows.Override(new Vector4(0.9f, 0.95f, 1.1f, -0.05f));
            smh.highlights.Override(new Vector4(1.1f, 1.0f, 0.85f, 0f));
            Smh = smh;
        }
    }
}
