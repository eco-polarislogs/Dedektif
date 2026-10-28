from PIL import Image, ImageOps, ImageEnhance
import numpy as np

selma_img = Image.open(r"c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\images\jail_selma.png").convert("RGB").resize((1024, 1024))
target_img = Image.open(r"c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\images\towns\sisoren\npcler\npc_213.jpg").convert("RGB").resize((1024, 1024))

target_gray = ImageOps.grayscale(target_img)
target_noir = ImageEnhance.Contrast(target_gray).enhance(1.4)
target_noir = ImageEnhance.Brightness(target_noir).enhance(0.85).convert("RGB")

selma_np = np.array(selma_img)
target_np = np.array(target_noir)

selma_lum = np.mean(selma_np, axis=2)

# Sadece dikey parmaklık sütunlarındaki KOYU pikseller (demir)
bars_mask = np.zeros((1024, 1024), dtype=bool)

# Belirgin dikey parmaklık merkezleri
centers = [25, 145, 288, 418, 585, 710, 870]
for c in centers:
    # Merkez etrafında 18 piksel ve koyu olanlar
    region = slice(max(0, c - 18), min(1024, c + 18))
    col_mask = selma_lum[:, region] < 70
    bars_mask[:, region] |= col_mask

# Yatay raylar
for r in [128, 835]:
    region = slice(r - 12, r + 12)
    row_mask = selma_lum[region, :] < 70
    bars_mask[region, :] |= row_mask

# Sağ kilit kutusu
kilit_mask = (selma_lum[375:525, 860:1000] < 80)
bars_mask[375:525, 860:1000] |= kilit_mask

# Üst pencere ışığı ve sol/sağ kenar taşları
bars_mask[:, :40] = (selma_lum[:, :40] < 100)
bars_mask[:, 980:] = (selma_lum[:, 980:] < 100)

out = np.where(bars_mask[:, :, None], selma_np, target_np)
Image.fromarray(out).save(r"c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\images\towns\sisoren\npcler\jail_npc_213.png", quality=95)
print("Akıllı parmaklık maskesi uygulandı!")
