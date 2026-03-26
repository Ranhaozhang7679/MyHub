#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MultiAxis
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Device.Functions
* 文 件 名:       MultiAxis.cs
* 创建时间:       2022/6/14 14:49:47
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      1be807ee-0ba2-4f40-9d57-082ceb9294f7
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/14 14:49:47
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Device.Functions
{

    public class MultiAxis : MotionFunction, IGetExtResult, IPauseFunction, IStopFunction
    {
        [NotEmpty]
        [Parameter("多轴仿真设备", 1, CN = "多轴")]
        public VAxisMDevice AxisM { get; set; }

        [Parameter("运动模式分为JOG和点位模式", 2, CN = "运动模式", DefaultV = AxisMode.Move)]
        public AxisMode AxisMode { get; set; }

        [DependOn("AxisMode", AxisMode.JOG)]
        [Parameter("在皮带模式下，轴的动作", 4, CN = "轴动作", DefaultV = JOGMethod.Start)]
        public JOGMethod JOGMethod { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public MultiAxis()
        {
            this.DynParam = true;
            this.Icon = "\xe67c";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            // 1.获取硬件
            GetVDevice<VAxisM>(AxisM, out var vAxisM);

            // 2.配置动态补偿
            SetDynamicAxisM(AxisM);

            // 3.运动
            if (AxisMode == AxisMode.Move)
            {
                vAxisM.Move(AxisM);
            }
            else if (AxisMode == AxisMode.LineMove)
            {
                vAxisM.MoveLineAbs(AxisM);
            }
            else if (AxisMode == AxisMode.Home)
            {
                vAxisM.Home();
            }
            else if (AxisMode == AxisMode.CircleMove)
            {
                vAxisM.MoveCircleAbs(AxisM, 0, 0);
            }
            else
            {
                if (JOGMethod == JOGMethod.Start)
                {
                    vAxisM.Jog(AxisM);
                }
                else
                {
                    vAxisM.Stop();
                }
            }

            // 更新实时位置信息
            SetOutDynamicAxisM(AxisM);

            MyOwner.OnLog(Common.DataStruct.Enums.LogType.Debug, $"模块:{MyOwner.Alias} 运动位置:{GetPositionStr()}");

            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 获取当前位置的字符串
        /// </summary>
        /// <returns></returns>
        private string GetPositionStr()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in MyOwner.Parameters)
            {
                if (item.Value.Property == null && item.Value.ParamType == TaskFlow.Common.Enums.ParamType.OUT)
                {
                    sb.Append($"{item.Value.CN}_{item.Value.Value}_");
                }
            }

            if (sb.Length > 0)
            {
                sb.Remove(sb.Length - 1, 1);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="newV"></param>
        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
            if (parameter.Name == "AxisM")
            {
                BuildDynamicAxisM(newV as VAxisMDevice, 8);

                // 构建动态位置输出
                BuildDynamicAxisM(newV as VAxisMDevice, 20, TaskFlow.Common.Enums.ParamType.OUT, false);
            }

        }

        /// <summary>
        /// 获取额外信息
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetExtResult()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            GetVDevice<VAxisM>(AxisM, out var vAxisM);

            if (vAxisM != null)
            {
                for (int i = 0; i < vAxisM.Number; i++)
                {
                    if (vAxisM.Axises[i].Axis.AxisType == AxisType.X)
                    {
                        result.Add("X_Spd", vAxisM.Axises[i].Axis.GetSpeed(AxisM.Items[i].Speed));
                        result.Add("X_Acc", vAxisM.Axises[i].Axis.GetSpeed(AxisM.Items[i].Speed) / double.Parse(vAxisM.Axises[i].Axis.Acc.ToString()));
                        result.Add("X_Dec", vAxisM.Axises[i].Axis.GetSpeed(AxisM.Items[i].Speed) / double.Parse(vAxisM.Axises[i].Axis.Dec.ToString()));
                        result.Add("X_SpdTarget", vAxisM.Axises[i].Axis.MoveSpeed * 0.8);
                        result.Add("X_AccTarget", 0.1);
                        if (vAxisM.Axises[i] != null)
                        {
                            result.Add("X_Distance", Math.Abs(vAxisM.Axises[i].Position - vAxisM.Axises[i].PrevMovePos));
                        }
                    }
                    if (vAxisM.Axises[i].Axis.AxisType == AxisType.Y)
                    {
                        result.Add("Y_Spd", vAxisM.Axises[i].Axis.GetSpeed(AxisM.Items[i].Speed));
                        result.Add("Y_Acc", vAxisM.Axises[i].Axis.GetSpeed(AxisM.Items[i].Speed) / double.Parse(vAxisM.Axises[i].Axis.Acc.ToString()));
                        result.Add("Y_Dec", vAxisM.Axises[i].Axis.GetSpeed(AxisM.Items[i].Speed) / double.Parse(vAxisM.Axises[i].Axis.Dec.ToString()));
                        result.Add("Y_SpdTarget", vAxisM.Axises[i].Axis.MoveSpeed * 0.8);
                        result.Add("Y_AccTarget", 0.1);
                        if (vAxisM.Axises[i] != null)
                        {
                            result.Add("Y_Distance", Math.Abs(vAxisM.Axises[i].Position - vAxisM.Axises[i].PrevMovePos));
                        }
                    }
                    if (vAxisM.Axises[i].Axis.AxisType == AxisType.Z)
                    {
                        result.Add("Z_Spd", vAxisM.Axises[i].Axis.GetSpeed(AxisM.Items[i].Speed));
                        result.Add("Z_Acc", vAxisM.Axises[i].Axis.GetSpeed(AxisM.Items[i].Speed) / double.Parse(vAxisM.Axises[i].Axis.Acc.ToString()));
                        result.Add("Z_Dec", vAxisM.Axises[i].Axis.GetSpeed(AxisM.Items[i].Speed) / double.Parse(vAxisM.Axises[i].Axis.Dec.ToString()));
                        result.Add("Z_SpdTarget", vAxisM.Axises[i].Axis.MoveSpeed * 0.8);
                        result.Add("Z_AccTarget", 0.1);
                        if (vAxisM.Axises[i] != null)
                        {
                            result.Add("Z_Distance", Math.Abs(vAxisM.Axises[i].Position - vAxisM.Axises[i].PrevMovePos));
                        }
                    }
                    if (vAxisM.Axises[i].Axis.AxisType == AxisType.U)
                    {
                        result.Add("U_Spd", vAxisM.Axises[i].Axis.GetSpeed(AxisM.Items[i].Speed));
                        result.Add("U_Acc", vAxisM.Axises[i].Axis.GetSpeed(AxisM.Items[i].Speed) / double.Parse(vAxisM.Axises[i].Axis.Acc.ToString()));
                        result.Add("U_Dec", vAxisM.Axises[i].Axis.GetSpeed(AxisM.Items[i].Speed) / double.Parse(vAxisM.Axises[i].Axis.Dec.ToString()));
                        result.Add("U_SpdTarget", vAxisM.Axises[i].Axis.MoveSpeed * 0.8);
                        result.Add("U_AccTarget", 0.1);
                        if (vAxisM.Axises[i] != null)
                        {
                            result.Add("U_Distance", Math.Abs(vAxisM.Axises[i].Position - vAxisM.Axises[i].PrevMovePos));
                        }
                    }

                    if (vAxisM.Axises[i].Axis.AxisType == AxisType.V)
                    {
                        result.Add("R_Spd", vAxisM.Axises[i].Axis.GetSpeed(AxisM.Items[i].Speed));
                        result.Add("R_Acc", vAxisM.Axises[i].Axis.GetSpeed(AxisM.Items[i].Speed) / double.Parse(vAxisM.Axises[i].Axis.Acc.ToString()));
                        result.Add("R_Dec", vAxisM.Axises[i].Axis.GetSpeed(AxisM.Items[i].Speed) / double.Parse(vAxisM.Axises[i].Axis.Dec.ToString()));
                        result.Add("R_SpdTarget", vAxisM.Axises[i].Axis.MoveSpeed * 0.8);
                        result.Add("R_AccTarget", 0.1);
                        if (vAxisM.Axises[i] != null)
                        {
                            result.Add("R_Distance", Math.Abs(vAxisM.Axises[i].Position - vAxisM.Axises[i].PrevMovePos));
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 停止
        /// </summary>
        public override void Stop()
        {
            GetVDevice<VAxisM>(AxisM, out var vAxisM);
            if (vAxisM != null)
            {
                vAxisM.Stop();
            }
        }
    }
}