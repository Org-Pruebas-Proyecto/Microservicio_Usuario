using Application.Interfaces;
using Domain.Entities;


namespace Application.Factories
{
    public class UsuarioFactory: IUsuarioFactory
    {
        public Usuario CrearUsuario(string nombre, string username, string password, string correo, string telefono)
        {
            return new Usuario(nombre, username, password, correo, telefono);
        }
    }
}
