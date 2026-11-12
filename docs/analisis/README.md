# Análisis del sistema

Documentación analítica derivada del README y del código vigente. No se encontró un PRD. Los hechos observados se distinguen de **[Inferido]** y **[Pendiente]**; estas páginas no sustituyen los ADR, diagramas existentes ni la documentación operativa.

## Contenido

| Documento | Contenido |
|---|---|
| [00 — Descripción global](00-descripcion-global.md) | Propósito observable, alcance y límites conocidos. |
| [01 — Actores y roles](01-actores-y-roles.md) | Actores observables y vacíos sobre identidad y permisos. |
| [02 — Requerimientos](02-requerimientos.md) | Capacidades verificables y requisitos pendientes. |
| [03 — Reglas de negocio](03-reglas-de-negocio.md) | Umbrales AML, importación, validación y persistencia. |
| [04 — Diagramas](04-diagramas.md) | Diagramas Mermaid de contexto, componentes y flujos. |
| [05 — Límites del sistema](05-limites-del-sistema.md) | Límites funcionales/técnicos y decisiones por confirmar. |
| [Análisis integrado previo](README-analisis.md) | Síntesis original ampliada y conservada como referencia de análisis. |

## Fuentes de referencia

El análisis se apoya en `README.md`, `Corvux/Program.cs`, `Corvux/Corvux.csproj`, `Corvux/Data/ComplianceDbContext.cs`, `Corvux/Models/`, `Corvux/Services/`, `Corvux/Controllers/TransactionsController.cs` y tests de `Corvux.Tests/`. Para el código vigente prevalece la implementación; las discrepancias con el README se anotan como pendientes, no se resuelven por suposición.

> **[Pendiente] Motor de base de datos:** el README describe SQL Server LocalDB; el proyecto referencia EF Core SQLite y `Program.cs` configura `UseSqlite`. La decisión de proveedor y la reconciliación de README/runbooks requieren confirmación.
