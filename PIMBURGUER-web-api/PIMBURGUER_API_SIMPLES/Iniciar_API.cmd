@echo off
cd /d "%~dp0API"
echo PIM Burguer - API web e aplicativo
echo Swagger: http://localhost:5181/swagger
curl.exe --silent --fail --max-time 3 http://localhost:5181/mobile/api/saude >nul 2>&1
if not errorlevel 1 (
  echo A API ja esta em execucao na porta 5181.
  pause
  exit /b 0
)
echo Para encerrar, pressione Ctrl+C nesta janela.
dotnet run --launch-profile http
pause
