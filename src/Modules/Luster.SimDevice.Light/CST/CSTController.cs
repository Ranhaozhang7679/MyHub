#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LMILineLaser
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.LMILaser
* 文 件 名:       LMILineLaser.cs
* 创建时间:       2022/4/11 14:34:12
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      cdc2140b-4779-4266-bd3e-ede4b987a795
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/11 14:34:12
* 修 改 人:		  L05123
************************************************************************************/
#endregion


using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.Adapter;
using Luster.SimDevice.Light;
using Luster.SimDevice.Real;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.SimDevice.Light.CST
{
    /// <summary>
    /// LMI 激光设备
    /// </summary>
    public class CSTController : LightControllertBase, ILightController
    {
        public override string Brand => "CST";

       public int Channel { get ; set ; }

        // 光源连接超时时间
        private const int TimeOut = 5000;

        private CSTControllerAPI _cTSLightAPI;

        private bool _isConnected;

        public CSTController()
        {
            _cTSLightAPI = new CSTControllerAPI();
            _isConnected = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channelIndex"></param>
        /// <param name="intensity"></param>
        public void SetChannelAndVal(int channelIndex, int intensity) 
        {
            if (!_isConnected)
            {
                return;
            }
            SafeNativeMethod(() =>
            {
                return _cTSLightAPI.SetDigitalValue(channelIndex, intensity) == CSTControllerAPI.SUCCESS;
            }, $"通道{channelIndex}设置光源亮度级数失败，级数：{intensity}");
        }

        /// <summary>
        /// 获取当前通道的光源亮度等级
        /// </summary>
        /// <param name="channelIndex"></param>
        /// <param name="intensity"></param>
        public void GetChannelIntensity(int channelIndex, ref int intensity)
        {
            if (!_isConnected)
            {
                return;
            }
            var level = 0;
            SafeNativeMethod(() =>
            {
                return _cTSLightAPI.GetDigitalValue(channelIndex, ref level) == CSTControllerAPI.SUCCESS;
            }, $"通道{channelIndex}获取光源亮度级数失败");
            intensity = level;
        }


        public override void InitApi()
        {
            if (_isConnected) 
            {
                return;
            }

            if (Adapter is Network network)
            {
                SafeNativeMethod(() =>
                {
                    if (_cTSLightAPI.ConnectIP(network.Ip, TimeOut) == CSTControllerAPI.SUCCESS) 
                    {
                        _isConnected = true;
                        return true;
                    }
                    return false; ;
                }, $"连接光源失败，IP：{network.Ip}");
            }

            if (Adapter is Com com) 
            {
                SafeNativeMethod(() =>
                {
                    int index = 0;
                    if (int.TryParse(com.Name.Substring(3), out index)) 
                    {
                        if (_cTSLightAPI.CreateSerialPort(index) == CSTControllerAPI.SUCCESS) 
                        {
                            _isConnected = true;
                            return true;
                        }
                    }
                    return false;
                }, $"连接光源失败，{com.Name}");              
            }
        }

        public override void Open()
        {
            base.Open();
        }

        public override void Close()
        {
            if (!_isConnected) 
            {
                return;
            }
            if (Adapter is Network network)
            {
                SafeNativeMethod(() =>
                {
                    if (_cTSLightAPI.DestroyIpConnection() == CSTControllerAPI.SUCCESS) 
                    {
                        _isConnected = false;
                        return true;
                    }
                   return false ;
                }, "断开光源连接失败");
            }

            if (Adapter is Com com)
            {
                SafeNativeMethod(() =>
                {
                    if (_cTSLightAPI.ReleaseSerialPort() == CSTControllerAPI.SUCCESS)
                    {
                        _isConnected = false;
                        return true;
                    }
                    return false;
                }, "断开光源连接失败");
            }
        }

    }
}