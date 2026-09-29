const fs = require('fs');

let appJs = fs.readFileSync('wwwroot/js/app.js', 'utf8');

appJs = appJs.replace(
    /window\.SisorenEngine\.clearSisorenMap\(\);\s*\}/,
    `window.SisorenEngine.clearSisorenMap();
                    }
                    const tuccar = document.getElementById('tuccar-quick-tip-btn');
                    if (tuccar) tuccar.remove();
                    const cinematicBox = document.getElementById('cinematic-helper-box');
                    if (cinematicBox) cinematicBox.classList.remove('visible');`
);

fs.writeFileSync('wwwroot/js/app.js', appJs, 'utf8');

let sisorenJs = fs.readFileSync('wwwroot/js/towns/sisoren/SisorenEngine.js', 'utf8');
sisorenJs = sisorenJs.replace(
    /document\.body\.classList\.remove\('sisoren-theme'\);/,
    `document.body.classList.remove('sisoren-theme');
        const tuccar = document.getElementById('tuccar-quick-tip-btn');
        if (tuccar) tuccar.remove();`
);
fs.writeFileSync('wwwroot/js/towns/sisoren/SisorenEngine.js', sisorenJs, 'utf8');
