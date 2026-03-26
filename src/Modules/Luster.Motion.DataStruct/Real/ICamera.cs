#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 接口名称:       ICamera
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Real
* 文 件 名:       ICamera.cs
* 创建时间:       2022/4/8 8:56:06
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      30f0e3e8-2dc0-4135-bda6-f1137b2e924e
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/8 8:56:06
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Real
{
    /// <summary>
    /// 相机的API
    /// </summary>
    public interface ICamera
    {
        /// <summary>
        /// 序列号
        /// </summary>
        string SerialNumber { get; set; }

        /// <summary>
        /// 帧率
        /// </summary>
        float FrameRate { get; set; }

        /// <summary>
        /// 曝光时间
        /// </summary>
        float ExposureTime { get; set; }

        /// <summary>
        /// 增益
        /// </summary>
        float Gain { get; set; }

        /// <summary>
        /// 是否已经打开相机
        /// </summary>
        bool IsOpen { get; set; }

        /// <summary>
        /// 相机取流事件
        /// </summary>
        event Action<LImage> FrameImageEvent;

        /// 读取相机列表
        /// </summary>
        /// <returns></returns>
        void CameraListRead(out List<IDevice> m_stDeviceList);

        /// <summary>
        /// 打开相机
        /// </summary>
        /// <param name="device">相机序列号</param>
        /// <returns></returns>
        bool CameraOpen(out string message);

        /// <summary>
        /// 相机采图
        /// </summary>
        /// <returns></returns>
        void CameraStartGrab();

        /// <summary>
        /// 相机停止采图
        /// </summary>
        /// <returns></returns>
        void CameraStopGrab();

        /// <summary>
        /// 相机图片保存
        /// </summary>
        void CameraPicSave();

        /// <summary>
        /// 相机视频流保存
        /// </summary>
        void CameraVedioSave();

        /// <summary>
        /// 相机名更改
        /// </summary>
        /// <param name="cameraName"></param>
        /// <returns></returns>
        void CameraNameSet(string cameraName);

        /// <summary>
        /// 相机IP更改
        /// </summary>
        /// <param name="cameraName"></param>
        /// <returns></returns>
        void CameraIpSet(uint cameraIp, uint cameraSubnet, uint cameraGateway);

        /// <summary>
        /// 单个相机参数读取
        /// </summary>
        /// <param name="frameRate">帧率</param>
        /// <param name="eposureTime">曝光</param>
        /// <param name="gain">增益</param>
        /// <param name="gamma">伽马</param>
        /// <returns></returns>
        void CameraParaRead(out float frameRate, out float exposureTime, out float gain, out float gamma);

        /// <summary>
        /// 相机枚举参数设置
        /// </summary>
        /// <param name="strKey"></param>
        /// <param name="nValue"></param>
        /// <returns></returns>
        void CameraEnumParaSet(string strKey, uint nValue);

        /// <summary>
        /// 相机命令设置
        /// </summary>
        /// <param name="strKey"></param>
        /// <returns></returns>
        void CameraCommandValSet(string strKey);

        /// <summary>
        /// 关闭相机
        /// </summary>
        void CloseCamera();

        /// <summary>
        /// 通过文件加载
        /// </summary>
        /// <param name="filename">文件路径</param>
        void CameraFromFile(string filename);
    }
}