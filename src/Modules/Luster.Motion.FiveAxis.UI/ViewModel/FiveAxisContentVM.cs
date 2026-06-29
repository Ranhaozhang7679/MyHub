using Luster.Motion.CommonUI.ViewModel;

namespace Luster.Motion.FiveAxis.UI.ViewModel
{
    /// <summary>
    /// 五轴标定参数面板 VM（P6-A 基建）。
    /// 职责：为 ParamGrid 托载 [Parameter] 数据契约 + 角色权限门控（继承 MotionVM 的 SysRole）。
    ///
    /// 特性对齐契约（源端 WinForm PropertyGrid → 目标端 [Parameter]，供全栈实现真实数据模型时遵循）：
    ///   [DisplayName(...)]              → [Parameter] CN          （中文显示名）
    ///   [Category(...)]                 → [Parameter] Group        （分组，如实轴点位/五轴标定参数/锁存参数）
    ///   [Description(...)]              → [Parameter] Tips         （提示）
    ///   [Editor(typeof(UITypeEditor))]  → [Parameter] EditorType   （如 FileDialogEditor→路径 Editor、FrameRouteEditor→阵列点 Editor）
    ///   [Permission(Administrator)]     → HasPermission(Admin) / [Parameter] Visible 门控
    ///   [Browsable(false)]              → [Parameter] Visible = false
    ///   [ReadOnly(true)]                → [Parameter] IsReadOnly = true
    ///   [TypeConverter(ExpandableObjectConverter)] → [Parameter] CanExpand（嵌套展开）
    /// 源端核心 Profile 锚点：AutoCaliProfile（球采样间距/标准球半径/标定延时/激光标定…）、
    ///   Check5AxisBaseProfile（安全位置/五轴模式/FiveAxisPara/Tool2Work/锁存参数…）。
    ///
    /// ⚠️ 后端接口状态（阻塞项，已按派单"后端未就位先用 mock"处理）：
    ///   ParamGrid.SelectedObject 需 TaskFlow IModule，其 Parameters 字典由引擎从所挂载 TaskFunction 的
    ///   [Parameter] 反射填充（AbsModule.ctor → InitFunctions；ParameterAttribute 反射 module.TaskFunction）。
    ///   真实五轴标定 [Parameter] 数据模型（对齐源端 AutoCaliProfile / Check5AxisBaseProfile 的
    ///   IModule + TaskFunction）尚未由全栈工程师交付。当前 CaliParamModule 留空，View 显示占位提示；
    ///   全栈交付后把实例赋给 CaliParamModule，ParamGrid 即自动生成面板——无需改 View/VM 装配代码。
    ///   此为后端数据契约缺失，需 @项目经理 协调全栈（非前端可自主完成）。
    /// </summary>
    public class FiveAxisContentVM : MotionVM
    {
        private object? _caliParamModule;

        /// <summary>
        /// ParamGrid 绑定的 [Parameter] 数据契约（TaskFlow IModule）。
        /// 类型用 object 对齐 InParamContentVM.ModuleObj 约定；运行时 ParamGrid 内部 as IModule。
        /// 当前为 null（待全栈交付真实五轴标定数据模型）。
        /// </summary>
        public object? CaliParamModule
        {
            get => _caliParamModule;
            set => SetProperty(ref _caliParamModule, value);
        }

        /// <summary>占位用：数据模型未就位时 View 显示提示。</summary>
        public bool HasCaliParamModule => _caliParamModule != null;

        public FiveAxisContentVM()
        {
            // 基建阶段留空。全栈交付五轴标定 [Parameter] IModule 后赋值即可。
        }
    }
}
