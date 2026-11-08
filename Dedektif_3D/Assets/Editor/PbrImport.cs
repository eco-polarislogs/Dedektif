#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Assets/Resources/PBR altındaki gerçek (CC0, Poly Haven) yüzey dokuları:
///  • *_n  → Normal Map (kabartma),
///  • *_d  → sRGB renk + alfa kanalında pürüzsüzlük (1 - pürüzlülük; ıslak taş parlaması için).
/// </summary>
public class PbrImport : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        var path = assetPath.Replace('\\', '/');
        if (!path.Contains("/Resources/PBR/")) return;
        var ti = (TextureImporter)assetImporter;
        var file = System.IO.Path.GetFileNameWithoutExtension(path);
        ti.maxTextureSize = 1024;
        ti.mipmapEnabled = true;
        ti.anisoLevel = 8;
        ti.wrapMode = TextureWrapMode.Repeat;
        ti.filterMode = FilterMode.Trilinear;
        ti.textureCompression = TextureImporterCompression.CompressedHQ;
        if (file.EndsWith("_n"))
        {
            ti.textureType = TextureImporterType.NormalMap;
            ti.sRGBTexture = false;
        }
        else if (file.EndsWith("_h"))
        {
            // Yükseklik haritası: renk değil veri (paralaks ve su birikintisi hesabı doğrudan okur)
            ti.textureType = TextureImporterType.Default;
            ti.sRGBTexture = false;
            ti.alphaSource = TextureImporterAlphaSource.None;
            ti.maxTextureSize = 512;
        }
        else
        {
            ti.textureType = TextureImporterType.Default;
            ti.sRGBTexture = true;
            ti.alphaSource = TextureImporterAlphaSource.FromInput;
            ti.alphaIsTransparency = false;
        }
    }
}
#endif
