@echo off
rem Clean shutdown for the hosted lane: core + gate + tunnel (see hosting_down.ps1).
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0hosting_down.ps1"
echo.
echo %cmdcmdline% | findstr /i /l /c:"%~f0" >nul 2>&1 && pause
