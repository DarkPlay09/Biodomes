document.addEventListener("DOMContentLoaded", () => {
    const uploadBlocks = document.querySelectorAll("[data-file-upload]");

    uploadBlocks.forEach(block => {
        const input = block.querySelector(".entity-file-upload__input");
        const fileName = block.querySelector("[data-file-name]");
        const preview = block.querySelector("[data-file-preview]");
        const previewWrapper = block.querySelector("[data-file-preview-wrapper]");
        const placeholder = block.querySelector("[data-file-placeholder]");

        if (!input) {
            return;
        }

        input.addEventListener("change", () => {
            const file = input.files && input.files.length > 0
                ? input.files[0]
                : null;

            if (!file) {
                if (fileName) {
                    fileName.textContent = "Aucun fichier choisi";
                    fileName.classList.remove("entity-file-upload__filename--selected");
                }

                if (preview) {
                    preview.src = "";
                    preview.style.display = "none";
                }

                if (previewWrapper) {
                    previewWrapper.hidden = true;
                }

                if (placeholder) {
                    placeholder.style.display = "flex";
                }

                return;
            }

            if (fileName) {
                fileName.textContent = file.name;
                fileName.classList.add("entity-file-upload__filename--selected");
            }

            if (!file.type.startsWith("image/")) {
                if (preview) {
                    preview.src = "";
                    preview.style.display = "none";
                }

                if (previewWrapper) {
                    previewWrapper.hidden = true;
                }

                if (placeholder) {
                    placeholder.style.display = "flex";
                }

                return;
            }

            const reader = new FileReader();

            reader.onload = event => {
                if (preview) {
                    preview.src = event.target?.result ?? "";
                    preview.style.display = "block";
                }

                if (previewWrapper) {
                    previewWrapper.hidden = false;
                }

                if (placeholder) {
                    placeholder.style.display = "none";
                }
            };

            reader.readAsDataURL(file);
        });
    });
});