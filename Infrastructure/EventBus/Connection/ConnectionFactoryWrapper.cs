using RabbitMQ.Client;

namespace Infrastructure.EventBus.Connection;
public class ConnectionFactoryWrapper : IConnectionFactoryWrapper
{
    private readonly ConnectionFactory _factory;

    public ConnectionFactoryWrapper(string host, string username, string password)
    {
        _factory = new ConnectionFactory
        {
            HostName = host,
            UserName = username,
            Password = password,
            DispatchConsumersAsync = true
        };
    }

    public IConnection CreateConnection()
    {
        return _factory.CreateConnection();
    }
}