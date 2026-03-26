#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AnalogConvert
* 机器名称:       F05852
* 命名空间:       Luster.Module.Motion.Device.Functions
* 文 件 名:       AnalogueCali.cs
* 创建时间:       2023/3/1 11:06:43
* 作    者:       F05852
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       leifan@lusterinc.com 
* 唯一标识：      cba51670-d782-4db2-b175-166def5c037c
* 登录用户:       樊磊
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/3/1 11:06:43
* 修 改 人:		  F05852
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Device.Functions
{
    public class AnalogConvert : MotionFunction
    {
        [NotEmpty]
        [Parameter("选择对应的模拟量IO", 1, CN = "模拟量IO", EditorType = typeof(VIO))]
        public VDevice Device { get; set; }

        [Parameter("显示板卡读取到的模拟量值", 2, CN = "输出原始值")]
        public bool ShowCardAnalogValue { get; set; }

        [NotEmpty]
        [Parameter("填写外部设备真实的值1", 11, CN = "示教外设值1", DefaultV = 0)]
        public double CaliDeviceRealValue1 { get; set; }

        [NotEmpty]
        [Parameter("填写外部设备真实的值2", 12, CN = "示教外设值2", DefaultV = 10)]
        public double CaliDeviceRealValue2 { get; set; }

        [Parameter("填写板卡读取的模拟量值1", 13, CN = "示教板卡值1", DefaultV = 0)]
        public int CaliAnalogValue1 { get; set; }

        [Parameter("填写板卡读取的模拟量值2", 14, CN = "示教板卡值2", DefaultV = 1000)]
        public int CaliAnalogValue2 { get; set; }

        [Parameter("将板卡读取到的模拟值转换成外设真实的值", 100, CN = "外设真实值", ParamType = TaskFlow.Common.Enums.ParamType.OUT, DefaultV = 0)]
        public double DeviceRealValue { get; set; }

        [DependOn("ShowCardAnalogValue", true)]
        [Parameter("板卡读取到的模拟值", 101, CN = "板卡原始值", ParamType = TaskFlow.Common.Enums.ParamType.OUT, DefaultV = 0)]
        public double CardAnalogValue { get; set; }

        private double ratio = 1;
        private double offset = 0;
        // y = ratio * x + offset
        // x 为板卡读取的模拟量，y 为对应输出的设备值

        public AnalogConvert()
        {
            this.Tips = "设备模拟量获取与转换";
            this.Icon = "\xe67f";
        }

        public override bool DoExcute(out string errMsg)
        {
            // 空跑模式直接返回
            if (IsEmptyMode)
            {
                errMsg = "";

                CardAnalogValue = 0;
                DeviceRealValue = 0;

                return true;
            }

            var io = MyOwner.DeviceEngine.GetVirtualByID(Device.DeviceID) as VIO;
            CardAnalogValue = io.GetAnglogIn();

            // y=ratio*x+offset
            ratio = (CaliDeviceRealValue2 - CaliDeviceRealValue1) / (double)(CaliAnalogValue2 - CaliAnalogValue1);
            offset = CaliDeviceRealValue1 - ratio * CaliAnalogValue1;

            DeviceRealValue =Math.Round(CardAnalogValue * ratio + offset,3);

            return base.DoExcute(out errMsg);
        }


    }
}