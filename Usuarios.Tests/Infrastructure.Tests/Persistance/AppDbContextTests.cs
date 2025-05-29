using Infrastructure.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;

namespace Usuarios.Tests.Infrastructure.Tests.Persistance;
public class AppDbContextTests
{
    [Fact]
    public async Task CanInsertUsuarioIntoDatabase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        var usuario = new Usuario(
            nombre: "Juan",
            apellido: "Pérez",
            username: "testuser",
            password: "securepass123",
            correo: "juan.perez@example.com",
            telefono: "1234567890",
            direccion: "Calle Falsa 123"
        );

        using (var context = new AppDbContext(options))
        {
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();
        }

        using (var context = new AppDbContext(options))
        {
            var usuarioGuardado = await context.Usuarios.FirstOrDefaultAsync(u => u.Username == "testuser");

            Assert.NotNull(usuarioGuardado);
            Assert.Equal("Juan", usuarioGuardado.Nombre);
        }
    }

    [Fact]
    public void OnModelCreating_SetsUsuarioPrimaryKey()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("TestDb_Model")
            .Options;

        using var context = new AppDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Usuario));

        Assert.NotNull(entityType);
        Assert.Equal("Id", entityType.FindPrimaryKey()?.Properties[0].Name);
    }
}