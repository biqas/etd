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
    function estElektro(rooms, isNeubau, zsNeu) {
        rooms = Math.max(1, Math.min(12, rooms));
        const per = isNeubau ? { low: 1800, high: 2800 } : { low: 900, high: 1700 };
        let low = rooms * per.low, high = rooms * per.high;
        if (zsNeu) { low += 1200; high += 2200; }
        return { low, high };
    }
    function estKnx(rooms, funcCount) {
        rooms = Math.max(1, Math.min(12, rooms));
        funcCount = Math.max(1, Math.min(6, funcCount));
        const mult = 1 + (funcCount - 1) * 0.20;
        return {
            low: Math.round(rooms * 800 * mult) + 1500,
            high: Math.round(rooms * 1400 * mult) + 2800
        };
    }
    function estKlima(rooms, system) {
        rooms = Math.max(1, Math.min(8, rooms));
        if (system === 'vrf')   return { low: rooms * 3200, high: rooms * 4800 };
        if (system === 'multi') return { low: rooms * 1800, high: rooms * 2900 };
        return { low: rooms * 1500, high: rooms * 2400 };
    }
    function estSecurity(comps, units) {
        comps = Math.max(1, Math.min(4, comps));
        units = Math.max(1, Math.min(30, units));
        return {
            low: comps * 600 + units * 220,
            high: comps * 1000 + units * 380
        };
    }
    function readNum(name) {
        const el = document.querySelector('[name="' + name + '"]');
        return el ? +el.value : 0;
    }
    function readRadio(name) {
        const el = document.querySelector('[name="' + name + '"]:checked');
        return el ? el.value : '';
    }
    function readCheckbox(name) {
        const el = document.querySelector('[name="' + name + '"]');
        return el ? el.checked : false;
    }
    function syncChipGroupToHidden(groupId, hiddenId, attr) {
        const group = document.getElementById(groupId);
        const hidden = document.getElementById(hiddenId);
        if (!group || !hidden) return [];
        const slugs = Array.from(group.querySelectorAll('input[type=checkbox][' + attr + ']:checked'))
            .map(el => el.getAttribute(attr));
        hidden.value = slugs.join('|');
        return slugs;
    }
    function setOut(key, value) {
        document.querySelectorAll('[data-out="' + key + '"]').forEach(e => e.textContent = value);
    }
    function updatePrice() {
        if (document.querySelector('[name="ElektroRoomCount"]')) {
            const rooms = readNum('ElektroRoomCount');
            const isNeubau = readRadio('ElektroProjektart') === 'neubau';
            const zsNeu = readCheckbox('ElektroZaehlerschrankNeu');
            setOut('el-rooms', rooms);
            const e = estElektro(rooms, isNeubau, zsNeu);
            setOut('el-est', `~ ${fmt(e.low)} – ${fmt(e.high)} €`);
        }
        if (document.querySelector('[name="KnxRoomCount"]')) {
            const rooms = readNum('KnxRoomCount');
            const funcs = syncChipGroupToHidden('knx-functions-group', 'knx-functions-raw', 'data-knx-func');
            setOut('knx-rooms', rooms);
            const e = estKnx(rooms, Math.max(funcs.length, 1));
            setOut('knx-est', `~ ${fmt(e.low)} – ${fmt(e.high)} €`);
        }
        if (document.querySelector('[name="KlimaRoomCount"]')) {
            const rooms = readNum('KlimaRoomCount');
            const sys = readRadio('KlimaSystem') || 'single';
            setOut('klima-rooms', rooms);
            const e = estKlima(rooms, sys);
            setOut('klima-est', `~ ${fmt(e.low)} – ${fmt(e.high)} €`);
        }
        if (document.querySelector('[name="SecuritySensorCount"]')) {
            const units = readNum('SecuritySensorCount');
            const comps = syncChipGroupToHidden('sec-components-group', 'sec-components-raw', 'data-sec-comp');
            setOut('sec-units', units);
            const e = estSecurity(Math.max(comps.length, 1), units);
            setOut('sec-est', `~ ${fmt(e.low)} – ${fmt(e.high)} €`);
        }
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
