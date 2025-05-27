using MongoDB.Driver;

namespace Application.Interfaces;

public interface IMongoRepository<TDocument>
{
    Task<TDocument> GetByIdAsync(string id);
    Task<IEnumerable<TDocument>> FindAsync(FilterDefinition<TDocument> filter);
}