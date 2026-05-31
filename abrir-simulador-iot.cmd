@echo off
setlocal

cd /d "%~dp0"

title GranjaTech - Simulador IoT Manual

echo.
echo ==========================================
echo   GranjaTech - Simulador IoT Manual
echo ==========================================
echo.

where docker >nul 2>nul
if errorlevel 1 (
  echo Docker nao foi encontrado no PATH.
  echo Instale ou abra o Docker Desktop e tente novamente.
  echo.
  pause
  exit /b 1
)

docker info >nul 2>nul
if errorlevel 1 (
  echo Docker nao esta respondendo.
  echo Abra o Docker Desktop, aguarde ele iniciar e execute este arquivo de novo.
  echo.
  pause
  exit /b 1
)

if "%POSTGRES_HOST_PORT%"=="" set "POSTGRES_HOST_PORT=15432"

echo Iniciando banco, MQTT, backend e frontend...
docker compose up -d --build postgres mqtt-broker backend frontend
if errorlevel 1 (
  echo.
  echo Nao foi possivel subir o ambiente Docker.
  echo Verifique a mensagem acima e tente novamente.
  echo.
  pause
  exit /b 1
)

echo.
echo Pausando o simulador automatico para voce controlar os valores manualmente...
docker compose stop iot-simulator >nul 2>nul

echo.
echo Frontend: http://localhost:3000
echo Status IoT: http://localhost:5099/api/iot/status
echo.
echo Quando o menu abrir:
echo   1 = Temperatura
echo   2 = Umidade
echo   3 = Luminosidade
echo Digite o valor e pressione Enter.
echo.

docker compose run --rm iot-manual-simulator

echo.
if /I "%GRANJATECH_SKIP_RESTART_PROMPT%"=="1" goto end

choice /C SN /N /M "Deseja ligar o simulador automatico novamente? [S/N]: "
if errorlevel 2 goto end

echo.
echo Religando simulador automatico...
docker compose up -d iot-simulator

:end
echo.
echo Finalizado.
if /I "%GRANJATECH_NO_PAUSE%"=="1" exit /b 0
pause
