#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Feeder
* 机器名称:       Z05150
* 命名空间:       Luster.Module.Motion.Device.Functions
* 文 件 名:       Feeder.cs
* 创建时间:       2022/7/12 9:59:19
* 作    者:       张宇
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       yuzhang5150@lusterinc.com 
* 唯一标识：      f755faa4-fccd-4770-b0df-84c151e25fc5
* 登录用户:       zhangyu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/12 9:59:19
* 修 改 人:		  Z05150
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Device.Functions
{
    /// <summary>
    /// 飞达动作
    /// </summary>
    public enum FeederType
    {
        [Description("出标")]
        BidOut,

        [Description("抽拉")]
        PullOut
    }

    /// <summary>
    /// 飞达动作类型
    /// </summary>
    public enum FeederActionType
    {
        [Description("气缸")]
        CylDevice,

        [Description("IO仿真设备")]
        IoDevice
    }

    /// <summary>
    /// 飞达供料设备
    /// </summary>
    public class Feeder : OverTimeFunction
    {
        [NotEmpty]
        //[Parameter("飞达设备，通过关键字搜索", 0, CN = "飞达名称", EditorType = typeof(VFeeder))]
        public VDevice Device { get; set; }

        [NotEmpty]
        [Parameter("飞达动作类型", 1, CN = "动作类型", DefaultV = FeederActionType.CylDevice)]
        public FeederActionType FeederActionType { get; set; }

        #region 飞达动作类型
        [DependOn("FeederActionType", FeederActionType.CylDevice)]
        [Parameter("飞达要做的动作", 2, CN = "飞达动作", DefaultV = FeederType.BidOut)]
        public FeederType FType { get; set; }

        [DependOn("FeederActionType", FeederActionType.IoDevice)]
        [Parameter("IO集成仿真设备",3, CN = "IO集成设备", EditorType = typeof(VIOSimulation))]
        public VDevice IoSimulation { get; set; }

        [DependOn("FeederActionType", FeederActionType.IoDevice)]
        [Parameter("用于吸取物体的设备", 4, CN = "动作")]
        public string Action { get; set; }

        [DependOn("FeederActionType", FeederActionType.IoDevice)]
        [Parameter("用于吸取物体的设备", 5, CN = "动作完成")]
        public string Check { get; set; }
        #endregion

        public Feeder()
        {
            this.Tips = "飞达供料设备";
            this.Icon = "\xe691";
        }

        public override bool DoExcute(out string errMsg)
        {
            //GetVDevice<VFeeder>(Device, out var feeder);
            GetVDevice<VIOSimulation>(IoSimulation, out var iOSimulation);

            if (FeederActionType == FeederActionType.CylDevice)
            {
                if (FType == FeederType.BidOut)
                {
                    //feeder.BidOut(OverTime);

                    Thread.Sleep(1000);

                    //feeder.BidOutClose(OverTime);
                }
                else
                {
                    //feeder.PullOut(OverTime);

                    Thread.Sleep(1000);

                    //feeder.PullOutClose(OverTime);
                }
            }
            else if (FeederActionType == FeederActionType.IoDevice)
            {
                if (string.IsNullOrEmpty(Action))
                {
                    errMsg = "动作是必填的";
                    return false;
                }

                iOSimulation.Action(Action);

                // 感知动作是否完成
                if (!string.IsNullOrEmpty(Check))
                {
                    iOSimulation.Check(Check, 1000);
                }
            }


            return base.DoExcute(out errMsg);
        }

        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
            if (parameter.Name == "IoSimulation")
            {
                GetVDevice<VIOSimulation>(newV as VDevice, out var outIO);
                if (outIO != null)
                {
                    var actions = outIO.Actions.Where(u => !u.IsCheck).Select(u => u.Action).ToArray();
                    OnDataSource(nameof(Action), actions);

                    var checks = outIO.Actions.Where(u => u.IsCheck).Select(u => u.Action).ToArray();
                    OnDataSource(nameof(Check), checks);
                }
            }
        }

    }
}