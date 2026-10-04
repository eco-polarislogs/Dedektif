import re

def fix_portrait(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    def replace_null(match):
        return f"portrait: 'images/towns/sisoren/npcler/npc_{match.group(1)}.jpg',"
        
    # Match numericId: 123, followed by some spaces/lines, then portrait: null,
    # Ensure we don't match across different object boundaries.
    # The safest way is to split by {id: '...' } and fix each.
    
    parts = re.split(r'(id:\s*\'[^\']+\',)', content)
    new_parts = []
    
    for part in parts:
        if 'portrait: null' in part and 'numericId:' in part:
            num_match = re.search(r'numericId:\s*(\d+)', part)
            if num_match:
                num = num_match.group(1)
                part = part.replace('portrait: null', f"portrait: 'images/towns/sisoren/npcler/npc_{num}.jpg'")
        new_parts.append(part)
        
    new_content = "".join(new_parts)
    
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(new_content)

fix_portrait('wwwroot/js/towns/sisoren/SisorenConfig.js')
