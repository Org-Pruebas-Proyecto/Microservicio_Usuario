
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;


namespace Application.Services;

public class SmtpEmailService: ISmtpEmailService
{
    private readonly IConfiguration _configuration;

    public SmtpEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task EnviarCorreoConfirmacion(string email, string nombre, string codigo)
    {
        var host = _configuration.GetValue<string>("EmailSettings:Host");
        var port = _configuration.GetValue<int>("EmailSettings:Port");
        var user = _configuration.GetValue<string>("EmailSettings:Username");
        var password = _configuration.GetValue<string>("EmailSettings:Password");


        var cliente = new SmtpClient(host, port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(user, password)
        };

        var titulo = "Confirmación de cuenta";
        var cuerpo = $"Hola {nombre}, Tu código de confirmación es: {codigo}";
        var mensaje = new MailMessage(user, email, titulo, cuerpo);

        await cliente.SendMailAsync(mensaje);
    }
}