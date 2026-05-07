namespace LegalDoc.Core.Helpers
{
    /// <summary>
    /// Centralized S3 key generation - one place to change paths
    /// </summary>
    public static class S3KeyHelper
    {
        public static string TemplateKey(int templateId)
            => $"templates/{templateId}.docx";

        public static string ContractDocxKey(int contractId, DateTime createdAt)
            => $"contracts/{createdAt:yyyy}/{createdAt:MM}/{contractId}.docx";

        public static string ContractPdfKey(int contractId, DateTime createdAt)
            => $"contracts/{createdAt:yyyy}/{createdAt:MM}/{contractId}.pdf";

        public static string TempKey(string fileName)
            => $"temp/{Guid.NewGuid()}-{fileName}";
    }
}
