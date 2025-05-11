# Use the official SQL Server image
FROM mcr.microsoft.com/mssql/server:latest AS sqlserver
ENV ACCEPT_EULA=Y 
ENV SA_PASSWORD=YourStrong!Passw0rd

# Add optional environment variable for database name
ENV MSSQL_DBNAME=BookingDB