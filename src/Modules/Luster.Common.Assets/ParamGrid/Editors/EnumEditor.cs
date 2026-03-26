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
using Luster.Common.DataStruct.Attributes;

namespace Luster.Common.Assets.ParamGrid
{
    public class EnumEditor : ParamEditorBase
    {
        public override FrameworkElement CreateElement(ParamItem propertyItem)
        {
            bool isChinese = Application.Current.Dispatcher.Thread.CurrentUICulture.Name == "zh-CN";

            List<KeyValue> dataSource = new List<KeyValue>();
            if (propertyItem.PropertyType.IsEnum)
            {
                dataSource = propertyItem.PropertyType.EnumToDataSource();
            }
            else if (propertyItem.PropertyType == typeof(LItems))
            {
                var propItem = propertyItem.Value as PropItemAttribute;
                if (propItem != null && propItem.Value is LItems lItems)
                {
                    dataSource = lItems.ItemSource;
                }
            }

            var comboBox = new System.Windows.Controls.ComboBox
            {
                IsEnabled = !propertyItem.IsReadOnly,
                ItemsSource = dataSource,
                DisplayMemberPath = isChinese ? "Desc" : "Key",
                SelectedValuePath = "Value",
            };

            comboBox.SelectionChanged -= Box_SelectionChanged;
            comboBox.SelectionChanged += Box_SelectionChanged;

            return comboBox;
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