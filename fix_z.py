import re
content = open('wwwroot/js/towns/sisoren/SisorenEngine.js', 'r', encoding='utf-8').read()
content = content.replace("marker.style.zIndex = '60';", "marker.style.zIndex = '10';")
open('wwwroot/js/towns/sisoren/SisorenEngine.js', 'w', encoding='utf-8').write(content)
