using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;

namespace Luster.Motion.LightTuning.Functions
{
    /// <summary>
    /// 光调参数数据契约（TES-64 P6-F）：ParamGrid 的 SelectedObject。
    /// 映射源端 <c>LightBxProfile</c>（<c>Plugin.CommonPlugin\Model\Args\LightBxProfile.cs</c>）+
    /// <c>LightControllerBX.TriggerMode</c>（<c>LightControllerBX.cs:51</c>，1=软触发/0=硬触发）+
    /// <c>Check5AxisBaseProfile</c> 通道绑定 <c>chanelR/G/B/Mono</c>（<c>Check5AxisBaseProfile.cs:419-441</c>）。
    /// 由 TaskFlow 引擎反射 <c>[Parameter]</c> 填充进 <c>IModule.Parameters</c> 字典供 ParamGrid 绑定。
    /// </summary>
    /// <remarks>
    /// <b>特性映射契约</b>（对齐 P6-A 固化映射）：
    /// - <c>[DisplayName]</c> → <c>CN</c>；<c>[Category]</c> → <c>Group</c>；<c>[Description]</c> → tips（位置参数[0]）。
    /// - <c>[Browsable(false)]</c> → <c>Visible=false</c>；<c>[Permission(Admin)]</c> → 前端 HasPermission 绑 SysRole + RoleEnabledCoverter(Admin)。
    /// <b>范围</b>：本类仅提供 ParamGrid 编辑契约（通道亮度/触发模式/分组参数 + 通道绑定 + 灰度目标），
    /// DoExcute 留占位——真实下发光源参数由 <c>LightTuningContentVM</c> 调 P2-A BX 设备（<c>ILightController</c>）执行。
    /// </remarks>
    public class LightTuningParam : OverTimeFunction
    {
        #region 分组参数（对齐源端 LightBxProfile.Group + LightControllerBX.TriggerMode）

        /// <summary>组序号（源端 LightBxProfile.Group，范围 1-800）。注：命名为 LightGroup 以避让基类 Function.Group(string)。</summary>
        [Parameter("组序号(范围1-800)", 0, Group = "分组参数", CN = "组序号", DefaultV = 1)]
        public int LightGroup { get; set; } = 1;

        /// <summary>触发模式：1=软触发，0=硬触发（源端 LightControllerBX.mTriggerMode）</summary>
        [Parameter("触发模式(1=软触发,0=硬触发)", 1, Group = "分组参数", CN = "触发模式", DefaultV = 0)]
        public int TriggerMode { get; set; } = 0;

        #endregion

        #region 通道亮度（对齐源端 LightParamProfile.Width/Delay，当前选中通道的脉宽/延时）

        /// <summary>当前通道索引（0 起，对应 ILightController.SetChannelAndVal 的 channelIndex）</summary>
        [Parameter("当前通道索引(0起)", 10, Group = "通道亮度", CN = "当前通道", DefaultV = 0)]
        public int ChannelIndex { get; set; } = 0;

        /// <summary>当前通道脉宽/亮度（源端 LightParamProfile.Width，下发 SetChannelAndVal 的 intensity）</summary>
        [Parameter("当前通道脉宽(亮度值)", 11, Group = "通道亮度", CN = "通道脉宽", DefaultV = 50)]
        public int ChannelWidth { get; set; } = 50;

        /// <summary>当前通道延时（源端 LightParamProfile.Delay）</summary>
        [Parameter("当前通道延时", 12, Group = "通道亮度", CN = "通道延时", DefaultV = 80)]
        public int ChannelDelay { get; set; } = 80;

        /// <summary>通道总数（源端 LightControllerBX.mLightNum，8 或 16）</summary>
        [Parameter("通道总数(8或16)", 13, Group = "通道亮度", CN = "通道总数", DefaultV = 8)]
        public int ChannelCount { get; set; } = 8;

        #endregion

        #region 通道绑定（对齐源端 Check5AxisBaseProfile.chanelR/G/B/Mono，CSV 1 起通道号）

        /// <summary>R 通道绑定（源端 chanelR，CSV 如 "1,6,8,12,14"）</summary>
        [Parameter("R通道绑定(CSV,1起通道号)", 20, Group = "通道绑定", CN = "R通道绑定")]
        public string ChanelR { get; set; } = "1,6,8,12,14";

        /// <summary>G 通道绑定（源端 chanelG）</summary>
        [Parameter("G通道绑定(CSV,1起通道号)", 21, Group = "通道绑定", CN = "G通道绑定")]
        public string ChanelG { get; set; } = "2,5,7,9,11,13,15";

        /// <summary>B 通道绑定（源端 chanelB）</summary>
        [Parameter("B通道绑定(CSV,1起通道号)", 22, Group = "通道绑定", CN = "B通道绑定")]
        public string ChanelB { get; set; } = "3,4,10";

        /// <summary>白光通道绑定（源端 chanelMono）</summary>
        [Parameter("白光通道绑定(CSV,1起通道号)", 23, Group = "通道绑定", CN = "白光通道绑定")]
        public string ChanelMono { get; set; } = "16";

        #endregion

        #region 目标灰度 / 明暗场 / 联动（对齐源端 LightBxProfile 顶层字段）

        /// <summary>灰度目标 Mono（源端 GrayTarget_Mono）</summary>
        [Parameter("灰度目标Mono", 30, Group = "目标与联动", CN = "灰度目标Mono", DefaultV = 120)]
        public int GrayTargetMono { get; set; } = 120;

        /// <summary>明暗场（源端 belongScreen：0=默认/1=明场/2=暗场）</summary>
        [Parameter("明暗场(0默认/1明场/2暗场)", 31, Group = "目标与联动", CN = "明暗场", DefaultV = 0)]
        public int BelongScreen { get; set; } = 0;

        /// <summary>联动使能（源端 LinkEnable）</summary>
        [Parameter("联动使能", 32, Group = "目标与联动", CN = "联动使能")]
        public string LinkEnable { get; set; } = "0";

        /// <summary>联动间隔时间（源端 LinkIntervalTime）</summary>
        [Parameter("联动间隔时间", 33, Group = "目标与联动", CN = "联动间隔时间")]
        public string LinkIntervalTime { get; set; } = "255";

        #endregion

        public LightTuningParam()
        {
            this.Tips = "光源调试参数(对齐源端 LightBxProfile/LightControllerBX)";
            this.Icon = "\xe6b2";
        }

        /// <summary>记入追溯的关键参数名</summary>
        public override string[] NoteParams { get; set; } = { nameof(LightGroup), nameof(TriggerMode), nameof(ChannelIndex), nameof(ChannelWidth) };

        /// <summary>
        /// 执行占位。真实下发光源参数（通道亮度/触发模式/分组）由 <c>LightTuningContentVM</c> 调
        /// P2-A BX 设备 <c>ILightController.SetChannelAndVal</c> + <c>SetTrigMode/SetGroupParm</c> 执行，
        /// 本数据契约不实现设备侧算法（范围冻结 + 不侵入设备层）。
        /// </summary>
        public override bool DoExcute(out string errMsg)
        {
            errMsg = "光调参数数据契约，下发由 LightTuningContentVM 调 BX 设备执行";
            return true;
        }
    }
}
