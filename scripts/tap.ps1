# Clicks a screen point as the mouse-as-touch input the Unity build reads.
# Usage: pwsh -File scripts/tap.ps1 -X 960 -Y 1010 [-ProcId <game pid>] [-Hold 60]
param(
  [Parameter(Mandatory = $true)][int]$X,
  [Parameter(Mandatory = $true)][int]$Y,
  [int]$ProcId = 0,
  [int]$Hold = 60
)
$ErrorActionPreference = 'Stop'
Add-Type @"
using System; using System.Runtime.InteropServices;
public class Tap {
 [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
 [DllImport("user32.dll")] public static extern void mouse_event(uint f, uint x, uint y, uint d, int extra);
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
 [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int n);
 public const uint LEFTDOWN = 0x0002, LEFTUP = 0x0004;
}
"@
if ($ProcId -ne 0) {
  $proc = Get-Process -Id $ProcId -ErrorAction Stop
  $hwnd = $proc.MainWindowHandle
  if ($hwnd -ne [IntPtr]::Zero) {
    [void][Tap]::ShowWindow($hwnd, 9)
    [void][Tap]::SetForegroundWindow($hwnd)
    Start-Sleep -Milliseconds 350
  }
}
[Tap]::SetCursorPos($X, $Y) | Out-Null
Start-Sleep -Milliseconds 120
[Tap]::mouse_event([Tap]::LEFTDOWN, 0, 0, 0, 0)
Start-Sleep -Milliseconds $Hold
[Tap]::mouse_event([Tap]::LEFTUP, 0, 0, 0, 0)
"tapped $X,$Y"
