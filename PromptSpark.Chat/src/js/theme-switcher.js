/**
 * PromptSpark.Chat Theme Switcher
 * Uses Bootstrap 5.3 data-bs-theme attribute for native dark mode support
 */
(function() {
    'use strict';

    // Theme configuration
    const THEME_KEY = 'promptspark-theme';
    const DEFAULT_THEME = 'light';
    const THEMES = {
        light: {
            icon: 'bi-moon-fill',
            label: 'Switch to Dark Mode'
        },
        dark: {
            icon: 'bi-sun-fill',
            label: 'Switch to Light Mode'
        }
    };

    let themeToggle;

    // Apply theme immediately to prevent flash of wrong theme
    const savedTheme = localStorage.getItem(THEME_KEY) || DEFAULT_THEME;
    document.documentElement.setAttribute('data-bs-theme', savedTheme);

    function initThemeSystem() {
        // Apply saved theme
        setTheme(savedTheme);

        // Setup toggle button
        themeToggle = document.getElementById('theme-toggle');
        if (themeToggle) {
            themeToggle.addEventListener('click', toggleTheme);
            updateToggleButton(savedTheme);
        }
    }

    function setTheme(themeName) {
        if (!THEMES[themeName]) {
            themeName = DEFAULT_THEME;
        }

        // Bootstrap 5.3 native dark mode
        document.documentElement.setAttribute('data-bs-theme', themeName);

        // Keep body class for any custom CSS that targets .theme-dark / .theme-light
        document.body.classList.remove('theme-light', 'theme-dark');
        document.body.classList.add('theme-' + themeName);

        localStorage.setItem(THEME_KEY, themeName);

        if (themeToggle) {
            updateToggleButton(themeName);
        }

        // Dispatch event for components that need to react to theme changes
        document.dispatchEvent(new CustomEvent('themeChanged', {
            detail: { theme: themeName }
        }));
    }

    function toggleTheme() {
        var currentTheme = localStorage.getItem(THEME_KEY) || DEFAULT_THEME;
        setTheme(currentTheme === 'light' ? 'dark' : 'light');
    }

    function updateToggleButton(themeName) {
        if (!themeToggle) return;
        var icon = themeToggle.querySelector('i');
        if (icon) {
            icon.className = 'bi ' + THEMES[themeName].icon;
        }
        themeToggle.title = THEMES[themeName].label;
        themeToggle.setAttribute('aria-label', THEMES[themeName].label);
    }

    document.addEventListener('DOMContentLoaded', initThemeSystem);

    // Public API
    window.ThemeSwitcher = {
        setTheme: setTheme,
        toggleTheme: toggleTheme
    };
})();
