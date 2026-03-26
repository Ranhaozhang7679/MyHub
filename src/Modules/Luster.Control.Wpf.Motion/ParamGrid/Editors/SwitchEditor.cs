using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using HandyControl.Tools;

namespace Luster.Control.Wpf.Motion.Editors
{
    public class SwitchEditor : ParamEditorBase
    {
        public override FrameworkElement CreateElement(ParamItem propertyItem)
        {
            bool isTrue = false;
            if (propertyItem.Value != null && bool.TryParse(propertyItem.Value?.ToString(), out isTrue)) { }
            var ckBox = new CheckBox()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                IsEnabled = !propertyItem.IsReadOnly,
                Margin = new Thickness(0, 0, 5, 0),
                IsChecked = isTrue
            };

            //var toggoleBtn = new ToggleButton
            //{
            //    Style = ResourceHelper.GetResource<Style>("ToggleButtonSwitch"),
            //    HorizontalAlignment = HorizontalAlignment.Right,
            //    IsEnabled = !propertyItem.IsReadOnly
            //};

            ckBox.Checked -= ToggoleBtn_Checked;
            ckBox.Checked += ToggoleBtn_Checked;

            ckBox.Unchecked -= ToggoleBtn_Checked;
            ckBox.Unchecked += ToggoleBtn_Checked;
            return ckBox;
        }

        private void ToggoleBtn_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;
            if (toggleButton != null)
                OnParamValueChanged(toggleButton.IsChecked);
        }

        public override DependencyProperty GetDependencyProperty() => CheckBox.IsCheckedProperty;
    }
}