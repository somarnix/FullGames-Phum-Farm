@echo off
setlocal
set "PHUM_UNITY=C:\Users\Sophanaroth Lem\UnityEditors\6000.5.10f1-clean\Editor\Unity.exe"
set "PHUM_PROJECT=%~dp0UnityProject"
if not exist "%PHUM_UNITY%" (
  echo Clean Unity editor was not found:
  echo %PHUM_UNITY%
  pause
  exit /b 1
)
start "Phum Farm" "%PHUM_UNITY%" -projectPath "%PHUM_PROJECT%"
endlocal
