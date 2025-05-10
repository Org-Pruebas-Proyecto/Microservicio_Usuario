namespace Domain.Entities
{
    public class Usuario
    { 
        public Guid Id { get; private set; }
        public string Nombre { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Correo { get; private set; }
        public string Telefono { get; private set; }

        // Constructor
        public Usuario(string nombre, string username, string password, string correo, string telefono) 
        { 
            Id = Guid.NewGuid();
            Nombre = nombre;
            Username = username;
            Password = password;
            Correo = correo;
            Telefono = telefono;
        }
    }
}

