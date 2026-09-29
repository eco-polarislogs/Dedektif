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

# Inject state isolation logic
injection = """
let currentBag = [];
window.currentBag = currentBag;
window.townStates = {
    'gizemli': { bag: [] },
    'golge_sehir': { bag: [] },
    'sisoren': { bag: [] }
};
let _currentActiveTown = 'gizemli';

Object.defineProperty(window, 'currentActiveTown', {
    get: function() { return _currentActiveTown; },
    set: function(val) {
        if (_currentActiveTown && window.townStates[_currentActiveTown]) {
            window.townStates[_currentActiveTown].bag = [...currentBag];
        }
        _currentActiveTown = val;
        if (window.townStates[val]) {
            currentBag = [...window.townStates[val].bag];
        } else {
            window.townStates[val] = { bag: [] };
            currentBag = [];
        }
        window.currentBag = currentBag;
        if (typeof updateBagUI === 'function') updateBagUI();
        if (typeof checkAutopsyConditions === 'function') checkAutopsyConditions();
        if (typeof updateAutopsyUI === 'function') updateAutopsyUI();
    }
});
"""

content = content.replace('let currentBag = [];\nwindow.currentBag = currentBag;', injection)

with open(app_path, 'w', encoding='utf-8') as f:
    f.write(content)
print("State isolation injected into app.js!")
