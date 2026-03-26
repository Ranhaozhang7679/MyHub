#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       TreeListBoxItem
* 机器名称:       L05123-NB
* 命名空间:       Luster.Controls.Wpf.TreeListBox
* 文 件 名:       TreeListBoxItem
* 创建时间:       2021/11/4 9:52:32
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      528dfc39-a25f-45c0-9a36-078a73c93f2d
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/4 9:52:32
* 修 改 人:		  luster
************************************************************************************/
#endregion

using Luster.Controls.Wpf.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Luster.Controls.Wpf.TreeEx
{
    public class TreeListBoxItem : ListBoxItem
    {
        /// <summary>
        /// Identifies the <see cref="HasItems"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HasItemsProperty = DependencyProperty.Register(
            nameof(HasItems),
            typeof(bool),
            typeof(TreeListBoxItem),
            new UIPropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="IsDropTarget"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsDropTargetProperty = DependencyProperty.Register(
            nameof(IsDropTarget),
            typeof(bool),
            typeof(TreeListBoxItem),
            new UIPropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="IsExpanded"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
            nameof(IsExpanded),
            typeof(bool),
            typeof(TreeListBoxItem),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (s, e) => ((TreeListBoxItem)s).IsExpandedChanged()));

        /// <summary>
        /// Identifies the <see cref="LevelPadding"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LevelPaddingProperty = DependencyProperty.Register(
            nameof(LevelPadding),
            typeof(Thickness),
            typeof(TreeListBoxItem));

        /// <summary>
        /// Identifies the <see cref="Level"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LevelProperty = DependencyProperty.Register(
            nameof(Level),
            typeof(int),
            typeof(TreeListBoxItem),
            new UIPropertyMetadata(0, (s, e) => ((TreeListBoxItem)s).LevelOrIndentationChanged()));

        /// <summary>
        /// Gets the expand toggle command.
        /// </summary>
        /// <value>The command.</value>
        public ICommand ToggleExpandCommand
        {
            get
            {
                return new RelayCommand(() => this.IsExpanded = !this.IsExpanded);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this item has child items.
        /// </summary>
        public bool HasItems
        {
            get
            {
                return (bool)this.GetValue(HasItemsProperty);
            }

            set
            {
                this.SetValue(HasItemsProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance should preview as a drop target.
        /// </summary>
        public bool IsDropTarget
        {
            get
            {
                return (bool)this.GetValue(IsDropTargetProperty);
            }

            set
            {
                this.SetValue(IsDropTargetProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                return (bool)this.GetValue(IsExpandedProperty);
            }

            set
            {
                this.SetValue(IsExpandedProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the hierarchy level of the item.
        /// </summary>
        /// <value>The level.</value>
        public int Level
        {
            get
            {
                return (int)this.GetValue(LevelProperty);
            }

            set
            {
                this.SetValue(LevelProperty, value);
            }
        }

        /// <summary>
        /// Gets the padding due to hierarchy level and the parent control Indentation.
        /// </summary>
        /// <value>The level padding.</value>
        public Thickness LevelPadding
        {
            get
            {
                return (Thickness)this.GetValue(LevelPaddingProperty);
            }

            private set
            {
                this.SetValue(LevelPaddingProperty, value);
            }
        }

        /// <summary>
        /// Gets the parent <see cref="TreeListBox" />.
        /// </summary>
        internal TreeListBox ParentTreeListBox
        {
            get
            {
                return (TreeListBox)ItemsControl.ItemsControlFromItemContainer(this);
            }
        }

        /// <summary>
        /// Handles changes in <see cref="Level" /> or <see cref="TreeListBox.Indentation" /> (in the parent control).
        /// </summary>
        internal void LevelOrIndentationChanged()
        {
            this.LevelPadding = new Thickness(this.Level * this.ParentTreeListBox.Indentation, 0, 0, 0);
        }

        /// <summary>
        /// Handles changes in the <see cref="IsExpanded" /> property.
        /// </summary>
        private void IsExpandedChanged()
        {
            if (this.IsExpanded)
            {
                this.ParentTreeListBox.Expand(this.Content);
            }
            else
            {
                this.ParentTreeListBox.Collapse(this.Content);
            }
        }
    }
}
