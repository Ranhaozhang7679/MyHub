# run-regression.ps1 — AOI 自动化回归测试基线一键 runner (TES-165 P9-D)
# 在仓库根执行：跑新回归集（按 Suite 过滤）+ 5 类相关现有工程，输出分类 Pass/Fail 报告。
# 退出码：全 PASS→0，否则→1。

$ErrorActionPreference = "Continue"
# 控制台 UTF-8 输出（中文报告不乱码）
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8
# 强制 dotnet test 英文输出（避免中文本地化导致计数解析失败）
$env:DOTNET_CLI_UI_LANGUAGE = "en"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir
Set-Location $repoRoot

$newProj = "tests/Luster.Module.Motion.Regression.Baseline.Tests/Luster.Module.Motion.Regression.Baseline.Tests.csproj"

# 现有工程 → 所属 Suite
$existing = @(
    @{ Suite="①状态机";   Proj="src/Tests/Luster.Module.Motion.Production.Tests/Luster.Module.Motion.Production.Tests.csproj" },
    @{ Suite="②模式切换"; Proj="src/Tests/Luster.Module.Motion.TestToolchain.Tests/Luster.Module.Motion.TestToolchain.Tests.csproj" },
    @{ Suite="④握手";     Proj="tests/Luster.Module.Motion.Handover.Tests/Luster.Module.Motion.Handover.Tests.csproj" },
    @{ Suite="⑤超时";     Proj="src/Tests/Luster.Module.Motion.Safety.Tests/Luster.Module.Motion.Safety.Tests.csproj" },
    @{ Suite="⑤超时";     Proj="src/Tests/Luster.Module.Motion.Recovery.Tests/Luster.Module.Motion.Recovery.Tests.csproj" }
)

# 新工程按 Suite 跑
$newSuites = @(
    @{ Suite="①状态机";   Filter="Suite=StateMachine" },
    @{ Suite="②模式切换"; Filter="Suite=ModeSwitch" },
    @{ Suite="③IO轴";     Filter="Suite=IOAxis" },
    @{ Suite="④握手";     Filter="Suite=Handshake" },
    @{ Suite="⑤超时";     Filter="Suite=Timeout" },
    @{ Suite="⑥互锁";     Filter="Suite=Safety" }
)

function Invoke-DotnetTest {
    param([string]$Project, [string]$Filter)
    $testArgs = @("test", $Project, "--logger", "console;verbosity=normal", "--nologo")
    if ($Filter) { $testArgs += @("--filter", $Filter) }
    $out = (& dotnet @testArgs 2>&1) | Out-String
    $code = $LASTEXITCODE
    # 英文分行格式：Total tests: N / Passed: N / Failed: N / Skipped: N
    $passed = 0; $failed = 0; $skipped = 0; $total = 0
    $mT = [regex]::Match($out, "Total tests:\s*(\d+)")
    if ($mT.Success) { $total = [int]$mT.Groups[1].Value }
    $mP = [regex]::Match($out, "(?m)^\s*Passed:\s*(\d+)")
    if ($mP.Success) { $passed = [int]$mP.Groups[1].Value }
    $mF = [regex]::Match($out, "(?m)^\s*Failed:\s*(\d+)")
    if ($mF.Success) { $failed = [int]$mF.Groups[1].Value }
    $mS = [regex]::Match($out, "(?m)^\s*Skipped:\s*(\d+)")
    if ($mS.Success) { $skipped = [int]$mS.Groups[1].Value }
    # 兼容旧版单行格式：Failed: 0, Passed: 5, Skipped: 0, Total: 5
    if ($total -eq 0) {
        $m1 = [regex]::Match($out, "Failed:\s*(\d+),\s*Passed:\s*(\d+),\s*Skipped:\s*(\d+),\s*Total:\s*(\d+)")
        if ($m1.Success) {
            $failed = [int]$m1.Groups[1].Value; $passed = [int]$m1.Groups[2].Value
            $skipped = [int]$m1.Groups[3].Value; $total = [int]$m1.Groups[4].Value
        }
    }
    if ($total -eq 0) { $total = $passed + $failed + $skipped }
    # 未解析到任何计数且退出码非 0：判失败
    if ($total -eq 0 -and $code -ne 0) { $failed = 1; $total = 1 }
    return @{ Passed=$passed; Failed=$failed; Skipped=$skipped; Total=$total; ExitCode=$code }
}

$summary = @{}
foreach ($s in @("①状态机","②模式切换","③IO轴","④握手","⑤超时","⑥互锁")) {
    $summary[$s] = @{ Passed=0; Failed=0; Total=0 }
}

Write-Host "=== AOI 回归测试基线 (TES-165 P9-D) ===" -ForegroundColor Cyan
Write-Host ""

# 1. 验证 Category=Regression trait 过滤可用（新工程全量）
Write-Host "[Trait 验证] 新工程 Category=Regression 全量..." -ForegroundColor Yellow
$r = Invoke-DotnetTest -Project $newProj -Filter "Category=Regression"
Write-Host ("  Passed={0} Failed={1} Total={2}" -f $r.Passed, $r.Failed, $r.Total)

# 2. 新工程按 Suite 分跑
Write-Host ""
Write-Host "[新回归集] 按 Suite 分类：" -ForegroundColor Yellow
foreach ($s in $newSuites) {
    $r = Invoke-DotnetTest -Project $newProj -Filter $s.Filter
    $summary[$s.Suite].Passed += $r.Passed
    $summary[$s.Suite].Failed += $r.Failed
    $summary[$s.Suite].Total += $r.Total
    $status = if ($r.Failed -eq 0) { "PASS" } else { "FAIL" }
    Write-Host ("  {0} [{1}] Passed={2} Failed={3} Total={4} [{5}]" -f $s.Suite, $s.Filter, $r.Passed, $r.Failed, $r.Total, $status)
}

# 3. 现有工程全跑
Write-Host ""
Write-Host "[现有工程] 全量回归：" -ForegroundColor Yellow
foreach ($e in $existing) {
    $r = Invoke-DotnetTest -Project $e.Proj -Filter ""
    $summary[$e.Suite].Passed += $r.Passed
    $summary[$e.Suite].Failed += $r.Failed
    $summary[$e.Suite].Total += $r.Total
    $status = if ($r.Failed -eq 0) { "PASS" } else { "FAIL" }
    $projName = Split-Path $e.Proj -Leaf
    Write-Host ("  {0} [{1}] Passed={2} Failed={3} Total={4} [{5}]" -f $e.Suite, $projName, $r.Passed, $r.Failed, $r.Total, $status)
}

# 4. 汇总
Write-Host ""
Write-Host "=== 分类汇总 ===" -ForegroundColor Cyan
$overallPass = $true
$gP = 0; $gF = 0; $gT = 0
foreach ($s in @("①状态机","②模式切换","③IO轴","④握手","⑤超时","⑥互锁")) {
    $p = $summary[$s].Passed; $f = $summary[$s].Failed; $t = $summary[$s].Total
    $gP += $p; $gF += $f; $gT += $t
    if ($f -ne 0) { $overallPass = $false }
    $status = if ($f -eq 0) { "PASS" } else { "FAIL" }
    $color = if ($f -eq 0) { "Green" } else { "Red" }
    Write-Host ("  {0}: Passed={1} Failed={2} Total={3} [{4}]" -f $s, $p, $f, $t, $status) -ForegroundColor $color
}
Write-Host ""
Write-Host ("  合计: Passed={0} Failed={1} Total={2}" -f $gP, $gF, $gT) -ForegroundColor Cyan
Write-Host ""
if ($overallPass -and $gF -eq 0) {
    Write-Host "=== OVERALL: PASS ===" -ForegroundColor Green
    exit 0
} else {
    Write-Host "=== OVERALL: FAIL ===" -ForegroundColor Red
    exit 1
}
