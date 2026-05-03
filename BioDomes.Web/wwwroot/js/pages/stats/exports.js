import { downloadFile, escapeCsvCell } from "./utils.js";

export function registerExports({ data, charts }) {
    document.getElementById("exportPdfBtn")?.addEventListener("click", () => {
        charts.forEach(chart => chart.resize());

        window.setTimeout(() => {
            window.print();
        }, 250);
    });

    document.getElementById("exportCsvBtn")?.addEventListener("click", () => {
        const rows = [
            ["Espèce", "Classification", "Population", "Tendance"],
            ...(data.criticalSpecies || []).map(item => [
                item.name,
                item.classification,
                item.population,
                item.trend
            ])
        ];

        const csv = rows
            .map(row => row.map(escapeCsvCell).join(";"))
            .join("\n");

        downloadFile("biodomes-statistiques.csv", csv, "text/csv;charset=utf-8;");
    });

    document.getElementById("exportJsonBtn")?.addEventListener("click", () => {
        downloadFile(
            "biodomes-statistiques.json",
            JSON.stringify(data, null, 2),
            "application/json;charset=utf-8;"
        );
    });
}
