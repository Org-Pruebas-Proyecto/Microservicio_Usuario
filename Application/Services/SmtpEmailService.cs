
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
        var cliente = CrearClienteSmtp();
        var titulo = "Confirmación de cuenta";
        var cuerpo = $"Hola {nombre}, Tu código de confirmación es: {codigo}";
        var mensaje = CrearMensaje(cliente.Credentials.GetCredential(cliente.Host, cliente.Port, "").UserName, email, titulo, cuerpo);

        await cliente.SendMailAsync(mensaje);
    }

    public async Task EnviarNotificacionCambioPassword(string email, string nombre)
    {
        var cliente = CrearClienteSmtp();
        var titulo = "Notificación de cambio de contraseña";
        var cuerpo = $"Hola {nombre}, Tu contraseña ha sido cambiada exitosamente.";
        var mensaje = CrearMensaje(cliente.Credentials.GetCredential(cliente.Host, cliente.Port, "").UserName, email, titulo, cuerpo);

        await cliente.SendMailAsync(mensaje);
    }

    public async Task EnviarEnlaceRecuperacion(string email, string nombre, string token)
    {
        var cliente = CrearClienteSmtp();
        var titulo = "Recuperación de contraseña";
        //CAMBIAR URL por el link de la aplicación
        var cuerpo = $"Hola {nombre}, Para recuperar tu contraseña, haz clic en el siguiente enlace: " +
                     $"https://tuaplicacion.com/recuperar?token={token}";
        var mensaje = CrearMensaje(cliente.Credentials.GetCredential(cliente.Host, cliente.Port, "").UserName, email, titulo, cuerpo);
        await cliente.SendMailAsync(mensaje);
    }

    private SmtpClient CrearClienteSmtp()
    {
        var host = _configuration.GetValue<string>("EmailSettings:Host");
        var port = _configuration.GetValue<int>("EmailSettings:Port");
        var user = _configuration.GetValue<string>("EmailSettings:Username");
        var password = _configuration.GetValue<string>("EmailSettings:Password");

        return new SmtpClient(host, port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(user, password)
        };
    }

    private MailMessage CrearMensaje(string from, string to, string subject, string body)
    {
        return new MailMessage(from, to, subject, body);
    }
}