# ADR-006: Tailwind CSS via CDN con Prefijo para Componentes Nuevos

**Fecha:** 2026-03-12
**Estado:** Aprobado

## Contexto

El challenge requería integrar Tailwind CSS para dar "tonos" a los componentes del dashboard. El proyecto ya tiene un sistema CSS maduro:
- `app.css` (527 líneas de custom properties, utility classes, componentes)
- Bootstrap 5 como reset base
- Scoped CSS por componente (`.razor.css`)

Opciones de integración:
1. **Tailwind CDN Play** — script tag, sin build pipeline
2. **npm + PostCSS build** — pipeline Node.js integrado al build de .NET
3. **Tailwind como reemplazo total** — reescribir `app.css`

## Decisión

Usar **Tailwind CSS v3 Play CDN** con:
- `prefix: 'tw-'` → todas las utilities requieren el prefijo (`tw-flex`, `tw-grid`, etc.)
- `corePlugins: { preflight: false }` → no inyecta CSS reset (Bootstrap ya lo cubre)

Tailwind se aplica **exclusivamente a componentes nuevos**: `Reportes.razor`, `BottomNavBar.razor`, `LeafletMap.razor`. Los componentes existentes mantienen sus clases de `app.css`.

## Consecuencias

**Positivas:**
- Cero conflictos con Bootstrap y el CSS custom existente (gracias al prefix y preflight desactivado)
- Sin pipeline Node.js extra — el proyecto sigue siendo `dotnet run` puro
- Componentes nuevos se benefician de utilities de layout (`tw-grid`, `tw-flex`, `tw-gap-*`)

**Negativas:**
- Tailwind Play CDN (~400KB) se carga en cada request (no tree-shakeable en modo CDN)
- Para producción, se necesitaría un build pipeline (`npx tailwindcss`) para generar solo las clases usadas
- El archivo CSS generado dinámicamente por Tailwind CDN puede tardar algunos ms en el primer render

**Deuda técnica documentada:**
Si el proyecto crece, se recomienda como ADR-007 migrar al build pipeline de Tailwind con `@tailwindcss/cli` y un task en el `.csproj` pre-build.
