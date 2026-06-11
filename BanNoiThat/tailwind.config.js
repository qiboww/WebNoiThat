/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Pages/**/*.cshtml",
    "./Views/**/*.cshtml",
    "./Areas/**/*.cshtml",
    "./wwwroot/**/*.html"
  ],
  theme: {
    extend: {
      colors: {
        "primary": "#795736",
        "primary-container": "#b58d67",
        "on-primary": "#ffffff",
        "surface": "#fcf9f8",
        "on-surface": "#1b1c1c",
        "surface-variant": "#e4e2e1",
        "on-surface-variant": "#4f453c",
        "background": "#fcf9f8",
        "secondary-container": "#e5e2dd",
        "outline-variant": "#d3c4b8"
      },
      fontFamily: {
        "body-md": ["Inter", "sans-serif"],
        "headline": ["Playfair Display", "serif"]
      }
    }
  },
  plugins: [],
}
