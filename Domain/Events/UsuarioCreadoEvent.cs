namespace Domain.Events
{
    public class UsuarioCreadoEvent
    {
        public string Type => "UsuarioRegistradoEvent"; 
        public Guid Id { get; }
        public string Nombre { get; }
        public string Apellido { get; }
        public string Username { get; }
        public string Password { get; }
        public string Telefono { get; }
        public string Correo { get; }
        public string Direccion { get; }

        public string CodigoConfirmacion { get; }

        public UsuarioCreadoEvent(Guid id, string nombre,string apellido, string username, string password,string telefono,string correo, string direccion, string codigoConfirmacion)
        {
            Id = id;
            Nombre = nombre;
            Apellido = apellido;
            Username = username;
            Password = password;
            Telefono = telefono;
            Correo = correo;
            Direccion = direccion;
            CodigoConfirmacion = codigoConfirmacion;
        }
    }
}
