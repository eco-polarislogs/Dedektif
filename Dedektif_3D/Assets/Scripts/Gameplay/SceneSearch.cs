using System.Collections.Generic;
using Dedektif3D.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dedektif3D.Gameplay
{
    /// <summary>
    /// FAREYLE ARAMA — web'deki "olay yerinde fareyi gezdir, delil parlasın" sisteminin 3D karşılığı.
    /// Bina içinde kendiliğinden açılır (Ayarlar'dan kapatılabilir), her yerde [Sağ Tık] / [V] ile açılıp kapanır:
    ///  • Kamera dedektifin gözüne geçer, imleç serbest kalır; fareyi ekranın kenarına götürmek etrafa baktırır.
    ///  • İmlecin altındaki delil, saklama yeri, şüpheli, kapı ya da ceset parlar ve adı imlecin yanında görünür.
    ///  • Sol tık incelemeyi açar: görüş hattı yeterlidir, eşyanın dibine yürümek gerekmez.
    /// Seçim, nesnenin ekrandaki kutusuyla yapılır (küçük eşyalar da kolay yakalanır); duvar arkasındakiler seçilmez.
    /// </summary>
    public class SceneSearch : MonoBehaviour
    {
        public static SceneSearch Instance { get; private set; }

        /// <summary>Normal kamerada ilgili eşyanın parlaması için en yakın etkileşim de vurgulanır.</summary>
        public Interactable Hovered { get; private set; }
        public bool Active { get; private set; }
        /// <summary>İmleç arayüz bağlantısının üstündeyken kamera kaydırılmaz (GameUI her kare yazar).</summary>
        public bool PanPaused { get; set; }

        GameManager _gm;
        ThirdPersonCamera _cam;
        Camera _camera;
        bool _bodyHidden;
        Interactable _forced;
        bool _hasForced;

        // Parıltı
        struct Applied { public Renderer R; public Material[] Original; }
        readonly List<Applied> _applied = new List<Applied>();
        static readonly List<Renderer> _rendererBuffer = new List<Renderer>();
        readonly Dictionary<Color, Material> _glowMaterials = new Dictionary<Color, Material>();
        Light _light;
        Shader _glowShader;
        bool _glowShaderMissing;

        void Awake() => Instance = this;
        void OnDestroy() { if (Instance == this) Instance = null; }

        public void Init(GameManager gm)
        {
            _gm = gm;
            _cam = gm.Player != null ? gm.Player.Cam : null;
            _camera = _cam != null ? _cam.GetComponent<Camera>() : null;
            var go = new GameObject("AramaIşığı");
            go.transform.SetParent(transform, false);
            _light = go.AddComponent<Light>();
            _light.type = LightType.Point;
            _light.range = 3.2f;
            _light.intensity = 0f;
            _light.shadows = LightShadows.None;
            _light.enabled = false;
        }

        // =================================================================
        //  Açma / kapama
        // =================================================================
        public void SetActive(bool on)
        {
            if (Active == on) return;
            Active = on;
            if (_cam != null) _cam.SearchMode = on;
            if (!on) SetHover(null);
        }

        /// <summary>Binaya girildi: ayar açıksa fareyle arama başlar.</summary>
        public void OnEnterRoom() { if (GameSettings.AutoSearch) SetActive(true); }

        /// <summary>Binadan çıkıldı: dışarıda normal üçüncü şahıs kamerası.</summary>
        public void OnLeaveRoom() => SetActive(false);

        /// <summary>Menü / sonuç ekranı: mod anında kapanır, kamera ve dedektifin bedeni normale döner.</summary>
        public void ForceOff()
        {
            SetActive(false);
            SetHover(null);
            _bodyHidden = false;
            if (_cam != null) _cam.ResetSearchBlend();
        }

        // =================================================================
        //  Her kare
        // =================================================================
        void Update()
        {
            if (_gm == null || _gm.Player == null) return;
            if (_gm.Phase != GamePhase.Playing) { if (Hovered != null) SetHover(null); return; }

            var ui = _gm.UI;
            bool modal = ui != null && ui.IsModalOpen;
            bool cinematic = _gm.Director != null && _gm.Director.Current == CinematicDirector.Mode.Focus;

            var mouse = Mouse.current;
            var kb = Keyboard.current;
            if (!modal && !cinematic && ((mouse != null && mouse.rightButton.wasPressedThisFrame) || (kb != null && kb.vKey.wasPressedThisFrame)))
            {
                SetActive(!Active);
                AudioManager.Instance?.TypeClick();
            }

            // Göz hizasına geçince dedektifin bedeni görüşü kapatmasın
            if (_cam != null)
            {
                bool hide = _cam.SearchBlend > 0.55f;
                if (hide != _bodyHidden) { _bodyHidden = hide; _gm.Player.SetVisible(!hide); }
            }

            Interactable target = null;
            if (!modal && !cinematic)
            {
                if (_hasForced) target = _forced != null && _forced.Searchable ? _forced : null;
                else if (Active) target = Application.isBatchMode || mouse == null ? null : Pick(mouse.position.ReadValue());
                else
                {
                    // Yanımızda yürüyen yol arkadaşı [E] ile konuşulabilir ama sürekli parlayıp dikkat dağıtmaz
                    var focus = _gm.Player.Focus;
                    target = focus != null && focus.FocusPenalty > 0f ? null : focus;
                }
            }
            SetHover(target);
            UpdateLight();
        }

        /// <summary>Sol tık: imlecin altındaki şeyi açar. GameUI, tıklama hiçbir arayüz öğesine gitmediyse çağırır.</summary>
        public bool ClickHovered()
        {
            if (!Active || Hovered == null || _gm == null || _gm.Phase != GamePhase.Playing) return false;
            if (_gm.UI != null && _gm.UI.IsModalOpen) return false;
            var it = Hovered;
            SetHover(null);
            it.Interact();
            return true;
        }

        /// <summary>Otomatik test / ekran görüntüsü aracı: imleç bu nesnenin üstündeymiş gibi davranır. null = gerçek imleç.</summary>
        public void DebugForceHover(Interactable it) { _forced = it; _hasForced = it != null; }

        // =================================================================
        //  Seçim
        // =================================================================
        /// <summary>Verilen ekran noktasının altındaki etkileşimi bulur (piksel, sol alt köşe başlangıçlı).</summary>
        public Interactable Pick(Vector2 mousePx)
        {
            if (_camera == null) return null;
            var camPos = _camera.transform.position;
            float pad = 12f * Screen.height / 1080f;
            Interactable best = null, companion = null;
            float bestArea = float.MaxValue;
            foreach (var it in Interactable.All)
            {
                if (it == null || !it.isActiveAndEnabled || !it.Searchable) continue;
                var quick = it.Point - camPos;
                float range = it.SearchRange;
                if (quick.sqrMagnitude > (range + 2f) * (range + 2f)) continue;
                var b = it.HoverBounds;
                if ((b.center - camPos).sqrMagnitude > range * range) continue;
                if (!ScreenRect(b, out var r)) continue;
                r.xMin -= pad; r.xMax += pad; r.yMin -= pad; r.yMax += pad;
                if (!r.Contains(mousePx)) continue;
                // Yanımızda yürüyen yol arkadaşı delilin ve saklama yerinin önüne geçmez: yalnızca imleçte başka bir şey yoksa seçilir
                if (it.FocusPenalty > 0f)
                {
                    if (companion == null && Visible(it, b)) companion = it;
                    continue;
                }
                float area = r.width * r.height;
                if (area >= bestArea) continue;
                if (!Visible(it, b)) continue;
                bestArea = area;
                best = it;
            }
            return best != null ? best : companion;
        }

        /// <summary>Nesnenin görüş hattında olup olmadığı (test aracı da kullanır).</summary>
        public bool CanSee(Interactable it) => it != null && Visible(it, it.HoverBounds);

        /// <summary>Nesnenin ekrandaki merkezi (test aracı için; görünmüyorsa false).</summary>
        public bool ScreenCenter(Interactable it, out Vector2 px)
        {
            px = default;
            if (it == null || !ScreenRect(it.HoverBounds, out var r)) return false;
            px = r.center;
            return true;
        }

        bool ScreenRect(Bounds b, out Rect rect)
        {
            rect = default;
            var c = b.center; var e = b.extents;
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            for (int i = 0; i < 8; i++)
            {
                var p = c + new Vector3((i & 1) == 0 ? -e.x : e.x, (i & 2) == 0 ? -e.y : e.y, (i & 4) == 0 ? -e.z : e.z);
                var s = _camera.WorldToScreenPoint(p);
                if (s.z <= 0.05f) return false;
                minX = Mathf.Min(minX, s.x); maxX = Mathf.Max(maxX, s.x);
                minY = Mathf.Min(minY, s.y); maxY = Mathf.Max(maxY, s.y);
            }
            rect = Rect.MinMaxRect(minX, minY, maxX, maxY);
            return true;
        }

        bool Visible(Interactable it, Bounds b)
        {
            var from = _camera.transform.position;
            int mask = ~((1 << 2) | (1 << World.TownGenerator.NpcBarrierLayer));
            // Merkez, üst kenar ve eşyanın biraz üstü: sandıkta / tezgâhta duran eşya da görülebilsin
            var top = b.center + Vector3.up * b.extents.y;
            return Sees(from, b.center, it, mask) || Sees(from, top, it, mask) || Sees(from, top + Vector3.up * 0.25f, it, mask);
        }

        static bool Sees(Vector3 from, Vector3 to, Interactable it, int mask)
        {
            var d = to - from;
            float dist = d.magnitude;
            if (dist < 0.01f) return true;
            if (!Physics.Raycast(from, d / dist, out var hit, dist, mask, QueryTriggerInteraction.Ignore)) return true;
            if (hit.distance >= dist - 0.3f) return true;
            return hit.transform == it.transform || hit.transform.IsChildOf(it.transform);
        }

        // =================================================================
        //  Parıltı
        // =================================================================
        void SetHover(Interactable it)
        {
            if (Hovered == it) return;
            ClearGlow();
            if (Hovered != null) Hovered.OnHoverChanged(false);
            Hovered = it;
            if (it == null) return;
            it.OnHoverChanged(true);
            ApplyGlow(it);
            if (Active) AudioManager.Instance?.Shimmer();
        }

        Material GlowMaterial(Color c)
        {
            if (_glowMaterials.TryGetValue(c, out var m) && m != null) return m;
            if (_glowShader == null && !_glowShaderMissing)
            {
                _glowShader = Resources.Load<Shader>("Shaders/HoverGlow");
                if (_glowShader == null || !_glowShader.isSupported)
                {
                    _glowShaderMissing = true;
                    Debug.LogWarning("[Dedektif3D] HoverGlow shader yüklenemedi: parıltı yalnızca ışık ve etiketle gösterilecek.");
                }
            }
            if (_glowShaderMissing) return null;
            m = new Material(_glowShader) { name = "M_AramaParıltısı" };
            m.SetColor("_GlowColor", c);
            _glowMaterials[c] = m;
            return m;
        }

        void ApplyGlow(Interactable it)
        {
            var mat = GlowMaterial(it.GlowColor);
            if (mat != null)
            {
                _rendererBuffer.Clear();
                it.CollectGlowRenderers(_rendererBuffer);
                foreach (var r in _rendererBuffer)
                {
                    // Deri mesh'lerinde bu malzeme figürü bembeyaz yapıyor: kişiler ışık ve isim etiketiyle vurgulanır
                    if (r == null || !r.enabled || r is SkinnedMeshRenderer) continue;
                    var original = r.sharedMaterials;
                    var withGlow = new Material[original.Length + 1];
                    original.CopyTo(withGlow, 0);
                    withGlow[original.Length] = mat;
                    r.sharedMaterials = withGlow;
                    _applied.Add(new Applied { R = r, Original = original });
                }
            }
            if (_light != null)
            {
                _light.color = it.GlowColor;
                _light.enabled = true;
            }
        }

        void ClearGlow()
        {
            foreach (var a in _applied)
                if (a.R != null) a.R.sharedMaterials = a.Original;
            _applied.Clear();
            if (_light != null) { _light.enabled = false; _light.intensity = 0f; }
        }

        void UpdateLight()
        {
            if (_light == null || !_light.enabled || Hovered == null) return;
            var b = Hovered.HoverBounds;
            _light.transform.position = b.center + Vector3.up * Mathf.Max(0.35f, b.extents.y + 0.2f);
            _light.intensity = 1.5f + Mathf.Sin(Time.time * 4.6f) * 0.35f;
        }

        /// <summary>Oda bilgisi: içinde bulunulan binada alınmış / toplam delil.</summary>
        public static void RoomCounts(GameManager gm, out int taken, out int total)
        {
            taken = 0; total = 0;
            if (gm == null || gm.CurrentBuilding == null) return;
            foreach (var c in gm.Clues)
            {
                if (c.Def.BuildingNpcId != gm.CurrentBuilding.NpcId) continue;
                total++;
                if (c.InBag) taken++;
            }
        }
    }
}
