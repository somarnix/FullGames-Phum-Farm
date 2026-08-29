@echo off
setlocal
set "UNITY_EDITOR=C:\Program Files\Unity\Hub\Editor\6000.5.10f1\Editor\Unity.exe"
set "UNITY_PACKAGE_MANAGER=C:\Program Files\Unity\Hub\Editor\6000.5.10f1\Editor\Data\Resources\PackageManager\Server\UnityPackageManager.exe"
set "PROJECT_DIR=%~dp0UnityProject"

if not exist "%UNITY_EDITOR%" (
  echo Unity 6000.5.10f1 is missing.
  pause
  exit /b 1
)
if not exist "%UNITY_PACKAGE_MANAGER%" (
  echo Unity Package Manager is missing. Run Repair-Unity-Then-Launch.bat.
  pause
  exit /b 2
)

start "Phum Farm Unity Editor" "%UNITY_EDITOR%" -force-d3d11 -projectPath "%PROJECT_DIR%" -executeMethod PhumFarm.Editor.ProjectBootstrap.OpenBootScene
endlocal
