namespace Application.Interfaces;

public abstract class IMongoUsuarioRepository<TDocument>
{
    public abstract Task<TDocument> GetByIdAsync(string id);
}