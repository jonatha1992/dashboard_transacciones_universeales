# 05 — Límites del sistema

Este documento registra límites observables y temas no especificados. No implica un diseño futuro ni decisiones aprobadas.

## Límites funcionales observados

- El dominio persistido identificado es `TransactionRecord`: identificador de transacción, monto USD, origen/destino y timestamps UTC.
- El motor implementa un conjunto fijo de alias de países bloqueados y dos umbrales monetarios; no se observó integración con una fuente de sanciones externa.
- El parser acepta extensiones CSV/XLSX y un conjunto fijo de encabezados. XLSX se resuelve leyendo `sheet1.xml`; no asumir selección de hojas ni fórmulas.
- La importación es de todo o nada a nivel de validaciones del servicio: errores de parseo/duplicados detienen el guardado del lote.
- El mapa se describe como datos estáticos de coordenadas de países y Leaflet. No se observó geocodificación externa.

## Límites técnicos observados

- Aplicación ASP.NET Core/.NET 9, EF Core, componentes Razor interactivos de servidor y controladores REST.
- Persistencia vía `IDbContextFactory<ComplianceDbContext>` y `EnsureCreatedAsync()` al iniciar; no equivale a aplicar migraciones.
- Swagger se registra, pero middleware/UI se activa solamente en Development.
- El controlador examinado no declara atributos de autorización; los requisitos de seguridad quedan por confirmar y no se infiere una postura completa solo a partir de ese archivo.

## Discrepancias y pendientes

1. **[Pendiente] Proveedor de base:** README y runbooks refieren SQL Server/LocalDB; `Corvux.csproj` referencia SQLite y `Program.cs` usa `UseSqlite`. Confirmar proveedor soportado y reconciliar documentos, configuración y pruebas. No alterar silenciosamente el código.
2. **[Pendiente] Migraciones/esquema:** README menciona una carpeta Migrations, pero el flujo observado al inicio es `EnsureCreatedAsync()`. Confirmar qué artefactos/flujo se mantienen antes de documentar actualización de esquema.
3. **[Pendiente] Seguridad:** especificar identidad, autorización para UI/API, exposición de Swagger y protección de información financiera.
4. **[Pendiente] Operación:** despliegue, retención, backup/restore, observabilidad y gestión de secretos requieren fuente de verdad operativa.
5. **[Pendiente] Importaciones:** límites de tamaño, límites ZIP, hojas compatibles, formatos CSV, concurrencia y tratamiento de fallos requieren definición.
6. **[Pendiente] Cumplimiento:** autoridad regulatoria, procedencia y ciclo de actualización de reglas/listas, revisión humana y auditoría formal no están establecidos.

## Referencia heredada integrada

`README-analisis.md` se conserva íntegro como síntesis previa. Sus observaciones sobre seguridad, operación, importación, auditoría y divergencias se mantienen como análisis, no como requisitos confirmados. El contenido nuevo distribuye la información por tema y el índice de esta carpeta proporciona navegación.
