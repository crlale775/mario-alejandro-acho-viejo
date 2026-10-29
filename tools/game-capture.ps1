param(
  [string]$Out = "shot.png",
  [string]$Keys = "",
  [int]$WaitMs = 1200,
  [int]$Width = 1536,
  [int]$Height = 960
)

Add-Type @"
using System;
using System.Runtime.InteropServices;
public class Win32 {
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
}
"@

Add-Type -AssemblyName System.Windows.Forms, System.Drawing

$proc = Get-Process -Name "Mario Acho" -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowHandle -ne 0 } | Select-Object -First 1

if (-not $proc) {
  Write-Output "NO WINDOW"
  exit 1
}

$handle = $proc.MainWindowHandle
[Win32]::ShowWindow($handle, 9) | Out-Null
[Win32]::MoveWindow($handle, 0, 0, $Width, $Height, $true) | Out-Null
[Win32]::SetForegroundWindow($handle) | Out-Null
Start-Sleep -Milliseconds 700

if ($Keys -ne "") {
  $wsh = New-Object -ComObject WScript.Shell

  foreach ($key in $Keys.Split(",")) {
    $wsh.SendKeys($key)
    Start-Sleep -Milliseconds 400
  }
}

Start-Sleep -Milliseconds $WaitMs

$bounds = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
$bitmap = New-Object System.Drawing.Bitmap($bounds.Width, $bounds.Height)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.CopyFromScreen(0, 0, 0, 0, $bitmap.Size)
$bitmap.Save($Out, [System.Drawing.Imaging.ImageFormat]::Png)
$graphics.Dispose()
$bitmap.Dispose()

Write-Output "OK $Out ($($bounds.Width)x$($bounds.Height))"
