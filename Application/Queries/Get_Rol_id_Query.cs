using Domain.ValueObjects;
using MediatR;

namespace Application.Queries;

public record Get_Rol_id_Query(Guid rol_id): IRequest<Rol_Mongo>;