const fs = require('fs');

// 1. Fix app.js
let appJs = fs.readFileSync('wwwroot/js/app.js', 'utf8');

// Fix NPC Image logic in openNpcTalkModal
appJs = appJs.replace(
    /characterLayer\.style\.backgroundImage \= \(window\.currentActiveTown \=\=\= \'sisoren\' \&\& \(npc\.portrait \|\| talkBgImage\)\)/g,
    "characterLayer.style.backgroundImage = ((npc.portrait || talkBgImage))"
);

// Fix returning to map (clear both themes)
appJs = appJs.replace(
    /document\.body\.classList\.remove\('golge-sehir-theme'\);/g,
    "document.body.classList.remove('golge-sehir-theme', 'sisoren-theme');\n                            if(typeof window.resetHelperWidget === 'function') window.resetHelperWidget();"
);

// Add resetHelperWidget function
if (!appJs.includes('resetHelperWidget')) {
    const helperLogic = `
window.resetHelperWidget = function() {
    const tBtn = document.getElementById('town-helper-btn');
    if(tBtn) {
        tBtn.title = 'Yardımcı Dedektif Çetin\\'den İpucu Al';
        tBtn.innerHTML = \`
            <img src="images/dedektif_helper.png?v=11" alt="Yardımcı Dedektif" class="helper-detective-img cetin-img">
            <div class="helper-detective-badge cetin-badge"><i class="fa-solid fa-user-ninja"></i> YARDIMCI DEDEKTİF</div>
        \`;
    }
    const iBtn = document.getElementById('interior-helper-btn');
    if(iBtn) {
        iBtn.title = 'Yardımcı Dedektif Çetin\\'den İpucu Al';
        iBtn.innerHTML = \`
            <img src="images/dedektif_helper.png?v=11" alt="Yardımcı Dedektif" class="helper-detective-img cetin-img">
            <div class="helper-detective-badge cetin-badge"><i class="fa-solid fa-user-ninja"></i> YARDIMCI DEDEKTİF</div>
        \`;
    }
};
`;
    appJs += helperLogic;
}

fs.writeFileSync('wwwroot/js/app.js', appJs, 'utf8');

// 2. Fix style.css
let styleCss = fs.readFileSync('wwwroot/css/style.css', 'utf8');

// Replace hardcoded Gizemli Kasaba hotspot yellow with dynamic theme color
styleCss = styleCss.replace(
    /rgba\(245, 158, 11, 0\.9\)/g,
    "rgba(var(--accent-rgb), 0.9)"
);
styleCss = styleCss.replace(
    /rgba\(251, 191, 36, 0\.8\)/g,
    "rgba(var(--accent-rgb), 0.8)"
);
styleCss = styleCss.replace(
    /rgba\(245, 158, 11, 0\.4\)/g,
    "rgba(var(--accent-rgb), 0.4)"
);

// Add cetin specific classes so he is ALWAYS BLUE
if (!styleCss.includes('.cetin-badge')) {
    styleCss += `
/* Çetin Helper Always Blue */
.cetin-badge {
    border: 1px solid #45f3ff !important;
    color: #45f3ff !important;
}
.helper-detective-widget:hover .cetin-img {
    filter: drop-shadow(0 8px 30px rgba(69, 243, 255, 0.8)) drop-shadow(0 0 40px rgba(69, 243, 255, 0.6)) !important;
}
`;
}

// Change .npc-talk-stage-indicator to use var(--accent) instead of var(--warning) so it matches town theme
styleCss = styleCss.replace(
    /\.npc-talk-stage-indicator \{([\s\S]*?)border: 1px solid var\(--warning\);([\s\S]*?)color: var\(--warning\);/g,
    ".npc-talk-stage-indicator {$1border: 1px solid var(--accent);$2color: var(--accent);"
);

// Fix the Q/A Counter box on Talk modal
styleCss = styleCss.replace(
    /border: 1px solid var\(--warning\);/g,
    "border: 1px solid var(--accent);"
);

fs.writeFileSync('wwwroot/css/style.css', styleCss, 'utf8');

console.log('Fixed app.js and style.css successfully!');
