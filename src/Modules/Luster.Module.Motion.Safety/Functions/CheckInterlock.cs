using Luster.Module.Motion.Safety.Models;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System.ComponentModel;

namespace Luster.Module.Motion.Safety.Functions
{
    /// <summary>
    /// 上下游互锁检查节点（TES-38，InterlockMatrix 子项）。
    /// 求值一组配置化的 <see cref="InterlockRule"/>，任一触发即上报对应 <see cref="AlarmSchema"/> 并返回 false。
    /// 条件求值回调 <see cref="ConditionResolverName"/> 指向宿主注入的解析器名（默认走 IO 输入读取），
    /// 握手位类条件（InterlockConditionType.HandshakeBit）等 TES-37 的 HandoverNode 落地后挂载。
    /// </summary>
    public class CheckInterlock : MotionFunction
    {
        /// <summary>互锁矩阵配置名（指向已加载的 InterlockMatrix）</summary>
        [Parameter("互锁矩阵配置名（启动时由 AlarmMatrixLoader/InterlockMatrix 加载）", 0, CN = "互锁矩阵名", DefaultV = "Default")]
        public string MatrixName { get; set; } = "Default";

        /// <summary>条件求值解析器名（宿主注入，缺省走 IO 输入）</summary>
        [Parameter("条件求值解析器名", 1, CN = "解析器", DefaultV = "IOInput")]
        public string ConditionResolverName { get; set; } = "IOInput";

        public CheckInterlock()
        {
            this.Tips = "上下游互锁检查";
            this.Icon = "\xe729";
        }

        public override string[] NoteParams { get; set; } = new[] { nameof(MatrixName) };

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;
            // 互锁矩阵由 SafetyModule 在初始化时注册到共享服务（TES-28 接入后由 MotionController 注入）。
            // 当前阶段：节点提供执行骨架与配置入口，矩阵实例解析走 MyOwner.DeviceEngine 共享服务。
            // 待 TES-37 HandoverNode 落地后，握手位条件在此处接入。
            var matrix = SafetyModule.LookupMatrix(MatrixName);
            if (matrix == null)
            {
                // 矩阵未配置不阻断流程（仅提示），避免配置缺失导致全线卡死
                OnAlarm(Luster.Motion.DataStruct.Enums.AlarmType.InfoTip,
                    $"互锁矩阵 {MatrixName} 未加载，跳过互锁检查", "INTERLOCK_SKIP");
                return base.DoExcute(out errMsg);
            }

            var resolver = SafetyModule.LookupResolver(ConditionResolverName, MyOwner);
            if (resolver == null)
            {
                OnAlarm(Luster.Motion.DataStruct.Enums.AlarmType.WarningTip,
                    $"互锁解析器 {ConditionResolverName} 未注册", "INTERLOCK_NO_RESOLVER");
                errMsg = "互锁解析器缺失";
                return false;
            }

            var triggered = matrix.Evaluate(resolver);
            if (triggered.Count == 0)
            {
                return base.DoExcute(out errMsg);
            }

            // 上报最高级别触发项
            var top = triggered[0];
            OnAlarm(top.PlatformAlarmType,
                $"[互锁]{top.Message}({top.Code})@{top.Source}", top.ToAlarmCode());
            errMsg = $"互锁触发：{top.Message}";
            return false;
        }
    }
}
