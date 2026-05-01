FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# ✅ FIX: missing system dependency for PostgreSQL / Kerberos auth
RUN apt-get update && apt-get install -y \
    libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["BudgetBuddyAPI.csproj", "."]
RUN dotnet restore "BudgetBuddyAPI.csproj"

COPY . .
RUN dotnet build "BudgetBuddyAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BudgetBuddyAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BudgetBuddyAPI.dll"]