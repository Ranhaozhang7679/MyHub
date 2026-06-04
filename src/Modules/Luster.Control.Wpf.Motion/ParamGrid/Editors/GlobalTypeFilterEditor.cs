using Luster.Common.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Luster.Control.Wpf.Motion.Editors
{
    /// <summary>
    /// 按变量类型筛选的全局变量选择编辑器（用于设置全局变量模块）
    /// </summary>
    public class GlobalTypeFilterEditor : GlobalEditor
    {
        private System.Windows.Controls.ComboBox _comboBox;
        private ParameterAttribute _glabalTypeParam;

        /// <summary>
        /// 变量类型枚举名称到.NET类型的映射
        /// </summary>
        private static readonly Dictionary<string, Type> TypeNameMapping = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            { "Bool", typeof(bool) },
            { "String", typeof(string) },
            { "Int", typeof(int) },
            { "Double", typeof(double) },
        };

        public override FrameworkElement CreateElement(ParamItem propertyItem)
        {
            var element = base.CreateElement(propertyItem);
            _comboBox = element as System.Windows.Controls.ComboBox;

            // 查找 GlabalType 参数并订阅变更
            _glabalTypeParam = FindGlabalTypeParam();
            if (_glabalTypeParam != null)
            {
                (_glabalTypeParam as INotifyPropertyChanged).PropertyChanged += OnGlabalTypePropertyChanged;
            }

            return element;
        }

        protected override List<KeyValue> BuildItems()
        {
            var targetType = GetTargetType();
            if (targetType == null)
                return base.BuildItems();

            var keyVals = new List<KeyValue>();
            var gID = GlobalModule.GlobalID;
            if (pAttr.Owner.TaskModules.Contains(gID))
            {
                var gModule = pAttr.Owner.TaskModules[gID];
                foreach (var item in gModule.Parameters)
                {
                    if (item.Value.Type == typeof(LStatus)) continue;
                    if (item.Value.Type != targetType) continue;

                    keyVals.Add(new KeyValue() { Value = item.Key, Desc = $"Global.{item.Value.Alias}" });
                }
            }
            return keyVals;
        }

        private ParameterAttribute FindGlabalTypeParam()
        {
            foreach (var param in pAttr.Owner.Parameters)
            {
                if (param.Key == "GlabalType")
                    return param.Value;
            }
            return null;
        }

        private Type GetTargetType()
        {
            if (_glabalTypeParam?.Value == null)
                return null;

            var typeName = _glabalTypeParam.Value.ToString();
            return TypeNameMapping.TryGetValue(typeName, out var type) ? type : null;
        }

        private void OnGlabalTypePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Value" || _comboBox == null)
                return;

            // 清空当前选中项
            _comboBox.SelectedItem = null;
            pAttr.Value = null;
            _filterText = "";

            // 重建变量列表
            _allItems = BuildItems();
            _comboBox.ItemsSource = _allItems;

            // 重新设置筛选
            var collectionView = CollectionViewSource.GetDefaultView(_allItems);
            collectionView.Filter = FilterItem;
        }
    }
}
