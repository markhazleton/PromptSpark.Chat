'use strict';

const renderSCSS = require('./render-scss');
const themeBuilder = require('./build-themes');

// Build the default CSS
renderSCSS();

// Build the theme CSS files
themeBuilder.buildThemes();
