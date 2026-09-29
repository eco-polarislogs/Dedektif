import os

def fix_file(path):
    print(f"Processing {path}...")
    try:
        with open(path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Some characters might not be pure latin1 if they were already valid UTF-8.
        # We can try to encode to latin1 and decode to utf-8.
        # If it fails, we fall back to a manual replace of common mojibake.
        try:
            fixed_content = content.encode('cp1252').decode('utf-8')
            with open(path, 'w', encoding='utf-8') as f:
                f.write(fixed_content)
            print(f"Fixed {path} using cp1252->utf8.")
        except Exception as e:
            print(f"Could not fix {path} directly with cp1252: {e}")
            # Try manual replacements for common double-encoded utf-8 chars
            # Map UTF-8 bytes to cp1252 characters
            replacements = {
                'Ä°': 'İ', 'Ä±': 'ı', 'Ã–': 'Ö', 'Ã¶': 'ö', 'Ãœ': 'Ü', 'Ã¼': 'ü',
                'Ã‡': 'Ç', 'Ã§': 'ç', 'Åž': 'Ş', 'ÅŸ': 'ş', 'Äž': 'Ğ', 'ÄŸ': 'ğ',
                'â€œ': '“', 'â€ ': '”', 'â€™': '’', 'â€”': '—', 'â€“': '–', 'Â': '',
                'Ã¢': 'â', 'Ã®': 'î', 'Ã»': 'û', 'âœ“': '✓', 'ğŸŒ²': '🌲',
                'ğÅ¸â€œÂ£': '📣', 'ğŸ‘¥': '👥', 'ğŸŽ²': '🎲', 'ğŸ’¼': '💼'
            }
            new_content = content
            for bad, good in replacements.items():
                new_content = new_content.replace(bad, good)
            if new_content != content:
                with open(path, 'w', encoding='utf-8') as f:
                    f.write(new_content)
                print(f"Fixed {path} using string replacements.")
            else:
                print(f"No changes made to {path}.")
                
    except Exception as e:
        print(f"Error reading {path}: {e}")

if __name__ == '__main__':
    fix_file('wwwroot/js/app.js')
    fix_file('wwwroot/js/towns/sisoren/SisorenEngine.js')
    fix_file('wwwroot/js/towns/golge_sehir/GolgeSehirEngine.js')
    fix_file('wwwroot/index.html')
