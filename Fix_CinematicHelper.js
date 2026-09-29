const fs = require('fs');
let appJs = fs.readFileSync('wwwroot/js/app.js', 'utf8');

// Hide when cinematic opens
const hideRegex = /document\.getElementById\('interior-helper-btn'\)\?\.classList\.add\('hidden'\);\s*document\.getElementById\('town-helper-btn'\)\?\.classList\.add\('hidden'\);/g;
const hideReplacement = `document.getElementById('interior-helper-btn')?.classList.add('hidden');
    document.getElementById('town-helper-btn')?.classList.add('hidden');
    if (document.getElementById('bekci-quick-tip-btn')) document.getElementById('bekci-quick-tip-btn').style.display = 'none';
    if (document.getElementById('tuccar-quick-tip-btn')) document.getElementById('tuccar-quick-tip-btn').style.display = 'none';`;
appJs = appJs.replace(hideRegex, hideReplacement);

// Show when cinematic closes (close btn)
const showRegex = /document\.getElementById\('interior-helper-btn'\)\?\.classList\.remove\('hidden'\);\s*document\.getElementById\('town-helper-btn'\)\?\.classList\.remove\('hidden'\);/g;
const showReplacement = `document.getElementById('interior-helper-btn')?.classList.remove('hidden');
    document.getElementById('town-helper-btn')?.classList.remove('hidden');
    if (window.currentActiveTown === 'golge_sehir' && document.getElementById('bekci-quick-tip-btn')) document.getElementById('bekci-quick-tip-btn').style.display = 'flex';
    if (window.currentActiveTown === 'sisoren' && document.getElementById('tuccar-quick-tip-btn')) document.getElementById('tuccar-quick-tip-btn').style.display = 'flex';`;
appJs = appJs.replace(showRegex, showReplacement);

fs.writeFileSync('wwwroot/js/app.js', appJs, 'utf8');
