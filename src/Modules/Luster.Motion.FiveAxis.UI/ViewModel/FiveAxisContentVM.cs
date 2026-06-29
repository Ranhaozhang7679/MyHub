using Luster.Motion.CommonUI.ViewModel;
using Luster.Module.Motion.Business;
using Luster.Module.Motion.Business.Functions;

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
    /// ✅ 已接线（TES-70 P6-A 收尾）：Business.SetFunction(FiveAxisCaliParam) 触发 InitParameters
    ///   反射填充 Parameters，产出可被 ParamGrid 接受的 TaskFlow IModule。裸 new FiveAxisCaliParam
    ///   是 IFunction 非 IModule，ParamGrid as IModule=null 会拒绝——须经宿主 Business 挂载。
    /// </summary>
    public class FiveAxisContentVM : MotionVM
    {
        private object? _caliParamModule;

        /// <summary>
        /// ParamGrid 绑定的 [Parameter] 数据契约（TaskFlow IModule）。
        /// 类型用 object 对齐 InParamContentVM.ModuleObj 约定；运行时 ParamGrid 内部 as IModule。
        /// 已接线：构造时由 Business.SetFunction(FiveAxisCaliParam) 填充 Parameters。
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
            // P6-A 收尾接线：宿主 Business 挂载 FiveAxisCaliParam 触发 InitParameters 反射填充 Parameters，
            // 产出可被 ParamGrid 接受的 TaskFlow IModule（裸 new FiveAxisCaliParam 是 IFunction 非 IModule，ParamGrid 会拒绝）。
            var biz = new Business();
            biz.SetFunction(nameof(FiveAxisCaliParam));
            CaliParamModule = biz;
        }
    }
}
