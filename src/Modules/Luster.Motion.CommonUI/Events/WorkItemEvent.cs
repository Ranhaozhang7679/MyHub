using Luster.Motion.CommonUI.Models;
using Luster.Motion.TaskFlow.Engine.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.Events
{
    public class WorkItemAddedEvent : PubSubEvent<WorkItem>
    {
    }
}
