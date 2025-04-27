/**
 * PromptSpark.Chat Theme Switcher
 * Handles switching between light and dark themes
 */
(function() {
    'use strict';

    // Theme configuration
    const THEME_KEY = 'promptspark-theme';
    const DEFAULT_THEME = 'light';
    const THEMES = {
        light: {
            cssFile: 'promptspark-light.min.css',
            iconClass: 'bi-moon-fill',
            toggleTitle: 'Switch to Dark Mode'
        },
        dark: {
            cssFile: 'promptspark-dark.min.css',
            iconClass: 'bi-sun-fill',
            toggleTitle: 'Switch to Light Mode'
        }
    };

    // DOM elements
    let themeStylesheet;
    let themeToggle;

    // Initialize the theme system
    function initThemeSystem() {
        // Create the theme stylesheet link element if it doesn't exist
        if (!themeStylesheet) {
            themeStylesheet = document.createElement('link');
            themeStylesheet.rel = 'stylesheet';
            themeStylesheet.id = 'theme-stylesheet';
            document.head.appendChild(themeStylesheet);
        }

        // Get the saved theme or use default
        const savedTheme = localStorage.getItem(THEME_KEY) || DEFAULT_THEME;
        setTheme(savedTheme);

        // Setup theme toggle button event listener
        themeToggle = document.getElementById('theme-toggle');
        if (themeToggle) {
            themeToggle.addEventListener('click', toggleTheme);
            updateToggleButton(savedTheme);
        }
    }

    // Set the theme
    function setTheme(themeName) {
        // Validate theme name
        if (!THEMES[themeName]) {
            console.error(`Theme '${themeName}' not found. Using default.`);
            themeName = DEFAULT_THEME;
        }

        // Update the document body class
        document.body.classList.remove('theme-light', 'theme-dark');
        document.body.classList.add(`theme-${themeName}`);

        // Update the theme stylesheet
        themeStylesheet.href = `/dist/css/${THEMES[themeName].cssFile}`;

        // Save the theme preference
        localStorage.setItem(THEME_KEY, themeName);

        // Update the toggle button if it exists
        if (themeToggle) {
            updateToggleButton(themeName);
        }

        // Dispatch theme change event
        document.dispatchEvent(new CustomEvent('themeChanged', { 
            detail: { theme: themeName } 
        }));
    }

    // Toggle between light and dark themes
    function toggleTheme() {
        const currentTheme = localStorage.getItem(THEME_KEY) || DEFAULT_THEME;
        const newTheme = currentTheme === 'light' ? 'dark' : 'light';
        setTheme(newTheme);
    }

    // Update the toggle button appearance
    function updateToggleButton(themeName) {
        if (!themeToggle) return;

        // Clear existing classes
        themeToggle.querySelector('i').className = '';
        
        // Set new icon and title
        themeToggle.querySelector('i').className = `bi ${THEMES[themeName].iconClass}`;
        themeToggle.title = THEMES[themeName === 'light' ? 'dark' : 'light'].toggleTitle;
    }

    // Initialize on DOMContentLoaded
    document.addEventListener('DOMContentLoaded', initThemeSystem);

    // Expose the API to window for external use
    window.ThemeSwitcher = {
        setTheme: setTheme,
        toggleTheme: toggleTheme
    };
})();