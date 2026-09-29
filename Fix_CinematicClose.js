const fs = require('fs');
let appJs = fs.readFileSync('wwwroot/js/app.js', 'utf8');

// We will just patch the show replacement in app.js
const showRegex2 = /if \(window\.currentActiveTown === 'golge_sehir'[^;]+;\s*if \(window\.currentActiveTown === 'sisoren'[^;]+;/g;
const showReplacement2 = `if (document.body.classList.contains('golge-sehir-theme') && document.getElementById('bekci-quick-tip-btn')) document.getElementById('bekci-quick-tip-btn').style.display = 'flex';
    if (document.body.classList.contains('sisoren-theme') && document.getElementById('tuccar-quick-tip-btn')) document.getElementById('tuccar-quick-tip-btn').style.display = 'flex';`;
appJs = appJs.replace(showRegex2, showReplacement2);

fs.writeFileSync('wwwroot/js/app.js', appJs, 'utf8');
