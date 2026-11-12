# 01 — Actores y roles

## Actores observables

| Actor | Evidencia en el producto | Capacidades observables |
|---|---|---|
| Persona usuaria de la interfaz | Páginas Razor descritas en README: vista general, gestión, carga masiva y reportes. | Consultar el dashboard, gestionar registros, iniciar importaciones y consultar/exportar reportes según las páginas documentadas. |
| Cliente de API | `TransactionsController` expone `/api/transactions`. | Consumir operaciones CRUD e importación desde HTTP. |
| Operador/desarrollador local | README y configuración de ejecución local. | Ejecutar la aplicación y las pruebas en entorno de desarrollo. |

La implementación examinada no permite afirmar que estos actores correspondan a identidades autenticadas; son categorías de interacción, no roles de seguridad.

## Roles y autorización

No se observó una definición documentada de roles, permisos por acción ni proveedor de identidad. El controlador inspeccionado no declara políticas ni atributos de autorización. Esto describe la evidencia de ese controlador, no una garantía de configuración de seguridad integral.

**[Pendiente]** Confirmar si la aplicación requiere autenticación/autorización, quién administra usuarios y qué acciones deben restringirse. No interpretar «operador», «analista» o «administrador» como roles implementados.

## Interacciones principales

- La UI utiliza servicios de aplicación para operaciones de transacción.
- Clientes externos pueden invocar la API; la disponibilidad y controles de acceso por entorno deben verificarse antes de exponerla.
- La importación recibe un archivo y devuelve resultado de operación/errores.

Ver [04 — Diagramas](04-diagramas.md) para los flujos derivados.
