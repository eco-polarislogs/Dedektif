from PIL import Image, ImageOps, ImageEnhance, ImageFilter
import numpy as np

# Dosya yolları
selma_jail_path = r"c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\images\jail_selma.png"
emine_path = r"c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\images\towns\sisoren\npcler\npc_213.jpg"
out_path = r"c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\images\towns\sisoren\npcler\jail_npc_213.png"

# 1. Görselleri yükle
selma_img = Image.open(selma_jail_path).convert("RGBA").resize((1024, 1024))
emine_img = Image.open(emine_path).convert("RGBA").resize((1024, 1024))

# 2. Emine'yi noir siyah-beyaz tonuna çevir ve kontrastını artır
emine_gray = ImageOps.grayscale(emine_img)
enhancer = ImageEnhance.Contrast(emine_gray)
emine_noir = enhancer.enhance(1.4)
enhancer2 = ImageEnhance.Brightness(emine_noir)
emine_noir = enhancer2.enhance(0.85).convert("RGBA")

# 3. Selma görselinden demir parmaklıkları (bars) ve sağdaki kilidi tespit edelim
# Demir parmaklıklar ve kilit çok koyu siyahtır ve dikey çubuklar halindedir
selma_np = np.array(selma_img)
# Parmaklık sütunları yaklaşık koordinatlar: x=0..60, x=130..170, x=260..300, x=400..440, x=560..600, x=690..730, x=850..1024 (kilit kısmı)
# Veya parlaklık maskesi: çok koyu siyah olan ön plan elemanları
bars_mask = np.zeros((1024, 1024), dtype=bool)

# Dikey parmaklıklar
bar_ranges = [
    (0, 50),
    (115, 175),
    (260, 315),
    (390, 445),
    (560, 615),
    (685, 735),
    (855, 1024) # Kilit kutusu ve sağ parmaklık
]
for x1, x2 in bar_ranges:
    bars_mask[:, x1:x2] = True

# Yatay bağlantı çubukları (üst ve alt)
bars_mask[110:145, :] = True
bars_mask[815:855, :] = True

# Selma görselindeki parmaklık piksellerini al, diğer yerlere Emine'yi koy
result_np = np.array(emine_noir)
for c in range(3):
    result_np[:, :, c] = np.where(bars_mask, selma_np[:, :, c], result_np[:, :, c])

# Arka plan duvar ve ışık efektini hafif harmanla
result_img = Image.fromarray(result_np).convert("RGB")
result_img.save(out_path, quality=95)
print("Kompozit başarıyla oluşturuldu!")
