# generate_sisoren_pngs.py
"""Generate PNG versions of Sisören clue images.
For each JPEG file named <id>.jpg (or <id>_back.jpg etc.) in
wwwroot/images/towns/sisoren/deliller we create a PNG named
doc_<id>.png if it does not already exist.
The PNG will simply be a copy of the main <id>.jpg image,
converted to PNG using Pillow.
"""
import os
from pathlib import Path
from PIL import Image

base_dir = Path(r"C:/Users/solma/OneDrive/Desktop/Dedektiflik/wwwroot/images/towns/sisoren/deliller")

# ensure output pngs exist
for jpg in base_dir.glob("*.jpg"):
    # ignore back/left/right variants
    if any(suffix in jpg.name for suffix in ["_back", "_left", "_right"]):
        continue
    id_part = jpg.stem  # e.g., "2011"
    png_name = f"doc_{id_part}.png"
    png_path = base_dir / png_name
    if png_path.exists():
        continue
    try:
        img = Image.open(jpg)
        img.save(png_path, "PNG")
        print(f"Created {png_name}")
    except Exception as e:
        print(f"Failed {jpg.name}: {e}")

