document.addEventListener("DOMContentLoaded", function () {
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

    updateThemeIcon();
});
