#!/usr/bin/env bash
set -euo pipefail
# Usage: ./scripts/init-db.sh [container-name] [root-password]
# Example: ./scripts/init-db.sh cinema-mysql myrootpassword

CONTAINER=${1:-cinema-mysql}
ROOT_PW=${2:-}

if [ -z "$ROOT_PW" ]; then
  echo "No root password supplied. If your container has a root password, pass it as the second argument." >&2
  echo "Attempting to run without password (useful if root has empty password or auth is disabled)." >&2
fi

SQLFILE="Dockerfile/db/init.sql"

if [ ! -f "$SQLFILE" ]; then
  echo "Init SQL not found at $SQLFILE" >&2
  exit 1
fi

echo "Executing $SQLFILE inside container $CONTAINER..."

if [ -z "$ROOT_PW" ]; then
  docker exec -i "$CONTAINER" mysql -uroot < "$SQLFILE"
else
  docker exec -i "$CONTAINER" mysql -uroot -p"$ROOT_PW" < "$SQLFILE"
fi

echo "Database initialization script executed."
