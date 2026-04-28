document.addEventListener("DOMContentLoaded", () => {
    const modal = document.getElementById("deleteConfirmModal");
    const openButton = document.querySelector("[data-open-delete-modal]");
    const closeButtons = document.querySelectorAll("[data-close-delete-modal]");
    const confirmButton = document.getElementById("confirmDeleteButton");
    const deleteForm = document.getElementById("deleteEntityForm");

    if (!modal || !openButton || !confirmButton || !deleteForm) {
        return;
    }

    const openModal = () => {
        modal.classList.add("custom-modal--visible");
        modal.setAttribute("aria-hidden", "false");
        document.body.classList.add("modal-open");
    };

    const closeModal = () => {
        modal.classList.remove("custom-modal--visible");
        modal.setAttribute("aria-hidden", "true");
        document.body.classList.remove("modal-open");
    };

    openButton.addEventListener("click", openModal);

    closeButtons.forEach(button => {
        button.addEventListener("click", closeModal);
    });

    confirmButton.addEventListener("click", () => {
        deleteForm.submit();
    });

    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape" && modal.classList.contains("custom-modal--visible")) {
            closeModal();
        }
    });
});