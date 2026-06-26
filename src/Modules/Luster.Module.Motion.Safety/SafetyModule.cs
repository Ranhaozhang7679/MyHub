using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Module.Motion.Safety.Functions;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;

namespace Luster.Module.Motion.Safety
{
    /// <summary>
    /// 安全/互锁模块（TES-38）。
    /// 注册 <see cref="CheckSafety"/> / <see cref="CheckInterlock"/> 运控功能节点，
    /// 并提供 <see cref="InterlockMatrix"/> / <see cref="IInputSnapshot"/> 工厂的共享注册表，
    /// 供节点在 <c>DoExcute</c> 中按名查找。零侵入 Shell：卸载本模块 DLL 后平台标准运控不受影响。
    /// </summary>
    public class SafetyModule : MotionModule
    {
        /// <summary>互锁矩阵注册表（按名查找）</summary>
        private static readonly Dictionary<string, InterlockMatrix> _matrices = new Dictionary<string, InterlockMatrix>();

        /// <summary>输入快照工厂注册表（按名查找，工厂接收宿主 IMotionModule 以读取设备）</summary>
        private static readonly Dictionary<string, Func<IMotionModule, IInputSnapshot>> _snapshotFactories
            = new Dictionary<string, Func<IMotionModule, IInputSnapshot>>();

        static SafetyModule()
        {
            // 默认注册 IOInput 快照工厂：通过 DeviceEngine.GetVirtualByName 解析 VIO/VAxis
            RegisterSnapshotFactory("IOInput", module => new InputSnapshotAdapter(module));
        }

        public override void InitFunctions()
        {
            AddFunction<CheckSafety>();
            AddFunction<CheckInterlock>();
        }

        /// <summary>注册互锁矩阵</summary>
        public static void RegisterMatrix(string name, InterlockMatrix matrix)
        {
            if (string.IsNullOrEmpty(name) || matrix == null) return;
            _matrices[name] = matrix;
        }

        /// <summary>注册输入快照工厂（站级可覆盖默认 IOInput）</summary>
        public static void RegisterSnapshotFactory(string name, Func<IMotionModule, IInputSnapshot> factory)
        {
            if (string.IsNullOrEmpty(name) || factory == null) return;
            _snapshotFactories[name] = factory;
        }

        /// <summary>查找已注册的互锁矩阵</summary>
        public static InterlockMatrix LookupMatrix(string name)
            => string.IsNullOrEmpty(name) || !_matrices.TryGetValue(name, out var m) ? null : m;

        /// <summary>查找并构造输入快照</summary>
        public static IInputSnapshot LookupSnapshot(string name, IMotionModule module)
            => string.IsNullOrEmpty(name) || !_snapshotFactories.TryGetValue(name, out var f) ? null : f(module);
    }

    /// <summary>模块创建器（被 IModuleFactory.LoadModules 反射发现）</summary>
    public class SafetyModuleCreator : MotionModuleCreator<SafetyModule>
    {
        public override int Sort => 5;

        public override string Icon => "\xe728";
    }
}
