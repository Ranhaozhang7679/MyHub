#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       FloatingInfoWindow
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Assets.FloatingInfo.Views
* 文 件 名:       FloatingInfoWindow.xaml.cs
* 创建时间:       2026/03/24
* 作    者:       Luster
* 版    权:       <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 唯一标识：      a1b2c3d4-e5f6-7890-abcd-ef1234567898
* 创建年份:       2026
************************************************************************************/

#endregion

using Luster.Common.Assets.FloatingInfo.ViewModel;
using System;
using System.Windows;

namespace Luster.Common.Assets.FloatingInfo.Views
{
    /// <summary>
    /// 浮动信息窗口
    /// </summary>
    public partial class FloatingInfoWindow : System.Windows.Window
    {
        /// <summary>
        /// 窗口ViewModel
        /// </summary>
        public FloatingInfoWindowVM ViewModel => DataContext as FloatingInfoWindowVM;

        /// <summary>
        /// 构造函数
        /// </summary>
        public FloatingInfoWindow()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        /// <summary>
        /// 数据上下文改变事件
        /// </summary>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is FloatingInfoWindowVM vm)
            {
                vm.SetWindow(this);
            }
        }

        /// <summary>
        /// 窗口位置改变时保存位置
        /// </summary>
        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            ViewModel?.SaveWindowPosition();
        }

        /// <summary>
        /// 窗口大小改变时保存位置
        /// </summary>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            ViewModel?.SaveWindowPosition();
        }

        /// <summary>
        /// 标题栏鼠标左键按下事件，用于拖动窗口
        /// </summary>
        private void OnTitleBarMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
