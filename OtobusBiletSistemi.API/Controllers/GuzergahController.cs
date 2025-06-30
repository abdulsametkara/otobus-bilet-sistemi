using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OtobusBiletSistemi.Core.Entities;
using OtobusBiletSistemi.Core.Interfaces;

namespace OtobusBiletSistemi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GuzergahController : ControllerBase
    {
        private readonly IRepository<Guzergah> _guzergahRepository;

        public GuzergahController(IRepository<Guzergah> guzergahRepository)
        {
            _guzergahRepository = guzergahRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<Guzergah>>> GetAllGuzergah()
        {
            try
            {
                var guzergahlar = await _guzergahRepository.GetAllAsync();
                return Ok(guzergahlar);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Guzergah>> GetGuzergah(int id)
        {
            try
            {
                var guzergah = await _guzergahRepository.GetByIdAsync(id);
                if (guzergah == null)
                {
                    return NotFound($"ID {id} olan güzergah bulunamadı.");
                }
                return Ok(guzergah);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Guzergah>> CreateGuzergah(Guzergah guzergah)
        {
            try
            {
                if (guzergah == null)
                {
                    return BadRequest("Güzergah bilgileri boş olamaz.");

                }
                var createdGuzergah = await _guzergahRepository.AddAsync(guzergah);

                return CreatedAtAction(
                    nameof(GetGuzergah),
                    new {id = createdGuzergah.GuzergahID},
                    createdGuzergah
                    );
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]

        public async Task<ActionResult<Guzergah>> UpdateGuzergah(int id, Guzergah guzergah)
        {
            try
            {
                if (id != guzergah.GuzergahID)
                {
                    return BadRequest("ID uyumsuzluğu");
                }
     
                var existingGuzergah = await _guzergahRepository.GetByIdAsync(id);
                
                if (existingGuzergah == null)
                {
                    return NotFound($"ID {id} olan güzergah bulunamadı.");
                }

                var updatedGuzergah = await _guzergahRepository.UpdateAsync(guzergah);
                return Ok(updatedGuzergah);
            }
            
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteGuzergah(int id)
        {
            try
            {
                var result = await _guzergahRepository.DeleteAsync(id);
                if (!result)
                {
                    return NotFound($"ID {id} olan güzergah bulunamadı.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
