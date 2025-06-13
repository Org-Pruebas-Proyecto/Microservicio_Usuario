using Application;
using Infrastructure;
using Infrastructure.DataBase;
using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);


// Configurar servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Microservicio Usuario", Version = "v1" });
});

// Configurar modulos
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Configuración de autenticación
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:auth-server-url"];
        options.Audience = builder.Configuration["Keycloak:resource"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true
        };

        // Solo para desarrollo
        if (builder.Environment.IsDevelopment())
        {
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters.ValidateIssuer = false;
        }
    });

// Configurar autorización
builder.Services.AddAuthorization( options =>
{
    options.AddPolicy("Requiere_Administrador", policy => policy.RequireRealmRoles("Administrador"));
    options.AddPolicy("Requiere_Postor", policy => policy.RequireRealmRoles("Postor"));
    options.AddPolicy("Requiere_Subastador", policy => policy.RequireRealmRoles("Subastador"));
    options.AddPolicy("Requiere_Soporte", policy => policy.RequireRealmRoles("Soporte"));
});



var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Usuario API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Inicializacion de MongoDB
using (var scope = app.Services.CreateScope())
{
    var mongoInitializer = scope.ServiceProvider.GetRequiredService<MongoInitializer>();
    mongoInitializer.Initialize();
}

app.Run();