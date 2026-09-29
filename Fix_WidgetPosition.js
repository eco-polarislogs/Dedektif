const fs = require('fs');
let styleCss = fs.readFileSync('wwwroot/css/style.css', 'utf8');

// Replace the previous rule
styleCss = styleCss.replace(/\.golge-bekci-widget,\s*\.sisoren-tuccar-widget\s*\{[^}]+\}/, `.golge-bekci-widget, .sisoren-tuccar-widget {
    position: fixed !important;
    left: auto !important;
    right: 20px !important;
    bottom: 15px !important;
    z-index: 9999 !important;
}`);

fs.writeFileSync('wwwroot/css/style.css', styleCss, 'utf8');
