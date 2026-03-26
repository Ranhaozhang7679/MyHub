#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 接口名称:       IVirtual
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Virtual
* 文 件 名:       IVirtual.cs
* 创建时间:       2022/4/2 15:21:28
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      00a65a85-31b6-4c3b-9379-7e295df99f1e
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/2 15:21:28
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Virtual
{
    /// <summary>
    /// 虚拟设备
    /// </summary>
    public interface IVirtualDevice : IXMLParser, IName
    {
        #region 1.事件

        /// <summary>
        /// 日志
        /// </summary>
        event Action<LogType, string> LogEvent;

        /// <summary>
        /// 输出变更
        /// </summary>
        event Action<IVirtualDevice, string, object, object> PropertyChanged;
        #endregion

        #region 2.属性

        /// <summary>
        /// 模块
        /// </summary>
        string Module { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        int Sort { get; }

        /// <summary>
        /// 设备ID
        /// </summary>
        Guid ID { get; set; }

        /// <summary>
        /// 隶属设备ID
        /// </summary>
        Guid DeviceID { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 设备模式 设置为虚拟设备|真实设备
        /// </summary>
        DeviceMode Mode { get; set; }

        /// <summary>
        /// 设备
        /// </summary>
        IDeviceEngine Engine { get; set; }

        /// <summary>
        /// 图标信息
        /// </summary>
        string Icon { get; }

        /// <summary>
        /// 被那个设备引用
        /// </summary>
        IVirtualDevice ByUse { get; set; }

        /// <summary>
        /// 是否中断
        /// </summary>
        bool IsBreak { get; set; }


        /// <summary>
        /// 是否暂停
        /// </summary>
        bool IsPause { get; set; }
        /// <summary>
        /// 虚拟设备状态
        /// </summary>
        VStatus VStatus { get; set; }

        /// <summary>
        /// 设备异常后，添加报警代码
        /// </summary>
        string AlarmCode { get; set; }

        /// <summary>
        /// 错误提示
        /// </summary>
        string ErrorMessage { get; set; }
        #endregion

        #region 3.方法
        /// <summary>
        /// 设备信息
        /// </summary>
        /// <param name="hardDevice">真实设备</param>
        /// <param name="errMsg">设备错误</param>
        /// <returns>设备绑定成功与否</returns>
        bool SetDevice(IDevice hardDevice, out string errMsg);

        /// <summary>
        /// 获取真实设备
        /// </summary>
        /// <returns></returns>
        IDevice GetDevice();

        /// <summary>
        /// 属性变更
        /// </summary>
        /// <param name="srcV"></param>
        /// <param name="newV"></param>
        void OnPropertyChanged(string propName, object srcV, object newV);
        #endregion
    }
}