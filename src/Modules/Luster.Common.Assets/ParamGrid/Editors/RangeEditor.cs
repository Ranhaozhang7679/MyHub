#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       RangeEditor
* 机器名称:       L05123-NB
* 命名空间:       Luster.SubSystem.ThreeD.Controls.ParameterGrid.Editors
* 文 件 名:       RangeEditor.cs
* 创建时间:       2022/2/8 13:53:26
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      87ee9a4a-09fc-4a6a-8782-74f83d69d5a4
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/2/8 13:53:26
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Luster.Common.Assets.ParamGrid
{
    public class RangeEditor : ParamEditorBase
    {
        public override FrameworkElement CreateElement(ParamItem propertyItem)
        {
            return new Slider();
        }

        public override DependencyProperty GetDependencyProperty() => Slider.ValueProperty;
    }
}