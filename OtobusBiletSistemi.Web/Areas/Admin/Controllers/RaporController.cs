using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtobusBiletSistemi.Core.Entities;
using OtobusBiletSistemi.Core.Interfaces;

namespace OtobusBiletSistemi.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RaporController : Controller
    {
        private readonly IRepository<Bilet> _biletRepository;
        private readonly IRepository<Sefer> _seferRepository;
        private readonly IRepository<Odeme> _odemeRepository;

        public RaporController(
            IRepository<Bilet> biletRepository,
            IRepository<Sefer> seferRepository,
            IRepository<Odeme> odemeRepository)
        {
            _biletRepository = biletRepository;
            _seferRepository = seferRepository;
            _odemeRepository = odemeRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> SatisRaporu()
        {
            var biletler = await _biletRepository.GetAllAsync();
            // Satış raporu logic'i buraya eklenecek
            return View(biletler);
        }

        public async Task<IActionResult> GelirRaporu()
        {
            var odemeler = await _odemeRepository.GetAllAsync();
            // Gelir raporu logic'i buraya eklenecek
            return View(odemeler);
        }

        public async Task<IActionResult> SeferRaporu()
        {
            var seferler = await _seferRepository.GetAllAsync();
            // Sefer raporu logic'i buraya eklenecek
            return View(seferler);
        }
    }
} 
 