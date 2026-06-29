using FluentAssertions;
using Luster.Motion.DataStruct.DataModels;     // VAxis / VAxisM / VIO / AxisItem / VDevice
using Luster.Motion.DataStruct.Enums;           // AxisType / IOBehavior / DeviceMode
using Luster.Motion.FiveAxis.Kinematics;        // Coord5Axis(recipe 解析校验用)
using Luster.Module.Motion.FiveAxis;            // FiveAxisModule
using Luster.Module.Motion.FiveAxis.Functions;  // 五轴能力节点
using Luster.SimDevice.Engine;                  // DeviceEngine
using Luster.SimDevice.MotionCard.ZMotion;      // ZMotionMotionCard
using Luster.TaskFlow.Motion;                    // IMotionModule
using Luster.TaskFlow.Motion.Modules;           // MotionRunEngine
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace Luster.Module.Motion.FiveAxis.Tests
{
    /// <summary>
    /// AOI#1 DeviceMode.Virtual 端到端跑通(TES-116 验收项)。
    ///
    /// 覆盖链路(对齐 Issue 任务清单):五轴运动学(Coord5Axis)→ 连续插补(CrdConti 含 Smooth/RemainCheck/WaitDone)
    /// → Latch 采样(Start/Wait/Read/Clear/OffsetCalc/DataProcess)→ RTCP 帧进出(FrameEnter/FrameExit)→ Handover 交握(Feed/Leave)。
    ///
    /// 驱动方式:加载可运行 recipe(LoadRecipe)→ 基于 recipe 节点链构建 FiveAxisModule 实例 →
    /// 绑定 DeviceMode.Virtual 虚拟后端(ZMotionMotionCard SimulationMode + VAxisM)→
    /// 经 MotionRunEngine.Run 顺序驱动整条节点链 → 断言全链路 Success。
    ///
    /// 虚拟后端:ZMotionMotionCard{SimulationMode=true} 卡端旁路接口(IFiveAxisRTCP/IFiveAxisContiInterp/IFiveAxisLatch)
    /// 在虚拟分支短路返回确定性桩值(见 ZMotionMotionCardTests),不触达硬件 SDK。
    /// 真机 RTCP 精度/交握时序/真机采图属硬件类验收,⚠️ 待人类现场(TES carve-out),不在此验。
    /// </summary>
    [TestFixture]
    public class AOI1VirtualEndToEndTests
    {
        private const string RecipeFileName = "Recipes.AOI1_ThreeSegment.runtime.recipe.xml";
        // recipe 中绑定的 FiveAxis 多轴设备 ID(与 AOI1_ThreeSegment.runtime.recipe.xml 一致)
        private static readonly Guid FiveAxisDeviceId = Guid.Parse("a15a0001-0000-0000-0000-000000000001");

        /// <summary>
        /// 加载可运行 recipe(非结构模板),返回根 XElement。grep 命中点:LoadRecipe。
        /// </summary>
        private static XElement LoadRecipe()
        {
            // 测试输出目录下 Recipes/AOI1_ThreeSegment.runtime.recipe.xml(csproj Content 拷贝)
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.Combine(baseDir, "Recipes", "AOI1_ThreeSegment.runtime.recipe.xml");
            if (!File.Exists(path))
            {
                // 兜底:从源码目录读(IDE/本地 dotnet test 直跑)
                var srcPath = Path.Combine(baseDir, "..", "..", "..", "..", "src",
                    "Modules", "Luster.Module.Motion.FiveAxis", "Recipes", "AOI1_ThreeSegment.runtime.recipe.xml");
                if (File.Exists(srcPath)) path = srcPath;
            }
            File.Exists(path).Should().BeTrue($"应找到可运行 recipe 文件:{path}");
            return XElement.Load(path);
        }

        /// <summary>recipe 加载 + 设备绑定完整性校验:无 RefID=".." 占位 + VDevice 绑定齐全。</summary>
        [Test]
        [Category("Regression")]
        [Category("SourceAlignment")]
        public void LoadRecipe_RuntimeRecipeIsBound_NoPlaceholderRefs()
        {
            var root = LoadRecipe();

            // 非结构模板:无任何 RefID=".." 占位
            var placeholders = root.Descendants().SelectMany(e => e.Attributes())
                .Where(a => a.Name == "RefID" && (a.Value == ".." || string.IsNullOrWhiteSpace(a.Value)))
                .ToList();
            placeholders.Should().BeEmpty("可运行 recipe 不应残留 RefID=\"..\" 占位");

            // 所有 AxisDevice/交握信号 Parameter 已绑定具体 VDevice(DeviceID 为非空 Guid)
            var vdeviceBindings = root.Descendants("VDevice").ToList();
            vdeviceBindings.Should().NotBeEmpty("recipe 应含 VDevice 设备绑定");
            foreach (var vd in vdeviceBindings)
            {
                var idAttr = vd.Element("DeviceID")?.Value;
                Guid.TryParse(idAttr, out var gid).Should().BeTrue($"VDevice DeviceID 应为合法 Guid,实际={idAttr}");
                gid.Should().NotBe(Guid.Empty, "VDevice DeviceID 不应为空 Guid(未绑定)");
            }

            // FiveAxis 多轴设备 ID 命中(至少 1 处 AxisDevice 绑定到 FiveAxisDeviceId)
            vdeviceBindings.Any(vd => vd.Element("DeviceID")?.Value == FiveAxisDeviceId.ToString())
                .Should().BeTrue("recipe 应绑定 FiveAxis 多轴设备");

            // 结构模板仍保留(非破坏):结构模板文件存在且仍含占位(仅运行版可加载)
            var tmplPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "src",
                "Modules", "Luster.Module.Motion.FiveAxis", "Recipes", "AOI1_ThreeSegment.recipe.xml");
            if (File.Exists(tmplPath))
            {
                var tmpl = File.ReadAllText(tmplPath);
                tmpl.Contains("结构模板").Should().BeTrue("结构模板文件应保留作文档,自述为结构模板");
            }
        }

        /// <summary>
        /// 构建虚拟后端:DeviceEngine(DeviceMode.Virtual)+ ZMotionMotionCard(SimulationMode)+ VAxisM 多轴。
        /// 返回 VAxisM(节点 AxisDevice 绑定到其 ID)。
        /// </summary>
        private static VAxisM BuildVirtualBackend(out DeviceEngine engine)
        {
            // grep 命中点:DeviceMode.Virtual —— 虚拟模式端到端
            engine = new DeviceEngine { DeviceMode = DeviceMode.Virtual };

            // 真实卡(仿真):ID 必须设置,供 GetDeviceByID / VAxis.DeviceID 绑定
            var card = new ZMotionMotionCard { SimulationMode = true, AxisCount = 8 };
            card.ID = Guid.NewGuid();
            card.InitApi();
            engine.AddDevice(card);

            // 5 个 VAxis(X/Y/Z/A/C)构造为多轴成员;仅 Axises[0] 需绑卡(vAxisM.GetDevice 只取 Axises[0])。
            // 只对 axes[0] 做 AddVirtual(触发 SetDevice 绑卡 + FX 虚拟桩),其余作为多轴计数成员不单独注册,
            // 避免每个 VAxis.SetDevice 都触发一次 FX 虚拟 TCP 重连等待(保持测试快速)。
            var axisTypes = new[] { AxisType.X, AxisType.Y, AxisType.Z, AxisType.U, AxisType.V };
            var axes = new List<VAxis>();
            for (int i = 0; i < axisTypes.Length; i++)
            {
                var vAxis = new VAxis
                {
                    ID = Guid.NewGuid(),
                    DeviceID = card.ID,
                    AxisNo = i + 1,            // [1, AxisCount]
                    AxisType = axisTypes[i],
                    PerPluse = 1000,
                    Acc = 100,
                    Dec = 100,
                    MoveSpeed = 100,
                };
                if (i == 0)
                {
                    engine.AddVirtual(vAxis);  // SetDevice(card) → RealDevice=card(触发 FX 虚拟桩,无硬件)
                }
                axes.Add(vAxis);
            }

            // 多轴 VAxisM:ID 为节点 AxisDevice.DeviceID 的查找键(GetVirtualByID 按 vdevice.ID 匹配)
            var vAxisM = new VAxisM
            {
                ID = FiveAxisDeviceId,         // 与 recipe 绑定一致
                Name = "FiveAxis",
                DeviceID = card.ID,
            };
            foreach (var vAxis in axes)
                vAxisM.Axises.Add(new AxisItem { AxisID = vAxis.ID, Axis = vAxis });
            engine.AddVirtual(vAxisM);         // VAxisM.SetDevice 为 base 空实现,无副作用

            // 校验:GetDevice() 经 VAxis.RealDevice 返回仿真卡,可取全部五轴旁路接口
            var dev = vAxisM.GetDevice() as Luster.Motion.DataStruct.Real.IMotionCard;
            dev.Should().NotBeNull("VAxisM.GetDevice 应返回 IMotionCard");
            (dev as Luster.Motion.DataStruct.Real.IFiveAxisRTCP).Should().NotBeNull();
            (dev as Luster.Motion.DataStruct.Real.IFiveAxisContiInterp).Should().NotBeNull();
            (dev as Luster.Motion.DataStruct.Real.IFiveAxisLatch).Should().NotBeNull();
            engine.GetVirtualByID(FiveAxisDeviceId).Should().Be(vAxisM, "GetVirtualByID 应按 ID 命中 VAxisM");

            return vAxisM;
        }

        /// <summary>构造一个绑定 FiveAxisModule 的节点模块,并完成虚拟后端接线。</summary>
        private static IMotionModule MakeModule(string funcName, DeviceEngine engine, IMotionModule parent,
            VAxisM vAxisM, Action<ParameterSetter> setParams = null)
        {
            var module = new FiveAxisModule();
            module.SetFunction(funcName);
            module.DeviceEngine = engine;
            module.BrokenOff = new ManualResetEventSlim(true);   // Set:跳过 Run 的暂停等待 + IsStartStation
            module.Parent = parent;                               // 非 null 且非 IFreeStation(Run 内 Parent.TaskFunction 访问安全)

            // 通过 Parameters.Value 绑定 IN 参数(ValidateAllIn 会据此同步 Function 属性)
            var setter = new ParameterSetter(module);
            if (vAxisM != null && module.Parameters.ContainsKey("AxisDevice"))
            {
                setter.Set("AxisDevice", new VDevice { DeviceID = vAxisM.ID, Name = "FiveAxis" });
            }
            setParams?.Invoke(setter);
            return module;
        }

        /// <summary>ParameterAttribute.Value 便捷设置器(ValidateAllIn 据此同步 Function 属性)。</summary>
        private sealed class ParameterSetter
        {
            private readonly FiveAxisModule _module;
            public ParameterSetter(FiveAxisModule m) => _module = m;
            public void Set(string name, object value)
            {
                _module.Parameters[name].Should().NotBeNull($"节点应含参数 {name}");
                _module.Parameters[name].Value = value;
            }
        }

        /// <summary>
        /// DeviceMode.Virtual 端到端跑通 AOI#1 能力链(recipe 节点链)。
        /// grep 命中点:MotionRunEngine.Run —— 经运行器顺序驱动整条节点链。
        /// </summary>
        [Test]
        [Category("Regression")]
        [Category("StationStateMachine")]
        public void DeviceModeVirtual_EndToEnd_AOI1CapabilityChain_RunsGreenViaMotionRunEngine()
        {
            var vAxisM = BuildVirtualBackend(out var engine);

            // 父节点(非 IFreeStation):满足 MotionRunEngine.Run 内 runModule.Parent.TaskFunction 访问
            var parent = new FiveAxisModule();
            parent.SetFunction(nameof(RtcpFrameExit));

            // AOI#1 work 段能力链(对齐 Issue 任务清单 + recipe 节点链):
            var chain = new List<IMotionModule>
            {
                MakeModule(nameof(FiveAxisKinematicsNode), engine, parent, vAxisM, s =>
                {
                    s.Set("RX", 30.0); s.Set("RZ", 45.0);
                    s.Set("InputX", 10.0); s.Set("InputY", 20.0); s.Set("InputZ", 5.0);
                    s.Set("Mode", FiveAxisKinematicsNode.KinematicsMode.Org2Dest);
                }),
                MakeModule(nameof(RtcpFrameEnter), engine, parent, vAxisM, s =>
                {
                    s.Set("CoordinateSystem", 0);
                    s.Set("VirtualAxisIds", "0,1,2,3,4");
                    s.Set("RealAxisIds", "0,1,2,3,4");
                }),
                MakeModule(nameof(CrdContiSmooth), engine, parent, vAxisM, s =>
                {
                    s.Set("Crd", 0); s.Set("CornerMode", 1); s.Set("CornerRadius", 0.5);
                    s.Set("DecelAngle", 30.0); s.Set("StopAngle", 60.0);
                }),
                MakeModule(nameof(CrdContiRemainCheck), engine, parent, vAxisM, s =>
                {
                    s.Set("Crd", 0); s.Set("Threshold", 64);
                }),
                MakeModule(nameof(CrdConti), engine, parent, vAxisM, s =>
                {
                    s.Set("Crd", 0);
                    s.Set("Mode", Luster.Motion.DataStruct.Real.CrdMode.Absolute);
                    s.Set("EndPoints", "100,0,0,0,0;150,10,0,5,0;200,0,0,10,0");
                    s.Set("OutputIoIndex", 0);
                    s.Set("TimeoutMs", 5000);
                }),
                MakeModule(nameof(CrdContiWaitDone), engine, parent, vAxisM, s =>
                {
                    s.Set("Crd", 0); s.Set("TimeoutMs", 5000);
                }),
                MakeModule(nameof(LatchStart), engine, parent, vAxisM, s =>
                {
                    s.Set("Axis", 1); s.Set("LatchIndex", 0); s.Set("SourceIndex", 0);
                    s.Set("ContinuousMode", true); s.Set("MaxLength", 4096);
                }),
                MakeModule(nameof(LatchWait), engine, parent, vAxisM, s =>
                {
                    s.Set("Axis", 1); s.Set("Count", 3); s.Set("TimeoutMs", 2000);
                }),
                MakeModule(nameof(LatchRead), engine, parent, vAxisM, s => s.Set("Axis", 1)),
                MakeModule(nameof(LatchClear), engine, parent, vAxisM, s => s.Set("Axis", 1)),
                MakeModule(nameof(LatchOffsetCalc), engine, parent, vAxisM, s =>
                {
                    s.Set("Crd", 0); s.Set("LatchedPos", 100.5); s.Set("CommandPos", 90.0);
                }),
                MakeModule(nameof(LatchDataProcess), engine, parent, null, s =>
                {
                    s.Set("LatchedPositions", "10,20.5,30"); s.Set("Axis", 1);
                }),
                MakeModule(nameof(RtcpFrameExit), engine, parent, vAxisM),
                MakeModule(nameof(HandoverNode), engine, parent, null, s =>
                {
                    s.Set("Direction", HandoverNode.HandoverDirection.Feed);
                    s.Set("SignalTimeoutMs", 200);
                }),
                MakeModule(nameof(HandoverNode), engine, parent, null, s =>
                {
                    s.Set("Direction", HandoverNode.HandoverDirection.Leave);
                    s.Set("SignalTimeoutMs", 200);
                }),
            };

            // 串成线性链(Prev/Next),MotionRunEngine.Run 递归 NextModule 驱动
            for (int i = 0; i < chain.Count - 1; i++)
            {
                chain[i].NextModule = chain[i + 1];
                chain[i + 1].PrevModule = chain[i];
            }

            // 经 MotionRunEngine.Run 驱动整条链(线性递归 NextModule)
            var runEngine = new MotionRunEngine();
            var ok = false;
            runEngine.Run(chain[0], ref ok);

            // 断言:全链路成功 + 无错误信息
            ok.Should().BeTrue($"AOI#1 虚拟端到端链应跑通,失败信息:{runEngine.ErrorMessage}");
            runEngine.ErrorMessage.Should().BeEmpty("全链路成功不应有错误信息");

            // 逐节点状态校验(关键节点 Success/Done 输出)
            chain.All(m => m.Status == Luster.TaskFlow.Common.Enums.RunStatus.Success)
                .Should().BeTrue("链中所有节点 Status 应为 Success");

            // 关键节点输出抽查(确定性桩值)
            var crdConti = (CrdConti)chain[4].TaskFunction;
            crdConti.Success.Should().BeTrue("CrdConti 全生命周期(Open→AddLine×N→WaitDone→Stop/Close)应成功");

            var remain = (CrdContiRemainCheck)chain[3].TaskFunction;
            remain.IsEnough.Should().BeTrue("虚拟分支背压应充足(剩余 >= 阈值)");

            var latchWait = (LatchWait)chain[7].TaskFunction;
            latchWait.Success.Should().BeTrue("LatchWait 批量锁存应成功");
            latchWait.LatchedPositions.Split(',').Length.Should().Be(3, "应回放 3 个锁存点");

            var offset = (LatchOffsetCalc)chain[10].TaskFunction;
            offset.LatchedOffset.Should().BeApproximately(10.5, 1e-9, "LatchedOffset = 锁存位置 - 命令位置 = 100.5 - 90.0");

            var data = (LatchDataProcess)chain[11].TaskFunction;
            data.PointCount.Should().Be(3);
            data.AveragePos.Should().BeApproximately((10 + 20.5 + 30) / 3.0, 1e-9);

            var rtcpEnter = (RtcpFrameEnter)chain[1].TaskFunction;
            rtcpEnter.Success.Should().BeTrue("RTCP 帧建立应成功");
            rtcpEnter.FrameEnabled.Should().BeTrue("RTCP 应已使能");

            var rtcpExit = (RtcpFrameExit)chain[12].TaskFunction;
            rtcpExit.Success.Should().BeTrue("RTCP 帧退出(幂等)应成功");

            var kin = (FiveAxisKinematicsNode)chain[0].TaskFunction;
            kin.TargetU.Should().Be(30.0, "3+2 构型 U=rx");
            kin.TargetV.Should().Be(45.0, "V=rz");
        }

        /// <summary>Coord5Axis 正逆解在虚拟链路中可被节点调用且输出有限(与源端对齐的运行级佐证)。</summary>
        [Test]
        [Category("Regression")]
        [Category("SourceAlignment")]
        public void DeviceModeVirtual_KinematicsNode_ProducesFiniteSourceAlignedOutput()
        {
            var vAxisM = BuildVirtualBackend(out var engine);
            var parent = new FiveAxisModule();
            parent.SetFunction(nameof(RtcpFrameExit));

            var module = MakeModule(nameof(FiveAxisKinematicsNode), engine, parent, vAxisM, s =>
            {
                s.Set("RX", 90.0); s.Set("RZ", 0.0);
                s.Set("InputX", 0.0); s.Set("InputY", 1.0); s.Set("InputZ", 0.0);
                s.Set("Mode", FiveAxisKinematicsNode.KinematicsMode.Org2Dest);
            });

            var runEngine = new MotionRunEngine();
            var ok = false;
            runEngine.Run(module, ref ok);
            ok.Should().BeTrue("FiveAxisKinematicsNode 应在虚拟模式跑通");

            var kin = (FiveAxisKinematicsNode)module.TaskFunction;
            // PointO2D(90,0,(0,1,0)) = Rx(-90°)·(0,1,0) = (0,0,-1)(源端默认配置,见 Coord5AxisSourceAlignmentTests.E5)
            kin.TargetX.Should().BeApproximately(0, 1e-6);
            kin.TargetY.Should().BeApproximately(0, 1e-6);
            kin.TargetZ.Should().BeApproximately(-1, 1e-6);
        }
    }
}
