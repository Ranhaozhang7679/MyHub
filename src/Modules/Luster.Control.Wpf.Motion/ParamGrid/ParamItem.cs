using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using HandyControl.Data;
using Luster.TaskFlow.Common.Attributes;
using Luster.Common.DataStruct.DataModels;

namespace Luster.Control.Wpf.Motion
{
    public class ParamItem : ListBoxItem
    {
        public Guid ID
        {
            get { return (Guid)GetValue(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ID.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IDProperty =
            DependencyProperty.Register("ID", typeof(Guid), typeof(ParamItem), new PropertyMetadata(Guid.Empty));

        public object Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(object), typeof(ParamItem), new PropertyMetadata(default(object), new PropertyChangedCallback(ParamValueChangedCallback)));

        private static void ParamValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register(
            "DisplayName", typeof(string), typeof(ParamItem), new PropertyMetadata(default(string)));

        public string DisplayName
        {
            get => (string)GetValue(DisplayNameProperty);
            set => SetValue(DisplayNameProperty, value);
        }

        public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.Register(
            "PropertyName", typeof(string), typeof(ParamItem), new PropertyMetadata(default(string)));

        public string PropertyName
        {
            get => (string)GetValue(PropertyNameProperty);
            set => SetValue(PropertyNameProperty, value);
        }

        public static readonly DependencyProperty PropertyTypeProperty = DependencyProperty.Register(
            "PropertyType", typeof(Type), typeof(ParamItem), new PropertyMetadata(default(Type)));

        public Type PropertyType
        {
            get => (Type)GetValue(PropertyTypeProperty);
            set => SetValue(PropertyTypeProperty, value);
        }

        public static readonly DependencyProperty PropertyTypeNameProperty = DependencyProperty.Register(
            "PropertyTypeName", typeof(string), typeof(ParamItem), new PropertyMetadata(default(string)));

        public string PropertyTypeName
        {
            get => (string)GetValue(PropertyTypeNameProperty);
            set => SetValue(PropertyTypeNameProperty, value);
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            "Description", typeof(string), typeof(ParamItem), new PropertyMetadata(default(string)));

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            "IsReadOnly", typeof(bool), typeof(ParamItem), new PropertyMetadata(false));

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        // 是否是输入参数
        public static readonly DependencyProperty ParamTypeProperty = DependencyProperty.Register(
            "ParamType", typeof(string), typeof(ParamItem), new PropertyMetadata("Normal"));

        public string ParamType
        {
            get => (string)GetValue(ParamTypeProperty);
            set => SetValue(ParamTypeProperty, value);
        }

        public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register(
            "Category", typeof(string), typeof(ParamItem), new PropertyMetadata(default(string)));

        public string Category
        {
            get => (string)GetValue(CategoryProperty);
            set => SetValue(CategoryProperty, value);
        }

        public static readonly DependencyProperty SortProperty = DependencyProperty.Register(
            "Sort", typeof(int), typeof(ParamItem), new PropertyMetadata(0));

        public int Sort
        {
            get => (int)GetValue(SortProperty);
            set => SetValue(SortProperty, value);
        }

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(
            "Filter", typeof(string), typeof(ParamItem), new PropertyMetadata(default(string)));

        public string Filter
        {
            get => (string)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }

        public static readonly DependencyProperty EditorProperty = DependencyProperty.Register(
            "Editor", typeof(ParamEditorBase), typeof(ParamItem), new PropertyMetadata(default(ParamEditorBase)));

        public ParamEditorBase Editor
        {
            get => (ParamEditorBase)GetValue(EditorProperty);
            set => SetValue(EditorProperty, value);
        }

        public static readonly DependencyProperty EditorElementProperty = DependencyProperty.Register(
            "EditorElement", typeof(FrameworkElement), typeof(ParamItem), new PropertyMetadata(default(FrameworkElement)));

        public FrameworkElement EditorElement
        {
            get => (FrameworkElement)GetValue(EditorElementProperty);
            set => SetValue(EditorElementProperty, value);
        }

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
        /// 是否可见
        /// </summary>
        public bool UIVisible
        {
            get => (bool)GetValue(UIVisibleProperty);
            set => SetValue(UIVisibleProperty, value);
        }

        public static readonly DependencyProperty UIVisibleProperty = DependencyProperty.Register(
            "UIVisible", typeof(bool), typeof(ParamItem), new PropertyMetadata(false));

        /// <summary>
        /// 是否引用其他对象
        /// </summary>
        public bool IsRef
        {
            get => (bool)GetValue(IsRefProperty);
            set => SetValue(IsRefProperty, value);
        }

        public static readonly DependencyProperty IsRefProperty = DependencyProperty.Register(
           "IsRef", typeof(bool), typeof(ParamItem), new PropertyMetadata(true));

        public string ForeColor
        {
            get { return (string)GetValue(ForeColorProperty); }
            set { SetValue(ForeColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ForeColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForeColorProperty =
            DependencyProperty.Register("ForeColor", typeof(string), typeof(ParamItem), new PropertyMetadata("Red"));

        public PropertyDescriptor PropertyDescriptor { get; set; }

        public string ShowRefLabel { get; set; }

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