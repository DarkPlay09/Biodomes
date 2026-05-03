document.addEventListener("DOMContentLoaded", () => {
    const buttons = document.querySelectorAll("[data-cooldown-seconds]");

    buttons.forEach((button) => {
        let remaining = parseInt(button.dataset.cooldownSeconds || "0", 10);
        const readyLabel = button.dataset.labelReady || "Renvoyer l’e-mail";
        const waitingPrefix = button.dataset.labelWaiting || "Renvoyer l’e-mail";

        const update = () => {
            if (remaining > 0) {
                button.disabled = true;
                button.textContent = `${waitingPrefix} (${remaining}s)`;
            } else {
                button.disabled = false;
                button.textContent = readyLabel;
            }
        };

        update();

        if (remaining > 0) {
            const timer = setInterval(() => {
                remaining--;
                update();

                if (remaining <= 0) {
                    clearInterval(timer);
                }
            }, 1000);
        }
    });
});