// tag-autocomplete.js
// Initialises tag autocomplete on inputs with data-tag-input="true".
// Fetches suggestions from /Tag/Search?prefix=... and uses a simple dropdown UI.

document.addEventListener('DOMContentLoaded', () => {
    const inputs = document.querySelectorAll('[data-tag-input="true"]');

    inputs.forEach(input => {
        let dropdown = document.createElement('ul');
        dropdown.className = 'list-group position-absolute shadow z-3 bg-white';
        dropdown.style.cssText = 'min-width:200px;display:none;';
        input.parentElement.style.position = 'relative';
        input.parentElement.appendChild(dropdown);

        let debounce;
        input.addEventListener('input', () => {
            clearTimeout(debounce);
            const raw = input.value;
            const parts = raw.split(',');
            const prefix = parts[parts.length - 1].trim();

            if (prefix.length < 1) { dropdown.style.display = 'none'; return; }

            debounce = setTimeout(async () => {
                const res = await fetch(`/Tag/Search?prefix=${encodeURIComponent(prefix)}`);
                const tags = await res.json();
                dropdown.innerHTML = '';
                if (!tags.length) { dropdown.style.display = 'none'; return; }
                tags.forEach(t => {
                    const li = document.createElement('li');
                    li.className = 'list-group-item list-group-item-action px-3 py-2';
                    li.textContent = t.text;
                    li.style.cursor = 'pointer';
                    li.addEventListener('click', () => {
                        parts[parts.length - 1] = t.text;
                        input.value = parts.join(', ') + ', ';
                        dropdown.style.display = 'none';
                        input.focus();
                    });
                    dropdown.appendChild(li);
                });
                dropdown.style.display = 'block';
            }, 200);
        });

        document.addEventListener('click', e => {
            if (e.target !== input) dropdown.style.display = 'none';
        });
    });
});
