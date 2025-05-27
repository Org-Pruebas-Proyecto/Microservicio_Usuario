using Xunit;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Commands;
using Application.Handlers;
using Application.Interfaces;
using Domain.Events;
using Domain.Entities;
using System.Reflection;


namespace Usuarios.Tests.Application.Tests.CommandsHandlers;

public class RestablecerPasswordCommandHandlerTests
{
    private readonly Mock<IUsuarioRepository> _repositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly RestablecerPasswordCommandHandler _handler;

    public RestablecerPasswordCommandHandlerTests()
    {
        _repositoryMock = new Mock<IUsuarioRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _handler = new RestablecerPasswordCommandHandler(
            _repositoryMock.Object,
            _eventPublisherMock.Object
        );
    }

    /// Caso base: Restablecimiento exitoso de contraseña
    [Fact]
    public async Task Handle_DeberiaRestablecerPassword_CuandoTokenEsValido_YNoExpirado()
    {
        var usuario = new Usuario("Nombre", "Apellido", "usuario123", "passwordActual", "usuario@test.com", "123456789", "Dirección");

        // Simular un token válido
        typeof(Usuario).GetField("<TokenRecuperacion>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(usuario, "tokenValido");

        typeof(Usuario).GetField("<ExpiracionTokenRecuperacion>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(usuario, DateTime.UtcNow.AddHours(1));

        var command = new RestablecerPasswordCommand("tokenValido", "nuevoPassword");

        _repositoryMock.Setup(r => r.GetByTokenRecuperacion(command.Token)).ReturnsAsync(usuario);
        _repositoryMock.Setup(r => r.UpdateAsync(usuario)).Returns(Task.CompletedTask);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        Assert.True(resultado);
        Assert.Equal("nuevoPassword", usuario.Password);
        _repositoryMock.Verify(r => r.UpdateAsync(usuario), Times.Once);
        _eventPublisherMock.Verify(e => e.Publish(It.IsAny<UsuarioPasswordCambiadoEvent>(), "usuarios_exchange", "usuario.password.cambiado"), Times.Once);
    }

    /// Error: Usuario no encontrado
    [Fact]
    public async Task Handle_DeberiaRetornarFalse_CuandoUsuarioNoExiste()
    {
        var command = new RestablecerPasswordCommand("tokenInvalido", "nuevoPassword");

        _repositoryMock.Setup(r => r.GetByTokenRecuperacion(command.Token)).ReturnsAsync((Usuario)null);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        Assert.False(resultado);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Usuario>()), Times.Never);
        _eventPublisherMock.Verify(e => e.Publish(It.IsAny<UsuarioPasswordCambiadoEvent>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    /// Error: Token expirado
    [Fact]
    public async Task Handle_DeberiaRetornarFalse_CuandoTokenHaExpirado()
    {
        var usuario = new Usuario("Nombre", "Apellido", "usuario123", "passwordActual", "usuario@test.com", "123456789", "Dirección");

        typeof(Usuario).GetField("<TokenRecuperacion>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(usuario, "tokenExpirado");

        typeof(Usuario).GetField("<ExpiracionTokenRecuperacion>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(usuario, DateTime.UtcNow.AddHours(-1)); // Token expirado

        var command = new RestablecerPasswordCommand("tokenExpirado", "nuevoPassword");

        _repositoryMock.Setup(r => r.GetByTokenRecuperacion(command.Token)).ReturnsAsync(usuario);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        Assert.False(resultado);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Usuario>()), Times.Never);
        _eventPublisherMock.Verify(e => e.Publish(It.IsAny<UsuarioPasswordCambiadoEvent>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    /// Error: Fallo en la actualización de la contraseña
    [Fact]
    public async Task Handle_DeberiaLanzarExcepcion_SiActualizacionUsuarioFalla()
    {
        var usuario = new Usuario("Nombre", "Apellido", "usuario123", "passwordActual", "usuario@test.com", "123456789", "Dirección");

        typeof(Usuario).GetField("<TokenRecuperacion>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(usuario, "tokenValido");

        typeof(Usuario).GetField("<ExpiracionTokenRecuperacion>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(usuario, DateTime.UtcNow.AddHours(1));

        var command = new RestablecerPasswordCommand("tokenValido", "nuevoPassword");

        _repositoryMock.Setup(r => r.GetByTokenRecuperacion(command.Token)).ReturnsAsync(usuario);
        _repositoryMock.Setup(r => r.UpdateAsync(usuario)).ThrowsAsync(new Exception("Error al actualizar usuario"));

        var ex = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal("Error al actualizar usuario", ex.Message);
    }

    /// Error: Fallo en la publicación del evento
    [Fact]
    public async Task Handle_DeberiaCapturarError_CuandoPublicacionDeEventoFalla()
    {
        var usuario = new Usuario("Nombre", "Apellido", "usuario123", "passwordActual", "usuario@test.com", "123456789", "Dirección");

        typeof(Usuario).GetField("<TokenRecuperacion>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(usuario, "tokenValido");

        typeof(Usuario).GetField("<ExpiracionTokenRecuperacion>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(usuario, DateTime.UtcNow.AddHours(1));

        var command = new RestablecerPasswordCommand("tokenValido", "nuevoPassword");

        _repositoryMock.Setup(r => r.GetByTokenRecuperacion(command.Token)).ReturnsAsync(usuario);
        _repositoryMock.Setup(r => r.UpdateAsync(usuario)).Returns(Task.CompletedTask);
        _eventPublisherMock.Setup(e => e.Publish(It.IsAny<UsuarioPasswordCambiadoEvent>(), "usuarios_exchange", "usuario.password.cambiado"))
            .Throws(new Exception("Error al publicar evento"));

        var resultado = await _handler.Handle(command, CancellationToken.None);

        Assert.True(resultado);
        _eventPublisherMock.Verify(e => e.Publish(It.IsAny<UsuarioPasswordCambiadoEvent>(), "usuarios_exchange", "usuario.password.cambiado"), Times.Once);
    }
}