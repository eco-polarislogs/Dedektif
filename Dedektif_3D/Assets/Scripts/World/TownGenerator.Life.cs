using System.Collections.Generic;
using Dedektif3D.Core;
using UnityEngine;
using static Dedektif3D.World.Shapes;
using M = Dedektif3D.World.MaterialLibrary;

namespace Dedektif3D.World
{
    /// <summary>
    /// YAŞAYAN KASABA — ana binaların ve şüphelilerin dışında kasabayı dolduran hayat:
    ///  • sokak kenarlarına isimsiz evler (kasabanın mimarisine uygun),
    ///  • günlük hayat sahneleri: sohbet eden komşular, bankta çay içen yaşlılar, arabasının başındaki seyyar satıcı,
    ///    odun kıran köylü, kovalamaca oynayan çocuklar, sandık taşıyan hamal, duvara yaslanıp bekleyen adam,
    ///  • olay yeri şeridinin çevresinde toplanan meraklılar (şeridi geçemezler — yalnızca dedektif girebilir),
    ///  • daha fazla yürüyen kasabalı.
    /// Yerleşim kasabanın tohumuyla sabittir (her oyunda aynı kasaba).
    /// </summary>
    public partial class TownGenerator
    {
        void PopulateLife(Transform town, TownData data)
        {
            var life = Group("KasabaHayatı", town, Vector3.zero).transform;
            bool rural = data.Id != "gizemli";
            var pts = new List<Vector3>(WalkPoints);
            if (pts.Count < 2) return;
            // Sisören: yalnızca yamaç dibindeki düzlükte ev / sahne kur (teras kenarlarından düşmesin)
            var groundPts = data.Id == "sisoren" ? pts.FindAll(x => x.y < 0.6f) : pts;
            if (groundPts.Count < 2) groundPts = pts;

            // ---- Ek evler: yolların iki yanına (Gölgeşehir / Sisören seyrek; Gizemli zaten sık) ----
            int houses = data.Id == "golge_sehir" ? 12 : data.Id == "sisoren" ? 6 : 4;
            int placed = 0;
            for (int tries = 0; tries < 120 && placed < houses; tries++)
            {
                int i = Random.Range(0, groundPts.Count - 1);
                var a = groundPts[i]; var b = groundPts[i + 1];
                var dir = b - a; dir.y = 0; if (dir.sqrMagnitude < 0.5f) continue;
                dir.Normalize();
                var side = Vector3.Cross(Vector3.up, dir) * (Random.value > 0.5f ? 1f : -1f);
                var p = Vector3.Lerp(a, b, Random.value) + side * Random.Range(11f, 15f);
                p.y = a.y - 0.05f;
                if (!Free(p, 9.5f) || Mathf.Abs(p.x) > 60f || p.z < -54f || p.z > 58f) continue;
                if (Vector3.Distance(new Vector3(p.x, 0, p.z), new Vector3(CrimeScene.x, 0, CrimeScene.z)) < 12f) continue;
                float yaw = Mathf.Atan2(side.x, side.z) * Mathf.Rad2Deg;   // ön cephe (-Z yerel) yola baksın
                if (rural)
                {
                    var def = new BuildingDef { Key = "kasabali_evi", NpcId = placed, DisplayName = "", Style = BuildingStyle.Rural, Neon = new Color(1f, 0.72f, 0.42f) };
                    def.Size = RuralBuildings.SizeFor(def.Key);
                    DecorBuildings.Add(RuralBuildings.Build(def, life, p, yaw));
                }
                else
                {
                    var size = new Vector3(Random.Range(5.5f, 7.5f), Random.Range(4.8f, 6.4f), Random.Range(5.5f, 6.8f));
                    if (Random.value > 0.5f) TownProps.OttomanHouse(life, p, yaw, size, true); else TownProps.TimberHouse(life, p, yaw, size, true);
                }
                _occupied.Add(p);
                placed++;
            }

            // ---- Günlük hayat sahneleri (yol kenarında, yürüyüş yolunu kapatmadan) ----
            var kinds = rural
                ? new[] { "chat", "bench", "vendor", "chop", "kids", "lean", "chat", "porter", "bench" }
                : new[] { "chat", "bench", "vendor", "kids", "lean", "chat", "porter", "bench" };
            int k = 0;
            for (int tries = 0; tries < 200 && k < kinds.Length; tries++)
            {
                int i = Random.Range(0, groundPts.Count - 1);
                var a = groundPts[i]; var b = groundPts[i + 1];
                var dir = b - a; dir.y = 0; if (dir.sqrMagnitude < 0.5f) continue;
                dir.Normalize();
                var side = Vector3.Cross(Vector3.up, dir) * (Random.value > 0.5f ? 1f : -1f);
                var p = Vector3.Lerp(a, b, Random.value) + side * Random.Range(4.2f, 6f);
                p.y = a.y;
                if (!Free(p, 4.5f)) continue;
                if (Vector3.Distance(new Vector3(p.x, 0, p.z), new Vector3(CrimeScene.x, 0, CrimeScene.z)) < 7f) continue;
                // Sahne yola baksın
                float yaw = Mathf.Atan2(-side.x, -side.z) * Mathf.Rad2Deg;
                Vignette(life, kinds[k], p, yaw, data);
                _occupied.Add(p);
                k++;
            }

            // ---- Olay yeri meraklıları (kırsal kasabalarda; Gizemli'de zaten kalabalık var) ----
            if (rural)
            {
                var watchers = Group("Meraklılar", life, Vector3.zero).transform;
                var focus = new GameObject("OlayYeriOdağı").transform; focus.SetParent(watchers, false); focus.position = CrimeScene + Vector3.up * 0.6f;
                for (int i = 0; i < 5; i++)
                {
                    float ang = -60f + i * 30f + Random.Range(-8f, 8f);
                    var p = CrimeScene + Quaternion.Euler(0, ang + 180f, 0) * new Vector3(0, 0, 5.2f + Random.Range(0f, 1.2f));
                    p.y = CrimeScene.y - 0.05f;
                    var rig = Person(watchers, p, null);
                    var to = CrimeScene - p; to.y = 0;
                    if (to.sqrMagnitude > 0.01f) rig.Root.rotation = Quaternion.LookRotation(to);
                    if (rig.Human != null) rig.Human.LookTarget = focus;
                }
            }

            // ---- Daha fazla yürüyen kasabalı ----
            int extra = data.Id == "golge_sehir" ? 8 : data.Id == "sisoren" ? 6 : 4;
            var walkers = Group("EkKasabalılar", life, Vector3.zero).transform;
            for (int i = 0; i < extra; i++) Townsfolk.CreateWalker(walkers, pts, (i * 7 + 3) % pts.Count);
        }

        static PropFactory.PersonRig Person(Transform parent, Vector3 pos, string pose, string name = "Kasabalı")
        {
            var face = FaceLibrary.NextTownsfolkFace();
            var info = FaceLibrary.Info(face);
            var coat = info != null ? FaceLibrary.Readable(info.Coat, 0.1f, 0.4f) : new Color(0.2f, 0.18f, 0.16f);
            return PropFactory.BuildPerson(name, parent, pos, coat, new Color(0.2f, 0.18f, 0.15f), info != null && info.Female, null, face, pose);
        }

        void Vignette(Transform parent, string kind, Vector3 p, float yaw, TownData data)
        {
            // Not: BuildPerson konumu ebeveyne göre YEREL alır; sahne kişileri g'nin yerel uzayında verilir.
            // Sahnenin +Z ekseni yola bakar (insanlar yoldan geçenleri görür).
            var g = Group("Sahne_" + kind, parent, p, yaw).transform;
            switch (kind)
            {
                case "chat":
                {
                    // 2-3 komşu yüz yüze sohbet ediyor: sırayla konuşur, el kol hareketi yapar, onaylar
                    int n = Random.value > 0.5f ? 3 : 2;
                    var rigs = new List<PropFactory.PersonRig>();
                    for (int i = 0; i < n; i++)
                    {
                        float a = i / (float)n * 360f + 20f;
                        var pos = g.position + Quaternion.Euler(0, yaw + a, 0) * new Vector3(0, 0, 0.75f);
                        var rig = Person(g, g.InverseTransformPoint(pos), null);
                        var to = g.position - pos; to.y = 0; rig.Root.rotation = Quaternion.LookRotation(to);
                        rigs.Add(rig);
                    }
                    g.gameObject.AddComponent<Chatter>().Init(rigs);
                    break;
                }
                case "bench":
                {
                    // Bankta çay içen yaşlılar
                    PropFactory.Bench(g, Vector3.zero, 180f);   // arkalık -Z'de, oturanlar yola bakar
                    for (int i = 0; i < 2; i++)
                    {
                        var rig = Person(g, new Vector3(i == 0 ? -0.45f : 0.45f, 0f, 0.05f), "sit", "Müşteri");
                        rig.Root.rotation = Quaternion.Euler(0, yaw, 0);
                    }
                    Cyl("ÇayBardağı", g, new Vector3(0.05f, 0.56f, 0.05f), new Vector3(0.05f, 0.05f, 0.05f), M.Get("tea", new Color(0.6f, 0.15f, 0.05f), 0.9f), null, false);
                    break;
                }
                case "vendor":
                {
                    // Seyyar satıcı: arabası, feneri, kollarını kavuşturmuş bekliyor
                    RuralBuildings.Cart(g, new Vector3(0, 0, 0.9f), 90f);
                    for (int i = 0; i < 5; i++) PropFactory.Crate(g, new Vector3(-0.6f + i * 0.3f, 1.0f, 0.9f), 0.25f);
                    var rig = Person(g, new Vector3(0.4f, 0f, -0.4f), null);
                    rig.Root.rotation = Quaternion.Euler(0, yaw, 0);
                    rig.Human?.UseIdle("idle_arms");
                    var lamp = PointLight("SatıcıFeneri", g, new Vector3(-0.9f, 1.6f, 0.9f), new Color(1f, 0.7f, 0.4f), 1.4f, 5f);
                    lamp.gameObject.AddComponent<LightFlicker>().Init(lamp, 0.15f);
                    lamp.gameObject.AddComponent<LanternByNight>();
                    Box("SatıcıFeneriCam", g, new Vector3(-0.9f, 1.6f, 0.9f), new Vector3(0.14f, 0.2f, 0.14f), M.Emissive("lantern_glow", new Color(1f, 0.7f, 0.35f), 2f), null, false);
                    break;
                }
                case "chop":
                {
                    // Odun kıran köylü: kütük, balta, istif
                    Cyl("Kütük", g, new Vector3(0, 0.25f, 0.6f), new Vector3(0.5f, 0.25f, 0.5f), M.Wood(new Color(0.35f, 0.24f, 0.14f)), null, true);
                    for (int i = 0; i < 6; i++)
                        Cyl("İstif", g, new Vector3(1.2f, 0.12f + (i / 3) * 0.22f, -0.3f + (i % 3) * 0.25f), new Vector3(0.2f, 0.45f, 0.2f), M.Wood(new Color(0.32f, 0.22f, 0.12f)), new Vector3(0, 0, 90), false);
                    var rig = Person(g, Vector3.zero, "chop", "Müşteri");
                    rig.Root.rotation = Quaternion.Euler(0, yaw, 0);     // kütük önünde
                    break;
                }
                case "kids":
                {
                    // Kovalamaca oynayan çocuklar (gündüz)
                    var rigs = new List<PropFactory.PersonRig>();
                    for (int i = 0; i < 2; i++)
                    {
                        var rig = PropFactory.BuildPerson("Kasabalı", g, new Vector3(0, 0, 0), new Color(0.3f, 0.25f, 0.2f), Color.gray, i == 1, null, "cocuk_" + i + "_" + (int)(p.x * 10 + p.z));
                        rigs.Add(rig);
                    }
                    g.gameObject.AddComponent<ChildPlay>().Init(rigs, 2.2f);
                    break;
                }
                case "lean":
                {
                    // Çit dibinde yaslanıp yoldan geçenleri izleyen adam
                    PropFactory.FencePost(g, new Vector3(0, 0, -0.35f), 0f, 3f);   // sırtını direğe yaslar
                    var rig = Person(g, Vector3.zero, "lean", "Müşteri");
                    rig.Root.rotation = Quaternion.Euler(0, yaw, 0);
                    break;
                }
                case "porter":
                {
                    // Hamal: sandığı kollarında, yol boyunca taşıyor
                    var t = Townsfolk.CreateWalker(g, new List<Vector3>(WalkPoints), Random.Range(0, WalkPoints.Count));
                    var h = t.GetComponentInChildren<HumanAnimator>();
                    if (h != null && h.HoldWithArms("carry") && h.ChestAnchor != null)
                        PropFactory.Crate(h.ChestAnchor, new Vector3(0, -0.28f, 0.32f), 0.42f);
                    t.speed = 0.9f;
                    break;
                }
            }
        }
    }

    /// <summary>Sohbet eden grup: konuşmacı sırayla değişir, dinleyenler ona bakar ve arada onaylar.</summary>
    public class Chatter : MonoBehaviour
    {
        List<PropFactory.PersonRig> _rigs; int _speaker; float _next; readonly List<Transform> _heads = new List<Transform>();
        public void Init(List<PropFactory.PersonRig> rigs)
        {
            _rigs = rigs;
            foreach (var r in rigs) _heads.Add(r.Human != null ? r.Human.Head : r.Head);
            _next = Time.time + Random.Range(0.5f, 3f);
        }
        void Update()
        {
            if (_rigs == null || Time.time < _next) return;
            _next = Time.time + Random.Range(3f, 7f);
            _speaker = (_speaker + 1 + Random.Range(0, _rigs.Count - 1)) % _rigs.Count;
            for (int i = 0; i < _rigs.Count; i++)
            {
                var h = _rigs[i].Human; if (h == null) continue;
                h.Talking = i == _speaker;
                h.LookTarget = i == _speaker ? _heads[(i + 1) % _heads.Count] : _heads[_speaker];
                if (i != _speaker && Random.value < 0.25f) h.Gesture(Random.value < 0.8f ? "yes" : "no");
            }
        }
    }

    /// <summary>Kovalamaca: çocuklar küçük bir çember çevresinde koşar (gece evdedirler).</summary>
    public class ChildPlay : MonoBehaviour
    {
        List<PropFactory.PersonRig> _rigs; float _r, _a, _speed = 1f;
        public void Init(List<PropFactory.PersonRig> rigs, float radius) { _rigs = rigs; _r = radius; _a = Random.value * 6f; }
        void Update()
        {
            if (_rigs == null) return;
            var dn = DayNightCycle.Instance;
            bool day = dn == null || !dn.IsNight;
            foreach (var r in _rigs) if (r.Root.gameObject.activeSelf != day) r.Root.gameObject.SetActive(day);
            if (!day) return;
            _speed = Mathf.Lerp(_speed, Mathf.PerlinNoise(Time.time * 0.2f, transform.position.x) > 0.45f ? 1.6f : 0f, Time.deltaTime);
            _a += Time.deltaTime * _speed;
            for (int i = 0; i < _rigs.Count; i++)
            {
                float a = _a - i * 0.9f;
                var p = transform.position + new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a)) * _r;
                var tangent = new Vector3(Mathf.Cos(a), 0, -Mathf.Sin(a));
                _rigs[i].Root.position = p;
                if (_speed > 0.1f) _rigs[i].Root.rotation = Quaternion.LookRotation(tangent);
            }
        }
    }
}
