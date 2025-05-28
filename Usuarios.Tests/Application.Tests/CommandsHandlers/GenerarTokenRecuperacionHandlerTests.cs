using Xunit;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Commands;
using Application.Handlers;
using Application.Interfaces;
using Domain.Entities;
using Domain.ValueObjects;

namespace Usuarios.Tests.Application.Tests.CommandsHandlers;

public class GenerarTokenRecuperacionHandlerTests
{
    private readonly Mock<IUsuarioRepository> _repositoryMock;
    private readonly Mock<ISmtpEmailService> _smtpEmailServiceMock;
    private readonly Mock<IActividadRepository> _actividadRepositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisher;
    private readonly GenerarTokenRecuperacionHandler _handler;

    public GenerarTokenRecuperacionHandlerTests()
    {
        _repositoryMock = new Mock<IUsuarioRepository>();
        _smtpEmailServiceMock = new Mock<ISmtpEmailService>();
        _actividadRepositoryMock = new Mock<IActividadRepository>();
         _eventPublisher = new Mock<IEventPublisher>();

        _handler = new GenerarTokenRecuperacionHandler(
            _repositoryMock.Object,
            _smtpEmailServiceMock.Object,
            _actividadRepositoryMock.Object,
            _eventPublisher.Object
        );
    }

    /// ✅ **Caso base: Token generado correctamente**
    [Fact]
    public async Task Handle_DeberiaGenerarTokenRecuperacion_CuandoUsuarioExiste()
    {
        var usuario = new Usuario("Nombre", "Apellido", "usuario123", "password123", "usuario@test.com", "123456789", "Dirección");
        var command = new GenerarTokenRecuperacionCommand(usuario.Correo);

        _repositoryMock.Setup(r => r.GetByEmail(usuario.Correo)).ReturnsAsync(usuario);
        _repositoryMock.Setup(r => r.UpdateAsync(usuario)).Returns(Task.CompletedTask);
        _actividadRepositoryMock.Setup(ar => ar.RegistrarActividad(It.IsAny<Actividad>())).Returns(Task.CompletedTask);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        Assert.True(resultado);
        Assert.NotNull(usuario.TokenRecuperacion);
        _repositoryMock.Verify(r => r.UpdateAsync(usuario), Times.Once);
        _smtpEmailServiceMock.Verify(s => s.EnviarEnlaceRecuperacion(usuario.Correo, usuario.Nombre, usuario.TokenRecuperacion), Times.Once);
        _actividadRepositoryMock.Verify(ar => ar.RegistrarActividad(It.IsAny<Actividad>()), Times.Once);
    }

    /// ❌ **Error: Usuario no encontrado**
    [Fact]
    public async Task Handle_DeberiaRetornarFalse_CuandoUsuarioNoExiste()
    {
        var command = new GenerarTokenRecuperacionCommand("inexistente@test.com");

        _repositoryMock.Setup(r => r.GetByEmail(command.Correo)).ReturnsAsync((Usuario)null);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        Assert.False(resultado);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Usuario>()), Times.Never);
        _smtpEmailServiceMock.Verify(s => s.EnviarEnlaceRecuperacion(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _actividadRepositoryMock.Verify(ar => ar.RegistrarActividad(It.IsAny<Actividad>()), Times.Never);
    }

    /// ❌ **Error: Fallo en la actualización del usuario**
    [Fact]
    public async Task Handle_DeberiaLanzarExcepcion_SiActualizacionUsuarioFalla()
    {
        var usuario = new Usuario("Nombre", "Apellido", "usuario123", "password123", "usuario@test.com", "123456789", "Dirección");
        var command = new GenerarTokenRecuperacionCommand(usuario.Correo);

        _repositoryMock.Setup(r => r.GetByEmail(usuario.Correo)).ReturnsAsync(usuario);
        _repositoryMock.Setup(r => r.UpdateAsync(usuario)).ThrowsAsync(new Exception("Error al actualizar usuario"));

        var ex = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal("Error al actualizar usuario", ex.Message);
    }

    /// ❌ **Error: Fallo en el envío del correo**
    [Fact]
    public async Task Handle_DeberiaCapturarError_CuandoEnvioDeCorreoFalla()
    {
        var usuario = new Usuario("Nombre", "Apellido", "usuario123", "password123", "usuario@test.com", "123456789", "Dirección");
        var command = new GenerarTokenRecuperacionCommand(usuario.Correo);

        _repositoryMock.Setup(r => r.GetByEmail(usuario.Correo)).ReturnsAsync(usuario);
        _repositoryMock.Setup(r => r.UpdateAsync(usuario)).Returns(Task.CompletedTask);
        _smtpEmailServiceMock.Setup(s => s.EnviarEnlaceRecuperacion(usuario.Correo, usuario.Nombre, usuario.TokenRecuperacion))
            .ThrowsAsync(new Exception("Error al enviar correo"));

        _actividadRepositoryMock.Setup(ar => ar.RegistrarActividad(It.IsAny<Actividad>())).Returns(Task.CompletedTask);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        Assert.True(resultado);
        _smtpEmailServiceMock.Verify(s => s.EnviarEnlaceRecuperacion(usuario.Correo, usuario.Nombre, usuario.TokenRecuperacion), Times.Once);
        _actividadRepositoryMock.Verify(ar => ar.RegistrarActividad(It.IsAny<Actividad>()), Times.Once);
    }
}