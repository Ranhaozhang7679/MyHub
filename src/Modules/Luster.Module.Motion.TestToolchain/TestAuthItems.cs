using DC.Authorization;
using DC.Authorization.Models;
using System.Linq;

namespace Luster.Module.Motion.TestToolchain
{
    /// <summary>
    /// 测试工具链权限项（TES-34 P9-A/B/C，非侵入新增）。
    /// 对齐 <see cref="Luster.Module.Motion.Production.RecipeAuthItems"/> 模式，定义调试/手动操作权限 AuthItem，
    /// 由 <see cref="TestToolchainModule"/> 启动时经 <c>IAuthorizationFacade.RegisterRights</c> 注册，
    /// 不改既有 <c>AuthKeys.cs</c>（R1 非侵入）。
    /// </summary>
    /// <remarks>
    /// 角色对齐源端三角色（工程师/操作员/管理员）：手动操作/回退/模式切换/调试参数编辑
    /// 管理员+工程师可操作，操作员只读（由 lmv 既有动态 Role + HasAuth 判定）。
    /// </remarks>
    public static class TestAuthItems
    {
        /// <summary>手动操作（IO 强制/单轴点动/轴组动作）</summary>
        public static readonly AuthItem ManualOperate = new AuthItem("测试工具链", "手动操作", "手动操作", "IO强制/单轴点动/轴组动作等手动操作", 1);

        /// <summary>手动回退（Undo/全部回退）</summary>
        public static readonly AuthItem ManualBackup = new AuthItem("测试工具链", "手动操作", "手动回退", "回退栈单步/全部回退", 2);

        /// <summary>切换运行模式（生产/调机/空跑）</summary>
        public static readonly AuthItem DebugModeSwitch = new AuthItem("测试工具链", "调试模式", "切换运行模式", "切换生产/调机/空跑模式", 3);

        /// <summary>编辑调试参数（DebugProfile 开关）</summary>
        public static readonly AuthItem DebugProfileEdit = new AuthItem("测试工具链", "调试模式", "编辑调试参数", "编辑DebugProfile开关", 4);

        /// <summary>权限项全集（供 RegisterRights 注册）</summary>
        public static readonly AuthItem[] All =
        {
            ManualOperate, ManualBackup, DebugModeSwitch, DebugProfileEdit
        };

        /// <summary>
        /// 转换为 <see cref="Right"/> 数组供 <c>IAuthorizationFacade.RegisterRights</c> 注册
        /// （对齐 <c>RecipeAuthItems.ToRights</c> 的 AuthItem→Right 转换，全部为 Operation 类型）。
        /// </summary>
        public static Right[] ToRights()
        {
            return All.Select(a => new Right
            {
                Name = a.Operation,
                ModuleName = a.Module,
                ViewName = a.View,
                Description = a.Description,
                Type = RightType.Operation,
                SortOrder = a.Order
            }).ToArray();
        }
    }
}
