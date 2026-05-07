namespace LegalDoc.Core.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; }
    }
}