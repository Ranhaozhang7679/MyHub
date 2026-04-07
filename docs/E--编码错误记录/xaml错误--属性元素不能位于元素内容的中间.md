# XAML 编码错误记录

## 1. 属性元素不能位于元素内容的中间

- **日期**: 2026-04-07
- **文件**: `Luster.Motion.DigitalSetup\Views\DigitalAssContent.xaml`
- **错误信息**: `属性元素不能位于元素内容的中间。它们必须位于内容之前或之后。`

### 原因

在 WPF XAML 中，**属性元素**（如 `<Grid.ColumnDefinitions>`、`<hc:Interaction.Triggers>`、`<GridSplitter.Style>`）不能穿插在**内容元素**（如 `<Grid>`、`<Button>` 等）之间。属性元素必须全部放在内容元素之前或之后。

错误写法示例：
```xml
<Grid>
    <Grid.ColumnDefinitions>...</Grid.ColumnDefinitions>  <!-- 属性元素 -->
    <Button Content="OK" />                                <!-- 内容元素 -->
    <Grid Background="Red" />                              <!-- 内容元素 -->
    <hc:Interaction.Triggers>...</hc:Interaction.Triggers> <!-- 属性元素 ❌ 不能在内容之后 -->
    <TextBlock Text="提示" />                              <!-- 内容元素 ❌ 属性元素后面不能跟内容 -->
</Grid>
```

### 正确做法

将所有属性元素集中放在内容元素之前，或将遮罩等内容元素放在属性元素之前：
```xml
<Grid>
    <Grid.ColumnDefinitions>...</Grid.ColumnDefinitions>  <!-- 属性元素在前 -->
    <Button Content="OK" />
    <Grid Background="Red" />
    <TextBlock Text="提示" />
    <hc:Interaction.Triggers>...</hc:Interaction.Triggers> <!-- 属性元素在后也可以 -->
</Grid>
```

### 总结

- 属性元素（Property Element）= 以 `父元素.属性名` 格式的标签，如 `<Grid.ColumnDefinitions>`、`<Button.Style>`
- 内容元素（Content Element）= 直接作为子元素的标签，如 `<Button>`、`<TextBlock>`
- **规则**: 所有属性元素不能穿插在内容元素中间，必须集中放在前面或后面
