using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.Events
{
    /// <summary>
    /// Hive网络连接失败/注册失败的报警弹窗关闭事件
    /// </summary>
    public class HiveReportStateChangedEvent : PubSubEvent<bool>
    {
    }
}