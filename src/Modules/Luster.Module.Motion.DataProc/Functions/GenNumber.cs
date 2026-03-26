#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       GenNumber
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.DataProc.Functions
* 文 件 名:       GenNumber.cs
* 创建时间:       2022/9/23 9:06:42
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      9ae9f59d-538c-4439-867e-30d5fa232b79
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/23 9:06:42
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.DataProc.Functions
{
    public class GenRandomNumber : MotionFunction
    {
        [Parameter("数字类型", 0, CN = "数据类型", DefaultV = DataType.Int)]
        public DataType DataType { get; set; }

        [Parameter("数字类型", 1, DecimalPlaces = 3, CN = "最小值", DefaultV = 0)]
        public double Min { get; set; }

        [Parameter("数字类型", 2, DecimalPlaces = 3, CN = "最大值", DefaultV = 100)]
        public double Max { get; set; }


        [Parameter("数字类型", 3, CN = "随机数", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double RandomVal { get; set; }

        public GenRandomNumber()
        {
            this.Tips = "创建随机数";
            this.Icon = "\xe6ad";
        }


        public static double NextDouble(Random random, double miniDouble, double maxiDouble)
        {
            if (random != null)
            {
                return Math.Round(random.NextDouble() * (maxiDouble - miniDouble) + miniDouble, 3);
            }
            else
            {
                return 0.0d;
            }
        }

        public override bool DoExcute(out string errMsg)
        {
            Random random = new Random();
            switch (DataType)
            {
                case DataType.Bool:
                    RandomVal = 0;
                    break;
                case DataType.String:
                    RandomVal = 0;
                    break;
                case DataType.Int:
                    RandomVal = random.Next((int)Min, (int)Max);
                    break;
                case DataType.Double:
                    RandomVal = NextDouble(random, Min, Max);
                    break;
                case DataType.Short:
                    RandomVal = random.Next((int)Min, (int)Max);
                    break;
                default:
                    RandomVal = 0;
                    break;
            }

            return base.DoExcute(out errMsg);
        }
    }
}