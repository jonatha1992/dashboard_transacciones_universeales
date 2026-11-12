# 04 — Diagramas

Diagramas de comportamiento observado, no contratos de integración completos. Se escriben en Mermaid.

## Contexto

```mermaid
flowchart LR
    U[Persona usuaria] --> UI[Interfaz Blazor interactiva]
    C[Cliente HTTP] --> API[TransactionsController\n/api/transactions]
    UI --> TS[TransactionService]
    API --> TS
    TS --> DB[(ComplianceDbContext / EF Core)]
    TS --> P[TransactionImportParser]
    API --> E[ComplianceRuleEngine]
    UI --> E
    DB --> SQLITE[(SQLite configurado en código)]
```

**[Pendiente]** README documenta SQL Server LocalDB en lugar de SQLite; el diagrama representa el proveedor configurado en `Program.cs`, no una decisión aprobada.

## Importación

```mermaid
sequenceDiagram
    actor U as Usuario / cliente
    participant I as UI o API
    participant S as TransactionService
    participant P as TransactionImportParser
    participant D as ComplianceDbContext
    U->>I: Enviar archivo CSV/XLSX
    I->>S: ImportAsync(stream, fileName)
    S->>P: ParseAsync(stream, fileName)
    P-->>S: Filas y errores de parseo
    alt Error de parseo o lote vacío
        S-->>I: Fallo, cero registros guardados
    else Parser sin errores
        S->>D: Consultar TransactionId existentes
        S->>S: Detectar IDs repetidos en DB y lote
        alt Hay duplicados
            S-->>I: Fallo, cero registros guardados
        else Lote válido
            S->>D: AddRange y SaveChangesAsync
            S-->>I: Éxito y cantidad importada
        end
    end
```

## Evaluación de transacción

```mermaid
flowchart TD
    A[Transacción: monto USD y países] --> B{Monto > 10.000?}
    B -->|Sí| C[Requiere declaración]
    B -->|No| D[No requiere declaración]
    C --> E{Origen/destino coincide con país bloqueado?}
    D --> E
    E -->|Sí| F[Bloqueada; guardar razón]
    E -->|No| G[No bloqueada]
    F --> H{Monto > 50.000?}
    G --> H
    H -->|Sí| J[Tasa = monto × 0,005]
    H -->|No| K[Tasa = 0]
    J --> L[Resultado de evaluación]
    K --> L
```

La evaluación devuelve un resultado; no afirmar que por sí sola persiste ni impide una transacción. Los efectos concretos dependen de su consumidor.
