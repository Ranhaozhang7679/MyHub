using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Luster.Common.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using System.IO;

namespace Luster.Module.Motion.DataProc.Functions
{
    public class RunExe : MotionFunction
    {
        private readonly string Path = AppDomain.CurrentDomain.BaseDirectory + "NetForSFC.exe";

        [Parameter("exe路径",1,CN ="运行指定exe的路径",ParamType =TaskFlow.Common.Enums.ParamType.IN)]
        public LPath LPathExe { get; set; }


        public RunExe()
        {
            Tips = "执行程序";
            Icon = "\xe6af";
        }
        public override bool DoExcute(out string errMsg)
        {
            try
            {
                Process.Start(LPathExe.ToString());
            }
            catch 
            {

            }
            return base.DoExcute(out errMsg);
        }
    }
}
