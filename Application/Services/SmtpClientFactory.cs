using Application.Interfaces;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Application.Services;

public class SmtpClientFactory : ISmtpClientFactory
{
    private readonly EmailSettings _settings;

    public SmtpClientFactory(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public ISmtpClient Create()
    {
        var smtpClient = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password)
        };

        return new SmtpClientAdapter(smtpClient);
    }
}