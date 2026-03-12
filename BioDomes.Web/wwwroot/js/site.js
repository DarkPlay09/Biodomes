const toggle = document.getElementById("menuToggle");
const closeBtn = document.getElementById("menuClose");
const panel = document.getElementById("mobileMenuPanel");
const overlay = document.getElementById("mobileMenuOverlay");

function openMenu() {
    panel?.classList.add("is-open");
    overlay?.classList.add("is-open");
    document.body.style.overflow = "hidden";
}

function closeMenu() {
    panel?.classList.remove("is-open");
    overlay?.classList.remove("is-open");
    document.body.style.overflow = "";
}

toggle?.addEventListener("click", openMenu);
closeBtn?.addEventListener("click", closeMenu);
overlay?.addEventListener("click", closeMenu);