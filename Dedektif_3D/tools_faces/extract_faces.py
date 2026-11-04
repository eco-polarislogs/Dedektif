"""
Portre görsellerinden (wwwroot/images) NPC renk bilgisi (ten / saç / kıyafet) üretir.

Her portre için:
  * OpenCV Haar ile yüz bulunur (bulunamazsa görselin üst-orta bölgesi kullanılır),
  * kafa merkezli kare kırpılır (saç + sakal dahil), kenarları yumuşak elips maskeyle saydamlaşır,
  * ten / saç / kıyafet renkleri örneklenip Resources/Faces/faces.json'a yazılır.

Kullanım:  python -I extract_faces.py <wwwroot/images> <Assets/Resources/Faces> [kontrol_sayfasi.png]
"""
import json
import os
import sys

import cv2
import numpy as np

SIZE = 256


def portraits(root):
    out = {}
    for name in ["dedektif.png", "dedektif_helper.png", "hasan.png", "selma.png", "kemal.png", "gunes.png", "yahya.png"]:
        out[os.path.splitext(name)[0]] = os.path.join(root, name)
    gs = os.path.join(root, "towns", "golge_sehir", "npcler")
    for i in range(101, 109):
        out["npc_%d_talk" % i] = os.path.join(gs, "npc_%d_talk.jpg" % i)
    out["bekci_hasan_helper"] = os.path.join(gs, "bekci_hasan_helper.png")
    out["bekci_ihsan"] = os.path.join(gs, "bekci_ihsan.png")
    so = os.path.join(root, "towns", "sisoren", "npcler")
    for i in list(range(201, 214)) + list(range(301, 319)):
        out["npc_%d" % i] = os.path.join(so, "npc_%d.jpg" % i)
    out["tuccar_ilyas_helper"] = os.path.join(so, "tuccar_ilyas_helper.png")
    return {k: v for k, v in out.items() if os.path.exists(v)}


MODEL = os.environ.get("YUNET_MODEL", os.path.join(os.path.dirname(os.path.abspath(__file__)), "yunet.onnx"))


def detect(img):
    """YuNet (OpenCV FaceDetectorYN): portredeki en büyük ve üst yarıdaki yüzü döndürür."""
    h, w = img.shape[:2]
    scale = 640.0 / max(h, w)
    small = cv2.resize(img, (int(w * scale), int(h * scale)))
    det = cv2.FaceDetectorYN.create(MODEL, "", (small.shape[1], small.shape[0]), 0.6, 0.3, 5000)
    _, faces = det.detect(small)
    if faces is not None and len(faces):
        cands = [f for f in faces if (f[1] + f[3] / 2) / scale < h * 0.65]
        if cands:
            f = max(cands, key=lambda f: f[2] * f[3] * f[14])
            return tuple(float(v) / scale for v in f[:4]), True
    s = w * 0.2
    return (w / 2 - s / 2, h * 0.22, s, s), False


def median_color(img, x0, y0, x1, y1):
    h, w = img.shape[:2]
    x0, x1 = max(0, int(x0)), min(w, int(x1))
    y0, y1 = max(0, int(y0)), min(h, int(y1))
    if x1 <= x0 or y1 <= y0:
        return [0.3, 0.3, 0.3]
    patch = img[y0:y1, x0:x1].reshape(-1, 3).astype(np.float32)
    b, g, r = np.median(patch, axis=0) / 255.0
    return [round(float(r), 3), round(float(g), 3), round(float(b), 3)]


def process(key, path):
    img = cv2.imread(path, cv2.IMREAD_COLOR)
    if img is None:
        return None, None
    h, w = img.shape[:2]
    (fx, fy, fw, fh), ok = detect(img)
    cx, cy = fx + fw / 2.0, fy + fh / 2.0
    # Kafa karesi: yüzün ~1.75 katı, saç için hafif yukarı
    side = fw * 1.75
    x0, y0 = cx - side / 2, cy - side * 0.56
    m = np.float32([[SIZE / side, 0, -x0 * SIZE / side], [0, SIZE / side, -y0 * SIZE / side]])
    crop = cv2.warpAffine(img, m, (SIZE, SIZE), flags=cv2.INTER_AREA, borderMode=cv2.BORDER_REPLICATE)

    # Yumuşak elips maske (yüz + saç çizgisi), dış kenarda tamamen saydam
    yy, xx = np.mgrid[0:SIZE, 0:SIZE].astype(np.float32)
    nx = (xx - SIZE / 2) / (SIZE * 0.40)
    ny = (yy - SIZE * 0.53) / (SIZE * 0.47)
    d = np.sqrt(nx * nx + ny * ny)
    alpha = np.clip((1.0 - d) / 0.16, 0, 1)
    alpha = alpha * alpha * (3 - 2 * alpha)
    rgba = cv2.cvtColor(crop, cv2.COLOR_BGR2BGRA)
    rgba[:, :, 3] = (alpha * 255).astype(np.uint8)

    skin = median_color(img, cx - fw * 0.22, cy - fh * 0.05, cx + fw * 0.22, cy + fh * 0.2)
    hair = median_color(img, cx - fw * 0.3, fy - fh * 0.22, cx + fw * 0.3, fy + fh * 0.02)
    coat = median_color(img, cx - fw * 1.2, cy + fh * 1.7, cx - fw * 0.45, cy + fh * 2.6)
    shirt = median_color(img, cx - fw * 0.18, cy + fh * 1.15, cx + fw * 0.18, cy + fh * 1.6)
    info = {"key": key, "detected": bool(ok), "skin": skin, "hair": hair, "coat": coat, "accent": shirt}
    return rgba, info


def main():
    src, dst = sys.argv[1], sys.argv[2]
    sheet_path = sys.argv[3] if len(sys.argv) > 3 else None
    os.makedirs(dst, exist_ok=True)
    infos, tiles = [], []
    for key, path in sorted(portraits(src).items()):
        rgba, info = process(key, path)
        if rgba is None:
            print("okunamadı:", path)
            continue
        # Yüz resmi karaktere yapıştırılmıyor: yalnızca renk/cinsiyet bilgisi (faces.json) üretilir
        infos.append(info)
        tile = rgba.copy()
        a = tile[:, :, 3:4].astype(np.float32) / 255.0
        tile = (tile[:, :, :3] * a + 40 * (1 - a)).astype(np.uint8)
        cv2.putText(tile, key + ("" if info["detected"] else " ?"), (4, 14), cv2.FONT_HERSHEY_SIMPLEX, 0.4, (0, 255, 255), 1)
        tiles.append(cv2.resize(tile, (160, 160)))
        print(("OK  " if info["detected"] else "YEDEK"), key)
    with open(os.path.join(dst, "faces.json"), "w", encoding="utf-8") as f:
        json.dump({"faces": infos}, f, ensure_ascii=False, indent=1)
    if sheet_path and tiles:
        cols = 8
        rows = (len(tiles) + cols - 1) // cols
        sheet = np.full((rows * 160, cols * 160, 3), 20, np.uint8)
        for i, t in enumerate(tiles):
            r, c = divmod(i, cols)
            sheet[r * 160:(r + 1) * 160, c * 160:(c + 1) * 160] = t
        cv2.imwrite(sheet_path, sheet)


if __name__ == "__main__":
    main()
