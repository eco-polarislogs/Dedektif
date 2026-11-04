using System.Collections.Generic;
using UnityEngine;
using static Dedektif3D.World.Shapes;
using M = Dedektif3D.World.MaterialLibrary;

namespace Dedektif3D.World
{
    /// <summary>
    /// Kasabayı canlı tutan şemsiyeli kasabalılar (town_bg.png'deki yağmurluklu, şemsiyeli kalabalık).
    /// Yürüyenler çevre yolu ve meydan arasında rastgele güzergahta dolaşır; duranlar olay yerini seyreder.
    /// </summary>
    public class Townsfolk : MonoBehaviour
    {
        static readonly Color[] Coats =
        {
            new Color(0.12f, 0.13f, 0.16f), new Color(0.22f, 0.2f, 0.17f), new Color(0.15f, 0.22f, 0.3f),
            new Color(0.3f, 0.28f, 0.12f), new Color(0.25f, 0.12f, 0.12f), new Color(0.18f, 0.2f, 0.18f)
        };
        static readonly Color[] Umbrellas =
        {
            new Color(0.05f, 0.05f, 0.06f), new Color(0.35f, 0.05f, 0.07f), new Color(0.1f, 0.15f, 0.35f), new Color(0.2f, 0.25f, 0.2f)
        };

        /// <summary>Sokakta yürüyen tüm kasabalılar (gün/gece kalabalık ayarı için).</summary>
        public static readonly List<Townsfolk> All = new List<Townsfolk>();

        public List<Vector3> route;
        public float speed = 1.3f;
        PropFactory.PersonRig _rig;
        int _target;
        float _cycle, _pause;
        CharacterController _cc;

        public static GameObject CreateStanding(Transform parent, Vector3 pos, bool umbrella)
        {
            var rig = MakePerson(parent, pos, umbrella);
            rig.ArmL.localRotation = Quaternion.Euler(-10, 0, 8);
            var idle = rig.Root.gameObject.AddComponent<IdleSway>();
            idle.rig = rig;
            return rig.Root.gameObject;
        }

        public static Townsfolk CreateWalker(Transform parent, List<Vector3> route, int startIndex)
        {
            var rig = MakePerson(parent, route[startIndex % route.Count], Random.value > 0.25f);
            var go = rig.Root.gameObject;
            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.8f; cc.radius = 0.3f; cc.center = new Vector3(0, 0.9f, 0);
            var t = go.AddComponent<Townsfolk>();
            t.route = route; t._rig = rig; t._target = (startIndex + 1) % route.Count; t._cc = cc;
            t.speed = Random.Range(1.0f, 1.6f);
            All.Add(t);
            return t;
        }

        static PropFactory.PersonRig MakePerson(Transform parent, Vector3 pos, bool umbrella)
        {
            // Her kasabalının yüzü Sisören kasabalı portrelerinden (npc_301-318) — sokakta gerçek insanlar dolaşır
            var faceKey = Core.FaceLibrary.NextTownsfolkFace();
            var info = Core.FaceLibrary.Info(faceKey);
            bool female = info != null ? info.Female : Random.value > 0.5f;
            var coat = info != null ? Core.FaceLibrary.Readable(info.Coat, 0.1f, 0.4f) : Coats[Random.Range(0, Coats.Length)];
            var rig = PropFactory.BuildPerson("Kasabalı", parent, pos, coat, new Color(0.2f, 0.18f, 0.15f), female, null, faceKey);
            if (rig.HasModel)
            {
                // Elinde portresindeki eşya (baston, değnek...) varsa şemsiye / fener taşımaz
                bool handFree = rig.Look == null || string.IsNullOrEmpty(rig.Look.Prop);
                bool rainy = Core.GameData.Current == null || Core.GameData.Current.RainIntensity > 0.05f;
                if (handFree) { if (umbrella && rainy) CarryUmbrella(rig); else if (Random.value < 0.4f) CarryLantern(rig); }
                return rig;
            }
            if (info != null && info.Child) rig.Root.localScale = Vector3.one * 0.72f;
            // Uzun yağmurluk eteği
            Cyl("Yağmurluk", rig.Body, new Vector3(0, 0.8f, 0), new Vector3(0.5f, 0.28f, 0.38f), M.Get("raincoat_" + Random.Range(0, 4), Coats[Random.Range(0, Coats.Length)], 0.6f), null, false);
            if (umbrella)
            {
                // Şemsiye başın üstünde: sap elden yukarı dik çıkar, kubbe başı örter
                rig.ArmR.localRotation = Quaternion.Euler(-40, 0, -12);
                var u = Group("Şemsiye", rig.Body, new Vector3(0.18f, 1.2f, 0.3f)).transform;
                var cloth = M.Get("umbrella_" + Random.Range(0, Umbrellas.Length), Umbrellas[Random.Range(0, Umbrellas.Length)], 0.85f);
                Cyl("Sap", u, new Vector3(0, 0.55f, 0), new Vector3(0.025f, 0.55f, 0.025f), M.Metal, null, false);
                Box("Kulp", u, new Vector3(0, 0.02f, 0.04f), new Vector3(0.03f, 0.04f, 0.1f), M.Wood(new Color(0.3f, 0.18f, 0.1f)), null, false);
                Sphere("Kubbe", u, new Vector3(-0.12f, 1.12f, -0.22f), new Vector3(1.2f, 0.34f, 1.2f), cloth);
                Cyl("Uç", u, new Vector3(-0.12f, 1.32f, -0.22f), new Vector3(0.02f, 0.08f, 0.02f), M.Metal, null, false);
            }
            else
            {
                // Şapka (fötr veya kasket)
                if (rig.HasFace) { /* başlık portrede */ }
                else if (female) Sphere("Bone", rig.Head, new Vector3(0, 0.08f, -0.03f), new Vector3(0.36f, 0.3f, 0.36f), M.Get("bonnet", new Color(0.25f, 0.2f, 0.22f), 0.3f));
                else TownProps.TopHat(rig.Head);
            }
            InteriorGenerator.SetLayerRecursive(rig.Root.gameObject, 0);
            return rig;
        }

        /// <summary>Gerçek model: sağ kol fener tutma pozunda, elde dik duran şemsiye (gövde yürümeye devam eder).</summary>
        static void CarryUmbrella(PropFactory.PersonRig rig)
        {
            if (rig.Human == null || rig.Human.HandAnchor == null || !rig.Human.HoldWithRightArm("lantern")) return;
            var u = Group("Şemsiye", rig.Human.HandAnchor, Vector3.zero).transform;
            u.gameObject.AddComponent<KeepUpright>().facing = rig.Root;
            var cloth = M.Get("umbrella_" + Random.Range(0, Umbrellas.Length), Umbrellas[Random.Range(0, Umbrellas.Length)], 0.85f);
            Cyl("Sap", u, new Vector3(0, 0.55f, 0), new Vector3(0.022f, 0.55f, 0.022f), M.Metal, null, false);
            Box("Kulp", u, new Vector3(0, -0.02f, 0.03f), new Vector3(0.03f, 0.06f, 0.08f), M.Wood(new Color(0.3f, 0.18f, 0.1f)), null, false);
            Sphere("Kubbe", u, new Vector3(0, 1.08f, -0.08f), new Vector3(1.15f, 0.32f, 1.15f), cloth);
            Cyl("Uç", u, new Vector3(0, 1.28f, -0.08f), new Vector3(0.02f, 0.07f, 0.02f), M.Metal, null, false);
        }

        static int _lanternLights;

        /// <summary>Gerçek model: elde gaz/yağ feneri (gece yanar). Performans için yalnızca ilk birkaçı gerçek ışık yayar.</summary>
        static void CarryLantern(PropFactory.PersonRig rig)
        {
            if (rig.Human == null || rig.Human.HandAnchor == null || !rig.Human.HoldWithRightArm("lantern")) return;
            var l = Group("ElFeneri", rig.Human.HandAnchor, Vector3.zero).transform;
            l.gameObject.AddComponent<KeepUpright>().facing = rig.Root;
            var iron = M.Get("lantern_iron", new Color(0.08f, 0.08f, 0.08f), 0.5f, 0.8f);
            Box("Kulp", l, new Vector3(0, -0.03f, 0), new Vector3(0.012f, 0.07f, 0.012f), iron, null, false);
            Cyl("Kapak", l, new Vector3(0, -0.08f, 0), new Vector3(0.11f, 0.02f, 0.11f), iron, null, false);
            Cyl("Cam", l, new Vector3(0, -0.17f, 0), new Vector3(0.09f, 0.07f, 0.09f), M.Emissive("lantern_glow", new Color(1f, 0.7f, 0.35f), 2.2f), null, false);
            Cyl("Taban", l, new Vector3(0, -0.25f, 0), new Vector3(0.11f, 0.015f, 0.11f), iron, null, false);
            if (_lanternLights < 6)
            {
                _lanternLights++;
                var li = PointLight("FenerIşığı", l, new Vector3(0, -0.17f, 0), new Color(1f, 0.66f, 0.3f), 1.6f, 5f);
                li.gameObject.AddComponent<LightFlicker>().Init(li, 0.12f);
                li.gameObject.AddComponent<LanternByNight>();
            }
        }

        void Update()
        {
            if (route == null || route.Count < 2 || _cc == null) return;
            if (_pause > 0f) { _pause -= Time.deltaTime; Animate(0f); Gravity(); return; }
            var target = route[_target];
            var to = target - transform.position; to.y = 0;
            if (to.magnitude < 0.8f)
            {
                _target = Random.value > 0.85f ? Random.Range(0, route.Count) : (_target + 1) % route.Count;
                if (Random.value > 0.7f) _pause = Random.Range(1.5f, 4f);
                return;
            }
            var dir = to.normalized;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 4f);
            _cc.Move((transform.forward * speed + Vector3.down * 4f) * Time.deltaTime);
            Animate(speed);
        }

        void Gravity() { if (!_cc.isGrounded) _cc.Move(Vector3.down * 4f * Time.deltaTime); }

        void Animate(float spd)
        {
            if (_rig == null) return;
            _cycle += Time.deltaTime * (3f + spd * 2.5f);
            float swing = Mathf.Sin(_cycle) * 28f * Mathf.Clamp01(spd);
            _rig.LegL.localRotation = Quaternion.Euler(swing, 0, 0);
            _rig.LegR.localRotation = Quaternion.Euler(-swing, 0, 0);
            _rig.ArmL.localRotation = Quaternion.Euler(-swing * 0.6f, 0, 0);
        }
    }

    /// <summary>Elde taşınan fener: sokak lambalarıyla birlikte akşam yanar, sabah söner.</summary>
    public class LanternByNight : MonoBehaviour
    {
        Light _l; LightFlicker _f;
        void Start() { _l = GetComponent<Light>(); _f = GetComponent<LightFlicker>(); }
        void Update()
        {
            var dn = Core.DayNightCycle.Instance;
            float k = dn != null ? dn.LampFactor : 1f;
            if (_f != null) _f.Multiplier = k;
            if (_l != null) _l.enabled = k > 0.02f;
        }
    }

    /// <summary>Duran kalabalığın hafif sallanması / kafa çevirmesi.</summary>
    public class IdleSway : MonoBehaviour
    {
        public PropFactory.PersonRig rig;
        float _seed;
        void Start() => _seed = Random.value * 10f;
        void Update()
        {
            if (rig == null || rig.HasModel) return;   // gerçek model: HumanAnimator nefes alır / bakınır
            rig.Body.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 0.8f + _seed) * 1.5f);
            rig.Head.localRotation = Quaternion.Euler(Mathf.Sin(Time.time * 0.3f + _seed) * 5f, Mathf.Sin(Time.time * 0.25f + _seed * 2f) * 25f, 0);
        }
    }
}
