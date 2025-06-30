using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OtobusBiletSistemi.Core.Entities;
using OtobusBiletSistemi.Core.Interfaces;

namespace OtobusBiletSistemi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class YolcuController : ControllerBase
    {
        private readonly IRepository<Yolcu> _yolcuRepository;

        public YolcuController(IRepository<Yolcu> yolcuRepository)
        {
            _yolcuRepository = yolcuRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<Yolcu>>> GetAllYolcular()
        {
            try
            {
                var yolcular = await _yolcuRepository.GetAllAsync();
                return Ok(yolcular);
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Yolcu>> GetYolcu(int id)
        {
            try
            {
                var yolcu = await _yolcuRepository.GetByIdAsync(id);

                if (yolcu == null)
                {
                    return NotFound($"ID {id} olan yolcu bulunamadı.");
                }
                return Ok(yolcu);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Yolcu>> CreateYolcu(Yolcu yolcu)
        {
            try
            {
                if (yolcu == null)
                {
                    return BadRequest("Yolcu bilgisi boş olamaz.");
                }

                var createdYolcu = await _yolcuRepository.AddAsync(yolcu);
                
                return CreatedAtAction(
                    nameof(GetYolcu),
                    new { id = createdYolcu.YolcuID },
                    createdYolcu
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Yolcu>> UpdateYolcu(int id, Yolcu yolcu)
        {
            try
            {
                if (id != yolcu.YolcuID)
                {
                    return BadRequest("ID uyumsuzluğu.");
                }

                var existingYolcu = await _yolcuRepository.GetByIdAsync(id);
                if (existingYolcu == null)
                {
                    return NotFound($"ID {id} olan yolcu bulunamadı.");
                }

                var updatedYolcu = await _yolcuRepository.UpdateAsync(yolcu);
                return Ok(updatedYolcu);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteYolcu(int id)
        {
            try
            {
                var result = await _yolcuRepository.DeleteAsync(id);
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
