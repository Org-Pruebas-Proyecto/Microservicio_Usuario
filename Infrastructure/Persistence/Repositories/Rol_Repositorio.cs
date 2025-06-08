using Application.Interfaces;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Persistence.Repositories;

public class Rol_Repositorio : IRol_Repositorio
{
    private readonly AppDbContext _context;

    public Rol_Repositorio(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Rol> GetByIdAsync(Guid id)
    {
        return await  _context.Roles.FirstOrDefaultAsync(r => r.Id == id);
    }
}