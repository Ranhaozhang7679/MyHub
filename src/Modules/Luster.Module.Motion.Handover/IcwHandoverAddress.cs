#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IcwHandoverAddress
* 文 件 名:       IcwHandoverAddress.cs
* 创建时间:       2026/06/22
* 作    者:       全栈工程师
* 版    权:       Luster Technology Co.,Ltd.
* 说    明:       TES-37-4 / TES-47 ICW Req/Resp 寄存器对地址表
************************************************************************************/
#endregion

using System;
using System.ComponentModel;

namespace Luster.Module.Motion.Handover
{
    /// <summary>
    /// ICW 交握地址表:Request/Response 寄存器对 + 心跳 + 配方/模式轮询地址。
    /// <para>对齐源端 <c>SP-2025140\Plugin.CommonPlugin\Task\Machine\CheckStationTask.cs</c>
    /// 的 ICW 路径(<c>ICWSetting.LoadProductInfoProfile.RequestAddress/ResponseAddress</c>、
    /// 心跳 <c>socketProfile.Heart</c>、配方/模式 <c>测试模式/扫描数量</c> 寄存器,见 ICW 通讯配置 CSV)。</para>
    /// <para>ADR-TES-37 决策 D2 / §3.2:ICW 走 Request/Response 寄存器对 +
    /// <see cref="ICWSyncResultCodeType"/> 枚举,与普通交握的逐位握手根本不同,故不复用
    /// <c>HandoverSignalAddress</c> 的 8 位地址表。</para>
    /// <para>ADR-TES-37 决策 D3:实际读写仍走字符串地址(对齐源端 <c>GetActualAddress</c> 运行路径)。</para>
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class IcwHandoverAddress
    {
        #region Req/Resp 寄存器对(核心)

        /// <summary>请求地址:本站写 1 触发 ICW 流程(对齐源端 WriteStartStatus → RequestAddress)</summary>
        [DisplayName("请求地址")]
        public string RequestAddress { get; set; }

        /// <summary>响应地址:读 ICWSyncResultCodeType(对齐源端 ReadEndStatus → ResponseAddress)</summary>
        [DisplayName("响应地址")]
        public string ResponseAddress { get; set; }

        #endregion

        #region 心跳(对齐源端 socketProfile.Heart)

        /// <summary>心跳地址:本站周期性写入维持在线(对齐源端 WriteHeartBeat → Heart)</summary>
        [DisplayName("心跳地址")]
        public string HeartbeatAddress { get; set; }

        #endregion

        #region 配方/模式轮询(对齐源端 ICW 通讯配置 CSV:测试模式/扫描数量)

        /// <summary>配方/测试模式地址:写 ICW 检测配方(可选,留空则跳过;对齐源端 测试模式=4564)</summary>
        [DisplayName("配方地址")]
        public string RecipeAddress { get; set; }

        /// <summary>模式地址:写运行模式(可选,留空则跳过;对齐源端 扫描数量=4565)</summary>
        [DisplayName("模式地址")]
        public string ModeAddress { get; set; }

        #endregion

        /// <summary>
        /// 是否核心 Req/Resp 寄存器对已配置(Request + Response)。
        /// 用于 <see cref="HandoverICWNode"/> 启动前自检,避免空地址进入状态机。
        /// </summary>
        [Browsable(false)]
        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(RequestAddress) &&
            !string.IsNullOrWhiteSpace(ResponseAddress);
    }
}
