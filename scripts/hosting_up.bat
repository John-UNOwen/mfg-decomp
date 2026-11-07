@echo off
setlocal EnableExtensions
rem Hosting lane launcher (same posture as hosting_up.ps1, docs/28):
rem core + WebFront gate, loopback binds throughout.
rem Usage: hosting_up.bat [local]   default = full public stack (core + gate + cloudflared).
rem "local" skips the tunnel and its posture flags (LAN/dev shape only).
rem The servers run inside THIS window: keep it open while the lane serves,
rem press Ctrl+C in it to stop core + gate (and tunnel). Logs: C:\mfgtmp\*.out/err.txt.

rem --- clean posture: never inherit lane env from earlier sessions ---
for %%N in (WEBFRONT_ROOT WEBFRONT_UPSTREAM WEBFRONT_AUTH WEBFRONT_PORT WEBFRONT_BIND WEBFRONT_GATE WEBFRONT_TRUST_PROXY WEBFRONT_PRESENTER_SECRET WEBFRONT_PRESENTER_SECRET_FILE WEBFRONT_ACCESS_CODE WEBFRONT_ACCESS_CODE_FILE WEBFRONT_PAGE_FRONTURL mfg_http_bind mfg_http_port VFG_PRESENTER_SECRET WEB_SECURE_COOKIE) do set "%%N="

set "RC=0"
set "MODE=tunnel"
if /i "%~1"=="local" set "MODE=local"
set "REPO=C:\mfg"
rem the core walks stack frames against its cwd-derived plugins path: launch from the
rem REAL repo, never through the C:\mfg junction (KB: stack-path checks break under junction cwd)
for /f "delims=" %%I in ('powershell -NoProfile -Command "$i=Get-Item -LiteralPath 'C:\mfg' -EA SilentlyContinue; if($i -and $i.Target){$i.Target}else{'C:\mfg'}"') do set "REPO=%%I"
if not exist "%REPO%\tmp\RyuNET-core\dist" set "REPO=%~dp0.."
set "RYU_DIST=%REPO%\tmp\RyuNET-core\dist"
set "WF_DIR=%REPO%\webfront\bin\Release\net8.0"
set "SEC_FILE=%USERPROFILE%\.mfg-web\presenter.secret"
set "CODE_FILE=%USERPROFILE%\.mfg-web\access.code"
set "CF_EXE=C:\PROGRA~2\cloudflared\cloudflared.exe"

if not exist "%SEC_FILE%" (
  echo presenter secret missing: create %SEC_FILE% first - see scripts\hosting_deploy.md
  set RC=1
  goto end
)
set /p PRESENTER_SECRET=<"%SEC_FILE%"

rem --- refuse to double-start the core only (a live core means the server is still up).
rem Read-only, and it stays the FIRST action of the script: the reclaim below used to run
rem ahead of it, so a refusal killed the running gate and tunnel and then aborted, leaving
rem the core up with no front and no tunnel (checkup 2026-11-06 item 12, same order flip
rem as hosting_up.ps1) ---
netstat -ano | findstr /r /c:":8083 .*LISTENING" >nul 2>&1 && (echo core already answers 8083 - the server is still up - close it first or run scripts\hosting_down.ps1& set RC=1& goto end)

rem --- reclaim leftover lane processes (gates/tunnels started detached by chain
rem restarts or earlier sessions used to block every re-run of this script). Only
rem reached when nothing answers 8083, so what is left here has no core behind it.
rem From the pid records only, as hosting_up.ps1 does: this used to signal every
rem WebFront.exe and every cloudflared.exe on the box by image name, which also
rem reaches another lane's front and the operator's own tunnel - checkup 2026-11-06
rem item 12, ledger L66. hosting_down.ps1 stops exactly the pids recorded in
rem C:\mfgtmp\lane_pids after re-proving each identity, and exits 1 on anything it
rem cannot prove, so an unrecorded leftover is reported, never killed ---
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0hosting_down.ps1"
if errorlevel 1 (echo leftover lane processes were not reclaimed - hosting_down.ps1 refused. Stop the pids it printed, or adopt one into C:\mfgtmp\lane_pids, then run this again& set RC=1& goto end)

rem --- core: presenter armed, native listener loopback-only, https-ready cookies on tunnel shape ---
if exist "%RYU_DIST%\config.ini" powershell -NoProfile -Command "(Get-Content -LiteralPath '%RYU_DIST%\config.ini' -Raw) -replace 'mfg_http_bind=[^\r\n]*','mfg_http_bind=127.0.0.1' | Set-Content -LiteralPath '%RYU_DIST%\config.ini' -Encoding UTF8 -NoNewline"
set "mfg_http_bind=127.0.0.1"
set "VFG_PRESENTER_SECRET=%PRESENTER_SECRET%"
if /i "%MODE%"=="tunnel" set "WEB_SECURE_COOKIE=1"
if not exist "C:\mfgtmp" mkdir "C:\mfgtmp"
start /b "mfg-core" cmd /c "cd /d ""%RYU_DIST%"" && node AsphyxiaCore.js > C:\mfgtmp\ryunet.out.txt 2> C:\mfgtmp\ryunet.err.txt"

set /a TRY=0
:waitcore
netstat -ano | findstr /r /c:":8083 .*LISTENING" >nul 2>&1 && goto coreup
set /a TRY+=1
if %TRY% geq 60 (echo core did not bind 8083 - see C:\mfgtmp\ryunet.err.txt& set RC=1& goto end)
ping -n 2 127.0.0.1 >nul
goto waitcore
:coreup
echo core up: presenter armed, mfg_http_bind=127.0.0.1

rem --- webfront gate ---
set "WEBFRONT_ROOT=C:\mfgtmp\webroot"
set "WEBFRONT_UPSTREAM=http://127.0.0.1:8083"
set "WEBFRONT_AUTH=http://127.0.0.1:8083"
set "WEBFRONT_PORT=8090"
set "WEBFRONT_BIND=loopback"
set "WEBFRONT_GATE=1"
set "WEBFRONT_PRESENTER_SECRET_FILE=%SEC_FILE%"
if /i "%MODE%"=="tunnel" set "WEBFRONT_TRUST_PROXY=cloudflare"
if exist "%CODE_FILE%" set "WEBFRONT_ACCESS_CODE_FILE=%CODE_FILE%"
start /b "mfg-webfront" cmd /c "cd /d ""%WF_DIR%"" && WebFront.exe > C:\mfgtmp\webfront8090.out.txt 2> C:\mfgtmp\webfront8090.err.txt"

set /a TRY=0
:waitgate
netstat -ano | findstr /r /c:":8090 .*LISTENING" >nul 2>&1 && goto gateup
set /a TRY+=1
if %TRY% geq 30 (echo webfront did not bind 8090 - see C:\mfgtmp\webfront8090.err.txt& set RC=1& goto end)
ping -n 2 127.0.0.1 >nul
goto waitgate
:gateup
echo webfront gate up on 127.0.0.1:8090

if "%MODE%"=="tunnel" call :starttunnel
if errorlevel 1 goto end

echo lane running in this window - press Ctrl+C to stop core, gate and tunnel.

rem --- hold: stay attached so Ctrl+C reaches the servers (same console group) ---
:hold
set COREUP=0
set GATEUP=0
netstat -ano | findstr /r /c:":8083 .*LISTENING" >nul 2>&1 && set COREUP=1
netstat -ano | findstr /r /c:":8090 .*LISTENING" >nul 2>&1 && set GATEUP=1
if %COREUP%==0 if %GATEUP%==0 (echo core and gate have exited.& goto end)
ping -n 3 127.0.0.1 >nul
goto hold

:end
echo.
rem keep the window open only when launched by double-click (not from a console)
echo %cmdcmdline% | findstr /i /l /c:"%~f0" >nul 2>&1 && pause
exit /b %RC%

:starttunnel
if not exist %CF_EXE% goto notunnelexe
start /b "mfg-tunnel" cmd /c %CF_EXE% tunnel run mfg > C:\mfgtmp\cloudflared.out.txt 2> C:\mfgtmp\cloudflared.err.txt
echo cloudflared tunnel starting - logs in C:\mfgtmp\cloudflared.out.txt
exit /b 0
:notunnelexe
echo cloudflared not found at %CF_EXE% - tunnel skipped
exit /b 1
