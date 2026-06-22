#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       HandoverLeaveNode
* 文 件 名:       HandoverLeaveNode.cs
* 创建时间:       2026/06/22
* 作    者:       全栈工程师
* 版    权:       Luster Technology Co.,Ltd.
* 说    明:       TES-37-3 / TES-46 下料交握状态机(13 步主路径 + 异常分支 101/102)
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Enums;
using Luster.Module.Motion.Handover.Signals;
using Luster.TaskFlow.Motion;
using System;
using System.ComponentModel;

namespace Luster.Module.Motion.Handover
{
    /// <summary>
    /// 下料交握节点:作 Client 写下游去料信号,移植源端
    /// <c>SP-2025140\Plugin.CommonPlugin\Task\FlowNormal\CheckNormalAction.cs:1169-1315</c>
    /// 的 <c>HandoverLeaveToDownArmToStage</c> 13 步 step-switch 状态机(可还原,含异常分支 101/102)。
    /// <para>ADR-TES-37 决策 D2:角色 = 下料(Client),对应源端 <c>HandoverModbusTcpClient</c> 逐位握手。</para>
    /// <para>信号读写经基类 <see cref="HandoverNode.ReadSignal"/> / <see cref="HandoverNode.WriteSignal"/>
    /// 字符串地址(对齐源端 <c>ReadUnloadSignal</c>/<c>WriteUnloadSignal</c> 运行路径)。</para>
    /// <para>运动/产品在籍/站级动作抽为基类钩子,站层在 TES-37-7 集成时 override 接线。</para>
    /// </summary>
    public class HandoverLeaveNode : HandoverNode
    {
        /// <summary>
        /// 构造函数:固定角色为下料。
        /// </summary>
        public HandoverLeaveNode()
        {
            this.Role = HandoverRole.Leave;
            this.Tips = "下料交握(写下游,13 步 + 异常 101/102)";
        }

        /// <summary>
        /// 清零下游交握信号:对齐源端 <c>UnloadSingleClaer</c>。
        /// </summary>
        protected override void ClearSignals()
        {
            ClearSendSignals();
        }

        /// <summary>
        /// 下料 13 步状态机:逐 case 还原源端
        /// <c>CheckNormalAction.cs:1178-1304</c> 的 switch(step) 逻辑。
        /// <para>步号语义与源端一致:0-12 主路径,99/100 复位与完成,
        /// 101/102 异常撤离分支,-1 异常重启。</para>
        /// </summary>
        protected override int StepMachine(int step)
        {
            // 下料方向标识(对齐源端 send/rece 字符串)
            string send = "[本站=>下游][发送]";
            string rece = "[下游=>本站][读取]";

            switch (step)
            {
                case -1:
                    // 源端 :1180 异常结束,清信号后重新开始
                    StepInfo(step, $"{send} 交握流程异常结束,返回等待位置,重新开始");
                    ClearSignals();
                    step++;
                    break;

                case 0:
                    // 源端 :1186 开始交握,移动到下料等待位
                    StepInfo(step, $"{send} 开始交握,移动到下料等待位");
                    if (!MoveToWaitPosition()) return HandoverNode.StepAbort;
                    step++;
                    break;

                case 1:
                    // 源端 :1191 等待 RecReady ON
                    StepInfo(step, $"{rece} 等待 RecReady ON");
                    if (ReadSignal(Address.RecReadyAddress)) step++;
                    else SyncWait(WaitSleep);
                    break;

                case 2:
                    // 源端 :1196 原写入帐料信息 已前置异步执行(直通)
                    step++;
                    break;

                case 3:
                    // 源端 :1199 等待 RecInterLock ON
                    StepInfo(step, $"{rece} 等待 RecInterLock ON");
                    if (!ReadSignal(Address.RecReadyAddress)) { step = -1; break; }
                    if (ReadSignal(Address.RecInterLockAddress)) step++;
                    else SyncWait(WaitSleep);
                    break;

                case 4:
                    // 源端 :1205 移动到下料位
                    StepInfo(step, $"{send} 移动到下料位");
                    if (!MoveToWorkPosition()) return HandoverNode.StepAbort;
                    step++;
                    break;

                case 5:
                    // 源端 :1210 发送 SendInterLock ON
                    StepInfo(step, $"{send} 发送 SendInterLock ON");
                    if (!WriteSignal(Address.SendInterLockAddress, true)) { step = -1; break; }
                    if (ReadSignal(Address.SendInterLockAddress)) step++;
                    else SyncWait(WaitSleep);
                    break;

                case 6:
                    // 源端 :1216 发送 Sending ON
                    StepInfo(step, $"{send} 发送 Sending ON");
                    if (!ReadSignal(Address.RecReadyAddress)) { step = -1; break; }
                    if (!WriteSignal(Address.SendingAddress, true)) { step = -1; break; }
                    if (ReadSignal(Address.SendingAddress)) step++;
                    else SyncWait(WaitSleep);
                    break;

                case 7:
                    // 源端 :1228 等待 Recing ON
                    StepInfo(step, $"{rece} 等待 Recing ON");
                    if (!ReadSignal(Address.RecReadyAddress)) { step = -1; break; }
                    if (ReadSignal(Address.RecingAddress)) step++;
                    else SyncWait(WaitSleep);
                    break;

                case 8:
                    // 源端 :1237 破真空 + 产品在籍清除/复位
                    StepInfo(step, $"{send} 破真空");
                    if (!ReadSignal(Address.RecReadyAddress)) { step = -1; break; }
                    if (!SorbControl(false)) return HandoverNode.StepAbort;
                    // 清除本站数据/更新在籍(对齐源端 Sorbs.Reset + ProductCache.Reset + SetDataSource)
                    if (!TransferProductInfo()) return HandoverNode.StepAbort;
                    step++;
                    break;

                case 9:
                    // 源端 :1258 SendTransfer ON
                    StepInfo(step, $"{send} SendTransfer ON");
                    if (!WriteSignal(Address.SendTranSferAddress, true)) { step = 9; break; }
                    if (ReadSignal(Address.SendTranSferAddress)) step++;
                    else SyncWait(WaitSleep);
                    break;

                case 10:
                    // 源端 :1264 RecTransfer ON(否则进异常 101)
                    StepInfo(step, $"{rece} RecTransfer ON");
                    if (!ReadSignal(Address.RecReadyAddress)) { step = 101; break; }
                    if (ReadSignal(Address.RecTranSferAddress)) step++;
                    else { step = 101; }
                    break;

                case 11:
                    // 源端 :1270 SendInterLock、Sending、SendTransfer OFF
                    StepInfo(step, $"{send} SendInterLock、Sending、SendTransfer OFF");
                    WriteSignal(Address.SendingAddress, false);
                    WriteSignal(Address.SendTranSferAddress, false);
                    WriteSignal(Address.SendInterLockAddress, false);
                    if (!ReadSignal(Address.SendingAddress) &&
                        !ReadSignal(Address.SendTranSferAddress) &&
                        !ReadSignal(Address.SendInterLockAddress)) step++;
                    break;

                case 12:
                    // 源端 :1279 等待 RecInterLock、RecTransfer OFF(完成进 99)
                    StepInfo(step, $"{rece} 等待 RecInterLock RecTransfer OFF");
                    if (!ReadSignal(Address.RecInterLockAddress) &&
                        !ReadSignal(Address.RecTranSferAddress)) step = 99;
                    else SyncWait(WaitSleep);
                    break;

                case 99:
                    // 源端 :1286 慢速返回下料等待位 → 100
                    if (!MoveToSafePosition()) return HandoverNode.StepAbort;
                    step = HandoverNode.StepDone;
                    break;

                case 101:
                    // 源端 :1290 异常撤离
                    StepInfo(step, $"{send} 异常撤离");
                    if (!MoveToSafePosition()) return HandoverNode.StepAbort;
                    step++;
                    break;

                case 102:
                    // 源端 :1296 Transfer、Sending OFF → 100
                    StepInfo(step, $"{send} Transfer、Sending OFF");
                    ClearSignals();
                    step = HandoverNode.StepDone;
                    break;

                default:
                    // 源端 :1301 步骤异常
                    MyOwner.OnLog(LogType.Error, $"HandoverLeaveNode 步骤异常 Step={step}");
                    return HandoverNode.StepAbort;
            }

            return step;
        }
    }
}
