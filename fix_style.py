
import re
import sys

with open('wwwroot/css/style.css', 'r', encoding='utf-8') as f:
    content = f.read()

replacement = """/* Oynanabilir Kasaba (Gizemli Kasaba) */
.region-town.town-active:hover {
    background: rgba(var(--accent-rgb), 0.14);
    border-color: var(--primary-glow);
    box-shadow: 0 0 30px rgba(var(--accent-rgb), 0.5), inset 0 0 20px rgba(var(--accent-rgb), 0.2);
    transform: scale(1.04);
}

/* Gölge Şehir Özel Hover */
.region-town[data-town-name="Gölgeşehir"].town-active:hover {
    background: rgba(255, 204, 0, 0.14) !important;
    border-color: #ffcc00 !important;
    box-shadow: 0 0 30px rgba(255, 204, 0, 0.5), inset 0 0 20px rgba(255, 204, 0, 0.2) !important;
    transform: scale(1.04) !important;
}"""

content = re.sub(r'/\* Oynanabilir Kasaba \(Gizemli Kasaba\)[^}]*\}', replacement, content)

with open('wwwroot/css/style.css', 'w', encoding='utf-8') as f:
    f.write(content)
