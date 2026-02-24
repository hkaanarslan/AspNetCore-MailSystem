using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Project2EmailNight.Dtos;
using MailKit.Net.Smtp;

namespace Project2EmailNight.Controllers
{
    public class EmailController : Controller
    {
     
        [HttpGet]
        public IActionResult SendEmail()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SendEmail(MailRequestDto mailRequestDto)
        {
            MimeMessage mimeMessage = new MimeMessage();

            MailboxAddress mailboxAddressFrom =
                new MailboxAddress("Identity Admin", "ulkelistesi@gmail.com");
            mimeMessage.From.Add(mailboxAddressFrom);

            MailboxAddress mailboxAddressTo =
                new MailboxAddress("User", mailRequestDto.ReceiverEmail);
            mimeMessage.To.Add(mailboxAddressTo);

            mimeMessage.Subject = mailRequestDto.Subject;

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = mailRequestDto.MessageDetail;
            mimeMessage.Body = bodyBuilder.ToMessageBody();

            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Connect("smtp.gmail.com", 587, false);
            smtpClient.Authenticate("ulkelistesi@gmail.com", "odqh pqxo najq rkyn");
            smtpClient.Send(mimeMessage);
            smtpClient.Disconnect(true);

            
            return RedirectToAction("Inbox", "Message");
        }
    }
}
