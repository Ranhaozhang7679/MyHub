#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LineLaserContentVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Device
* 文 件 名:       LineLaserContentVM.cs
* 创建时间:       2022/4/14 17:56:20
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      d1b6c431-0f00-449b-81fa-9148ca057cea
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/14 17:56:20
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.EngineUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Device
{
    public class LineLaserContentVM : BaseDeviceVM
    {
        public override bool IsShowAdd => true;

        /// <summary>
        /// 自动设备自动搜索
        /// </summary>
        public override bool IsShowAuto => true;

        protected LineLaserContentVM(ISimDeviceEngineUI _engine) : base(_engine, typeof(ILineLaser))
        {

        }

      /// <summary>
      /// 检查硬件设备是否存在
      /// </summary>
      /// <param name="device"></param>
      /// <returns></returns>
        public override bool CheckDeviceIsExist(IDevice device)
        {
            var newLaser = device as LineLaserBase;
            if (newLaser != null)
            {
                IEnumerable<LineLaserBase> list = null;
                if (newLaser.Adapter != null)
                {
                    list = deviceEngine.GetRealDevices(typeof(ILineLaser))
                        .Select(u => u as LineLaserBase).
                        Where(u => (u.Adapter!=null)&&(u.Adapter.GetMethod() == newLaser.Adapter.GetMethod()));
                    if (list.Count() > 0)
                    {
                        deviceEngine.OnLog(LogType.Error, $"激光已存在，端口号{newLaser.Adapter.GetMethod()}");
                        return true;
                    }
                }
                else
                {
                    list = deviceEngine.GetRealDevices(typeof(ILineLaser))
                            .Select(u => u as LineLaserBase).
                            Where(u => (u.SN == newLaser.SN));
                    if (list.Count() > 0)
                    {
                        deviceEngine.OnLog(LogType.Error, $"激光已存在，序列号{newLaser.SN}");
                        return true;
                    }
                }
                
            }
            return false;
        }
    }
}