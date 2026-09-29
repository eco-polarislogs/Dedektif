import re
import os

app_path = 'wwwroot/js/app.js'
with open(app_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Remove the bad filters in app.js map click logic
content = re.sub(
    r'currentBag\s*=\s*currentBag\.filter\(c\s*=>\s*c\.id\s*>=\s*1000\);[\s\n]*window\.currentBag\s*=\s*currentBag;',
    '/* Bag filtering removed in favor of state isolation */',
    content
)

content = re.sub(
    r'currentBag\s*=\s*currentBag\.filter\(c\s*=>\s*c\.id\s*<\s*1000\);[\s\n]*window\.currentBag\s*=\s*currentBag;',
    '/* Bag filtering removed in favor of state isolation */',
    content
)

# Replace 'let currentBag = [];' and 'var helperMessageHistory = [];' blocks
# We'll just prepend the proxy logic at the very top of app.js!

proxy_logic = """
// --- TOWN STATE ISOLATION (BAG & HELPER HISTORY) ---
window.townStates = {
    'gizemli': { bag: [], helperHistory: [], helperHistoryIdx: -1 },
    'golge_sehir': { bag: [], helperHistory: [], helperHistoryIdx: -1 },
    'sisoren': { bag: [], helperHistory: [], helperHistoryIdx: -1 }
};
let _currentActiveTown = 'gizemli';

Object.defineProperty(window, 'currentActiveTown', {
    get: function() { return _currentActiveTown; },
    set: function(val) {
        if (_currentActiveTown && window.townStates[_currentActiveTown]) {
            if (typeof currentBag !== 'undefined') {
                window.townStates[_currentActiveTown].bag = [...currentBag];
            }
            if (typeof helperMessageHistory !== 'undefined') {
                window.townStates[_currentActiveTown].helperHistory = [...helperMessageHistory];
            }
            if (typeof currentHelperHistoryIndex !== 'undefined') {
                window.townStates[_currentActiveTown].helperHistoryIdx = currentHelperHistoryIndex;
            }
        }
        _currentActiveTown = val;
        if (!window.townStates[val]) {
            window.townStates[val] = { bag: [], helperHistory: [], helperHistoryIdx: -1 };
        }
        
        if (typeof currentBag !== 'undefined') {
            currentBag = [...window.townStates[val].bag];
            window.currentBag = currentBag;
        }
        if (typeof helperMessageHistory !== 'undefined') {
            helperMessageHistory = [...window.townStates[val].helperHistory];
        }
        if (typeof currentHelperHistoryIndex !== 'undefined') {
            currentHelperHistoryIndex = window.townStates[val].helperHistoryIdx;
        }
        
        if (typeof updateBagUI === 'function') updateBagUI();
        if (typeof checkAutopsyConditions === 'function') checkAutopsyConditions();
    }
});
// ----------------------------------------------------
"""

# Insert right after DOM elements declaration so that currentBag exists later
# Actually, if currentBag is declared with `let`, it's block scoped to the script. But wait, `currentBag` is global.
# We should insert the proxy logic after `let currentBag = [];\nwindow.currentBag = currentBag;`

content = content.replace(
    'let currentBag = [];\nwindow.currentBag = currentBag;',
    'let currentBag = [];\nwindow.currentBag = currentBag;\n' + proxy_logic
)

with open(app_path, 'w', encoding='utf-8') as f:
    f.write(content)
print("State isolation injected into app.js!")
