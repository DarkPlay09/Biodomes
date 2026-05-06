(() => {
    const root = document.querySelector('.select-species-page');
    const form = document.getElementById('confirmSelectionForm');

    if (!root || !form) {
        return;
    }

    const slug = root.getAttribute('data-biome-slug') || '';
    if (!slug) {
        return;
    }

    const storageKey = `biodomes:selected-species:${slug}`;

    const readSet = () => {
        try {
            const raw = localStorage.getItem(storageKey);
            if (!raw) return new Set();
            const values = JSON.parse(raw);
            if (!Array.isArray(values)) return new Set();
            return new Set(values.map((x) => String(x)));
        } catch {
            return new Set();
        }
    };

    const writeSet = (set) => {
        localStorage.setItem(storageKey, JSON.stringify(Array.from(set)));
    };

    const selected = readSet();
    const checkboxes = Array.from(document.querySelectorAll('.select-species-page__select-checkbox'));

    checkboxes.forEach((checkbox) => {
        const speciesId = checkbox.getAttribute('data-species-id');
        if (!speciesId) return;

        checkbox.checked = selected.has(speciesId);

        checkbox.addEventListener('change', () => {
            if (checkbox.checked) {
                selected.add(speciesId);
            } else {
                selected.delete(speciesId);
            }

            writeSet(selected);
        });
    });

    form.addEventListener('submit', () => {
        form.querySelectorAll('input[name="SelectedSpeciesIds"]').forEach((input) => input.remove());

        selected.forEach((speciesId) => {
            const hidden = document.createElement('input');
            hidden.type = 'hidden';
            hidden.name = 'SelectedSpeciesIds';
            hidden.value = speciesId;
            form.appendChild(hidden);
        });

        localStorage.removeItem(storageKey);
    });
})();
