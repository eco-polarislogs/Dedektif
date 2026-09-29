const fs = require('fs');
let appJs = fs.readFileSync('wwwroot/js/app.js', 'utf8');

const regex = /const imgRect = imgEl\.getBoundingClientRect\(\);\s*const wrapperRect = wrapper\.getBoundingClientRect\(\);\s*const imgOffsetLeft = imgRect\.left - wrapperRect\.left;\s*const imgOffsetTop = imgRect\.top - wrapperRect\.top;\s*for \(let i = 0; i < 18; i\+\+\) \{\s*const rx = \(Math\.random\(\) - 0\.5\) \* 36;\s*const ry = \(Math\.random\(\) - 0\.5\) \* 36;\s*const finalX = x \+ rx;\s*const finalY = y \+ ry;\s*\/\/ Sadece resim sınırları içinde çiz\s*if \(finalX >= imgOffsetLeft && finalX <= imgOffsetLeft \+ imgRect\.width &&\s*finalY >= imgOffsetTop && finalY <= imgOffsetTop \+ imgRect\.height\) \{\s*ctx\.beginPath\(\);\s*ctx\.arc\(finalX, finalY, Math\.random\(\) \* 2\.5 \+ 1, 0, Math\.PI \* 2\);\s*ctx\.fill\(\);\s*\}\s*\}/s;

const replacement = `const imgRect = imgEl.getBoundingClientRect();
            const wrapperRect = wrapper.getBoundingClientRect();
            
            // Calculate actual rendered image dimensions inside object-fit: contain
            const imgRatio = imgEl.naturalWidth / imgEl.naturalHeight;
            const containerRatio = imgRect.width / imgRect.height;
            let renderWidth, renderHeight, renderLeft, renderTop;
            if (imgRatio > containerRatio) {
                renderWidth = imgRect.width;
                renderHeight = imgRect.width / imgRatio;
                renderLeft = imgRect.left;
                renderTop = imgRect.top + (imgRect.height - renderHeight) / 2;
            } else {
                renderHeight = imgRect.height;
                renderWidth = imgRect.height * imgRatio;
                renderTop = imgRect.top;
                renderLeft = imgRect.left + (imgRect.width - renderWidth) / 2;
            }

            const imgOffsetLeft = renderLeft - wrapperRect.left;
            const imgOffsetTop = renderTop - wrapperRect.top;

            for (let i = 0; i < 18; i++) {
                const rx = (Math.random() - 0.5) * 36;
                const ry = (Math.random() - 0.5) * 36;
                const finalX = x + rx;
                const finalY = y + ry;

                // Sadece GERÇEK render edilen resim sınırları içinde çiz
                if (finalX >= imgOffsetLeft && finalX <= imgOffsetLeft + renderWidth &&
                    finalY >= imgOffsetTop && finalY <= imgOffsetTop + renderHeight) {
                    ctx.beginPath();
                    ctx.arc(finalX, finalY, Math.random() * 2.5 + 1, 0, Math.PI * 2);
                    ctx.fill();
                }
            }`;

if (regex.test(appJs)) {
    appJs = appJs.replace(regex, replacement);
    fs.writeFileSync('wwwroot/js/app.js', appJs, 'utf8');
    console.log('Fixed dust bounding logic.');
} else {
    console.log('Regex did not match.');
}
