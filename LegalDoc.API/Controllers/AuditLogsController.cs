using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LegalDoc.Core.Interfaces;
using LegalDoc.Core.DTOs;

namespace LegalDoc.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ILogger<AuditLogsController> _logger;

        public AuditLogsController(IAuditLogRepository auditLogRepository, ILogger<AuditLogsController> logger)
        {
            _auditLogRepository = auditLogRepository;
            _logger = logger;
        }

        /// <summary>Get recent audit logs (All authenticated users)</summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetRecent([FromQuery] int limit = 50)
        {
            try
            {
                var logs = await _auditLogRepository.GetRecentAsync(limit);
                var logDtos = logs.Select(log => new AuditLogDto
                {
                    Id = log.Id,
                    ContractId = log.ContractId,
                    UserId = log.UserId,
                    Action = log.Action,
                    Details = log.Details,
                    Timestamp = log.Timestamp
                });

                return Ok(logDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent audit logs");
                return StatusCode(500, "An error occurred while retrieving audit logs");
            }
        }

        /// <summary>Get audit logs by contract (All authenticated users)</summary>
        [HttpGet("contract/{contractId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetByContract(int contractId)
        {
            try
            {
                var logs = await _auditLogRepository.GetByContractIdAsync(contractId);
                var logDtos = logs.Select(log => new AuditLogDto
                {
                    Id = log.Id,
                    ContractId = log.ContractId,
                    UserId = log.UserId,
                    Action = log.Action,
                    Details = log.Details,
                    Timestamp = log.Timestamp
                });

                return Ok(logDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs for contract {ContractId}", contractId);
                return StatusCode(500, "An error occurred while retrieving audit logs");
            }
        }

        /// <summary>Get audit logs by user (Admin only)</summary>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetByUser(int userId)
        {
            try
            {
                var logs = await _auditLogRepository.GetByUserIdAsync(userId);
                var logDtos = logs.Select(log => new AuditLogDto
                {
                    Id = log.Id,
                    ContractId = log.ContractId,
                    UserId = log.UserId,
                    Action = log.Action,
                    Details = log.Details,
                    Timestamp = log.Timestamp
                });

                return Ok(logDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs for user {UserId}", userId);
                return StatusCode(500, "An error occurred while retrieving audit logs");
            }
        }
    }
}
