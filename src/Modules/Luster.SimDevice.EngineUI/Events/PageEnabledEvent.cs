using Luster.Motion.DataStruct.Enums;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Events
{
    /// <summary>
    /// 页面启用或禁用
    /// </summary>
    public class PageEnabledEvent : PubSubEvent<bool>
    {
    }
}
