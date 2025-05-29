using Application.Interfaces;
using System.Net;
using System.Net.Mail;

namespace Application.Services;

public class SmtpClientAdapter : ISmtpClient
{
    private readonly SmtpClient _smtpClient;

    public SmtpClientAdapter(SmtpClient smtpClient)
    {
        _smtpClient = smtpClient;
    }

    public ICredentialsByHost Credentials
    {
        get => _smtpClient.Credentials;
        set => _smtpClient.Credentials = value;
    }

    public Task SendMailAsync(MailMessage message)
    {
        return _smtpClient.SendMailAsync(message);
    }

    public void Dispose()
    {
        _smtpClient.Dispose();
    }
}