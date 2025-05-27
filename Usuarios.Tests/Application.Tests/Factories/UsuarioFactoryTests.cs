using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Application.Factories;
using Domain.Entities;


namespace Usuarios.Tests.Application.Tests.Factories;

public class UsuarioFactoryTests
{
    private readonly UsuarioFactory _factory;

    public UsuarioFactoryTests()
    {
        _factory = new UsuarioFactory();
    }

    [Fact]
    public void CrearUsuario_DeberiaRetornarUsuario_ConDatosCorrectos()
    {
        var usuario = _factory.CrearUsuario("Nombre", "Apellido", "usuario123", "password123", "usuario@test.com", "123456789", "Dirección");

        Assert.NotNull(usuario);
        Assert.Equal("Nombre", usuario.Nombre);
        Assert.Equal("Apellido", usuario.Apellido);
        Assert.Equal("usuario123", usuario.Username);
        Assert.Equal("password123", usuario.Password);
        Assert.Equal("usuario@test.com", usuario.Correo);
        Assert.Equal("123456789", usuario.Telefono);
        Assert.Equal("Dirección", usuario.Direccion);
    }
}