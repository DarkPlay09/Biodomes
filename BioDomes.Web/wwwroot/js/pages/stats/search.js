export function registerSearch() {
    const searchInput = document.getElementById("speciesSearch");
    const tableBody = document.getElementById("criticalSpeciesTableBody");

    searchInput?.addEventListener("input", () => {
        const search = (searchInput.value || "").trim().toLowerCase();
        const rows = tableBody?.querySelectorAll("tr") || [];

        rows.forEach(row => {
            const value = row.getAttribute("data-filter") || "";
            row.style.display = value.includes(search) ? "" : "none";
        });
    });
}
