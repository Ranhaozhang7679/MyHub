#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       HandoverModule
* 文 件 名:       HandoverModule.cs
* 创建时间:       2026/06/22
* 作    者:       全栈工程师
* 版    权:       Luster Technology Co.,Ltd.
* 说    明:       TES-37-3 / TES-46 交握模块(注册 Feed/Leave 节点);TES-37-4 / TES-47 追加 ICW 节点
************************************************************************************/
#endregion

using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Modules;

namespace Luster.Module.Motion.Handover
{
    /// <summary>
    /// 交握模块:注册上下游交握节点。
    /// <para>ADR-TES-37 决策 D2:本 Issue(TES-37-4)在 TES-37-3 的 Feed/Leave 基础上
    /// 追加 ICW(Server 侧)节点;AutoSignalService(TES-37-6)、Client 接线集成(TES-37-7)不在本 Issue。</para>
    /// </summary>
    public class HandoverModule : MotionModule
    {
        /// <summary>
        /// 初始化节点:用 AddFunction 注册交握节点。
        /// </summary>
        public override void InitFunctions()
        {
            AddFunction<HandoverFeedNode>();
            AddFunction<HandoverLeaveNode>();
            AddFunction<HandoverICWNode>();
        }
    }

    /// <summary>
    /// 交握模块创建器:供平台动态加载。
    /// </summary>
    public class HandoverModuleCreator : MotionModuleCreator<HandoverModule>
    {
        /// <summary>模块排序(置于 Protocol 之后)</summary>
        public override int Sort => 6;

        /// <summary>模块图标</summary>
        public override string Icon => "\xea1a";
    }
}
