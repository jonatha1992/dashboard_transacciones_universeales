# 🛡️ Sentinel Compliance Engine — Corvux AML Dashboard

> Dashboard de cumplimiento anti-lavado de activos (AML) construido con **Blazor Server (.NET 9)**, **Entity Framework Core** y **SQL Server**. Permite registrar, auditar, importar y visualizar transacciones financieras internacionales con evaluación automática de reglas de compliance.

---

## 📋 Tabla de Contenidos

- [Características Principales](#-características-principales)
- [Arquitectura](#-arquitectura)
- [Stack Tecnológico](#-stack-tecnológico)
- [Estructura del Proyecto](#-estructura-del-proyecto)
- [Requisitos Previos](#-requisitos-previos)
- [Instalación y Setup Local](#-instalación-y-setup-local)
- [Ejecución](#-ejecución)
- [Tests](#-tests)
- [API REST (Swagger)](#-api-rest-swagger)
- [Reglas de Compliance](#-reglas-de-compliance)
- [Páginas del Dashboard](#-páginas-del-dashboard)
- [Importación Masiva](#-importación-masiva)
- [Mapa Geográfico Interactivo](#-mapa-geográfico-interactivo)
- [Scripts de Desarrollo](#-scripts-de-desarrollo)
- [Problemas Conocidos](#-problemas-conocidos)
- [Documentación Adicional](#-documentación-adicional)
- [Licencia](#-licencia)

---

## ✨ Características Principales

| Funcionalidad | Descripción |
|---|---|
| **CRUD de Transacciones** | Crear, editar y eliminar transacciones con validación en tiempo real |
| **Motor de Compliance AML** | Evaluación automática de reglas anti-lavado sobre cada transacción |
| **Importación Masiva** | Carga batch de transacciones desde archivos CSV y XLSX |
| **Dashboard de KPIs** | Indicadores en tiempo real: total de operaciones, declaraciones requeridas, operaciones bloqueadas, tasas administrativas |
| **Mapa Geográfico** | Visualización interactiva con Leaflet.js de la exposición geográfica de las transacciones |
| **Reportes y Exportación** | Generación de reportes de compliance con exportación a CSV |
| **API REST + Swagger** | Endpoints RESTful documentados para integración con sistemas externos |
| **Diseño Responsive** | Interfaz adaptable a desktop y mobile con bottom nav bar |

---

## 🏗️ Arquitectura

**Monolito Blazor Server** con capa REST API expuesta vía Swagger.

```
Browser
  │
  ├── Blazor Server (SignalR)
  │     └── Razor Pages
  │           ├── TransactionService   →  ComplianceDbContext  →  SQL Server
  │           ├── ComplianceRuleEngine  →  (sin estado, puro)
  │           └── TransactionImportParser → (sin estado, puro)
  │
  └── REST API (HTTP)
        └── TransactionsController
              ├── TransactionService   →  ComplianceDbContext  →  SQL Server
              └── ComplianceRuleEngine →  (sin estado, puro)
```

### Decisiones Arquitectónicas Clave

| Decisión | Justificación |
|---|---|
| `IDbContextFactory` en lugar de DbContext scoped | Correcto para Blazor Server donde el circuito SignalR vive toda la sesión |
| Blazor Server (no WASM) | El estado se mantiene en servidor; reducción de carga en cliente |
| `EnsureCreated()` en startup | Apropiado para desarrollo con SQL Server LocalDB, sin migraciones EF |
| Servicios Scoped | `ComplianceRuleEngine`, `TransactionImportParser`, `TransactionService` — un circuito = un scope |
| `CountryCoordinateMap` estático | Cero I/O, cero DI; datos geográficos inmutables sin necesidad de base de datos |
| Tailwind CSS con prefijo `tw-` | Coexistencia con Bootstrap y CSS custom existente sin conflictos |

> 📖 Para más detalle, ver los ADRs en [`docs/decisions/`](docs/decisions/).

---

## 🛠️ Stack Tecnológico

| Capa | Tecnología |
|---|---|
| **Frontend** | Blazor Server (Razor Components), Tailwind CSS v3 (CDN), Bootstrap 5 |
| **Backend** | .NET 9, ASP.NET Core, C# |
| **Base de Datos** | SQL Server LocalDB, Entity Framework Core 9 |
| **Mapa Interactivo** | Leaflet.js 1.9.4 (OpenStreetMap tiles) vía JSInterop |
| **API** | ASP.NET Core MVC Controllers + Swagger (Swashbuckle) |
| **Testing** | xUnit (38 tests) |

---

## 📁 Estructura del Proyecto

```
Corvux/                            # Proyecto principal
├── Components/
│   ├── Pages/                     # Páginas Razor del dashboard
│   │   ├── VistaGeneral.razor     # Dashboard KPIs, mapa, tabla read-only
│   │   ├── GestionarTransacciones.razor  # CRUD completo
│   │   ├── CargaMasiva.razor      # Importación CSV/XLSX
│   │   └── Reportes.razor         # Reportes de compliance, exportación CSV
│   ├── Layout/                    # MainLayout, NavMenu, BottomNavBar
│   └── Shared/                    # Componentes compartidos (LeafletMap, etc.)
├── Controllers/
│   └── TransactionsController.cs  # REST API — GET/POST/PUT/DELETE + /import
├── Data/
│   ├── ComplianceDbContext.cs     # DbContext EF Core — tabla Transactions
│   └── Migrations/               # Migraciones EF Core
├── Models/
│   ├── TransactionRecord.cs       # Entidad EF Core
│   ├── TransactionFormModel.cs    # Form binding Blazor + DataAnnotations
│   ├── ComplianceEvaluation.cs    # Resultado de evaluación de reglas
│   ├── TransactionCommandResult.cs # Resultado de operaciones CRUD
│   ├── TransactionImportRow.cs    # Fila parseada de CSV/XLSX
│   ├── TransactionImportResult.cs # Resultado del parser
│   ├── TransactionImportExecutionResult.cs # Resultado del import completo
│   ├── ImportError.cs             # Error de validación con fila y campo
│   └── TransactionMapPoint.cs    # Punto geográfico para el mapa
├── Services/
│   ├── TransactionService.cs      # Orquesta CRUD e import (IDbContextFactory)
│   ├── ComplianceRuleEngine.cs    # Evalúa reglas AML
│   ├── TransactionImportParser.cs # Parsea CSV y XLSX sin dependencias externas
│   └── CountryCoordinateMap.cs    # Diccionario estático de coordenadas (80+ países)
├── wwwroot/
│   ├── js/
│   │   └── map-interop.js        # JSInterop para Leaflet.js
│   └── lib/                      # Librerías estáticas
├── appsettings.json               # Configuración (connection string, etc.)
└── Corvux.csproj                  # Archivo de proyecto .NET

Corvux.Tests/                      # Proyecto de tests unitarios
├── ComplianceRuleEngineTests/     # 17 tests de reglas AML
└── TransactionImportParserTests/  # 21 tests del parser

Samples/                           # Archivos de ejemplo
└── transactions-sample.csv        # CSV de ejemplo para importación masiva

docs/                              # Documentación técnica
├── architecture/                  # Arquitectura general
├── decisions/                     # ADRs (Architecture Decision Records)
├── plantuml/                      # Diagramas PlantUML
├── runbooks/                      # Guías de operación
├── troubleshooting/               # Problemas conocidos
└── uml/                           # Diagramas UML (C4, casos de uso, secuencia, actividad)

tools/
└── scripts/
    └── dev-reset.ps1              # Script de reset del entorno de desarrollo
```

---

## 📦 Requisitos Previos

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- **SQL Server LocalDB** (incluido con Visual Studio, o instalar el paquete standalone)

### Verificar LocalDB

```bash
sqllocaldb info
```

Si no aparece la instancia `MSSQLLocalDB`:

```bash
sqllocaldb create MSSQLLocalDB
sqllocaldb start MSSQLLocalDB
```

---

## 🚀 Instalación y Setup Local

### 1. Clonar el repositorio

```bash
git clone https://github.com/jonatha1992/dashboard_transacciones_universeales.git
cd dashboard_transacciones_universeales
```

### 2. Restaurar dependencias

```bash
dotnet restore Corvux/Corvux.csproj
dotnet restore Corvux.Tests/Corvux.Tests.csproj
```

### 3. Configurar la base de datos

La connection string ya está configurada en `Corvux/appsettings.json`. La base de datos se **crea automáticamente** al arrancar la aplicación (`EnsureCreated()`).

Para usar otra instancia de SQL Server, editar la connection string en `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "ComplianceDb": "Server=.\\MSSQLSERVER2;Database=onecore;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False;TrustServerCertificate=True"
  }
}
```

---

## ▶️ Ejecución

```bash
dotnet run --project Corvux/Corvux.csproj --launch-profile http
```

| Recurso | URL |
|---|---|
| 🖥️ Dashboard | http://localhost:5180 |
| 📘 Swagger UI | http://localhost:5180/swagger |

---

## 🧪 Tests

```bash
dotnet test Corvux.Tests/Corvux.Tests.csproj --verbosity minimal
```

**Cobertura de tests:**

| Suite | Tests | Descripción |
|---|---|---|
| `ComplianceRuleEngineTests` | 17 | Reglas AML: montos, países bloqueados, tasas administrativas |
| `TransactionImportParserTests` | 21 | Parseo de CSV/XLSX, validación de encabezados, manejo de errores |
| **Total** | **38** | |

---

## 📘 API REST (Swagger)

La API está disponible en `/api/transactions` y documentada en Swagger UI (`/swagger`).

| Método | Endpoint | Descripción |
|---|---|---|
| `GET` | `/api/transactions` | Lista todas las transacciones con evaluación de compliance |
| `POST` | `/api/transactions` | Crear una nueva transacción |
| `PUT` | `/api/transactions/{id}` | Editar una transacción existente |
| `DELETE` | `/api/transactions/{id}` | Eliminar una transacción |
| `POST` | `/api/transactions/import` | Importar un archivo CSV/XLSX de transacciones |

### Ejemplo: Importar vía cURL

```bash
curl -X POST http://localhost:5180/api/transactions/import \
  -F "file=@Samples/transactions-sample.csv"
```

---

## ⚖️ Reglas de Compliance

El `ComplianceRuleEngine` evalúa automáticamente cada transacción según las siguientes reglas AML:

| Condición | Efecto |
|---|---|
| Monto > **USD 10.000** | ⚠️ Requiere declaración de origen de fondos |
| País de origen o destino: **Irán** o **Corea del Norte** | 🚫 Operación **bloqueada** |
| Monto > **USD 50.000** | 💰 Tasa administrativa del **0,5%** |

---

## 📄 Páginas del Dashboard

| Página | Ruta | Descripción |
|---|---|---|
| **Vista General** | `/` | Dashboard de lectura: KPIs, mapa geográfico interactivo, tabla de transacciones read-only |
| **Gestionar Transacciones** | `/transacciones` | CRUD completo con formulario y tabla editable |
| **Carga Masiva** | `/carga-masiva` | Importación de archivos CSV/XLSX con vista previa y validación |
| **Reportes** | `/reportes` | Reportes de compliance, análisis y exportación a CSV |

Cada página inyecta sus propios servicios y mantiene estado local independiente.

---

## 📤 Importación Masiva

Soporta archivos **CSV** y **XLSX** con las siguientes columnas obligatorias:

| Columna | Aliases aceptados |
|---|---|
| `TransactionId` | ID, transaction_id |
| `AmountUsd` | Amount, amount_usd, Monto |
| `OriginCountry` | Origin, origin_country, País Origen |
| `DestinationCountry` | Destination, destination_country, País Destino |

### Características del importador:
- ✅ Parseo de CSV línea a línea con soporte de comillas
- ✅ Parseo de XLSX nativo (descomprime ZIP, lee XML) — **sin dependencias externas**
- ✅ Validación completa antes de persistir (fallo total si hay errores)
- ✅ Detección de duplicados contra IDs existentes en la base de datos
- ✅ Reporte detallado de errores por fila y campo

### Archivo de ejemplo

```
Samples/transactions-sample.csv
```

---

## 🗺️ Mapa Geográfico Interactivo

El dashboard incluye un mapa interactivo implementado con **Leaflet.js** que visualiza la exposición geográfica de las transacciones:

- 🟢 **Verde** (`#8bf0a0`): Transacciones aprobadas
- 🟡 **Amarillo** (`#ffd089`): Requieren declaración de fondos
- 🔴 **Rojo** (`#ff8d7f`): Operaciones bloqueadas

Las coordenadas se resuelven en C# mediante `CountryCoordinateMap`, un diccionario estático que cubre **80+ países** con búsqueda exacta y parcial por nombre normalizado. No se utiliza ninguna API de geocoding externa.

---

## 🔧 Scripts de Desarrollo

### Reset del entorno

Mata procesos colgados, limpia artefactos de build y vuelve a compilar:

```powershell
.\tools\scripts\dev-reset.ps1
```

### Build completo

```bash
dotnet build Corvux/Corvux.csproj
```

---

## ⚠️ Problemas Conocidos

| Problema | Causa | Solución |
|---|---|---|
| Puerto en uso al reiniciar | Proceso anterior quedó colgado en puerto 5180 | `Stop-Process -Name Corvux -Force` |
| Base de datos no se crea al arrancar | LocalDB no está corriendo | `sqllocaldb start MSSQLLocalDB` |
| Build falla con "file is locked" | Instancia anterior sigue corriendo | `Stop-Process -Name Corvux -Force` |

> 📖 Ver [`docs/troubleshooting/known_issues.md`](docs/troubleshooting/known_issues.md) para más detalle.

---

## 📚 Documentación Adicional

| Documento | Descripción |
|---|---|
| [`docs/architecture/0-overview.md`](docs/architecture/0-overview.md) | Arquitectura general del sistema |
| [`docs/decisions/`](docs/decisions/) | ADRs (Architecture Decision Records) |
| [`docs/uml/c4-architecture.md`](docs/uml/c4-architecture.md) | Modelo C4 a tres niveles |
| [`docs/uml/use-cases.md`](docs/uml/use-cases.md) | Casos de uso con diagramas Mermaid |
| [`docs/uml/sequence-diagrams.md`](docs/uml/sequence-diagrams.md) | Diagramas de secuencia |
| [`docs/uml/activity-diagrams.md`](docs/uml/activity-diagrams.md) | Diagramas de actividad |
| [`docs/runbooks/setup-local.md`](docs/runbooks/setup-local.md) | Guía detallada de setup local |
| [`docs/troubleshooting/known_issues.md`](docs/troubleshooting/known_issues.md) | Bugs conocidos y soluciones |

---

## 📊 Modelo de Datos

### Tabla `Transactions`

| Campo | Tipo | Restricciones |
|---|---|---|
| `Id` | `int` | PK, autoincremental |
| `TransactionId` | `nvarchar(40)` | UNIQUE, obligatorio |
| `AmountUsd` | `decimal(18,2)` | Obligatorio |
| `OriginCountry` | `nvarchar(80)` | Obligatorio |
| `DestinationCountry` | `nvarchar(80)` | Obligatorio |
| `CreatedAtUtc` | `datetime2` | Generado al crear |
| `UpdatedAtUtc` | `datetime2` | Actualizado en cada edición |

---

## 📝 Licencia

Este proyecto fue desarrollado como parte del **Corvux Challenge** — desafío técnico de desarrollo de un dashboard de compliance AML.

## Configuration

`SentinelAI:APIKey` is intentionally empty in `appsettings.json`. Provide it at
runtime through the environment instead of committing it:

```bash
# Linux / macOS
export SentinelAI__APIKey="your-key"

# Windows (PowerShell)
$env:SentinelAI__APIKey = "your-key"
```

ASP.NET Core maps the `__` separator to the configuration hierarchy, so the
environment variable overrides the empty value without any code change.
