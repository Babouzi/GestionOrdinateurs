using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.API.Services;
using GestionOrdinateurs.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GestionOrdinateurs", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Entrez : Bearer {votre_token}"
    });
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["Keycloak:Authority"]
        ?? "http://localhost:8180/realms/GestionOrdinateurs";
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = false,
        ValidateIssuer = true,
        ValidIssuers = new[]
        {
            "http://localhost:8180/realms/GestionOrdinateurs",
            "http://keycloak:8080/realms/GestionOrdinateurs"
        },
        RoleClaimType = ClaimTypes.Role
    };
    options.MapInboundClaims = false;
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var realmAccess = context.Principal?.FindFirst("realm_access")?.Value;
            if (realmAccess is not null
                && context.Principal?.Identity is ClaimsIdentity identity)
            {
                using var document = JsonDocument.Parse(realmAccess);
                if (document.RootElement.TryGetProperty("roles", out var roles)
                    && roles.ValueKind == JsonValueKind.Array)
                {
                    foreach (var role in roles.EnumerateArray())
                    {
                        if (role.ValueKind == JsonValueKind.String && role.GetString() is { } roleName)
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }
    };
});

/*
Ancienne validation avec clé symétrique locale, remplacée par l'autorité Keycloak :
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});
*/

builder.Services.AddAuthorization();
builder.Services.AddDbContext<ParcInformatiqueContext>();
builder.Services.AddScoped<IOrdinateurService, OrdinateurService>();
builder.Services.AddScoped<IReparationService, ReparationService>();
builder.Services.AddSingleton<StockService>();

//CORS pour autoriser Blazor
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("http://localhost:5000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowBlazorClient");

// Initialisation de la BDD (Création des users si vide)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ParcInformatiqueContext>();
    db.Database.Migrate();

    if (!db.Users.Any())
    {
        db.Users.AddRange(
            new User { Login = "user1", Role = RoleEnum.Admin },
            new User { Login = "user2", Role = RoleEnum.Gestionnaire },
            new User { Login = "user3", Role = RoleEnum.Gestionnaire }
        );
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middlewares de sécurité 
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();