using Domain.Entities;
using Domain.Events;
using Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Infrastructure.EventBus.Consumers;

public class RabbitMQConsumerService : IHostedService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceProvider _serviceProvider;
    private const string ExchangeName = "usuarios_exchange";
    private const string QueueName = "usuarios_mongo_queue";

    public RabbitMQConsumerService(
        string host,
        string username,
        string password,
        IServiceProvider serviceProvider)
    {
        var factory = new ConnectionFactory
        {
            HostName = host,
            UserName = username,
            Password = password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _serviceProvider = serviceProvider;

        SetupRabbitMQ();
    }

    private void SetupRabbitMQ()
    {
        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout, durable: true);
        _channel.QueueDeclare(QueueName, durable: true, exclusive: false);
        _channel.QueueBind(QueueName, ExchangeName, routingKey: "");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            using var scope = _serviceProvider.CreateScope();
            await ProcessMessage(message, scope.ServiceProvider);
        };

        _channel.BasicConsume(QueueName, autoAck: true, consumer: consumer);
        return Task.CompletedTask;
    }

    private async Task ProcessMessage(string message, IServiceProvider serviceProvider)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(message);
            JsonElement root = doc.RootElement;

            if (!root.TryGetProperty("Type", out JsonElement typeElement))
                throw new InvalidOperationException("Mensaje sin propiedad 'Type'");

            string eventType = typeElement.GetString();

            switch (eventType)
            {
                case "UsuarioRegistradoEvent":
                    await HandleEvent<UsuarioCreadoEvent>(message, sp => GetMongoCollectionUsuarios(sp), HandleUsuarioRegistradoEvent);
                    break;
                case "UsuarioConfirmadoEvent":
                    await HandleEvent<UsuarioConfirmadoEvent>(message, sp => GetMongoCollectionUsuarios(sp), HandleUsuarioConfirmadoEvent);
                    break;
                case "UsuarioPasswordCambiadoEvent":
                    await HandleEvent<UsuarioPasswordCambiadoEvent>(message, sp => GetMongoCollectionUsuarios(sp), HandleUsuarioPasswordCambiadoEvent);
                    break;
                case "PerfilActualizadoEvent":
                    await HandleEvent<PerfilActualizadoEvent>(message, sp => GetMongoCollectionUsuarios(sp), HandlePerfilActualizadoEvent);
                    break;
                case "ActividadRegistradaEvent":
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    await HandleEvent<ActividadRegistradaEvent>(message, sp => GetMongoCollectionActividad(sp), GuardarActividadMongo, options);
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

    private async Task HandleEvent<TEvent>(string message, Func<IServiceProvider, IMongoCollection<UsuarioMongo>> getCollection, Func<TEvent, IMongoCollection<UsuarioMongo>, Task> handler, JsonSerializerOptions options = null)
    {
        var evento = JsonSerializer.Deserialize<TEvent>(message, options);
        var collection = getCollection(_serviceProvider);
        await handler(evento, collection);
    }

    private async Task HandleEvent<TEvent>(string message, Func<IServiceProvider, IMongoCollection<ActividadMongo>> getCollection, Func<TEvent, IMongoCollection<ActividadMongo>, Task> handler, JsonSerializerOptions options = null)
    {
        var evento = JsonSerializer.Deserialize<TEvent>(message, options);
        var collection = getCollection(_serviceProvider);
        await handler(evento, collection);
    }

    private IMongoCollection<UsuarioMongo> GetMongoCollectionUsuarios(IServiceProvider serviceProvider)
    {
        var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
        var database = mongoClient.GetDatabase("usuarios_db");
        return database.GetCollection<UsuarioMongo>("usuarios");
    }

    private IMongoCollection<ActividadMongo> GetMongoCollectionActividad(IServiceProvider serviceProvider)
    {
        var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
        var database = mongoClient.GetDatabase("usuarios_db");
        return database.GetCollection<ActividadMongo>("actividades");
    }

    private async Task HandleUsuarioRegistradoEvent(UsuarioCreadoEvent evento, IMongoCollection<UsuarioMongo> collection)
    {
        var usuarioMongo = new UsuarioMongo
        {
            Id = evento.Id,
            Nombre = evento.Nombre,
            Apellido = evento.Apellido,
            Username = evento.Username,
            Password = evento.Password,
            Telefono = evento.Telefono,
            Correo = evento.Correo,
            Direccion = evento.Direccion,
        };

        await collection.InsertOneAsync(usuarioMongo);
    }

    private async Task HandleUsuarioConfirmadoEvent(UsuarioConfirmadoEvent evento, IMongoCollection<UsuarioMongo> collection)
    {
        await ActualizarConfirmacionMongo(evento, collection);
    }

    private async Task HandleUsuarioPasswordCambiadoEvent(UsuarioPasswordCambiadoEvent evento, IMongoCollection<UsuarioMongo> collection)
    {
        var filter = Builders<UsuarioMongo>.Filter.Eq(u => u.Id, evento.UsuarioId);
        var update = Builders<UsuarioMongo>.Update.Set(u => u.Password, evento.Password);
        await collection.UpdateOneAsync(filter, update);
    }

    private async Task ActualizarConfirmacionMongo(UsuarioConfirmadoEvent evento, IMongoCollection<UsuarioMongo> collection)
    {
        var filter = Builders<UsuarioMongo>.Filter.Eq(u => u.Id, evento.UsuarioId);
        var update = Builders<UsuarioMongo>.Update.Set(u => u.Verificado, evento.Confirmado);
        await collection.UpdateOneAsync(filter, update);
    }

    private async Task HandlePerfilActualizadoEvent(PerfilActualizadoEvent evento, IMongoCollection<UsuarioMongo> collection)
    {
        var filter = Builders<UsuarioMongo>.Filter.Eq(u => u.Id, evento.UsuarioId);
        var update = Builders<UsuarioMongo>.Update
            .Set(u => u.Nombre, evento.Nombre)
            .Set(u => u.Apellido, evento.Apellido)
            .Set(u => u.Correo, evento.Correo)
            .Set(u => u.Telefono, evento.Telefono)
            .Set(u => u.Direccion, evento.Direccion);
        await collection.UpdateOneAsync(filter, update);
    }

    private async Task GuardarActividadMongo(ActividadRegistradaEvent evento, IMongoCollection<ActividadMongo> collection)
    {
        var actividadMongo = new ActividadMongo
        {
            Id = evento.ActividadId,
            UsuarioId = evento.UsuarioId,
            TipoAccion = evento.TipoAccion,
            Detalles = evento.Detalles,
            Fecha = evento.Fecha
        };

        await collection.InsertOneAsync(actividadMongo);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Close();
        _connection?.Close();
        return Task.CompletedTask;
    }
}
