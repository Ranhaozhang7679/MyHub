using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Module.Motion.Safety.Functions;
using Luster.Module.Motion.Safety.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;

namespace Luster.Module.Motion.Safety
{
    /// <summary>
    /// 安全/互锁模块（TES-38）。
    /// 注册 <see cref="CheckSafety"/> / <see cref="CheckInterlock"/> 运控功能节点，
    /// 并提供 <see cref="InterlockMatrix"/> / 条件解析器的共享注册表，
    /// 供节点在 <c>DoExcute</c> 中按名查找。零侵入 Shell：卸载本模块 DLL 后平台标准运控不受影响。
    /// </summary>
    public class SafetyModule : MotionModule
    {
        /// <summary>互锁矩阵注册表（按名查找）</summary>
        private static readonly Dictionary<string, InterlockMatrix> _matrices = new Dictionary<string, InterlockMatrix>();

        /// <summary>条件解析器工厂注册表（按名查找，工厂接收宿主 IMotionModule 以读取设备）</summary>
        private static readonly Dictionary<string, Func<IMotionModule, Func<InterlockCondition, bool>>> _resolvers
            = new Dictionary<string, Func<IMotionModule, Func<InterlockCondition, bool>>>();

        static SafetyModule()
        {
            // 默认注册一个 IO 输入解析器：condition.Target 视为 VIO 设备名，读 GetDigitalIn 与 Expected 比较
            RegisterResolver("IOInput", module => condition =>
            {
                if (module == null || string.IsNullOrEmpty(condition.Target)) return false;
                try
                {
                    var vd = module.DeviceEngine?.GetVirtualByName(condition.Target);
                    if (vd is VIO vio)
                    {
                        bool actual = vio.GetDigitalIn();
                        return bool.TryParse(condition.Expected, out var expect) && actual == expect;
                    }
                }
                catch
                {
                    // 设备解析失败视为条件不成立，避免单点异常导致互锁误触发
                    return false;
                }
                return false;
            });
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

        /// <summary>注册条件解析器工厂</summary>
        public static void RegisterResolver(string name, Func<IMotionModule, Func<InterlockCondition, bool>> factory)
        {
            if (string.IsNullOrEmpty(name) || factory == null) return;
            _resolvers[name] = factory;
        }

        /// <summary>查找已注册的互锁矩阵</summary>
        public static InterlockMatrix LookupMatrix(string name)
            => string.IsNullOrEmpty(name) || !_matrices.TryGetValue(name, out var m) ? null : m;

        /// <summary>查找并构造条件解析器</summary>
        public static Func<InterlockCondition, bool> LookupResolver(string name, IMotionModule module)
            => string.IsNullOrEmpty(name) || !_resolvers.TryGetValue(name, out var f) ? null : f(module);
    }

    /// <summary>模块创建器（被 IModuleFactory.LoadModules 反射发现）</summary>
    public class SafetyModuleCreator : MotionModuleCreator<SafetyModule>
    {
        public override int Sort => 5;

        public override string Icon => "\xe728";
    }
}
