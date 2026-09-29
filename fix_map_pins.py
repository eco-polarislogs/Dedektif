import re

app_path = 'wwwroot/js/app.js'
with open(app_path, 'r', encoding='utf-8') as f:
    content = f.read()

target = """        if (typeof updateBagUI === 'function') updateBagUI();
        if (typeof checkAutopsyConditions === 'function') checkAutopsyConditions();
    }
});"""

repl = """        if (typeof updateBagUI === 'function') updateBagUI();
        if (typeof checkAutopsyConditions === 'function') checkAutopsyConditions();

        // Harita üzerindeki binaları kasabaya göre gizle/göster
        if (typeof document !== 'undefined') {
            const mapStage = document.getElementById('town-map-stage');
            if (mapStage) {
                const allBuildings = mapStage.querySelectorAll('.map-building');
                allBuildings.forEach(el => {
                    if (val === 'gizemli') {
                        if (el.className.includes('building-golge-') || el.className.includes('building-sisoren-')) {
                            el.style.display = 'none';
                        } else {
                            el.style.display = 'block';
                        }
                    } else if (val === 'golge_sehir') {
                        if (el.className.includes('building-golge-')) {
                            el.style.display = 'block';
                        } else {
                            el.style.display = 'none';
                        }
                    } else if (val === 'sisoren') {
                        if (el.className.includes('building-sisoren-')) {
                            el.style.display = 'block';
                        } else {
                            el.style.display = 'none';
                        }
                    }
                });
            }
        }
    }
});"""

content = content.replace(target, repl)

with open(app_path, 'w', encoding='utf-8') as f:
    f.write(content)

print("Map pins fix applied!")
