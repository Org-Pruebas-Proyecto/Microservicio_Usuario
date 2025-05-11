namespace Domain.Entities;

public class UsuarioMongo
{
    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public string Username { get; set; }
    public string Correo { get; set; }
}