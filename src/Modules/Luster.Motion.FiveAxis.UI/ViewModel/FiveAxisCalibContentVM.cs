using DC.Authorization;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.FiveAxis.Service;

namespace Luster.Motion.FiveAxis.UI.ViewModel
{
    /// <summary>
    /// 五轴标定向导顶级页容器 ViewModel（TES-158 阶段 1：骨架）。
    /// 消费 <see cref="IFiveAxisCalibrationService"/>（P2-B 标定 Service，将 RegisterSingleton 入容器）。
    /// 本 issue 仅搭骨架，不实现 ParamGrid 绑定 / 命令接线（范围冻结，留待 TES-75~80）。
    /// </summary>
    /// <remarks>
    /// 注入 IFiveAxisCalibrationService + IAuthorizationFacade(经 MotionPageVM 基类链)。
    /// </remarks>
    public class FiveAxisCalibContentVM : MotionPageVM
    {
        private readonly IFiveAxisCalibrationService _calibService;

        public FiveAxisCalibContentVM(IFiveAxisCalibrationService calibService, ICommonBus commonBus, IAuthorizationFacade auth = null)
            : base(commonBus, auth)
        {
            _calibService = calibService;
        }
    }
}
