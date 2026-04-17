# ADR-005: Integración del Mapa con Leaflet.js y JSInterop

**Fecha:** 2026-03-12
**Estado:** Aprobado

## Contexto

El dashboard requería visualizar la exposición geográfica de las transacciones en un mapa interactivo. El área "PREVISUALIZACIÓN DEL MAPA GLOBAL" existente era un placeholder sin funcionalidad.

Opciones evaluadas:
- **Leaflet.js**: Open-source, zero-dependency, OpenStreetMap tiles gratuitos, CDN público
- **Mapbox GL**: Requiere API key, costoso en producción
- **Google Maps**: Requiere API key y facturación
- **Chart.js / D3.js geo**: Solo útil para mapas de calor estáticos, no mapas interactivos

## Decisión

Usar **Leaflet.js 1.9.4** cargado via CDN en `App.razor` + un módulo JavaScript `wwwroot/js/map-interop.js` + componente Blazor `LeafletMap.razor`.

**Arquitectura del interop:**
- Leaflet.js cargado como script global (no ES module) antes de `blazor.web.js`
- `map-interop.js` expone `window.corvuxMap = { init, updateMarkers, destroy }`
- `LeafletMap.razor` implementa `IAsyncDisposable` para limpiar instancias al navegar
- Coordenadas resueltas en C# via `CountryCoordinateMap` (diccionario estático, sin API externa)
- Datos pasados a JS como array de objetos serializables (sin `IJSObjectReference` de módulo)

**Razón del diccionario estático:**
Las transacciones almacenan países como texto libre. No se usa una API de geocoding (latencia, costo, dependencia externa). El diccionario cubre +60 países y hace match parcial para formatos como "Berlín, DEU".

## Consecuencias

**Positivas:**
- Sin API keys ni costos externos
- Funciona offline (si se vendoriza Leaflet, actualmente CDN)
- Markers coloreados (#ff8d7f bloqueada, #ffd089 declaración, #8bf0a0 aprobada) coinciden con los CSS custom properties del sistema
- `IAsyncDisposable` previene el error "container already initialized" al re-navegar

**Negativas:**
- CDN de Leaflet requiere conexión a internet para la demo (mitigable descargando a `wwwroot/lib/leaflet/`)
- El diccionario de países es manual: si un país ingresado en la UI no está en el dict, no aparece marcador (fallo silencioso, aceptable)
- OpenStreetMap tiles requieren conexión a internet para renderizar el mapa base
