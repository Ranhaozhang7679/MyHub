# clean.ps1
Write-Host "正在执行核弹级清理..." -ForegroundColor Cyan

# 1. 清理根目录的 artifacts (新配置的输出)
if (Test-Path "artifacts") {
    Remove-Item "artifacts" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "已删除 artifacts 目录" -ForegroundColor Gray
}

# 2. 遍历所有子目录，寻找并删除本地的 bin 和 obj (旧配置的残留)
# -Exclude "node_modules", ".git" 是为了防止误删前端或git文件，虽然它们里面通常没有bin/obj，但安全第一
Get-ChildItem -Path . -Include bin,obj -Recurse -Directory -Exclude ".git",".vs","node_modules" | ForEach-Object {
    Write-Host "正在删除: $($_.FullName)" -ForegroundColor DarkGray
    Remove-Item $_.FullName -Force -Recurse -ErrorAction SilentlyContinue
}

Write-Host "✅ 清理完成！建议重启 VS 后再编译。" -ForegroundColor Green