using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OtobusBiletSistemi.Core.Entities;
using OtobusBiletSistemi.Core.Interfaces;


namespace OtobusBiletSistemi.API.Controllers
{
    [Route ("api/[controller]")]
    [ApiController]
    public class KoltukController : ControllerBase
    {

        private readonly IRepository<Koltuk> _koltukRepository;

        public KoltukController(IRepository<Koltuk> koltukRepository)
        {
            _koltukRepository = koltukRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<Koltuk>>> GetAllKoltuklar()
        {
            try
            {
                var koltuklar = await _koltukRepository.GetAllAsync();
                return Ok(koltuklar);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Koltuk>> GetKoltuk(int id)
        {
            try
            {
                var koltuk = await _koltukRepository.GetByIdAsync(id);
                if (koltuk == null)
                {
                    return NotFound($"ID {id} olan koltuk bulunamadı.");
                }
                return Ok(koltuk);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Koltuk>> CreateKoltuk(Koltuk koltuk)
        {
            try
            {
                if (koltuk == null)
                {
                    return BadRequest("Koltuk bilgileri boş olamaz.");
                }
                var createdKoltuk = await _koltukRepository.AddAsync(koltuk);
                return CreatedAtAction(
                    nameof(GetKoltuk),
                    new {id = createdKoltuk.KoltukID},
                    createdKoltuk
                    );
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Koltuk>> UpdateKoltuk(int id, Koltuk koltuk)
        {
            try
            {
                if (id != koltuk.KoltukID)
                {
                    return BadRequest("ID uyumsuzluğu.");
                }

                var existingKoltuk = await _koltukRepository.GetByIdAsync(id);
                if (existingKoltuk == null)
                {
                    return NotFound($"ID {id} olan koltuk bulunamadı.");

                }

                var updatedKoltuk = await _koltukRepository.UpdateAsync(koltuk);
                return Ok(koltuk);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteKoltuk(int id)
        {
            try
            {
                var result = await _koltukRepository.DeleteAsync(id);
                if (!result)
                {
                    return NotFound($"ID {id} olan koltuk bulunamadı.");
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
