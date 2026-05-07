using LegalDoc.Core.Enums;

namespace LegalDoc.Core.Models
{
    public class Contract
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public ContractStatus Status { get; set; }
        public int ClientId { get; set; }
        public int? TemplateId { get; set; }
        public int CreatedByUserId { get; set; }
        public int? AssignedToUserId { get; set; }
        public string? S3Key { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? SignedAt { get; set; }
        public string? SignedByName { get; set; }
        public string? Notes { get; set; }
    }
}