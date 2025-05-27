namespace Application.Interfaces;

public interface ISmtpEmailService
{
    Task EnviarCorreoConfirmacion(string email, string nombre, string codigo);
    Task EnviarNotificacionCambioPassword(string email, string nombre);

    Task EnviarEnlaceRecuperacion(string email, string nombre, string tokenRecuperacion);
}