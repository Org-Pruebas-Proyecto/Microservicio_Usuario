using Domain.Entities;
using Domain.Events;
using Domain.ValueObjects;
using Infrastructure.EventBus.Consumers;
using MongoDB.Driver;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Usuarios.Tests.Infrastructure.Tests.EventBus.Consumers;
public class UsuarioEventHandlerTests
{
    private readonly Mock<IMongoClient> _mongoClientMock;
    private readonly Mock<IMongoDatabase> _mongoDatabaseMock;
    private readonly Mock<IMongoCollection<UsuarioMongo>> _usuariosCollectionMock;
    private readonly Mock<IMongoCollection<ActividadMongo>> _actividadesCollectionMock;
    private readonly UsuarioEventHandler _handler;

    public UsuarioEventHandlerTests()
    {
        _mongoClientMock = new Mock<IMongoClient>();
        _mongoDatabaseMock = new Mock<IMongoDatabase>();
        _usuariosCollectionMock = new Mock<IMongoCollection<UsuarioMongo>>();
        _actividadesCollectionMock = new Mock<IMongoCollection<ActividadMongo>>();

        _mongoClientMock
            .Setup(m => m.GetDatabase("usuarios_db", null))
            .Returns(_mongoDatabaseMock.Object);

        _mongoDatabaseMock
            .Setup(db => db.GetCollection<UsuarioMongo>("usuarios", null))
            .Returns(_usuariosCollectionMock.Object);

        _mongoDatabaseMock
            .Setup(db => db.GetCollection<ActividadMongo>("actividades", null))
            .Returns(_actividadesCollectionMock.Object);

        _handler = new UsuarioEventHandler(_mongoClientMock.Object);
    }

    [Fact]
    public async Task HandleUsuarioRegistradoAsync_ShouldInsertUsuario()
    {
        var id = Guid.NewGuid();
        var evento = new UsuarioCreadoEvent(
            id,
            nombre: "Juan",
            apellido: "Pérez",
            username: "juanp",
            password: "1234",
            telefono: "123456789",
            correo: "juan@example.com",
            direccion: "Calle 123",
            codigoConfirmacion: "ABC123"
        );

        await _handler.HandleUsuarioRegistradoAsync(evento);

        _usuariosCollectionMock.Verify(c =>
            c.InsertOneAsync(It.Is<UsuarioMongo>(u =>
                    u.Id == evento.Id &&
                    u.Nombre == evento.Nombre &&
                    u.Apellido == evento.Apellido &&
                    u.Username == evento.Username &&
                    u.Password == evento.Password &&
                    u.Telefono == evento.Telefono &&
                    u.Correo == evento.Correo &&
                    u.Direccion == evento.Direccion),
                null,
                default), Times.Once);
    }


    [Fact]
    public async Task HandleUsuarioConfirmadoAsync_ShouldUpdateUsuario()
    {
        var evento = new UsuarioConfirmadoEvent(
            Guid.NewGuid(), true);

        await _handler.HandleUsuarioConfirmadoAsync(evento);

        _usuariosCollectionMock.Verify(c =>
            c.UpdateOneAsync(
                It.IsAny<FilterDefinition<UsuarioMongo>>(),
                It.Is<UpdateDefinition<UsuarioMongo>>(u => true),
                null,
                default), Times.Once);
    }

    [Fact]
    public async Task HandleUsuarioPasswordCambiadoAsync_ShouldUpdatePassword()
    {
        var evento = new UsuarioPasswordCambiadoEvent(
            Guid.NewGuid(), "newpassword");

        await _handler.HandleUsuarioPasswordCambiadoAsync(evento);

        _usuariosCollectionMock.Verify(c =>
            c.UpdateOneAsync(
                It.IsAny<FilterDefinition<UsuarioMongo>>(),
                It.Is<UpdateDefinition<UsuarioMongo>>(u => true),
                null,
                default), Times.Once);
    }

    [Fact]
    public async Task HandlePerfilActualizadoAsync_ShouldUpdateUsuario()
    {
        var evento = new PerfilActualizadoEvent
        (
            Guid.NewGuid(),
            "Ana",
            "Gómez",
            "ana@example.com",
            "999888777",
            "Av. Siempre Viva"
        );

        await _handler.HandlePerfilActualizadoAsync(evento);

        _usuariosCollectionMock.Verify(c =>
            c.UpdateOneAsync(
                It.IsAny<FilterDefinition<UsuarioMongo>>(),
                It.Is<UpdateDefinition<UsuarioMongo>>(u => true),
                null,
                default), Times.Once);
    }

    [Fact]
    public async Task HandleActividadRegistradaAsync_ShouldInsertActividad()
    {
        var evento = new ActividadRegistradaEvent
        (
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Login",
            "Desde Chrome",
            DateTime.UtcNow
        );

        await _handler.HandleActividadRegistradaAsync(evento);

        _actividadesCollectionMock.Verify(c =>
            c.InsertOneAsync(It.Is<ActividadMongo>(a =>
                a.Id == evento.ActividadId &&
                a.TipoAccion == evento.TipoAccion),
                null,
                default), Times.Once);
    }
}
