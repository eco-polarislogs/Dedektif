using UnityEngine;

namespace Dedektif3D.World
{
    /// <summary>
    /// Koddan üretilen yardımcı dokular. Random kullanmaz: kasaba üretiminin tohumlu sırası değişmez.
    /// </summary>
    public static class ProceduralTextures
    {
        const int WindowVariants = 3;
        static readonly Texture2D[] _windows = new Texture2D[WindowVariants];
        static Texture2D _puddle, _needles, _leaves, _leafCard;

        static float Smooth(float edge0, float edge1, float x)
        {
            float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        static Texture2D _tile;

        /// <summary>Sırlı fayans: koyu derz, taşlar arasında hafif ton farkı (bir tekrarda 4 x 4 taş).</summary>
        public static Texture2D Tile
        {
            get
            {
                if (_tile != null) return _tile;
                const int S = 256, Cell = S / 4, Grout = 3;
                var t = new Texture2D(S, S, TextureFormat.RGB24, true) { name = "Fayans", wrapMode = TextureWrapMode.Repeat, anisoLevel = 4 };
                var px = new Color[S * S];
                for (int y = 0; y < S; y++)
                    for (int x = 0; x < S; x++)
                    {
                        int cx = x / Cell, cy = y / Cell;
                        bool grout = x % Cell < Grout || y % Cell < Grout;
                        float tone = 0.88f + ((cx * 7 + cy * 13) % 5) * 0.03f;
                        float v = grout ? 0.32f : tone;
                        px[y * S + x] = new Color(v, v, v);
                    }
                t.SetPixels(px);
                t.Apply(true);
                _tile = t;
                return t;
            }
        }

        static Texture2D _weave;

        /// <summary>Yün kumaş dokusu: çapraz dokuma ve hafif leke. Kıyafetlerin plastik görünmesini engeller.</summary>
        public static Texture2D Weave
        {
            get
            {
                if (_weave != null) return _weave;
                const int S = 128;
                var t = new Texture2D(S, S, TextureFormat.RGB24, true) { name = "Dokuma", wrapMode = TextureWrapMode.Repeat, anisoLevel = 4 };
                var px = new Color[S * S];
                for (int y = 0; y < S; y++)
                    for (int x = 0; x < S; x++)
                    {
                        float twill = Mathf.Sin((x + y) * 0.098f * 8f) * 0.5f + 0.5f;
                        float cross = Mathf.Sin((x - y) * 0.098f * 16f) * 0.5f + 0.5f;
                        float blotch = Mathf.PerlinNoise(x * 0.04f + 3f, y * 0.04f + 7f);
                        float v = 0.8f + twill * 0.09f + cross * 0.04f + (blotch - 0.5f) * 0.14f;
                        px[y * S + x] = new Color(v, v, v);
                    }
                t.SetPixels(px); t.Apply(true);
                _weave = t;
                return t;
            }
        }

        /// <summary>Camın ardında lamba ışığı alan oda: perde, stor veya mobilya gölgesi. Parlaklık odanın içinde değişir.</summary>
        public static Texture2D WindowInterior(int variant)
        {
            int v = Mathf.Abs(variant) % WindowVariants;
            if (_windows[v] != null) return _windows[v];

            const int W = 64, H = 96;
            var tex = new Texture2D(W, H, TextureFormat.RGB24, true) { name = "PencereIci_" + v, wrapMode = TextureWrapMode.Clamp };
            var px = new Color[W * H];
            float lampX = v == 1 ? 0.34f : v == 2 ? 0.68f : 0.5f;
            float curtainWidth = v == 0 ? 0.3f : v == 1 ? 0.22f : 0.15f;
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    float u = (x + 0.5f) / W, n = (y + 0.5f) / H;

                    float dist = new Vector2((u - lampX) * 1.2f, n - 0.38f).magnitude;
                    float glow = Smooth(0f, 1f, Mathf.Clamp01(1.25f - dist * 1.7f));
                    var c = Color.Lerp(new Color(0.2f, 0.085f, 0.025f), new Color(1f, 0.7f, 0.36f), glow);
                    c *= 0.88f + 0.24f * Mathf.PerlinNoise(u * 5f + v * 3f, n * 7f);

                    float side = Mathf.Min(u, 1f - u);
                    if (side < curtainWidth)
                    {
                        float fold = 0.55f + 0.45f * Mathf.Sin(u * W * 0.75f + v);
                        var curtain = Color.Lerp(new Color(0.3f, 0.1f, 0.05f), new Color(0.62f, 0.34f, 0.16f), glow) * fold;
                        c = Color.Lerp(curtain, c, Smooth(curtainWidth - 0.04f, curtainWidth, side));
                    }

                    if (v == 1 && n > 0.62f)
                        c = Color.Lerp(c, new Color(0.55f, 0.36f, 0.17f) * (0.82f + 0.18f * Mathf.Sin(n * 90f)), 0.92f);
                    if (v == 2 && n < 0.24f && u > 0.2f && u < 0.72f)
                        c *= 0.22f;

                    c.a = 1f;
                    px[y * W + x] = c;
                }
            }
            tex.SetPixels(px);
            tex.Apply(true, true);
            _windows[v] = tex;
            return tex;
        }

        /// <summary>Su birikintisi maskesi: düzensiz kenarlı, dışa doğru solan alfa.</summary>
        public static Texture2D PuddleMask
        {
            get
            {
                if (_puddle != null) return _puddle;
                const int S = 128;
                var tex = new Texture2D(S, S, TextureFormat.RGBA32, true) { name = "SuBirikintisiMaskesi", wrapMode = TextureWrapMode.Clamp };
                var px = new Color[S * S];
                for (int y = 0; y < S; y++)
                {
                    for (int x = 0; x < S; x++)
                    {
                        float u = (x + 0.5f) / S - 0.5f, w = (y + 0.5f) / S - 0.5f;
                        float wobble = (Mathf.PerlinNoise(u * 7f + 11f, w * 7f + 5f) - 0.5f) * 0.24f;
                        float r = new Vector2(u, w).magnitude * 2f + wobble;
                        float a = 1f - Smooth(0.5f, 0.95f, r);
                        px[y * S + x] = new Color(1f, 1f, 1f, a);
                    }
                }
                tex.SetPixels(px);
                tex.Apply(true, true);
                _puddle = tex;
                return tex;
            }
        }

        /// <summary>Çam iğneleri: U yönünde sık çizgiler (mesh'te tepeden uca doğru uzanır).</summary>
        public static Texture2D Needles
        {
            get
            {
                if (_needles != null) return _needles;
                const int S = 128;
                var tex = new Texture2D(S, S, TextureFormat.RGB24, true) { name = "CamIgneleri", wrapMode = TextureWrapMode.Repeat };
                var px = new Color[S * S];
                for (int y = 0; y < S; y++)
                {
                    for (int x = 0; x < S; x++)
                    {
                        float u = (float)x / S, n = (float)y / S;
                        float streak = 0.5f + 0.5f * Mathf.Sin(u * Mathf.PI * 2f * 21f + Mathf.PerlinNoise(u * 9f, n * 3f) * 7f);
                        float mottled = Mathf.PerlinNoise(u * 6f + 3f, n * 6f + 9f);
                        float value = Mathf.Lerp(0.42f, 1f, streak * 0.62f + mottled * 0.38f);
                        px[y * S + x] = new Color(value, value, value, 1f);
                    }
                }
                tex.SetPixels(px);
                tex.Apply(true, true);
                _needles = tex;
                return tex;
            }
        }

        static float Hash(int a, int b)
        {
            float s = Mathf.Sin(a * 12.9898f + b * 78.233f) * 43758.547f;
            return s - Mathf.Floor(s);
        }

        /// <summary>Yaprak kartı: saydam zemin üzerinde üst üste binen tek tek yapraklar.</summary>
        public static Texture2D LeafCard
        {
            get
            {
                if (_leafCard != null) return _leafCard;
                const int S = 128, Count = 170;
                var tex = new Texture2D(S, S, TextureFormat.RGBA32, true) { name = "YaprakKarti", wrapMode = TextureWrapMode.Clamp };
                var px = new Color[S * S];
                // Saydam pikseller de yaprak tonunda: uzak mip seviyelerinde koyu hale oluşmaz.
                for (int i = 0; i < px.Length; i++) px[i] = new Color(0.7f, 0.7f, 0.7f, 0f);

                for (int leaf = 0; leaf < Count; leaf++)
                {
                    float angle = Hash(leaf, 1) * Mathf.PI * 2f;
                    float dist = Mathf.Sqrt(Hash(leaf, 2)) * S * 0.44f;
                    float cx = S * 0.5f + Mathf.Cos(angle) * dist, cy = S * 0.5f + Mathf.Sin(angle) * dist;
                    float rot = Hash(leaf, 3) * Mathf.PI;
                    float major = Mathf.Lerp(5.5f, 9f, Hash(leaf, 4)), minor = Mathf.Lerp(2.4f, 3.8f, Hash(leaf, 5));
                    float shade = Mathf.Lerp(0.5f, 1f, Hash(leaf, 6));
                    float cos = Mathf.Cos(rot), sin = Mathf.Sin(rot);
                    int reach = Mathf.CeilToInt(major);
                    for (int y = Mathf.Max(0, (int)cy - reach); y <= Mathf.Min(S - 1, (int)cy + reach); y++)
                    {
                        for (int x = Mathf.Max(0, (int)cx - reach); x <= Mathf.Min(S - 1, (int)cx + reach); x++)
                        {
                            float dx = x - cx, dy = y - cy;
                            float along = (dx * cos + dy * sin) / major, across = (-dx * sin + dy * cos) / minor;
                            if (along * along + across * across > 1f) continue;
                            float value = shade * (0.88f + 0.12f * Mathf.Abs(across));
                            px[y * S + x] = new Color(value, value, value, 1f);
                        }
                    }
                }
                tex.SetPixels(px);
                tex.Apply(true, true);
                _leafCard = tex;
                return tex;
            }
        }

        /// <summary>Yaprak kümeleri: koyu boşluklu benekli doku.</summary>
        public static Texture2D Leaves
        {
            get
            {
                if (_leaves != null) return _leaves;
                const int S = 128;
                var tex = new Texture2D(S, S, TextureFormat.RGB24, true) { name = "YaprakKumeleri", wrapMode = TextureWrapMode.Repeat };
                var px = new Color[S * S];
                for (int y = 0; y < S; y++)
                {
                    for (int x = 0; x < S; x++)
                    {
                        float u = (float)x / S, n = (float)y / S;
                        float coarse = Mathf.PerlinNoise(u * 8f, n * 8f);
                        float fine = Mathf.PerlinNoise(u * 34f + 7f, n * 34f + 2f);
                        float value = Mathf.Lerp(0.3f, 1f, Smooth(0.25f, 0.8f, coarse * 0.55f + fine * 0.45f));
                        px[y * S + x] = new Color(value, value, value, 1f);
                    }
                }
                tex.SetPixels(px);
                tex.Apply(true, true);
                _leaves = tex;
                return tex;
            }
        }
    }
}
