using Domain.ValueObjects;
using MediatR;

namespace Application.Queries;

public record ObtenerHistorialQuery(
    Guid UsuarioId,
    string? TipoAccion,
    DateTime? Desde,
    DateTime? Hasta
) : IRequest<IEnumerable<ActividadMongo>>;