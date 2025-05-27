using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Application.Validators;
using Application.Commands;
using FluentValidation.TestHelper;

namespace Usuarios.Tests.Application.Tests.Validators;

public class ActualizarPerfilValidatorTests
{
    private readonly ActualizarPerfilValidator _validator;

    public ActualizarPerfilValidatorTests()
    {
        _validator = new ActualizarPerfilValidator();
    }

    /// ✅ **Caso base: Datos válidos pasan la validación**
    [Fact]
    public void Validacion_DeberiaSerExitosa_CuandoDatosSonValidos()
    {
        var command = new ActualizarPerfilCommand(Guid.NewGuid(), "Nombre", "Apellido", "usuario@test.com", "+1234567890", "Dirección válida");

        var resultado = _validator.TestValidate(command);

        resultado.ShouldNotHaveValidationErrorFor(x => x.Nombre);
        resultado.ShouldNotHaveValidationErrorFor(x => x.Telefono);
        resultado.ShouldNotHaveValidationErrorFor(x => x.Direccion);
    }

    /// ❌ **Error: Nombre vacío**
    [Fact]
    public void Validacion_DeberiaFallar_CuandoNombreEstaVacio()
    {
        var command = new ActualizarPerfilCommand(Guid.NewGuid(), "", "Apellido", "usuario@test.com", "+1234567890", "Dirección válida");

        var resultado = _validator.TestValidate(command);

        resultado.ShouldHaveValidationErrorFor(x => x.Nombre)
            .WithErrorMessage("El nombre es requerido");
    }

    /// ❌ **Error: Nombre excede el límite de caracteres**
    [Fact]
    public void Validacion_DeberiaFallar_CuandoNombreSuperaLongitudMaxima()
    {
        var command = new ActualizarPerfilCommand(Guid.NewGuid(), new string('A', 101), "Apellido", "usuario@test.com", "+1234567890", "Dirección válida");

        var resultado = _validator.TestValidate(command);

        resultado.ShouldHaveValidationErrorFor(x => x.Nombre);
    }

    /// ❌ **Error: Formato de teléfono inválido**
    [Fact]
    public void Validacion_DeberiaFallar_CuandoTelefonoTieneFormatoIncorrecto()
    {
        var command = new ActualizarPerfilCommand(Guid.NewGuid(), "Nombre", "Apellido", "usuario@test.com", "telefonoInvalido", "Dirección válida");

        var resultado = _validator.TestValidate(command);

        resultado.ShouldHaveValidationErrorFor(x => x.Telefono)
            .WithErrorMessage("Formato de teléfono inválido");
    }

    /// ❌ **Error: Dirección supera la longitud máxima**
    [Fact]
    public void Validacion_DeberiaFallar_CuandoDireccionSuperaLongitudMaxima()
    {
        var command = new ActualizarPerfilCommand(Guid.NewGuid(), "Nombre", "Apellido", "usuario@test.com", "+1234567890", new string('D', 201));

        var resultado = _validator.TestValidate(command);

        resultado.ShouldHaveValidationErrorFor(x => x.Direccion);
    }
}
