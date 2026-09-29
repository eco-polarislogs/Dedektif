import re

with open('wwwroot/add_signs_v2.html', 'r', encoding='utf-8') as f:
    lines = f.readlines()

new_lines = []
in_string = False
for line in lines:
    stripped = line.strip()
    if not in_string and 'SecretInfo' in line and '=' in line and '\"' in line.split('=')[1]:
        if not line.rstrip().endswith('}') and not line.rstrip().endswith('\";') and not line.rstrip().endswith('\",'):
            new_lines.append(line.rstrip())
            in_string = True
            continue
            
    if in_string:
        if '}' in line or '\";' in line or '\",' in line:
            new_lines[-1] = new_lines[-1] + ' ' + line.lstrip()
            in_string = False
        else:
            new_lines[-1] = new_lines[-1] + ' ' + line.strip()
        continue
        
    new_lines.append(line)

with open('Controllers/GameEndpoints.cs', 'w', encoding='utf-8') as f:
    f.writelines(new_lines)
