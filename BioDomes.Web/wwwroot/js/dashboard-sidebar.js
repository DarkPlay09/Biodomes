document.addEventListener("DOMContentLoaded", () => {
    const sidebar = document.getElementById("dashboardSidebar");
    const overlay = document.getElementById("dashboardOverlay");
    const openButton = document.getElementById("dashboardMenuToggle");
    const closeButton = document.getElementById("dashboardSidebarClose");

    if (!sidebar || !overlay || !openButton) {
        return;
    }

    function openSidebar() {
        sidebar.classList.add("is-open");
        overlay.classList.add("is-open");
        document.body.classList.add("dashboard-menu-open");
        document.documentElement.classList.add("dashboard-menu-open");
    }

    function closeSidebar() {
        sidebar.classList.remove("is-open");
        overlay.classList.remove("is-open");
        document.body.classList.remove("dashboard-menu-open");
        document.documentElement.classList.remove("dashboard-menu-open");
    }

    openButton.addEventListener("click", openSidebar);
    overlay.addEventListener("click", closeSidebar);

    if (closeButton) {
        closeButton.addEventListener("click", closeSidebar);
    }

    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape") {
            closeSidebar();
        }
    });
});