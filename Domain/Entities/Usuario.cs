namespace Domain.Entities
{
    public class Usuario
    { 
        public Guid Id { get; private set; }
        public string Nombre { get; private set; }
        public string Apellido { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Correo { get; private set; }
        public string Telefono { get; private set; }
        public string Direccion { get; private set; }
        public bool Verificado { get; private set; }
        public string CodigoConfirmacion { get; private set; }
        public DateTime FechaExpiracionCodigo { get; private set; }


        // Constructor
        public Usuario(string nombre,string apellido, string username, string password, string correo, string telefono, string direccion) 
        { 
            Id = Guid.NewGuid();
            Nombre = nombre;
            Apellido = apellido;
            Username = username;
            Password = password;
            Correo = correo;
            Telefono = telefono;
            Direccion = direccion;
            Verificado = false;
            GenerarCodigo();
        }
        private void GenerarCodigo()
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            CodigoConfirmacion = new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            FechaExpiracionCodigo = DateTime.UtcNow.AddHours(24);
        }
        public void VerificarCuenta()
        {
            Verificado = true;

        }


    }
}

