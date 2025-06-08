using Application.Interfaces;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class Permiso_Repositorio: IPermiso_Repositorio
{
    private readonly AppDbContext _context;

    public Permiso_Repositorio(AppDbContext context)
    {
        context = _context;
    }

    public async Task<Permiso> GetByIdAsync(Guid id)
    {
        return await _context.Permisos.FirstOrDefaultAsync(p => p.Id == id);
    }
}