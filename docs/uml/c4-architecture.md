# C4 Architecture Model — Corvux AML Compliance Dashboard

Modelo C4 a tres niveles usando Mermaid. Convención de colores:
- Azul oscuro → sistema principal
- Azul claro → actor / usuario
- Gris → sistema externo
- Verde → contenedor o componente interno

Referencias: [C4 Model](https://c4model.com/) · ADR-002 (IDbContextFactory Blazor Server)

---

## Level 1 — System Context

Muestra el sistema como una caja negra con sus actores y sistemas externos.

```mermaid
C4Context
    title System Context — Sentinel Compliance Engine

    Person(auditor, "Auditor de Cumplimiento", "Registra, revisa y audita transacciones financieras internacionales")

    System(corvux, "Sentinel Compliance Engine", "Dashboard AML Blazor Server. Registra transacciones, evalúa compliance y soporta importación masiva CSV/XLSX.")

    System_Ext(sqlserver, "SQL Server LocalDB", "Motor de base de datos relacional. Almacena la tabla Transactions.")
    System_Ext(browser, "Navegador Web", "Renderiza la UI Blazor Server y ejecuta JavaScript (Leaflet.js).")

    Rel(auditor, corvux, "Opera desde el navegador", "HTTP / SignalR WebSocket")
    Rel(corvux, sqlserver, "Lee y escribe transacciones", "EF Core 9 / TCP")
    Rel(corvux, browser, "Envía HTML y eventos UI", "Blazor Server Circuit")
```

---

## Level 2 — Containers

Descompone el sistema en sus contenedores de ejecución.

```mermaid
C4Container
    title Containers — Sentinel Compliance Engine

    Person(auditor, "Auditor de Cumplimiento")

    System_Boundary(corvux, "Sentinel Compliance Engine (.NET 9)") {
        Container(blazor, "Blazor Server App", ".NET 9 / ASP.NET Core", "Renderiza la SPA, gestiona el Blazor circuit por SignalR. Sirve componentes Razor interactivos.")

        Container(api, "REST API (Swagger)", "ASP.NET Core MVC Controllers", "Endpoints RESTful para integración external: GET/POST/PUT/DELETE /api/transactions y POST /api/transactions/import")

        Container(services, "Application Services", "C# Scoped Services", "TransactionService, ComplianceRuleEngine, TransactionImportParser, CountryCoordinateMap")

        Container(data, "Data Layer", "EF Core 9 / IDbContextFactory", "ComplianceDbContext. EnsureCreated() en startup. Patrón factory para circuito Blazor.")
    }

    System_Ext(sqlserver, "SQL Server LocalDB", "Tabla Transactions\n(TransactionId UNIQUE, AmountUsd,\nOriginCountry, DestinationCountry,\nCreatedAtUtc, UpdatedAtUtc)")
    System_Ext(leaflet, "Leaflet.js (CDN)", "Mapa interactivo de marcadores\ngeográficos vía JSInterop")

    Rel(auditor, blazor, "Navega y opera el dashboard", "HTTPS / SignalR")
    Rel(auditor, api, "Consume API directamente o via Swagger UI", "HTTPS / JSON")
    Rel(blazor, services, "Llama a servicios inyectados", "In-process / DI")
    Rel(api, services, "Llama a servicios inyectados", "In-process / DI")
    Rel(services, data, "Crea contextos y ejecuta queries", "IDbContextFactory<ComplianceDbContext>")
    Rel(data, sqlserver, "Lee y escribe registros", "EF Core / TDS")
    Rel(blazor, leaflet, "Inicializa mapa y pasa puntos", "JSInterop (invokeVoidAsync)")
```

---

## Level 3 — Components

Descompone los contenedores internos en sus componentes de código.

### Components — Blazor Server App

```mermaid
C4Component
    title Components — Blazor Server App

    Container_Boundary(blazor, "Blazor Server App") {
        Component(app, "App.razor", "Razor Component", "Punto de entrada. Configura Routes\ny HeadOutlet.")

        Component(layout, "MainLayout.razor", "Razor Layout", "Shell visual: sidebar NavMenu\ny área de contenido principal.")

        Component(nav, "NavMenu.razor", "Razor Component", "Barra de navegación lateral.\nEnlace a 'Compliance dashboard'.")

        Component(home, "Home.razor\n(@page \"/\")", "Razor Page (Interactive Server)", "Página principal. Contiene:\n· KPI cards (operaciones, declaraciones, bloqueadas, fees)\n· Formulario CRUD (EditForm + DataAnnotationsValidator)\n· Panel de carga masiva (InputFile)\n· Tabla de transacciones con compliance badges\n· Panel de errores de importación")

        Component(routes, "Routes.razor", "Razor Component", "Configura el enrutador Blazor.")
    }

    Container_Ext(services, "Application Services", "C# Services")
    Container_Ext(js, "JavaScript / Leaflet", "JSInterop")

    Rel(app, layout, "Usa como layout")
    Rel(layout, nav, "Renderiza")
    Rel(layout, home, "Renderiza vía @Body")
    Rel(home, services, "Inyecta TransactionService\ny ComplianceRuleEngine", "@inject")
    Rel(home, js, "Invoca funciones JS\npara mapa y blob download", "IJSRuntime")
    Rel(app, routes, "Usa")
```

### Components — Application Services

```mermaid
C4Component
    title Components — Application Services

    Container_Boundary(services, "Application Services") {
        Component(txsvc, "TransactionService", "Scoped C# Service", "Orquesta operaciones CRUD y la importación masiva.\nMétodos: GetTransactionsAsync, SaveAsync,\nDeleteAsync, ImportAsync.\nUsa IDbContextFactory para crear contextos cortos.")

        Component(engine, "ComplianceRuleEngine", "Scoped C# Service", "Motor de reglas AML.\nMétodo: Evaluate(amountUsd, origin, destination).\nReglas:\n· > $10 000 → RequiresFundsDeclaration\n· País bloqueado → IsBlocked\n· > $50 000 → AdministrativeFee (0,5%)")

        Component(parser, "TransactionImportParser", "Scoped C# Service", "Parsea CSV y XLSX sin dependencias externas.\nCSV: lectura línea a línea con soporte de comillas.\nXLSX: descomprime ZIP, lee XML (xl/worksheets, sharedStrings).\nAliases de encabezados en español e inglés.")

        Component(ccmap, "CountryCoordinateMap", "Static C# Service", "Diccionario estático de 80+ países/ciudades\na coordenadas (Lat, Lng).\nBúsqueda exacta y parcial por nombre normalizado.\nIncluye países bloqueados AML (Iran, Corea del Norte).")
    }

    Container_Ext(data, "Data Layer", "EF Core / IDbContextFactory")

    Rel(txsvc, parser, "Delega parseo de archivos a", "ParseAsync(stream, fileName)")
    Rel(txsvc, data, "Crea DbContext por operación", "IDbContextFactory.CreateDbContextAsync()")
    Rel(engine, engine, "Sin estado externo.\nEvalúa en memoria pura")
    Rel(ccmap, ccmap, "Diccionario estático inmutable.\nSin inyección de dependencias")
```

### Components — Data Layer

```mermaid
C4Component
    title Components — Data Layer

    Container_Boundary(data, "Data Layer") {
        Component(factory, "IDbContextFactory\n<ComplianceDbContext>", "EF Core Factory", "Registrado como Singleton por AddDbContextFactory.\nCrea instancias cortas de DbContext por operación.\nEvita conflictos con el circuito Blazor (ADR-002).")

        Component(ctx, "ComplianceDbContext", "EF Core DbContext", "DbSet<TransactionRecord> Transactions.\nConfigura tabla, PK, índice único en TransactionId,\nprecisión decimal(18,2) y longitudes máximas.")

        Component(record, "TransactionRecord", "EF Core Entity (class)", "Entidad persistida.\nCampos: Id (PK), TransactionId (UNIQUE, max 40),\nAmountUsd (decimal 18,2), OriginCountry (max 80),\nDestinationCountry (max 80),\nCreatedAtUtc, UpdatedAtUtc.")
    }

    Container_Ext(sqlserver, "SQL Server LocalDB")

    Rel(factory, ctx, "Crea instancias de")
    Rel(ctx, record, "Mapea a tabla Transactions")
    Rel(ctx, sqlserver, "EnsureCreated() en startup;\noperaciones CRUD vía LINQ", "EF Core / TDS")
```

### Components — REST API

```mermaid
C4Component
    title Components — REST API (TransactionsController)

    Container_Boundary(api, "REST API") {
        Component(ctrl, "TransactionsController", "ASP.NET Core ApiController\n[Route: api/transactions]", "Expone operaciones CRUD y de importación:\nGET  /api/transactions        → lista con compliance\nPOST /api/transactions        → crear\nPUT  /api/transactions/{id}   → editar\nDELETE /api/transactions/{id} → eliminar\nPOST /api/transactions/import → importar archivo")

        Component(swagger, "Swagger UI", "Swashbuckle / OpenAPI", "Disponible en /swagger.\nPermite probar todos los endpoints\ndirectamente desde el navegador.")
    }

    Container_Ext(services, "Application Services")

    Rel(ctrl, services, "Invoca TransactionService\ny ComplianceRuleEngine", "Constructor injection")
    Rel(swagger, ctrl, "Documenta e invoca endpoints")
```

---

## Resumen de decisiones arquitectónicas reflejadas en el modelo

| Decisión | Impacto visible en el C4 |
|---|---|
| **IDbContextFactory** (ADR-002) | Data Layer usa factory en vez de DbContext scoped para sobrevivir al circuito Blazor |
| **Blazor Server** (no WASM) | El circuito SignalR mantiene estado en servidor; JSInterop necesario para operaciones DOM puras |
| **Corte preventivo en import** | `TransactionService.ImportAsync` valida completamente antes de abrir transacción de escritura |
| **Sin migraciones EF** | `EnsureCreated()` en startup; apropiado para SQL Server LocalDB en desarrollo |
| **Servicios Scoped** | `ComplianceRuleEngine`, `TransactionImportParser`, `TransactionService` son Scoped — un circuito = un scope |
| **CountryCoordinateMap estático** | Cero I/O, cero DI; datos geográficos inmutables que no justifican base de datos |
