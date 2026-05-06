import { createStatsCharts } from "./charts.js";
import { registerExports } from "./exports.js";
import { registerSearch } from "./search.js";
import { createDetailRenderer, readJsonElement } from "./utils.js";

const dataElement = document.getElementById("stats-data");
const echartsInstance = window.echarts;

if (dataElement && echartsInstance) {
    const data = readJsonElement(dataElement);
    const showDetail = createDetailRenderer(document.getElementById("chartDetailPanel"));

    const charts = createStatsCharts({
        data,
        echarts: echartsInstance,
        showDetail
    });

    registerExports({ data, charts });
    registerSearch();
    registerResizeHandlers(charts);
}

function registerResizeHandlers(charts) {
    const resizeCharts = () => {
        charts.forEach(chart => chart.resize());
    };

    window.addEventListener("resize", resizeCharts);
    window.addEventListener("beforeprint", resizeCharts);
    window.addEventListener("afterprint", resizeCharts);
}
