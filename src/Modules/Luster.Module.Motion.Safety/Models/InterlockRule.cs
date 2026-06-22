using Luster.Motion.DataStruct.Enums;
using Luster.Module.Motion.Safety.Network;
using System;
using System.Collections.Generic;

namespace Luster.Module.Motion.Safety.Models
{
    /// <summary>
    /// 互锁规则：输入 IO 条件 → 输出 IO 动作 + 报警。
    /// 抽象自源端 <c>CheckStationTask.BuildSafeCondition</c> /
    /// <c>GetLoadAxisPosint</c> / <c>GetUnLoadAxisPosint</c> 的散点互锁判定，
    /// 配置化后可挂在 <c>HandoverNode</c>（TES-37 产出）上随交接流程求值。
    /// </summary>
    public class InterlockRule
    {
        /// <summary>规则名（便于配置追溯）</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>规则类别</summary>
        public AlarmCategory Category { get; set; } = AlarmCategory.Interlock;

        /// <summary>触发的报警契约（绑恢复策略/锁存/抑制）</summary>
        public AlarmSchema Alarm { get; set; } = new AlarmSchema();

        /// <summary>输入条件集合，全部满足才视为互锁触发</summary>
        public List<InterlockCondition> Conditions { get; set; } = new List<InterlockCondition>();

        /// <summary>触发时要执行的输出 IO 动作集合</summary>
        public List<InterlockAction> Actions { get; set; } = new List<InterlockAction>();

        /// <summary>是否启用</summary>
        public bool Enable { get; set; } = true;

        /// <summary>
        /// 求值：所有 <see cref="Conditions"/> 是否全部成立。
        /// condition 的求值回调由调用方（InterlockMatrix/HandoverNode）注入，
        /// 避免本数据模型直接耦合 VIO/设备解析。
        /// </summary>
        public bool Evaluate(Func<InterlockCondition, bool> resolver)
        {
            if (!Enable || Conditions.Count == 0) return false;
            foreach (var cond in Conditions)
            {
                if (!resolver(cond)) return false;
            }
            return true;
        }
    }

    /// <summary>互锁输入条件</summary>
    public class InterlockCondition
    {
        /// <summary>IO 设备名 / 轴名 / 握手信号位</summary>
        public string Target { get; set; } = string.Empty;

        /// <summary>条件类型</summary>
        public InterlockConditionType Type { get; set; } = InterlockConditionType.IOInput;

        /// <summary>期望值（IO=true/轴位=double/握手位=HandshakeBit）</summary>
        public string Expected { get; set; } = bool.TrueString;

        /// <summary>比较运算（仅轴位类生效）</summary>
        public InterlockCompare Compare { get; set; } = InterlockCompare.Equal;
    }

    /// <summary>互锁输出动作</summary>
    public class InterlockAction
    {
        /// <summary>输出 IO 设备名</summary>
        public string Target { get; set; } = string.Empty;

        /// <summary>动作类型</summary>
        public InterlockActionType Type { get; set; } = InterlockActionType.SetOutput;

        /// <summary>动作参数（输出值/暂停/急停等）</summary>
        public string Argument { get; set; } = bool.TrueString;
    }

    public enum InterlockConditionType
    {
        /// <summary>数字输入 IO</summary>
        IOInput = 0,
        /// <summary>数字输出 IO</summary>
        IOOutput = 1,
        /// <summary>轴位（比较位置）</summary>
        AxisPosition = 2,
        /// <summary>轴状态位（限位/伺服/急停）</summary>
        AxisStatus = 3,
        /// <summary>握手信号位（上下游互锁）</summary>
        HandshakeBit = 4
    }

    public enum InterlockActionType
    {
        /// <summary>置输出 IO</summary>
        SetOutput = 0,
        /// <summary>暂停流程</summary>
        Pause = 1,
        /// <summary>急停</summary>
        EmergencyStop = 2,
        /// <summary>触发报警（走 AlarmSchema）</summary>
        RaiseAlarm = 3,
        /// <summary>清零握手信号</summary>
        ResetHandshake = 4
    }

    public enum InterlockCompare
    {
        Equal = 0,
        NotEqual = 1,
        Greater = 2,
        Less = 3,
        GreaterOrEqual = 4,
        LessOrEqual = 5
    }
}
