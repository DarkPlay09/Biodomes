import {
    axisShadowTooltip,
    axisTooltip,
    categoryAxis,
    colors,
    dataZoom,
    donutLegend,
    grid,
    hiddenToolbox,
    isSmallScreen,
    palette,
    roundedTopBar,
    valueAxis
} from "./chart-options.js";

import { escapeHtml, getTrophicCategoryName } from "./utils.js";

export function createStatsCharts({ data, echarts, showDetail }) {
    const charts = [];
    const createChart = chartFactory(echarts, charts);

    createBiodiversityChart({ data, createChart, showDetail });
    createClassificationChart({ data, createChart, showDetail });
    createSpeciesChart({ data, createChart, showDetail });
    createDietChart({ data, createChart, showDetail });
    createTrophicGraphChart({ data, createChart, showDetail });

    return charts;
}

function chartFactory(echarts, charts) {
    return chartId => {
        const element = document.getElementById(chartId);

        if (!element) {
            return null;
        }

        const chart = echarts.init(element);
        charts.push(chart);

        return chart;
    };
}

function createBiodiversityChart({ data, createChart, showDetail }) {
    const chart = createChart("biodiversityChart");

    if (!chart) {
        return;
    }

    const timeline = data.biodiversityTimeline || [];

    chart.setOption({
        color: [colors.accent, colors.primary],
        grid: grid({ left: 52, right: 48, top: 72, bottom: 74 }),
        tooltip: axisTooltip(),
        legend: {
            top: 4,
            left: 0
        },
        toolbox: hiddenToolbox,
        dataZoom: dataZoom(18),
        xAxis: categoryAxis(timeline.map(point => point.date)),
        yAxis: [
            valueAxis(),
            valueAxis({
                min: 0,
                max: 100,
                splitLine: {
                    show: false
                }
            })
        ],
        series: [
            {
                name: "Individus",
                type: "bar",
                barWidth: 24,
                itemStyle: roundedTopBar(),
                data: timeline.map(point => point.individualCount)
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
                data: timeline.map(point => point.biodiversityIndex)
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
}

function createClassificationChart({ data, createChart, showDetail }) {
    const chart = createChart("classificationChart");

    if (!chart) {
        return;
    }

    const series = data.classificationSeries || [];
    const smallScreen = isSmallScreen();

    chart.setOption({
        color: palette,
        tooltip: {
            trigger: "item",
            formatter: "{b}<br/>{c} individu(s) ({d}%)"
        },
        legend: donutLegend(smallScreen),
        toolbox: hiddenToolbox,
        series: [
            {
                name: "Classification",
                type: "pie",
                radius: ["56%", "72%"],
                center: smallScreen ? ["50%", "45%"] : ["38%", "53%"],
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
}

function createSpeciesChart({ data, createChart, showDetail }) {
    const chart = createChart("speciesChart");

    if (!chart) {
        return;
    }

    const series = data.speciesSeries || [];

    chart.setOption({
        color: [colors.primary],
        grid: grid({ bottom: 86 }),
        tooltip: axisShadowTooltip(),
        toolbox: hiddenToolbox,
        dataZoom: dataZoom(),
        xAxis: categoryAxis(series.map(item => item.label), {
            rotate: 30,
            width: 90,
            overflow: "truncate"
        }),
        yAxis: valueAxis(),
        series: [
            {
                name: "Individus",
                type: "bar",
                barMaxWidth: 34,
                itemStyle: roundedTopBar(),
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
}

function createDietChart({ data, createChart, showDetail }) {
    const chart = createChart("dietChart");

    if (!chart) {
        return;
    }

    const series = data.dietSeries || [];

    chart.setOption({
        color: [colors.moss],
        grid: grid({ left: 120, right: 42, top: 36, bottom: 36 }),
        tooltip: axisShadowTooltip(),
        toolbox: hiddenToolbox,
        xAxis: valueAxis(),
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
                color: colors.axisDark,
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
}

function createTrophicGraphChart({ data, createChart, showDetail }) {
    const chart = createChart("trophicGraphChart");

    if (!chart) {
        return;
    }

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
        toolbox: hiddenToolbox,
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
}
