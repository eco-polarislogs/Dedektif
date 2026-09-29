import os

def unlock_towns():
    path = 'wwwroot/index.html'
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()

    # Unlock Gölgeşehir
    content = content.replace(
        'class="region-town town-locked" style="top: 18%; left: 10%; width: 20%; height: 20%;"\n                data-town-name="Gölgeşehir"',
        'class="region-town town-active" style="top: 18%; left: 10%; width: 20%; height: 20%;"\n                data-town-id="golge_sehir" data-town-name="Gölgeşehir"'
    )
    content = content.replace(
        '<i class="fa-solid fa-lock"></i> GÖLGEŞEHİR\n                    <span class="status-badge status-locked">Kilitli Bölge</span>',
        '<i class="fa-solid fa-magnifying-glass"></i> GÖLGEŞEHİR\n                    <span class="status-badge status-open" style="background:#f59e0b; color:#111;">Soruşturmaya Açık</span>'
    )

    # Unlock Sisören
    content = content.replace(
        'class="region-town town-locked town-sisoren-btn" style="top: 9%; left: 42%; width: 20%; height: 18%;"\n                data-town-id="sisoren" data-town-name="Sisören"',
        'class="region-town town-active town-sisoren-active" style="top: 9%; left: 42%; width: 20%; height: 18%;"\n                data-town-id="sisoren" data-town-name="Sisören"'
    )
    content = content.replace(
        '<i class="fa-solid fa-lock"></i> SİSÖREN\n                    <span class="status-badge status-locked">Kilitli Bölge</span>',
        '<i class="fa-solid fa-magnifying-glass"></i> SİSÖREN\n                    <span class="status-badge status-open" style="background:#f59e0b; color:#111;">Soruşturmaya Açık</span>'
    )
    
    # Unlock in app.js
    app_path = 'wwwroot/js/app.js'
    with open(app_path, 'r', encoding='utf-8') as f:
        app_content = f.read()
    
    app_content = app_content.replace(
        'window.gizemliSolved = false;',
        'window.gizemliSolved = true;'
    )
    app_content = app_content.replace(
        'window.golgeSolved = false;',
        'window.golgeSolved = true;'
    )

    with open(path, 'w', encoding='utf-8') as f:
        f.write(content)
        
    with open(app_path, 'w', encoding='utf-8') as f:
        f.write(app_content)
        
    print("Towns unlocked!")

if __name__ == '__main__':
    unlock_towns()
