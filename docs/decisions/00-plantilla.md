# Decisiones Arquitectónicas

## ADR-001 — Mantener Blazor Server (monolito) en lugar de separar en WASM + API

- Fecha: 2026-03-10
- Estado: Aprobado

**Contexto:** Se evaluó separar el proyecto en Blazor WASM (frontend) + ASP.NET Core Web API (backend).

**Decisión:** Mantener Blazor Server como monolito. Agregar controllers REST con Swagger para acceso externo.

**Razones:**
- Blazor Server tiene SSR real. WASM corre en el browser — pierde esa ventaja.
- La escala del proyecto no justifica la complejidad de dos deployments separados.
- Agregar controllers REST al monolito da acceso programático sin duplicar infraestructura.

**Trade-offs:** Blazor Server escala peor horizontalmente (SignalR stateful). Si el proyecto crece mucho en usuarios concurrentes, revisar.

---

## ADR-002 — IDbContextFactory en lugar de AddDbContext scoped

- Fecha: 2026-03-10
- Estado: Aprobado

**Contexto:** En Blazor Server el circuito (conexión SignalR) vive toda la sesión del usuario — puede ser horas. Un DbContext scoped viviría igual de largo, acumulando datos obsoletos y consumiendo memoria.

**Decisión:** Usar `IDbContextFactory<ComplianceDbContext>` — cada operación crea y descarta su propio contexto.

---

## ADR-003 — Sin propiedades en modelos (excepto EF y Blazor)

- Fecha: 2026-03-10
- Estado: Aprobado

**Decisión:**
- Modelos inmutables → `record` con parámetros posicionales
- Modelos mutables simples → campos públicos
- `TransactionRecord` → propiedades (EF Core)
- `TransactionFormModel` → propiedades (Blazor EditForm)

---

## Plantilla para nuevas decisiones

## ADR-XXX — [Título]
- Fecha:
- Estado: [Propuesto / Aprobado / Rechazado / Deprecado]

**Contexto:**

**Decisión:**

**Razones:**

**Trade-offs:**
