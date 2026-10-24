# Screen-captures a window region (works where PrintWindow goes black on flip swapchains).
# Brings the window to front first. Usage: pwsh -File scripts/s1_screen.ps1 -ProcId 44500 -Out out/s1/shot.png
param(
  [Parameter(Mandatory = $true)][int]$ProcId,
  [string]$Out = 'out/s1/shot.png'
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System; using System.Runtime.InteropServices;
public class U2 {
 [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out R r);
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
 [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int n);
 public struct R { public int Left, Top, Right, Bottom; }
}
"@
$proc = Get-Process -Id $ProcId -ErrorAction Stop
$hwnd = $proc.MainWindowHandle
if ($hwnd -eq [IntPtr]::Zero) { throw "no main window for pid $ProcId" }
[void][U2]::ShowWindow($hwnd, 9)   # SW_RESTORE
[void][U2]::SetForegroundWindow($hwnd)
Start-Sleep -Milliseconds 800
$r = New-Object U2+R
[void][U2]::GetWindowRect($hwnd, [ref]$r)
$w = $r.Right - $r.Left
$h = $r.Bottom - $r.Top
$size = New-Object System.Drawing.Size($w, $h)
$bmp = New-Object System.Drawing.Bitmap($w, $h)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($r.Left, $r.Top, 0, 0, $size)
$g.Dispose()
New-Item -ItemType Directory -Force (Split-Path $Out) | Out-Null
$bmp.Save($Out, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
"saved $Out from rect $($r.Left),$($r.Top) ${w}x${h}"
