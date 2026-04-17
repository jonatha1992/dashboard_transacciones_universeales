# Activity Diagrams — Corvux AML Compliance Dashboard

Diagramas de actividad en formato `flowchart TD` (Mermaid) para los seis casos de uso.
Los colores de nodo siguen la convención: inicio/fin en círculo, decisión en rombo, proceso en rectángulo.

---

## AD01 — Registrar transacción manual

```mermaid
flowchart TD
    START(( Inicio ))
    FORM["Auditor completa formulario (TransactionId, AmountUsd, Origin, Destination)"]
    SUBMIT["Clic en 'Guardar transacción'"]
    VAL_DA{DataAnnotations validas?}
    SHOW_FORM_ERR["Mostrar errores de validación en ValidationSummary"]
    SVC_SAVE["TransactionService.SaveAsync(formModel)"]
    CHECK_DUP{TransactionId ya existe en DB?}
    SHOW_DUP_ERR["Mostrar banner de error: Ya existe una transacción con ese ID"]
    PERSIST["Crear nueva entidad TransactionRecord dbContext.Transactions.Add() SaveChangesAsync()"]
    RELOAD["GetTransactionsAsync() Reevaluar compliance con ComplianceRuleEngine"]
    UPDATE_UI["Actualizar KPIs y tabla del dashboard Resetear formulario"]
    END(( Fin ))

    START --> FORM
    FORM --> SUBMIT
    SUBMIT --> VAL_DA
    VAL_DA -- No --> SHOW_FORM_ERR
    SHOW_FORM_ERR --> FORM
    VAL_DA -- Sí --> SVC_SAVE
    SVC_SAVE --> CHECK_DUP
    CHECK_DUP -- Sí --> SHOW_DUP_ERR
    SHOW_DUP_ERR --> FORM
    CHECK_DUP -- No --> PERSIST
    PERSIST --> RELOAD
    RELOAD --> UPDATE_UI
    UPDATE_UI --> END
```

---

## AD02 — Editar transacción

```mermaid
flowchart TD
    START(( Inicio ))
    SEL["Auditor hace clic en 'Editar'\nen una fila de la tabla"]
    PRELOAD["Precargar formulario con\nTransactionFormModel.FromEntity()"]
    MODIFY["Auditor modifica uno o más campos"]
    SUBMIT["Clic en 'Actualizar transacción'"]
    VAL_DA{DataAnnotations\nválidas?}
    SHOW_ERR["Mostrar errores de validación"]
    SVC_SAVE["TransactionService.SaveAsync(formModel)\n(formModel.Id presente → modo edición)"]
    FIND_REC{Registro encontrado\nen DB por Id?}
    NOT_FOUND["Retornar Failure:\n'No se encontró la transacción'"]
    CHECK_DUP{Nuevo TransactionId\npertenece a otra fila?}
    DUP_ERR["Mostrar banner de error:\n'Ya existe una transacción con ese ID'"]
    UPDATE["Actualizar campos de la entidad\nSaveChangesAsync()"]
    RELOAD["Recargar transacciones\ny reevaluar compliance"]
    UPDATE_UI["Actualizar KPIs, tabla\nResetear formulario"]
    END(( Fin ))

    START --> SEL
    SEL --> PRELOAD
    PRELOAD --> MODIFY
    MODIFY --> SUBMIT
    SUBMIT --> VAL_DA
    VAL_DA -- No --> SHOW_ERR
    SHOW_ERR --> MODIFY
    VAL_DA -- Sí --> SVC_SAVE
    SVC_SAVE --> FIND_REC
    FIND_REC -- No --> NOT_FOUND
    NOT_FOUND --> UPDATE_UI
    FIND_REC -- Sí --> CHECK_DUP
    CHECK_DUP -- Sí --> DUP_ERR
    DUP_ERR --> MODIFY
    CHECK_DUP -- No --> UPDATE
    UPDATE --> RELOAD
    RELOAD --> UPDATE_UI
    UPDATE_UI --> END
```

---

## AD03 — Eliminar transacción

```mermaid
flowchart TD
    START(( Inicio ))
    CLICK["Auditor hace clic en 'Eliminar'\nen una fila de la tabla"]
    SVC_DEL["TransactionService.DeleteAsync(id)"]
    FIND{Registro encontrado\nen DB?}
    NOT_FOUND["Retornar Failure:\n'La transacción ya no existe'"]
    REMOVE["dbContext.Transactions.Remove()\nSaveChangesAsync()"]
    EDITING{¿El formulario\ntenía ese registro?}
    RESET_FORM["Resetear formulario\na estado vacío"]
    RELOAD["GetTransactionsAsync()\nReevaluar compliance"]
    UPDATE_UI["Actualizar KPIs y tabla"]
    SHOW_MSG["Mostrar banner informativo"]
    END(( Fin ))

    START --> CLICK
    CLICK --> SVC_DEL
    SVC_DEL --> FIND
    FIND -- No --> NOT_FOUND
    NOT_FOUND --> SHOW_MSG
    SHOW_MSG --> END
    FIND -- Sí --> REMOVE
    REMOVE --> EDITING
    EDITING -- Sí --> RESET_FORM
    RESET_FORM --> RELOAD
    EDITING -- No --> RELOAD
    RELOAD --> UPDATE_UI
    UPDATE_UI --> END
```

---

## AD04 — Importar lote CSV/XLSX

```mermaid
flowchart TD
    START(( Inicio ))
    SELECT["Auditor selecciona archivo\nCSV o XLSX (InputFile)"]
    CLICK_IMPORT["Clic en 'Importar lote'"]
    SVC_IMPORT["TransactionService.ImportAsync(stream, fileName)"]
    CHECK_EXT{Extensión\n.csv o .xlsx?}
    ERR_EXT["ImportError: 'Formato no soportado'"]
    PARSE["TransactionImportParser.ParseAsync()"]
    CHECK_HEADERS{Encabezados\nobligatorios presentes?}
    ERR_HDR["ImportErrors: columnas faltantes\no duplicadas"]
    MAP_ROWS["MapRow() por cada fila\n(tipos, nulos, rangos)"]
    CHECK_ROWS{Alguna fila\ncon error?}
    ERR_ROWS["ImportErrors: detalle por fila y campo"]
    VAL_BIZ["ValidateRows() — duplicados internos\nen el archivo"]
    CHECK_DUP_INT{Duplicados\ninternos?}
    ERR_DUP_INT["ImportErrors: IDs repetidos\ndentro del archivo"]
    LOAD_DB_IDS["Cargar IDs existentes\nde Transactions (DB)"]
    CHECK_DUP_EXT{Duplicados\nvs. DB?}
    ERR_DUP_EXT["ImportErrors: IDs ya existentes\nen la base de datos"]
    PERSIST["AddRange(nuevasEntidades)\nSaveChangesAsync()"]
    RELOAD["GetTransactionsAsync()\nReevaluar compliance"]
    UPDATE_UI["Actualizar KPIs y tabla\nLimpiar archivo seleccionado"]
    SHOW_ERRORS["Mostrar panel de errores\n(sin persistencia)"]
    END(( Fin ))

    START --> SELECT
    SELECT --> CLICK_IMPORT
    CLICK_IMPORT --> SVC_IMPORT
    SVC_IMPORT --> CHECK_EXT
    CHECK_EXT -- No --> ERR_EXT
    ERR_EXT --> SHOW_ERRORS
    SHOW_ERRORS --> END
    CHECK_EXT -- Sí --> PARSE
    PARSE --> CHECK_HEADERS
    CHECK_HEADERS -- No --> ERR_HDR
    ERR_HDR --> SHOW_ERRORS
    CHECK_HEADERS -- Sí --> MAP_ROWS
    MAP_ROWS --> CHECK_ROWS
    CHECK_ROWS -- Sí --> ERR_ROWS
    ERR_ROWS --> SHOW_ERRORS
    CHECK_ROWS -- No --> VAL_BIZ
    VAL_BIZ --> CHECK_DUP_INT
    CHECK_DUP_INT -- Sí --> ERR_DUP_INT
    ERR_DUP_INT --> SHOW_ERRORS
    CHECK_DUP_INT -- No --> LOAD_DB_IDS
    LOAD_DB_IDS --> CHECK_DUP_EXT
    CHECK_DUP_EXT -- Sí --> ERR_DUP_EXT
    ERR_DUP_EXT --> SHOW_ERRORS
    CHECK_DUP_EXT -- No --> PERSIST
    PERSIST --> RELOAD
    RELOAD --> UPDATE_UI
    UPDATE_UI --> END
```

---

## AD05 — Visualizar mapa geográfico

```mermaid
flowchart TD
    START(( Inicio ))
    NAV["Auditor navega a la sección de mapa"]
    AFTER_RENDER["OnAfterRenderAsync(firstRender=true)"]
    LOAD_TXN["TransactionService.GetTransactionsAsync()"]
    ITER["Iterar sobre cada TransactionRecord"]
    EVAL["ComplianceRuleEngine.Evaluate()\n→ ComplianceEvaluation"]
    GET_ORIGIN["CountryCoordinateMap.GetCoordinates(OriginCountry)"]
    CHECK_ORI{Coordenadas\nde origen?}
    GET_DEST["CountryCoordinateMap.GetCoordinates(DestinationCountry)"]
    CHECK_DEST{Coordenadas\nde destino?}
    SKIP["Omitir marcador\n(país no reconocido)"]
    BUILD_POINT["Construir TransactionMapPoint\n(IsBlocked, RequiresFundsDeclaration)"]
    MORE{¿Más\ntransacciones?}
    JS_INIT["JSInterop: invokeVoidAsync('initLeafletMap', points[])"]
    RENDER_MAP["Leaflet.js renderiza mapa\ncon marcadores y líneas de ruta"]
    END(( Fin ))

    START --> NAV
    NAV --> AFTER_RENDER
    AFTER_RENDER --> LOAD_TXN
    LOAD_TXN --> ITER
    ITER --> EVAL
    EVAL --> GET_ORIGIN
    GET_ORIGIN --> CHECK_ORI
    CHECK_ORI -- No --> SKIP
    SKIP --> MORE
    CHECK_ORI -- Sí --> GET_DEST
    GET_DEST --> CHECK_DEST
    CHECK_DEST -- No --> SKIP
    CHECK_DEST -- Sí --> BUILD_POINT
    BUILD_POINT --> MORE
    MORE -- Sí --> ITER
    MORE -- No --> JS_INIT
    JS_INIT --> RENDER_MAP
    RENDER_MAP --> END
```

---

## AD06 — Consultar reporte

```mermaid
flowchart TD
    START(( Inicio ))
    NAV["Auditor navega a la sección de reportes"]
    LOAD["Datos ya en memoria (dashboardRows)"]
    CALC_KPIS["Calcular KPIs:\n· Total operaciones\n· Requieren declaración (> $10 000)\n· Bloqueadas (países restringidos)\n· Suma de tasas administrativas (0,5%)"]
    SHOW_KPIS["Mostrar KPI cards en pantalla"]
    EXPORT_CHOICE{Auditor solicita\nexportar CSV?}
    BUILD_CSV["Serializar dashboardRows\ncomo texto CSV"]
    JS_BLOB["JSInterop: downloadCsvBlob(fileName, csvContent)"]
    DOWNLOAD["Navegador descarga\narchivo .csv localmente"]
    END_EXPORT(( Fin exportación ))
    END_VIEW(( Fin visualización ))

    START --> NAV
    NAV --> LOAD
    LOAD --> CALC_KPIS
    CALC_KPIS --> SHOW_KPIS
    SHOW_KPIS --> EXPORT_CHOICE
    EXPORT_CHOICE -- No --> END_VIEW
    EXPORT_CHOICE -- Sí --> BUILD_CSV
    BUILD_CSV --> JS_BLOB
    JS_BLOB --> DOWNLOAD
    DOWNLOAD --> END_EXPORT
```
