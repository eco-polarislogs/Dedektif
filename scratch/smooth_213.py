from PIL import Image, ImageOps, ImageEnhance, ImageFilter
import cv2
import numpy as np

img = cv2.imread(r"c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\images\towns\sisoren\npcler\jail_npc_213.png")
# Hafif bilateral filtre uygulayarak çizgi geçişlerini doğal illüstrasyona yedirelim
smooth = cv2.bilateralFilter(img, 5, 50, 50)
cv2.imwrite(r"c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\images\towns\sisoren\npcler\jail_npc_213.png", smooth)
print("Pürüzsüzleştirme tamamlandı!")
