namespace InventoryManager.Models.DTOs
{
    // Returned by the tag autocomplete endpoint — minimal data for Select2 / TomSelect.
    public class TagAutocompleteDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
