##!/bin/bash
## Start SQL Server in the background
#/opt/mssql/bin/sqlservr &
#
## Tunggu SQL Server siap
#echo "Waiting for SQL Server to start..."
#sleep 20
#
## Eksekusi init.sql
#echo "Running init.sql..."
#/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Fahri@2024!" -C -i /init.sql
#
## Biar container tetap hidup
#wait
