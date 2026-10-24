# S2 arcade capture launcher
# Reads runtime identity from a non-committed file (default: install-root s2-local.txt
# with lines EA_URL=..., PCBID=..., CARD0=..., PIN=...) and launches the shipping
# build in arcade mode against a real provider, with the logging mod injected.
# Do NOT commit the identity file or this script's console output: they carry PCBID/card.
param(
  [string]$LocalFile = '\\MYAPPSANDBOX\c\Games\MFG\s2-local.txt',
  [string]$InstallRoot = '\\MYAPPSANDBOX\c\Games\MFG',
  [switch]$NoMod,
  [switch]$NoApi
)
$cfg = @{}
Get-Content $LocalFile | ForEach-Object { $kv = $_ -split '=', 2; if ($kv.Count -eq 2) { $cfg[$kv[0].Trim()] = $kv[1].Trim() } }
foreach ($k in 'EA_URL','PCBID','CARD0') { if (-not $cfg[$k]) { throw "s2-local.txt missing key $k" } }

$gone = Get-Process -Name spice64 -ErrorAction SilentlyContinue
if ($gone) { "stopping existing game instance(s): $($gone.Id -join ',')"; Stop-Process -Name spice64 -Force; Start-Sleep -Seconds 4 }

$argv = @('-url', $cfg['EA_URL'], '-p', $cfg['PCBID'], '-card0', $cfg['CARD0'], '-w', '-windowborder', '2')
if (-not $NoApi)  { $argv += @('-api', '1337') }
if (-not $NoMod)  { $argv += @('-k', 'doorstop.dll') }
# -netdump is deliberately absent: it was implicated in process crashes during S2.

Remove-Item "$InstallRoot\dev\raw\log\output_log.txt" -ErrorAction SilentlyContinue
Remove-Item "$InstallRoot\BepInEx\LogOutput.log" -ErrorAction SilentlyContinue

$p = Start-Process -FilePath "$InstallRoot\spice64.exe" -ArgumentList $argv -WorkingDirectory $InstallRoot -PassThru
"game pid=$($p.Id) (args kept in this process only)"

$logFile = "$InstallRoot\dev\raw\log\output_log.txt"
for ($i = 0; $i -lt 24; $i++) {
  Start-Sleep -Seconds 25
  if (-not (Get-Process -Id $p.Id -ErrorAction SilentlyContinue)) { "poll $i : PROCESS GONE"; break }
  $hit = Get-Content $logFile -ErrorAction SilentlyContinue | Select-String 'cardmng.inquire' | Select-Object -First 1
  if ($hit) { 'poll {0} : card inquire seen - cabinet is on the card flow' -f $i; break }
}
'next manual steps: F1 (credit) -> tap attract -> insert card handled by -card0/-api -> PIN on scrambled pad -> login chain.'
