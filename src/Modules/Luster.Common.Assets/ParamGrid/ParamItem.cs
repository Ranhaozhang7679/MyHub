#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ParamItem
* 机器名称:       L05123-NB
* 命名空间:       Luster.Controls.Wpf.ParamGrid
* 文 件 名:       ParamItem.cs
* 创建时间:       2022/4/18 16:46:40
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      f56b1d50-34b5-4c3b-bcdd-69a00f21c2c2
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/18 16:46:40
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

namespace Luster.Common.Assets.ParamGrid
{
    public class ParamItem : ListBoxItem
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropertyName
        {
            get => (string)GetValue(PropertyNameProperty);
            set => SetValue(PropertyNameProperty, value);
        }
        public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.Register(
            "PropertyName", typeof(string), typeof(ParamItem), new PropertyMetadata(default(string)));


        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName
        {
            get => (string)GetValue(DisplayNameProperty);
            set => SetValue(DisplayNameProperty, value);
        }

        public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register(
            "DisplayName", typeof(string), typeof(ParamItem), new PropertyMetadata(default(string)));


        /// <summary>
        /// 值对象
        /// </summary>
        public object Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(object), typeof(ParamItem), new PropertyMetadata(default));


        public Type PropertyType
        {
            get => (Type)GetValue(PropertyTypeProperty);
            set => SetValue(PropertyTypeProperty, value);
        }
        public static readonly DependencyProperty PropertyTypeProperty = DependencyProperty.Register(
            "PropertyType", typeof(Type), typeof(ParamItem), new PropertyMetadata(default(Type)));

     
        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            "Description", typeof(string), typeof(ParamItem), new PropertyMetadata(default(string)));

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            "IsReadOnly", typeof(bool), typeof(ParamItem), new PropertyMetadata(false));

        public string Category
        {
            get => (string)GetValue(CategoryProperty);
            set => SetValue(CategoryProperty, value);
        }

        public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register(
            "Category", typeof(string), typeof(ParamItem), new PropertyMetadata(default(string)));


        /// <summary>
        /// 排序
        /// </summary>
        public int Sort
        {
            get => (int)GetValue(SortProperty);
            set => SetValue(SortProperty, value);
        }
        public static readonly DependencyProperty SortProperty = DependencyProperty.Register(
            "Sort", typeof(int), typeof(ParamItem), new PropertyMetadata(0));


        public string Filter
        {
            get => (string)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(
            "Filter", typeof(string), typeof(ParamItem), new PropertyMetadata(default(string)));

        /// <summary>
        /// 是否可见
        /// </summary>
        public bool UIVisible
        {
            get => (bool)GetValue(UIVisibleProperty);
            set => SetValue(UIVisibleProperty, value);
        }

        public static readonly DependencyProperty UIVisibleProperty = DependencyProperty.Register(
            "UIVisible", typeof(bool), typeof(ParamItem), new PropertyMetadata(false));


        public ParamEditorBase Editor
        {
            get => (ParamEditorBase)GetValue(EditorProperty);
            set => SetValue(EditorProperty, value);
        }
        public static readonly DependencyProperty EditorProperty = DependencyProperty.Register(
            "Editor", typeof(ParamEditorBase), typeof(ParamItem), new PropertyMetadata(default(ParamEditorBase)));


        public FrameworkElement EditorElement
        {
            get => (FrameworkElement)GetValue(EditorElementProperty);
            set => SetValue(EditorElementProperty, value);
        }
        public static readonly DependencyProperty EditorElementProperty = DependencyProperty.Register(
            "EditorElement", typeof(FrameworkElement), typeof(ParamItem), new PropertyMetadata(default(FrameworkElement)));

        /// <summary>
        /// 是否是展开的
        /// </summary>
        public bool IsExpanded
        {
            get => (bool)GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, value);
        }

        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
            "IsExpanded", typeof(bool), typeof(ParamItem), new PropertyMetadata(false));

        

        /// <summary>
        /// 参数值变更
        /// </summary>
        public event Action<ParamItem, object> ParamValueChangedEvent;

        public virtual void InitElement()
        {
            if (Editor == null) return;
            EditorElement = Editor.CreateElement(this);

            // 先注册事件，在初始化的时候也进行绑定
            Editor.ParamItemValueChanged -= Editor_ParamItemValueChanged;
            Editor.ParamItemValueChanged += Editor_ParamItemValueChanged;

            Editor.CreateBinding(this, EditorElement);
        }

        /// <summary>
        /// 事件变更
        /// </summary>
        /// <param name="obj">新的值</param>
        private void Editor_ParamItemValueChanged(object obj)
        {
            ParamValueChangedEvent?.Invoke(this, obj);
        }
    }
}