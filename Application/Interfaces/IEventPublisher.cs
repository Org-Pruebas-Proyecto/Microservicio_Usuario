namespace Application.Interfaces;

public interface IEventPublisher
{
    void Publish<T>(T message, string exchangeName, string routingKey);
}