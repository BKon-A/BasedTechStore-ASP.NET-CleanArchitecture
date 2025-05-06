document.addEventListener("DOMContentLoaded", function () {
    // Modal open trigger (реєстрація/вхід)
    //var openModal = "@(ViewData["OpenModal"] ?? "")";
    //if (openModal === "authModal") {
    //    var modal = new bootstrap.Modal(document.getElementById('authModal'));
    //    modal.show();
    //} else if (openModal === "regModal") {
    //    var modal = new bootstrap.Modal(document.getElementById('regModal'));
    //    modal.show();
    //}

    // Theme switch
    const modeSwitch = document.getElementById('btnDisplayModeSwitch');
    const lightIcon = document.getElementById('light-icon');
    const darkIcon = document.getElementById('dark-icon');

    // Читання теми з localStorage
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme) {
        document.documentElement.setAttribute('data-bs-theme', savedTheme);
    }

    function updateThemeIcon() {
        const isDark = document.documentElement.getAttribute('data-bs-theme') === 'dark';
        if (lightIcon && darkIcon) {
            lightIcon.style.display = isDark ? 'none' : 'inline';
            darkIcon.style.display = isDark ? 'inline' : 'none';
        }
    }

    if (modeSwitch) {
        modeSwitch.addEventListener('click', () => {
            const isCurrentlyDark = document.documentElement.getAttribute('data-bs-theme') === 'dark';
            const newTheme = isCurrentlyDark ? 'light' : 'dark';
            document.documentElement.setAttribute('data-bs-theme', newTheme);
            localStorage.setItem('theme', newTheme);
            updateThemeIcon();
        });
    }

    document.querySelectorAll('.product-card').forEach(card => {
        card.addEventListener('click', function () {
            const id = this.getAttribute('data-product-id');
            window.location.href = `/Product/Details/${id}`;
        });
    });

    updateThemeIcon();
});
