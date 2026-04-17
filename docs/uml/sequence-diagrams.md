# Sequence Diagrams — Corvux AML Compliance Dashboard

Los siguientes diagramas detallan los flujos de interacción entre el navegador (Blazor circuit),
los servicios de la capa de aplicación y la base de datos SQL Server LocalDB.

---

## SD01 — Registrar transacción manual

Cubre tanto el registro (nuevo) como la edición (formModel.Id presente). La diferencia en
`TransactionService.SaveAsync` es si se hace `Add` o se actualiza la entidad existente.

```mermaid
sequenceDiagram
    autonumber
    actor Auditor as Auditor de Cumplimiento
    participant Page as Home.razor (Blazor Server)
    participant EditCtx as EditContext (DataAnnotations)
    participant SVC as TransactionService
    participant RE as ComplianceRuleEngine
    participant DB as ComplianceDbContext (SQL Server LocalDB)

    Auditor->>Page: Completa formulario y hace clic en "Guardar"
    Page->>EditCtx: OnValidSubmit disparado
    EditCtx->>EditCtx: Validar DataAnnotations (Required, StringLength, Range)
    alt Validación fallida
        EditCtx-->>Page: ValidationSummary con errores
        Page-->>Auditor: Muestra errores en pantalla
    else Validación exitosa
        Page->>SVC: SaveAsync(formModel)
        SVC->>DB: CreateDbContextAsync()
        DB-->>SVC: dbContext
        SVC->>DB: AnyAsync(TransactionId == x && Id != y)
        alt TransactionId duplicado
            DB-->>SVC: true
            SVC-->>Page: TransactionCommandResult.Failure("Ya existe…")
            Page-->>Auditor: Banner de error
        else ID único
            DB-->>SVC: false
            alt formModel.Id == null (nuevo)
                SVC->>DB: Transactions.Add(nuevaEntidad)
            else formModel.Id presente (edición)
                SVC->>DB: FirstOrDefaultAsync(Id == key)
                DB-->>SVC: existingTransaction
                SVC->>DB: Actualiza campos de la entidad
            end
            SVC->>DB: SaveChangesAsync()
            DB-->>SVC: OK
            SVC-->>Page: TransactionCommandResult.Success("Transacción registrada/actualizada")
            Page->>SVC: GetTransactionsAsync()
            SVC->>DB: Transactions.AsNoTracking().OrderBy(…).ToListAsync()
            DB-->>SVC: List<TransactionRecord>
            SVC-->>Page: IReadOnlyList<TransactionRecord>
            Page->>RE: Evaluate(transaction) por cada fila
            RE-->>Page: ComplianceEvaluation (flags, fee)
            Page-->>Auditor: Dashboard actualizado con KPIs y tabla
        end
    end
```

---

## SD02 — Importar lote CSV/XLSX

El flujo aplica un **corte preventivo total**: si cualquier fila contiene errores de formato
o duplicidad, no se persiste ningún registro.

```mermaid
sequenceDiagram
    autonumber
    actor Auditor as Auditor de Cumplimiento
    participant Page as Home.razor (Blazor Server)
    participant SVC as TransactionService
    participant Parser as TransactionImportParser
    participant DB as ComplianceDbContext (SQL Server LocalDB)

    Auditor->>Page: Selecciona archivo CSV/XLSX (InputFile)
    Page-->>Auditor: Muestra nombre del archivo seleccionado
    Auditor->>Page: Clic en "Importar lote"
    Page->>SVC: ImportAsync(stream, fileName)

    SVC->>Parser: ParseAsync(stream, fileName)
    Parser->>Parser: Detecta extensión (.csv / .xlsx / otro)

    alt Extensión no soportada
        Parser-->>SVC: TransactionImportResult con error "Formato no soportado"
        SVC-->>Page: TransactionImportExecutionResult.Failure([error])
        Page-->>Auditor: Panel de errores visible
    else CSV o XLSX válido
        Parser->>Parser: Lee encabezados → BuildHeaderMap()
        alt Faltan columnas obligatorias
            Parser-->>SVC: TransactionImportResult con errores de encabezado
            SVC-->>Page: TransactionImportExecutionResult.Failure([errores])
            Page-->>Auditor: Panel de errores visible
        else Encabezados completos
            Parser->>Parser: MapRow() por cada fila de datos
            alt Alguna fila tiene datos inválidos
                Parser-->>SVC: TransactionImportResult con errores por fila
                SVC-->>Page: TransactionImportExecutionResult.Failure([errores])
                Page-->>Auditor: Panel de errores (fila, campo, mensaje)
            else Todas las filas válidas
                Parser-->>SVC: TransactionImportResult{Rows, Errors=[]}
                SVC->>SVC: ValidateRows() — detecta duplicados internos
                SVC->>DB: Obtiene IDs existentes en DB
                DB-->>SVC: HashSet<string> persistedIds
                SVC->>SVC: Cruza Rows vs persistedIds → duplicados externos
                alt Hay errores de negocio
                    SVC-->>Page: TransactionImportExecutionResult.Failure([errores])
                    Page-->>Auditor: Panel de errores con detalle
                else Sin errores
                    SVC->>DB: Transactions.AddRange(nuevasEntidades)
                    SVC->>DB: SaveChangesAsync()
                    DB-->>SVC: OK
                    SVC-->>Page: TransactionImportExecutionResult.Success("N importadas")
                    Page->>SVC: GetTransactionsAsync()
                    SVC->>DB: ToListAsync()
                    DB-->>SVC: registros actualizados
                    SVC-->>Page: IReadOnlyList<TransactionRecord>
                    Page-->>Auditor: Dashboard actualizado
                end
            end
        end
    end
```

---

## SD03 — Cargar mapa geográfico

El mapa resuelve coordenadas de país mediante `CountryCoordinateMap` (diccionario estático)
y las entrega a Leaflet.js vía JSInterop en `OnAfterRenderAsync`.

```mermaid
sequenceDiagram
    autonumber
    actor Auditor as Auditor de Cumplimiento
    participant Page as Home.razor (Blazor Server)
    participant SVC as TransactionService
    participant RE as ComplianceRuleEngine
    participant CCM as CountryCoordinateMap (servicio estatico)
    participant JS as JSInterop (Leaflet.js)

    Auditor->>Page: Navega a la sección de mapa
    Page->>Page: OnAfterRenderAsync(firstRender=true)
    Page->>SVC: GetTransactionsAsync()
    SVC-->>Page: IReadOnlyList<TransactionRecord>

    loop Por cada TransactionRecord
        Page->>RE: Evaluate(transaction)
        RE-->>Page: ComplianceEvaluation
        Page->>CCM: GetCoordinates(OriginCountry)
        CCM-->>Page: (Lat, Lng)? — null si no reconoce el país
        Page->>CCM: GetCoordinates(DestinationCountry)
        CCM-->>Page: (Lat, Lng)?
        Page->>Page: Construye TransactionMapPoint si ambas coords presentes
    end

    Page->>JS: invokeVoidAsync("initLeafletMap", mapPoints[])
    JS->>JS: Inicializa mapa Leaflet con tiles OpenStreetMap
    loop Por cada TransactionMapPoint
        JS->>JS: Agrega marcador origen (color según IsBlocked)
        JS->>JS: Agrega marcador destino
        JS->>JS: Dibuja línea origen → destino
    end
    JS-->>Page: Mapa renderizado en DOM
    Page-->>Auditor: Vista de mapa con marcadores geográficos
```

---

## SD04 — Exportar reporte CSV

El reporte se genera completamente en el lado del cliente mediante un blob URL de JavaScript,
evitando un round-trip al servidor.

```mermaid
sequenceDiagram
    autonumber
    actor Auditor as Auditor de Cumplimiento
    participant Page as Home.razor (Blazor Server)
    participant RE as ComplianceRuleEngine
    participant JS as JSInterop (browser blob download)

    Auditor->>Page: Clic en "Exportar CSV"
    Page->>Page: Itera dashboardRows (ya en memoria)

    loop Por cada DashboardRow
        Page->>RE: Evaluation ya calculada (en DashboardRow)
        Page->>Page: Serializa fila como línea CSV<br/>(TransactionId, AmountUsd, Origin, Destination,<br/>IsBlocked, RequiresDeclaration, AdministrativeFee)
    end

    Page->>Page: Construye contenido CSV completo como string
    Page->>JS: invokeVoidAsync("downloadCsvBlob", fileName, csvContent)
    JS->>JS: new Blob([content], {type:"text/csv"})
    JS->>JS: URL.createObjectURL(blob)
    JS->>JS: Simula clic en <a href=blobUrl download=fileName>
    JS-->>Page: Descarga iniciada en el navegador
    Page-->>Auditor: Archivo CSV descargado localmente
```
