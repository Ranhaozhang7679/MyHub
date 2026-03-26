#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       RelyEditor
* 机器名称:       L05123-NB
* 命名空间:       Luster.SubSystem.ThreeD.Controls.ParameterGrid.Editors
* 文 件 名:       RelyEditor.cs
* 创建时间:       2022/1/21 13:37:48
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      9f2b73a0-107b-45bb-9d11-c729d1fb75e8
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/1/21 13:37:48
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.TaskFlow.Common.Attributes;
using Luster.Common.DataStruct.DataModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Luster.Control.Wpf.Motion.Editors
{
    public class RelyEditor : ParamEditorBase
    {
        public override FrameworkElement CreateElement(ParamItem propertyItem)
        {
            var box = new System.Windows.Controls.ComboBox
            {
                IsEnabled = !propertyItem.IsReadOnly,
                ItemsSource = GetDataSource(propertyItem),
                DisplayMemberPath = "Desc",
                SelectedValuePath = "Value",
            };

            return box;
        }

        /// <summary>
        /// 构造数据源
        /// </summary>
        /// <param name="pItem"></param>
        /// <returns></returns>
        private List<KeyValue> GetDataSource(ParamItem pItem)
        {
            var data = new List<KeyValue>();

            // 获取RelyType对象
            var pAttr = pItem.Value as ParameterAttribute;
            if (pAttr == null) return data;

            var relyType = pAttr.Value as LRelyType;
            if (relyType == null) return data;

            return relyType.DataSource;
        }

        /// <summary>
        /// 字段来源
        /// </summary>
        /// <returns></returns>
        public override DependencyProperty GetDependencyProperty()
        {
            return Selector.SelectedValueProperty;
        }

        protected override IValueConverter GetConverter(ParamItem propertyItem)
        {
            return new RelyConverter(propertyItem);
        }
    }

    internal class RelyConverter : IValueConverter
    {
        private ParamItem _paramItem;

        private LRelyType _relyType;

        public RelyConverter(ParamItem pItem)
        {
            _paramItem = pItem;
            var pAttr = _paramItem.Value as ParameterAttribute;
            if (pAttr != null)
            {
                _relyType = pAttr.Value as LRelyType;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LRelyType relyType)
            {
                return relyType.PropertyName;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (_relyType != null)
            {
                _relyType.PropertyName = value?.ToString();
                return _relyType;
            }

            return value;
        }
    }
}