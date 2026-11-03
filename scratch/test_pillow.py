from PIL import Image, ImageDraw, ImageFont, ImageFilter
import os

deliller_dir = r"c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\images\towns\sisoren\deliller"
old_paper_path = os.path.join(deliller_dir, "sisoren_old_paper_bg.png")
pres_paper_path = os.path.join(deliller_dir, "sisoren_prescription_bg.png")
book_bg_path = os.path.join(deliller_dir, "sisoren_book_bg.png")

print("Texture exists:", os.path.exists(old_paper_path), os.path.exists(pres_paper_path))

