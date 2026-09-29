import sys

def fix_file(path):
    try:
        with open(path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Reverse the mojibake: encode as cp1252, decode as utf-8
        try:
            fixed_content = content.encode('cp1252').decode('utf-8')
            with open(path, 'w', encoding='utf-8') as f:
                f.write(fixed_content)
            print(f"Successfully fixed {path}")
        except Exception as e:
            print(f"Skipping {path}, might not be pure mojibake: {e}")
    except Exception as e:
        print(f"Error reading {path}: {e}")

if __name__ == '__main__':
    fix_file('wwwroot/js/app.js')
    fix_file('wwwroot/js/towns/sisoren/SisorenEngine.js')
    fix_file('wwwroot/js/towns/golge_sehir/GolgeSehirEngine.js')
