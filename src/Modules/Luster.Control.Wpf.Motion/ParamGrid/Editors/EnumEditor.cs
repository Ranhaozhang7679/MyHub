using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Luster.Common.DataStruct.Extensions;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;
using Luster.Common.DataStruct.DataModels;

namespace Luster.Control.Wpf.Motion.Editors
{
    public class EnumEditor : ParamEditorBase
    {
        public override FrameworkElement CreateElement(ParamItem propertyItem)
        {
            bool isChinese = Application.Current.Dispatcher.Thread.CurrentUICulture.Name == "zh-CN";

            var box = new System.Windows.Controls.ComboBox
            {
                IsEnabled = !propertyItem.IsReadOnly,
                //ItemsSource = Enum.GetValues(propertyItem.PropertyType),
                ItemsSource = propertyItem.PropertyType.EnumToDataSource(),
                DisplayMemberPath = isChinese ? "Desc" : "Key",
                SelectedValuePath = "Value",
            };

            box.SelectionChanged -= Box_SelectionChanged;
            box.SelectionChanged += Box_SelectionChanged;

            return box;
        }

        /// <summary>
        /// 参数值变更
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void Box_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
                OnParamValueChanged(e.AddedItems[0]);
        }

        public override DependencyProperty GetDependencyProperty() => Selector.SelectedValueProperty;
    }
}