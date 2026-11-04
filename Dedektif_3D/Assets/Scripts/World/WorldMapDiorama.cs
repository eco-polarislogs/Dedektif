using System;
using System.Collections.Generic;
using Dedektif3D.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static Dedektif3D.World.Shapes;
using M = Dedektif3D.World.MaterialLibrary;

namespace Dedektif3D.World
{
    /// <summary>
    /// 3D BÖLGE HARİTASI — düz resim yerine dedektifin masasında açık duran, YAŞAYAN bir minyatür diorama.
    ///
    ///  • Kabartma arazi (dağlar, nehir, çam ormanları, tarlalar, toprak yollar) koddan üretilir; doku da koddan boyanır.
    ///  • Her kasaba kendi havasıyla yaşar: Gizemli Kasaba MAVİ ışık + hafif yağmur, Gölgeşehir SARI fenerler + sağanak ve şimşek,
    ///    Sisören YEŞİL sis içinde dağ yamacında. Kilitli bölgeler (Yıkıkköy, Karaorman) duman, karga ve karanlıkla mühürlü.
    ///  • Yollarda fenerli at arabaları gidip gelir, bacalardan duman tüter, bulut gölgeleri haritanın üstünden geçer.
    ///  • Her vakada kırmızı başlı bir harita iğnesi; çözülen davalar kırmızı iple bir sonrakine bağlanır (dedektif panosu).
    ///  • Masada gaz lambası, fötr şapka, cep saati, büyüteç, telgraf ve vaka dosyaları: hikâyenin geçtiği oda.
    ///
    /// Kamera bu haritayı bir film gibi çeker: açılışta yüksekten süzülür, fareyle hafif paralaks yapar, seçilen kasabaya
    /// yaklaşır, "Soruşturmaya Başla" ile kasabanın içine dalar. Minyatür (tilt-shift) etkisi için diorama gerçek boyutta
    /// küçüktür (1 harita birimi = 6 cm) ve sığ alan derinliği uygulanır.
    /// </summary>
    public class WorldMapDiorama : MonoBehaviour
    {
        public static WorldMapDiorama Instance { get; private set; }

        public static readonly Vector3 Origin = new Vector3(2600f, 40f, 2600f);
        public const float S = 0.06f;                  // 1 harita birimi = 6 cm (minyatür)
        public const float W = 64f, D = 44f;           // harita tahtası (harita birimi)
        const int NX = 161, NZ = 111;                  // 0.4 birim aralıklı yükseklik ızgarası
        const float DeskY = -1.8f;

        static readonly Vector3 BaseLook = new Vector3(0f, 1.2f, 1.2f);
        static readonly Vector3 BaseOffset = new Vector3(0f, 39f, -47f);
        static readonly Vector3 IntroOffset = new Vector3(-30f, 84f, -112f);

        public class Site
        {
            public RegionDef Region;
            public TownData Data;            // kilitli bölgelerde null
            public Vector3 Local;            // harita uzayında kasaba merkezi (zemin)
            public Vector3 PinTop;           // harita uzayında iğne başı
            public Color Theme;
            public Light Glow; public float GlowBase;
            public string Teaser, Weather, Number;
            public float Radius;
            public bool Playable => Region.Level > 0;
        }

        public readonly List<Site> Sites = new List<Site>();
        public bool Active { get; private set; }
        public Site Hover { get; set; }
        public Site Selected { get; private set; }
        public float SelectedAt { get; private set; }
        public bool Diving => _dive != null;
        public float ShownFor => Active ? Time.time - _shownAt : 0f;
        /// <summary>Alan derinliği odağı (dünya uzayı).</summary>
        public Vector3 FocusPoint { get; private set; }
        public float FocusDistance { get; private set; } = 3f;

        Camera _cam;
        CameraClearFlags _prevClear; Color _prevBg; float _prevFov, _prevNear; bool _prevFog; float _prevShadowDist = -1f;
        Color _prevAmbSky, _prevAmbEq, _prevAmbGround;
        float _shownAt;
        Site _dive; float _diveT; Action _onDive;
        Vector3 _lookCur, _offCur;
        Light _lamp;
        readonly List<GameObject> _strings = new List<GameObject>();
        readonly List<(Site site, Renderer head)> _pins = new List<(Site, Renderer)>();

        readonly float[,] _h = new float[NX, NZ];
        readonly System.Random _rng = new System.Random(1887);
        float R() => (float)_rng.NextDouble();
        float R(float a, float b) => a + (b - a) * (float)_rng.NextDouble();

        struct Flat { public Vector2 Xz; public float R, H; }
        readonly List<Flat> _flats = new List<Flat>();
        readonly List<Vector2[]> _roads = new List<Vector2[]>();

        // Nehir: kuzey dağlarından doğar, Gizemli Kasaba'nın doğusundan geçip güneyden çıkar
        static readonly Vector2[] River =
        {
            new Vector2(10f, 23f), new Vector2(7.5f, 17f), new Vector2(10.5f, 10f), new Vector2(7f, 4f),
            new Vector2(5.2f, -2.5f), new Vector2(2.5f, -8.5f), new Vector2(-3.5f, -14f), new Vector2(-1.5f, -23f),
        };

        // =================================================================
        //  KURULUM
        // =================================================================
        public static WorldMapDiorama Create()
        {
            var go = new GameObject("BölgeHaritası (3D Diorama)");
            go.transform.position = Origin;
            go.transform.localScale = Vector3.one * S;
            var d = go.AddComponent<WorldMapDiorama>();
            Instance = d;
            float t0 = Time.realtimeSinceStartup;
            try { d.Build(); }
            catch (Exception e) { Debug.LogException(e); }
            go.SetActive(false);
            Debug.Log($"[Dedektif3D] 3D bölge haritası {(Time.realtimeSinceStartup - t0) * 1000f:0} ms'de kuruldu ({go.GetComponentsInChildren<Transform>(true).Length} obje).");
            return d;
        }

        void Build()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var times = new System.Text.StringBuilder();
            void Mark(string n) { times.Append(n).Append('=').Append(sw.ElapsedMilliseconds).Append("ms "); sw.Restart(); }
            InitSites();
            for (int i = 0; i < NX; i++)
                for (int k = 0; k < NZ; k++)
                    _h[i, k] = Height(GX(i), GZ(k));
            foreach (var s in Sites) { s.Local.y = SampleH(s.Local.x, s.Local.z); }
            BuildRoads();
            Mark("yükseklik");
            BuildTerrain(); Mark("arazi");
            BuildBoardAndDesk();
            BuildWater(); Mark("su");
            BuildForests(); Mark("orman");
            BuildTowns(); Mark("kasabalar");
            BuildPinsAndStrings();
            BuildWeather();
            BuildLife();
            BuildDeskProps(); Mark("diğer");
            Debug.Log("[Dedektif3D] Harita adımları: " + times);
        }

        // =================================================================
        //  KASABALAR (bölge haritasındaki konumlarından)
        // =================================================================
        void InitSites()
        {
            foreach (var r in GameData.Regions)
            {
                var c = r.MapRect.center;
                var s = new Site
                {
                    Region = r,
                    Data = r.Level > 0 ? GameData.Town(r.Id) : null,
                    Local = new Vector3((c.x / 100f - 0.5f) * W, 0f, (0.5f - c.y / 100f) * D),
                };
                switch (r.Id)
                {
                    case "gizemli":
                        s.Radius = 3.2f; s.Number = "#104";
                        s.Teaser = "Kasaba meydanında herkesin tanıdığı tüccar Osman Bey ölü bulundu. Kasap, eczacı, muhtar, komiser ve terzi... Her birinin onunla görülecek bir hesabı vardı.";
                        s.Weather = "Hafif yağmur • kasvetli mavi gece • ilk soruşturma";
                        break;
                    case "golge_sehir":
                        s.Radius = 2.8f; s.Number = "#201";
                        s.Teaser = "Çam ormanının girişinde zengin ve karanlık tüccar Ekrem Bey katledildi. Sekiz dükkân, sekiz şüpheli ve fenerini hiç söndürmeyen bir gece bekçisi.";
                        s.Weather = "Sağanak ve şimşek • soğuk, karanlık sokaklar • sarı gaz lambaları";
                        break;
                    case "sisoren":
                        s.Radius = 2.8f; s.Number = "#301";
                        s.Teaser = "Dış dünyaya kapalı dağ kasabasında vahşi bir cinayet. Telgrafhane, kahvehane, sinema, sahaf... Sis her şeyi saklıyor, kasabalılar daha fazlasını.";
                        s.Weather = "Yoğun dağ sisi • aydınlık ama ürkütücü • rüzgâr ve uzak uluma";
                        break;
                    case "yikikkoy":
                        s.Radius = 2.4f; s.Number = "#???";
                        s.Teaser = "Yanmış evler, terk edilmiş sokaklar ve hâlâ tüten küller. Yol barikatla kapatılmış; bu dosya henüz mühürlü.";
                        s.Weather = "Kül ve duman • kimse yaşamıyor";
                        break;
                    default:
                        s.Radius = 1.6f; s.Number = "#???";
                        s.Teaser = "Kimsenin gece girmeye cesaret edemediği orman. Söylentiler çok, delil yok. Dosya mühürlü.";
                        s.Weather = "Karanlık orman • kargalar";
                        break;
                }
                s.Theme = s.Data != null ? s.Data.ThemeColor : r.Id == "yikikkoy" ? new Color(0.85f, 0.42f, 0.28f) : new Color(0.55f, 0.5f, 0.7f);
                Sites.Add(s);
            }
            foreach (var s in Sites)
            {
                float flatR = s.Region.Id == "sisoren" ? 1.3f : s.Region.Id == "karaorman" ? 1.1f : s.Radius * 0.85f;
                _flats.Add(new Flat { Xz = new Vector2(s.Local.x, s.Local.z), R = flatR, H = HeightRaw(s.Local.x, s.Local.z) });
            }
        }

        Site SiteById(string id) => Sites.Find(s => s.Region.Id == id);

        void BuildRoads()
        {
            Vector2 P(string id) { var s = SiteById(id); return s != null ? new Vector2(s.Local.x, s.Local.z) : Vector2.zero; }
            var g = P("gizemli");
            _roads.Add(new[] { g, new Vector2(-6f, 0.8f), new Vector2(-11.5f, 4.6f), new Vector2(-16f, 8.2f), P("golge_sehir") });
            _roads.Add(new[] { g, new Vector2(-2.6f, 3.2f), new Vector2(-0.8f, 8.2f), new Vector2(0.6f, 11.6f), P("sisoren") });
            _roads.Add(new[] { g, new Vector2(4.2f, -0.6f), new Vector2(10f, 3.8f), new Vector2(15.5f, 7.6f), P("yikikkoy") });
            _roads.Add(new[] { g, new Vector2(4.8f, -6.8f), new Vector2(11f, -10.2f), new Vector2(16f, -12.2f), P("karaorman") });
            _roads.Add(new[] { g, new Vector2(-3.2f, -9f), new Vector2(-5.5f, -15f), new Vector2(-6.5f, -21.8f) });   // İstanbul yolu
        }

        // =================================================================
        //  ARAZİ
        // =================================================================
        static float GX(int i) => -W / 2f + i * (W / (NX - 1));
        static float GZ(int k) => -D / 2f + k * (D / (NZ - 1));
        static float Smooth(float a, float b, float x) { float t = Mathf.Clamp01((x - a) / (b - a)); return t * t * (3f - 2f * t); }
        static float Bump(float x, float z, float cx, float cz, float r) { float d2 = ((x - cx) * (x - cx) + (z - cz) * (z - cz)) / (r * r); return Mathf.Exp(-d2 * 2f); }
        static float Fbm(float x, float z)
        {
            float s = 0f, a = 0.5f, f = 1f;
            for (int i = 0; i < 4; i++) { s += (Mathf.PerlinNoise(x * f + 17.3f, z * f + 5.1f) - 0.5f) * a; f *= 2.03f; a *= 0.5f; }
            return s;
        }

        static float SegDist(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a; float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(1e-5f, ab.sqrMagnitude));
            return Vector2.Distance(p, a + ab * t);
        }
        static float PolyDist(Vector2 p, Vector2[] pts)
        {
            float d = float.MaxValue;
            for (int i = 0; i < pts.Length - 1; i++) d = Mathf.Min(d, SegDist(p, pts[i], pts[i + 1]));
            return d;
        }
        static float RiverDist(float x, float z) => PolyDist(new Vector2(x, z), River);

        /// <summary>Sınırlı mesafe: maxD'den uzak parçalar kutu testiyle atlanır (doku boyama hızlı olsun).</summary>
        static float PolyDistMax(Vector2 p, Vector2[] pts, float maxD)
        {
            float d = maxD;
            for (int i = 0; i < pts.Length - 1; i++)
            {
                var a = pts[i]; var b = pts[i + 1];
                if (p.x < Mathf.Min(a.x, b.x) - d || p.x > Mathf.Max(a.x, b.x) + d || p.y < Mathf.Min(a.y, b.y) - d || p.y > Mathf.Max(a.y, b.y) + d) continue;
                d = Mathf.Min(d, SegDist(p, a, b));
            }
            return d;
        }
        float RoadDistMax(float x, float z, float maxD)
        {
            var p = new Vector2(x, z); float d = maxD;
            foreach (var r in _roads) d = Mathf.Min(d, PolyDistMax(p, r, d));
            return d;
        }
        float RoadDist(float x, float z)
        {
            var p = new Vector2(x, z); float d = float.MaxValue;
            foreach (var r in _roads) d = Mathf.Min(d, PolyDist(p, r));
            return d;
        }

        static float HeightRaw(float x, float z)
        {
            float h = 0.45f + Fbm(x * 0.07f, z * 0.07f) * 1.5f;
            float north = Smooth(5f, 21f, z);
            float ridge = 1f - Mathf.Abs(Mathf.PerlinNoise(x * 0.075f + 3.1f, z * 0.05f + 1.7f) - 0.5f) * 2f;
            h += north * (2.4f + ridge * ridge * 7.8f);
            h += Bump(x, z, -21f, 12f, 11f) * 2.0f;      // Gölgeşehir'in çamlı tepeleri
            h += Bump(x, z, 22f, 10f, 8f) * 1.4f;        // Yıkıkköy yaylası
            h += Bump(x, z, 15f, 19f, 9f) * 3.2f;        // kuzeydoğu doruğu
            h += Bump(x, z, -11f, -18f, 9f) * 1.3f;      // güney tepeleri
            h += Bump(x, z, 24f, -19f, 7f) * 0.9f;       // Karaorman sırtı
            float rd = RiverDist(x, z);
            h -= 1.25f * Mathf.Exp(-(rd * rd) / 1.8f);
            return h;
        }

        float Height(float x, float z)
        {
            float h = HeightRaw(x, z);
            var p = new Vector2(x, z);
            foreach (var f in _flats)
            {
                float d = Vector2.Distance(p, f.Xz);
                h = Mathf.Lerp(h, f.H, 1f - Smooth(f.R, f.R + 2.4f, d));
            }
            return h;
        }

        /// <summary>Izgaradan çift doğrusal yükseklik (mesh ile birebir aynı yüzey).</summary>
        float SampleH(float x, float z)
        {
            float fx = Mathf.Clamp((x + W / 2f) / W * (NX - 1), 0f, NX - 1.001f);
            float fz = Mathf.Clamp((z + D / 2f) / D * (NZ - 1), 0f, NZ - 1.001f);
            int i = (int)fx, k = (int)fz; float tx = fx - i, tz = fz - k;
            float a = Mathf.Lerp(_h[i, k], _h[i + 1, k], tx), b = Mathf.Lerp(_h[i, k + 1], _h[i + 1, k + 1], tx);
            return Mathf.Lerp(a, b, tz);
        }
        Vector3 OnGround(float x, float z, float lift = 0f) => new Vector3(x, SampleH(x, z) + lift, z);

        float ForestMask(float x, float z, float h)
        {
            float m = 0.05f;
            m = Mathf.Max(m, Bump(x, z, -21f, 11f, 10.5f) * 1.05f);                       // Gölgeşehir çam ormanı
            m = Mathf.Max(m, Bump(x, z, 19.5f, -14f, 9.5f) * 1.35f);                      // Karaorman
            m = Mathf.Max(m, Smooth(3.5f, 7.5f, z) * (1f - Smooth(14.5f, 18.5f, z)) * 0.5f); // dağ etekleri
            m = Mathf.Max(m, Bump(x, z, -12f, -17f, 6f) * 0.55f);                          // güney koruluğu
            if (h > 7.0f) m *= Mathf.Clamp01(1f - (h - 7f));                               // ağaç sınırı
            return m;
        }

        Vector2 _gizemliXz = new Vector2(float.NaN, 0f);
        float FieldZone(float x, float z, float h)
        {
            if (float.IsNaN(_gizemliXz.x)) { var g = SiteById("gizemli"); _gizemliXz = g != null ? new Vector2(g.Local.x, g.Local.z) : new Vector2(1e6f, 1e6f); }
            float d = Vector2.Distance(new Vector2(x, z), _gizemliXz);
            float k = Smooth(3.8f, 5f, d) * (1f - Smooth(10f, 14f, d));
            k = Mathf.Max(k, Bump(x, z, -14f, -8f, 6f) * 0.9f);             // batı ovası
            if (h > 2.3f) k *= Mathf.Clamp01(1f - (h - 2.3f) * 2f);
            return k;
        }

        void BuildTerrain()
        {
            // ---- Mesh ----
            var verts = new Vector3[NX * NZ];
            var uvs = new Vector2[NX * NZ];
            for (int k = 0; k < NZ; k++)
                for (int i = 0; i < NX; i++)
                {
                    float x = GX(i), z = GZ(k);
                    verts[k * NX + i] = new Vector3(x, _h[i, k], z);
                    uvs[k * NX + i] = new Vector2((x + W / 2f) / W, (z + D / 2f) / D);
                }
            var tris = new int[(NX - 1) * (NZ - 1) * 6];
            int t = 0;
            for (int k = 0; k < NZ - 1; k++)
                for (int i = 0; i < NX - 1; i++)
                {
                    int a = k * NX + i, b = a + 1, c = a + NX, d = c + 1;
                    tris[t++] = a; tris[t++] = c; tris[t++] = b;
                    tris[t++] = b; tris[t++] = c; tris[t++] = d;
                }
            var mesh = new Mesh { name = "HaritaArazisi", indexFormat = IndexFormat.UInt32 };
            mesh.vertices = verts; mesh.uv = uvs; mesh.triangles = tris;
            mesh.RecalculateNormals(); mesh.RecalculateBounds();

            var go = new GameObject("Arazi");
            go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = M.Get("map_terrain", Color.white, 0.08f, 0f, PaintTerrain());

            // ---- Kesit (diorama kenarı: toprak katmanları) ----
            var sv = new List<Vector3>(); var st = new List<int>(); var su = new List<Vector2>();
            void Side(Func<int, Vector3> top, int count)
            {
                int start = sv.Count;
                for (int n = 0; n < count; n++)
                {
                    var p = top(n);
                    sv.Add(p); sv.Add(new Vector3(p.x, DeskY + 0.4f, p.z));
                    su.Add(new Vector2(n / (float)count * 8f, p.y * 0.25f)); su.Add(new Vector2(n / (float)count * 8f, 0f));
                }
                for (int n = 0; n < count - 1; n++)
                {
                    int a = start + n * 2, b = a + 1, c = a + 2, d = a + 3;
                    st.Add(a); st.Add(c); st.Add(b); st.Add(b); st.Add(c); st.Add(d);
                }
            }
            Side(n => verts[n], NX);                                  // güney (-Z): kameraya bakar
            Side(n => verts[(NZ - 1) * NX + (NX - 1 - n)], NX);      // kuzey
            Side(n => verts[(NZ - 1 - n) * NX], NZ);                 // batı
            Side(n => verts[n * NX + NX - 1], NZ);                   // doğu
            var sm = new Mesh { name = "HaritaKesiti", indexFormat = IndexFormat.UInt32 };
            sm.SetVertices(sv); sm.SetUVs(0, su); sm.SetTriangles(st, 0);
            sm.RecalculateNormals(); sm.RecalculateBounds();
            var side = new GameObject("Kesit");
            side.transform.SetParent(transform, false);
            side.AddComponent<MeshFilter>().sharedMesh = sm;
            side.AddComponent<MeshRenderer>().sharedMaterial = M.Get("map_soil", new Color(0.3f, 0.22f, 0.15f), 0.05f, 0f, SoilTexture());
        }

        /// <summary>Arazinin rengi: çimen, çam ormanı tabanı, kaya, kar, tarlalar, yollar, nehir kıyısı, kasaba zeminleri.</summary>
        Texture2D PaintTerrain()
        {
            const int TW = 640, TH = 440;
            var tex = new Texture2D(TW, TH, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Trilinear, anisoLevel = 4, name = "HaritaDokusu" };
            var px = new Color32[TW * TH];
            var grassA = new Color(0.22f, 0.26f, 0.14f); var grassB = new Color(0.32f, 0.33f, 0.18f);
            var forestFloor = new Color(0.1f, 0.14f, 0.09f);
            var rock = new Color(0.4f, 0.37f, 0.33f); var rockDark = new Color(0.25f, 0.23f, 0.21f);
            var snow = new Color(0.86f, 0.88f, 0.92f);
            var dirt = new Color(0.5f, 0.41f, 0.28f);
            var water = new Color(0.06f, 0.09f, 0.12f);
            var townGround = new Color(0.42f, 0.39f, 0.34f);
            var burnt = new Color(0.12f, 0.1f, 0.09f);
            Color[] fields = { new Color(0.56f, 0.48f, 0.25f), new Color(0.31f, 0.23f, 0.15f), new Color(0.27f, 0.33f, 0.15f), new Color(0.43f, 0.38f, 0.22f), new Color(0.5f, 0.42f, 0.2f) };
            float cos = Mathf.Cos(0.35f), sin = Mathf.Sin(0.35f);
            for (int v = 0; v < TH; v++)
                for (int u = 0; u < TW; u++)
                {
                    float x = (u + 0.5f) / TW * W - W / 2f, z = (v + 0.5f) / TH * D - D / 2f;
                    float h = SampleH(x, z);
                    float gx = SampleH(x + 0.4f, z) - SampleH(x - 0.4f, z), gz = SampleH(x, z + 0.4f) - SampleH(x, z - 0.4f);
                    float slope = Mathf.Sqrt(gx * gx + gz * gz) / 0.8f;
                    float n1 = Mathf.PerlinNoise(x * 0.35f + 11f, z * 0.35f + 7f), n2 = Mathf.PerlinNoise(x * 1.4f + 3f, z * 1.4f + 9f);
                    var c = Color.Lerp(grassA, grassB, n1);
                    c = Color.Lerp(c, c * 0.82f, n2 * 0.55f);

                    // Tarlalar (döndürülmüş parsel ızgarası)
                    float fz = FieldZone(x, z, h);
                    if (fz > 0.01f)
                    {
                        float xr = x * cos - z * sin, zr = x * sin + z * cos;
                        int cx = Mathf.FloorToInt(xr / 2.3f + Mathf.PerlinNoise(zr * 0.3f, 1f) * 0.6f), cz = Mathf.FloorToInt(zr / 1.6f);
                        int hash = (cx * 73856093) ^ (cz * 19349663);
                        var fc = fields[Mathf.Abs(hash) % fields.Length];
                        float edge = Mathf.Min(Mathf.Abs(Mathf.Repeat(xr / 2.3f, 1f) - 0.5f), Mathf.Abs(Mathf.Repeat(zr / 1.6f, 1f) - 0.5f));
                        fc *= 0.92f + Mathf.Sin(xr * 26f) * 0.06f;                          // sürülmüş toprak izleri
                        if (edge > 0.46f) fc = Color.Lerp(fc, new Color(0.2f, 0.24f, 0.12f), 0.7f);   // çit / sınır
                        c = Color.Lerp(c, fc, fz * 0.9f);
                    }
                    // Orman tabanı
                    c = Color.Lerp(c, forestFloor, Mathf.Clamp01(ForestMask(x, z, h) * 1.25f));
                    // Kaya + kar
                    float rockK = Mathf.Clamp01((slope - 0.5f) * 2.2f) + Mathf.Clamp01((h - 5f) * 0.55f);
                    c = Color.Lerp(c, Color.Lerp(rock, rockDark, n2), Mathf.Clamp01(rockK));
                    float snowK = Mathf.Clamp01((h - 7.9f - n1 * 0.9f) * 1.6f) * (1f - Mathf.Clamp01(slope - 1.5f));
                    c = Color.Lerp(c, snow, snowK);

                    var p = new Vector2(x, z);
                    for (int si = 0; si < Sites.Count; si++)
                    {
                        var s = Sites[si];
                        float dx = x - s.Local.x, dz = z - s.Local.z;
                        if (dx * dx + dz * dz > 81f) continue;     // 9 birimden uzak: etkisi yok
                        float d = Mathf.Sqrt(dx * dx + dz * dz);
                        if (s.Region.Id == "yikikkoy") c = Color.Lerp(c, burnt, (1f - Smooth(1.5f, 4.5f, d)) * 0.85f);
                        else c = Color.Lerp(c, townGround, (1f - Smooth(s.Radius * 0.55f, s.Radius + 0.4f, d)) * 0.75f);
                        if (s.Playable) c = Color.Lerp(c, s.Theme * 0.55f, (1f - Smooth(1.5f, 8f, d)) * 0.12f);   // kasaba teması (mavi / sarı / yeşil)
                    }
                    // Yollar
                    float rd = RoadDistMax(x, z, 0.5f);
                    if (rd < 0.45f) c = Color.Lerp(dirt * (0.9f + n2 * 0.2f), c, Smooth(0.16f, 0.45f, rd));
                    // Nehir ve ıslak kıyı
                    float rv = PolyDistMax(new Vector2(x, z), River, 1.5f);
                    c = Color.Lerp(c, c * 0.55f, 1f - Smooth(0.45f, 1.4f, rv));
                    if (rv < 0.5f) c = Color.Lerp(water, c, Smooth(0.3f, 0.5f, rv));
                    px[v * TW + u] = c;
                }
            tex.SetPixels32(px);
            tex.Apply(true, false);
            return tex;
        }

        static Texture2D SoilTexture()
        {
            const int s = 64;
            var t = new Texture2D(s, s, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Repeat, name = "Toprak" };
            var px = new Color[s * s];
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    float band = Mathf.PerlinNoise(x * 0.05f, y * 0.35f);
                    var c = Color.Lerp(new Color(0.42f, 0.31f, 0.2f), new Color(0.22f, 0.16f, 0.11f), band);
                    if (y > s - 6) c = Color.Lerp(c, new Color(0.2f, 0.26f, 0.12f), 0.8f);   // üstte çim örtüsü
                    c *= 0.85f + Mathf.PerlinNoise(x * 0.6f, y * 0.6f) * 0.3f;
                    px[y * s + x] = c;
                }
            t.SetPixels(px); t.Apply(true);
            return t;
        }

        void BuildBoardAndDesk()
        {
            var wood = M.Wood(new Color(0.3f, 0.18f, 0.1f));
            var darkWood = M.Wood(new Color(0.16f, 0.09f, 0.05f));
            float fy = DeskY + 1.15f;
            // Çerçeve (cilalı ceviz)
            Box("ÇerçeveG", transform, new Vector3(0, fy, -D / 2f - 0.7f), new Vector3(W + 2.8f, 2.3f, 1.4f), wood, null, false, 0.15f);
            Box("ÇerçeveK", transform, new Vector3(0, fy, D / 2f + 0.7f), new Vector3(W + 2.8f, 2.3f, 1.4f), wood, null, false, 0.15f);
            Box("ÇerçeveB", transform, new Vector3(-W / 2f - 0.7f, fy, 0), new Vector3(1.4f, 2.3f, D), wood, null, false, 0.15f);
            Box("ÇerçeveD", transform, new Vector3(W / 2f + 0.7f, fy, 0), new Vector3(1.4f, 2.3f, D), wood, null, false, 0.15f);
            // Pirinç köşe süsleri
            var brass = M.Get("map_brass", new Color(0.72f, 0.54f, 0.26f), 0.75f, 0.85f);
            foreach (var sx in new[] { -1f, 1f })
                foreach (var sz in new[] { -1f, 1f })
                    Box("PirinçKöşe", transform, new Vector3(sx * (W / 2f + 0.7f), fy + 1.2f, sz * (D / 2f + 0.7f)), new Vector3(1.6f, 0.15f, 1.6f), brass, null, false);
            // Pirinç künye: "BÖLGE HARİTASI — 1887"
            Box("Künye", transform, new Vector3(0, fy + 0.2f, -D / 2f - 1.42f), new Vector3(9f, 1.1f, 0.08f), brass, null, false);
            var plate = Text3D("KünyeYazı", transform, new Vector3(0, fy + 0.2f, -D / 2f - 1.5f), "ANADOLU VİLAYETİ — 1887", new Color(0.18f, 0.1f, 0.04f), 0.07f);
            plate.transform.localScale = Vector3.one;
            // Masa
            Box("Masa", transform, new Vector3(0, DeskY - 2f, 0), new Vector3(160f, 4f, 112f), darkWood, null, false, 0.05f);
            // Masa örtüsü / yeşil çuha (haritanın altında)
            Box("Çuha", transform, new Vector3(0, DeskY + 0.02f, 0), new Vector3(W + 14f, 0.06f, D + 12f), M.Get("map_baize", new Color(0.09f, 0.15f, 0.11f), 0.05f), null, false);
        }

        void BuildWater()
        {
            var verts = new List<Vector3>(); var tris = new List<int>(); var uv = new List<Vector2>();
            // Nehri 0.35 birim adımlarla örnekle
            var pts = new List<Vector2>();
            for (int i = 0; i < River.Length - 1; i++)
            {
                float len = Vector2.Distance(River[i], River[i + 1]);
                int n = Mathf.Max(2, Mathf.CeilToInt(len / 0.35f));
                for (int k = 0; k < n; k++) pts.Add(CatmullRom(River, (i + k / (float)n) / (River.Length - 1)));
            }
            pts.Add(River[River.Length - 1]);
            float acc = 0f;
            for (int i = 0; i < pts.Count; i++)
            {
                var dir = (i < pts.Count - 1 ? pts[i + 1] - pts[i] : pts[i] - pts[i - 1]).normalized;
                var nrm = new Vector2(-dir.y, dir.x);
                float wdt = Mathf.Lerp(0.28f, 0.62f, i / (float)pts.Count);
                float y = SampleH(pts[i].x, pts[i].y) + 0.12f;
                var a = pts[i] + nrm * wdt; var b = pts[i] - nrm * wdt;
                verts.Add(new Vector3(a.x, y, a.y)); verts.Add(new Vector3(b.x, y, b.y));
                if (i > 0) acc += Vector2.Distance(pts[i], pts[i - 1]);
                uv.Add(new Vector2(0, acc * 0.5f)); uv.Add(new Vector2(1, acc * 0.5f));
                if (i > 0)
                {
                    int q = verts.Count - 4;
                    tris.Add(q); tris.Add(q + 2); tris.Add(q + 1); tris.Add(q + 1); tris.Add(q + 2); tris.Add(q + 3);
                }
            }
            var m = new Mesh { name = "Nehir" };
            m.SetVertices(verts); m.SetUVs(0, uv); m.SetTriangles(tris, 0);
            m.RecalculateNormals(); m.RecalculateBounds();
            var go = new GameObject("Nehir");
            go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = m;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = M.Get("map_water", new Color(0.12f, 0.18f, 0.22f), 0.96f, 0.15f);
            mr.shadowCastingMode = ShadowCastingMode.Off;

            // Köprüler: yolun nehri kestiği yerlerde ahşap köprü
            var plank = M.Wood(new Color(0.36f, 0.25f, 0.15f));
            foreach (var road in _roads)
            {
                float best = float.MaxValue; Vector2 bp = Vector2.zero, bd = Vector2.right;
                for (int i = 0; i < road.Length - 1; i++)
                    for (int k = 0; k <= 20; k++)
                    {
                        var p = Vector2.Lerp(road[i], road[i + 1], k / 20f);
                        float d = RiverDist(p.x, p.y);
                        if (d < best) { best = d; bp = p; bd = (road[i + 1] - road[i]).normalized; }
                    }
                if (best > 0.4f) continue;
                float yaw = Mathf.Atan2(bd.x, bd.y) * Mathf.Rad2Deg;
                var bg = Group("Köprü", transform, new Vector3(bp.x, SampleH(bp.x, bp.y) + 0.45f, bp.y), yaw).transform;
                Box("Döşeme", bg, Vector3.zero, new Vector3(0.6f, 0.08f, 2.4f), plank, null, false);
                foreach (var sx in new[] { -0.3f, 0.3f })
                    Box("Korkuluk", bg, new Vector3(sx, 0.16f, 0), new Vector3(0.04f, 0.04f, 2.3f), plank, null, false);
            }
        }

        static Vector2 CatmullRom(Vector2[] pts, float u)
        {
            float f = Mathf.Clamp01(u) * (pts.Length - 1);
            int i = Mathf.Min(Mathf.FloorToInt(f), pts.Length - 2);
            float t = f - i;
            var p0 = pts[Mathf.Max(i - 1, 0)]; var p1 = pts[i]; var p2 = pts[i + 1]; var p3 = pts[Mathf.Min(i + 2, pts.Length - 1)];
            float t2 = t * t, t3 = t2 * t;
            return 0.5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
        }

        // =================================================================
        //  ORMANLAR + KAYALAR (tek mesh'te birleştirilir)
        // =================================================================
        static Mesh _pine, _round;
        static Mesh PineMesh()
        {
            if (_pine != null) return _pine;
            var v = new List<Vector3>(); var t = new List<int>();
            void Cone(float r, float h, float y0, int sides)
            {
                int apex = v.Count; v.Add(new Vector3(0, y0 + h, 0));
                int ring = v.Count;
                for (int i = 0; i < sides; i++) { float a = i / (float)sides * Mathf.PI * 2f; v.Add(new Vector3(Mathf.Cos(a) * r, y0, Mathf.Sin(a) * r)); }
                int center = v.Count; v.Add(new Vector3(0, y0, 0));
                for (int i = 0; i < sides; i++)
                {
                    int a = ring + i, b = ring + (i + 1) % sides;
                    t.Add(apex); t.Add(b); t.Add(a);
                    t.Add(center); t.Add(a); t.Add(b);
                }
            }
            Cone(0.46f, 0.95f, 0.12f, 7);
            Cone(0.34f, 0.8f, 0.6f, 7);
            Cone(0.2f, 0.55f, 1.05f, 6);
            _pine = new Mesh { name = "Çam" };
            _pine.SetVertices(v); _pine.SetTriangles(t, 0); _pine.RecalculateNormals(); _pine.RecalculateBounds();
            return _pine;
        }
        static Mesh RoundMesh()
        {
            if (_round != null) return _round;
            var tmp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _round = tmp.GetComponent<MeshFilter>().sharedMesh;
            Destroy(tmp);
            return _round;
        }

        void BuildForests()
        {
            var pineA = new List<CombineInstance>(); var pineB = new List<CombineInstance>(); var leafy = new List<CombineInstance>();
            var rocks = new List<CombineInstance>(); var dead = new List<CombineInstance>();
            var pine = PineMesh(); var round = RoundMesh();
            int count = 0;
            for (int n = 0; n < 16000 && count < 2800; n++)
            {
                float x = R(-W / 2f + 0.5f, W / 2f - 0.5f), z = R(-D / 2f + 0.5f, D / 2f - 0.5f);
                float h = SampleH(x, z);
                float m = ForestMask(x, z, h);
                if (R() > m * 0.9f) continue;
                if (RiverDist(x, z) < 0.95f || RoadDist(x, z) < 0.65f) continue;
                if (FieldZone(x, z, h) > 0.35f) continue;
                bool blocked = false;
                foreach (var s in Sites)
                {
                    float keep = s.Region.Id == "karaorman" ? 1.2f : s.Radius + 0.3f;
                    if (Vector2.Distance(new Vector2(x, z), new Vector2(s.Local.x, s.Local.z)) < keep) { blocked = true; break; }
                }
                if (blocked) continue;
                float sc = R(0.75f, 1.35f) * (h > 4f ? 0.85f : 1f);
                var mtx = Matrix4x4.TRS(new Vector3(x, h - 0.05f, z), Quaternion.Euler(0, R(0, 360), 0), new Vector3(sc, sc * R(0.9f, 1.25f), sc));
                bool deciduous = h < 1.6f && R() < 0.35f && Bump(x, z, 19.5f, -14f, 9.5f) < 0.4f;
                if (deciduous)
                    leafy.Add(new CombineInstance { mesh = round, transform = Matrix4x4.TRS(new Vector3(x, h + 0.42f * sc, z), Quaternion.identity, new Vector3(0.75f, 0.7f, 0.75f) * sc) });
                else (R() < 0.5f ? pineA : pineB).Add(new CombineInstance { mesh = pine, transform = mtx });
                count++;
            }
            // Dağlarda kaya blokları
            for (int n = 0; n < 900 && rocks.Count < 110; n++)
            {
                float x = R(-W / 2f + 0.5f, W / 2f - 0.5f), z = R(4f, D / 2f - 0.5f);
                float h = SampleH(x, z);
                if (h < 3.5f || R() > 0.5f) continue;
                float sc = R(0.25f, 0.7f);
                rocks.Add(new CombineInstance { mesh = round, transform = Matrix4x4.TRS(new Vector3(x, h, z), Quaternion.Euler(R(0, 40), R(0, 360), R(0, 40)), new Vector3(sc * 1.4f, sc * 0.7f, sc)) });
            }
            // Yıkıkköy'ün yanık, kuru ağaçları
            var yk = SiteById("yikikkoy");
            if (yk != null)
                for (int n = 0; n < 26; n++)
                {
                    float a = R(0, Mathf.PI * 2f), r = R(2.6f, 5f);
                    float x = yk.Local.x + Mathf.Cos(a) * r, z = yk.Local.z + Mathf.Sin(a) * r;
                    if (RoadDist(x, z) < 0.6f) continue;
                    float sc = R(0.6f, 1f);
                    dead.Add(new CombineInstance { mesh = pine, transform = Matrix4x4.TRS(OnGround(x, z, -0.05f), Quaternion.Euler(R(-8, 8), R(0, 360), R(-8, 8)), new Vector3(0.28f, 1f, 0.28f) * sc) });
                }

            Combine("ÇamlarA", pineA, M.Get("map_pine_a", new Color(0.07f, 0.15f, 0.09f), 0.1f));
            Combine("ÇamlarB", pineB, M.Get("map_pine_b", new Color(0.1f, 0.19f, 0.1f), 0.1f));
            Combine("Ağaçlar", leafy, M.Get("map_leafy", new Color(0.22f, 0.27f, 0.11f), 0.1f));
            Combine("Kayalar", rocks, M.Get("map_rock", new Color(0.36f, 0.34f, 0.32f), 0.15f));
            Combine("KuruAğaçlar", dead, M.Get("map_deadwood", new Color(0.13f, 0.11f, 0.1f), 0.1f));
        }

        void Combine(string name, List<CombineInstance> list, Material mat)
        {
            if (list.Count == 0) return;
            var mesh = new Mesh { name = name, indexFormat = IndexFormat.UInt32 };
            mesh.CombineMeshes(list.ToArray(), true, true);
            mesh.RecalculateBounds();
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        }

        // =================================================================
        //  MİNYATÜR KASABALAR
        // =================================================================
        void House(Transform parent, Vector3 pos, float yaw, Vector3 size, Material wall, Material roof, Material window, float litChance, float ridge = 0.35f, bool roofless = false, float tilt = 0f)
        {
            var g = Group("Ev", parent, pos, yaw).transform;
            if (tilt != 0f) g.localRotation *= Quaternion.Euler(tilt, 0, tilt * 0.5f);
            Box("Duvar", g, new Vector3(0, size.y / 2f - 0.05f, 0), size + new Vector3(0, 0.1f, 0), wall, null, false);
            if (!roofless)
                HipRoof("Çatı", g, new Vector3(0, size.y - 0.04f, 0), new Vector3(size.x * 1.14f, size.y * 0.62f, size.z * 1.18f), roof, ridge, 0f, 0f);
            if (window == null) return;
            int wn = size.x > 0.7f ? 2 : 1;
            for (int i = 0; i < wn; i++)
            {
                if (R() > litChance) continue;
                float wx = wn == 1 ? 0f : (i == 0 ? -size.x * 0.24f : size.x * 0.24f);
                Box("Pencere", g, new Vector3(wx, size.y * 0.52f, -size.z / 2f - 0.012f), new Vector3(0.1f, 0.12f, 0.02f), window, null, false);
                if (R() < 0.5f) Box("Pencere", g, new Vector3(-wx, size.y * 0.52f, size.z / 2f + 0.012f), new Vector3(0.1f, 0.12f, 0.02f), window, null, false);
            }
        }

        /// <summary>Kasaba çevresine çakışmadan ev yerleştirir (ön cephe merkeze / yola bakar).</summary>
        List<Vector3> Scatter(Site s, int count, float rMin, float rMax, float minGap, float upBias = 0f)
        {
            var list = new List<Vector3>();
            for (int n = 0; n < count * 40 && list.Count < count; n++)
            {
                float a = R(0, Mathf.PI * 2f);
                if (upBias > 0f && R() < upBias) a = R(Mathf.PI * 0.15f, Mathf.PI * 0.85f);    // kuzeye (yokuş yukarı)
                float r = Mathf.Lerp(rMin, rMax, Mathf.Sqrt(R()));
                float x = s.Local.x + Mathf.Cos(a) * r, z = s.Local.z + Mathf.Sin(a) * r;
                if (RiverDist(x, z) < 1.0f || RoadDist(x, z) < 0.38f) continue;
                if (Mathf.Abs(x) > W / 2f - 1f || Mathf.Abs(z) > D / 2f - 1f) continue;
                bool ok = true;
                foreach (var q in list) if ((new Vector2(q.x, q.z) - new Vector2(x, z)).sqrMagnitude < minGap * minGap) { ok = false; break; }
                if (!ok) continue;
                list.Add(OnGround(x, z, 0f));
            }
            return list;
        }

        /// <summary>Kasabadan çıkan yolların iki yanına dizilen evler (ön cephe yola bakar).</summary>
        void RoadHouses(Transform root, Site s, Vector2[] road, float from, float to, float spacing, Func<Vector3> size, Material wall, Material roof, Material win, float lit, float ridge = 0.35f)
        {
            var list = new List<Vector2>();
            float total = 0f;
            for (int i = 0; i < road.Length - 1; i++) total += Vector2.Distance(road[i], road[i + 1]);
            if (total <= 0f) return;
            var c2 = new Vector2(s.Local.x, s.Local.z);
            bool fromEnd = Vector2.Distance(road[road.Length - 1], c2) < Vector2.Distance(road[0], c2);
            for (float d = from; d < to; d += spacing * R(0.85f, 1.25f))
            {
                float u = Mathf.Clamp01((fromEnd ? total - d : d) / total);
                var p = PolyPoint(road, u);
                var q = PolyPoint(road, Mathf.Clamp01(u + 0.01f));
                var dir = (q - p).normalized;
                if (dir.sqrMagnitude < 0.5f) continue;
                var nrm = new Vector2(-dir.y, dir.x);
                foreach (var side in new[] { -1f, 1f })
                {
                    if (R() < 0.18f) continue;
                    var hp = p + nrm * side * R(0.62f, 0.8f);
                    if (RiverDist(hp.x, hp.y) < 1.0f) continue;
                    bool clash = false;
                    foreach (var o in list) if ((o - hp).sqrMagnitude < 0.36f) { clash = true; break; }
                    if (clash) continue;
                    list.Add(hp);
                    var face = -nrm * side;   // yola bakan cephe (-Z yerel)
                    float yaw = Mathf.Atan2(-face.x, -face.y) * Mathf.Rad2Deg;
                    House(root, OnGround(hp.x, hp.y), yaw, size(), wall, roof, win, lit, ridge);
                }
            }
        }

        float FaceCenterYaw(Vector3 p, Vector3 c) { var away = p - c; away.y = 0; return Mathf.Atan2(away.x, away.z) * Mathf.Rad2Deg; }

        void BuildTowns()
        {
            var warm = M.Emissive("map_window_warm", new Color(1f, 0.64f, 0.3f), 3.2f);
            var amber = M.Emissive("map_window_amber", new Color(1f, 0.78f, 0.32f), 3.6f);
            var pale = M.Emissive("map_window_pale", new Color(0.86f, 0.92f, 0.74f), 2.2f);
            foreach (var s in Sites)
            {
                var root = Group("Kasaba_" + s.Region.Id, transform, Vector3.zero).transform;
                switch (s.Region.Id)
                {
                    case "gizemli": BuildGizemli(s, root, warm); break;
                    case "golge_sehir": BuildGolge(s, root, amber); break;
                    case "sisoren": BuildSisoren(s, root, pale); break;
                    case "yikikkoy": BuildYikik(s, root); break;
                    default: BuildKaraorman(s, root, warm); break;
                }
                // Kasabanın tema ışığı (mavi / sarı / yeşil); seçilince parlar
                var glow = PointLight("TemaIşığı", root, s.Local + new Vector3(0, 3.2f, 0), s.Theme, 1f, 10f * S);
                s.Glow = glow; s.GlowBase = s.Playable ? 0.09f : 0.055f;
                glow.intensity = s.GlowBase;
                glow.shadows = LightShadows.None;
            }
        }

        void CrimeTape(Transform root, Vector3 c)
        {
            var tape = M.Emissive("map_tape", new Color(1f, 0.82f, 0.1f), 1.2f);
            float y = SampleH(c.x, c.z) + 0.06f, h = 0.25f;
            Box("Şerit", root, new Vector3(c.x, y, c.z - h), new Vector3(h * 2f, 0.025f, 0.025f), tape, null, false);
            Box("Şerit", root, new Vector3(c.x, y, c.z + h), new Vector3(h * 2f, 0.025f, 0.025f), tape, null, false);
            Box("Şerit", root, new Vector3(c.x - h, y, c.z), new Vector3(0.025f, 0.025f, h * 2f), tape, null, false);
            Box("Şerit", root, new Vector3(c.x + h, y, c.z), new Vector3(0.025f, 0.025f, h * 2f), tape, null, false);
            Box("Ceset", root, new Vector3(c.x, y - 0.02f, c.z), new Vector3(0.12f, 0.03f, 0.26f), M.Get("map_sheet", new Color(0.82f, 0.8f, 0.75f), 0.2f), new Vector3(0, 30f, 0), false);
        }

        void BuildGizemli(Site s, Transform root, Material win)
        {
            var c = s.Local;
            // Meydan taşları
            Cyl("Meydan", root, c + new Vector3(0, 0.02f, 0), new Vector3(2.2f, 0.03f, 2.2f), M.Get("map_plaza", new Color(0.46f, 0.45f, 0.44f), 0.35f), null, false);
            Color[] walls = { new Color(0.78f, 0.74f, 0.66f), new Color(0.7f, 0.62f, 0.5f), new Color(0.62f, 0.66f, 0.72f), new Color(0.8f, 0.78f, 0.72f) };
            Color[] roofs = { new Color(0.48f, 0.2f, 0.13f), new Color(0.38f, 0.16f, 0.11f), new Color(0.3f, 0.26f, 0.24f) };
            foreach (var p in Scatter(s, 24, 1.25f, 3.3f, 0.82f))
            {
                var size = new Vector3(R(0.55f, 0.85f), R(0.45f, 0.75f), R(0.5f, 0.7f));
                House(root, p, FaceCenterYaw(p, c) + R(-12f, 12f), size, M.Plaster(walls[_rng.Next(walls.Length)]), M.RoofTiles(roofs[_rng.Next(roofs.Length)]), win, 0.75f);
            }
            // Kasabadan çıkan yollar boyunca sıralanan evler (kasaba yollara doğru uzar)
            foreach (var road in _roads)
                RoadHouses(root, s, road, 3.4f, R(5.5f, 7.5f), 0.95f, () => new Vector3(R(0.5f, 0.75f), R(0.42f, 0.62f), R(0.45f, 0.62f)),
                    M.Plaster(walls[_rng.Next(walls.Length)]), M.RoofTiles(roofs[_rng.Next(roofs.Length)]), win, 0.6f);
            // Saat kulesi
            var stone = M.Stone;
            var tower = Group("SaatKulesi", root, c + new Vector3(0.7f, 0f, 0.55f)).transform;
            Box("Gövde", tower, new Vector3(0, 0.8f, 0), new Vector3(0.36f, 1.6f, 0.36f), stone, null, false);
            HipRoof("Külah", tower, new Vector3(0, 1.6f, 0), new Vector3(0.46f, 0.5f, 0.46f), M.RoofTiles(new Color(0.3f, 0.24f, 0.22f)), 0f, 0f, 0f);
            var clock = M.Emissive("map_clockface", new Color(0.95f, 0.88f, 0.7f), 0.9f);
            Box("Saat", tower, new Vector3(0, 1.38f, -0.185f), new Vector3(0.2f, 0.2f, 0.02f), clock, null, false);
            Box("Saat", tower, new Vector3(0, 1.38f, 0.185f), new Vector3(0.2f, 0.2f, 0.02f), clock, null, false);
            // Cami: kubbe + minare
            var mosque = Group("Cami", root, c + new Vector3(-1.1f, 0f, 1.2f)).transform;
            var mw = M.Plaster(new Color(0.84f, 0.82f, 0.78f));
            Box("Harim", mosque, new Vector3(0, 0.3f, 0), new Vector3(1f, 0.6f, 1f), mw, null, false);
            Sphere("Kubbe", mosque, new Vector3(0, 0.62f, 0), new Vector3(0.8f, 0.7f, 0.8f), M.Get("map_lead", new Color(0.36f, 0.4f, 0.44f), 0.5f, 0.4f));
            Cyl("Minare", mosque, new Vector3(0.62f, 0.95f, 0.45f), new Vector3(0.13f, 0.95f, 0.13f), mw, null, false);
            Cyl("Şerefe", mosque, new Vector3(0.62f, 1.5f, 0.45f), new Vector3(0.2f, 0.03f, 0.2f), mw, null, false);
            HipRoof("Külah", mosque, new Vector3(0.62f, 1.9f, 0.45f), new Vector3(0.14f, 0.42f, 0.14f), M.Get("map_lead", new Color(0.36f, 0.4f, 0.44f), 0.5f, 0.4f), 0f, 0f, 0f);
            CrimeTape(root, c + new Vector3(-0.2f, 0, -0.55f));
            Smoke(root, c + new Vector3(-2.2f, 0.9f, -1.4f), 0.18f);
            Smoke(root, c + new Vector3(2.4f, 0.9f, 1.2f), 0.18f);
        }

        void BuildGolge(Site s, Transform root, Material win)
        {
            var c = s.Local;
            var logs = M.Wood(new Color(0.32f, 0.21f, 0.12f));
            var logsDark = M.Wood(new Color(0.22f, 0.15f, 0.09f));
            var slate = M.RoofTiles(new Color(0.2f, 0.19f, 0.18f));
            foreach (var p in Scatter(s, 18, 0.9f, 2.9f, 0.8f))
            {
                var size = new Vector3(R(0.6f, 0.9f), R(0.4f, 0.55f), R(0.5f, 0.65f));
                House(root, p, FaceCenterYaw(p, c) + R(-15f, 15f), size, R() < 0.5f ? logs : logsDark, slate, win, 0.7f, 0.55f);
            }
            RoadHouses(root, s, _roads[0], 2.9f, 6f, 1.05f, () => new Vector3(R(0.55f, 0.8f), R(0.38f, 0.5f), R(0.5f, 0.6f)), logs, slate, win, 0.6f, 0.55f);
            // Gözetleme kulesi (bekçi Rıfat'ın)
            var tw = Group("GözetlemeKulesi", root, OnGround(c.x - 1.6f, c.z + 1.4f)).transform;
            foreach (var lx in new[] { -0.15f, 0.15f })
                foreach (var lz in new[] { -0.15f, 0.15f })
                    Box("Ayak", tw, new Vector3(lx, 0.7f, lz), new Vector3(0.05f, 1.4f, 0.05f), logsDark, null, false);
            Box("Kulübe", tw, new Vector3(0, 1.5f, 0), new Vector3(0.45f, 0.3f, 0.45f), logs, null, false);
            HipRoof("Çatı", tw, new Vector3(0, 1.65f, 0), new Vector3(0.6f, 0.3f, 0.6f), slate, 0f, 0f, 0f);
            // Yol boyu sarı fenerler
            var lamp = M.Emissive("map_lantern_yellow", new Color(1f, 0.8f, 0.3f), 5f);
            var road = _roads[0];
            for (int i = 0; i < 9; i++)
            {
                var p2 = Vector2.Lerp(road[road.Length - 1], road[road.Length - 2], i / 9f * 1.6f);
                var side = (i % 2 == 0 ? 1f : -1f) * 0.42f;
                var dir = (road[road.Length - 2] - road[road.Length - 1]).normalized;
                var pp = p2 + new Vector2(-dir.y, dir.x) * side;
                var b = OnGround(pp.x, pp.y);
                Cyl("FenerDireği", root, b + new Vector3(0, 0.22f, 0), new Vector3(0.025f, 0.22f, 0.025f), logsDark, null, false);
                Sphere("Fener", root, b + new Vector3(0, 0.47f, 0), Vector3.one * 0.07f, lamp);
            }
            CrimeTape(root, c + new Vector3(0.4f, 0, -0.6f));
            Smoke(root, c + new Vector3(1.3f, 0.8f, 1.1f), 0.2f);
        }

        void BuildSisoren(Site s, Transform root, Material win)
        {
            var c = s.Local;
            var stone = M.Get("map_fieldstone", new Color(0.52f, 0.53f, 0.5f), 0.1f, 0f, M.NoiseTexture);
            var stone2 = M.Get("map_fieldstone2", new Color(0.62f, 0.6f, 0.55f), 0.1f, 0f, M.NoiseTexture);
            var roof = M.RoofTiles(new Color(0.22f, 0.27f, 0.25f));
            foreach (var p in Scatter(s, 24, 0.8f, 3.4f, 0.75f, 0.45f))
            {
                var size = new Vector3(R(0.5f, 0.75f), R(0.45f, 0.7f), R(0.45f, 0.62f));
                // Yamaçta evin alt tarafı toprağa gömülür, kaymasın diye taş temel
                House(root, p + new Vector3(0, -0.12f, 0), FaceCenterYaw(p, c) + 180f, size + new Vector3(0, 0.12f, 0), R() < 0.5f ? stone : stone2, roof, win, 0.55f, 0.3f);
            }
            // Sinema (kasabanın tek ışıklı tabelası) + telgrafhane direkleri
            var cin = Group("Sinema", root, OnGround(c.x + 0.9f, c.z - 0.6f), 15f).transform;
            Box("Salon", cin, new Vector3(0, 0.4f, 0), new Vector3(1.2f, 0.8f, 0.9f), stone2, null, false);
            Box("Tabela", cin, new Vector3(0, 0.72f, -0.47f), new Vector3(0.8f, 0.12f, 0.03f), M.Emissive("map_marquee", new Color(1f, 0.85f, 0.55f), 2.4f), null, false);
            HipRoof("Çatı", cin, new Vector3(0, 0.8f, 0), new Vector3(1.3f, 0.35f, 1f), roof, 0.6f, 0f, 0f);
            var road = _roads[1];
            var poleMat = M.Wood(new Color(0.24f, 0.17f, 0.11f));
            var wire = M.Get("map_wire", new Color(0.05f, 0.05f, 0.05f), 0.3f);
            Vector3? last = null;
            for (int i = 0; i <= 12; i++)
            {
                var p2 = PolyPoint(road, i / 12f);
                var dir = (PolyPoint(road, Mathf.Min(1f, i / 12f + 0.02f)) - PolyPoint(road, Mathf.Max(0f, i / 12f - 0.02f))).normalized;
                var pp = p2 + new Vector2(-dir.y, dir.x) * 0.45f;
                var b = OnGround(pp.x, pp.y);
                Cyl("TelgrafDireği", root, b + new Vector3(0, 0.35f, 0), new Vector3(0.03f, 0.35f, 0.03f), poleMat, null, false);
                var top = b + new Vector3(0, 0.68f, 0);
                if (last.HasValue) Beam(root, last.Value, top, 0.012f, wire);
                last = top;
            }
            CrimeTape(root, c + new Vector3(-0.3f, 0, -0.4f));
            Smoke(root, c + new Vector3(-1.2f, 1.2f, 1.6f), 0.16f);
        }

        void BuildYikik(Site s, Transform root)
        {
            var c = s.Local;
            var charred = M.Get("map_charred", new Color(0.17f, 0.14f, 0.12f), 0.05f, 0f, M.NoiseTexture);
            var ash = M.Get("map_ashwall", new Color(0.36f, 0.32f, 0.28f), 0.05f, 0f, M.NoiseTexture);
            foreach (var p in Scatter(s, 12, 0.5f, 2.6f, 0.85f))
            {
                var size = new Vector3(R(0.5f, 0.8f), R(0.18f, 0.5f), R(0.45f, 0.65f));
                House(root, p, R(0, 360), size, R() < 0.6f ? charred : ash, null, null, 0f, 0.3f, true, R(-9f, 9f));
            }
            // Barikat: yol kapalı
            var road = _roads[2];
            var bp = PolyPoint(road, 0.78f);
            var dir = (PolyPoint(road, 0.8f) - PolyPoint(road, 0.76f)).normalized;
            float yaw = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
            var bar = Group("Barikat", root, OnGround(bp.x, bp.y), yaw).transform;
            var plank = M.Wood(new Color(0.42f, 0.3f, 0.18f));
            var red = M.Get("map_barrier_red", new Color(0.6f, 0.08f, 0.06f), 0.3f);
            Box("Çapraz", bar, new Vector3(0, 0.16f, 0), new Vector3(0.7f, 0.05f, 0.05f), plank, new Vector3(0, 0, 18f), false);
            Box("Çapraz", bar, new Vector3(0, 0.16f, 0), new Vector3(0.7f, 0.05f, 0.05f), red, new Vector3(0, 0, -18f), false);
            Smoke(root, c + new Vector3(0.3f, 0.3f, 0.2f), 0.5f, true);
            Smoke(root, c + new Vector3(-0.9f, 0.3f, -0.7f), 0.4f, true);
        }

        void BuildKaraorman(Site s, Transform root, Material win)
        {
            var c = s.Local;
            var wood = M.Wood(new Color(0.18f, 0.12f, 0.08f));
            House(root, OnGround(c.x, c.z), 30f, new Vector3(0.55f, 0.38f, 0.45f), wood, M.RoofTiles(new Color(0.15f, 0.14f, 0.13f)), win, 0.6f, 0.6f);
            House(root, OnGround(c.x + 0.8f, c.z - 0.5f), -20f, new Vector3(0.45f, 0.32f, 0.4f), wood, M.RoofTiles(new Color(0.15f, 0.14f, 0.13f)), null, 0f, 0.6f);
        }

        static Vector2 PolyPoint(Vector2[] pts, float u)
        {
            float total = 0f;
            for (int i = 0; i < pts.Length - 1; i++) total += Vector2.Distance(pts[i], pts[i + 1]);
            float target = Mathf.Clamp01(u) * total;
            for (int i = 0; i < pts.Length - 1; i++)
            {
                float l = Vector2.Distance(pts[i], pts[i + 1]);
                if (target <= l) return Vector2.Lerp(pts[i], pts[i + 1], l > 0f ? target / l : 0f);
                target -= l;
            }
            return pts[pts.Length - 1];
        }

        GameObject Beam(Transform parent, Vector3 a, Vector3 b, float thick, Material mat)
        {
            var go = Box("Tel", parent, (a + b) / 2f, new Vector3(thick, thick, Vector3.Distance(a, b)), mat, null, false);
            go.transform.localRotation = Quaternion.LookRotation(b - a);
            go.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            return go;
        }

        // =================================================================
        //  HARİTA İĞNELERİ + KIRMIZI İP (dedektif panosu)
        // =================================================================
        void BuildPinsAndStrings()
        {
            var steel = M.Get("map_pin_steel", new Color(0.7f, 0.7f, 0.72f), 0.85f, 0.9f);
            foreach (var s in Sites)
            {
                var b = OnGround(s.Local.x + 0.55f, s.Local.z + 0.35f);
                Cyl("İğne", transform, b + new Vector3(0, 1.3f, 0), new Vector3(0.06f, 1.3f, 0.06f), steel, null, false);
                var headCol = s.Playable ? s.Theme : new Color(0.32f, 0.3f, 0.3f);
                var head = Sphere("İğneBaşı", transform, b + new Vector3(0, 2.7f, 0), Vector3.one * 0.48f, M.Emissive("map_pinhead_" + s.Region.Id, headCol, s.Playable ? 0.35f : 0.05f));
                s.PinTop = b + new Vector3(0, 2.95f, 0);
                _pins.Add((s, head.GetComponent<Renderer>()));
            }
            // Vaka zinciri: Gizemli → Gölgeşehir → Sisören
            var yarn = M.Get("map_yarn", new Color(0.62f, 0.05f, 0.05f), 0.25f);
            string[] chain = { "gizemli", "golge_sehir", "sisoren" };
            for (int i = 0; i < chain.Length - 1; i++)
            {
                var a = SiteById(chain[i]); var b = SiteById(chain[i + 1]);
                if (a == null || b == null) continue;
                var go = Beam(transform, a.PinTop + Vector3.down * 0.25f, b.PinTop + Vector3.down * 0.25f, 0.07f, yarn);
                go.name = "Ip_" + chain[i + 1];
                _strings.Add(go);
            }
        }

        // =================================================================
        //  HAVA: yağmur, bulutlar, sis, duman, şimşek
        // =================================================================
        static ParticleSystem Ps(string name, Transform parent, Vector3 pos, Vector3 euler, Material mat)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(euler);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.playOnAwake = true; main.loop = true;
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false;
            return ps;
        }

        static Gradient FadeInOut(float inAt = 0.2f, float outAt = 0.75f)
        {
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                      new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, inAt), new GradientAlphaKey(1, outAt), new GradientAlphaKey(0, 1) });
            return g;
        }

        void Rain(Transform parent, Vector3 center, float area, float rate)
        {
            var mat = Effects.TransparentMaterial(new Color(0.7f, 0.78f, 0.92f, 0.32f));
            if (mat == null) return;
            var ps = Ps("Yağmur", parent, center + new Vector3(0, 6.5f, 0), new Vector3(90, 0, 0), mat);
            var main = ps.main;
            main.startLifetime = 0.55f; main.startSpeed = new ParticleSystem.MinMaxCurve(10f, 13f); main.startSize = 0.035f;
            main.startColor = new Color(0.7f, 0.78f, 0.92f, 0.32f); main.maxParticles = 1500; main.prewarm = true;
            var em = ps.emission; em.rateOverTime = rate;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(area, area, 0.1f);
            var r = ps.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.Stretch; r.lengthScale = 4f; r.velocityScale = 0.02f;
        }

        void Clouds(Transform parent, Vector3 center, Vector3 area, Color col, int count, float size, float life = 14f, float drift = 0.25f)
        {
            var mat = Effects.TransparentMaterial(col, true);
            if (mat == null) return;
            var ps = Ps("Bulutlar", parent, center, Vector3.zero, mat);
            var main = ps.main;
            main.startLifetime = life; main.startSpeed = 0f; main.startSize = new ParticleSystem.MinMaxCurve(size * 0.6f, size);
            main.startColor = col; main.maxParticles = count * 2; main.prewarm = true;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            var em = ps.emission; em.rateOverTime = count / life;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = area;
            var vel = ps.velocityOverLifetime; vel.enabled = true; vel.space = ParticleSystemSimulationSpace.Local;
            vel.x = new ParticleSystem.MinMaxCurve(drift * 0.6f, drift); vel.y = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f); vel.z = new ParticleSystem.MinMaxCurve(-drift * 0.3f, drift * 0.3f);
            var colo = ps.colorOverLifetime; colo.enabled = true; colo.color = FadeInOut(0.25f, 0.7f);
            var rot = ps.rotationOverLifetime; rot.enabled = true; rot.z = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f);
        }

        void Smoke(Transform parent, Vector3 pos, float size, bool heavy = false)
        {
            var col = heavy ? new Color(0.2f, 0.19f, 0.18f, 0.45f) : new Color(0.62f, 0.62f, 0.64f, 0.28f);
            var mat = Effects.TransparentMaterial(col, true);
            if (mat == null) return;
            var ps = Ps("Duman", parent, pos, new Vector3(-90, 0, 0), mat);
            var main = ps.main;
            main.startLifetime = heavy ? 9f : 6f; main.startSpeed = heavy ? 0.45f : 0.3f;
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.6f, size * 1.2f);
            main.startColor = col; main.maxParticles = 60; main.prewarm = true;
            var em = ps.emission; em.rateOverTime = heavy ? 5f : 3f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 6f; sh.radius = size * 0.3f;
            var sz = ps.sizeOverLifetime; sz.enabled = true; sz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 0.5f, 1, heavy ? 4f : 3f));
            var vel = ps.velocityOverLifetime; vel.enabled = true; vel.space = ParticleSystemSimulationSpace.Local;
            vel.x = new ParticleSystem.MinMaxCurve(0.15f, 0.3f); vel.y = new ParticleSystem.MinMaxCurve(0f, 0f); vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            var colo = ps.colorOverLifetime; colo.enabled = true; colo.color = FadeInOut(0.15f, 0.5f);
        }

        void BuildWeather()
        {
            var g = SiteById("gizemli"); var o = SiteById("golge_sehir"); var si = SiteById("sisoren");
            var y = SiteById("yikikkoy"); var k = SiteById("karaorman");
            // Gizemli: hafif yağmur, mavi-gri bulut örtüsü
            if (g != null)
            {
                Rain(transform, g.Local, 8f, 420f);
                Clouds(transform, g.Local + new Vector3(0, 7.2f, 0), new Vector3(9f, 0.6f, 8f), new Color(0.2f, 0.24f, 0.32f, 0.55f), 14, 5f);
            }
            // Gölgeşehir: sağanak, alçak kara bulutlar, şimşek
            if (o != null)
            {
                Rain(transform, o.Local, 8f, 1100f);
                Clouds(transform, o.Local + new Vector3(0, 6.6f, 0), new Vector3(10f, 0.8f, 9f), new Color(0.1f, 0.1f, 0.12f, 0.72f), 18, 5.5f);
                gameObject.AddComponent<MapThunder>();   // ışık çakması yok (titreme olmasın) — yalnızca gür gök gürültüsü
            }
            // Sisören: dağ yamacında yeşilimsi sis, alçak bulut
            if (si != null)
            {
                Clouds(transform, si.Local + new Vector3(0, 1.4f, 0.6f), new Vector3(9f, 1.6f, 7f), new Color(0.66f, 0.82f, 0.68f, 0.2f), 34, 4.2f, 16f, 0.12f);
                Clouds(transform, si.Local + new Vector3(0, 4.4f, 3f), new Vector3(12f, 1f, 6f), new Color(0.78f, 0.86f, 0.8f, 0.16f), 14, 6f, 18f, 0.15f);
            }
            // Karaorman: yerde sürünen koyu sis
            if (k != null) Clouds(transform, k.Local + new Vector3(0, 0.8f, 0), new Vector3(9f, 0.6f, 8f), new Color(0.16f, 0.15f, 0.2f, 0.4f), 22, 3.5f, 16f, 0.08f);
            // Bütün haritanın üstünden geçen ince bulut tülü (gölgeler)
            Clouds(transform, new Vector3(-4f, 10.5f, 0f), new Vector3(70f, 0.5f, 46f), new Color(0.55f, 0.58f, 0.66f, 0.07f), 26, 11f, 40f, 0.55f);
        }

        // =================================================================
        //  HAYAT: at arabaları, kargalar
        // =================================================================
        void BuildLife()
        {
            var wood = M.Wood(new Color(0.2f, 0.12f, 0.07f));
            var horse = M.Get("map_horse", new Color(0.23f, 0.15f, 0.1f), 0.2f);
            var lamp = M.Emissive("map_cart_lamp", new Color(1f, 0.72f, 0.35f), 6f);
            int[] roadIdx = { 0, 1, 4, 3 };
            float[] start = { 0.1f, 0.55f, 0.3f, 0.8f };
            for (int n = 0; n < roadIdx.Length; n++)
            {
                if (roadIdx[n] >= _roads.Count) continue;
                var road = _roads[roadIdx[n]];
                var path = new List<Vector3>();
                for (int i = 0; i <= 60; i++) { var p = PolyPoint(road, i / 60f); path.Add(OnGround(p.x, p.y, 0.02f)); }
                var cart = new GameObject("AtArabası").transform;
                cart.SetParent(transform, false);
                var body = Group("Gövde", cart, Vector3.zero).transform;
                Box("Kasa", body, new Vector3(0, 0.22f, -0.12f), new Vector3(0.3f, 0.22f, 0.42f), wood, null, false);
                Box("Tente", body, new Vector3(0, 0.38f, -0.14f), new Vector3(0.32f, 0.08f, 0.44f), M.Get("map_canvas", new Color(0.5f, 0.45f, 0.36f), 0.1f), null, false);
                foreach (var sx in new[] { -0.17f, 0.17f })
                    foreach (var sz in new[] { -0.26f, 0.02f })
                        Cyl("Teker", body, new Vector3(sx, 0.08f, sz), new Vector3(0.16f, 0.015f, 0.16f), wood, new Vector3(0, 0, 90f), false);
                Box("At", body, new Vector3(0, 0.16f, 0.32f), new Vector3(0.1f, 0.12f, 0.28f), horse, null, false);
                Box("AtBaşı", body, new Vector3(0, 0.25f, 0.5f), new Vector3(0.07f, 0.1f, 0.12f), horse, new Vector3(-30f, 0, 0), false);
                Sphere("Fener", body, new Vector3(0.17f, 0.36f, 0.1f), Vector3.one * 0.06f, lamp);
                cart.gameObject.AddComponent<MapCarriage>().Init(path, R(0.55f, 0.8f), start[n]);
            }
            // Kargalar: kilitli bölgelerin üstünde dönüp duran kara gölgeler
            var crowMat = M.Get("map_crow", new Color(0.03f, 0.03f, 0.035f), 0.2f);
            foreach (var id in new[] { "karaorman", "yikikkoy" })
            {
                var s = SiteById(id); if (s == null) continue;
                var flock = new GameObject("Kargalar").transform;
                flock.SetParent(transform, false);
                flock.localPosition = s.Local + new Vector3(0, 4.2f, 0);
                var wings = new List<(Transform l, Transform r, Transform bird, float phase, float radius, float speed, float h)>();
                for (int i = 0; i < 6; i++)
                {
                    var bird = new GameObject("Karga").transform; bird.SetParent(flock, false);
                    Box("Gövde", bird, Vector3.zero, new Vector3(0.06f, 0.05f, 0.18f), crowMat, null, false);
                    var l = Box("Kanat", bird, new Vector3(-0.13f, 0, 0), new Vector3(0.24f, 0.012f, 0.09f), crowMat, null, false).transform;
                    var r = Box("Kanat", bird, new Vector3(0.13f, 0, 0), new Vector3(0.24f, 0.012f, 0.09f), crowMat, null, false).transform;
                    wings.Add((l, r, bird, R(0, 6.28f), R(1.2f, 2.6f), R(0.35f, 0.6f), R(-0.6f, 0.8f)));
                }
                flock.gameObject.AddComponent<MapCrows>().Init(wings);
            }
        }

        // =================================================================
        //  MASA: gaz lambası, şapka, cep saati, büyüteç, telgraf, dosyalar
        // =================================================================
        void BuildDeskProps()
        {
            float y0 = DeskY + 0.05f;
            var brass = M.Get("map_brass", new Color(0.72f, 0.54f, 0.26f), 0.75f, 0.85f);
            var paper = M.Get("map_paper", new Color(0.86f, 0.81f, 0.68f), 0.15f);
            var manila = M.Get("map_manila", new Color(0.66f, 0.53f, 0.32f), 0.15f);
            var felt = M.Get("map_felt", new Color(0.09f, 0.085f, 0.08f), 0.1f);
            var ink = new Color(0.12f, 0.08f, 0.05f);

            // Gaz lambası (sol arka) — haritanın ana sıcak ışığı
            var lampG = Group("GazLambası", transform, new Vector3(-40f, y0, 21f)).transform;
            Cyl("Kaide", lampG, new Vector3(0, 0.5f, 0), new Vector3(4.2f, 0.5f, 4.2f), brass, null, false);
            Sphere("Hazne", lampG, new Vector3(0, 2.2f, 0), new Vector3(3.6f, 2.6f, 3.6f), M.Get("map_lamp_glass_green", new Color(0.12f, 0.25f, 0.16f), 0.9f, 0.1f));
            Cyl("Boğaz", lampG, new Vector3(0, 3.8f, 0), new Vector3(1.6f, 0.4f, 1.6f), brass, null, false);
            Cyl("Şişe", lampG, new Vector3(0, 6.2f, 0), new Vector3(2.0f, 2.2f, 2.0f), M.Emissive("map_lamp_flame", new Color(1f, 0.74f, 0.4f), 2.2f), null, false);
            _lamp = PointLight("LambaIşığı", lampG, new Vector3(4f, 13f, -3f), new Color(1f, 0.74f, 0.44f), 1f, 130f * S, true);
            _lamp.intensity = 13f;   // sabit ışık: harita üzerinde titreme yok

            // Fötr şapka (sol ön)
            var hat = Group("FötrŞapka", transform, new Vector3(-45f, y0, -16f), 25f).transform;
            Cyl("Kenar", hat, new Vector3(0, 0.15f, 0), new Vector3(10f, 0.15f, 9f), felt, null, false);
            Cyl("Tepe", hat, new Vector3(0, 2.0f, 0), new Vector3(6f, 1.8f, 5.4f), felt, null, false);
            Cyl("Kurdele", hat, new Vector3(0, 0.75f, 0), new Vector3(6.15f, 0.45f, 5.55f), M.Get("map_hatband", new Color(0.22f, 0.07f, 0.05f), 0.3f), null, false);

            // Vaka dosyaları (sağ ön): üst üste üç dosya, en üstte GİZLİ damgası
            var files = Group("VakaDosyaları", transform, new Vector3(44f, y0, -18f), -12f).transform;
            for (int i = 0; i < 3; i++)
                Box("Dosya", files, new Vector3(R(-0.6f, 0.6f), 0.25f + i * 0.5f, R(-0.6f, 0.6f)), new Vector3(15f, 0.45f, 11f), manila, new Vector3(0, R(-7f, 7f), 0), false);
            var t1 = Text3D("DosyaYazı", files, new Vector3(-1.5f, 1.55f, 1.5f), "VAKA DOSYALARI\n1887", ink, 0.32f);
            t1.transform.localRotation = Quaternion.Euler(90, 0, 0);
            var t2 = Text3D("Damga", files, new Vector3(2.5f, 1.56f, -2.6f), "GİZLİ", new Color(0.62f, 0.08f, 0.06f), 0.45f);
            t2.transform.localRotation = Quaternion.Euler(90, -14f, 0);

            // Cep saati (sağ arka) + zinciri
            var watch = Group("CepSaati", transform, new Vector3(43f, y0, 15f)).transform;
            Cyl("Kasa", watch, new Vector3(0, 0.3f, 0), new Vector3(5.6f, 0.3f, 5.6f), brass, null, false);
            Cyl("Kadran", watch, new Vector3(0, 0.62f, 0), new Vector3(4.8f, 0.04f, 4.8f), M.Get("map_watchface", new Color(0.93f, 0.89f, 0.78f), 0.6f), null, false);
            Box("Akrep", watch, new Vector3(0.5f, 0.68f, 0.3f), new Vector3(0.2f, 0.04f, 1.4f), M.Get("map_hand", new Color(0.08f, 0.06f, 0.04f), 0.4f), new Vector3(0, 60f, 0), false);
            Box("Yelkovan", watch, new Vector3(-0.2f, 0.7f, 0.9f), new Vector3(0.14f, 0.04f, 2f), M.Get("map_hand", new Color(0.08f, 0.06f, 0.04f), 0.4f), new Vector3(0, -12f, 0), false);
            Cyl("Kurma", watch, new Vector3(0, 0.3f, 3.1f), new Vector3(0.8f, 0.3f, 0.8f), brass, new Vector3(90, 0, 0), false);
            for (int i = 0; i < 16; i++)
            {
                float a = i / 15f;
                Sphere("Zincir", watch, new Vector3(Mathf.Sin(a * 3.4f) * 3f - 1f, 0.2f, 3.6f + a * 7f), Vector3.one * 0.45f, brass);
            }

            // Büyüteç (sağ kenar)
            var mag = Group("Büyüteç", transform, new Vector3(41f, y0, -1f), -35f).transform;
            Cyl("Çerçeve", mag, new Vector3(0, 0.2f, 0), new Vector3(7f, 0.2f, 7f), brass, null, false);
            Cyl("Mercek", mag, new Vector3(0, 0.42f, 0), new Vector3(6.3f, 0.06f, 6.3f), M.Get("map_lens", new Color(0.55f, 0.62f, 0.66f), 0.98f, 0.2f), null, false);
            Box("Sap", mag, new Vector3(0, 0.4f, -6.3f), new Vector3(1.1f, 0.8f, 6f), M.Wood(new Color(0.12f, 0.07f, 0.04f)), null, false);

            // Telgraf (sol arka-ön arası)
            var tel = Group("Telgraf", transform, new Vector3(-41f, y0, 2f), 8f).transform;
            Box("Kâğıt", tel, new Vector3(0, 0.05f, 0), new Vector3(11f, 0.08f, 7.5f), paper, null, false);
            var tt = Text3D("TelgrafYazı", tel, new Vector3(0, 0.12f, 0), "TELGRAF — ACELE\n\nDEDEKTİF BEY'E\nKASABADA CİNAYET STOP\nDERHAL GELİNİZ STOP", ink, 0.17f);
            tt.transform.localRotation = Quaternion.Euler(90, 0, 0);

            // Mürekkep hokkası + tüy kalem
            var inkG = Group("Hokka", transform, new Vector3(-46f, y0, -1f)).transform;
            Cyl("Şişe", inkG, new Vector3(0, 1f, 0), new Vector3(2.4f, 1f, 2.4f), M.Get("map_inkwell", new Color(0.04f, 0.04f, 0.06f), 0.9f), null, false);
            Box("Tüy", inkG, new Vector3(0.6f, 3.2f, 0.4f), new Vector3(0.35f, 5.5f, 1.1f), paper, new Vector3(18f, 30f, 12f), false);
        }

        // =================================================================
        //  GÖSTER / GİZLE + KAMERA
        // =================================================================
        public void Show(Camera cam)
        {
            if (cam == null) return;
            _cam = cam;
            gameObject.SetActive(true);
            Active = true; Selected = null; Hover = null; _dive = null; _onDive = null;
            _shownAt = Time.time;

            _prevClear = cam.clearFlags; _prevBg = cam.backgroundColor; _prevFov = cam.fieldOfView; _prevNear = cam.nearClipPlane;
            _prevFog = RenderSettings.fog;
            _prevAmbSky = RenderSettings.ambientSkyColor; _prevAmbEq = RenderSettings.ambientEquatorColor; _prevAmbGround = RenderSettings.ambientGroundColor;
            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urp != null) { _prevShadowDist = urp.shadowDistance; urp.shadowDistance = 10f; }

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.016f, 0.017f, 0.024f);
            cam.fieldOfView = 36f;
            cam.nearClipPlane = 0.02f;
            RenderSettings.fog = false;
            RenderSettings.ambientSkyColor = new Color(0.3f, 0.32f, 0.42f);
            RenderSettings.ambientEquatorColor = new Color(0.18f, 0.18f, 0.21f);
            RenderSettings.ambientGroundColor = new Color(0.08f, 0.07f, 0.06f);
            var sun = RenderSettings.sun;
            if (sun != null)
            {
                sun.transform.rotation = Quaternion.Euler(50f, 28f, 0f);
                sun.color = new Color(0.66f, 0.74f, 0.96f);
                sun.intensity = 1.05f;
                sun.shadowStrength = 0.8f;
            }
            RefreshProgress();
            _lookCur = BaseLook; _offCur = IntroOffset;
            ApplyCamera(0f);
        }

        public void Hide()
        {
            if (!Active) return;
            Active = false;
            gameObject.SetActive(false);
            if (_cam != null)
            {
                _cam.clearFlags = _prevClear; _cam.backgroundColor = _prevBg; _cam.fieldOfView = _prevFov; _cam.nearClipPlane = _prevNear;
            }
            RenderSettings.fog = _prevFog;
            RenderSettings.ambientSkyColor = _prevAmbSky; RenderSettings.ambientEquatorColor = _prevAmbEq; RenderSettings.ambientGroundColor = _prevAmbGround;
            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urp != null) urp.shadowDistance = GameSettings.ShadowDistance;
            _dive = null; _onDive = null;
        }

        /// <summary>Kırmızı ip ve iğneler ilerlemeyi gösterir: açılan vakalar iple bağlanır, çözülenler parlar.</summary>
        void RefreshProgress()
        {
            var gm = GameManager.Instance;
            foreach (var go in _strings)
            {
                var id = go.name.Replace("Ip_", "");
                var s = SiteById(id);
                go.SetActive(gm != null && s != null && gm.IsTownUnlocked(s.Region));
            }
        }

        public void Select(Site s)
        {
            if (_dive != null) return;
            if (s != Selected) { Selected = s; SelectedAt = Time.time; }
        }

        /// <summary>"Soruşturmaya Başla": kamera kasabanın içine dalar, ardından callback (hikâye) çalışır.</summary>
        public void Dive(Site s, Action onDone)
        {
            if (s == null || _dive != null) return;
            Selected = s; _dive = s; _diveT = 0f; _onDive = onDone;
            AudioManager.Instance?.Shimmer();
        }

        void LateUpdate()
        {
            if (!Active || _cam == null) return;
            // Seçili / üzerine gelinen kasabanın ışığı nabız gibi parlar
            foreach (var s in Sites)
            {
                if (s.Glow == null) continue;
                float k = s == _dive || s == Selected ? 3.2f : s == Hover ? 2.2f : 1f;
                s.Glow.intensity = Mathf.Lerp(s.Glow.intensity, s.GlowBase * k, Time.deltaTime * 3f);
            }
            ApplyCamera(Time.deltaTime);
        }

        void ApplyCamera(float dt)
        {
            Vector3 look = BaseLook, off = BaseOffset;
            var focus = _dive ?? Selected ?? Hover;
            if (focus != null && focus == Selected && _dive == null)
            {
                // Seçili kasaba ekranın sol üçte birinde; sağda vaka dosyası açılır
                look = focus.Local + new Vector3(3.4f, 0.6f, 0f);
                off = BaseOffset * 0.36f;
            }
            else if (focus != null && _dive == null)
            {
                look = Vector3.Lerp(BaseLook, focus.Local + Vector3.up * 0.6f, 0.22f);
                off = BaseOffset * 0.9f;
            }
            if (_dive != null)
            {
                _diveT += dt;
                float dk = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_diveT / 1.7f));
                look = _dive.Local + Vector3.up * 0.35f;
                off = Vector3.Lerp(BaseOffset * 0.36f, new Vector3(0.8f, 1.6f, -3.6f), dk);
                if (_diveT > 1.15f && _onDive != null) { var cb = _onDive; _onDive = null; cb(); }
            }
            // Yavaş "nefes alan" kamera + fareyle hafif paralaks
            float mx = 0f, my = 0f;
            var mouse = Mouse.current;
            if (mouse != null && Screen.width > 0)
            {
                var mp = mouse.position.ReadValue();
                mx = Mathf.Clamp(mp.x / Screen.width - 0.5f, -0.5f, 0.5f);
                my = Mathf.Clamp(mp.y / Screen.height - 0.5f, -0.5f, 0.5f);
            }
            float t = Time.time;
            float yaw = mx * 6f + Mathf.Sin(t * 0.06f) * 3f;
            off = Quaternion.Euler(-my * 3f, yaw, 0f) * off;

            float introK = Mathf.Clamp01((Time.time - _shownAt) / 4f);
            float rate = dt <= 0f ? 0f : 1f - Mathf.Exp(-dt * Mathf.Lerp(0.7f, _dive != null ? 3.5f : 2.4f, introK));
            _lookCur = Vector3.Lerp(_lookCur, look, rate);
            _offCur = Vector3.Lerp(_offCur, off, rate);
            var breathe = new Vector3(Mathf.Sin(t * 0.31f), Mathf.Sin(t * 0.23f) * 0.6f, Mathf.Sin(t * 0.17f)) * 0.25f;
            var posW = transform.TransformPoint(_lookCur + _offCur + breathe);
            var lookW = transform.TransformPoint(_lookCur);
            _cam.transform.SetPositionAndRotation(posW, Quaternion.LookRotation(lookW - posW));
            FocusPoint = lookW;
            FocusDistance = Vector3.Distance(posW, lookW);
        }

        /// <summary>Dünya uzayında kasaba merkezi ve iğne başı (arayüz etiketleri için).</summary>
        public Vector3 WorldOf(Vector3 local) => transform.TransformPoint(local);

        void OnDestroy() { if (Instance == this) Instance = null; }
    }

    /// <summary>Harita açıkken ara ara gür bir gök gürültüsü: ekran titremez, ışık çakmaz — yalnızca ses.</summary>
    public class MapThunder : MonoBehaviour
    {
        float _next;
        void OnEnable() { _next = Time.time + UnityEngine.Random.Range(5f, 9f); }
        void Update()
        {
            if (Time.time < _next) return;
            _next = Time.time + UnityEngine.Random.Range(12f, 22f);
            AudioManager.Instance?.Thunder(0f, false, true);
        }
    }

    /// <summary>Yol boyunca gidip gelen fenerli at arabası (uçlarda kısa mola).</summary>
    public class MapCarriage : MonoBehaviour
    {
        List<Vector3> _path; float[] _cum; float _len, _s, _speed, _wait; int _dir = 1;
        public void Init(List<Vector3> path, float speed, float startFrac)
        {
            _path = path; _speed = speed;
            _cum = new float[path.Count];
            for (int i = 1; i < path.Count; i++) _cum[i] = _cum[i - 1] + Vector3.Distance(path[i - 1], path[i]);
            _len = _cum[path.Count - 1];
            _s = Mathf.Clamp01(startFrac) * _len;
            Place();
        }
        Vector3 At(float s)
        {
            s = Mathf.Clamp(s, 0f, _len);
            int i = 1; while (i < _cum.Length - 1 && _cum[i] < s) i++;
            float seg = _cum[i] - _cum[i - 1];
            return Vector3.Lerp(_path[i - 1], _path[i], seg > 0f ? (s - _cum[i - 1]) / seg : 0f);
        }
        void Place()
        {
            var p = At(_s); var q = At(_s + _dir * 0.3f);
            transform.localPosition = p;
            var d = q - p; d.y *= 0.5f;
            if (d.sqrMagnitude > 1e-5f) transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.LookRotation(d), 0.25f);
        }
        void Update()
        {
            if (_path == null || _len <= 0f) return;
            if (_wait > 0f) { _wait -= Time.deltaTime; return; }
            _s += _dir * _speed * Time.deltaTime;
            if (_s >= _len) { _s = _len; _dir = -1; _wait = 3f; }
            else if (_s <= 0f) { _s = 0f; _dir = 1; _wait = 3f; }
            Place();
        }
    }

    /// <summary>Daire çizerek süzülen, kanat çırpan kargalar.</summary>
    public class MapCrows : MonoBehaviour
    {
        List<(Transform l, Transform r, Transform bird, float phase, float radius, float speed, float h)> _b;
        public void Init(List<(Transform l, Transform r, Transform bird, float phase, float radius, float speed, float h)> birds) => _b = birds;
        void Update()
        {
            if (_b == null) return;
            float t = Time.time;
            foreach (var b in _b)
            {
                float a = b.phase + t * b.speed;
                b.bird.localPosition = new Vector3(Mathf.Cos(a) * b.radius, b.h + Mathf.Sin(t * 0.7f + b.phase) * 0.25f, Mathf.Sin(a) * b.radius);
                b.bird.localRotation = Quaternion.LookRotation(new Vector3(-Mathf.Sin(a), 0f, Mathf.Cos(a))) * Quaternion.Euler(0, 0, 18f);
                float flap = Mathf.Sin(t * 9f + b.phase * 3f) * 28f;
                b.l.localRotation = Quaternion.Euler(0, 0, flap);
                b.r.localRotation = Quaternion.Euler(0, 0, -flap);
            }
        }
    }
}
