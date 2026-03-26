#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       PathEditor
* 机器名称:       L05123-NB
* 命名空间:       Luster.SubSystem.ThreeD.Controls.ParamGrid.Editors
* 文 件 名:       PathEditor.cs
* 创建时间:       2021/11/11 11:13:32
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      e70b30d4-3787-4b14-9387-b3969683d558
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/11 11:13:32
* 修 改 人:		  luster
************************************************************************************/

#endregion

using HandyControl.Controls;
using Luster.Common.DataStruct.DataModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Luster.Common.Assets.ParamGrid
{
    internal class PathConveter : IValueConverter
    {
        private ParamItem _pItem;

        public PathConveter(ParamItem pItem)
        {
            _pItem = pItem;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is LPath path)
            {
                return new Uri(path.Path, UriKind.Absolute);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is Uri uri)
            {
                return new LPath(uri);
            }

            return null;
        }
    }

    internal class PathEditor : ParamEditorBase
    {
        public override FrameworkElement CreateElement(ParamItem propertyItem)
        {
            var fileSelector = new FileSelector()
            {
                Filter = propertyItem.Filter,
            };

            return fileSelector;
        }

        public override DependencyProperty GetDependencyProperty() => FileSelector.UriPathProperty;

        protected override IValueConverter GetConverter(ParamItem propertyItem)
        {
            return new PathConveter(propertyItem);
        }
    }
}