using Domain.Entities;

namespace Application.Interfaces;

public interface IUsuarioRepository
{
    Task AddAsync(Usuario usuario);
    Task UpdateAsync(Usuario usuario);

    Task<Usuario> GetByEmail(string email);

}