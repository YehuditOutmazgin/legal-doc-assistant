@echo off
echo ============================================
echo LegalDoc Database Setup
echo ============================================
echo.

REM Get container name
set CONTAINER_NAME=oracle-xe

REM Get password
set /p PASSWORD="Enter password for C##CSHARP_DEV: "

echo.
echo Step 1: Copying SQL files to container...
echo.

docker cp 01_create_tables.sql %CONTAINER_NAME%:/tmp/
docker cp 02_create_indexes.sql %CONTAINER_NAME%:/tmp/
docker cp 03_seed_data.sql %CONTAINER_NAME%:/tmp/
docker cp run_all.sql %CONTAINER_NAME%:/tmp/

echo.
echo Step 2: Running database setup...
echo.

docker exec -i %CONTAINER_NAME% sqlplus -S C##CSHARP_DEV/%PASSWORD%@XE @/tmp/run_all.sql

echo.
echo Step 3: Verifying installation...
echo.

docker exec -i %CONTAINER_NAME% sqlplus -S C##CSHARP_DEV/%PASSWORD%@XE <<EOF
SELECT table_name FROM user_tables ORDER BY table_name;
SELECT COUNT(*) AS USER_COUNT FROM USERS;
SELECT COUNT(*) AS CLIENT_COUNT FROM CLIENTS;
SELECT COUNT(*) AS CONTRACT_COUNT FROM CONTRACTS;
EXIT;
EOF

echo.
echo ============================================
echo Setup Complete!
echo ============================================
pause
