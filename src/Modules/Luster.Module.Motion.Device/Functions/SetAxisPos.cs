using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Module.Motion.Logic.Functions;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FreeSql.Internal.GlobalFilter;

namespace Luster.Module.Motion.Device.Functions
{
    public class SetAxisPos:MotionFunction
    {
        [NotEmpty]
        [Parameter("轴的点位及对应的参数配置", 1, CN = "点位参数", MultiValues = true)]
        public VAxisPos SelectAxisPos { get; set; }

        [Parameter("变量类型", 2, CN = "变量类型", DefaultV = GlobalType.Double)]
        public GlobalType GlabalType { get; set; }  

        [DependOn("GlabalType", GlobalType.Bool)]
        [Parameter("数据源", 3, CN = "变量值", CanRef = ParamRef.Ref)]
        public bool BoolVal { get; set; }

        [DependOn("GlabalType", GlobalType.String)]
        [Parameter("数据源", 4, CN = "变量值", CanRef = ParamRef.Ref, DefaultV = "")]
        public string StringVal { get; set; }

        [DependOn("GlabalType", GlobalType.Double)]
        [Parameter("数据源", 6, CN = "变量值", CanRef = ParamRef.Ref)]
        public double DoubleVal { get; set; }

        [DependOn("GlabalType", GlobalType.Int)]
        [Parameter("数据源", 7, CN = "变量值", CanRef = ParamRef.Ref)]
        public int IntVal { get; set; }

        public SetAxisPos()
        {
            this.Tips = "设置轴点位";
            this.Icon = "\xe6bd";
        }

        public override bool DoExcute(out string errMsg)
        {
            double sourcePra = 0;
            switch (GlabalType)
            {
                case GlobalType.Bool:
                    MyOwner.OnLog(Common.DataStruct.Enums.LogType.Debug, "设置轴点位：bool类型不匹配");
                    break;
                case GlobalType.String:
                    sourcePra = Convert.ToDouble(StringVal);
                    break;
                case GlobalType.Int:
                    sourcePra = IntVal;
                    break;
                case GlobalType.Double:
                    sourcePra = DoubleVal;               
                    break;
            }

            foreach (var item in SelectAxisPos)
            {
                item.AxisPostion.Position = sourcePra;
                MyOwner.OnLog(Common.DataStruct.Enums.LogType.Debug, $"设置轴点位{item.AxisPostion.Name}：{sourcePra}");
            }
            
            return base.DoExcute(out errMsg);
        }




        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);

            if (parameter.Name == nameof(SelectAxisPos) && newV is VAxisPos vPos)
            {
                vPos.UpdateAxis(MyOwner.DeviceEngine);
                BuildDynamicAxisPos(vPos, 5);
                BuildDynamicAxisPos(vPos, 20, TaskFlow.Common.Enums.ParamType.OUT, false);
            }
        }
    }
}
