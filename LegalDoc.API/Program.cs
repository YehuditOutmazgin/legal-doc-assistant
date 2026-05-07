using LegalDoc.Core.Interfaces;
using LegalDoc.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("OracleConnection")
    ?? throw new InvalidOperationException("Connection string 'OracleConnection' not found.");

builder.Services.AddScoped<IUserRepository>(sp =>
    new UserRepository(connectionString, sp.GetRequiredService<ILogger<UserRepository>>()));

builder.Services.AddScoped<IClientRepository>(sp =>
    new ClientRepository(connectionString, sp.GetRequiredService<ILogger<ClientRepository>>()));

builder.Services.AddScoped<ITemplateRepository>(sp =>
    new TemplateRepository(connectionString, sp.GetRequiredService<ILogger<TemplateRepository>>()));

builder.Services.AddScoped<IContractRepository>(sp =>
    new ContractRepository(connectionString, sp.GetRequiredService<ILogger<ContractRepository>>()));

builder.Services.AddScoped<IAuditLogRepository>(sp =>
    new AuditLogRepository(connectionString, sp.GetRequiredService<ILogger<AuditLogRepository>>()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
