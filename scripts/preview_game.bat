@echo off
rem preview_game.bat - double-click launcher for the interactive game preview.
rem args: -Reassemble | -Park | -Stop   (see preview_game.ps1 header)
setlocal
set PS=pwsh
where pwsh >nul 2>nul || set PS=powershell
"%PS%" -NoProfile -ExecutionPolicy Bypass -File "%~dp0preview_game.ps1" %*
if errorlevel 1 pause
endlocal
