import re

with open('wwwroot/js/app.js', 'r', encoding='utf-8') as f:
    content = f.read()

target1 = """    // Kasaba delil kontrolü (Gölge Şehir'de sadece 1000+, Gizemli Kasaba'da <1000)
    const isGolge = (window.currentActiveTown === 'golge_sehir');
    if (isGolge && currentPendingObject.id < 1000) {
        showGlobalNotification('Uyarı', 'Bu delil başka bir kasabaya aittir.', true);
        buildingClueModal.classList.add('hidden');
        return;
    } else if (!isGolge && currentPendingObject.id >= 1000) {
        showGlobalNotification('Uyarı', 'Bu delil başka bir kasabaya aittir.', true);
        buildingClueModal.classList.add('hidden');
        return;
    }"""

repl1 = """    // Kasaba delil kontrolü (Gölge Şehir'de 1000-1999, Gizemli Kasaba'da <1000, Sisören'de >= 2000)
    const activeTown = window.currentActiveTown;
    const objId = currentPendingObject.id;
    let wrongTown = false;
    if (activeTown === 'gizemli' && objId >= 1000) wrongTown = true;
    if (activeTown === 'golge_sehir' && (objId < 1000 || objId >= 2000)) wrongTown = true;
    if (activeTown === 'sisoren' && objId < 2000) wrongTown = true;

    if (wrongTown) {
        showGlobalNotification('Uyarı', 'Bu delil başka bir kasabaya aittir.', true);
        buildingClueModal.classList.add('hidden');
        return;
    }"""

target2 = """    // Kasaba delil kontrolü (Gölge Şehir'de sadece 1000+, Gizemli Kasaba'da <1000)
    const isGolge = (window.currentActiveTown === 'golge_sehir');
    if (isGolge && currentPendingObject.id < 1000) {
        showGlobalNotification('Uyarı', 'Bu delil başka bir kasabaya aittir.', true);
        clueInspectModal.classList.add('hidden');
        return;
    } else if (!isGolge && currentPendingObject.id >= 1000) {
        showGlobalNotification('Uyarı', 'Bu delil başka bir kasabaya aittir.', true);
        clueInspectModal.classList.add('hidden');
        return;
    }"""

repl2 = """    // Kasaba delil kontrolü (Gölge Şehir'de 1000-1999, Gizemli Kasaba'da <1000, Sisören'de >= 2000)
    const activeTown = window.currentActiveTown;
    const objId = currentPendingObject.id;
    let wrongTown = false;
    if (activeTown === 'gizemli' && objId >= 1000) wrongTown = true;
    if (activeTown === 'golge_sehir' && (objId < 1000 || objId >= 2000)) wrongTown = true;
    if (activeTown === 'sisoren' && objId < 2000) wrongTown = true;

    if (wrongTown) {
        showGlobalNotification('Uyarı', 'Bu delil başka bir kasabaya aittir.', true);
        clueInspectModal.classList.add('hidden');
        return;
    }"""

content = content.replace(target1, repl1).replace(target2, repl2)

with open('wwwroot/js/app.js', 'w', encoding='utf-8') as f:
    f.write(content)

print("Fixed clue town check!")
