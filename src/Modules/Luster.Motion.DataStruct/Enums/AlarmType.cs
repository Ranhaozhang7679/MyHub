#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AlarmType
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct._6.Enums
* 文 件 名:       AxisStatus.cs
* 创建时间:       2022/4/21 17:28:16
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      ac0e689a-7612-4840-aa54-3f35cbdfcb9e
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/21 17:28:16
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Enums
{
    /// <summary>
    /// 报警类型
    /// </summary>
    public enum AlarmType
    {
        /// <summary>
        /// 提示类报警
        /// </summary>
        [Description("信息提示")]
        InfoTip,

        /// <summary>
        /// 警告类,取消或者忽略
        /// </summary>
        [Description("警告提示")]
        WarningTip,

        /// <summary>
        /// 检查到设备问题
        /// </summary>
        [Description("设备异常")]
        DeviceError,

        /// <summary>
        /// 初始化进行回零报错
        /// </summary>
        [Description("回零异常")]
        HomeError,

        /// <summary>
        /// 一般实现耗时超出范围了
        /// </summary>
        [Description("运行超时")]
        Timeout,

        /// <summary>
        /// 运行失败
        /// </summary>
        [Description("运行失败")]
        FailError,

        /// <summary>
        /// PLC报警
        /// </summary>
        [Description("PLC报警")]
        PlcAlarm,

        /// <summary>
        /// 弹窗提示
        /// </summary>
        [Description("弹窗提示")]
        PopInfoTip,

        /// <summary>
        /// 取料弹窗提示
        /// </summary>
        //[Description("取料弹窗提示")]
        /// 取料弹窗提示 修改为 报警断点
        /// 2025-4-15
        [Description("报警断点")]
        RetryAlarm,

        /// <summary>
        /// 人工介入提示
        /// </summary>
        [Description("人工介入提示")]
        ManuOperationAlarm,

    }

    /// <summary>
    /// 报警处理方式
    /// </summary>
    public enum AlarmProc
    {
        [Description("设备维修")]
        Repair,

        [Description("设备检测")]
        Check,

        [Description("产品NG")]
        Ng,

        [Description("动作重试")]
        Retry,

        [Description("继续")]
        Contine,

        [Description("停止")]
        Stop,

        [Description("PLC清错")]
        PlcClearError,

        [Description("取料弹窗提示")]
        RetryAlarm
    }
}
