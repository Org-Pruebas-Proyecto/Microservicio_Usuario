using Domain.ValueObjects;

namespace Application.Interfaces;

public interface IPermiso_Repositorio
{
    Task<Permiso> GetByIdAsync(Guid id);
}