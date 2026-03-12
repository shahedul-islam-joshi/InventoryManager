// theme-switch.js
// Toggles dark/light theme by swapping the theme stylesheet link and persisting
// the choice in localStorage. The server reads the preference on next login via
// ApplicationUser.Theme.

document.addEventListener('DOMContentLoaded', () => {
    const toggle = document.getElementById('theme-toggle');
    if (!toggle) return;

    // Apply stored theme immediately (avoid FOUC)
    const stored = localStorage.getItem('theme') || 'light';
    applyTheme(stored);

    toggle.addEventListener('click', () => {
        const current = localStorage.getItem('theme') || 'light';
        const next = current === 'dark' ? 'light' : 'dark';
        applyTheme(next);
        localStorage.setItem('theme', next);

        // Persist preference to server (best-effort, no reload required)
        fetch(`/Profile/SetTheme?theme=${next}`, { method: 'POST' }).catch(() => { });
    });
});

function applyTheme(theme) {
    const link = document.getElementById('theme-stylesheet');
    if (!link) return;
    link.href = `/css/${theme}-theme.css`;

    const toggle = document.getElementById('theme-toggle');
    if (toggle) toggle.textContent = theme === 'dark' ? '☀️ Light' : '🌙 Dark';
}
