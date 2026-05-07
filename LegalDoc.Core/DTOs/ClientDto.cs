using LegalDoc.Core.Enums;

namespace LegalDoc.Core.DTOs
{
    public class ClientDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ClientType Type { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? CompanyRegistrationNumber { get; set; }
        public string? ContactPersonName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
    
    public class CreateClientDto
    {
        public string Name { get; set; } = string.Empty;
        public ClientType Type { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? CompanyRegistrationNumber { get; set; }
        public string? ContactPersonName { get; set; }
    }
    
    public class UpdateClientDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? CompanyRegistrationNumber { get; set; }
        public string? ContactPersonName { get; set; }
        public bool? IsActive { get; set; }
    }
}