import urllib.request
import urllib.parse
import os
import time

prompt = "Black and white noir ink sketch, photorealistic charcoal, Turkish mountain weaver woman with braided grey hair, striped shawl, sitting in stone jail cell behind iron bars, high contrast 1940s film noir"
encoded = urllib.parse.quote(prompt)

urls = [
    f"https://image.pollinations.ai/prompt/{encoded}?model=flux&width=1024&height=1024&nologo=true",
    f"https://image.pollinations.ai/prompt/{encoded}?model=turbo&width=1024&height=1024&nologo=true",
    f"https://image.pollinations.ai/prompt/{encoded}?width=768&height=768&nologo=true"
]

dest_path = r"c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\images\towns\sisoren\npcler\jail_npc_213.png"

success = False
for url in urls:
    for attempt in range(2):
        try:
            print(f"Deneme yapılıyor: {url}")
            req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64)'})
            with urllib.request.urlopen(req, timeout=30) as resp, open(dest_path, 'wb') as f:
                f.write(resp.read())
            print("Başarılı!")
            success = True
            break
        except Exception as e:
            print(f"Hata: {e}")
            time.sleep(2)
    if success:
        break

if not success:
    print("Alternatif yol deneniyor...")
