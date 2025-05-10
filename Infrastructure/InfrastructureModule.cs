using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;

namespace Infrastructure
{
    public static class InfrastructureModule
    {
        public static void AddInfrastructure(this IServiceCollection services, string postgresConnection)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(postgresConnection));

            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        }
    }
}