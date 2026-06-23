using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.SimDevice.Engine;
using Luster.SimDevice.MotionCard.ZMotion;
using Luster.TaskFlow.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
// HomeProfile 既是命名空间又是节点类型名,用别名消歧
using HomeProfileNode = Luster.Module.Motion.HomeProfile.Functions.HomeProfile;
using HomeSafetyCheckNode = Luster.Module.Motion.HomeProfile.Functions.HomeSafetyCheck;
using AxisInitVerifierNode = Luster.Module.Motion.HomeProfile.Functions.AxisInitVerifier;

namespace Luster.Module.Motion.HomeProfileTests
{
    /// <summary>
    /// TES-39 P7-D 引擎端到端冒烟(PM 重点核实项:不接受仅纯逻辑单测)。
    ///
    /// 在真实 DeviceEngine + 真实 ZMotionMotionCard(模拟模式) + 真实 VAxis 绑定环境下,
    /// 跑通 HomeProfile/HomeSafetyCheck/AxisInitVerifier 节点的 DoExcute 真实链路:
    ///   param 反射注册 Owner.Parameters(PM 关注项①)
    ///   → GetVDevice 经 DeviceEngine 解析 VAxis
    ///   → axis.Home()→CheckHomeDone 回零闭环(PM 关注项②,模拟模式真跑)
    ///   → HomeSafetyCheck 真实读 axis.GetCurrentPos 安全位前置(PM 关注项②)
    ///   → AxisInitVerifier 真实校验 Engine + 轴通信
    /// 回零顺序(Z→Y→Rx→Rz 串行)以多节点顺序 DoExcute 表达。
    ///
    /// 完整 recipe Group/AsyncGroup 编排依赖 MotionRunEngine+WPF Prism,非单测可拉起,
    /// 不在本测试范围(真机 recipe 链端到端 ⚠️ 待人类现场);本测试覆盖软件层可验的
    /// 节点→DeviceEngine→VAxis→ZMotion卡 闭环。
    /// </summary>
    public class HomeProfileEngineSmokeTests
    {
        /// <summary>构造真实 DeviceEngine + 一张 ZMotion 模拟卡 + 多个 VAxis 绑定。</summary>
        private static (DeviceEngine engine, ZMotionMotionCard card, List<VAxis> axes) BuildSimEngine(int axisCount)
        {
            var engine = new DeviceEngine();
            var card = new ZMotionMotionCard { SimulationMode = true, AxisCount = axisCount };
            card.ID = Guid.NewGuid();   // DeviceBase.ID 默认 Empty,需显式设非空 ID 才能被 GetDeviceByID 命中并完成 VAxis 绑定
            card.InitApi();
            engine.AddDevice(card);

            var axes = new List<VAxis>();
            for (int i = 1; i <= axisCount; i++)
            {
                var axis = new VAxis
                {
                    Name = $"Axis{i}",
                    AxisNo = i,
                    PerPluse = 1000,
                    AxisType = AxisType.X,
                    HomeMode = HomeMode.CurrentHome,
                    DeviceID = card.ID,
                };
                axis.ID = Guid.NewGuid();
                engine.AddVirtual(axis);   // AddVirtual 会 SetDevice 绑定 card
                axes.Add(axis);
            }

            // Empty 模式:VAxis.ProcessAction 走真实卡路径(模拟卡 SimulationMode 内部不碰硬件)
            engine.SetEngineMode(DeviceMode.Empty, out _);
            return (engine, card, axes);
        }

        [Fact]
        public void HomeProfile节点_param反射注册进OwnerParameters()
        {
            var (engine, _, _) = BuildSimEngine(1);
            var module = new Luster.Module.Motion.HomeProfile.HomeProfileModule { DeviceEngine = engine };
            module.SetFunction("HomeProfile");

            // [Parameter] 经 InitParameters 反射注册进 Owner.Parameters(PM 关注项①:param 绑定)
            Assert.True(module.Parameters.ContainsKey(nameof(HomeProfileNode.Device)));
            Assert.True(module.Parameters.ContainsKey(nameof(HomeProfileNode.HomeMode)));
            Assert.True(module.Parameters.ContainsKey(nameof(HomeProfileNode.SearchDirection)));
            Assert.True(module.Parameters.ContainsKey(nameof(HomeProfileNode.HomeHighEffect)));
            Assert.True(module.Parameters.ContainsKey(nameof(HomeProfileNode.ReScanEnable)));
            Assert.True(module.Parameters.ContainsKey(nameof(HomeProfileNode.RetSwOffset)));
            Assert.True(module.Parameters.ContainsKey(nameof(HomeProfileNode.HomeTimeout)));
            Assert.True(module.Parameters.ContainsKey(nameof(HomeProfileNode.IsHomeDone)));
        }

        [Fact]
        public void HomeProfile节点_端到端回零闭环_模拟模式()
        {
            var (engine, card, axes) = BuildSimEngine(1);
            var module = new Luster.Module.Motion.HomeProfile.HomeProfileModule { DeviceEngine = engine };
            module.SetFunction("HomeProfile");
            var axis = axes[0];

            var node = (HomeProfileNode)module.TaskFunction;
            node.Device = new VDevice { Name = axis.Name, DeviceID = axis.ID };
            node.HomeMode = HomeMode.CurrentHome;
            node.OverrideAxisParams = true;
            node.HomeSpeedHigh = 10;
            node.HomeSpeedLow = 2;
            node.HomeAcc = 100;
            node.HomeOffset = 0f;
            node.HomeTimeout = 5;
            node.CheckDone = true;

            bool ok = node.DoExcute(out string errMsg);

            Assert.True(ok, $"回零应成功,errMsg={errMsg}");
            Assert.True(node.IsHomeDone);
            Assert.True(axis.IsHome);   // VAxis 回零完成标志置位
        }

        [Fact]
        public void HomeSafetyCheck节点_安全位前置_真实读轴位置触发()
        {
            var (engine, card, axes) = BuildSimEngine(1);
            var module = new Luster.Module.Motion.HomeProfile.HomeProfileModule { DeviceEngine = engine };
            var axis = axes[0];

            // 先把轴位置设到安全区内(≤ 安全位阈值 100)
            axis.SetCurrentPos(50);

            module.SetFunction("HomeSafetyCheck");
            var safeNode = (HomeSafetyCheckNode)module.TaskFunction;
            safeNode.Axes = new List<VDevice> { new VDevice { Name = axis.Name, DeviceID = axis.ID } };
            safeNode.SafePosition = 100;
            safeNode.LessOrEqual = true;
            safeNode.Enable = true;

            bool ok = safeNode.DoExcute(out string errMsg);
            Assert.True(ok, $"安全位内应通过,errMsg={errMsg}");

            // 把轴位置设到安全区外(150 > 100)→ 应拦截回零
            axis.SetCurrentPos(150);
            module.SetFunction("HomeSafetyCheck");
            var safeNode2 = (HomeSafetyCheckNode)module.TaskFunction;
            safeNode2.Axes = new List<VDevice> { new VDevice { Name = axis.Name, DeviceID = axis.ID } };
            safeNode2.SafePosition = 100;
            safeNode2.LessOrEqual = true;
            safeNode2.Enable = true;
            bool ok2 = safeNode2.DoExcute(out string errMsg2);
            Assert.False(ok2);
            Assert.Contains("不在安全位", errMsg2);
        }

        [Fact]
        public void AxisInitVerifier节点_校验引擎已初始化且轴可通信()
        {
            var (engine, card, axes) = BuildSimEngine(2);
            var module = new Luster.Module.Motion.HomeProfile.HomeProfileModule { DeviceEngine = engine };
            module.SetFunction("AxisInitVerifier");

            var node = (AxisInitVerifierNode)module.TaskFunction;
            node.Axes = axes.Select(a => new VDevice { Name = a.Name, DeviceID = a.ID }).ToList();
            node.RequireHomed = false;

            bool ok = node.DoExcute(out string errMsg);
            Assert.True(ok, $"初始化校验应通过,errMsg={errMsg}");
        }

        [Fact]
        public void 回零顺序链_多轴串行DoExcute闭环_Z_Y_Rx_Rz()
        {
            // 表达源端回零顺序(Z→Y→Rx→Rz 串行):多个 HomeProfile 节点顺序 DoExcute,
            // 每个轴独立回零闭环。完整 Group/AsyncGroup 编排依赖 MotionRunEngine,非本测试范围。
            var (engine, card, axes) = BuildSimEngine(4);
            var module = new Luster.Module.Motion.HomeProfile.HomeProfileModule { DeviceEngine = engine };

            foreach (var axis in axes)
            {
                module.SetFunction("HomeProfile");
                var node = (HomeProfileNode)module.TaskFunction;
                node.Device = new VDevice { Name = axis.Name, DeviceID = axis.ID };
                node.HomeMode = HomeMode.CurrentHome;
                node.OverrideAxisParams = true;
                node.HomeTimeout = 5;
                node.CheckDone = true;
                bool ok = node.DoExcute(out string errMsg);
                Assert.True(ok, $"轴 {axis.Name} 回零应成功,errMsg={errMsg}");
                Assert.True(axis.IsHome, $"轴 {axis.Name} 应已回零");
            }

            // 全部轴回零后,AxisInitVerifier 校验 RequireHomed 应通过
            module.SetFunction("AxisInitVerifier");
            var verifier = (AxisInitVerifierNode)module.TaskFunction;
            verifier.Axes = axes.Select(a => new VDevice { Name = a.Name, DeviceID = a.ID }).ToList();
            verifier.RequireHomed = true;
            Assert.True(verifier.DoExcute(out _), "全部回零后 RequireHomed 校验应通过");
        }
    }
}
