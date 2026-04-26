#Requires -RunAsAdministrator
# ============================================================
#  网口自动配置工具 v3.0
#  模式1: 标定 - 首台机器上建立 PCI槽位 → 物理端口标签 的映射
#  模式2: 部署 - 选工站类型 + 输入PLC IP → 按槽位自动配置所有网口
# ============================================================

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$ConfigPath = Join-Path $ScriptDir "config.json"
$HwMapPath  = Join-Path $ScriptDir "hardware_map.json"

# ============================================================
# 工具函数
# ============================================================

function Show-Banner {
    Write-Host ""
    Write-Host "  =============================================" -ForegroundColor Cyan
    Write-Host "    网口自动配置工具 v3.0" -ForegroundColor White
    Write-Host "    Hypertrain Mistral CG Link 产线专用" -ForegroundColor DarkGray
    Write-Host "    模式：PCI 槽位绑定 + 工站模板" -ForegroundColor DarkGray
    Write-Host "  =============================================" -ForegroundColor Cyan
    Write-Host ""
}

function Get-WiredAdaptersWithPCI {
    <#
    .SYNOPSIS
    获取所有有线网口及其 PCI 硬件信息，按 PCI 位置排序
    #>
    $adapters = Get-NetAdapter | Where-Object {
        $_.MediaType -eq "802.3" -or $_.PhysicalMediaType -match "802.3"
    }

    $result = @()
    foreach ($a in $adapters) {
        $hw = Get-NetAdapterHardwareInfo -Name $a.Name -ErrorAction SilentlyContinue
        $pciKey = ""
        $bus = 0; $dev = 0; $func = 0
        if ($hw) {
            $bus  = [int]$hw.Bus
            $dev  = [int]$hw.Device
            $func = [int]$hw.Function
            $pciKey = "{0:D3}-{1:D3}-{2:D1}" -f $bus, $dev, $func
        }

        # 获取当前 IP
        $currentIP = ""
        try {
            $ipCfg = Get-NetIPAddress -InterfaceIndex $a.InterfaceIndex -AddressFamily IPv4 -ErrorAction SilentlyContinue
            if ($ipCfg) { $currentIP = ($ipCfg | ForEach-Object { "$($_.IPAddress)/$($_.PrefixLength)" }) -join ", " }
        } catch { }

        $result += [PSCustomObject]@{
            Name        = $a.Name
            Description = $a.InterfaceDescription
            MAC         = $a.MacAddress
            Status      = $a.Status
            PCIBus      = $bus
            PCIDevice   = $dev
            PCIFunction = $func
            PCIKey      = $pciKey
            CurrentIP   = $currentIP
            Index       = $a.InterfaceIndex
        }
    }

    return $result | Sort-Object PCIKey
}

function Show-AdapterList {
    param ($Adapters, [switch]$WithLabels, $HwMap = $null)

    Write-Host ""
    Write-Host "  ─────────────────────────────────────────────────────────────────────" -ForegroundColor DarkGray
    Write-Host "  序号  PCI位置        当前名称                  状态      当前IP" -ForegroundColor Yellow

    if ($WithLabels) {
        Write-Host "                                                                标签" -ForegroundColor Cyan
    }
    Write-Host "  ─────────────────────────────────────────────────────────────────────" -ForegroundColor DarkGray

    $i = 0
    foreach ($a in $Adapters) {
        $status = if ($a.Status -eq "Up") { "[连接]" } else { "[断开]" }
        $statusColor = if ($a.Status -eq "Up") { "Green" } else { "DarkGray" }
        $ipStr = if ($a.CurrentIP) { $a.CurrentIP } else { "未配置" }

        Write-Host ("  {0,-4}" -f "[$i]") -NoNewline -ForegroundColor Yellow
        Write-Host ("  {0,-15}" -f "Bus$($a.PCIBus):Dev$($a.PCIDevice):Fn$($a.PCIFunction)") -NoNewline -ForegroundColor Cyan
        Write-Host ("  {0,-25}" -f $a.Name) -NoNewline -ForegroundColor White
        Write-Host ("  {0,-8}" -f $status) -NoNewline -ForegroundColor $statusColor
        Write-Host ("  {0}" -f $ipStr) -ForegroundColor $(if ($a.CurrentIP) { "Green" } else { "DarkGray" })

        # 显示描述
        Write-Host ("        {0}" -f $a.Description) -ForegroundColor DarkGray

        if ($WithLabels -and $HwMap) {
            $match = $HwMap | Where-Object { $_.bus -eq $a.PCIBus -and $_.device -eq $a.PCIDevice -and $_.function -eq $a.PCIFunction }
            if ($match) {
                Write-Host ("        标签: {0}" -f $match.label) -ForegroundColor Cyan
            } else {
                Write-Host "        标签: [未标定]" -ForegroundColor DarkGray
            }
        }

        $i++
    }

    Write-Host "  ─────────────────────────────────────────────────────────────────────" -ForegroundColor DarkGray
    Write-Host ""
}

# ============================================================
# 模式1: 硬件标定
# ============================================================

function Start-Calibration {
    Write-Host ""
    Write-Host "  =============================================" -ForegroundColor Magenta
    Write-Host "  [模式1] 硬件标定" -ForegroundColor White
    Write-Host "  在首台机器上执行一次，建立槽位 → 端口标签的映射" -ForegroundColor DarkGray
    Write-Host "  =============================================" -ForegroundColor Magenta

    $adapters = Get-WiredAdaptersWithPCI

    if ($adapters.Count -eq 0) {
        Write-Host "  [错误] 未检测到有线网口！" -ForegroundColor Red
        return
    }

    Write-Host ""
    Write-Host "  检测到 $($adapters.Count) 个有线网口：" -ForegroundColor White
    Show-AdapterList -Adapters $adapters

    # 显示可用标签
    $labels = @(
        "主板网口1", "主板网口2",
        "网卡1-1", "网卡1-2", "网卡1-3", "网卡1-4",
        "网卡2-1", "网卡2-2", "网卡2-3", "网卡2-4",
        "网卡3-1", "网卡3-2", "网卡3-3", "网卡3-4"
    )

    Write-Host "  可用的物理端口标签（来自拓扑图）：" -ForegroundColor Yellow
    Write-Host ""
    for ($j = 0; $j -lt $labels.Count; $j++) {
        Write-Host "    [$j] $($labels[$j])" -ForegroundColor White
    }
    Write-Host "    [-] 跳过（不标定此网口）" -ForegroundColor DarkGray
    Write-Host ""

    # 逐个标定
    $hwMap = @()
    $i = 0
    foreach ($a in $adapters) {
        Write-Host "  网口 #$i: " -NoNewline -ForegroundColor Yellow
        Write-Host "$($a.Name)" -NoNewline -ForegroundColor White
        Write-Host "  (Bus$($a.PCIBus):Dev$($a.PCIDevice):Fn$($a.PCIFunction)  $($a.Description))" -ForegroundColor DarkGray

        $labelChoice = Read-Host "    对应标签编号（0-$($labels.Count-1)，直接回车跳过）"

        if ($labelChoice -eq "" -or $labelChoice -eq "-") {
            Write-Host "    已跳过" -ForegroundColor DarkGray
        } else {
            $labelIdx = [int]$labelChoice
            if ($labelIdx -ge 0 -and $labelIdx -lt $labels.Count) {
                $selectedLabel = $labels[$labelIdx]
                Write-Host "    -> $selectedLabel" -ForegroundColor Green

                $hwMap += @{
                    bus      = $a.PCIBus
                    device   = $a.PCIDevice
                    function = $a.PCIFunction
                    label    = $selectedLabel
                    description = $a.Description
                }
            } else {
                Write-Host "    无效编号，已跳过" -ForegroundColor Yellow
            }
        }

        $i++
        Write-Host ""
    }

    if ($hwMap.Count -eq 0) {
        Write-Host "  [警告] 没有标定任何网口！" -ForegroundColor Yellow
        return
    }

    # 保存
    $output = @{
        "_说明" = "硬件标定文件 - PCI槽位 → 物理端口标签映射"
        "_创建时间" = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
        "_机器名" = $env:COMPUTERNAME
        "pci_map" = $hwMap
    }

    $output | ConvertTo-Json -Depth 5 | Out-File -FilePath $HwMapPath -Encoding UTF8
    Write-Host "  =============================================" -ForegroundColor Green
    Write-Host "  标定完成！已保存 $($hwMap.Count) 个映射到：" -ForegroundColor Green
    Write-Host "  $HwMapPath" -ForegroundColor White
    Write-Host "  =============================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "  将此文件复制到其他同型号 PC，即可直接使用部署模式。" -ForegroundColor DarkGray

    # 回显
    Write-Host ""
    Write-Host "  已标定的映射：" -ForegroundColor Yellow
    foreach ($m in $hwMap) {
        Write-Host "    Bus$($m.bus):Dev$($m.device):Fn$($m.function)  →  $($m.label)" -ForegroundColor Cyan
    }
    Write-Host ""
}

# ============================================================
# 模式2: 一键部署
# ============================================================

function Start-Deploy {
    # 检查 hardware_map.json 是否存在
    if (-not (Test-Path $HwMapPath)) {
        Write-Host ""
        Write-Host "  [错误] 未找到硬件标定文件: hardware_map.json" -ForegroundColor Red
        Write-Host "  请先在首台机器上运行「标定模式」生成此文件，" -ForegroundColor Yellow
        Write-Host "  然后将文件复制到本机的同级目录。" -ForegroundColor Yellow
        Write-Host ""
        return
    }

    $hwMapData = Get-Content $HwMapPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $pciMap = $hwMapData.pci_map

    Write-Host ""
    Write-Host "  =============================================" -ForegroundColor Green
    Write-Host "  [模式2] 一键部署" -ForegroundColor White
    Write-Host "  硬件标定来源: $($hwMapData._机器名) ($($hwMapData._创建时间))" -ForegroundColor DarkGray
    Write-Host "  已标定 $($pciMap.Count) 个网口" -ForegroundColor DarkGray
    Write-Host "  =============================================" -ForegroundColor Green

    # 读取工站模板
    $config = Get-Content $ConfigPath -Raw -Encoding UTF8 | ConvertFrom-Json

    # 选择工站类型
    Write-Host ""
    Write-Host "  请选择工站类型：" -ForegroundColor White
    Write-Host "  ────────────────────────────────────────" -ForegroundColor DarkGray

    $stationTypes = @($config.station_types.PSObject.Properties.Name)
    for ($i = 0; $i -lt $stationTypes.Count; $i++) {
        $typeName = $stationTypes[$i]
        $typeDesc = $config.station_types.$typeName.description
        Write-Host "  [$($i+1)] $typeName - $typeDesc" -ForegroundColor White
    }

    Write-Host "  ────────────────────────────────────────" -ForegroundColor DarkGray
    $choice = Read-Host "  请输入编号"

    $idx = [int]$choice - 1
    if ($idx -lt 0 -or $idx -ge $stationTypes.Count) {
        Write-Host "  无效选择" -ForegroundColor Yellow
        return
    }

    $selectedType = $stationTypes[$idx]
    $stationConfig = $config.station_types.$selectedType
    $portConfig = $stationConfig.port_config

    Write-Host ""
    Write-Host "  工站类型: $selectedType - $($stationConfig.description)" -ForegroundColor White

    # 检查是否需要输入 PLC IP
    $stationIP = $null
    $needsIP = $false
    foreach ($prop in $portConfig.PSObject.Properties) {
        if ($prop.Value.ip_template) { $needsIP = $true; break }
    }

    if ($needsIP) {
        Write-Host ""
        Write-Host "  请输入本站 PLC 网段 IP 的最后一位:" -ForegroundColor Yellow
        Write-Host "  (参考拓扑图，如 CGL-1 填 151，CGSF-2 填 182)" -ForegroundColor DarkGray
        $stationIP = Read-Host "  192.168.101."

        if (-not $stationIP -or -not ($stationIP -match '^\d{1,3}$') -or [int]$stationIP -gt 254) {
            Write-Host "  无效输入" -ForegroundColor Yellow
            return
        }
    }

    # 获取本机所有有线网口
    $adapters = Get-WiredAdaptersWithPCI

    Write-Host ""
    Write-Host "  本机网口与标定映射对照：" -ForegroundColor Yellow
    Show-AdapterList -Adapters $adapters -WithLabels -HwMap $pciMap

    # 确认执行
    Write-Host "  即将应用以下配置：" -ForegroundColor White
    Write-Host "  ────────────────────────────────────────────────────" -ForegroundColor DarkGray

    $configPlan = @()
    foreach ($prop in $portConfig.PSObject.Properties) {
        $portLabel = $prop.Name
        $netCfg = $prop.Value

        # 通过 PCI 槽位找到对应网口
        $pciEntry = $pciMap | Where-Object { $_.label -eq $portLabel }
        if (-not $pciEntry) {
            Write-Host "  [跳过] $portLabel - 未在标定文件中找到" -ForegroundColor Yellow
            continue
        }

        $adapter = $adapters | Where-Object {
            $_.PCIBus -eq $pciEntry.bus -and $_.PCIDevice -eq $pciEntry.device -and $_.PCIFunction -eq $pciEntry.function
        }

        if (-not $adapter) {
            Write-Host "  [跳过] $portLabel (Bus$($pciEntry.bus):Dev$($pciEntry.device):Fn$($pciEntry.function)) - 本机未找到此硬件" -ForegroundColor Yellow
            continue
        }

        # 计算 IP
        $targetIP = ""
        if ($netCfg.rename_only) {
            $targetIP = "(仅重命名)"
        } elseif ($netCfg.ip_template) {
            $targetIP = $netCfg.ip_template -replace '\{station_ip\}', $stationIP
        } else {
            $targetIP = $netCfg.ip
        }

        $jumboStr = if ($netCfg.jumbo) { " [Jumbo=$($netCfg.jumbo)]" } else { "" }

        Write-Host ("  {0,-10} {1,-20} → 重命名为 {2,-18} IP: {3}{4}" -f
            $portLabel, $adapter.Name, $netCfg.name, $targetIP, $jumboStr) -ForegroundColor Cyan

        $configPlan += @{
            PortLabel   = $portLabel
            AdapterName = $adapter.Name
            NewName     = $netCfg.name
            IP          = $targetIP
            Prefix      = $netCfg.prefix
            Jumbo       = $netCfg.jumbo
            RenameOnly  = [bool]$netCfg.rename_only
        }
    }

    Write-Host "  ────────────────────────────────────────────────────" -ForegroundColor DarkGray
    Write-Host ""

    if ($configPlan.Count -eq 0) {
        Write-Host "  [错误] 没有可配置的网口，请检查标定文件" -ForegroundColor Red
        return
    }

    $confirm = Read-Host "  确认执行？(Y/N)"
    if ($confirm -ne "Y" -and $confirm -ne "y") {
        Write-Host "  已取消" -ForegroundColor Yellow
        return
    }

    # 执行配置
    Write-Host ""
    Write-Host "  =============================================" -ForegroundColor Cyan
    Write-Host "  开始配置..." -ForegroundColor White
    Write-Host "  =============================================" -ForegroundColor Cyan
    Write-Host ""

    $successCount = 0

    foreach ($plan in $configPlan) {
        $adapterName = $plan.AdapterName
        $newName = $plan.NewName

        Write-Host "  [$($plan.PortLabel)] $adapterName" -ForegroundColor White

        # 1. 重命名
        if ($adapterName -ne $newName) {
            try {
                # 检查名称冲突
                $conflict = Get-NetAdapter -Name $newName -ErrorAction SilentlyContinue
                if ($conflict -and $conflict.InterfaceIndex -ne (Get-NetAdapter -Name $adapterName).InterfaceIndex) {
                    $tempName = "${newName}_old_$(Get-Random -Maximum 9999)"
                    Rename-NetAdapter -Name $newName -NewName $tempName -Confirm:$false
                    Write-Host "    名称冲突，已将 $newName 暂改为 $tempName" -ForegroundColor DarkGray
                }
                Rename-NetAdapter -Name $adapterName -NewName $newName -Confirm:$false
                $adapterName = $newName
                Write-Host "    重命名 → $newName" -ForegroundColor Cyan
            } catch {
                Write-Host "    [警告] 重命名失败: $($_.Exception.Message)" -ForegroundColor Yellow
                $newName = $adapterName
            }
        } else {
            Write-Host "    名称已正确" -ForegroundColor DarkGray
        }

        # 2. 仅重命名的跳过 IP 配置
        if ($plan.RenameOnly) {
            Write-Host "    [OK] 仅重命名，跳过IP配置" -ForegroundColor Green
            $successCount++
            Write-Host ""
            continue
        }

        # 3. 清理现有 IP
        try {
            Get-NetIPAddress -InterfaceAlias $newName -AddressFamily IPv4 -ErrorAction SilentlyContinue |
                ForEach-Object { Remove-NetIPAddress -InterfaceAlias $newName -IPAddress $_.IPAddress -Confirm:$false -ErrorAction SilentlyContinue }
            Remove-NetRoute -InterfaceAlias $newName -AddressFamily IPv4 -Confirm:$false -ErrorAction SilentlyContinue
        } catch { }

        # 4. 配置 IP
        try {
            New-NetIPAddress -InterfaceAlias $newName -IPAddress $plan.IP -PrefixLength $plan.Prefix -ErrorAction Stop | Out-Null
            Write-Host "    [OK] IP = $($plan.IP)/$($plan.Prefix)" -ForegroundColor Green
            $successCount++
        } catch {
            Write-Host "    [失败] $($_.Exception.Message)" -ForegroundColor Red
            Write-Host ""
            continue
        }

        # 5. 巨型帧
        if ($plan.Jumbo) {
            $jumboNames = @("JumboPacket", "*JumboPacket", "JumboFrame", "MaximumFrameSize")
            $jumboSet = $false
            foreach ($jn in $jumboNames) {
                $prop = Get-NetAdapterAdvancedProperty -Name $newName -RegistryKeyword $jn -ErrorAction SilentlyContinue
                if ($prop) {
                    try {
                        Set-NetAdapterAdvancedProperty -Name $newName -RegistryKeyword $jn -RegistryValue $plan.Jumbo -ErrorAction Stop
                        Write-Host "    [OK] 巨型帧 = $($plan.Jumbo)" -ForegroundColor Green
                        $jumboSet = $true
                    } catch {
                        Write-Host "    [警告] 巨型帧配置失败" -ForegroundColor Yellow
                    }
                    break
                }
            }
            if (-not $jumboSet) {
                Write-Host "    [提示] 此网卡不支持巨型帧" -ForegroundColor DarkGray
            }
        }

        Write-Host ""
    }

    # 结果汇报
    Write-Host "  =============================================" -ForegroundColor Cyan
    Write-Host "  配置完成: $successCount/$($configPlan.Count) 成功" -ForegroundColor $(if ($successCount -eq $configPlan.Count) { "Green" } else { "Yellow" })
    Write-Host "  =============================================" -ForegroundColor Cyan

    # 显示最终状态
    $finalAdapters = Get-WiredAdaptersWithPCI
    Show-AdapterList -Adapters $finalAdapters -WithLabels -HwMap $pciMap
}

# ============================================================
# 主程序
# ============================================================

Show-Banner

# 检查管理员权限
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "  [错误] 请以管理员身份运行此脚本！" -ForegroundColor Red
    Write-Host "  提示：右键「一键配置网口.bat」→「以管理员身份运行」" -ForegroundColor Yellow
    Read-Host "  按回车退出"
    exit 1
}

if (-not (Test-Path $ConfigPath)) {
    Write-Host "  [错误] 未找到配置文件: $ConfigPath" -ForegroundColor Red
    Read-Host "  按回车退出"
    exit 1
}

# 主菜单循环
while ($true) {
    Write-Host ""
    Write-Host "  请选择操作：" -ForegroundColor White
    Write-Host "  ────────────────────────────────────────────" -ForegroundColor DarkGray
    Write-Host "  [0] 查看当前网口信息" -ForegroundColor Cyan
    Write-Host "  [1] 硬件标定（首台机器运行一次）" -ForegroundColor Magenta
    Write-Host "  [2] 一键部署（选工站类型 + 输入IP → 全自动配置）" -ForegroundColor Green
    Write-Host "  [Q] 退出" -ForegroundColor DarkGray
    Write-Host "  ────────────────────────────────────────────" -ForegroundColor DarkGray
    Write-Host ""

    $mainChoice = Read-Host "  请输入编号"

    switch ($mainChoice) {
        "0" {
            $adapters = Get-WiredAdaptersWithPCI
            $hwMap = $null
            if (Test-Path $HwMapPath) {
                $hwMapData = Get-Content $HwMapPath -Raw -Encoding UTF8 | ConvertFrom-Json
                $hwMap = $hwMapData.pci_map
            }
            Show-AdapterList -Adapters $adapters -WithLabels -HwMap $hwMap
        }
        "1" { Start-Calibration }
        "2" { Start-Deploy }
        { $_ -eq "Q" -or $_ -eq "q" } {
            Write-Host "  再见！" -ForegroundColor Cyan
            break
        }
        default {
            Write-Host "  无效选择" -ForegroundColor Yellow
        }
    }

    if ($mainChoice -eq "Q" -or $mainChoice -eq "q") { break }
}
