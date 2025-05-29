using System.Threading.Tasks;

namespace Infrastructure.EventBus.Interfaces;

public interface IRabbitMQMessageProcessor
{
    Task ProcessMessageAsync(string message);
}