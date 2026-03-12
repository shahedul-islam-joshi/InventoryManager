// access-autocomplete.js
// Provides email autocomplete for the grant-access input.
// Fetches users matching the typed prefix from /Inventory/UserSearch?prefix=...

document.addEventListener('DOMContentLoaded', () => {
    const input = document.getElementById('access-email-input');
    if (!input) return;

    let dropdown = document.createElement('ul');
    dropdown.className = 'list-group position-absolute shadow z-3 bg-white';
    dropdown.style.cssText = 'min-width:260px;display:none;';
    input.parentElement.style.position = 'relative';
    input.parentElement.appendChild(dropdown);

    let debounce;
    input.addEventListener('input', () => {
        clearTimeout(debounce);
        const prefix = input.value.trim();
        if (prefix.length < 2) { dropdown.style.display = 'none'; return; }

        debounce = setTimeout(async () => {
            try {
                const res = await fetch(`/Inventory/UserSearch?prefix=${encodeURIComponent(prefix)}`);
                const users = await res.json();
                dropdown.innerHTML = '';
                if (!users.length) { dropdown.style.display = 'none'; return; }
                users.forEach(u => {
                    const li = document.createElement('li');
                    li.className = 'list-group-item list-group-item-action px-3 py-2';
                    li.textContent = u;
                    li.style.cursor = 'pointer';
                    li.addEventListener('click', () => {
                        input.value = u;
                        dropdown.style.display = 'none';
                    });
                    dropdown.appendChild(li);
                });
                dropdown.style.display = 'block';
            } catch { dropdown.style.display = 'none'; }
        }, 250);
    });

    document.addEventListener('click', e => {
        if (e.target !== input) dropdown.style.display = 'none';
    });
});
