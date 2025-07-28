#!/bin/bash
set -e

echo "Setting up PostgreSQL primary for replication..."

# Create replication user
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    -- Create replication user
    CREATE USER $POSTGRES_REPLICATION_USER WITH REPLICATION ENCRYPTED PASSWORD '$POSTGRES_REPLICATION_PASSWORD';
    
    -- Grant necessary permissions
    GRANT CONNECT ON DATABASE $POSTGRES_DB TO $POSTGRES_REPLICATION_USER;
EOSQL

# Add replication configuration to pg_hba.conf
echo "# Replication connections" >> "$PGDATA/pg_hba.conf"
echo "host replication $POSTGRES_REPLICATION_USER 0.0.0.0/0 md5" >> "$PGDATA/pg_hba.conf"
echo "host replication $POSTGRES_REPLICATION_USER ::0/0 md5" >> "$PGDATA/pg_hba.conf"

# Create archive directory
mkdir -p "$PGDATA/archive"
chmod 700 "$PGDATA/archive"

# Reload configuration
pg_ctl reload

echo "PostgreSQL primary replication setup completed"