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

public class CambiarPasswordValidatorTests
{
    private readonly CambiarPasswordValidator _validator;

    public CambiarPasswordValidatorTests()
    {
        _validator = new CambiarPasswordValidator();
    }

    /// ✅ **Caso base: Contraseña válida cumple todas las reglas**
    [Fact]
    public void Validacion_DeberiaSerExitosa_CuandoPasswordEsValida()
    {
        var command = new CambiarPasswordCommand(Guid.NewGuid(), "passwordActual", "P@ssw0rd!");

        var resultado = _validator.TestValidate(command);

        resultado.ShouldNotHaveValidationErrorFor(x => x.NuevoPassword);
    }

    /// ❌ **Error: Contraseña demasiado corta**
    [Fact]
    public void Validacion_DeberiaFallar_CuandoPasswordEsMenorA8Caracteres()
    {
        var command = new CambiarPasswordCommand(Guid.NewGuid(), "passwordActual", "P@ss1");

        var resultado = _validator.TestValidate(command);

        resultado.ShouldHaveValidationErrorFor(x => x.NuevoPassword);
    }

    /// ❌ **Error: Falta una mayúscula**
    [Fact]
    public void Validacion_DeberiaFallar_CuandoPasswordNoContieneMayusculas()
    {
        var command = new CambiarPasswordCommand(Guid.NewGuid(), "passwordActual", "p@ssw0rd");

        var resultado = _validator.TestValidate(command);

        resultado.ShouldHaveValidationErrorFor(x => x.NuevoPassword)
            .WithErrorMessage("Debe contener al menos una mayúscula");
    }

    /// ❌ **Error: Falta una minúscula**
    [Fact]
    public void Validacion_DeberiaFallar_CuandoPasswordNoContieneMinusculas()
    {
        var command = new CambiarPasswordCommand(Guid.NewGuid(), "passwordActual", "P@SSW0RD");

        var resultado = _validator.TestValidate(command);

        resultado.ShouldHaveValidationErrorFor(x => x.NuevoPassword)
            .WithErrorMessage("Debe contener al menos una minúscula");
    }

    /// ❌ **Error: Falta un número**
    [Fact]
    public void Validacion_DeberiaFallar_CuandoPasswordNoContieneNumeros()
    {
        var command = new CambiarPasswordCommand(Guid.NewGuid(), "passwordActual", "P@ssword!");

        var resultado = _validator.TestValidate(command);

        resultado.ShouldHaveValidationErrorFor(x => x.NuevoPassword)
            .WithErrorMessage("Debe contener al menos un número");
    }

    /// ❌ **Error: Falta un carácter especial**
    [Fact]
    public void Validacion_DeberiaFallar_CuandoPasswordNoContieneCaracterEspecial()
    {
        var command = new CambiarPasswordCommand(Guid.NewGuid(), "passwordActual", "Passw0rd");

        var resultado = _validator.TestValidate(command);

        resultado.ShouldHaveValidationErrorFor(x => x.NuevoPassword)
            .WithErrorMessage("Debe contener al menos un carácter especial");
    }
}