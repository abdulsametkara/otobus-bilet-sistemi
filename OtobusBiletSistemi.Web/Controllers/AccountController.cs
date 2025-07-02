using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OtobusBiletSistemi.Core.Data;
using OtobusBiletSistemi.Web.Models;

namespace OtobusBiletSistemi.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<YolcuUser> _userManager;
        private readonly SignInManager<YolcuUser> _signInManager;

        public AccountController(UserManager<YolcuUser> userManager, SignInManager<YolcuUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
                    UserName = model.UserName,
                    Email = model.Email,
                    TCNo = model.TCNo,
                    Ad = model.Ad,
                    Soyad = model.Soyad,
                    TelNo = model.TelNo,
                    EmailConfirmed = true,
                    CreateDate = DateTime.Now  // CreateDate'i set et
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
    }
} 