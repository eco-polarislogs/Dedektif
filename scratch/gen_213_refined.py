from PIL import Image, ImageOps, ImageEnhance, ImageFilter
import urllib.request
import urllib.parse
import os

# 1. npc_213'ü aç ve noir tonuna çevir
npc_path = r"c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\images\towns\sisoren\npcler\npc_213.jpg"
jail_ref_path = r"c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\images\jail_selma.png"

# Pollinations ile özel noir hücre oluşturalım
prompt = "charcoal ink sketch of vertical prison iron bars, empty cell bed, stone wall with GUILTY text, dramatic light beam, black and white noir"
url = f"https://image.pollinations.ai/prompt/{urllib.parse.quote(prompt)}?width=1024&height=1024&nologo=true"

# Alternatif olarak kaliteli bir FLUX prompt deneyelim:
prompt_full = "masterpiece black and white charcoal drawing, mature Turkish weaver woman with braided hair sitting defeated inside dark stone prison cell behind heavy black iron bars, head in hands, dramatic chiaroscuro lighting, 1940s film noir"
url_full = f"https://image.pollinations.ai/prompt/{urllib.parse.quote(prompt_full)}?width=1024&height=1024&nologo=true&seed=49218"

dest_path = r"c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\images\towns\sisoren\npcler\jail_npc_213.png"

print("İndiriliyor...")
req = urllib.request.Request(url_full, headers={'User-Agent': 'Mozilla/5.0'})
try:
    with urllib.request.urlopen(req, timeout=40) as resp, open(dest_path, 'wb') as f:
        f.write(resp.read())
    print("Tamamlandı!")
except Exception as e:
    print(f"Hata: {e}")
