# 02 — Requerimientos

Este catálogo separa capacidades observadas (**C**) de asuntos por decidir (**P**). No constituye una especificación aprobada ni agrega funcionalidades no presentes en las fuentes.

## Capacidades observadas

| ID | Capacidad | Criterio observable | Fuente principal |
|---|---|---|---|
| C-01 | Consultar transacciones | `GET /api/transactions` devuelve registros con evaluación de compliance. | `TransactionsController.GetAll` |
| C-02 | Crear y actualizar transacciones | POST/PUT reciben `TransactionFormModel`; el servicio valida duplicado de ID y persiste cambios. | `TransactionsController`, `TransactionService` |
| C-03 | Eliminar transacción | DELETE delega al servicio; resultado inexistente se informa como NotFound. | `TransactionsController.Delete` |
| C-04 | Evaluar compliance | El motor marca declaración si monto > USD 10.000, bloquea coincidencia de país configurado y calcula 0,5% si monto > USD 50.000. | `ComplianceRuleEngine` |
| C-05 | Importar archivo | Parser admite extensiones `.csv` y `.xlsx`; valida columnas/datos y servicio rechaza el lote entero ante errores o duplicados. | `TransactionImportParser`, `TransactionService.ImportAsync` |
| C-06 | Mostrar dashboard y reportes | README documenta páginas `/`, `/transacciones`, `/carga-masiva`, `/reportes`, KPIs, mapa y exportación CSV. | `README.md`, componentes Razor |
| C-07 | Persistir datos | EF Core configura entidad `Transactions` y unicidad de `TransactionId`; startup llama a `EnsureCreatedAsync()`. | `ComplianceDbContext`, `Program.cs` |
| C-08 | Proveer API documentada en desarrollo | Swagger se activa si `app.Environment.IsDevelopment()`. | `Program.cs` |

## Pendientes de definición

| ID | Pregunta abierta |
|---|---|
| P-01 | ¿SQLite es el proveedor deseado o el README debe actualizarse a la configuración vigente? El README dice LocalDB, el código configura SQLite. |
| P-02 | ¿Qué actores, autenticación, autorización y permisos son necesarios? |
| P-03 | ¿Qué fuente/autoridad, revisión y actualización deben tener las reglas y listas de países? |
| P-04 | ¿Qué obligaciones de auditoría, retención, respaldo, cifrado y privacidad aplican? |
| P-05 | ¿Qué límites de tamaño, concurrencia y protección de archivos de importación se requieren? |
| P-06 | ¿Qué estrategia de evolución del esquema/despliegue se desea frente al `EnsureCreatedAsync()` observado? |
| P-07 | ¿Qué formato de errores y códigos HTTP deben considerarse contrato estable de la API? |

Los pendientes requieren decisión del equipo; no son requisitos confirmados. Para reglas concretas, véase [03 — Reglas de negocio](03-reglas-de-negocio.md).
