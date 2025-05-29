using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Domain.Events;
using Infrastructure.EventBus.Interfaces;

namespace Infrastructure.EventBus.Consumers;

public class RabbitMQMessageProcessor : IRabbitMQMessageProcessor
{
    private readonly IServiceProvider _serviceProvider;

    public RabbitMQMessageProcessor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ProcessMessageAsync(string message)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IUsuarioEventHandler>();

            using JsonDocument doc = JsonDocument.Parse(message);
            var root = doc.RootElement;

            if (!root.TryGetProperty("Type", out var typeElement))
                throw new InvalidOperationException("Mensaje sin propiedad 'Type'");

            var eventType = typeElement.GetString();

            switch (eventType)
            {
                case "UsuarioRegistradoEvent":
                case "UsuarioCreadoEvent": // 👈 Alias aceptado
                    Console.WriteLine($"➡️ Recibido evento de tipo {eventType}");
                    var creadoEvent = JsonSerializer.Deserialize<UsuarioCreadoEvent>(message);
                    await handler.HandleUsuarioRegistradoAsync(creadoEvent);
                    break;

                case "UsuarioConfirmadoEvent":
                    var confirmadoEvent = JsonSerializer.Deserialize<UsuarioConfirmadoEvent>(message);
                    await handler.HandleUsuarioConfirmadoAsync(confirmadoEvent);
                    break;

                case "UsuarioPasswordCambiadoEvent":
                    var passwordEvent = JsonSerializer.Deserialize<UsuarioPasswordCambiadoEvent>(message);
                    await handler.HandleUsuarioPasswordCambiadoAsync(passwordEvent);
                    break;

                case "PerfilActualizadoEvent":
                    var perfilEvent = JsonSerializer.Deserialize<PerfilActualizadoEvent>(message);
                    await handler.HandlePerfilActualizadoAsync(perfilEvent);
                    break;

                case "ActividadRegistradaEvent":
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, IncludeFields = true };
                    var actividadEvent = JsonSerializer.Deserialize<ActividadRegistradaEvent>(message, options);
                    await handler.HandleActividadRegistradaAsync(actividadEvent);
                    break;

                default:
                    throw new InvalidOperationException($"Tipo de evento desconocido: {eventType}");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error procesando mensaje: {ex.Message}");
        }
    }
}
