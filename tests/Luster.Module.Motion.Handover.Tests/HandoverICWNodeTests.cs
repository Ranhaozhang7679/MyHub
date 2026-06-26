#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       HandoverICWNodeTests
* 文 件 名:       HandoverICWNodeTests.cs
* 创建时间:       2026/06/22
* 作    者:       全栈工程师
* 版    权:       Luster Technology Co.,Ltd.
* 说    明:       TES-37-4 / TES-47 ICW Req/Resp 寄存器对往返 + 心跳 + 结果码单测
************************************************************************************/
#endregion

using Luster.Module.Motion.Handover;
using System.Collections.Generic;
using Xunit;

namespace Luster.Module.Motion.Handover.Tests
{
    /// <summary>
    /// ICW 交握节点单测:验证 Req/Resp 寄存器对往返、心跳写入、ICWSyncResultCodeType 枚举完整,
    /// 及异常码(检测软件异常重试 / 其余异常退出)对齐源端 <c>CheckStationTask</c> ICW 路径。
    /// <para>通过 <see cref="IcwTestNode"/> 注入"寄存器字典 + 响应脚本 + 可控时钟",
    /// 真实还原源端"写请求位 → 轮询响应位 → 读结果码"的 Req/Resp 语义。</para>
    /// </summary>
    public class HandoverICWNodeTests
    {
        /// <summary>构造一个配置好 ICW 地址的测试节点</summary>
        private static IcwTestNode CreateNode()
        {
            var node = new IcwTestNode
            {
                IcwAddress = new IcwHandoverAddress
                {
                    RequestAddress = "1 03 4000 1",   // 对齐源端 上料开始=4000
                    ResponseAddress = "1 03 4100 1",  // 对齐源端 上料完成=4100
                    HeartbeatAddress = "1 03 3833 1", // 对齐源端 心跳同步信号=3833
                    RecipeAddress = "1 03 4564 1",    // 对齐源端 测试模式=4564
                    ModeAddress = "1 03 4565 1",      // 对齐源端 扫描数量=4565
                },
                RecipeValue = 7,
                ModeValue = 4,
                ResponseTimeout = 5000,
                PollInterval = 100,
                HeartbeatInterval = 300,
            };
            // IcwServerDevice 仅需非空以通过 DoExcute 自检(读写已被测试子类接管)
            node.IcwServerDevice = new Luster.Motion.DataStruct.DataModels.VDevice
            {
                Name = "TestIcwServer",
            };
            return node;
        }

        /// <summary>
        /// Req/Resp 寄存器对往返:Clear(Response=0) → 写配方/模式 → Request=1 →
        /// 轮询 Response(None → Success) → 收尾清响应。验证主路径完整走通。
        /// </summary>
        [Fact]
        public void ICW_ReqResp_RoundTrip_Completes()
        {
            var node = CreateNode();
            var a = node.IcwAddress;

            // 响应脚本:前 2 次 None,第 3 次 Success(对齐源端轮询等待)
            node.ResponseScript = new Queue<ushort>(new ushort[] { 0, 0, (ushort)ICWSyncResultCodeType.Success });

            bool ok = node.DoExcute(out string errMsg);

            Assert.True(ok, errMsg);
            // 请求位写入 1(对齐源端 WriteStartStatus)
            Assert.Contains((a.RequestAddress, (ushort)1), node.Writes);
            // 响应位被清零(对齐源端 ClearStatus)
            Assert.Contains((a.ResponseAddress, (ushort)0), node.Writes);
            // 配方/模式被写入
            Assert.Contains((a.RecipeAddress, node.RecipeValue), node.Writes);
            Assert.Contains((a.ModeAddress, node.ModeValue), node.Writes);
        }

        /// <summary>
        /// 心跳:轮询间隙经 <see cref="HandoverICWNode.HeartbeatInterval"/> 周期写心跳寄存器(0)。
        /// 验证心跳写入对齐源端 <c>WriteHeartBeat</c>(写 0 到 Heart 地址)。
        /// </summary>
        [Fact]
        public void ICW_Heartbeat_WritesHeartbeatRegister()
        {
            var node = CreateNode();
            var a = node.IcwAddress;

            // 多次 None 让轮询跨过若干心跳周期,最后 Success
            node.ResponseScript = new Queue<ushort>(new ushort[]
            {
                0, 0, 0, 0, 0, 0, (ushort)ICWSyncResultCodeType.Success,
            });

            bool ok = node.DoExcute(out string errMsg);
            Assert.True(ok, errMsg);

            // 心跳寄存器至少被写 0 一次(周期触发)
            int heartbeats = node.Writes.FindAll(w => w.Item1 == a.HeartbeatAddress && w.Item2 == 0).Count;
            Assert.True(heartbeats >= 1, $"心跳应被周期写入,实际 {heartbeats} 次");
        }

        /// <summary>
        /// ICWSyncResultCodeType 枚举完整:值与中文成员名对齐源端(可还原硬指标)。
        /// </summary>
        [Fact]
        public void ICW_ResultCodeEnum_AlignsWithSource()
        {
            Assert.Equal((ushort)0, (ushort)ICWSyncResultCodeType.None);
            Assert.Equal((ushort)1, (ushort)ICWSyncResultCodeType.Success);
            Assert.Equal((ushort)10, (ushort)ICWSyncResultCodeType.校验PLC数据异常);
            Assert.Equal((ushort)11, (ushort)ICWSyncResultCodeType.ICW流程异常);
            Assert.Equal((ushort)12, (ushort)ICWSyncResultCodeType.检测软件异常);
            Assert.Equal((ushort)13, (ushort)ICWSyncResultCodeType.MES入站异常);
            Assert.Equal((ushort)14, (ushort)ICWSyncResultCodeType.MES出站异常);

            // ReadEndStatus 把寄存器原始 ushort 强转为枚举(对齐源端 (ICWSyncResultCodeType)Result)
            var node = CreateNode();
            node.IcwAddress.ResponseAddress = "1 03 4100 1";
            node.ResponseScript.Enqueue((ushort)ICWSyncResultCodeType.MES出站异常);
            Assert.True(node.ReadEndStatusPublic(out var code));
            Assert.Equal(ICWSyncResultCodeType.MES出站异常, code);
        }

        /// <summary>
        /// 异常码(非检测软件异常):对齐源端 outer break —— 记 WarningPause 后退出(完成,不中止)。
        /// </summary>
        [Fact]
        public void ICW_OtherErrorCode_FinishesWithWarning()
        {
            var node = CreateNode();

            // MES 入站异常(13)= 非 None / 非 Success / 非检测软件异常 → 源端 outer break 退出
            node.ResponseScript = new Queue<ushort>(new ushort[]
            {
                (ushort)ICWSyncResultCodeType.MES入站异常,
            });

            bool ok = node.DoExcute(out string errMsg);

            // 源端该分支 return true(退出循环),节点视作完成
            Assert.True(ok, errMsg);
            // WarningPause 记录了异常码
            Assert.Contains(node.Warnings, w => w.Contains("MES入站异常"));
        }

        /// <summary>
        /// 检测软件异常:对齐源端 outer continue —— 重试整轮后成功。
        /// 验证 Request 被写入两次(首轮 + 重试轮)。
        /// </summary>
        [Fact]
        public void ICW_SoftwareError_RetriesThenSucceeds()
        {
            var node = CreateNode();
            var a = node.IcwAddress;

            // 首轮 检测软件异常(12)→ 重试;第二轮 Success
            node.ResponseScript = new Queue<ushort>(new ushort[]
            {
                (ushort)ICWSyncResultCodeType.检测软件异常,
                (ushort)ICWSyncResultCodeType.Success,
            });

            bool ok = node.DoExcute(out string errMsg);
            Assert.True(ok, errMsg);

            // 请求位被写 1 至少两次(首轮 + 重试轮,对齐源端 outer continue 回到 ClearStatus→WriteStart)
            int starts = node.Writes.FindAll(w => w.Item1 == a.RequestAddress && w.Item2 == 1).Count;
            Assert.True(starts >= 2, $"检测软件异常应触发整轮重试,Request 应写 ≥2 次,实际 {starts} 次");
            // WarningPause 记录了检测软件异常
            Assert.Contains(node.Warnings, w => w.Contains("检测软件异常"));
        }

        /// <summary>
        /// 测试子类:用寄存器字典 + 响应脚本 + 可控时钟模拟 VModbusServer 读写,
        /// 真实还原源端 Req/Resp 寄存器对往返与心跳。
        /// </summary>
        private class IcwTestNode : HandoverICWNode
        {
            /// <summary>寄存器字典(模拟 VModbusServer DataPool)</summary>
            private readonly Dictionary<string, ushort> _regs = new Dictionary<string, ushort>();

            /// <summary>响应脚本:每次读 ResponseAddress 出队一个值;空则返回 Success</summary>
            public Queue<ushort> ResponseScript = new Queue<ushort>();

            /// <summary>记录所有写操作(address, value)</summary>
            public List<(string, ushort)> Writes = new List<(string, ushort)>();

            /// <summary>WarningPause 记录的消息</summary>
            public List<string> Warnings = new List<string>();

            /// <summary>可控时钟(轮询间隙由 SyncWait 推进)</summary>
            private System.DateTime _clock = new System.DateTime(2026, 6, 22, 0, 0, 0);

            /// <summary>直接置寄存器(测试注入响应码用)</summary>
            public void SetRegister(string address, ushort value) => _regs[address] = value;

            /// <summary>暴露 protected ReadEndStatus 供枚举单测直接调用</summary>
            public bool ReadEndStatusPublic(out ICWSyncResultCodeType code) => ReadEndStatus(out code);

            protected override bool WriteIcwUshort(string address, ushort value)
            {
                if (string.IsNullOrWhiteSpace(address)) return false;
                _regs[address] = value;
                Writes.Add((address, value));
                return true;
            }

            protected override bool ReadIcwUshort(string address, out ushort value)
            {
                value = 0;
                if (string.IsNullOrWhiteSpace(address)) return false;

                // ResponseAddress 走脚本;其余地址回读寄存器字典(还原源端写后回读)
                if (address == IcwAddress.ResponseAddress)
                {
                    value = ResponseScript.Count > 0 ? ResponseScript.Dequeue() : (ushort)ICWSyncResultCodeType.Success;
                    _regs[address] = value;
                    return true;
                }

                value = _regs.TryGetValue(address, out var v) ? v : (ushort)0;
                return true;
            }

            // 轮询间隙推进时钟(模拟真实等待),驱动心跳周期与超时判定
            protected override void SyncWait(int milliseconds)
            {
                _clock = _clock.AddMilliseconds(milliseconds);
            }

            protected override System.DateTime DateTimeNow() => _clock;

            // 测试不依赖 MyOwner,接管日志/告警钩子
            protected override void StepInfo(int step, string message) { }
            protected override void WarningPause(string message) { Warnings.Add(message); }
        }
    }
}
