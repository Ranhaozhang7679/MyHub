#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LightVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Device
* 文 件 名:       LightVM.cs
* 创建时间:       2022/4/15 9:30:45
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      c43cadd1-aa22-49e8-8dc2-7e53e7edab1e
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/15 9:30:45
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.SimDevice.EngineUI;
using Luster.Motion.DataStruct.Real;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.Common.DataStruct.DataModels;
using Prism.Commands;

namespace Luster.SimDevice.SubSystem.ViewModel.Device
{
    public class LightControllerContentVM : BaseDeviceVM
    {

        public override bool IsLightController => true;
        /// <summary>
        /// 显示添加按钮
        /// </summary>
        public override bool IsShowAdd => true;

        protected LightControllerContentVM(ISimDeviceEngineUI _engine) : base(_engine, typeof(ILightController))
        {

        }


        /// <summary>
        /// 根据名字检查设备是否存在
        /// </summary>
        /// <param name="device">设备信息</param>
        /// <returns>true已存在 false不存在</returns>
        public override bool CheckDeviceIsExist(IDevice device)
        {           
            if (device != null)
            {
                var list = deviceEngine.GetRealDevices(typeof(ILightController)).
                    Where(u => u.Name == device.Name);

                if (list.Count() > 0)
                {
                    deviceEngine.OnLog(Common.DataStruct.Enums.LogType.Error, $"光源名称{device.Name}已存在");
                    return true;
                }
            }
            return false;
        }
    }
}