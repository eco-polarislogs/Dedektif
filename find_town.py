import re

with open('wwwroot/js/app.js', 'r', encoding='utf-8') as f:
    content = f.read()

matches = re.finditer(r'window\.currentActiveTown\s*=\s*[\'"](.*?)[\'"]', content)
for m in matches:
    print(m.start(), m.group(0))
