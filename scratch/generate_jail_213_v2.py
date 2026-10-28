import urllib.request
import urllib.parse
import os
import random

prompt = (
    "Black and white noir ink sketch, charcoal pencil crosshatching, "
    "exact same mature Turkish mountain weaver woman character with greying hair braided to one side, "
    "wearing thick dark woolen striped shawl over knitted sweater, "
    "sitting in despair inside a cold stone jail cell behind vertical black iron bars in the foreground, "
    "head bowed down in guilt, iron lock on the right, high barred window with a beam of light, "
    "1940s detective noir masterpiece, high quality, sharp focus"
)

encoded_prompt = urllib.parse.quote(prompt)
seed = random.randint(10000, 999999)
url = f"https://image.pollinations.ai/prompt/{encoded_prompt}?width=1024&height=1024&seed={seed}&nologo=true"

dest_path = r"c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\images\towns\sisoren\npcler\jail_npc_213.png"

print(f"URL: {url}")
req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0'})
with urllib.request.urlopen(req, timeout=60) as resp, open(dest_path, 'wb') as f:
    f.write(resp.read())

print("Başarıyla indirildi!")
