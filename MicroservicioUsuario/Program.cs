using Application;
using Infrastructure;
using Infrastructure.DataBase;

var builder = WebApplication.CreateBuilder(args);

// Obtener la conexi�n a PostgreSQL desde la configuraci�n
var postgresConnection = builder.Configuration.GetConnectionString("PostgresConnection");

// Configurar m�dulos
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Configurar MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationModule).Assembly));

// Configurar controladores
builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var mongoInitializer = scope.ServiceProvider.GetRequiredService<MongoInitializer>();
    mongoInitializer.Initialize();
}


app.MapControllers();
app.Run();