# LegalDoc Database Setup Guide

## Overview

This directory contains all Oracle database scripts for the LegalDoc project. The database uses Oracle XE 21c with IDENTITY columns for auto-incrementing primary keys.

## Prerequisites

- Oracle XE 21c running in Docker
- Docker CLI installed
- User `C##CSHARP_DEV` created in Oracle with appropriate privileges

## Quick Start

### 1. Start Oracle Container

```bash
docker run -d --name oracle-xe -p 1521:1521 -e ORACLE_PASSWORD=your_password gvenzl/oracle-xe:latest
```

### 2. Create Database User

Connect to the container and create the user:

```bash
docker exec -it oracle-xe sqlplus system/oracle@XE
```

In SQL*Plus:

```sql
CREATE USER C##CSHARP_DEV IDENTIFIED BY Pass1234;
GRANT CREATE SESSION, CREATE TABLE, CREATE SEQUENCE, CREATE PROCEDURE TO C##CSHARP_DEV;
GRANT UNLIMITED TABLESPACE TO C##CSHARP_DEV;
EXIT;
```

### 3. Run Database Setup

From the `Database` directory:

```bash
.\setup.bat
```

When prompted, enter the password: `Pass1234`

The script will:
1. Copy SQL files to the container
2. Create all tables with IDENTITY columns
3. Create indexes for performance
4. Insert test data
5. Verify the installation

## Database Schema

### Tables

| Table | Purpose | Rows |
|-------|---------|------|
| USERS | System users (ADMIN, LAWYER, CLERK) | 3 |
| CLIENTS | Law firm clients (INDIVIDUAL, COMPANY) | 2 |
| TEMPLATES | Contract templates with CLOB content | 1 |
| CONTRACTS | Created contracts with workflow status | 1 |
| AUDIT_LOGS | Audit trail for all operations | 1 |

### Key Features

- **IDENTITY Columns**: Auto-incrementing primary keys (Oracle 12c+)
- **CLOB Fields**: Large text storage for contract content
- **Foreign Keys**: Referential integrity constraints
- **Indexes**: Performance optimization on frequently queried columns
- **Timestamps**: Automatic creation/update tracking

## Test Data

### Users

| Email | Password | Role |
|-------|----------|------|
| admin@legaldoc.com | Password123! | ADMIN |
| lawyer@legaldoc.com | Password123! | LAWYER |
| clerk@legaldoc.com | Password123! | CLERK |

### Clients

- Yossi Mizrahi (INDIVIDUAL)
- Tech Solutions Ltd (COMPANY)

### Templates

- Employment Contract (Employment category)

### Contracts

- Employment Agreement - Yossi Mizrahi (DRAFT status)

## Connection String

For ODP.NET (Oracle.ManagedDataAccess.Core):

```
User Id=C##CSHARP_DEV;Password=Pass1234;Data Source=localhost:1521/XE;Pooling=true;Min Pool Size=1;Max Pool Size=10;
```

For appsettings.json:

```json
{
  "ConnectionStrings": {
    "OracleConnection": "User Id=C##CSHARP_DEV;Password=Pass1234;Data Source=localhost:1521/XE;Pooling=true;Min Pool Size=1;Max Pool Size=10;"
  }
}
```

## Manual Operations

### Query Data

Connect to the database:

```bash
docker exec -it oracle-xe sqlplus C##CSHARP_DEV/Pass1234@XE
```

In SQL*Plus:

```sql
SELECT * FROM USERS;
SELECT * FROM CLIENTS;
SELECT * FROM CONTRACTS;
EXIT;
```

### Clear All Data

```bash
docker exec -it oracle-xe sqlplus C##CSHARP_DEV/Pass1234@XE @/tmp/clear_data.sql
```

### Reinitialize Database

```bash
docker exec -it oracle-xe sqlplus C##CSHARP_DEV/Pass1234@XE @/tmp/run_all.sql
```

## File Structure

```
Database/
├── setup.bat                 # Main setup script (run this)
├── 01_create_tables.sql      # Table definitions
├── 02_create_indexes.sql     # Index creation
├── 03_seed_data.sql          # Test data insertion
├── run_all.sql               # Master script (called by setup.bat)
├── test_connection.sql       # Verification queries
├── clear_data.sql            # Data cleanup
└── README.md                 # This file
```

## Troubleshooting

### Error: ORA-01017 (Invalid username/password)

Verify the password is correct:

```bash
docker exec -it oracle-xe sqlplus C##CSHARP_DEV/Pass1234@XE
```

If it fails, reset the user password:

```bash
docker exec -it oracle-xe sqlplus system/oracle@XE
```

Then:

```sql
ALTER USER C##CSHARP_DEV IDENTIFIED BY Pass1234;
EXIT;
```

### Error: SP2-0310 (Unable to open file)

The SQL files are not in the container. Run setup.bat again to copy them.

### Error: ORA-00942 (Table or view does not exist)

This is normal on first run when dropping non-existent tables. The script continues and creates them.

### Error: ORA-02291 (Integrity constraint violated)

This occurs if seed data references non-existent parent records. Run `clear_data.sql` first, then `run_all.sql`.

## GUI Tools

### SQL Developer

1. Download from Oracle
2. Create new connection:
   - Username: C##CSHARP_DEV
   - Password: Pass1234
   - Hostname: localhost
   - Port: 1521
   - SID: XE

### DBeaver

1. Download from https://dbeaver.io
2. New Database Connection → Oracle
3. Fill in connection details (same as above)

## Performance Considerations

- Indexes are created on frequently queried columns (EMAIL, STATUS, CREATED_AT)
- CLOB fields are used for large text (contract content)
- Connection pooling is configured in the connection string
- Foreign key constraints ensure data integrity

## Next Steps

1. Install `Oracle.ManagedDataAccess.Core` NuGet package in LegalDoc.Infrastructure
2. Implement repository classes using ODP.NET
3. Configure dependency injection in LegalDoc.API
4. Test API endpoints with sample data

## References

- [Oracle Database 21c Documentation](https://docs.oracle.com/en/database/oracle/oracle-database/21/)
- [ODP.NET Documentation](https://docs.oracle.com/en/database/oracle/oracle-data-access-components/)
- [IDENTITY Columns in Oracle](https://docs.oracle.com/en/database/oracle/oracle-database/21/sqlrf/CREATE-TABLE.html)
