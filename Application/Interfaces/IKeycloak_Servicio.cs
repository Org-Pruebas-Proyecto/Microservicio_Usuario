using Domain.Entities;

namespace Application.Interfaces;

public interface IKeycloak_Servicio
{
    Task<string> Crear_Usuario_Keycloak(Usuario usuario);
    Task Asignar_Rol_Usuario_Keycloak(string keycloak_Id, string rol);
    Task Actualizar_Usuario_Keycloak(Usuario usuario);

}