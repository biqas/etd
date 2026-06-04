(() => {
    // ---------- Subtle fade-up reveal (progressive enhancement) ----------
    // Content is visible by default (CSS .fade-in has opacity:1).
    // When JS is available AND IntersectionObserver works, we ARM elements with
    // .fade-in-armed (which hides them) BEFORE they observe, then add .in when
    // they enter the viewport. This guarantees no Flash of Invisible Content
    // even if JS fails or IO never fires.

    const reducedMotion =
        window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    let fadeObserver = null;
    if (!reducedMotion && 'IntersectionObserver' in window) {
        fadeObserver = new IntersectionObserver(
            (entries) => {
                for (const e of entries) {
                    if (e.isIntersecting) {
                        e.target.classList.add('in');
                        fadeObserver.unobserve(e.target);
                    }
                }
            },
            { rootMargin: '0px 0px -10% 0px', threshold: 0.05 },
        );
    }
    const armFadeIns = () => {
        if (!fadeObserver) return;
        document.querySelectorAll('.fade-in:not(.fade-in-armed):not([data-fade-skip])').forEach((el) => {
            // Only arm elements that are NOT already in the viewport on first paint.
            // Already-visible elements stay at their default (opacity: 1).
            const r = el.getBoundingClientRect();
            const inView = r.top < window.innerHeight && r.bottom > 0;
            if (inView) {
                // Mark as done so we don't observe nor flash it.
                el.setAttribute('data-fade-skip', '1');
                return;
            }
            el.classList.add('fade-in-armed');
            fadeObserver.observe(el);
        });
    };

    // ---------- Pause SMIL animations on SVGs that are off-screen ----------
    // 34 simultaneous indefinite SVG animations destroy mobile scroll perf.
    // We toggle data-anim-paused on each SVG so CSS *can* stop CSS-anim chains,
    // and we call svg.pauseAnimations() / unpauseAnimations() (SVG DOM API) so
    // SMIL <animate>/<animateTransform> stops too.
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
        // Only watch SVGs that actually contain SMIL animations.
        document.querySelectorAll('svg').forEach((svg) => {
            if (svg.dataset.animObserved === '1') return;
            if (!svg.querySelector('animate, animateTransform, animateMotion')) return;
            svg.dataset.animObserved = '1';
            svgObserver.observe(svg);
        });
    };

    // ---------- Lifecycle ----------
    const onReady = () => {
        armFadeIns();
        armSvgPauser();
    };

    if (document.readyState !== 'loading') onReady();
    else document.addEventListener('DOMContentLoaded', onReady);

    // After Blazor enhanced navigation, re-scan: new DOM, fresh elements.
    if (typeof Blazor !== 'undefined' && Blazor.addEventListener) {
        Blazor.addEventListener('enhancedload', () => {
            // Disconnect stale observers' targets if those nodes are gone.
            // For simplicity we just re-arm new elements; old ones are GC'd with the DOM.
            onReady();
        });
    }
})();
