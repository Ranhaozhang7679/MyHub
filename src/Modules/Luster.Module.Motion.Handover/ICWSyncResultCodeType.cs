#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ICWSyncResultCodeType
* 文 件 名:       ICWSyncResultCodeType.cs
* 创建时间:       2026/06/22
* 作    者:       全栈工程师
* 版    权:       Luster Technology Co.,Ltd.
* 说    明:       TES-37-4 / TES-47 ICW 同步结果码枚举(对齐源端)
************************************************************************************/
#endregion

namespace Luster.Module.Motion.Handover
{
    /// <summary>
    /// ICW 同步结果码:PLC 与 ICW 工控机之间 Request/Response 寄存器对的 Response 字取值。
    /// <para>对齐源端 <c>SP-2025140\Plugin.CommonPlugin\Model\ICW\ICWSyncResultCodeType.cs</c>
    /// (枚举值与中文成员名均按源端原样迁入,<see cref="ICWSyncResultCodeType.校验PLC数据异常"/> = 10 起
    /// 连续自增,保证线缆字面值与源端一致 —— 可还原硬指标)。</para>
    /// <para>用法(对齐源端 <c>CheckStationTask.ReadEndStatus</c>):读 Response 寄存器 ushort,
    /// 强转为本枚举;None=未完成继续轮询,Success=成功结束,其余=异常码(对齐源端 WarningPause)。</para>
    /// </summary>
    public enum ICWSyncResultCodeType : ushort
    {
        /// <summary>无(未完成,继续轮询)</summary>
        None = 0,

        /// <summary>成功</summary>
        Success = 1,

        /// <summary>校验 PLC 数据异常(=10,源端起值)</summary>
        校验PLC数据异常 = 10,

        /// <summary>ICW 流程异常(=11)</summary>
        ICW流程异常,

        /// <summary>检测软件异常(=12;源端该码触发重试,其余异常码退出)</summary>
        检测软件异常,

        /// <summary>MES 入站异常(=13)</summary>
        MES入站异常,

        /// <summary>MES 出站异常(=14)</summary>
        MES出站异常,
    }
}
