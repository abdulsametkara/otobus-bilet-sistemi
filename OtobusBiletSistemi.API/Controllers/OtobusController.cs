using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OtobusBiletSistemi.Core.Entities;
using OtobusBiletSistemi.Core.Interfaces;

namespace OtobusBiletSistemi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OtobusController : ControllerBase
    {
        private readonly IRepository<Otobus> _otobusRepository;

        public OtobusController(IRepository<Otobus> otobusRepository)
        {
            _otobusRepository = otobusRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<Otobus>>> GetAllOtobusler()
        {
            try
            {
                var otobusler = await _otobusRepository.GetAllAsync();
                return Ok(otobusler);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Otobus>> GetOtobus(int id)
        {
            try
            {
                var otobus = await _otobusRepository.GetByIdAsync(id);

                if (otobus == null)
                {
                    return NotFound($"ID {id} olan otobus bulunamadı.");
                }

                return Ok(otobus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Otobus>> CreateOtobus(Otobus otobus)
        {
            try
            {
                if (otobus == null)
                {
                    return BadRequest("Otobus bilgileri boş olamaz.");
                }

                var createdOtobus = await _otobusRepository.AddAsync(otobus);

                return CreatedAtAction(
                    nameof(GetOtobus),
                    new { id = createdOtobus.OtobusID },
                    createdOtobus
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Otobus>> UpdateOtobus(int id, Otobus otobus)
        {
            try
            {
                if (id != otobus.OtobusID)
                {
                    return BadRequest("ID uyumsuzluğu.");
                }

                var existingOtobus = await _otobusRepository.GetByIdAsync(id);
                if (existingOtobus == null)
                {
                    return NotFound($"ID {id} olan otobus bulunamadı.");
                }

                var updatedOtobus = await _otobusRepository.UpdateAsync(otobus);
                return Ok(updatedOtobus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOtobus(int id)
        {
            try
            {
                var result = await _otobusRepository.DeleteAsync(id);

                if (!result)
                {
                    return NotFound($"ID {id} olan otobus bulunamadı.");
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
