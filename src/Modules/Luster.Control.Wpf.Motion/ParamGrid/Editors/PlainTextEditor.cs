using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Luster.Control.Wpf.Motion.Editors
{
    public class PlainTextEditor : ParamEditorBase
    {
        public override FrameworkElement CreateElement(ParamItem propertyItem)
        {
            return new TextBox()
            {
                IsReadOnly = propertyItem.IsReadOnly,
            };
        }

        public override DependencyProperty GetDependencyProperty()
        {
            return TextBox.TextProperty;
        }

        public override BindingMode GetBindingMode(ParamItem propertyItem) => BindingMode.TwoWay;

        public override void CreateBinding(ParamItem propertyItem, DependencyObject element)
        {
            base.CreateBinding(propertyItem, element);

            if (element is TextBox textBox)
            {
                BindingOperations.SetBinding(textBox, TextBox.IsReadOnlyProperty,
                    new Binding("IsReadOnly")
                    {
                        Source = propertyItem,
                        Mode = BindingMode.OneWay
                    });
            }
        }
    }
}