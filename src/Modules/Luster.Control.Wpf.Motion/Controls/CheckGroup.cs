#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       CheckGroup
* 机器名称:       L05123-NB
* 命名空间:       Luster.Control.Wpf.Motion.Components
* 文 件 名:       CheckGroup.cs
* 创建时间:       2022/6/7 8:39:32
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      1513bf0c-a0b7-46fa-93c4-ffab0b4664cf
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/7 8:39:32
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Luster.Control.Wpf.Motion.Controls
{
    /// <summary>
    /// 多选Checkbox组
    /// </summary>
    public class CheckGroup : ItemsControl
    {
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation", typeof(Orientation), typeof(CheckGroup), new PropertyMetadata(default(Orientation)));

        /// <summary>
        /// 初始值
        /// </summary>
        public List<string> InitChecks;

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            if (InitChecks != null && element is CheckBox chk)
            {
                chk.IsChecked = InitChecks.Contains(item.ToString());
                chk.Unchecked -= Chk_Checked;
                chk.Unchecked += Chk_Checked;
                chk.Checked -= Chk_Checked;
                chk.Checked += Chk_Checked;
            }
        }

        /// <summary>
        /// 子节点设置为CheckBox
        /// </summary>
        /// <returns></returns>
        protected override DependencyObject GetContainerForItemOverride() => new CheckBox();

        /// <summary>
        /// 触发值变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Chk_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            if (chk != null)
            {
                SetMultiChecked();
            }
        }

        internal bool IsOnUIChanged = false;

        private void SetMultiChecked()
        {
            List<string> vals = new List<string>();
            var count = Items.Count;
            for (int i = 0; i < count; i++)
            {
                var chk = ItemContainerGenerator.ContainerFromIndex(i) as CheckBox;
                if (chk != null)
                {
                    if (chk.IsChecked == true)
                    {
                        vals.Add(chk.Content.ToString());
                    }
                }
            }

            IsOnUIChanged = true;
            MultiChecked = vals;
        }

        /// <summary>
        /// 判断子节点是否是CheckBox
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override bool IsItemItsOwnContainerOverride(object item) => item is CheckBox;

        public List<string> MultiChecked
        {
            get { return (List<string>)GetValue(MultiCheckedProperty); }
            set { SetValue(MultiCheckedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MultiChecked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MultiCheckedProperty =
            DependencyProperty.Register("MultiChecked", typeof(List<string>), typeof(CheckGroup), new PropertyMetadata(default, new PropertyChangedCallback(CheckGroupPropertyChanged)));

        /// <summary>
        /// 回调
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void CheckGroupPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CheckGroup ckGroup && !ckGroup.IsOnUIChanged)
            {
                ckGroup.InitChecks = e.NewValue as List<string>;
            }
        }
    }
}