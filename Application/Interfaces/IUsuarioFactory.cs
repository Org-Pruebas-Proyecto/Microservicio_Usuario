using Domain.Entities;

namespace Application.Interfaces;

public interface IUsuarioFactory
{
    Usuario CrearUsuario(string nombre, string username, string password, string correo, string telefono);
}