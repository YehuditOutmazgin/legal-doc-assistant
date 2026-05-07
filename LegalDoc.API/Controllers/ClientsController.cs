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
    public class ClientsController : ControllerBase
    {
        private readonly IClientRepository _clientRepository;
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(IClientRepository clientRepository, ILogger<ClientsController> logger)
        {
            _clientRepository = clientRepository;
            _logger = logger;
        }

        /// <summary>Get all clients (All authenticated users)</summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ClientDto>>> GetAll()
        {
            try
            {
                var clients = await _clientRepository.GetAllAsync();
                var clientDtos = clients.Select(MapToDto);
                return Ok(clientDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all clients");
                return StatusCode(500, "An error occurred while retrieving clients");
            }
        }

        /// <summary>Get client by ID (All authenticated users)</summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ClientDto>> GetById(int id)
        {
            try
            {
                var client = await _clientRepository.GetByIdAsync(id);
                if (client == null)
                    return NotFound($"Client with ID {id} not found");

                return Ok(MapToDto(client));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client {ClientId}", id);
                return StatusCode(500, "An error occurred while retrieving the client");
            }
        }

        /// <summary>Create new client (Lawyers and Admins only)</summary>
        [HttpPost]
        [Authorize(Roles = "LAWYER,ADMIN")]
        public async Task<ActionResult<ClientDto>> Create([FromBody] CreateClientDto createDto)
        {
            try
            {
                var client = new Client
                {
                    Name = createDto.Name,
                    Type = createDto.Type,
                    Email = createDto.Email,
                    Phone = createDto.Phone,
                    Address = createDto.Address,
                    CompanyRegistrationNumber = createDto.CompanyRegistrationNumber,
                    ContactPersonName = createDto.ContactPersonName,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var created = await _clientRepository.CreateAsync(client);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client");
                return StatusCode(500, "An error occurred while creating the client");
            }
        }

        /// <summary>Update client (Lawyers and Admins only)</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "LAWYER,ADMIN")]
        public async Task<ActionResult<ClientDto>> Update(int id, [FromBody] UpdateClientDto updateDto)
        {
            try
            {
                var existing = await _clientRepository.GetByIdAsync(id);
                if (existing == null)
                    return NotFound($"Client with ID {id} not found");

                existing.Name = updateDto.Name ?? existing.Name;
                existing.Email = updateDto.Email ?? existing.Email;
                existing.Phone = updateDto.Phone ?? existing.Phone;
                existing.Address = updateDto.Address ?? existing.Address;
                existing.CompanyRegistrationNumber = updateDto.CompanyRegistrationNumber;
                existing.ContactPersonName = updateDto.ContactPersonName;
                existing.UpdatedAt = DateTime.UtcNow;

                var updated = await _clientRepository.UpdateAsync(existing);
                return Ok(MapToDto(updated));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client {ClientId}", id);
                return StatusCode(500, "An error occurred while updating the client");
            }
        }

        /// <summary>Delete client (Admins only)</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _clientRepository.DeleteAsync(id);
                if (!success)
                    return NotFound($"Client with ID {id} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting client {ClientId}", id);
                return StatusCode(500, "An error occurred while deleting the client");
            }
        }

        private static ClientDto MapToDto(Client client) => new()
        {
            Id = client.Id,
            Name = client.Name,
            Type = client.Type,
            Email = client.Email,
            Phone = client.Phone,
            Address = client.Address,
            CompanyRegistrationNumber = client.CompanyRegistrationNumber,
            ContactPersonName = client.ContactPersonName,
            CreatedAt = client.CreatedAt,
            UpdatedAt = client.UpdatedAt,
            IsActive = client.IsActive
        };
    }
}