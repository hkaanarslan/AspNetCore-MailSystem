using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project2EmailNight.Dtos;
using Project2EmailNight.Entities;

namespace Project2EmailNight.Controllers
{
    public class LoginController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public LoginController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // GİRİŞ SAYFASI
        [HttpGet]
        public IActionResult UserLogin()
        {
            return View();
        }

        // GİRİŞ İŞLEMİ
        [HttpPost]
        public async Task<IActionResult> UserLogin(UserLoginDto userLoginDto)
        {
            if (!ModelState.IsValid)
                return View(userLoginDto);

            // Kullanıcıyı bul
            var user = await _userManager.FindByNameAsync(userLoginDto.Username);

            if (user == null)
            {
                ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
                return View(userLoginDto);
            }

            // EMAIL DOĞRULAMA KONTROLÜ 
            if (!user.EmailConfirmed)
            {
                ModelState.AddModelError("", "Lütfen önce email adresinizi doğrulayın.");
                return View(userLoginDto);
            }

            // Şifre kontrolü
            var result = await _signInManager.PasswordSignInAsync(
                userLoginDto.Username,
                userLoginDto.Password,
                true,
                false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Profile");
            }

            ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
            return View(userLoginDto);
        }

        // ÇIKIŞ
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("UserLogin", "Login");
        }
    }
}
