using Xunit;
using Moq;
using Application.Services;
using Application.Interfaces;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;

namespace Usuarios.Tests.Application.Tests.Services;

public class SmtpEmailServiceTests
{
    private readonly Mock<ISmtpClientFactory> _factoryMock;
    private readonly Mock<ISmtpClient> _smtpClientMock;
    private readonly SmtpEmailService _service;

    public SmtpEmailServiceTests()
    {
        _factoryMock = new Mock<ISmtpClientFactory>();
        _smtpClientMock = new Mock<ISmtpClient>();
        _smtpClientMock.SetupProperty(c => c.Credentials, new NetworkCredential("from@example.com", "password"));
        _factoryMock.Setup(f => f.Create()).Returns(_smtpClientMock.Object);
        _service = new SmtpEmailService(_factoryMock.Object);
    }

    [Fact]
    public async Task EnviarCorreoConfirmacion_DeberiaEnviarCorreo()
    {
        await _service.EnviarCorreoConfirmacion("to@example.com", "Carlos", "ABC123");

        _smtpClientMock.Verify(c => c.SendMailAsync(It.Is<MailMessage>(m =>
            m.To[0].Address == "to@example.com" &&
            m.Subject.Contains("Confirmación") &&
            m.Body.Contains("ABC123")
        )), Times.Once);
    }

    [Fact]
    public async Task EnviarNotificacionCambioPassword_DeberiaEnviarCorreo()
    {
        await _service.EnviarNotificacionCambioPassword("to@example.com", "Carlos");

        _smtpClientMock.Verify(c => c.SendMailAsync(It.Is<MailMessage>(m =>
            m.Subject.Contains("cambio de contraseña") &&
            m.Body.Contains("contraseña ha sido cambiada")
        )), Times.Once);
    }

    [Fact]
    public async Task EnviarEnlaceRecuperacion_DeberiaEnviarCorreo()
    {
        var token = "token123";
        await _service.EnviarEnlaceRecuperacion("to@example.com", "Carlos", token);

        _smtpClientMock.Verify(c => c.SendMailAsync(It.Is<MailMessage>(m =>
            m.Subject.Contains("Recuperación") &&
            m.Body.Contains(token)
        )), Times.Once);
    }
}
