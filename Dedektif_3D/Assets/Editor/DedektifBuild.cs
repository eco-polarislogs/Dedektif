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
            var hadNormal = copy.HasProperty("_BumpMap") && copy.GetTexture("_BumpMap") != null;
            foreach (var p in copy.GetTexturePropertyNames()) copy.SetTexture(p, null);
            // Normal haritası boş kalırsa URP _NORMALMAP anahtarını atar ve varyant derlemeye girmez
            if (hadNormal) copy.SetTexture("_BumpMap", KeepNormal);
            AssetDatabase.CreateAsset(copy, $"{KeepDir}/Keep_{Hash(sig)}.mat");
            n++;
        }
        AssetDatabase.SaveAssets();
        return n;
    }

    static Texture KeepNormal => AssetDatabase.LoadAssetAtPath<Texture>("Assets/Resources/PBR/plaster_n.jpg");

    /// <summary>
    /// Sahne koddan kurulduğu için malzemeler derlemede görünmez; URP/Lit'in oyunda kullanılan bütün anahtar birleşimleri
    /// (duvarlar: normal + pürüzsüzlük kanalı, lambalar: ışıma, yapraklar: alfa kesme, su: saydam) burada garanti edilir.
    /// Her birleşim için küçük bir malzeme Assets/Resources/ShaderKeep altına yazılır.
    /// </summary>
    static void EnsureLitVariants()
    {
        Directory.CreateDirectory(KeepDir);
        var lit = Shader.Find("Universal Render Pipeline/Lit");
        var normal = KeepNormal;
        if (lit == null || normal == null) { Debug.LogError("[BUILD] Lit shader ya da normal doku bulunamadı."); return; }
        const string N = "_NORMALMAP", S = "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A", E = "_EMISSION", A = "_ALPHATEST_ON", T = "_SURFACE_TYPE_TRANSPARENT", P = "_ALPHAPREMULTIPLY_ON";
        var sets = new[]
        {
            new string[0], new[] { N }, new[] { S }, new[] { N, S },
            new[] { E }, new[] { E, N }, new[] { E, S }, new[] { E, N, S },
            new[] { A }, new[] { A, N }, new[] { A, S }, new[] { A, N, S },
            new[] { T }, new[] { T, N }, new[] { T, P }, new[] { T, P, N },
        };
        int written = 0;
        foreach (var kw in sets)
        {
            string path = $"{KeepDir}/LitVariant_{(kw.Length == 0 ? "base" : string.Join("_", kw.Select(k => k.TrimStart('_').Substring(0, 3))))}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            var m = existing != null ? existing : new Material(lit);
            m.shader = lit;
            m.shaderKeywords = kw;
            m.enableInstancing = false;
            m.SetTexture("_BumpMap", kw.Contains(N) ? normal : null);
            m.SetFloat("_SmoothnessTextureChannel", kw.Contains(S) ? 1f : 0f);
            m.SetFloat("_AlphaClip", kw.Contains(A) ? 1f : 0f);
            m.SetFloat("_Surface", kw.Contains(T) ? 1f : 0f);
            m.SetFloat("_Blend", kw.Contains(P) ? 1f : 0f);
            if (kw.Contains(E)) { m.SetColor("_EmissionColor", Color.white); m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive; }
            m.renderQueue = kw.Contains(T) ? 3000 : kw.Contains(A) ? 2450 : 2000;
            if (existing == null) AssetDatabase.CreateAsset(m, path); else EditorUtility.SetDirty(m);
            written++;
        }
        AssetDatabase.SaveAssets();
        foreach (var kw in sets)
        {
            string path = $"{KeepDir}/LitVariant_{(kw.Length == 0 ? "base" : string.Join("_", kw.Select(k => k.TrimStart('_').Substring(0, 3))))}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            var missing = kw.Where(k => m == null || !m.IsKeywordEnabled(k)).ToArray();
            if (missing.Length > 0) Debug.LogError($"[BUILD] {path} anahtarı kaybetti: {string.Join(",", missing)}");
        }
        Debug.Log($"[BUILD] URP/Lit varyant malzemeleri hazır: {written}");
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

    /// <summary>
    /// Sis koddan açıldığı için (sahne dosyasında kapalı) "otomatik" sis ayıklama tüm sis varyantlarını atar: elle koru.
    /// Oyun yalnızca üstel-kare sis kullanır (GameBootstrap); diğer iki mod her URP/Lit varyantını boşuna çoğaltır.
    /// </summary>
    static void KeepFogVariants()
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
        if (assets == null || assets.Length == 0) return;
        var so = new SerializedObject(assets[0]);
        void Set(string name, int v) { var p = so.FindProperty(name); if (p != null) p.intValue = v; }
        void SetB(string name, bool v) { var p = so.FindProperty(name); if (p != null) p.boolValue = v; }
        Set("m_FogStripping", 1);            // 1 = Custom
        SetB("m_FogKeepLinear", false); SetB("m_FogKeepExp", false); SetB("m_FogKeepExp2", true);
        so.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// Oyun kodu instancing kullanmaz; eski anlık görüntülerde kalan bayrak bütün URP/Lit varyantlarını iki katına çıkarır.
    /// </summary>
    static void ClearKeepInstancing()
    {
        int n = 0;
        foreach (var f in Directory.GetFiles(KeepDir, "*.mat"))
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(f.Replace('\\', '/'));
            if (m == null || !m.enableInstancing) continue;
            m.enableInstancing = false;
            EditorUtility.SetDirty(m);
            n++;
        }
        if (n > 0) AssetDatabase.SaveAssets();
        Debug.Log($"[BUILD] Keep malzemelerinde instancing kapatıldı: {n}");
    }

    [MenuItem("Dedektif/Windows Oyunu Derle")]
    public static void BuildWindows()
    {
        KeepFogVariants();
        SavePostProfile();
        EnsureLitVariants();
        ClearKeepInstancing();
        AssetDatabase.Refresh();
        // Tek grafik API'si: D3D11 her Windows oyuncusunda çalışır, shader derleme süresi yarıya iner
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[] { GraphicsDeviceType.Direct3D11 });
        PlayerSettings.productName = "Dedektif";
        PlayerSettings.companyName = "Dedektiflik";
        PlayerSettings.bundleVersion = Version;
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        PlayerSettings.resizableWindow = true;
        // Oyun arka planda çalışmasın: pencere değiştirilince ekran kartı boşuna tam güçte kalmaz
        PlayerSettings.runInBackground = false;
        SetIcon();
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
        int images = sum.result == BuildResult.Succeeded ? CopyWebImages(outPath) : 0;
        bool ok = sum.result == BuildResult.Succeeded && images > 0;
        if (ok) CopyPackageFiles(outPath);
        Debug.Log($"[BUILD] sonuç={sum.result} boyut={sum.totalSize / 1048576} MB süre={sum.totalTime} hata={sum.totalErrors} uyarı={sum.totalWarnings} görsel={images} -> {outPath}");
        if (!ok) Debug.LogError("[BUILD] Başarısız: derleme hatası veya oyun görselleri kopyalanamadı.");
        if (Application.isBatchMode) EditorApplication.Exit(ok ? 0 : 1);
    }

    public const string Version = "1.0.0";

    /// <summary>Oyuncu kılavuzu ve lisans notları exe'nin yanına gider; Unity'nin "dağıtma" klasörü pakette kalmaz.</summary>
    static void CopyPackageFiles(string exePath)
    {
        string dir = Path.GetDirectoryName(exePath);
        foreach (var name in new[] { "OKU_BENI.txt" })
            CopyWithRetry(Path.GetFullPath("Assets/Editor/Package/" + name), Path.Combine(dir, name));
        CopyWithRetry(Path.GetFullPath("Assets/Resources/Humans/Parts/LICENSE_Quaternius_CC0.txt"), Path.Combine(dir, "LICENSE_Quaternius_CC0.txt"));
        string backup = Path.Combine(dir, Path.GetFileNameWithoutExtension(exePath) + "_BackUpThisFolder_ButDontShipItWithYourGame");
        try { if (Directory.Exists(backup)) Directory.Delete(backup, true); }
        catch (System.Exception e) when (e is IOException || e is System.UnauthorizedAccessException) { Debug.LogWarning($"[BUILD] Yedek klasör silinemedi, pakete koyma: {backup} ({e.Message})"); }
    }

    /// <summary>Uygulama simgesi (Assets/Editor/Package/icon.png).</summary>
    static void SetIcon()
    {
        const string path = "Assets/Editor/Package/icon.png";
        var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (icon == null) { Debug.LogWarning("[BUILD] Simge bulunamadı: " + path); return; }
#pragma warning disable CS0618
        var sizes = PlayerSettings.GetIconSizesForTargetGroup(BuildTargetGroup.Standalone);
        var icons = new Texture2D[sizes.Length];
        for (int i = 0; i < icons.Length; i++) icons[i] = icon;
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Standalone, icons);
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new[] { icon });
#pragma warning restore CS0618
    }

    /// <summary>
    /// Gölgeşehir ve Sisören görsellerini wwwroot'tan oyunun StreamingAssets klasörüne kopyalar (WebTextures buradan okur).
    /// Depoda ikinci bir kopya tutulmaz; dosyalar yalnızca derlenen oyunun yanına gider. Kopyalanan görsel sayısını döndürür.
    /// </summary>
    static int CopyWebImages(string exePath)
    {
        string srcRoot = Path.GetFullPath("../wwwroot/images/towns");
        string dataDir = Path.Combine(Path.GetDirectoryName(exePath), Path.GetFileNameWithoutExtension(exePath) + "_Data");
        string dstRoot = Path.Combine(dataDir, "StreamingAssets", "Web", "images", "towns");
        // Klasör silinmez, üzerine yazılır: proje OneDrive altında, eşitleme kilidi Directory.Delete'i "erişim engellendi" ile düşürebiliyor
        var kept = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

        int copied = 0;
        foreach (var town in new[] { "golge_sehir", "sisoren" })
        {
            string src = Path.Combine(srcRoot, town);
            if (!Directory.Exists(src))
            {
                Debug.LogError($"[BUILD] Görsel klasörü bulunamadı: {src}");
                return 0;
            }
            foreach (var file in Directory.GetFiles(src, "*", SearchOption.AllDirectories))
            {
                string ext = Path.GetExtension(file).ToLowerInvariant();
                if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".webp") continue;
                string dst = Path.Combine(dstRoot, town, file.Substring(src.Length + 1));
                Directory.CreateDirectory(Path.GetDirectoryName(dst));
                CopyWithRetry(file, dst);
                kept.Add(Path.GetFullPath(dst));
                copied++;
            }
        }
        // Kaynakta artık olmayan eski görseller pakette kalmasın; kilitli olan varsa derlemeyi düşürmeden bildirilir
        foreach (var old in Directory.GetFiles(dstRoot, "*", SearchOption.AllDirectories))
        {
            if (kept.Contains(Path.GetFullPath(old))) continue;
            try { File.Delete(old); }
            catch (System.Exception e) when (e is IOException || e is System.UnauthorizedAccessException) { Debug.LogWarning($"[BUILD] Eski görsel silinemedi: {old} ({e.Message})"); }
        }
        return copied;
    }

    /// <summary>OneDrive veya virüs tarayıcı dosyayı kısa süre kilitleyebilir: birkaç kez bekleyip yeniden dener.</summary>
    static void CopyWithRetry(string src, string dst)
    {
        for (int attempt = 1; ; attempt++)
        {
            try { File.Copy(src, dst, true); return; }
            catch (System.Exception e) when ((e is IOException || e is System.UnauthorizedAccessException) && attempt < 6)
            {
                System.Threading.Thread.Sleep(1000 * attempt);
            }
        }
    }
}
#endif
