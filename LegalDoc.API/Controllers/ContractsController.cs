using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LegalDoc.Core.Interfaces;
using LegalDoc.Core.DTOs;
using LegalDoc.Core.Models;

namespace LegalDoc.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Protect entire controller
    public class ContractsController : ControllerBase
    {
        private readonly IContractRepository _contractRepository;
        private readonly ILogger<ContractsController> _logger;

        public ContractsController(IContractRepository contractRepository, ILogger<ContractsController> logger)
        {
            _contractRepository = contractRepository;
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

        private static ContractDto MapToDto(Contract contract) => new()
        {
            Id = contract.Id,
            Title = contract.Title,
            Content = contract.Content,
            Status = contract.Status,
            ClientId = contract.ClientId,
            TemplateId = contract.TemplateId,
            CreatedByUserId = contract.CreatedByUserId,
            CreatedAt = contract.CreatedAt,
            UpdatedAt = contract.UpdatedAt
        };
    }
}