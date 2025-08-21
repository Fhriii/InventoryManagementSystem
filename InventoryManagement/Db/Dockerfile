FROM mcr.microsoft.com/mssql/server:2022-latest

WORKDIR /init

# Copy entrypoint dengan permission yang bener
COPY --chmod=755 entrypoint.sh /entrypoint.sh
COPY init.sql /init.sql

ENTRYPOINT ["/entrypoint.sh"]
