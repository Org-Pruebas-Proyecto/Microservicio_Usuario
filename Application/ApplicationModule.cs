using System.Reflection;
using Application.Factories;
using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class ApplicationModule
    {
        public static void AddApplication(this IServiceCollection services)
        {
            // Register MediatR services
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Register IUsuarioFactory as a scoped service
            services.AddScoped<IUsuarioFactory, UsuarioFactory>();
        }
    }
}