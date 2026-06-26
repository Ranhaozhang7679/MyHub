# TES-139 截图脚本：启动最小 WPF 宿主 exe，按窗口标题截图
param(
    [string]$IssueId = "TES-139",
    [string]$Shot = "tes139-mode-step"
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms
Add-Type -Path ".\Win32Helper.cs"

$exe = "artifacts\bin\net472\Luster.Tools.UIHost.exe"
$title = "TES-139 FiveAxisManualControl"
$outDir = "screenshots\$IssueId"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

Remove-Item "_uiready.txt" -ErrorAction SilentlyContinue

Write-Host "Starting host exe: $exe"
$proc = Start-Process $exe -PassThru

$ready = $false
$deadline = (Get-Date).AddSeconds(15)
while ((Get-Date) -lt $deadline) {
    if (Test-Path "_uiready.txt") { $ready = $true; break }
    Start-Sleep -Milliseconds 300
}
Write-Host "UI ready: $ready"

Start-Sleep -Seconds 1

$win = Get-Process -Id $proc.Id -ErrorAction SilentlyContinue
$hwnd = [IntPtr]::Zero
if ($win -and $win.MainWindowHandle -ne [IntPtr]::Zero) {
    $hwnd = $win.MainWindowHandle
} else {
    $pw = Get-Process | Where-Object { $_.MainWindowTitle -like "*$title*" } | Select-Object -First 1
    if ($pw) { $hwnd = $pw.MainWindowHandle }
}

if ($hwnd -eq [IntPtr]::Zero) {
    Write-Host "ERROR: window not found"
    $proc | Stop-Process -Force -ErrorAction SilentlyContinue
    exit 1
}

[Win32Helper]::ShowWindow($hwnd, 9) | Out-Null
[Win32Helper]::SetForegroundWindow($hwnd) | Out-Null
Start-Sleep -Milliseconds 500

$rect = New-Object Win32Helper+RECT
[Win32Helper]::GetWindowRect($hwnd, [ref]$rect) | Out-Null
$w = $rect.Right - $rect.Left
$ht = $rect.Bottom - $rect.Top
Write-Host "Window rect: L=$($rect.Left) T=$($rect.Top) W=$w H=$ht"

if ($w -le 0 -or $ht -le 0) {
    Write-Host "ERROR: invalid window size"
    $proc | Stop-Process -Force -ErrorAction SilentlyContinue
    exit 1
}

$bmp = New-Object System.Drawing.Bitmap $w, $ht
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($rect.Left, $rect.Top, 0, 0, $bmp.Size)
$g.Flush([System.Drawing.Drawing2D.FlushIntention]::Sync)
# 必须先 Dispose Graphics 再 Save：Graphics.FromImage 会锁住 bitmap，PNG codec 在锁定状态下报 generic error
$g.Dispose()
$out = Join-Path $outDir "$Shot.png"
$bmp.Save($out, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()

Write-Host "Saved: $out"

$proc | Stop-Process -Force -ErrorAction SilentlyContinue
Remove-Item "_uiready.txt" -ErrorAction SilentlyContinue
