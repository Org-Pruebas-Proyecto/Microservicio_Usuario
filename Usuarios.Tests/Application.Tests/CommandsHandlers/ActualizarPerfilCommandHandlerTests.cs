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
using Domain.ValueObjects;

namespace Usuarios.Tests.Application.Tests.CommandsHandlers;

public class ActualizarPerfilCommandHandlerTests
{
    private readonly Mock<IUsuarioRepository> _repositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly Mock<IActividadRepository> _actividadRepositoryMock;
    private readonly ActualizarPerfilCommandHandler _handler;

    public ActualizarPerfilCommandHandlerTests()
    {
        _repositoryMock = new Mock<IUsuarioRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _actividadRepositoryMock = new Mock<IActividadRepository>();

        _handler = new ActualizarPerfilCommandHandler(
            _repositoryMock.Object,
            _eventPublisherMock.Object,
            _actividadRepositoryMock.Object
        );
    }

    /// ✅ **Caso base: Perfil actualizado correctamente**
    [Fact]
    public async Task Handle_DeberiaActualizarPerfil_CuandoDatosSonValidos()
    {
        var usuario = new Usuario("Nombre", "Apellido", "usuario123", "password123", "usuario@test.com", "123456789", "Dirección");
        var command = new ActualizarPerfilCommand(usuario.Id, "NuevoNombre", "NuevoApellido", "nuevo@test.com", "987654321", "Nueva Dirección");

        _repositoryMock.Setup(r => r.GetByIdAsync(usuario.Id)).ReturnsAsync(usuario);
        _repositoryMock.Setup(r => r.UpdateAsync(usuario)).Returns(Task.CompletedTask);
        _actividadRepositoryMock.Setup(ar => ar.RegistrarActividad(It.IsAny<Actividad>())).Returns(Task.CompletedTask);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        Assert.True(resultado);
        Assert.Equal("NuevoNombre", usuario.Nombre);
        Assert.Equal("NuevoApellido", usuario.Apellido);
        Assert.Equal("nuevo@test.com", usuario.Correo);
        Assert.Equal("987654321", usuario.Telefono);
        Assert.Equal("Nueva Dirección", usuario.Direccion);

        _repositoryMock.Verify(r => r.UpdateAsync(usuario), Times.Once);
        _eventPublisherMock.Verify(e => e.Publish(It.IsAny<PerfilActualizadoEvent>(), "usuarios_exchange", "perfil.actualizado"), Times.Once);
        _actividadRepositoryMock.Verify(ar => ar.RegistrarActividad(It.IsAny<Actividad>()), Times.Once);
    }

    /// ❌ **Error: Usuario no encontrado**
    [Fact]
    public async Task Handle_DeberiaRetornarFalse_CuandoUsuarioNoExiste()
    {
        var command = new ActualizarPerfilCommand(Guid.NewGuid(), "NuevoNombre", "NuevoApellido", "nuevo@test.com", "987654321", "Nueva Dirección");

        _repositoryMock.Setup(r => r.GetByIdAsync(command.UsuarioId)).ReturnsAsync((Usuario)null);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        Assert.False(resultado);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Usuario>()), Times.Never);
        _eventPublisherMock.Verify(e => e.Publish(It.IsAny<PerfilActualizadoEvent>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _actividadRepositoryMock.Verify(ar => ar.RegistrarActividad(It.IsAny<Actividad>()), Times.Never);
    }

    /// ❌ **Error: Fallo en la actualización del perfil**
    [Fact]
    public async Task Handle_DeberiaLanzarExcepcion_SiActualizacionUsuarioFalla()
    {
        var usuario = new Usuario("Nombre", "Apellido", "usuario123", "password123", "usuario@test.com", "123456789", "Dirección");
        var command = new ActualizarPerfilCommand(usuario.Id, "NuevoNombre", "NuevoApellido", "nuevo@test.com", "987654321", "Nueva Dirección");

        _repositoryMock.Setup(r => r.GetByIdAsync(usuario.Id)).ReturnsAsync(usuario);
        _repositoryMock.Setup(r => r.UpdateAsync(usuario)).ThrowsAsync(new Exception("Error al actualizar perfil"));

        var ex = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal("Error al actualizar perfil", ex.Message);
    }

    /// ❌ **Error: Fallo en la publicación del evento**
    [Fact]
    public async Task Handle_DeberiaCapturarError_CuandoPublicacionDeEventoFalla()
    {
        var usuario = new Usuario("Nombre", "Apellido", "usuario123", "password123", "usuario@test.com", "123456789", "Dirección");
        var command = new ActualizarPerfilCommand(usuario.Id, "NuevoNombre", "NuevoApellido", "nuevo@test.com", "987654321", "Nueva Dirección");

        _repositoryMock.Setup(r => r.GetByIdAsync(usuario.Id)).ReturnsAsync(usuario);
        _repositoryMock.Setup(r => r.UpdateAsync(usuario)).Returns(Task.CompletedTask);
        _eventPublisherMock.Setup(e => e.Publish(It.IsAny<PerfilActualizadoEvent>(), "usuarios_exchange", "perfil.actualizado"))
            .Throws(new Exception("Error al publicar evento"));

        _actividadRepositoryMock.Setup(ar => ar.RegistrarActividad(It.IsAny<Actividad>())).Returns(Task.CompletedTask);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        Assert.True(resultado);
        _eventPublisherMock.Verify(e => e.Publish(It.IsAny<PerfilActualizadoEvent>(), "usuarios_exchange", "perfil.actualizado"), Times.Once);
        _actividadRepositoryMock.Verify(ar => ar.RegistrarActividad(It.IsAny<Actividad>()), Times.Once);
    }
}