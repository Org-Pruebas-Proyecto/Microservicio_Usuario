// Connection/RabbitMQConnectionFactory.cs
using Infrastructure.EventBus.Interfaces;
using RabbitMQ.Client;

namespace Infrastructure.EventBus.Connection;
public class RabbitMQConnectionFactory : IRabbitMQConnectionFactory
{
    private readonly IConnectionFactoryWrapper _connectionFactoryWrapper;

    public RabbitMQConnectionFactory(IConnectionFactoryWrapper connectionFactoryWrapper)
    {
        _connectionFactoryWrapper = connectionFactoryWrapper;
    }

    public RabbitMQConnectionFactory(string host, string username, string password)
        : this(new ConnectionFactoryWrapper(host, username, password))
    {
    }


    public IConnection CreateConnection()
    {
        return _connectionFactoryWrapper.CreateConnection();
    }
}