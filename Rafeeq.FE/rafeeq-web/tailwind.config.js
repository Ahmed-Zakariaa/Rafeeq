/** @type {import('tailwindcss').Config} */
const primeui = require('tailwindcss-primeui');

module.exports = {
  content: ['./src/**/*.{html,ts}'],
  // PrimeNG provides component base styles; disable Tailwind's preflight so
  // it doesn't override them. We add the minimal resets we need in styles.scss.
  corePlugins: { preflight: false },
  theme: {
    extend: {
      // Rafeeq brand palette (docs/brand-identity.md)
      colors: {
        brand: {
          DEFAULT: '#117C6F',
          600: '#0E6A5E',
          300: '#1FA796',
          '050': '#E6F4F1',
        },
        accent: '#F4A62A',
        ink: '#1C2B33',
        muted: '#6B7C84',
        bg: '#F6F8F8',
        surface: '#FFFFFF',
        line: '#E2E8E9',
        success: '#2BB673',
        warning: '#F4A62A',
        error: '#E5484D',
        info: '#2D8CF0',
        female: '#9B6DD6',
      },
      fontFamily: {
        sans: ['Cairo', 'system-ui', 'Segoe UI', 'sans-serif'],
      },
      borderRadius: {
        xl: '12px',
      },
    },
  },
  plugins: [primeui],
};
