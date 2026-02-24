using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project2EmailNight.Dtos;
using Project2EmailNight.Entities;
using MimeKit;
using MailKit.Net.Smtp;

namespace Project2EmailNight.Controllers
{
    public class RegisterController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public RegisterController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        /* ================= KULLANICI OLUŞTUR ================= */

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(UserRegisterDto userRegisterDto)
        {
            if (!ModelState.IsValid)
                return View(userRegisterDto);

            string confirmCode = new Random().Next(100000, 999999).ToString();

            AppUser appUser = new AppUser()
            {
                Name = userRegisterDto.Name,
                Surname = userRegisterDto.Surname,
                UserName = userRegisterDto.Username,
                Email = userRegisterDto.Email,
                ConfirmCode = confirmCode
            };

            var result = await _userManager.CreateAsync(appUser, userRegisterDto.Password);

            if (result.Succeeded)
            {
         

                MimeMessage mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress("Mail@", "ulkelistesi@gmail.com"));
                mimeMessage.To.Add(new MailboxAddress("User", appUser.Email));
                mimeMessage.Subject = "Email Doğrulama Kodunuz";

                var bodyBuilder = new BodyBuilder
                {
                    TextBody = $"Doğrulama Kodunuz: {confirmCode}"
                };

                mimeMessage.Body = bodyBuilder.ToMessageBody();

                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Connect("smtp.gmail.com", 587, false);

                    smtpClient.Authenticate(
                        "ulkelistesi@gmail.com",
                        "BURASI_SENIN_APP_PASSWORD"
                    );

                    smtpClient.Send(mimeMessage);
                    smtpClient.Disconnect(true);
                }

                // =================================================

                return RedirectToAction("ConfirmEmail", "Register", new { email = appUser.Email });
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(userRegisterDto);
        }

        /* ================= EMAIL DOĞRULAMA ================= */

        [HttpGet]
        public IActionResult ConfirmEmail(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmEmail(string email, string confirmCode)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                ModelState.AddModelError("", "Kullanıcı bulunamadı.");
                return View();
            }

            if (user.ConfirmCode == confirmCode)
            {
                user.EmailConfirmed = true;
                user.ConfirmCode = null;
                await _userManager.UpdateAsync(user);

                return RedirectToAction("UserLogin", "Login");
            }

            ModelState.AddModelError("", "Doğrulama kodu hatalı.");
            ViewBag.Email = email;

            return View();
        }
    }
}