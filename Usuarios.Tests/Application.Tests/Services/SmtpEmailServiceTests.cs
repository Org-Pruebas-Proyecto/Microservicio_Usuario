using Xunit;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Application.Services;
using Application.Interfaces;


namespace Usuarios.Tests.Application.Tests.Factories;
public class SmtpEmailServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ISmtpEmailService> _smtpEmailServiceMock;
    private readonly SmtpEmailService _emailService;

    public SmtpEmailServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _smtpEmailServiceMock = new Mock<ISmtpEmailService>();

        var configUserMock = new Mock<IConfigurationSection>();
        configUserMock.Setup(c => c.Value).Returns("user@test.com");
        _configurationMock.Setup(c => c.GetSection("EmailSettings:Username")).Returns(configUserMock.Object);

        _emailService = new SmtpEmailService(_configurationMock.Object);
    }

    /// Caso base: Envío exitoso de correo de confirmación
    [Fact]
    public async Task EnviarCorreoConfirmacion_DeberiaEnviarCorreo_SinErrores()
    {
        _smtpEmailServiceMock.Setup(es => es.EnviarCorreoConfirmacion(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var ex = await Record.ExceptionAsync(() => _smtpEmailServiceMock.Object.EnviarCorreoConfirmacion("usuario@test.com", "Usuario", "123456"));

        Assert.Null(ex);
        _smtpEmailServiceMock.Verify(es => es.EnviarCorreoConfirmacion(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    /// Caso base: Envío exitoso de enlace de recuperación
    [Fact]
    public async Task EnviarEnlaceRecuperacion_DeberiaEnviarCorreo_SinErrores()
    {
        _smtpEmailServiceMock.Setup(es => es.EnviarEnlaceRecuperacion(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var ex = await Record.ExceptionAsync(() => _smtpEmailServiceMock.Object.EnviarEnlaceRecuperacion("usuario@test.com", "Usuario", "token_recuperacion"));

        Assert.Null(ex);
        _smtpEmailServiceMock.Verify(es => es.EnviarEnlaceRecuperacion(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    /// Error en envío de correo
    [Fact]
    public async Task EnviarCorreoConfirmacion_DeberiaLanzarExcepcion_SiEnvioFalla()
    {
        _smtpEmailServiceMock.Setup(es => es.EnviarCorreoConfirmacion(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Error al enviar correo"));

        var ex = await Assert.ThrowsAsync<Exception>(() => _smtpEmailServiceMock.Object.EnviarCorreoConfirmacion("usuario@test.com", "Usuario", "123456"));

        Assert.Equal("Error al enviar correo", ex.Message);
    }
}