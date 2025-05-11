using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.EventBus.Events;
using Infrastructure.EventBus.Consumers;
using MongoDB.Driver;

namespace Infrastructure
{
    public static class InfrastructureModule
    {
        public static void AddInfrastructure(this IServiceCollection services, string postgresConnection, IConfiguration configuration)
        {
            var rabbitHost = configuration["RabbitMQ:Host"];
            var rabbitUser = configuration["RabbitMQ:Username"];
            var rabbitPass = configuration["RabbitMQ:Password"];

            services.AddSingleton<IEventPublisher>(new RabbitMQEventPublisher(
                rabbitHost,
                rabbitUser,
                rabbitPass
            ));
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(postgresConnection));

            // Configuracion MongoDB
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