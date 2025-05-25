using Application.Commands;
using Application.Interfaces;
using FluentValidation;

namespace Application.Validators;

public class CrearUsuarioValidator : AbstractValidator<CrearUsuarioCommand>
{
    private readonly IUsuarioRepository _repository;
    public CrearUsuarioValidator(IUsuarioRepository repository)
    {
        _repository = repository;
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.");
        RuleFor(x => x.Apellido)
            .NotEmpty().WithMessage("El apellido es obligatorio.");
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("Debe contener al menos una mayúscula")
            .Matches("[a-z]").WithMessage("Debe contener al menos una minúscula")
            .Matches("[0-9]").WithMessage("Debe contener al menos un número");
        RuleFor(x => x.Correo)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("El correo electrónico no es válido.");
        RuleFor(x => x.Telefono)
            .NotEmpty().WithMessage("El teléfono es obligatorio.");
        RuleFor(x => x.Direccion)
            .NotEmpty().WithMessage("La dirección es obligatoria.");
    }   
}