document.addEventListener("DOMContentLoaded", () => {
    const uploadBlocks = document.querySelectorAll("[data-file-upload]");

    uploadBlocks.forEach(block => {
        const input = block.querySelector(".entity-file-upload__input");
        const fileName = block.querySelector("[data-file-name]");
        const iconPreview = block.querySelector("[data-file-icon-preview]");
        const iconFallback = block.querySelector("[data-file-icon-fallback]");

        if (!input) {
            return;
        }

        const currentName = fileName?.getAttribute("data-current-name") || "Aucun fichier choisi";
        const currentIconSrc = iconPreview?.getAttribute("data-current-src") || "";

        input.addEventListener("change", () => {
            const file = input.files && input.files.length > 0
                ? input.files[0]
                : null;

            if (!file) {
                if (fileName) {
                    fileName.textContent = currentName;
                    fileName.classList.toggle(
                        "entity-file-upload__filename--selected",
                        currentName !== "Aucun fichier choisi");
                }

                if (iconPreview && iconFallback) {
                    iconPreview.src = currentIconSrc;
                    iconPreview.hidden = !currentIconSrc;
                    iconFallback.hidden = !!currentIconSrc;
                }

                return;
            }

            if (fileName) {
                fileName.textContent = file.name;
                fileName.classList.add("entity-file-upload__filename--selected");
            }

            if (!file.type.startsWith("image/")) {
                return;
            }

            const reader = new FileReader();

            reader.onload = event => {
                if (iconPreview && iconFallback) {
                    iconPreview.src = event.target?.result ?? "";
                    iconPreview.hidden = false;
                    iconFallback.hidden = true;
                }
            };

            reader.readAsDataURL(file);
        });
    });
});
