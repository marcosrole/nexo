// Lightweight scroll-reveal for elements marked with the ".reveal" class.
// Runs independently of Blazor's render cycle via a MutationObserver, so it
// keeps working as the component tree changes after WASM interactivity kicks in.
(function () {
    var shownClass = "in-view";
    var revealSelector = ".reveal";

    var reduceMotion = window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches;

    function revealAll(root) {
        root.querySelectorAll(revealSelector).forEach(function (el) {
            el.classList.add(shownClass);
        });
    }

    if (reduceMotion) {
        // Respect the user's OS preference: show content immediately, no motion.
        document.addEventListener("DOMContentLoaded", function () {
            revealAll(document);
        });
        new MutationObserver(function () {
            revealAll(document);
        }).observe(document.documentElement, { childList: true, subtree: true });
        return;
    }

    var io = new IntersectionObserver(
        function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add(shownClass);
                    io.unobserve(entry.target);
                }
            });
        },
        { threshold: 0.1, rootMargin: "0px 0px -10% 0px" }
    );

    function observeAll(root) {
        root.querySelectorAll(revealSelector).forEach(function (el) {
            if (!el.classList.contains(shownClass)) {
                io.observe(el);
            }
        });
    }

    // Safety net: IntersectionObserver can miss elements that enter and leave the
    // viewport within a single very fast, non-animated jump (e.g. a scrollbar drag
    // or a scroll library that bypasses smooth-scroll physics). A throttled scroll
    // listener double-checks bounding boxes directly so nothing gets stuck at
    // opacity: 0 forever.
    var pendingCheck = false;
    function checkPositions() {
        pendingCheck = false;
        var vh = window.innerHeight || document.documentElement.clientHeight;
        document.querySelectorAll(revealSelector + ":not(.in-view)").forEach(function (el) {
            var rect = el.getBoundingClientRect();
            if (rect.top < vh && rect.bottom > 0) {
                el.classList.add(shownClass);
                io.unobserve(el);
            }
        });
    }

    window.addEventListener(
        "scroll",
        function () {
            if (!pendingCheck) {
                pendingCheck = true;
                requestAnimationFrame(checkPositions);
            }
        },
        { passive: true }
    );

    var mo = new MutationObserver(function () {
        observeAll(document);
        checkPositions();
    });

    document.addEventListener("DOMContentLoaded", function () {
        observeAll(document);
        checkPositions();
        mo.observe(document.body, { childList: true, subtree: true });

        // Belt-and-suspenders: some inputs (scrollbar drag, very fast fling
        // scrolling) can move the page without ever firing an IntersectionObserver
        // callback for a section that briefly passed through view. A short-lived
        // poll catches anything still hidden, then stops once the page is settled.
        var pollCount = 0;
        var poll = setInterval(function () {
            checkPositions();
            pollCount++;
            if (document.querySelectorAll(revealSelector + ":not(.in-view)").length === 0 || pollCount > 40) {
                clearInterval(poll);
            }
        }, 600);
    });
})();
