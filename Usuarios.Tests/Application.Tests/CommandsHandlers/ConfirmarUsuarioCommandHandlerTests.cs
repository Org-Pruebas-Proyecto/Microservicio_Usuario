using Xunit;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using Application.Commands;
using Application.Handlers;
using Application.Interfaces;
using Domain.Events;
using Domain.Entities;
using Domain.ValueObjects;

namespace Usuarios.Tests.Application.Tests.CommandsHandlers;

public class ConfirmarUsuarioCommandHandlerTests
{
    private readonly Mock<IUsuarioRepository> _repositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly Mock<IActividadRepository> _actividadRepositoryMock;
    private readonly ConfirmarUsuarioCommandHandler _handler;

    public ConfirmarUsuarioCommandHandlerTests()
    {
        _repositoryMock = new Mock<IUsuarioRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _actividadRepositoryMock = new Mock<IActividadRepository>();

        _handler = new ConfirmarUsuarioCommandHandler(
            _repositoryMock.Object,
            _eventPublisherMock.Object,
            _actividadRepositoryMock.Object
        );
    }

    /// ✅ **Caso base: Usuario confirmado exitosamente**
    [Fact]
    public async Task Handle_DeberiaConfirmarUsuario_CuandoCodigoEsCorrecto_YNoExpirado()
    {
        var usuario = new Usuario("Nombre", "Apellido", "usuario123", "password123", "usuario@test.com", "123456789", "Dirección");

        // Modificar propiedades privadas con Reflection
        typeof(Usuario).GetField("<CodigoConfirmacion>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(usuario, "123456");

        typeof(Usuario).GetField("<FechaExpiracionCodigo>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(usuario, DateTime.UtcNow.AddHours(1));

        var command = new ConfirmarUsuarioCommand(usuario.Correo, "123456");

        _repositoryMock.Setup(r => r.GetByEmail(usuario.Correo)).ReturnsAsync(usuario);
        _repositoryMock.Setup(r => r.UpdateAsync(usuario)).Returns(Task.CompletedTask);
        _actividadRepositoryMock.Setup(ar => ar.RegistrarActividad(It.IsAny<Actividad>())).Returns(Task.CompletedTask);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        Assert.True(resultado);
        _repositoryMock.Verify(r => r.UpdateAsync(usuario), Times.Once);
        _eventPublisherMock.Verify(e => e.Publish(It.IsAny<UsuarioConfirmadoEvent>(), "usuarios_exchange", "usuario.confirmado"), Times.Once);
        _actividadRepositoryMock.Verify(ar => ar.RegistrarActividad(It.IsAny<Actividad>()), Times.Once);
    }

    /// ❌ **Error: Usuario no encontrado**
    [Fact]
    public async Task Handle_DeberiaRetornarFalse_CuandoUsuarioNoExiste()
    {
        var command = new ConfirmarUsuarioCommand("inexistente@test.com", "123456");

        _repositoryMock.Setup(r => r.GetByEmail(command.Email)).ReturnsAsync((Usuario)null);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        Assert.False(resultado);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Usuario>()), Times.Never);
        _eventPublisherMock.Verify(e => e.Publish(It.IsAny<UsuarioConfirmadoEvent>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _actividadRepositoryMock.Verify(ar => ar.RegistrarActividad(It.IsAny<Actividad>()), Times.Never);
    }

    /// ❌ **Error: Código incorrecto**
    [Fact]
    public async Task Handle_DeberiaRetornarFalse_CuandoCodigoConfirmacionEsIncorrecto()
    {
        var usuario = new Usuario("Nombre", "Apellido", "usuario123", "password123", "usuario@test.com", "123456789", "Dirección");

        typeof(Usuario).GetField("<CodigoConfirmacion>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(usuario, "654321");

        typeof(Usuario).GetField("<FechaExpiracionCodigo>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(usuario, DateTime.UtcNow.AddHours(1));

        var command = new ConfirmarUsuarioCommand(usuario.Correo, "123456");

        _repositoryMock.Setup(r => r.GetByEmail(usuario.Correo)).ReturnsAsync(usuario);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        Assert.False(resultado);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Usuario>()), Times.Never);
        _eventPublisherMock.Verify(e => e.Publish(It.IsAny<UsuarioConfirmadoEvent>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _actividadRepositoryMock.Verify(ar => ar.RegistrarActividad(It.IsAny<Actividad>()), Times.Never);
    }

    /// ❌ **Error: Código expirado**
    [Fact]
    public async Task Handle_DeberiaRetornarFalse_CuandoCodigoConfirmacionHaExpirado()
    {
        var usuario = new Usuario("Nombre", "Apellido", "usuario123", "password123", "usuario@test.com", "123456789", "Dirección");

        typeof(Usuario).GetField("<CodigoConfirmacion>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(usuario, "123456");

        typeof(Usuario).GetField("<FechaExpiracionCodigo>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(usuario, DateTime.UtcNow.AddHours(-1)); // Código expirado

        var command = new ConfirmarUsuarioCommand(usuario.Correo, usuario.CodigoConfirmacion);

        _repositoryMock.Setup(r => r.GetByEmail(usuario.Correo)).ReturnsAsync(usuario);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        Assert.False(resultado);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Usuario>()), Times.Never);
        _eventPublisherMock.Verify(e => e.Publish(It.IsAny<UsuarioConfirmadoEvent>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _actividadRepositoryMock.Verify(ar => ar.RegistrarActividad(It.IsAny<Actividad>()), Times.Never);
    }

    /// ❌ **Error: Fallo en la actualización del usuario**
    [Fact]
    public async Task Handle_DeberiaLanzarExcepcion_SiActualizacionUsuarioFalla()
    {
        var usuario = new Usuario("Nombre", "Apellido", "usuario123", "password123", "usuario@test.com", "123456789", "Dirección");

        typeof(Usuario).GetField("<CodigoConfirmacion>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(usuario, "123456");

        typeof(Usuario).GetField("<FechaExpiracionCodigo>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(usuario, DateTime.UtcNow.AddHours(1));

        var command = new ConfirmarUsuarioCommand(usuario.Correo, usuario.CodigoConfirmacion);

        _repositoryMock.Setup(r => r.GetByEmail(usuario.Correo)).ReturnsAsync(usuario);
        _repositoryMock.Setup(r => r.UpdateAsync(usuario)).ThrowsAsync(new Exception("Error al actualizar usuario"));

        var ex = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal("Error al actualizar usuario", ex.Message);
    }
}