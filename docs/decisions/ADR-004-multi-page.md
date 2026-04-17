# ADR-004: Estructura Multi-Página del Dashboard

**Fecha:** 2026-03-12
**Estado:** Aprobado

## Contexto

El componente `Home.razor` original concentraba todo el sistema en una sola página (430 líneas): KPIs, formulario CRUD, importador masivo y tabla de transacciones. Esto dificultaba:

- La navegación directa a secciones específicas (sin URL propia, sin bookmarks)
- La separación de responsabilidades entre vistas
- El crecimiento futuro de cada sección de forma independiente

Los mockups del challenge proponen claramente una arquitectura multi-página con sidebar de navegación y rutas específicas.

## Decisión

Dividir en cuatro páginas Razor con rutas propias:

| Página | Ruta | Responsabilidad |
|---|---|---|
| `VistaGeneral.razor` | `/` | Dashboard de lectura: KPIs, mapa, tabla read-only |
| `GestionarTransacciones.razor` | `/transacciones` | CRUD completo con formulario y tabla editable |
| `CargaMasiva.razor` | `/carga-masiva` | Importación CSV/XLSX con vista previa y validación |
| `Reportes.razor` | `/reportes` | Reportes de compliance, análisis y exportación CSV |

Cada página inyecta sus propios servicios y mantiene su propio estado local. No se introduce estado compartido entre páginas (no hay singleton de sesión).

## Consecuencias

**Positivas:**
- URLs directas y navegables para cada función
- Bottom nav bar en mobile mapea 1:1 con las rutas
- Cada página es independiente y testeable en aislamiento
- El sidebar desktop y el bottom nav mobile son consistentes

**Negativas:**
- Cada página carga sus transacciones de forma independiente (sin caché compartida). Aceptable para el alcance del challenge.
- Tres páginas (VistaGeneral, GestionarTransacciones, Reportes) llaman a `GetTransactionsAsync()` independientemente. Overhead mínimo en LocalDB.
