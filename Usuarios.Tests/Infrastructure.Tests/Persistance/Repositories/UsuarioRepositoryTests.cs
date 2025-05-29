using Xunit;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence.Repositories;
using Domain.Entities;
using System;
using System.Threading.Tasks;
using Infrastructure.Persistence;

namespace Usuarios.Tests.Infrastructure.Tests.Persistance.Repositories;
public class UsuarioRepositoryTests
{
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private Usuario CrearUsuarioValido()
    {
        return new Usuario(
            nombre: "Daniel",
            apellido: "Gomez",
            username: "danielg",
            password: "password123",
            correo: "daniel@example.com",
            telefono: "123456789",
            direccion: "Calle Falsa 123"
        );
    }

    [Fact]
    public async Task AddAsync_DeberiaAgregarUsuario()
    {
        var context = GetInMemoryDbContext();
        var repo = new UsuarioRepository(context);
        var usuario = CrearUsuarioValido();

        await repo.AddAsync(usuario);

        var result = await context.Usuarios.FirstOrDefaultAsync(u => u.Id == usuario.Id);

        Assert.NotNull(result);
        Assert.Equal("daniel@example.com", result.Correo);
    }

    [Fact]
    public async Task UpdateAsync_DeberiaActualizarUsuario()
    {
        var context = GetInMemoryDbContext();
        var usuario = CrearUsuarioValido();
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        var repo = new UsuarioRepository(context);

        usuario.ActualizarPerfil("NuevoNombre", "NuevoApellido", "nuevo@email.com", "987654321", "Calle Nueva 456");
        await repo.UpdateAsync(usuario);

        var actualizado = await context.Usuarios.FirstOrDefaultAsync(u => u.Id == usuario.Id);
        Assert.Equal("NuevoNombre", actualizado.Nombre);
        Assert.Equal("nuevo@email.com", actualizado.Correo);
    }

    [Fact]
    public async Task GetByIdAsync_DeberiaRetornarUsuario()
    {
        var context = GetInMemoryDbContext();
        var usuario = CrearUsuarioValido();
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        var repo = new UsuarioRepository(context);
        var result = await repo.GetByIdAsync(usuario.Id);

        Assert.NotNull(result);
        Assert.Equal(usuario.Correo, result.Correo);
    }

    [Fact]
    public async Task GetByEmail_DeberiaRetornarUsuario()
    {
        var context = GetInMemoryDbContext();
        var usuario = CrearUsuarioValido();
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        var repo = new UsuarioRepository(context);
        var result = await repo.GetByEmail(usuario.Correo);

        Assert.NotNull(result);
        Assert.Equal(usuario.Id, result.Id);
    }

    [Fact]
    public async Task GetByTokenRecuperacion_DeberiaRetornarUsuario()
    {
        var context = GetInMemoryDbContext();
        var usuario = CrearUsuarioValido();
        usuario.GenerarTokenRecuperacion(TimeSpan.FromHours(1));

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        var repo = new UsuarioRepository(context);
        var result = await repo.GetByTokenRecuperacion(usuario.TokenRecuperacion!);

        Assert.NotNull(result);
        Assert.Equal(usuario.Id, result.Id);
    }
}
