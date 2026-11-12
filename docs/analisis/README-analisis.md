# Análisis derivado del README y del código

> Alcance: síntesis del README y de la implementación inspeccionada. No se encontró PRD. Los puntos que no pueden verificarse en esas fuentes están marcados **[Inferido]** o **[Pendiente]**.

## 1. Propósito y alcance observable

El repositorio implementa un dashboard de cumplimiento AML para registrar y consultar transacciones, aplicar reglas de evaluación, importar lotes y presentar indicadores, reportes y exposición geográfica. La misma lógica de transacciones se ofrece desde la interfaz Blazor y desde una API REST.

**Confirmado por README y código:** CRUD de transacciones, evaluación de compliance, importación CSV/XLSX, páginas de dashboard, exportación CSV en reportes y API REST con Swagger.

**[Pendiente]** No hay definición de usuarios, perfiles/roles, autenticación, autorización, objetivos regulatorios, niveles de servicio ni requisitos de auditoría formal. No asumir que la solución satisface un marco legal específico.

## 2. Arquitectura y componentes

La implementación es un monolito web ASP.NET Core con componentes Razor interactivos del lado servidor y controladores REST:

- `Corvux/Program.cs`: registra `IDbContextFactory<ComplianceDbContext>`, servicios scoped, Razor Components interactivos, controllers y Swagger. Al iniciar ejecuta `EnsureCreatedAsync()`.
- `Corvux/Components/Pages/`: `VistaGeneral`, `GestionarTransacciones`, `CargaMasiva` y `Reportes`.
- `Corvux/Controllers/TransactionsController.cs`: operaciones HTTP sobre transacciones e importación.
- `Corvux/Services/TransactionService.cs`: CRUD, detección de duplicados e importación con validación antes de guardar.
- `Corvux/Services/ComplianceRuleEngine.cs`: evaluación de reglas AML.
- `Corvux/Services/TransactionImportParser.cs`: lectura de CSV y XLSX.
- `Corvux/Data/ComplianceDbContext.cs` y `Corvux/Models/`: persistencia y modelos del dominio.

La UI y la API comparten `TransactionService` y `ComplianceRuleEngine`. El servicio crea un DbContext por operación mediante la factoría, en lugar de conservarlo durante el circuito Blazor.

## 3. Discrepancia importante: motor de base de datos

El README describe SQL Server LocalDB, pero el proyecto actual referencia `Microsoft.EntityFrameworkCore.Sqlite` y `Program.cs` configura `UseSqlite(...)`. Por tanto, el código observado configura SQLite, no LocalDB. También hay documentos de setup y arquitectura con rutas/nombres previos (`CorvuxChallenge`) que no coinciden con la estructura actual (`Corvux`).

**Acción pendiente:** reconciliar README, runbooks y documentos heredados con la configuración y el flujo de persistencia que el equipo quiera soportar. No cambiar el proveedor ni afirmar que se usa LocalDB sin confirmar la decisión.

## 4. Reglas de negocio observadas

Según `ComplianceRuleEngine` y el README:

| Condición | Efecto observado |
|---|---|
| Monto mayor que USD 10.000 | Requiere declaración de fondos. |
| Origen o destino coincide con Irán o Corea del Norte | La transacción queda bloqueada; se conserva una razón. La comparación ignora mayúsculas/minúsculas y admite coincidencia parcial para los alias configurados. |
| Monto mayor que USD 50.000 | Calcula una tasa administrativa equivalente al 0,5% del monto. |

Los límites son estrictos (`>`), no inclusivos. No inferir tratamiento de montos exactamente iguales al umbral fuera de lo que muestra ese operador. Las reglas se aplican en memoria al evaluar; revisar usos concretos en UI/API antes de describir todos los flujos.

**[Pendiente]** No se documenta una fuente externa para listas de sanciones, actualización de reglas, moneda distinta de USD ni revisión humana de alertas.

## 5. Importación masiva

`TransactionImportParser` selecciona formato por extensión `.csv` o `.xlsx`. Mapea encabezados canónicos y aliases en español/inglés; exige TransactionId, AmountUsd, OriginCountry y DestinationCountry. Valida ID no vacío, monto decimal positivo y países no vacíos. XLSX se lee desde el ZIP/XML y se consulta `sheet1.xml`.

`TransactionService.ImportAsync` no persiste si el parser reporta errores, si no hay filas o si encuentra IDs ya existentes o duplicados dentro del lote. Si valida todo, guarda el lote. El controlador recibe el archivo como multipart en `POST /api/transactions/import`.

**[Pendiente]** Límites de tamaño, protección ante archivos comprimidos maliciosos, soporte de varias hojas, CSV con saltos de línea dentro de campos entrecomillados y límites de concurrencia no están establecidos por la documentación examinada. No afirmar que se soportan.

## 6. API observada

Ruta base: `/api/transactions`.

- `GET /api/transactions`: lista registros y adjunta evaluación de compliance.
- `POST /api/transactions`: crea desde `TransactionFormModel`.
- `PUT /api/transactions/{id}`: actualiza.
- `DELETE /api/transactions/{id}`: elimina.
- `POST /api/transactions/import`: importa CSV/XLSX multipart.

Swagger se habilita únicamente cuando `app.Environment.IsDevelopment()` es verdadero. Los resultados HTTP concretos dependen del resultado de servicio y del controlador; consultar el código antes de cambiar o documentar contratos.

## 7. Persistencia y modelo

El README describe una tabla `Transactions` con ID, identificador de transacción, monto USD, países de origen/destino y marcas temporales UTC. El contexto y la entidad vigentes son las fuentes a verificar para restricciones exactas. El arranque usa `EnsureCreatedAsync`, que crea la base si no existe; no equivale a un flujo de migraciones.

**[Pendiente]** La documentación no aclara estrategia de evolución de esquema, retención, respaldo, cifrado, concurrencia ni despliegues productivos. El `appsettings` y el entorno de ejecución deben revisarse antes de declarar un connection string/proveedor como contrato estable.

## 8. UI y visualización

El README asigna estas rutas a las páginas: `/` (vista general), `/transacciones` (gestión), `/carga-masiva` y `/reportes`. La vista general muestra KPIs, mapa y tabla de lectura; el mapa usa Leaflet vía JSInterop y datos de coordenadas estáticas en `CountryCoordinateMap` según README y estructura.

**[Inferido]** La separación de páginas sugiere flujos de consulta, edición, carga y análisis diferenciados; los criterios de UX, accesibilidad y permisos no están definidos por un PRD.

## 9. Desarrollo y pruebas

Requisitos indicados en README: SDK .NET 9 y proveedor de base configurado. Comandos principales:

```bash
dotnet restore Corvux/Corvux.csproj
dotnet restore Corvux.Tests/Corvux.Tests.csproj
dotnet run --project Corvux/Corvux.csproj --launch-profile http
dotnet test Corvux.Tests/Corvux.Tests.csproj --verbosity minimal
dotnet build Corvux/Corvux.csproj
```

El README informa 38 tests xUnit, distribuidos entre motor AML y parser de importación. El número debe verificarse ejecutando la suite; no es una garantía permanente.

## 10. Riesgos y preguntas abiertas

1. **Proveedor de base discrepante:** confirmar si SQLite es intencional y actualizar documentos heredados o el código según la decisión.
2. **Seguridad:** confirmar requisitos de autenticación/autorización y manejo de datos financieros; no se identifican en la configuración revisada.
3. **Reglas AML:** confirmar autoridad, alcance y proceso de actualización de países/umbrales.
4. **Operación:** definir despliegue, observabilidad, backups, recuperación y gestión de esquema.
5. **Importación:** establecer límites de carga y casos de formato que se deben admitir.
6. **Auditoría:** el producto se describe como compliance, pero no se ha confirmado un registro inmutable de cambios ni evidencia regulatoria.
7. **Documentación:** reconciliar rutas, nombres de proyecto y proveedor en README, setup local y arquitectura.

## Fuentes consultadas

- `README.md`
- `Corvux/Program.cs`
- `Corvux/Corvux.csproj`
- `Corvux/Services/ComplianceRuleEngine.cs`
- `Corvux/Services/TransactionService.cs`
- `Corvux/Services/TransactionImportParser.cs`
- `Corvux/Controllers/TransactionsController.cs`
- `docs/architecture/0-overview.md`
- `docs/runbooks/setup-local.md`
- `docs/troubleshooting/known_issues.md`
