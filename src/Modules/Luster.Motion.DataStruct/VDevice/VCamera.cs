#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VirtualCamera
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Virtual
* 文 件 名:       VirtualCamera.cs
* 创建时间:       2022/4/6 9:29:21
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      b8530b8a-cdfb-4f29-a0d0-dcf326cd0ee3
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/6 9:29:21
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.Virtual;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.DataStruct.DataModels
{
    /// <summary>
    /// 相机对象
    /// </summary>
    public class VCamera : VirtualDeviceBase
    {
        /// <summary>
        /// 排序
        /// </summary>
        public override int Sort => 13;

        /// <summary>
        /// 通道
        /// </summary>
        private const string LightChannel = "LightChannel";

        /// <summary>
        /// 是否打开
        /// </summary>
        public bool IsOpened => _realCamera == null ? false : _realCamera.IsOpen;

        /// <summary>
        /// 相机取流事件
        /// </summary>
        public event Action<LImage> FrameImageEvent;

        /// <summary>
        /// 光源对象
        /// </summary>
        [Ignore]
        public IDevice LightController { get; set; }

        /// <summary>
        /// 真实相机序列号
        /// </summary>
        public string CameraSerialNumber { get; set; }

        /// <summary>
        /// 真实相机名称
        /// </summary>
        public string CameraName { get; set; }

        /// <summary>
        /// 光源控制器名称
        /// </summary>
        public string LightControllerName { get; set; }

        /// <summary>
        /// 离线图像
        /// </summary>
        public LPath ImagePath { get; set; }

        /// <summary>
        /// 光源通道数
        /// </summary>
        public int ChannelCount { get; set; }

        /// <summary>
        /// 光源通道熄灭延时事件
        /// </summary>
        public int LightOffDelayTime { get; set; }

        /// <summary>
        /// 记录通道与通道光强
        /// </summary>
        [Ignore]
        public Dictionary<int, int> ChannelInfos { get; set; }


        /// <summary>
        /// 临时变量相机
        /// </summary>
        private ICamera _realCamera;

        public VCamera()
        {

        }

        /// <summary>
        /// 设置设备
        /// </summary>
        /// <param name="hardDevice"></param>
        public override bool SetDevice(IDevice hardDevice, out string errMsg)
        {
            errMsg = "";
            base.SetDevice(hardDevice, out var msg);

            _realCamera = hardDevice as ICamera;
            if (_realCamera != null)
            {
                _realCamera.FrameImageEvent += Camera_FrameImageEvent;
            }
            ProcessAction(() =>
            {
                //if (_realCamera.CameraOpen(out errMsg))
                //{
                //    // 相机打开后设置参数
                //    SetCameraParas();
                //}
            }, () =>
            {

            });

            return true;
        }



        /// <summary>
        /// 触发相机取流
        /// </summary>
        /// <param name="image"></param>
        private void Camera_FrameImageEvent(LImage image)
        {
            FrameImageEvent?.Invoke(image);
        }

        /// <summary>
        /// 打开相机
        /// </summary>
        public void OpenCamera()
        {
            ProcessAction(() =>
            {
                var errMsg = string.Empty;
                _realCamera.CameraOpen(out errMsg);

            }, () =>
            {

            });
        }


        /// <summary>
        /// 拍照
        /// </summary>
        /// <returns></returns>
        public void OnCamera(bool isTriggerLight=true)
        {
            ProcessAction(() =>
            {
                // 1打开相机取图
                _realCamera.CameraStartGrab();

                // 2.打开光源
                if(isTriggerLight)
                {
                    LightOn();
                }

                // 3.相机取图
                _realCamera.CameraPicSave();
                System.Threading.Thread.Sleep(5000);

                // 4.延时关闭光源
                if (isTriggerLight)
                {
                    System.Threading.Thread.Sleep(LightOffDelayTime);
                    LightOff();
                }


                // 5.关闭取流
                _realCamera.CameraStopGrab();

            }, () =>
            {
                // 1.加载ImagePath 离线图片，根据图片生成LImage对象
                // 2.触发事件FrameImageEvent
                // 创建一个相机

                _realCamera.CameraFromFile(ImagePath.Path);
            });
        }

        /// <summary>
        /// 开启视频流
        /// </summary>
        public void CameraStarGrab()
        {
            ProcessAction(() =>
            {
                LightOn();

                // 1.打开相机取图
                _realCamera.CameraStartGrab();

                // 2.设置相机连续取图
                _realCamera.CameraVedioSave();
            }, () =>
            {
                // 1.加载ImagePath 离线图片，根据图片生成LImage对象
                // 2.开启一个线程定时触发事件FrameImageEvent
            });
        }

        /// <summary>
        /// 关闭相机取流
        /// </summary>
        public void CameraStopGrab()
        {
            ProcessAction(() =>
            {
                // 1.关闭光源
                LightOff();

                // 2.关闭相机取流
                _realCamera.CameraStopGrab();
            }, () =>
            {

            });
        }


        /// <summary>
        /// 关闭相机
        /// </summary>
        public void CloseCamera()
        {
            ProcessAction(() =>
            {
                // 1.关闭光源
                LightOff();
                // 2.关闭相机            
                _realCamera.CloseCamera();

            }, () =>
            {

            });
        }


        /// <summary>
        /// 设置光源强度
        /// </summary>
        /// <param name="channelIndex"></param>
        /// <param name="intensity"></param>
        /// <param name="originalId"></param>
        public void SetLightIntensity(int channelIndex, int intensity, int originalId = 0)
        {
            ProcessAction(() =>
            {
                if (LightController != null)
                {
                    var controller = LightController as ILightController;
                    LightController.InitApi();
                    controller.SetChannelAndVal(channelIndex, intensity);
                    LightController.Close();

                    UpdateLightChannel(channelIndex, intensity, originalId);
                }
            }, () =>
            {

            });

        }

        /// <summary>
        /// 打开光源
        /// </summary>
        private void LightOn()
        {
            var controller = LightController as ILightController;
            foreach (var item in ChannelInfos)
            {
                LightController.InitApi();
                controller.SetChannelAndVal(item.Key, item.Value);
                LightController.Close();
            }
        }

        /// <summary>
        /// 关闭光源
        /// </summary>
        private void LightOff()
        {
            var controller = LightController as ILightController;
            foreach (var item in ChannelInfos)
            {
                LightController.InitApi();
                controller.SetChannelAndVal(item.Key, 0);
                LightController.Close();
            }
        }

        /// <summary>
        /// 跟新光源通道配置
        /// </summary>
        /// <param name="channelIndex">光源通道</param>
        /// <param name="intensity">强度等级</param>
        /// <param name="originalId">原光源通道</param>
        private void UpdateLightChannel(int channelIndex, int intensity, int originalId)
        {
            if (channelIndex == originalId)
            {
                if (ChannelInfos.Keys.Contains(channelIndex))
                {
                    ChannelInfos[channelIndex] = intensity;
                }
                else
                {
                    ChannelInfos.Add(channelIndex, intensity);
                }
            }
            else
            {
                if (ChannelInfos.Keys.Contains(originalId))
                {
                    ChannelInfos.Remove(originalId);
                }
                if (ChannelInfos.ContainsKey(channelIndex))
                {
                    ChannelInfos[channelIndex] = intensity;
                }
                else
                {
                    ChannelInfos.Add(channelIndex, intensity);
                }

            }
        }

        /// <summary>
        /// 设置相机参数
        /// </summary>
        public void SetCameraParas()
        {
            ProcessAction(() =>
            {
                RealDevice.UpdateParam<float>("ExposureTime", _realCamera.ExposureTime);
                RealDevice.UpdateParam<float>("FrameRate", _realCamera.FrameRate);
                RealDevice.UpdateParam<float>("Gain", _realCamera.Gain);
            }, () =>
            {

            });
        }

        public ICamera GetRealCamera()
        {
            return _realCamera;
        }

        /// <summary>
        /// 设置光源
        /// </summary>
        /// <param name="lightControllerId"></param>
        private void SetLight(Guid lightControllerId)
        {
            var device = Engine.GetDeviceByID(lightControllerId);
            if (device != null)
            {
                LightController = device;
            }
        }

        #region 参数导入导出
        public override XElement ExportXml()
        {
            var xml = base.ExportXml();
            xml.Add(new XElement("LightController", LightController.ID));

            // 生成光源通道信息
            foreach (var item in ChannelInfos)
            {
                var xElemet = new XElement(LightChannel);
                xElemet.SetAttributeValue("ChannelId", item.Key);
                xElemet.SetAttributeValue("Intensity", item.Value);
                xml.Add(xElemet);
            }

            return xml;
        }

        public override void ParserXml(XElement xElement)
        {
            base.ParserXml(xElement);

            // 1.解析光源
            var xLight = xElement.Element("LightController");
            ;
            if (xLight != null)
            {
                SetLight(Guid.Parse(xLight.Value));
            }

            // 2.解析光源通道配置
            ChannelInfos = new Dictionary<int, int>();

            int channelIndex = 0, intensity = 0;
            foreach (var item in xElement.Elements(LightChannel))
            {
                item.GetAttribute("ChannelId", (ch) =>
                {
                    channelIndex = int.Parse(ch);
                });

                item.GetAttribute("Intensity", (ty) =>
                {
                    intensity = int.Parse(ty);
                });

                ChannelInfos.Add(channelIndex, intensity);
            }

            // 3.解析相机
            xElement.GetElement("DeviceID", (id) =>
            {
                var camera = Engine.GetDeviceByID(Guid.Parse(id));

                _realCamera = camera as ICamera;
            });
        }
        #endregion
    }
}