document.addEventListener("DOMContentLoaded", () => {
    const refreshCardState = card => {
        const input = card.querySelector("[data-linked-count-input]");
        const saveButton = card.querySelector("[data-linked-save-button]");

        if (!input || !saveButton) {
            return;
        }

        const initialValue = Number.parseInt(input.dataset.initialValue || "0", 10);
        const currentValue = Number.parseInt(input.value || "0", 10);
        const safeCurrentValue = Number.isNaN(currentValue) ? 0 : currentValue;
        const hasChanged = safeCurrentValue !== initialValue;

        saveButton.disabled = !hasChanged;
        card.classList.toggle("is-dirty", hasChanged);
    };

    document.querySelectorAll("[data-linked-item-card]").forEach(card => {
        const input = card.querySelector("[data-linked-count-input]");

        if (!input) {
            return;
        }

        input.addEventListener("input", () => refreshCardState(card));
        input.addEventListener("change", () => refreshCardState(card));

        refreshCardState(card);
    });

    document.querySelectorAll("[data-counter-dec], [data-counter-inc]").forEach(button => {
        button.addEventListener("click", () => {
            const targetId = button.dataset.targetInput;
            const input = document.getElementById(targetId);

            if (!input) {
                return;
            }

            const card = input.closest("[data-linked-item-card]");
            const min = Number.parseInt(input.min || "0", 10);
            let currentValue = Number.parseInt(input.value || "0", 10);

            if (Number.isNaN(currentValue)) {
                currentValue = min;
            }

            currentValue = button.hasAttribute("data-counter-dec")
                ? Math.max(min, currentValue - 1)
                : currentValue + 1;

            input.value = currentValue;
            input.dispatchEvent(new Event("input", { bubbles: true }));

            if (card) {
                refreshCardState(card);
            }
        });
    });
});