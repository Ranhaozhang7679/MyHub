using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.Events
{
    /// <summary>
    /// 多语言切换
    /// </summary>
    public class LangChangedEvent : PubSubEvent<string>
    {

    }

    public class LangChangedDoneEvent : PubSubEvent
    {

    }
}
