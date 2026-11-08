using UnityEngine;

namespace Dedektif3D.World
{
    /// <summary>
    /// Primitive (küp, silindir, küre, kapsül) ve prosedürel prizma/çatı mesh'leri üreten yardımcı sınıf.
    /// Tüm 3D dünya bu fonksiyonlarla inşa edilir.
    /// </summary>
    public static class Shapes
    {
        static Font _font;
        static Mesh _prism;

        public static Font DefaultFont
        {
            get
            {
                if (_font == null)
                {
                    try { _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
                    if (_font == null) { try { _font = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { } }
                    if (_font == null) _font = Font.CreateDynamicFontFromOSFont("Arial", 64);
                }
                return _font;
            }
        }

        public static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 localPos, Vector3 scale,
            Material mat, Vector3? euler = null, bool collider = true, float texTiling = 0f)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(euler ?? Vector3.zero);
            go.transform.localScale = scale;
            var r = go.GetComponent<Renderer>();
            if (mat != null) r.sharedMaterial = mat;
            if (!collider)
            {
                var c = go.GetComponent<Collider>();
                if (c != null) Object.Destroy(c);
            }
            if (texTiling > 0f)
            {
                // İnce zemin levhasında doku üst yüzeye göre tekrarlanır; yoksa yükseklik çok küçük olduğundan doku çizgi gibi uzar.
                bool flat = scale.y < 0.15f * Mathf.Min(scale.x, scale.z);
                SetTiling(r, flat
                    ? new Vector2(scale.x * texTiling, scale.z * texTiling)
                    : new Vector2(Mathf.Max(scale.x, scale.z) * texTiling, scale.y * texTiling));
            }
            return go;
        }

        public static GameObject Box(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat,
            Vector3? euler = null, bool collider = true, float texTiling = 0f)
            => Prim(PrimitiveType.Cube, name, parent, pos, scale, mat, euler, collider, texTiling);

        public static GameObject Cyl(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat,
            Vector3? euler = null, bool collider = true)
            => Prim(PrimitiveType.Cylinder, name, parent, pos, scale, mat, euler, collider);

        public static GameObject Sphere(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat,
            bool collider = false)
            => Prim(PrimitiveType.Sphere, name, parent, pos, scale, mat, null, collider);

        public static GameObject Capsule(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat,
            bool collider = false)
            => Prim(PrimitiveType.Capsule, name, parent, pos, scale, mat, null, collider);

        public static GameObject Group(string name, Transform parent, Vector3 localPos, float yRot = 0f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(0, yRot, 0);
            return go;
        }

        /// <summary>
        /// Dokuyu objenin ölçüsüne göre tekrar ettirir (gerilmiş tuğla görünmesin diye). Aynı tekrar değerini paylaşan objeler
        /// aynı malzeme kopyasını kullanır: MaterialPropertyBlock'lu çizim derlenmiş oyunda duvarları kaybettiği ve toplu çizimi
        /// (SRP Batcher) bozduğu için kullanılmaz.
        /// </summary>
        public static void SetTiling(Renderer r, Vector2 tiling)
        {
            var mat = r.sharedMaterial;
            if (mat != null) r.sharedMaterial = MaterialLibrary.Tiled(mat, tiling);
        }

        /// <summary>Üçgen prizma: beşik çatı (gable roof) için. Taban 1x1, yükseklik 1, merkez tabanın ortası.</summary>
        public static Mesh PrismMesh
        {
            get
            {
                if (_prism != null) return _prism;
                var m = new Mesh { name = "Prism" };
                Vector3 a0 = new Vector3(-0.5f, 0, -0.5f), b0 = new Vector3(0.5f, 0, -0.5f), c0 = new Vector3(0, 1, -0.5f);
                Vector3 a1 = new Vector3(-0.5f, 0, 0.5f), b1 = new Vector3(0.5f, 0, 0.5f), c1 = new Vector3(0, 1, 0.5f);
                var v = new[]
                {
                    // ön üçgen
                    a0, c0, b0,
                    // arka üçgen
                    b1, c1, a1,
                    // sol eğim
                    a1, c1, c0, a0,
                    // sağ eğim
                    b0, c0, c1, b1,
                    // taban
                    a0, b0, b1, a1
                };
                var uv = new[]
                {
                    new Vector2(0,0), new Vector2(0.5f,1), new Vector2(1,0),
                    new Vector2(0,0), new Vector2(0.5f,1), new Vector2(1,0),
                    new Vector2(0,0), new Vector2(0,1), new Vector2(1,1), new Vector2(1,0),
                    new Vector2(0,0), new Vector2(0,1), new Vector2(1,1), new Vector2(1,0),
                    new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1)
                };
                var tris = new[]
                {
                    0,1,2,
                    3,4,5,
                    6,7,8, 6,8,9,
                    10,11,12, 10,12,13,
                    14,15,16, 14,16,17
                };
                m.vertices = v;
                m.uv = uv;
                m.triangles = tris;
                m.RecalculateNormals();
                m.RecalculateBounds();
                _prism = m;
                return m;
            }
        }

        public static GameObject Prism(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat, float yRot = 0f, float texTiling = 0f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(0, yRot, 0);
            go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = PrismMesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            if (texTiling > 0f) SetTiling(r, new Vector2(scale.z * texTiling, scale.x * texTiling * 0.5f));
            return go;
        }

        static readonly System.Collections.Generic.Dictionary<int, Mesh> _hipCache = new System.Collections.Generic.Dictionary<int, Mesh>();

        /// <summary>
        /// Kırma (hip) çatı: dört tarafı eğimli çatı. ridge = mahya uzunluğunun taban genişliğine oranı (0 → tam piramit).
        /// Taban 1x1 (X genişlik, Z derinlik), yükseklik 1; merkez tabanın ortası. Mahya X ekseni boyuncadır.
        /// </summary>
        public static Mesh HipRoofMesh(float ridge)
        {
            ridge = Mathf.Clamp01(ridge);
            int key = Mathf.RoundToInt(ridge * 100);
            if (_hipCache.TryGetValue(key, out var cached) && cached != null) return cached;
            float r = ridge * 0.5f;
            Vector3 a = new Vector3(-0.5f, 0, -0.5f), b = new Vector3(0.5f, 0, -0.5f), c = new Vector3(0.5f, 0, 0.5f), d = new Vector3(-0.5f, 0, 0.5f);
            Vector3 r0 = new Vector3(-r, 1, 0), r1 = new Vector3(r, 1, 0);
            var v = new[]
            {
                a, r0, r1, b,      // ön eğim (-Z)
                c, r1, r0, d,      // arka eğim (+Z)
                d, r0, a,          // sol üçgen
                b, r1, c,          // sağ üçgen
                a, b, c, d         // taban
            };
            var uv = new[]
            {
                new Vector2(0,0), new Vector2(0.5f - r,1), new Vector2(0.5f + r,1), new Vector2(1,0),
                new Vector2(0,0), new Vector2(0.5f - r,1), new Vector2(0.5f + r,1), new Vector2(1,0),
                new Vector2(0,0), new Vector2(0.5f,1), new Vector2(1,0),
                new Vector2(0,0), new Vector2(0.5f,1), new Vector2(1,0),
                new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1)
            };
            var tris = new[] { 0,1,2, 0,2,3, 4,5,6, 4,6,7, 8,9,10, 11,12,13, 14,16,15, 14,17,16 };
            var m = new Mesh { name = "HipRoof_" + key, vertices = v, uv = uv, triangles = tris };
            m.RecalculateNormals(); m.RecalculateBounds();
            _hipCache[key] = m;
            return m;
        }

        public static GameObject HipRoof(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat, float ridge = 0.4f, float yRot = 0f, float texTiling = 0.8f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(0, yRot, 0);
            go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = HipRoofMesh(ridge);
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            if (texTiling > 0f) SetTiling(r, new Vector2(scale.x * texTiling, scale.z * texTiling * 0.7f));
            return go;
        }

        /// <summary>3D uzayda okunabilir tabela yazısı (TextMesh).</summary>
        public static TextMesh Text3D(string name, Transform parent, Vector3 pos, string text, Color color, float size = 0.1f,
            float yRot = 0f, TextAnchor anchor = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Bold)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(0, yRot, 0);
            var tm = go.AddComponent<TextMesh>();
            tm.font = DefaultFont;
            tm.text = text;
            tm.fontSize = 96;
            tm.characterSize = size;
            tm.anchor = anchor;
            tm.alignment = TextAlignment.Center;
            tm.fontStyle = style;
            tm.color = color;
            var mr = go.GetComponent<MeshRenderer>();
            if (tm.font != null)
            {
                var culled = CulledTextMaterial(tm.font);
                mr.sharedMaterial = culled != null ? culled : tm.font.material;
            }
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return tm;
        }

        static Material _culledText;
        static Font _culledFont;
        /// <summary>Yalnızca önden görünen yazı malzemesi (arkadan ters harf görünmesin). Font dokusu yenilenince güncellenir.</summary>
        static Material CulledTextMaterial(Font font)
        {
            if (_culledText != null && _culledFont == font) return _culledText;
            var shader = Resources.Load<Shader>("Shaders/TextCulled");
            if (shader == null || !shader.isSupported || font.material == null) return null;
            _culledFont = font;
            _culledText = new Material(shader) { name = "M_TabelaYazısı", mainTexture = font.material.mainTexture };
            Font.textureRebuilt += f => { if (_culledText != null && f == _culledFont && f.material != null) _culledText.mainTexture = f.material.mainTexture; };
            return _culledText;
        }

        public static Light PointLight(string name, Transform parent, Vector3 pos, Color color, float intensity, float range, bool shadows = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.intensity = intensity;
            l.range = range;
            l.shadows = shadows ? LightShadows.Soft : LightShadows.None;
            return l;
        }
    }
}
