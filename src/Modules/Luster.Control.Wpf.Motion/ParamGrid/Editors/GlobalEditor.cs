#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       GlobalEditor
* 机器名称:       L05123-NB
* 命名空间:       Luster.Control.Wpf.Motion.ParamGrid.Editors
* 文 件 名:       GlobalEditor.cs
* 创建时间:       2022/7/11 9:01:12
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      de59946e-b9e2-49ae-a439-f8c61c501f10
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/11 9:01:12
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Luster.Control.Wpf.Motion.Editors
{
    /// <summary>
    /// 全局变量选择
    /// </summary>
    public class GlobalEditor : ParamEditorBase
    {
        /// <summary>
        /// 参数属性
        /// </summary>
        private ParameterAttribute pAttr;

        public override FrameworkElement CreateElement(ParamItem propertyItem)
        {
            pAttr = propertyItem.Value as ParameterAttribute;

            // 更新关联的属性是
            pAttr.EditorValue = pAttr.Value;

            List<KeyValue> keyVals = new List<KeyValue>();
            var gID = GlobalModule.GlobalID;
            if (pAttr.Owner.TaskModules.Contains(gID))
            {
                var gModule = pAttr.Owner.TaskModules[gID];
                foreach (var item in gModule.Parameters)
                {
                    if (item.Value.Type == typeof(LStatus)) continue;

                    keyVals.Add(new KeyValue() { Value = item.Key, Desc = $"Global.{item.Value.Alias}" });
                }
            }

            var box = new System.Windows.Controls.ComboBox
            {
                IsEnabled = !propertyItem.IsReadOnly,
                ItemsSource = keyVals,
                DisplayMemberPath = "Desc",
                SelectedValuePath = "Value",
            };

            box.SelectionChanged -= Box_SelectionChanged;
            box.SelectionChanged += Box_SelectionChanged;

            return box;
        }

        private void Box_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var val = e.AddedItems[0] as KeyValue;
                if (val != null)
                {
                    pAttr.Value = val.Value;
                }
            }
        }

        public override DependencyProperty GetDependencyProperty() => System.Windows.Controls.ComboBox.SelectedValueProperty;
    }
}