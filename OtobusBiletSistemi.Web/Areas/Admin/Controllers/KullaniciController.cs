using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OtobusBiletSistemi.Core.Data;
using OtobusBiletSistemi.Web.Models;
using System.Collections.Generic;
using System.Linq;

namespace OtobusBiletSistemi.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class KullaniciController : Controller
    {
        private readonly UserManager<YolcuUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public KullaniciController(UserManager<YolcuUser> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string searchTerm = "")
        {
            var users = _userManager.Users.ToList();
            
            if (!string.IsNullOrEmpty(searchTerm))
            {
                users = users.Where(u => 
                    (u.Ad != null && u.Ad.ToLower().Contains(searchTerm.ToLower())) ||
                    (u.Soyad != null && u.Soyad.ToLower().Contains(searchTerm.ToLower())) ||
                    (u.Email != null && u.Email.ToLower().Contains(searchTerm.ToLower())) ||
                    (u.TCNo != null && u.TCNo.Contains(searchTerm)) ||
                    (u.TelNo != null && u.TelNo.Contains(searchTerm)) ||
                    (u.Ad != null && u.Soyad != null && (u.Ad + " " + u.Soyad).ToLower().Contains(searchTerm.ToLower()))
                ).ToList();
            }
            
            var usersWithRoles = new List<UserWithRolesViewModel>();
            
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                usersWithRoles.Add(new UserWithRolesViewModel
                {
                    User = user,
                    IsAdmin = roles.Contains("Admin")
                });
            }
            
            ViewBag.SearchTerm = searchTerm;
            
            return View(usersWithRoles);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(YolcuUser model, string password)
        {
            try
            {
                var tcNo = Request.Form["TCNo"].ToString();
                var ad = Request.Form["Ad"].ToString();
                var soyad = Request.Form["Soyad"].ToString();
                var email = Request.Form["Email"].ToString();
                var telNo = Request.Form["TelNo"].ToString();
                var sifre = Request.Form["password"].ToString();

                if (string.IsNullOrEmpty(tcNo) || string.IsNullOrEmpty(ad) || 
                    string.IsNullOrEmpty(soyad) || string.IsNullOrEmpty(email) || 
                    string.IsNullOrEmpty(telNo) || string.IsNullOrEmpty(sifre))
                {
                    ModelState.AddModelError("", "Tüm alanlar zorunludur.");
                    return View(model);
                }

                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Bu email adresi zaten kullanılıyor.");
                    return View(model);
                }

                var user = new YolcuUser
                {
                    TCNo = tcNo,
                    Ad = ad,
                    Soyad = soyad,
                    Email = email,
                    TelNo = telNo,
                    UserName = email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, sifre);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Kullanıcı başarıyla oluşturuldu.";
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", $"Identity Hatası: {error.Description}");
                    }
                }
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                ModelState.AddModelError("", $"Veritabanı Hatası: {innerException}");
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError("", $"Genel Hata: {innerMsg}");
            }

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, YolcuUser model)
        {
            try
            {
                var tcNo = Request.Form["TCNo"].ToString();
                var ad = Request.Form["Ad"].ToString();
                var soyad = Request.Form["Soyad"].ToString();
                var email = Request.Form["Email"].ToString();
                var telNo = Request.Form["TelNo"].ToString();

                if (string.IsNullOrEmpty(tcNo) || string.IsNullOrEmpty(ad) || 
                    string.IsNullOrEmpty(soyad) || string.IsNullOrEmpty(email) || 
                    string.IsNullOrEmpty(telNo))
                {
                    ModelState.AddModelError("", "Tüm alanlar zorunludur.");
                    var userForError = await _userManager.FindByIdAsync(id.ToString());
                    return View(userForError ?? model);
                }

                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("Index");
                }

                user.TCNo = tcNo;
                user.Ad = ad;
                user.Soyad = soyad;
                user.Email = email;
                user.TelNo = telNo;
                user.UserName = email;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Kullanıcı bilgileri başarıyla güncellendi.";
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", $"Identity Hatası: {error.Description}");
                    }
                }
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                ModelState.AddModelError("", $"Veritabanı Hatası: {innerException}");
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError("", $"Genel Hata: {innerMsg}");
            }

            var userForView = await _userManager.FindByIdAsync(id.ToString());
            return View(userForView ?? model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    TempData["Success"] = "Kullanıcı başarıyla silindi.";
                }
                else
                {
                    TempData["Error"] = "Kullanıcı silinirken bir hata oluştu.";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminYap(int id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("Index");
                }

                var adminRole = await _roleManager.FindByNameAsync("Admin");
                if (adminRole == null)
                {
                    await _roleManager.CreateAsync(new IdentityRole<int>("Admin"));
                }

                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    TempData["InfoMessage"] = "Kullanıcı zaten admin yetkisine sahip.";
                }
                else
                {
                    var result = await _userManager.AddToRoleAsync(user, "Admin");
                    if (result.Succeeded)
                    {
                        TempData["SuccessMessage"] = $"{user.FullName} kullanıcısına admin yetkisi verildi.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Admin yetkisi verilirken bir hata oluştu.";
                    }
                }

                return RedirectToAction("Details", new { id = id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Hata: " + ex.Message;
                return RedirectToAction("Details", new { id = id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminKaldir(int id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("Index");
                }

                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    var result = await _userManager.RemoveFromRoleAsync(user, "Admin");
                    if (result.Succeeded)
                    {
                        TempData["SuccessMessage"] = $"{user.FullName} kullanıcısının admin yetkisi kaldırıldı.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Admin yetkisi kaldırılırken bir hata oluştu.";
                    }
                }
                else
                {
                    TempData["InfoMessage"] = "Kullanıcı zaten admin yetkisine sahip değil.";
                }

                return RedirectToAction("Details", new { id = id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Hata: " + ex.Message;
                return RedirectToAction("Details", new { id = id });
            }
        }
    }
}
 