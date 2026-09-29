const fs = require('fs');
let styleCss = fs.readFileSync('wwwroot/css/style.css', 'utf8');

const newCSS = `
/* ========================================================= */
/* YARDIMCI TEMA RENKLERİ (İLYAS / RIFAT / ÇETİN)            */
/* ========================================================= */

.ilyas-speaking .cinematic-helper-filmstrip {
    border-color: rgba(74, 222, 128, 0.3) !important;
}
.ilyas-speaking .cinematic-helper-avatar img {
    filter: drop-shadow(0 0 15px rgba(74, 222, 128, 0.4)) !important;
}

.rifat-speaking .cinematic-helper-filmstrip {
    border-color: rgba(234, 179, 8, 0.3) !important;
}
.rifat-speaking .cinematic-helper-avatar img {
    filter: drop-shadow(0 0 15px rgba(234, 179, 8, 0.4)) !important;
}
`;

if (!styleCss.includes('.ilyas-speaking')) {
    styleCss += newCSS;
    fs.writeFileSync('wwwroot/css/style.css', styleCss, 'utf8');
}
