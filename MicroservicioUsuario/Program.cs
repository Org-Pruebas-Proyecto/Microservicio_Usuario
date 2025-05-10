using Application;
using Infrastructure;
using Web;

var builder = WebApplication.CreateBuilder(args);

// Configurar módulos
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("Postgres"));

// Configurar MediatR
builder.Services.AddMediatR(typeof(ApplicationModule).Assembly);

// Configurar controladores
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.Run();