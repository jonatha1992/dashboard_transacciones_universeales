# Known Issues

## Puerto en uso al reiniciar durante desarrollo

- Contexto: Al matar el proceso con Ctrl+C y volver a correr `dotnet run`, falla con "address already in use"
- Causa: El proceso anterior quedó colgado (PID bloqueando el puerto 5180)
- Solución: Desde PowerShell: `Stop-Process -Name CorvuxChallenge -Force`, luego volver a correr

---

## La base de datos no se crea al arrancar

- Contexto: Error al iniciar — "Cannot open database" o similar
- Causa: LocalDB no está corriendo o la instancia MSSQLLocalDB no existe
- Solución:
  ```bash
  sqllocaldb start MSSQLLocalDB
  ```
  Si la instancia no existe: `sqllocaldb create MSSQLLocalDB`

---

## Build falla con "file is locked by another process"

- Contexto: `dotnet build` falla porque el .exe está bloqueado
- Causa: La instancia anterior de la app sigue corriendo en background
- Solución: `Stop-Process -Name CorvuxChallenge -Force` antes de buildear

---

## Plantilla para nuevos bugs

## [Nombre del Error/Bug]
- Contexto/Cuándo ocurre:
- Intentos fallidos:
- Solución final:
