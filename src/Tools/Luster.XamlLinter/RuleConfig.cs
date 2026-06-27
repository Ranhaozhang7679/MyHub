using System.Collections.Generic;

namespace Luster.XamlLinter
{
    /// <summary>
    /// XAML 静态检查规则配置:命名空间常量 + 各规则的属性/控件清单。
    /// 清单来源 docs/wpf-design-contract.md §1/§2/§3,集中此处便于维护。
    /// </summary>
    internal static class RuleConfig
    {
        // 命名空间 URI(判断裸控件:元素 ns == PresentationNs)
        public const string PresentationNs = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        public const string XNs = "http://schemas.microsoft.com/winfx/2006/xaml";
        public const string DesignNs = "http://schemas.microsoft.com/expression/blend/2008";
        public const string MarkupCompatNs = "http://schemas.openxmlformats.org/markup-compatibility/2006";
        public const string HandyControlNs = "https://handyorg.github.io/handycontrol";

        /// <summary>契约 §1 禁裸的 presentation-ns 控件(应改 hc: 或 Luster.Controls.Wpf)</summary>
        public static readonly HashSet<string> BareControlNames = new HashSet<string>
        {
            "Button", "TextBox", "PasswordBox", "ComboBox", "ListBox",
            "Border", "Label", "CheckBox", "RadioButton", "Slider",
            "ProgressBar", "Expander", "GroupBox", "TabControl", "TreeView",
            "DataGrid", "DatePicker", "Calendar", "Menu", "ContextMenu",
            "ToolBar", "StatusBar"
        };

        /// <summary>契约 §2.1/§2.2:颜色类属性,值若为 #hex 应改 {StaticResource}</summary>
        public static readonly HashSet<string> ColorProperties = new HashSet<string>
        {
            "Background", "Foreground", "BorderBrush", "Fill", "Color",
            "Stroke", "OpacityMask", "BorderBrush"
        };

        /// <summary>契约 §2.4:尺寸/间距类属性,值若为裸数值应引用 Sizes.xaml Key</summary>
        public static readonly HashSet<string> SizeProperties = new HashSet<string>
        {
            "Height", "Width", "Padding", "Margin", "CornerRadius",
            "MinWidth", "MinHeight", "MaxWidth", "MaxHeight"
        };

        /// <summary>契约 §4:HandyControl 附加属性里的尺寸类(写死像素值也应引 Sizes.xaml Key)。
        /// 这些是 hc: 附加属性,Member.LocalName 去掉 owner 前缀,故用短名匹配 + ns==HandyControlNs 判断。
        /// 例:hc:InfoElement.TitleWidth / hc:TitleElement.TitleWidth / hc:InfoElement.TitlePlacement</summary>
        public static readonly HashSet<string> HcAttachedSizeProperties = new HashSet<string>
        {
            "TitleWidth", "TitlePlacement"
        };

        /// <summary>契约 §3 字号三档(标题20/正文14/标签12),图标字号 16/28/32 另算不算正文</summary>
        public static readonly HashSet<int> ValidFontSizes = new HashSet<int> { 12, 14, 20 };

        /// <summary>契约 §1/§6:View 内禁自绘的模板元素(presentation ns),应进资源字典</summary>
        public static readonly HashSet<string> InlineTemplateNames = new HashSet<string>
        {
            "ControlTemplate", "DataTemplate", "ItemsPanelTemplate", "HierarchicalDataTemplate"
        };
    }
}
