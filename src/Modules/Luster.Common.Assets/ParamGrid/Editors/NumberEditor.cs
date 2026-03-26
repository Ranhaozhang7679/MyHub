using HandyControl.Controls;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Luster.Common.Assets.ParamGrid
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
            return new DoubleConverter();
        }
    }

    internal class DoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0;
            if (double.TryParse(value.ToString(), out double d))
            {
                return d;
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}