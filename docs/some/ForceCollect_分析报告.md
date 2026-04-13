# ForceCollect.cs 代码分析报告

> 分析日期：2026-04-01
> 文件路径：`src/Modules/Luster.Module.Motion.Device/Functions/ForceCollect.cs`
> 分析背景：最近提交（86e864c0）新增了文件写入异常捕获和资源自动释放

---

## 一、代码问题分析

### 1. 严重 Bug：Y/Z 轴比例系数使用了 RatioX（P0）

**位置**：第 170 行、第 173 行

```csharp
// 第170行 - Y轴应该用 RatioY，实际用了 RatioX
double currentYforce = Math.Abs(Math.Round(adiy.GetAnglogIn() / RatioX, 3));

// 第173行 - Z轴应该用 RatioZ，实际用了 RatioX
double currentZforce = Math.Abs(Math.Round(adiz.GetAnglogIn() / RatioX, 3));
```

**影响**：Y/Z 轴力值全部按 X 轴比例换算，采集数据不准确。属于 copy-paste 错误。

---

### 2. NullReferenceException / KeyNotFoundException 风险（P1）

**位置**：第 152-156 行

```csharp
if (!gModule.Parameters.ContainsKey(EndCondition))
{
    MyOwner.OnLog(..., $"全局变量:{EndCondition}不存在!");
}
gParameter = gModule.Parameters[EndCondition]; // 无论 Key 是否存在都会执行
```

**影响**：日志记录了错误但未 return 或抛出异常，接下来直接访问该 Key 会抛出 `KeyNotFoundException`，且该异常未被外层 catch 捕获。

---

### 3. 文件写入性能问题（P2）

**位置**：第 179-294 行

每次循环迭代都重新打开文件、读取全部内容、检查表头、追加一行数据。在高速采集场景下（每 45ms 一次），频繁的文件 I/O 操作性能较差。

---

### 4. 重复 using 和无用 using（P3）

**位置**：第 34-48 行

- 第 34 行和第 40 行重复 `using System.IO;`
- 以下 using 未被使用：`System.IO.Ports`、`System.Data.Common`、`System.Drawing.Imaging`、`System.Runtime.Remoting.Messaging`

---

### 5. 硬编码路径（P3）

所有文件路径 `"D:\\TaiKeScrewDatas\\CowlingForceData\\..."` 硬编码，不符合规范，应提取为可配置项。

---

### 6. 死代码（P3）

**位置**：第 87-92 行

`lstPressVal` 和 `lstTimeVal` 声明为实例字段但在 `DoExcute` 中完全未使用，实际使用的是局部变量 `xforce`、`yforce` 等。

---

## 二、重复调用 DoExcute 分析

> 设计意图：不断写入同一个 CSV，不清除之前的数据

### 1. CSV 文件数据追加 — 符合预期

使用 `FileMode.OpenOrCreate` 追加数据到同一文件，符合设计意图。

### 2. gParameter 缓存不刷新（中等风险）

**位置**：第 150-157 行

`gParameter` 是实例字段，只在第一次调用时赋值。如果两次调用之间 `EndCondition` 参数名发生变化，第二次仍使用旧引用。

**影响**：如果外部不会改变 `EndCondition` 的值，则无影响。

### 3. 终止条件残留 — 可能只采一个点就退出（中等风险）

**位置**：第 301 行

```csharp
if (pVal.Equals(true)) break;
```

如果上次采集结束 `EndCondition` 为 `true`，且外部调用前未重置为 `false`，则第二次调用在第一次循环末尾就 `break`，只写了一个数据点。

**影响**：需确认外部调用方是否负责重置该全局变量。

### 4. uiRegistered 防止重复注册 — 正常

多次调用时不会重复注册 UI 回调，行为正确。

---

## 三、问题优先级总结

| 优先级 | 问题 | 影响范围 |
|--------|------|----------|
| **P0 严重** | Y/Z 轴使用 RatioX 而非 RatioY/RatioZ | 采集数据计算错误 |
| **P1 高** | 全局变量不存在时未中止执行 | 运行时异常崩溃 |
| **P2 中** | 终止条件未重置可能立即退出 | 取决于外部是否重置全局变量 |
| **P2 中** | 每次循环打开/读取/关闭文件 | 性能问题 |
| **P3 低** | 重复/无用 using、硬编码路径、死代码 | 代码规范 |

---

## 四、建议修复项

1. **必须修复**：第 170 行 `RatioX` → `RatioY`，第 173 行 `RatioX` → `RatioZ`
2. **建议修复**：第 152-156 行增加 `return false` 或抛出异常，避免 KeyNotFoundException
3. **建议优化**：考虑将文件流提升到循环外部，避免每次迭代重复打开/关闭
4. **建议清理**：移除重复 using、无用 using、死代码字段
