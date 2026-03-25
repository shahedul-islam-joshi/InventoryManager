using System.Collections.Generic;

namespace InventoryManager.Models.DTOs
{
    public class SupportTicketDto
    {
        public string? ReportedBy { get; set; }
        public string? Inventory { get; set; }
        public string? Link { get; set; }
        public string? Priority { get; set; }
        public string? Summary { get; set; }
        public List<string>? AdminEmails { get; set; }
    }
}
