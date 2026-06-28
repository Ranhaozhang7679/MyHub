using FluentAssertions;
using Luster.Motion.FiveAxis.Data.Calibration;
using Luster.TaskFlow.Common.Module;
using NUnit.Framework;

namespace Luster.Module.Motion.FiveAxis.Tests
{
    /// <summary>
    /// 验证 Motion ParamGrid 能否绑定裸 POCO LaserCaliResult。
    /// ParamGrid.UpdateItems(ParamGrid.cs:201-206) 门槛:
    ///   module = obj as IModule; if (module == null) return;
    /// LaserCaliResult 只实现 IXMLParser,非 IModule → 第一行即 return,补 [Parameter] 也无法渲染。
    /// 本测试为 TES-163 受控验证,用纯 .NET 反射/类型断言定论,不启动 WPF 宿主。
    /// </summary>
    [TestFixture]
    public class LaserCaliResultParamGridBindingTests
    {
        /// <summary>
        /// 路径 A:ParamGrid.UpdateItems 的 IModule 门槛(决定性)。
        /// LaserCaliResult 不是 IModule → obj as IModule == null → UpdateItems 立即 return,不渲染。
        /// </summary>
        [Test]
        public void LaserCaliResult_AsIModule_IsNull_ParamGridEarlyReturns()
        {
            var result = new LaserCaliResult();

            // 模拟 ParamGrid.UpdateItems 第一行门槛:module = obj as IModule (ParamGrid.cs:205-206)
            IModule module = result as IModule;

            // 决定性事实:LaserCaliResult as IModule == null
            module.Should().BeNull(
                "LaserCaliResult 只实现 IXMLParser,不实现 IModule;" +
                "ParamGrid.UpdateItems 第一行 module=obj as IModule 即 null,if(null) return 直接退出," +
                "无论是否补 [Parameter] 特性都无法进入参数收集分支");
        }

        /// <summary>
        /// 路径 B:LaserCaliResult 类型不实现 IModule 接口。
        /// IModule 拥有 Parameters 字典/TaskFunction/InitParameters 等;LaserCaliResult 无这些,
        /// 即使补 [Parameter] 特性,ParamGrid 遍历 module.Parameters 也无对象可遍历(根本走不到这步)。
        /// </summary>
        [Test]
        public void LaserCaliResult_DoesNotImplement_IModule_Interface()
        {
            var type = typeof(LaserCaliResult);

            typeof(IModule).IsAssignableFrom(type)
                .Should().BeFalse(
                "LaserCaliResult 是裸 POCO(仅 IXMLParser),不实现 IModule 接口," +
                "无 Parameters 字典/TaskFunction;ParamGrid.UpdateItems 的 IModule 门槛在前,[Parameter] 收集在后,门槛不通过则 [Parameter] 无意义");
        }
    }
}
