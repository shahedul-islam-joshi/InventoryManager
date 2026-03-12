(function () {
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('.like-btn');
        if (!btn) return;

        const itemId = btn.dataset.id;
        if (!itemId) return;

        btn.disabled = true;

        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        const tokenValue = tokenInput ? tokenInput.value : '';

        fetch('/Item/ToggleLike', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': tokenValue
            },
            body: JSON.stringify({ itemId: itemId })
        })
        .then(function (response) {
            if (response.status === 401) {
                window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                return null;
            }
            if (!response.ok) throw new Error('Request failed: ' + response.status);
            return response.json();
        })
        .then(function (data) {
            if (!data) return;
            const countSpan = document.getElementById('like-count-' + itemId);
            if (countSpan) countSpan.textContent = data.likes;
        })
        .catch(function (err) {
            console.error('Like error:', err);
        })
        .finally(function () {
            btn.disabled = false;
        });
    });
})();
