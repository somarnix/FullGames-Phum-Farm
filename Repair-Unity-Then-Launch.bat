@echo off
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Repair-Unity-6000.5.10f1.ps1"
if errorlevel 1 (
  echo Unity repair did not complete.
  pause
  exit /b 1
)
start "Phum Farm Unity" "C:\Program Files\Unity\Hub\Editor\6000.5.10f1\Editor\Unity.exe" -force-d3d11 -projectPath "%~dp0UnityProject"
endlocal
