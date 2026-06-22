using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;

namespace Luster.Motion.DataStruct.DataModels
{
    /// <summary>
    /// 联锁矩阵单条规则（ADR-C seam，TES-38 产出 / TES-28 P0-G 消费）。
    /// 输入条件按 <see cref="JudgeMode"/> 聚合(All=AND / Any=OR) → 触发动作 + 报警码 + 恢复策略。
    /// 对齐架构师 ADR-C 绑定契约：<c>RuleId / Inputs / Actions / AlarmCode / Recovery</c>。
    /// </summary>
    /// <remarks>
    /// 本类不持有 <c>AlarmSchema</c>（报警完整契约在业务层），只持 <see cref="AlarmCode"/> 字符串，
    /// TES-28 通过 <c>AlarmCode</c> 向 <c>ErrorManager</c>/<c>TbAlarm</c> 查询完整报警内容。
    /// 这样框架层不反向依赖业务模块，三方解耦。
    /// </remarks>
    public sealed class InterlockRule
    {
        /// <summary>规则标识（配置追溯）</summary>
        public string RuleId { get; set; } = string.Empty;

        /// <summary>输入条件集合，按 <see cref="JudgeMode"/> 聚合</summary>
        public InterlockInput[] Inputs { get; set; } = new InterlockInput[0];

        /// <summary>输入条件聚合模式，默认 All(全部满足)。Any=任一满足(OR)</summary>
        public InterlockJudgeMode JudgeMode { get; set; } = InterlockJudgeMode.All;

        /// <summary>触发时要执行的动作集合（停轴/锁机/写 PLC/...）</summary>
        public InterlockAction[] Actions { get; set; } = new InterlockAction[0];

        /// <summary>绑定的报警码（可空=只联锁不报警）</summary>
        public string AlarmCode { get; set; } = string.Empty;

        /// <summary>恢复策略（喂 ADR-C / P0-G 恢复闭环）</summary>
        public RecoveryPolicy Recovery { get; set; } = RecoveryPolicy.Manual;

        /// <summary>是否启用</summary>
        public bool Enable { get; set; } = true;

        /// <summary>
        /// 求值：按 <see cref="JudgeMode"/> 聚合 <see cref="Inputs"/>。
        /// <para>All：所有条件成立才触发（源端 BuildSafeCondition 默认语义）。</para>
        /// <para>Any：任一条件成立即触发（对齐源端"上游或下游载台不在安全位置"等 OR 判定）。</para>
        /// </summary>
        public bool Evaluate(IInputSnapshot snapshot)
        {
            if (!Enable || Inputs == null || Inputs.Length == 0) return false;

            if (JudgeMode == InterlockJudgeMode.Any)
            {
                // 任一条件成立即触发
                foreach (var input in Inputs)
                {
                    bool actual = snapshot?.IsTriggered(input.Kind, input.Target) ?? false;
                    if (actual == input.Expected) return true;
                }
                return false;
            }

            // All：全部成立才触发
            foreach (var input in Inputs)
            {
                bool actual = snapshot?.IsTriggered(input.Kind, input.Target) ?? false;
                if (actual != input.Expected) return false;
            }
            return true;
        }
    }

    /// <summary>联锁输入条件</summary>
    public sealed class InterlockInput
    {
        /// <summary>输入维度</summary>
        public SafetyInputKind Kind { get; set; }

        /// <summary>目标对象名（轴名/IO 设备名/上下游站点名，可空=默认对象）</summary>
        public string Target { get; set; } = string.Empty;

        /// <summary>期望值：true=该维度触发时条件成立，false=该维度未触发时条件成立</summary>
        public bool Expected { get; set; } = true;
    }

    /// <summary>联锁输出动作</summary>
    public sealed class InterlockAction
    {
        /// <summary>动作目标（输出 IO 设备名/轴名）</summary>
        public string Target { get; set; } = string.Empty;

        /// <summary>动作类型</summary>
        public InterlockActionType Type { get; set; } = InterlockActionType.SetOutput;

        /// <summary>动作参数（输出值/暂停/急停等）</summary>
        public string Argument { get; set; } = bool.TrueString;
    }

    /// <summary>联锁动作类型</summary>
    public enum InterlockActionType
    {
        [Description("置输出IO")]
        SetOutput = 0,
        [Description("暂停")]
        Pause = 1,
        [Description("急停")]
        EmergencyStop = 2,
        [Description("触发报警")]
        RaiseAlarm = 3,
        [Description("清零握手")]
        ResetHandshake = 4
    }
}
