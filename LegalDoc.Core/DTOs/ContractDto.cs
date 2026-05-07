using LegalDoc.Core.Enums;

namespace LegalDoc.Core.DTOs
{
    public class ContractDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public ContractStatus Status { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public int? TemplateId { get; set; }
        public string? TemplateName { get; set; }
        public int CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public int? AssignedToUserId { get; set; }
        public string? AssignedToUserName { get; set; }
        public string? S3Key { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? SignedAt { get; set; }
        public string? SignedByName { get; set; }
        public string? Notes { get; set; }
    }
    
    public class CreateContractDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int ClientId { get; set; }
        public int? TemplateId { get; set; }
        public int? AssignedToUserId { get; set; }
        public string? Notes { get; set; }
    }
    
    public class UpdateContractDto
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public ContractStatus? Status { get; set; }
        public int? AssignedToUserId { get; set; }
        public string? SignedByName { get; set; }
        public string? Notes { get; set; }
    }
}