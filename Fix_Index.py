import re

with open('wwwroot/index.html', 'r', encoding='utf-8') as f:
    html = f.read()

# Replace golge sehir badge
html = re.sub(
    r'<span class="status-badge status-open">(Soruşturmaya Açık\s*)</span>',
    r'<span class="status-badge status-open status-open-golge">\1</span>',
    html
)

with open('wwwroot/index.html', 'w', encoding='utf-8') as f:
    f.write(html)

with open('wwwroot/css/style.css', 'r', encoding='utf-8') as f:
    css = f.read()

css += """
.status-open-golge {
    background: rgba(245, 158, 11, 0.15) !important;
    border: 1px solid rgba(245, 158, 11, 0.5) !important;
    color: #f59e0b !important;
    box-shadow: 0 0 10px rgba(245, 158, 11, 0.2) !important;
}

.status-open-sisoren {
    background: rgba(46, 164, 79, 0.15) !important;
    border: 1px solid rgba(46, 164, 79, 0.5) !important;
    color: #2ea44f !important;
    box-shadow: 0 0 10px rgba(46, 164, 79, 0.2) !important;
}
"""

with open('wwwroot/css/style.css', 'w', encoding='utf-8') as f:
    f.write(css)
