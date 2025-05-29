using Application.Interfaces;
using Application.Queries;
using Domain.ValueObjects;
using MediatR;
using MongoDB.Driver;



public class ObtenerTodasLasActividadesHandler : IRequestHandler<ObtenerTodasLasActividadesQuery, IEnumerable<ActividadMongo>>
{
    private readonly IMongoRepository<ActividadMongo> _repository;

    public ObtenerTodasLasActividadesHandler(IMongoRepository<ActividadMongo> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ActividadMongo>> Handle(ObtenerTodasLasActividadesQuery request, CancellationToken cancellationToken)
    {
        var filter = Builders<ActividadMongo>.Filter.Empty;
        return await _repository.FindAsync(filter);

    }
}