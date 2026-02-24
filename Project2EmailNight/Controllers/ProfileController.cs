using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project2EmailNight.Context;
using Project2EmailNight.Dtos;
using Project2EmailNight.Entities;
using Project2EmailNight.ViewModels;

namespace Project2EmailNight.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly EmailContext _context;

        public ProfileController(UserManager<AppUser> userManager, EmailContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        /* =====================================================
           1️⃣ PROFİL SAYFASI
        ===================================================== */
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("UserLogin", "Login");

            var inbox = _context.Messages.Where(x => x.ReceiverEmail == user.Email);
            var sent = _context.Messages.Where(x => x.SenderEmail == user.Email);

            var model = new ProfileViewModel
            {
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                ImageUrl = user.ImageUrl,
                About = user.About,
                Phone = user.PhoneNumber,
                BirthDate = user.BirthDate,
                City = user.City,

                // =================  İSTATİSTİK =================

                InboxCount = await inbox
                    .CountAsync(x => !x.IsDeleted),

                SendCount = await sent
                    .CountAsync(x => !x.IsDeleted && !x.IsDraft),

                StarredCount = await _context.Messages
                    .CountAsync(x =>
                        x.IsStarred &&
                        !x.IsDeleted &&
                        (x.ReceiverEmail == user.Email ||
                         x.SenderEmail == user.Email)),

                UnreadCount = await inbox
                    .CountAsync(x => !x.IsRead && !x.IsDeleted),

                DeletedCount = await _context.Messages
                    .CountAsync(x =>
                        x.IsDeleted &&
                        (x.ReceiverEmail == user.Email ||
                         x.SenderEmail == user.Email)),

                Last7DaysCount = await inbox
                    .CountAsync(x =>
                        !x.IsDeleted &&
                        x.SendDate >= DateTime.Now.AddDays(-7)),

                DraftCount = await _context.Messages
                    .CountAsync(x =>
                        x.IsDraft &&
                        !x.IsDeleted &&
                        x.SenderEmail == user.Email),

                TotalMessageCount = await _context.Messages
                    .CountAsync(x =>
                        !x.IsDeleted &&
                        (x.ReceiverEmail == user.Email
                      || x.SenderEmail == user.Email)),

                LastMessages = await inbox
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.SendDate)
                    .Take(3)
                    .ToListAsync()
            };

            // ================= ETİKET İSTATİSTİĞİ =================

            var categoryStats = await _context.Messages
                .Where(x =>
                    x.ReceiverEmail == user.Email &&
                    !x.IsDeleted &&
                    x.Category != null)
                .GroupBy(x => x.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            ViewBag.CategoryLabels = categoryStats.Select(x => x.Category).ToList();
            ViewBag.CategoryCounts = categoryStats.Select(x => x.Count).ToList();

            return View(model);
        }

        /* =====================================================
           PROFİL DÜZENLE
        ===================================================== */
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("UserLogin", "Login");

            var dto = new UserEditDto
            {
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                ImageUrl = user.ImageUrl,
                Phone = user.PhoneNumber,
                About = user.About,
                BirthDate = user.BirthDate,
                City = user.City
            };

            return View(dto);
        }

        /* =====================================================
           PROFİL DÜZENLE
        ===================================================== */
        [HttpPost]
        public async Task<IActionResult> Edit(UserEditDto dto, string? OldPassword, string? ConfirmPassword)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("UserLogin", "Login");

            user.Name = dto.Name;
            user.Surname = dto.Surname;
            user.Email = dto.Email;
            user.UserName = dto.Email;
            user.PhoneNumber = dto.Phone;
            user.About = dto.About;
            user.BirthDate = dto.BirthDate;
            user.City = dto.City;

            if (dto.Image != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(dto.Image.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(),
                                        "wwwroot/images",
                                        fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }

                user.ImageUrl = "/images/" + fileName;
            }

            // ================= ŞİFRE DEĞİŞTİRME =================

            if (!string.IsNullOrEmpty(dto.Password))
            {
                if (string.IsNullOrEmpty(OldPassword))
                {
                    ModelState.AddModelError("Password", "Mevcut şifre gerekli.");
                    return View(dto);
                }

                if (dto.Password != ConfirmPassword)
                {
                    ModelState.AddModelError("Password", "Yeni şifreler uyuşmuyor.");
                    return View(dto);
                }

                var result = await _userManager.ChangePasswordAsync(user, OldPassword, dto.Password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("Password", error.Description);
                    }

                    return View(dto);
                }
            }

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(dto);
            }

            return RedirectToAction("Index");
        }

        // ================= HARD DELETE =================

        public async Task<IActionResult> HardDelete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("UserLogin", "Login");

            var message = await _context.Messages
                .FirstOrDefaultAsync(x =>
                    x.MessageId == id &&
                    (x.ReceiverEmail == user.Email ||
                     x.SenderEmail == user.Email) &&
                    x.IsDeleted);

            if (message == null)
                return NotFound();

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return RedirectToAction("Trash");
        }
    }
}