export const colors = {
    primary: "#0b3d39",
    moss: "#4a6d4a",
    accent: "#84a98c",
    soft: "#dbe7e4",
    blue: "#2563eb",
    orange: "#ea580c",
    red: "#dc2626",
    green: "#16a34a",
    muted: "#94a3b8",
    axis: "#64748b",
    axisDark: "#475569",
    axisLine: "#d8e1e1",
    splitLine: "#eef3f3"
};

export const palette = [
    colors.primary,
    colors.moss,
    colors.accent,
    colors.blue,
    colors.orange,
    colors.red,
    "#0f766e",
    "#64748b"
];

export const hiddenToolbox = {
    show: false
};

export function grid(overrides = {}) {
    return {
        left: 54,
        right: 24,
        top: 54,
        bottom: 54,
        ...overrides
    };
}

export function axisTooltip() {
    return {
        trigger: "axis"
    };
}

export function axisShadowTooltip() {
    return {
        trigger: "axis",
        axisPointer: {
            type: "shadow"
        }
    };
}

export function valueAxis(overrides = {}) {
    return {
        type: "value",
        axisLabel: {
            color: colors.axis
        },
        splitLine: {
            lineStyle: {
                color: colors.splitLine
            }
        },
        ...overrides
    };
}

export function categoryAxis(labels, axisLabelOverrides = {}, overrides = {}) {
    return {
        type: "category",
        data: labels,
        axisLine: {
            lineStyle: {
                color: colors.axisLine
            }
        },
        axisLabel: {
            color: colors.axis,
            ...axisLabelOverrides
        },
        ...overrides
    };
}

export function dataZoom(bottom = 20) {
    return [
        {
            type: "inside",
            xAxisIndex: 0
        },
        {
            type: "slider",
            xAxisIndex: 0,
            height: 24,
            bottom
        }
    ];
}

export function roundedTopBar() {
    return {
        borderRadius: [8, 8, 0, 0]
    };
}

export function isSmallScreen() {
    return window.matchMedia("(max-width: 900px)").matches;
}

export function donutLegend(smallScreen) {
    return {
        orient: smallScreen ? "horizontal" : "vertical",
        right: smallScreen ? "auto" : 24,
        left: smallScreen ? "center" : "auto",
        top: smallScreen ? "auto" : "center",
        bottom: smallScreen ? 0 : "auto",
        icon: "circle",
        itemWidth: 10,
        itemHeight: 10,
        itemGap: 14,
        textStyle: {
            color: colors.axisDark,
            fontSize: 13,
            fontWeight: 600
        }
    };
}
