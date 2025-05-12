using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.EventBus.Events;
using Infrastructure.EventBus.Consumers;
using MongoDB.Driver;
using Infrastructure.DataBase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure
{
    public static class InfrastructureModule
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<MongoInitializer>();
            services.AddSingleton<IEventPublisher, RabbitMQEventPublisher>(sp =>
            {
                var mongoInitializer = sp.GetRequiredService<MongoInitializer>();
                mongoInitializer.Initialize();
                return new RabbitMQEventPublisher(
                    configuration["RabbitMQ:Host"],
                    configuration["RabbitMQ:Username"],
                    configuration["RabbitMQ:Password"]
                );
            });
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

            // Configuracion RabbitMQ Consumer
            services.AddSingleton<IHostedService>(sp =>
                (IHostedService)new RabbitMQConsumerService( 
                    configuration["RabbitMQ:Host"],
                    configuration["RabbitMQ:Username"],
                    configuration["RabbitMQ:Password"],
                    sp.GetRequiredService<IServiceProvider>()
                ));

            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        }
    }
}