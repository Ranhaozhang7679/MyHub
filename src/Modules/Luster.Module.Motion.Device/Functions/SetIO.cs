#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       WriteOutput
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Device.Functions
* 文 件 名:       WriteOutput.cs
* 创建时间:       2022/5/30 9:36:41
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      b0171b65-cb6e-4fd0-97fd-62a80d2a0632
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/30 9:36:41
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Device.Functions
{
    public class SetIO : MotionFunction
    {
        [NotEmpty]
        [Parameter("IO类型，通过关键字搜索", 0, CN = "IO名称", EditorType = typeof(VIO), CanRef = ParamRef.None)]
        public VDevice IO { get; set; }

        [Parameter("IO类型，通过关键字搜索", 1, CN = "IO类型")]
        public IOType IOType { get; set; }

        [DependOn("IOType", IOType.Digital)]
        [Parameter("数字信号", 2, CN = "数字信号", CanRef = ParamRef.Ref)]
        public bool DigitalVal { get; set; }

        [DependOn("IOType", IOType.Analog)]
        [Parameter("模拟信号", 3, CN = "模拟信号", CanRef = ParamRef.Ref)]
        public double AnglogVal { get; set; }

        /// <summary>
        /// 自动注释
        /// </summary>
        public override string[] NoteParams => new string[] { "设置", nameof(Device) };
        public SetIO()
        {
            this.Tips = "设置IO值";
            this.Icon = "\xe670";
        }

        public override bool DoExcute(out string errMsg)
        {
            GetVDevice<VIO>(IO, out var io);
            if (IOType == IOType.Digital)
            {
                io.SetDigital(DigitalVal);
            }
            else
            {
                io.SetAnglog(AnglogVal);
            }

            return base.DoExcute(out errMsg);
        }
    }
}