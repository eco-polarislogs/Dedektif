import codecs

# Read lines from the original app.js which is utf-8 with BOM
with codecs.open('scratch_original_app.js', 'r', 'utf-8-sig') as f:
    lines = f.readlines()

# Extract the lines we need
# openNpcTalk starts at line 2675 (index 2674) to 3517 (index 3516)
dialog_lines = lines[2674:3517]

# showCinematicHelper starts at line 4257 (index 4256) to 4569 (index 4568)
helper_lines = lines[4256:4569]

# Combine them
final_lines = dialog_lines + helper_lines

# Add the proxy for openBag and exports
exports = """

window.openBag = function() {
    if (window.inventoryManager) {
        window.inventoryManager.open();
    } else {
        console.error('inventoryManager not found!');
    }
};

window.openNpcTalk = openNpcTalk;
window.showCinematicHelper = showCinematicHelper;
window.triggerHelperMessage = triggerHelperMessage;
window.reopenHelperSpeechBubble = reopenHelperSpeechBubble;
"""
final_lines.append(exports)

# Write to DialogManager.js with utf-8 encoding (no BOM needed for HTML5 script tags usually, but we can just use utf-8)
with codecs.open('wwwroot/js/managers/DialogManager.js', 'w', 'utf-8') as f:
    f.writelines(final_lines)

print("DialogManager.js successfully generated with UTF-8 encoding.")
