using System;
using System.Windows;
using System.Windows.Data;

namespace Luster.Control.Wpf.Motion
{
    public abstract class ParamEditorBase : DependencyObject
    {
        /// <summary>
        /// 隶属模块ID
        /// </summary>
        public Guid ModuleID { get; set; }

        public abstract FrameworkElement CreateElement(ParamItem propertyItem);

        public virtual void CreateBinding(ParamItem propertyItem, DependencyObject element)
        {
            BindingOperations.SetBinding(element, GetDependencyProperty(),
               new Binding($"{propertyItem.PropertyName}")
               {
                   Source = propertyItem.Value,
                   Mode = GetBindingMode(propertyItem),
                   UpdateSourceTrigger = GetUpdateSourceTrigger(propertyItem),
                   Converter = GetConverter(propertyItem),
                   Delay = Delay
               });
        }

        public abstract DependencyProperty GetDependencyProperty();

        public virtual BindingMode GetBindingMode(ParamItem propertyItem) => propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;

        public virtual UpdateSourceTrigger GetUpdateSourceTrigger(ParamItem propertyItem) => UpdateSourceTrigger.PropertyChanged;

        protected virtual IValueConverter GetConverter(ParamItem propertyItem) => null;

        protected virtual int Delay => 0;

        /// <summary>
        /// 参数变更事件
        /// </summary>
        public event Action<object> ParamItemValueChanged;

        protected void OnParamValueChanged(object newV) => ParamItemValueChanged?.Invoke(newV);
    }
}