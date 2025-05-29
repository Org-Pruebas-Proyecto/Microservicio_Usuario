using Domain.ValueObjects;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;


namespace Usuarios.Tests.Infrastructure.Tests.Persistance.Configurations;

public class ActividadConfigurationTests
{
    [Fact]
    public async Task CanInsertActividadIntoDatabase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("Test_ActividadDb")
            .Options;

        var actividad = new Actividad(
            Guid.NewGuid(),      // UsuarioId
            "LOGIN",             // TipoAccion
            "El usuario inició sesión" // Detalles
        );


        using (var context = new AppDbContext(options))
        {
            context.Actividades.Add(actividad);
            await context.SaveChangesAsync();
        }

        using (var context = new AppDbContext(options))
        {
            var result = await context.Actividades.FirstOrDefaultAsync(a => a.TipoAccion == "LOGIN");

            Assert.NotNull(result);
            Assert.Equal("LOGIN", result.TipoAccion);
            Assert.Equal("El usuario inició sesión", result.Detalles);
        }
    }
}