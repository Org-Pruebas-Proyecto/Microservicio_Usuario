using Application.Commands;
using FluentValidation;

namespace Application.Validators;

public class ActualizarPerfilValidator : AbstractValidator<ActualizarPerfilCommand>
{
    public ActualizarPerfilValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100);

        RuleFor(x => x.Telefono)
            .Matches(@"^\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d$")
            .WithMessage("Formato de teléfono inválido");

        RuleFor(x => x.Direccion)
            .MaximumLength(200);
    }
}