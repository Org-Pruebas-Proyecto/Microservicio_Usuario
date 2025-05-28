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
using Domain.ValueObjects;

namespace Usuarios.Tests.Application.Tests.CommandsHandlers;

public class RestablecerPasswordCommandHandlerTests
{
    private readonly Mock<IUsuarioRepository> _repositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly Mock<IActividadRepository> _actividadRepositoryMock;
    private readonly RestablecerPasswordCommandHandler _handler;

    public RestablecerPasswordCommandHandlerTests()
    {
        _repositoryMock = new Mock<IUsuarioRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _actividadRepositoryMock = new Mock<IActividadRepository>();

        _handler = new RestablecerPasswordCommandHandler(
            _repositoryMock.Object,
            _eventPublisherMock.Object,
            _actividadRepositoryMock.Object
        );
    }

    /// ✅ **Caso exitoso: Restablecimiento correcto de contraseña**
    [Fact]
    public async Task Handle_DeberiaRetornarTrue_CuandoRestablecimientoEsExitoso()
    {
        var usuario = new Usuario("Test", "User", "testuser", "oldPassword", "test@user.com", "123456789", "Address");
        usuario.GenerarTokenRecuperacion(TimeSpan.FromHours(1));

        var command = new RestablecerPasswordCommand(usuario.TokenRecuperacion!, "newPassword");

        _repositoryMock.Setup(r => r.GetByTokenRecuperacion(command.Token)).ReturnsAsync(usuario);
        _repositoryMock.Setup(r => r.UpdateAsync(usuario)).Returns(Task.CompletedTask);
        _actividadRepositoryMock.Setup(ar => ar.RegistrarActividad(It.IsAny<Actividad>())).Returns(Task.CompletedTask);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        Assert.True(resultado);
        _repositoryMock.Verify(r => r.UpdateAsync(usuario), Times.Once);
        _eventPublisherMock.Verify(e => e.Publish(It.IsAny<UsuarioPasswordCambiadoEvent>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        _actividadRepositoryMock.Verify(ar => ar.RegistrarActividad(It.IsAny<Actividad>()), Times.Once);
    }

    /// ❌ **Error al publicar evento de cambio de contraseña**
    [Fact]
    public async Task Handle_DeberiaContinuar_CuandoErrorAlPublicarEvento()
    {
        var usuario = new Usuario("Test", "User", "testuser", "oldPassword", "test@user.com", "123456789", "Address");
        usuario.GenerarTokenRecuperacion(TimeSpan.FromHours(1));

        var command = new RestablecerPasswordCommand(usuario.TokenRecuperacion!, "newPassword");

        _repositoryMock.Setup(r => r.GetByTokenRecuperacion(command.Token)).ReturnsAsync(usuario);
        _repositoryMock.Setup(r => r.UpdateAsync(usuario)).Returns(Task.CompletedTask);
        _actividadRepositoryMock.Setup(ar => ar.RegistrarActividad(It.IsAny<Actividad>())).Returns(Task.CompletedTask);
        _eventPublisherMock.Setup(e => e.Publish(It.IsAny<UsuarioPasswordCambiadoEvent>(), It.IsAny<string>(), It.IsAny<string>()))
                           .Throws(new Exception("Error simulado en el evento"));

        var resultado = await _handler.Handle(command, CancellationToken.None);

        Assert.True(resultado);
        _repositoryMock.Verify(r => r.UpdateAsync(usuario), Times.Once);
        _actividadRepositoryMock.Verify(ar => ar.RegistrarActividad(It.IsAny<Actividad>()), Times.Once);
        _eventPublisherMock.Verify(e => e.Publish(It.IsAny<UsuarioPasswordCambiadoEvent>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    /// ❌ **Error al registrar actividad**
    [Fact]
    public async Task Handle_DeberiaContinuar_CuandoErrorAlRegistrarActividad()
    {
        var usuario = new Usuario("Test", "User", "testuser", "oldPassword", "test@user.com", "123456789", "Address");
        usuario.GenerarTokenRecuperacion(TimeSpan.FromHours(1));

        var command = new RestablecerPasswordCommand(usuario.TokenRecuperacion!, "newPassword");

        _repositoryMock.Setup(r => r.GetByTokenRecuperacion(command.Token)).ReturnsAsync(usuario);
        _repositoryMock.Setup(r => r.UpdateAsync(usuario)).Returns(Task.CompletedTask);
        _actividadRepositoryMock.Setup(ar => ar.RegistrarActividad(It.IsAny<Actividad>()))
                               .Throws(new Exception("Error simulado en actividad"));

        var resultado = await _handler.Handle(command, CancellationToken.None);

        Assert.True(resultado);
        _repositoryMock.Verify(r => r.UpdateAsync(usuario), Times.Once);
        _eventPublisherMock.Verify(e => e.Publish(It.IsAny<UsuarioPasswordCambiadoEvent>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        _actividadRepositoryMock.Verify(ar => ar.RegistrarActividad(It.IsAny<Actividad>()), Times.Once);
    }
}