(() => {
    // Fade-in is pure CSS now (@keyframes etdFadeInUp in base.css).
    // Content is visible by default and remains visible if JS never runs.
    // This file only handles SVG SMIL-pause for off-screen performance.

    const reducedMotion =
        window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    let svgObserver = null;
    if (!reducedMotion && 'IntersectionObserver' in window) {
        svgObserver = new IntersectionObserver(
            (entries) => {
                for (const e of entries) {
                    const svg = e.target;
                    if (e.isIntersecting) {
                        svg.removeAttribute('data-anim-paused');
                        try { if (typeof svg.unpauseAnimations === 'function') svg.unpauseAnimations(); } catch (_) {}
                    } else {
                        svg.setAttribute('data-anim-paused', 'true');
                        try { if (typeof svg.pauseAnimations === 'function') svg.pauseAnimations(); } catch (_) {}
                    }
                }
            },
            { rootMargin: '200px 0px', threshold: 0 },
        );
    }
    const armSvgPauser = () => {
        if (!svgObserver) return;
        document.querySelectorAll('svg').forEach((svg) => {
            if (svg.dataset.animObserved === '1') return;
            if (!svg.querySelector('animate, animateTransform, animateMotion')) return;
            svg.dataset.animObserved = '1';
            svgObserver.observe(svg);
        });
    };

    const onReady = () => { armSvgPauser(); };

    if (document.readyState !== 'loading') onReady();
    else document.addEventListener('DOMContentLoaded', onReady);

    if (typeof Blazor !== 'undefined' && Blazor.addEventListener) {
        Blazor.addEventListener('enhancedload', onReady);
    }
})();
