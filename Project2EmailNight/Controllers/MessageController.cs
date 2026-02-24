using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project2EmailNight.Context;
using Project2EmailNight.Entities;

namespace Project2EmailNight.Controllers
{
    public class MessageController : Controller
    {
        private readonly EmailContext _context;
        private readonly UserManager<AppUser> _userManager;

        public MessageController(EmailContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ================= CREATE MESSAGE =================

        [HttpGet]
        public async Task<IActionResult> CreateMessage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("UserLogin", "Login");



            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(Message message, string submitType)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("UserLogin", "Login");

            message.MessageDetail ??= "";
            message.SenderEmail = user.Email;
            message.SendDate = DateTime.Now;
            message.IsRead = false;
            message.IsStarred = false;
            message.IsDeleted = false;
            message.IsStatus = true;
            message.IsDraft = submitType == "draft";

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return message.IsDraft
                ? RedirectToAction("Draft")
                : RedirectToAction("Sendbox");
        }

        // ================= INBOX =================

        public async Task<IActionResult> Inbox(string search)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("UserLogin", "Login");

            var query = _context.Messages
                .Where(x => x.ReceiverEmail == user.Email && !x.IsDeleted);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x =>
                    x.Subject.Contains(search) ||
                    x.SenderEmail.Contains(search));
            }

            var messages = await query
                .OrderByDescending(x => x.SendDate)
                .ToListAsync();

            ViewBag.Categories = await _context.Messages
                .Where(x => x.ReceiverEmail == user.Email &&
                            !x.IsDeleted &&
                            x.Category != null)
                .GroupBy(x => x.Category)
                .Select(g => new
                {
                    Name = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            ViewBag.ActiveCategory = null;

            return View(messages);
        }

        // ================= CATEGORY =================

        public async Task<IActionResult> Category(string name)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("UserLogin", "Login");

            var messages = await _context.Messages
                .Where(x => x.ReceiverEmail == user.Email &&
                            !x.IsDeleted &&
                            x.Category == name)
                .OrderByDescending(x => x.SendDate)
                .ToListAsync();

            ViewBag.Categories = await _context.Messages
                .Where(x => x.ReceiverEmail == user.Email &&
                            !x.IsDeleted &&
                            x.Category != null)
                .GroupBy(x => x.Category)
                .Select(g => new
                {
                    Name = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            ViewBag.ActiveCategory = name;

            return View("Inbox", messages);
        }

        // ================= SENDBOX =================

        public async Task<IActionResult> Sendbox()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("UserLogin", "Login");

            var messages = await _context.Messages
                .Where(x => x.SenderEmail == user.Email &&
                            !x.IsDeleted &&
                            !x.IsDraft)
                .OrderByDescending(x => x.SendDate)
                .ToListAsync();

            return View(messages);
        }

        // ================= STARRED =================

        public async Task<IActionResult> Starred()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("UserLogin", "Login");

            var messages = await _context.Messages
                .Where(x =>
                    x.IsStarred &&
                    !x.IsDeleted &&
                    (x.ReceiverEmail == user.Email ||
                     x.SenderEmail == user.Email))
                .OrderByDescending(x => x.SendDate)
                .ToListAsync();

            return View(messages);
        }

        // ================= DRAFT =================

        public async Task<IActionResult> Draft()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("UserLogin", "Login");

            var messages = await _context.Messages
                .Where(x => x.SenderEmail == user.Email &&
                            x.IsDraft &&
                            !x.IsDeleted)
                .OrderByDescending(x => x.SendDate)
                .ToListAsync();

            return View(messages);
        }

        // ================= EDIT DRAFT =================

        public async Task<IActionResult> EditDraft(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("UserLogin", "Login");

            var message = await _context.Messages
                .FirstOrDefaultAsync(x =>
                    x.MessageId == id &&
                    x.SenderEmail == user.Email &&
                    x.IsDraft);

            if (message == null)
                return NotFound();

            return View(message);
        }

        [HttpPost]
        public async Task<IActionResult> EditDraft(Message message, string submitType)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("UserLogin", "Login");

            var existingMessage = await _context.Messages
                .FirstOrDefaultAsync(x =>
                    x.MessageId == message.MessageId &&
                    x.SenderEmail == user.Email &&
                    x.IsDraft);

            if (existingMessage == null)
                return NotFound();

            existingMessage.ReceiverEmail = message.ReceiverEmail;
            existingMessage.Subject = message.Subject;
            existingMessage.MessageDetail = message.MessageDetail;

            if (submitType == "send")
            {
                existingMessage.IsDraft = false;
                existingMessage.SendDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return submitType == "draft"
                ? RedirectToAction("Draft")
                : RedirectToAction("Sendbox");
        }

        // ================= MESSAGE DETAIL =================

        public async Task<IActionResult> MessageDetail(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("UserLogin", "Login");

            var message = await _context.Messages
                .FirstOrDefaultAsync(x =>
                    x.MessageId == id &&
                    (x.ReceiverEmail == user.Email ||
                     x.SenderEmail == user.Email));

            if (message == null)
                return NotFound();

            if (message.ReceiverEmail == user.Email && !message.IsRead)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return View(message);
        }

        // ================= REPLY =================

        public async Task<IActionResult> Reply(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("UserLogin", "Login");

            var message = await _context.Messages
                .FirstOrDefaultAsync(x =>
                    x.MessageId == id &&
                    (x.ReceiverEmail == user.Email ||
                     x.SenderEmail == user.Email));

            if (message == null)
                return NotFound();

            var replyMessage = new Message
            {
                ReceiverEmail = message.SenderEmail,
                Subject = "Re: " + message.Subject,
                MessageDetail = "<br><br><hr><strong>Önceki Mesaj:</strong><br>" + message.MessageDetail
            };

            return View("CreateMessage", replyMessage);
        }

        // ================= DELETE =================

        public async Task<IActionResult> MessageDelete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("UserLogin", "Login");

            var message = await _context.Messages
                .FirstOrDefaultAsync(x =>
                    x.MessageId == id &&
                    (x.ReceiverEmail == user.Email ||
                     x.SenderEmail == user.Email));

            if (message == null)
                return NotFound();

            message.IsDeleted = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("Inbox");
        }

        // ================= TRASH =================

        public async Task<IActionResult> Trash()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("UserLogin", "Login");

            var messages = await _context.Messages
                .Where(x =>
                    (x.ReceiverEmail == user.Email ||
                     x.SenderEmail == user.Email) &&
                    x.IsDeleted)
                .OrderByDescending(x => x.SendDate)
                .ToListAsync();

            return View(messages);
        }

        // ================= RESTORE =================

        public async Task<IActionResult> Restore(int id)
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

            message.IsDeleted = false;

            await _context.SaveChangesAsync();

            return RedirectToAction("Trash");
        }

        // ================= TOGGLE STAR =================

        [HttpPost]
        public async Task<IActionResult> ToggleStar(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var message = await _context.Messages
                .FirstOrDefaultAsync(x =>
                    x.MessageId == id &&
                    (x.ReceiverEmail == user.Email ||
                     x.SenderEmail == user.Email));

            if (message == null)
                return NotFound();

            message.IsStarred = !message.IsStarred;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // ================= DELETE MULTIPLE =================

        [HttpPost]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            if (ids == null || !ids.Any())
                return BadRequest();

            var messages = await _context.Messages
                .Where(x =>
                    ids.Contains(x.MessageId) &&
                    (x.ReceiverEmail == user.Email ||
                     x.SenderEmail == user.Email))
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsDeleted = true;
            }

            await _context.SaveChangesAsync();

            return Ok();
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

        // ================= EMPTY TRASH =================

        [HttpGet]
        public async Task<IActionResult> EmptyTrash()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("UserLogin", "Login");

            var messages = await _context.Messages
                .Where(x =>
                    (x.ReceiverEmail == user.Email ||
                     x.SenderEmail == user.Email) &&
                    x.IsDeleted)
                .ToListAsync();

            if (!messages.Any())
                return RedirectToAction("Trash");

            _context.Messages.RemoveRange(messages);
            await _context.SaveChangesAsync();

            return RedirectToAction("Trash");
        }

        // ================= UPDATE CATEGORY =================

        [HttpPost]
        public async Task<IActionResult> UpdateCategory(int id, string category)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var message = await _context.Messages
                .FirstOrDefaultAsync(x =>
                    x.MessageId == id &&
                    (x.ReceiverEmail == user.Email ||
                     x.SenderEmail == user.Email));

            if (message == null)
                return NotFound();

            message.Category = category;

            await _context.SaveChangesAsync();

            return RedirectToAction("MessageDetail", new { id });
        }
    }
}