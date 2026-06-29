<#
.SYNOPSIS
  P9-D 五轴 AOI 迁移 - 自动化回归测试基线一键运行脚本。

.DESCRIPTION
  源端 vs 迁移后行为对齐回归：覆盖工站状态机 / 模式切换 / 关键 IO 轴动作 / 握手信号 / 异常超时
  五类核心行为。全程虚拟模式（DeviceMode.Virtual + ZMotionMotionCard.SimulationMode=true）运行，
  无硬件依赖。

  依次执行三个步骤并聚合 Pass/Fail：
    1) Luster.Module.Motion.FiveAxis.Tests  --filter Category=Regression
    2) Luster.SimDevice.MotionCard.Tests    --filter Category=Regression
    3) Luster.Tools.DiffRegression --self-test  （源端 vs 迁移后 diff 自检）

  任一步失败仍继续执行后续步骤，最终汇总并按整体结果返回退出码。

.NOTES
  运行: pwsh scripts/run-regression.ps1   或   powershell -File scripts/run-regression.ps1
  退出码: 0 = 全部通过; 1 = 存在失败或错误。
  前置: 仓库根需有可还原的 .\packages 本地包源（Luster.* 内部包），否则测试工程编译失败。
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Continue'
$repoRoot = Split-Path -Parent $PSScriptRoot

$script:summary = [System.Collections.Generic.List[pscustomobject]]::new()
$script:anyFailed = $false

function Invoke-DotnetTest {
    param([string]$Label, [string]$Project)

    Write-Host ""
    Write-Host "==== [$Label] dotnet test --filter Category=Regression ====" -ForegroundColor Cyan

    $log = Join-Path $env:TEMP ("p9d-reg-" + [guid]::NewGuid().ToString('N') + ".log")
    # 2>&1 捕获 MSBuild/测试全量输出；ForEach-Object 把 ErrorRecord 拍平为字符串，避免 PS5.1 管道异常
    & dotnet test $Project --filter "Category=Regression" --logger "console;verbosity=normal" 2>&1 |
        ForEach-Object { $_.ToString() } | Tee-Object -FilePath $log
    $code = $LASTEXITCODE

    $text = ""
    if (Test-Path $log) { $text = Get-Content $log -Raw }
    Remove-Item $log -Force -ErrorAction SilentlyContinue

    # 解析 NUnit3TestAdapter 摘要行（兼容 "Passed: 12" 与 "Passed: 12, Failed: 0, Skipped: 0" 两种格式）
    $passed = 0; $failed = 0; $skipped = 0; $total = 0
    if ($text -match 'Passed:\s*(\d+)')  { $passed  = [int]$Matches[1] }
    if ($text -match 'Failed:\s*(\d+)')  { $failed  = [int]$Matches[1] }
    if ($text -match 'Skipped:\s*(\d+)') { $skipped = [int]$Matches[1] }
    if ($text -match '(?:Total|总计)[:\s]*\s*(\d+)') { $total = [int]$Matches[1] }
    if ($total -eq 0) { $total = $passed + $failed + $skipped }

    $status = if ($code -eq 0 -and $failed -eq 0) { 'PASS' } else { 'FAIL' }
    if ($status -eq 'FAIL') { $script:anyFailed = $true }

    $script:summary.Add([pscustomobject]@{
        Step     = $Label
        Status   = $status
        Passed   = $passed
        Failed   = $failed
        Skipped  = $skipped
        Total    = $total
        ExitCode = $code
    })

    # 失败时尽量列出失败用例名（best-effort 解析控制台输出）
    if ($failed -gt 0) {
        Write-Host ""
        Write-Host "---- 失败用例（$Label）----" -ForegroundColor Yellow
        $text -split "`r?`n" |
            Where-Object { $_ -match '^Failed\s+\S.*\[' -or $_ -match '\[FAIL\]' -or $_ -match '错误消息' } |
            Select-Object -First 60 |
            ForEach-Object { Write-Host ("  " + $_.Trim()) -ForegroundColor Yellow }
    }
}

function Invoke-DiffSelfTest {
    Write-Host ""
    Write-Host "==== [DiffRegression] dotnet run -- --self-test ====" -ForegroundColor Cyan

    $proj = Join-Path $repoRoot 'src/Tools/Luster.Tools.DiffRegression/Luster.Tools.DiffRegression.csproj'
    $log = Join-Path $env:TEMP ("p9d-diff-" + [guid]::NewGuid().ToString('N') + ".log")
    & dotnet run --project $proj -c Release -- --self-test 2>&1 |
        ForEach-Object { $_.ToString() } | Tee-Object -FilePath $log
    $code = $LASTEXITCODE
    Remove-Item $log -Force -ErrorAction SilentlyContinue

    $status = if ($code -eq 0) { 'PASS' } else { 'FAIL' }
    if ($status -eq 'FAIL') { $script:anyFailed = $true }

    $script:summary.Add([pscustomobject]@{
        Step     = 'DiffRegression --self-test'
        Status   = $status
        Passed   = if ($code -eq 0) { 1 } else { 0 }
        Failed   = if ($code -ne 0) { 1 } else { 0 }
        Skipped  = 0
        Total    = 1
        ExitCode = $code
    })
}

$faProj = Join-Path $repoRoot 'tests/Luster.Module.Motion.FiveAxis.Tests/Luster.Module.Motion.FiveAxis.Tests.csproj'
$sdProj = Join-Path $repoRoot 'tests/Luster.SimDevice.MotionCard.Tests/Luster.SimDevice.MotionCard.Tests.csproj'

Invoke-DotnetTest  -Label 'FiveAxis.Tests'  -Project $faProj
Invoke-DotnetTest  -Label 'SimDevice.Tests' -Project $sdProj
Invoke-DiffSelfTest

Write-Host ""
Write-Host "================ P9-D 回归基线汇总 ================" -ForegroundColor Cyan
$script:summary | Format-Table -AutoSize

$totPass = ($script:summary | Measure-Object -Property Passed  -Sum).Sum
$totFail = ($script:summary | Measure-Object -Property Failed  -Sum).Sum
$totSkip = ($script:summary | Measure-Object -Property Skipped -Sum).Sum
Write-Host ("合计: Passed={0}  Failed={1}  Skipped={2}" -f $totPass, $totFail, $totSkip) -ForegroundColor Cyan

if ($script:anyFailed) {
    Write-Host "结论: FAIL（存在失败项）" -ForegroundColor Red
    exit 1
} else {
    Write-Host "结论: PASS（全部通过）" -ForegroundColor Green
    exit 0
}
