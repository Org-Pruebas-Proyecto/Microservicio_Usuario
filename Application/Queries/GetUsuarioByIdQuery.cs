using Domain.Entities;
using MediatR;

namespace Application.Queries;

public record GetUsuarioByIdQuery(Guid UsuarioId) : IRequest<UsuarioMongo>;