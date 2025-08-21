FROM mcr.microsoft.com/mssql/server:2022-latest

# Set environment
ENV ACCEPT_EULA=Y
ENV SA_PASSWORD=Fahri@2024!
ENV MSSQL_PID=Developer

# Copy SQL file & entrypoint
COPY init.sql /init.sql
#COPY entrypoint.sh /entrypoint.sh
#RUN chmod +x /entrypoint.sh

# Pakai script sebagai entrypoint
CMD ["/bin/bash", "/entrypoint.sh"]
