using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LegalDoc.Core.Interfaces;
using LegalDoc.Core.DTOs;
using LegalDoc.Core.Models;
using LegalDoc.Core.Helpers;

namespace LegalDoc.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Protect entire controller
    public class ContractsController : ControllerBase
    {
        private readonly IContractRepository _contractRepository;
        private readonly IS3Service _s3Service;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly ILogger<ContractsController> _logger;

        public ContractsController(
            IContractRepository contractRepository,
            IS3Service s3Service,
            IAuditLogRepository auditLogRepository,
            ITemplateRepository templateRepository,
            ILogger<ContractsController> logger)
        {
            _contractRepository = contractRepository;
            _s3Service = s3Service;
            _auditLogRepository = auditLogRepository;
            _templateRepository = templateRepository;
            _logger = logger;
        }

        /// <summary>Get all contracts (All authenticated users)</summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ContractDto>>> GetAll()
        {
            try
            {
                var contracts = await _contractRepository.GetAllAsync();
                var contractDtos = contracts.Select(MapToDto);
                return Ok(contractDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all contracts");
                return StatusCode(500, "An error occurred while retrieving contracts");
            }
        }

        /// <summary>Get contract by ID (All authenticated users)</summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ContractDto>> GetById(int id)
        {
            try
            {
                var contract = await _contractRepository.GetByIdAsync(id);
                if (contract == null)
                    return NotFound($"Contract with ID {id} not found");

                return Ok(MapToDto(contract));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contract {ContractId}", id);
                return StatusCode(500, "An error occurred while retrieving the contract");
            }
        }

        /// <summary>Get contracts by client (All authenticated users)</summary>
        [HttpGet("client/{clientId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ContractDto>>> GetByClient(int clientId)
        {
            try
            {
                var contracts = await _contractRepository.GetByClientIdAsync(clientId);
                var contractDtos = contracts.Select(MapToDto);
                return Ok(contractDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contracts for client {ClientId}", clientId);
                return StatusCode(500, "An error occurred while retrieving contracts");
            }
        }

        /// <summary>Create new contract (Lawyers and Admins only)</summary>
        [HttpPost]
        [Authorize(Roles = "LAWYER,ADMIN")]
        public async Task<ActionResult<ContractDto>> Create([FromBody] CreateContractDto createDto)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

                var contract = new Contract
                {
                    Title = createDto.Title,
                    Content = createDto.Content,
                    Status = createDto.Status,
                    ClientId = createDto.ClientId,
                    TemplateId = createDto.TemplateId,
                    CreatedByUserId = currentUserId,
                    CreatedAt = DateTime.UtcNow
                };

                var created = await _contractRepository.CreateAsync(contract);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contract");
                return StatusCode(500, "An error occurred while creating the contract");
            }
        }

        /// <summary>Update contract (Lawyers and Admins only)</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "LAWYER,ADMIN")]
        public async Task<ActionResult<ContractDto>> Update(int id, [FromBody] UpdateContractDto updateDto)
        {
            try
            {
                var existing = await _contractRepository.GetByIdAsync(id);
                if (existing == null)
                    return NotFound($"Contract with ID {id} not found");

                existing.Title = updateDto.Title ?? existing.Title;
                existing.Content = updateDto.Content ?? existing.Content;
                existing.Status = updateDto.Status ?? existing.Status;
                existing.UpdatedAt = DateTime.UtcNow;

                var updated = await _contractRepository.UpdateAsync(existing);
                return Ok(MapToDto(updated));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contract {ContractId}", id);
                return StatusCode(500, "An error occurred while updating the contract");
            }
        }

        /// <summary>Delete contract (Admins only)</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _contractRepository.DeleteAsync(id);
                if (!success)
                    return NotFound($"Contract with ID {id} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contract {ContractId}", id);
                return StatusCode(500, "An error occurred while deleting the contract");
            }
        }

        /// <summary>Get pre-signed URL to download contract .docx file</summary>
        [HttpGet("{id}/download/docx")]
        [Authorize(Policy = "RequireAnyUser")]
        public async Task<ActionResult<FileDownloadDto>> DownloadDocx(int id)
        {
            try
            {
                var contract = await _contractRepository.GetByIdAsync(id);
                if (contract is null) return NotFound();
                if (string.IsNullOrEmpty(contract.S3Key))
                    return NotFound(new { message = "No file attached to this contract." });

                var url = await _s3Service.GenerateDownloadUrlAsync(contract.S3Key);

                await _auditLogRepository.LogAsync(id, GetCurrentUserId(), "DOWNLOAD_DOCX");

                return Ok(new FileDownloadDto
                {
                    PresignedUrl = url,
                    FileName = $"{contract.Title}.docx",
                    ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading docx for contract {ContractId}", id);
                return StatusCode(500, "An error occurred while generating download URL");
            }
        }

        /// <summary>Get pre-signed URL to download contract PDF (generated by Lambda)</summary>
        [HttpGet("{id}/download/pdf")]
        [Authorize(Policy = "RequireAnyUser")]
        public async Task<ActionResult<FileDownloadDto>> DownloadPdf(int id)
        {
            try
            {
                var contract = await _contractRepository.GetByIdAsync(id);
                if (contract is null) return NotFound();

                var pdfKey = S3KeyHelper.ContractPdfKey(id, contract.CreatedAt);
                if (!await _s3Service.FileExistsAsync(pdfKey))
                    return NotFound(new { message = "PDF not yet generated. Try again in a moment." });

                var url = await _s3Service.GenerateDownloadUrlAsync(pdfKey);

                await _auditLogRepository.LogAsync(id, GetCurrentUserId(), "DOWNLOAD_PDF");

                return Ok(new FileDownloadDto
                {
                    PresignedUrl = url,
                    FileName = $"{contract.Title}.pdf",
                    ContentType = "application/pdf",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading pdf for contract {ContractId}", id);
                return StatusCode(500, "An error occurred while generating download URL");
            }
        }

        /// <summary>Get pre-signed URL for direct client upload to S3, then confirm</summary>
        [HttpPost("{id}/upload-url")]
        [Authorize(Policy = "RequireLawyer")]
        public async Task<ActionResult<FileUploadUrlDto>> GetUploadUrl(int id)
        {
            try
            {
                var contract = await _contractRepository.GetByIdAsync(id);
                if (contract is null) return NotFound();

                var key = S3KeyHelper.ContractDocxKey(id, contract.CreatedAt);
                var url = await _s3Service.GenerateUploadUrlAsync(key,
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

                return Ok(new FileUploadUrlDto
                {
                    PresignedUrl = url,
                    S3Key = key,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating upload URL for contract {ContractId}", id);
                return StatusCode(500, "An error occurred while generating upload URL");
            }
        }

        /// <summary>After client uploads directly to S3, confirm and save the S3 key</summary>
        [HttpPost("{id}/confirm-upload")]
        [Authorize(Policy = "RequireLawyer")]
        public async Task<IActionResult> ConfirmUpload(int id, [FromBody] string s3Key)
        {
            try
            {
                var contract = await _contractRepository.GetByIdAsync(id);
                if (contract is null) return NotFound();

                if (!await _s3Service.FileExistsAsync(s3Key))
                    return BadRequest(new { message = "File not found in S3. Upload may have failed." });

                contract.S3Key = s3Key;
                await _contractRepository.UpdateAsync(contract);
                await _auditLogRepository.LogAsync(id, GetCurrentUserId(), "UPLOAD_DOCX");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming upload for contract {ContractId}", id);
                return StatusCode(500, "An error occurred while confirming upload");
            }
        }

        /// <summary>Create contract from template — copies template file to contracts folder</summary>
        [HttpPost("{id}/from-template/{templateId}")]
        [Authorize(Policy = "RequireLawyer")]
        public async Task<IActionResult> CreateFromTemplate(int id, int templateId)
        {
            try
            {
                var contract = await _contractRepository.GetByIdAsync(id);
                var template = await _templateRepository.GetByIdAsync(templateId);

                if (contract is null || template is null) return NotFound();

                var sourceKey = S3KeyHelper.TemplateKey(templateId);
                var destKey = S3KeyHelper.ContractDocxKey(id, contract.CreatedAt);

                await _s3Service.CopyFileAsync(sourceKey, destKey);

                contract.S3Key = destKey;
                contract.TemplateId = templateId;
                await _contractRepository.UpdateAsync(contract);
                await _auditLogRepository.LogAsync(id, GetCurrentUserId(), "CREATED_FROM_TEMPLATE");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contract from template. ContractId: {ContractId}, TemplateId: {TemplateId}", id, templateId);
                return StatusCode(500, "An error occurred while creating contract from template");
            }
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }

        private static ContractDto MapToDto(Contract contract) => new()
        {
            Id = contract.Id,
            Title = contract.Title,
            Content = contract.Content,
            Status = contract.Status,
            ClientId = contract.ClientId,
            TemplateId = contract.TemplateId,
            CreatedByUserId = contract.CreatedByUserId,
            S3Key = contract.S3Key,
            CreatedAt = contract.CreatedAt,
            UpdatedAt = contract.UpdatedAt
        };
    }
}