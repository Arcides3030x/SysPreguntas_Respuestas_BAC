/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        "./Views/**/*.cshtml", // Escanea todas las vistas Razor
        "./wwwroot/**/*.{html,js,css}" // Escanea HTML, JS y CSS en wwwroot
    ],
    theme: {
        extend: {},
    },
    plugins: [],
}
