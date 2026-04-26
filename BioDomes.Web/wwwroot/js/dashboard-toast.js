document.addEventListener("DOMContentLoaded", () => {
    const toast = document.querySelector("[data-dashboard-toast]");

    if (!toast) {
        return;
    }

    const closeButton = toast.querySelector("[data-dashboard-toast-close]");

    const hideToast = () => {
        toast.classList.add("dashboard-toast--hidden");

        window.setTimeout(() => {
            toast.remove();
        }, 220);
    };

    if (closeButton) {
        closeButton.addEventListener("click", hideToast);
    }

    window.setTimeout(hideToast, 4500);
});