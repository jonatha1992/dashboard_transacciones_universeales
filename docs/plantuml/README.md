# Corvux AML Dashboard - Estado verificado

## Resultado de la verificacion

Estado actual contra el plan aprobado:

- Cumplido: separacion multi-pagina en `/`, `/transacciones`, `/carga-masiva` y `/reportes`.
- Cumplido: `BottomNavBar.razor` existe y `MainLayout.razor` ya lo renderiza fuera del grid.
- Cumplido: Tailwind CDN ya esta cargado con `preflight: false` y `prefix: 'tw-'`.
- Cumplido: `TransactionMapPoint`, `CountryCoordinateMap` y `CountryCoordinateMapTests` ya existen.
- Cumplido: `dotnet build Corvux/Corvux.csproj` compila sin errores.
- Cumplido: `dotnet test Corvux.Tests/Corvux.Tests.csproj` deja 38 tests en verde.

Pendiente o desalineado:

- Gap 1: `App.razor` sigue usando Leaflet por CDN en lugar de `wwwroot/lib/leaflet`.
- Gap 2: `wwwroot/js/map-interop.js` sigue exponiendo `window.corvuxMap`; no esta implementado como ES module.
- Gap 3: la bottom nav depende de reglas en `MainLayout.razor.css` que se compilan con scope propio del layout. En CSS isolation eso no alcanza al componente hijo. La correccion es usar `::deep .bottom-nav` o mover la visibilidad responsive al CSS aislado de `BottomNavBar`.
- Gap 4: la documentacion Mermaid sigue referenciando `Home.razor`, CDN de Leaflet y nombres viejos de JSInterop.

## Decisiones tomadas

- Decision 1: no tocar servicios, modelos ni tests existentes. La auditoria confirma que la separacion actual de modelos ya es correcta.
- Decision 2: documentar en PlantUML en una carpeta nueva (`docs/plantuml`) para no romper la documentacion Mermaid ya presente mientras se hace la migracion.
- Decision 3: modelar la arquitectura objetivo usando el estado aprobado del plan, no los atajos temporales que todavia quedaron en el codigo.
- Decision 4: mantener `TransactionMapPoint` como unico record liviano para el mapa; no se justifican DTOs nuevos.

## Errores detectados y como se corrigieron

- Error 1: la primera validacion corriendo `dotnet build` y `dotnet test` en paralelo produjo `CS2012` por lock sobre `Corvux.dll`.
  Correccion: ejecutar build y test en secuencia. Resultado final verificado: build OK y test OK.

- Error 2: el CSS responsive de `.bottom-nav` quedo atado al scope de `MainLayout`.
  Correccion propuesta: usar `::deep .bottom-nav` en `MainLayout.razor.css` o trasladar esas reglas al CSS del propio `BottomNavBar`.

- Error 3: la documentacion quedo desfasada respecto del refactor multi-pagina.
  Correccion: se agrega este set PlantUML con los nombres reales de paginas, componentes y decisiones.

- Error 4: la integracion del mapa no cumple todavia el objetivo offline del plan.
  Correccion propuesta: servir `leaflet.css` y `leaflet.js` desde `wwwroot/lib/leaflet` y convertir `map-interop.js` a modulo importado con `IJSObjectReference`.

## Artefactos PlantUML creados

- `use-cases.puml`
- `sequence-diagrams.puml`
- `activity-diagrams.puml`
- `c4-architecture.puml`

## Comandos verificados

```powershell
dotnet build Corvux\Corvux.csproj
dotnet test Corvux.Tests\Corvux.Tests.csproj
```
