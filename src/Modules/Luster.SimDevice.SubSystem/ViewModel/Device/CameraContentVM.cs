#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       CameraContentVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Device
* 文 件 名:       CameraContentVM.cs
* 创建时间:       2022/4/14 17:12:05
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      de2dd8d6-325c-4cab-8fc8-99b61d7bb5e1
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/14 17:12:05
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.Engine;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.Real;
using Luster.SimDevice.SubSystem.Extension;
using Prism.Commands;
using Prism.DryIoc;
using Prism.Events;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Luster.SimDevice.SubSystem.ViewModel.Device
{
    /// <summary>
    /// 相机视图配置
    /// </summary>
    public class CameraContentVM : BaseDeviceVM
    {    
        /// <summary>
        /// 是否为相机设备
        /// </summary>
        public override bool IsCameraDevice => true;

        /// <summary>
        /// 显示添加按钮
        /// </summary>
        public override bool IsShowAdd => true;

        /// <summary>
        /// 自动设备自动搜索
        /// </summary>
        public override bool IsShowAuto => true;      

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_engine"></param>
        /// <param name="dialogService"></param>
        protected CameraContentVM(ISimDeviceEngineUI _engine) : base(_engine, typeof(ICamera))
        {
           
        }

        ///// <summary>
        ///// 根据序列号检查设备是否存在
        ///// </summary>
        ///// <param name="device">设备信息</param>
        ///// <returns>true已存在 false不存在</returns>
        public override bool CheckDeviceIsExist(IDevice device)
        {
            var newCamera = device as CameraBase;
            if (newCamera != null)
            {
                var list = deviceEngine.GetRealDevices(typeof(ICamera))
                    .Select(u => u as CameraBase).
                    Where(u => u.SerialNumber == newCamera.SerialNumber);
                if (list.Count() > 0)
                {
                    deviceEngine.OnLog(LogType.Error, $"相机已存在，序列号{newCamera.SerialNumber}");
                    return true;
                }
            }
            return false;
        }
    }
}