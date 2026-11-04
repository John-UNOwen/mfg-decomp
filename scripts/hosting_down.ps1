# Clean shutdown for the hosted lane (docs/28): core, gate and tunnel.
# Stops only lane components: the gate binary, the cloudflared binary and node
# processes running AsphyxiaCore.js. Nothing else on the machine is touched.
$ProgressPreference = 'SilentlyContinue'
$killed = @()
foreach ($p in Get-CimInstance Win32_Process -EA SilentlyContinue | Where-Object {
    $_.Name -eq 'WebFront.exe' -or $_.Name -eq 'cloudflared.exe' -or
    ($_.Name -eq 'node.exe' -and $_.CommandLine -match 'AsphyxiaCore\.js')
}) {
    Stop-Process -Id $p.ProcessId -Force -EA SilentlyContinue
    $killed += ('{0} pid {1}' -f $p.Name, $p.ProcessId)
}
if ($killed.Count -eq 0) { 'lane already down' } else { 'lane down: ' + ($killed -join ', ') }
