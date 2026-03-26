#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LoadingSilo
* 机器名称:       Z05150
* 命名空间:       Luster.Module.Motion.Business.Functions
* 文 件 名:       LoadingSilo.cs
* 创建时间:       2022/7/14 9:34:36
* 作    者:       张宇
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       yuzhang5150@lusterinc.com 
* 唯一标识：      f23ec222-6345-4885-a22d-7e7e487b0cc6
* 登录用户:       zhangyu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/14 9:34:36
* 修 改 人:		  Z05150
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Business.Functions
{
    /// <summary>
    /// 料仓上下料类型
    /// </summary>
    public enum SiloMoveType
    {
        [Description("Z轴上下料")]
        ZAxis,

        [Description("顶升气缸上下料")]
        JackingCylinder
    }

    /// <summary>
    /// 阻挡设备类型
    /// </summary>
    public enum BlockType
    {
        [Description("气缸")]
        CylDevice,

        [Description("IO仿真设备")]
        IoDevice
    }

    /// <summary>
    /// 上料仓
    /// </summary>
    public class LoadingSilo : MotionFunction, IPauseFunction
    {
        [NotEmpty]
        [Parameter("用于上下料设备", 0, CN = "料仓类型", DefaultV = SiloMoveType.ZAxis)]
        public SiloMoveType SiloMoveType { get; set; }

        [NotEmpty]
        [Parameter("IO类型", 1, CN = "来料检测", EditorType = typeof(VIO))]
        public VDevice ComeIO { get; set; }

        [NotEmpty]
        [Parameter("IO类型", 2, CN = "到料检测1", EditorType = typeof(VIO))]
        public VDevice PlaceIO { get; set; }

        [NotEmpty]
        [Parameter("IO类型", 2, CN = "到料检测2", EditorType = typeof(VIO))]
        public VDevice PlaceIO2 { get; set; }

        [Limit(1, 500)]
        [Parameter("Z轴Jog速度，不受比例控制", 2, CN = "Jog速度", CanRef = ParamRef.Ref, DefaultV = 100)]
        public int ZJogSpd { get; set; }

        [Limit(1000, 30000)]
        [Parameter("Z轴上升超时,单位ms", 2, CN = "上升超时", DefaultV = 10000)]
        public int ZJogTimeOut { get; set; }

        [NotEmpty]
        [Parameter("IO类型", 1, CN = "下方传感器", EditorType = typeof(VIO))]
        public VDevice DownSensor { get; set; }


        #region 轴参数
        [DependOn("SiloMoveType", SiloMoveType.ZAxis)]
        [Parameter("单轴仿真设备", 3, CN = "单轴")]
        public VAxisDevice AxisZ { get; set; }

        [DependOn("SiloMoveType", SiloMoveType.ZAxis)]
        [Parameter("轴运动方向", 4, CN = "轴运动方向", DefaultV = true)]
        public bool AxisDirection { get; set; }

        [DependOn("SiloMoveType", SiloMoveType.ZAxis)]
        [Parameter("预警位", 5, CN = "预警位")]
        public double AlarmPalce { get; set; }

        [DependOn("SiloMoveType", SiloMoveType.ZAxis)]
        [Parameter("料盘高度", 6, CN = "料盘高度")]
        public double TrayHeight { get; set; }
        #endregion

        #region 顶升气缸参数
        [DependOn("SiloMoveType", SiloMoveType.JackingCylinder)]
        [Parameter("气缸仿真类型", 7, CN = "顶升气缸", EditorType = typeof(VCylinder))]
        public VDevice CylJacking { get; set; }

        [DependOn("SiloMoveType", SiloMoveType.JackingCylinder)]
        [Parameter("气缸仿真设备", 8, CN = "分盘气缸", EditorType = typeof(VCylinder))]
        public VDevice Cylinder { get; set; }
        #endregion

        [Parameter("空仓信号", 9, CN = "空仓信号", CanRef = ParamRef.Ref)]
        public bool EmptyBin { get; set; }

        public bool EnableAlarm { get; set; }

        public LoadingSilo()
        {
            this.Tips = "上料仓";
            this.Icon = "\xe66e";
        }

        public override bool DoExcute(out string errMsg)
        {
            //0.硬件获取
            GetVDevice<VIO>(ComeIO, out var comeIO);
            GetVDevice<VIO>(PlaceIO2, out var placeIO2);

            GetVDevice<VIO>(PlaceIO, out var placeIO);
            GetVDevice<VIO>(DownSensor, out var downSensor);


            if (SiloMoveType == SiloMoveType.ZAxis)
            {
                GetVDevice<VAxis>(AxisZ, out var axisZ);

                // 1、等待来料
                if (comeIO.GetDigitalIn())
                {
                    // 2、Z轴动,Z轴速度只能是定值，太大会过冲，太小会影响ct
                    axisZ.Jog(AxisDirection, ZJogSpd, true);

                    placeIO.WaitIO(true, ZJogTimeOut);
                    placeIO2.WaitIO(true, ZJogTimeOut);

                    axisZ.Stop();
                }
                else if (axisZ.GetCurrentPos() < AlarmPalce || !comeIO.GetDigitalIn())
                {
                    //Z轴正下方没有托盘感应，才允许下降
                    //否则报警，提示拿走
                    if (!downSensor.GetDigitalIn())
                    {
                        // Z轴回到原点，等待上料
                        axisZ.MoveAbs(new VAxisDevice() { Axis = axisZ, Position = 0, Speed = axisZ.MoveSpeed });
                        axisZ.CheckMotionDone();
                        // 空仓
                        SetParameterRefVal(nameof(EmptyBin), true);
                    }
                    else
                    {
                        OnAlarm(Luster.Motion.DataStruct.Enums.AlarmType.InfoTip, "Z轴正下方有Tray,请取走！！！");
                    }

                }
                else
                {
                    OnAlarm(Luster.Motion.DataStruct.Enums.AlarmType.InfoTip, "上料仓传感器感应异常！！！");
                }

            }
            else if (SiloMoveType == SiloMoveType.JackingCylinder)
            {

                GetVDevice<VCylinder>(Cylinder, out var cylinder);
                GetVDevice<VCylinder>(CylJacking, out var cylJacking);

                // 1、顶升气缸伸出
                cylJacking.Extend();
                cylJacking.InExtendSensorCheck(3000);

                // 2、分盘气缸缩回
                cylinder.Retract();
                cylinder.InRetractSensorCheck(3000);

                // 3、分盘气缸伸出
                cylinder.Extend();
                cylinder.InExtendSensorCheck(3000);

                // 4、顶升气缸缩回
                cylJacking.Retract();
                cylJacking.InRetractSensorCheck(3000);

                // 5、等待来料
                if (!comeIO.GetDigitalIn())
                {
                    // 空仓
                    SetParameterRefVal(nameof(EmptyBin), true);
                }
            }

            return base.DoExcute(out errMsg);
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
                                        OnAlarm(Luster.Motion.DataStruct.Enums.AlarmType.Timeout, "料盘即将清空，请及时补料");
                                        Thread.Sleep(4);
                                    }
                                }
                                else if (SiloMoveType != SiloMoveType.JackingCylinder)
                                {
                                    GetVDevice<VIO>(PlaceIO, out var placeIo);

                                    if (placeIo.GetDigitalIn())
                                    {
                                        OnAlarm(Luster.Motion.DataStruct.Enums.AlarmType.Timeout, "料盘已清空，请及时补料");
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
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            tokenSource?.Cancel();
        }

        public override bool IsNeedPause => true;
    }
}