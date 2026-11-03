# server_status.ps1 - the only honest source for a "server left running (pid N)" claim.
#
# Reports used to write that sentence from memory; at the start of the last check
# nothing was listening on 8083 at all. Run this before quoting a pid, or let a
# sweep's own "server ... LISTENING pid=N" line be the evidence.
#
# usage: pwsh .audit/server_status.ps1 [-Ports 8083,8093]
# Default list = the ports that exist in this tree: 8083/22421 integrated core (dist),
# 8093/22431 the staged verify-dist lane (pwsh scripts\ryunet_stage.ps1 -Action lane),
# 8090 webfront, 8094 static front, 8080 mock. Retired lane ports (8095/8096/8097) are not
# listed: a frozen snapshot has no core, so a port nobody can start is not worth probing.
$Ports = if ($env:PORTS) { $env:PORTS -split ',' | ForEach-Object { [int]$_ } } else { @(8083, 22421, 22422, 8093, 22431, 8090, 8094, 8080) }
$ProgressPreference = 'SilentlyContinue'
$found = 0
foreach ($port in $Ports) {
    $conns = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
    if (-not $conns) { "port {0,-6} : not listening" -f $port; continue }
    foreach ($c in $conns) {
        $p = Get-CimInstance Win32_Process -Filter "ProcessId=$($c.OwningProcess)" -ErrorAction SilentlyContinue
        $found++
        $cmd = if ($p) { $p.CommandLine } else { 'command line unavailable' }
        $started = if ($p) { $p.CreationDate } else { '?' }
        "port {0,-6} : LISTENING pid={1}  {2}  start={3}" -f $port, $c.OwningProcess, $cmd, $started
    }
}
$cores = Get-CimInstance Win32_Process -Filter "Name='node.exe'" -ErrorAction SilentlyContinue |
    Where-Object { $_.CommandLine -match 'AsphyxiaCore' }
if ($cores) {
    "--- AsphyxiaCore processes ---"
    foreach ($c in $cores) { "pid={0} start={1} {2}" -f $c.ProcessId, $c.CreationDate, $c.CommandLine }
} else {
    "--- no AsphyxiaCore process running ---"
    'start it as a managed job from C:\mfg\tmp\RyuNET-core\dist:'
    '  pwsh -c "cd C:\mfg\tmp\RyuNET-core\dist; node AsphyxiaCore.js"   (then re-run this script)'
}
exit $(if ($found -gt 0) { 0 } else { 1 })
