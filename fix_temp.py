import os

path = 'scratch_extract/temp_app.js'
with open(path, 'rb') as f:
    raw = f.read()

start_idx = raw.find(b'/\x00/\x00')
if start_idx != -1:
    utf16_data = raw[start_idx:]
    if len(utf16_data) % 2 != 0:
        utf16_data = utf16_data[:-1]
    
    try:
        decoded = utf16_data.decode('utf-16le', errors='ignore')
        with open('wwwroot/js/app.js', 'w', encoding='utf-8') as f:
            f.write(decoded)
        print("Successfully decoded and saved to wwwroot/js/app.js!")
    except Exception as e:
        print("Decode failed:", e)
else:
    print("Could not find start of utf-16 content.")
