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

    // ---------- Page lifecycle ----------
    const onPageReady = () => {
        armFadeIn();
    };

    if (document.readyState !== 'loading') onPageReady();
    else document.addEventListener('DOMContentLoaded', onPageReady);
    // Re-arm after Blazor enhanced navigation (form POST, link nav)
    if (typeof Blazor !== 'undefined' && Blazor.addEventListener) {
        Blazor.addEventListener('enhancedload', onPageReady);
    }
})();
