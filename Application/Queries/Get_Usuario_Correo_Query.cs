using Domain.Entities;
using MediatR;

namespace Application.Queries;

public record Get_Usuario_Correo_Query(string correo): IRequest<UsuarioMongo>;