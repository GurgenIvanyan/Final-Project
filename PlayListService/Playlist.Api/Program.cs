using System.Text;
using Application.Services;
using Application.Services.IServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Playlist.Api.Application.Mapping;               
using Playlist.Api.Application.Services;              
using Playlist.Api.Core.Interfaces.Repositories;
using Playlist.Api.Infrastructure.Caching;
using Playlist.Api.Infrastructure.Persistence;
using Playlist.Api.Infrastructure.Persistence.Repositories;
using Playlist.Api.Infrastructure.Security;
using Playlist.Api.Web.Middleware;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

// DB
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(cfg.GetConnectionString("Postgres")));

// Redis
builder.Services.AddStackExchangeRedisCache(o =>
{
    o.Configuration = cfg["Redis:Connection"];
});
builder.Services.AddSingleton<RedisCacheService>();

// Repositories
builder.Services.AddScoped<IArtistRepository, ArtistRepository>();
builder.Services.AddScoped<ISongRepository, SongRepository>();
builder.Services.AddScoped<IPlaylistRepository, PlaylistRepository>();
builder.Services.AddScoped<IVoteRepository, VoteRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISongLikeRepository, SongLikeRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application services
builder.Services.AddScoped<IArtistService, ArtistService>();
builder.Services.AddScoped<ISongService, SongService>();
builder.Services.AddScoped<IPlaylistService, PlaylistService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISongLikeService, SongLikeService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// JWT
var jwt = cfg.GetSection("Jwt").Get<JwtOptions>()
          ?? throw new InvalidOperationException("Jwt section missing.");
builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton<JwtTokenService>();

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

// Web + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Playlist API", Version = "v1" });

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

builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(p =>
        p.AllowAnyOrigin()
         .AllowAnyHeader()
         .AllowAnyMethod());
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
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Playlist API v1");
    c.RoutePrefix = "";
});

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
