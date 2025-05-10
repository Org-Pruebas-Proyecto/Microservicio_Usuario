using Domain.Entities;

namespace Application.Interfaces;

public interface IUsuarioRepository
{
    Task AddAsync(Usuario usuario);
}