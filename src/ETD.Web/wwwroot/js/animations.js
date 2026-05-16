(() => {
    // ---------- Fade-in reveal (enhancement on top of CSS auto-animation) ----------
    let io = null;
    if ('IntersectionObserver' in window) {
        io = new IntersectionObserver((entries) => {
            for (const e of entries) {
                if (e.isIntersecting) {
                    e.target.classList.add('in');
                    io.unobserve(e.target);
                }
            }
        }, { rootMargin: '0px 0px -80px 0px', threshold: 0.05 });
    }
    const armFadeIn = () => {
        if (!io) return;
        document.querySelectorAll('.fade-in').forEach(el => io.observe(el));
    };

    // ---------- Live price preview (Preisrahmen / step 2) ----------
    const fmt = n => Math.round(n).toLocaleString('de-DE');

    function estWallbox(kw, dist) {
        kw = Math.max(4, Math.min(22, kw));
        dist = Math.max(1, Math.min(50, dist));
        let low = 1500 + (kw - 11) * 30 + Math.max(0, dist - 5) * 20;
        let high = low + 400 + (kw > 11 ? 200 : 0) + (dist > 15 ? 300 : 0);
        return { low: Math.max(low, 1100), high: Math.max(high, low + 300) };
    }
    function estPv(area, storageKwh) {
        area = Math.max(10, Math.min(200, area));
        storageKwh = Math.max(0, Math.min(30, storageKwh));
        const kwp = Math.round(area / 6);
        let low = kwp * 1100, high = kwp * 1500;
        low += storageKwh * 600;
        high += storageKwh * 900;
        return { low, high };
    }
    function readNum(name) {
        const el = document.querySelector('[name="' + name + '"]');
        return el ? +el.value : 0;
    }
    function setOut(key, value) {
        document.querySelectorAll('[data-out="' + key + '"]').forEach(e => e.textContent = value);
    }
    function updatePrice() {
        if (document.querySelector('[name="WallboxKw"]')) {
            const kw = readNum('WallboxKw');
            const dist = readNum('WallboxDistanceMeters');
            setOut('wb-kw', kw);
            setOut('wb-dist', dist);
            const e = estWallbox(kw, dist);
            setOut('wb-est', `~ ${fmt(e.low)} – ${fmt(e.high)} €`);
        }
        if (document.querySelector('[name="PvAreaSqm"]')) {
            const area = readNum('PvAreaSqm');
            const storage = readNum('PvStorageKwh');
            setOut('pv-area', area);
            setOut('pv-storage', storage === 0 ? 'Kein' : storage);
            setOut('pv-storage-unit', storage === 0 ? 'Speicher' : 'kWh');
            const e = estPv(area, storage);
            setOut('pv-est', `~ ${fmt(e.low)} – ${fmt(e.high)} €`);
        }
    }
    window.etdPrice = { update: updatePrice };

    // ---------- Page lifecycle ----------
    const onPageReady = () => {
        armFadeIn();
        updatePrice();
    };

    if (document.readyState !== 'loading') onPageReady();
    else document.addEventListener('DOMContentLoaded', onPageReady);
    // Re-arm after Blazor enhanced navigation (form POST, link nav)
    if (typeof Blazor !== 'undefined' && Blazor.addEventListener) {
        Blazor.addEventListener('enhancedload', onPageReady);
    }
})();
