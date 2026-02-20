using System.Text;
using FractPal.Data;
using FractPal.Model.Entities;
using FractPal.Service.Implementation;
using FractPal.Service.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// OpenAPI / Scalar
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info.Title = "FractPal API";
        document.Info.Version = "v1";
        document.Info.Description = "A social media for fractals";
        return Task.CompletedTask;
    });

    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");

builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseSqlServer(connectionString)
);

// Identity configuration
builder.Services.AddIdentity<FractPalUser, IdentityRole<Guid>>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// JWT
var secretKey = builder.Configuration["JWT_SECRET_KEY"]
    ?? builder.Configuration["JwtSettings:SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey not configured. Set JWT_SECRET_KEY environment variable.");

var issuer = builder.Configuration["JWT_ISSUER"]
    ?? builder.Configuration["JwtSettings:Issuer"]
    ?? "FractPal";

var audience = builder.Configuration["JWT_AUDIENCE"]
    ?? builder.Configuration["JwtSettings:Audience"]
    ?? "FractPal";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = issuer,
    ValidAudience = audience,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
});

// CORS
builder.Services.AddCors(
    options => options.AddDefaultPolicy(
        policy => policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()));

// Services
builder.Services.AddScoped(typeof(FractPal.Data.IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IFractalService, FractalService>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/api/swagger/v1.json");
    app.MapScalarApiReference(options =>
    {
        options.Title = "FractPal API";
        options.AddHttpAuthentication("Bearer", auth => auth.Token = "");
    });
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

internal sealed class BearerSecuritySchemeTransformer(
    IAuthenticationSchemeProvider authenticationSchemeProvider
) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken ct)
    {
        var schemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (!schemes.Any(s => s.Name == "Bearer"))
        {
            return;
        }

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>(StringComparer.Ordinal);
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token."
        };

        foreach (var path in document.Paths.Values)
        {
            foreach (var operation in path.Operations.Values)
            {
                operation.Security ??= [];
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
            }
        }
    }
}
