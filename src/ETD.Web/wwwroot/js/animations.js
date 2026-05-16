(() => {
    if (!('IntersectionObserver' in window)) return;
    const io = new IntersectionObserver((entries) => {
        for (const e of entries) {
            if (e.isIntersecting) {
                e.target.classList.add('in');
                io.unobserve(e.target);
            }
        }
    }, { rootMargin: '0px 0px -80px 0px', threshold: 0.05 });

    const start = () => document.querySelectorAll('.fade-in').forEach(el => io.observe(el));
    if (document.readyState !== 'loading') start();
    else document.addEventListener('DOMContentLoaded', start);
    // Re-arm after enhanced navigation
    Blazor.addEventListener?.('enhancedload', start);
})();
