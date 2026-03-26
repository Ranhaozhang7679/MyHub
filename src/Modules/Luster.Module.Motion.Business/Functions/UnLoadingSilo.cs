#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       UnLoadingSilo
* 机器名称:       Z05150
* 命名空间:       Luster.Module.Motion.Business.Functions
* 文 件 名:       UnLoadingSilo.cs
* 创建时间:       2022/7/14 9:35:00
* 作    者:       张宇
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       yuzhang5150@lusterinc.com 
* 唯一标识：      55a362dd-157e-4c58-a7ec-91e5a6493d26
* 登录用户:       zhangyu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/14 9:35:00
* 修 改 人:		  Z05150
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Functions;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Business.Functions
{
    /// <summary>
    /// 下料仓
    /// </summary>
    public class UnLoadingSilo : MotionFunction, IPauseFunction
    {
        [NotEmpty]
        [Parameter("用于上下料设备", 0, CN = "料仓类型", DefaultV = SiloMoveType.ZAxis)]
        public SiloMoveType SiloMoveType { get; set; }

        [NotEmpty]
        [Parameter("IO类型", 2, CN = "来料检测", EditorType = typeof(VIO))]
        public VDevice ComeIO { get; set; }

        [NotEmpty]
        [Parameter("IO类型", 3, CN = "到料检测", EditorType = typeof(VIO))]
        public VDevice PlaceIO { get; set; }

        [NotEmpty]
        [Parameter("用于阻挡料", 4, CN = "阻挡设备类型", DefaultV = BlockType.CylDevice)]
        public BlockType BlockType { get; set; }

        #region 阻挡设备类型
        [DependOn("BlockType", BlockType.CylDevice)]
        [Parameter("气缸仿真设备", 5, CN = "插销气缸", EditorType = typeof(VCylinder))]
        public VDevice Cylinder { get; set; }

        [DependOn("BlockType", BlockType.IoDevice)]
        [Parameter("IO集成仿真设备", 6, CN = "IO集成设备", EditorType = typeof(VIOSimulation))]
        public VDevice IoSimulation { get; set; }

        [DependOn("BlockType", BlockType.IoDevice)]
        [Parameter("用于吸取物体的设备", 7, CN = "动作")]
        public string Action { get; set; }

        [DependOn("BlockType", BlockType.IoDevice)]
        [Parameter("用于吸取物体的设备", 8, CN = "动作完成")]
        public string Check { get; set; }
        #endregion

        #region 轴参数
        [DependOn("SiloMoveType", SiloMoveType.ZAxis)]
        [Parameter("单轴仿真设备", 9, CN = "单轴", EditorType = typeof(VAxis))]
        public VDevice AxisZ { get; set; }

        [DependOn("SiloMoveType", SiloMoveType.ZAxis)]
        [Parameter("预警位", 10, CN = "预警位")]
        public double AlarmPalce { get; set; }

        [DependOn("SiloMoveType", SiloMoveType.ZAxis)]
        [Parameter("下料位", 11, CN = "下料位")]
        public double UnloadPlace { get; set; }

        [DependOn("SiloMoveType", SiloMoveType.ZAxis)]
        [Parameter("料盘高度", 12, CN = "料盘高度")]
        public double TrayHeight { get; set; }
        #endregion

        #region 顶升气缸参数
        [DependOn("SiloMoveType", SiloMoveType.JackingCylinder)]
        [Parameter("气缸仿真类型", 13, CN = "顶升气缸", EditorType = typeof(VCylinder))]
        public VDevice CylJacking { get; set; }

        [DependOn("SiloMoveType", SiloMoveType.JackingCylinder)]
        [Parameter("气缸仿真类型", 14, CN = "插销升降气缸", EditorType = typeof(VCylinder))]
        public VDevice CylBlockJacking { get; set; }

        [DependOn("SiloMoveType", SiloMoveType.JackingCylinder)]
        [Parameter("气缸仿真类型", 15, CN = "分盘气缸", EditorType = typeof(VCylinder))]
        public VDevice CylDisking { get; set; }
        #endregion

        [Parameter("满仓信号", 17, CN = "满仓信号", CanRef = ParamRef.Ref)]
        public bool FullBin { get; set; }

        public bool EnableAlarm { get; set; }

        /// <summary>
        /// 超时时间
        /// </summary>
        [Parameter("超时", 16, CN = "超时时间", DefaultV = 3000)]
        public int TimeOut { get; set; }

        public UnLoadingSilo()
        {
            this.Tips = "下料仓";
            this.Icon = "\xe66f";
        }

        public override bool DoExcute(out string errMsg)
        {
            GetVDevice<VIO>(ComeIO, out var comeIO);
            GetVDevice<VIO>(PlaceIO, out var placeIO);
            GetVDevice<VCylinder>(Cylinder, out var cylinder);
            GetVDevice<VIOSimulation>(IoSimulation, out var iOSimulation);

            if (SiloMoveType == SiloMoveType.ZAxis)
            {
                GetVDevice<VAxis>(AxisZ, out var axisZ);

                if (!comeIO.GetDigitalIn() || axisZ.GetCurrentPos() >= AlarmPalce)
                {
                    axisZ.MoveRel(UnloadPlace);
                }

                // 当前位置与预警位料位的间距
                double space = axisZ.GetCurrentPos() - AlarmPalce;

                if (space <= (TrayHeight * 2) || space >= -(TrayHeight * 2))
                {
                    if (BlockType == BlockType.CylDevice)
                    {
                        // 阻挡气缸或插销气缸伸出
                        cylinder.Extend();
                        cylinder.InExtendSensorCheck(3000);
                    }
                    else if (BlockType == BlockType.IoDevice)
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
                    // 下降一个料盘
                    axisZ.MoveRel(-TrayHeight);

                    // 8、满仓
                    if (placeIO.GetDigitalIn())
                        SetParameterRefVal(nameof(FullBin), true);
                }
                else
                {
                    // Z轴回到原点，等待清料
                    axisZ.MoveRel(0);

                    if (BlockType == BlockType.CylDevice)
                    {
                        // 满料时，阻挡气缸或插销气缸缩回
                        cylinder.Retract();
                        cylinder.InRetractSensorCheck(3000);
                    }
                    else if (BlockType == BlockType.IoDevice)
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
                }

            }
            else if (SiloMoveType == SiloMoveType.JackingCylinder)
            {
                GetVDevice<VCylinder>(CylJacking, out var cylJacking);
                GetVDevice<VCylinder>(CylBlockJacking, out var cylBlockJacking);
                GetVDevice<VCylinder>(CylDisking, out var cylDisking);

                // 1、等待来料
                if (comeIO.GetDigitalIn())
                {
                    // 2、分盘气缸伸出
                    if (cylDisking != null)
                    {
                        cylDisking.Extend();
                        cylDisking.InExtendSensorCheck(TimeOut);

                        Thread.Sleep(200);
                    }

                    // 3、顶升气缸伸出
                    if (cylJacking != null)
                    {
                        cylJacking.Extend();
                        cylJacking.InExtendSensorCheck(TimeOut);

                        Thread.Sleep(200);
                    }

                    // 4、插销气缸伸出
                    if (cylinder != null)
                    {
                        cylinder.Extend();
                        cylinder.InExtendSensorCheck(TimeOut);

                        Thread.Sleep(200);
                    }

                    // 5、顶升气缸缩回
                    if (cylJacking != null)
                    {
                        cylJacking.Retract();
                        cylJacking.InRetractSensorCheck(TimeOut);

                        Thread.Sleep(200);
                    }

                    // 6、分盘气缸缩回
                    if (cylDisking != null)
                    {
                        cylDisking.Retract();
                        cylDisking.InRetractSensorCheck(TimeOut);

                        Thread.Sleep(200);
                    }

                    // 7、插销升降气缸伸出
                    if (cylBlockJacking != null)
                    {
                        cylBlockJacking.Extend();
                        cylBlockJacking.InExtendSensorCheck(TimeOut);

                        Thread.Sleep(200);
                    }

                    // 8、分盘气缸伸出
                    if (cylDisking != null)
                    {
                        cylDisking.Extend();
                        cylDisking.InExtendSensorCheck(TimeOut);

                        Thread.Sleep(200);
                    }

                    // 9、插销升降气缸下降
                    if (cylBlockJacking != null)
                    {
                        cylBlockJacking.Retract();
                        cylBlockJacking.InRetractSensorCheck(TimeOut);

                        Thread.Sleep(200);
                    }

                    // 10、插销气缸缩回
                    if (cylinder != null)
                    {
                        cylinder.Retract();
                        cylinder.InRetractSensorCheck(TimeOut);
                    }

                    // 11、满仓
                    if (placeIO.GetDigitalIn())
                        SetParameterRefVal(nameof(FullBin), true);
                }
            }

            return base.DoExcute(out errMsg);
        }

        public override bool IsNeedPause => true;

        [Step(1, "等待来料")]
        public void WaitEmptyTray()
        {
            GetVDevice<VIO>(ComeIO, out var comeIO);

            comeIO.WaitIO(true, -1);
        }

        [Step(2, "分盘气缸伸出")]
        public void ExtendDiskingCyl()
        {
            GetVDevice<VCylinder>(CylDisking, out var cylDisking);

            if (cylDisking != null)
            {
                cylDisking.Extend();
                cylDisking.InExtendSensorCheck(3000);

                Thread.Sleep(200);
            }
            else
            {
                return;
            }
        }

        [Step(3, "顶升气缸伸出")]
        public void ExtendJackingCyl()
        {
            GetVDevice<VCylinder>(CylJacking, out var cylJacking);

            if (cylJacking != null)
            {
                cylJacking.Extend();
                cylJacking.InExtendSensorCheck(TimeOut);

                Thread.Sleep(200);
            }
            else
            {
                return;
            }
        }

        [Step(4, "插销气缸伸出")]
        public void ExtendCylinder()
        {
            GetVDevice<VCylinder>(Cylinder, out var cylinder);

            if (cylinder != null)
            {
                cylinder.Extend();
                cylinder.InExtendSensorCheck(TimeOut);

                Thread.Sleep(200);
            }
            else
            {
                return;
            }
        }

        [Step(5, "顶升气缸缩回")]
        public void RetractJackingCyl()
        {
            GetVDevice<VCylinder>(CylJacking, out var cylJacking);

            if (cylJacking != null)
            {
                cylJacking.Retract();
                cylJacking.InRetractSensorCheck(TimeOut);

                Thread.Sleep(200);
            }
            else
            {
                return;
            }
        }

        [Step(6, "分盘气缸缩回")]
        public void RetractDiskingCyl()
        {
            GetVDevice<VCylinder>(CylDisking, out var cylDisking);

            if (cylDisking != null)
            {
                cylDisking.Retract();
                cylDisking.InRetractSensorCheck(TimeOut);

                Thread.Sleep(200);
            }
            else
            {
                return;
            }
        }

        [Step(7, "插销升降气缸伸出")]
        public void ExtendBlockJackingCyl()
        {
            GetVDevice<VCylinder>(CylBlockJacking, out var cylBlockJacking);

            if (cylBlockJacking != null)
            {
                cylBlockJacking.Extend();
                cylBlockJacking.InExtendSensorCheck(TimeOut);

                Thread.Sleep(200);
            }
        }

        [Step(8, "分盘气缸伸出")]
        public void ExtendDiskingCylAgain()
        {
            GetVDevice<VCylinder>(CylDisking, out var cylDisking);

            if (cylDisking != null)
            {
                cylDisking.Extend();
                cylDisking.InExtendSensorCheck(TimeOut);

                Thread.Sleep(200);
            }
            else
            {
                return;
            }
        }

        [Step(9, "插销升降气缸下降")]
        public void RetractBlockJackingCyl()
        {
            GetVDevice<VCylinder>(CylBlockJacking, out var cylBlockJacking);

            if (cylBlockJacking != null)
            {
                cylBlockJacking.Retract();
                cylBlockJacking.InRetractSensorCheck(TimeOut);

                Thread.Sleep(200);
            }
            else
            {
                return;
            }
        }

        [Step(10, "插销气缸缩回")]
        public void RetractCylinder()
        {
            GetVDevice<VCylinder>(Cylinder, out var cylinder);

            if (cylinder != null)
            {
                cylinder.Retract();
                cylinder.InRetractSensorCheck(TimeOut);
            }
            else
            {
                return;
            }
        }

        [Step(11, "输出满仓信号")]
        public void OutputFullBinSignal()
        {
            GetVDevice<VIO>(PlaceIO, out var placeIO);

            if (placeIO.GetDigitalIn())
                SetParameterRefVal(nameof(FullBin), true);
            else
                return;
        }

        private static CancellationTokenSource tokenSource;

        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
            if (parameter.Name == "EnableAlarm")
            {
                if (bool.TryParse(newV?.ToString(), out var isEnable))
                {
                    if (isEnable)
                    {
                        tokenSource = new CancellationTokenSource();
                        Task.Run(() =>
                        {
                            while (true)
                            {
                                if (tokenSource.IsCancellationRequested)
                                {
                                    break;
                                }

                                if (SiloMoveType == SiloMoveType.ZAxis)
                                {
                                    GetVDevice<VAxis>(AxisZ, out var axisz);

                                    // 获取Z轴当前位置与预警位之间的间距
                                    double spacing = axisz.GetCurrentPos() - AlarmPalce;

                                    // 判断是否达到预警位
                                    if (spacing <= (TrayHeight * 5) || spacing >= -(TrayHeight * 5))
                                    {
                                        OnAlarm(Luster.Motion.DataStruct.Enums.AlarmType.Timeout, "料盘即将满仓，请及时清料");
                                        Thread.Sleep(4);
                                    }
                                }
                                else if (SiloMoveType != SiloMoveType.JackingCylinder)
                                {
                                    GetVDevice<VIO>(PlaceIO, out var placeio);

                                    if (placeio.GetDigitalIn())
                                    {
                                        OnAlarm(Luster.Motion.DataStruct.Enums.AlarmType.Timeout, "料盘即将满仓，请及时清料");
                                        Thread.Sleep(4);
                                    }
                                }
                            }
                        }, tokenSource.Token);
                    }
                    else
                    {
                        tokenSource?.Cancel();
                    }
                }
            }
            else if (parameter.Name == "IoSimulation")
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            tokenSource?.Cancel();
        }
    }
}