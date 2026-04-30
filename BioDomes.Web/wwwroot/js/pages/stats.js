/**
 * Initialise les graphiques et les interactions de la page Statistiques d'un biome.
 * Le script est encapsulé dans une IIFE pour éviter de polluer le scope global.
 */
(() => {
    const dataElement = document.getElementById("stats-data");

    if (!dataElement || typeof echarts === "undefined") {
        return;
    }

    const data = JSON.parse(dataElement.textContent || "{}");

    const detailPanel = document.getElementById("chartDetailPanel");
    const searchInput = document.getElementById("speciesSearch");
    const tableBody = document.getElementById("criticalSpeciesTableBody");

    const pdfBtn = document.getElementById("exportPdfBtn");
    const csvBtn = document.getElementById("exportCsvBtn");
    const jsonBtn = document.getElementById("exportJsonBtn");

    /**
     * Liste des instances ECharts créées sur la page.
     * Elle permet de redimensionner tous les graphiques en une seule fois.
     */
    const charts = [];

    /**
     * Palette principale utilisée par les graphiques.
     */
    const colors = {
        primary: "#0b3d39",
        moss: "#4a6d4a",
        accent: "#84a98c",
        soft: "#dbe7e4",
        blue: "#2563eb",
        orange: "#ea580c",
        red: "#dc2626",
        green: "#16a34a",
        muted: "#94a3b8"
    };

    /**
     * Palette secondaire utilisée pour les graphiques à plusieurs catégories.
     */
    const palette = [
        colors.primary,
        colors.moss,
        colors.accent,
        colors.blue,
        colors.orange,
        colors.red,
        "#0f766e",
        "#64748b"
    ];

    /**
     * Retourne la configuration de la toolbox ECharts.
     * Elle est volontairement désactivée pour éviter d'afficher les icônes
     * de téléchargement, de zoom ou de restauration sur les graphiques.
     *
     * @returns {{ show: boolean }} Configuration ECharts de la toolbox.
     */
    const toolbox = () => ({
        show: false
    });

    createBiodiversityChart();
    createClassificationChart();
    createSpeciesChart();
    createDietChart();
    createTrophicGraphChart();
    registerExports();
    registerSearch();

    /**
     * Redimensionne les graphiques lorsque la fenêtre change de taille.
     */
    window.addEventListener("resize", () => {
        charts.forEach(chart => chart.resize());
    });

    /**
     * Redimensionne les graphiques juste avant l'impression.
     * Cela évite que les graphiques soient coupés dans l'export PDF.
     */
    window.addEventListener("beforeprint", () => {
        charts.forEach(chart => chart.resize());
    });

    /**
     * Redimensionne les graphiques après l'impression,
     * afin de les réadapter à l'affichage normal de la page.
     */
    window.addEventListener("afterprint", () => {
        charts.forEach(chart => chart.resize());
    });

    /**
     * Crée le graphique représentant la biodiversité du biome dans le temps.
     * Le graphique combine :
     * - des barres pour le nombre d'individus ;
     * - une courbe pour l'indice de biodiversité.
     *
     * Le graphique permet aussi le zoom via la barre située sous l'axe X.
     *
     * @returns {void}
     */
    function createBiodiversityChart() {
        const element = document.getElementById("biodiversityChart");

        if (!element) {
            return;
        }

        const chart = echarts.init(element);
        const timeline = data.biodiversityTimeline || [];

        chart.setOption({
            color: [colors.accent, colors.primary],
            grid: {
                left: 52,
                right: 48,
                top: 72,
                bottom: 74
            },
            tooltip: {
                trigger: "axis"
            },
            legend: {
                top: 4,
                left: 0
            },
            toolbox: toolbox(),
            dataZoom: [
                {
                    type: "inside",
                    xAxisIndex: 0
                },
                {
                    type: "slider",
                    xAxisIndex: 0,
                    height: 24,
                    bottom: 18
                }
            ],
            xAxis: {
                type: "category",
                data: timeline.map(x => x.date),
                axisLine: {
                    lineStyle: {
                        color: "#d8e1e1"
                    }
                },
                axisLabel: {
                    color: "#64748b"
                }
            },
            yAxis: [
                {
                    type: "value",
                    axisLabel: {
                        color: "#64748b"
                    },
                    splitLine: {
                        lineStyle: {
                            color: "#eef3f3"
                        }
                    }
                },
                {
                    type: "value",
                    min: 0,
                    max: 100,
                    axisLabel: {
                        color: "#64748b"
                    },
                    splitLine: {
                        show: false
                    }
                }
            ],
            series: [
                {
                    name: "Individus",
                    type: "bar",
                    barWidth: 24,
                    itemStyle: {
                        borderRadius: [8, 8, 0, 0]
                    },
                    data: timeline.map(x => x.individualCount)
                },
                {
                    name: "Indice biodiversité",
                    type: "line",
                    yAxisIndex: 1,
                    smooth: true,
                    symbolSize: 8,
                    lineStyle: {
                        width: 3
                    },
                    data: timeline.map(x => x.biodiversityIndex)
                }
            ]
        });

        chart.on("click", params => {
            const point = timeline[params.dataIndex];

            if (!point) {
                return;
            }

            showDetail("Biodiversité au fil du temps", [
                ["Date", point.date],
                ["Individus", point.individualCount],
                ["Indice biodiversité", `${point.biodiversityIndex}/100`]
            ]);
        });

        charts.push(chart);
    }

    /**
     * Crée le graphique donut de répartition des espèces par classification.
     * La position de la légende et du donut est adaptée pour les petits écrans.
     *
     * @returns {void}
     */
    function createClassificationChart() {
        const element = document.getElementById("classificationChart");

        if (!element) {
            return;
        }

        const chart = echarts.init(element);
        const series = data.classificationSeries || [];
        const isSmallScreen = window.matchMedia("(max-width: 900px)").matches;

        chart.setOption({
            color: palette,
            tooltip: {
                trigger: "item",
                formatter: "{b}<br/>{c} individu(s) ({d}%)"
            },
            legend: {
                orient: isSmallScreen ? "horizontal" : "vertical",
                right: isSmallScreen ? "auto" : 24,
                left: isSmallScreen ? "center" : "auto",
                top: isSmallScreen ? "auto" : "center",
                bottom: isSmallScreen ? 0 : "auto",
                icon: "circle",
                itemWidth: 10,
                itemHeight: 10,
                itemGap: 14,
                textStyle: {
                    color: "#475569",
                    fontSize: 13,
                    fontWeight: 600
                }
            },
            toolbox: toolbox(),
            series: [
                {
                    name: "Classification",
                    type: "pie",
                    radius: ["56%", "72%"],
                    center: isSmallScreen ? ["50%", "45%"] : ["38%", "53%"],
                    avoidLabelOverlap: true,
                    label: {
                        show: false
                    },
                    labelLine: {
                        show: false
                    },
                    data: series.map(item => ({
                        name: item.label,
                        value: item.value,
                        percentage: item.percentage,
                        speciesCount: item.speciesCount,
                        biomeCount: item.biomeCount
                    }))
                }
            ]
        });

        chart.on("click", params => {
            showDetail(`Classification : ${params.name}`, [
                ["Individus", params.value],
                ["Pourcentage", `${params.data.percentage}%`],
                ["Espèces distinctes", params.data.speciesCount],
                ["Biomes concernés", params.data.biomeCount]
            ]);
        });

        charts.push(chart);
    }

    /**
     * Crée le graphique en barres du nombre d'individus par espèce.
     * Le graphique permet de visualiser rapidement les espèces les plus représentées.
     *
     * @returns {void}
     */
    function createSpeciesChart() {
        const element = document.getElementById("speciesChart");

        if (!element) {
            return;
        }

        const chart = echarts.init(element);
        const series = data.speciesSeries || [];

        chart.setOption({
            color: [colors.primary],
            grid: {
                left: 54,
                right: 24,
                top: 54,
                bottom: 86
            },
            tooltip: {
                trigger: "axis",
                axisPointer: {
                    type: "shadow"
                }
            },
            toolbox: toolbox(),
            dataZoom: [
                {
                    type: "inside",
                    xAxisIndex: 0
                },
                {
                    type: "slider",
                    xAxisIndex: 0,
                    height: 24,
                    bottom: 20
                }
            ],
            xAxis: {
                type: "category",
                data: series.map(item => item.label),
                axisLabel: {
                    rotate: 30,
                    color: "#64748b",
                    width: 90,
                    overflow: "truncate"
                },
                axisLine: {
                    lineStyle: {
                        color: "#d8e1e1"
                    }
                }
            },
            yAxis: {
                type: "value",
                axisLabel: {
                    color: "#64748b"
                },
                splitLine: {
                    lineStyle: {
                        color: "#eef3f3"
                    }
                }
            },
            series: [
                {
                    name: "Individus",
                    type: "bar",
                    barMaxWidth: 34,
                    itemStyle: {
                        borderRadius: [8, 8, 0, 0]
                    },
                    data: series.map(item => ({
                        value: item.value,
                        percentage: item.percentage,
                        biomeCount: item.biomeCount
                    }))
                }
            ]
        });

        chart.on("click", params => {
            showDetail(`Espèce : ${params.name}`, [
                ["Population", params.value],
                ["Part dans les biomes", `${params.data.percentage}%`],
                ["Biomes concernés", params.data.biomeCount]
            ]);
        });

        charts.push(chart);
    }

    /**
     * Crée le graphique horizontal de répartition des individus par régime alimentaire.
     *
     * @returns {void}
     */
    function createDietChart() {
        const element = document.getElementById("dietChart");

        if (!element) {
            return;
        }

        const chart = echarts.init(element);
        const series = data.dietSeries || [];

        chart.setOption({
            color: [colors.moss],
            grid: {
                left: 120,
                right: 42,
                top: 36,
                bottom: 36
            },
            tooltip: {
                trigger: "axis",
                axisPointer: {
                    type: "shadow"
                }
            },
            toolbox: toolbox(),
            xAxis: {
                type: "value",
                axisLabel: {
                    color: "#64748b"
                },
                splitLine: {
                    lineStyle: {
                        color: "#eef3f3"
                    }
                }
            },
            yAxis: {
                type: "category",
                inverse: true,
                data: series.map(item => item.label),
                axisLine: {
                    show: false
                },
                axisTick: {
                    show: false
                },
                axisLabel: {
                    color: "#475569",
                    fontWeight: 700
                }
            },
            series: [
                {
                    name: "Individus",
                    type: "bar",
                    showBackground: true,
                    backgroundStyle: {
                        color: "#edf3f2",
                        borderRadius: 20
                    },
                    label: {
                        show: true,
                        position: "right",
                        color: "#0f172a",
                        fontWeight: 800
                    },
                    barWidth: 14,
                    itemStyle: {
                        borderRadius: 20
                    },
                    data: series.map(item => ({
                        value: item.value,
                        percentage: item.percentage,
                        speciesCount: item.speciesCount,
                        biomeCount: item.biomeCount
                    }))
                }
            ]
        });

        chart.on("click", params => {
            showDetail(`Régime alimentaire : ${params.name}`, [
                ["Individus", params.value],
                ["Pourcentage", `${params.data.percentage}%`],
                ["Espèces distinctes", params.data.speciesCount],
                ["Biomes concernés", params.data.biomeCount]
            ]);
        });

        charts.push(chart);
    }

    /**
     * Crée le graphe trophique interactif.
     * Les nœuds représentent les espèces et les liens représentent les relations trophiques.
     *
     * @returns {void}
     */
    function createTrophicGraphChart() {
        const element = document.getElementById("trophicGraphChart");

        if (!element) {
            return;
        }

        const chart = echarts.init(element);
        const graph = data.trophicGraph || { nodes: [], links: [] };

        chart.setOption({
            color: [
                colors.green,
                colors.moss,
                colors.red,
                colors.orange,
                colors.muted
            ],
            tooltip: {
                formatter: params => {
                    if (params.dataType === "edge") {
                        return `${escapeHtml(params.data.label)}<br/>Biome : ${escapeHtml(params.data.biomeName)}`;
                    }

                    return `${escapeHtml(params.name)}<br/>${params.data.value} individu(s)<br/>${escapeHtml(params.data.diet || "")}`;
                }
            },
            legend: {
                bottom: 0,
                data: [
                    "Producteurs",
                    "Herbivores",
                    "Carnivores",
                    "Omnivores",
                    "Autres"
                ]
            },
            toolbox: toolbox(),
            series: [
                {
                    name: "Réseau trophique",
                    type: "graph",
                    layout: "force",
                    roam: true,
                    draggable: true,
                    focusNodeAdjacency: true,
                    categories: [
                        { name: "Producteurs" },
                        { name: "Herbivores" },
                        { name: "Carnivores" },
                        { name: "Omnivores" },
                        { name: "Autres" }
                    ],
                    force: {
                        repulsion: 180,
                        edgeLength: [70, 150]
                    },
                    edgeSymbol: ["none", "arrow"],
                    edgeSymbolSize: 8,
                    label: {
                        show: true,
                        fontSize: 11,
                        color: "#0f172a"
                    },
                    lineStyle: {
                        opacity: 0.55,
                        width: 1.5,
                        curveness: 0.12
                    },
                    data: graph.nodes || [],
                    links: graph.links || []
                }
            ]
        });

        chart.on("click", params => {
            if (params.dataType === "edge") {
                showDetail("Relation trophique", [
                    ["Relation", params.data.label],
                    ["Biome", params.data.biomeName]
                ]);

                return;
            }

            showDetail(`Espèce : ${params.name}`, [
                ["Population", params.data.value],
                ["Régime", params.data.diet || "Non renseigné"],
                ["Catégorie", getTrophicCategoryName(params.data.category)]
            ]);
        });

        charts.push(chart);
    }

    /**
     * Affiche les détails de l'élément cliqué dans le panneau d'information.
     *
     * @param {string} title Titre du détail affiché.
     * @param {Array<[string, string|number]>} rows Liste des lignes à afficher.
     * @returns {void}
     */
    function showDetail(title, rows) {
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
    }

    /**
     * Enregistre les événements liés aux boutons d'export.
     * Le PDF utilise l'impression du navigateur.
     * Le CSV et le JSON sont générés côté client.
     *
     * @returns {void}
     */
    function registerExports() {
        pdfBtn?.addEventListener("click", () => {
            charts.forEach(chart => chart.resize());

            setTimeout(() => {
                window.print();
            }, 250);
        });

        csvBtn?.addEventListener("click", () => {
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

        jsonBtn?.addEventListener("click", () => {
            downloadFile(
                "biodomes-statistiques.json",
                JSON.stringify(data, null, 2),
                "application/json;charset=utf-8;"
            );
        });
    }

    /**
     * Enregistre la recherche dans le tableau des espèces critiques.
     * Les lignes sont filtrées grâce à leur attribut data-filter.
     *
     * @returns {void}
     */
    function registerSearch() {
        searchInput?.addEventListener("input", () => {
            const search = (searchInput.value || "").trim().toLowerCase();
            const rows = tableBody?.querySelectorAll("tr") || [];

            rows.forEach(row => {
                const value = row.getAttribute("data-filter") || "";
                row.style.display = value.includes(search) ? "" : "none";
            });
        });
    }

    /**
     * Convertit l'identifiant numérique d'une catégorie trophique en libellé lisible.
     *
     * @param {number} category Identifiant de catégorie utilisé par le graphe ECharts.
     * @returns {string} Libellé de la catégorie trophique.
     */
    function getTrophicCategoryName(category) {
        return [
            "Producteur",
            "Herbivore",
            "Carnivore",
            "Omnivore",
            "Autre"
        ][category] || "Autre";
    }

    /**
     * Échappe une valeur pour l'insérer correctement dans un fichier CSV.
     *
     * @param {unknown} value Valeur à écrire dans une cellule CSV.
     * @returns {string} Valeur échappée et entourée de guillemets.
     */
    function escapeCsvCell(value) {
        const stringValue = String(value ?? "");
        return `"${stringValue.replaceAll('"', '""')}"`;
    }

    /**
     * Télécharge un fichier généré côté client.
     *
     * @param {string} filename Nom du fichier téléchargé.
     * @param {string} content Contenu du fichier.
     * @param {string} contentType Type MIME du fichier.
     * @returns {void}
     */
    function downloadFile(filename, content, contentType) {
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

    /**
     * Échappe les caractères HTML sensibles.
     * Cette fonction évite d'injecter du HTML non désiré dans le panneau de détails.
     *
     * @param {unknown} value Valeur à sécuriser.
     * @returns {string} Chaîne sécurisée pour une insertion HTML.
     */
    function escapeHtml(value) {
        return String(value ?? "")
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");
    }
})();