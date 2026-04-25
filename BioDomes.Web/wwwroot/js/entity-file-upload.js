document.addEventListener("DOMContentLoaded", () => {
    const uploadBlocks = document.querySelectorAll("[data-file-upload]");

    uploadBlocks.forEach(block => {
        const input = block.querySelector(".entity-file-upload__input");
        if (!input) return;

        const fileName = block.querySelector("[data-file-name]");
        const preview = block.querySelector("[data-file-preview]");
        const previewWrapper = block.querySelector("[data-file-preview-wrapper]");
        const placeholder = block.querySelector("[data-file-placeholder]");
        const iconPreview = block.querySelector("[data-file-icon-preview]");
        const iconFallback = block.querySelector("[data-file-icon-fallback]");

        const currentName = fileName?.getAttribute("data-current-name") || "Aucun fichier choisi";
        const currentPreviewSrc = preview?.getAttribute("data-current-src") || preview?.getAttribute("src") || "";
        const currentIconSrc = iconPreview?.getAttribute("data-current-src") || iconPreview?.getAttribute("src") || "";

        const setFileName = (name, selected) => {
            if (!fileName) return;
            fileName.textContent = name;
            fileName.classList.toggle("entity-file-upload__filename--selected", selected);
        };

        const setPreview = (src) => {
            if (!preview) return;
            preview.src = src || "";
            preview.style.display = src ? "block" : "none";
            if (previewWrapper) {
                previewWrapper.hidden = !src;
            }
        };

        const setPlaceholder = show => {
            if (!placeholder) return;
            placeholder.style.display = show ? "flex" : "none";
        };

        const setIcon = (src) => {
            if (iconPreview) {
                iconPreview.src = src || "";
                iconPreview.hidden = !src;
            }
            if (iconFallback) {
                iconFallback.hidden = !!src;
            }
        };

        input.addEventListener("change", () => {
            const file = input.files && input.files.length > 0 ? input.files[0] : null;

            if (!file) {
                setFileName(currentName, currentName !== "Aucun fichier choisi");
                setPreview(currentPreviewSrc);
                setPlaceholder(!currentPreviewSrc);
                setIcon(currentIconSrc);
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

        // Initial sync at page load
        if (preview) {
            const hasPreview = !!(preview.getAttribute("src") || currentPreviewSrc);
            setPreview(hasPreview ? (preview.getAttribute("src") || currentPreviewSrc) : "");
            setPlaceholder(!hasPreview);
        } else {
            setPlaceholder(false);
        }

        if (iconPreview || iconFallback) {
            const hasIcon = !!(iconPreview?.getAttribute("src") || currentIconSrc);
            setIcon(hasIcon ? (iconPreview?.getAttribute("src") || currentIconSrc) : "");
        }
    });
});
