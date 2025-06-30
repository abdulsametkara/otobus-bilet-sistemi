using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OtobusBiletSistemi.Core.Entities;
using OtobusBiletSistemi.Core.Interfaces;

namespace OtobusBiletSistemi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BiletController : ControllerBase
    {
        private readonly IRepository<Bilet> _biletRepository;
        public BiletController(IRepository<Bilet> biletRepository)
        {
            _biletRepository = biletRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<Bilet>>> GetAllBiletler()
        {
            try
            {
                var biletler = await _biletRepository.GetAllAsync();
                return Ok(biletler);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Bilet>> GetBilet(int id)
        {
            try
            {
                var biletler = await _biletRepository.GetByIdAsync(id);
                
                if (biletler == null)
                {
                    return NotFound($"ID {id} olan bilet bulunamadı.");
                }

                return Ok(biletler);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Bilet>> CreateBilet(Bilet bilet)
        {
            try
            {
                if (bilet == null)
                {
                    return BadRequest("Bilet bilgileri boş olamaz.");
                }
                var createdBilet = await _biletRepository.AddAsync(bilet);
                return CreatedAtAction(
                    nameof(GetBilet),
                    new {id = createdBilet.KoltukID},
                    createdBilet
                    );
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Bilet>> UpdateBilet(int id, Bilet bilet)
        {
            try
            {
                if (id != bilet.BiletID)
                {
                    return BadRequest("ID uyumsuzluğu.");
                }
                var existingBilet = await _biletRepository.GetByIdAsync(id);
                if (existingBilet == null)
                {
                    return NotFound($"ID {id} olan bilet bulunamadı.");
                }
                var updatedBilet = await _biletRepository.UpdateAsync(bilet);
                return Ok(bilet);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBilet(int id)
        {
            try
            {
                var result = await _biletRepository.DeleteAsync(id);
                if (!result)
                {
                    return NotFound($"ID {id} olan yolcu bulunamadı.");
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
