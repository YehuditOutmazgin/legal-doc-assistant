# Oracle CLI Guide - LegalDoc Database

## Quick Reference

### Connect to Oracle Container

```powershell
docker exec -it oracle-xe sqlplus C##CSHARP_DEV/Pass1234@XE
```

### Run SQL File from Outside Container

```powershell
docker exec -i oracle-xe sqlplus -S C##CSHARP_DEV/Pass1234@XE @/tmp/filename.sql
```

### Copy File to Container

```powershell
docker cp ./filename.sql oracle-xe:/tmp/
```

---

## Common Tasks

### 1. View All Users

```powershell
docker exec -i oracle-xe sqlplus -S C##CSHARP_DEV/Pass1234@XE <<EOF
SELECT ID, EMAIL, FIRST_NAME, LAST_NAME, ROLE FROM USERS;
EXIT;
EOF
```

### 2. View All Contracts

```powershell
docker exec -i oracle-xe sqlplus -S C##CSHARP_DEV/Pass1234@XE <<EOF
SELECT ID, TITLE, STATUS, CLIENT_ID FROM CONTRACTS;
EXIT;
EOF
```

### 3. Count Records in Each Table

```powershell
docker exec -i oracle-xe sqlplus -S C##CSHARP_DEV/Pass1234@XE <<EOF
SELECT 'USERS' AS TABLE_NAME, COUNT(*) FROM USERS
UNION ALL
SELECT 'CLIENTS', COUNT(*) FROM CLIENTS
UNION ALL
SELECT 'TEMPLATES', COUNT(*) FROM TEMPLATES
UNION ALL
SELECT 'CONTRACTS', COUNT(*) FROM CONTRACTS
UNION ALL
SELECT 'AUDIT_LOGS', COUNT(*) FROM AUDIT_LOGS;
EXIT;
EOF
```

### 4. Insert New User

```powershell
docker exec -i oracle-xe sqlplus -S C##CSHARP_DEV/Pass1234@XE <<EOF
INSERT INTO USERS (EMAIL, PASSWORD_HASH, FIRST_NAME, LAST_NAME, ROLE, CREATED_AT, IS_ACTIVE)
VALUES ('newuser@legaldoc.com', 'hashed_password_here', 'John', 'Doe', 'LAWYER', CURRENT_TIMESTAMP, 1);
COMMIT;
EXIT;
EOF
```

### 5. Update Contract Status

```powershell
docker exec -i oracle-xe sqlplus -S C##CSHARP_DEV/Pass1234@XE <<EOF
UPDATE CONTRACTS SET STATUS = 'REVIEW' WHERE ID = 1;
COMMIT;
EXIT;
EOF
```

### 6. Delete All Data (Reset Database)

```powershell
docker exec -i oracle-xe sqlplus -S C##CSHARP_DEV/Pass1234@XE @/tmp/clear_data.sql
```

### 7. Reinitialize Database

```powershell
docker exec -i oracle-xe sqlplus -S C##CSHARP_DEV/Pass1234@XE @/tmp/run_all.sql
```

---

## Interactive Mode (SQL*Plus Shell)

### Enter Interactive Mode

```powershell
docker exec -it oracle-xe sqlplus C##CSHARP_DEV/Pass1234@XE
```

You'll see:

```
SQL>
```

### Run Commands in Interactive Mode

```sql
SQL> SELECT * FROM USERS;

SQL> INSERT INTO CLIENTS (NAME, TYPE, EMAIL, PHONE, ADDRESS, IS_ACTIVE)
     VALUES ('New Client', 'INDIVIDUAL', 'client@example.com', '050-1234567', 'Tel Aviv', 1);

SQL> COMMIT;

SQL> EXIT;
```

### Useful SQL*Plus Commands

| Command | Purpose |
|---------|---------|
| `DESC USERS;` | Show table structure |
| `SELECT * FROM USERS;` | Query data |
| `COMMIT;` | Save changes |
| `ROLLBACK;` | Undo changes |
| `EXIT;` | Exit SQL*Plus |
| `CLEAR SCREEN;` | Clear terminal |
| `SET PAGESIZE 50;` | Set rows per page |
| `SET LINESIZE 200;` | Set line width |

---

## Working with SQL Files

### Create New Query File

1. Create file: `my_query.sql`

```sql
SELECT ID, EMAIL, ROLE FROM USERS WHERE ROLE = 'LAWYER';
```

2. Copy to container:

```powershell
docker cp ./my_query.sql oracle-xe:/tmp/
```

3. Run it:

```powershell
docker exec -i oracle-xe sqlplus -S C##CSHARP_DEV/Pass1234@XE @/tmp/my_query.sql
```

---

## Troubleshooting

### Error: ORA-01017 (Invalid username/password)

Check password is correct:

```powershell
docker exec -it oracle-xe sqlplus C##CSHARP_DEV/Pass1234@XE
```

### Error: SP2-0310 (Unable to open file)

File not in container. Copy it first:

```powershell
docker cp ./filename.sql oracle-xe:/tmp/
```

### Error: ORA-00942 (Table or view does not exist)

Table hasn't been created. Run setup:

```powershell
.\setup.bat
```

### Container Not Running

Start it:

```powershell
docker start oracle-xe
```

Check status:

```powershell
docker ps
```

---

## Tips & Tricks

### Format Output

```sql
COLUMN EMAIL FORMAT A30;
COLUMN ROLE FORMAT A10;
SELECT EMAIL, ROLE FROM USERS;
```

### Export to File

```powershell
docker exec -i oracle-xe sqlplus -S C##CSHARP_DEV/Pass1234@XE > output.txt <<EOF
SELECT * FROM USERS;
EXIT;
EOF
```

### Run Multiple Statements

```powershell
docker exec -i oracle-xe sqlplus -S C##CSHARP_DEV/Pass1234@XE <<EOF
DELETE FROM AUDIT_LOGS;
DELETE FROM CONTRACTS;
COMMIT;
SELECT COUNT(*) FROM CONTRACTS;
EXIT;
EOF
```

### Check Table Size

```sql
SELECT SEGMENT_NAME, BYTES/1024/1024 AS SIZE_MB FROM USER_SEGMENTS WHERE SEGMENT_NAME = 'USERS';
```

---

## Next: C# Integration

Once you're comfortable with CLI, you'll use ODP.NET in C# to:

1. Open connection
2. Execute queries
3. Read results into objects
4. Map to DTOs

See `LegalDoc.Infrastructure/Repositories` for implementation.
