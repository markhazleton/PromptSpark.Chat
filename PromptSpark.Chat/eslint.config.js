const globals = require('globals');

module.exports = [
  {
    languageOptions: {
      ecmaVersion: 2021,
      sourceType: 'script',
      globals: {
        ...globals.browser,
        ...globals.node,
        ...globals.jquery
      }
    },
    rules: {
      'no-console': 'off',
      'no-unused-vars': ['warn', { argsIgnorePattern: '^_' }],
      'quotes': ['warn', 'single', { avoidEscape: true }],
      'semi': ['warn', 'always']
    }
  },
  {
    ignores: [
      'node_modules/**',
      'wwwroot/**',
      'bin/**',
      'obj/**'
    ]
  }
];
