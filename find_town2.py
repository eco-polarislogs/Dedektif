import re

with open('wwwroot/js/towns/sisoren/SisorenEngine.js', 'r', encoding='utf-8') as f:
    content = f.read()

matches = re.finditer(r'window\.currentActiveTown\s*=\s*[\'"](.*?)[\'"]', content)
for m in matches:
    print("SisorenEngine.js:", m.start(), m.group(0))

with open('wwwroot/js/app.js', 'r', encoding='utf-8') as f:
    content = f.read()
matches = re.finditer(r'currentBag = currentBag\.filter', content)
for m in matches:
    print("app.js:", m.start(), m.group(0))

