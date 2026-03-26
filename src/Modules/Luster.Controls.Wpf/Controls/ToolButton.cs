#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ToolButton
* 机器名称:       L05123-NB
* 命名空间:       Luster.Controls.Wpf.Controls
* 文 件 名:       ToolButton.cs
* 创建时间:       2022/1/14 13:28:32
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      9b007baa-27d7-46bc-a08e-d61e10eed614
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/1/14 13:28:32
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Luster.Controls.Wpf.Controls
{
    public class ToolButton : TextBlock
    {
        public ICommand Command
        {
            get
            {
                return (ICommand)GetValue(CommandProperty);
            }
            set
            {
                SetValue(CommandProperty, value);
            }
        }

        public object CommandParameter { get; set; }

        public static readonly DependencyProperty CommandProperty =
                DependencyProperty.Register(
                        "Command",
                        typeof(ICommand),
                        typeof(ToolButton),
                        new FrameworkPropertyMetadata((ICommand)null));

        public System.Windows.IInputElement CommandTarget
        {
            get;
            set;
        }

        public ToolButton()
        {
            this.CommandTarget = this;
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            Command?.Execute(CommandParameter);
        }
    }
}