using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OtobusBiletSistemi.Core.Data;
using OtobusBiletSistemi.Core.Interfaces;
using OtobusBiletSistemi.Core.Entities;
using OtobusBiletSistemi.Web.Models;
using OtobusBiletSistemi.Web.Services;
using System.Text.Encodings.Web;

namespace OtobusBiletSistemi.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<YolcuUser> _userManager;
        private readonly SignInManager<YolcuUser> _signInManager;
        private readonly IRepository<Bilet> _biletRepository;
        private readonly IRepository<Sefer> _seferRepository;
        private readonly IEmailService _emailService;

        public AccountController(
            UserManager<YolcuUser> userManager, 
            SignInManager<YolcuUser> signInManager,
            IRepository<Bilet> biletRepository,
            IRepository<Sefer> seferRepository,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _biletRepository = biletRepository;
            _seferRepository = seferRepository;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new YolcuUser
                {
                    UserName = model.Email, // Email adresini kullanıcı adı olarak kullan
                    Email = model.Email,
                    TCNo = model.TCNo,
                    Ad = model.Ad,
                    Soyad = model.Soyad,
                    TelNo = model.TelNo,
                    EmailConfirmed = true,
                    CreateDate = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    TempData["SuccessMessage"] = "Kayıt işlemi başarılı! Hoşgeldiniz.";
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Email veya Username ile login yapabilsin
                var user = await _userManager.FindByEmailAsync(model.Email) 
                          ?? await _userManager.FindByNameAsync(model.Email);
                          
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(
                        user,
                        model.Password,
                        model.RememberMe,
                        lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        // Admin kullanıcısıysa admin paneline yönlendir
                        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                        if (isAdmin)
                        {
                            TempData["SuccessMessage"] = "Admin girişi başarılı!";
                            return RedirectToAction("Index", "Home", new { area = "Admin" });
                        }
                        
                        TempData["SuccessMessage"] = "Giriş başarılı! Hoşgeldiniz.";
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError("", "Email/Kullanıcı adı veya şifre hatalı!");
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["SuccessMessage"] = "Başarıyla çıkış yaptınız.";
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(YolcuUser model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Güncellenebilir alanları güncelle
            user.Ad = model.Ad;
            user.Soyad = model.Soyad;
            user.Email = model.Email;
            user.TelNo = model.TelNo;
            user.NormalizedEmail = model.Email?.ToUpper();

            var result = await _userManager.UpdateAsync(user);
            
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profil bilgileriniz başarıyla güncellendi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Profil güncellenirken bir hata oluştu: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("Profile");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            if (NewPassword != ConfirmPassword)
            {
                TempData["ErrorMessage"] = "Yeni şifreler uyuşmuyor.";
                return RedirectToAction("Profile");
            }

            var result = await _userManager.ChangePasswordAsync(user, CurrentPassword, NewPassword);
            
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirildi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Şifre değiştirirken bir hata oluştu. Mevcut şifrenizi kontrol edin.";
            }

            return RedirectToAction("Profile");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserStats()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "Kullanıcı bulunamadı" });
                }

                // Kullanıcının tüm biletlerini getir
                var tumBiletler = await _biletRepository.GetAllAsync();
                var kullaniciBiletleri = tumBiletler.Where(b => b.YolcuID == user.Id).ToList();

                // Toplam bilet sayısı
                var totalTickets = kullaniciBiletleri.Count;

                // Aktif bilet sayısı (iptal edilmemiş)
                var activeTickets = kullaniciBiletleri.Count(b => b.BiletDurumu != "İptal");

                // Toplam harcama hesapla
                decimal totalSpent = 0;
                foreach (var bilet in kullaniciBiletleri)
                {
                    if (bilet.BiletDurumu != "İptal") // İptal edilmiş biletleri dahil etme
                    {
                        var sefer = await _seferRepository.GetByIdAsync(bilet.SeferID);
                        if (sefer != null)
                        {
                            totalSpent += sefer.Fiyat;
                        }
                    }
                }

                var stats = new
                {
                    success = true,
                    totalTickets = totalTickets,
                    activeTickets = activeTickets,
                    totalSpent = (double)totalSpent
                };

                return Json(stats);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "İstatistik yüklenirken hata oluştu: " + ex.Message });
            }
        }

        #region Şifre Sıfırlama

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Güvenlik açısından kullanıcı bulunamadığında da success göster
                return RedirectToAction("ForgotPasswordConfirmation");
            }

            try
            {
                // Password reset token oluştur
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                
                // Reset link oluştur
                var resetLink = Url.Action("ResetPassword", "Account", 
                    new { token = token, email = model.Email }, Request.Scheme);

                // Email gönder
                await _emailService.SendPasswordResetEmailAsync(model.Email, resetLink);

                return RedirectToAction("ForgotPasswordConfirmation");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Email gönderilirken bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Geçersiz şifre sıfırlama linki.";
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("Login");
            }

            try
            {
                var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
                
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirildi. Yeni şifrenizle giriş yapabilirsiniz.";
                    return RedirectToAction("Login");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Şifre sıfırlama sırasında bir hata oluştu. Link süresi dolmuş olabilir.";
            }

            return View(model);
        }

        #endregion
    }
} 