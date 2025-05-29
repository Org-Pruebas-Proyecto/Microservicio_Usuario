using Application.Services;
using System.Net;
using System.Net.Mail;
using Xunit;

namespace Usuarios.Tests.Application.Tests.Services;
public class SmtpClientAdapterTests
{
    [Fact]
    public void Credentials_DeberiaAsignarYRecuperar()
    {
        // Arrange
        var smtpClient = new SmtpClient();
        var adapter = new SmtpClientAdapter(smtpClient);
        var credentials = new NetworkCredential("user", "pass");

        // Act
        adapter.Credentials = credentials;

        // Assert
        Assert.Equal(credentials, adapter.Credentials);
    }

    [Fact]
    public async Task SendMailAsync_DeberiaNoLanzarExcepcionConMensajeVacio()
    {
        // Arrange
        var pickupDir = Path.GetFullPath("mails");
        Directory.CreateDirectory(pickupDir); // Asegura que el directorio exista

        var smtpClient = new SmtpClient("localhost", 2525)
        {
            DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
            PickupDirectoryLocation = pickupDir
        };

        var adapter = new SmtpClientAdapter(smtpClient);
        var message = new MailMessage("test@localhost", "destino@localhost", "test", "cuerpo");

        // Act & Assert
        await adapter.SendMailAsync(message); // No debe lanzar excepción
    }


    [Fact]
    public void Dispose_DeberiaNoLanzarExcepcion()
    {
        // Arrange
        var smtpClient = new SmtpClient();
        var adapter = new SmtpClientAdapter(smtpClient);

        // Act & Assert
        adapter.Dispose(); // Solo verifica que no falle
    }
}