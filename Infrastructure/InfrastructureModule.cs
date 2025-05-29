using Application.Interfaces;
using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.DataBase;
using Infrastructure.EventBus.Connection;
using Infrastructure.EventBus.Consumers;
using Infrastructure.EventBus.Publisher;
using Infrastructure.EventBus.Interfaces;
using Infrastructure.EventBus.Publisher;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace Infrastructure
{
    public static class InfrastructureModule
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            services.AddSingleton<MongoInitializer>();

            // Configuracion RabbitMQ Consumer
            services.AddSingleton<IRabbitMQConnectionFactory>(sp =>
                new RabbitMQConnectionFactory(
                    configuration["RabbitMQ:Host"],
                    configuration["RabbitMQ:Username"],
                    configuration["RabbitMQ:Password"]
                ));

            services.AddSingleton<IRabbitMQMessageProcessor, RabbitMQMessageProcessor>();

            services.AddScoped<IUsuarioEventHandler, UsuarioEventHandler>();
            services.AddHostedService<RabbitMQConsumerService>();

            services.AddSingleton<IEventPublisher, RabbitMQEventPublisher>();

            // Obtener la cadena de conexión correctamente
            var postgresConnection = configuration.GetConnectionString("Postgres");

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(postgresConnection));
            // Configuracion MongoDB
            services.AddSingleton<IMongoDatabase>(sp =>
            {
                var mongoClient = sp.GetRequiredService<IMongoClient>();
                return mongoClient.GetDatabase("usuarios_db");
            });
            services.AddSingleton<IMongoClient>(sp =>
                new MongoClient(configuration.GetConnectionString("MongoDB")));


            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddSingleton<IMongoRepository<UsuarioMongo>, MongoRepository<UsuarioMongo>>(
                sp => new MongoRepository<UsuarioMongo>(
                    sp.GetRequiredService<IMongoClient>(),
                    "usuarios_db",
                    "usuarios"
                )
            );
            services.AddScoped<IActividadRepository, ActividadRepository>();
            services.AddSingleton<IMongoRepository<ActividadMongo>>(sp =>
                new MongoRepository<ActividadMongo>(
                    sp.GetRequiredService<IMongoClient>(),
                    "usuarios_db",
                    "actividades"
                ));

        }
    }
}