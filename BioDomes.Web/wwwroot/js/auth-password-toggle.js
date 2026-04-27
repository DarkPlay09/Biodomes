document.addEventListener("DOMContentLoaded", () => {
    const passwordInput = document.getElementById("passwordInput");
    const togglePasswordButton = document.getElementById("togglePasswordButton");

    if (!passwordInput || !togglePasswordButton) {
        return;
    }

    togglePasswordButton.addEventListener("click", () => {
        const isPasswordVisible = passwordInput.type === "text";

        passwordInput.type = isPasswordVisible ? "password" : "text";

        togglePasswordButton.classList.toggle(
            "auth-password-toggle--visible",
            !isPasswordVisible
        );

        togglePasswordButton.setAttribute(
            "aria-label",
            isPasswordVisible ? "Afficher le mot de passe" : "Masquer le mot de passe"
        );

        togglePasswordButton.setAttribute(
            "aria-pressed",
            String(!isPasswordVisible)
        );
    });
});