using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OtobusBiletSistemi.Core.Entities;
using OtobusBiletSistemi.Core.Interfaces;
using System.Runtime.InteropServices;

namespace OtobusBiletSistemi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OdemeController : ControllerBase
    {
        private readonly IRepository<Odeme> _odemeRepository;
        public OdemeController(IRepository<Odeme> odemeRepository)
        {
            _odemeRepository = odemeRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<Odeme>>> GetAllOdemeler()
        {
            try
            {
                var odemeler = await _odemeRepository.GetAllAsync();
                return Ok(odemeler);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Odeme>> GetOdeme(int id)
        {
            try
            {
                var odeme = await _odemeRepository.GetByIdAsync(id);
                if (odeme == null)
                {
                    return NotFound($"ID {id} olan odeme bulunamadı.");
                }
                return Ok(odeme);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Odeme>> CreateOdeme(Odeme odeme)
        {
            try
            {
                if (odeme == null)
                {
                    return BadRequest("Odeme bilgisi boş olamaz.");
                }
                var createdOdeme = await _odemeRepository.AddAsync(odeme);
                return CreatedAtAction(
                    nameof(GetOdeme),
                    new {id = createdOdeme.OdemeID},
                    createdOdeme
                    );
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Odeme>> UpdateOdeme(int id, Odeme odeme)
        {
            try
            {
                if (id != odeme.OdemeID)
                {
                    return BadRequest("ID uyumsuzluğu.");
                }

                var existingOdeme = await _odemeRepository.GetByIdAsync(id);

                if (existingOdeme == null)
                {
                    return NotFound($"ID {id} olan yolcu bulunamadı.");
                }

                var updatedOdeme = await _odemeRepository.UpdateAsync(odeme);
                return Ok(odeme);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOdeme(int id)
        {
            try
            {
                var result = await _odemeRepository.DeleteAsync(id);
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
