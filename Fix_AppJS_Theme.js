const fs = require('fs');
let appJs = fs.readFileSync('wwwroot/js/app.js', 'utf8');

appJs = appJs.replace(
    /box\.classList\.remove\('rifat-speaking', 'cetin-speaking'\);/g,
    "box.classList.remove('rifat-speaking', 'cetin-speaking', 'ilyas-speaking');"
);

fs.writeFileSync('wwwroot/js/app.js', appJs, 'utf8');
