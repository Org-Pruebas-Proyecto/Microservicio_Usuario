using System;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Events;
using Infrastructure.EventBus.Consumers;
using Infrastructure.EventBus.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Usuarios.Tests.Infrastructure.Tests.EventBus.Consumers;
public class RabbitMQMessageProcessorTests
{
    private Mock<IUsuarioEventHandler> SetupHandler(IServiceCollection services)
    {
        var handlerMock = new Mock<IUsuarioEventHandler>();
        services.AddSingleton(handlerMock.Object);
        return handlerMock;
    }

    private IRabbitMQMessageProcessor BuildProcessor(out Mock<IUsuarioEventHandler> handlerMock)
    {
        var services = new ServiceCollection();
        handlerMock = SetupHandler(services);

        var providerMock = new Mock<IServiceProvider>();
        var scopeMock = new Mock<IServiceScope>();
        var scopeFactoryMock = new Mock<IServiceScopeFactory>();

        var realProvider = services.BuildServiceProvider();

        scopeMock.Setup(s => s.ServiceProvider).Returns(realProvider);
        scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);
        providerMock.Setup(p => p.GetService(typeof(IServiceScopeFactory))).Returns(scopeFactoryMock.Object);

        return new RabbitMQMessageProcessor(providerMock.Object);
    }

    [Theory]
    [InlineData("UsuarioRegistradoEvent")]
    [InlineData("UsuarioCreadoEvent")]
    public async Task ProcessMessageAsync_ShouldHandleUsuarioCreado(string eventType)
    {
        var processor = BuildProcessor(out var handlerMock);
        var message = JsonSerializer.Serialize(new
        {
            Type = eventType,
            Id = Guid.NewGuid(),
            Nombre = "Test User"
        });

        await processor.ProcessMessageAsync(message);

        handlerMock.Verify(h => h.HandleUsuarioRegistradoAsync(It.IsAny<UsuarioCreadoEvent>()), Times.Once);
    }

    [Fact]
    public async Task ProcessMessageAsync_ShouldHandleUsuarioConfirmado()
    {
        var processor = BuildProcessor(out var handlerMock);
        var message = JsonSerializer.Serialize(new
        {
            Type = "UsuarioConfirmadoEvent",
            Id = Guid.NewGuid()
        });

        await processor.ProcessMessageAsync(message);

        handlerMock.Verify(h => h.HandleUsuarioConfirmadoAsync(It.IsAny<UsuarioConfirmadoEvent>()), Times.Once);
    }

    [Fact]
    public async Task ProcessMessageAsync_ShouldHandleUsuarioPasswordCambiado()
    {
        var processor = BuildProcessor(out var handlerMock);
        var message = JsonSerializer.Serialize(new
        {
            Type = "UsuarioPasswordCambiadoEvent",
            Id = Guid.NewGuid(),
            NuevaPassword = "123456"
        });

        await processor.ProcessMessageAsync(message);

        handlerMock.Verify(h => h.HandleUsuarioPasswordCambiadoAsync(It.IsAny<UsuarioPasswordCambiadoEvent>()), Times.Once);
    }

    [Fact]
    public async Task ProcessMessageAsync_ShouldHandlePerfilActualizado()
    {
        var processor = BuildProcessor(out var handlerMock);
        var message = JsonSerializer.Serialize(new
        {
            Type = "PerfilActualizadoEvent",
            UserId = Guid.NewGuid(),
            Nombre = "Nuevo Nombre"
        });

        await processor.ProcessMessageAsync(message);

        handlerMock.Verify(h => h.HandlePerfilActualizadoAsync(It.IsAny<PerfilActualizadoEvent>()), Times.Once);
    }

    [Fact]
    public async Task ProcessMessageAsync_ShouldHandleActividadRegistrada()
    {
        var processor = BuildProcessor(out var handlerMock);
        var message = JsonSerializer.Serialize(new
        {
            Type = "ActividadRegistradaEvent",
            UserId = Guid.NewGuid(),
            Actividad = "Login"
        });

        await processor.ProcessMessageAsync(message);

        handlerMock.Verify(h => h.HandleActividadRegistradaAsync(It.IsAny<ActividadRegistradaEvent>()), Times.Once);
    }

    [Fact]
    public async Task ProcessMessageAsync_ShouldHandleMissingType()
    {
        var processor = BuildProcessor(out var handlerMock);
        var message = JsonSerializer.Serialize(new
        {
            SinTipo = "Oops"
        });

        var ex = await Record.ExceptionAsync(() => processor.ProcessMessageAsync(message));

        handlerMock.VerifyNoOtherCalls();
        Assert.Null(ex); // Debe manejar la excepción internamente
    }

    [Fact]
    public async Task ProcessMessageAsync_ShouldHandleUnknownType()
    {
        var processor = BuildProcessor(out var handlerMock);
        var message = JsonSerializer.Serialize(new
        {
            Type = "EventoDesconocido"
        });

        var ex = await Record.ExceptionAsync(() => processor.ProcessMessageAsync(message));

        handlerMock.VerifyNoOtherCalls();
        Assert.Null(ex); // Debe manejar la excepción internamente
    }
}
