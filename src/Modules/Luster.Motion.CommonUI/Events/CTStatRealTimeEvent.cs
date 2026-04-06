using Luster.Common.DataAccess.Tables;
using Prism.Events;
using System.Collections.Generic;

namespace Luster.Motion.CommonUI.Events
{
    /// <summary>
    /// CT统计实时数据更新事件
    /// 当DbManager生成新的ctInfoSelected数据时发布此事件
    /// </summary>
    public class CTStatRealTimeEvent : PubSubEvent<List<TbCTInfo2>>
    {
    }
}
