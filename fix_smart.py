import re

def fix_mixed_encoding(text):
    # Find sequences of 2 or more non-ascii characters (or specific ones like â€œ)
    # Actually, UTF-8 encoded as cp1252 usually produces 2-4 byte sequences.
    # We can use a regex to match sequences of characters in the range \x80-\xFF
    
    def replacer(match):
        s = match.group(0)
        try:
            return s.encode('cp1252').decode('utf-8')
        except:
            return s

    # Match 2 or more characters in \x80-\xFF, or specific known ones
    pattern = re.compile(r'[\x80-\xff]+')
    
    # We need to be careful: if a sequence is ALREADY valid utf-8, python's read() decoded it to a unicode character!
    # Wait, if Python read() it as utf-8, then the MOJIBAKE characters (which were utf-8 bytes read as cp1252)
    # are NOW unicode characters corresponding to the cp1252 codepoints!
    # So `encode('cp1252')` will convert them back to the original UTF-8 bytes!
    # And then `decode('utf-8')` will parse those bytes as the correct UTF-8 string!
    # If the text has a correct 'Ş' (U+015E), encode('cp1252') will FAIL because U+015E is not in cp1252!
    # This is PERFECT! It will only decode the mojibake and leave the correct characters alone!
    
    # We just need to split the string into words, or just apply it to the whole string?
    # No, we can't apply it to the whole string because one 'Ş' will fail the whole string.
    # So we apply it to every chunk of characters that CAN be encoded in cp1252!
    
    fixed_chunks = []
    current_chunk = ""
    for char in text:
        try:
            char.encode('cp1252')
            current_chunk += char
        except UnicodeEncodeError:
            # We hit a character that is NOT in cp1252 (like a valid 'Ş' or 'İ')
            # So process the current chunk
            if current_chunk:
                try:
                    # Try to un-mojibake the chunk
                    decoded = current_chunk.encode('cp1252').decode('utf-8')
                    fixed_chunks.append(decoded)
                except UnicodeDecodeError:
                    # If it's not valid utf-8, just keep it as is
                    fixed_chunks.append(current_chunk)
                current_chunk = ""
            fixed_chunks.append(char)
            
    if current_chunk:
        try:
            decoded = current_chunk.encode('cp1252').decode('utf-8')
            fixed_chunks.append(decoded)
        except UnicodeDecodeError:
            fixed_chunks.append(current_chunk)
            
    return "".join(fixed_chunks)

def process_file(path):
    print(f"Processing {path}...")
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()
        
    fixed = fix_mixed_encoding(content)
    if fixed != content:
        with open(path, 'w', encoding='utf-8') as f:
            f.write(fixed)
        print(f"Fixed {path}")
    else:
        print(f"No changes {path}")

if __name__ == '__main__':
    process_file('wwwroot/js/app.js')
    process_file('wwwroot/index.html')
    process_file('wwwroot/js/towns/sisoren/SisorenEngine.js')
    process_file('wwwroot/js/towns/golge_sehir/GolgeSehirEngine.js')
