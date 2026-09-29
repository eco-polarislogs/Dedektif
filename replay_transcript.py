import json
import os
import sys

def apply_edits(repo_path, transcript_path):
    print(f"Reading transcript {transcript_path}...")
    with open(transcript_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    
    print(f"Found {len(lines)} lines.")
    for line in lines:
        try:
            step = json.loads(line)
        except:
            continue
        
        if step.get('type') != 'PLANNER_RESPONSE':
            continue
            
        tool_calls = step.get('tool_calls', [])
        for tool in tool_calls:
            name = tool.get('name')
            if name not in ('write_to_file', 'replace_file_content', 'multi_replace_file_content'):
                continue
                
            args = tool.get('args', {})
            
            # Extract target file path and normalize it
            target_file = args.get('TargetFile') or args.get('targetFile')
            if not target_file:
                continue
            
            # Only care about files in Dedektiflik
            if 'Dedektiflik' not in target_file:
                continue
                
            # Make the path relative and apply to repo_path
            rel_path = target_file.split('Dedektiflik')[-1].lstrip('\\/')
            abs_path = os.path.join(repo_path, rel_path)
            
            print(f"Applying {name} to {rel_path}...")
            
            if name == 'write_to_file':
                content = args.get('CodeContent') or args.get('codeContent') or ""
                os.makedirs(os.path.dirname(abs_path), exist_ok=True)
                with open(abs_path, 'w', encoding='utf-8') as out:
                    out.write(content)
                    
            elif name == 'replace_file_content':
                if not os.path.exists(abs_path):
                    print(f"  Warning: {abs_path} doesn't exist for replace_file_content.")
                    continue
                with open(abs_path, 'r', encoding='utf-8') as f:
                    content = f.read()
                
                target = args.get('TargetContent') or args.get('targetContent') or ""
                replacement = args.get('ReplacementContent') or args.get('replacementContent') or ""
                
                if target in content:
                    content = content.replace(target, replacement, 1)
                    with open(abs_path, 'w', encoding='utf-8') as out:
                        out.write(content)
                    print("  Success")
                else:
                    print("  Warning: TargetContent not found!")
                    
            elif name == 'multi_replace_file_content':
                if not os.path.exists(abs_path):
                    print(f"  Warning: {abs_path} doesn't exist for multi_replace.")
                    continue
                with open(abs_path, 'r', encoding='utf-8') as f:
                    content = f.read()
                
                chunks = args.get('ReplacementChunks') or args.get('replacementChunks') or []
                success_count = 0
                for chunk in chunks:
                    target = chunk.get('TargetContent') or chunk.get('targetContent') or ""
                    replacement = chunk.get('ReplacementContent') or chunk.get('replacementContent') or ""
                    if target in content:
                        content = content.replace(target, replacement, 1)
                        success_count += 1
                    else:
                        print(f"  Warning: TargetContent not found for a chunk!")
                        
                with open(abs_path, 'w', encoding='utf-8') as out:
                    out.write(content)
                print(f"  Applied {success_count}/{len(chunks)} chunks.")

if __name__ == '__main__':
    repo = sys.argv[1]
    transcript = sys.argv[2]
    apply_edits(repo, transcript)
