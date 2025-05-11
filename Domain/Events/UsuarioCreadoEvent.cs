namespace Domain.Events
{
    public class UsuarioCreadoEvent
    {
        public Guid Id { get; }
        public string Nombre { get; }
        public string Username { get; }
        public string Correo { get; }

        public UsuarioCreadoEvent(Guid id, string nombre, string username, string correo)
        {
            Id = id;
            Nombre = nombre;
            Username = username;
            Correo = correo;
        }
    }
}
