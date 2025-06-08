using Domain.ValueObjects;

namespace Application.Interfaces;

public interface IRol_Repositorio
{
    Task<Rol> GetByIdAsync(Guid id);
}