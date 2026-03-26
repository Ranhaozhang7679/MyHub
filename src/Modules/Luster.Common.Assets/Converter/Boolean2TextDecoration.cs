#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Boolean2TextDecoration
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Assets.Converter
* 文 件 名:       Boolean2TextDecoration.cs
* 创建时间:       2022/3/14 13:26:52
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      b268218d-0805-4a26-a87b-ded9bf21c8d2
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/3/14 13:26:52
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Luster.Common.Assets.Converter
{
    public class Boolean2TextDecoration : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TextDecoration myUnderline = new TextDecoration();
            myUnderline.Location = TextDecorationLocation.Strikethrough;
            if (bool.TryParse(value?.ToString(), out bool result) && result)
            {
                Pen myPen = new Pen();
                myPen.Brush = new LinearGradientBrush(Colors.Red, Colors.IndianRed, new Point(0, 0.5), new Point(1, 0.5));
                myPen.Brush.Opacity = 0.8;
                myPen.Thickness = 3;
                myPen.DashStyle = DashStyles.Solid;
                myUnderline.Pen = myPen;
                myUnderline.PenThicknessUnit = TextDecorationUnit.FontRecommended;
                TextDecorationCollection myCollection = new TextDecorationCollection();
                myCollection.Add(myUnderline);
                return myCollection;
            }

            return myUnderline;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}