/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./**/*.razor",
    "./wwwroot/index.html",
  ],
  theme: {
    extend: {
      colors: {
        background: "#0B0F14",
        surface: "#151A22",
        primary: "#6C5CE7",
        accent: "#00D4FF",
        success: "#22C55E",
        warning: "#F59E0B",
        danger: "#EF4444",
        "text-primary": "#FFFFFF",
        "text-secondary": "#9CA3AF",
      },
      fontFamily: {
        sans: ["Inter", "system-ui", "-apple-system", "sans-serif"],
      },
      boxShadow: {
        glow: "0 0 0 1px rgba(108, 92, 231, 0.4), 0 8px 30px -8px rgba(108, 92, 231, 0.55)",
        "glow-accent": "0 0 0 1px rgba(0, 212, 255, 0.35), 0 8px 30px -8px rgba(0, 212, 255, 0.45)",
        card: "0 4px 24px -8px rgba(0, 0, 0, 0.55)",
      },
      animation: {
        float: "float 6s ease-in-out infinite",
        "pulse-slow": "pulse-slow 3s ease-in-out infinite",
        "dash": "dash 3s linear infinite",
        "flow": "flow 2.5s linear infinite",
      },
      keyframes: {
        float: {
          "0%, 100%": { transform: "translateY(0px)" },
          "50%": { transform: "translateY(-10px)" },
        },
        "pulse-slow": {
          "0%, 100%": { opacity: "0.6" },
          "50%": { opacity: "1" },
        },
        dash: {
          to: { strokeDashoffset: "-24" },
        },
        flow: {
          "0%": { backgroundPosition: "0% 0%" },
          "100%": { backgroundPosition: "200% 0%" },
        },
      },
      backgroundImage: {
        "grid-glow":
          "radial-gradient(ellipse 80% 50% at 50% -20%, rgba(108, 92, 231, 0.25), transparent)",
        "flow-gradient":
          "linear-gradient(90deg, transparent, #6C5CE7, #00D4FF, transparent, #6C5CE7, #00D4FF, transparent)",
      },
    },
  },
  plugins: [],
};
