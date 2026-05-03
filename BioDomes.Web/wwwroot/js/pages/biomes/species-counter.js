document.addEventListener("DOMContentLoaded", () => {
    const SCROLL_KEY = "biomeSpeciesScrollY";

    const savedY = sessionStorage.getItem(SCROLL_KEY);
    if (savedY !== null) {
        const y = Number.parseInt(savedY, 10);
        if (!Number.isNaN(y)) {
            window.scrollTo(0, y);
        }
        sessionStorage.removeItem(SCROLL_KEY);
    }

    const decButtons = document.querySelectorAll("[data-counter-dec]");
    const incButtons = document.querySelectorAll("[data-counter-inc]");
    const preserveScrollForms = document.querySelectorAll("form[data-preserve-scroll]");
    const preserveScrollTriggers = document.querySelectorAll("[data-preserve-scroll-trigger]");
    const removeConfirmForms = document.querySelectorAll("form[data-remove-confirm-form]");

    const clampInput = (input) => {
        const min = Number.parseInt(input.min || "0", 10);
        const value = Number.parseInt(input.value || "0", 10);

        if (Number.isNaN(value)) {
            input.value = String(min);
            return min;
        }

        const safe = Math.max(min, value);
        input.value = String(safe);
        return safe;
    };

    const update = (button, delta) => {
        const targetId = button.getAttribute("data-target-input");
        if (!targetId) return;

        const input = document.getElementById(targetId);
        if (!(input instanceof HTMLInputElement)) return;

        const current = clampInput(input);
        input.value = String(Math.max(0, current + delta));
    };

    decButtons.forEach((button) => {
        button.addEventListener("click", () => update(button, -1));
    });

    incButtons.forEach((button) => {
        button.addEventListener("click", () => update(button, 1));
    });

    preserveScrollForms.forEach((form) => {
        form.addEventListener("submit", () => {
            sessionStorage.setItem(SCROLL_KEY, String(window.scrollY));
        });
    });

    preserveScrollTriggers.forEach((trigger) => {
        trigger.addEventListener("click", () => {
            sessionStorage.setItem(SCROLL_KEY, String(window.scrollY));
        });
    });

    removeConfirmForms.forEach((form) => {
        const button = form.querySelector("[data-remove-confirm-button]");
        if (!(button instanceof HTMLButtonElement)) {
            return;
        }

        form.addEventListener("submit", (event) => {
            const isArmed = button.dataset.confirmArmed === "true";
            if (isArmed) {
                return;
            }

            event.preventDefault();

            button.dataset.confirmArmed = "true";
            button.classList.add("is-confirm");
            button.textContent = "Confirmer";
        });

        button.addEventListener("blur", () => {
            if (button.dataset.confirmArmed !== "true") {
                return;
            }

            window.setTimeout(() => {
                button.dataset.confirmArmed = "false";
                button.classList.remove("is-confirm");
                button.textContent = "Retirer";
            }, 180);
        });
    });
});
