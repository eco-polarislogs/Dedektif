const fs = require('fs');
let css = fs.readFileSync('wwwroot/css/style.css', 'utf8');
css = css.replace(/rgba\(\s*69\s*,\s*243\s*,\s*255\s*,\s*([0-9.]+)\s*\)/g, 'rgba(var(--accent-rgb), $1)');
fs.writeFileSync('wwwroot/css/style.css', css);
