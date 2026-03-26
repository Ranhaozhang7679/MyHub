using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Luster.Common.Assets.ParamGrid
{
    public class PlainTextEditor : ParamEditorBase
    {
        public override FrameworkElement CreateElement(ParamItem propertyItem)
        {
            return new TextBox();
        }

        public override DependencyProperty GetDependencyProperty()
        {
            return TextBox.TextProperty;
        }
    }
}