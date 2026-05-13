using System.Text;
using Amazon.S3;
using LegalDoc.Core.Interfaces;
using LegalDoc.Infrastructure.Repositories;
using LegalDoc.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("OracleConnection")
    ?? throw new InvalidOperationException("Connection string 'OracleConnection' not found.");

// Repository registrations
builder.Services.AddScoped<IUserRepository>(sp =>
    new UserRepository(connectionString, sp.GetRequiredService<ILogger<UserRepository>>()));

builder.Services.AddScoped<IRefreshTokenRepository>(sp =>
    new RefreshTokenRepository(connectionString, sp.GetRequiredService<ILogger<RefreshTokenRepository>>()));

builder.Services.AddScoped<IClientRepository>(sp =>
    new ClientRepository(connectionString, sp.GetRequiredService<ILogger<ClientRepository>>()));

builder.Services.AddScoped<ITemplateRepository>(sp =>
    new TemplateRepository(connectionString, sp.GetRequiredService<ILogger<TemplateRepository>>()));

builder.Services.AddScoped<IContractRepository>(sp =>
    new ContractRepository(connectionString, sp.GetRequiredService<ILogger<ContractRepository>>()));

builder.Services.AddScoped<IAuditLogRepository>(sp =>
    new AuditLogRepository(connectionString, sp.GetRequiredService<ILogger<AuditLogRepository>>()));

// Authentication services
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<ITokenService>(sp => sp.GetRequiredService<JwtTokenService>());
builder.Services.AddScoped<IAuthService, AuthService>();

// Background services
builder.Services.AddHostedService<RefreshTokenCleanupService>();

// AWS S3
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddScoped<IS3Service, S3Service>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();

// Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter JWT token"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("ADMIN"));
    options.AddPolicy("RequireLawyer", policy => policy.RequireRole("LAWYER", "ADMIN"));
    options.AddPolicy("RequireAnyUser", policy => policy.RequireAuthenticatedUser());
});

// CORS for Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173", 
            "http://localhost:3000", 
            "http://127.0.0.1:5500",
            "https://localhost:5173",
            "https://localhost:3000"
        )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS must come before Authentication
app.UseCors("FrontendPolicy");

// Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
