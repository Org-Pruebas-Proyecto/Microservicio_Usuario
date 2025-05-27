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

public class CrearUsuarioCommandHandlerTests
{
    private readonly Mock<IUsuarioRepository> _repositoryMock;
    private readonly Mock<IUsuarioFactory> _factoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly Mock<ISmtpEmailService> _smtpEmailServiceMock;
    private readonly Mock<IActividadRepository> _actividadRepositoryMock;
    private readonly CrearUsuarioCommandHandler _handler;

    public CrearUsuarioCommandHandlerTests()
    {
        _repositoryMock = new Mock<IUsuarioRepository>();
        _factoryMock = new Mock<IUsuarioFactory>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _smtpEmailServiceMock = new Mock<ISmtpEmailService>();
        _actividadRepositoryMock = new Mock<IActividadRepository>();

        _handler = new CrearUsuarioCommandHandler(
            _repositoryMock.Object,
            _factoryMock.Object,
            _eventPublisherMock.Object,
            _smtpEmailServiceMock.Object,
            _actividadRepositoryMock.Object
        );
    }

    /// ✅ **Caso base: Usuario creado correctamente**
    [Fact]
    public async Task Handle_DeberiaCrearUsuarioYRetornarId()
    {
        var usuarioId = Guid.NewGuid();
        var usuario = new Usuario("Test", "Test", "Test123", "password123",
            "Test@test.com", "123456789", "Calle Falsa 123");

        var command = new CrearUsuarioCommand(usuario.Nombre, usuario.Apellido, usuario.Username, usuario.Password,
            usuario.Correo, usuario.Telefono, usuario.Direccion);

        _factoryMock.Setup(f => f.CrearUsuario(command.Nombre, command.Apellido, command.Username, command.Password,
            command.Correo, command.Telefono, command.Direccion))
            .Returns(usuario);

        _repositoryMock.Setup(r => r.AddAsync(usuario)).Returns(Task.CompletedTask);
        _actividadRepositoryMock.Setup(ar => ar.RegistrarActividad(It.IsAny<Actividad>())).Returns(Task.CompletedTask);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(usuario.Id, resultado);
        _repositoryMock.Verify(r => r.AddAsync(usuario), Times.Once);
        _eventPublisherMock.Verify(e => e.Publish(It.IsAny<UsuarioCreadoEvent>(), "usuarios_exchange", "usuario.creado"), Times.Once);
        _smtpEmailServiceMock.Verify(s => s.EnviarCorreoConfirmacion(usuario.Correo, usuario.Nombre, usuario.CodigoConfirmacion), Times.Once);
        _actividadRepositoryMock.Verify(ar => ar.RegistrarActividad(It.IsAny<Actividad>()), Times.Once);
    }

    /// ❌ **Error: El usuario no puede ser creado**
    [Fact]
    public async Task Handle_DeberiaLanzarExcepcion_CuandoFactoryRetornaNull()
    {
        var command = new CrearUsuarioCommand("Test", "Test", "Test123", "password123",
            "Test@test.com", "123456789", "Calle Falsa 123");

        _factoryMock.Setup(f => f.CrearUsuario(command.Nombre, command.Apellido, command.Username, command.Password,
            command.Correo, command.Telefono, command.Direccion))
            .Returns((Usuario)null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal("Error al crear usuario", ex.Message);
    }

    /// ❌ **Error: El repositorio falla al guardar**
    [Fact]
    public async Task Handle_DeberiaLanzarExcepcion_SiGuardarUsuarioFalla()
    {
        var usuario = new Usuario("Test", "Test", "Test123", "password123", "Test@test.com", "123456789", "Calle Falsa 123");
        var command = new CrearUsuarioCommand(usuario.Nombre, usuario.Apellido, usuario.Username, usuario.Password, usuario.Correo, usuario.Telefono, usuario.Direccion);

        _factoryMock.Setup(f => f.CrearUsuario(command.Nombre, command.Apellido, command.Username, command.Password, command.Correo, command.Telefono, command.Direccion))
            .Returns(usuario);

        _repositoryMock.Setup(r => r.AddAsync(usuario))
            .ThrowsAsync(new Exception("Fallo al guardar"));

        var ex = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal("Fallo al guardar", ex.Message);
    }

    /// ❌ **Error: Fallo en el envío de correo**
    [Fact]
    public async Task Handle_DeberiaCapturarError_SiCorreoNoPuedeSerEnviado()
    {
        var usuario = new Usuario("Test", "Test", "Test123", "password123", "Test@test.com", "123456789", "Calle Falsa 123");
        var command = new CrearUsuarioCommand(usuario.Nombre, usuario.Apellido, usuario.Username, usuario.Password, usuario.Correo, usuario.Telefono, usuario.Direccion);

        _factoryMock.Setup(f => f.CrearUsuario(command.Nombre, command.Apellido, command.Username, command.Password, command.Correo, command.Telefono, command.Direccion))
            .Returns(usuario);

        _repositoryMock.Setup(r => r.AddAsync(usuario)).Returns(Task.CompletedTask);
        _smtpEmailServiceMock.Setup(s => s.EnviarCorreoConfirmacion(usuario.Correo, usuario.Nombre, usuario.CodigoConfirmacion))
            .ThrowsAsync(new Exception("Error al enviar correo"));

        _actividadRepositoryMock.Setup(ar => ar.RegistrarActividad(It.IsAny<Actividad>())).Returns(Task.CompletedTask);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(usuario.Id, resultado);
        _smtpEmailServiceMock.Verify(s => s.EnviarCorreoConfirmacion(usuario.Correo, usuario.Nombre, usuario.CodigoConfirmacion), Times.Once);
        _actividadRepositoryMock.Verify(ar => ar.RegistrarActividad(It.IsAny<Actividad>()), Times.Once);
    }
}