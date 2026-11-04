using Dedektif3D.Gameplay;
using UnityEngine;

namespace Dedektif3D.Core
{
    /// <summary>
    /// index.html'deki ses öğelerinin (rain-sound, thunder-sound, typewriter-sound, door-creak, door-close, gavel-sound)
    /// Unity karşılığı. Tüm sesler kodla sentezlenir — hiçbir ses dosyası gerekmez.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        public bool Muted { get; private set; }

        AudioSource _rain, _rainIndoor, _sfx, _type;
        AudioClip _rainClip, _thunder, _click, _creak, _knock, _gavel, _ding, _bell, _heart, _shimmer, _stepStone, _stepWood;
        const int Rate = 44100;

        public static AudioManager Create(Transform parent)
        {
            var go = new GameObject("SesYöneticisi");
            go.transform.SetParent(parent, false);
            return go.AddComponent<AudioManager>();
        }

        void Awake()
        {
            Instance = this;
            _rainClip = MakeRain(4f);
            _thunder = MakeThunder(4.5f);
            _click = MakeClick();
            _bell = MakeBell(4.5f);
            _heart = MakeHeartbeat();
            _shimmer = MakeShimmer();
            _stepStone = MakeStep(false);
            _stepWood = MakeStep(true);
            _creak = MakeCreak();
            _knock = MakeKnock(0.18f, 90f);
            _gavel = MakeKnock(0.35f, 140f);
            _ding = MakeDing();
            _rain = Src(true, 0.45f); _rain.clip = _rainClip;
            _rainIndoor = Src(true, 0f); _rainIndoor.clip = _rainClip; _rainIndoor.pitch = 0.6f;
            _sfx = Src(false, 0.8f);
            _type = Src(false, 0.35f);
            _thunderSrc = Src(false, 0.9f); _thunderSrc.clip = _thunder;
            _thunderLow = Src(false, 0.7f); _thunderLow.clip = _thunder; _thunderLow.pitch = 0.72f;
            // Kara film müziği (koddan sentez): ana tema + sorgu gerilimi katmanı + sahne vuruşu
            _scoreClip = MakeNoirScore(32f);
            _tensionClip = MakeTension(8f);
            _sting = MakeSting();
            _score = Src(true, 0f); _score.clip = _scoreClip;
            _tension = Src(true, 0f); _tension.clip = _tensionClip;
            _score.Play(); _tension.Play();
            ApplyVolumes();
        }

        // =================================================================
        //  AYARLAR + MÜZİK DURUMU
        // =================================================================
        AudioSource _thunderSrc, _thunderLow, _score, _tension;
        AudioClip _scoreClip, _tensionClip, _sting;
        public enum Mood { Silent, Menu, Investigation, Tension, Finale }
        Mood _mood = Mood.Menu;
        float _scoreVol, _tensionVol;

        /// <summary>Ayarlar menüsündeki kanal seviyelerini kaynaklara uygular.</summary>
        public void ApplyVolumes()
        {
            float fx = GameSettings.Effects;
            if (_sfx != null) _sfx.volume = 0.8f * fx;
            if (_type != null) _type.volume = 0.35f * fx;
            if (_thunderSrc != null) _thunderSrc.volume = 0.9f * fx;
            if (_thunderLow != null) _thunderLow.volume = 0.7f * fx;
            SetRainLevel(_rainK);
            SetIndoor(_indoor);
        }

        /// <summary>Sahneye göre müzik: menüde tema, soruşturmada alçak, sorguda gerilim, finalde tam.</summary>
        public void SetMood(Mood m) => _mood = m;

        void LateUpdate()
        {
            if (_score == null) return;
            float music = GameSettings.Music;
            float wantScore = _mood == Mood.Menu ? 0.55f : _mood == Mood.Investigation ? 0.3f : _mood == Mood.Tension ? 0.2f : _mood == Mood.Finale ? 0.65f : 0f;
            float wantTension = _mood == Mood.Tension ? 0.32f : 0f;
            _scoreVol = Mathf.MoveTowards(_scoreVol, wantScore * music, Time.unscaledDeltaTime * 0.25f);
            _tensionVol = Mathf.MoveTowards(_tensionVol, wantTension * music, Time.unscaledDeltaTime * 0.35f);
            _score.volume = _scoreVol;
            _tension.volume = _tensionVol;
        }

        /// <summary>Film vuruşu: bölüm kartı, delil bulma ve suçlama anlarında alçak, yankılı bir akor.</summary>
        public void Sting(float volume = 0.8f) { if (!Muted && _sting != null) _sfx.PlayOneShot(_sting, volume); }

        AudioSource Src(bool loop, float vol)
        {
            var s = gameObject.AddComponent<AudioSource>();
            s.loop = loop; s.volume = vol; s.playOnAwake = false; s.spatialBlend = 0f;
            return s;
        }

        public void StartRain() { if (!_rain.isPlaying) _rain.Play(); if (!_rainIndoor.isPlaying) _rainIndoor.Play(); }
        /// <summary>İç mekanda yağmur boğuk duyulur.</summary>
        public void SetIndoor(bool indoor)
        {
            _indoor = indoor;
            bool raining = _rainK > 0.001f;
            float amb = GameSettings.Ambience;
            _rain.volume = indoor || !raining ? 0f : Mathf.Lerp(0.12f, 0.5f, _rainK) * amb;
            _rainIndoor.volume = indoor && raining ? 0.25f * amb : 0f;
            if (_wind != null) _wind.volume = Muted ? 0f : _windVol * (indoor ? 0.35f : 1f) * amb;
        }
        /// <summary>Gök gürültüsü. shake=false: kamera sarsılmaz (bölge haritası). loud: alçak ikinci katmanla gür, derin gümbürtü.</summary>
        public void Thunder(float delay = 0f, bool shake = true, bool loud = false)
        {
            if (shake)
            {
                // 4D: gök gürültüsü yere ve ele işler
                CameraFx.Shake(0.07f, 1.4f, delay + 0.25f);
                CameraFx.Rumble(0.7f, 0.15f, 0.9f, delay + 0.25f);
            }
            if (Muted) return;
            _thunderSrc.pitch = Random.Range(0.92f, 1.06f);
            _thunderSrc.PlayDelayed(delay);
            if (loud) _thunderLow.PlayDelayed(delay + 0.05f);
        }
        /// <summary>Ayak sesi: dışarıda ıslak parke taşı, içeride gıcırdayan ahşap döşeme.</summary>
        public void Footstep(bool wood, float volume = 0.35f)
        {
            if (Muted) return;
            _type.pitch = Random.Range(0.85f, 1.12f);
            _type.PlayOneShot(wood ? _stepWood : _stepStone, volume);
        }
        bool _indoor;
        public void SetRainLevel(float k)
        {
            _rainK = k;
            float amb = GameSettings.Ambience;
            if (_rain != null && !_indoor) _rain.volume = k <= 0.001f ? 0f : Mathf.Lerp(0.12f, 0.5f, k) * amb;
            if (_rainIndoor != null && _indoor) _rainIndoor.volume = k <= 0.001f ? 0f : 0.25f * amb;
        }
        float _rainK = 0.5f;

        // ---- Kasaba ambiyansı: rüzgâr uğultusu + uzak uluma (Sisören), yağmur seviyesi DayNightCycle'dan ----
        AudioSource _wind; AudioClip _windClip, _howl; float _windVol, _nextHowl;
        public void SetTownAmbience(TownData t)
        {
            if (_wind == null)
            {
                _windClip = MakeWind(12f); _howl = MakeHowl(3.2f);
                _wind = gameObject.AddComponent<AudioSource>(); _wind.clip = _windClip; _wind.loop = true; _wind.playOnAwake = false; _wind.spatialBlend = 0f;
            }
            _windVol = t != null ? t.Wind : 0f;
            _wind.volume = Muted ? 0f : _windVol * (_indoor ? 0.35f : 1f) * GameSettings.Ambience;
            if (_windVol > 0f && !_wind.isPlaying) _wind.Play(); else if (_windVol <= 0f) _wind.Stop();
            _nextHowl = Time.time + Random.Range(25f, 45f);
        }

        void Update()
        {
            if (_wind == null || _windVol < 0.3f || Muted) return;
            if (Time.time >= _nextHowl)
            {
                _nextHowl = Time.time + Random.Range(45f, 90f);
                _sfx.PlayOneShot(_howl, 0.18f);
            }
        }

        static AudioClip MakeWind(float seconds)
        {
            int n = (int)(Rate * seconds); var d = new float[n];
            float b = 0f, lp = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                b += (Random.value * 2f - 1f) * 0.02f; b *= 0.998f;            // kahverengi gürültü
                float gust = 0.55f + 0.45f * Mathf.Sin(t * 0.7f) * Mathf.Sin(t * 0.23f + 1.3f);
                lp += (b - lp) * (0.02f + 0.03f * gust);
                d[i] = lp * 6f * gust;
            }
            int fade = Rate / 2;                                              // döngü dikişi
            for (int i = 0; i < fade; i++) { float k = i / (float)fade; d[i] = d[i] * k + d[n - fade + i] * (1f - k); }
            var o = new float[n - fade]; System.Array.Copy(d, o, n - fade);
            var c = AudioClip.Create("Rüzgâr", n - fade, 1, Rate, false); c.SetData(o, 0); return c;
        }

        static AudioClip MakeHowl(float seconds)
        {
            int n = (int)(Rate * seconds); var d = new float[n]; float ph = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate, k = t / seconds;
                float f = Mathf.Lerp(380f, 560f, Mathf.Sin(k * Mathf.PI)) + Mathf.Sin(t * 30f) * 6f;
                ph += f / Rate * Mathf.PI * 2f;
                float env = Mathf.Sin(Mathf.Clamp01(k) * Mathf.PI); env *= env;
                d[i] = (Mathf.Sin(ph) * 0.6f + Mathf.Sin(ph * 2f) * 0.15f) * env * 0.5f;
            }
            var c = AudioClip.Create("UzakUluma", n, 1, Rate, false); c.SetData(d, 0); return c;
        }
        public void TypeClick() { if (!Muted) { _type.pitch = Random.Range(0.85f, 1.2f); _type.PlayOneShot(_click, 0.5f); } }
        public void DoorOpen() { CameraFx.Shake(0.015f, 0.35f); if (!Muted) { _sfx.PlayOneShot(_creak, 0.7f); _sfx.PlayOneShot(_knock, 0.4f); } }
        public void DoorClose() { CameraFx.Shake(0.035f, 0.3f); CameraFx.Rumble(0.35f, 0.5f, 0.15f); if (!Muted) _sfx.PlayOneShot(_knock, 0.8f); }
        public void Ding() { CameraFx.Rumble(0.1f, 0.45f, 0.12f); if (!Muted) _sfx.PlayOneShot(_ding, 0.5f); }
        /// <summary>Saat kulesi çanı: her saat başı (6/12/18/24'te üç vuruş).</summary>
        public void Bell(int hour)
        {
            if (Muted || _bell == null) return;
            int strikes = hour % 6 == 0 ? 3 : 1;
            for (int i = 0; i < strikes; i++) Invoke(nameof(BellOnce), i * 1.6f);
        }
        void BellOnce() { if (!Muted) _sfx.PlayOneShot(_bell, 0.35f); }
        /// <summary>Dedektif Gözü: kalp atışı + sezgi parıltısı.</summary>
        public void Heartbeat()
        {
            CameraFx.Shake(0.008f, 0.25f);
            CameraFx.Rumble(0.45f, 0f, 0.14f);
            if (!Muted) _sfx.PlayOneShot(_heart, 0.8f);
        }
        public void Shimmer() { if (!Muted) _sfx.PlayOneShot(_shimmer, 0.45f); }
        /// <summary>playJudgeGavelTripleStrike — hakim tokmağı üç vuruş.</summary>
        public void GavelTriple() { if (Muted) return; for (int i = 0; i < 3; i++) Invoke(nameof(GavelOnce), i * 0.38f); }
        void GavelOnce() => _sfx.PlayOneShot(_gavel, 1f);

        public void ToggleMute()
        { CameraFx.Shake(0.05f, 0.25f); CameraFx.Rumble(0.8f, 0.6f, 0.12f);
            Muted = !Muted;
            AudioListener.volume = Muted ? 0f : GameSettings.Master;
        }

        // ------------------------------------------------------------ müzik sentezi
        const int MusicRate = 22050;

        static float Voice(float t, float f)
        {
            // Yumuşak yaylı sesi: hafif akortsuz iki osilatör + az armonik (koro etkisi)
            float a = 2f * Mathf.PI * f * t, b = 2f * Mathf.PI * f * 1.0045f * t;
            return Mathf.Sin(a) + 0.22f * Mathf.Sin(2f * a) + 0.08f * Mathf.Sin(3f * a) + 0.75f * Mathf.Sin(b);
        }

        /// <summary>Kara film teması: re minör → si bemol → sol minör → la akorları, nefes alan dinamik, uzak yankılı piyano.</summary>
        static AudioClip MakeNoirScore(float seconds)
        {
            int n = (int)(MusicRate * seconds); var d = new float[n];
            float[][] chords =
            {
                new[] { 73.42f, 110f, 146.83f, 174.61f },    // Dm
                new[] { 58.27f, 116.54f, 146.83f, 174.61f }, // Bb
                new[] { 98f, 116.54f, 146.83f, 196f },       // Gm
                new[] { 55f, 110f, 138.59f, 164.81f },       // A
            };
            float chordLen = seconds / chords.Length;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)MusicRate;
                int ci = Mathf.Min(chords.Length - 1, (int)(t / chordLen));
                float local = t - ci * chordLen;
                float xf = Mathf.Clamp01((local - (chordLen - 2f)) / 2f);
                var a = chords[ci]; var b = chords[(ci + 1) % chords.Length];
                float s = 0f;
                for (int k = 0; k < 4; k++)
                {
                    float va = Voice(t, a[k]) * (k == 0 ? 1.2f : 0.8f);
                    s += xf > 0f ? Mathf.Lerp(va, Voice(t, b[k]) * (k == 0 ? 1.2f : 0.8f), xf) : va;
                }
                float breath = 0.7f + 0.3f * Mathf.Sin(t * 2f * Mathf.PI / 8f);
                float attack = Mathf.Clamp01(local / 1.2f) * 0.35f + 0.65f;
                d[i] = s * 0.045f * breath * attack;
            }
            // Uzak piyano: re minör pentatonik, yankılı
            float[] piano = { 293.66f, 349.23f, 392f, 440f, 523.25f, 587.33f, 220f };
            var rng = new System.Random(1887);
            for (float t0 = 2f; t0 < seconds - 3f; t0 += 2.4f + (float)rng.NextDouble() * 2.8f)
            {
                float f = piano[rng.Next(piano.Length)];
                int start = (int)(t0 * MusicRate), len = (int)(MusicRate * 3.2f), echo = (int)(MusicRate * 0.45f);
                for (int j = 0; j < len && start + j < n; j++)
                {
                    float tt = j / (float)MusicRate;
                    float env = Mathf.Exp(-tt * 1.8f) * Mathf.Min(1f, tt * 300f);
                    float ph = 2f * Mathf.PI * f * tt;
                    float v = (Mathf.Sin(ph) + 0.4f * Mathf.Sin(2f * ph) + 0.12f * Mathf.Sin(3f * ph)) * env * 0.055f;
                    d[start + j] += v;
                    if (start + j + echo < n) d[start + j + echo] += v * 0.32f;
                    if (start + j + echo * 2 < n) d[start + j + echo * 2] += v * 0.12f;
                }
            }
            return LoopClip("KaraFilmTeması", d, MusicRate);
        }

        /// <summary>Sorgu gerilimi: yüksek, uyumsuz yaylı tremolosu + derin nabız.</summary>
        static AudioClip MakeTension(float seconds)
        {
            int n = (int)(MusicRate * seconds); var d = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)MusicRate;
                float trem = 0.55f + 0.45f * Mathf.Sin(t * 2f * Mathf.PI * 7f);
                float cluster = Mathf.Sin(2f * Mathf.PI * 587.33f * t) + Mathf.Sin(2f * Mathf.PI * 622.25f * t) * 0.8f + Mathf.Sin(2f * Mathf.PI * 880f * t) * 0.5f;
                float beat = Mathf.Repeat(t, 1.1f);
                float pulse = Mathf.Sin(2f * Mathf.PI * 52f * t) * Mathf.Exp(-beat * 9f) + Mathf.Sin(2f * Mathf.PI * 50f * t) * Mathf.Exp(-Mathf.Repeat(t - 0.28f, 1.1f) * 11f) * 0.7f;
                d[i] = cluster * trem * 0.018f + pulse * 0.16f;
            }
            return LoopClip("SorguGerilimi", d, MusicRate);
        }

        /// <summary>Sahne vuruşu: derin davul + karanlık akor, uzun kuyruk.</summary>
        static AudioClip MakeSting()
        {
            int n = (int)(MusicRate * 3.5f); var d = new float[n];
            float[] chord = { 73.42f, 87.31f, 110f, 146.83f, 155.56f };
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)MusicRate;
                float boom = Mathf.Sin(2f * Mathf.PI * (48f - t * 6f) * t) * Mathf.Exp(-t * 3.2f);
                float pad = 0f;
                foreach (var f in chord) pad += Mathf.Sin(2f * Mathf.PI * f * t);
                pad *= Mathf.Exp(-t * 0.9f) * Mathf.Min(1f, t * 40f) * 0.09f;
                d[i] = boom * 0.6f + pad;
            }
            var c = AudioClip.Create("SahneVuruşu", n, 1, MusicRate, false); c.SetData(d, 0); return c;
        }

        static AudioClip LoopClip(string name, float[] d, int rate)
        {
            int n = d.Length, fade = rate * 2;
            for (int i = 0; i < fade; i++) { float k = i / (float)fade; d[i] = d[i] * k + d[n - fade + i] * (1f - k); }
            var o = new float[n - fade]; System.Array.Copy(d, o, n - fade);
            var c = AudioClip.Create(name, n - fade, 1, rate, false); c.SetData(o, 0); return c;
        }

        // ------------------------------------------------------------ sentez
        static AudioClip MakeRain(float seconds)
        {
            int n = (int)(Rate * seconds);
            var d = new float[n];
            float lp = 0f, lp2 = 0f;
            for (int i = 0; i < n; i++)
            {
                float w = Random.value * 2f - 1f;
                lp += (w - lp) * 0.35f;      // yumuşak hışırtı
                lp2 += (w - lp2) * 0.04f;    // alçak uğultu
                float drop = Random.value > 0.9993f ? Random.Range(0.3f, 0.7f) : 0f;
                d[i] = lp * 0.35f + lp2 * 0.5f + drop;
            }
            // Döngü dikişini yumuşat
            int fade = Rate / 10;
            for (int i = 0; i < fade; i++) { float k = i / (float)fade; d[i] = d[i] * k + d[n - fade + i] * (1 - k); }
            var c = AudioClip.Create("Yağmur", n - fade, 1, Rate, false);
            var trimmed = new float[n - fade]; System.Array.Copy(d, trimmed, n - fade);
            c.SetData(trimmed, 0);
            return c;
        }

        static AudioClip MakeThunder(float seconds)
        {
            int n = (int)(Rate * seconds);
            var d = new float[n];
            float lp = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float env = t < 0.05f ? t / 0.05f : Mathf.Exp(-(t - 0.05f) * 1.1f);
                float crackle = t < 0.4f ? (Random.value * 2 - 1) * (0.4f - t) * 1.6f : 0f;
                lp += ((Random.value * 2f - 1f) - lp) * 0.012f;
                d[i] = Mathf.Clamp(lp * 9f * env * (0.7f + 0.3f * Mathf.Sin(t * 7f)) + crackle * 0.4f, -1f, 1f);
            }
            var c = AudioClip.Create("GökGürültüsü", n, 1, Rate, false); c.SetData(d, 0); return c;
        }

        static AudioClip MakeClick()
        {
            int n = Rate / 25; var d = new float[n];
            for (int i = 0; i < n; i++) { float t = i / (float)n; d[i] = (Random.value * 2 - 1) * Mathf.Exp(-t * 18f) * 0.8f + Mathf.Sin(i * 0.9f) * Mathf.Exp(-t * 30f) * 0.4f; }
            var c = AudioClip.Create("Daktilo", n, 1, Rate, false); c.SetData(d, 0); return c;
        }

        static AudioClip MakeCreak()
        {
            int n = (int)(Rate * 0.9f); var d = new float[n]; float ph = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float f = 180f + Mathf.Sin(t * 9f) * 60f + t * 120f;
                ph += f / Rate * Mathf.PI * 2f;
                float saw = (ph / Mathf.PI % 2f) - 1f;
                d[i] = saw * 0.25f * Mathf.Sin(t / 0.9f * Mathf.PI) * (0.6f + 0.4f * Mathf.Sin(t * 70f));
            }
            var c = AudioClip.Create("Gıcırtı", n, 1, Rate, false); c.SetData(d, 0); return c;
        }

        static AudioClip MakeKnock(float len, float freq)
        {
            int n = (int)(Rate * len); var d = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                d[i] = (Mathf.Sin(t * freq * Mathf.PI * 2f) * 0.8f + (Random.value * 2 - 1) * 0.3f) * Mathf.Exp(-t * 22f);
            }
            var c = AudioClip.Create("Vuruş", n, 1, Rate, false); c.SetData(d, 0); return c;
        }

        static AudioClip MakeBell(float seconds)
        {
            int n = (int)(Rate * seconds); var d = new float[n];
            // Bronz çan: ana ton + uyumsuz kısmi harmonikler, uzun sönüm
            float[] f = { 196f, 392f * 1.19f, 196f * 2.76f, 196f * 5.4f }; float[] a = { 0.5f, 0.28f, 0.16f, 0.08f }; float[] k = { 0.9f, 1.4f, 2.2f, 3.5f };
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate, v = 0f;
                for (int j = 0; j < f.Length; j++) v += Mathf.Sin(t * f[j] * Mathf.PI * 2f) * a[j] * Mathf.Exp(-t * k[j]);
                d[i] = v * Mathf.Min(1f, t * 200f) * 0.8f;
            }
            var c = AudioClip.Create("SaatKulesiÇanı", n, 1, Rate, false); c.SetData(d, 0); return c;
        }

        static AudioClip MakeStep(bool wood)
        {
            int n = (int)(Rate * 0.22f); var d = new float[n];
            var rnd = new System.Random(wood ? 7 : 3);
            float lp = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float noise = (float)rnd.NextDouble() * 2f - 1f;
                lp += (noise - lp) * (wood ? 0.18f : 0.45f);
                float body = wood ? Mathf.Sin(t * 140f * Mathf.PI * 2f) * Mathf.Exp(-t * 30f) * 0.6f : 0f;
                float splash = wood ? 0f : noise * 0.25f * Mathf.Exp(-t * 18f);     // ıslak taş: şıpırtı
                d[i] = (lp * Mathf.Exp(-t * (wood ? 22f : 35f)) + body + splash) * 0.9f;
            }
            var c = AudioClip.Create(wood ? "AdımAhşap" : "AdımTaş", n, 1, Rate, false); c.SetData(d, 0); return c;
        }

        static AudioClip MakeHeartbeat()
        {
            int n = (int)(Rate * 1.1f); var d = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float b1 = Mathf.Exp(-Mathf.Pow((t - 0.08f) / 0.035f, 2f)), b2 = Mathf.Exp(-Mathf.Pow((t - 0.36f) / 0.04f, 2f)) * 0.75f;
                d[i] = Mathf.Sin(t * 55f * Mathf.PI * 2f) * (b1 + b2) * 0.9f;
            }
            var c = AudioClip.Create("KalpAtışı", n, 1, Rate, false); c.SetData(d, 0); return c;
        }

        static AudioClip MakeShimmer()
        {
            int n = (int)(Rate * 1.4f); var d = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float v = Mathf.Sin(t * 1568f * Mathf.PI * 2f) * 0.3f + Mathf.Sin(t * 2093f * Mathf.PI * 2f) * 0.2f + Mathf.Sin(t * 2637f * Mathf.PI * 2f) * 0.12f;
                d[i] = v * Mathf.Exp(-t * 2.2f) * Mathf.Min(1f, t * 60f) * (0.7f + 0.3f * Mathf.Sin(t * 38f));
            }
            var c = AudioClip.Create("SezgiParıltısı", n, 1, Rate, false); c.SetData(d, 0); return c;
        }

        static AudioClip MakeDing()
        {
            int n = (int)(Rate * 0.6f); var d = new float[n];
            for (int i = 0; i < n; i++) { float t = i / (float)Rate; d[i] = (Mathf.Sin(t * 880 * Mathf.PI * 2) * 0.5f + Mathf.Sin(t * 1320 * Mathf.PI * 2) * 0.25f) * Mathf.Exp(-t * 6f); }
            var c = AudioClip.Create("Bildirim", n, 1, Rate, false); c.SetData(d, 0); return c;
        }
    }

    /// <summary>Şimşek: rastgele aralıklarla gökyüzü + ana ışık parlar, ardından gök gürültüsü (web: startThunder).</summary>
    public class LightningController : MonoBehaviour
    {
        public Light sun;
        public bool Indoor;
        float _next, _flashEnd, _baseIntensity, _baseAmbient;
        Color _baseSky;

        void Start() { _next = Time.time + Random.Range(12f, 25f); Rebase(); }

        /// <summary>Kasaba atmosferi değişince temel ışık değerlerini yeniden al.</summary>
        public void Rebase()
        {
            _flashEnd = 0f;
            if (sun != null) _baseIntensity = sun.intensity;
            _baseAmbient = RenderSettings.ambientIntensity;
            _baseSky = RenderSettings.ambientSkyColor;
        }

        // Gün/gece döngüsü ışığı her karede Update'te yazar; şimşek LateUpdate'te üstüne parlama ekler.
        void LateUpdate()
        {
            if (sun == null) return;
            // Gök gürültüsü yalnızca fırtınalı kasabalarda (Gölgeşehir); Gizemli Kasaba'da sadece yağmur sesi
            if (GameData.Current != null && !GameData.Current.Thunder) { if (Time.time >= _next) _next = Time.time + 5f; }
            else if (Time.time >= _next)
            {
                _next = Time.time + Random.Range(22f, 45f);
                _flashEnd = Time.time + 0.45f;
                if (AudioManager.Instance != null) AudioManager.Instance.Thunder(Random.Range(0.4f, 1.6f));
            }
            if (Time.time < _flashEnd && !Indoor)
            {
                float k = Mathf.PingPong((_flashEnd - Time.time) * 14f, 1f);
                if (DayNightCycle.Instance != null)
                {
                    sun.intensity += k * 3.5f;
                    RenderSettings.ambientSkyColor = Color.Lerp(RenderSettings.ambientSkyColor, new Color(0.85f, 0.9f, 1f), k * 0.6f);
                }
                else
                {
                    sun.intensity = _baseIntensity + k * 3.5f;
                    RenderSettings.ambientSkyColor = Color.Lerp(_baseSky, new Color(0.85f, 0.9f, 1f), k * 0.6f);
                }
            }
            else if (DayNightCycle.Instance == null)
            {
                sun.intensity = _baseIntensity;
                RenderSettings.ambientSkyColor = _baseSky;
            }
        }
    }
}
