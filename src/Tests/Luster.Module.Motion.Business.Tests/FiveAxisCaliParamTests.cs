using Luster.Module.Motion.Business.Functions;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Luster.Module.Motion.Business.Tests
{
    /// <summary>
    /// TES-29 P2-D 补遗/P6-A 配套:五轴标定 [Parameter] 数据模型 特性映射契约验证。
    /// 验证源端 [DisplayName]/[Category]/[Description]/[Browsable]/[TypeConverter] → [Parameter] CN/Group/Tips/Visible/CanExpand 映射正确,
    /// 供前端 P6-B ParamGrid SelectedObject 绑定。
    /// </summary>
    public class FiveAxisCaliParamTests
    {
        private static ParameterAttribute GetParam(string propName)
        {
            var prop = typeof(FiveAxisCaliParam).GetProperty(propName);
            return prop?.GetCustomAttribute<ParameterAttribute>();
        }

        #region 特性映射契约（[DisplayName]→CN, [Category]→Group, [Description]→tips, [Browsable(false)]→Visible）

        [Fact]
        public void VirMode_映射源端DisplayName和Category()
        {
            var p = GetParam(nameof(FiveAxisCaliParam.VirMode));
            Assert.NotNull(p);
            Assert.Equal("五轴模式", p.CN);                    // [DisplayName("五轴模式")]
            Assert.Equal("五轴标定参数", p.Group);              // [Category("五轴标定参数")]
            Assert.Equal(false, p.DefaultV);                    // DefaultV
        }

        [Fact]
        public void BallSampleSpan_映射源端DisplayName和Description()
        {
            var p = GetParam(nameof(FiveAxisCaliParam.BallSampleSpan));
            Assert.NotNull(p);
            Assert.Equal("球采样间距", p.CN);                    // [DisplayName("球采样间距")]
            Assert.Contains("5点采样", p.Tips);                  // [Description("5点采样")] → tips
            Assert.Equal("自动标定参数", p.Group);
            Assert.Equal(1.0, p.DefaultV);
        }

        [Fact]
        public void BallRadius_ReadOnly映射()
        {
            var p = GetParam(nameof(FiveAxisCaliParam.BallRadius));
            Assert.NotNull(p);
            Assert.True(p.IsReadOnly);                           // 源端 [ReadOnly(true)]
            Assert.Equal(12.7, p.DefaultV);
        }

        [Fact]
        public void FiveAxisPara_CanExpand映射TypeConverter()
        {
            var p = GetParam(nameof(FiveAxisCaliParam.FiveAxisPara));
            Assert.NotNull(p);
            Assert.True(p.CanExpand);                            // [TypeConverter(ExpandableObjectConverter)] → CanExpand
        }

        [Fact]
        public void Rough5Para_ParamTypeOUT映射结果字段()
        {
            var p = GetParam(nameof(FiveAxisCaliParam.Rough5Para));
            Assert.NotNull(p);
            Assert.Equal(ParamType.OUT, p.ParamType);            // 结果字段 → OUT
            Assert.True(p.CanExpand);
        }

        [Fact]
        public void Accurate5Para_ParamTypeOUT映射()
        {
            var p = GetParam(nameof(FiveAxisCaliParam.Accurate5Para));
            Assert.Equal(ParamType.OUT, p.ParamType);
        }

        [Fact]
        public void WorkOriginResult_ParamTypeOUT映射()
        {
            var p = GetParam(nameof(FiveAxisCaliParam.WorkOriginResult));
            Assert.Equal(ParamType.OUT, p.ParamType);
        }

        #endregion

        #region 分组覆盖（源端 Category 全覆盖）

        [Theory]
        [InlineData(nameof(FiveAxisCaliParam.VirMode), "五轴标定参数")]
        [InlineData(nameof(FiveAxisCaliParam.CameraOffset), "五轴标定参数")]
        [InlineData(nameof(FiveAxisCaliParam.FiveAxisPara), "五轴标定参数")]
        [InlineData(nameof(FiveAxisCaliParam.Tool2Work), "五轴标定参数")]
        [InlineData(nameof(FiveAxisCaliParam.BoxSetting), "五轴标定参数")]
        [InlineData(nameof(FiveAxisCaliParam.BallSampleSpan), "自动标定参数")]
        [InlineData(nameof(FiveAxisCaliParam.BallRadius), "自动标定参数")]
        [InlineData(nameof(FiveAxisCaliParam.CaliDelay), "自动标定参数")]
        [InlineData(nameof(FiveAxisCaliParam.RoughRx), "粗略标定")]
        [InlineData(nameof(FiveAxisCaliParam.AccurateRxSpan), "精确标定")]
        [InlineData(nameof(FiveAxisCaliParam.ZeroRx), "精确标定")]
        [InlineData(nameof(FiveAxisCaliParam.WorkOriginResult), "工件原点示教")]
        [InlineData(nameof(FiveAxisCaliParam.SafePosi), "实轴点位")]
        [InlineData(nameof(FiveAxisCaliParam.FeedPosi), "实轴点位")]
        [InlineData(nameof(FiveAxisCaliParam.LeavePosi), "实轴点位")]
        [InlineData(nameof(FiveAxisCaliParam.ModelBasePathMono), "光学参数")]
        [InlineData(nameof(FiveAxisCaliParam.AutoModePath), "光学参数")]
        [InlineData(nameof(FiveAxisCaliParam.AutoSaveImage), "光学参数")]
        public void 分组对齐源端Category(string propName, string expectedGroup)
        {
            var p = GetParam(propName);
            Assert.NotNull(p);
            Assert.Equal(expectedGroup, p.Group);
        }

        #endregion

        #region 全字段 [Parameter] 标注覆盖（前端绑定契约完整性）

        [Fact]
        public void 所有关键标定字段均有Parameter特性()
        {
            // 前端 ParamGrid 需这些字段都进 Parameters 字典(经 [Parameter] 反射)
            string[] mustHave = {
                nameof(FiveAxisCaliParam.VirMode), nameof(FiveAxisCaliParam.CameraOffset),
                nameof(FiveAxisCaliParam.FiveAxisPara), nameof(FiveAxisCaliParam.Tool2Work),
                nameof(FiveAxisCaliParam.BoxSetting), nameof(FiveAxisCaliParam.BallSampleSpan),
                nameof(FiveAxisCaliParam.BallRadius), nameof(FiveAxisCaliParam.CaliDelay),
                nameof(FiveAxisCaliParam.RoughRx), nameof(FiveAxisCaliParam.RoughRz),
                nameof(FiveAxisCaliParam.Rough5Para), nameof(FiveAxisCaliParam.AccurateRxSpan),
                nameof(FiveAxisCaliParam.Accurate5Para), nameof(FiveAxisCaliParam.ZeroRx),
                nameof(FiveAxisCaliParam.WorkOriginResult), nameof(FiveAxisCaliParam.SafePosi),
                nameof(FiveAxisCaliParam.ModelBasePathMono), nameof(FiveAxisCaliParam.AutoSaveImage),
            };
            foreach (var name in mustHave)
            {
                Assert.NotNull(GetParam(name));
            }
        }

        [Fact]
        public void ParameterCount覆盖源端核心字段()
        {
            int count = typeof(FiveAxisCaliParam)
                .GetProperties()
                .Count(p => p.GetCustomAttribute<ParameterAttribute>() != null);
            // 至少 25 个 [Parameter] 字段(五轴标定参数+自动标定+粗略+精确+原点+实轴点位+光学)
            Assert.True(count >= 25, $"实际 [Parameter] 字段数: {count}");
        }

        #endregion

        #region DoExcute 占位（数据模型不实现标定算法）

        [Fact]
        public void DoExcute_数据模型占位返回true()
        {
            var cali = new FiveAxisCaliParam();
            bool ok = cali.DoExcute(out string errMsg);
            Assert.True(ok);
            Assert.Contains("P1 Coord5Axis", errMsg);
        }

        #endregion

        #region 默认值对齐源端

        [Fact]
        public void 默认值对齐源端AutoCaliProfile()
        {
            var cali = new FiveAxisCaliParam();
            Assert.Equal(12.7, cali.BallRadius);      // 源端标准球半径
            Assert.Equal(500, cali.CaliDelay);         // 源端标定延时
        }

        [Fact]
        public void 默认值对齐源端精确标定()
        {
            var cali = new FiveAxisCaliParam();
            Assert.Equal(5.0, cali.AccurateRxSpan);    // 源端 RxSpan
            Assert.Equal(3, cali.AccurateRxFCount);    // 源端 RxFCount
            Assert.Equal(3, cali.AccurateRxBCount);
        }

        [Fact]
        public void 默认值对齐源端粗略标定()
        {
            var cali = new FiveAxisCaliParam();
            Assert.Equal(45.0, cali.RoughRx);          // 源端 Rx 旋转角度
            Assert.Equal(45.0, cali.RoughRz);
        }

        #endregion
    }
}
