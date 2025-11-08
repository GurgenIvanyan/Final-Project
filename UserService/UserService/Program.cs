using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using User.Application.Abstractions.Http;
using User.Application.Abstractions.Security;
using User.Application.Services;
using User.Application.Services.IServices;
using User.Core.Interfaces.Repositories;
using User.Infrastructure.Http;                 // ForwardAuthHeaderHandler, PlaylistGateway, ProblemDetailsHandler
using User.Infrastructure.Persistence;
using User.Infrastructure.Persistence.Repositories;
using User.Infrastructure.Security;
using User.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

// DB
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(cfg.GetConnectionString("Postgres")));

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserPlaylistRepository, UserPlaylistRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserPlaylistService, UserPlaylistService>();

// Current User
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// HTTP → Playlist.Api (проброс Authorization + перехват RFC7807)
builder.Services.AddTransient<ForwardAuthHeaderHandler>();
builder.Services.AddTransient<ProblemDetailsHandler>();

builder.Services.AddHttpClient<IPlaylistGateway, PlaylistGateway>(http =>
{
    var baseUrl = cfg["PlaylistService:BaseUrl"]
        ?? throw new InvalidOperationException("PlaylistService:BaseUrl missing");
    http.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
})
.AddHttpMessageHandler<ForwardAuthHeaderHandler>()
.AddHttpMessageHandler<ProblemDetailsHandler>();

// JWT
var jwt = cfg.GetSection("Jwt").Get<JwtOptions>()
          ?? throw new InvalidOperationException("Jwt missing");
builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey))
        };
    });

builder.Services.AddAuthorization();

// MVC + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "User Service", Version = "v1" });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter **Bearer {token}**",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };

    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { scheme, Array.Empty<string>() } });
});

var app = builder.Build();

// === Apply EF Core migrations on startup ===
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//    db.Database.Migrate();
//}

// Pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Service v1");
    c.RoutePrefix = "";
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
