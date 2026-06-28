using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.FiveAxis.Service;

namespace Luster.Motion.SubSystem.ViewModel
{
    /// <summary>
    /// 五轴标定向导顶级页容器 ViewModel（TES-158 阶段 1：骨架）。
    /// 消费 <see cref="IFiveAxisCalibrationService"/>（P2-B 标定 Service，将 RegisterSingleton 入容器）。
    /// 本 issue 仅搭骨架，不实现 ParamGrid 绑定 / 命令接线（范围冻结，留待 TES-75~80）。
    /// </summary>
    /// <remarks>
    /// <b>构造注入决策</b>：本阶段只注入 <see cref="IFiveAxisCalibrationService"/>（容器可解析，保证运行时可达性）。
    /// 不注入 <c>IFiveAxisFrame</c>（未注册到容器）和 <c>FiveAxisCaliParam</c>（OverTimeFunction 算子节点，非容器 Service），
    /// 否则 Prism 解析 VM 失败、运行时崩溃。
    /// </remarks>
    public class FiveAxisCalibContentVM : MotionVM
    {
        private readonly IFiveAxisCalibrationService _calibService;

        public FiveAxisCalibContentVM(IFiveAxisCalibrationService calibService)
            : base()
        {
            _calibService = calibService;
            // TODO TES-75~80：ParamGrid 绑定/命令接线时再注入 IFiveAxisFrame + FiveAxisCaliParam（需先注册 IFiveAxisFrame 适配），并实现各 Tab 命令
        }
    }
}
