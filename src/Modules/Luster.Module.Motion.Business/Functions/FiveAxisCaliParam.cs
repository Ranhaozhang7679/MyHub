using Luster.Motion.FiveAxis.Calibration;
using Luster.Motion.FiveAxis.Kinematics;
using Luster.Motion.FiveAxis.Position;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using System.ComponentModel;

namespace Luster.Module.Motion.Business.Functions
{
    /// <summary>
    /// 五轴标定参数数据模型（TES-29 P2-D 补遗 / P6-A 配套，前端 P6-B 标定 UI 的 SelectedObject 数据契约）。
    /// 映射源端 <c>AutoCaliProfile</c>（<c>AutoCaliProfile.cs</c>）+ <c>Check5AxisBaseProfile</c> 五轴标定参数组
    /// （<c>Check5AxisBaseProfile.cs:226-263</c>）的关键字段为 <c>[Parameter]</c>，由 TaskFlow 引擎反射填充
    /// 进 <c>IModule.Parameters</c> 字典供 ParamGrid 绑定。
    /// </summary>
    /// <remarks>
    /// <b>特性映射契约</b>（对齐前端 P6-A 固化的映射）：
    /// - <c>[DisplayName]</c> → <c>CN</c>（中文显示名）
    /// - <c>[Category]</c> → <c>Group</c>（分组）
    /// - <c>[Description]</c> → tips（位置参数[0]）
    /// - <c>[Editor(UITypeEditor)]</c> → <c>EditorType</c>
    /// - <c>[Browsable(false)]</c> → <c>Visible=false</c>
    /// - <c>[TypeConverter(ExpandableObjectConverter)]</c> → <c>CanExpand=true</c>
    /// - <c>[Permission(Administrator)]</c> → 由前端权限门控（HasPermission 绑 SysRole + RoleEnabledConverter(Admin)）
    ///
    /// <b>范围</b>：本类是数据契约（ParamGrid SelectedObject），DoExcute 留标定算法占位——
    /// 五轴标定算法（粗标/精标/激光/原点）属 P1 Coord5Axis（TES-29 已 done），不在本数据模型范围。
    /// 前端 P6-B 标定 UI 绑定本 IModule 后可实战验证 ParamGrid 编辑/权限/持久化，无需改 View/VM 装配。
    /// </remarks>
    public class FiveAxisCaliParam : OverTimeFunction
    {
        #region 五轴标定参数组（对齐源端 Check5AxisBaseProfile:226-263，Category="五轴标定参数"）

        /// <summary>五轴模式（源端 VirMode，True=启用五轴功能，Administrator 权限）</summary>
        [Parameter("五轴模式,True-启用五轴功能", 0, Group = "五轴标定参数", CN = "五轴模式", DefaultV = false)]
        public bool VirMode { get; set; } = false;

        /// <summary>相机相对激光的偏移（源端 CameraOffset，Administrator）</summary>
        [Parameter("相机相对激光的偏移", 1, Group = "五轴标定参数", CN = "相机偏移")]
        public string CameraOffset { get; set; } = "0,0,0";

        /// <summary>相机五轴结构参数（源端 FiveAxisPara:Coord5Axis，Administrator）</summary>
        [Parameter("相机五轴结构参数", 2, Group = "五轴标定参数", CN = "相机五轴结构参数", CanExpand = true)]
        public string FiveAxisPara { get; set; } = "";

        /// <summary>工具转工件（源端 Tool2Work:CoordTransForm，Administrator）</summary>
        [Parameter("工具转工件坐标系", 3, Group = "五轴标定参数", CN = "工具转工件", CanExpand = true)]
        public string Tool2Work { get; set; } = "";

        /// <summary>包围盒参数（源端 boxSetting:BoxProfile，Administrator）</summary>
        [Parameter("包围盒参数(防撞)", 4, Group = "五轴标定参数", CN = "包围盒参数", CanExpand = true)]
        public string BoxSetting { get; set; } = "";

        #endregion

        #region 自动标定参数（对齐源端 AutoCaliProfile 顶层字段）

        /// <summary>球采样间距（源端 AutoCaliProfile.BallSampleSpan，5点采样）</summary>
        [Parameter("球采样间距(5点采样)", 10, Group = "自动标定参数", CN = "球采样间距", DefaultV = 1.0)]
        public double BallSampleSpan { get; set; } = 1.0;

        /// <summary>标准球半径（源端 BallRadius，ReadOnly）</summary>
        [Parameter("标准球半径", 11, Group = "自动标定参数", CN = "标准球半径", DefaultV = 12.7, IsReadOnly = true)]
        public double BallRadius { get; set; } = 12.7;

        /// <summary>标定延时（源端 CaliDelay）</summary>
        [Parameter("标定延时(ms)", 12, Group = "自动标定参数", CN = "标定延时", DefaultV = 500)]
        public int CaliDelay { get; set; } = 500;

        /// <summary>激光测量值限制（源端 LaserValidOffset）</summary>
        [Parameter("激光测量值限制", 13, Group = "自动标定参数", CN = "激光测量值限制", DefaultV = 0.1)]
        public double LaserValidOffset { get; set; } = 0.1;

        #endregion

        #region 粗略标定参数（对齐源端 Rough5AxisAutoCaliProfile）

        /// <summary>Rx 旋转角度（源端 Rough5AxisAutoCaliProfile.Rx）</summary>
        [Parameter("Rx旋转角度", 20, Group = "粗略标定", CN = "Rx旋转角度", DefaultV = 45.0)]
        public double RoughRx { get; set; } = 45.0;

        /// <summary>Rz 旋转角度（源端 Rough5AxisAutoCaliProfile.Rz）</summary>
        [Parameter("Rz旋转角度", 21, Group = "粗略标定", CN = "Rz旋转角度", DefaultV = 45.0)]
        public double RoughRz { get; set; } = 45.0;

        /// <summary>五轴粗略参数结果（源端 Rough5AxisAutoCaliProfile.Rough5Para:Coord5Axis，结果）</summary>
        [Parameter("五轴粗略参数结果", 22, Group = "粗略标定", CN = "五轴粗略参数", ParamType = ParamType.OUT, CanExpand = true)]
        public string Rough5Para { get; set; } = "";

        #endregion

        #region 精确标定参数（对齐源端 Accurate5AxisAutoCaliProfile）

        /// <summary>Rx 间隔（源端 Accurate5AxisAutoCaliProfile.RxSpan）</summary>
        [Parameter("Rx间隔", 30, Group = "精确标定", CN = "Rx间隔", DefaultV = 5.0)]
        public double AccurateRxSpan { get; set; } = 5.0;

        /// <summary>Rx 正向数量（源端 RxFCount）</summary>
        [Parameter("Rx正向数量", 31, Group = "精确标定", CN = "Rx正向数量", DefaultV = 3)]
        public int AccurateRxFCount { get; set; } = 3;

        /// <summary>Rx 反向数量（源端 RxBCount）</summary>
        [Parameter("Rx反向数量", 32, Group = "精确标定", CN = "Rx反向数量", DefaultV = 3)]
        public int AccurateRxBCount { get; set; } = 3;

        /// <summary>Rz 间隔（源端 RzSpan）</summary>
        [Parameter("Rz间隔", 33, Group = "精确标定", CN = "Rz间隔", DefaultV = 5.0)]
        public double AccurateRzSpan { get; set; } = 5.0;

        /// <summary>Rz 正向数量（源端 RzFCount）</summary>
        [Parameter("Rz正向数量", 34, Group = "精确标定", CN = "Rz正向数量", DefaultV = 3)]
        public int AccurateRzFCount { get; set; } = 3;

        /// <summary>Rz 反向数量（源端 RzBCount）</summary>
        [Parameter("Rz反向数量", 35, Group = "精确标定", CN = "Rz反向数量", DefaultV = 3)]
        public int AccurateRzBCount { get; set; } = 3;

        /// <summary>Rx 零点位置（源端 ZeroRx）</summary>
        [Parameter("Rx零点位置", 36, Group = "精确标定", CN = "Rx零点位置", DefaultV = 0.0)]
        public double ZeroRx { get; set; } = 0.0;

        /// <summary>精确五轴参数结果（源端 Accurate5Para:Coord5Axis，结果）</summary>
        [Parameter("精确五轴参数结果", 37, Group = "精确标定", CN = "精确五轴参数", ParamType = ParamType.OUT, CanExpand = true)]
        public string Accurate5Para { get; set; } = "";

        #endregion

        #region 工件原点示教（对齐源端 TeachWorkOriginProfile）

        /// <summary>工件坐标系结果（源端 TeachWorkOriginProfile.RltTool2Work:CoordTransForm）</summary>
        [Parameter("工件坐标系结果", 40, Group = "工件原点示教", CN = "工件坐标系结果", ParamType = ParamType.OUT, CanExpand = true)]
        public string WorkOriginResult { get; set; } = "";

        /// <summary>原点位置类型（源端 OrgPosiType 枚举）</summary>
        [Parameter("原点位置类型", 41, Group = "工件原点示教", CN = "原点位置类型", DefaultV = 0)]
        public int OrgPosiType { get; set; } = 0;

        #endregion

        #region 实轴点位（对齐源端 Check5AxisBaseProfile:184-208，Category="实轴点位"）

        /// <summary>安全位置（源端 SafePosi:PositionXYZRxRyRz）</summary>
        [Parameter("安全位置,安全的位置", 50, Group = "实轴点位", CN = "安全位置", CanExpand = true)]
        public string SafePosi { get; set; } = "0,0,0,0,0";

        /// <summary>上料位置（源端 FeedPosi）</summary>
        [Parameter("上料位置", 51, Group = "实轴点位", CN = "上料位置", CanExpand = true)]
        public string FeedPosi { get; set; } = "0,0,0,0,0";

        /// <summary>下料位置（源端 LeavePosi）</summary>
        [Parameter("下料位置", 52, Group = "实轴点位", CN = "下料位置", CanExpand = true)]
        public string LeavePosi { get; set; } = "0,0,0,0,0";

        #endregion

        #region 光学参数（对齐源端 Check5AxisBaseProfile:397-457，含 [Editor]）

        /// <summary>白光模型库路径（源端 ModelBasePath_Mono，[Editor(FileDialogEditor)]）</summary>
        [Parameter("白光模型库路径", 60, Group = "光学参数", CN = "白光模型库路径")]
        public string ModelBasePathMono { get; set; } = "";

        /// <summary>自动建模路径（源端 AutoModePath，[Editor(FolderEditor)]）</summary>
        [Parameter("自动建模路径", 61, Group = "光学参数", CN = "自动建模路径")]
        public string AutoModePath { get; set; } = "";

        /// <summary>是否自动保存图像（源端 blAutoSaveImage）</summary>
        [Parameter("是否自动保存图像(True:保存;False:不保存)", 62, Group = "光学参数", CN = "是否自动保存图像", DefaultV = false)]
        public bool AutoSaveImage { get; set; } = false;

        #endregion

        public FiveAxisCaliParam()
        {
            this.Tips = "五轴标定参数(对齐源端AutoCaliProfile/Check5AxisBaseProfile)";
            this.Icon = "\xe6b2";
        }

        public override string[] NoteParams { get; set; } = new[] { nameof(VirMode) };

        /// <summary>
        /// 标定算法执行（TES-190 P2-B Service 化）：经宿主模块 Ioc 服务定位 <see cref="IFiveAxisCalibrationService"/>，
        /// 把 [Parameter] 输入适配为 <see cref="CalibrationInput"/>，调用服务并把结果回写三个 OUT
        /// （<see cref="Rough5Para"/>/<see cref="Accurate5Para"/>/<see cref="WorkOriginResult"/>）。
        /// </summary>
        /// <remarks>
        /// 本节点被 TaskFlow 引擎反射 new，无法构造注入，故走服务定位（<c>MyOwner.Ioc.Resolve</c>，对齐 SetMachineMode 范式）。
        /// ⚠️ 标定数值求解本体（粗标/精标/激光/原点）仍在旧程序 Form5Cali/FrameCal/ZFrameCali，不在本 issue 范围——
        /// Service 实现为诚实失败壳：<see cref="IFiveAxisCalibrationService.Calibrate"/> 抛 <see cref="NotImplementedException"/>，
        /// 求解本体留待后续 issue 迁移（D2 精度 + 硬件验证），避免 fake 标定结果流入下游 recipe。
        /// </remarks>
        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;

            // 经宿主模块 Ioc 服务定位取标定服务（对齐 SetMachineMode.cs:69 MyOwner.Ioc.Resolve 范式）。
            var svc = MyOwner.Ioc.Resolve<IFiveAxisCalibrationService>();

            var input = new CalibrationInput
            {
                FiveAxisPara = ParseCoord5Axis(this.FiveAxisPara),
                RoughRx = this.RoughRx,
                RoughRz = this.RoughRz,
                ToolPose = ParseToolPose(this.Tool2Work),
                OrgPosiType = this.OrgPosiType,
            };

            CalibrationResult result = svc.Calibrate(input);

            // 回写 OUT（对齐 FiveAxisCaliParam 的三个结果字段）
            this.Rough5Para = result.Rough5Para;
            this.Accurate5Para = result.Accurate5Para;
            this.WorkOriginResult = result.WorkOriginResult;

            MyOwner?.OnLog(Luster.Common.DataStruct.Enums.LogType.Debug,
                $"FiveAxisCali: rx/rz=({RoughRx},{RoughRz}) -> Rough5Para={Rough5Para} | Accurate5Para={Accurate5Para} | WorkOriginResult={WorkOriginResult}");

            return true;
        }

        /// <summary>
        /// 解析 <see cref="FiveAxisPara"/>（源端 Coord5Axis 序列化字符串）为 <see cref="Coord5Axis"/>。
        /// TODO: 对齐源端序列化格式（JSON/XmlSerializer）反序列化，空值用默认运动学参数。
        /// </summary>
        private static Coord5Axis ParseCoord5Axis(string fiveAxisPara)
        {
            if (string.IsNullOrWhiteSpace(fiveAxisPara)) return new Coord5Axis();
            // TODO: 源端 Coord5Axis 序列化反序列化待迁移，暂用默认参数
            return new Coord5Axis();
        }

        /// <summary>
        /// 解析 <see cref="Tool2Work"/>（源端 CoordTransForm 序列化字符串）为 <see cref="PositionXYZRxRyRz"/>。
        /// TODO: 对齐源端序列化格式反序列化，空值用默认姿态。
        /// </summary>
        private static PositionXYZRxRyRz ParseToolPose(string tool2Work)
        {
            if (string.IsNullOrWhiteSpace(tool2Work)) return new PositionXYZRxRyRz();
            // TODO: 源端 CoordTransForm 序列化反序列化待迁移，暂用默认姿态
            return new PositionXYZRxRyRz();
        }
    }
}
