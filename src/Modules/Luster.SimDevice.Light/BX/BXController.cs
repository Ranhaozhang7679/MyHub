#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       BXController
* 命名空间:       Luster.SimDevice.Light.BX
* 文 件 名:       BXController.cs
* 创建时间:       2026/06/24
* 作    者:       全栈工程师
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 创建年份:       2026
************************************************************************************/
#endregion

using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.Adapter;
using Luster.SimDevice.Light;
using Luster.SimDevice.Real;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.Light.BX
{
    /// <summary>
    /// 博兴(BX)光源控制器设备类(P2-A,TES-99)。
    /// 仿 <c>CSTController</c> 范式,实现目标端 <see cref="ILightController"/> 通道亮度契约,
    /// 经 <c>IDeviceEngine</c> 动态发现(编译进 Luster.SimDevice.Light.dll,落 Devices\,被 LoadDrivers 自动加载)。
    /// </summary>
    /// <remarks>
    /// <b>双层映射(对齐知识库 P2-A)</b>:
    /// 1. 硬件控制器设备层(本类):实现 <see cref="ILightController"/> 通道亮度 + 厂家硬件方法,走 IDeviceEngine(注册链同 CST)。
    /// 2. 三色灯状态机(复用已有):三色灯/蜂鸣器走平台 <c>ILightManager</c> + <c>LightFlashing</c> 算子节点,已就绪无需新代码。
    ///
    /// <b>⚠️ 真机 SDK 待人类现场接入</b>:vendor 托管程序集 YanMai_DLL 未纳入仓库,
    /// <see cref="BXControllerAPI"/> 原生调用为桩(抛 NotImplementedException,不编造 P/Invoke)。
    /// 软件层验收(编译/发现/ILightController 契约)已满足;真机出数随 TES-57 carve-out,不阻塞虚拟验收。
    ///
    /// <b>R1 非侵入</b>:本类为 Luster.SimDevice.Light 叶子插件新增品牌设备,平台主干零改动;
    /// 消费侧(VCamera.SetLightIntensity / 设备 UI 按 typeof(ILightController) 过滤)自动纳入,无需改动。
    /// </remarks>
    public class BXController : LightControllertBase, ILightController
    {
        /// <summary>厂家品牌(博兴 BX)</summary>
        public override string Brand => "BX";

        /// <summary>光源通道(ILightController 契约)</summary>
        public int Channel { get; set; }

        /// <summary>光源连接超时(秒),对齐 CSTController.TimeOut</summary>
        private const int TimeOut = 5000;

        /// <summary>默认 TCP 端口(源端 LightControllerBX.cs:45 ConnectPort=10000)</summary>
        private const int DefaultPort = 10000;

        private readonly BXControllerAPI _bxAPI;
        private bool _isConnected;

        public BXController()
        {
            _bxAPI = new BXControllerAPI();
            _isConnected = false;
        }

        /// <summary>
        /// 设置通道亮度级数(ILightController 契约)。
        /// 亮度级数 → 博兴通道脉宽(OutWidth,0-300)映射,调 <see cref="BXControllerAPI.SetChannelPulseWidth"/>。
        /// </summary>
        /// <param name="channelIndex">通道号</param>
        /// <param name="intensity">光源亮度级数</param>
        public void SetChannelAndVal(int channelIndex, int intensity)
        {
            if (!_isConnected)
            {
                return;
            }
            // 目标端亮度级数 → 源端脉宽(OutWidth 0-300)。映射比例待真机接入时按厂家量程校准,
            // 此处保留线性映射占位(intensity 0-255 量级 → 脉宽 0-300)。
            int pulseWidth = MapIntensityToPulseWidth(intensity);
            SafeNativeMethod(() =>
            {
                return _bxAPI.SetChannelPulseWidth(channelIndex, pulseWidth) == BXControllerAPI.SUCCESS;
            }, $"通道{channelIndex}设置光源亮度级数失败,级数:{intensity}(脉宽:{pulseWidth})");
        }

        /// <summary>
        /// 获取当前通道亮度级数(ILightController 契约)。
        /// </summary>
        public void GetChannelIntensity(int channelIndex, ref int intensity)
        {
            if (!_isConnected)
            {
                return;
            }
            int pulseWidth = 0;
            SafeNativeMethod(() =>
            {
                return _bxAPI.GetChannelPulseWidth(channelIndex, ref pulseWidth) == BXControllerAPI.SUCCESS;
            }, $"通道{channelIndex}获取光源亮度级数失败");
            intensity = MapPulseWidthToIntensity(pulseWidth);
        }

        /// <summary>亮度级数 → 脉宽(OutWidth 0-300)线性映射占位,待真机接入按厂家量程校准。</summary>
        private static int MapIntensityToPulseWidth(int intensity)
        {
            // 亮度级数假定 0-255(平台典型量级),脉宽 0-300(源端 OutWidth 量程)
            int width = (int)Math.Round(intensity * 300.0 / 255.0);
            return Math.Max(0, Math.Min(300, width));
        }

        /// <summary>脉宽 → 亮度级数反向映射。</summary>
        private static int MapPulseWidthToIntensity(int pulseWidth)
        {
            int intensity = (int)Math.Round(pulseWidth * 255.0 / 300.0);
            return Math.Max(0, Math.Min(255, intensity));
        }

        /// <summary>
        /// 初始化厂家 API 并连接(对齐 CSTController.InitApi,按 Adapter 类型分支)。
        /// </summary>
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
                    if (_bxAPI.ConnectIP(network.Ip, DefaultPort, TimeOut) == BXControllerAPI.SUCCESS)
                    {
                        _isConnected = true;
                        return true;
                    }
                    return false;
                }, $"连接博兴光源失败,IP:{network.Ip}");
            }

            if (Adapter is Com com)
            {
                // 博兴 BX 源端走 TCP(192.168.0.1:10000);串口分支保留对齐 CST 范式,真机接入时按厂家串口协议实现。
                SafeNativeMethod(() =>
                {
                    int index = 0;
                    if (int.TryParse(com.Name.Substring(3), out index))
                    {
                        // 串口连接待 vendor SDK 接入;暂复用 ConnectIP 桩路径以保持结构一致。
                        if (_bxAPI.ConnectIP(com.Name, index, TimeOut) == BXControllerAPI.SUCCESS)
                        {
                            _isConnected = true;
                            return true;
                        }
                    }
                    return false;
                }, $"连接博兴光源失败,{com.Name}");
            }
        }

        public override void Open()
        {
            base.Open();
        }

        /// <summary>断开连接(对齐 CSTController.Close)。</summary>
        public override void Close()
        {
            if (!_isConnected)
            {
                return;
            }
            SafeNativeMethod(() =>
            {
                if (_bxAPI.DisconnectIP() == BXControllerAPI.SUCCESS)
                {
                    _isConnected = false;
                    return true;
                }
                return false;
            }, "断开博兴光源连接失败");
        }
    }
}
