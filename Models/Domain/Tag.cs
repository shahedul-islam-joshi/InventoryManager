namespace InventoryManager.Models.Domain
{
    public class Tag
    {
        public int Id { get; set; }

        // Tag name — lowercase, trimmed; unique enforced in DB
        public string Name { get; set; } = string.Empty;

        // Navigation property for the join table
        public ICollection<InventoryTag> InventoryTags { get; set; } = new List<InventoryTag>();
    }
}
