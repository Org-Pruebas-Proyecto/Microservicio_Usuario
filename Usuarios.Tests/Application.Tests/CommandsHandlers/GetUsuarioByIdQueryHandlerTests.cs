using Xunit;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Queries;
using Application.Handlers;
using Application.Interfaces;
using Domain.Entities;

namespace Usuarios.Tests.Application.Tests.CommandsHandlers;

public class GetUsuarioByIdQueryHandlerTests
{
    private readonly Mock<IMongoRepository<UsuarioMongo>> _mongoRepositoryMock;
    private readonly GetUsuarioByIdQueryHandler _handler;

    public GetUsuarioByIdQueryHandlerTests()
    {
        _mongoRepositoryMock = new Mock<IMongoRepository<UsuarioMongo>>();
        _handler = new GetUsuarioByIdQueryHandler(_mongoRepositoryMock.Object);
    }

    /// ✅ **Caso base: Usuario encontrado en la base de datos**
    [Fact]
    public async Task Handle_DeberiaRetornarUsuario_CuandoUsuarioExiste()
    {
        var usuarioId = Guid.NewGuid();
        var usuarioMongo = new UsuarioMongo
        {
            Id = usuarioId,
            Nombre = "Nombre",
            Apellido = "Apellido",
            Username = "usuario123",
            Password = "password123",
            Correo = "usuario@test.com",
            Telefono = "123456789",
            Direccion = "Dirección",
            Verificado = true
        };

        var query = new GetUsuarioByIdQuery(usuarioId);

        _mongoRepositoryMock.Setup(r => r.GetByIdAsync(usuarioId.ToString())).ReturnsAsync(usuarioMongo);

        var resultado = await _handler.Handle(query, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal(usuarioMongo.Id, resultado.Id);
        Assert.Equal(usuarioMongo.Nombre, resultado.Nombre);
        _mongoRepositoryMock.Verify(r => r.GetByIdAsync(usuarioId.ToString()), Times.Once);
    }

    /// ❌ **Error: Usuario no encontrado**
    [Fact]
    public async Task Handle_DeberiaRetornarNull_CuandoUsuarioNoExiste()
    {
        var usuarioId = Guid.NewGuid();
        var query = new GetUsuarioByIdQuery(usuarioId);

        _mongoRepositoryMock.Setup(r => r.GetByIdAsync(usuarioId.ToString())).ReturnsAsync((UsuarioMongo)null);

        var resultado = await _handler.Handle(query, CancellationToken.None);

        Assert.Null(resultado);
        _mongoRepositoryMock.Verify(r => r.GetByIdAsync(usuarioId.ToString()), Times.Once);
    }

    /// ❌ **Error: Fallo en la base de datos**
    [Fact]
    public async Task Handle_DeberiaLanzarExcepcion_SiAccesoABaseDeDatosFalla()
    {
        var usuarioId = Guid.NewGuid();
        var query = new GetUsuarioByIdQuery(usuarioId);

        _mongoRepositoryMock.Setup(r => r.GetByIdAsync(usuarioId.ToString())).ThrowsAsync(new Exception("Error en MongoDB"));

        var ex = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(query, CancellationToken.None));
        Assert.Equal("Error en MongoDB", ex.Message);
    }
}