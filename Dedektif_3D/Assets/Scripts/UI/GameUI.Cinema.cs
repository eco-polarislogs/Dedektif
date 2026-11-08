using System.Collections.Generic;
using System.Linq;
using Dedektif3D.Core;
using UnityEngine;

namespace Dedektif3D.UI
{
    /// <summary>
    /// Film gibi oynanan, oyuncu odaklı katman:
    ///  • Duraklatma menüsü (Esc) + Ayarlar paneli (parlaklık, ses kanalları, grafik, tam ekran, film efektleri, fare, altyazı hızı)
    ///  • Bölüm kartı: her vaka film jeneriği gibi açılır ("BİR ANADOLU GİZEMİ — BÖLÜM I — GİZEMLİ KASABA")
    ///  • İç ses: dedektifin kara film anlatıcısı gibi düşünceleri (altyazı)
    ///  • Defter: sıradaki adım ve ilerleme (oyuncu ne yapacağını hep bilir)
    ///  • Vaka panosu [B]: şüpheliler, deliller, aralarında kırmızı ip ve otomatik çıkarımlar (Sherlock / Alan Wake panosu gibi)
    ///  • Sinema modu [C]: arayüz gizlenir, film şeritleri iner
    ///  • Final jeneriği: dava kapanınca oyuncu kadrosu film sonu gibi akar
    /// </summary>
    public partial class GameUI
    {
        // =================================================================
        //  STİLLER
        // =================================================================
        bool _cinStyles;
        GUIStyle _setTitle, _setLabel, _setValue, _setSection, _pauseItem, _pauseItemBig, _monoText, _monoSpeaker, _objHead, _objText, _objDetail,
            _chapKicker, _chapNum, _chapTitle, _chapSub, _boardName, _boardRole, _boardNote, _boardHead, _boardDeduce, _creditRole, _creditName, _creditHead;

        void EnsureCinemaStyles()
        {
            if (_cinStyles) return;
            _cinStyles = true;
            GUIStyle L(int size, Color c, FontStyle fs = FontStyle.Normal, TextAnchor a = TextAnchor.MiddleLeft, bool wrap = false, bool mono = false)
            {
                var s = new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = fs, alignment = a, wordWrap = wrap, richText = true };
                var f = mono ? _mono : _serif;
                if (f != null) s.font = f;
                s.normal.textColor = c; s.hover.textColor = c;
                return s;
            }
            var cream = new Color(0.95f, 0.91f, 0.82f);
            _setTitle = L(44, Accent, FontStyle.Bold);
            _setSection = L(15, Accent, FontStyle.Bold);
            _setLabel = L(19, cream);
            _setValue = L(17, TextMuted, FontStyle.Normal, TextAnchor.MiddleRight);
            _pauseItem = L(26, cream, FontStyle.Bold);
            _pauseItemBig = L(30, Accent, FontStyle.Bold);
            _monoText = L(25, new Color(0.96f, 0.93f, 0.86f), FontStyle.Italic, TextAnchor.UpperCenter, true);
            _monoSpeaker = L(13, Accent, FontStyle.Bold, TextAnchor.MiddleCenter);
            _objHead = L(13, Accent, FontStyle.Bold);
            _objText = L(19, cream, FontStyle.Bold, TextAnchor.MiddleLeft, true);
            _objDetail = L(15, new Color(0.78f, 0.74f, 0.66f), FontStyle.Italic, TextAnchor.MiddleLeft, true);
            _chapKicker = L(18, Accent, FontStyle.Bold, TextAnchor.MiddleCenter);
            _chapNum = L(26, new Color(0.8f, 0.76f, 0.68f), FontStyle.Normal, TextAnchor.MiddleCenter);
            _chapTitle = L(92, cream, FontStyle.Bold, TextAnchor.MiddleCenter);
            _chapSub = L(26, new Color(0.84f, 0.8f, 0.72f), FontStyle.Italic, TextAnchor.MiddleCenter);
            _boardName = L(17, new Color(0.15f, 0.1f, 0.06f), FontStyle.Bold, TextAnchor.MiddleCenter, true);
            _boardRole = L(13, new Color(0.35f, 0.27f, 0.18f), FontStyle.Italic, TextAnchor.MiddleCenter, true);
            _boardNote = L(15, new Color(0.18f, 0.12f, 0.07f), FontStyle.Normal, TextAnchor.UpperLeft, true, true);
            _boardHead = L(40, cream, FontStyle.Bold);
            _boardDeduce = L(17, cream, FontStyle.Normal, TextAnchor.UpperLeft, true);
            _creditHead = L(22, Accent, FontStyle.Bold, TextAnchor.MiddleCenter);
            _creditRole = L(17, TextMuted, FontStyle.Italic, TextAnchor.MiddleRight);
            _creditName = L(19, cream, FontStyle.Bold, TextAnchor.MiddleLeft);
        }

        // =================================================================
        //  DURAKLATMA + AYARLAR
        // =================================================================
        bool _paused, _settingsOpen, _boardOpen, _cinemaMode;
        public bool Paused => _paused;

        public void OpenSettings() { _settingsOpen = true; AudioManager.Instance?.TypeClick(); }

        /// <summary>Otomatik ekran görüntüsü aracı için: "pause", "settings", "board" ya da "none".</summary>
        public void DebugOverlay(string what)
        {
            _settingsOpen = what == "settings";
            _boardOpen = what == "board";
            if (what == "pause" || what == "settings") { if (!_paused) OpenPause(); } else if (_paused) ClosePause();
        }
        void CloseSettings() { _settingsOpen = false; GameSettings.Save(); }

        void OpenPause()
        {
            if (_paused) return;
            _paused = true;
            Time.timeScale = 0f;
            AudioManager.Instance?.TypeClick();
        }

        void ClosePause()
        {
            _paused = false;
            Time.timeScale = 1f;
        }

        void DrawPause()
        {
            EnsureCinemaStyles();
            Fill(new Rect(0, 0, VW, VH), new Color(0.01f, 0.01f, 0.015f, 0.72f));
            for (int i = 0; i < 24; i++) Fill(new Rect(0, 0, VW * 0.5f * (1f - i / 24f), VH), new Color(0, 0, 0, 0.03f));
            float x = Mathf.Max(80f, VW * 0.08f), y = VH * 0.24f;
            GUI.Label(new Rect(x, y, 900, 30), "S O R U Ş T U R M A   D U R A K L A T I L D I", _splashKicker);
            GUI.Label(new Rect(x - 4, y + 34, 1200, 90), GameData.TownName.ToUpperInvariant(), _splashTitle);
            var dn = DayNightCycle.Instance;
            GUI.Label(new Rect(x, y + 128, 1000, 34), dn != null ? $"{dn.Day}. gün, saat {dn.Clock} — {GameData.Current.Victim} cinayeti" : GameData.Current.Victim, _splashSub);
            Fill(new Rect(x, y + 176, 420, 2), new Color(Accent.r, Accent.g, Accent.b, 0.8f));
            float yy = y + 204;
            if (MenuLink(new Rect(x, yy, 560, 54), "DEVAM ET", true)) Do(ClosePause); yy += 64;
            if (MenuLink(new Rect(x, yy, 560, 46), "VAKA PANOSU", false)) Do(() => { ClosePause(); _boardOpen = true; }); yy += 52;
            if (MenuLink(new Rect(x, yy, 560, 46), "AYARLAR", false)) Do(OpenSettings); yy += 52;
            if (MenuLink(new Rect(x, yy, 560, 46), "VAKA HARİTASINA DÖN", false)) Do(() => { ClosePause(); GM.RequestExitToWorldMap(); }); yy += 52;
            if (MenuLink(new Rect(x, yy, 560, 46), "ANA MENÜ", false)) Do(() => { ClosePause(); Confirm("ANA MENÜ", "Ana menüye dönmek istiyor musunuz? Soruşturmanız kaydedilecek, kaldığınız yerden devam edebilirsiniz.", GM.BackToSplash); }); yy += 52;
            if (MenuLink(new Rect(x, yy, 560, 46), "OYUNDAN ÇIK", false)) Do(AskQuit);
            GUI.Label(new Rect(x, VH - 80, 900, 26), "[Esc] devam   •   [B] vaka panosu   •   [C] sinema modu   •   [F1] kontroller", _splashFoot);
        }

        void AskQuit() => Confirm("OYUNDAN ÇIK", "Dedektiflik bu gecelik bitti mi? Oyundan çıkılsın mı?", () => { GameSettings.Save(); Application.Quit(); });

        // ---- Ayar kontrolleri (kutusuz, film menüsü tarzı) ----
        float SettingSlider(Rect r, string label, float v, float min, float max, string valueText)
        {
            GUI.Label(new Rect(r.x, r.y, 300, r.height), label, _setLabel);
            var track = new Rect(r.x + 310, r.y + r.height / 2f - 2f, r.width - 310 - 90, 4);
            var hit = new Rect(track.x - 8, r.y, track.width + 16, r.height);
            int id = GUIUtility.GetControlID(FocusType.Passive);
            var e = Event.current;
            float Val() => Mathf.Lerp(min, max, Mathf.Clamp01((e.mousePosition.x - track.x) / track.width));
            switch (e.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    if (e.button == 0 && hit.Contains(e.mousePosition)) { GUIUtility.hotControl = id; v = Val(); GUI.changed = true; e.Use(); }
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id) { v = Val(); GUI.changed = true; e.Use(); }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id) { GUIUtility.hotControl = 0; e.Use(); }
                    break;
            }
            float k = Mathf.InverseLerp(min, max, v);
            bool hover = hit.Contains(e.mousePosition) || GUIUtility.hotControl == id;
            Fill(track, new Color(1, 1, 1, 0.14f));
            Fill(new Rect(track.x, track.y, track.width * k, track.height), new Color(Accent.r, Accent.g, Accent.b, hover ? 1f : 0.8f));
            var knob = new Rect(track.x + track.width * k - 7, track.center.y - 9, 14, 18);
            Fill(knob, hover ? Color.white : new Color(0.95f, 0.9f, 0.8f));
            GUI.Label(new Rect(r.xMax - 84, r.y, 84, r.height), valueText, _setValue);
            return v;
        }

        bool SettingToggle(Rect r, string label, bool v)
        {
            GUI.Label(new Rect(r.x, r.y, 300, r.height), label, _setLabel);
            var pill = new Rect(r.x + 310, r.y + (r.height - 30) / 2f, 150, 30);
            bool hover = pill.Contains(Event.current.mousePosition);
            Fill(pill, v ? new Color(Accent.r, Accent.g, Accent.b, hover ? 0.4f : 0.28f) : new Color(1, 1, 1, hover ? 0.12f : 0.06f));
            Border(pill, v ? Accent : new Color(1, 1, 1, 0.25f), 1);
            GUI.Label(pill, v ? "AÇIK" : "KAPALI", Styled(_hudKey, v ? Color.white : TextMuted));
            if (GUI.Button(pill, GUIContent.none, GUIStyle.none)) { v = !v; GUI.changed = true; AudioManager.Instance?.TypeClick(); }
            return v;
        }

        int SettingChoice(Rect r, string label, int v, string[] names)
        {
            GUI.Label(new Rect(r.x, r.y, 300, r.height), label, _setLabel);
            float w = (r.width - 310 - 90) / names.Length;
            for (int i = 0; i < names.Length; i++)
            {
                var b = new Rect(r.x + 310 + i * w, r.y + (r.height - 30) / 2f, w - 6, 30);
                bool hover = b.Contains(Event.current.mousePosition), on = v == i;
                Fill(b, on ? new Color(Accent.r, Accent.g, Accent.b, 0.32f) : new Color(1, 1, 1, hover ? 0.12f : 0.05f));
                Border(b, on ? Accent : new Color(1, 1, 1, 0.2f), 1);
                GUI.Label(b, names[i], Styled(_hudKey, on ? Color.white : TextMuted));
                if (GUI.Button(b, GUIContent.none, GUIStyle.none) && !on) { v = i; GUI.changed = true; AudioManager.Instance?.TypeClick(); }
            }
            return v;
        }

        void DrawSettings()
        {
            EnsureCinemaStyles();
            Fill(new Rect(0, 0, VW, VH), new Color(0.01f, 0.01f, 0.015f, 0.86f));
            float w = Mathf.Min(980f, VW - 80f), h = Mathf.Min(860f, VH - 60f);
            var r = new Rect((VW - w) / 2f, (VH - h) / 2f, w, h);
            Fill(r, new Color(0.04f, 0.035f, 0.03f, 0.96f));
            Fill(new Rect(r.x, r.y, r.width, 3), Accent);
            GUI.Label(new Rect(r.x + 50, r.y + 28, w - 100, 56), "AYARLAR", _setTitle);
            GUI.Label(new Rect(r.x + 52, r.y + 84, w - 100, 26), "Değişiklikler anında uygulanır ve saklanır.", _mutedItalic);
            float x = r.x + 50, cw = w - 100, y = r.y + 128, rh = 40;
            GUI.changed = false;
            bool screenChanged = false;

            GUI.Label(new Rect(x, y, cw, 24), "GÖRÜNTÜ", _setSection); y += 30;
            GameSettings.Brightness = SettingSlider(new Rect(x, y, cw, rh), "Parlaklık", GameSettings.Brightness, -1f, 2f, (GameSettings.Brightness >= 0 ? "+" : "") + GameSettings.Brightness.ToString("0.0")); y += rh;
            GameSettings.Quality = SettingChoice(new Rect(x, y, cw, rh), "Grafik kalitesi", GameSettings.Quality, GameSettings.QualityNames); y += rh;
            bool fs = SettingToggle(new Rect(x, y, cw, rh), "Tam ekran", GameSettings.Fullscreen); y += rh;
            if (fs != GameSettings.Fullscreen) { GameSettings.Fullscreen = fs; screenChanged = true; }
            GameSettings.FilmGrain = SettingToggle(new Rect(x, y, cw, rh), "Film greni", GameSettings.FilmGrain); y += rh;
            GameSettings.DepthOfField = SettingToggle(new Rect(x, y, cw, rh), "Sinematik alan derinliği", GameSettings.DepthOfField); y += rh;
            GameSettings.CameraShake = SettingToggle(new Rect(x, y, cw, rh), "Kamera sarsıntısı", GameSettings.CameraShake); y += rh + 12;

            GUI.Label(new Rect(x, y, cw, 24), "SES", _setSection); y += 30;
            GameSettings.Master = SettingSlider(new Rect(x, y, cw, rh), "Ana ses", GameSettings.Master, 0f, 1f, Mathf.RoundToInt(GameSettings.Master * 100) + "%"); y += rh;
            GameSettings.Music = SettingSlider(new Rect(x, y, cw, rh), "Müzik", GameSettings.Music, 0f, 1f, Mathf.RoundToInt(GameSettings.Music * 100) + "%"); y += rh;
            GameSettings.Effects = SettingSlider(new Rect(x, y, cw, rh), "Efektler (gök gürültüsü, kapı...)", GameSettings.Effects, 0f, 1f, Mathf.RoundToInt(GameSettings.Effects * 100) + "%"); y += rh;
            GameSettings.Ambience = SettingSlider(new Rect(x, y, cw, rh), "Ortam (yağmur, rüzgâr)", GameSettings.Ambience, 0f, 1f, Mathf.RoundToInt(GameSettings.Ambience * 100) + "%"); y += rh + 12;

            GUI.Label(new Rect(x, y, cw, 24), "OYNANIŞ", _setSection); y += 30;
            GameSettings.MouseSensitivity = SettingSlider(new Rect(x, y, cw, rh), "Fare hassasiyeti", GameSettings.MouseSensitivity, 0.3f, 2.5f, GameSettings.MouseSensitivity.ToString("0.0") + "x"); y += rh;
            GameSettings.TextSpeed = SettingSlider(new Rect(x, y, cw, rh), "Anlatı / altyazı hızı", GameSettings.TextSpeed, 0.5f, 2f, GameSettings.TextSpeed.ToString("0.0") + "x"); y += rh;
            GameSettings.AutoSearch = SettingToggle(new Rect(x, y, cw, rh), "Binada fareyle arama", GameSettings.AutoSearch); y += rh;

            if (GUI.changed || screenChanged) GameSettings.Apply(screenChanged);

            var close = new Rect(r.xMax - 50 - 240, r.yMax - 76, 240, 50);
            var reset = new Rect(r.x + 50, r.yMax - 76, 280, 50);
            if (ColoredButton(reset, "Varsayılana döndür", new Color(0.14f, 0.12f, 0.1f))) Do(() => { GameSettings.ResetDefaults(); GameSettings.Apply(); });
            if (ColoredButton(close, "KAPAT  [Esc]", new Color(0.45f, 0.3f, 0.12f))) Do(CloseSettings);
        }

        // =================================================================
        //  İÇ SES (kara film anlatıcısı)
        // =================================================================
        class MonoLine { public string Text; public float Start, Dur; }
        readonly Queue<MonoLine> _monoQueue = new Queue<MonoLine>();
        MonoLine _monoCur;

        /// <summary>Dedektifin iç sesi: ekranın altında, film altyazısı gibi akar.</summary>
        public void Monologue(string text, float duration = 0f)
        {
            if (string.IsNullOrEmpty(text)) return;
            if (duration <= 0f) duration = Mathf.Clamp(text.Length / 16f, 4.5f, 10f);
            _monoQueue.Enqueue(new MonoLine { Text = text, Dur = duration });
            if (_monoQueue.Count > 3) _monoQueue.Dequeue();
        }

        void DrawMonologue()
        {
            if (_monoCur != null && Time.time > _monoCur.Start + _monoCur.Dur) _monoCur = null;
            if (_monoCur == null && _monoQueue.Count > 0) { _monoCur = _monoQueue.Dequeue(); _monoCur.Start = Time.time; }
            if (_monoCur == null) return;
            EnsureCinemaStyles();
            float t = Time.time - _monoCur.Start;
            float a = Mathf.Clamp01(t / 0.5f) * Mathf.Clamp01((_monoCur.Dur - t) / 0.6f);
            int chars = Mathf.Min(_monoCur.Text.Length, (int)(t * 40f * GameSettings.TextSpeed));
            float w = Mathf.Min(1100f, VW - 160f);
            float h = _monoText.CalcHeight(new GUIContent(_monoCur.Text), w) + 10f;
            float bottom = _helperOpen ? 296f : _modal == Modal.None && GM.Player.Focus != null ? 160f : 92f;
            var r = new Rect((VW - w) / 2f, VH - bottom - h - Bar, w, h);
            var prev = GUI.color; GUI.color = new Color(1, 1, 1, a);
            for (int i = 0; i < 6; i++) Fill(new Rect(r.x - 40 - i * 20, r.y - 30, r.width + 80 + i * 40, r.height + 40), new Color(0, 0, 0, 0.06f));
            GUI.Label(new Rect(r.x, r.y - 26, r.width, 20), "—  İ Ç   S E S  —", _monoSpeaker);
            string shown = _monoCur.Text.Substring(0, chars);
            GUI.Label(new Rect(r.x + 1.5f, r.y + 2f, r.width, r.height), shown, Styled(_monoText, new Color(0, 0, 0, 0.9f)));
            GUI.Label(r, shown, _monoText);
            GUI.color = prev;
        }

        // =================================================================
        //  DEFTER — sıradaki adım
        // =================================================================
        string _objLast; float _objChangedAt = -10f;

        string CurrentObjective(out string detail, out int step)
        {
            detail = ""; step = 0;
            if (!GM.CorpseInspected) { detail = "Sarı şeridin içine gir ve cesedin başında [E]'ye bas."; return "Olay yerini incele"; }
            step = 1;
            if (GM.VisitedCount < GameData.ReqBuildings) { detail = $"{GM.VisitedCount}/{GameData.ReqBuildings} bina incelendi — kapılara yaklaş, [E]."; return "Dükkânları gez, şüphelileri sorgula"; }
            step = 2;
            if (GM.LabCount < GameData.ReqLabs) { detail = $"{GM.LabCount}/{GameData.ReqLabs} bulgu gönderildi — dışarıda [Tab] çanta → kan ayıracı / iz tozu."; return "Delilleri adli laboratuvara gönder"; }
            step = 3;
            if (!GM.AutopsyReady) { int s = Mathf.CeilToInt(GM.AutopsyTimeLeft); detail = GM.AutopsyTimerStarted ? $"Adli Tıp raporu hazırlıyor — {s / 60:00}:{s % 60:00}" : "Rapor hazırlanıyor."; return "Otopsi raporunu bekle"; }
            step = 4;
            detail = "[R] otopsi raporu   •   [B] vaka panosu   •   [F] BULDUM!";
            return "Katili belirle";
        }

        void DrawObjectives()
        {
            EnsureCinemaStyles();
            string obj = CurrentObjective(out var detail, out int step);
            if (obj != _objLast)
            {
                if (_objLast != null) { _objChangedAt = Time.time; AudioManager.Instance?.Sting(0.35f); }
                _objLast = obj;
            }
            float x = 34f, y = 300f, w = 470f;
            float flash = Mathf.Clamp01(1f - (Time.time - _objChangedAt) / 2.5f);
            if (flash > 0f) Fill(new Rect(x - 12, y - 6, w + 24, 104), new Color(Accent.r, Accent.g, Accent.b, 0.16f * flash));
            ShadowLabel(new Rect(x, y, w, 18), flash > 0f ? "DEFTER — YENİ ADIM" : "DEFTER — SIRADAKİ ADIM", _objHead);
            for (int i = 0; i < 5; i++)
            {
                var dot = new Rect(x + 218 + i * 16, y + 5, 9, 9);
                Fill(dot, i < step ? Accent : i == step ? new Color(0.95f, 0.91f, 0.82f) : new Color(1, 1, 1, 0.2f));
            }
            ShadowLabel(new Rect(x, y + 20, w, 30), "▸ " + obj, _objText);
            ShadowLabel(new Rect(x + 18, y + 50, w - 18, 44), detail, _objDetail);
        }

        // =================================================================
        //  BÖLÜM KARTI — film açılışı
        // =================================================================
        public const float ChapterSeconds = 6.5f;
        float _chapterStart = -100f;
        static readonly string[] Roman = { "", "I", "II", "III", "IV", "V" };

        void DrawChapterCard()
        {
            EnsureCinemaStyles();
            float t = Time.time - _chapterStart;
            float fadeOut = Mathf.Clamp01((ChapterSeconds - t) / 1.2f);
            Fill(new Rect(0, 0, VW, VH), new Color(0, 0, 0, fadeOut));
            var town = GameData.Current;
            float a1 = Mathf.Clamp01((t - 0.4f) / 0.8f) * fadeOut, a2 = Mathf.Clamp01((t - 1.4f) / 1f) * fadeOut, a3 = Mathf.Clamp01((t - 2.6f) / 1f) * fadeOut;
            var prev = GUI.color;
            float cy = VH / 2f;
            GUI.color = new Color(1, 1, 1, a1);
            GUI.Label(new Rect(0, cy - 190, VW, 30), "B İ R   A N A D O L U   G İ Z E M İ", _chapKicker);
            GUI.Label(new Rect(0, cy - 140, VW, 40), "BÖLÜM " + (town.Level >= 0 && town.Level < Roman.Length ? Roman[town.Level] : town.Level.ToString()), _chapNum);
            GUI.color = new Color(1, 1, 1, a2);
            float lw = 520f * a2;
            Fill(new Rect(VW / 2f - lw / 2f, cy - 92, lw, 1), Accent);
            GUI.Label(new Rect(0, cy - 90, VW, 130), town.Name.ToUpperInvariant(), _chapTitle);
            Fill(new Rect(VW / 2f - lw / 2f, cy + 44, lw, 1), Accent);
            GUI.color = new Color(1, 1, 1, a3);
            GUI.Label(new Rect(0, cy + 60, VW, 40), $"{town.Victim} cinayeti", _chapSub);
            GUI.Label(new Rect(0, cy + 104, VW, 30), "Anadolu, Sonbahar 1887 — " + town.Mood, _centerMuted);
            GUI.color = prev;
            GUI.Label(new Rect(0, VH - 60, VW - 40, 30), "[Boşluk] geç", _rightMuted);
        }

        // =================================================================
        //  VAKA PANOSU [B]
        // =================================================================
        Vector2 _boardScroll;

        static void Line(Vector2 a, Vector2 b, float width, Color c, float scale)
        {
            var d = b - a; float len = d.magnitude; if (len < 1f) return;
            float ang = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            var m = GUI.matrix;
            GUIUtility.RotateAroundPivot(ang, a * scale);
            Fill(new Rect(a.x, a.y - width / 2f, len, width), c);
            GUI.matrix = m;
        }

        List<string> Deductions()
        {
            var list = new List<string>();
            var collected = GM.Clues.Where(c => c.InBag || c.BloodSent || c.PrintSent).ToList();
            foreach (var n in GM.Npcs)
            {
                int k = collected.Count(c => c.Def.BuildingNpcId == n.Id);
                if (GM.IsInnocent(n.Id)) list.Add($"<color=#6f9a5a>{n.Def.Name}</color> masum ilan edildi.");
                else if (k >= 2) list.Add($"<b>{n.Def.Name}</b>: dükkânından {k} delil çıktı. İzlerin çoğu ona çıkıyor.");
                if (n.Stress >= 60 && n.Log.Count > 0) list.Add($"<b>{n.Def.Name}</b> sorguda çok gerildi (%{n.Stress}). Bir şey saklıyor olabilir.");
                else if (n.Log.Count == 0 && GM.IsVisited(n.Id)) list.Add($"{n.Def.Name} ile hiç konuşulmadı — ifadesi eksik.");
            }
            foreach (var c in collected)
            {
                if (c.BloodSent) list.Add($"'{c.Def.Name}' üzerinde kan bulundu; örnek laboratuvarda.");
                if (c.PrintSent) list.Add($"'{c.Def.Name}' üzerinde parmak izi var; eşleşme bekleniyor.");
            }
            if (GM.AutopsyReady) list.Add("Otopsi raporu geldi: ölüm sebebini şüphelilerin anlattıklarıyla karşılaştır.");
            if (list.Count == 0) list.Add("Henüz pano boş. Olay yerini incele, dükkânları gez ve delil topla — her bulgu buraya iğnelenir.");
            return list;
        }

        void DrawCaseBoard()
        {
            EnsureCinemaStyles();
            Fill(new Rect(0, 0, VW, VH), new Color(0.01f, 0.01f, 0.01f, 0.9f));
            // Mantar pano
            var board = new Rect(50, 110, VW - 520, VH - 170);
            Fill(board, new Color(0.36f, 0.25f, 0.15f));
            for (int i = 0; i < 40; i++)
            {
                float px = board.x + Mathf.Repeat(i * 137.5f, board.width - 20), py = board.y + Mathf.Repeat(i * 89.3f, board.height - 20);
                Fill(new Rect(px, py, 18, 12), new Color(0.3f, 0.2f, 0.12f, 0.5f));
            }
            Border(board, new Color(0.22f, 0.14f, 0.08f), 10);
            GUI.Label(new Rect(52, 34, 900, 56), "VAKA PANOSU — " + GameData.TownName.ToUpperInvariant(), _boardHead);
            GUI.Label(new Rect(54, 82, 1200, 24), $"{GameData.CaseTitle}  •  Maktul: {GameData.Current.Victim}", _mutedItalic);

            var npcs = GM.Npcs.ToList();
            var clues = GM.Clues.Where(c => c.InBag || c.BloodSent || c.PrintSent).ToList();
            var npcPos = new Dictionary<int, Vector2>();
            int n = Mathf.Max(1, npcs.Count);
            float cardW = Mathf.Min(150f, (board.width - 60f) / n - 14f), cardH = cardW * 1.32f + 46f;
            for (int i = 0; i < npcs.Count; i++)
            {
                var np = npcs[i];
                float cx = board.x + 30 + i * (cardW + 14) + ((board.width - 60) - n * (cardW + 14)) / 2f;
                var card = new Rect(cx, board.y + 30 + (i % 2) * 18, cardW, cardH);
                npcPos[np.Id] = new Vector2(card.center.x, card.y + 8);
                Fill(new Rect(card.x + 5, card.y + 6, card.width, card.height), new Color(0, 0, 0, 0.35f));
                Fill(card, new Color(0.93f, 0.89f, 0.8f));
                var tex = T(np.Def.Portrait);
                var pr = new Rect(card.x + 8, card.y + 8, card.width - 16, card.width * 1.2f);
                if (tex != null) GUI.DrawTexture(pr, tex, ScaleMode.ScaleAndCrop); else Fill(pr, new Color(0.3f, 0.27f, 0.24f));
                GUI.Label(new Rect(card.x + 4, pr.yMax + 2, card.width - 8, 22), np.Def.Name, _boardName);
                GUI.Label(new Rect(card.x + 4, pr.yMax + 22, card.width - 8, 20), np.Def.Building, _boardRole);
                // Durum damgası
                string st = GM.IsInnocent(np.Id) ? "MASUM" : np.Log.Count > 0 ? $"SORGULANDI %{np.Stress}" : GM.IsVisited(np.Id) ? "ZİYARET EDİLDİ" : "";
                if (st.Length > 0)
                {
                    var stc = GM.IsInnocent(np.Id) ? new Color(0.2f, 0.45f, 0.22f, 0.9f) : np.Stress >= 60 ? new Color(0.65f, 0.1f, 0.08f, 0.9f) : new Color(0.25f, 0.22f, 0.4f, 0.85f);
                    var sr = new Rect(card.x + 6, pr.y + pr.height - 30, card.width - 12, 24);
                    Fill(sr, new Color(stc.r, stc.g, stc.b, 0.85f));
                    GUI.Label(sr, st, Styled(_hudKey, Color.white));
                }
                Fill(new Rect(card.center.x - 6, card.y - 6, 12, 12), new Color(0.75f, 0.1f, 0.08f));   // iğne
            }
            // Deliller (kâğıt notlar) + kırmızı ip
            float noteW = 200f, noteH = 96f;
            int perRow = Mathf.Max(1, Mathf.FloorToInt((board.width - 60f) / (noteW + 22f)));
            var notes = new List<(Rect r, ClueRuntime c)>();
            for (int i = 0; i < clues.Count; i++)
            {
                int row = i / perRow, col = i % perRow;
                float nx = board.x + 34 + col * (noteW + 22) + (row % 2) * 30;
                float ny = board.y + cardH + 110 + row * (noteH + 40);
                if (ny + noteH > board.yMax - 10) break;
                notes.Add((new Rect(nx, ny, noteW, noteH), clues[i]));
            }
            float sc = _scale;
            foreach (var (r, c) in notes)
                if (npcPos.TryGetValue(c.Def.BuildingNpcId, out var to))
                    Line(new Vector2(r.center.x, r.y + 6), to, 3f, new Color(0.7f, 0.06f, 0.05f, 0.9f), sc);
            foreach (var (r, c) in notes)
            {
                Fill(new Rect(r.x + 4, r.y + 5, r.width, r.height), new Color(0, 0, 0, 0.3f));
                Fill(r, c.BloodSent || c.PrintSent ? new Color(0.96f, 0.92f, 0.78f) : new Color(0.98f, 0.96f, 0.88f));
                string tag = (c.BloodSent ? " [KAN]" : "") + (c.PrintSent ? " [İZ]" : "");
                GUI.Label(new Rect(r.x + 10, r.y + 10, r.width - 20, r.height - 16), $"<b>{c.Def.Name}</b>{tag}\n<size=12>{Short(c.Def.Description, 70)}</size>", _boardNote);
                Fill(new Rect(r.center.x - 6, r.y - 2, 12, 12), new Color(0.75f, 0.1f, 0.08f));
            }
            if (clues.Count == 0)
                GUI.Label(new Rect(board.x + 40, board.y + cardH + 120, board.width - 80, 60), "Henüz panoya iğnelenmiş delil yok.", Styled(_boardDeduce, new Color(0.95f, 0.9f, 0.8f, 0.7f)));

            // Sağ sütun: çıkarımlar
            var side = new Rect(VW - 440, 110, 400, VH - 170);
            Fill(side, new Color(0.05f, 0.045f, 0.04f, 0.95f));
            Fill(new Rect(side.x, side.y, side.width, 3), Accent);
            GUI.Label(new Rect(side.x + 24, side.y + 18, side.width - 48, 26), "ÇIKARIMLAR", _setSection);
            GUILayout.BeginArea(new Rect(side.x + 24, side.y + 54, side.width - 48, side.height - 130));
            _boardScroll = GUILayout.BeginScrollView(_boardScroll);
            foreach (var d in Deductions()) { GUILayout.Label("•  " + d, _boardDeduce); GUILayout.Space(10); }
            if (!string.IsNullOrEmpty(GM.DetectiveNotes)) { GUILayout.Space(8); GUILayout.Label("<b>NOTLARIM</b>\n" + GM.DetectiveNotes, _boardDeduce); }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
            if (ColoredButton(new Rect(side.x + 24, side.yMax - 66, side.width - 48, 46), "PANOYU KAPAT  [B / Esc]", new Color(0.45f, 0.3f, 0.12f))) Do(() => _boardOpen = false);
        }

        static string Short(string s, int max) => string.IsNullOrEmpty(s) ? "" : s.Length <= max ? s : s.Substring(0, max - 1) + "…";

        // =================================================================
        //  FİNAL JENERİĞİ
        // =================================================================
        float _creditsStart = -100f;

        void DrawCredits(Rect area)
        {
            EnsureCinemaStyles();
            var lines = new List<(string role, string name, bool head)>
            {
                ("", "OYUNCULAR", true),
                ("Dedektif", "Sen", false),
                ("Yardımcı Dedektif", "Çetin", false),
            };
            var td = GameData.Current;
            if (!string.IsNullOrEmpty(td.LocalHelperName)) lines.Add(("Yerel rehber", td.LocalHelperName, false));
            foreach (var n in GM.Npcs) lines.Add((string.IsNullOrEmpty(n.Def.Building) ? n.Def.Role : n.Def.Building, n.Def.Name, false));
            lines.Add(("Maktul", td.Victim, false));
            lines.Add(("", "", false));
            lines.Add(("", "MEKÂN", true));
            lines.Add(("Kasaba", td.Name, false));
            lines.Add(("Zaman", "Sonbahar, 1887", false));
            lines.Add(("Hava", td.Mood, false));
            lines.Add(("", "", false));
            lines.Add(("", "BİR ANADOLU GİZEMİ", true));
            lines.Add(("", "— SON —", true));
            float lh = 38f, total = lines.Count * lh + area.height;
            float off = Mathf.Repeat((Time.time - _creditsStart) * 34f, total);
            GUI.BeginGroup(area);
            for (int i = 0; i < lines.Count; i++)
            {
                float y = area.height - off + i * lh;
                if (y < -lh || y > area.height) continue;
                float fade = Mathf.Clamp01(y / 80f) * Mathf.Clamp01((area.height - y) / 80f);
                var prev = GUI.color; GUI.color = new Color(1, 1, 1, fade);
                var (role, name, head) = lines[i];
                if (head) GUI.Label(new Rect(0, y, area.width, lh), name, _creditHead);
                else if (name.Length > 0)
                {
                    GUI.Label(new Rect(0, y, area.width / 2f - 14f, lh), role, _creditRole);
                    GUI.Label(new Rect(area.width / 2f + 14f, y, area.width / 2f - 14f, lh), name, _creditName);
                }
                GUI.color = prev;
            }
            GUI.EndGroup();
        }

        /// <summary>Dava sonu: film finali gibi — solda hüküm ve o gecenin hikâyesi, sağda akan jenerik.</summary>
        void DrawResultCinematic()
        {
            EnsureCinemaStyles();
            if (_creditsStart < 0f || Time.time - _creditsStart > 600f) _creditsStart = Time.time;
            Fill(new Rect(0, 0, VW, VH), new Color(0.005f, 0.005f, 0.008f, 0.94f));
            Fill(new Rect(0, 0, VW, VH * 0.08f), Color.black);
            Fill(new Rect(0, VH * 0.92f, VW, VH * 0.08f), Color.black);
            var col = GM.ResultWin ? Success : Danger;
            float x = Mathf.Max(80f, VW * 0.07f), w = VW * 0.5f, y = VH * 0.13f;
            GUI.Label(new Rect(x, y, w, 30), GM.ResultWin ? "D A V A   K A P A N D I" : "D A V A   Ç Ö Z Ü L E M E D İ", Styled(_splashKicker, col));
            GUI.Label(new Rect(x - 4, y + 32, w + 200, 80), GM.ResultTitle, Styled(_mapTitle, new Color(0.95f, 0.91f, 0.82f)));
            float vh = _centerBodyLarge.CalcHeight(new GUIContent(GM.ResultVerdict), w);
            GUI.Label(new Rect(x, y + 118, w, vh + 10), GM.ResultVerdict, Styled(_bodyLarge, new Color(0.88f, 0.84f, 0.76f)));
            float sy = y + 140 + vh;
            Fill(new Rect(x, sy, 380, 2), new Color(col.r, col.g, col.b, 0.7f));
            GUI.Label(new Rect(x, sy + 12, w, 26), "O GECENİN HİKÂYESİ", _accentSmall);
            GUILayout.BeginArea(new Rect(x, sy + 46, w, VH * 0.86f - sy - 170));
            _resultScroll = GUILayout.BeginScrollView(_resultScroll);
            GUILayout.Label(GM.ResultStory ?? "", Styled(_body, new Color(0.84f, 0.8f, 0.72f)));
            GUILayout.EndScrollView();
            GUILayout.EndArea();
            if (!string.IsNullOrEmpty(GM.UnlockedMessage)) GUI.Label(new Rect(x, VH * 0.86f - 110, w, 34), GM.UnlockedMessage, Styled(_bodyBold, Warning));
            if (MenuLink(new Rect(x, VH * 0.86f - 66, 600, 54), (GM.ResultButton ?? "Vaka haritasına dön").ToUpperInvariant(), true)) Do(GM.ResultContinue);
            DrawCredits(new Rect(VW * 0.62f, VH * 0.1f, VW * 0.34f, VH * 0.8f));
        }
    }
}
