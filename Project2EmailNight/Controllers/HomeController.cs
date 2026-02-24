using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project2EmailNight.Context;
using Project2EmailNight.Entities;

namespace Project2EmailNight.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly EmailContext _context;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(EmailContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // ================= KULLANICI =================

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("UserLogin", "Login");

            var userEmail = user.Email;

            // ================= MAIL SAYILARI =================

            var inboxCount = await _context.Messages
                .CountAsync(x => x.ReceiverEmail == userEmail
                              && !x.IsDeleted
                              && !x.IsDraft);

            var sendCount = await _context.Messages
                .CountAsync(x => x.SenderEmail == userEmail
                              && !x.IsDeleted
                              && !x.IsDraft);

            var draftCount = await _context.Messages
                .CountAsync(x => x.SenderEmail == userEmail
                              && x.IsDraft
                              && !x.IsDeleted);

            var starCount = await _context.Messages
                .CountAsync(x =>
                    (x.ReceiverEmail == userEmail || x.SenderEmail == userEmail)
                    && x.IsStarred
                    && !x.IsDeleted);

            var trashCount = await _context.Messages
                .CountAsync(x =>
                    (x.ReceiverEmail == userEmail || x.SenderEmail == userEmail)
                    && x.IsDeleted);

            var totalCount = await _context.Messages
                .CountAsync(x =>
                    (x.ReceiverEmail == userEmail || x.SenderEmail == userEmail)
                    && !x.IsDeleted);

            var unreadCount = await _context.Messages
                .CountAsync(x =>
                    x.ReceiverEmail == userEmail
                    && !x.IsDeleted
                    && !x.IsRead);

            ViewBag.InboxCount = inboxCount;
            ViewBag.SendCount = sendCount;
            ViewBag.DraftCount = draftCount;
            ViewBag.StarCount = starCount;
            ViewBag.TrashCount = trashCount;
            ViewBag.TotalCount = totalCount;
            ViewBag.UnreadCount = unreadCount;

            var todayInboxCount = await _context.Messages
    .CountAsync(x =>
        x.ReceiverEmail == userEmail &&
        !x.IsDeleted &&
        !x.IsDraft &&
        x.SendDate.Date == DateTime.Today);

            ViewBag.TodayInboxCount = todayInboxCount;

            // ================= OKUNMAMIÞ SON 5 MESAJ =================

            var unreadMessages = await _context.Messages
                .Where(x =>
                    x.ReceiverEmail == userEmail &&
                    !x.IsDeleted &&
                    !x.IsRead)
                .OrderByDescending(x => x.SendDate)
                .Take(5)
                .ToListAsync();

            // ================= SON 7 GÜN GRAFÝÐÝ =================

            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-i))
                .OrderBy(d => d)
                .ToList();

            var dailyCounts = new List<int>();

            foreach (var day in last7Days)
            {
                var count = await _context.Messages
                    .CountAsync(x =>
                        x.ReceiverEmail == userEmail &&
                        !x.IsDeleted &&
                        !x.IsDraft &&
                        x.SendDate.Date == day.Date);

                dailyCounts.Add(count);
            }

            ViewBag.Last7DaysLabels = last7Days
                .Select(d => d.ToString("dd MMM"))
                .ToList();

            ViewBag.Last7DaysCounts = dailyCounts;
            ViewBag.Last7DaysTotal = dailyCounts.Sum();

            // ================= VIEW =================

            return View(unreadMessages);
        }


    }
}