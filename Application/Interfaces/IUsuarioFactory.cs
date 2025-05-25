using Domain.Entities;

namespace Application.Interfaces;

public interface IUsuarioFactory
{
    Usuario CrearUsuario(string nombre, string apellido, string username, string password, string correo, string telefono, string direccion);

}