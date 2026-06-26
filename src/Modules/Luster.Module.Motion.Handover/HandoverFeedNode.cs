#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       HandoverFeedNode
* 文 件 名:       HandoverFeedNode.cs
* 创建时间:       2026/06/22
* 作    者:       全栈工程师
* 版    权:       Luster Technology Co.,Ltd.
* 说    明:       TES-37-3 / TES-46 上料交握状态机(15 步主路径 + 异常分支 101/102)
************************************************************************************/
#endregion

using Luster.Module.Motion.Handover.Signals;
using Luster.TaskFlow.Motion;
using System;
using System.ComponentModel;

namespace Luster.Module.Motion.Handover
{
    /// <summary>
    /// 上料交握节点:作 Client 读上游来料信号,移植源端
    /// <c>SP-2025140\Plugin.CommonPlugin\Task\FlowNormal\CheckNormalAction.cs:1000-1163</c>
    /// 的 <c>HandoverFeedFromUpArmToStage</c> 15 步 step-switch 状态机(可还原,含异常分支 101/102)。
    /// <para>ADR-TES-37 决策 D2:角色 = 上料(Client),对应源端 <c>HandoverModbusTcpClient</c> 逐位握手。</para>
    /// <para>信号读写经基类 <see cref="HandoverNode.ReadSignal"/> / <see cref="HandoverNode.WriteSignal"/>
    /// 字符串地址(对齐源端 <c>ReadLoadSignal</c>/<c>WriteLoadSignal</c> 运行路径)。</para>
    /// <para>运动/产品在籍/站级动作抽为基类钩子(<see cref="HandoverNode.MoveToWaitPosition"/> 等),
    /// 默认占位返回 true,站层在 TES-37-7 集成时 override 接线。</para>
    /// </summary>
    public class HandoverFeedNode : HandoverNode
    {
        /// <summary>
        /// 构造函数:固定角色为上料。
        /// </summary>
        public HandoverFeedNode()
        {
            this.Role = HandoverRole.Feed;
            this.Tips = "上料交握(读上游,15 步 + 异常 101/102)";
        }

        /// <summary>
        /// 清零上游交握信号:对齐源端 <c>LoadSingleClaer</c>。
        /// </summary>
        protected override void ClearSignals()
        {
            ClearSendSignals();
        }

        /// <summary>
        /// 上料 15 步状态机:逐 case 还原源端
        /// <c>CheckNormalAction.cs:1010-1150</c> 的 switch(step) 逻辑。
        /// <para>步号语义与源端一致:0-14 主路径,98/99/100 复位与完成,
        /// 101/102 异常撤离分支。</para>
        /// </summary>
        protected override int StepMachine(int step)
        {
            // 上料方向标识(对齐源端 send/rece 字符串)
            string send = "[本站=>上游][发送]";
            string rece = "[上游=>本站][读取]";

            switch (step)
            {
                case -1:
                    // 源端 :1012 等待上游 Ready 信号
                    StepInfo(step, $"{rece} 等待上游 Ready 信号");
                    if (ReadSignal(Address.RecReadyAddress)) step = 0;
                    else SyncWait(200);
                    break;

                case 0:
                    // 源端 :1018 清除交握信号 + 移动到上料等待位
                    StepInfo(step, $"{send} 清除交握信号");
                    if (!MoveToWaitPosition()) return HandoverNode.StepAbort; // 中止:运动失败
                    ClearSignals();
                    step++;
                    break;

                case 1:
                    // 源端 :1026 RecReadyReady ON
                    StepInfo(step, $"{rece} RecReady ON");
                    if (ReadSignal(Address.RecReadyAddress)) step++;
                    else SyncWait(WaitSleep);
                    break;

                case 2:
                    // 源端 :1031 直通
                    step++;
                    break;

                case 3:
                    // 源端 :1034 SendInterLock ON(上游 Ready 撤销则回退)
                    StepInfo(step, $"{send} SendInterLock ON");
                    if (!ReadSignal(Address.RecReadyAddress)) { step = -1; break; }
                    if (!WriteSignal(Address.SendInterLockAddress, true)) { step = -1; break; }
                    if (ReadSignal(Address.SendInterLockAddress)) step++;
                    else SyncWait(WaitSleep);
                    break;

                case 4:
                    // 源端 :1041 RecInterLock ON
                    StepInfo(step, $"{rece} RecInterLock ON");
                    if (ReadSignal(Address.RecInterLockAddress)) step++;
                    else SyncWait(WaitSleep);
                    step++;
                    break;

                case 5:
                    // 源端 :1047 移动到上料位
                    StepInfo(step, $"{send} 移动到上料位");
                    if (!MoveToWorkPosition()) return HandoverNode.StepAbort;
                    step++;
                    break;

                case 6:
                    // 源端 :1054 RecSending ON + 读取上游产品信息
                    StepInfo(step, $"{rece} RecSending ON");
                    if (!ReadSignal(Address.RecReadyAddress)) { step = -1; break; }
                    if (ReadSignal(Address.RecingAddress))
                    {
                        StepInfo(step, $"{rece} 读取上游产品信息");
                        if (TransferProductInfo())
                        {
                            step++;
                        }
                    }
                    else SyncWait(WaitSleep);
                    break;

                case 7:
                    // 源端 :1069 Sending ON + 真空吸
                    StepInfo(step, $"{send} Sending ON");
                    if (!ReadSignal(Address.RecReadyAddress)) { step = -1; break; }
                    if (!SorbControl(true)) return HandoverNode.StepAbort;
                    if (!WriteSignal(Address.SendingAddress, true)) { step = -1; break; }
                    if (ReadSignal(Address.SendingAddress)) step++;
                    break;

                case 8:
                    // 源端 :1085 直通
                    step++;
                    break;

                case 9:
                    // 源端 :1088 RecTransfer ON
                    StepInfo(step, $"{rece} RecTransfer ON");
                    if (!ReadSignal(Address.RecReadyAddress)) { step = 9; break; }
                    if (ReadSignal(Address.RecTranSferAddress) ||
                        !ReadSignal(Address.RecInterLockAddress)) step++;
                    else SyncWait(WaitSleep);
                    break;

                case 10:
                    // 源端 :1095 慢速撤离
                    StepInfo(step, $"{send} 慢速撤离");
                    if (!MoveToSafePosition()) return HandoverNode.StepAbort;
                    step++;
                    break;

                case 11:
                    // 源端 :1100 等待入料在籍清除(直通)
                    StepInfo(step, $"{rece} 等待入料在籍清除");
                    step++;
                    break;

                case 12:
                    // 源端 :1104 SendTransfer 信号 ON
                    StepInfo(step, $"{send} SendTransfer ON");
                    if (!WriteSignal(Address.SendTranSferAddress, true)) { step = 12; break; }
                    if (ReadSignal(Address.SendTranSferAddress)) step++;
                    break;

                case 13:
                    // 源端 :1109 RecInterLock、RecTransfer OFF(否则进异常 101)
                    StepInfo(step, $"{rece} RecInterLock、RecTransfer OFF");
                    if (!ReadSignal(Address.RecInterLockAddress)
                        && !ReadSignal(Address.RecTranSferAddress))
                        step++;
                    else { step = 101; }
                    break;

                case 14:
                    // 源端 :1116 SendInterLock、Sending、SendTransfer OFF(完成则进 98)
                    StepInfo(step, $"{send} SendInterLock、Sending、SendTransfer OFF");
                    WriteSignal(Address.SendingAddress, false);
                    WriteSignal(Address.SendTranSferAddress, false);
                    WriteSignal(Address.SendInterLockAddress, false);
                    if (!ReadSignal(Address.SendingAddress) &&
                        !ReadSignal(Address.SendTranSferAddress) &&
                        !ReadSignal(Address.SendInterLockAddress)) step = 98;
                    else SyncWait(WaitSleep);
                    break;

                case 98:
                    // 源端 :1126 复位发送全部信号
                    StepInfo(step, $"{send} 复位发送全部信号");
                    ClearSignals();
                    step++;
                    break;

                case 99:
                    // 源端 :1131 → 100
                    step = HandoverNode.StepDone;
                    break;

                case 101:
                    // 源端 :1134 异常结束:慢速撤离 + 复位 + SendReady ON
                    StepInfo(step, $"{send} 交握流程异常结束,复位发送全部信号");
                    if (!MoveToSafePosition()) return HandoverNode.StepAbort;
                    ClearSignals();
                    if (!WriteSignal(Address.SendReadyAddress, true)) { step = -1; break; }
                    if (ReadSignal(Address.SendReadyAddress)) step++;
                    else SyncWait(WaitSleep);
                    break;

                case 102:
                    // 源端 :1143 异常结束收尾 → 99
                    StepInfo(step, $"{rece} 交握流程异常结束");
                    step = 99;
                    break;

                default:
                    // 源端 :1147 步骤异常
                    MyOwner.OnLog(Luster.Common.DataStruct.Enums.LogType.Error,
                        $"HandoverFeedNode 步骤异常 Step={step}");
                    return HandoverNode.StepAbort;
            }

            return step;
        }
    }
}
