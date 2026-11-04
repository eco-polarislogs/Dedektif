using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Dedektif3D.World
{
    /// <summary>
    /// GERÇEK İNSAN MODELLERİ — Assets/Resources/Humans:
    ///
    ///   Parts/       → Quaternius (CC0) modüler insan kiti: base_male/base_female (yalnızca BAŞ kullanılır),
    ///                  köylü / korucu kıyafet parçaları (Body, Arms, Legs, Feet) ve saçlar. Hepsi aynı iskelette;
    ///                  her NPC için portre anahtarından sabit (her oyunda aynı) bir kombinasyon montajlanır:
    ///                  gövde + kollar + bacak + ayakkabı + baş + saç (+ sakal) — kıyafet dokusu ve rengi kişiye göre.
    ///   Characters/  → (isteğe bağlı) hazır Humanoid karakterler (Mixamo vb.). Varsa montajın yerine bunlar kullanılır;
    ///                  dosya adı portre anahtarıyla aynıysa (dedektif, hasan, npc_201...) o kişiye verilir.
    ///   Animations/  → Humanoid animasyonlar. Klip adı "tür_çeşit" (idle_1, walk_formal, sit_talk) ya da Mixamo dosya adı
    ///                  (idle / walk / run / talk / sit / kneel ...). İçe aktarma: Editor/HumanImport.cs.
    ///
    /// Hiç model yoksa oyun eski primitive figürlerle çalışır.
    /// </summary>
    public static class HumanLibrary
    {
        class Entry { public GameObject Prefab; public string Name; public bool Female, Child; }

        static List<Entry> _chars;
        static Dictionary<string, List<AnimationClip>> _clips;
        static Dictionary<string, GameObject> _parts;

        public static bool Available { get { Load(); return (_chars.Count > 0 || KitReady) && Clip("idle", 0) != null; } }
        public static int CharacterCount { get { Load(); return _chars.Count + (KitReady ? 1 : 0); } }
        public static int ClipCount { get { Load(); return _clips.Values.Sum(l => l.Count); } }
        static bool KitReady => _parts.ContainsKey("base_male") && _parts.ContainsKey("male_peasant_body");

        /// <summary>Mixamo tarzı tek klipli dosyalar için anahtar kelimeler (kütüphane klipleri "tür_" önekiyle gelir).</summary>
        static readonly Dictionary<string, string[]> Kinds = new Dictionary<string, string[]>
        {
            { "idle", new[] { "idle", "bekle", "standing", "breathing" } },
            { "walk", new[] { "walk", "yuru", "yürü" } },
            { "run", new[] { "run", "kos", "koş", "jog" } },
            { "talk", new[] { "talk", "konus", "konuş", "argu", "explain", "convers" } },
            { "sit", new[] { "sit", "otur" } },
            { "kneel", new[] { "kneel", "diz" } },
            { "crouch", new[] { "crouch", "squat" } },
            { "look", new[] { "look", "bak" } },
            { "lantern", new[] { "lantern", "fener" } },
            { "yes", new[] { "nod", "yes" } },
            { "no", new[] { "shake", "no" } },
            { "lean", new[] { "lean", "rail" } },
            { "drink", new[] { "drink", "consume" } },
            { "carry", new[] { "carry" } },
            { "chop", new[] { "chop" } },
            { "push", new[] { "push" } },
            { "interact", new[] { "interact" } },
            { "pickup", new[] { "pickup", "pick up" } },
            { "dead", new[] { "dead", "death", "dying" } },
        };

        static bool Has(string s, params string[] keys) { foreach (var k in keys) if (s.Contains(k)) return true; return false; }

        static void Load()
        {
            if (_chars != null) return;
            _chars = new List<Entry>();
            _clips = new Dictionary<string, List<AnimationClip>>();
            _parts = new Dictionary<string, GameObject>();
            foreach (var k in Kinds.Keys) _clips[k] = new List<AnimationClip>();
            foreach (var go in Resources.LoadAll<GameObject>("Humans/Characters"))
            {
                var anim = go.GetComponentInChildren<Animator>(true);
                if (anim == null || anim.avatar == null || !anim.avatar.isHuman) { Debug.LogWarning($"[Dedektif3D] '{go.name}' Humanoid değil — atlandı (Rig: Humanoid olmalı)."); continue; }
                string n = go.name.ToLowerInvariant();
                _chars.Add(new Entry
                {
                    Prefab = go, Name = n,
                    Female = Has(n, "female", "kadin", "kadın", "woman", "girl", "kiz", "kız", "lady"),
                    Child = Has(n, "child", "cocuk", "çocuk", "kid", "boy", "girl", "oglan", "oğlan")
                });
            }
            foreach (var go in Resources.LoadAll<GameObject>("Humans/Parts")) _parts[go.name.ToLowerInvariant()] = go;
            foreach (var c in Resources.LoadAll<AnimationClip>("Humans/Animations"))
            {
                if (c.name.StartsWith("__preview")) continue;
                string n = c.name.ToLowerInvariant();
                int us = n.IndexOf('_');
                if (us > 0 && _clips.TryGetValue(n.Substring(0, us), out var byPrefix)) { byPrefix.Add(c); continue; }
                foreach (var kv in Kinds) if (Has(n, kv.Value)) { _clips[kv.Key].Add(c); break; }
            }
            Debug.Log($"[Dedektif3D] İnsan modelleri: {_chars.Count} hazır karakter, montaj kiti {(KitReady ? "VAR (" + _parts.Count + " parça)" : "yok")}, " +
                      $"{_clips.Values.Sum(l => l.Count)} animasyon (idle {_clips["idle"].Count}, walk {_clips["walk"].Count}, talk {_clips["talk"].Count}, sit {_clips["sit"].Count}).");
        }

        public static AnimationClip ClipNamed(string name)
        {
            Load();
            foreach (var l in _clips.Values) foreach (var c in l) if (c.name == name) return c;
            return null;
        }

        public static AnimationClip Clip(string kind, int seed)
        {
            Load();
            if (string.IsNullOrEmpty(kind) || !_clips.TryGetValue(kind, out var l) || l.Count == 0) return null;
            return l[Mathf.Abs(seed) % l.Count];
        }

        public static int Hash(string s) { unchecked { int h = 23; foreach (var ch in s ?? "") h = h * 31 + ch; return h & 0x7fffffff; } }

        static Entry Pick(string key, bool female, bool child)
        {
            Load();
            if (_chars.Count == 0) return null;
            if (!string.IsNullOrEmpty(key))
            {
                var exact = _chars.Find(e => e.Name == key.ToLowerInvariant());
                if (exact != null) return exact;
            }
            var pool = _chars.Where(e => e.Female == female && e.Child == child).ToList();
            if (pool.Count == 0) pool = _chars.Where(e => e.Female == female).ToList();
            if (pool.Count == 0) pool = _chars;
            // Portre anahtarına özel adlandırılmış modeller rastgele dağıtılmaz
            var generic = pool.Where(e => !e.Name.StartsWith("npc_") && e.Name != "dedektif" && e.Name != "dedektif_helper").ToList();
            if (generic.Count > 0) pool = generic;
            return pool[Hash(key ?? Random.value.ToString()) % pool.Count];
        }

        /// <summary>Kişiye özel görünüm: portreden gelen renkler (yoksa anahtardan türetilir).</summary>
        public class Look
        {
            public Color? Clothes, Skin, Hair;
            /// <summary>Kıyafet tarzı zorlaması: "peasant", "ranger" ya da null (anahtara göre).</summary>
            public string Outfit;
            public bool NoBeard;
            /// <summary>Başlık takılacak: saç kısa / toplu seçilir (şapkanın içinden taşmasın).</summary>
            public bool Covered;
            /// <summary>Portreden çıkarılmış görünüm tarifi (CharacterLooks).</summary>
            public CharLook Spec;
        }

        /// <summary>Modeli parent altına kurar; ayakları zeminde, boyu hedef yükseklikte. Başarısızsa null.</summary>
        public static HumanAnimator Spawn(Transform parent, string key, bool female, bool child, string pose, Look look = null)
        {
            Load();
            if (Clip("idle", 0) == null) return null;
            key = string.IsNullOrEmpty(key) ? "anon_" + Random.Range(0, 99999) : key;
            GameObject go = null;
            var e = Pick(key.Split('#')[0], female, child);
            if (e != null)
            {
                go = Object.Instantiate(e.Prefab, parent, false);
                go.name = "İnsan_" + e.Prefab.name;
            }
            else if (KitReady)
            {
                go = Assemble(parent, key, female, child, look ?? new Look());
            }
            if (go == null) return null;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            // Boy normalizasyonu (modellerin ölçekleri farklı olabilir)
            var rends = go.GetComponentsInChildren<Renderer>(true);
            if (rends.Length > 0)
            {
                var b = rends[0].bounds; foreach (var r in rends) b.Encapsulate(r.bounds);
                float target = look?.Spec != null && look.Spec.Height > 0.5f ? look.Spec.Height : child ? 1.3f : female ? 1.63f : 1.75f;
                float h = Mathf.Max(0.1f, b.size.y);
                float s = target / h * (0.97f + (Hash(key) % 7) * 0.01f);
                go.transform.localScale = go.transform.localScale * s;
                var b2 = rends[0].bounds; foreach (var r in rends) b2.Encapsulate(r.bounds);
                go.transform.position += Vector3.up * (parent.position.y - b2.min.y);
            }
            var anim = go.GetComponentInChildren<Animator>();
            foreach (var r in rends)
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                if (r is SkinnedMeshRenderer smr)
                {
                    smr.updateWhenOffscreen = false;
                    // Montajda kök kemik değişti: animasyonda kırpılmasın diye cömert sınır kutusu
                    var rb = smr.rootBone != null ? smr.rootBone : smr.transform;
                    float ls = Mathf.Max(1e-4f, rb.lossyScale.x);
                    smr.localBounds = new Bounds(smr.rootBone != null ? Vector3.zero : Vector3.up * (0.9f / ls), Vector3.one * (2.6f / ls));
                }
            }
            anim.applyRootMotion = false;
            anim.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            anim.runtimeAnimatorController = null;
            var ha = go.AddComponent<HumanAnimator>();
            ha.Init(anim, Hash(key), pose);
            ha.BuildAnchors(go.transform);
            var sp = look?.Spec;
            if (sp != null)
            {
                if (!string.IsNullOrEmpty(sp.Idle) && string.IsNullOrEmpty(pose)) ha.UseIdle(sp.Idle);
                if (sp.Old) ha.Stoop = Mathf.Lerp(5f, 15f, Mathf.InverseLerp(60f, 85f, sp.Age));
            }
            return ha;
        }

        // =================================================================
        //  MONTAJ (Quaternius modüler kit)
        // =================================================================
        static GameObject Part(string n) => _parts.TryGetValue(n.ToLowerInvariant(), out var p) ? p : null;

        static readonly HashSet<Headwear.Kind> Covering = new HashSet<Headwear.Kind> { Headwear.Kind.Fez, Headwear.Kind.FlatCap, Headwear.Kind.Bowler, Headwear.Kind.TopHat,
            Headwear.Kind.Fedora, Headwear.Kind.Beanie, Headwear.Kind.PostCap, Headwear.Kind.PoliceCap, Headwear.Kind.HeadWrap, Headwear.Kind.Headscarf };

        static GameObject Assemble(Transform parent, string key, bool female, bool child, Look look)
        {
            int h = Hash(key);
            var spec = look.Spec ?? CharacterLooks.Generic(h, "gizemli", female);
            female = spec.Female;
            string g = female ? "Female" : "Male";
            string S(string st) => st == "ranger" ? "Ranger" : "Peasant";
            string body = $"{g}_{S(spec.Body)}_Body", arms = $"{g}_{S(spec.Arms ?? spec.Body)}_Arms";
            string legs = $"{g}_{S(spec.Legs)}_Legs";
            string feet = spec.Feet == "ranger" ? (female ? "Female_Ranger_Feet" : "Male_Ranger_Feet_Boots") : $"{g}_Peasant_Feet";
            var baseP = Part(female ? "base_female" : "base_male") ?? Part("base_male");
            if (baseP == null) return null;

            // Ana iskelet: Humanoid avatarı olan ilk parça (bazı gövde dosyalarında avatar kurulamıyor)
            GameObject masterP = null;
            foreach (var cand in new[] { body, legs, feet, female ? "base_female" : "base_male" })
            {
                var pp = Part(cand); if (pp == null) continue;
                var a = pp.GetComponentInChildren<Animator>(true);
                if (a != null && a.avatar != null && a.avatar.isHuman) { masterP = pp; break; }
            }
            if (masterP == null) return null;
            var go = Object.Instantiate(masterP, parent, false);
            go.name = "İnsan_" + key;
            var anim = go.GetComponentInChildren<Animator>();
            if (masterP == baseP) foreach (var smr in go.GetComponentsInChildren<SkinnedMeshRenderer>(true)) { var hm = HeadOnly(smr.sharedMesh, smr.bones); if (hm != null) smr.sharedMesh = hm; }

            var bones = new Dictionary<string, Transform>();
            foreach (var t in go.GetComponentsInChildren<Transform>(true)) if (!bones.ContainsKey(t.name)) bones[t.name] = t;

            // Saç: başlık varsa şapkanın içinden taşmayan kısa / toplu saç
            bool covered = Covering.Contains(spec.Hat);
            string hairStyle = spec.Hair;
            if (covered && hairStyle == "long") hairStyle = female ? "buns" : "short";
            string hair = hairStyle == "short" ? "Hair_Buzzed" : hairStyle == "parted" ? (covered ? "Hair_Buzzed" : "Hair_SimpleParted") : hairStyle == "long" ? "Hair_Long" : hairStyle == "buns" ? "Hair_Buns" : null;
            bool beard = !female && !spec.Child && (spec.Beard == "full" || spec.Beard == "stubble");

            foreach (var pn in new[] { body, arms, legs, feet }) if (Part(pn) != masterP) Attach(Part(pn), go.transform, bones);
            if (masterP != baseP) Attach(baseP, go.transform, bones, headOnly: true);
            if (hair != null && spec.Hat != Headwear.Kind.Headscarf) Attach(Part(hair), go.transform, bones);
            if (beard) Attach(Part("Hair_Beard"), go.transform, bones);

            ApplyMaterials(go, spec, look);
            Garments.Dress(anim, go.transform, spec);
            return go;
        }

        /// <summary>Parçanın deri mesh'lerini ana iskelete (kemik adıyla) bağlar; parçanın kendi iskeleti atılır.</summary>
        static void Attach(GameObject prefab, Transform master, Dictionary<string, Transform> bones, bool headOnly = false)
        {
            if (prefab == null) return;
            var inst = Object.Instantiate(prefab);
            inst.SetActive(false);
            foreach (var smr in inst.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                var src = smr.bones;
                var nb = new Transform[src.Length];
                for (int i = 0; i < src.Length; i++)
                    nb[i] = src[i] != null && bones.TryGetValue(src[i].name, out var b) ? b : master;
                if (headOnly)
                {
                    var m = HeadOnly(smr.sharedMesh, src);
                    if (m == null) continue;   // göz / kaş gibi tamamen başa ait mesh'ler zaten tam kalır
                    smr.sharedMesh = m;
                }
                smr.transform.SetParent(master, false);
                smr.bones = nb;
                smr.rootBone = smr.rootBone != null && bones.TryGetValue(smr.rootBone.name, out var rb) ? rb : master;
                smr.gameObject.SetActive(true);
            }
            Object.Destroy(inst);
        }

        static readonly Dictionary<Mesh, Mesh> _headCache = new Dictionary<Mesh, Mesh>();

        /// <summary>
        /// Tam beden mesh'ten yalnızca baş + boyun üçgenlerini ayırır (kıyafet altında gövde çakışmasın).
        /// Mesh zaten tamamen başa aitse (göz, kaş) olduğu gibi döner.
        /// </summary>
        static Mesh HeadOnly(Mesh src, Transform[] bones)
        {
            if (src == null) return null;
            if (_headCache.TryGetValue(src, out var cached)) return cached;
            if (!src.isReadable) { _headCache[src] = src; return src; }
            var keepBone = new bool[bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                var n = bones[i] != null ? bones[i].name.ToLowerInvariant() : "";
                keepBone[i] = n == "head" || n == "neck_01" || n.StartsWith("head");
            }
            var bw = src.boneWeights;
            int vc = src.vertexCount;
            if (bw.Length != vc) { _headCache[src] = src; return src; }
            float W(int bi, float w) => bi >= 0 && bi < keepBone.Length && keepBone[bi] ? w : 0f;
            var vk = new bool[vc];
            int kept = 0;
            for (int v = 0; v < vc; v++)
            {
                var w = bw[v];
                vk[v] = W(w.boneIndex0, w.weight0) + W(w.boneIndex1, w.weight1) + W(w.boneIndex2, w.weight2) + W(w.boneIndex3, w.weight3) >= 0.5f;
                if (vk[v]) kept++;
            }
            if (kept > vc * 0.9f) { _headCache[src] = src; return src; }   // zaten baş mesh'i

            var remap = new int[vc]; for (int i = 0; i < vc; i++) remap[i] = -1;
            var order = new List<int>();
            var subTris = new List<int[]>();
            for (int s = 0; s < src.subMeshCount; s++)
            {
                var t = src.GetTriangles(s);
                var outT = new List<int>();
                for (int i = 0; i + 2 < t.Length; i += 3)
                {
                    if (!(vk[t[i]] || vk[t[i + 1]] || vk[t[i + 2]])) continue;
                    for (int k = 0; k < 3; k++)
                    {
                        int v = t[i + k];
                        if (remap[v] < 0) { remap[v] = order.Count; order.Add(v); }
                        outT.Add(remap[v]);
                    }
                }
                subTris.Add(outT.ToArray());
            }
            if (order.Count == 0) { _headCache[src] = null; return null; }

            T[] Pick<T>(T[] a) { if (a == null || a.Length != vc) return null; var r = new T[order.Count]; for (int i = 0; i < r.Length; i++) r[i] = a[order[i]]; return r; }
            var m = new Mesh { name = src.name + "_Baş" };
            if (order.Count > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            m.vertices = Pick(src.vertices);
            var nrm = Pick(src.normals); if (nrm != null) m.normals = nrm;
            var tan = Pick(src.tangents); if (tan != null) m.tangents = tan;
            var uv0 = Pick(src.uv); if (uv0 != null) m.uv = uv0;
            var uv1 = Pick(src.uv2); if (uv1 != null) m.uv2 = uv1;
            var col = Pick(src.colors); if (col != null) m.colors = col;
            m.boneWeights = Pick(bw);
            m.bindposes = src.bindposes;
            m.subMeshCount = subTris.Count;
            for (int s = 0; s < subTris.Count; s++) m.SetTriangles(subTris[s], s);
            if (src.blendShapeCount > 0)
            {
                var dv = new Vector3[vc]; var dn = new Vector3[vc]; var dt = new Vector3[vc];
                for (int bs = 0; bs < src.blendShapeCount; bs++)
                    for (int f = 0; f < src.GetBlendShapeFrameCount(bs); f++)
                    {
                        src.GetBlendShapeFrameVertices(bs, f, dv, dn, dt);
                        m.AddBlendShapeFrame(src.GetBlendShapeName(bs), src.GetBlendShapeFrameWeight(bs, f), Pick(dv), Pick(dn), Pick(dt));
                    }
            }
            m.RecalculateBounds();
            _headCache[src] = m;
            return m;
        }

        // =================================================================
        //  MALZEMELER
        // =================================================================
        static readonly Dictionary<string, Texture2D> _tex = new Dictionary<string, Texture2D>();
        static readonly Dictionary<string, Material> _mats = new Dictionary<string, Material>();

        static Texture2D Tex(string n)
        {
            if (!_tex.TryGetValue(n, out var t)) { t = Resources.Load<Texture2D>("Humans/Parts/Textures/" + n); _tex[n] = t; }
            return t;
        }

        static Color SkinTint(Color? c)
        {
            if (c == null) return Color.white;
            var reference = new Color(0.82f, 0.6f, 0.48f);   // dokunun ortalama ten rengi
            var r = new Color(Mathf.Clamp(c.Value.r / reference.r, 0.5f, 1.15f), Mathf.Clamp(c.Value.g / reference.g, 0.5f, 1.15f), Mathf.Clamp(c.Value.b / reference.b, 0.5f, 1.15f));
            return Color.Lerp(Color.white, r, 0.22f);   // portre fotoğrafları karanlık: tene yalnızca hafif yansıt
        }

        static Color HairTint(Color col)
        {
            // Saç dokusu açık gri: renk doğrudan çarpılır
            Color.RGBToHSV(col, out var hh, out var s, out var v);
            return Color.HSVToRGB(hh, Mathf.Min(s, 0.6f), Mathf.Clamp(v * (v > 0.35f ? 1.45f : 1.15f), 0.06f, 1.2f));   // kır / ak saç parlak kalsın
        }

        // ---- Kıyafet atlası bölge ayrımı: koyu kumaş / açık kumaş / atlas içindeki ten ----
        enum Slot { Dark, Light, SkinAtlas, Skin, Hair, Beard, Eye }
        class SplitMesh { public Mesh Mesh; public Slot[] Slots; public string Atlas; }
        static readonly Dictionary<Mesh, SplitMesh> _splits = new Dictionary<Mesh, SplitMesh>();

        static string AtlasOf(string matName)
        {
            var n = (matName ?? "").ToLowerInvariant();
            return n.Contains("peasant") ? "T_Peasant_BaseColor" : n.Contains("ranger") ? "T_Ranger_3_BaseColor" : null;
        }

        static Slot Classify(Color c)
        {
            Color.RGBToHSV(c, out var h, out var s, out var v);
            if (h > 0.02f && h < 0.09f && s > 0.33f && s < 0.6f && v > 0.6f) return Slot.SkinAtlas;
            if (v > 0.5f && s < 0.3f) return Slot.Light;
            return Slot.Dark;
        }

        static SplitMesh Split(Mesh src, Material[] mats, bool beard)
        {
            if (_splits.TryGetValue(src, out var cached)) return cached;
            var groups = new List<(Slot slot, List<int> tris)>();
            string atlas = null;
            var uv = src.isReadable ? src.uv : null;
            for (int sm = 0; sm < src.subMeshCount; sm++)
            {
                string mn = sm < mats.Length && mats[sm] != null ? mats[sm].name.ToLowerInvariant() : "";
                var tris = src.isReadable ? src.GetTriangles(sm) : null;
                var at = AtlasOf(mn);
                if (at != null)
                {
                    atlas = at;
                    var tex = Tex(at);
                    if (tris == null || uv == null || uv.Length == 0 || tex == null || !tex.isReadable) { groups.Add((Slot.Dark, tris != null ? new List<int>(tris) : null)); continue; }
                    var buckets = new Dictionary<Slot, List<int>> { { Slot.Dark, new List<int>() }, { Slot.Light, new List<int>() }, { Slot.SkinAtlas, new List<int>() } };
                    for (int i = 0; i + 2 < tris.Length; i += 3)
                    {
                        var c = (uv[tris[i]] + uv[tris[i + 1]] + uv[tris[i + 2]]) / 3f;
                        var k = Classify(tex.GetPixelBilinear(c.x, c.y));
                        var b = buckets[k]; b.Add(tris[i]); b.Add(tris[i + 1]); b.Add(tris[i + 2]);
                    }
                    foreach (var kv in buckets) if (kv.Value.Count > 0) groups.Add((kv.Key, kv.Value));
                }
                else
                {
                    var slot = mn.Contains("eye") ? Slot.Eye : (mn.Contains("hair") || mn.Contains("brow")) ? (beard ? Slot.Beard : Slot.Hair) : Slot.Skin;
                    groups.Add((slot, tris != null ? new List<int>(tris) : null));
                }
            }
            SplitMesh res;
            if (!src.isReadable || groups.Exists(g => g.tris == null))
                res = new SplitMesh { Mesh = src, Slots = groups.ConvertAll(g => g.slot).ToArray(), Atlas = atlas };
            else
            {
                var m = Object.Instantiate(src); m.name = src.name + "_Bölge";
                m.subMeshCount = groups.Count;
                for (int i = 0; i < groups.Count; i++) m.SetTriangles(groups[i].tris, i);
                res = new SplitMesh { Mesh = m, Slots = groups.ConvertAll(g => g.slot).ToArray(), Atlas = atlas };
            }
            _splits[src] = res;
            return res;
        }

        // ---- Gri (renksiz) atlas: kumaş rengini tarif belirler, doku detayı korunur ----
        class GrayAtlas { public Texture2D Tex; public float MeanDark = 0.3f, MeanLight = 0.7f; }
        static readonly Dictionary<string, GrayAtlas> _gray = new Dictionary<string, GrayAtlas>();

        static GrayAtlas Gray(string atlas)
        {
            if (_gray.TryGetValue(atlas, out var g)) return g;
            g = new GrayAtlas();
            var src = Tex(atlas);
            if (src != null && src.isReadable)
            {
                int mip = Mathf.Clamp(Mathf.RoundToInt(Mathf.Log(src.width / 1024f, 2f)), 0, src.mipmapCount - 1);
                var px = src.GetPixels32(mip);
                int w = Mathf.Max(1, src.width >> mip), hgt = Mathf.Max(1, src.height >> mip);
                var outPx = new Color32[px.Length];
                double sd = 0, sl = 0; int nd = 0, nl = 0;
                for (int i = 0; i < px.Length; i++)
                {
                    Color c = px[i];
                    float lum = c.r * 0.3f + c.g * 0.59f + c.b * 0.11f;
                    byte b = (byte)Mathf.Clamp(lum * 255f, 0, 255);
                    outPx[i] = new Color32(b, b, b, 255);
                    if ((i & 7) != 0) continue;
                    var k = Classify(c);
                    if (k == Slot.Dark) { sd += lum; nd++; } else if (k == Slot.Light) { sl += lum; nl++; }
                }
                if (nd > 0) g.MeanDark = Mathf.Max(0.08f, (float)(sd / nd));
                if (nl > 0) g.MeanLight = Mathf.Max(0.2f, (float)(sl / nl));
                g.Tex = new Texture2D(w, hgt, TextureFormat.RGBA32, true) { name = atlas + "_Gri", wrapMode = TextureWrapMode.Clamp, anisoLevel = 4 };
                g.Tex.SetPixels32(outPx); g.Tex.Apply(true); g.Tex.Compress(true);
                g.Tex.Apply(false, true);
            }
            _gray[atlas] = g;
            return g;
        }

        static Material Mat(string key, Texture2D tex, Texture2D normal, Color col, float smooth)
        {
            Color q = new Color(Mathf.Round(col.r * 40f) / 40f, Mathf.Round(col.g * 40f) / 40f, Mathf.Round(col.b * 40f) / 40f, 1f);
            string k = key + "|" + (tex != null ? tex.name : "-") + "|" + q.r.ToString("0.000") + "," + q.g.ToString("0.000") + "," + q.b.ToString("0.000");
            if (_mats.TryGetValue(k, out var m) && m != null) return m;
            m = new Material(MaterialLibrary.LitShader) { name = "MH_" + key };
            if (tex != null) { m.SetTexture("_BaseMap", tex); m.mainTexture = tex; }
            m.SetColor("_BaseColor", q); m.color = q;
            if (normal != null) { m.SetTexture("_BumpMap", normal); m.EnableKeyword("_NORMALMAP"); }
            m.SetFloat("_Smoothness", smooth);
            m.SetFloat("_Glossiness", smooth);
            _mats[k] = m;
            return m;
        }

        static Color Div(Color c, float mean) { float k = 1f / Mathf.Max(0.08f, mean); return new Color(Mathf.Min(3f, c.r * k), Mathf.Min(3f, c.g * k), Mathf.Min(3f, c.b * k)); }

        /// <summary>Tüm skin mesh'lere tarif renklerini uygular (parça: gövde / kol / bacak / ayak; bölge: koyu, açık, ten).</summary>
        static void ApplyMaterials(GameObject go, CharLook spec, Look look)
        {
            var skin = SkinTint(spec.Skin ?? look.Skin);
            var hairCol = HairTint(spec.HairColor ?? look.Hair ?? new Color(0.12f, 0.09f, 0.07f));
            var skinNeutral = new Color(0.5f, 0.38f, 0.32f);
            var stubble = spec.Beard == "stubble" ? Color.Lerp(hairCol, skinNeutral, 0.5f) : hairCol;
            string skinTex = spec.Female ? "T_Skin_Female" : "T_Skin_Male";
            var skinMat = Mat("skin", Tex(skinTex), Tex(skinTex + "_Normal"), skin, 0.3f);
            bool hv = Hash(spec.Female + "" + spec.Age) % 2 == 1;
            foreach (var smr in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (smr.sharedMesh == null) continue;
                string n = (smr.name + " " + smr.sharedMesh.name).ToLowerInvariant();
                bool isBeard = n.Contains("beard");
                var split = Split(smr.sharedMesh, smr.sharedMaterials, isBeard);
                smr.sharedMesh = split.Mesh;
                Color dark, light;
                if (n.Contains("_body")) { dark = spec.TopColor; light = spec.Shirt; }
                else if (n.Contains("_arms")) { dark = spec.SleeveColor; light = spec.Coat || spec.LabCoat ? spec.SleeveColor : spec.Shirt; }
                else if (n.Contains("_legs")) { dark = light = spec.Pants; }
                else if (n.Contains("_feet")) { dark = light = spec.Shoes; }
                else { dark = spec.Top; light = spec.Shirt; }
                var gray = split.Atlas != null ? Gray(split.Atlas) : null;
                Texture2D normal = split.Atlas == null ? null : Tex(split.Atlas.Contains("Peasant") ? "T_Peasant_Normal" : "T_Ranger_Normal");
                var mats = new Material[split.Slots.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    switch (split.Slots[i])
                    {
                        case Slot.Dark: mats[i] = Mat("cloth_d", gray?.Tex, normal, gray != null ? Div(dark, gray.MeanDark) : dark, 0.12f); break;
                        case Slot.Light: mats[i] = Mat("cloth_l", gray?.Tex, normal, gray != null ? Div(light, gray.MeanLight) : light, 0.1f); break;
                        case Slot.SkinAtlas: mats[i] = Mat("skin_atlas", Tex(split.Atlas), normal, skin, 0.3f); break;
                        case Slot.Hair: mats[i] = Mat("hair", Tex(hv ? "T_Hair_2_BaseColor" : "T_Hair_1_BaseColor"), Tex(hv ? "T_Hair_2_Normal" : "T_Hair_1_Normal"), hairCol, 0.3f); break;
                        case Slot.Beard: mats[i] = Mat("beard", Tex("T_Hair_1_BaseColor"), Tex("T_Hair_1_Normal"), stubble, 0.2f); break;
                        case Slot.Eye: mats[i] = Mat("eye", Tex("T_Eye_Brown"), null, Color.white, 0.85f); break;
                        default: mats[i] = skinMat; break;
                    }
                }
                smr.sharedMaterials = mats;
            }
        }
    }

    /// <summary>Elde taşınan eşyayı (fener, şemsiye) dünya dikeyinde tutar; el sallanırken sarkaç gibi.</summary>
    public class KeepUpright : MonoBehaviour
    {
        public Transform facing;
        void LateUpdate()
        {
            var f = facing != null ? facing.forward : transform.forward; f.y = 0;
            if (f.sqrMagnitude < 1e-4f) f = Vector3.forward;
            transform.rotation = Quaternion.LookRotation(f.normalized, Vector3.up);
        }
    }

    /// <summary>
    /// İnsan simülasyonu animatörü (Playables): hıza göre bekleme ↔ yürüme ↔ koşma geçişi, konuşurken jest animasyonu,
    /// oturma / diz çökme pozu, elde eşya taşıma (sağ kol katmanı), başını konuştuğu kişiye çevirme,
    /// tek seferlik jestler (baş sallama, onaylama) ve (blendshape varsa) göz kırpma.
    /// </summary>
    public class HumanAnimator : MonoBehaviour
    {
        public Animator Animator { get; private set; }
        public Transform Head => Animator != null ? Animator.GetBoneTransform(HumanBodyBones.Head) : null;
        public Transform HandR => Animator != null ? Animator.GetBoneTransform(HumanBodyBones.RightHand) : null;
        public Transform HandL => Animator != null ? Animator.GetBoneTransform(HumanBodyBones.LeftHand) : null;
        /// <summary>Başın merkezinde, karakterin önüne bakan, primitive figür ölçüsünde çerçeve (şapka vb. için).</summary>
        public Transform HeadAnchor { get; private set; }
        /// <summary>Sağ elde, dünya ölçeğinde çerçeve (taşınan eşyalar için).</summary>
        public Transform HandAnchor { get; private set; }
        public Transform ChestAnchor { get; private set; }

        /// <summary>Konuşma jesti (sorguda cevap verirken).</summary>
        public bool Talking;
        /// <summary>Başın döneceği hedef (ör. dedektifin kamerası).</summary>
        public Transform LookTarget;

        PlayableGraph _graph;
        AnimationMixerPlayable _mix;
        AnimationLayerMixerPlayable _layers;
        int _iIdle = -1, _iWalk = -1, _iRun = -1, _iTalk = -1, _iGesture = -1;
        bool _posed;
        Vector3 _lastPos; float _speed, _lookW, _blinkT, _nextBlink, _gestureT, _gestureLen;
        float[] _w;
        int _seed;
        SkinnedMeshRenderer _blinkMesh; int _blinkIndex = -1;

        public void Init(Animator anim, int seed, string pose)
        {
            Animator = anim;
            _seed = seed;
            _graph = PlayableGraph.Create("İnsan_" + name);
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            var output = AnimationPlayableOutput.Create(_graph, "Anim", anim);
            var clips = new List<AnimationClip>();
            AnimationClip poseClip = string.IsNullOrEmpty(pose) ? null : HumanLibrary.Clip(pose, seed);
            if (poseClip != null) { clips.Add(poseClip); _posed = true; _iIdle = 0; }
            else
            {
                void Add(string kind, ref int idx) { var c = HumanLibrary.Clip(kind, seed); if (c != null) { idx = clips.Count; clips.Add(c); } }
                Add("idle", ref _iIdle); Add("walk", ref _iWalk); Add("run", ref _iRun); Add("talk", ref _iTalk);
                _iGesture = clips.Count; clips.Add(null);   // tek seferlik jest yuvası
            }
            _mix = AnimationMixerPlayable.Create(_graph, clips.Count);
            for (int i = 0; i < clips.Count; i++)
            {
                if (clips[i] == null) continue;
                var p = AnimationClipPlayable.Create(_graph, clips[i]);
                p.SetApplyFootIK(true);
                if (clips[i].isLooping) p.SetTime(Random.value * clips[i].length);   // herkes aynı anda aynı hareketi yapmasın
                else if (_posed) { p.SetTime(clips[i].length); p.SetSpeed(0); }   // tek seferlik poz: son karede kal
                _graph.Connect(p, 0, _mix, i);
                _mix.SetInputWeight(i, i == _iIdle ? 1f : 0f);
            }
            _w = new float[clips.Count];
            _layers = AnimationLayerMixerPlayable.Create(_graph, 2);
            _graph.Connect(_mix, 0, _layers, 0);
            _layers.SetInputWeight(0, 1f);
            output.SetSourcePlayable(_layers);
            _graph.Play();
            _lastPos = transform.position;
            _nextBlink = Time.time + Random.Range(1f, 4f);

            foreach (var smr in GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var m = smr.sharedMesh; if (m == null) continue;
                for (int i = 0; i < m.blendShapeCount; i++)
                    if (m.GetBlendShapeName(i).ToLowerInvariant().Contains("blink")) { _blinkMesh = smr; _blinkIndex = i; break; }
                if (_blinkMesh != null) break;
            }
        }

        /// <summary>
        /// Baş / el / göğüs çerçevelerini kurar. Model henüz bağlama pozundayken çağrılmalı (Spawn sonrası, ilk kareden önce).
        /// HeadAnchor ölçeği, eski primitive kafanın (yarı yükseklik 0.17) ölçülerini gerçek kafaya oturtur:
        /// böylece mevcut şapka/aksesuar kodu iki figür türünde de aynı ofsetlerle çalışır.
        /// </summary>
        public void BuildAnchors(Transform root)
        {
            var head = Head; var hand = HandR; var chest = Animator != null ? Animator.GetBoneTransform(HumanBodyBones.UpperChest) ?? Animator.GetBoneTransform(HumanBodyBones.Chest) : null;
            // Bağlama pozunda modelin baktığı yön (bazı modeller -Z'ye bakar; Humanoid animasyon sonra kökün önüne çevirir):
            // sağ-sol kalça ekseni ile yukarının çarpımı
            var face = root.forward;
            var lUp = Animator != null ? Animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg) : null;
            var rUp = Animator != null ? Animator.GetBoneTransform(HumanBodyBones.RightUpperLeg) : null;
            if (lUp != null && rUp != null)
            {
                var right = rUp.position - lUp.position; right.y = 0;
                if (right.sqrMagnitude > 1e-6f) face = Vector3.Cross(right.normalized, Vector3.up);
            }
            var fwd = Quaternion.LookRotation(Flat(face), Vector3.up);
            if (head != null)
            {
                // Kafa sınırları: başa ağırlıklı skin mesh'lerin bağlama pozu köşeleri
                Bounds wb = default; bool any = false;
                foreach (var smr in GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    var bs = smr.bones; var bp = smr.sharedMesh != null ? smr.sharedMesh.bindposes : null;
                    if (bs == null || bp == null) continue;
                    int hi = System.Array.IndexOf(bs, head);
                    if (hi < 0 || hi >= bp.Length || smr.sharedMesh.vertexCount > 20000) continue;
                    string n = smr.sharedMesh.name.ToLowerInvariant();
                    if (!n.Contains("baş") && !n.Contains("face") && !n.Contains("head")) continue;
                    var mtx = head.localToWorldMatrix * bp[hi];
                    var b = smr.sharedMesh.bounds;
                    for (int c = 0; c < 8; c++)
                    {
                        var p = mtx.MultiplyPoint3x4(b.center + Vector3.Scale(b.extents, new Vector3((c & 1) == 0 ? -1 : 1, (c & 2) == 0 ? -1 : 1, (c & 4) == 0 ? -1 : 1)));
                        if (!any) { wb = new Bounds(p, Vector3.zero); any = true; } else wb.Encapsulate(p);
                    }
                }
                float half = any ? Mathf.Clamp(wb.extents.y * 0.62f, 0.06f, 0.2f) : 0.12f;   // baş+boyun kutusu → kafatası
                var center = any ? new Vector3(head.position.x, wb.max.y - half * 1.05f, head.position.z) : head.position + Vector3.up * 0.09f;
                HeadAnchor = MakeAnchor("BaşÇerçevesi", head, center, fwd, half / 0.17f);
            }
            if (hand != null) HandAnchor = MakeAnchor("ElÇerçevesi", hand, hand.position, fwd, 1f);
            if (chest != null) ChestAnchor = MakeAnchor("GöğüsÇerçevesi", chest, chest.position, fwd, 1f);
        }

        static Vector3 Flat(Vector3 v) { v.y = 0; return v.sqrMagnitude < 1e-6f ? Vector3.forward : v.normalized; }

        static Transform MakeAnchor(string n, Transform bone, Vector3 pos, Quaternion rot, float worldScale)
        {
            var a = new GameObject(n).transform;
            a.SetParent(bone, false);
            a.position = pos; a.rotation = rot;
            float ps = Mathf.Max(1e-5f, bone.lossyScale.x);
            a.localScale = Vector3.one * (worldScale / ps);
            return a;
        }

        /// <summary>Sağ kolu verilen klibin pozuna geçirir (gövde yürümeye devam eder): fener, şemsiye taşıma.</summary>
        public bool HoldWithRightArm(string kind) => HoldArms(kind, false);

        /// <summary>İki kolu birden klibin pozuna geçirir (sandık / sepet taşıma).</summary>
        public bool HoldWithArms(string kind) => HoldArms(kind, true);

        bool HoldArms(string kind, bool both)
        {
            if (!_graph.IsValid()) return false;
            var c = HumanLibrary.Clip(kind, _seed);
            if (c == null) return false;
            var p = AnimationClipPlayable.Create(_graph, c);
            p.SetTime(Random.value * c.length);
            if (_layers.GetInput(1).IsValid()) _graph.Disconnect(_layers, 1);
            _graph.Connect(p, 0, _layers, 1);
            var mask = new AvatarMask();
            for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++) mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, false);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightArm, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightFingers, true);
            if (both)
            {
                mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftArm, true);
                mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftFingers, true);
            }
            _layers.SetLayerMaskFromAvatarMask(1u, mask);
            _layers.SetInputWeight(1, 1f);
            return true;
        }

        /// <summary>Yaşlılık: omurganın öne eğilme açısı (derece).</summary>
        public float Stoop;

        /// <summary>Bekleme klibini adıyla değiştirir (ör. "idle_arms": kollar kavuşturulmuş).</summary>
        public void UseIdle(string clipName)
        {
            if (!_graph.IsValid() || _iIdle < 0) return;
            var c = HumanLibrary.ClipNamed(clipName);
            if (c == null) return;
            var old = _mix.GetInput(_iIdle);
            float w = _mix.GetInputWeight(_iIdle);
            _graph.Disconnect(_mix, _iIdle);
            if (old.IsValid()) old.Destroy();
            var p = AnimationClipPlayable.Create(_graph, c);
            p.SetApplyFootIK(true);
            p.SetTime(Random.value * c.length);
            _graph.Connect(p, 0, _mix, _iIdle);
            _mix.SetInputWeight(_iIdle, w);
        }

        /// <summary>Tek seferlik jest (yes / no / interact / pickup / drink): oynar ve beklemeye döner.</summary>
        public bool Gesture(string kind)
        {
            if (!_graph.IsValid() || _iGesture < 0 || _posed) return false;
            var c = HumanLibrary.Clip(kind, _seed + (int)(Time.time * 10));
            if (c == null) return false;
            if (_mix.GetInput(_iGesture).IsValid()) { var old = _mix.GetInput(_iGesture); _graph.Disconnect(_mix, _iGesture); old.Destroy(); }
            var p = AnimationClipPlayable.Create(_graph, c);
            p.SetApplyFootIK(true);
            _graph.Connect(p, 0, _mix, _iGesture);
            _gestureLen = Mathf.Min(c.length, 3.5f);
            _gestureT = _gestureLen;
            return true;
        }

        public bool GestureActive => _gestureT > 0f;

        void Update()
        {
            if (!_graph.IsValid()) return;
            var p = transform.position;
            float inst = Time.deltaTime > 0f ? new Vector3(p.x - _lastPos.x, 0, p.z - _lastPos.z).magnitude / Time.deltaTime : 0f;
            if (inst > 20f) inst = 0f; // ışınlanma
            _lastPos = p;
            _speed = Mathf.Lerp(_speed, inst, 1f - Mathf.Exp(-Time.deltaTime * 8f));
            if (_posed) return;

            for (int i = 0; i < _w.Length; i++) _w[i] = 0f;
            float walk = Mathf.Clamp01(_speed / 1.4f);
            float run = _iRun >= 0 ? Mathf.Clamp01((_speed - 3.2f) / 1.8f) : 0f;
            float gesture = 0f;
            if (_gestureT > 0f)
            {
                _gestureT -= Time.deltaTime;
                float el = _gestureLen - _gestureT;
                gesture = Mathf.Clamp01(Mathf.Min(el / 0.25f, _gestureT / 0.35f)) * (1f - walk);
            }
            float talk = Talking && _iTalk >= 0 ? (1f - walk) * (1f - gesture) : 0f;
            float rest = (1f - walk) * (1f - talk) * (1f - gesture);
            if (_iIdle >= 0) _w[_iIdle] = rest;
            if (_iTalk >= 0) _w[_iTalk] += talk;
            if (_iGesture >= 0 && _mix.GetInput(_iGesture).IsValid()) _w[_iGesture] = gesture; else if (_iIdle >= 0) _w[_iIdle] += gesture;
            if (_iWalk >= 0) _w[_iWalk] = walk * (1f - run); else if (_iIdle >= 0) _w[_iIdle] += walk;
            if (_iRun >= 0) _w[_iRun] = walk * run;
            for (int i = 0; i < _w.Length; i++)
                _mix.SetInputWeight(i, Mathf.Lerp(_mix.GetInputWeight(i), _w[i], 1f - Mathf.Exp(-Time.deltaTime * 10f)));
            // Yürüme adımı hıza uysun
            if (_iWalk >= 0) _mix.GetInput(_iWalk).SetSpeed(Mathf.Clamp(_speed / 1.4f, 0.7f, 1.4f));
        }

        void LateUpdate()
        {
            // Yaşlı duruşu: üst gövde öne eğik, baş hafif kalkık
            if (Stoop > 0.1f && Animator != null)
            {
                var sp = Animator.GetBoneTransform(HumanBodyBones.Spine);
                var ch = Animator.GetBoneTransform(HumanBodyBones.UpperChest) ?? Animator.GetBoneTransform(HumanBodyBones.Chest);
                var right = transform.right;
                if (sp != null) sp.rotation = Quaternion.AngleAxis(Stoop * 0.5f, right) * sp.rotation;
                if (ch != null) ch.rotation = Quaternion.AngleAxis(Stoop * 0.6f, right) * ch.rotation;
                var hd = Head; if (hd != null) hd.rotation = Quaternion.AngleAxis(-Stoop * 0.7f, right) * hd.rotation;
            }
            // Baş takibi: animasyonun üstüne, gövde yönünden en fazla 65° sapma
            var head = Head;
            bool want = LookTarget != null && head != null && _speed < 0.5f;
            _lookW = Mathf.MoveTowards(_lookW, want ? 1f : 0f, Time.deltaTime * 2.5f);
            if (_lookW > 0.001f && head != null && LookTarget != null)
            {
                var dir = LookTarget.position - head.position;
                var fwd = transform.forward;
                float yaw = Mathf.Clamp(Vector3.SignedAngle(new Vector3(fwd.x, 0, fwd.z), new Vector3(dir.x, 0, dir.z), Vector3.up), -65f, 65f);
                float pitch = Mathf.Clamp(-Mathf.Atan2(dir.y, new Vector2(dir.x, dir.z).magnitude) * Mathf.Rad2Deg, -25f, 25f);
                var delta = Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.AngleAxis(pitch, transform.right);
                head.rotation = Quaternion.Slerp(Quaternion.identity, delta, _lookW * 0.85f) * head.rotation;
            }
            // Göz kırpma (modelde "blink" blendshape'i varsa)
            if (_blinkMesh != null)
            {
                if (Time.time >= _nextBlink) { _blinkT = 0.16f; _nextBlink = Time.time + Random.Range(2.5f, 6f); }
                _blinkT = Mathf.Max(0f, _blinkT - Time.deltaTime);
                _blinkMesh.SetBlendShapeWeight(_blinkIndex, Mathf.Sin(Mathf.Clamp01(_blinkT / 0.16f) * Mathf.PI) * 100f);
            }
        }

        void OnDestroy() { if (_graph.IsValid()) _graph.Destroy(); }
    }
}
