using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Application.Services;

public class Keycloak_Servicio: IKeycloak_Servicio
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public Keycloak_Servicio(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }
    public async Task<string> Crear_Usuario_Keycloak(Usuario usuario)
    {
        var client = _httpClientFactory.CreateClient();
        var token = await ObtenerTokenAdmin();

        var keycloakUser = new
        {
            username = usuario.Username,
            email = usuario.Correo,
            firstName = usuario.Nombre,
            lastName = usuario.Apellido,
            enabled = true,
            credentials = new[]
            {
                new
                {
                    type = "password",
                    value = usuario.Password,
                    temporary = false
                }
            }
        };

        var realm = _configuration["Keycloak:realm"];
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync(
            $"{_configuration["Keycloak:auth-server-url"]}admin/realms/{realm}/users",
            keycloakUser);

        if (!response.IsSuccessStatusCode)
            throw new ApplicationException("Error creando usuario en Keycloak");

        // Obtener ID del usuario recién creado
        var location = response.Headers.Location.ToString();
        return location.Split('/').Last();
    }

    public async Task Asignar_Rol_Usuario_Keycloak(string keycloakId, string rol)
    {
        var client = _httpClientFactory.CreateClient();
        var token = await ObtenerTokenAdmin();
        var realm = _configuration["Keycloak:realm"];
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        //  Obtener el rol específico
        var rolResponse = await client.GetAsync(
            $"{_configuration["Keycloak:auth-server-url"]}admin/realms/{realm}/roles/{rol}");

        if (!rolResponse.IsSuccessStatusCode)
            throw new ApplicationException($"Rol {rol} no encontrado en Keycloak");

        var keycloakRole = await rolResponse.Content.ReadFromJsonAsync<KeycloakRole>();

        // Asignar el único rol al usuario
        var response = await client.PostAsJsonAsync(
            $"{_configuration["Keycloak:auth-server-url"]}admin/realms/{realm}/users/{keycloakId}/role-mappings/realm",
            new List<KeycloakRole> { keycloakRole });

        if (!response.IsSuccessStatusCode)
            throw new ApplicationException($"Error asignando rol: {rol}");
    }

    public async Task Actualizar_Usuario_Keycloak(Usuario usuario)
    {
        var client = _httpClientFactory.CreateClient();
        var token = await ObtenerTokenAdmin();
        var realm = _configuration["Keycloak:realm"];
        var keycloakUser = new
        {
            username = usuario.Username,
            email = usuario.Correo,
            firstName = usuario.Nombre,
            lastName = usuario.Apellido,
            enabled = true
        };
        // Actualizar usuario en Keycloak
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.PutAsJsonAsync(
            $"{_configuration["Keycloak:auth-server-url"]}admin/realms/{realm}/users/{usuario.KeycloakId}",
            keycloakUser);
        if (!response.IsSuccessStatusCode)
            throw new ApplicationException("Error actualizando usuario en Keycloak");
    }

    private async Task<string> ObtenerTokenAdmin()
    {
        var client = _httpClientFactory.CreateClient();
        var realm = _configuration["Keycloak:realm"];

        var parameters = new Dictionary<string, string>
        {
            {"client_id", "admin-cli"},
            {"grant_type", "client_credentials"},
            {"client_secret", _configuration["Keycloak:AdminClientSecret"]}
        };

        var response = await client.PostAsync(
            $"{_configuration["Keycloak:auth-server-url"]}realms/master/protocol/openid-connect/token",
            new FormUrlEncodedContent(parameters));

        var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>();
        return tokenResponse.access_token;
    }

    private record KeycloakTokenResponse(string access_token);
    private record KeycloakRole(string id, string name, string description);

}