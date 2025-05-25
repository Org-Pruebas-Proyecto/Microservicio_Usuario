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

public class RabbitMQConsumerService: IHostedService
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
            var evento = JsonSerializer.Deserialize<UsuarioCreadoEvent>(message);
            var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
            var database = mongoClient.GetDatabase("usuarios_db");
            var collection = database.GetCollection<UsuarioMongo>("usuarios");

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
        catch (Exception ex)
        {
            // Implementar logging aquí
            Console.WriteLine($"Error procesando mensaje: {ex.Message}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Close();
        _connection?.Close();
        return Task.CompletedTask;
    }
}
