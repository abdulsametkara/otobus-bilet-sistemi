using Microsoft.AspNetCore.Mvc;
using OtobusBiletSistemi.Core.Entities;
using OtobusBiletSistemi.Core.Interfaces;

namespace OtobusBiletSistemi.Web.Controllers
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
    }

    [Route("api/[controller]")]
    [ApiController]
    public class SeferApiController : ControllerBase
    {
        private readonly IRepository<Sefer> _seferRepository;

        public SeferApiController(IRepository<Sefer> seferRepository)
        {
            _seferRepository = seferRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<Sefer>>> GetAllSefer()
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
    }

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
        public async Task<ActionResult<List<Otobus>>> GetAllOtobus()
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
                    return NotFound($"ID {id} olan otobüs bulunamadı.");
                }
                return Ok(otobus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class KoltukApiController : ControllerBase
    {
        private readonly IRepository<Koltuk> _koltukRepository;
        private readonly IRepository<Bilet> _biletRepository;

        public KoltukApiController(IRepository<Koltuk> koltukRepository, IRepository<Bilet> biletRepository)
        {
            _koltukRepository = koltukRepository;
            _biletRepository = biletRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<Koltuk>>> GetAllKoltuk()
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

        [HttpGet("otobus/{otobusId}")]
        public async Task<ActionResult<List<Koltuk>>> GetKoltukByOtobus(int otobusId)
        {
            try
            {
                var allKoltuklar = await _koltukRepository.GetAllAsync();
                var otobusKoltuklari = allKoltuklar.Where(k => k.OtobusID == otobusId).ToList();
                return Ok(otobusKoltuklari);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("dolu/{seferId}")]
        public async Task<ActionResult<List<int>>> GetDoluKoltuklar(int seferId)
        {
            try
            {
                var allBiletler = await _biletRepository.GetAllAsync();
                var doluKoltukIdleri = allBiletler
                    .Where(b => b.SeferID == seferId && b.BiletDurumu != "İptal")
                    .Select(b => b.KoltukID)
                    .ToList();
                return Ok(doluKoltukIdleri);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
} 