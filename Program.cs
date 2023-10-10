using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TokenJwt;
using TokenJwt.Extension;
using TokenJwt.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureCors();
// * Configuracion del Swagger
builder.Services.AddSwaggerGen(c =>
{
    // * Configuracion y documentacion de una version de vuestra API en Swagger
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "JWT - Manual", Version = "v1" });

    // * Agrega la descripcion de seguridad para nuestro JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Bearer token for authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    // * Agrega un requerimiento de seguridad para indicar que se necesita en el JWT
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// * Configuracion y registro de un contexto en la base de datos PostgreSQL
builder.Services.AddDbContext<DbAppContext>(options =>
{
    options.UseNpgsql("Host=localhost;Database=TokenJwt;Username=postgres;Password=1122809631");
});

// * Registros de servicios en una coleccion de servicios
builder.Services.AddScoped<IAutorizacionService, AutorizacionService>();

// * Obtencion de un valor de una clave especifica en configuracion
var key = builder.Configuration.GetValue<string>("JwtSettings:Key");
// * Conversion a una cadena de caracteres
var keyBytes = Encoding.ASCII.GetBytes(key);

// * Configuracion por defecto de autenticacion basada en JWT
builder.Services.AddAuthentication(config => 
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(config => 
{
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;
    config.TokenValidationParameters = new TokenValidationParameters{
        ValidateIssuerSigningKey = true,
        // * Dato relevante: obtencion del token para la firma
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    // * Configura y habilita la interfaz de usuario de Swagger
    app.UseSwaggerUI( c => 
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "TokenJwt V1");
        }   
    );
}

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

// * Se jabilita y se usa el middleware de autenticacion
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();