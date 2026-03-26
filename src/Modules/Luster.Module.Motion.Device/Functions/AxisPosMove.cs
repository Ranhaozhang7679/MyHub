#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AxisPosMove
* 机器名称:       L05123-02
* 命名空间:       Luster.Module.Motion.Device.Functions
* 文 件 名:       AxisPosMove.cs
* 创建时间:       2022/12/15 16:15:20
* 作    者:       刘克志
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      d7c563ec-904c-4d99-a78e-f3534eb44487
* 登录用户:       刘克志
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/15 16:15:20
* 修 改 人:		  刘克志
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Device.Functions
{
    public class AxisPosMove : MotionFunction, IPauseFunction, IStopFunction, INote, IGetExtResult
    {
        [NotEmpty]
        [Parameter("轴的点位及对应的参数配置", 1, CN = "点位参数", MultiValues = true)]
        public VAxisPos AxisPos { get; set; }

        [Parameter("运动模式分为JOG和点位模式", 2, CN = "运动模式", DefaultV = AxisMode.Move)]
        public AxisMode AxisMode { get; set; }

        [DependOn("AxisMode", AxisMode.CircleMove)]
        [Parameter("圆弧插补运动的半径", 3, CN = "半径", DefaultV = 0.5)]
        public double Radius { get; set; }

        [DependOn("AxisMode", AxisMode.CircleMove)]
        [Parameter("圆弧插补运动的运动方向分为顺时针和逆时针", 4, CN = "运动方向", DefaultV = AngleDirection.Clockwise)]
        public AngleDirection Direction { get; set; }

        [Parameter("用于Z轴卡控", 10, CN = "安全Z轴", Group = "ZAxisSafeRegion")]
        public VAxisDevice ZAxis { get; set; }

        [Parameter("Z轴安全区,单位mm", 11, Group = "ZAxisSafeRegion", CN = "Z轴安全区")]
        public LRange ZSafeRange { get; set; }

        [Parameter("安全IO，True为条件满足", 12, CN = "安全IO", Group = "ZAxisSafeRegion", EditorType = typeof(VIO))]
        public VDevice SafeIO { get; set; }

        [Parameter("速度固定，就不受比例控制", 12, CN = "速度固定", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool IsFixedSpd { get; set; }

        public override string[] NoteParams => new string[] { nameof(AxisPos), nameof(AxisMode) };

        public AxisPosMove()
        {
            this.Tips = "轴点位运动";
            this.Icon = "\xe6bd";
            this.DynParam = true;

            // 配置默认值
            ZSafeRange = new LRange(-10, 10);
        }

        public override bool DoExcute(out string errMsg)
        {
            // 更新补偿值
            SetDynamicAxisPos(AxisPos);

            // Z轴安全区间
            if (ZAxis != null)
            {
                GetVDevice<VAxis>(ZAxis, out var zAxis);
                if (zAxis != null)
                {
                    var postion = zAxis.GetCurrentPos();
                    if (postion < ZSafeRange.Min || postion > ZSafeRange.Max)
                    {
                        OnAlarm(AlarmType.DeviceError, $"Z轴位置:{postion},不在安全范围:{ZSafeRange.Min} -- {ZSafeRange.Max}之间", DeviceError.SafeFail.ToString());
                    }
                }

            }

            //IO运动前提
            if (SafeIO != null)
            {
                GetVDevice<VIO>(SafeIO, out var safeIO);
                if (!safeIO.GetDigitalIn())
                {
                    OnAlarm(AlarmType.WarningTip, $"IO:{SafeIO.Name}未满足条件，值为False!");
                }
            }




            switch (AxisMode)
            {
                case AxisMode.Move:
                    AxisPos.MovePostion(MyOwner.DeviceEngine, IsFixedSpd);
                    break;
                case AxisMode.LineMove:
                    AxisPos.MoveLineAbs(MyOwner.DeviceEngine);
                    break;
                case AxisMode.CircleMove:
                    AxisPos.MoveCircleAbs(MyOwner.DeviceEngine, Radius, (short)Direction);
                    break;
                case AxisMode.Home:
                    AxisPos.Home(MyOwner.DeviceEngine);
                    break;
                default:
                    errMsg = $"该模式:{AxisMode}不支持";
                    return false;
            }

            // 给结果进行赋值
            SetOutDynamicAxisPos(AxisPos);

            return base.DoExcute(out errMsg);
        }


        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);

            if (parameter.Name == nameof(AxisPos) && newV is VAxisPos vPos)
            {
                vPos.UpdateAxis(MyOwner.DeviceEngine);
                BuildDynamicAxisPos(vPos, 5);
                BuildDynamicAxisPos(vPos, 20, TaskFlow.Common.Enums.ParamType.OUT, false);
            }
        }

        /// <summary>
        /// 点位停止 
        /// </summary>
        public override void Stop()
        {
            if (AxisPos != null)
            {
                bool isCrdStop = (AxisMode == AxisMode.LineMove || AxisMode == AxisMode.CircleMove);
                AxisPos.Stop(MyOwner.DeviceEngine, isCrdStop);
            }
        }

        /// <summary>
        /// 暂停相当于停止
        /// </summary>
        public override void Pause()
        {
            Stop();
        }

        public override bool IsNeedPause => true;

        /// <summary>
        /// 获取参数信息
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Dictionary<string, object> GetExtResult()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            // 多点位
            foreach (var item in AxisPos)
            {
                string spdKey = $"{item.Axis.AxisType}_Spd";
                if (!result.ContainsKey(spdKey))
                {
                    result.Add(spdKey, item.Speed);
                }

                string spdTarget = $"{item.Axis.AxisType}_Spd_Target";
                if (!result.ContainsKey(spdTarget))
                {
                    result.Add(spdTarget, item.Speed * 0.8);
                }

                string disKey = $"{item.Axis.AxisType}_Distance";
                if (!result.ContainsKey(disKey))
                {
                    result.Add(disKey, item.Distance);
                }

                string rotateKey = $"{item.Axis.AxisType}_RotateSpd";
                if (!result.ContainsKey(rotateKey))
                {
                    result.Add(rotateKey, 3000);
                }

                string pathKey = $"{item.Axis.AxisType}_Path";
                if (!result.ContainsKey(pathKey))
                {
                    result.Add(pathKey, 10000 / (item.Axis.PerPluse == 0 ? 1 : item.Axis.PerPluse));
                }

                string accKey = $"{item.Axis.AxisType}_Acc_Target";
                if (!result.ContainsKey(accKey))
                {
                    result.Add(accKey, Math.Sqrt(2 * item.Speed / item.Acc));
                }

                //result.Add($"{item.Axis.AxisType}_Spd_Target", item.Axis.MoveSpeed * 0.8);
                //result.Add($"{item.Axis.AxisType}_Distance", item.Distance);
                //result.Add($"{item.Axis.AxisType}_RotateSpd", 3000);
                //result.Add($"{item.Axis.AxisType}_Path", 10000 / (item.Axis.PerPluse == 0 ? 1 : item.Axis.PerPluse));
                //result.Add($"{item.Axis.AxisType}_Acc_Target", Math.Sqrt(2 * item.Speed / item.Acc));
            }

            return result;
        }
    }
}