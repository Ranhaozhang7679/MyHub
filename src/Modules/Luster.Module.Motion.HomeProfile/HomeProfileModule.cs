using Luster.Module.Motion.HomeProfile.Functions;
using Luster.TaskFlow.Motion;

namespace Luster.Module.Motion.HomeProfile
{
    /// <summary>
    /// 回零/初始化链路模块（TES-39 P7-D）。
    /// 注册回零参数节点 / 回零前安全检查 / 板卡初始化校验三个运控功能节点。
    /// 零侵入 Shell：卸载本模块 DLL 后平台标准运控不受影响（可还原）。
    /// </summary>
    public class HomeProfileModule : MotionModule
    {
        public override void InitFunctions()
        {
            AddFunction<Functions.HomeProfile>();
            AddFunction<HomeSafetyCheck>();
            AddFunction<AxisInitVerifier>();
        }
    }

    /// <summary>模块创建器（被 IModuleFactory.LoadModules 反射发现）</summary>
    public class HomeProfileModuleCreator : MotionModuleCreator<HomeProfileModule>
    {
        public override int Sort => 6;

        public override string Icon => "\xe6a1";
    }
}
