using Application.Services;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using Xunit;

namespace Usuarios.Tests.Application.Tests.Services;
public class SmtpClientFactoryTests
{
    [Fact]
    public void Create_DeberiaCrearSmtpClientConConfiguracionCorrecta()
    {
        // Arrange
        var settings = new EmailSettings
        {
            Host = "smtp.test.com",
            Port = 587,
            Username = "usuario@test.com",
            Password = "clave123"
        };

        var options = Options.Create(settings);
        var factory = new SmtpClientFactory(options);

        // Act
        var result = factory.Create();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<SmtpClientAdapter>(result);
    }
}