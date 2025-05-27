using Xunit;
using Moq;
using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Application.Commands;
using Application.Handlers;
using Application.Interfaces;
using Domain.Events;
using Domain.Entities;
using Domain.ValueObjects;

namespace Usuarios.Tests.Application.Tests.CommandsHandlers;

public class CambiarPasswordCommandHandlerTests
{
    private readonly Mock<IUsuarioRepository> _repositoryMock;
    private readonly Mock<ISmtpEmailService> _smtpEmailServiceMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly Mock<IActividadRepository> _actividadRepositoryMock;
    private readonly CambiarPasswordCommandHandler _handler;

    public CambiarPasswordCommandHandlerTests()
    {
        _repositoryMock = new Mock<IUsuarioRepository>();
        _smtpEmailServiceMock = new Mock<ISmtpEmailService>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _actividadRepositoryMock = new Mock<IActividadRepository>();

        _handler = new CambiarPasswordCommandHandler(
            _repositoryMock.Object,
            _smtpEmailServiceMock.Object,
            _eventPublisherMock.Object,
            _actividadRepositoryMock.Object
        );
    }

    /// ✅ **Caso base: Cambio de contraseña exitoso**
    [Fact]
    public async Task Handle_DeberiaCambiarPassword_CuandoDatosSonCorrectos()
    {
        var usuario = new Usuario("Nombre", "Apellido", "usuario123", "passwordActual", "usuario@test.com", "123456789", "Dirección");
        var command = new CambiarPasswordCommand(usuario.Id, "passwordActual", "nuevoPassword");

        _repositoryMock.Setup(r => r.GetByIdAsync(usuario.Id)).ReturnsAsync(usuario);
        _repositoryMock.Setup(r => r.UpdateAsync(usuario)).Returns(Task.CompletedTask);
        _actividadRepositoryMock.Setup(ar => ar.RegistrarActividad(It.IsAny<Actividad>())).Returns(Task.CompletedTask);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        Assert.True(resultado);
        Assert.Equal("nuevoPassword", usuario.Password);
        _repositoryMock.Verify(r => r.UpdateAsync(usuario), Times.Once);
        _smtpEmailServiceMock.Verify(s => s.EnviarNotificacionCambioPassword(usuario.Correo, usuario.Nombre), Times.Once);
        _eventPublisherMock.Verify(e => e.Publish(It.IsAny<UsuarioPasswordCambiadoEvent>(), "usuarios_exchange", "usuario.password.cambiado"), Times.Once);
        _actividadRepositoryMock.Verify(ar => ar.RegistrarActividad(It.IsAny<Actividad>()), Times.Once);
    }

    /// ❌ **Error: Usuario no encontrado**
    [Fact]
    public async Task Handle_DeberiaRetornarFalse_CuandoUsuarioNoExiste()
    {
        var command = new CambiarPasswordCommand(Guid.NewGuid(), "passwordActual", "nuevoPassword");

        _repositoryMock.Setup(r => r.GetByIdAsync(command.UsuarioId)).ReturnsAsync((Usuario)null);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        Assert.False(resultado);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Usuario>()), Times.Never);
        _smtpEmailServiceMock.Verify(s => s.EnviarNotificacionCambioPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _eventPublisherMock.Verify(e => e.Publish(It.IsAny<UsuarioPasswordCambiadoEvent>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _actividadRepositoryMock.Verify(ar => ar.RegistrarActividad(It.IsAny<Actividad>()), Times.Never);
    }

    /// ❌ **Error: Contraseña actual incorrecta**
    [Fact]
    public async Task Handle_DeberiaLanzarExcepcion_CuandoPasswordActualEsIncorrecta()
    {
        var usuario = new Usuario("Nombre", "Apellido", "usuario123", "passwordActual", "usuario@test.com", "123456789", "Dirección");
        var command = new CambiarPasswordCommand(usuario.Id, "passwordIncorrecta", "nuevoPassword");

        _repositoryMock.Setup(r => r.GetByIdAsync(usuario.Id)).ReturnsAsync(usuario);

        var ex = await Assert.ThrowsAsync<SecurityException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal("Contraseña actual incorrecta", ex.Message);
    }

    /// ❌ **Error: Fallo al actualizar contraseña**
    [Fact]
    public async Task Handle_DeberiaLanzarExcepcion_SiActualizacionDeUsuarioFalla()
    {
        var usuario = new Usuario("Nombre", "Apellido", "usuario123", "passwordActual", "usuario@test.com", "123456789", "Dirección");
        var command = new CambiarPasswordCommand(usuario.Id, "passwordActual", "nuevoPassword");

        _repositoryMock.Setup(r => r.GetByIdAsync(usuario.Id)).ReturnsAsync(usuario);
        _repositoryMock.Setup(r => r.UpdateAsync(usuario)).ThrowsAsync(new Exception("Error al actualizar usuario"));

        var ex = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal("Error al actualizar usuario", ex.Message);
    }

    /// ❌ **Error: Fallo en el envío de notificación de cambio**
    [Fact]
    public async Task Handle_DeberiaCapturarError_CuandoEnvioDeCorreoFalla()
    {
        var usuario = new Usuario("Nombre", "Apellido", "usuario123", "passwordActual", "usuario@test.com", "123456789", "Dirección");
        var command = new CambiarPasswordCommand(usuario.Id, "passwordActual", "nuevoPassword");

        _repositoryMock.Setup(r => r.GetByIdAsync(usuario.Id)).ReturnsAsync(usuario);
        _repositoryMock.Setup(r => r.UpdateAsync(usuario)).Returns(Task.CompletedTask);
        _smtpEmailServiceMock.Setup(s => s.EnviarNotificacionCambioPassword(usuario.Correo, usuario.Nombre))
            .ThrowsAsync(new Exception("Error al enviar correo"));

        _actividadRepositoryMock.Setup(ar => ar.RegistrarActividad(It.IsAny<Actividad>())).Returns(Task.CompletedTask);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        Assert.True(resultado);
        _smtpEmailServiceMock.Verify(s => s.EnviarNotificacionCambioPassword(usuario.Correo, usuario.Nombre), Times.Once);
        _actividadRepositoryMock.Verify(ar => ar.RegistrarActividad(It.IsAny<Actividad>()), Times.Once);
    }
}