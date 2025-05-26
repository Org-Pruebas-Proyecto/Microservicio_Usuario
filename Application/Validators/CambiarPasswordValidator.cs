using Application.Commands;
using FluentValidation;

namespace Application.Validators;

public class CambiarPasswordValidator : AbstractValidator<CambiarPasswordCommand>
{
    public CambiarPasswordValidator()
    {
        RuleFor(x => x.NuevoPassword)
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Debe contener al menos una mayúscula")
            .Matches("[a-z]").WithMessage("Debe contener al menos una minúscula")
            .Matches("[0-9]").WithMessage("Debe contener al menos un número")
            .Matches("[^a-zA-Z0-9]").WithMessage("Debe contener al menos un carácter especial");
    }
}