using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Usuarios.Tests.Infrastructure.Tests.Persistance.Repositories;
public class ActividadRepositoryTests
{
    [Fact]
    public async Task RegistrarActividad_DeberiaGuardarEnBaseDeDatos()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // DB aislada por test
            .Options;

        await using var context = new AppDbContext(options);
        var repository = new ActividadRepository(context);

        var usuarioId = Guid.NewGuid();
        var actividad = new Actividad(usuarioId, "LOGIN", "El usuario inició sesión");

        // Act
        await repository.RegistrarActividad(actividad);

        // Assert
        var actividadesEnDb = await context.Actividades.ToListAsync();
        Assert.Single(actividadesEnDb);
        Assert.Equal("LOGIN", actividadesEnDb[0].TipoAccion);
        Assert.Equal(usuarioId, actividadesEnDb[0].UsuarioId);
    }
}