using System.Text;
using System.Text.Json;
using Domain.Entities;
using Domain.Events;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infrastructure.EventBus.Consumers;

public class MongoConfirmationConsumer: BackgroundService
{
    private readonly IModel _channel;
    private readonly IMongoCollection<UsuarioMongo> _collection;

    public MongoConfirmationConsumer(
        IConnection connection,
        IMongoDatabase database)
    {
        _channel = connection.CreateModel();
        _collection = database.GetCollection<UsuarioMongo>("usuarios");

        _channel.ExchangeDeclare("usuarios_exchange", ExchangeType.Topic);
        _channel.QueueDeclare("mongo_confirmation_queue", durable: true);
        _channel.QueueBind("mongo_confirmation_queue", "usuarios_exchange", "usuario.confirmado");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var evento = JsonSerializer.Deserialize<UsuarioConfirmadoEvent>(message);

            var filter = Builders<UsuarioMongo>.Filter.Eq(u => u.Id, evento.UsuarioId);
            var update = Builders<UsuarioMongo>.Update
                .Set(u => u.Verificado, true);
      
            await _collection.UpdateOneAsync(filter, update);
        };

        _channel.BasicConsume("mongo_confirmation_queue", true, consumer);
    }
}