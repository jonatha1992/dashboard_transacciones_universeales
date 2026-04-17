# Setup Local

## Requisitos previos

- .NET 9 SDK → https://dotnet.microsoft.com/download
- SQL Server LocalDB (viene incluido con Visual Studio, o instalá el paquete standalone)

Verificar que LocalDB esté disponible:
```bash
sqllocaldb info
```

Si no aparece la instancia `MSSQLLocalDB`, crearla:
```bash
sqllocaldb create MSSQLLocalDB
sqllocaldb start MSSQLLocalDB
```

## Instalación de dependencias

```bash
cd corvux-desafio
dotnet restore CorvuxChallenge/CorvuxChallenge.csproj
dotnet restore CorvuxChallenge.Tests/CorvuxChallenge.Tests.csproj
```

## Configuración de base de datos

La connection string ya está en `CorvuxChallenge/appsettings.json`:
```json
"ComplianceDb": "Server=(localdb)\\MSSQLLocalDB;Database=CorvuxChallengeDb;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False"
```

No se necesita configuración adicional. La base de datos se crea automáticamente al arrancar la app (`EnsureCreated()`).

Para usar otra instancia SQL Server, editá la connection string en `appsettings.json`.

## Levantar el proyecto

```bash
dotnet run --project CorvuxChallenge/CorvuxChallenge.csproj --launch-profile http
```

- Dashboard: http://localhost:5180
- Swagger UI: http://localhost:5180/swagger

## Correr los tests

```bash
dotnet test CorvuxChallenge.Tests/CorvuxChallenge.Tests.csproj --verbosity minimal
```

29 tests — ComplianceRuleEngine (17) + TransactionImportParser (12)

## Build completo

```bash
dotnet build CorvuxChallenge/CorvuxChallenge.csproj
```

## Importar transacciones de ejemplo

Desde el dashboard en http://localhost:5180, usar el importador con el archivo:
```
CorvuxChallenge/Samples/transactions-sample.csv
```

O vía API:
```bash
curl -X POST http://localhost:5180/api/transactions/import \
  -F "file=@CorvuxChallenge/Samples/transactions-sample.csv"
```

## Problemas frecuentes

Ver `docs/troubleshooting/known_issues.md`
