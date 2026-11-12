# 00 — Descripción global

## Propósito observable

Corvux es una aplicación web que registra y consulta transacciones, evalúa algunas reglas AML configuradas localmente, permite importar lotes CSV/XLSX y presenta páginas de dashboard y reportes. La solución combina componentes Razor interactivos de servidor y una API REST bajo `/api/transactions`.

## Alcance confirmado

- CRUD de transacciones mediante UI y endpoints REST.
- Evaluación de reglas de monto y países bloqueados.
- Importación de archivo CSV o XLSX con validación y control de duplicados.
- KPIs, mapa geográfico y exportación CSV según README.
- Persistencia EF Core con SQLite configurado actualmente en `Program.cs`.

## Arquitectura resumida

Monolito ASP.NET Core .NET 9. Los componentes Razor y el controlador consumen servicios de aplicación; `TransactionService` crea contextos breves con `IDbContextFactory<ComplianceDbContext>`. `ComplianceRuleEngine` calcula evaluaciones y `TransactionImportParser` interpreta archivos. El arranque invoca `EnsureCreatedAsync()`.

## Fuera de alcance confirmado / incógnitas

No se encontró PRD ni definición verificable de usuarios, perfiles, autenticación, autorización, obligaciones regulatorias, SLAs o trazabilidad de auditoría. No afirmar certificación AML, integración con listas externas, ni preparación productiva sin evidencia adicional.

**[Pendiente]** El README declara SQL Server LocalDB, pero el código configura SQLite. No se toma ninguna decisión de proveedor aquí; véase [05 — Límites del sistema](05-limites-del-sistema.md).

## Referencias

`README.md`; `Corvux/Program.cs`; `Corvux/Corvux.csproj`; `Corvux/Services/TransactionService.cs`; `Corvux/Services/ComplianceRuleEngine.cs`; `Corvux/Services/TransactionImportParser.cs`.
