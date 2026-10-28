from PIL import Image, ImageOps, ImageEnhance
import numpy as np

# Görseller
selma_img = Image.open(r"c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\images\jail_selma.png").convert("RGB").resize((1024, 1024))
target_img = Image.open(r"c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\images\towns\sisoren\npcler\npc_213.jpg").convert("RGB").resize((1024, 1024))

# Hedef görseli noir tonuna çevir
target_gray = ImageOps.grayscale(target_img)
enh = ImageEnhance.Contrast(target_gray)
target_noir = enh.enhance(1.4)
target_noir = ImageEnhance.Brightness(target_noir).enhance(0.85).convert("RGB")

selma_arr = np.array(selma_img)
target_arr = np.array(target_noir)

# Selma görselindeki parmaklık pikselleri (çok koyu siyah olan ve dikey sütunlarda yer alanlar)
gray_selma = np.mean(selma_arr, axis=2)

# Parmaklıkların olduğu x aralıkları
bars_mask = np.zeros((1024, 1024), dtype=bool)
x_bars = [
    (0, 50),
    (118, 168),
    (264, 314),
    (395, 445),
    (562, 612),
    (685, 735),
    (855, 1024)
]
for x1, x2 in x_bars:
    # Sadece o sütundaki gerçekten parmaklık olan koyu pikseller
    bars_mask[:, x1:x2] = True

# Yatay raylar
bars_mask[115:140, :] = True
bars_mask[820:845, :] = True

# Selma'nın hücresindeki sol duvar ve sağ kapı/kilit parçalarını da koruyalım
bars_mask[:, :60] = True
bars_mask[:, 860:] = True
bars_mask[:130, :] = True # Üst tavan ve pencere ışığı
bars_mask[830:, :] = True # Zemin ve şişeler

out_arr = np.where(bars_mask[:, :, None], selma_arr, target_arr)
final_img = Image.fromarray(out_arr)
final_img.save(r"c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\images\towns\sisoren\npcler\jail_npc_213.png", quality=95)
print("jail_npc_213 hazırlandı!")
