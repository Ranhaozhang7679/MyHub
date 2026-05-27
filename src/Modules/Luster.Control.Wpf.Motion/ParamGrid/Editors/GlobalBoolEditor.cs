using Luster.Common.DataStruct.DataModels;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Control.Wpf.Motion.Editors
{
    /// <summary>
    /// 布尔类型全局变量选择编辑器（仅显示bool类型变量，带筛选功能）
    /// </summary>
    public class GlobalBoolEditor : GlobalEditor
    {
        protected override List<KeyValue> BuildItems()
        {
            var keyVals = new List<KeyValue>();
            var gID = GlobalModule.GlobalID;
            if (pAttr.Owner.TaskModules.Contains(gID))
            {
                var gModule = pAttr.Owner.TaskModules[gID];
                foreach (var item in gModule.Parameters)
                {
                    if (item.Value.Type == typeof(LStatus)) continue;
                    if (item.Value.Type != typeof(bool)) continue;

                    keyVals.Add(new KeyValue() { Value = item.Key, Desc = $"Global.{item.Value.Alias}" });
                }
            }
            return keyVals;
        }
    }
}
