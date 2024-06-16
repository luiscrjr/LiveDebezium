./stop.sh
docker compose -f docker-compose.yaml up -d && sleep 15 && cat script.sql | docker exec -i sqlserver bash -c '/opt/mssql-tools/bin/sqlcmd -U sa -P 1q2w3e4r@#'