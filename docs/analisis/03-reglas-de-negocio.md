# 03 — Reglas de negocio

Reglas derivadas de `Corvux/Services/ComplianceRuleEngine.cs`, `TransactionService.cs` y `TransactionImportParser.cs`. Los valores aquí reflejan implementación observada, no una política regulatoria validada.

## Evaluación AML

| Regla | Condición exacta en código | Resultado |
|---|---|---|
| Declaración de fondos | `amountUsd > 10_000m` | `RequiresFundsDeclaration = true`. |
| País bloqueado | Coincidencia de origen o destino con `Iran`, `Irán`, `North Korea`, `Corea del Norte` o `DPRK`, sin distinguir mayúsculas/minúsculas; también búsqueda parcial tras recortar espacios. | Bloquea y registra razón indicando origen o destino. Origen tiene precedencia si ambos coinciden. |
| Tasa administrativa | `amountUsd > 50_000m` | `amountUsd * 0.005m`; de otro modo, cero. |

Los límites son estrictos: igualdad con el umbral no satisface la condición correspondiente. En esta clase no se consulta una lista externa ni se realiza I/O. No se observó conversión de moneda; el parámetro y las reglas están expresados en USD.

## Alta y edición

`TransactionFormModel` requiere ID y países no vacíos, limita ID a 40 caracteres, países a 80 y exige monto mayor que cero. `ComplianceDbContext` impone clave primaria, índice único de `TransactionId`, campos requeridos y precisión `18,2` para monto. `TransactionService.SaveAsync` comprueba duplicado antes de escribir; en edición actualiza `UpdatedAtUtc`, en creación inicializa ambas marcas UTC.

## Importación

- La extensión del nombre determina el parser: `.csv` o `.xlsx`; otras extensiones generan error.
- Se requieren `TransactionId`, `AmountUsd`, `OriginCountry` y `DestinationCountry`; se admiten aliases declarados en el parser.
- ID y países no pueden quedar vacíos; el monto debe parsear con cultura invariante y ser positivo.
- CSV se lee línea por línea; XLSX lee `xl/sharedStrings.xml` si existe y `xl/worksheets/sheet1.xml`.
- El servicio no guarda si el parseo tiene errores, no hay filas o hay ID duplicado contra base o dentro del lote. Las comparaciones de duplicados de importación ignoran mayúsculas/minúsculas.
- Si todo valida, se persiste el lote en una operación de guardado; se asigna el mismo instante UTC a creación/actualización de filas importadas.

No extrapolar soporte de varias hojas, límites de archivo, detección de archivos hostiles, CSV multilínea entrecomillado ni procesamiento parcial. La documentación previa `README-analisis.md` los marca como no establecidos; quedan **[Pendiente]** de especificar y probar.

## Discrepancia documental

**[Pendiente]** README declara SQL Server LocalDB, pero `Corvux.csproj` referencia `Microsoft.EntityFrameworkCore.Sqlite` y `Program.cs` configura `UseSqlite`. No modificar proveedor ni presentar LocalDB como comportamiento vigente hasta confirmar la decisión.
