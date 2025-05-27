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
            _eventPublisherMock.Object
        );
    }

    /// FALTA HACERLO SE PUSO COMICO Y ESTOY CANSADO
    /// ✅ **Caso base: Restablecimiento exitoso de contraseña**


    /// ❌ **Error: Usuario no encontrado**
    [Fact]
    public async Task Handle_DeberiaRetornarFalse_CuandoUsuarioNoExiste()
    {
        var command = new RestablecerPasswordCommand("tokenInvalido", "nuevoPassword");

        _repositoryMock.Setup(r => r.GetByTokenRecuperacion(command.Token)).ReturnsAsync((Usuario)null);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        Assert.False(resultado);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Usuario>()), Times.Never);
        _eventPublisherMock.Verify(e => e.Publish(It.IsAny<UsuarioPasswordCambiadoEvent>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _actividadRepositoryMock.Verify(ar => ar.RegistrarActividad(It.IsAny<Actividad>()), Times.Never);
    }

    /// ❌ **Error: Token expirado**
    [Fact]
    public async Task Handle_DeberiaRetornarFalse_CuandoTokenHaExpirado()
    {
        var usuario = new Usuario("Nombre", "Apellido", "usuario123", "passwordActual", "usuario@test.com", "123456789", "Dirección");

        typeof(Usuario).GetField("<TokenRecuperacion>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(usuario, "tokenExpirado");

        typeof(Usuario).GetField("<ExpiracionTokenRecuperacion>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(usuario, DateTime.UtcNow.AddHours(-1));

        var command = new RestablecerPasswordCommand("tokenExpirado", "nuevoPassword");

        _repositoryMock.Setup(r => r.GetByTokenRecuperacion(command.Token)).ReturnsAsync(usuario);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        Assert.False(resultado);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Usuario>()), Times.Never);
        _eventPublisherMock.Verify(e => e.Publish(It.IsAny<UsuarioPasswordCambiadoEvent>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _actividadRepositoryMock.Verify(ar => ar.RegistrarActividad(It.IsAny<Actividad>()), Times.Never);
    }
}