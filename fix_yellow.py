import re

html_path = 'wwwroot/index.html'
with open(html_path, 'r', encoding='utf-8') as f:
    html_content = f.read()

# Add tag-golge to Gölge Şehir
target_html = 'data-town-name="Gölgeşehir">\n                <div class="region-town-tag tag-active">'
repl_html = 'data-town-name="Gölgeşehir">\n                <div class="region-town-tag tag-active tag-golge">'
html_content = html_content.replace(target_html, repl_html)
with open(html_path, 'w', encoding='utf-8') as f:
    f.write(html_content)

css_path = 'wwwroot/css/style.css'
with open(css_path, 'r', encoding='utf-8') as f:
    css_content = f.read()

yellow_css = """
/* Gölge Şehir Sarı Hover ve Glow */
.region-town-tag.tag-golge {
    border-color: #f59e0b;
    box-shadow: 0 0 15px rgba(245, 158, 11, 0.4);
    animation: pulse-glow-golge 2.5s infinite alternate;
}
.region-town-tag.tag-golge:hover {
    box-shadow: 0 0 25px rgba(245, 158, 11, 0.8);
}
@keyframes pulse-glow-golge {
    0% { box-shadow: 0 0 10px rgba(245, 158, 11, 0.3); }
    100% { box-shadow: 0 0 25px rgba(245, 158, 11, 0.7); }
}
.region-town-tag.tag-golge .status-open {
    color: #f59e0b;
    border-color: rgba(245, 158, 11, 0.5);
    background: rgba(245, 158, 11, 0.1);
}
"""

if "tag-golge" not in css_content:
    css_content += yellow_css
    with open(css_path, 'w', encoding='utf-8') as f:
        f.write(css_content)

print("Added yellow hover for Gölge Şehir!")
