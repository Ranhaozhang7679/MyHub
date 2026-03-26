#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LinkLabel
* 机器名称:       L05123-NB
* 命名空间:       Luster.Controls.Wpf.Controls
* 文 件 名:       LinkLabel.cs
* 创建时间:       2021/12/2 8:53:36
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      a5bce87c-1d9e-4731-a577-f77554a523e7
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2021
* 修改时间:		  2021/12/2 8:53:36
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using System.Windows;
using System.Windows.Controls;

namespace Luster.Controls.Wpf.Controls
{
    /// <summary>
    /// 标签
    /// </summary>
    public class LinkLabel : Label
    {
        public LinkLabel()
        {
        }

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Label.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(LinkLabel), new PropertyMetadata(""));
    }
}