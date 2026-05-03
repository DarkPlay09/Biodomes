document.addEventListener("DOMContentLoaded", () => {
    const toasts = document.querySelectorAll("[data-dashboard-toast]");

    if (!toasts.length) {
        return;
    }

    toasts.forEach((toast) => {
        const closeButton = toast.querySelector("[data-dashboard-toast-close]");

        const hideToast = () => {
            toast.classList.add("dashboard-toast--hidden");

            window.setTimeout(() => {
                toast.remove();
            }, 220);
        };

        const timeoutId = window.setTimeout(() => {
            hideToast();
        }, 4500);

        if (closeButton) {
            closeButton.addEventListener("click", () => {
                window.clearTimeout(timeoutId);
                hideToast();
            });
        }
    });
});