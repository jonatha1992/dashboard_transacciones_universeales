# Arquitectura General — CorvuxChallenge

## Tipo de arquitectura
Monolito Blazor Server con capa REST API (controllers) expuesta vía Swagger.

## Estructura de carpetas

```
CorvuxChallenge/
├── Components/
│   ├── Pages/Home.razor        # UI principal — dashboard, formulario CRUD, importador
│   ├── Layout/                 # MainLayout, NavMenu
│   └── App.razor               # Raíz de la app Blazor
├── Controllers/
│   └── TransactionsController  # REST API — GET/POST/PUT/DELETE + /import
├── Data/
│   └── ComplianceDbContext     # DbContext EF Core — tabla Transactions
├── Models/
│   ├── TransactionRecord       # Entidad EF Core (propiedades requeridas por EF)
│   ├── TransactionFormModel    # Form binding Blazor (propiedades + DataAnnotations)
│   ├── ComplianceEvaluation    # record — resultado de evaluación de reglas
│   ├── TransactionCommandResult# record — resultado de operaciones CRUD
│   ├── TransactionImportRow    # record — fila parseada de CSV/XLSX
│   ├── TransactionImportResult # class — resultado del parser (campos públicos)
│   ├── TransactionImportExecutionResult # record — resultado del import completo
│   └── ImportError             # record — error de validación con fila y campo
├── Services/
│   ├── TransactionService      # Orquesta CRUD e import — usa IDbContextFactory
│   ├── ComplianceRuleEngine    # Evalúa reglas AML sobre una transacción
│   └── TransactionImportParser # Parsea CSV y XLSX sin dependencias externas
├── Samples/
│   └── transactions-sample.csv # Archivo de ejemplo para importación masiva
└── wwwroot/                    # Assets estáticos — app.css, bootstrap
```

## Flujo de datos

```
Browser
  │
  ├── Blazor Server (SignalR)
  │     └── Home.razor
  │           ├── TransactionService   →  ComplianceDbContext  →  SQL Server
  │           ├── ComplianceRuleEngine →  (sin estado, puro)
  │           └── TransactionImportParser → (sin estado, puro)
  │
  └── REST API (HTTP)
        └── TransactionsController
              ├── TransactionService   →  ComplianceDbContext  →  SQL Server
              └── ComplianceRuleEngine →  (sin estado, puro)
```

## Reglas de compliance (ComplianceRuleEngine)

| Condición | Efecto |
|---|---|
| Monto > USD 10.000 | Requiere declaración de fondos |
| Origen o destino: Irán / Corea del Norte | Operación bloqueada |
| Monto > USD 50.000 | Tasa administrativa 0.5% |

## Base de datos

- Motor: SQL Server LocalDB (desarrollo)
- Inicialización: `EnsureCreated()` al arrancar — crea la tabla si no existe
- Tabla: `Transactions` — campos: Id, TransactionId (único, max 40), AmountUsd, OriginCountry, DestinationCountry, CreatedAtUtc, UpdatedAtUtc

## Decisiones arquitectónicas clave

- `IDbContextFactory` en lugar de `AddDbContext` scoped — correcto para Blazor Server donde el circuito vive toda la sesión
- `TransactionRecord` mantiene propiedades — EF Core necesita propiedades para LINQ→SQL
- `TransactionFormModel` mantiene propiedades — Blazor EditForm + DataAnnotationsValidator las requiere
- El resto de modelos usa records o campos públicos para ser más directos y legibles
