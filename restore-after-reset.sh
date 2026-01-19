#!/bin/bash
# restore-after-reset.sh
# ПРИЗНАЧЕННЯ: Автоматичне відновлення БД після docker-compose down -v
# ВИКОРИСТАННЯ: ./restore-after-reset.sh

set -e

MSSQL_CONTAINER="basedtechstore-mssql"
DATABASE="BasedTechStore"
SA_PASSWORD="MyStrong!Passw0rd123"
BACKUP_FILE="BasedTechStore.bak"

echo "=== Starting Database Restore Process ===" 
echo "Container: $MSSQL_CONTAINER"
echo "Database: $DATABASE"
echo "Backup: $BACKUP_FILE"

# Перевірити що backup файл існує
if [ ! -f "./backup/$BACKUP_FILE" ]; then
    echo "ERROR: Backup file not found: ./backup/$BACKUP_FILE"
    exit 1
fi

echo "✓ Backup file found"

# Запустити mssql контейнер якщо не запущено
echo ""
echo "Starting SQL Server container..."
docker-compose up mssql -d

# Дочекатися готовності SQL Server
echo "Waiting for SQL Server to be ready..."
for i in {1..30}; do
    if docker exec $MSSQL_CONTAINER /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$SA_PASSWORD" -Q "SELECT 1" &>/dev/null; then
        echo "✓ SQL Server is ready!"
        break
    fi
    echo "Attempt $i/30: SQL Server not ready yet..."
    sleep 2
    
    if [ $i -eq 30 ]; then
        echo "ERROR: SQL Server failed to start"
        exit 1
    fi
done

# Перевірити чи існує база даних
echo ""
echo "Checking if database exists..."
DB_EXISTS=$(docker exec $MSSQL_CONTAINER /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$SA_PASSWORD" -h -1 -Q "SET NOCOUNT ON; SELECT CASE WHEN EXISTS(SELECT name FROM sys.databases WHERE name = '$DATABASE') THEN 1 ELSE 0 END" 2>/dev/null | tr -d '[:space:]')

if [ "$DB_EXISTS" = "1" ]; then
    echo "✓ Database already exists. Skipping restore."
else
    echo "Database does not exist. Starting restore..."
    
    # Отримати логічні імена файлів
    echo "Getting logical file names from backup..."
    FILELISTONLY=$(docker exec $MSSQL_CONTAINER /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$SA_PASSWORD" -Q "RESTORE FILELISTONLY FROM DISK = '/var/opt/mssql/backup/$BACKUP_FILE'" -h -1 2>/dev/null)
    
    LOGICAL_DATA_NAME=$(echo "$FILELISTONLY" | awk 'NR==1 {print $1}')
    LOGICAL_LOG_NAME=$(echo "$FILELISTONLY" | awk 'NR==2 {print $1}')
    
    echo "Data file: $LOGICAL_DATA_NAME"
    echo "Log file: $LOGICAL_LOG_NAME"
    
    # Якщо не вдалося визначити імена, використовуємо стандартні
    if [ -z "$LOGICAL_DATA_NAME" ]; then
        LOGICAL_DATA_NAME="$DATABASE"
        LOGICAL_LOG_NAME="${DATABASE}_log"
        echo "Using default logical names"
    fi
    
    # Відновити базу даних
    echo ""
    echo "Restoring database..."
    docker exec $MSSQL_CONTAINER /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$SA_PASSWORD" -Q "RESTORE DATABASE [$DATABASE] FROM DISK = '/var/opt/mssql/backup/$BACKUP_FILE' WITH REPLACE, MOVE '$LOGICAL_DATA_NAME' TO '/var/opt/mssql/data/$DATABASE.mdf', MOVE '$LOGICAL_LOG_NAME' TO '/var/opt/mssql/data/${DATABASE}_log.ldf', STATS = 10"
    
    if [ $? -eq 0 ]; then
        echo "✓ Database restored successfully!"
    else
        echo "ERROR: Database restore failed!"
        exit 1
    fi
fi

# Перевірити дані
echo ""
echo "Verifying data..."
docker exec $MSSQL_CONTAINER /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$SA_PASSWORD" -d $DATABASE -Q "SELECT 'Categories' as TableName, COUNT(*) as RecordCount FROM Categories UNION ALL SELECT 'SubCategories', COUNT(*) FROM SubCategories UNION ALL SELECT 'Products', COUNT(*) FROM Products UNION ALL SELECT 'Users', COUNT(*) FROM AspNetUsers" 2>/dev/null || echo "Warning: Could not verify all tables"

echo ""
echo "=== Database Restore Completed ==="
echo ""
echo "Starting other services..."
# ДОДАНО: автоматичний запуск webapi та react-app БЕЗ перестворення mssql
docker-compose up webapi react-app -d --no-recreate

if [ $? -eq 0 ]; then
    echo "✓ All services started successfully!"
    echo ""
    echo "Services status:"
    docker-compose ps
else
    echo "Warning: Failed to start some services"
    echo "You can manually start them with:"
    echo "  docker-compose up webapi react-app -d --no-recreate"
fi