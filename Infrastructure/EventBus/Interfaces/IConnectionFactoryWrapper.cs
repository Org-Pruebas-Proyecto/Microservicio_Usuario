using RabbitMQ.Client;

public interface IConnectionFactoryWrapper
{
    IConnection CreateConnection();
}