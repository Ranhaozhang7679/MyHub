using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
    }
}