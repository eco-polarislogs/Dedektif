using System;
using System.Collections.Generic;
using System.Linq;
using Dedektif3D.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dedektif3D.UI
{
    /// <summary>
    /// Tamamen koddan çizilen (IMGUI) arayüz — wwwroot/index.html ekranlarının birebir karşılığı.
    /// Web görselleri Resources/Web altından yüklenir (town_bg, world_map_generated, portreler, delil resimleri, hapis sahneleri).
    /// Palet (1800'ler sinematik noir): mürekkep siyahı #0e0b08, pirinç #d4a65a, parşömen #efe6d2, kan kırmızısı #a3222a, küf yeşili #6f9a5a, kehribar #d89a2b.
    /// Yazılar tırnaklı (serif) gazete harfleriyle, belgeler daktilo harfiyle; paneller çift çerçeveli vaka dosyası görünümünde.
    /// Tüm buton eylemleri bir sonraki Update'e ertelenir (IMGUI Layout/Repaint uyuşmazlığını önler).
    /// </summary>
    public partial class GameUI : MonoBehaviour
    {
        enum Modal { None, DoorEntry, BuildingClue, Bag, Lab, Interrogation, Found, History, Jail, ExitWarning, Autopsy }
        enum Tool { None, UV, Dust, Magnifier }

        static readonly Color BgNoir = Hex("#0e0b08", 0.95f);
        static readonly Color BgCard = Hex("#17120d", 0.96f);
        static readonly Color BgGlass = Hex("#120e0a", 0.84f);
        static readonly Color Accent = Hex("#d4a65a");
        static readonly Color TextPrimary = Hex("#efe6d2");
        static readonly Color TextMuted = Hex("#9c8f7a");
        static readonly Color Danger = Hex("#a3222a");
        static readonly Color Success = Hex("#6f9a5a");
        static readonly Color Warning = Hex("#d89a2b");
        static readonly Color Paper = Hex("#e9dfc8");
        static readonly Color Ink = Hex("#2a1a0e");

        GameManager GM => GameManager.Instance;

        Modal _modal = Modal.None;
        BuildingDef _door;
        ClueRuntime _clue;
        NpcRuntime _npc, _historyNpc, _jailNpc;
        string _exitText;
        string _question = "";
        Vector2 _chatScroll, _bagScroll, _autopsyScroll, _resultScroll, _historyScroll, _docScroll;
        int _lastChatCount;
        float _jailStart;

        // Adli lab
        Tool _tool;
        float _scanTime, _spotTime;
        bool _showDoc;

        // Hikaye
        float _storyStart; bool _storySkipped; int _lastTypedChar;

        // Bildirimler
        class ToastMsg { public string Text; public float Until; }
        readonly List<ToastMsg> _toasts = new List<ToastMsg>();
        class Box { public string Title, Text; public bool Warning, Confirm; public Action OnYes; }
        readonly List<Box> _boxes = new List<Box>();

        // Çetin sinematik kutusu
        bool _helperOpen; int _helperIndex = -1; float _helperShownAt;

        // Geçiş (transition-overlay / building-entry fade)
        float _fadeStart = -10f; string _fadeTitle; Color _fadeColor; Action _fadeMid; bool _fadeMidDone;
        const float FadeDuration = 1.2f;

        readonly Queue<Action> _actions = new Queue<Action>();
        void Do(Action a) => _actions.Enqueue(a);

        bool _showControls = true;
        float _scale = 1f;
        float VW => Screen.width / _scale;
        float VH => Screen.height / _scale;

        bool FadeActive => Time.time - _fadeStart < FadeDuration;
        public bool IsModalOpen => _modal != Modal.None || _boxes.Count > 0 || FadeActive || _paused || _settingsOpen || _boardOpen;

        // =================================================================
        //  Görseller (Resources/Web)
        // =================================================================
        readonly Dictionary<string, Texture2D> _tex = new Dictionary<string, Texture2D>();
        Texture2D T(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            return WebTextures.Get(name);
        }

        // =================================================================
        //  DIŞ API (GameManager çağırır)
        // =================================================================
        public void OnPhaseChanged(GamePhase p)
        {
            _modal = Modal.None;
            _boxes.Clear();
            if (p == GamePhase.Story)
            {
                _chapterStart = Time.time;
                _storyStart = Time.time + ChapterSeconds;      // önce film tarzı bölüm kartı
                _storySkipped = false; _lastTypedChar = 0;
                AudioManager.Instance?.Sting(0.9f);
            }
            _boardOpen = false;
            if (p != GamePhase.Playing && _paused) ClosePause();
            if (p == GamePhase.Splash) _splashStart = Time.time;
            if (p == GamePhase.WorldMap) _mapUiA = 0f;
            if (p == GamePhase.Playing) _ctlUntil = Time.time + 40f;
            if (p == GamePhase.Splash || p == GamePhase.Story) _helperOpen = false;
        }

        public void Toast(string text)
        {
            _toasts.Add(new ToastMsg { Text = text, Until = Time.time + 4.5f });
            if (_toasts.Count > 4) _toasts.RemoveAt(0);
        }

        public void MessageBox(string title, string text, bool warning) { _boxes.Add(new Box { Title = title, Text = text, Warning = warning }); AudioManager.Instance?.Ding(); }
        public void Confirm(string title, string text, Action onYes) => _boxes.Add(new Box { Title = title, Text = text, Confirm = true, OnYes = onYes });

        public void FadeTransition(string title, Color color, Action atMidpoint)
        {
            _fadeStart = Time.time; _fadeTitle = title; _fadeColor = color; _fadeMid = atMidpoint; _fadeMidDone = false;
            _modal = Modal.None;
        }

        public void ShowDoorEntry(BuildingDef def) { _door = def; _modal = Modal.DoorEntry; }
        public void ShowBuildingClue(ClueRuntime c) { _clue = c; _modal = Modal.BuildingClue; }
        public void ShowLab(ClueRuntime c) { _clue = c; _tool = Tool.None; _scanTime = _spotTime = 0f; _showDoc = false; _modal = Modal.Lab; }
        public void ShowExitWarning(string text) { _exitText = text; _modal = Modal.ExitWarning; }
        public void OpenInterrogation(NpcRuntime n) { _npc = n; _question = ""; _lastChatCount = -1; _modal = Modal.Interrogation; AudioManager.Instance?.DoorClose(); }
        public void ShowFound() => _modal = Modal.Found;
        /// <summary>Sorgu ekranı bu şüpheliyle mi açık (konuşma animasyonu için).</summary>
        public bool IsTalkingWith(int npcId) => _modal == Modal.Interrogation && _npc != null && _npc.Id == npcId;
        public void ShowJail(NpcRuntime n) { _jailNpc = n; _jailStart = Time.time; _modal = Modal.Jail; }
        public void ShowCinematicHelper() { _helperOpen = true; _helperIndex = GM.HelperHistory.Count - 1; _helperShownAt = Time.time; }

        // =================================================================
        //  GİRDİ
        // =================================================================
        void Update()
        {
            while (_actions.Count > 0) _actions.Dequeue()?.Invoke();

            if (FadeActive && !_fadeMidDone && Time.time - _fadeStart > FadeDuration * 0.5f)
            {
                _fadeMidDone = true;
                var mid = _fadeMid; _fadeMid = null;
                mid?.Invoke();
            }
            if (GM == null) return;

            // Sahneye göre müzik: menü/harita teması, soruşturmada alçak, sorgu ve suçlamada gerilim, finalde tam
            var am = AudioManager.Instance;
            if (am != null)
            {
                var mood = GM.Phase == GamePhase.Playing
                    ? (_modal == Modal.Interrogation || _modal == Modal.Found || _modal == Modal.Jail ? AudioManager.Mood.Tension : AudioManager.Mood.Investigation)
                    : GM.Phase == GamePhase.Result ? AudioManager.Mood.Finale : AudioManager.Mood.Menu;
                am.SetMood(mood);
            }

            bool playing = GM.Phase == GamePhase.Playing;
            bool wantCursor = !playing || IsModalOpen;
            Cursor.lockState = wantCursor ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = wantCursor;

            // Hikaye daktilo sesi
            if (GM.Phase == GamePhase.Story && !_storySkipped)
            {
                int chars = StoryChars;
                if (chars > _lastTypedChar && chars < GameData.StoryText.Length) { if (chars % 2 == 0) AudioManager.Instance?.TypeClick(); _lastTypedChar = chars; }
            }
            // Çetin kutusu kendiliğinden kapanır
            if (_helperOpen && _helperIndex >= 0 && _helperIndex < GM.HelperHistory.Count)
            {
                var m = GM.HelperHistory[_helperIndex];
                if (Time.time - _helperShownAt > 9f + m.Text.Length / HelperMessage.CharsPerSecond && _modal == Modal.None) _helperOpen = false;
            }

            var kb = Keyboard.current;
            if (kb == null) return;

            if (GM.Phase == GamePhase.Descent && (kb.spaceKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame)) GM.SkipDescent();
            if (GM.Phase == GamePhase.Story && Time.time < _storyStart && !_settingsOpen && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame)) _storyStart = Time.time;
            // 3D bölge haritası: ← → kasabalar, Enter dosyayı aç / başla, Esc dosyayı kapat
            var wmap = World.WorldMapDiorama.Instance;
            if (GM.Phase == GamePhase.WorldMap && wmap != null && wmap.Active && !wmap.Diving && _boxes.Count == 0 && !FadeActive && !_settingsOpen)
            {
                if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame) CycleSite(wmap, 1);
                else if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame) CycleSite(wmap, -1);
                else if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
                {
                    if (wmap.Selected != null) StartCase(wmap, wmap.Selected);
                    else CycleSite(wmap, 1);
                }
                else if (kb.escapeKey.wasPressedThisFrame && wmap.Selected != null) { wmap.Select(null); return; }
            }
            if (kb.f1Key.wasPressedThisFrame)
            {
                if (ControlsVisible) _showControls = false;
                else { _showControls = true; _ctlUntil = float.MaxValue; }
            }
            if (kb.escapeKey.wasPressedThisFrame)
            {
                if (_boxes.Count > 0) { CloseBox(false); return; }
                if (_settingsOpen) { CloseSettings(); return; }
                if (_boardOpen) { _boardOpen = false; return; }
                if (_paused) { ClosePause(); return; }
                if (_modal == Modal.Lab && _showDoc) { _showDoc = false; return; }
                if (_modal != Modal.None && _modal != Modal.Jail) { CloseModal(); return; }
                if (_helperOpen) { _helperOpen = false; return; }
                if (GM.Phase == GamePhase.Playing && !FadeActive && _modal == Modal.None) { OpenPause(); return; }
            }
            if (!playing || FadeActive || _boxes.Count > 0 || _paused || _settingsOpen) return;
            if (_boardOpen) { if (kb.bKey.wasPressedThisFrame || kb.tabKey.wasPressedThisFrame) _boardOpen = false; return; }

            if (_helperOpen && _modal == Modal.None)
            {
                if (kb.digit1Key.wasPressedThisFrame) GM.AnalyzeBag();
                else if (kb.digit2Key.wasPressedThisFrame) GM.ProvideBuildingHint();
                else if (kb.gKey.wasPressedThisFrame) _helperOpen = false;
            }

            if (_modal == Modal.None)
            {
                if (kb.tabKey.wasPressedThisFrame) OpenBag();
                else if (kb.fKey.wasPressedThisFrame) GM.OpenAccuse();
                else if (kb.rKey.wasPressedThisFrame && GM.AutopsyReady) _modal = Modal.Autopsy;
                else if (kb.hKey.wasPressedThisFrame) GM.OpenHelperPanel();
                else if (kb.mKey.wasPressedThisFrame) AudioManager.Instance?.ToggleMute();
                else if (kb.bKey.wasPressedThisFrame) { _boardOpen = true; AudioManager.Instance?.TypeClick(); }
                else if (kb.cKey.wasPressedThisFrame) _cinemaMode = !_cinemaMode;
            }
            else if (_modal == Modal.Bag && kb.tabKey.wasPressedThisFrame) CloseModal();
        }

        void OpenBag() { _modal = Modal.Bag; GM.TriggerHelper("bag_open"); }
        void CloseModal() { _modal = Modal.None; GUIUtility.keyboardControl = 0; GM?.EndCinematicShot(); }
        void CloseBox(bool yes)
        {
            if (_boxes.Count == 0) return;
            var b = _boxes[0]; _boxes.RemoveAt(0);
            if (yes) b.OnYes?.Invoke();
        }

        int StoryChars => _storySkipped ? GameData.StoryText.Length : Mathf.Clamp((int)((Time.time - _storyStart) * 28.5f * GameSettings.TextSpeed), 0, GameData.StoryText.Length);

        // =================================================================
        //  ÇİZİM
        // =================================================================
        void OnGUI()
        {
            if (GM == null) return;
            EnsureStyles();
            _scale = Mathf.Max(0.5f, Screen.height / 1080f);
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(_scale, _scale, 1));

            switch (GM.Phase)
            {
                case GamePhase.Splash: DrawSplash(); break;
                case GamePhase.WorldMap: DrawWorldMap(); break;
                case GamePhase.Story: DrawStory(); break;
                case GamePhase.Descent: DrawDescent(); break;
                case GamePhase.Playing: DrawLetterbox(); if (!Cinematic) DrawHud(); DrawMonologue(); DrawModal(); if (_boardOpen) DrawCaseBoard(); break;
                case GamePhase.Result: DrawResult(); break;
            }
            var openFile = GM.Phase == GamePhase.WorldMap && World.WorldMapDiorama.Instance != null && World.WorldMapDiorama.Instance.Selected != null;
            if (GM.Phase != GamePhase.Splash && GM.Phase != GamePhase.Story && GM.Phase != GamePhase.Descent && !openFile) DrawCinematicHelper();
            DrawToasts();
            DrawMuteButton();
            if (_paused && !_settingsOpen) DrawPause();
            if (_settingsOpen) DrawSettings();
            DrawBoxes();
            DrawFade();
        }

        float _lb;
        bool Cinematic => GM.Director != null && GM.Director.Letterbox || _modal == Modal.Interrogation || _modal == Modal.BuildingClue || _cinemaMode;
        float Bar => VH * 0.115f * _lb;

        /// <summary>Film şeritleri: sinematik anlarda ekranın üstü ve altı siyah bantla kapanır.</summary>
        void DrawLetterbox()
        {
            if (Event.current.type == EventType.Repaint) _lb = Mathf.MoveTowards(_lb, Cinematic ? 1f : 0f, Time.unscaledDeltaTime * 2.2f);
            if (_lb <= 0.001f) return;
            Fill(new Rect(0, 0, VW, Bar), Color.black);
            Fill(new Rect(0, VH - Bar, VW, Bar), Color.black);
        }

        /// <summary>Film altyazısı: alt şeridin üstünde, gölgeli krem metin.</summary>
        void Subtitle(string speaker, string text, Color speakerColor, float yFromBottom = 0f, Texture2D portrait = null)
        {
            float w = Mathf.Min(1300, VW - 120);
            float h = Mathf.Max(70, _subtitle2.CalcHeight(new GUIContent(text), w - (portrait != null ? 150 : 0)) + 40);
            var r = new Rect((VW - w) / 2, VH - Bar - h - 18 - yFromBottom, w, h);
            Fill(new Rect(r.x - 20, r.y - 8, r.width + 40, r.height + 16), new Color(0, 0, 0, 0.45f));
            float tx = r.x;
            if (portrait != null)
            {
                var pr = new Rect(r.x, r.y + 4, 120, 120 * Mathf.Min(1f, h / 120f));
                GUI.DrawTexture(new Rect(r.x, r.y, 120, Mathf.Min(h, 120)), portrait, ScaleMode.ScaleAndCrop);
                Border(new Rect(r.x, r.y, 120, Mathf.Min(h, 120)), new Color(0.7f, 0.55f, 0.3f), 2);
                tx += 140;
            }
            if (!string.IsNullOrEmpty(speaker)) GUI.Label(new Rect(tx, r.y, w, 28), speaker.ToUpperInvariant(), Styled(_accentSmall, speakerColor));
            GUI.Label(new Rect(tx + 2, r.y + 30, r.xMax - tx, h - 26), text, _subtitleShadow);
            GUI.Label(new Rect(tx, r.y + 28, r.xMax - tx, h - 26), text, _subtitle2);
        }

        void DrawModal()
        {
            switch (_modal)
            {
                case Modal.DoorEntry: DrawDoorEntry(); break;
                case Modal.BuildingClue: DrawBuildingClue(); break;
                case Modal.Bag: DrawBag(); break;
                case Modal.Lab: DrawLab(); break;
                case Modal.Interrogation: DrawInterrogation(); break;
                case Modal.Found: DrawFound(); break;
                case Modal.History: DrawHistory(); break;
                case Modal.Jail: DrawJail(); break;
                case Modal.ExitWarning: DrawExitWarning(); break;
                case Modal.Autopsy: DrawAutopsy(); break;
            }
        }

        // ---------- 1. AÇILIŞ: resim yok — yağmurlu kasabanın üstünde canlı 3D vinç çekimi + film afişi tipografisi ----------
        float _splashStart;
        void DrawSplash()
        {
            float t = Time.time - _splashStart;
            // Soldan sağa açılan afiş gölgesi (yazı okunur, kasaba sağda nefes alır)
            for (int i = 0; i < 28; i++)
            {
                float k = i / 27f;
                Fill(new Rect(VW * 0.66f * k, 0, VW * 0.66f / 28f + 1f, VH), new Color(0.008f, 0.01f, 0.018f, Mathf.Lerp(0.86f, 0f, k * k)));
            }
            Fill(new Rect(0, 0, VW, VH * 0.075f), Color.black);
            Fill(new Rect(0, VH * 0.925f, VW, VH * 0.075f), Color.black);
            float a1 = Mathf.Clamp01((t - 0.3f) / 1.4f), a2 = Mathf.Clamp01((t - 1.2f) / 1.4f), a3 = Mathf.Clamp01((t - 2.2f) / 1.2f);
            float x = Mathf.Max(70f, VW * 0.075f), y = VH * 0.24f;
            var prev = GUI.color;
            GUI.color = new Color(1, 1, 1, a1);
            GUI.Label(new Rect(x, y, 900, 30), "A N A D O L U    •    S O N B A H A R    1 8 8 7", _splashKicker);
            GUI.Label(new Rect(x - 6, y + 30, 1300, 130), "D E D E K T İ F", _splashTitle);
            GUI.color = new Color(1, 1, 1, a2);
            GUI.Label(new Rect(x, y + 160, 1000, 50), "Karanlık Kasabanın Sırrı", _splashSub);
            Fill(new Rect(x, y + 222, 440f * a2, 2), new Color(Accent.r, Accent.g, Accent.b, 0.85f));
            GUI.Label(new Rect(x, y + 242, 640, 150),
                "Yağmur üç gündür dinmedi. Telgraf gece yarısı geldi: bir ceset, korkmuş bir kasaba ve herkesin sakladığı bir sır. " +
                "Otuz beş yaşında, yorgun ama keskin gözlü bir dedektifsiniz — ve bu gece yine yola çıkıyorsunuz.", _splashBody);
            GUI.color = new Color(1, 1, 1, a3);
            if (MenuLink(new Rect(x, y + 400, 520, 56), "SORUŞTURMAYA BAŞLA", true)) Do(GM.StartFromSplash);
            if (MenuLink(new Rect(x, y + 464, 520, 44), "AYARLAR", false)) Do(OpenSettings);
            if (MenuLink(new Rect(x, y + 514, 520, 44), "OYUNDAN ÇIK", false)) Do(AskQuit);
            GUI.Label(new Rect(x, y + 576, 900, 28), "WASD yürü   •   E incele / konuş   •   Q Dedektif Gözü   •   B vaka panosu   •   Esc duraklat", _splashFoot);
            GUI.Label(new Rect(x, VH * 0.925f - 40, 900, 26), (GM.Api.IsOnline ? "● Sorgu sunucusu bağlı" : "○ Sunucu kapalı — şüpheliler hazır cevaplarla konuşur") + $"   ({GM.Api.baseUrl})", _splashFoot);
            GUI.color = prev;
        }

        /// <summary>Afiş menüsü bağlantısı: kutu yok, üzerine gelince altın çizgi ve ok belirir.</summary>
        bool MenuLink(Rect r, string text, bool primary)
        {
            bool hover = r.Contains(Event.current.mousePosition);
            var col = primary ? Accent : TextPrimary;
            if (hover) col = Color.Lerp(col, Color.white, 0.35f);
            float off = hover ? 18f : 0f;
            if (hover) GUI.Label(new Rect(r.x - 4, r.y, 30, r.height), "▶", Styled(primary ? _menuLinkBig : _menuLink, Accent));
            GUI.Label(new Rect(r.x + off, r.y, r.width, r.height), text, Styled(primary ? _menuLinkBig : _menuLink, col));
            if (primary || hover) Fill(new Rect(r.x + off, r.yMax - 6, (primary ? 330f : 220f) * (hover ? 1f : 0.55f), 2), new Color(Accent.r, Accent.g, Accent.b, hover ? 0.95f : 0.5f));
            return GUI.Button(r, GUIContent.none, GUIStyle.none);
        }

        // ---------- 2. BÖLGE HARİTASI: dedektifin masasındaki 3D diorama ----------
        float _mapUiA; Rect _caseRect;

        Vector2? GuiPoint(Camera cam, Vector3 world)
        {
            var sp = cam.WorldToScreenPoint(world);
            if (sp.z <= 0.05f) return null;
            return new Vector2(sp.x / _scale, (Screen.height - sp.y) / _scale);
        }

        void EdgeVignette(float a)
        {
            for (int i = 0; i < 12; i++)
            {
                float m = (i + 1) / 12f * 0.17f;
                var c = new Color(0.008f, 0.009f, 0.014f, a * 0.11f);
                Fill(new Rect(0, 0, VW, VH * m), c); Fill(new Rect(0, VH - VH * m, VW, VH * m), c);
                Fill(new Rect(0, 0, VW * m, VH), c); Fill(new Rect(VW - VW * m, 0, VW * m, VH), c);
            }
        }

        void DrawWorldMap()
        {
            var map = World.WorldMapDiorama.Instance;
            var cam = Camera.main;
            if (map == null || !map.Active || cam == null) { DrawWorldMapFlat(); return; }
            float want = map.Diving ? 0f : Mathf.Clamp01((map.ShownFor - 1.8f) / 1.2f);
            if (Event.current.type == EventType.Repaint) _mapUiA = Mathf.MoveTowards(_mapUiA, want, Time.unscaledDeltaTime * 2.5f);
            EdgeVignette(0.8f);
            var mouse = Event.current.mousePosition;
            var prev = GUI.color;
            GUI.color = new Color(1, 1, 1, _mapUiA);

            for (int i = 0; i < 10; i++) Fill(new Rect(0, 0, 1180f * (1f - i / 10f), 170f), new Color(0.01f, 0.01f, 0.015f, 0.07f));
            GUI.Label(new Rect(64, 44, 1000, 62), "VAKA HARİTASI", _mapTitle);
            GUI.Label(new Rect(67, 104, 1100, 30), "Anadolu Vilayeti, 1887 — masanızda açık duran bölge haritası. Soruşturacağınız kasabayı seçin, Amirim.", _mutedItalic);
            Fill(new Rect(67, 140, 400, 2), new Color(Accent.r, Accent.g, Accent.b, 0.7f));

            World.WorldMapDiorama.Site hover = null;
            foreach (var s in map.Sites)
            {
                var top = GuiPoint(cam, map.WorldOf(s.PinTop));
                var ground = GuiPoint(cam, map.WorldOf(s.Local));
                if (!top.HasValue) continue;
                bool open = GM.IsTownUnlocked(s.Region), solved = GM.IsSolved(s.Region.Id), sel = map.Selected == s;
                var col = solved ? Success : open ? s.Theme : new Color(0.62f, 0.56f, 0.5f);
                var g = top.Value;
                const float w = 262f, h = 60f;
                var tag = new Rect(g.x - w / 2f, g.y - h - 30f, w, h);
                bool over = tag.Contains(mouse) || Vector2.Distance(mouse, g) < 28f || (ground.HasValue && Vector2.Distance(mouse, ground.Value) < 72f);
                if (over && _boxes.Count == 0) hover = s;
                bool hi = over || sel;
                Fill(new Rect(g.x - 1f, tag.yMax, 2f, Mathf.Max(0f, g.y - tag.yMax - 6f)), new Color(col.r, col.g, col.b, hi ? 0.95f : 0.5f));
                Fill(new Rect(tag.x + 4, tag.y + 5, tag.width, tag.height), new Color(0, 0, 0, 0.35f));
                Fill(tag, new Color(0.035f, 0.03f, 0.03f, hi ? 0.92f : 0.74f));
                Fill(new Rect(tag.x, tag.y, 5f, tag.height), col);
                Border(tag, new Color(col.r, col.g, col.b, hi ? 0.95f : 0.32f), 1);
                GUI.Label(new Rect(tag.x + 17, tag.y + 5, w - 24, 30), s.Region.Name, Styled(_mapSiteName, hi ? Color.white : TextPrimary));
                string lvl = s.Region.Level > 0 ? $"LEVEL {s.Region.Level}" : "MÜHÜRLÜ";
                string status = s.Region.Level <= 0 ? "yol kapalı" : solved ? "✓ dava çözüldü" : open ? "soruşturmaya açık" : "kilitli";
                GUI.Label(new Rect(tag.x + 17, tag.y + 34, w - 24, 22), $"{lvl}  •  {status}", Styled(_mapSiteStatus, col));
                var copy = s;
                if (GUI.Button(tag, GUIContent.none, GUIStyle.none)) Do(() => SelectSite(map, copy));
                if (ground.HasValue && GUI.Button(new Rect(ground.Value.x - 62, ground.Value.y - 62, 124, 124), GUIContent.none, GUIStyle.none)) Do(() => SelectSite(map, copy));
            }
            if (!map.Diving) map.Hover = hover;
            GUI.color = prev;

            _caseRect = new Rect(-1, -1, 0, 0);
            if (map.Selected != null && !map.Diving) DrawCaseFile(map, map.Selected);

            GUI.color = new Color(1, 1, 1, _mapUiA);
            if (map.Selected == null)
                GUI.Label(new Rect(0, VH - 58, VW, 28), "Kasabanın üstüne gelin ya da tıklayın   •   [← →] kasabalar arasında gezin   •   [Enter] dosyayı aç", _centerMuted);
            if (ColoredButton(new Rect(40, VH - 84, 190, 46), "← Ana Menü", new Color(0.13f, 0.12f, 0.12f))) Do(GM.BackToSplash);
            if (ColoredButton(new Rect(244, VH - 84, 160, 46), "Ayarlar", new Color(0.13f, 0.12f, 0.12f))) Do(OpenSettings);
            if (ColoredButton(new Rect(418, VH - 84, 290, 46), GM.UnlockAllForTesting ? "Test: Tüm Leveller Açık ✓" : "Test: Tüm Levelleri Aç", new Color(0.16f, 0.14f, 0.11f)))
                Do(() => GM.UnlockAllForTesting = !GM.UnlockAllForTesting);
            if (map.Selected == null) HelperWidget(new Rect(VW - 150, VH - 175, 130, 155));
            GUI.color = prev;

            // Boş bir yere tıklamak dosyayı kapatır
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && map.Selected != null && _boxes.Count == 0 && !_caseRect.Contains(mouse))
                Do(() => map.Select(null));
        }

        void SelectSite(World.WorldMapDiorama map, World.WorldMapDiorama.Site s)
        {
            if (map.Diving) return;
            map.Select(s);
            AudioManager.Instance?.TypeClick();
        }

        void CycleSite(World.WorldMapDiorama map, int dir)
        {
            var order = map.Sites.OrderBy(x => x.Local.x).ToList();
            if (order.Count == 0) return;
            int i = map.Selected != null ? order.IndexOf(map.Selected) : (dir > 0 ? -1 : order.Count);
            i = (i + dir + order.Count) % order.Count;
            SelectSite(map, order[i]);
        }

        void StartCase(World.WorldMapDiorama map, World.WorldMapDiorama.Site s)
        {
            if (s == null || map.Diving) return;
            if (!GM.IsTownUnlocked(s.Region)) { GM.SelectRegion(s.Region); return; }   // kilit mesajı
            map.Dive(s, () => GM.SelectRegion(s.Region));
        }

        /// <summary>Sağdan kayan vaka dosyası: manila kapak, daktilo bilgileri, damga, "Soruşturmaya Başla".</summary>
        void DrawCaseFile(World.WorldMapDiorama map, World.WorldMapDiorama.Site s)
        {
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((Time.time - map.SelectedAt) / 0.45f));
            float w = 520f, h = Mathf.Min(660f, VH - 190f);
            var r = new Rect(VW - w - 72f + (1f - k) * (w + 140f), (VH - h) / 2f + 18f, w, h);
            _caseRect = new Rect(r.x, r.y - 34f, r.width, r.height + 34f);
            bool open = GM.IsTownUnlocked(s.Region), solved = GM.IsSolved(s.Region.Id);
            var td = s.Data;
            var manila = new Color(0.7f, 0.58f, 0.38f, 1f);
            Fill(new Rect(r.x + 12, r.y + 14, r.width, r.height), new Color(0, 0, 0, 0.5f));
            Fill(new Rect(r.x + 28, r.y - 32, 200, 34), new Color(manila.r * 0.92f, manila.g * 0.92f, manila.b * 0.92f, 1f));
            GUI.Label(new Rect(r.x + 28, r.y - 32, 200, 34), "VAKA " + s.Number, _fileTab);
            Fill(r, manila);
            var p = new Rect(r.x + 16, r.y + 16, r.width - 32, r.height - 32);
            Fill(p, Paper);
            Fill(new Rect(p.x, p.y, p.width, 6), s.Theme);
            float x = p.x + 28, cw = p.width - 56, y = p.y + 24;
            GUI.Label(new Rect(x, y, cw - 150, 24), td != null ? td.CaseTitle.Split('—')[0].Trim() : "MÜHÜRLÜ DOSYA", _fileKicker); y += 28;
            GUI.Label(new Rect(x, y, cw, 56), s.Region.Name, _fileTitle); y += 60;
            Fill(new Rect(x, y, cw, 1), new Color(Ink.r, Ink.g, Ink.b, 0.45f)); y += 14;
            void Row(string key, string val)
            {
                GUI.Label(new Rect(x, y, 150, 26), key, _fileKey);
                GUI.Label(new Rect(x + 150, y, cw - 150, 26), val, _fileVal);
                y += 29;
            }
            if (td != null)
            {
                if (!string.IsNullOrEmpty(td.Victim) && td.Victim != "Kurban") Row("MAKTUL", td.Victim);
                Row("ŞÜPHELİLER", $"{td.Npcs.Count} kişi");
                Row("REHBER", string.IsNullOrEmpty(td.LocalHelperName) ? "Yardımcı Dedektif Çetin" : td.LocalHelperName);
                Row("SEVİYE", $"Level {s.Region.Level}");
            }
            else Row("DURUM", "Soruşturma izni verilmedi");
            y += 8;
            GUI.Label(new Rect(x, y, cw, 26), s.Weather, Styled(_fileWeather, Color.Lerp(s.Theme * 0.55f, Ink, 0.35f))); y += 36;
            float th = _fileBody.CalcHeight(new GUIContent(s.Teaser), cw);
            GUI.Label(new Rect(x, y, cw, th), s.Teaser, _fileBody);

            // Damga
            string stamp = solved ? "ÇÖZÜLDÜ" : !open ? (s.Region.Level <= 0 ? "MÜHÜRLÜ" : "KİLİTLİ") : "AÇIK DOSYA";
            var stampCol = solved ? new Color(0.14f, 0.42f, 0.2f, 0.85f) : !open ? new Color(0.6f, 0.08f, 0.06f, 0.8f) : new Color(0.18f, 0.24f, 0.42f, 0.6f);
            var m = GUI.matrix;
            var pivot = new Vector2(p.xMax - 128f, p.yMax - 152f);
            GUIUtility.RotateAroundPivot(-13f, pivot * _scale);
            var sr = new Rect(pivot.x - 104f, pivot.y - 28f, 208f, 56f);
            Border(sr, stampCol, 3);
            Border(new Rect(sr.x + 5, sr.y + 5, sr.width - 10, sr.height - 10), new Color(stampCol.r, stampCol.g, stampCol.b, stampCol.a * 0.6f), 1);
            GUI.Label(sr, stamp, Styled(_fileStamp, stampCol));
            GUI.matrix = m;

            var btn = new Rect(x, p.yMax - 96f, cw, 60f);
            if (open)
            {
                bool hov = btn.Contains(Event.current.mousePosition);
                Fill(btn, hov ? new Color(0.1f, 0.075f, 0.05f) : new Color(0.17f, 0.12f, 0.08f));
                Fill(new Rect(btn.x, btn.yMax - 4f, btn.width, 4f), s.Theme);
                if (GUI.Button(btn, solved ? "DOSYAYI YENİDEN AÇ   ▶" : "SORUŞTURMAYA BAŞLA   ▶", _fileButton)) { var c = s; Do(() => StartCase(map, c)); }
            }
            else
            {
                Fill(btn, new Color(0.35f, 0.27f, 0.2f, 0.18f));
                Border(btn, new Color(0.35f, 0.27f, 0.2f, 0.5f), 1);
                GUI.Label(btn, s.Region.Level <= 0 ? "Bu bölge henüz soruşturmaya kapalı" : $"Önce Level {s.Region.Level - 1} davasını çözün", _fileLocked);
            }
            GUI.Label(new Rect(x, p.yMax - 30f, cw, 20f), "[Enter] başla     [Esc] haritaya dön     [← →] diğer kasabalar", _fileHint);
        }

        // Yedek: 3D harita kurulamazsa eski görselli harita
        void DrawWorldMapFlat()
        {
            Fill(new Rect(0, 0, VW, VH), new Color(0.03f, 0.035f, 0.05f, 1f));
            float size = Mathf.Min(VH, VW);
            var map = new Rect((VW - size) / 2, (VH - size) / 2, size, size);
            var tex = T("world_map_generated");
            if (tex != null) GUI.DrawTexture(map, tex, ScaleMode.StretchToFill);
            Fill(new Rect(0, 0, VW, 120), new Color(0, 0, 0, 0.55f));
            GUI.Label(new Rect(0, 22, VW, 50), "BÖLGE HARİTASI", _mapHeader);
            GUI.Label(new Rect(0, 72, VW, 30), "Soruşturma yürütmek istediğiniz kasabayı seçin", _centerMuted);

            foreach (var reg in GameData.Regions)
            {
                var rr = new Rect(map.x + reg.MapRect.x / 100f * size, map.y + reg.MapRect.y / 100f * size, reg.MapRect.width / 100f * size, reg.MapRect.height / 100f * size);
                bool hover = rr.Contains(Event.current.mousePosition);
                bool open = GM.IsTownUnlocked(reg), solved = GM.IsSolved(reg.Id);
                var col = solved ? Success : open ? Accent : Danger;
                if (hover) { Fill(rr, new Color(col.r, col.g, col.b, 0.12f)); Border(rr, col, 2); }
                var tag = new Rect(rr.center.x - 150, rr.center.y - 34, 300, 68);
                Panel(tag, new Color(0, 0, 0, hover ? 0.85f : 0.68f), col);
                GUI.Label(new Rect(tag.x, tag.y + 6, tag.width, 30), (open ? "" : "🔒 ") + (reg.Level > 0 ? $"LEVEL {reg.Level} — " : "") + reg.Name, Styled(_centerBold, col));
                string status = reg.Level <= 0 ? "Kilitli Bölge" : solved ? "✓ Dava Çözüldü" : open ? "Soruşturmaya Açık" : $"Kilitli — önce Level {reg.Level - 1}";
                GUI.Label(new Rect(tag.x, tag.y + 36, tag.width, 26), status, Styled(_centerSmall, col));
                if (GUI.Button(rr, GUIContent.none, GUIStyle.none)) { var copy = reg; Do(() => GM.SelectRegion(copy)); }
            }
            if (ColoredButton(new Rect(25, 25, 170, 50), "← Ana Menü", new Color(0.15f, 0.17f, 0.2f))) Do(GM.BackToSplash);
            if (ColoredButton(new Rect(25, VH - 75, 290, 46), GM.UnlockAllForTesting ? "Test: Tüm Leveller Açık ✓" : "Test: Tüm Levelleri Aç", new Color(0.18f, 0.16f, 0.12f)))
                Do(() => GM.UnlockAllForTesting = !GM.UnlockAllForTesting);
            HelperWidget(new Rect(VW - 150, VH - 175, 130, 155));
        }

        void HelperWidget(Rect r)
        {
            var tex = T("dedektif_helper");
            bool hover = r.Contains(Event.current.mousePosition);
            if (tex != null) GUI.DrawTexture(new Rect(r.x, r.y, r.width, r.height - 26), tex, ScaleMode.ScaleToFit);
            Panel(new Rect(r.x - 10, r.yMax - 28, r.width + 20, 28), new Color(0, 0, 0, 0.75f), hover ? Accent : Accent * 0.6f);
            GUI.Label(new Rect(r.x - 10, r.yMax - 28, r.width + 20, 28), "YARDIMCI DEDEKTİF", _accentSmallCenter);
            if (GUI.Button(r, GUIContent.none, GUIStyle.none)) Do(() => GM.TriggerHelper("splash", null, false));
        }

        // ---------- 3. HİKAYE (daktilo) ----------
        void DrawStory()
        {
            if (Time.time < _storyStart) { DrawChapterCard(); return; }
            // Arkada CinematicDirector olay yerinin etrafında ağır çekim yapar; metin film altyazısı gibi akar
            Fill(new Rect(0, 0, VW, VH), new Color(0.01f, 0.01f, 0.02f, 0.25f));
            DrawLetterbox();
            GUI.Label(new Rect(60, Bar * 0.5f - 18, VW, 36), GameData.CaseTitle, Styled(_badge, new Color(0.86f, 0.7f, 0.4f)));
            GUI.Label(new Rect(0, Bar * 0.5f - 18, VW - 60, 36), GameData.Current.Name.ToUpperInvariant() + $"  •  LEVEL {GameData.Current.Level}", _rightMuted);
            int chars = StoryChars;
            string story = GameData.StoryText;
            bool done = chars >= story.Length;
            // Cümle cümle altyazı: imlecin bulunduğu cümle (ve bir önceki, soluk)
            var parts = System.Text.RegularExpressions.Regex.Split(story, @"(?<=[\.\!\?…])\s+");
            int acc = 0, idx = 0;
            for (int i = 0; i < parts.Length; i++) { if (chars <= acc + parts[i].Length) { idx = i; break; } acc += parts[i].Length + 1; idx = i; }
            string cur = parts[idx].Substring(0, Mathf.Clamp(chars - acc, 0, parts[idx].Length));
            if (idx > 0)
            {
                var prev = GUI.color; GUI.color = new Color(1, 1, 1, 0.45f);
                Subtitle(null, parts[idx - 1], Accent, 110f);
                GUI.color = prev;
            }
            Subtitle(idx == 0 ? "ANLATICI" : null, cur + (done ? "" : (Time.time % 1f > 0.5f ? "▌" : " ")), new Color(0.86f, 0.7f, 0.4f));
            var btn = new Rect(VW - 360, VH - Bar + (Bar - 56) / 2f, 320, 56);
            if (!done) { if (ColoredButton(btn, "Hikayeyi Geç  ▶▶", new Color(0.15f, 0.14f, 0.12f))) Do(() => _storySkipped = true); }
            else if (ColoredButton(btn, "SORUŞTURMAYI BAŞLAT", new Color(0.45f, 0.3f, 0.12f))) Do(GM.FinishStory);
        }

        // ---------- KASABAYA İNİŞ (sinematik) ----------
        void DrawDescent()
        {
            float p = GM.Director != null ? GM.Director.DescentProgress : 1f;
            float bar = 110f * Mathf.Clamp01(Mathf.Min(p * 6f, (1f - p) * 6f + 0.2f));
            Fill(new Rect(0, 0, VW, bar), Color.black);
            Fill(new Rect(0, VH - bar, VW, bar), Color.black);
            float a = Mathf.Clamp01(Mathf.Min((p - 0.08f) * 5f, (0.85f - p) * 5f));
            var prev = GUI.color; GUI.color = new Color(1, 1, 1, a);
            GUI.Label(new Rect(80, VH - 300, VW, 70), GameData.TownName, _descentTitle);
            GUI.Label(new Rect(84, VH - 232, VW, 36), $"Level {GameData.Current.Level}  •  {GameData.Current.Victim} cinayeti  •  1887 — {GameData.Current.Mood}", _descentSub);
            GUI.color = prev;
            GUI.Label(new Rect(0, VH - 60, VW - 40, 30), "[Boşluk] Geç", _rightMuted);
        }

        // ---------- 4. HUD — kutusuz, sinematik: yazılar sahnenin üstünde süzülür ----------
        float _ctlUntil;
        bool ControlsVisible => _showControls && Time.time < _ctlUntil;

        void KeyBadge(Rect r, string key)
        {
            Fill(r, new Color(0.05f, 0.045f, 0.04f, 0.82f));
            Border(r, new Color(Accent.r, Accent.g, Accent.b, 0.85f), 1);
            GUI.Label(r, key, _hudKey);
        }

        bool HudLink(Rect r, string key, string label, Color col)
        {
            bool hover = r.Contains(Event.current.mousePosition);
            float kw = key == null ? 0f : Mathf.Max(30f, _hudKey.CalcSize(new GUIContent(key)).x + 14f);
            if (key != null) KeyBadge(new Rect(r.x, r.y + (r.height - 30f) / 2f, kw, 30f), key);
            var lr = new Rect(r.x + kw + (key != null ? 10f : 0f), r.y, r.width - kw - 10f, r.height);
            GUI.Label(new Rect(lr.x + 1, lr.y + 2, lr.width, lr.height), label, Styled(_hudLink, new Color(0, 0, 0, 0.8f)));
            GUI.Label(lr, label, Styled(_hudLink, hover ? Color.Lerp(col, Color.white, 0.4f) : col));
            if (hover) Fill(new Rect(lr.x, r.yMax - 4, Mathf.Min(lr.width, _hudLink.CalcSize(new GUIContent(label)).x), 2), col);
            return GUI.Button(r, GUIContent.none, GUIStyle.none);
        }

        void ShadowLabel(Rect r, string text, GUIStyle st)
        {
            GUI.Label(new Rect(r.x + 1.5f, r.y + 2f, r.width, r.height), text, Styled(st, new Color(0, 0, 0, 0.85f)));
            GUI.Label(r, text, st);
        }

        void DrawHud()
        {
            // Üst kenarda yumuşak karartma: yazılar okunur, kutu yok
            for (int i = 0; i < 10; i++) Fill(new Rect(0, 0, VW, 150f * (1f - i / 10f)), new Color(0, 0, 0, 0.045f));

            // Sol üst: kasaba + yer + cep saati
            var dn = DayNightCycle.Instance;
            bool rainy = GameData.Current == null || GameData.Current.RainMax > 0.02f;
            string wx = rainy ? "yağmurlu" : "sisli";
            string weather = dn == null || dn.IsNight ? $"{wx} gece" : dn.PartOfDay == "Şafak" || dn.PartOfDay == "Akşam" ? $"{wx} {dn.PartOfDay.ToLowerInvariant()}" : rainy ? "kurşuni, yağmurlu gün" : "sisli, solgun gün";
            var theme = GameData.Current != null ? GameData.Current.ThemeColor : Accent;
            Fill(new Rect(30, 28, 4, 58), theme);
            ShadowLabel(new Rect(46, 22, 700, 40), GameData.TownName.ToUpperInvariant(), _hudTown);
            ShadowLabel(new Rect(47, 60, 760, 28), GM.InsideBuilding ? $"{GM.CurrentBuilding.DisplayName} — olay yeri incelemesi" : $"Kasaba sokakları — {weather}", _hudPlace);
            if (_modal == Modal.None)
            {
                if (!GM.InsideBuilding && HudLink(new Rect(46, 96, 230, 34), "Esc", "Duraklat", TextMuted)) Do(OpenPause);
                if (GM.InsideBuilding && HudLink(new Rect(46, 96, 230, 34), null, "← Kasabaya dön", TextMuted)) Do(GM.RequestExitBuilding);
            }
            DrawPocketWatch(new Vector2(34, 160));
            if (_modal == Modal.None) DrawObjectives();
            DrawVisionOverlay();

            // Üst orta: otopsi durumu (ince yazı + tema çizgisi)
            var aCol = GM.AutopsyReady ? Success : GM.AutopsyTimerStarted ? Warning : new Color(0.82f, 0.77f, 0.68f);
            var au = new Rect(VW / 2 - 300, 26, 600, 30);
            ShadowLabel(au, GM.AutopsyStatus, Styled(_hudCenter, aCol));
            Fill(new Rect(VW / 2 - 90, 60, 180, 1), new Color(aCol.r, aCol.g, aCol.b, 0.6f));
            if (GM.AutopsyReady && GUI.Button(au, GUIContent.none, GUIStyle.none)) Do(() => _modal = Modal.Autopsy);

            // Sağ üst: defter bağlantıları
            float rx = VW - 40;
            var foundR = new Rect(rx - 170, 26, 170, 36); rx = foundR.x - 34;
            if (HudLink(foundR, "F", "BULDUM!", new Color(0.92f, 0.36f, 0.32f))) Do(GM.OpenAccuse);
            var bagR = new Rect(rx - 200, 26, 200, 36); rx = bagR.x - 34;
            if (HudLink(bagR, "Tab", $"Çanta {GM.BagCount}/{GameData.MaxBagSize}", TextPrimary)) Do(OpenBag);
            var labCol = GM.LabCount >= GameData.ReqLabs ? Success : Warning;
            HudLink(new Rect(rx - 210, 26, 210, 36), null, GM.LabStatus, labCol);
            ShadowLabel(new Rect(VW - 420, 70, 380, 24), GM.Api.IsOnline ? "● sorgu sunucusu bağlı" : "○ yerel mod — sunucu kapalı", Styled(_rightMuted, GM.Api.IsOnline ? Success : TextMuted));

            // Alt orta: etkileşim ipucu
            var focus = GM.Player.Focus;
            if (_modal == Modal.None && focus != null)
            {
                float w = Mathf.Min(820f, _hudPrompt.CalcSize(new GUIContent(focus.Prompt)).x + 110f);
                var pr = new Rect(VW / 2 - w / 2, VH - 138, w, 50);
                for (int i = 0; i < 6; i++) Fill(new Rect(pr.x - i * 18, pr.y, pr.width + i * 36, pr.height), new Color(0, 0, 0, 0.07f));
                Fill(new Rect(pr.x + 20, pr.yMax - 2, pr.width - 40, 1), new Color(Accent.r, Accent.g, Accent.b, 0.7f));
                KeyBadge(new Rect(pr.x + 14, pr.y + 9, 32, 32), "E");
                ShadowLabel(new Rect(pr.x + 58, pr.y, pr.width - 64, pr.height), focus.Prompt, _hudPrompt);
            }
            // Sol alt: kontroller (vakanın ilk 40 saniyesi; F1 ile aç/kapa)
            if (ControlsVisible && _modal == Modal.None)
            {
                var hr = new Rect(30, VH - 294, 470, 268);
                for (int i = 0; i < 8; i++) Fill(new Rect(hr.x - 30, hr.y + i * 4, hr.width * (1f - i * 0.06f), hr.height - i * 8), new Color(0, 0, 0, 0.05f));
                ShadowLabel(new Rect(hr.x, hr.y + 8, hr.width, 26), "KONTROLLER", Styled(_accentSmall, Accent));
                string[] rows =
                {
                    "W A S D|Yürü  •  Shift koş  •  Fare kamera",
                    "E|Kapı, delil, konuşma, çekmece / sandık ara",
                    "Q|Dedektif Gözü — saklanmış izleri sez",
                    "Tab|Çanta ve adli laboratuvar   •   R otopsi",
                    "H|Yardımcı Dedektif   •   F suçluyu göster",
                    "B|Vaka panosu   •   C sinema modu   •   Esc duraklat",
                    "F1|Bu listeyi gizle   •   M ses",
                };
                float yy = hr.y + 40;
                foreach (var row in rows)
                {
                    var parts = row.Split('|');
                    float kw = Mathf.Max(30f, _hudKey.CalcSize(new GUIContent(parts[0])).x + 14f);
                    KeyBadge(new Rect(hr.x, yy, kw, 28), parts[0]);
                    ShadowLabel(new Rect(hr.x + kw + 12, yy, hr.width - kw - 12, 28), parts[1], _hudSmall);
                    yy += 32;
                }
            }
        }

        // ---------- KAPI GİRİŞİ (building-entry-modal) ----------
        void DrawDoorEntry()
        {
            if (_door == null) { CloseModal(); return; }
            Fill(new Rect(0, 0, VW, VH), new Color(0.01f, 0.01f, 0.02f, 0.88f));
            var c = _door.Neon;
            for (int i = 0; i < 6; i++) Fill(new Rect(VW / 2 - 300 - i * 60, VH / 2 - 260 - i * 30, 600 + i * 120, 520 + i * 60), new Color(c.r, c.g, c.b, 0.025f));
            var frame = new Rect(VW / 2 - 330, VH / 2 - 270, 660, 540);
            Border(frame, c, 3);
            GUILayout.BeginArea(Pad(frame, 40));
            GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
            GUILayout.Label("BİNA GİRİŞİ", Styled(_badgeCenter, c), GUILayout.Width(300));
            GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();
            GUILayout.Label(_door.DisplayName, Styled(_doorTitle, Color.Lerp(c, Color.white, 0.3f)));
            GUILayout.Label("İçeri girmek için kapıyı çalın", _centerMuted);
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
            if (GlowButton("KAPIYI ÇAL / İÇERİ GİR", c, 440, 74, 24)) { var d = _door; Do(() => { CloseModal(); GM.EnterBuilding(d); }); }
            GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndArea();
            if (ColoredButton(new Rect(25, 25, 200, 50), "← Kasabaya Dön", new Color(0.15f, 0.17f, 0.2f))) Do(CloseModal);
        }

        // ---------- BİNA İÇİ DELİL KARTI (building-clue-modal) ----------
        void DrawBuildingClue()
        {
            if (_clue == null) { CloseModal(); return; }
            GUI.Label(new Rect(60, Bar * 0.5f - 16, VW, 32), "OLAY YERİ İNCELEME  •  " + (GM.CurrentBuilding?.DisplayName ?? ""), Styled(_badge, new Color(0.86f, 0.7f, 0.4f)));
            Subtitle(_clue.Def.Name, _clue.Def.Description ?? "", new Color(0.86f, 0.7f, 0.4f));
            float by = VH - Bar + (Bar - 54) / 2f;
            var b1 = new Rect(VW / 2 - 290, by, 280, 54);
            var b2 = new Rect(VW / 2 + 10, by, 280, 54);
            if (_clue.InBag) { Panel(b1, new Color(0, 0, 0, 0.6f), Success); GUI.Label(b1, "✓ Çantada Mevcut", Styled(_centerBold, Success)); }
            else if (ColoredButton(b1, "Çantaya Al", new Color(0.4f, 0.28f, 0.12f))) { var id = _clue.Id; Do(() => { GM.TakeClue(id); if (GM.GetClue(id).InBag) CloseModal(); }); }
            if (ColoredButton(b2, "Kapat  [Esc]", new Color(0.15f, 0.14f, 0.12f))) Do(CloseModal);
        }

        // ---------- ÇANTA (bag-modal) + Dedektif Notları ----------
        void DrawBag()
        {
            Dim();
            var r = Center(1240, 780);
            Panel(r, BgCard, Accent);
            GUI.Label(new Rect(r.x + 30, r.y + 22, 700, 44), $"DEDEKTİF ÇANTASI   <color=#9c8f7a>{GM.BagCount}/{GameData.MaxBagSize}</color>", _h2);
            if (ColoredButton(new Rect(r.xMax - 70, r.y + 20, 50, 46), "X", new Color(0.2f, 0.22f, 0.26f))) Do(CloseModal);
            var left = new Rect(r.x + 30, r.y + 80, r.width * 0.6f - 40, r.height - 170);
            var right = new Rect(left.xMax + 24, left.y, r.xMax - left.xMax - 54, left.height);
            GUI.Label(new Rect(left.x, left.y - 4, left.width, 28), "Toplanan İpuçları", _accentSmall);
            GUILayout.BeginArea(new Rect(left.x, left.y + 28, left.width, left.height - 28));
            _bagScroll = GUILayout.BeginScrollView(_bagScroll);
            var bag = GM.Bag.ToList();
            if (bag.Count == 0) GUILayout.Label("Çantan boş. Binalara gir, delilleri [E] ile incele ve çantaya al. Saklanmış deliller için [Q] Dedektif Gözü!", _bodyLarge);
            foreach (var c in bag)
            {
                GUILayout.BeginHorizontal(_card, GUILayout.Height(104));
                var thumb = GUILayoutUtility.GetRect(84, 84, GUILayout.Width(84), GUILayout.Height(84));
                Fill(thumb, new Color(0, 0, 0, 0.35f));
                var tx = T(c.Def.Image); if (tx != null) GUI.DrawTexture(Pad(thumb, 4), tx, ScaleMode.ScaleToFit);
                GUILayout.Space(12);
                GUILayout.BeginVertical();
                GUILayout.Label($"<b>{c.Def.Name}</b>  <color=#9c8f7a>— {GameData.BuildingOf(c.Def.BuildingNpcId)?.DisplayName}</color>", _body);
                GUILayout.Label(c.Def.Description ?? "", _small);
                string marks = (c.BloodSent ? "<color=#c0504d>● Kan lab'da</color>  " : c.BloodFound ? "<color=#c0504d>○ Kan bulundu</color>  " : "") +
                               (c.PrintSent ? "<color=#d4a65a>● İz lab'da</color>" : c.PrintFound ? "<color=#d4a65a>○ İz bulundu</color>" : "");
                if (marks.Length > 0) GUILayout.Label(marks, _small);
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical(); GUILayout.FlexibleSpace();
                if (GlowButton("İncele / Lab", Warning, 170, 46)) { var id = c.Id; Do(() => GM.OpenLabInspect(id)); }
                GUILayout.FlexibleSpace(); GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.Space(6);
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
            GUI.Label(new Rect(right.x, right.y - 4, right.width, 28), "Dedektif Notları", _accentSmall);
            GM.DetectiveNotes = GUI.TextArea(new Rect(right.x, right.y + 28, right.width, right.height - 28), GM.DetectiveNotes, 2000, _notes);
            if (ColoredButton(new Rect(r.x + 30, r.yMax - 74, 300, 50), "Çetin'e Analiz Ettir", new Color(0.08f, 0.4f, 0.45f))) Do(() => { CloseModal(); GM.AnalyzeBag(); });
            if (ColoredButton(new Rect(r.xMax - 250, r.yMax - 74, 220, 50), "Çantayı Kapat [Tab]", new Color(0.2f, 0.22f, 0.26f))) Do(CloseModal);
        }

        // ---------- ADLİ LABORATUVAR (clue-inspect-modal: UV / toz / büyüteç) ----------
        void DrawLab()
        {
            if (_clue == null) { CloseModal(); return; }
            Fill(new Rect(0, 0, VW, VH), new Color(0, 0, 0, 0.85f));
            var r = Center(1500, 860);
            Panel(r, BgCard, Accent);
            var info = new Rect(r.x + 36, r.y + 30, 480, r.height - 60);
            var vis = new Rect(info.xMax + 30, r.y + 30, r.xMax - info.xMax - 66, r.height - 60);
            var c = _clue;

            GUILayout.BeginArea(info);
            GUILayout.Label("ADLİ LABORATUVAR İNCELEME", _badge);
            GUILayout.Space(6);
            GUILayout.Label(c.Def.Name, _h2);
            Fill(GUILayoutUtility.GetRect(info.width, 2), Accent * 0.5f);
            GUILayout.Space(8);
            GUILayout.Label(c.Def.Description ?? "", _bodyLarge);
            GUILayout.Space(14);
            GUILayout.Label("Sağdaki araçlardan birini seçip fareyi delilin üzerinde basılı tutarak tarayın.", _small);
            GUILayout.Space(12);
            if (c.BloodChecked) FindingRow(c, true);
            if (c.PrintChecked) FindingRow(c, false);
            GUILayout.FlexibleSpace();
            if (c.Def.Document != null && GlowButton("İçeriği Oku / İncele", Accent, 480, 50)) Do(() => { _showDoc = true; _docScroll = Vector2.zero; });
            GUILayout.Space(8);
            if (GlowButton("Kapat", TextMuted, 480, 50)) Do(CloseModal);
            GUILayout.EndArea();

            // Görsel alanı
            var bg = T(GameData.Npc(c.Def.BuildingNpcId)?.Portrait);
            Fill(vis, new Color(0.02f, 0.02f, 0.03f, 1f));
            if (bg != null) { var p = GUI.color; GUI.color = new Color(1, 1, 1, 0.12f); GUI.DrawTexture(vis, bg, ScaleMode.ScaleAndCrop); GUI.color = p; }
            float s = Mathf.Min(vis.width - 160, vis.height - 60);
            var img = new Rect(vis.x + (vis.width - 120 - s) / 2, vis.y + (vis.height - s) / 2, s, s);
            var tex = T(c.Def.Image);
            if (tex != null) GUI.DrawTexture(img, tex, ScaleMode.ScaleToFit);
            if (_tool == Tool.UV) Fill(img, new Color(0.35f, 0.1f, 0.85f, 0.35f));
            // Bulunan izleri delilin üstüne çiz
            if (c.BloodFound && (_tool == Tool.UV || c.BloodSent)) Stamp(img, c.Def.BloodSpot, T("real_blood_stain"), new Color(1f, 0.25f, 0.3f, 0.9f));
            if (c.PrintFound && (_tool == Tool.Dust || c.PrintSent)) Stamp(img, c.Def.PrintSpot, T("real_fingerprint"), new Color(0.8f, 0.95f, 1f, 0.9f));

            // Araç butonları (forensic-tools)
            var tools = new[] { (Tool.UV, "UV\nIşığı", new Color(0.55f, 0.3f, 1f)), (Tool.Dust, "Tozlama\nFırçası", new Color(0.8f, 0.8f, 0.85f)), (Tool.Magnifier, "Büyüteç", Accent) };
            for (int i = 0; i < tools.Length; i++)
            {
                var tr = new Rect(vis.xMax - 110, vis.y + 30 + i * 120, 90, 100);
                bool on = _tool == tools[i].Item1;
                Panel(tr, on ? new Color(tools[i].Item3.r, tools[i].Item3.g, tools[i].Item3.b, 0.3f) : BgGlass, tools[i].Item3);
                var t = tools[i].Item1;
                if (GUI.Button(tr, tools[i].Item2, Styled(_centerSmallBold, tools[i].Item3))) Do(() => { _tool = _tool == t ? Tool.None : t; _scanTime = _spotTime = 0f; });
            }

            // Tarama etkileşimi
            var mp = Event.current.mousePosition;
            if (_tool != Tool.None && img.Contains(mp) && !_showDoc)
            {
                var lens = new Rect(mp.x - 70, mp.y - 70, 140, 140);
                if (_tool == Tool.Magnifier && tex != null)
                {
                    float u = (mp.x - img.x) / img.width, v = 1f - (mp.y - img.y) / img.height;
                    GUI.DrawTextureWithTexCoords(lens, tex, new Rect(u - 0.12f, v - 0.12f, 0.24f, 0.24f));
                }
                else Fill(lens, _tool == Tool.UV ? new Color(0.5f, 0.2f, 1f, 0.25f) : new Color(1f, 1f, 1f, 0.12f));
                Border(lens, _tool == Tool.UV ? new Color(0.6f, 0.3f, 1f) : Accent, 2);
                if (Event.current.type == EventType.Repaint && Mouse.current != null && Mouse.current.leftButton.isPressed && _tool != Tool.Magnifier)
                    Scan(c, img, mp);
            }
            if (_tool == Tool.UV || _tool == Tool.Dust)
            {
                bool blood = _tool == Tool.UV;
                bool checkedAlready = blood ? c.BloodChecked : c.PrintChecked;
                string hint = checkedAlready ? (blood ? (c.BloodFound ? "Kan lekesi tespit edildi." : "UV taraması tamamlandı — temiz.") : (c.PrintFound ? "Parmak izi tespit edildi." : "Tozlama tamamlandı — temiz."))
                                             : $"Taranıyor... {Mathf.RoundToInt(Mathf.Clamp01(_scanTime / 2.5f) * 100)}%  (fareyi delilin üzerinde basılı tutun)";
                GUI.Label(new Rect(vis.x, vis.yMax - 44, vis.width - 120, 30), hint, _centerMuted);
            }
            if (_showDoc && c.Def.Document != null) DrawDocument(vis, c);
        }

        void Scan(ClueRuntime c, Rect img, Vector2 mp)
        {
            bool blood = _tool == Tool.UV;
            if (blood ? c.BloodChecked : c.PrintChecked) return;
            bool has = blood ? c.Def.HasBlood : c.Def.HasPrint;
            var spot = blood ? c.Def.BloodSpot : c.Def.PrintSpot;
            var rel = new Vector2((mp.x - img.x) / img.width, (mp.y - img.y) / img.height);
            float dt = Time.unscaledDeltaTime;
            _scanTime += dt;
            if (has && Vector2.Distance(rel, spot) < 0.14f) _spotTime += dt;
            int id = c.Id;
            if (has && _spotTime > 0.6f) { _spotTime = 0; Do(() => GM.ReportForensicScan(id, blood)); }
            else if (!has && _scanTime > 2.5f) { _scanTime = 0; Do(() => GM.ReportForensicScan(id, blood)); }
        }

        void FindingRow(ClueRuntime c, bool blood)
        {
            bool found = blood ? c.BloodFound : c.PrintFound;
            bool sent = blood ? c.BloodSent : c.PrintSent;
            var col = found ? (blood ? new Color(1f, 0.35f, 0.4f) : Accent) : Success;
            GUILayout.BeginVertical(_card);
            GUILayout.Label(GameManager.FindingText(blood, found), Styled(_bodyBold, col));
            if (found)
            {
                if (sent) GUILayout.Label($"✓ {(blood ? "KAN LEKESİ" : "PARMAK İZİ")} ADLİ TIBBA GÖNDERİLDİ", Styled(_small, Success));
                else if (GlowButton(blood ? "KAN LEKESİNİ ADLİ TIBBA GÖNDER" : "PARMAK İZİNİ ADLİ TIBBA GÖNDER", col, 440, 46, 16)) { int id = c.Id; Do(() => GM.SendToForensics(id, blood)); }
            }
            GUILayout.EndVertical();
            GUILayout.Space(8);
        }

        void Stamp(Rect img, Vector2 spot, Texture2D tex, Color tint)
        {
            var r = new Rect(img.x + spot.x * img.width - 50, img.y + spot.y * img.height - 50, 100, 100);
            var p = GUI.color; GUI.color = tint;
            if (tex != null) GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit); else Fill(Pad(r, 30), tint);
            GUI.color = p;
            Border(r, tint, 2);
        }

        void DrawDocument(Rect area, ClueRuntime c)
        {
            Fill(area, new Color(0, 0, 0, 0.75f));
            var paper = new Rect(area.x + 60, area.y + 50, area.width - 120, area.height - 100);
            var tex = T("old_paper_texture");
            if (tex != null) GUI.DrawTexture(paper, tex, ScaleMode.ScaleAndCrop); else Fill(paper, Paper);
            GUILayout.BeginArea(Pad(paper, 50));
            _docScroll = GUILayout.BeginScrollView(_docScroll);
            GUILayout.Label(c.Def.Document, _docText);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
            if (ColoredButton(new Rect(paper.xMax - 240, paper.y - 30, 230, 46), "İncelemeyi Bitir", new Color(0.2f, 0.15f, 0.1f))) Do(() => _showDoc = false);
        }

        // ---------- SORGU (npc-talk-modal) ----------
        void DrawInterrogation()
        {
            var n = _npc;
            if (n == null) { CloseModal(); return; }
            // Üst şerit: geri, başlık, kalan soru, nabız monitörü
            if (ColoredButton(new Rect(30, (Bar - 46) / 2f, 170, 46), "← Geri Dön", new Color(0.15f, 0.14f, 0.12f))) Do(CloseModal);
            int remaining = Mathf.Max(0, GameData.MaxQuestions - n.Asked);
            GUI.Label(new Rect(220, (Bar - 30) / 2f, 700, 30), $"SORGU  •  {n.Def.Name.ToUpperInvariant()}   <color=#9c8f7a>{n.Def.Role}</color>", Styled(_badge, new Color(0.86f, 0.7f, 0.4f)));
            var sc = n.Stress >= 75 ? Danger : n.Stress >= 40 ? Warning : Success;
            var mon = new Rect(VW - 560, (Bar - 64) / 2f, 530, 64);
            float beat = Mathf.Pow(Mathf.Abs(Mathf.Sin(Time.time * Mathf.PI * n.Bpm / 60f)), 8f);
            GUI.Label(new Rect(mon.x, mon.y, 170, 34), $"<size={24 + (int)(beat * 5)}>♥</size> <b>{n.Bpm}</b> BPM", Styled(_body, sc));
            GUI.Label(new Rect(mon.x + 170, mon.y + 2, 200, 30), n.Symptom, Styled(_smallBold, sc));
            GUI.Label(new Rect(mon.x + 380, mon.y + 2, 150, 30), $"Hak: {remaining}/{GameData.MaxQuestions}", Styled(_rightSmall, remaining > 0 ? Accent : Danger));
            var track = new Rect(mon.x, mon.y + 44, mon.width, 8);
            Fill(track, new Color(1, 1, 1, 0.1f));
            Fill(new Rect(track.x, track.y, track.width * n.Stress / 100f, track.height), sc);

            // Sol: konuşma geçmişi (yarı saydam, film karesini kapatmaz)
            var log = new Rect(30, Bar + 20, 470, VH - Bar * 2 - 220);
            Fill(log, new Color(0, 0, 0, 0.35f));
            if (n.Log.Count != _lastChatCount || !n.Log.All(l => l.Typed)) { _chatScroll.y = float.MaxValue; _lastChatCount = n.Log.Count; }
            GUILayout.BeginArea(Pad(log, 14));
            _chatScroll = GUILayout.BeginScrollView(_chatScroll);
            foreach (var line in n.Log)
            {
                GUILayout.Label(line.FromPlayer ? $"<color=#d9b26a><b>Dedektif:</b></color> {line.Text}" : $"<color=#e6dcc6><b>{line.Speaker}:</b></color> {line.Visible}", _small);
                GUILayout.Space(6);
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();

            // Alt: son cevap altyazı olarak (şüphelinin portresiyle)
            var last = n.Log.LastOrDefault(l => !l.FromPlayer);
            var lastQ = n.Log.LastOrDefault(l => l.FromPlayer);
            if (n.Waiting) Subtitle(n.Def.Name, $"*{n.Def.Name} bir süre susar*" + new string('.', 1 + (int)(Time.time * 3) % 3), new Color(0.75f, 0.8f, 0.85f), 0f, T(n.Def.Portrait));
            else if (last != null && (lastQ == null || n.Log.IndexOf(last) > n.Log.IndexOf(lastQ))) Subtitle(n.Def.Name, last.Visible, new Color(0.75f, 0.8f, 0.85f), 0f, T(n.Def.Portrait));
            else if (n.Log.Count == 0) Subtitle(n.Def.Name, string.IsNullOrEmpty(n.Def.Greeting) ? $"*{n.Def.Name} size şüpheyle bakıyor...*" : n.Def.Greeting, new Color(0.75f, 0.8f, 0.85f), 0f, T(n.Def.Portrait));

            // Sağ: soru seçenekleri + serbest soru
            var qa = new Rect(VW - 600, Bar + 20, 570, VH - Bar * 2 - 230);
            bool typing = n.Waiting || !n.Log.All(l => l.Typed);
            if (n.LeftInterrogation) GUI.Label(new Rect(qa.x, qa.y, qa.width, 80), "(!) Şüpheli aşırı stres ve panikle sorguyu terk etti!", Styled(_bodyBold, Danger));
            else if (n.Finished) GUI.Label(new Rect(qa.x, qa.y, qa.width, 80), $"✓ Sorgu tamamlandı ({GameData.MaxQuestions}/{GameData.MaxQuestions}).", Styled(_bodyBold, Success));
            else if (!typing)
            {
                var list = n.Suggestions.Take(4).ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    var br = new Rect(qa.x, qa.y + i * 70, qa.width, 62);
                    var q = list[i];
                    if (GUI.Button(br, $"<color=#d9b26a>{i + 1}.</color>  {q.Q}", _questionStyle)) { int id = n.Id; Do(() => GM.AskQuestion(id, q)); }
                }
                var input = new Rect(qa.x, qa.y + 4 * 70 + 10, qa.width - 120, 54);
                GUI.SetNextControlName("free_q");
                if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter) && GUI.GetNameOfFocusedControl() == "free_q")
                {
                    var txt = _question; int id = n.Id; _question = ""; Event.current.Use();
                    Do(() => GM.AskFree(id, txt));
                }
                _question = GUI.TextField(input, _question, 150, _input);
                if (string.IsNullOrEmpty(_question) && GUI.GetNameOfFocusedControl() != "free_q")
                    GUI.Label(new Rect(input.x + 16, input.y, input.width, input.height), "Kendi sorunu yaz...", _placeholder);
                if (ColoredButton(new Rect(input.xMax + 10, input.y, 110, 54), "SOR", new Color(0.45f, 0.3f, 0.12f))) { var txt = _question; int id = n.Id; _question = ""; Do(() => GM.AskFree(id, txt)); }
            }
        }

        // ---------- SUÇLAMA (found-modal) ----------
        void DrawFound()
        {
            DrawCover(T("town_bg"), new Rect(0, 0, VW, VH));
            Fill(new Rect(0, 0, VW, VH), new Color(0, 0, 0, 0.88f));
            GUI.Label(new Rect(0, 40, VW, 56), "KİMİ SUÇLAYACAKSIN AMİRİMS?", _foundTitle);
            GUI.Label(new Rect(0, 100, VW, 30), "Dosyayı kapatma vakti geldi. Hangi fotoğrafa çarpı atalım?", _centerMuted);
            if (!GM.AutopsyReady || GM.LabCount < GameData.ReqLabs)
                GUI.Label(new Rect(0, 134, VW, 30), "(!) Otopsi raporu ve adli lab sonuçları olmadan suçlama yapmak risklidir.", Styled(_centerSmallBold, Warning));
            if (ColoredButton(new Rect(VW - 90, 40, 56, 52), "X", new Color(0.2f, 0.22f, 0.26f))) Do(CloseModal);

            var npcs = GM.Npcs.ToList();
            int cols = Mathf.Min(npcs.Count, npcs.Count > 8 ? 7 : npcs.Count > 5 ? 4 : 5);
            int rows = Mathf.CeilToInt(npcs.Count / (float)cols);
            float ch = Mathf.Min(640, (VH - 220) / rows - 16);
            float cw = Mathf.Min(300, Mathf.Min((VW - 120) / cols - 16, ch * 0.62f));
            float total = cols * cw + (cols - 1) * 18;
            float x0 = (VW - total) / 2, y0 = 180;
            for (int i = 0; i < npcs.Count; i++)
            {
                var n = npcs[i];
                bool innocent = GM.IsInnocent(n.Id);
                var card = new Rect(x0 + (i % cols) * (cw + 18), y0 + (i / cols) * (ch + 16), cw, ch);
                // Masaüstüne dağılmış fotoğraf hissi
                var m = GUI.matrix;
                GUIUtility.RotateAroundPivot(((i % cols) - cols / 2) * 1.4f, card.center * _scale);
                Panel(card, new Color(0.93f, 0.9f, 0.82f, 0.97f), innocent ? Success : new Color(0.35f, 0.25f, 0.15f));
                var ph = new Rect(card.x + 10, card.y + 10, cw - 20, Mathf.Min(cw - 10, ch - 230));
                Fill(ph, new Color(0.1f, 0.1f, 0.1f));
                var tex = T(n.Def.Portrait);
                if (tex != null) GUI.DrawTexture(ph, tex, ScaleMode.ScaleAndCrop);
                if (innocent)
                {
                    Fill(ph, new Color(0.05f, 0.4f, 0.2f, 0.35f));
                    GUI.Label(ph, "MASUM", _stamp);
                }
                GUI.Label(new Rect(card.x, ph.yMax + 10, cw, 34), n.Def.Name, Styled(_h3Center, Ink));
                GUI.Label(new Rect(card.x, ph.yMax + 44, cw, 28), $"{n.Def.Building} ({n.Def.Role})", Styled(_centerSmall, new Color(0.35f, 0.3f, 0.25f)));
                GUI.Label(new Rect(card.x, ph.yMax + 72, cw, 26), $"Stres {n.Stress}%  •  Soru {n.Asked}/{GameData.MaxQuestions}  •  Delil {GM.Bag.Count(c => c.Def.BuildingNpcId == n.Id)}", Styled(_centerSmall, new Color(0.35f, 0.3f, 0.25f)));
                float by = card.yMax - 128;
                if (ColoredButton(new Rect(card.x + 14, by, cw - 28, 46), "Notlar", new Color(0.25f, 0.22f, 0.2f))) { var nn = n; Do(() => { _historyNpc = nn; _historyScroll = Vector2.zero; _modal = Modal.History; }); }
                if (innocent)
                {
                    Panel(new Rect(card.x + 14, by + 56, cw - 28, 50), new Color(0.1f, 0.4f, 0.2f, 0.9f), Success);
                    GUI.Label(new Rect(card.x + 14, by + 56, cw - 28, 50), "MASUM İLAN EDİLDİ", _hudButton);
                }
                else
                {
                    float half = (cw - 38) / 2f;
                    if (ColoredButton(new Rect(card.x + 14, by + 56, half, 50), "SUÇLA", Danger)) { int id = n.Id; Do(() => GM.Accuse(id)); }
                    if (ColoredButton(new Rect(card.x + 24 + half, by + 56, half, 50), "Masum", new Color(0.12f, 0.5f, 0.28f))) { int id = n.Id; Do(() => GM.DeclareInnocent(id)); }
                }
                GUI.matrix = m;
            }
        }

        // ---------- KONUŞMA GEÇMİŞİ (npc-history-modal) ----------
        void DrawHistory()
        {
            var n = _historyNpc;
            if (n == null) { _modal = Modal.Found; return; }
            Dim();
            var r = Center(900, 700);
            Panel(r, BgCard, Accent);
            GUI.Label(new Rect(r.x + 30, r.y + 22, r.width - 120, 44), $"Konuşma Geçmişi — {n.Def.Name}", _h2);
            if (ColoredButton(new Rect(r.xMax - 70, r.y + 20, 50, 46), "X", new Color(0.2f, 0.22f, 0.26f))) Do(() => _modal = Modal.Found);
            GUILayout.BeginArea(new Rect(r.x + 30, r.y + 84, r.width - 60, r.height - 110));
            _historyScroll = GUILayout.BeginScrollView(_historyScroll);
            if (n.Log.Count == 0) GUILayout.Label("Bu şüpheliyle henüz konuşulmadı.", _mutedItalic);
            foreach (var l in n.Log)
            {
                GUILayout.Label(l.FromPlayer ? $"<color=#d4a65a><b>Dedektif:</b></color> {l.Text}" : $"<color=#dbab09><b>{l.Speaker}:</b></color> {l.Text}", _body);
                GUILayout.Space(8);
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        // ---------- HAPİS ANİMASYONU (jail-overlay) ----------
        void DrawJail()
        {
            Fill(new Rect(0, 0, VW, VH), Color.black);
            if (_jailNpc == null) return;
            float t = Time.time - _jailStart;
            var tex = (_jailNpc.Def.Portrait != null && !_jailNpc.Def.Portrait.Contains("/") ? T("jail_" + _jailNpc.Def.Portrait) : null) ?? T(_jailNpc.Def.Portrait);
            float s = Mathf.Min(VH - 120, VW * 0.6f);
            var img = new Rect((VW - s) / 2, (VH - s) / 2 - 20, s, s);
            if (tex != null) { var p = GUI.color; GUI.color = new Color(1, 1, 1, Mathf.Clamp01(t * 2f)); GUI.DrawTexture(img, tex, ScaleMode.ScaleToFit); GUI.color = p; }
            // Demir parmaklıklar yukarıdan iner
            float drop = Mathf.Clamp01((t - 0.3f) / 0.6f);
            for (int i = 0; i < 12; i++)
            {
                float x = img.x + i * img.width / 11f - 9;
                Fill(new Rect(x, img.y - img.height + img.height * drop, 18, img.height), new Color(0.15f, 0.15f, 0.17f, 0.95f));
                Fill(new Rect(x + 3, img.y - img.height + img.height * drop, 4, img.height), new Color(0.4f, 0.4f, 0.45f, 0.9f));
            }
            Fill(new Rect(img.x - 20, img.y + img.height * 0.2f, img.width + 40, 16), new Color(0.15f, 0.15f, 0.17f, drop));
            GUI.Label(new Rect(0, img.yMax - 10, VW, 50), _jailNpc.Def.Name, _jailName);
            if (t > 1.1f)
            {
                var m = GUI.matrix;
                float sc = Mathf.Lerp(2.2f, 1f, Mathf.Clamp01((t - 1.1f) * 5f));
                GUIUtility.ScaleAroundPivot(new Vector2(sc, sc), new Vector2(VW / 2, VH / 2) * _scale);
                GUIUtility.RotateAroundPivot(-12, new Vector2(VW / 2, VH / 2) * _scale);
                GUI.Label(new Rect(0, VH / 2 - 60, VW, 120), "TUTUKLANDIN", _arrested);
                GUI.matrix = m;
            }
        }

        // ---------- ÇIKIŞ UYARISI (exit-warning-modal) ----------
        void DrawExitWarning()
        {
            Dim();
            var r = Center(760, 470);
            Panel(r, BgCard, Warning);
            GUI.Label(new Rect(r.x, r.y + 26, r.width, 70), "⚠", Styled(_bigIcon, Warning));
            GUI.Label(new Rect(r.x + 30, r.y + 100, r.width - 60, 40), "Binadan Ayrılmak İstediğinize Emin Misiniz?", Styled(_h3Center, TextPrimary));
            GUI.Label(new Rect(r.x + 50, r.y + 150, r.width - 100, 190), _exitText, _centerBody);
            if (ColoredButton(new Rect(r.x + 60, r.yMax - 90, 300, 58), "Evet, Ayrıl", Danger)) Do(() => { CloseModal(); GM.ConfirmExitBuilding(); });
            if (ColoredButton(new Rect(r.xMax - 360, r.yMax - 90, 300, 58), "Hayır, Kal", new Color(0.08f, 0.45f, 0.5f))) Do(CloseModal);
        }

        // ---------- OTOPSİ (autopsy-modal) ----------
        void DrawAutopsy()
        {
            Dim();
            var r = Center(900, 800);
            var tex = T("vintage_autopsy_bg") ?? T("old_paper_texture");
            if (tex != null) GUI.DrawTexture(r, tex, ScaleMode.ScaleAndCrop); else Fill(r, Paper);
            Fill(r, new Color(0.95f, 0.9f, 0.8f, 0.55f));
            Border(r, new Color(0.35f, 0.25f, 0.15f), 3);
            GUI.Label(new Rect(r.x + 40, r.y + 30, r.width - 140, 44), "ADLİ TIP OTOPSİ VAKA DOSYASI", _paperTitle);
            if (ColoredButton(new Rect(r.xMax - 80, r.y + 26, 50, 46), "X", new Color(0.35f, 0.25f, 0.15f))) Do(CloseModal);
            var m = GUI.matrix;
            GUIUtility.RotateAroundPivot(-14, new Vector2(r.xMax - 150, r.y + 140) * _scale);
            GUI.Label(new Rect(r.xMax - 300, r.y + 110, 300, 60), "GİZLİ", _stampRed);
            GUI.matrix = m;
            GUILayout.BeginArea(new Rect(r.x + 50, r.y + 100, r.width - 100, r.height - 140));
            _autopsyScroll = GUILayout.BeginScrollView(_autopsyScroll);
            GUILayout.Label(GM.AutopsyReport ?? "", _paperText);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
            GUI.Label(new Rect(r.x + 40, r.yMax - 50, 400, 30), "SORGULAMA ONAYLI", Styled(_smallBold, new Color(0.15f, 0.35f, 0.6f)));
        }

        // ---------- SONUÇ (result-modal) ----------
        void DrawResult() => DrawResultCinematic();

        void DrawResultClassic()
        {
            DrawCover(T("town_bg"), new Rect(0, 0, VW, VH));
            Fill(new Rect(0, 0, VW, VH), new Color(0, 0, 0, 0.88f));
            var r = Center(980, 860);
            var col = GM.ResultWin ? Success : Danger;
            Panel(r, BgCard, col);
            GUI.Label(new Rect(r.x, r.y + 20, r.width, 80), GM.ResultWin ? "🏆" : "☠", Styled(_bigIcon, col));
            GUI.Label(new Rect(r.x, r.y + 100, r.width, 60), GM.ResultTitle, Styled(_resultTitle, col));
            GUI.Label(new Rect(r.x + 60, r.y + 170, r.width - 120, 110), GM.ResultVerdict, _centerBodyLarge);
            Fill(new Rect(r.x + 60, r.y + 290, r.width - 120, 2), col * 0.6f);
            GUI.Label(new Rect(r.x + 60, r.y + 300, r.width - 120, 30), "O GECENİN HİKAYESİ", _accentSmall);
            GUILayout.BeginArea(new Rect(r.x + 60, r.y + 336, r.width - 120, r.height - 450));
            _resultScroll = GUILayout.BeginScrollView(_resultScroll);
            GUILayout.Label(GM.ResultStory ?? "", _body);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
            if (!string.IsNullOrEmpty(GM.UnlockedMessage)) GUI.Label(new Rect(r.x, r.yMax - 140, r.width, 36), GM.UnlockedMessage, Styled(_centerBold, Warning));
            if (ColoredButton(new Rect(r.center.x - 230, r.yMax - 90, 460, 62), GM.ResultButton ?? "Haritaya Dön", new Color(0.08f, 0.45f, 0.5f))) Do(GM.ResultContinue);
        }

        // ---------- ÇETİN SİNEMATİK KUTUSU (cinematic-helper-box) ----------
        void DrawCinematicHelper()
        {
            if (!_helperOpen || GM.HelperHistory.Count == 0 || _modal == Modal.Jail || _modal == Modal.Interrogation || _modal == Modal.Lab) return;
            _helperIndex = Mathf.Clamp(_helperIndex, 0, GM.HelperHistory.Count - 1);
            var msg = GM.HelperHistory[_helperIndex];
            float w = Mathf.Min(1250, VW - 60), h = 230;
            var r = new Rect((VW - w) / 2, VH - h - 24, w, h);
            Fill(r, new Color(0.02f, 0.03f, 0.04f, 0.93f));
            for (int i = 0; i < w / 26; i++) { Fill(new Rect(r.x + i * 26 + 6, r.y + 4, 14, 8), new Color(1, 1, 1, 0.08f)); Fill(new Rect(r.x + i * 26 + 6, r.yMax - 12, 14, 8), new Color(1, 1, 1, 0.08f)); }
            Border(r, Accent * 0.8f, 2);
            var av = new Rect(r.x + 24, r.y + 26, 170, 178);
            Fill(av, new Color(Accent.r, Accent.g, Accent.b, 0.08f + Mathf.Sin(Time.time * 2f) * 0.03f));
            bool local = !string.IsNullOrEmpty(msg.Speaker) && !msg.Speaker.StartsWith("Çetin");
            var tex = local && !string.IsNullOrEmpty(GameData.Current.LocalHelperPortrait) ? T(GameData.Current.LocalHelperPortrait) : local ? null : T("dedektif_helper");
            if (tex != null) GUI.DrawTexture(av, tex, ScaleMode.ScaleToFit);
            else if (local) GUI.Label(av, msg.Speaker.Substring(0, 1), _bigIcon);
            var tx = new Rect(av.xMax + 24, r.y + 22, r.width - av.width - 100, r.height - 40);
            GUI.Label(new Rect(tx.x, tx.y, tx.width, 28), local ? msg.Speaker.ToUpperInvariant() : "YARDIMCI DEDEKTİF ÇETİN", Styled(_accentSmall, local ? Warning : Accent));
            int chars = Mathf.Min(msg.Text.Length, (int)((Time.time - msg.StartTime) * HelperMessage.CharsPerSecond));
            GUI.Label(new Rect(tx.x, tx.y + 32, tx.width, 110), msg.Text.Substring(0, chars), _helperText);
            float bx = tx.x, by = r.yMax - 66;
            if (_helperIndex > 0 && SmallBtn(ref bx, by, "◀ Önceki")) Do(() => { _helperIndex--; _helperShownAt = Time.time; GM.HelperHistory[_helperIndex].StartTime = Time.time - 999; });
            if (GM.Phase == GamePhase.Playing)
            {
                if (SmallBtn(ref bx, by, "[1] Delilleri İncele")) Do(GM.AnalyzeBag);
                if (SmallBtn(ref bx, by, "[2] Başka İpucu Söyle")) Do(GM.ProvideBuildingHint);
            }
            if (SmallBtn(ref bx, by, "[G] Geç")) Do(() => _helperOpen = false);
            if (ColoredButton(new Rect(r.xMax - 56, r.y + 12, 42, 40), "X", new Color(0.2f, 0.22f, 0.26f))) Do(() => _helperOpen = false);
        }

        bool SmallBtn(ref float x, float y, string text)
        {
            float w = _helperBtn.CalcSize(new GUIContent(text)).x + 32;
            var r = new Rect(x, y, w, 44);
            x += w + 10;
            bool hover = r.Contains(Event.current.mousePosition);
            Fill(r, new Color(Accent.r, Accent.g, Accent.b, hover ? 0.3f : 0.14f));
            Border(r, Accent * 0.7f, 1);
            return GUI.Button(r, text, _helperBtn);
        }

        // ---------- BİLDİRİMLER, SES, MESAJ KUTUSU, GEÇİŞ ----------
        void DrawToasts()
        {
            _toasts.RemoveAll(t => Time.time > t.Until);
            float y = GM.Phase == GamePhase.Playing ? 160 : 130;
            foreach (var t in _toasts)
            {
                float a = Mathf.Clamp01((t.Until - Time.time) * 2f);
                var r = new Rect(VW / 2 - 330, y, 660, 46);
                var prev = GUI.color; GUI.color = new Color(1, 1, 1, a);
                Panel(r, BgGlass, Accent * 0.7f);
                GUI.Label(r, t.Text, _centerSmallBold);
                GUI.color = prev;
                y += 52;
            }
        }

        void DrawMuteButton()
        {
            if (AudioManager.Instance == null || GM.Phase == GamePhase.Descent) return;
            var r = GM.Phase == GamePhase.Playing ? new Rect(VW - 74, VH - 74, 54, 54) : new Rect(VW - 80, 25, 54, 54);
            if (ColoredButton(r, AudioManager.Instance.Muted ? "🔇" : "🔊", new Color(0.12f, 0.14f, 0.17f))) Do(AudioManager.Instance.ToggleMute);
        }

        void DrawBoxes()
        {
            if (_boxes.Count == 0) return;
            var b = _boxes[0];
            Fill(new Rect(0, 0, VW, VH), new Color(0, 0, 0, 0.6f));
            float h = Mathf.Max(320, _centerBody.CalcHeight(new GUIContent(b.Text), 600) + 220);
            var r = Center(700, h);
            var col = b.Warning ? Danger : Accent;
            Panel(r, BgCard, col);
            GUI.Label(new Rect(r.x, r.y + 18, r.width, 60), b.Warning ? "⚠" : "✉", Styled(_bigIconSmall, col));
            GUI.Label(new Rect(r.x + 30, r.y + 80, r.width - 60, 40), b.Title, Styled(_h3Center, TextPrimary));
            GUI.Label(new Rect(r.x + 50, r.y + 126, r.width - 100, h - 210), b.Text, _centerBody);
            if (b.Confirm)
            {
                if (ColoredButton(new Rect(r.x + 50, r.yMax - 84, 280, 56), "Hayır / İptal", new Color(0.2f, 0.22f, 0.26f))) Do(() => CloseBox(false));
                if (ColoredButton(new Rect(r.xMax - 330, r.yMax - 84, 280, 56), "Evet", new Color(0.08f, 0.45f, 0.5f))) Do(() => CloseBox(true));
            }
            else if (ColoredButton(new Rect(r.center.x - 150, r.yMax - 84, 300, 56), "Anladım", new Color(0.08f, 0.45f, 0.5f))) Do(() => CloseBox(true));
        }

        void DrawFade()
        {
            float t = (Time.time - _fadeStart) / FadeDuration;
            if (t < 0f || t > 1f) return;
            float a = Mathf.SmoothStep(0, 1, Mathf.Clamp01((t < 0.5f ? t * 2f : (1f - t) * 2f) * 1.4f));
            Fill(new Rect(0, 0, VW, VH), new Color(0, 0, 0, a));
            if (string.IsNullOrEmpty(_fadeTitle)) return;
            var prev = GUI.color;
            GUI.color = new Color(_fadeColor.r, _fadeColor.g, _fadeColor.b, a);
            GUI.Label(new Rect(0, VH / 2 - 60, VW, 80), _fadeTitle, _fadeStyle);
            GUI.color = new Color(1, 1, 1, a * 0.7f);
            GUI.Label(new Rect(0, VH / 2 + 24, VW, 40), "kapı gıcırdayarak açılıyor...", _centerMuted);
            GUI.color = prev;
        }

        // =================================================================
        //  YARDIMCILAR
        // =================================================================
        Rect Center(float w, float h)
        {
            w = Mathf.Min(w, VW - 40); h = Mathf.Min(h, VH - 40);
            return new Rect((VW - w) / 2, (VH - h) / 2, w, h);
        }
        static Rect Pad(Rect r, float p) => new Rect(r.x + p, r.y + p, r.width - p * 2, r.height - p * 2);
        void Dim() => Fill(new Rect(0, 0, VW, VH), new Color(0, 0, 0, 0.6f));

        void DrawCover(Texture2D tex, Rect r)
        {
            if (tex != null) GUI.DrawTexture(r, tex, ScaleMode.ScaleAndCrop);
            else Fill(r, BgNoir);
        }

        void RadialDark(float inner, float outer)
        {
            // Merkezden kenarlara koyulaşan katmanlar (radial-gradient)
            for (int i = 0; i < 10; i++)
            {
                float k = i / 9f;
                float a = Mathf.Lerp(inner, outer, k) * 0.18f;
                float m = k * 0.45f;
                Fill(new Rect(0, 0, VW, VH * m * 0.5f), new Color(0.02f, 0.02f, 0.03f, a));
                Fill(new Rect(0, VH - VH * m * 0.5f, VW, VH * m * 0.5f), new Color(0.02f, 0.02f, 0.03f, a));
                Fill(new Rect(0, 0, VW * m * 0.5f, VH), new Color(0.02f, 0.02f, 0.03f, a));
                Fill(new Rect(VW - VW * m * 0.5f, 0, VW * m * 0.5f, VH), new Color(0.02f, 0.02f, 0.03f, a));
            }
            Fill(new Rect(0, 0, VW, VH), new Color(0.02f, 0.03f, 0.05f, inner));
        }

        void Panel(Rect r, Color bg, Color border)
        {
            Fill(r, bg);
            Border(r, border, 2);
            // İç ince yaldız çizgi + köşe süsleri (Viktorya dönemi çerçeve)
            var inner = new Rect(r.x + 5, r.y + 5, r.width - 10, r.height - 10);
            Border(inner, new Color(Accent.r, Accent.g, Accent.b, 0.28f * border.a), 1);
            var orn = new Color(Accent.r, Accent.g, Accent.b, Mathf.Min(1f, border.a + 0.2f));
            foreach (var p in new[] { new Vector2(r.x, r.y), new Vector2(r.xMax - 7, r.y), new Vector2(r.x, r.yMax - 7), new Vector2(r.xMax - 7, r.yMax - 7) })
                Fill(new Rect(p.x, p.y, 7, 7), orn);
        }

        // ---------- CEP SAATİ (gün hesabı: 24 dk = 24 saat) ----------
        Texture2D _watchFace;
        Texture2D WatchFace()
        {
            if (_watchFace != null) return _watchFace;
            const int S = 160;
            var tex = new Texture2D(S, S, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var px = new Color[S * S];
            var rim = new Color(0.72f, 0.55f, 0.24f); var rimDark = new Color(0.36f, 0.25f, 0.1f); var face = new Color(0.93f, 0.89f, 0.78f);
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float dx = (x - S / 2f + 0.5f) / (S / 2f), dy = (y - S / 2f + 0.5f) / (S / 2f);
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    Color c = Color.clear;
                    if (d < 1f) c = Color.Lerp(rim, rimDark, Mathf.Clamp01((d - 0.9f) / 0.1f));
                    if (d < 0.88f) c = Color.Lerp(face, face * 0.82f, d * 0.6f);
                    // saat çentikleri
                    float ang = Mathf.Atan2(dx, dy) / (Mathf.PI * 2f) * 12f;
                    float frac = Mathf.Abs(ang - Mathf.Round(ang));
                    if (d > 0.7f && d < 0.84f && frac < 0.05f) c = new Color(0.12f, 0.08f, 0.05f);
                    if (d < 1f) c.a = Mathf.Clamp01((1f - d) * S / 4f);
                    px[y * S + x] = c;
                }
            tex.SetPixels(px); tex.Apply();
            _watchFace = tex;
            return tex;
        }

        void DrawPocketWatch(Vector2 pos)
        {
            var dn = DayNightCycle.Instance;
            if (dn == null) return;
            const float size = 112f;
            var r = new Rect(pos.x, pos.y, size, size);
            // Zincir halkası
            Fill(new Rect(r.center.x - 6, r.y - 10, 12, 12), new Color(0.72f, 0.55f, 0.24f));
            GUI.DrawTexture(r, WatchFace());
            var pivot = r.center;
            var m = GUI.matrix;
            float h = dn.Hour % 12f, min = (dn.Hour - Mathf.Floor(dn.Hour)) * 60f;
            GUIUtility.RotateAroundPivot(h / 12f * 360f, pivot * _scale);
            Fill(new Rect(pivot.x - 2.5f, pivot.y - 28, 5, 30), new Color(0.1f, 0.07f, 0.04f));
            GUI.matrix = m;
            GUIUtility.RotateAroundPivot(min / 60f * 360f, pivot * _scale);
            Fill(new Rect(pivot.x - 1.5f, pivot.y - 42, 3, 44), new Color(0.1f, 0.07f, 0.04f));
            GUI.matrix = m;
            Fill(new Rect(pivot.x - 4, pivot.y - 4, 8, 8), new Color(0.72f, 0.55f, 0.24f));
            // Yanında: gün, saat, vakit, soruşturma dakikası
            var tr = new Rect(r.xMax + 14, r.y + 8, 300, 30);
            GUI.Label(tr, $"{dn.Day}. GÜN  •  {dn.Clock}", _hudTitle);
            GUI.Label(new Rect(tr.x, tr.y + 32, 300, 26), (dn.IsNight ? "☾  " : "☀  ") + dn.PartOfDay + (dn.LampsOn ? "  —  fenerler yanık" : ""), _muted);
            GUI.Label(new Rect(tr.x, tr.y + 58, 320, 26), $"Soruşturmanın {Mathf.FloorToInt(dn.ElapsedMinutes) + 1}. dakikası", _mutedItalic);
        }

        // ---------- DEDEKTİF GÖZÜ katmanı ----------
        void DrawVisionOverlay()
        {
            var v = GM.Vision;
            float k = v != null ? v.Strength : 0f;
            if (k > 0.001f)
            {
                Fill(new Rect(0, 0, VW, VH), new Color(0.22f, 0.14f, 0.05f, 0.14f * k));
                var lbl = new Rect(VW / 2 - 300, 86, 600, 40);
                GUI.Label(lbl, "◈  D E D E K T İ F   G Ö Z Ü  ◈", Styled(_centerBold, new Color(Accent.r, Accent.g, Accent.b, k)));
                var cam = Camera.main;
                if (cam != null)
                    foreach (var spot in Dedektif3D.Gameplay.HidingSpotInteractable.All)
                    {
                        if (spot == null || spot.Opened || !spot.Sensed) continue;
                        var sp = cam.WorldToScreenPoint(spot.Point);
                        if (sp.z <= 0.1f) continue;
                        var g = new Vector2(sp.x / _scale, (Screen.height - sp.y) / _scale);
                        float pulse = 0.6f + Mathf.Sin(Time.time * 5f) * 0.4f;
                        GUI.Label(new Rect(g.x - 30, g.y - 30, 60, 60), "◆", Styled(_bigIconSmall, new Color(1f, 0.8f, 0.4f, k * pulse)));
                        GUI.Label(new Rect(g.x - 160, g.y + 22, 320, 26), spot.def.Label, Styled(_centerSmallBold, new Color(1f, 0.86f, 0.6f, k)));
                    }
            }
            if (v != null && !v.Active && v.CooldownLeft > 0f && GM.InsideBuilding)
                GUI.Label(new Rect(VW / 2 - 200, VH - 92, 400, 26), $"Dedektif Gözü dinleniyor… {v.CooldownLeft:0} sn", Styled(_centerSmall, TextMuted));
        }

        static void Border(Rect r, Color c, float w)
        {
            Fill(new Rect(r.x, r.y, r.width, w), c);
            Fill(new Rect(r.x, r.yMax - w, r.width, w), c);
            Fill(new Rect(r.x, r.y, w, r.height), c);
            Fill(new Rect(r.xMax - w, r.y, w, r.height), c);
        }

        static void Fill(Rect r, Color c)
        {
            var prev = GUI.color;
            GUI.color = new Color(c.r, c.g, c.b, c.a * prev.a);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = prev;
        }

        bool GlowButton(string text, Color color, float w, float h, int fontSize = 18)
        {
            var st = new GUIStyle(_button) { fontSize = fontSize };
            st.normal.textColor = color; st.hover.textColor = Color.Lerp(color, Color.white, 0.5f);
            var r = GUILayoutUtility.GetRect(w, h, GUILayout.Width(w), GUILayout.Height(h));
            bool hover = r.Contains(Event.current.mousePosition);
            Fill(r, new Color(color.r, color.g, color.b, hover ? 0.3f : 0.14f));
            Border(r, new Color(color.r, color.g, color.b, hover ? 1f : 0.6f), 2);
            if (hover) Fill(new Rect(r.x - 6, r.y - 6, r.width + 12, r.height + 12), new Color(color.r, color.g, color.b, 0.05f));
            return GUI.Button(r, text, st);
        }

        bool ColoredButton(Rect r, string text, Color color)
        {
            bool hover = r.Contains(Event.current.mousePosition);
            Fill(r, new Color(color.r * (hover ? 1.25f : 1f), color.g * (hover ? 1.25f : 1f), color.b * (hover ? 1.25f : 1f), 0.92f));
            Border(r, new Color(1, 1, 1, hover ? 0.35f : 0.12f), 1);
            return GUI.Button(r, text, _hudButton);
        }

        static Color Hex(string hex, float alpha = 1f)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            c.a = alpha;
            return c;
        }

        static GUIStyle Styled(GUIStyle s, Color c) { var n = new GUIStyle(s); n.normal.textColor = c; n.hover.textColor = c; return n; }

        // ---------- STİLLER ----------
        GUIStyle _title, _subtitle, _centerMuted, _badge, _badgeCenter, _typewriter, _hudTitle, _muted, _mutedItalic, _centerBold, _centerSmall, _centerSmallBold,
            _rightMuted, _rightSmall, _centerLarge, _small, _smallBold, _body, _bodyBold, _bodyLarge, _accentSmall, _accentSmallCenter, _h2, _h2Center, _h3Center, _card,
            _questionStyle, _input, _placeholder, _notes, _paperTitle, _paperText, _docText, _bigIcon, _bigIconSmall, _resultTitle, _centerBody, _centerBodyLarge,
            _fadeStyle, _button, _hudButton, _mapHeader, _doorTitle, _descentTitle, _descentSub, _foundTitle, _stamp, _stampRed, _jailName, _arrested,
            _helperText, _helperBtn, _subtitle2, _subtitleShadow,
            _mapTitle, _mapSiteName, _mapSiteStatus, _splashKicker, _splashTitle, _splashSub, _splashBody, _splashFoot, _menuLink, _menuLinkBig,
            _fileTab, _fileKicker, _fileTitle, _fileKey, _fileVal, _fileWeather, _fileBody, _fileStamp, _fileButton, _fileLocked, _fileHint,
            _hudTown, _hudPlace, _hudLink, _hudKey, _hudCenter, _hudPrompt, _hudSmall;
        bool _stylesReady;
        Font _serif, _mono;

        static Font OsFont(string[] names, int size)
        {
            try { return Font.CreateDynamicFontFromOSFont(names, size); }
            catch { return null; }
        }

        void EnsureStyles()
        {
            if (_stylesReady) return;
            _stylesReady = true;
            _serif = OsFont(new[] { "Georgia", "Palatino Linotype", "Book Antiqua", "Times New Roman", "DejaVu Serif", "Liberation Serif" }, 20);
            _mono = OsFont(new[] { "Courier New", "Lucida Console", "DejaVu Sans Mono", "Liberation Mono" }, 20);

            GUIStyle L(int size, Color color, FontStyle fs = FontStyle.Normal, TextAnchor anchor = TextAnchor.UpperLeft, bool wrap = true)
            {
                var s = new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = fs, alignment = anchor, wordWrap = wrap, richText = true };
                if (_serif != null) s.font = _serif;
                s.normal.textColor = color; s.hover.textColor = color;
                return s;
            }
            Texture2D Tex(Color c) { var t = new Texture2D(1, 1); t.SetPixel(0, 0, c); t.Apply(); return t; }

            _title = L(70, Accent, FontStyle.Bold, TextAnchor.MiddleCenter);
            _subtitle = L(26, TextPrimary, FontStyle.Normal, TextAnchor.MiddleCenter);
            _centerMuted = L(18, TextMuted, FontStyle.Normal, TextAnchor.MiddleCenter);
            _badge = L(18, Warning, FontStyle.Bold);
            _badgeCenter = L(20, Warning, FontStyle.Bold, TextAnchor.MiddleCenter);
            _typewriter = L(27, Paper);
            _hudTitle = L(22, Accent, FontStyle.Bold);
            _muted = L(17, TextMuted);
            _mutedItalic = L(19, TextMuted, FontStyle.Italic);
            _centerBold = L(20, TextPrimary, FontStyle.Bold, TextAnchor.MiddleCenter);
            _centerSmall = L(16, TextPrimary, FontStyle.Normal, TextAnchor.MiddleCenter);
            _centerSmallBold = L(17, TextPrimary, FontStyle.Bold, TextAnchor.MiddleCenter);
            _rightMuted = L(16, TextMuted, FontStyle.Normal, TextAnchor.MiddleRight);
            _rightSmall = L(17, TextPrimary, FontStyle.Bold, TextAnchor.MiddleRight);
            _centerLarge = L(23, TextPrimary, FontStyle.Bold, TextAnchor.MiddleCenter);
            _small = L(16, TextPrimary);
            _smallBold = L(16, TextPrimary, FontStyle.Bold);
            _body = L(19, TextPrimary);
            _bodyBold = L(18, TextPrimary, FontStyle.Bold);
            _bodyLarge = L(21, TextPrimary);
            _centerBody = L(20, TextPrimary, FontStyle.Normal, TextAnchor.UpperCenter);
            _centerBodyLarge = L(23, TextPrimary, FontStyle.Normal, TextAnchor.UpperCenter);
            _accentSmall = L(16, Accent, FontStyle.Bold);
            _accentSmallCenter = L(13, Accent, FontStyle.Bold, TextAnchor.MiddleCenter);
            _h2 = L(34, TextPrimary, FontStyle.Bold);
            _h2Center = L(34, TextPrimary, FontStyle.Bold, TextAnchor.MiddleCenter);
            _h3Center = L(24, TextPrimary, FontStyle.Bold, TextAnchor.MiddleCenter);
            _paperTitle = L(30, new Color(0.25f, 0.12f, 0.06f), FontStyle.Bold);
            _paperText = L(21, new Color(0.15f, 0.1f, 0.06f));
            _docText = L(24, new Color(0.15f, 0.09f, 0.05f));
            _bigIcon = L(60, TextPrimary, FontStyle.Normal, TextAnchor.MiddleCenter);
            _bigIconSmall = L(44, TextPrimary, FontStyle.Normal, TextAnchor.MiddleCenter);
            _resultTitle = L(48, Success, FontStyle.Bold, TextAnchor.MiddleCenter);
            _fadeStyle = L(60, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
            _mapHeader = L(36, Accent, FontStyle.Bold, TextAnchor.MiddleCenter);
            _doorTitle = L(72, TextPrimary, FontStyle.Bold, TextAnchor.MiddleCenter);
            _descentTitle = L(64, Color.white, FontStyle.Bold);
            _descentSub = L(22, new Color(0.85f, 0.88f, 0.95f));
            _foundTitle = L(44, Danger, FontStyle.Bold, TextAnchor.MiddleCenter);
            _stamp = L(56, new Color(0.3f, 1f, 0.55f, 0.9f), FontStyle.Bold, TextAnchor.MiddleCenter);
            _stampRed = L(52, new Color(0.75f, 0.08f, 0.08f, 0.75f), FontStyle.Bold, TextAnchor.MiddleCenter);
            _jailName = L(40, TextPrimary, FontStyle.Bold, TextAnchor.MiddleCenter);
            _arrested = L(96, new Color(0.9f, 0.1f, 0.12f), FontStyle.Bold, TextAnchor.MiddleCenter);
            _helperText = L(21, TextPrimary);
            _subtitle2 = L(27, new Color(0.96f, 0.92f, 0.82f), FontStyle.Normal, TextAnchor.UpperLeft);
            _subtitleShadow = L(27, new Color(0, 0, 0, 0.85f), FontStyle.Normal, TextAnchor.UpperLeft);
            _helperBtn = L(16, Accent, FontStyle.Bold, TextAnchor.MiddleCenter, false);

            // Açılış afişi + 3D harita + vaka dosyası
            _mapTitle = L(50, Accent, FontStyle.Bold, TextAnchor.MiddleLeft, false);
            _mapSiteName = L(21, TextPrimary, FontStyle.Bold, TextAnchor.MiddleLeft, false);
            _mapSiteStatus = L(15, TextMuted, FontStyle.Normal, TextAnchor.MiddleLeft, false);
            _splashKicker = L(17, Accent, FontStyle.Bold, TextAnchor.MiddleLeft, false);
            _splashTitle = L(104, new Color(0.95f, 0.91f, 0.82f), FontStyle.Bold, TextAnchor.MiddleLeft, false);
            _splashSub = L(36, new Color(0.9f, 0.84f, 0.72f), FontStyle.Italic, TextAnchor.MiddleLeft, false);
            _splashBody = L(21, new Color(0.78f, 0.74f, 0.66f), FontStyle.Italic, TextAnchor.UpperLeft, true);
            _splashFoot = L(15, new Color(0.6f, 0.57f, 0.52f), FontStyle.Normal, TextAnchor.MiddleLeft, false);
            _menuLink = L(22, TextPrimary, FontStyle.Bold, TextAnchor.MiddleLeft, false);
            _menuLinkBig = L(30, Accent, FontStyle.Bold, TextAnchor.MiddleLeft, false);
            var inkC = new Color(0.17f, 0.11f, 0.06f);
            _fileTab = L(17, new Color(0.25f, 0.16f, 0.07f), FontStyle.Bold, TextAnchor.MiddleCenter, false);
            _fileKicker = L(16, new Color(0.45f, 0.12f, 0.08f), FontStyle.Bold, TextAnchor.MiddleLeft, false);
            _fileTitle = L(40, inkC, FontStyle.Bold, TextAnchor.MiddleLeft, false);
            _fileKey = L(15, new Color(0.42f, 0.33f, 0.22f), FontStyle.Bold, TextAnchor.MiddleLeft, false);
            _fileVal = L(19, inkC, FontStyle.Normal, TextAnchor.MiddleLeft, false);
            _fileWeather = L(17, inkC, FontStyle.Italic, TextAnchor.MiddleLeft, false);
            _fileBody = L(19, new Color(0.2f, 0.13f, 0.07f), FontStyle.Normal, TextAnchor.UpperLeft, true);
            _fileStamp = L(30, new Color(0.6f, 0.08f, 0.06f), FontStyle.Bold, TextAnchor.MiddleCenter, false);
            _fileButton = L(22, new Color(0.96f, 0.9f, 0.76f), FontStyle.Bold, TextAnchor.MiddleCenter, false);
            _fileLocked = L(18, new Color(0.38f, 0.28f, 0.2f), FontStyle.Italic, TextAnchor.MiddleCenter, true);
            _fileHint = L(13, new Color(0.45f, 0.37f, 0.27f), FontStyle.Normal, TextAnchor.MiddleLeft, false);
            // Sinematik HUD
            _hudTown = L(30, new Color(0.95f, 0.91f, 0.82f), FontStyle.Bold, TextAnchor.MiddleLeft, false);
            _hudPlace = L(18, new Color(0.78f, 0.74f, 0.66f), FontStyle.Italic, TextAnchor.MiddleLeft, false);
            _hudLink = L(19, TextPrimary, FontStyle.Bold, TextAnchor.MiddleLeft, false);
            _hudKey = L(14, Accent, FontStyle.Bold, TextAnchor.MiddleCenter, false);
            _hudCenter = L(17, TextPrimary, FontStyle.Bold, TextAnchor.MiddleCenter, false);
            _hudPrompt = L(22, new Color(0.96f, 0.93f, 0.85f), FontStyle.Normal, TextAnchor.MiddleLeft, false);
            _hudSmall = L(17, new Color(0.88f, 0.85f, 0.78f), FontStyle.Normal, TextAnchor.MiddleLeft, false);

            _card = new GUIStyle(GUI.skin.box) { padding = new RectOffset(14, 14, 10, 10) };
            _card.normal.background = Tex(new Color(1, 1, 1, 0.05f));
            _button = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, richText = true };
            _hudButton = new GUIStyle(_button) { fontSize = 18 };
            _hudButton.normal.textColor = Color.white; _hudButton.hover.textColor = Color.white;

            _questionStyle = new GUIStyle(GUI.skin.button) { fontSize = 18, alignment = TextAnchor.MiddleLeft, wordWrap = true, padding = new RectOffset(16, 16, 8, 8), richText = true };
            _questionStyle.normal.background = Tex(new Color(1, 1, 1, 0.06f)); _questionStyle.hover.background = Tex(new Color(Accent.r, Accent.g, Accent.b, 0.22f));
            _questionStyle.active.background = Tex(new Color(Accent.r, Accent.g, Accent.b, 0.35f));
            _questionStyle.normal.textColor = TextPrimary; _questionStyle.hover.textColor = Color.white; _questionStyle.active.textColor = Color.white;

            _input = new GUIStyle(GUI.skin.textField) { fontSize = 20, padding = new RectOffset(16, 16, 16, 16) };
            _input.normal.background = Tex(new Color(0, 0, 0, 0.55f)); _input.focused.background = Tex(new Color(0, 0, 0, 0.75f));
            _input.normal.textColor = TextPrimary; _input.focused.textColor = Color.white;
            _placeholder = L(18, new Color(0.55f, 0.58f, 0.62f), FontStyle.Italic, TextAnchor.MiddleLeft);
            _notes = new GUIStyle(GUI.skin.textArea) { fontSize = 18, wordWrap = true, padding = new RectOffset(14, 14, 12, 12) };
            _notes.normal.background = Tex(new Color(0.9f, 0.86f, 0.74f, 0.95f)); _notes.focused.background = Tex(new Color(0.95f, 0.91f, 0.8f, 1f));
            _notes.normal.textColor = Ink; _notes.focused.textColor = Ink;
            foreach (var st in new[] { _button, _hudButton, _questionStyle, _input, _card })
                if (st != null && _serif != null) st.font = _serif;
            // Belgeler, hikaye ve notlar daktilo harfiyle
            foreach (var st in new[] { _typewriter, _paperText, _docText, _notes, _fileBody, _fileVal, _fileKey, _fileHint })
                if (st != null && _mono != null) st.font = _mono;
        }
    }
}
