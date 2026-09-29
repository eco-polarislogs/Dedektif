const fs = require('fs');
let sisorenJs = fs.readFileSync('wwwroot/js/towns/sisoren/SisorenEngine.js', 'utf8');

const regex = /createTuccarHelperWidget:\s*function\s*\(\)\s*\{[\s\S]*?triggerTuccarTip:/;
const replacement = `createTuccarHelperWidget: function () {
        const helpers = [document.getElementById('town-helper-btn'), document.getElementById('interior-helper-btn')];
        helpers.forEach(widget => {
            if (widget) {
                widget.innerHTML = \`
                    <img src="images/towns/sisoren/npcler/tuccar_ilyas_helper.png" class="helper-detective-img" alt="Tüccar İlyas">
                    <div class="helper-detective-badge">TÜCCAR İLYAS</div>
                \`;
                
                // Remove all previous click listeners by replacing the element
                const newWidget = widget.cloneNode(true);
                widget.parentNode.replaceChild(newWidget, widget);
                
                newWidget.addEventListener('click', (e) => {
                    e.stopPropagation();
                    this.triggerTuccarTip();
                });
            }
        });
    },

    triggerTuccarTip:`;

sisorenJs = sisorenJs.replace(regex, replacement);
fs.writeFileSync('wwwroot/js/towns/sisoren/SisorenEngine.js', sisorenJs, 'utf8');
