# Guía para agentes — Corvux AML Dashboard

## Fuente de verdad y honestidad
- No hay PRD disponible. Deriva el comportamiento confirmado del código y el `README.md`; no conviertas supuestos en requisitos.
- Al escribir análisis, etiqueta explícitamente los puntos no verificables como **[Inferido]** o **[Pendiente]**.
- Si README y código discrepan, prioriza el comportamiento del código vigente y documenta la discrepancia como pendiente de reconciliar.
- Mantén la documentación existente; no reemplaces ADRs ni diagramas sin una solicitud explícita.

## Estructura y tecnologías
- Aplicación principal: `Corvux/`, Blazor Server con componentes Razor interactivos, ASP.NET Core Controllers y Swagger.
- Tests: `Corvux.Tests/` (xUnit). Datos de ejemplo: `Samples/`.
- Documentación técnica existente: `docs/` (arquitectura, decisiones, UML, runbooks y troubleshooting).
- Stack confirmado en el proyecto: .NET 9, EF Core 9 con SQLite configurado en `Corvux/Program.cs` y `Corvux/Corvux.csproj`. No asumir SQL Server/LocalDB: parte del README y documentos heredados todavía lo mencionan y deben considerarse discrepancia pendiente.

## Convenciones de trabajo
- Antes de cambiar código o documentación, inspecciona el README y los archivos fuente relevantes; evita inferir contratos a partir de documentos obsoletos.
- Conserva el idioma español y los nombres de dominio existentes; mantén comandos y rutas exactos.
- No añadas dependencias, endpoints, reglas de negocio o decisiones arquitectónicas no respaldadas por el código o una decisión explícita.
- Mantén cambios acotados y coherentes con la estructura actual. No edites secretos ni introduzcas credenciales.

## Verificación
- Tras cambios de código, ejecuta `dotnet test Corvux.Tests/Corvux.Tests.csproj --verbosity minimal` y, cuando aplique, `dotnet build Corvux/Corvux.csproj`.
- Tras cambios de documentación, comprueba enlaces/rutas mencionados y que las afirmaciones coincidan con código o README; distingue hechos de inferencias.
- Reporta verificaciones realmente ejecutadas y cualquier bloqueo; no afirmes que una prueba pasó si no se ejecutó.

## Contexto funcional confirmado
- `TransactionService` orquesta operaciones CRUD e importación, creando contextos con `IDbContextFactory<ComplianceDbContext>`.
- `ComplianceRuleEngine` evalúa umbrales AML; consulta sus tests antes de alterar condiciones o efectos.
- `TransactionImportParser` acepta CSV y XLSX y valida datos antes de persistir; revisa tests al cambiar parsing o validaciones.
- API: `TransactionsController` expone rutas bajo `/api/transactions`; páginas Razor están en `Corvux/Components/Pages/`.
