using Application.Interfaces;
using Application.Queries;
using Domain.ValueObjects;
using MediatR;
using MongoDB.Driver;

namespace Application.Handlers;

public class Get_Rol_id_Query_Handler: IRequestHandler<Get_Rol_id_Query,Rol_Mongo>
{
    private readonly IMongoRepository<Rol_Mongo> _mongo_repositorio;
    public Get_Rol_id_Query_Handler(IMongoRepository<Rol_Mongo> mongo_repositorio)
    {
        _mongo_repositorio = mongo_repositorio;
    }

    public async Task<Rol_Mongo> Handle(Get_Rol_id_Query request, CancellationToken cancellationToken)
    {
       var filtro = Builders<Rol_Mongo>.Filter.Eq(r => r.Id, request.rol_id);
       var rol = await _mongo_repositorio.FindAsync(filtro);
       return rol.FirstOrDefault() ?? throw new KeyNotFoundException($"Rol con ID {request.rol_id} no encontrado.");
    }
}