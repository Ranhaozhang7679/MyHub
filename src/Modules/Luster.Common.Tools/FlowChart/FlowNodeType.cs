using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.Tools.FlowChart
{
    [Serializable]
    public enum FlowNodeType
    {
        Node,//其他节点
        Component,//组件Group,AsyncGroup,FreeStation
        Condition,//条件Judge
        Branch,//分支Switch
        Parallel,//并行Parallel
        Cycle,//循环Loop
        Return,// Return
        Goto//GoToModule
    }
}
