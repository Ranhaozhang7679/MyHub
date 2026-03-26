using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using Luster.TaskFlow.Motion.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Device.Functions
{
    /// <summary>
    /// 转盘
    /// </summary>
    public class Turntable : MotionFunction, IPauseFunction, IStopFunction
    {
        [NotEmpty]
        [Parameter("轴类型，通过关键字搜索", 1, CN = "转盘轴")]
        public VAxisDevice Device { get; set; }

        [NotEmpty]
        [Parameter("到达目标位置，选择走的路径", 2, CN = "到位方式", DefaultV = MoveMethod.Nearest)]
        public MoveMethod MoveMethod { get; set; }

        [NotEmpty]
        [Range(0, 360, ErrorMessage = "角度范围只能在一圈内")]
        [Parameter("转到角度[0,360]", 3, CN = "转到角度")]
        public double TargetAngle { get; set; }

        [Parameter("当前位置", 100, CN = "当前位置", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double OutPos { get; set; }


        [Parameter("转盘运动结束自动转移数据", 4, CN = "数据转移", DefaultV = false)]
        public bool IsDataTransfer { get; set; }

        [DependOn(nameof(IsDataTransfer), true)]
        [Range(1, 10)]
        [Parameter("工位数量", 6, CN = "工位数量", DefaultV = 1)]
        public int StationNums { get; set; }

        [DependOn(nameof(IsDataTransfer), true)]
        [Parameter("工位对象", 7, CN = "工位")]
        public LStation Stations { get; set; }

        private double currentAngel = 0;

        /// <summary>
        /// 构造函数
        /// </summary>
        public Turntable()
        {
            this.Tips = "转盘";
            this.Icon = "\xe899";
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="errMsg">错误消息</param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            GetVDevice<VAxis>(Device, out var axis);
            if (axis == null)
            {
                errMsg = $"设备:{Device.Name}未找到";
                return false;
            }

            // 1.先获取运动前转盘的位置，并转换到一圈内
            currentAngel = axis.GetCurrentPos();
            if (currentAngel < 0 || currentAngel >= 360)
            {
                currentAngel = AngleToRange_0_360(currentAngel);
                axis.SetCurrentPos(currentAngel);
            }

            // 2.判定接下来的运动是否超过一圈 如果超过一圈，先将当前位置反方向减一圈后再开始运动
            if (MoveMethod == MoveMethod.Nearest)
            {
                if (TargetAngle > currentAngel)
                {
                    if (Math.Abs(TargetAngle - currentAngel) > 180)
                    {
                        axis.SetCurrentPos(currentAngel + 360);
                    }
                }
                else
                {
                    if (Math.Abs(TargetAngle - currentAngel) >= 180)
                    {
                        axis.SetCurrentPos(currentAngel - 360);
                    }
                }
            }
            else if (MoveMethod == MoveMethod.Farthest)
            {
                if (TargetAngle > currentAngel)
                {
                    if (Math.Abs(TargetAngle - currentAngel) <= 180)
                    {
                        axis.SetCurrentPos(currentAngel + 360);
                    }
                }
                else
                {
                    if (Math.Abs(TargetAngle - currentAngel) < 180)
                    {
                        axis.SetCurrentPos(currentAngel - 360);
                    }
                }
            }

            // 3.开始运动 并等待运动完成
            axis.MoveAbs(new VAxisDevice() { Axis = axis, Position = TargetAngle, Speed = Device.Speed });
            //axis.MoveAbs(TargetAngle, Device.Speed);
            axis.CheckMotionDone();
            OutPos = axis.GetCurrentPos();

            // 4.执行数据转移
            if (IsDataTransfer)
            {
                if (Stations == null)
                {
                    errMsg = "工位不能为空!";
                    return false;
                }
                List<LStation> lStations = new List<LStation>();

                //lStations.Add(Stations);
                //string tempKey;
                //for (int i = 1; i < StationNums; i++)
                //{
                //    foreach (var item in MyOwner.Parameters)
                //    {
                //        tempKey = nameof(Stations) + (i + 1);
                //        if (MyOwner.Parameters.ContainsKey(tempKey))
                //        {
                //            lStations.Add((LStation)MyOwner.Parameters[tempKey].Value);
                //            break;
                //        }
                //    }
                //}

                // 工位顺序，需要按照上料、加工1、加工2、...下料的方式。
                lStations = GetParametersByType<LStation>();

                // 工站数量大于等于2，执行数据转移
                if (lStations.Count >= 2)
                {
                    foreach (var item in lStations)
                    {
                        if (item.Tag == null)
                        {
                            item.Tag = MyOwner.TaskModules[item.ID] as IMotionModule;
                        }
                    }

                    for (int i = lStations.Count - 1; i > 0; i--)
                    {
                        DataProcess(lStations[i].Tag as IMotionModule, lStations[i - 1].Tag as IMotionModule, DataTransType.Get);
                    }
                }
            }

            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 如果当前角度转换到[0,360)
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private double AngleToRange_0_360(double angle)
        {
            double target = angle;
            target %= 360;
            if (target < 0)
            {
                target += 360;
            }
            return target;
        }

        /// <summary>
        /// 界面工位数量变化，动态添加对应工站对象
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="newV"></param>
        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
            if (parameter.Name == nameof(Stations))
            {
                SetParameterVal(nameof(Stations), newV);
            }
            else if (parameter.Name == nameof(StationNums) && int.TryParse(newV.ToString(), out var all) && all >= 1)
            {
                BuildExternParameters(1, all, nameof(Stations), "工位", typeof(LStation), p =>
                {
                    p.DependOns = MyOwner.Parameters[nameof(Stations)].DependOns;
                });
            }
        }


    }
}
