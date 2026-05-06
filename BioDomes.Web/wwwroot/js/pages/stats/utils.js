export function readJsonElement(element) {
    try {
        return JSON.parse(element.textContent || "{}");
    } catch {
        return {};
    }
}

export function createDetailRenderer(detailPanel) {
    return (title, rows) => {
        if (!detailPanel) {
            return;
        }

        detailPanel.innerHTML = `
            <div>
                <span class="analytics-detail-panel__eyebrow">Détails interactifs</span>
                <h3>${escapeHtml(title)}</h3>
            </div>

            <dl class="analytics-detail-list">
                ${rows.map(([label, value]) => `
                    <div>
                        <dt>${escapeHtml(label)}</dt>
                        <dd>${escapeHtml(String(value))}</dd>
                    </div>
                `).join("")}
            </dl>
        `;
    };
}

export function getTrophicCategoryName(category) {
    return [
        "Producteur",
        "Herbivore",
        "Carnivore",
        "Omnivore",
        "Autre"
    ][category] || "Autre";
}

export function escapeCsvCell(value) {
    const stringValue = String(value ?? "");
    return `"${stringValue.replaceAll('"', '""')}"`;
}

export function downloadFile(filename, content, contentType) {
    const blob = new Blob([content], { type: contentType });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");

    link.href = url;
    link.download = filename;

    document.body.appendChild(link);
    link.click();
    link.remove();

    URL.revokeObjectURL(url);
}

export function escapeHtml(value) {
    return String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}
