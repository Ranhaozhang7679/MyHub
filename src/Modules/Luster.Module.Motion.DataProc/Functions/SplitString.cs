using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.DataProc.Functions
{
    public enum GlobalType
    {
        /// <summary>
        /// bool类型
        /// </summary>
        Bool,

        /// <summary>
        /// 字符串
        /// </summary>
        String,

        Int,

        /// <summary>
        /// double
        /// </summary>
        Double,
    }
    /// <summary>
    /// 字符串分割
    /// </summary>
    public class SplitString : MotionFunction
    {
        [Parameter("待分割的字符串", 0, CN = "字符串分割", ParamType = TaskFlow.Common.Enums.ParamType.IN, CanRef = ParamRef.Ref)]
        public string SplitStr { get; set; }

        [Parameter("分割符", 1, CN = "分割符", ParamType = TaskFlow.Common.Enums.ParamType.IN,DefaultV =',')]
        public char SplitName { get; set; }

        [Parameter("分割后的变量类型", 2, CN = "变量类型", DefaultV = GlobalType.Double)]
        public GlobalType GlabalType { get; set; }

        [Limit(0, 1000)]
        [Parameter("按位获取数据,从0开始", 3, CN = "位数", ParamType = TaskFlow.Common.Enums.ParamType.IN, DefaultV = 0)]
        public int Index { get; set; }

        [DependOn("GlabalType", GlobalType.Double)]
        [Parameter("按位获取的数据", 10, CN = "按位获取的数据", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double DoubleValue { get; set; }

        [DependOn("GlabalType", GlobalType.Int)]
        [Parameter("按位获取的数据", 11, CN = "按位获取的数据", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public int IntValue { get; set; }

        [DependOn("GlabalType", GlobalType.String)]
        [Parameter("按位获取的数据", 12, CN = "按位获取的数据", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string StringValue { get; set; }

        [DependOn("GlabalType", GlobalType.Bool)]
        [Parameter("按位获取的数据", 12, CN = "按位获取的数据", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool BoolValue { get; set; }



        /// <summary>
        /// 构造函数
        /// </summary>
        public SplitString()
        {
            this.Icon = "\xe6db";
            this.Tips = "字符串分割";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";

            var datas = SplitStr.Split(SplitName);

            if (datas.Length > 0)
            {
                switch (GlabalType)
                {
                    case GlobalType.Int:
                        IntValue = Convert.ToInt32(datas[Index]);
                        break;
                    case GlobalType.String:
                        StringValue = datas[Index];
                        break;

                    case GlobalType.Bool:
                        BoolValue = Convert.ToBoolean(datas[Index]);
                        break;

                    case GlobalType.Double:
                        DoubleValue = Convert.ToDouble(datas[Index]);
                        break;
                }
            }


            return base.DoExcute(out errMsg);
        }

    }
}
