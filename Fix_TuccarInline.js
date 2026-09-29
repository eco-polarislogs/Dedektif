const fs = require('fs');
let sisorenJs = fs.readFileSync('wwwroot/js/towns/sisoren/SisorenEngine.js', 'utf8');

const regex = /widget\.className = 'helper-detective-widget sisoren-tuccar-widget';\s*widget\.title = 'Tüccar İlyas\\'tan İpucu Al';/g;
const replacement = `widget.className = 'helper-detective-widget sisoren-tuccar-widget';
        widget.title = 'Tüccar İlyas\\'tan İpucu Al';
        widget.style.cssText = 'position: fixed !important; left: auto !important; right: 20px !important; bottom: 15px !important; z-index: 9999 !important; display: flex;';`;

sisorenJs = sisorenJs.replace(regex, replacement);

fs.writeFileSync('wwwroot/js/towns/sisoren/SisorenEngine.js', sisorenJs, 'utf8');
