using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OtobusBiletSistemi.Core.Entities;
using OtobusBiletSistemi.Core.Interfaces;

namespace OtobusBiletSistemi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeferController : ControllerBase
    {
        private readonly IRepository<Sefer> _seferRepository;

        public SeferController(IRepository<Sefer> seferRepository)
        {
            _seferRepository = seferRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<Sefer>>> GetAllSeferler()
        {
            try
            {
                var seferler = await _seferRepository.GetAllAsync();
                return Ok(seferler);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Sefer>> GetSefer(int id)
        {
            try
            {
                var sefer = await _seferRepository.GetByIdAsync(id);
                if (sefer == null)
                {
                    return NotFound($"ID {id} olan sefer bulunamadı.");
                }
                return Ok(sefer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Sefer>> CreateSefer(Sefer sefer)
        {
            try
            {
                if (sefer == null)
                {
                    return BadRequest("Sefer bilgileri boş olamaz.");
                }

                var createdSefer = await _seferRepository.AddAsync(sefer);

                return CreatedAtAction(
                    nameof(GetSefer),
                    new {id = createdSefer.SeferID},
                    createdSefer
                    );

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Sefer>> UpdateSefer(int id, Sefer sefer)
        {
            try
            {
                if (id != sefer.SeferID)
                {
                    return BadRequest("ID uyumsuzluğu.");

                }

                var existingSefer = await _seferRepository.GetByIdAsync(id);

                if (existingSefer == null)
                {
                    return NotFound($"ID {id} olan sefer bulunamadı.");
                }

                var updatedSefer = await _seferRepository.UpdateAsync(sefer);
                return Ok(sefer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSefer(int id)
        {
            try
            {
                var sefer = await _seferRepository.GetByIdAsync(id);
                if (sefer == null)
                {
                    return NotFound($"ID {id} olan sefer bulunamadı.");
                }

                await _seferRepository.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
