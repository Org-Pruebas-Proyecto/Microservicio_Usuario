using Domain.ValueObjects;

namespace Application.Interfaces;

public interface IActividadRepository
{
    Task RegistrarActividad(Actividad actividad);
}