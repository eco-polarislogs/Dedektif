using System.Collections.Generic;
using UnityEngine;

namespace Dedektif3D.World
{
    /// <summary>
    /// Runtime'da URP/Lit materyalleri ve prosedürel dokuları (tuğla, parke taşı, ahşap, kiremit) üretir.
    /// Hiçbir asset dosyasına ihtiyaç duymaz.
    /// </summary>
    public static class MaterialLibrary
    {
        static readonly Dictionary<string, Material> Cache = new Dictionary<string, Material>();
        static Shader _lit;
        static Shader _unlit;

        public static Shader LitShader
        {
            get
            {
                if (_lit == null)
                {
                    _lit = Shader.Find("Universal Render Pipeline/Lit");
                    if (_lit == null) _lit = Shader.Find("Standard");
                }
                return _lit;
            }
        }

        public static Shader UnlitShader
        {
            get
            {
                if (_unlit == null)
                {
                    _unlit = Shader.Find("Universal Render Pipeline/Unlit");
                    if (_unlit == null) _unlit = Shader.Find("Unlit/Color");
                }
                return _unlit;
            }
        }

        // ---------- Renk yardımcıları ----------
        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }

        // ---------- Temel materyaller ----------
        public static Material Get(string key, Color color, float smoothness = 0.2f, float metallic = 0f, Texture2D tex = null)
        {
            if (Cache.TryGetValue(key, out var m) && m != null) return m;
            m = new Material(LitShader) { name = "M_" + key };
            m.SetFloat("_Surface", 0f);
            m.SetFloat("_Blend", 0f);
            m.SetOverrideTag("RenderType", "Opaque");
            m.renderQueue = 2000;
            SetColor(m, color);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smoothness);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
            // Gerçek yüzey dokusu (fotoğraf tabanlı PBR): sıva, tuğla, taş, ahşap, kiremit, parke taşı, çamurlu yol...
            var set = PbrSetFor(key, tex, color);
            if (set != null && TryPbr(set, out var pd, out var pn))
            {
                var avg = PbrAvg.TryGetValue(set, out var a) ? a : new Vector4(0.5f, 0.5f, 0.5f, 0.2f);
                // İstenen renk = dokunun ortalama rengi olsun diye taban rengi dokunun ortalamasına bölünür
                var ratio = new Color(Mathf.Min(2.6f, color.r / Mathf.Max(0.04f, avg.x)), Mathf.Min(2.6f, color.g / Mathf.Max(0.04f, avg.y)),
                                      Mathf.Min(2.6f, color.b / Mathf.Max(0.04f, avg.z)), 1f);
                if (set == "plasterold")
                {
                    // Bu dokuda sıvanın altından tuğla görünür: güçlü renk çarpanı tuğlaları da boyar.
                    float mean = (ratio.r + ratio.g + ratio.b) / 3f;
                    ratio = Color.Lerp(new Color(mean, mean, mean, 1f), ratio, 0.4f);
                }
                SetColor(m, ratio);
                tex = pd;
                if (pn != null && m.HasProperty("_BumpMap"))
                {
                    m.SetTexture("_BumpMap", pn);
                    m.SetFloat("_BumpScale", 1f);
                    m.EnableKeyword("_NORMALMAP");
                }
                if (smoothness <= avg.w * 1.05f && m.HasProperty("_SmoothnessTextureChannel"))
                {
                    // Kuru yüzey: pürüzsüzlük dokunun alfa kanalından (taş aralarında mat, yüzlerde parlak)
                    m.SetFloat("_SmoothnessTextureChannel", 1f);
                    m.EnableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
                    m.SetFloat("_Smoothness", Mathf.Clamp01(smoothness / Mathf.Max(0.05f, avg.w)));
                }
                TileScale[m] = PbrTile.TryGetValue(set, out var ts) ? ts : 1f;
            }
            if (tex != null)
            {
                if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", tex);
                if (m.HasProperty("_MainTex")) m.SetTexture("_MainTex", tex);
            }
            Cache[key] = m;
            return m;
        }

        // ---------- Doku tekrarı: her tekrar değeri için paylaşılan malzeme kopyası ----------
        static readonly Dictionary<(Material, int, int), Material> _tiled = new Dictionary<(Material, int, int), Material>();
        static readonly Dictionary<Material, Material> _tiledBase = new Dictionary<Material, Material>();

        /// <summary>
        /// Malzemenin verilen doku tekrarlı kopyası. Tekrar değeri 0.05 adımına yuvarlanır, böylece benzer ölçüdeki
        /// objeler aynı kopyayı paylaşır. Doku boyutu çarpanı (TileScale) burada uygulanır.
        /// </summary>
        public static Material Tiled(Material mat, Vector2 tiling)
        {
            var source = _tiledBase.TryGetValue(mat, out var b) ? b : mat;
            if (TileScale.TryGetValue(source, out var k)) tiling *= k;
            int ix = Mathf.RoundToInt(tiling.x * 20f), iy = Mathf.RoundToInt(tiling.y * 20f);
            var key = (source, ix, iy);
            if (_tiled.TryGetValue(key, out var m) && m != null) return m;
            m = new Material(source) { name = source.name + $"_t{ix}x{iy}" };
            var scale = new Vector2(ix / 20f, iy / 20f);
            if (m.HasProperty("_BaseMap")) m.SetTextureScale("_BaseMap", scale);
            if (m.HasProperty("_MainTex")) m.SetTextureScale("_MainTex", scale);
            _tiled[key] = m;
            _tiledBase[m] = source;
            return m;
        }

        public static Material Emissive(string key, Color color, float intensity = 2f)
        {
            if (Cache.TryGetValue(key, out var m) && m != null) return m;
            m = new Material(LitShader) { name = "ME_" + key };
            SetColor(m, color * 0.6f);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.8f);
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", color * intensity);
            Cache[key] = m;
            return m;
        }

        // ---------- Gerçek yüzey dokuları (CC0 — Poly Haven; Assets/Resources/PBR) ----------
        /// <summary>false ise koddan üretilen eski dokular kullanılır (Ayarlar → düşük kalite).</summary>
        public static bool UsePbr = true;
        /// <summary>Dokunun ortalama rengi (rgb) ve ortalama pürüzsüzlüğü (w) — tools/pack_pbr.py ile ölçüldü.</summary>
        static readonly Dictionary<string, Vector4> PbrAvg = new Dictionary<string, Vector4>
        {
            { "plaster", new Vector4(0.556f, 0.531f, 0.480f, 0.093f) },
            { "brick", new Vector4(0.417f, 0.327f, 0.295f, 0.468f) },
            { "stone", new Vector4(0.541f, 0.494f, 0.398f, 0.478f) },
            { "wood", new Vector4(0.376f, 0.295f, 0.224f, 0.346f) },
            { "roof", new Vector4(0.570f, 0.313f, 0.168f, 0.085f) },
            { "slate", new Vector4(0.565f, 0.525f, 0.460f, 0.155f) },
            { "cobble", new Vector4(0.479f, 0.442f, 0.364f, 0.227f) },
            { "mud", new Vector4(0.313f, 0.186f, 0.108f, 0.172f) },
            { "grass", new Vector4(0.311f, 0.241f, 0.083f, 0.057f) },
            { "floor", new Vector4(0.339f, 0.285f, 0.243f, 0.186f) },
            { "ashlar", new Vector4(0.435f, 0.374f, 0.280f, 0.233f) },
            { "rubble", new Vector4(0.290f, 0.267f, 0.238f, 0.161f) },
            { "rockface", new Vector4(0.399f, 0.346f, 0.290f, 0.159f) },
            { "plasterold", new Vector4(0.572f, 0.506f, 0.443f, 0.094f) },
            { "bark", new Vector4(0.316f, 0.288f, 0.218f, 0.098f) },
        };
        /// <summary>Doku tekrar çarpanı: bir doku karesinin gerçek boyutuna göre (tuğla ~1 m, parke ~1.5 m...).</summary>
        static readonly Dictionary<string, float> PbrTile = new Dictionary<string, float>
        {
            { "plaster", 0.8f }, { "brick", 1.6f }, { "stone", 1.2f }, { "wood", 1.1f }, { "roof", 1.4f }, { "slate", 1.4f },
            { "cobble", 1.5f }, { "mud", 0.7f }, { "grass", 0.8f }, { "floor", 1.2f },
            { "ashlar", 0.55f }, { "rubble", 0.6f }, { "rockface", 0.35f }, { "plasterold", 0.6f }, { "bark", 1f },
        };
        /// <summary>Malzemeye özel tekrar çarpanı (Shapes.SetTiling uygular).</summary>
        public static readonly Dictionary<Material, float> TileScale = new Dictionary<Material, float>();
        static readonly Dictionary<string, (Texture2D d, Texture2D n)> _pbr = new Dictionary<string, (Texture2D, Texture2D)>();

        static bool TryPbr(string set, out Texture2D d, out Texture2D n)
        {
            d = n = null;
            if (!UsePbr) return false;
            if (!_pbr.TryGetValue(set, out var p))
            {
                p = (Resources.Load<Texture2D>("PBR/" + set + "_d"), Resources.Load<Texture2D>("PBR/" + set + "_n"));
                _pbr[set] = p;
            }
            d = p.d; n = p.n;
            return d != null;
        }

        static string PbrSetFor(string key, Texture2D tex, Color color)
        {
            // Duvarlar: kaba taş ev ve köy yapılarında, kesme taş resmî yapılarda; sıvanın yarısı eskimiş.
            if (key == "stonewall_rural" || key == "stonewall_house") return "rubble";
            if (key.StartsWith("stonewall") || key.StartsWith("karakol_stone") || key == "tower_stone" || key == "mosque_stone") return "ashlar";
            if (key == "cliff_rock") return "rockface";
            if (key.StartsWith("bark_")) return "bark";
            if (key.StartsWith("plaster_")) return "plasterold";
            if (key.StartsWith("brick_")) return "brick";
            if (key.StartsWith("wood_")) return "wood";
            if (key.StartsWith("roof_"))
            {
                Color.RGBToHSV(color, out _, out var sat, out _);
                return sat < 0.3f ? "slate" : "roof";
            }
            if (key.StartsWith("cobble")) return "cobble";
            if (key == "dirtroad") return "mud";
            if (key == "grass" || key.StartsWith("grass_") || key == "terrace_grass") return "grass";
            if (key == "floorboards") return "floor";
            if (key == "asphalt" || tex == null) return null;
            if (tex == _brick) return key.Contains("stone") ? "stone" : "brick";
            if (tex == _wood) return "wood";
            if (tex == _roof) return "roof";
            if (tex == _cobble) return "cobble";
            if (tex == _floor) return "floor";
            if (tex == _noise) return "stone";
            return null;
        }

        static uint StableHash(string s)
        {
            unchecked
            {
                uint h = 2166136261;
                foreach (char c in s) h = (h ^ c) * 16777619;
                return h;
            }
        }

        /// <summary>Önbellekteki materyal (yoksa null) — gün/gece döngüsü lamba camlarını bununla kısar.</summary>
        public static Material Cached(string key) => Cache.TryGetValue(key, out var m) ? m : null;

        public static Material Unlit(string key, Color color)
        {
            if (Cache.TryGetValue(key, out var m) && m != null) return m;
            m = new Material(UnlitShader) { name = "MU_" + key };
            SetColor(m, color);
            Cache[key] = m;
            return m;
        }

        static void SetColor(Material m, Color c)
        {
            var opaqueColor = new Color(c.r, c.g, c.b, 1f);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", opaqueColor);
            if (m.HasProperty("_Color")) m.SetColor("_Color", opaqueColor);
        }

        // ---------- Hazır yüzey materyalleri ----------
        public static Material Brick(Color tint) => Get("brick_" + ColorUtility.ToHtmlStringRGB(tint), tint, 0.1f, 0f, BrickTexture);
        public static Material Plaster(Color tint) => Get("plaster_" + ColorUtility.ToHtmlStringRGB(tint), tint, 0.05f, 0f, NoiseTexture);
        public static Material Wood(Color tint) => Get("wood_" + ColorUtility.ToHtmlStringRGB(tint), tint, 0.25f, 0f, WoodTexture);
        public static Material RoofTiles(Color tint) => Get("roof_" + ColorUtility.ToHtmlStringRGB(tint), tint, 0.3f, 0f, RoofTexture);
        public static Material Cobble => Get("cobble", new Color(0.55f, 0.55f, 0.58f), 0.35f, 0f, CobbleTexture);
        public static Material Asphalt => Get("asphalt", new Color(0.16f, 0.16f, 0.18f), 0.55f, 0f, NoiseTexture);
        public static Material Grass => Get("grass", new Color(0.13f, 0.22f, 0.13f), 0.05f, 0f, NoiseTexture);
        public static Material Stone => Get("stone", new Color(0.45f, 0.45f, 0.47f), 0.15f, 0f, NoiseTexture);
        public static Material Metal => Get("metal", new Color(0.18f, 0.18f, 0.2f), 0.7f, 0.9f);
        public static Material Glass => Get("glass", new Color(0.08f, 0.12f, 0.16f), 0.95f, 0.3f);

        /// <summary>DayNightCycle bu camları gündüz kısar.</summary>
        public static readonly string[] WarmWindowKeys = { "warm_window", "warm_window_1", "warm_window_2" };
        static int _windowCounter;

        /// <summary>Işığı yanan pencere: koyu parlak cam ve ardında perdeli bir oda. Çağrıldıkça üç farklı oda arasında döner.</summary>
        public static Material WarmWindow
        {
            get
            {
                int v = _windowCounter++ % WarmWindowKeys.Length;
                string key = WarmWindowKeys[v];
                if (Cache.TryGetValue(key, out var m) && m != null) return m;
                m = new Material(LitShader) { name = "ME_" + key };
                SetColor(m, new Color(0.03f, 0.028f, 0.025f));
                if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.88f);
                m.EnableKeyword("_EMISSION");
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                if (m.HasProperty("_EmissionMap")) m.SetTexture("_EmissionMap", ProceduralTextures.WindowInterior(v));
                if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.78f) * 0.8f);
                Cache[key] = m;
                return m;
            }
        }

        /// <summary>Kenarları solan ince su tabakası: gökyüzünü ve lambaları yansıtır, zemine karışır.</summary>
        public static Material PuddleWater
        {
            get
            {
                const string key = "puddle_soft";
                if (Cache.TryGetValue(key, out var m) && m != null) return m;
                m = new Material(LitShader) { name = "M_" + key };
                SetColor(m, new Color(0.42f, 0.46f, 0.52f, 0.78f));
                if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1f);
                if (m.HasProperty("_Blend")) m.SetFloat("_Blend", 0f);
                if (m.HasProperty("_SrcBlend")) m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                if (m.HasProperty("_DstBlend")) m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                if (m.HasProperty("_SrcBlendAlpha")) m.SetFloat("_SrcBlendAlpha", (float)UnityEngine.Rendering.BlendMode.One);
                if (m.HasProperty("_DstBlendAlpha")) m.SetFloat("_DstBlendAlpha", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                if (m.HasProperty("_ZWrite")) m.SetFloat("_ZWrite", 0f);
                m.SetOverrideTag("RenderType", "Transparent");
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                m.SetShaderPassEnabled("ShadowCaster", false);
                if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.96f);
                if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0.25f);
                if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", ProceduralTextures.PuddleMask);
                Cache[key] = m;
                return m;
            }
        }

        public static Material Bark(Color tint) => Get("bark_" + ColorUtility.ToHtmlStringRGB(tint), tint, 0.08f);
        public static Material PineNeedles(Color tint) => Get("needles_" + ColorUtility.ToHtmlStringRGB(tint), tint, 0.12f, 0f, ProceduralTextures.Needles);
        public static Material Foliage(Color tint) => Get("foliage_" + ColorUtility.ToHtmlStringRGB(tint), tint, 0.1f, 0f, ProceduralTextures.Leaves);

        /// <summary>Yaprak kartları: alfa kesimli, iki yüzlü. Ağaç tacının dış kenarına gerçek yaprak silueti verir.</summary>
        public static Material FoliageCards(Color tint)
        {
            string key = "foliagecard_" + ColorUtility.ToHtmlStringRGB(tint);
            if (Cache.TryGetValue(key, out var m) && m != null) return m;
            m = new Material(LitShader) { name = "M_" + key };
            SetColor(m, tint);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.15f);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0f);
            if (m.HasProperty("_AlphaClip")) m.SetFloat("_AlphaClip", 1f);
            if (m.HasProperty("_Cutoff")) m.SetFloat("_Cutoff", 0.42f);
            if (m.HasProperty("_Cull")) m.SetFloat("_Cull", 0f);
            m.EnableKeyword("_ALPHATEST_ON");
            m.SetOverrideTag("RenderType", "TransparentCutout");
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", ProceduralTextures.LeafCard);
            Cache[key] = m;
            return m;
        }
        public static Material Floorboards => Get("floorboards", new Color(0.45f, 0.3f, 0.18f), 0.35f, 0f, FloorTexture);

        // ---------- Prosedürel dokular ----------
        static Texture2D _brick, _noise, _wood, _roof, _cobble, _floor;

        public static Texture2D BrickTexture => _brick != null ? _brick : (_brick = MakeBrick());
        public static Texture2D NoiseTexture => _noise != null ? _noise : (_noise = MakeNoise());
        public static Texture2D WoodTexture => _wood != null ? _wood : (_wood = MakeWood());
        public static Texture2D RoofTexture => _roof != null ? _roof : (_roof = MakeRoof());
        public static Texture2D CobbleTexture => _cobble != null ? _cobble : (_cobble = MakeCobble());
        public static Texture2D FloorTexture => _floor != null ? _floor : (_floor = MakeFloor());

        static Texture2D NewTex(int size)
        {
            return new Texture2D(size, size, TextureFormat.RGBA32, true)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
                anisoLevel = 4
            };
        }

        static Texture2D MakeBrick()
        {
            const int s = 256;
            var t = NewTex(s);
            var px = new Color[s * s];
            int bw = 64, bh = 32, mortar = 3;
            for (int y = 0; y < s; y++)
            {
                int row = y / bh;
                int offset = (row % 2) * (bw / 2);
                for (int x = 0; x < s; x++)
                {
                    int lx = (x + offset) % bw;
                    int ly = y % bh;
                    bool isMortar = lx < mortar || ly < mortar;
                    int brickId = ((x + offset) / bw) * 31 + row * 17;
                    float variation = 0.82f + 0.18f * Mathf.PerlinNoise(brickId * 0.37f, row * 0.71f);
                    float grain = 0.92f + 0.08f * Mathf.PerlinNoise(x * 0.2f, y * 0.2f);
                    float v = isMortar ? 0.55f : variation * grain;
                    px[y * s + x] = isMortar ? new Color(0.62f, 0.6f, 0.56f) : new Color(v, v, v);
                }
            }
            t.SetPixels(px); t.Apply(true);
            return t;
        }

        static Texture2D MakeNoise()
        {
            const int s = 128;
            var t = NewTex(s);
            var px = new Color[s * s];
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    float n = 0.8f + 0.2f * Mathf.PerlinNoise(x * 0.08f, y * 0.08f) * Mathf.PerlinNoise(x * 0.31f + 5, y * 0.31f + 9);
                    n += Random.Range(-0.03f, 0.03f);
                    px[y * s + x] = new Color(n, n, n);
                }
            t.SetPixels(px); t.Apply(true);
            return t;
        }

        static Texture2D MakeWood()
        {
            const int s = 128;
            var t = NewTex(s);
            var px = new Color[s * s];
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    float ring = Mathf.Sin((x + Mathf.PerlinNoise(x * 0.05f, y * 0.02f) * 25f) * 0.35f) * 0.5f + 0.5f;
                    float v = 0.75f + ring * 0.25f;
                    px[y * s + x] = new Color(v, v, v);
                }
            t.SetPixels(px); t.Apply(true);
            return t;
        }

        static Texture2D MakeRoof()
        {
            const int s = 128;
            var t = NewTex(s);
            var px = new Color[s * s];
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    int row = y / 16;
                    float lx = ((x + (row % 2) * 8) % 16) / 16f;
                    float ly = (y % 16) / 16f;
                    float curve = Mathf.Sin(lx * Mathf.PI);
                    float v = 0.55f + 0.4f * curve * (0.6f + 0.4f * ly);
                    if (ly < 0.08f) v *= 0.5f;
                    px[y * s + x] = new Color(v, v, v);
                }
            t.SetPixels(px); t.Apply(true);
            return t;
        }

        static Texture2D MakeCobble()
        {
            const int s = 256;
            var t = NewTex(s);
            var px = new Color[s * s];
            int cell = 32;
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    // Basit Voronoi benzeri taş deseni
                    float minD = 999f, secD = 999f;
                    int cx = x / cell, cy = y / cell;
                    for (int oy = -1; oy <= 1; oy++)
                        for (int ox = -1; ox <= 1; ox++)
                        {
                            int gx = cx + ox, gy = cy + oy;
                            float jx = Mathf.PerlinNoise(gx * 1.7f, gy * 3.1f) * cell;
                            float jy = Mathf.PerlinNoise(gx * 2.9f + 7, gy * 1.3f + 3) * cell;
                            float dx = gx * cell + jx - x;
                            float dy = gy * cell + jy - y;
                            float d = Mathf.Sqrt(dx * dx + dy * dy);
                            if (d < minD) { secD = minD; minD = d; }
                            else if (d < secD) secD = d;
                        }
                    float edge = Mathf.Clamp01((secD - minD) / 4f);
                    float v = Mathf.Lerp(0.25f, 0.85f + 0.1f * Mathf.PerlinNoise(x * 0.1f, y * 0.1f), edge);
                    px[y * s + x] = new Color(v, v, v);
                }
            t.SetPixels(px); t.Apply(true);
            return t;
        }

        static Texture2D MakeFloor()
        {
            const int s = 128;
            var t = NewTex(s);
            var px = new Color[s * s];
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    int plank = x / 16;
                    bool seam = x % 16 == 0 || (y + plank * 37) % 96 == 0;
                    float v = seam ? 0.35f : 0.75f + 0.2f * Mathf.PerlinNoise(plank * 3.3f, y * 0.04f);
                    px[y * s + x] = new Color(v, v, v);
                }
            t.SetPixels(px); t.Apply(true);
            return t;
        }
    }
}
