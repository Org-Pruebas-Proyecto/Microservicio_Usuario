using Application.Interfaces;
using Application.Queries;
using Domain.Entities;
using MediatR;
using MongoDB.Driver;

namespace Application.Handlers;

public class Get_Usuario_Correo_Query_Handler : IRequestHandler<Get_Usuario_Correo_Query,UsuarioMongo>
{
    private readonly IMongoRepository<UsuarioMongo> _mongoRepositorio;

    public Get_Usuario_Correo_Query_Handler(IMongoRepository<UsuarioMongo> mongoRepositorio)
    {
        _mongoRepositorio = mongoRepositorio;
    } 

    public async Task<UsuarioMongo> Handle(Get_Usuario_Correo_Query request, CancellationToken cancellationToken)
    {
        var filtro = Builders<UsuarioMongo>.Filter.Eq(u => u.Correo, request.correo);
        var usuario = await _mongoRepositorio.FindAsync(filtro);
        return usuario.FirstOrDefault();

    }
}