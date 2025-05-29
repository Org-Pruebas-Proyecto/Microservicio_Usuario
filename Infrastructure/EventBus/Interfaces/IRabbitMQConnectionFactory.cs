using RabbitMQ.Client;

namespace Infrastructure.EventBus.Interfaces;

public interface IRabbitMQConnectionFactory
{
    IConnection CreateConnection();
}