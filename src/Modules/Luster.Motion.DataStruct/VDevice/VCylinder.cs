#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VCylinder
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.DataModels
* 文 件 名:       VCylinder.cs
* 创建时间:       2022/5/28 10:53:39
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      19f803e8-a49e-4d9b-9a85-b4dbc2a63722
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/28 10:53:39
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Virtual;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.DataStruct.DataModels
{
    /// <summary>
    /// 气缸
    /// </summary>
    public class VCylinder : VirtualDeviceBase, IHome, IDeviceShield, IPosition, IMaintain, IStop, IDeviceError, IAutoCheck, IParam
    {
        public override int Sort => 4;

        /// <summary>
        /// 气缸分类
        /// </summary>
        public CylinderCategory Category { get; set; }

        /// <summary>
        /// 缩回位磁环感应
        /// </summary>
        [Ignore]
        public VIO InRetractSensor { get; set; }

        [Ignore]
        /// <summary>
        /// 伸出位磁环感应
        /// </summary>

        public VIO InExtendSensor { get; set; }

        /// <summary>
        /// 双控气缸缩回动作输出
        /// </summary>
        [Ignore]
        public VIO OutRetract { get; set; }

        [Ignore]
        /// <summary>
        /// 伸出动作输出，单控气缸伸出和缩回气缸
        /// </summary>
        public VIO OutExtend { get; set; }

        /// <summary>
        /// 行程
        /// </summary>
        public double Distance { get; set; } = 100;

        /// <summary>
        /// 最高速度
        /// </summary>
        public double MaxSpeed { get; set; } = 500;

        /// <summary>
        /// 速度
        /// </summary>
        public double Speed { get; set; } = 400;

        public override string Unit => "Times";

        /// <summary>
        /// 错误代码
        /// </summary>
        public override DeviceError[] ErrorCodes => new DeviceError[] { DeviceError.ExtendFail, DeviceError.RetractFail };

        /// <summary>
        /// 最小值
        /// </summary>
        public int MinValue { get; set; } = 50;

        /// <summary>
        /// 最大值
        /// </summary>
        public int MaxValue { get; set; } = 1000;

        /// <summary>
        /// 目标值
        /// </summary>
        public int TargetValue { get; set; } = 300;


        /// <summary>
        /// 气缸伸出名称
        /// </summary>
        public string ExtendName { get ; set ; }

        /// <summary>
        /// 气缸缩回名称
        /// </summary>
        public string RetractName { get ; set ; }

        /// <summary>
        /// 气缸伸出传感器名称
        /// </summary>
        public string InExtendName { get; set; }

        /// <summary>
        /// 气缸缩回传感器名称
        /// </summary>
        public string InRetractName { get ; set ; }


        /// <summary>
        /// 构造函数
        /// </summary>
        public VCylinder()
        {
            NeedHome = true;
            Category = CylinderCategory.Double;
        }

        public event Func<string,bool, bool> OnCheckEvent;

        /// <summary>
        /// 气缸引用的对象
        /// </summary>
        /// <returns></returns>
        public override string[] GetRefProps()
        {
            if (Category == CylinderCategory.Double)
            {
                return new string[] {
                    nameof(InRetractSensor),
                    nameof(InExtendSensor),
                    nameof(OutRetract),
                    nameof(OutExtend)
                };
            }
            else
            {
                return new string[] {
                    nameof(InRetractSensor),
                    nameof(InExtendSensor),
                    nameof(OutExtend)
                };
            }
        }

        /// <summary>
        /// 伸出
        /// </summary>
        public void Extend()
        {
            OutExtend.SetDigital(true);
            CurrentErrorCode = DeviceError.ExtendFail;

            if (Category == CylinderCategory.Double)
            {
                OutRetract.SetDigital(false);
            }

            OnLog(Common.DataStruct.Enums.LogType.Debug, $"设备:{Name} 伸出");

            CurrentHP++;

            OnHPAlarmTips();
        }

        /// <summary>
        /// 伸出磁环感应检查
        /// </summary>
        /// <param name="timeOut"></param>
        public void InExtendSensorCheck(int timeOut)
        {
            ProcessAction(() =>
            {
                if (!GetIsShield())
                {
                    WaitDiagital(timeOut, new VIO[] { InExtendSensor, InRetractSensor }, new bool[] { true, false }, () =>
                    {
                        throw new DeviceTimeoutException(Errors[CurrentErrorCode], this.ID, $"Action:{Name} timeout>{timeOut}!", this.Module,this.Name);
                        
                    });
                }
                else
                {
                    System.Threading.Thread.Sleep(shieldSleep);
                }
            }, () =>
            {
                throw new DeviceTimeoutException(Errors[CurrentErrorCode], this.ID, $"Action:{Name} timeout>{timeOut}!", this.Module, this.Name);
                Thread.Sleep(TargetValue);
            }
            ) ;
        }

        /// <summary>
        /// 缩回
        /// </summary>
        public void Retract()
        {
            if (Category == CylinderCategory.Double)
            {
                OutRetract.SetDigital(true);
            }

            OutExtend.SetDigital(false);
            CurrentErrorCode = DeviceError.ExtendFail;

            OnLog(Common.DataStruct.Enums.LogType.Debug, $"设备:{Name} 缩回");

            CurrentHP++;

            OnHPAlarmTips();
        }

        /// <summary>
        /// 缩回磁环感应检查
        /// </summary>
        /// <param name="timeOut"></param>
        public void InRetractSensorCheck(int timeOut)
        {
            ///增加缩回检查的虚拟方法
            ProcessAction(() =>
            {
                if (!GetIsShield())
                {
                    WaitDiagital(timeOut, new VIO[] { InExtendSensor, InRetractSensor }, new bool[] { false, true }, 
                        () => { throw new DeviceTimeoutException(Errors[CurrentErrorCode], this.ID,"",this.Module, this.Name); });
                }
                else
                {
                    System.Threading.Thread.Sleep(shieldSleep);
                }
            },
            ()=> { Thread.Sleep(TargetValue); });
        }

        /// <summary>
        /// 回零
        /// </summary>
        public override void Home()
        {
            if (NeedHome)
            {
                Retract();

                // 回零必须检查气缸感应器有信号才能认为成功
                InRetractSensorCheck(2000);
            }
        }

        /// <summary>
        /// 10 代表伸出
        /// 0 代表缩回
        /// </summary>
        /// <returns></returns>
        public override double GetCurrentPos()
        {
            var isExtend = InExtendSensor.GetDigitalIn() && !InRetractSensor.GetDigitalIn();
            if (isExtend)
            {
                return 10;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 保养提示
        /// </summary>
        /// <returns></returns>
        public override string GetMaintainTips()
        {
            return $"气缸:{Name} 伸缩次数>{MaintainHP},请进行保养";
        }

        public void AddPosionPosition(PosionSafeModel position)
        {
            //throw new NotImplementedException();
        }

        public bool Check()
        {
            bool isOk = OnCheckEvent($"气缸:{this.Name}是否在安全位置?", true);
            if (!isOk) return false;
            ///伸出动作
            isOk = OnCheckEvent("即将进行伸出操作？" , true);
            if (!isOk) return false;
            Extend();
            
            isOk = OnCheckEvent("是否实际伸出?", true);
            if (!isOk) return false;

            ///缩回动作
            isOk = OnCheckEvent("即将进行缩回操作？", true);
            Retract();
            isOk = OnCheckEvent("是否实际缩回?",true);
            if (!isOk) return false;

            return isOk;
        }
    }
}