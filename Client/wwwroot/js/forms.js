// Focuses and scrolls to the first invalid field in a form after a failed
// submit, so the user immediately sees which field needs attention.
window.nexoForms = {
    focusFirstInvalid: function (container) {
        var root = container || document;
        var el = root.querySelector(".invalid");
        if (!el) return;

        var target = el.matches("input, select, textarea") ? el : el.querySelector("input, select, textarea");
        if (!target) return;

        target.scrollIntoView({ behavior: "smooth", block: "center" });
        target.focus({ preventScroll: true });
    }
};
