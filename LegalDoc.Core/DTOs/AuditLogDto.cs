namespace LegalDoc.Core.DTOs
{
    public class AuditLogDto
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public string ContractTitle { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; }
    }
}