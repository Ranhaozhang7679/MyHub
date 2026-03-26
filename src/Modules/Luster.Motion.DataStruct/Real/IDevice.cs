#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 接口名称:       IDevice
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Real
* 文 件 名:       IDevice.cs
* 创建时间:       2022/4/2 15:21:50
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      3e894ff1-d637-4c6e-b188-258f97063db4
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/2 15:21:50
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Real
{
    /// <summary>
    /// 设备信息
    /// </summary>
    public interface IDevice : IXMLParser, IDisposable
    {
        /// <summary>
        /// 设备对应的唯一标识
        /// </summary>
        Guid ID { get; set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        DeviceType DeviceType { get; }

        /// <summary>
        /// 设备别名
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 版本信息
        /// </summary>
        string Version { get; set; }

        /// <summary>
        /// 品牌
        /// </summary>
        string Brand { get; }

        /// <summary>
        /// 设备对应的图标
        /// </summary>
        string Icon { get; }

        /// <summary>
        /// 硬件错误消息
        /// </summary>
        string ErrMsg { get; set; }

        /// <summary>
        /// 设备状态
        /// </summary>
        DeviceStatus Status { get; set; }

        /// <summary>
        /// 日志使劲按
        /// </summary>
        event Action<LogType, string> LogEvent;

        /// <summary>
        /// 状态变更通知
        /// </summary>
        event Action<IDevice, DeviceStatus> StatusChangedEvent;

        /// <summary>
        /// 连接方式
        /// </summary>
        IAdapter Adapter { get; set; }
        #region 设备通用方法
        /// <summary>
        /// 初始化API
        /// </summary>
        void InitApi();

        /// <summary>
        /// 设备打开，通常连接设备
        /// </summary>
        void Open();

        /// <summary>
        /// 关闭设备
        /// </summary>
        void Close();

        /// <summary>
        /// 启用禁用设备
        /// </summary>
        /// <param name="enabled">启用或禁用</param>
        void SetEnabled(bool enabled);

        /// <summary>
        /// 更新硬件参数
        /// </summary>
        /// <typeparam name="T">参数类</typeparam>
        /// <param name="paramName">参数名称</param>
        /// <param name="val">参数值</param>
        void UpdateParam<T>(string paramName, T val);

        /// <summary>
        /// 是否有初始化并连接
        /// </summary>
        /// <returns></returns>
        bool HasInit();

        /// <summary>
        /// 驱动是否存在
        /// </summary>
        /// <returns></returns>
        bool CheckDriver();
        #endregion
    }
}