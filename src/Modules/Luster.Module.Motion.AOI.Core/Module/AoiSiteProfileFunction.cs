using Luster.TaskFlow.Motion;

namespace Luster.Module.Motion.AOI.Core.Module
{
    /// <summary>
    /// AOI Site Profile 占位 Function。
    /// 仅用于确保 <c>AoiCoreModule</c> 被 ModuleFactory 反射发现。
    /// 站点 profile 的实际加载与校验不在 Function 执行层，而在模块启动时由
    /// <see cref="AoiCoreModule.LoadSiteProfile"/> 和 <see cref="AoiCoreModule.ValidateProfile"/> 完成。
    /// </summary>
    public sealed class AoiSiteProfileFunction : MotionFunction
    {
        public override bool DoExcute(out string statusMsg)
        {
            // 占位：不做业务逻辑，仅标记完成
            statusMsg = "AOI SiteProfile 占位 Function 执行完毕。";
            return true;
        }
    }
}