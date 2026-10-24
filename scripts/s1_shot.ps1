# Captures the main window of a process via PrintWindow and saves a PNG.
# Usage: pwsh -File scripts/s1_shot.ps1 -ProcId 36776 -Out out/s1/title.png
param(
  [Parameter(Mandatory = $true)][int]$ProcId,
  [string]$Out = 'out/s1/shot.png'
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public class W {
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out R r);
  [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr h, IntPtr hdc, uint flags);
  public struct R { public int Left, Top, Right, Bottom; }
}
"@
$proc = Get-Process -Id $ProcId -ErrorAction Stop
$hwnd = $proc.MainWindowHandle
if ($hwnd -eq [IntPtr]::Zero) { throw "no main window for pid $Pid" }
$r = New-Object W+R
[void][W]::GetWindowRect($hwnd, [ref]$r)
$w = $r.Right - $r.Left; $h = $r.Bottom - $r.Top
if ($w -le 0 -or $h -le 0) { throw "degenerate window ${w}x${h}" }
$bmp = New-Object System.Drawing.Bitmap($w, $h)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$hdc = $g.GetHdc()
[void][W]::PrintWindow($hwnd, $hdc, 2)   # PW_RENDERFULLCONTENT, works for D3D windows on Win 8+
$g.ReleaseHdc($hdc)
$g.Dispose()
New-Item -ItemType Directory -Force (Split-Path $Out) | Out-Null
$bmp.Save($Out, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
"saved $Out ($w x $h)"
