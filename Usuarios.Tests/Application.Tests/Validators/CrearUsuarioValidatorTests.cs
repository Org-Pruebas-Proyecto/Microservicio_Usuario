using Xunit;
using Application.Validators;
using Application.Commands;
using FluentValidation.TestHelper;

namespace Usuarios.Tests.Application.Tests.Validators;
public class CrearUsuarioValidatorTests
{
    private readonly CrearUsuarioValidator _validator;

    public CrearUsuarioValidatorTests()
    {
        _validator = new CrearUsuarioValidator(null); // Se puede usar `null` porque no se usa el repositorio en la validación
    }

    /// ✅ **Caso base: Validación exitosa con datos correctos**
    [Fact]
    public void Validacion_DeberiaSerExitosa_CuandoDatosSonValidos()
    {
        var command = new CrearUsuarioCommand("Nombre", "Apellido", "usuario123", "P@ssw0rd!", "usuario@test.com", "123456789", "Dirección");

        var resultado = _validator.TestValidate(command);

        resultado.ShouldNotHaveValidationErrorFor(x => x.Nombre);
        resultado.ShouldNotHaveValidationErrorFor(x => x.Apellido);
        resultado.ShouldNotHaveValidationErrorFor(x => x.Username);
        resultado.ShouldNotHaveValidationErrorFor(x => x.Password);
        resultado.ShouldNotHaveValidationErrorFor(x => x.Correo);
        resultado.ShouldNotHaveValidationErrorFor(x => x.Telefono);
        resultado.ShouldNotHaveValidationErrorFor(x => x.Direccion);
    }

    /// ❌ **Error: Nombre vacío**
    [Fact]
    public void Validacion_DeberiaFallar_CuandoNombreEstaVacio()
    {
        var command = new CrearUsuarioCommand("", "Apellido", "usuario123", "P@ssw0rd!", "usuario@test.com", "123456789", "Dirección");

        var resultado = _validator.TestValidate(command);

        resultado.ShouldHaveValidationErrorFor(x => x.Nombre)
            .WithErrorMessage("El nombre es obligatorio.");
    }

    /// ❌ **Error: Contraseña demasiado corta**
    [Fact]
    public void Validacion_DeberiaFallar_CuandoPasswordEsMenorA8Caracteres()
    {
        var command = new CrearUsuarioCommand("Nombre", "Apellido", "usuario123", "Pass1!", "usuario@test.com", "123456789", "Dirección");

        var resultado = _validator.TestValidate(command);

        resultado.ShouldHaveValidationErrorFor(x => x.Password);
    }

    /// ❌ **Error: Falta una mayúscula**
    [Fact]
    public void Validacion_DeberiaFallar_CuandoPasswordNoContieneMayusculas()
    {
        var command = new CrearUsuarioCommand("Nombre", "Apellido", "usuario123", "p@ssw0rd", "usuario@test.com", "123456789", "Dirección");

        var resultado = _validator.TestValidate(command);

        resultado.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Debe contener al menos una mayúscula");
    }

    /// ❌ **Error: Correo electrónico inválido**
    [Fact]
    public void Validacion_DeberiaFallar_CuandoCorreoEsInvalido()
    {
        var command = new CrearUsuarioCommand("Nombre", "Apellido", "usuario123", "P@ssw0rd!", "usuario", "123456789", "Dirección");

        var resultado = _validator.TestValidate(command);

        resultado.ShouldHaveValidationErrorFor(x => x.Correo)
            .WithErrorMessage("El correo electrónico no es válido.");
    }
}