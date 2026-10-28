// Oyuncu oturumu: her sekme/tarayıcı kendi kimliğini üretir ve sunucuya gönderir.
// Böylece farklı oyuncular birbirinden BAĞIMSIZ oynar (katil, çanta, diyalog, adli bulgular oyuncuya özeldir).
(function () {
    const KEY = 'dedektiflik_session_id_v1';
    let sid = null;
    try { sid = sessionStorage.getItem(KEY); } catch (e) { /* depolama kapalı olabilir */ }
    if (!sid) {
        sid = (window.crypto && crypto.randomUUID)
            ? crypto.randomUUID()
            : 'sid-' + Date.now().toString(36) + '-' + Math.random().toString(36).slice(2, 12);
        try { sessionStorage.setItem(KEY, sid); } catch (e) { }
    }
    window.PLAYER_SESSION_ID = sid;

    const originalFetch = window.fetch.bind(window);
    window.fetch = function (input, init) {
        try {
            const url = typeof input === 'string' ? input : (input && input.url) || '';
            const isApi = url.startsWith('/api/') || url.startsWith(location.origin + '/api/');
            if (isApi) {
                init = init || {};
                const headers = new Headers(init.headers || (typeof input !== 'string' && input.headers) || {});
                headers.set('X-Session-Id', sid);
                init.headers = headers;
            }
        } catch (e) { /* başlık eklenemezse istek yine de gider */ }
        return originalFetch(input, init);
    };
})();
