const fs = require('fs');
let appJs = fs.readFileSync('wwwroot/js/app.js', 'utf8');

const regex = /<div class="found-npc-photo-wrap sisoren-placeholder-photo sisoren-extra-photo" data-npc-id="\$\{extraId\}" title="\$\{extraNpc\.name\}">\s*<i class="fa-solid fa-user sisoren-placeholder-icon" style="color: #6ee7b7;"><\/i>\s*<span class="sisoren-placeholder-text" style="font-size: 0\.65rem;">\$\{extraNpc\.role \|\| 'Kasabalı'\}<\/span>\s*<\/div>/g;

const replacement = `<div class="found-npc-photo-wrap" data-npc-id="\${extraId}" title="\${extraNpc.name}">
                    <img src="\${extraNpc.avatar || \`images/towns/sisoren/npcler/\${extraId}.png\`}" class="found-npc-photo" alt="\${extraNpc.name}" onerror="this.src='images/towns/sisoren/npcler/default_extra.png'; this.onerror=null;">
                </div>`;

appJs = appJs.replace(regex, replacement);
fs.writeFileSync('wwwroot/js/app.js', appJs, 'utf8');
