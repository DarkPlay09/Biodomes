document.addEventListener("DOMContentLoaded", () => {
    const uploadBlocks = document.querySelectorAll("[data-file-upload]");

    uploadBlocks.forEach(block => {
        const input = block.querySelector(".entity-file-upload__input");

        if (!input) {
            return;
        }

        const fileName = block.querySelector("[data-file-name]");
        const preview = block.querySelector("[data-file-preview]");
        const previewWrapper = block.querySelector("[data-file-preview-wrapper]");
        const placeholder = block.querySelector("[data-file-placeholder]");
        const iconPreview = block.querySelector("[data-file-icon-preview]");
        const iconFallback = block.querySelector("[data-file-icon-fallback]");

        const defaultFileName = "Aucun fichier choisi";

        const currentName = fileName?.getAttribute("data-current-name") || defaultFileName;

        const currentPreviewSrc =
            preview?.getAttribute("data-current-src") ||
            preview?.getAttribute("src") ||
            "";

        const currentIconSrc =
            iconPreview?.getAttribute("data-current-src") ||
            iconPreview?.getAttribute("src") ||
            "";

        const hasCurrentPreview = currentPreviewSrc.trim() !== "";
        const hasCurrentIcon = currentIconSrc.trim() !== "";

        const setFileName = (name, selected) => {
            if (!fileName) {
                return;
            }

            fileName.textContent = name;
            fileName.classList.toggle("entity-file-upload__filename--selected", selected);
        };

        const setPreview = src => {
            if (!preview) {
                return;
            }

            const hasSrc = src && src.trim() !== "";

            if (hasSrc) {
                preview.src = src;
                preview.style.display = "block";

                if (previewWrapper) {
                    previewWrapper.hidden = false;
                }

                return;
            }

            preview.removeAttribute("src");
            preview.style.display = "none";

            if (previewWrapper) {
                previewWrapper.hidden = true;
            }
        };

        const setPlaceholder = show => {
            if (!placeholder) {
                return;
            }

            placeholder.style.display = show ? "flex" : "none";
        };

        const setIcon = src => {
            const hasSrc = src && src.trim() !== "";

            if (iconPreview) {
                if (hasSrc) {
                    iconPreview.src = src;
                    iconPreview.hidden = false;
                    iconPreview.style.display = "block";
                } else {
                    iconPreview.removeAttribute("src");
                    iconPreview.hidden = true;
                    iconPreview.style.display = "none";
                }
            }

            if (iconFallback) {
                iconFallback.hidden = hasSrc;
                iconFallback.style.display = hasSrc ? "none" : "block";
            }
        };

        const restoreInitialState = () => {
            setFileName(currentName, currentName !== defaultFileName);

            setPreview(hasCurrentPreview ? currentPreviewSrc : "");

            setPlaceholder(!hasCurrentPreview);

            setIcon(hasCurrentIcon ? currentIconSrc : "");
        };

        input.addEventListener("change", () => {
            const file = input.files && input.files.length > 0
                ? input.files[0]
                : null;

            if (!file) {
                restoreInitialState();
                return;
            }

            setFileName(file.name, true);

            if (!file.type.startsWith("image/")) {
                setPreview("");
                setPlaceholder(true);
                setIcon("");
                return;
            }

            const reader = new FileReader();

            reader.onload = event => {
                const src = event.target?.result?.toString() ?? "";

                setPreview(src);
                setPlaceholder(false);
                setIcon(src);
            };

            reader.readAsDataURL(file);
        });

        restoreInitialState();
    });
});