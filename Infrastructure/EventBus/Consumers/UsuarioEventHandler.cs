using System.Threading.Tasks;
using Domain.Entities;
using Domain.Events;
using Domain.ValueObjects;
using Infrastructure.EventBus.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Infrastructure.EventBus.Consumers;

public class UsuarioEventHandler : IUsuarioEventHandler
{
    private readonly IMongoClient _mongoClient;

    public UsuarioEventHandler(IMongoClient mongoClient)
    {
        _mongoClient = mongoClient;
    }

    private IMongoCollection<UsuarioMongo> Usuarios =>
        _mongoClient.GetDatabase("usuarios_db").GetCollection<UsuarioMongo>("usuarios");

    private IMongoCollection<ActividadMongo> Actividades =>
        _mongoClient.GetDatabase("usuarios_db").GetCollection<ActividadMongo>("actividades");

    public async Task HandleUsuarioRegistradoAsync(UsuarioCreadoEvent evento)
    {

        Console.WriteLine("👤 Procesando creación de usuario en MongoDB...");

        var usuario = new UsuarioMongo
        {
            Id = evento.Id,
            Nombre = evento.Nombre,
            Apellido = evento.Apellido,
            Username = evento.Username,
            Password = evento.Password,
            Telefono = evento.Telefono,
            Correo = evento.Correo,
            Direccion = evento.Direccion,
        };
        Console.WriteLine($"➡️ Insertando usuario: {usuario.Id}, {usuario.Nombre}, {usuario.Correo}");
        await Usuarios.InsertOneAsync(usuario);
    }

    public async Task HandleUsuarioConfirmadoAsync(UsuarioConfirmadoEvent evento)
    {
        var filter = Builders<UsuarioMongo>.Filter.Eq(u => u.Id, evento.UsuarioId);
        var update = Builders<UsuarioMongo>.Update.Set(u => u.Verificado, evento.Confirmado);
        await Usuarios.UpdateOneAsync(filter, update);
    }

    public async Task HandleUsuarioPasswordCambiadoAsync(UsuarioPasswordCambiadoEvent evento)
    {
        var filter = Builders<UsuarioMongo>.Filter.Eq(u => u.Id, evento.UsuarioId);
        var update = Builders<UsuarioMongo>.Update.Set(u => u.Password, evento.Password);
        await Usuarios.UpdateOneAsync(filter, update);
    }

    public async Task HandlePerfilActualizadoAsync(PerfilActualizadoEvent evento)
    {
        var filter = Builders<UsuarioMongo>.Filter.Eq(u => u.Id, evento.UsuarioId);
        var update = Builders<UsuarioMongo>.Update
            .Set(u => u.Nombre, evento.Nombre)
            .Set(u => u.Apellido, evento.Apellido)
            .Set(u => u.Correo, evento.Correo)
            .Set(u => u.Telefono, evento.Telefono)
            .Set(u => u.Direccion, evento.Direccion);

        await Usuarios.UpdateOneAsync(filter, update);
    }

    public async Task HandleActividadRegistradaAsync(ActividadRegistradaEvent evento)
    {
        var actividad = new ActividadMongo
        {
            Id = evento.ActividadId,
            UsuarioId = evento.UsuarioId,
            TipoAccion = evento.TipoAccion,
            Detalles = evento.Detalles,
            Fecha = evento.Fecha
        };
        await Actividades.InsertOneAsync(actividad);
    }
}
