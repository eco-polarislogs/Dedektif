const fs = require('fs');

let sisorenJs = fs.readFileSync('wwwroot/js/towns/sisoren/SisorenConfig.js', 'utf8');

// We will replace every hotspot object that has fingerprintSpot or bloodSpot and make them logical based on the name/desc.
sisorenJs = sisorenJs.replace(/(\{\s*id:\s*\d+,\s*name:\s*'([^']+)',\s*desc:\s*'([^']+)'[\s\S]*?(?:fingerprintSpot:.*?|bloodSpot:.*?)\})/g, (match, fullMatch, name, desc) => {
    // Basic logic
    let hasBlood = true;
    let hasPrint = true;
    
    const n = name.toLowerCase();
    
    // Items that rarely have blood:
    if (n.includes('kese') || n.includes('para') || n.includes('defter') || n.includes('kitap') || n.includes('şerit') || n.includes('mesaj') || n.includes('fare zehri') || n.includes('kasa') || n.includes('şişe') || n.includes('kutu') || n.includes('sigara') || n.includes('tütün')) {
        if (!n.includes('kan')) {
            hasBlood = false; // logic: paper, poison, tobacco usually doesn't have blood unless specified
        }
    }
    
    // Items that might not have prints (too bloody or fabric)
    if (n.includes('kanlı') && !n.includes('parmak izi')) {
        // Just an example, let's keep prints mostly everywhere
    }

    if (n.includes('eldiven') || n.includes('kumaş') || n.includes('ip') || n.includes('halat')) {
        hasPrint = false;
    }

    // Always ensure at least one is true, just in case
    if (!hasBlood && !hasPrint) {
        hasPrint = true; 
    }

    // Now rewrite the fingerprintSpot and bloodSpot in the match string
    let updatedMatch = fullMatch;
    
    if (!hasBlood) {
        updatedMatch = updatedMatch.replace(/bloodSpot:\s*\{[^}]+\}/, 'bloodSpot: null');
    } else if (updatedMatch.includes('bloodSpot: null')) {
        // If it was null but we decided it should have blood (rare, but just in case), let's give it a generic spot
        updatedMatch = updatedMatch.replace(/bloodSpot:\s*null/, 'bloodSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }');
    }

    if (!hasPrint) {
        updatedMatch = updatedMatch.replace(/fingerprintSpot:\s*\{[^}]+\}/, 'fingerprintSpot: null');
    } else if (updatedMatch.includes('fingerprintSpot: null')) {
        updatedMatch = updatedMatch.replace(/fingerprintSpot:\s*null/, 'fingerprintSpot: { xRatio: 0.5, yRatio: 0.5, angle: 0 }');
    }

    return updatedMatch;
});

fs.writeFileSync('wwwroot/js/towns/sisoren/SisorenConfig.js', sisorenJs, 'utf8');
