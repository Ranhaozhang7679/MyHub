#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MEnumEditor
* 机器名称:       L05123-NB
* 命名空间:       Luster.SubSystem.ThreeD.Controls.ParamGrid.Editors
* 文 件 名:       MEnumEditor.cs
* 创建时间:       2021/12/2 8:50:52
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      6b500091-9684-44d6-9d3a-c2b8c41d0bdd
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2021
* 修改时间:		  2021/12/2 8:50:52
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Control.Wpf.Motion.Controls;
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
    /// <summary>
    /// 支持多选
    /// </summary>
    internal class MEnumEditor : ParamEditorBase
    {
        public override FrameworkElement CreateElement(ParamItem propertyItem) => new CheckGroup
        {
            IsEnabled = !propertyItem.IsReadOnly,
            ItemsSource = Enum.GetValues(propertyItem.PropertyType),
            Orientation = System.Windows.Controls.Orientation.Vertical
        };

        public override DependencyProperty GetDependencyProperty() => CheckGroup.MultiCheckedProperty;

        protected override IValueConverter GetConverter(ParamItem propertyItem)
        {
            return new MutiEnumConverter(propertyItem);
        }
    }

    internal class MutiEnumConverter : IValueConverter
    {
        private Type enumType;
        private Array sourceVal;

        public MutiEnumConverter(ParamItem pItem)
        {
            sourceVal = Enum.GetValues(pItem.PropertyType);
            enumType = pItem.PropertyType;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 将枚举转换为字符串集合
            if (value != null)
            {
                return value.ToString().Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            if (value is List<string> list)
            {
                string strVal = string.Join(",", list);
                if (!string.IsNullOrEmpty(strVal))
                    return Enum.Parse(enumType, strVal);
            }

            return null;
        }
    }
}