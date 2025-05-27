using Application.Interfaces;
using Domain.ValueObjects;

namespace Infrastructure.Persistence.Repositories;

public class ActividadRepository : IActividadRepository
{
    private readonly AppDbContext _context;

    public ActividadRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task RegistrarActividad(Actividad actividad)
    {
        await _context.Actividades.AddAsync(actividad);
        await _context.SaveChangesAsync();
    }
}