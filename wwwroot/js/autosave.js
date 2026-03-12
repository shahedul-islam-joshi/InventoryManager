// autosave.js
// Optional autosave for long forms (e.g., item description).
// Attaches to any textarea or input with data-autosave="true".

document.addEventListener('DOMContentLoaded', () => {
    const fields = document.querySelectorAll('[data-autosave="true"]');

    fields.forEach(field => {
        const key = `autosave_${window.location.pathname}_${field.name || field.id}`;

        // Restore saved content on load
        const saved = sessionStorage.getItem(key);
        if (saved && field.value === '') {
            field.value = saved;
        }

        // Save on input, debounced
        let timer;
        field.addEventListener('input', () => {
            clearTimeout(timer);
            timer = setTimeout(() => {
                sessionStorage.setItem(key, field.value);
            }, 500);
        });

        // Clear on form submit
        field.closest('form')?.addEventListener('submit', () => {
            sessionStorage.removeItem(key);
        });
    });
});
