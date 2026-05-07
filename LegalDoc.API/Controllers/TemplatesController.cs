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
    public class TemplatesController : ControllerBase
    {
        private readonly ITemplateRepository _templateRepository;
        private readonly ILogger<TemplatesController> _logger;

        public TemplatesController(ITemplateRepository templateRepository, ILogger<TemplatesController> logger)
        {
            _templateRepository = templateRepository;
            _logger = logger;
        }

        /// <summary>Get all templates (All authenticated users)</summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TemplateDto>>> GetAll()
        {
            try
            {
                var templates = await _templateRepository.GetAllAsync();
                var templateDtos = templates.Select(MapToDto);
                return Ok(templateDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all templates");
                return StatusCode(500, "An error occurred while retrieving templates");
            }
        }

        /// <summary>Get template by ID (All authenticated users)</summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<TemplateDto>> GetById(int id)
        {
            try
            {
                var template = await _templateRepository.GetByIdAsync(id);
                if (template == null)
                    return NotFound($"Template with ID {id} not found");

                return Ok(MapToDto(template));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving template {TemplateId}", id);
                return StatusCode(500, "An error occurred while retrieving the template");
            }
        }

        /// <summary>Get templates by category (All authenticated users)</summary>
        [HttpGet("category/{category}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TemplateDto>>> GetByCategory(string category)
        {
            try
            {
                var templates = await _templateRepository.GetByCategoryAsync(category);
                var templateDtos = templates.Select(MapToDto);
                return Ok(templateDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving templates for category {Category}", category);
                return StatusCode(500, "An error occurred while retrieving templates");
            }
        }

        /// <summary>Create new template (Lawyers and Admins only)</summary>
        [HttpPost]
        [Authorize(Roles = "LAWYER,ADMIN")]
        public async Task<ActionResult<TemplateDto>> Create([FromBody] CreateTemplateDto createDto)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

                var template = new Template
                {
                    Name = createDto.Name,
                    Description = createDto.Description,
                    Content = createDto.Content,
                    Category = createDto.Category,
                    CreatedByUserId = currentUserId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var created = await _templateRepository.CreateAsync(template);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating template");
                return StatusCode(500, "An error occurred while creating the template");
            }
        }

        /// <summary>Update template (Lawyers and Admins only)</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "LAWYER,ADMIN")]
        public async Task<ActionResult<TemplateDto>> Update(int id, [FromBody] UpdateTemplateDto updateDto)
        {
            try
            {
                var existing = await _templateRepository.GetByIdAsync(id);
                if (existing == null)
                    return NotFound($"Template with ID {id} not found");

                existing.Name = updateDto.Name ?? existing.Name;
                existing.Description = updateDto.Description ?? existing.Description;
                existing.Content = updateDto.Content ?? existing.Content;
                existing.Category = updateDto.Category ?? existing.Category;
                existing.UpdatedAt = DateTime.UtcNow;

                var updated = await _templateRepository.UpdateAsync(existing);
                return Ok(MapToDto(updated));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating template {TemplateId}", id);
                return StatusCode(500, "An error occurred while updating the template");
            }
        }

        /// <summary>Delete template (Admins only)</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _templateRepository.DeleteAsync(id);
                if (!success)
                    return NotFound($"Template with ID {id} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting template {TemplateId}", id);
                return StatusCode(500, "An error occurred while deleting the template");
            }
        }

        private static TemplateDto MapToDto(Template template) => new()
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            Content = template.Content,
            Category = template.Category,
            CreatedByUserId = template.CreatedByUserId,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt,
            IsActive = template.IsActive
        };
    }
}