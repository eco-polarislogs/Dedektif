using System.Collections.Generic;
using UnityEngine;

namespace Dedektif3D.World
{
    /// <summary>
    /// Ağaç taçları ve şemsiye kubbesi için paylaşılan mesh'ler. Her biri bir kez üretilir, aynı malzemeyle GPU instancing kullanır.
    /// Random kullanmaz: kasaba üretiminin tohumlu sırası değişmez.
    /// </summary>
    public static class NatureMeshes
    {
        const int Variants = 3;
        static readonly Mesh[] _pines = new Mesh[Variants];
        static readonly Mesh[] _leaves = new Mesh[Variants];
        static readonly Mesh[] _cards = new Mesh[Variants];
        static Mesh _umbrella;

        public static int VariantOf(Vector3 worldPosition)
        {
            int h = Mathf.FloorToInt(worldPosition.x * 3.7f) * 31 + Mathf.FloorToInt(worldPosition.z * 5.3f);
            return Mathf.Abs(h) % Variants;
        }

        /// <summary>Paylaşılan bir mesh'i çarpışmasız olarak yerleştirir.</summary>
        public static GameObject Place(string name, Transform parent, Vector3 localPos, Vector3 scale, Mesh mesh, Material mat, float yRot = 0f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(0f, yRot, 0f);
            go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        static float Jitter(int a, int b, int seed)
        {
            float s = Mathf.Sin(a * 12.9898f + b * 78.233f + seed * 37.719f) * 43758.547f;
            return s - Mathf.Floor(s);
        }

        /// <summary>Çam tacı: birim boyut (y 0..1, yarıçap en çok 0.5). Sarkık, testere uçlu dal katları.</summary>
        public static Mesh PineCanopy(int variant)
        {
            int v = Mathf.Abs(variant) % Variants;
            if (_pines[v] != null) return _pines[v];

            const int Tiers = 9, Points = 14;
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            for (int t = 0; t < Tiers; t++)
            {
                float f = t / (Tiers - 1f);
                float radius = Mathf.Lerp(0.5f, 0.05f, Mathf.Pow(f, 0.9f));
                float low = Mathf.Lerp(0f, 0.86f, f);
                float high = low + Mathf.Lerp(0.24f, 0.14f, f);
                float offset = Jitter(t, 3, v) * Mathf.PI * 2f;

                int apex = verts.Count;
                verts.Add(new Vector3(0f, high, 0f));
                uvs.Add(new Vector2(0.5f, 0f));

                int ring = verts.Count;
                int count = Points * 2;
                for (int i = 0; i < count; i++)
                {
                    float angle = offset + i / (float)count * Mathf.PI * 2f;
                    bool tip = i % 2 == 0;
                    float r = radius * (tip ? 0.86f + 0.14f * Jitter(t, i, v) : 0.68f);
                    float y = low + (tip ? -0.02f * Jitter(i, t, v + 5) : 0.025f);
                    verts.Add(new Vector3(Mathf.Cos(angle) * r, y, Mathf.Sin(angle) * r));
                    uvs.Add(new Vector2(i / (float)count * 3f, tip ? 1f : 0.82f));
                }

                int under = verts.Count;
                verts.Add(new Vector3(0f, low + 0.03f, 0f));
                uvs.Add(new Vector2(0.5f, 0.3f));

                for (int i = 0; i < count; i++)
                {
                    int a = ring + i, b = ring + (i + 1) % count;
                    tris.Add(apex); tris.Add(b); tris.Add(a);
                    tris.Add(under); tris.Add(a); tris.Add(b);
                }
            }

            var mesh = new Mesh { name = "Cam_" + v };
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            _pines[v] = mesh;
            return mesh;
        }

        /// <summary>Yaprak kümesi: birim çaplı, topaklı yüzeyli küre.</summary>
        public static Mesh LeafCanopy(int variant)
        {
            int v = Mathf.Abs(variant) % Variants;
            if (_leaves[v] != null) return _leaves[v];

            const int Lat = 11, Lon = 18;
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            float phase = v * 1.9f;

            for (int la = 0; la <= Lat; la++)
            {
                float theta = la / (float)Lat * Mathf.PI;
                for (int lo = 0; lo <= Lon; lo++)
                {
                    // Dikiş satırı ilk sütunla aynı konumda kalsın diye açı son sütunda sıfıra döner.
                    float phi = lo % Lon / (float)Lon * Mathf.PI * 2f;
                    var dir = new Vector3(Mathf.Sin(theta) * Mathf.Cos(phi), Mathf.Cos(theta), Mathf.Sin(theta) * Mathf.Sin(phi));
                    float bumps = Mathf.Sin(dir.x * 6.5f + phase) * Mathf.Sin(dir.y * 5.5f + phase * 1.7f) * Mathf.Sin(dir.z * 7.5f + 0.3f);
                    float fine = Mathf.Sin(dir.x * 15f + phase * 2f) * Mathf.Sin(dir.z * 13f - phase);
                    verts.Add(dir * 0.5f * (1f + 0.24f * bumps + 0.07f * fine));
                    uvs.Add(new Vector2(lo / (float)Lon * 3f, la / (float)Lat * 2f));
                }
            }
            int stride = Lon + 1;
            for (int la = 0; la < Lat; la++)
            {
                for (int lo = 0; lo < Lon; lo++)
                {
                    int a = la * stride + lo, b = a + 1, c = a + stride, d = c + 1;
                    tris.Add(a); tris.Add(b); tris.Add(c);
                    tris.Add(b); tris.Add(d); tris.Add(c);
                }
            }

            var mesh = new Mesh { name = "Yaprak_" + v };
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            _leaves[v] = mesh;
            return mesh;
        }

        /// <summary>Yaprak kartları: birim çaplı küre yüzeyine dağılmış kareler. Normaller dışa bakar, tacın ışığı yumuşak görünür.</summary>
        public static Mesh LeafCards(int variant)
        {
            int v = Mathf.Abs(variant) % Variants;
            if (_cards[v] != null) return _cards[v];

            const int Cards = 64;
            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            for (int i = 0; i < Cards; i++)
            {
                float y = 1f - 2f * (i + 0.5f) / Cards;
                float ring = Mathf.Sqrt(Mathf.Max(0f, 1f - y * y));
                float phi = i * 2.39996f + v * 1.3f;
                var dir = new Vector3(Mathf.Cos(phi) * ring, y, Mathf.Sin(phi) * ring);

                var center = dir * (0.36f + 0.12f * Jitter(i, 1, v));
                var tilt = new Vector3(Jitter(i, 2, v) - 0.5f, Jitter(i, 3, v) - 0.5f, Jitter(i, 4, v) - 0.5f) * 0.8f;
                var facing = (dir + tilt).normalized;
                var helper = Mathf.Abs(facing.y) > 0.9f ? Vector3.right : Vector3.up;
                var right = Vector3.Cross(helper, facing).normalized;
                var up = Vector3.Cross(facing, right);
                float size = 0.21f + 0.07f * Jitter(i, 5, v);

                int start = verts.Count;
                verts.Add(center + (-right - up) * size);
                verts.Add(center + (right - up) * size);
                verts.Add(center + (right + up) * size);
                verts.Add(center + (-right + up) * size);
                uvs.Add(new Vector2(0f, 0f));
                uvs.Add(new Vector2(1f, 0f));
                uvs.Add(new Vector2(1f, 1f));
                uvs.Add(new Vector2(0f, 1f));
                for (int k = 0; k < 4; k++) normals.Add(dir);

                tris.Add(start); tris.Add(start + 2); tris.Add(start + 1);
                tris.Add(start); tris.Add(start + 3); tris.Add(start + 2);
            }

            var mesh = new Mesh { name = "YaprakKartlari_" + v };
            mesh.SetVertices(verts);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            _cards[v] = mesh;
            return mesh;
        }

        /// <summary>Açık şemsiye: sekiz telli, kenarı kavisli, içi boş çift yüzlü kubbe. Tepe y=0.30, kenar y=0.</summary>
        public static Mesh UmbrellaCanopy
        {
            get
            {
                if (_umbrella != null) return _umbrella;

                const int Ribs = 8, Sides = Ribs * 2;
                float[] radius = { 0.2f, 0.38f, 0.52f, 0.585f };
                float[] height = { 0.285f, 0.2f, 0.07f, 0f };

                var verts = new List<Vector3>();
                var tris = new List<int>();

                void Surface(bool underside)
                {
                    int apex = verts.Count;
                    verts.Add(new Vector3(0f, 0.3f, 0f));
                    int first = verts.Count;
                    for (int ring = 0; ring < radius.Length; ring++)
                    {
                        for (int i = 0; i < Sides; i++)
                        {
                            bool rib = i % 2 == 0;
                            float angle = i / (float)Sides * Mathf.PI * 2f;
                            bool rim = ring == radius.Length - 1;
                            float r = radius[ring] * (rim && !rib ? 0.91f : 1f);
                            float y = height[ring] + (rib ? 0f : rim ? 0.022f : -0.012f);
                            verts.Add(new Vector3(Mathf.Cos(angle) * r, y, Mathf.Sin(angle) * r));
                        }
                    }
                    void Tri(int a, int b, int c)
                    {
                        tris.Add(a);
                        if (underside) { tris.Add(c); tris.Add(b); }
                        else { tris.Add(b); tris.Add(c); }
                    }
                    for (int i = 0; i < Sides; i++)
                        Tri(apex, first + (i + 1) % Sides, first + i);
                    for (int ring = 0; ring < radius.Length - 1; ring++)
                    {
                        int start = first + ring * Sides, next = start + Sides;
                        for (int i = 0; i < Sides; i++)
                        {
                            int j = (i + 1) % Sides;
                            Tri(start + i, start + j, next + i);
                            Tri(start + j, next + j, next + i);
                        }
                    }
                }

                Surface(false);
                Surface(true);

                var mesh = new Mesh { name = "SemsiyeKubbesi" };
                mesh.SetVertices(verts);
                mesh.SetTriangles(tris, 0);
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                _umbrella = mesh;
                return mesh;
            }
        }
    }
}
