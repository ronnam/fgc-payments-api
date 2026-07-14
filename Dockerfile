# === STAGE 1: BUILD ===
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Pacotes locais (Fgc.MessageContracts)
COPY LocalPackages ./LocalPackages
COPY nuget.config .

# Copia apenas os .csproj pra cache do restore
COPY ["Fgc.Payments/src/Fgc.Payments.Api/Fgc.Payments.Api.csproj", "Fgc.Payments/src/Fgc.Payments.Api/"]
COPY ["Fgc.Payments/src/Fgc.Payments.Application/Fgc.Payments.Application.csproj", "Fgc.Payments/src/Fgc.Payments.Application/"]
COPY ["Fgc.Payments/src/Fgc.Payments.Domain/Fgc.Payments.Domain.csproj", "Fgc.Payments/src/Fgc.Payments.Domain/"]
COPY ["Fgc.Payments/src/Fgc.Payments.Infraestructure/Fgc.Payments.Infraestructure.csproj", "Fgc.Payments/src/Fgc.Payments.Infraestructure/"]
RUN dotnet restore "Fgc.Payments/src/Fgc.Payments.Api/Fgc.Payments.Api.csproj"

# Copia o resto e faz o publish
COPY . .
WORKDIR "/src/Fgc.Payments/src/Fgc.Payments.Api"
RUN dotnet publish "Fgc.Payments.Api.csproj" -c Release -o /app/publish

# === STAGE 2: RUNTIME ===
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Fgc.Payments.Api.dll"]
