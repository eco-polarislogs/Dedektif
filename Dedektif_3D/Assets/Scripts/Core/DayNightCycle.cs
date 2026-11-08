using System.Collections.Generic;
using Dedektif3D.World;
using UnityEngine;

namespace Dedektif3D.Core
{
    /// <summary>
    /// GÜN HESABI — tam 24 gerçek dakika = 1 oyun günü. Her gerçek dakika günün bir saatidir:
    /// soruşturmanın 6. dakikası sabah 06:00'yı, 18. dakikası akşam 18:00'i temsil eder.
    /// Güneş doğudan doğup batıya iner, gece ay ışığı ve gaz lambaları kasabayı aydınlatır;
    /// gökyüzü, sis, ortam ışığı, pencere ışıkları ve sokaktaki kalabalık saate göre değişir.
    /// Saat yalnızca soruşturma sürerken (GamePhase.Playing) ilerler.
    /// </summary>
    public class DayNightCycle : MonoBehaviour
    {
        public static DayNightCycle Instance { get; private set; }

        public const float RealSecondsPerGameHour = 60f;   // 24 dk = 24 saat
        public const float CaseStartHour = 20f;             // Dosya "yağmurlu bir sonbahar gecesi" açılır
        const float MenuHour = 21.5f;

        /// <summary>0..24 arası oyun saati.</summary>
        public float Hour { get; private set; } = MenuHour;
        public int Day { get; private set; } = 1;
        /// <summary>Soruşturma başından beri geçen gerçek dakika (06 → 06:00 eşlemesi için gösterilir).</summary>
        public float ElapsedMinutes { get; private set; }

        public int HourInt => Mathf.FloorToInt(Hour) % 24;
        public int Minute => Mathf.FloorToInt((Hour - Mathf.Floor(Hour)) * 60f);
        public string Clock => $"{HourInt:00}:{Minute:00}";
        /// <summary>Güneşin ufka göre yüksekliği (-1 gece yarısı, +1 öğle).</summary>
        public float SunElevation => Mathf.Sin((Hour - 6f) / 12f * Mathf.PI);
        public bool IsNight => SunElevation < -0.05f;
        public bool LampsOn => LampFactor > 0.5f;
        public float LampFactor { get; private set; }
        public bool DeepNight => Hour >= 23f || Hour < 5f;

        public string PartOfDay
        {
            get
            {
                float h = Hour;
                if (h >= 5f && h < 7f) return "Şafak";
                if (h >= 7f && h < 11f) return "Sabah";
                if (h >= 11f && h < 14f) return "Öğle";
                if (h >= 14f && h < 17f) return "İkindi";
                if (h >= 17f && h < 20f) return "Akşam";
                if (h >= 20f && h < 23.5f) return "Gece";
                return "Gece Yarısı";
            }
        }

        Light _sun;
        Transform _townRoot;
        readonly List<Light> _lamps = new List<Light>();
        readonly Dictionary<Light, float> _lampBase = new Dictionary<Light, float>();
        readonly List<(Material mat, Color baseEmission)> _glowMats = new List<(Material, Color)>();
        static readonly Dictionary<Material, Color> GlowBase = new Dictionary<Material, Color>(); // ilk (tam parlak) emisyon
        Material _interiorWindow;
        int _lastHourInt = -1;
        float _crowdTimer;

        static readonly string[] GlowMaterialKeys = { "gaslamp_glass", "warm_window", "warm_window_1", "warm_window_2", "lampglow_warm", "lantern_rural" };

        public static DayNightCycle Create(GameObject host, Light sun)
        {
            var c = host.AddComponent<DayNightCycle>();
            c._sun = sun;
            Instance = c;
            return c;
        }

        /// <summary>Yeni dava: saat 20:00'den başlar.</summary>
        public void ResetDay(float hour = CaseStartHour)
        {
            Hour = hour; Day = 1; ElapsedMinutes = 0f; _lastHourInt = HourInt;
            Apply();
        }

        /// <summary>Kayıttan devam: saati, günü ve geçen süreyi geri yükler.</summary>
        public void RestoreTime(float hour, int day, float elapsedMinutes)
        {
            Hour = Mathf.Repeat(hour, 24f); Day = Mathf.Max(1, day); ElapsedMinutes = Mathf.Max(0f, elapsedMinutes); _lastHourInt = HourInt;
            Apply();
        }

        /// <summary>Menülerde (splash / bölge haritası) gece manzarası.</summary>
        public void MenuTime() { Hour = MenuHour; Apply(); }

        /// <summary>Kasaba değişince sokak lambalarını ve ışıklı malzemeleri yeniden topla.</summary>
        public void Rescan(Transform townRoot)
        {
            _townRoot = townRoot;
            _lamps.Clear(); _lampBase.Clear(); _glowMats.Clear();
            if (townRoot != null)
                foreach (var l in townRoot.GetComponentsInChildren<Light>(true))
                {
                    if (l.type == LightType.Directional) continue;
                    if (InInterior(l.transform)) continue;   // iç mekan ışıkları (lamba, sezgi pırıltısı, delil ışığı) hep yanar
                    if (l.name == "DelilIşığı") continue;
                    _lamps.Add(l); _lampBase[l] = l.intensity;
                }
            foreach (var key in GlowMaterialKeys)
            {
                var m = MaterialLibrary.Cached(key);
                if (m == null || !m.HasProperty("_EmissionColor")) continue;
                if (!GlowBase.TryGetValue(m, out var baseEm)) GlowBase[m] = baseEm = m.GetColor("_EmissionColor");
                _glowMats.Add((m, baseEm));
            }
            _interiorWindow = MaterialLibrary.Cached("interior_window");
            Apply();
        }

        static bool InInterior(Transform t)
        {
            for (var p = t; p != null; p = p.parent)
                if (p.gameObject.layer == InteriorGenerator.InteriorLayer) return true;
            return false;
        }

        /// <summary>true iken (3D bölge haritası açık) ışık / sis / gök kasabaya göre güncellenmez.</summary>
        public bool Suspended { get; set; }

        void Update()
        {
            if (Suspended) return;
            var gm = GameManager.Instance;
            if (gm != null && gm.Phase == GamePhase.Playing)
            {
                float dh = Time.deltaTime / RealSecondsPerGameHour;
                ElapsedMinutes += Time.deltaTime / 60f;
                Hour += dh;
                if (Hour >= 24f) { Hour -= 24f; Day++; }
                if (HourInt != _lastHourInt)
                {
                    _lastHourInt = HourInt;
                    OnHourChanged(_lastHourInt);
                }
            }
            Apply();
            _crowdTimer -= Time.deltaTime;
            if (_crowdTimer <= 0f) { _crowdTimer = 2f; UpdateCrowd(); }
        }

        void OnHourChanged(int h)
        {
            AudioManager.Instance?.Bell(h);
            var ui = GameManager.Instance != null ? GameManager.Instance.UI : null;
            if (ui == null) return;
            switch (h)
            {
                case 0: ui.Toast($"Saat 00:00 — Gece yarısı. {Day}. gün başladı; sokaklar boşaldı."); ui.Monologue("Gece yarısı. Kasaba uyuyor... ya da uyuyor gibi yapıyor."); break;
                case 5: ui.Toast("Saat 05:00 — Ufuk ağarıyor. Gaz lambaları birer birer sönecek."); break;
                case 6: ui.Toast("Saat 06:00 — Şafak söktü. Kasaba uyanıyor."); ui.Monologue("Şafak. Bir gece daha geçti ve katil hâlâ aramızda."); break;
                case 12: ui.Toast("Saat 12:00 — Öğle vakti. Çarşı en kalabalık hâlinde."); break;
                case 18: ui.Toast("Saat 18:00 — Akşam iniyor. Lamba yakıcı sokak fenerlerini tutuşturuyor."); break;
                case 21: ui.Toast("Saat 21:00 — Gece. Kepenkler iniyor, sokaklarda yalnızca gölgeler var."); break;
            }
        }

        static float Smooth(float a, float b, float x) { float t = Mathf.Clamp01((x - a) / (b - a)); return t * t * (3f - 2f * t); }

        void Apply()
        {
            var town = GameData.Current;
            if (town == null) return;
            float e = SunElevation;
            float dayK = Smooth(-0.15f, 0.3f, e);
            float twilight = Mathf.Clamp01(1f - Mathf.Abs(e) / 0.28f);         // ufuk çevresinde 1
            LampFactor = 1f - Smooth(-0.04f, 0.22f, e);

            // ---- Güneş / Ay ----
            if (_sun != null)
            {
                float hourAngle = (Hour - 12f) / 12f * 180f;                    // 06:00 → -90 (doğu), 18:00 → +90 (batı)
                if (e > 0f)
                {
                    float pitch = Mathf.Lerp(4f, 58f, e);
                    _sun.transform.rotation = Quaternion.Euler(pitch, town.SunYaw + hourAngle, 0f);
                    _sun.intensity = Mathf.Lerp(0.05f, 1.7f, Smooth(0f, 0.35f, e)) * town.DayLight;
                    _sun.color = Color.Lerp(new Color(1f, 0.6f, 0.36f), new Color(0.86f, 0.89f, 0.94f), Smooth(0.05f, 0.45f, e));
                    _sun.shadowStrength = 0.75f;
                }
                else
                {
                    float m = -e;
                    float nightPhase = Mathf.Repeat(Hour - 18f, 24f) / 12f;         // 18:00 → 0, 06:00 → 1
                    _sun.transform.rotation = Quaternion.Euler(Mathf.Lerp(18f, 38f, m), town.SunYaw + 180f + (nightPhase - 0.5f) * 140f, 0f);
                    _sun.intensity = Mathf.Lerp(0.08f, Mathf.Max(0.6f, town.SunIntensity * 1.4f), Smooth(0f, 0.3f, m));
                    _sun.color = new Color(0.56f, 0.66f, 0.92f);                    // soğuk ay ışığı
                    _sun.shadowStrength = 0.55f;
                }
            }

            // ---- Ortam ışığı ----
            var neutral = Color.Lerp(new Color(0.74f, 0.76f, 0.8f), town.DayFog * 1.1f, 0.45f) * Mathf.Lerp(0.75f, 1f, town.DayLight);
            var daySky = Color.Lerp(town.AmbientSky, neutral, 0.85f);
            var dayEq = Color.Lerp(town.AmbientEquator, neutral * 0.7f, 0.8f);
            var nightSky = town.AmbientSky * 1.85f;    // gece de sokaklar okunur kalsın (ay + sis yansıması)
            var nightEq = town.AmbientEquator * 1.8f;
            var sky = Color.Lerp(nightSky, daySky, dayK);
            sky = Color.Lerp(sky, new Color(0.62f, 0.45f, 0.38f), twilight * 0.22f);   // gün batımı kızıllığı
            RenderSettings.ambientSkyColor = sky;
            RenderSettings.ambientEquatorColor = Color.Lerp(nightEq, dayEq, dayK);

            // ---- Sis + gök ----
            var fogDay = Color.Lerp(town.Fog, town.DayFog, 0.85f);
            var fog = Color.Lerp(town.Fog, fogDay, dayK);
            fog = Color.Lerp(fog, new Color(0.45f, 0.33f, 0.3f), twilight * 0.25f);
            RenderSettings.fogColor = fog;
            var gm = GameManager.Instance;
            bool indoor = gm != null && gm.InsideBuilding;
            if (!indoor) RenderSettings.fogDensity = town.FogDensity * Mathf.Lerp(0.7f, town.DayFogFactor, dayK);
            if (RenderSettings.skybox != null)
            {
                var tint = Color.Lerp(town.SkyTint, Color.Lerp(new Color(0.5f, 0.55f, 0.62f), town.DayFog, 0.5f), dayK);
                tint = Color.Lerp(tint, new Color(0.62f, 0.38f, 0.28f), twilight * 0.45f);
                RenderSettings.skybox.SetColor("_SkyTint", tint);
                RenderSettings.skybox.SetColor("_GroundColor", fog);
                RenderSettings.skybox.SetFloat("_Exposure", Mathf.Lerp(0.32f, 1.35f, dayK));
            }
            if (Camera.main != null) Camera.main.backgroundColor = fog;

            // ---- Hava simülasyonu: yağmur saatlere göre çiseler, sağanağa döner, gece ağırlaşır ----
            float wave = Mathf.Clamp01(Mathf.PerlinNoise(Hour * 0.33f + Day * 7.1f, 3.7f) * 0.85f + (1f - dayK) * 0.2f);
            ApplyRain(town.RainMax <= 0.02f ? 0f : Mathf.Lerp(town.RainMin, town.RainMax, wave));

            // ---- Gaz lambaları + pencereler ----
            foreach (var l in _lamps)
            {
                if (l == null) continue;
                var flick = l.GetComponent<LightFlicker>();
                if (flick != null) flick.Multiplier = LampFactor;
                else if (_lampBase.TryGetValue(l, out var b)) l.intensity = b * LampFactor;
                l.enabled = LampFactor > 0.02f;
            }
            float glow = Mathf.Lerp(0.12f, 1f, LampFactor);
            foreach (var (mat, baseEm) in _glowMats) if (mat != null) mat.SetColor("_EmissionColor", baseEm * glow);
            if (_interiorWindow != null && _interiorWindow.HasProperty("_EmissionColor"))
            {
                // İç mekan pencereleri: gündüz soluk gri gün ışığı, gece lacivert karanlık
                var win = Color.Lerp(new Color(0.05f, 0.07f, 0.12f), new Color(0.75f, 0.8f, 0.86f), dayK);
                win = Color.Lerp(win, new Color(0.9f, 0.5f, 0.3f), twilight * 0.4f);
                _interiorWindow.SetColor("_EmissionColor", win * Mathf.Lerp(0.6f, 1.6f, dayK));
            }
        }

        public float RainLevel { get; private set; } = 1f;
        GameObject _rainGo; ParticleSystem[] _rainPs; float[] _rainBase;

        void ApplyRain(float k)
        {
            RainLevel = k;
            var go = GameManager.Instance != null ? GameManager.Instance.RainObject : null;
            if (go == null) return;
            if (go != _rainGo)
            {
                _rainGo = go;
                _rainPs = go.GetComponentsInChildren<ParticleSystem>(true);
                _rainBase = new float[_rainPs.Length];
                for (int i = 0; i < _rainPs.Length; i++) _rainBase[i] = _rainPs[i].emission.rateOverTimeMultiplier;
            }
            for (int i = 0; i < _rainPs.Length; i++)
            {
                var em = _rainPs[i].emission;
                em.rateOverTimeMultiplier = k <= 0.001f ? 0f : _rainBase[i] * Mathf.Lerp(0.15f, 1.1f, k);
            }
            AudioManager.Instance?.SetRainLevel(k);
        }

        /// <summary>Gece yarısından sonra sokaklar boşalır, gündüz çarşı kalabalıklaşır.</summary>
        void UpdateCrowd()
        {
            var all = Townsfolk.All;
            all.RemoveAll(x => x == null);
            for (int i = 0; i < all.Count; i++)
            {
                var t = all[i];
                if (t == null) continue;
                bool visible = DeepNight ? i % 4 == 0 : IsNight ? i % 3 != 2 : true;
                if (t.gameObject.activeSelf != visible) t.gameObject.SetActive(visible);
            }
        }

        void OnDestroy() { if (Instance == this) Instance = null; }
    }

    /// <summary>İç mekan duvar saati: akrep ve yelkovan oyun saatini gösterir.</summary>
    public class WallClock : MonoBehaviour
    {
        public Transform hourHand, minuteHand;
        void Update()
        {
            var dn = DayNightCycle.Instance;
            if (dn == null) return;
            float h = dn.Hour % 12f;
            if (hourHand != null) hourHand.localRotation = Quaternion.Euler(0, 0, -h / 12f * 360f);
            if (minuteHand != null) minuteHand.localRotation = Quaternion.Euler(0, 0, -(dn.Hour - Mathf.Floor(dn.Hour)) * 360f);
        }
    }
}
