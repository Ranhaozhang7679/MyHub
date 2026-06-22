#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       HandoverSignalAddress
* 文 件 名:       HandoverSignalAddress.cs
* 创建时间:       2026/06/22
* 作    者:       全栈工程师
* 版    权:       Luster Technology Co.,Ltd.
* 说    明:       TES-37-3 / TES-46 交握信号地址表
************************************************************************************/
#endregion

using System;
using System.ComponentModel;

namespace Luster.Module.Motion.Handover.Signals
{
    /// <summary>
    /// 交握信号地址表(字符串地址载体)。
    /// <para>迁自源端 SP-2025140 <c>Plugin.CommonPlugin.Model.LoadInteraction</c> /
    /// <c>UnLoadInteraction</c>(<c>Model\LoadInteraction.cs</c> / <c>Model\UnLoadInteraction.cs</c>),
    /// 用于 <see cref="HandoverNode"/> 子类型经 <c>VCommuncation</c> 字符串地址读写握手信号,
    /// 对齐源端 <c>CheckStationTask.ReadLoadSignal/WriteLoadSignal</c> 运行路径。</para>
    /// <para>ADR-TES-37 决策 D3:32 位位定义见 <see cref="HandoverSignalBit"/>,
    /// 实际读写走本表的字符串地址(与源端运行行为一致,可还原)。</para>
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class HandoverSignalAddress
    {
        #region Send(本站发送侧地址)

        /// <summary>发送准备 OK 地址(对应源端 SendReadyAddress)</summary>
        [DisplayName("发送准备OK地址")]
        public string SendReadyAddress { get; set; }

        /// <summary>发送开始地址(对应源端 SendingAddress)</summary>
        [DisplayName("发送开始地址")]
        public string SendingAddress { get; set; }

        /// <summary>发送完成地址(对应源端 SendTranSferAddress)</summary>
        [DisplayName("发送完成地址")]
        public string SendTranSferAddress { get; set; }

        /// <summary>互锁信号地址(对应源端 SendInterLockAddress)</summary>
        [DisplayName("互锁信号地址")]
        public string SendInterLockAddress { get; set; }

        /// <summary>心跳信号地址(对应源端 SendHeartBeatAddress)</summary>
        [DisplayName("心跳信号地址")]
        public string SendHeartBeatAddress { get; set; }

        /// <summary>本站手动操作安全信号地址(对应源端 SendLoadSecurityAddress / SendUnLoadSecurityAddress)</summary>
        [DisplayName("本站手动操作安全信号地址")]
        public string SendSecurityAddress { get; set; }

        #endregion

        #region Rec(对端接收侧地址)

        /// <summary>接收准备 OK 地址(对应源端 RecReadyAddress)</summary>
        [DisplayName("接收准备OK地址")]
        public string RecReadyAddress { get; set; }

        /// <summary>接收开始地址(对应源端 RecingAddress)</summary>
        [DisplayName("接收开始地址")]
        public string RecingAddress { get; set; }

        /// <summary>接收完成地址(对应源端 RecTranSferAddress)</summary>
        [DisplayName("接收完成地址")]
        public string RecTranSferAddress { get; set; }

        /// <summary>互锁信号地址(对应源端 RecInterLockAddress)</summary>
        [DisplayName("互锁信号地址")]
        public string RecInterLockAddress { get; set; }

        /// <summary>心跳信号地址(对应源端 RecHeartBeatAddress)</summary>
        [DisplayName("心跳信号地址")]
        public string RecHeartBeatAddress { get; set; }

        /// <summary>对端门锁信号地址(对应源端 bDoor_LockOK_Load / bDoor_LockOK_UnLoad)</summary>
        [DisplayName("对端门锁信号地址")]
        public string RecDoorLockAddress { get; set; }

        /// <summary>本端门锁信号地址(对应源端 bDoor_LockOK_AOI)</summary>
        [DisplayName("本端门锁信号地址")]
        public string SelfDoorLockAddress { get; set; }

        /// <summary>对端手动操作安全信号地址(对应源端 RecLoadSecurityAddress / RecUnLoadSecurityAddress)</summary>
        [DisplayName("对端手动操作安全信号地址")]
        public string RecSecurityAddress { get; set; }

        #endregion

        /// <summary>
        /// 是否所有关键握手地址都已配置(Ready/Sending/Transfer/InterLock 四组)。
        /// 用于 <see cref="HandoverNode"/> 启动前自检,避免空地址进入状态机。
        /// </summary>
        [Browsable(false)]
        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(SendReadyAddress) &&
            !string.IsNullOrWhiteSpace(SendingAddress) &&
            !string.IsNullOrWhiteSpace(SendTranSferAddress) &&
            !string.IsNullOrWhiteSpace(SendInterLockAddress) &&
            !string.IsNullOrWhiteSpace(RecReadyAddress) &&
            !string.IsNullOrWhiteSpace(RecingAddress) &&
            !string.IsNullOrWhiteSpace(RecTranSferAddress) &&
            !string.IsNullOrWhiteSpace(RecInterLockAddress);
    }
}
