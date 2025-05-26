using Application.Interfaces;
using Application.Queries;
using Domain.Entities;
using MediatR;

namespace Application.Handlers;

public class GetUsuarioByIdQueryHandler : IRequestHandler<GetUsuarioByIdQuery, UsuarioMongo>
{
    private readonly IMongoUsuarioRepository<UsuarioMongo> _mongoRepository;

    public GetUsuarioByIdQueryHandler(IMongoUsuarioRepository<UsuarioMongo> mongoRepository)
    {
        _mongoRepository = mongoRepository;
    }

    public async Task<UsuarioMongo> Handle(GetUsuarioByIdQuery request, CancellationToken cancellationToken)
    {
        return await _mongoRepository.GetByIdAsync(request.UsuarioId.ToString());
    }
}