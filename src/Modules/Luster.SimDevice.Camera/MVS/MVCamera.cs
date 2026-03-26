using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.Adapter;
using Luster.SimDevice.Real;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using MvCamCtrl.NET;

namespace Luster.SimDevice.Camera.MVS
{
    /// <summary>
    /// 海康相机
    /// </summary>
    public class MVCamera : CameraBase, ICamera
    {
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        public override string Brand => "海康";
        

        // 实例化相机
        private MyCamera _myCamera = new MyCamera();

        // 相机列表
        private MyCamera.MV_CC_DEVICE_INFO_LIST m_stDeviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();

        // 像素信息
        private MyCamera.MV_FRAME_OUT_INFO_EX m_stFrameInfo = new MyCamera.MV_FRAME_OUT_INFO_EX();
        // 相机参数
        public MyCamera.MVCC_FLOATVALUE stParam = new MyCamera.MVCC_FLOATVALUE();
        // 相机设备
        private MyCamera.MV_CC_DEVICE_INFO device;

        private uint int1 = (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON;

        // 回调函数
        private MyCamera.cbOutputExdelegate ImageCallback;

        // 相机是否开启取流
        private bool _isGrabbing = false;

        /// <summary>
        /// 是否已经打开相机
        /// </summary>
        public bool IsOpen { get; set; }

        /// <summary>
        /// 相机取流事件
        /// </summary>
        public event Action<LImage> FrameImageEvent;

        public MVCamera()
        {

        }

        #region 通用设备接口
        /// <summary>
        /// 初始化API
        /// </summary>
        public override void InitApi()
        {
            SafeNativeMethod(() =>
            {
                var nRet = MyCamera.MV_CC_GetSDKVersion_NET();
                return MyCamera.MV_CC_GetSDKVersion_NET() > 0;
            }, "相机驱动初始化错误！");
        }

        /// <summary>
        /// 设备打开
        /// </summary>
        public override void Open()
        {
            var errMessage = string.Empty;
            CameraOpen(out errMessage);
        }

        /// <summary>
        /// 设备关闭
        /// </summary>
        public override void Close()
        {
            CloseCamera();
        }
        #endregion

        /// <summary>
        /// 命令设置
        /// </summary>
        /// <param name="strKey">命令类型Key</param>
        /// <returns></returns>
        public void CameraCommandValSet(string strKey)
        {
            SafeNativeMethod(() =>
            {
                int nRet = _myCamera.MV_CC_SetCommandValue_NET(strKey);
                return nRet == MyCamera.MV_OK;
            }, $"相机{SerialNumber} 命令设置失败， 命令参数：{strKey}！");
        }

        /// <summary>
        /// 枚举参数设置
        /// </summary>
        /// <param name="strKey">枚举类型Key</param>
        /// <param name="nValue">参数值</param>
        /// <returns></returns>
        public void CameraEnumParaSet(string strKey, uint nValue)
        {
            SafeNativeMethod(() =>
            {
                int nRet = _myCamera.MV_CC_SetEnumValue_NET(strKey, nValue);
                return nRet == MyCamera.MV_OK;
            }, $"相机{SerialNumber} 枚举参数设置， 参数类型：{strKey}，参数值{nValue}！");
        }


        /// <summary>
        /// 修改相机IP设置
        /// </summary>
        /// <param name="cameraIp">IP</param>
        /// <param name="cameraSubnet">子网掩码</param>
        /// <param name="cameraGateway">网关</param>
        /// <returns></returns>
        public void CameraIpSet(uint cameraIp, uint cameraSubnet, uint cameraGateway)
        {
            SafeNativeMethod(() =>
            {
                int nRet = _myCamera.MV_GIGE_ForceIpEx_NET(cameraIp, cameraSubnet, cameraGateway);
                return nRet == MyCamera.MV_OK;
            }, $"相机{SerialNumber}修改IP地址失败！");
        }

        /// <summary>
        /// 读取相机列表
        /// </summary>
        /// <param name="cameraList">相机名称列表</param>
        /// <returns></returns>
        public void CameraListRead(out List<IDevice> cameraList)
        {
            cameraList = new List<IDevice>();

            // ch:创建设备列表 | en:Create Device List
            GC.Collect();
            cameraList.Clear();
            var reslut = false;
            SafeNativeMethod(() =>
            {
                m_stDeviceList.nDeviceNum = 0;
                int nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref m_stDeviceList);
                reslut = nRet == MyCamera.MV_OK;
                return reslut;
            }, $"获取相机列表信息失败！");

            int usbDeviceCount = 0;
            // ch:在窗体列表中显示设备名 | en:Display device name in the form list
            for (int i = 0; i < m_stDeviceList.nDeviceNum; i++)
            {
                MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_stDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                    cameraList.Add(new MVCamera()
                    {
                        Name = gigeInfo.chUserDefinedName,
                        Adapter = new Network()
                        {
                            Ip = IntToIp(gigeInfo.nCurrentIp)
                        },
                        SerialNumber = gigeInfo.chSerialNumber
                    });
                }
                else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                {
                    usbDeviceCount++;
                    MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                    if (usbInfo.chUserDefinedName != "")
                    {
                        cameraList.Add(new MVCamera()
                        {
                            Name = usbInfo.chUserDefinedName,
                            Adapter = new Usb()
                            {
                                Name = $"USB{usbDeviceCount}"
                            },
                            SerialNumber = usbInfo.chSerialNumber
                        });
                    }
                    else
                    {
                        cameraList.Add(new MVCamera()
                        {
                            Name = usbInfo.chManufacturerName,
                            Adapter = new Usb()
                            {
                                Name = $"USB{usbDeviceCount}"
                            },
                            SerialNumber = usbInfo.chSerialNumber
                        });
                    }
                }
            }
        }

        /// <summary>
        /// 设置相机名称
        /// </summary>
        /// <param name="cameraName">名称</param>
        /// <returns></returns>
        public void CameraNameSet(string cameraName)
        {
            SafeNativeMethod(() =>
            {
                int nRet = _myCamera.MV_CC_SetDeviceUserID_NET(cameraName);
                return nRet == MyCamera.MV_OK;
            }, $"相机{SerialNumber}设置名称失败！");
        }


        /// <summary>
        /// 更新相机参数
        /// </summary>
        /// <typeparam name="T">泛型对象</typeparam>
        /// <param name="paramName">参数名称</param>
        /// <param name="val">参数值</param>
        public override void UpdateParam<T>(string paramName, T val)
        {
            switch (paramName)
            {
                case "FrameRate":
                    if (val is float frameRate)
                        CameraFrameRateSet(frameRate);
                    break;
                case "ExposureTime":
                    if (val is float exposureTime)
                        CameraExposureTimeSet(exposureTime);
                    break;

                case "Gain":
                    if (val is float gain)
                        CameraGainSet(gain);
                    break;

                case "Gamma":
                    if (val is float gamma)
                        CameraGammaSet(gamma);
                    break;
            }

            return;
        }

        /// <summary>
        /// 设置相机FrameRate
        /// </summary>
        /// <param name="para">帧率</param>
        /// <returns></returns>
        private void CameraFrameRateSet(float para)
        {
            SafeNativeMethod(() =>
            {
                int nRet = _myCamera.MV_CC_SetFloatValue_NET("AcquisitionFrameRate", para);
                return nRet == MyCamera.MV_OK;
            }, $"相机{SerialNumber}设置FrameRate失败！");

        }

        /// <summary>
        /// 设置相机ExposureTime
        /// </summary>
        /// <param name="para">曝光时间</param>
        /// <returns></returns>
        private void CameraExposureTimeSet(float para)
        {
            SafeNativeMethod(() =>
            {
                _myCamera.MV_CC_SetEnumValue_NET("ExposureAuto", 0);
                int nRet = _myCamera.MV_CC_SetFloatValue_NET("ExposureTime", para);
                return nRet == MyCamera.MV_OK;
            }, $"相机{SerialNumber}设置ExposureTime失败！");
        }

        /// <summary>
        /// 设置相机Gain
        /// </summary>
        /// <param name="para">增益大小</param>
        /// <returns></returns>
        private void CameraGainSet(float para)
        {
            SafeNativeMethod(() =>
            {
                _myCamera.MV_CC_SetEnumValue_NET("GainAuto", 0);
                int nRet = _myCamera.MV_CC_SetFloatValue_NET("Gain", para);
                return nRet == MyCamera.MV_OK;

            }, $"相机{SerialNumber}设置Gain失败！");

        }

        /// <summary>
        /// 设置相机Gamma
        /// </summary>
        /// <param name="para">Gamma大小</param>
        /// <returns></returns>
        private void CameraGammaSet(float para)
        {
            SafeNativeMethod(() =>
            {
                _myCamera.MV_CC_SetEnumValue_NET("GammaAuto", 0);
                int nRet = _myCamera.MV_CC_SetFloatValue_NET("Gamma", para);
                return nRet == MyCamera.MV_OK;
            }, $"相机{SerialNumber}设置Gamma失败！");

        }

        /// <summary>
        /// 打开相机
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool CameraOpen(out string message)
        {
            message = string.Empty;
            IsOpen = false;
            if (IsOpen)
            {
                return true;
            }
            var openResult = false;
            var errMessage = string.Empty;
            SafeNativeMethod(() =>
            {
                int cameraindex = 0;
                List<IDevice> camlist;
                CameraListRead(out camlist);
                if (camlist.Count == 0)
                {
                    errMessage = "未连接相机!";
                    return openResult;
                }

                string chSerialNumber = SerialNumber;
                if (string.IsNullOrEmpty(chSerialNumber))
                {
                    errMessage = "序列号为空!";
                    return openResult;
                }

                for (cameraindex = 0; cameraindex < camlist.Count; cameraindex++)
                {
                    if ((camlist[cameraindex] as MVCamera).SerialNumber == chSerialNumber)
                    {
                        // ch:获取选择的设备信息 | en:Get selected device information
                        device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_stDeviceList.pDeviceInfo[cameraindex], typeof(MyCamera.MV_CC_DEVICE_INFO));
                        break;
                    }
                }

            if (!IsOpen)
            {
                // ch:打开设备 | en:Open device
                if (null == _myCamera)
                    {
                        _myCamera = new MyCamera();
                        if (null == _myCamera)
                        {
                            return openResult;
                        }
                    }

                    int nRet = _myCamera.MV_CC_CreateDevice_NET(ref device);
                    if (MyCamera.MV_OK != nRet)
                    {
                        errMessage = "相机创建失败!";
                        return openResult;
                    }
                    nRet = _myCamera.MV_CC_OpenDevice_NET();

                    if (MyCamera.MV_OK != nRet)
                    {
                        errMessage = "相机打开失败!";
                        _myCamera.MV_CC_DestroyDevice_NET();
                        return openResult;
                    }
                    // ch:探测网络最佳包大小(只对GigE相机有效) | en:Detection network optimal package size(It only works for the GigE camera)
                    if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                    {
                        int nPacketSize = _myCamera.MV_CC_GetOptimalPacketSize_NET();
                        if (nPacketSize > 0)
                        {
                            nRet = _myCamera.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)nPacketSize);
                            if (nRet != MyCamera.MV_OK)
                            {
                                errMessage = $"相机{SerialNumber}设置网络最佳包大小失败！"; ;
                            }
                        }
                        else
                        {
                            errMessage = $"相机{SerialNumber}获取网络最佳包大小失败！";
                        }
                    }

                    // ch:设置采集连续模式 | en:Set Continues Aquisition Mode
                    _myCamera.MV_CC_SetEnumValue_NET("AcquisitionMode", (uint)MyCamera.MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS);
                    _myCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);
                    IsOpen = true;

                }
                openResult = true;
                return openResult;

            }, $"相机{SerialNumber}打开失败");

            message = errMessage;
            if (openResult)
            {
                RegistCallback();
            }
            return openResult;
        }

        /// <summary>
        /// 开启相机取流
        /// </summary>
        public void CameraStartGrab()
        {
            SafeNativeMethod(() =>
            {
                if (!_isGrabbing)
                {
                    m_stFrameInfo.nFrameLen = 0;//取流之前先清除帧长度
                    m_stFrameInfo.enPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_Undefined;
                    var nRet = _myCamera.MV_CC_StartGrabbing_NET();
                    _isGrabbing = true;
                    return MyCamera.MV_OK == nRet;
                }
                return true; ;
            }, $"相机{SerialNumber}开启取流失败！");
        }

        /// <summary>
        /// 设置相机连续取流
        /// </summary>
        /// <returns></returns>
        public void CameraVedioSave()
        {
            SafeNativeMethod(() =>
            {
                var nRet = _myCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);

                return MyCamera.MV_OK == nRet;
            }, $"相机{SerialNumber}设置连续取流失败！");
        }


        /// <summary>
        /// 获取相机参数
        /// </summary>
        /// <param name="frameRate">帧率</param>
        /// <param name="exposureTime">曝光时间</param>
        /// <param name="gain">增益</param>
        /// <param name="gamma"></param>
        /// <returns></returns>
        public void CameraParaRead(out float frameRate, out float exposureTime, out float gain, out float gamma)
        {
            float gammaPara = 0f;
            SafeNativeMethod(() =>
            {
                int nRet = _myCamera.MV_CC_GetFloatValue_NET("ResultingFrameRate", ref stParam);
                if (MyCamera.MV_OK == nRet)
                {
                    FrameRate = stParam.fCurValue;
                }
                return MyCamera.MV_OK == nRet;
            }, $"相机{SerialNumber}读取FrameRate失败");

            SafeNativeMethod(() =>
            {
                int nRet = _myCamera.MV_CC_GetFloatValue_NET("ExposureTime", ref stParam);
                if (MyCamera.MV_OK == nRet)
                {
                    ExposureTime = stParam.fCurValue;
                }
                return MyCamera.MV_OK == nRet;
            }, $"相机{SerialNumber}读取ExposureTime失败");
            SafeNativeMethod(() =>
            {
                int nRet = _myCamera.MV_CC_GetFloatValue_NET("Gain", ref stParam);
                if (MyCamera.MV_OK == nRet)
                {
                    Gain = stParam.fCurValue;
                }
                return MyCamera.MV_OK == nRet;
            }, $"相机{SerialNumber}读取Gain失败");

            SafeNativeMethod(() =>
            {
                int nRet = _myCamera.MV_CC_GetFloatValue_NET("Gamma", ref stParam);
                if (MyCamera.MV_OK == nRet)
                {
                    //Gamma = stParam.fCurValue;
                    gammaPara = stParam.fCurValue;
                }
                return MyCamera.MV_OK == nRet;
            }, $"相机{SerialNumber}读取Gamma失败");

            frameRate = FrameRate;
            exposureTime = ExposureTime;
            gain = Gain;
            gamma = gammaPara;
            //gamma = Gamma;
        }

        /// <summary>
        /// 相机图片保存
        /// </summary>
        /// <returns></returns>
        public void CameraPicSave()
        {
            SafeNativeMethod(() =>
            {
                var nRet = _myCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);
                nRet = _myCamera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
                nRet = _myCamera.MV_CC_SetCommandValue_NET("TriggerSoftware");
                return MyCamera.MV_OK == nRet;
            }, $"相机{SerialNumber}存图失败！");
        }

        /// <summary>
        /// 关闭相机
        /// </summary>
        public void CloseCamera()
        {
            SafeNativeMethod(() =>
            {
                if (!IsOpen)
                {
                    return true;
                }
                int ret;
                // ch:取流标志位清零 | en:Reset flow flag bit
                if (_isGrabbing == true)
                {
                    CameraStopGrab();
                }

                // ch:关闭设备 | en:Close Device
                //ret = _myCamera.MV_CC_CloseDevice_NET();

                //ret = _myCamera.MV_CC_DestroyDevice_NET();
                ret = 0;
                IsOpen = false;
                return ret == MyCamera.MV_OK;
            }, $"相机{SerialNumber}关闭失败");
        }

        /// <summary>
        /// 停止相机采流
        /// </summary>
        /// <returns></returns>
        public void CameraStopGrab()
        {
            SafeNativeMethod(() =>
            {
                // ch:标志位设为false | en:Set flag bit false
                _isGrabbing = false;

                // ch:停止采集 | en:Stop Grabbing
                int nRet = _myCamera.MV_CC_StopGrabbing_NET();
                return nRet == MyCamera.MV_OK;
            }, $"相机{SerialNumber}关闭取流失败！");

        }

        private void ImageCallbackFunc(IntPtr pData, ref MyCamera.MV_FRAME_OUT_INFO_EX pFrameInfo, IntPtr pUser)
        {
            int channel = 3;
            if (IsMonoData(pFrameInfo.enPixelType))
            {
                channel = 1;
            }

            //发布事件
            FrameImageEvent?.Invoke(new LImage()
            {
                Pointer = pData,
                Width = pFrameInfo.nWidth,
                Height = pFrameInfo.nHeight,
                Channel = channel,
            });
        }

        private void RegistCallback()
        {
            SafeNativeMethod(() =>
            {
                int nRet;
                // ch:注册回调函数 | en:Register image callback
                ImageCallback = new MyCamera.cbOutputExdelegate(ImageCallbackFunc);
                nRet = _myCamera.MV_CC_RegisterImageCallBackEx_NET(ImageCallback, IntPtr.Zero);
                //nRet = m_MyCamera.MV_CC_RegisterImageCallBackForRGB_NET(ImageCallback, IntPtr.Zero);
                //nRet = _myCamera.MV_CC_RegisterImageCallBackForBGR_NET(ImageCallback, IntPtr.Zero);
                return MyCamera.MV_OK == nRet;

            }, $"相机{SerialNumber}注册ImageCallback失败！");

        }

        /// <summary>
        /// Int转换成IP地址
        /// </summary>
        /// <param name="ipInt"></param>
        /// <returns></returns>
        private string IntToIp(long ipInt)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append((ipInt >> 24) & 0xFF).Append(".");
            sb.Append((ipInt >> 16) & 0xFF).Append(".");
            sb.Append((ipInt >> 8) & 0xFF).Append(".");
            sb.Append(ipInt & 0xFF);
            return sb.ToString();
        }

        /// <summary>
        /// IP地址转换成Int
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        private uint IpToInt(string ip)
        {
            string[] items = ip.Split('.');
            //这里|可以换成+ 因为转化二进制 后面的位数都是0 所以能用 |
            return uint.Parse(items[0]) << 24
                    | uint.Parse(items[1]) << 16
                    | uint.Parse(items[2]) << 8
                    | uint.Parse(items[3]);
        }


        /// <summary>
        /// 判断是否为灰度图
        /// </summary>
        /// <param name="enGvspPixelType">像素类型</param>
        /// <returns></returns>
        private bool IsMonoData(MyCamera.MvGvspPixelType enGvspPixelType)
        {
            switch (enGvspPixelType)
            {
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12_Packed:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 判断是否为彩色图
        /// </summary>
        /// <param name="enGvspPixelType">像素格式</param>
        /// <returns></returns>
        private bool IsColorData(MyCamera.MvGvspPixelType enGvspPixelType)
        {
            switch (enGvspPixelType)
            {
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_YUV422_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_YUV422_YUYV_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_YCBCR411_8_CBYYCRYY:
                    return true;

                default:
                    return false;
            }
        }

        private bool RemoveCustomPixelFormats(MyCamera.MvGvspPixelType enPixelFormat)
        {
            int nResult = ((int)enPixelFormat) & (unchecked((int)0x80000000));
            if (unchecked((int)0x80000000) == nResult)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 对驱动进行检测
        /// </summary>
        /// <returns></returns>
        public override bool CheckDriver()
        {
            bool isExist = false;
            try
            {
                MyCamera.MV_CC_GetSDKVersion_NET();
                isExist = true;
            }
            catch (Exception)
            {

                throw;
            }

            return isExist;
        }


        /// <summary>
        /// 通过文件加载
        /// </summary>
        /// <param name="filename">文件路径</param>
        public void CameraFromFile(string filename)
        {
            if (string.IsNullOrEmpty(filename)) 
            {
                throw new FileNotFoundException("文件名为空");
            }

            if (!File.Exists(filename))
            {
                throw new FileNotFoundException(filename);
            }

            // 图像
            using (Bitmap bitmap = Bitmap.FromFile(filename) as Bitmap)
            {
                int pixel = Bitmap.GetPixelFormatSize(bitmap.PixelFormat);
                int channel = 1;
                if (bitmap.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    channel = 1;
                }
                else if (bitmap.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    channel = 3;
                }
                else
                {
                    throw new NotSupportedException($"图像格式：{bitmap.PixelFormat}不支持");
                }

                BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                int stride = bmpData.Stride;
                int len = stride * bmpData.Height;

                int allocLen = (Marshal.SizeOf(typeof(byte)) * len);
                IntPtr imgPtr = Marshal.AllocHGlobal(allocLen);
                CopyMemory(imgPtr, bmpData.Scan0, (uint)allocLen);
                bitmap.UnlockBits(bmpData);

                FrameImageEvent?.Invoke(new LImage() { ByteOfPixel = pixel, Channel = channel, Height = bitmap.Height, Width = bitmap.Width, Pointer = imgPtr });
            }
        }
    }
}