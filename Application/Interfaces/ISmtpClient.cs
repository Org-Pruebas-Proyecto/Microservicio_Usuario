using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface ISmtpClient : IDisposable
{
    Task SendMailAsync(MailMessage message);
    ICredentialsByHost Credentials { get; set; }
}