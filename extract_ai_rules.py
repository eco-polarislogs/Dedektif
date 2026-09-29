import re
import json
from collections import defaultdict

with open('Services/AI/LocalAiEngine.cs', 'r', encoding='utf-8') as f:
    lines = f.readlines()

npc_data = defaultdict(lambda: defaultdict(list))

current_method = "UnknownMethod"
current_state = None # guilty or innocent
for i, line in enumerate(lines):
    m_method = re.search(r'(private|public|internal)\s+(static\s+)?(string|AIInteractionResponse)\s+([A-Za-z0-9_]+)\(', line)
    if m_method:
        current_method = m_method.group(4)
            
    # Match patterns like: (1, 0) => "text"
    m_variant = re.search(r'\(\s*(\d+)\s*,\s*(\d+|_)\s*\)\s*=>\s*"(.*?)"', line)
    if m_variant:
        npc_id = m_variant.group(1)
        text = m_variant.group(3)
        npc_data[npc_id][current_method].append(text)
        continue
        
    # Match patterns like: (1, true) => "text" or (1, false) => "text"
    m_bool = re.search(r'\(\s*(\d+)\s*,\s*(true|false)\s*\)\s*=>\s*"(.*?)"', line)
    if m_bool:
        npc_id = m_bool.group(1)
        is_guilty = m_bool.group(2) == 'true'
        text = m_bool.group(3)
        key = current_method + ("Guilty" if is_guilty else "Innocent")
        npc_data[npc_id][key].append(text)
        continue
        
    # Match patterns like: 1 => "text"
    m_single = re.search(r'^\s*(\d+)\s*=>\s*"(.*?)"', line)
    if m_single:
        npc_id = m_single.group(1)
        text = m_single.group(2)
        npc_data[npc_id][current_method].append(text)
        continue

with open('Data/AI_Rules.json', 'w', encoding='utf-8') as f:
    json.dump({"NPCs": npc_data}, f, ensure_ascii=False, indent=2)

print("Extracted to Data/AI_Rules.json")
