import os

replacements = {
    'ğŸŒ²': '🌲',
    'ğÅ¸â€œÂ£': '📣',
    'ğŸ”²': '🔲',
    'ğŸ‘¥': '👥',
    'âœ“': '✓',
    'Åž': 'Ş',
    'ÄŸ': 'ğ',
    'Ã§': 'ç',
    'Ä±': 'ı',
    'Ã¶': 'ö',
    'Ã¼': 'ü',
    'ÅŸ': 'ş',
    'Ã‡': 'Ç',
    'Ä°': 'İ',
    'Ã–': 'Ö',
    'Ãœ': 'Ü',
    'Äž': 'Ğ',
    'ğŸŽ²': '🎲',
    'ğŸ’¼': '💼',
    'Ã¢': 'â',
    'Ã®': 'î'
}

def fix_file(path):
    if not os.path.exists(path):
        return
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original = content
    for bad, good in replacements.items():
        content = content.replace(bad, good)
        
    if content != original:
        with open(path, 'w', encoding='utf-8') as f:
            f.write(content)
        print(f"Fixed {path}")
    else:
        print(f"No changes in {path}")

if __name__ == '__main__':
    fix_file('wwwroot/js/app.js')
    fix_file('wwwroot/index.html')
    fix_file('wwwroot/js/towns/sisoren/SisorenEngine.js')
    fix_file('wwwroot/js/towns/golge_sehir/GolgeSehirEngine.js')
