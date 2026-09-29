const fs = require('fs');
let js = fs.readFileSync('wwwroot/js/towns/sisoren/SisorenConfig.js', 'utf8');

// Replace angle: 15 or angle: -30 or any angle: X with angle: 0
js = js.replace(/angle:\s*-?[0-9]+/g, 'angle: 0');

fs.writeFileSync('wwwroot/js/towns/sisoren/SisorenConfig.js', js, 'utf8');
console.log('Angles fixed.');
