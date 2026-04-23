document.addEventListener("DOMContentLoaded", () => {
    const uploadBlocks = document.querySelectorAll("[data-file-upload]");

    uploadBlocks.forEach(block => {
        const input = block.querySelector(".entity-file-upload__input");
        const fileName = block.querySelector("[data-file-name]");
        const preview = block.querySelector("[data-file-preview]");
        const previewWrapper = block.querySelector("[data-file-preview-wrapper]");

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

                if (preview && previewWrapper) {
                    preview.src = "";
                    previewWrapper.hidden = true;
                }

                return;
            }

            if (fileName) {
                fileName.textContent = file.name;
                fileName.classList.add("entity-file-upload__filename--selected");
            }

            if (file.type.startsWith("image/") && preview && previewWrapper) {
                const reader = new FileReader();

                reader.onload = event => {
                    preview.src = event.target?.result ?? "";
                    previewWrapper.hidden = false;
                };

                reader.readAsDataURL(file);
            } else if (preview && previewWrapper) {
                preview.src = "";
                previewWrapper.hidden = true;
            }
        });
    });
});