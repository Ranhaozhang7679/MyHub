namespace Luster.Module.Motion.TestToolchain
{
    /// <summary>
    /// 运行模式常量（TES-34 P9-A，迁移自源端 ModeManager）。
    /// 模式本身属运行时可切换状态（非 DebugProfile 字段），通过 lmv 既有
    /// <c>SystemConfig.ListRunMode</c>（自动含 ProductMode/EmptyMode/FirstMode）承载。
    /// 本类提供调机模式（DebugMode）常量，供流程层（P5/AOI 编排，挂账）切换 Action 时使用。
    /// </summary>
    /// <remarks>
    /// <b>对齐源端</b>：源端 ModeManager 四常量 NormalMode/FirstMode/EmptyMode/DebugMode，
    /// PluginComponent 仅 Add(DebugMode)→模式下拉含“生产模式+调机模式”。
    /// lmv 侧 SystemConfig 已自动添加 ProductMode/EmptyMode/FirstMode；
    /// 调机模式由本常量定义，切换入口对接 lmv RunModeModel + DeviceMode（不重建模式系统）。
    /// </remarks>
    public static class DebugWorkMode
    {
        /// <summary>生产模式（对应 lmv SystemConsts.ProductMode）</summary>
        public const string NormalMode = "生产模式";

        /// <summary>调机模式（源端 DebugMode，DebugProfile 开关在此模式下生效）</summary>
        public const string DebugMode = "调机模式";

        /// <summary>空跑模式（对应 lmv SystemConsts.EmptyMode）</summary>
        public const string EmptyMode = "空跑模式";

        /// <summary>首件模式（对应 lmv SystemConsts.FirstMode）</summary>
        public const string FirstMode = "首件模式";
    }
}
