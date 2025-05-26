using System.Text;
using Domain.Entities;
using Domain.Events;
using MongoDB.Driver;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            var collection = GetMongoCollection(serviceProvider);

            using JsonDocument doc = JsonDocument.Parse(message);
            JsonElement root = doc.RootElement;

            if (!root.TryGetProperty("Type", out JsonElement typeElement))
                throw new InvalidOperationException("Mensaje sin propiedad 'Type'");

            string eventType = typeElement.GetString();

            switch (eventType)
            {
                case "UsuarioRegistradoEvent":
                    await HandleUsuarioRegistradoEvent(message, collection);
                    break;
                case "UsuarioConfirmadoEvent":
                    await HandleUsuarioConfirmadoEvent(message, serviceProvider);
                    break;
                case "UsuarioPasswordCambiadoEvent":
                    await HandleUsuarioPasswordCambiadoEvent(message, collection);
                    break;
                case "PerfilActualizadoEvent":
                    await HandlePerfilActualizadoEvent(message, serviceProvider);
                    break;
                default:
                    throw new InvalidOperationException($"Tipo de evento desconocido: {eventType}");
            }
        }
        catch (Exception ex)
        {
            // Implementar logging aquí
            Console.WriteLine($"Error procesando mensaje: {ex.Message}");
        }
    }

    private IMongoCollection<UsuarioMongo> GetMongoCollection(IServiceProvider serviceProvider)
    {
        var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
        var database = mongoClient.GetDatabase("usuarios_db");
        return database.GetCollection<UsuarioMongo>("usuarios");
    }

    private async Task HandleUsuarioRegistradoEvent(string message, IMongoCollection<UsuarioMongo> collection)
    {
        var evento = JsonSerializer.Deserialize<UsuarioCreadoEvent>(message);
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

    private async Task HandleUsuarioConfirmadoEvent(string message, IServiceProvider serviceProvider)
    {
        var evento = JsonSerializer.Deserialize<UsuarioConfirmadoEvent>(message);
        await ActualizarConfirmacionMongo(evento, serviceProvider);
    }

    private async Task HandleUsuarioPasswordCambiadoEvent(string message, IMongoCollection<UsuarioMongo> collection)
    {
        var evento = JsonSerializer.Deserialize<UsuarioPasswordCambiadoEvent>(message);
        var filter = Builders<UsuarioMongo>.Filter.Eq(u => u.Id, evento.UsuarioId);
        var update = Builders<UsuarioMongo>.Update.Set(u => u.Password, evento.Password);
        await collection.UpdateOneAsync(filter, update);
    }

    private async Task ActualizarConfirmacionMongo(UsuarioConfirmadoEvent evento, IServiceProvider serviceProvider)
    {
        var collection = GetMongoCollection(serviceProvider);
        var filter = Builders<UsuarioMongo>.Filter.Eq(u => u.Id, evento.UsuarioId);
        var update = Builders<UsuarioMongo>.Update
            .Set(u => u.Verificado, evento.Confirmado);

        await collection.UpdateOneAsync(filter, update);
    }

    private async Task HandlePerfilActualizadoEvent(string message, IServiceProvider serviceProvider)
    {
        var evento = JsonSerializer.Deserialize<PerfilActualizadoEvent>(message);
        var collection = GetMongoCollection(serviceProvider);
        var filter = Builders<UsuarioMongo>.Filter.Eq(u => u.Id, evento.UsuarioId);
        var update = Builders<UsuarioMongo>.Update
            .Set(u => u.Nombre, evento.Nombre)
            .Set(u => u.Apellido, evento.Apellido)
            .Set(u => u.Correo, evento.Correo)
            .Set(u => u.Telefono, evento.Telefono)
            .Set(u => u.Direccion, evento.Direccion);
        await collection.UpdateOneAsync(filter, update);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Close();
        _connection?.Close();
        return Task.CompletedTask;
    }
}
