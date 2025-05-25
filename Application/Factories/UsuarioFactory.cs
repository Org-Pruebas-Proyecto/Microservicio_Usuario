using Application.Interfaces;
using Domain.Entities;


namespace Application.Factories
{
    public class UsuarioFactory: IUsuarioFactory
    {
        public Usuario CrearUsuario(string nombre, string apellido, string username, string password, string correo, string telefono, string direccion)
        {
            return new Usuario(nombre, apellido, username, password,correo,telefono,direccion);
        }
    }
}
