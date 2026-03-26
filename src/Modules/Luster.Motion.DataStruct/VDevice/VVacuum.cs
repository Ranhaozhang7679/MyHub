#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VVacuum
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.DataModels
* 文 件 名:       VVacuum.cs
* 创建时间:       2022/5/28 10:52:21
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      b4dd2653-4cb6-4657-a7b3-9aad8f75a0bb
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/28 10:52:21
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Real;
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
    /// 真空
    /// </summary>
    public class VVacuum : VirtualDeviceBase, IHome, IDeviceShield, IMaintain, IDeviceError,IAutoCheck,IParam
    {
        public override int Sort => 5;

        /// <summary>
        /// 真空检知
        /// </summary>
        [Ignore]
        public VIO InVacuumSensor { get; set; }

        /// <summary>
        /// 真空吹
        /// </summary>
        [Ignore]
        public VIO OutBlow { get; set; }

        /// <summary>
        /// 真空吸
        /// </summary>
        [Ignore]
        public VIO OutSuck { get; set; }


        public override DeviceError[] ErrorCodes => new DeviceError[]
        {
            DeviceError.SuckFail
        };

        public int MinValue { get; set; } = 10;
        public int MaxValue { get; set; } = 500;
        public int TargetValue { get; set; } = 200;

        public VVacuum()
        {
            NeedHome = true;
            CurrentErrorCode = DeviceError.SuckFail;
            MaintainHP = 5000;
        }

        public event Func<string,bool, bool> OnCheckEvent;

        /// <summary>
        /// 气缸引用的对象
        /// </summary>
        /// <returns></returns>
        public override string[] GetRefProps()
        {
            return new string[] {
                nameof(InVacuumSensor),
                nameof(OutBlow),
                nameof(OutSuck)};
        }

        /// <summary>
        /// 吸真空
        /// </summary>
        public void Suck()
        {
            CurrentErrorCode = DeviceError.SuckFail;
            OutSuck.SetDigital(true);
            OutBlow.SetDigital(false);

            CurrentHP++;
            OnHPAlarmTips();
        }

        /// <summary>
        /// 吹真空
        /// </summary>
        public void Blow()
        {
            OutSuck.SetDigital(false);
            OutBlow.SetDigital(true);
        }

        /// <summary>
        /// 关真空
        /// </summary>
        public void Close()
        {
            OutSuck.SetDigital(false);
            OutBlow.SetDigital(false);
        }

        /// <summary>
        /// 吹真空+关真空
        /// </summary>
        public void BlowClose(int delayTime = 100)
        {
            OutSuck.SetDigital(false);
            OutBlow.SetDigital(true);
            Thread.Sleep(delayTime);
            OutBlow.SetDigital(false);
        }

        /// <summary>
        /// 真空检知检查
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="state"></param>
        public void InVacuumSensorCheck(int timeout, bool state)
        {
            if (GetIsShield()) return;

            InVacuumSensor.WaitIO<bool>(state, timeout, () =>
            {
                throw new DeviceTimeoutException(Errors[CurrentErrorCode], this.ID,"",this.Module, this.Name);
            });
        }

        /// <summary>
        /// 回零
        /// </summary>
        public override void Home()
        {
            if (NeedHome)
            {
                Close();
            }
        }

        public bool Check()
        {
            bool IsOK = OnCheckEvent($"即将确认真空:{this.Name}",true);
            if (!IsOK) return false;

            ///真空吸确认
            IsOK = OnCheckEvent("即将真空吸",true);
            if (!IsOK) return false;
            Suck();
            IsOK = OnCheckEvent("是否真空吸",true);
            if (!IsOK) { Close(); return false; }

            ///真空破确认
            IsOK = OnCheckEvent("即将破真空", true);
            if (!IsOK) return false;
            Blow();
            IsOK = OnCheckEvent("是否破真空", true);
            Close();
            if (!IsOK) { return false; }
            return IsOK;
        }
    }
}