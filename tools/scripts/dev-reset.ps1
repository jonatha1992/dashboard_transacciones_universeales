# dev-reset.ps1
# Mata la app si está corriendo, limpia bin/obj y vuelve a buildear.
# Útil cuando el proceso queda colgado o hay artefactos corruptos.
#
# Uso: .\tools\scripts\dev-reset.ps1

Write-Host "Deteniendo instancias de CorvuxChallenge..."
Stop-Process -Name "CorvuxChallenge" -Force -ErrorAction SilentlyContinue

Write-Host "Limpiando bin/ y obj/..."
Remove-Item -Recurse -Force CorvuxChallenge/bin  -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force CorvuxChallenge/obj  -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force CorvuxChallenge.Tests/bin -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force CorvuxChallenge.Tests/obj -ErrorAction SilentlyContinue

Write-Host "Restaurando paquetes..."
dotnet restore CorvuxChallenge/CorvuxChallenge.csproj --verbosity quiet

Write-Host "Buildeando..."
dotnet build CorvuxChallenge/CorvuxChallenge.csproj --verbosity quiet

Write-Host "Corriendo tests..."
dotnet test CorvuxChallenge.Tests/CorvuxChallenge.Tests.csproj --verbosity minimal

Write-Host "Listo. Para levantar la app:"
Write-Host "  dotnet run --project CorvuxChallenge/CorvuxChallenge.csproj --launch-profile http"
