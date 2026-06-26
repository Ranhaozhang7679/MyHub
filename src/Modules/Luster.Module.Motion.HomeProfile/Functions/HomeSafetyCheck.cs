using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using System.Collections.Generic;

namespace Luster.Module.Motion.HomeProfile.Functions
{
    /// <summary>
    /// 回零前安全检查节点（TES-39 P7-D）。
    /// 对齐源端 <c>CheckNormalAction.onDoInitial()</c>（<c>CheckNormalAction.cs:81-91</c>）回零前置检查：
    /// 各轴当前位置在安全区内（对齐源端 <c>funcMotorZSafeCommon</c> Z 轴安全位互锁）。
    /// </summary>
    /// <remarks>
    /// 源端回零顺序（<c>onDoInitial:93-118</c>）：Z→X+Y+Rx+Rz（X 最后等待）→虚拟轴→SafePosi。
    /// 本节点不编码顺序（顺序由 HomeStation recipe 节点链表达，lmv 已支持），
    /// 只做"回零前各轴已在安全位"前置校验，避免回零运动中碰撞。
    /// 真机回零运动 ⚠️ 待人类现场验证。
    /// </remarks>
    public class HomeSafetyCheck : MotionFunction
    {
        /// <summary>待检查安全位的轴列表（对齐源端 SafePosi 各轴）</summary>
        [Parameter("待检查安全位的轴(多选)", 0, CN = "检查轴列表")]
        public List<VDevice> Axes { get; set; } = new List<VDevice>();

        /// <summary>安全位位置（各轴当前位须 ≤ 此值才视为安全，对齐源端 SafePosi.Z）</summary>
        [Parameter("安全位阈值", 1, CN = "安全位阈值", DefaultV = 0.0)]
        public double SafePosition { get; set; } = 0.0;

        /// <summary>比较方向：true=当前位置须 ≤ 安全位（如 Z 轴抬起到安全高度），false=须 ≥ 安全位</summary>
        [Parameter("比较方向(true小于等于/false大于等于)", 2, CN = "比较方向", DefaultV = true)]
        public bool LessOrEqual { get; set; } = true;

        /// <summary>是否启用（false=跳过检查）</summary>
        [Parameter("是否启用", 3, CN = "启用", DefaultV = true)]
        public bool Enable { get; set; } = true;

        public HomeSafetyCheck()
        {
            this.Tips = "回零前安全位检查(对齐源端SafePosi前置)";
            this.Icon = "\xe6a2";
        }

        public override string[] NoteParams { get; set; } = new[] { nameof(Enable), nameof(SafePosition) };

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;
            if (!Enable)
            {
                return base.DoExcute(out errMsg);
            }

            if (Axes == null || Axes.Count == 0)
            {
                // 未配置轴视为通过（仅提示），避免配置缺失卡死回零链
                OnAlarm(AlarmType.InfoTip, "回零安全检查未配置轴，跳过", "HOME_SAFETY_SKIP");
                return base.DoExcute(out errMsg);
            }

            foreach (var device in Axes)
            {
                GetVDevice<VAxis>(device, out var axis);
                if (axis == null) continue;

                double curPos = axis.GetCurrentPos();
                bool safe = IsPositionSafe(curPos, SafePosition, LessOrEqual);
                if (!safe)
                {
                    errMsg = $"轴 {axis.Name} 当前位置 {curPos} 不在安全位（阈值 {SafePosition}），禁止回零";
                    OnAlarm(AlarmType.WarningTip, errMsg, "HOME_SAFETY_FAIL");
                    return false;
                }
            }

            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 安全位判定（纯逻辑，便于单测）。
        /// 对齐源端 <c>funcMotorZSafeCommon</c>：<c>PosiHelper.IsClose(curActZ, SafePosi.Z, 1)</c>。
        /// </summary>
        /// <param name="currentPos">轴当前位置</param>
        /// <param name="safePosition">安全位阈值</param>
        /// <param name="lessOrEqual">true=须 ≤ 安全位，false=须 ≥ 安全位</param>
        /// <returns>true=在安全位</returns>
        public static bool IsPositionSafe(double currentPos, double safePosition, bool lessOrEqual)
        {
            return lessOrEqual ? (currentPos <= safePosition) : (currentPos >= safePosition);
        }
    }
}
