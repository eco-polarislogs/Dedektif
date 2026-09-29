const fs = require('fs');
const glob = require('glob');

const files = glob.sync('wwwroot/js/towns/**/*Config.js');

for (const file of files) {
    let js = fs.readFileSync(file, 'utf8');
    
    // Replace all existing xRatio and yRatio with 0.5, 0.5 for BOTH fingerprintSpot and bloodSpot to ensure they are centered on AI images
    js = js.replace(/fingerprintSpot:\s*\{\s*xRatio:\s*[0-9.]+,\s*yRatio:\s*[0-9.]+/g, 'fingerprintSpot: { xRatio: 0.5, yRatio: 0.5');
    js = js.replace(/bloodSpot:\s*\{\s*xRatio:\s*[0-9.]+,\s*yRatio:\s*[0-9.]+/g, 'bloodSpot: { xRatio: 0.5, yRatio: 0.5');

    fs.writeFileSync(file, js, 'utf8');
}
console.log('All hotspots centered!');
