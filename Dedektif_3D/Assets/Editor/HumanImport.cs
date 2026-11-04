using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Assets/Resources/Humans altına konan FBX'leri insan simülasyonu için otomatik ayarlar:
///  • Rig = Humanoid (animasyonlar her karaktere aktarılabilsin), avatar dosyanın kendisinden
///  • Characters / Parts: malzemeler FBX'in içinden; Parts mesh'leri okunabilir (baş kesme + kemik bağlama için)
///  • Animations: tek klipli dosyada klip adı = dosya adı (Mixamo hepsini "mixamo.com" adlandırır);
///    çok klipli kütüphanelerde (Quaternius UAL) yalnızca 1800'ler kasabasına uyan klipler seçilip
///    "tür_çeşit" adıyla (idle_1, walk_formal, sit_talk ...) içe alınır. Kök hareketi poza gömülür.
///  • Dokular: 4K kaynaklar oyunda 1K–2K'ya indirilir, *_Normal dokular normal map olur.
/// </summary>
public class HumanImport : AssetPostprocessor
{
    string P => assetPath.Replace('\\', '/');
    /// <summary>Ayar değişince Unity varlıkları yeniden içe aktarsın.</summary>
    public override uint GetVersion() => 4;

    bool InHumans => P.Contains("/Resources/Humans/");
    bool IsAnimation => P.Contains("/Resources/Humans/Animations/");

    /// <summary>Kütüphane klibi → oyundaki ad. Listede olmayan klipler (kılıç, büyü, zombi, telefon...) alınmaz.</summary>
    static readonly Dictionary<string, string> LibraryClips = new Dictionary<string, string>
    {
        // Universal Animation Library 1
        { "Idle_Loop", "idle_1" }, { "Walk_Loop", "walk_1" }, { "Walk_Formal_Loop", "walk_formal" },
        { "Jog_Fwd_Loop", "run_jog" }, { "Idle_Talking_Loop", "talk_1" },
        { "Sitting_Idle_Loop", "sit_1" }, { "Sitting_Talking_Loop", "sit_talk" },
        { "Fixing_Kneeling", "kneel_fix" }, { "Crouch_Idle_Loop", "crouch_1" },
        { "Interact", "interact_1" }, { "PickUp_Table", "pickup_1" }, { "Death01", "dead_1" },
        { "Push_Loop", "push_1" },
        // Universal Animation Library 2
        { "Idle_FoldArms_Loop", "idle_arms" }, { "Idle_Lantern_Loop", "lantern_1" },
        { "Idle_No_Loop", "no_1" }, { "Yes", "yes_1" }, { "Idle_Rail_Loop", "lean_1" },
        { "Consume", "drink_1" }, { "Walk_Carry_Loop", "carry_1" }, { "TreeChopping_Loop", "chop_1" },
        { "Chest_Open", "interact_chest" },
    };
    /// <summary>Bir kez oynayıp son karede kalan klipler.</summary>
    static readonly HashSet<string> OneShot = new HashSet<string> { "dead_1", "yes_1", "interact_1", "pickup_1", "interact_chest", "drink_1" };

    void OnPreprocessModel()
    {
        if (!InHumans) return;
        var mi = (ModelImporter)assetImporter;
        mi.animationType = ModelImporterAnimationType.Human;
        mi.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        mi.importAnimation = IsAnimation;
        mi.importBlendShapes = true;
        mi.isReadable = !IsAnimation;
        mi.materialImportMode = IsAnimation ? ModelImporterMaterialImportMode.None : ModelImporterMaterialImportMode.ImportViaMaterialDescription;
        if (IsAnimation) mi.importCameras = mi.importLights = false;
    }

    void OnPreprocessAnimation()
    {
        if (!IsAnimation) return;
        var mi = (ModelImporter)assetImporter;
        var clips = mi.defaultClipAnimations;
        string file = Path.GetFileNameWithoutExtension(assetPath);
        var keep = new List<ModelImporterClipAnimation>();
        for (int i = 0; i < clips.Length; i++)
        {
            var c = clips[i];
            string take = c.takeName ?? c.name ?? "";
            int bar = take.LastIndexOf('|');
            if (bar >= 0) take = take.Substring(bar + 1);
            if (clips.Length > 2)
            {
                if (!LibraryClips.TryGetValue(take, out var nm)) continue;
                c.name = nm;
            }
            else c.name = clips.Length == 1 ? file : file + "_" + i;
            c.loopTime = !OneShot.Contains(c.name);
            c.lockRootRotation = true;
            c.lockRootHeightY = true;
            c.lockRootPositionXZ = true;
            // Gövde yönüne göre hizala: Unreal / Unity dışa aktarımlı kütüphaneler aynı yöne baksın
            c.keepOriginalOrientation = false;
            c.rotationOffset = 0f;
            c.keepOriginalPositionY = true;
            c.keepOriginalPositionXZ = true;
            keep.Add(c);
        }
        mi.clipAnimations = keep.ToArray();
    }

    void OnPreprocessTexture()
    {
        if (!InHumans) return;
        var ti = (TextureImporter)assetImporter;
        string n = Path.GetFileNameWithoutExtension(assetPath);
        bool normal = n.EndsWith("_Normal");
        ti.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
        ti.sRGBTexture = !normal;
        bool outfit = n.Contains("Peasant") || n.Contains("Ranger");
        ti.maxTextureSize = outfit ? 2048 : 1024;
        ti.mipmapEnabled = true;
        // Kıyafet atlasları: kumaş / ten bölgeleri çalışma anında üçgen üçgen ayrılır (okunabilir olmalı)
        ti.isReadable = !normal && (outfit || n.StartsWith("T_Skin"));
        ti.textureCompression = TextureImporterCompression.Compressed;
    }
}
