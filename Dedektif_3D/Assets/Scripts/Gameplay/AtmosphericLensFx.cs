using UnityEngine;
using Dedektif3D.Core;

namespace Dedektif3D.Gameplay
{
    /// <summary>
    /// Gerçekçi dedektiflik simülasyonu lens ve atmosfer katmanı:
    /// - Yağmurlu havada kamera camına çarpan damlaların kırılması & akması
    /// - Soğuktan sıcak odaya girişte lens buğulanması (condensation fog)
    /// - Yüksek nabız / stres / karanlık anlarda nefes alma & vinyet salınımı
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class AtmosphericLensFx : MonoBehaviour
    {
        public static AtmosphericLensFx Instance { get; private set; }

        private Camera _cam;
        private Texture2D _dropletTex;
        private float _fogTransition = 0f;
        private float _lastIndoorState = 0f;
        private float _stressPulse = 0f;

        void Awake()
        {
            Instance = this;
            _cam = GetComponent<Camera>();
            GenerateDropletMap();
        }

        void GenerateDropletMap()
        {
            // Doku ekrana gerilir: küçük yarıçap ve yumuşak kenar, damlaların büyük halka lekesi gibi görünmesini önler
            const int size = 512;
            _dropletTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }

            // Rastgele su damlacıkları oluştur
            Random.State oldState = Random.state;
            Random.InitState(1887);

            for (int d = 0; d < 60; d++)
            {
                Vector2 center = new Vector2(Random.Range(10, size - 10), Random.Range(10, size - 10));
                float radius = Random.Range(1.5f, 3.5f);

                int minX = Mathf.Max(0, (int)(center.x - radius));
                int maxX = Mathf.Min(size - 1, (int)(center.x + radius));
                int minY = Mathf.Max(0, (int)(center.y - radius));
                int maxY = Mathf.Min(size - 1, (int)(center.y + radius));

                for (int y = minY; y <= maxY; y++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), center);
                        if (dist <= radius)
                        {
                            float alpha = 0.3f * (1f - dist / radius);
                            pixels[y * size + x] = new Color(0.85f, 0.95f, 1f, alpha);
                        }
                    }
                }
            }

            Random.state = oldState;
            _dropletTex.SetPixels(pixels);
            _dropletTex.Apply();
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            // İç / Dış mekan buğulanma geçişi
            bool isIndoor = gm.InsideBuilding;
            float targetIndoor = isIndoor ? 1f : 0f;
            _lastIndoorState = Mathf.MoveTowards(_lastIndoorState, targetIndoor, Time.deltaTime * 1.5f);

            // Binalara yeni girildiğinde anlık buğu
            if (isIndoor && _fogTransition < 1f)
            {
                _fogTransition = Mathf.MoveTowards(_fogTransition, 1f, Time.deltaTime * 0.4f);
            }
            else if (!isIndoor && _fogTransition > 0f)
            {
                _fogTransition = Mathf.MoveTowards(_fogTransition, 0f, Time.deltaTime * 0.8f);
            }

            // Stres / Nabız nabzı (Sorgu ekranı veya ceset başındayken)
            if (gm.UI != null && gm.UI.ShowingInterrogation)
            {
                _stressPulse = (Mathf.Sin(Time.time * 4.5f) + 1f) * 0.5f;
            }
            else
            {
                _stressPulse = Mathf.MoveTowards(_stressPulse, 0f, Time.deltaTime * 2f);
            }
        }

        void OnGUI()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Phase != GamePhase.Playing) return;

            // 1. Dışarıdayken lens üzerindeki yağmur damlası kırılmaları
            if (!gm.InsideBuilding && _dropletTex != null)
            {
                Color prev = GUI.color;
                float rainAlpha = 0.22f + Mathf.Sin(Time.time * 0.5f) * 0.05f;
                GUI.color = new Color(1f, 1f, 1f, rainAlpha);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _dropletTex, ScaleMode.StretchToFill);
                GUI.color = prev;
            }

            // 2. Odaya ilk girişteki buğulanma
            if (_fogTransition > 0.05f && _fogTransition < 0.95f)
            {
                float fogA = Mathf.Sin(_fogTransition * Mathf.PI) * 0.35f;
                Color prev = GUI.color;
                GUI.color = new Color(0.9f, 0.9f, 0.95f, fogA);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture, ScaleMode.StretchToFill);
                GUI.color = prev;
            }

            // 3. Stres ve gerilim anında hafif vinyet kararması
            if (_stressPulse > 0.02f)
            {
                Color prev = GUI.color;
                GUI.color = new Color(0.6f, 0.05f, 0.05f, _stressPulse * 0.14f);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture, ScaleMode.StretchToFill);
                GUI.color = prev;
            }
        }
    }
}

