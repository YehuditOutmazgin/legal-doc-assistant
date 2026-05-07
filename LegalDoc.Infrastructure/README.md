# LegalDoc.Infrastructure

Data access layer for LegalDoc using ODP.NET (Oracle Data Provider for .NET).

## Structure

```
LegalDoc.Infrastructure/
├── Data/
│   └── BaseRepository.cs          # Base class with common DB operations
├── Repositories/
│   ├── UserRepository.cs          # User data access
│   ├── ClientRepository.cs        # Client data access
│   ├── TemplateRepository.cs      # Template data access
│   ├── ContractRepository.cs      # Contract data access
│   └── AuditLogRepository.cs      # Audit log data access
└── README.md
```

## BaseRepository

Provides common database operations:

- `ExecuteScalarAsync<T>` - Execute query and return single value
- `ExecuteNonQueryAsync` - Execute INSERT/UPDATE/DELETE
- `ExecuteReaderAsync<T>` - Execute query and return list of objects
- `ExecuteReaderSingleAsync<T>` - Execute query and return single object
- `CreateParameter` - Create parameterized query parameter

## Repositories

Each repository implements its corresponding interface from `LegalDoc.Core.Interfaces`:

### UserRepository

- `GetByIdAsync(int id)` - Get user by ID
- `GetAllAsync()` - Get all users
- `GetByEmailAsync(string email)` - Get user by email
- `GetByRoleAsync(UserRole role)` - Get users by role
- `CreateAsync(User user)` - Create new user
- `UpdateAsync(User user)` - Update existing user
- `DeleteAsync(int id)` - Delete user
- `ExistsAsync(int id)` - Check if user exists
- `EmailExistsAsync(string email)` - Check if email exists

### ClientRepository

- `GetByIdAsync(int id)` - Get client by ID
- `GetAllAsync()` - Get all clients
- `GetByTypeAsync(ClientType type)` - Get clients by type
- `GetByEmailAsync(string email)` - Get client by email
- `CreateAsync(Client client)` - Create new client
- `UpdateAsync(Client client)` - Update existing client
- `DeleteAsync(int id)` - Delete client
- `ExistsAsync(int id)` - Check if client exists
- `EmailExistsAsync(string email)` - Check if email exists

### TemplateRepository

- `GetByIdAsync(int id)` - Get template by ID
- `GetAllAsync()` - Get all templates
- `GetActivesAsync()` - Get active templates only
- `GetByCategoryAsync(string category)` - Get templates by category
- `CreateAsync(Template template)` - Create new template
- `UpdateAsync(Template template)` - Update existing template
- `DeleteAsync(int id)` - Delete template
- `ExistsAsync(int id)` - Check if template exists

### ContractRepository

- `GetByIdAsync(int id)` - Get contract by ID
- `GetAllAsync()` - Get all contracts
- `GetByClientIdAsync(int clientId)` - Get contracts by client
- `GetByStatusAsync(ContractStatus status)` - Get contracts by status
- `GetByUserIdAsync(int userId)` - Get contracts by user (created or assigned)
- `CreateAsync(Contract contract)` - Create new contract
- `UpdateAsync(Contract contract)` - Update existing contract
- `DeleteAsync(int id)` - Delete contract
- `ExistsAsync(int id)` - Check if contract exists

### AuditLogRepository

- `LogAsync(int contractId, int userId, string action, string? details)` - Create audit log entry
- `GetByContractIdAsync(int contractId)` - Get audit logs for contract
- `GetByUserIdAsync(int userId)` - Get audit logs for user
- `GetRecentAsync(int count)` - Get recent audit logs

## Usage

### Dependency Injection (Program.cs)

```csharp
var connectionString = builder.Configuration.GetConnectionString("OracleConnection");

builder.Services.AddScoped<IUserRepository>(sp =>
    new UserRepository(connectionString, sp.GetRequiredService<ILogger<UserRepository>>()));
```

### In Controller

```csharp
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return NotFound();
        
        return Ok(MapToDto(user));
    }
}
```

## Connection String

Configure in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "OracleConnection": "User Id=C##CSHARP_DEV;Password=Pass1234;Data Source=localhost:1521/XE;Pooling=true;Min Pool Size=1;Max Pool Size=10;"
  }
}
```

## Security

- All queries use parameterized queries to prevent SQL injection
- Passwords are never stored in plain text (use BCrypt hashing)
- Connection pooling is enabled for performance
- All database operations are async for scalability

## Error Handling

All repositories catch `OracleException` and log errors using `ILogger`. Exceptions are re-thrown to be handled by the API layer.

## Testing

To test repositories:

1. Ensure Oracle XE container is running
2. Run database setup: `Database/setup.bat`
3. Use test data from seed script
4. Run API and test endpoints via Swagger

## Dependencies

- `Oracle.ManagedDataAccess.Core` (23.4.0) - Oracle data provider
- `LegalDoc.Core` - Domain models and interfaces
- `Microsoft.Extensions.Logging` - Logging abstraction
