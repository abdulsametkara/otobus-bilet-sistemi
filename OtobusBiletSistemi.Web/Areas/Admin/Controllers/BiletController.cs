using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtobusBiletSistemi.Core.Entities;
using OtobusBiletSistemi.Core.Interfaces;
using OtobusBiletSistemi.Core.Data;

namespace OtobusBiletSistemi.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BiletController : Controller
    {
        private readonly IRepository<Bilet> _biletRepository;
        private readonly AppDbContext _context;

        public BiletController(IRepository<Bilet> biletRepository, AppDbContext context)
        {
            _biletRepository = biletRepository;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var biletler = await _biletRepository.GetAllAsync();
                return View(biletler);
            }
            catch (Exception ex)
            {
                
                TempData["Error"] = "Veritabanından biletler alınamadı.";
                return NotFound();
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var bilet = await _context.Biletler
                .Include(b => b.Yolcu)
                .Include(b => b.Sefer)
                    .ThenInclude(s => s.Guzergah)
                .Include(b => b.Sefer)
                    .ThenInclude(s => s.Otobus)
                .Include(b => b.Koltuk)
                .FirstOrDefaultAsync(b => b.BiletID == id);
            
            if (bilet == null)
            {
                return NotFound();
            }
            return View(bilet);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var bilet = await _biletRepository.GetByIdAsync(id);
            if (bilet != null)
            {
                await _biletRepository.DeleteAsync(bilet);
                TempData["Success"] = "Bilet başarıyla iptal edildi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
} 
 