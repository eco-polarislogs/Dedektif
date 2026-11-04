#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Windows oyunu derleme aracı (menü: Dedektif ▸ Windows Oyunu Derle, ya da -executeMethod DedektifBuild.BuildWindows).
///
/// Bütün sahne koddan kurulduğu için derleyici hangi shader varyantlarının ve post-process efektlerinin
/// kullanıldığını göremez ve onları atar (yağmur, pencere ışıkları, alan derinliği kaybolur). Bu yüzden:
///  • SnapshotMaterials(): oyun Unity'de oynatılırken koddan üretilen her malzemenin (shader + anahtar kelimeler)
///    bir kopyasını Assets/Resources/ShaderKeep altına kaydeder;
///  • derlemeden önce aynı post-process ayarlarını içeren bir VolumeProfile kaydedilir.
/// </summary>
public static class DedektifBuild
{
    const string KeepDir = "Assets/Resources/ShaderKeep";

    public static int SnapshotMaterials()
    {
        Directory.CreateDirectory(KeepDir);
        var seen = new HashSet<string>();
        foreach (var f in Directory.GetFiles(KeepDir, "*.mat"))
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(f.Replace('\\', '/'));
            if (m != null && m.shader != null) seen.Add(Sig(m));
        }
        int n = 0;
        foreach (var m in Resources.FindObjectsOfTypeAll<Material>())
        {
            if (m == null || m.shader == null || EditorUtility.IsPersistent(m)) continue;
            var sn = m.shader.name;
            if (!(sn.StartsWith("Universal Render Pipeline/") || sn.StartsWith("Skybox/"))) continue;
            var sig = Sig(m);
            if (!seen.Add(sig)) continue;
            var copy = new Material(m) { name = "Keep_" + Hash(sig) };
            foreach (var p in copy.GetTexturePropertyNames()) copy.SetTexture(p, null);
            AssetDatabase.CreateAsset(copy, $"{KeepDir}/Keep_{Hash(sig)}.mat");
            n++;
        }
        AssetDatabase.SaveAssets();
        return n;
    }

    static string Sig(Material m) => m.shader.name + "|" + string.Join(",", m.shaderKeywords.OrderBy(k => k)) + "|" + m.renderQueue;
    static string Hash(string s) { unchecked { uint h = 2166136261; foreach (char c in s) h = (h ^ c) * 16777619; return h.ToString("x8"); } }

    /// <summary>GameBootstrap.SetupPostProcessing ile aynı efektler — varyant ayıklayıcı bunları görsün diye.</summary>
    static void SavePostProfile()
    {
        Directory.CreateDirectory(KeepDir);
        const string path = KeepDir + "/KeepPostProfile.asset";
        if (AssetDatabase.LoadAssetAtPath<VolumeProfile>(path) != null) AssetDatabase.DeleteAsset(path);
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(profile, path);
        void Add<T>(System.Action<T> setup) where T : VolumeComponent
        {
            var c = profile.Add<T>(true);
            c.name = typeof(T).Name;
            setup?.Invoke(c);
            AssetDatabase.AddObjectToAsset(c, profile);
        }
        Add<Bloom>(b => { b.intensity.Override(0.8f); b.highQualityFiltering.Override(true); });
        Add<Vignette>(v => v.intensity.Override(0.26f));
        Add<DepthOfField>(d => { d.mode.Override(DepthOfFieldMode.Bokeh); d.focusDistance.Override(6f); });
        Add<Tonemapping>(t => t.mode.Override(TonemappingMode.ACES));
        Add<ColorAdjustments>(c => { c.postExposure.Override(0.9f); c.contrast.Override(12f); c.saturation.Override(-10f); });
        Add<FilmGrain>(g => { g.type.Override(FilmGrainLookup.Medium3); g.intensity.Override(0.12f); });
        Add<ChromaticAberration>(c => c.intensity.Override(0.2f));
        Add<ShadowsMidtonesHighlights>(s => s.shadows.Override(new Vector4(0.9f, 0.95f, 1.1f, -0.05f)));
        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();
    }

    /// <summary>Sis koddan açıldığı için (sahne dosyasında kapalı) "otomatik" sis ayıklama tüm sis varyantlarını atar: elle koru.</summary>
    static void KeepFogVariants()
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
        if (assets == null || assets.Length == 0) return;
        var so = new SerializedObject(assets[0]);
        void Set(string name, int v) { var p = so.FindProperty(name); if (p != null) p.intValue = v; }
        void SetB(string name) { var p = so.FindProperty(name); if (p != null) p.boolValue = true; }
        Set("m_FogStripping", 1);            // 1 = Custom
        SetB("m_FogKeepLinear"); SetB("m_FogKeepExp"); SetB("m_FogKeepExp2");
        so.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Dedektif/Windows Oyunu Derle")]
    public static void BuildWindows()
    {
        KeepFogVariants();
        SavePostProfile();
        AssetDatabase.Refresh();
        PlayerSettings.productName = "Dedektif";
        PlayerSettings.companyName = "Dedektiflik";
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = true;
        var outPath = Path.GetFullPath("Build/Windows/Dedektif.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(outPath));
        var opts = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/SampleScene.unity" },
            locationPathName = outPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
        };
        var report = BuildPipeline.BuildPlayer(opts);
        var sum = report.summary;
        Debug.Log($"[BUILD] sonuç={sum.result} boyut={sum.totalSize / 1048576} MB süre={sum.totalTime} hata={sum.totalErrors} uyarı={sum.totalWarnings} -> {outPath}");
        if (Application.isBatchMode) EditorApplication.Exit(sum.result == BuildResult.Succeeded ? 0 : 1);
    }
}
#endif
