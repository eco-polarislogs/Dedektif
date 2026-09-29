const fs = require('fs');
let sisorenJs = fs.readFileSync('wwwroot/js/towns/sisoren/SisorenEngine.js', 'utf8');

const regex = /widget\.style\.cssText = 'position: fixed !important; left: auto !important; right: 20px !important; bottom: 15px !important; z-index: 9999 !important; display: flex;';/g;
const replacement = `widget.style.cssText = 'position: fixed !important; left: auto !important; right: 20px !important; bottom: 15px !important; z-index: 9999 !important; display: none;';`;

sisorenJs = sisorenJs.replace(regex, replacement);

fs.writeFileSync('wwwroot/js/towns/sisoren/SisorenEngine.js', sisorenJs, 'utf8');
