using Domain.Entities;

namespace Application.Interfaces;

public interface IUsuarioRepository
{
    Task AddAsync(Usuario usuario);
    Task UpdateAsync(Usuario usuario);



    Task<Usuario> GetByIdAsync(Guid id);
    Task<Usuario> GetByEmail(string email);

    Task<Usuario> GetByTokenRecuperacion(string token);
}