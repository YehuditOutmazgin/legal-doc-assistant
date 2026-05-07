@echo off
REM ============================================
REM LegalDoc Database Setup Script
REM Copies SQL files to Oracle container and executes them
REM ============================================

set CONTAINER_NAME=oracle-xe
set DB_USER=C##CSHARP_DEV

echo ============================================
echo LegalDoc Database Setup
echo ============================================
echo.

set /p DB_PASSWORD="Enter password for %DB_USER%: "

echo.
echo [1/3] Copying SQL files to container...
docker cp 01_create_tables.sql %CONTAINER_NAME%:/tmp/
docker cp 02_create_indexes.sql %CONTAINER_NAME%:/tmp/
docker cp 03_seed_data.sql %CONTAINER_NAME%:/tmp/
docker cp run_all.sql %CONTAINER_NAME%:/tmp/
docker cp test_connection.sql %CONTAINER_NAME%:/tmp/

echo.
echo [2/3] Running database setup scripts...
docker exec -i %CONTAINER_NAME% sqlplus -S %DB_USER%/%DB_PASSWORD%@XE @/tmp/run_all.sql

echo.
echo [3/3] Verifying installation...
docker exec -i %CONTAINER_NAME% sqlplus -S %DB_USER%/%DB_PASSWORD%@XE @/tmp/test_connection.sql

echo.
echo ============================================
echo Setup Complete!
echo ============================================
echo.
echo Test credentials:
echo   admin@legaldoc.com   / Password123!
echo   lawyer@legaldoc.com  / Password123!
echo   clerk@legaldoc.com   / Password123!
echo.
pause
