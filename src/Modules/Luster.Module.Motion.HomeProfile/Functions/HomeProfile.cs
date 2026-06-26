using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using System.ComponentModel;

namespace Luster.Module.Motion.HomeProfile.Functions
{
    /// <summary>
    /// 回零参数节点（TES-39 P7-D）。
    /// 映射源端 SP-2025140 <c>HomeSettingProfile</c>（<c>MotorComponent.cs:1890</c>）回零参数到 lmv <c>VAxis</c>，
    /// 并驱动 <c>VAxis.Home()</c> + <c>CheckHomeDone()</c>（对齐源端 <c>MotorComponent.MoveToHome</c>）。
    /// </summary>
    /// <remarks>
    /// <b>参数映射</b>（源端 → lmv VAxis）：
    /// - <c>HomeMode</c>（源端 int HM_MODE1..6）→ <see cref="HomeMode"/> 枚举（lmv 已有，对应 ZMotion datum mode）
    /// - <c>HighVel</c>/<c>LowVel</c>（源端 MotorSpeedLow/MotorSpeedBackHome）→ <see cref="VAxis.HomeSpeedHigh"/>/<see cref="VAxis.HomeSpeedLow"/>
    /// - <c>Tacc</c>（源端 MotorAccTime）→ <see cref="VAxis.HomeAcc"/>
    /// - <c>HomeOffset</c> → <see cref="VAxis.HomeOffset"/>
    /// 源端额外参数（<c>Dir</c>/<c>HomeHighEffect</c>/<c>ReScanEnable</c>/<c>RetSwOffset</c>）lmv <c>IMotionCard.Home</c> 签名不接收，
    /// 本节点作为扩展配置项暴露并持久化，卡端落地（DatumIn/SetInvertIn 电平映射）属 TES-26 ZMotion 适配器扩展或现场配置，
    /// 标 ⚠️ 待人类现场验证。
    /// </remarks>
    public class HomeProfile : MotionFunction
    {
        /// <summary>待回零轴</summary>
        [NotEmpty]
        [Parameter("待回零轴", 0, CN = "回零轴", EditorType = typeof(VAxis))]
        public VDevice Device { get; set; }

        /// <summary>回零模式（对齐源端 HomeMode HM_MODE1..6）</summary>
        [Parameter("回零模式", 1, CN = "回零模式", DefaultV = HomeMode.NegativeToZ)]
        public HomeMode HomeMode { get; set; } = HomeMode.NegativeToZ;

        /// <summary>搜寻方向：true=正向，false=负向（源端 Dir）</summary>
        [Parameter("搜寻方向(true正/false负)", 2, CN = "搜寻方向", DefaultV = false)]
        public bool SearchDirection { get; set; } = false;

        /// <summary>原点高电平有效（源端 HomeHighEffect）</summary>
        [Parameter("原点高电平有效", 3, CN = "原点高电平", DefaultV = true)]
        public bool HomeHighEffect { get; set; } = true;

        /// <summary>是否两次回零（源端 ReScanEnable）</summary>
        [Parameter("是否两次回零", 4, CN = "两次回零", DefaultV = false)]
        public bool ReScanEnable { get; set; } = false;

        /// <summary>二次回零离开开关距离（源端 RetSwOffset，脉冲）</summary>
        [Parameter("二次回零距离(脉冲)", 5, CN = "二次回零距离", DefaultV = 0)]
        public int RetSwOffset { get; set; } = 0;

        /// <summary>回零高速（源端 HighVel/MotorSpeedLow）</summary>
        [Parameter("回零高速", 6, CN = "回零高速", DefaultV = 0u)]
        public uint HomeSpeedHigh { get; set; } = 0;

        /// <summary>回零低速（源端 LowVel/MotorSpeedBackHome）</summary>
        [Parameter("回零低速", 7, CN = "回零低速", DefaultV = 0u)]
        public uint HomeSpeedLow { get; set; } = 0;

        /// <summary>回零加速度（源端 Tacc/MotorAccTime）</summary>
        [Parameter("回零加速度", 8, CN = "回零加速度", DefaultV = 0u)]
        public uint HomeAcc { get; set; } = 0;

        /// <summary>回零偏移（源端 HomeOffset）</summary>
        [Parameter("回零偏移", 9, CN = "回零偏移", DefaultV = 0f)]
        public float HomeOffset { get; set; } = 0f;

        /// <summary>是否覆盖 VAxis 已有回零参数（true=用本节点参数写入 VAxis 后回零）</summary>
        [Parameter("是否覆盖轴回零参数", 10, CN = "覆盖轴参数", DefaultV = true)]
        public bool OverrideAxisParams { get; set; } = true;

        /// <summary>回零完成检查超时（秒，对齐源端 BackHomeTimeOut）</summary>
        [Parameter("回零超时(秒)", 11, CN = "回零超时", DefaultV = 60)]
        public int HomeTimeout { get; set; } = 60;

        /// <summary>是否检查回零完成</summary>
        [Parameter("是否检查回零完成", 12, CN = "检查完成", DefaultV = true)]
        public bool CheckDone { get; set; } = true;

        /// <summary>回零结果（OUT：true=回零成功）</summary>
        [Parameter("回零结果", 20, CN = "回零结果", ParamType = ParamType.OUT)]
        public bool IsHomeDone { get; set; }

        public HomeProfile()
        {
            this.Tips = "回零参数节点(映射源端HomeSettingProfile)";
            this.Icon = "\xe6a1";
        }

        public override string[] NoteParams { get; set; } = new[] { nameof(Device), nameof(HomeMode) };

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;
            GetVDevice<VAxis>(Device, out var axis);
            if (axis == null)
            {
                errMsg = "回零轴未配置";
                return false;
            }

            // 覆盖 VAxis 回零参数（映射源端 HomeSettingProfile → VAxis）
            if (OverrideAxisParams)
            {
                axis.HomeMode = HomeMode;
                if (HomeSpeedHigh > 0) axis.HomeSpeedHigh = HomeSpeedHigh;
                if (HomeSpeedLow > 0) axis.HomeSpeedLow = HomeSpeedLow;
                if (HomeAcc > 0) axis.HomeAcc = HomeAcc;
                axis.HomeOffset = HomeOffset;
            }

            // 驱动回零（对齐源端 MotorComponent.MoveToHome → ZMotion SingleDatum）
            // 注：SearchDirection/HomeHighEffect/ReScanEnable/RetSwOffset 源端经 ZMotion SetDatumIn/SetInvertIn 落地，
            // lmv IMotionCard.Home 签名不接收，卡端电平/IO 映射属 TES-26 扩展或现场配置（⚠️ 待人类现场）
            axis.Home();

            if (CheckDone)
            {
                axis.CheckHomeDone(HomeTimeout);
            }

            IsHomeDone = axis.IsHome;
            if (!IsHomeDone)
            {
                errMsg = $"轴 {axis.Name} 回零未完成";
                OnAlarm(AlarmType.HomeError, errMsg, "HOME_FAIL");
                return false;
            }

            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 源端回零模式（HM_MODE int）→ lmv <see cref="HomeMode"/> 枚举映射（纯逻辑，便于单测）。
        /// 源端 <c>HomeSettingProfile.HomeMode</c>（int）直接作为 ZMotion datum mode 传给卡
        /// （<c>ZMCMotion.MoveToHome:1307</c> → <c>ZAux_Direct_Single_Datum(mode)</c>），
        /// lmv <see cref="HomeMode"/> 枚举值与 ZMotion datum mode 对齐，故直接强转。
        /// 源端特殊值：≥1000 表示"不运动"（flag bit），JOURNEY_HOME_MODE=100/ABSTRACT_HOME_MODE=101（非 datum）。
        /// </summary>
        public static HomeMode MapFromSourceHomeMode(int sourceHomeMode)
        {
            // 源端 ≥1000 = no-move flag，剥离后映射
            int mode = sourceHomeMode >= 1000 ? sourceHomeMode - 1000 : sourceHomeMode;
            // 100/101 是源端非 datum 模式（行程/坐标回零），lmv 无直接对应，映射到 CurrentHome（当前位置建立原点）
            if (mode == 100 || mode == 101) return HomeMode.CurrentHome;
            if (System.Enum.IsDefined(typeof(HomeMode), mode)) return (HomeMode)mode;
            // 未知模式默认负极限寻EZ
            return HomeMode.NegativeToZ;
        }

        /// <summary>源端搜寻方向（bool Dir）→ 是否正向（纯逻辑）</summary>
        public static bool MapSearchDirection(bool sourceDir) => sourceDir;
    }
}
