'use strict';
const autoprefixer = require('autoprefixer');
const fs = require('fs');
const packageJSON = require('../package.json');
const upath = require('upath');
const postcss = require('postcss');
const sass = require('sass');
const sh = require('shelljs');
const cssnano = require('cssnano');

// Theme paths
const baseThemePath = '../src/scss/themes/';
const lightThemePath = `${baseThemePath}light-theme.scss`;
const darkThemePath = `${baseThemePath}dark-theme.scss`;
const destFolder = upath.resolve(upath.dirname(__filename), '../wwwroot/dist/css/');

// Ensure themes directory exists
function ensureThemesDirectory() {
    const themesDir = upath.resolve(upath.dirname(__filename), baseThemePath);
    if (!sh.test('-e', themesDir)) {
        sh.mkdir('-p', themesDir);
        console.log(`Created themes directory: ${themesDir}`);
    }
}

// Create copyright comment block
function createCopyrightBlock(themeName) {
    return `/*!
* ${packageJSON.title || 'PromptSpark.Chat'} - ${themeName} theme v${packageJSON.version || '1.0.0'} (${packageJSON.homepage || ''})
* Copyright 2013-${new Date().getFullYear()} ${packageJSON.author || 'Mark Hazleton'}
* Licensed under ${packageJSON.license || 'MIT'}
*/`;
}

// Compile a theme
function compileTheme(themePath, outputName, themeName) {
    try {
        // Check if theme file exists, if not, log warning and return
        const fullThemePath = upath.resolve(upath.dirname(__filename), themePath);
        if (!fs.existsSync(fullThemePath)) {
            console.warn(`Theme file not found: ${fullThemePath}`);
            return;
        }

        // Prepare destination path
        const destPath = upath.join(destFolder, outputName);
        const destPathDirname = upath.dirname(destPath);
        if (!sh.test('-e', destPathDirname)) {
            sh.mkdir('-p', destPathDirname);
        }

        // Read the SCSS content
        const scssContent = fs.readFileSync(fullThemePath, 'utf8');
        
        // Compile SCSS
        const copyright = createCopyrightBlock(themeName);
        
        // Use renderSync for direct compilation
        const results = sass.compile(fullThemePath, {
            loadPaths: [
                upath.resolve(upath.dirname(__filename), '../node_modules'),
                upath.resolve(upath.dirname(__filename), '../src/scss')
            ]
        });

        // Process with PostCSS synchronously to ensure file is written
        postcss([
            autoprefixer,
            cssnano() // This minifies the CSS
        ]).process(results.css, { 
            from: undefined, 
            to: outputName 
        }).then(result => {
            // Add copyright notice to the beginning of the CSS
            const finalCss = `${copyright}\n${result.css}`;
            
            // Write the final CSS to the destination
            fs.writeFileSync(destPath, finalCss);
            console.log(`Theme compiled and minified to: ${destPath}`);
        }).catch(error => {
            console.error(`PostCSS processing error: ${error}`);
        });
    } catch (error) {
        console.error(`Error compiling theme ${themePath}:`, error);
    }
}

// Export the module functions
module.exports = {
    buildThemes: function() {
        try {
            console.log('Starting theme compilation...');
            ensureThemesDirectory();
            
            console.log('Compiling light theme...');
            compileTheme(lightThemePath, 'promptspark-light.min.css', 'Light');
            
            console.log('Compiling dark theme...');
            compileTheme(darkThemePath, 'promptspark-dark.min.css', 'Dark');
            
            console.log('Theme compilation completed.');
        } catch (error) {
            console.error('Error in buildThemes:', error);
        }
    }
};