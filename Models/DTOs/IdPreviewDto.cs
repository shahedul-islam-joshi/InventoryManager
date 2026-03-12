namespace InventoryManager.Models.DTOs
{
    // Preview of a generated custom ID — returned via AJAX as the user builds the template.
    public class IdPreviewDto
    {
        // The ID string as it would be generated right now
        public string Preview { get; set; } = string.Empty;

        // Any validation warning (e.g. segment conflict, empty result)
        public string? Warning { get; set; }
    }
}
