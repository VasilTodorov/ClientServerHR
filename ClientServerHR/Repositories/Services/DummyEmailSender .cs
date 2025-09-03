using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ClientServerHR.Repositories.Services
{
    public class DummyEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Console.WriteLine($"[Fake Email] To: {email}, Subject: {subject}\n{htmlMessage}");
            return Task.CompletedTask;
        }
    }
}
