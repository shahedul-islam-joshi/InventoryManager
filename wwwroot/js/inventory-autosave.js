/*
 * inventory-autosave.js
 *
 * Auto-saves the inventory edit form in the background.
 *
 * BEHAVIOUR:
 *   - Watches Title, Description, Category, ImageUrl, IsPublic for changes.
 *   - After any change, waits 8 seconds (debounced — each new change resets
 *     the timer) then POSTs to /Inventory/AutoSave.
 *   - Shows a small status badge near the top of the form:
 *       "Saving…"          — while the request is in flight
 *       "Saved ✓"          — after a 200 OK response
 *       "Conflict — please reload"  — after a 409 Conflict (stops auto-saving)
 *   - On 200 OK the server returns { version: "<base64>" }. The hidden Version
 *     field is updated so the next auto-save sends the current rowversion.
 *   - On 409 the timer is cancelled and no further saves are attempted.
 *
 * REQUIREMENTS:
 *   - The form must have id="autosave-form".
 *   - The page must include the antiforgery token in the form
 *     (asp-action="Edit" generates it automatically via AntiForgeryToken).
 *   - The status badge element with id="autosave-status" must exist in the DOM
 *     (injected by Edit.cshtml just above the form).
 */

(function () {
    'use strict';

    // -----------------------------------------------------------------------
    // Locate key DOM elements
    // -----------------------------------------------------------------------
    var form = document.getElementById('autosave-form');
    if (!form) return; // Guard: only run on the edit page

    var statusEl = document.getElementById('autosave-status');

    // Fields to watch for changes
    var watchedIds = ['Title', 'Description', 'Category', 'ImageUrl', 'IsPublic'];

    // -----------------------------------------------------------------------
    // State
    // -----------------------------------------------------------------------
    var debounceTimer = null;
    var DEBOUNCE_MS   = 8000; // 8 seconds
    var stopped       = false; // Set to true on 409 — no more saves

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------
    function setStatus(text, cssClass) {
        if (!statusEl) return;
        statusEl.textContent = text;
        statusEl.className   = 'autosave-status ' + (cssClass || '');
    }

    function scheduleAutoSave() {
        if (stopped) return;

        // Reset the debounce timer on every keystroke / change
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(doAutoSave, DEBOUNCE_MS);
    }

    // -----------------------------------------------------------------------
    // doAutoSave
    // Collects the form fields, sends them to /Inventory/AutoSave via fetch,
    // and handles the response.
    // -----------------------------------------------------------------------
    function doAutoSave() {
        if (stopped) return;

        setStatus('Saving\u2026', 'as-saving');

        // Build the payload from the current form values
        var data = new FormData();

        // Anti-forgery token — ASP.NET Core MVC requires this for all POSTs
        var token = form.querySelector('input[name="__RequestVerificationToken"]');
        if (token) data.append('__RequestVerificationToken', token.value);

        // Primary key — always required so the server knows which record to update
        var idField = form.querySelector('input[name="Id"]');
        if (idField) data.append('Id', idField.value);

        // Rowversion — critical for optimistic concurrency
        var versionField = form.querySelector('input[name="Version"]');
        if (versionField) data.append('Version', versionField.value);

        // Watched content fields
        watchedIds.forEach(function (fieldId) {
            var el = document.getElementById(fieldId);
            if (!el) return;
            if (el.type === 'checkbox') {
                // Checkboxes must send 'true'/'false' as a string so model binding works
                data.append(fieldId, el.checked ? 'true' : 'false');
            } else {
                data.append(fieldId, el.value);
            }
        });

        fetch('/Inventory/AutoSave', {
            method: 'POST',
            body:   data
        })
        .then(function (response) {
            if (response.status === 409) {
                // Optimistic lock conflict — another user saved this record.
                // Stop auto-saving so we don't keep overwriting their changes.
                stopped = true;
                setStatus('Conflict \u2014 please reload', 'as-conflict');
                return null;
            }

            if (!response.ok) {
                setStatus('Save failed (' + response.status + ')', 'as-error');
                return null;
            }

            return response.json();
        })
        .then(function (json) {
            if (!json) return; // null returned on error / conflict paths above

            // Update the hidden Version field with the new rowversion returned
            // by the server. Without this the next save would send a stale token
            // and the server would always return 409.
            var versionField = form.querySelector('input[name="Version"]');
            if (versionField && json.version != null) {
                versionField.value = json.version;
            }

            setStatus('Saved \u2713', 'as-saved');

            // Fade the "Saved" message out after 3 seconds so it doesn't distract
            setTimeout(function () {
                setStatus('', '');
            }, 3000);
        })
        .catch(function (err) {
            // Network failure — log but don't block the user
            console.error('AutoSave network error:', err);
            setStatus('Save failed (network error)', 'as-error');
        });
    }

    // -----------------------------------------------------------------------
    // Attach change listeners to watched fields
    // -----------------------------------------------------------------------
    watchedIds.forEach(function (fieldId) {
        var el = document.getElementById(fieldId);
        if (!el) return;

        // 'input' fires on every keystroke for text fields
        el.addEventListener('input', scheduleAutoSave);

        // 'change' fires when a checkbox is toggled or a field loses focus with
        // a new value — ensures checkbox changes are captured too
        el.addEventListener('change', scheduleAutoSave);
    });

})();
