#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ArrayEditor
* 机器名称:       L05123-NB
* 命名空间:       Luster.Control.Wpf.Motion.ParamGrid.Editors
* 文 件 名:       ArrayEditor.cs
* 创建时间:       2022/7/19 10:32:24
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      e9bb2958-041d-4455-8b77-ba749e92c72a
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/19 10:32:24
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.TaskFlow.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Luster.Control.Wpf.Motion.Editors
{
    public class ArrayEditor : ParamEditorBase
    {
        public override FrameworkElement CreateElement(ParamItem propertyItem)
        {
            var pAttr = propertyItem.Value as ParameterAttribute;

            // 更新关联的属性是
            pAttr.EditorValue = pAttr.Value;

            var box = new System.Windows.Controls.ComboBox
            {
                IsReadOnly = propertyItem.IsReadOnly,
                ItemsSource = pAttr.Datas,
            };


            return box;
        }


        public override DependencyProperty GetDependencyProperty()
        {
            return System.Windows.Controls.ComboBox.SelectedValueProperty;
        }
    }
}