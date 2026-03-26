using HandyControl.Controls;
using Luster.Common.DataStruct.Extensions;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Luster.Control.Wpf.Motion.Editors
{
    public class NumberEditor : ParamEditorBase
    {
        public NumberEditor()
        {
        }

        public NumberEditor(double minimum, double maximum, int decimalP = 0)
        {
            Minimum = minimum;
            Maximum = maximum;
            DecimalPalces = decimalP;
            if ((Maximum - Minimum) <= 1)
            {
                Increment = 0.1;
            }
        }

        public double Minimum { get; set; }

        public double Maximum { get; set; }

        public int DecimalPalces { get; set; }

        public double Increment { get; set; } = 1;

        public override FrameworkElement CreateElement(ParamItem propertyItem)
        {
            var editor = new NumericUpDown
            {
                IsReadOnly = propertyItem.IsReadOnly,
                Minimum = Minimum,
                Maximum = Maximum,
                DecimalPlaces = DecimalPalces,
                Increment = Increment,
            };

            return editor;
        }

        public override DependencyProperty GetDependencyProperty() => NumericUpDown.ValueProperty;

        protected override IValueConverter GetConverter(ParamItem propertyItem)
        {
            return new DoubleConverter(propertyItem.PropertyType);
        }

        protected override int Delay => 2000;
    }

    internal class DoubleConverter : IValueConverter
    {
        private Type _cvtType;
        public DoubleConverter(Type valueType)
        {
            _cvtType = valueType;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return 0;
            }

            if (_cvtType.IsNumeric())
            {
                return value.ConvertToType(_cvtType);
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (_cvtType.IsNumeric())
            {
                return value.ConvertToType(_cvtType);
            }

            return value;
        }
    }
}