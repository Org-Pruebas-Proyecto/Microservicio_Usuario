using Application.Interfaces;
using System.Net.Mail;
using System.Net;

namespace Application.Services;

public class SmtpEmailService : ISmtpEmailService
{
    private readonly ISmtpClientFactory _smtpClientFactory;

    public SmtpEmailService(ISmtpClientFactory smtpClientFactory)
    {
        _smtpClientFactory = smtpClientFactory;
    }

    public async Task EnviarCorreoConfirmacion(string email, string nombre, string codigo)
    {
        using var cliente = _smtpClientFactory.Create();
        var titulo = "Confirmación de cuenta";
        var cuerpo = $"Hola {nombre}, Tu código de confirmación es: {codigo}";
        var mensaje = CrearMensaje(((NetworkCredential)cliente.Credentials).UserName, email, titulo, cuerpo);
        await cliente.SendMailAsync(mensaje);
    }

    public async Task EnviarNotificacionCambioPassword(string email, string nombre)
    {
        using var cliente = _smtpClientFactory.Create();
        var titulo = "Notificación de cambio de contraseña";
        var cuerpo = $"Hola {nombre}, Tu contraseña ha sido cambiada exitosamente.";
        var mensaje = CrearMensaje(((NetworkCredential)cliente.Credentials).UserName, email, titulo, cuerpo);
        await cliente.SendMailAsync(mensaje);
    }

    public async Task EnviarEnlaceRecuperacion(string email, string nombre, string token)
    {
        using var cliente = _smtpClientFactory.Create();
        var titulo = "Recuperación de contraseña";
        var cuerpo = $"Hola {nombre}, Para recuperar tu contraseña, haz clic en el siguiente enlace: http://localhost:5173/recuperar?token={token}";
        var mensaje = CrearMensaje(((NetworkCredential)cliente.Credentials).UserName, email, titulo, cuerpo);
        await cliente.SendMailAsync(mensaje);
    }

    private MailMessage CrearMensaje(string from, string to, string subject, string body)
    {
        return new MailMessage(from, to, subject, body);
    }
}