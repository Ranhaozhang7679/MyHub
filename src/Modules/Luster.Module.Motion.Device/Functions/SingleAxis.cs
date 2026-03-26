#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Axis
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Device.Functions
* 文 件 名:       Axis.cs
* 创建时间:       2022/6/13 11:18:20
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      22a59be9-3630-4b45-a330-90c9ac6de525
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/13 11:18:20
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Device.Functions
{
    public class SingleAxis : MotionFunction, IGetExtResult, IPauseFunction, IStopFunction, INote
    {
        [NotEmpty]
        [Parameter("IO类型，通过关键字搜索", 0, CN = "轴名称")]
        public VAxisDevice Device { get; set; }

        [Parameter("运动类型", 1, CN = "运动类型", DefaultV = AxisMode.Move)]
        public AxisMode AxisMode { get; set; }

        [DependOn("AxisMode", AxisMode.Move)]
        [Parameter("点动方式", 2, CN = "点动方式", DefaultV = MoveMode.Abs)]
        public MoveMode MMode { get; set; }

        [DependOn("AxisMode", AxisMode.JOG)]
        [Parameter("运动模式分为JOG和点位模式", 3, CN = "转动方向", DefaultV = MoveDirection.Positive)]
        public MoveDirection MoveDirection { get; set; }

        [DependOn("AxisMode", AxisMode.JOG)]
        [Parameter("在皮带模式下，轴的动作", 4, CN = "轴动作", DefaultV = JOGMethod.Start)]
        public JOGMethod JOGMethod { get; set; }

        [DependOn("AxisMode", AxisMode.JOG)]
        [DependOn("JogFixSpd", true)]
        [Limit(1, 500)]
        [Parameter("Jog速度，不受比例控制", 11, CN = "Jog速度", CanRef = ParamRef.Ref, DefaultV = 100)]
        public int JogSpd { get; set; }

        [DependOn("AxisMode", AxisMode.JOG)]
        [Parameter("Jog速度固定，就不受比例控制", 12, CN = "Jog速度固定", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool JogFixSpd { get; set; }

        /// <summary>
        /// 正负10m
        /// </summary>
        [DependOn("AxisMode", AxisMode.Move)]
        [Parameter("位置,移动位置单位毫米", 6, CN = "位置补偿", CanRef = ParamRef.Ref)]
        public double Compensate { get; set; }

        [DependOn("AxisMode", AxisMode.Home)]
        [Parameter("检查电缸运动是否完成", 7, CN = "检查完成", DefaultV = true)]
        public bool CheckDone { get; set; }

        [DependOn("AxisMode", AxisMode.Home)]
        [Parameter("在回零模式下超时时间,单位是s", 8, CN = "回零超时", DefaultV = 60)]
        public int HomeTimeout { get; set; }

        [DependOn("AxisMode", AxisMode.JOG)]
        [Parameter("IO类型，通过关键字搜索，控制JOG停止", 9, CN = "JOG停止IO", EditorType = typeof(VIO), CanRef = ParamRef.None)]
        public VDevice StopIO { get; set; }


        [DependOn("AxisMode", AxisMode.JOG)]
        [Parameter("等待JOG停止IO的值==停止值", 10, CN = "JOG停止值", DefaultV = true)]
        public bool IOValue { get; set; }


        //[DependOn("AxisMode", AxisMode.JOG)]
        //[Parameter("取消异步线程", 10, CN = "取消异步线程", DefaultV = false, CanRef = ParamRef.Ref)]
        //public bool CancelAsynTask { get; set; }


        /// <summary>
        /// 变量的值
        /// </summary>
        [DependOn("AxisMode", AxisMode.JOG)]
        [DependOn("JOGMethod", JOGMethod.Start)]
        [Parameter("取消异步线程", 10, CN = "取消异步线程", EditorType = typeof(IGlobal))]
        public string GlobalVar { get; set; }


        [Parameter("当前位置", 10, CN = "当前位置", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double OutPos { get; set; }

        public override string[] NoteParams => new string[] { nameof(Device), nameof(AxisMode) };

        // JOG遇到感应器停止函数使用
        private bool StopJogIoCheck = false;


        /// <summary>
        /// 全局变量
        /// </summary>
        private IMotionModule gModule = null;
        /// <summary>
        /// 参数
        /// </summary>
        private ParameterAttribute gParameter = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public SingleAxis()
        {
            this.Tips = "单轴";
            this.Icon = "\xe678";
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="errMsg">错误消息</param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {

            //获取全局模块
            if (gModule == null)
            {
                gModule = MyOwner.TaskModules[GlobalModule.GlobalID] as IMotionModule;
            }

            GetVDevice<VAxis>(Device, out var axis);
            if (axis == null)
            {
                errMsg = $"设备:{Device.Name}未找到";
                return false;
            }

            if (AxisMode == AxisMode.Move)
            {
                // 更新动态补偿
                Device.Compensate = Compensate;
                double targetPulse;

                if (MMode == MoveMode.Abs)
                {
                    targetPulse = (Device.Position + Compensate) * axis.PerPluse;
                    axis.MoveAbs(Device);
                }
                else
                {
                    targetPulse = (axis.GetCurrentPos() + Compensate) * axis.PerPluse;
                    axis.MoveRel(Device);
                }

                axis.CheckMotionDone(Device,0,null,targetPulse);

                OutPos = axis.GetCurrentPos();
            }
            else if (AxisMode == AxisMode.Home)
            {
                axis.Home();
                if (CheckDone)
                {
                    axis.CheckHomeDone(HomeTimeout);
                }
            }
            else if (AxisMode == AxisMode.JOG)
            {
                if (JOGMethod == JOGMethod.Start)
                {
                    GetVDevice<VIO>(StopIO, out var stopIO);

                    // 初始等待停止信号满足，不用执行jog
                    if (stopIO != null && stopIO.GetDigital() == IOValue)
                    {
                        MyOwner.OnLog(Luster.Common.DataStruct.Enums.LogType.Debug, $"模块 {MyOwner.Alias} {IOValue} 初始等待停止信号满足，不执行JOG，直接完成");
                    }
                    else
                    {
                        if (JogFixSpd)
                        {
                            axis.Jog(MoveDirection == MoveDirection.Positive, JogSpd, true);
                        }
                        else
                        {
                            Device.Direction = MoveDirection;
                            axis.Jog(Device);
                        }

                        if (stopIO != null)
                        {
                            // 用于取消上次未完成的异步线程 单次循环的时间要小于30ms
                            StopJogIoCheck = true;
                            Thread.Sleep(30);

                            Task.Run(() =>
                            {
                                StopJogIoCheck = false;

                                MyOwner.OnLog(Luster.Common.DataStruct.Enums.LogType.Debug, $"模块 {MyOwner.Alias} {IOValue} 开始等待....");
                                while (true)
                                {

                                    //获取全局变量
                                    if (!string.IsNullOrEmpty(GlobalVar))
                                    {
                                        if (gParameter == null)
                                        {
                                            if (!gModule.Parameters.ContainsKey(GlobalVar))
                                            {
                                                MyOwner.OnLog(Luster.Common.DataStruct.Enums.LogType.Debug, $"全局变量:{GlobalVar}不存在!");
                                            }
                                            gParameter = gModule.Parameters[GlobalVar];
                                        }
                                    }

                                    //实时拿到全局对象的值
                                    object pVal = gParameter?.Value;
                                    if(pVal==null)
                                    {
                                        pVal = false;
                                    }
                                    if (StopJogIoCheck|| pVal.Equals(true))
                                    {
                                        MyOwner.OnLog(Luster.Common.DataStruct.Enums.LogType.Debug, $"模块 {MyOwner.Alias} {IOValue} 被取消....");
                                        break;
                                    }

                                    if (stopIO.GetDigital() == IOValue)
                                    {
                                        axis.Stop();
                                        Thread.Sleep(100);
                                        MyOwner.OnLog(Luster.Common.DataStruct.Enums.LogType.Debug, $"模块 {MyOwner.Alias} {IOValue} 等待成功完成....");
                                        break;
                                    }
                                    Thread.Sleep(1);
                                }
                            });
                        }
                    }
                }
                else
                {
                    axis.Stop();
                    OutPos = axis.GetCurrentPos();
                }
            }

            else if(AxisMode==AxisMode.GetPosition)
            {
                OutPos = axis.GetCurrentPos();
            }
            else if (AxisMode == AxisMode.GetOrg)
            {
                axis.ResetStatus();
                Thread.Sleep(10);
                OutPos = axis.GetAxisStatus().ToList().Where(x => x.Key == AxisStatus.Org).First().Value ? 1 : 0;
            }
            else if (AxisMode == AxisMode.GetPEL)
            {
                axis.ResetStatus();
                Thread.Sleep(10);
                OutPos = axis.GetAxisStatus().ToList().Where(x => x.Key == AxisStatus.Pel).First().Value ? 1 : 0;
            }
            else if (AxisMode == AxisMode.GetAEL)
            {
                axis.ResetStatus();
                Thread.Sleep(10);
                OutPos = axis.GetAxisStatus().ToList().Where(x => x.Key == AxisStatus.Mel).First().Value ? 1 : 0;
            }

            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 获取模块额外参数结果
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetExtResult()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            GetVDevice<VAxis>(Device, out var axis);
            if (axis != null)
            {
                double speed = axis.MoveSpeed * 0.8;
                result.Add($"{axis.AxisType}_Spd", axis.GetSpeed(Device.Speed));
                result.Add($"{axis.AxisType}_SpdTarget", speed);
                result.Add($"{axis.AxisType}_Distance", OutPos - Device.PrevMovePos);
                result.Add($"{axis.AxisType}_RotateSpd", 3000);
                result.Add($"{axis.AxisType}_Path", 10000 / (axis.PerPluse == 0 ? 1 : axis.PerPluse));
                result.Add($"{axis.AxisType}_AccTarget", Math.Sqrt(2 * speed / axis.Acc));
            }

            return result;
        }

        public override void Stop()
        {
            GetVDevice<VAxis>(Device, out var vAxis);
            if (vAxis != null)
            {
                vAxis.Stop();
            }
        }

        /// <summary>
        /// 需要暂停
        /// </summary>
        public override bool IsNeedPause => true;
    }
}