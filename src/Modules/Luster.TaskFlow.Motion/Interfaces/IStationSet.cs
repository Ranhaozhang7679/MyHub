using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Interfaces
{
    public interface IStationSet
    {
        bool IsClose { get; set; }
    }
}
