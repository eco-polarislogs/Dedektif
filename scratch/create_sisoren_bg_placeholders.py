from PIL import Image, ImageDraw, ImageFont
import os

# Directory for sisoren background images
dir_path = r"C:/Users/solma/OneDrive/Desktop/Dedektiflik/wwwroot/images/towns/sisoren/deliller"
os.makedirs(dir_path, exist_ok=True)

# Define placeholder specs: name and color
placeholders = [
    ("sisoren_telegraph_bg.png", (200, 180, 150)),
    ("sisoren_kiosk_ticket_bg.png", (220, 200, 170)),
    ("sisoren_burnt_bg.png", (100, 80, 60)),
]

for filename, color in placeholders:
    path = os.path.join(dir_path, filename)
    img = Image.new('RGB', (800, 600), color)
    draw = ImageDraw.Draw(img)
    # Optional: add text label
    try:
        font = ImageFont.truetype('arial.ttf', 40)
    except Exception:
        font = ImageFont.load_default()
    text = filename.replace('_bg.png', '').replace('sisoren_', '').replace('_', ' ').title()
    bbox = draw.textbbox((0, 0), text, font=font)
    w, h = bbox[2] - bbox[0], bbox[3] - bbox[1]
    draw.text(((800-w)/2, (600-h)/2), text, fill=(255,255,255), font=font)
    img.save(path)
    print(f"Created {path}")

