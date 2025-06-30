using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OtobusBiletSistemi.Web.Models;
using OtobusBiletSistemi.Core.Data;
using System.ComponentModel.DataAnnotations;

namespace OtobusBiletSistemi.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AuthController : Controller
    {
        private readonly SignInManager<YolcuUser> _signInManager;
        private readonly UserManager<YolcuUser> _userManager;

        public AuthController(SignInManager<YolcuUser> signInManager, UserManager<YolcuUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(AdminLoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email) ?? 
                          await _userManager.FindByNameAsync(model.Email);

                if (user != null)
                {
                    var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                    
                    if (isAdmin)
                    {
                        var result = await _signInManager.PasswordSignInAsync(
                            user.UserName, 
                            model.Password, 
                            model.RememberMe, 
                            lockoutOnFailure: true);

                        if (result.Succeeded)
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        else if (result.IsLockedOut)
                        {
                            ModelState.AddModelError("", "Hesabınız kilitlenmiştir. Lütfen daha sonra tekrar deneyin.");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Geçersiz giriş bilgileri.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Bu hesapla admin paneline erişim yetkiniz bulunmamaktadır.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Kullanıcı bulunamadı.");
                }
            }

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }

    public class AdminLoginViewModel
    {
        [Required(ErrorMessage = "E-posta adresi gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre gereklidir")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
} 