// inventory-id-builder.js
// Manages the custom ID template builder on the Inventory Details > Custom ID tab.
// Sends a GET to /Inventory/IdPreview?inventoryId=... to fetch a live preview.

function initIdBuilder(inventoryId, segmentCount) {
    if (segmentCount === 0) return;

    fetchPreview(inventoryId);
}

async function fetchPreview(inventoryId) {
    try {
        const response = await fetch(`/Inventory/IdPreview?inventoryId=${inventoryId}`);
        if (!response.ok) return;
        const data = await response.json();
        const el = document.getElementById('id-preview-value');
        if (el) el.textContent = data.preview || '(empty)';
    } catch (e) {
        console.warn('ID preview fetch failed:', e);
    }
}
