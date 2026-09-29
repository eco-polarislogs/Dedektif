const fs = require('fs');
let styleCss = fs.readFileSync('wwwroot/css/style.css', 'utf8');

// Add classes for right-aligned helpers
if (!styleCss.includes('.golge-bekci-widget')) {
    styleCss += `\n
.golge-bekci-widget, .sisoren-tuccar-widget {
    left: auto !important;
    right: 20px !important;
}
`;
    fs.writeFileSync('wwwroot/css/style.css', styleCss, 'utf8');
}

let sisorenJs = fs.readFileSync('wwwroot/js/towns/sisoren/SisorenEngine.js', 'utf8');
const regex = /createTuccarHelperWidget:\s*function\s*\(\)\s*\{[\s\S]*?triggerTuccarTip:/;
const original = `createTuccarHelperWidget: function () {
        let widget = document.getElementById('tuccar-quick-tip-btn');
        if (widget) widget.remove();

        widget = document.createElement('div');
        widget.id = 'tuccar-quick-tip-btn';
        widget.className = 'helper-detective-widget sisoren-tuccar-widget';
        widget.title = 'Tüccar İlyas\\'tan İpucu Al';
        widget.innerHTML = \`
            <img src="images/towns/sisoren/npcler/tuccar_ilyas_helper.png" class="helper-detective-img" alt="Tüccar İlyas">
            <div class="helper-detective-badge">TÜCCAR İLYAS</div>
        \`;

        widget.addEventListener('click', (e) => {
            e.stopPropagation();
            this.triggerTuccarTip();
        });

        const mapStage = document.getElementById('town-map-stage');
        const interiorStage = document.getElementById('interior-stage');
        
        if (interiorStage && !interiorStage.classList.contains('hidden')) {
            interiorStage.appendChild(widget);
        } else if (mapStage) {
            mapStage.appendChild(widget);
        } else {
            document.body.appendChild(widget);
        }
    },

    triggerTuccarTip:`;

sisorenJs = sisorenJs.replace(regex, original);

// Also need to handle interior transitions for Tuccar, so he shows up inside buildings!
// Wait, GolgeSehirEngine handles interior/map by appending to body, but with high z-index.
// Let's just append to document.body, but make sure it's removed on exit (which we already fixed).
let sisorenJsAppended = sisorenJs.replace(/if \(interiorStage.*?else \{.*?document\.body\.appendChild\(widget\);.*?\}/s, "document.body.appendChild(widget);");
fs.writeFileSync('wwwroot/js/towns/sisoren/SisorenEngine.js', sisorenJsAppended, 'utf8');
