#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DeviceBase
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.Real
* 文 件 名:       DeviceBase.cs
* 创建时间:       2022/4/2 15:21:59
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      20021210-2e8a-4983-ba36-6643e245c64c
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/2 15:21:59
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.Adapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.SimDevice.Real
{
    public abstract class DeviceBase : IDevice
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 版本信息
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 版本信息
        /// </summary>
        public string CurrentConnectionType { get; set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        public abstract DeviceType DeviceType { get; }

        /// <summary>
        /// 品牌信息
        /// </summary>
        public abstract string Brand { get; }

        /// <summary>
        /// 设备的图标
        /// </summary>
        public abstract string Icon { get; }

        /// <summary>
        /// 设备连接方式
        /// </summary>
        public IAdapter Adapter { get; set; }

        /// <summary>
        /// 设备状态
        /// </summary>
        [Ignore]
        public DeviceStatus Status { get; set; } = DeviceStatus.Offline;

        /// <summary>
        /// 状态变更通知
        /// </summary>
        public event Action<IDevice, DeviceStatus> StatusChangedEvent;

        /// <summary>
        /// 日志Log
        /// </summary>
        public event Action<LogType, string> LogEvent;

        /// <summary>
        /// 错误消息，用于记录硬件的错误消息
        /// </summary>
        [Ignore]
        public string ErrMsg { get; set; }

        /// <summary>
        /// 对象是否释放
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// 判断硬件是否有初始化完成
        /// </summary>
        /// <returns></returns>
        public bool HasInit()
        {
            return Status != DeviceStatus.Offline;
        }

        #region 用于处理信号等待
        /// <summary>
        /// 状态变更
        /// </summary>
        /// <param name="status"></param>
        protected void OnStatusChanged(DeviceStatus status)
        {
            StatusChangedEvent?.Invoke(this, status);
            Status = status;
        }
        #endregion

        #region 序列化实现
        public XElement ExportXml()
        {
            var xRoot = this.ToXml("Device");
            xRoot.SetAttributeValue("Type", GetType().Name);
            return xRoot;
        }

        public void ParserXml(XElement xElement)
        {
            this.FromXml(xElement);
        }

        #endregion

        #region 对象释放

        /// <summary>
        /// 对象释放
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 子类实现特定对象的释放工作
        /// </summary>
        /// <param name="isDispose"></param>
        protected virtual void Dispose(bool isDispose)
        {
        }
        #endregion

        #region 通用设备接口
        /// <summary>
        /// 初始化API
        /// </summary>
        public virtual void InitApi() { }

        /// <summary>
        /// 设备打开
        /// </summary>
        public virtual void Open() { }

        /// <summary>
        /// 设备关闭
        /// </summary>
        public virtual void Close()
        {

        }

        /// <summary>
        /// 启用禁用设备
        /// </summary>
        /// <param name="enabled">启用或禁用</param>
        public virtual void SetEnabled(bool enabled)
        {

        }

        /// <summary>
        /// 更新参数
        /// </summary>
        /// <typeparam name="T">泛型对象</typeparam>
        /// <param name="paramName">参数名称</param>
        /// <param name="val">参数值</param>
        public virtual void UpdateParam<T>(string paramName, T val)
        {
        }

        /// <summary>
        /// 驱动检测
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckDriver()
        {
            return true;
        }
        #endregion

        /// <summary>
        /// 事件通知
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        protected void OnLog(LogType type, string message)
        {
            LogEvent?.Invoke(type, message);
        }

        /// <summary>
        /// 状态检查
        /// </summary>
        protected void CheckInit()
        {
            if (!HasInit())
            {
                throw new DeviceException($"设备:{Name} 没有初始化!");
            }
        }

        /// <summary>
        /// 通用安全调用本地方法
        /// </summary>
        /// <param name="func">委托，返回值为short,short 为1 是</param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        protected void SafeNativeMethod(Func<bool> func, string errMsg = "")
        {
            try
            {
                bool res = func();
                if (!res)
                {
                    OnLog(LogType.Error, $"{errMsg}");
                    throw new DeviceException($"{DeviceType.GetDescription()}:{Name},{errMsg}");
                }
            }
            catch (DllNotFoundException dll)
            {
                OnLog(LogType.Error, $"{dll.Message}");
            }
            catch (Exception)
            {
                OnLog(LogType.Error, $"{errMsg}");
                throw;
            }
        }

        /// <summary>
        /// 安全本地方法
        /// </summary>
        /// <param name="func"></param>
        protected void SafeNativeMethod(NativeMethodDelegate func)
        {
            try
            {
                bool res = func(out string msg);
                if (!res)
                {
                    throw new DeviceException($"{DeviceType.GetDescription()}:{Name},{msg}");
                }
            }
            catch (DllNotFoundException dll)
            {
                OnLog(LogType.Error, $"{dll.Message}");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    /// <summary>
    /// 委托对象
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public delegate bool NativeMethodDelegate(out string msg);
}