using Application.Interfaces;
using Application.Queries;
using Domain.ValueObjects;
using MediatR;
using MongoDB.Driver;

namespace Application.Handlers;

public class ObtenerHistorialHandler : IRequestHandler<ObtenerHistorialQuery, IEnumerable<ActividadMongo>>
{
    private readonly IMongoRepository<ActividadMongo> _repository;

    public ObtenerHistorialHandler(IMongoRepository<ActividadMongo> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ActividadMongo>> Handle(ObtenerHistorialQuery request, CancellationToken cancellationToken)
    {
        var filter = Builders<ActividadMongo>.Filter.Eq(a => a.UsuarioId, request.UsuarioId);

        if (!string.IsNullOrEmpty(request.TipoAccion))
            filter &= Builders<ActividadMongo>.Filter.Eq(a => a.TipoAccion, request.TipoAccion);

        if (request.Desde.HasValue)
            filter &= Builders<ActividadMongo>.Filter.Gte(a => a.Fecha, request.Desde);

        if (request.Hasta.HasValue)
            filter &= Builders<ActividadMongo>.Filter.Lte(a => a.Fecha, request.Hasta);

        var resultados = await _repository.FindAsync(filter);
        return resultados.OrderByDescending(a => a.Fecha);
    }
}