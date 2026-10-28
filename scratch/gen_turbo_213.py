import urllib.request
import urllib.parse
import os
import random

prompt = "masterpiece high contrast black and white noir ink sketch, 50 years old Turkish mountain woman with greying hair braided to one side, wrapped in a thick wool shawl, locked inside a dark stone jail cell behind vertical black iron bars in the foreground, looking down in sorrow and guilt, high barred window with dramatic light beam, stone floor, 1940s detective pulp noir"
encoded = urllib.parse.quote(prompt)
seed = random.randint(1000, 999999)

# Pollinations Turbo
url = f"https://image.pollinations.ai/prompt/{encoded}?model=turbo&width=1024&height=1024&nologo=true&seed={seed}"
dest_path = r"c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\images\towns\sisoren\npcler\jail_npc_213.png"

print("İndiriliyor...")
req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0'})
with urllib.request.urlopen(req, timeout=40) as resp, open(dest_path, 'wb') as f:
    f.write(resp.read())
print("Tamamlandı!")
