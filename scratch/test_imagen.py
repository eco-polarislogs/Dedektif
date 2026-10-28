import urllib.request
import json
import base64

api_key = "AQ.Ab8RN6IYsYr84Jgi7rsUAkfpVtsTsRcOY0vKIxM83YBLiQE5Vw"
url = f"https://generativelanguage.googleapis.com/v1beta/models/imagen-3.0-generate-002:predict?key={api_key}"

prompt = "Black and white noir ink sketch style, middle-aged Turkish mountain weaver woman with braided hair, striped woolen shawl, sitting defeated inside a dark stone jail cell behind vertical black iron bars in the foreground, head bowed down in grief, iron prison door lock on right, dramatic ray of light, 1940s film noir"

payload = {
    "instances": [{"prompt": prompt}],
    "parameters": {"sampleCount": 1, "aspectRatio": "1:1"}
}

try:
    data = json.dumps(payload).encode('utf-8')
    req = urllib.request.Request(url, data=data, headers={'Content-Type': 'application/json'})
    with urllib.request.urlopen(req, timeout=30) as resp:
        res = json.loads(resp.read().decode('utf-8'))
        img_b64 = res['predictions'][0]['bytesBase64Encoded']
        with open(r"c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\images\towns\sisoren\npcler\jail_npc_213.png", "wb") as f:
            f.write(base64.b64decode(img_b64))
        print("Imagen-3 Başarılı!")
except Exception as e:
    print(f"Hata: {e}")
