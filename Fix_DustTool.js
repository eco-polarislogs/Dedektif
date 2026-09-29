const fs = require('fs');
let appJs = fs.readFileSync('wwwroot/js/app.js', 'utf8');

const regex = /\} else if \(activeForensicTool === 'dust'\) \{\s*const ctx = dustCanvas\.getContext\('2d'\);\s*ctx\.fillStyle = 'rgba\(25, 25, 30, 0\.85\)';\s*ctx\.shadowColor = 'transparent';\s*ctx\.shadowBlur = 0;\s*for \(let i = 0; i < 18; i\+\+\) \{\s*const rx = \(Math\.random\(\) - 0\.5\) \* 36;\s*const ry = \(Math\.random\(\) - 0\.5\) \* 36;\s*ctx\.beginPath\(\);\s*ctx\.arc\(x \+ rx, y \+ ry, Math\.random\(\) \* 2\.5 \+ 1, 0, Math\.PI \* 2\);\s*ctx\.fill\(\);\s*\}/s;

const replacement = `} else if (activeForensicTool === 'dust') {
            const ctx = dustCanvas.getContext('2d');
            ctx.fillStyle = 'rgba(25, 25, 30, 0.85)';
            ctx.shadowColor = 'transparent';
            ctx.shadowBlur = 0;

            const imgRect = imgEl.getBoundingClientRect();
            const wrapperRect = wrapper.getBoundingClientRect();
            const imgOffsetLeft = imgRect.left - wrapperRect.left;
            const imgOffsetTop = imgRect.top - wrapperRect.top;

            for (let i = 0; i < 18; i++) {
                const rx = (Math.random() - 0.5) * 36;
                const ry = (Math.random() - 0.5) * 36;
                const finalX = x + rx;
                const finalY = y + ry;

                // Sadece resim sınırları içinde çiz
                if (finalX >= imgOffsetLeft && finalX <= imgOffsetLeft + imgRect.width &&
                    finalY >= imgOffsetTop && finalY <= imgOffsetTop + imgRect.height) {
                    ctx.beginPath();
                    ctx.arc(finalX, finalY, Math.random() * 2.5 + 1, 0, Math.PI * 2);
                    ctx.fill();
                }
            }`;

appJs = appJs.replace(regex, replacement);
fs.writeFileSync('wwwroot/js/app.js', appJs, 'utf8');
