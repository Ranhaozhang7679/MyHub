using Luster.TaskFlow.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Luster.Control.Wpf.Motion.Editors
{
    public class CiteEditor : ParamEditorBase
    {
        public override FrameworkElement CreateElement(ParamItem propertyItem)
        {
            return new TextBox()
            {
                Foreground = Brushes.Blue,
                IsReadOnly = true
            };
        }

        public override DependencyProperty GetDependencyProperty()
        {
            return TextBox.TextProperty;
        }

        protected override IValueConverter GetConverter(ParamItem propertyItem)
        {
            return new CiteConverter(propertyItem);
        }
    }

    internal class CiteConverter : IValueConverter
    {
        private bool isRef = false;
        private string showLabel = string.Empty;

        public CiteConverter(ParamItem pItem)
        {
            var pAttr = pItem.Value as ParameterAttribute;
            isRef = pAttr.RefOut != null;
            if (pAttr.RefOut != null)
            {
                showLabel = $"{pAttr.RefOut.Owner.Alias}.{pAttr.RefOut.CN}";
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (isRef)
            {
                return showLabel;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}