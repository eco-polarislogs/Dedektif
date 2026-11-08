using UnityEngine;
using UnityEngine.Rendering;

namespace Dedektif3D.World
{
    /// <summary>Parçacık efektleri: yağmur, yağmur sıçraması, baca dumanı, sis bulutları. Hepsi koddan, asset'siz.</summary>
    public static class Effects
    {
        static Shader _particleShader;
        static Material _softDot;

        static Shader ParticleShader
        {
            get
            {
                if (_particleShader == null) _particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                return _particleShader;
            }
        }

        /// <summary>Yumuşak kenarlı yuvarlak parçacık dokusu (duman/sis için).</summary>
        static Texture2D SoftDotTexture()
        {
            const int s = 64;
            var t = new Texture2D(s, s, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var px = new Color[s * s];
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    float dx = (x - s / 2f) / (s / 2f), dy = (y - s / 2f) / (s / 2f);
                    float a = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                    px[y * s + x] = new Color(1, 1, 1, a * a);
                }
            t.SetPixels(px); t.Apply();
            return t;
        }

        public static Material TransparentMaterial(Color color, bool soft = false)
        {
            var shader = ParticleShader;
            if (shader == null) return null;
            var mat = new Material(shader);
            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)RenderQueue.Transparent;
            if (soft)
            {
                if (_softDot == null) _softDot = new Material(shader);
                mat.SetTexture("_BaseMap", SoftDotTexture());
            }
            return mat;
        }

        /// <summary>Kamerayı takip eden sağanak yağmur + yerde sıçrama (web: rain-overlay, "yağmurlu gece").</summary>
        public static GameObject Rain(Transform follow, float intensity = 1f)
        {
            if (ParticleShader == null) return null;
            var root = new GameObject("Yağmur");
            var f = root.AddComponent<FollowPosition>();
            f.target = follow; f.offset = Vector3.zero;

            var drops = new GameObject("Damlalar");
            drops.transform.SetParent(root.transform, false);
            drops.transform.localPosition = new Vector3(0, 14f, 0);
            drops.transform.rotation = Quaternion.Euler(90, 0, 0);
            var ps = drops.AddComponent<ParticleSystem>();
            var main = ps.main;
            // İnce ve yarı saydam: kameranın önünde kalın çubuk gibi görünmesin
            main.startLifetime = 1.2f; main.startSpeed = new ParticleSystem.MinMaxCurve(15f, 19f); main.startSize = 0.014f;
            main.startColor = new Color(0.75f, 0.82f, 0.95f, 0.28f); main.maxParticles = 5000;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var em = ps.emission; em.rateOverTime = 2600 * intensity;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(36, 36, 0.1f);
            var vel = ps.velocityOverLifetime; vel.enabled = true; vel.space = ParticleSystemSimulationSpace.World;
            vel.x = new ParticleSystem.MinMaxCurve(-1.2f, -0.6f); vel.y = new ParticleSystem.MinMaxCurve(0f, 0f); vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            var col = ps.collision; col.enabled = true; col.type = ParticleSystemCollisionType.World; col.mode = ParticleSystemCollisionMode.Collision3D;
            col.lifetimeLoss = 1f; col.quality = ParticleSystemCollisionQuality.Low; col.collidesWith = ~(1 << 2);
            var r = drops.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.Stretch; r.lengthScale = 4f; r.velocityScale = 0.05f;
            r.sharedMaterial = TransparentMaterial(new Color(0.75f, 0.82f, 0.95f, 0.28f));

            // Sıçrama: zemine yakın küçük halkalar
            var splash = new GameObject("Sıçrama");
            splash.transform.SetParent(root.transform, false);
            splash.transform.localPosition = new Vector3(0, 0.05f, 0);
            var ps2 = splash.AddComponent<ParticleSystem>();
            var m2 = ps2.main;
            m2.startLifetime = 0.25f; m2.startSpeed = 0.6f; m2.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.1f);
            m2.startColor = new Color(0.8f, 0.85f, 0.95f, 0.35f); m2.maxParticles = 800; m2.gravityModifier = 0.6f;
            m2.simulationSpace = ParticleSystemSimulationSpace.World;
            var e2 = ps2.emission; e2.rateOverTime = 500 * intensity;
            var s2 = ps2.shape; s2.shapeType = ParticleSystemShapeType.Box; s2.scale = new Vector3(18, 0.01f, 18);
            s2.rotation = new Vector3(-90, 0, 0);
            var r2 = splash.GetComponent<ParticleSystemRenderer>();
            r2.sharedMaterial = TransparentMaterial(new Color(0.8f, 0.85f, 0.95f, 0.35f), true);
            return root;
        }

        /// <summary>Baca dumanı (yavaş yükselen gri bulutçuklar).</summary>
        public static void ChimneySmoke(Transform parent, Vector3 localPos)
        {
            if (ParticleShader == null) return;
            var go = new GameObject("BacaDumanı");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 6f; main.startSpeed = 0.7f; main.startSize = new ParticleSystem.MinMaxCurve(0.6f, 1.4f);
            main.startColor = new Color(0.55f, 0.57f, 0.62f, 0.22f); main.maxParticles = 60;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var em = ps.emission; em.rateOverTime = 4;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 8; sh.radius = 0.15f;
            var sz = ps.sizeOverLifetime; sz.enabled = true; sz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 0.6f, 1, 2.4f));
            var vel = ps.velocityOverLifetime; vel.enabled = true; vel.space = ParticleSystemSimulationSpace.World;
            vel.x = new ParticleSystem.MinMaxCurve(-0.5f, -0.2f); vel.y = new ParticleSystem.MinMaxCurve(0f, 0f); vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            var colo = ps.colorOverLifetime; colo.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                         new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, 0.2f), new GradientAlphaKey(0, 1) });
            colo.color = grad;
            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = TransparentMaterial(new Color(0.55f, 0.57f, 0.62f, 0.22f), true);
        }

        /// <summary>Yerde sürünen sis bulutları (town_map'teki sisli meydan).</summary>
        public static void GroundFog(Transform parent, Vector3 center, Vector3 area, int count = 40)
        {
            if (ParticleShader == null) return;
            var go = new GameObject("YerSisi");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = center;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 16f; main.startSpeed = 0.15f; main.startSize = new ParticleSystem.MinMaxCurve(6f, 12f);
            main.startColor = new Color(0.62f, 0.67f, 0.75f, 0.10f); main.maxParticles = count; main.prewarm = true; main.loop = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var em = ps.emission; em.rateOverTime = count / 16f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = area;
            var colo = ps.colorOverLifetime; colo.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                         new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, 0.3f), new GradientAlphaKey(1, 0.7f), new GradientAlphaKey(0, 1) });
            colo.color = grad;
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = TransparentMaterial(new Color(0.62f, 0.67f, 0.75f, 0.10f), true);
            r.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
        }
    }

    /// <summary>Sadece konumu takip eder (dönüşü kopyalamaz).</summary>
    public class FollowPosition : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset;
        void LateUpdate() { if (target != null) transform.position = target.position + offset; }
    }
}
