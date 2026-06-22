using Luster.Module.Motion.HomeProfile.Functions;
using Luster.Motion.DataStruct.Enums;
using Xunit;

namespace Luster.Module.Motion.HomeProfileTests
{
    /// <summary>
    /// TES-39 P7-D:HomeProfile 参数映射 + 安全位判定 + 初始化校验 单测（纯逻辑）。
    /// DoExcute 集成路径依赖 VAxis/DeviceEngine（真机），标待集成/现场，软件层 mock 纯逻辑覆盖。
    /// </summary>
    public class HomeProfileLogicTests
    {
        #region HomeMode 映射

        [Theory]
        [InlineData(1, HomeMode.NegativeToZ)]
        [InlineData(2, HomeMode.PositiveToZ)]
        [InlineData(17, HomeMode.Negative)]
        [InlineData(18, HomeMode.Positive)]
        [InlineData(26, HomeMode.PositiveHome)]
        [InlineData(27, HomeMode.NegativeHome)]
        [InlineData(33, HomeMode.NegativeToEZ)]
        [InlineData(34, HomeMode.PositiveToEZ)]
        [InlineData(35, HomeMode.CurrentHome)]
        public void MapFromSourceHomeMode_标准HM_MODE映射(int source, HomeMode expected)
        {
            Assert.Equal(expected, Luster.Module.Motion.HomeProfile.Functions.HomeProfile.MapFromSourceHomeMode(source));
        }

        [Theory]
        [InlineData(1035, HomeMode.CurrentHome)]      // 1035 = 1000 + 35，剥离 flag → CurrentHome
        [InlineData(1018, HomeMode.Positive)]         // 1018 = 1000 + 18 → Positive
        public void MapFromSourceHomeMode_剥离NoMoveFlag(int source, HomeMode expected)
        {
            Assert.Equal(expected, Luster.Module.Motion.HomeProfile.Functions.HomeProfile.MapFromSourceHomeMode(source));
        }

        [Theory]
        [InlineData(100, HomeMode.CurrentHome)]       // JOURNEY_HOME_MODE → CurrentHome
        [InlineData(101, HomeMode.CurrentHome)]       // ABSTRACT_HOME_MODE → CurrentHome
        public void MapFromSourceHomeMode_行程坐标模式映射CurrentHome(int source, HomeMode expected)
        {
            Assert.Equal(expected, Luster.Module.Motion.HomeProfile.Functions.HomeProfile.MapFromSourceHomeMode(source));
        }

        [Theory]
        [InlineData(999)]   // 未知值
        [InlineData(-1)]
        [InlineData(50)]
        public void MapFromSourceHomeMode_未知值默认负极限寻EZ(int source)
        {
            Assert.Equal(HomeMode.NegativeToZ, Luster.Module.Motion.HomeProfile.Functions.HomeProfile.MapFromSourceHomeMode(source));
        }

        [Fact]
        public void MapSearchDirection_源端Dir映射()
        {
            Assert.True(Luster.Module.Motion.HomeProfile.Functions.HomeProfile.MapSearchDirection(true));   // 正向
            Assert.False(Luster.Module.Motion.HomeProfile.Functions.HomeProfile.MapSearchDirection(false));  // 负向
        }

        #endregion

        #region 安全位判定

        [Theory]
        [InlineData(0.0, 10.0, true, true)]     // Z 抬到 0 ≤ 安全位 10 → 安全
        [InlineData(5.0, 10.0, true, true)]     // 5 ≤ 10 → 安全
        [InlineData(15.0, 10.0, true, false)]   // 15 > 10 → 不安全
        [InlineData(10.0, 10.0, true, true)]    // 等于 → 安全（≤）
        public void IsPositionSafe_小于等于模式(double cur, double safe, bool lessOrEqual, bool expected)
        {
            Assert.Equal(expected, HomeSafetyCheck.IsPositionSafe(cur, safe, lessOrEqual));
        }

        [Theory]
        [InlineData(15.0, 10.0, false, true)]   // 15 ≥ 10 → 安全
        [InlineData(5.0, 10.0, false, false)]   // 5 < 10 → 不安全
        [InlineData(10.0, 10.0, false, true)]   // 等于 → 安全（≥）
        public void IsPositionSafe_大于等于模式(double cur, double safe, bool lessOrEqual, bool expected)
        {
            Assert.Equal(expected, HomeSafetyCheck.IsPositionSafe(cur, safe, lessOrEqual));
        }

        [Fact]
        public void IsPositionSafe_对齐源端Z轴安全位互锁()
        {
            // 源端 funcMotorZSafeCommon: Z 当前位须接近 SafePosi.Z
            // 本节点用 ≤ 表达"Z 已抬到安全高度以上"
            double safePosiZ = 100.0;
            Assert.True(HomeSafetyCheck.IsPositionSafe(80.0, safePosiZ, true));  // Z=80 ≤ 100 安全
            Assert.False(HomeSafetyCheck.IsPositionSafe(120.0, safePosiZ, true)); // Z=120 > 100 不安全
        }

        #endregion

        #region 初始化校验判定

        [Fact]
        public void EvaluateInit_引擎未初始化_失败()
        {
            Assert.False(AxisInitVerifier.EvaluateInit(false, true, true, false));
        }

        [Fact]
        public void EvaluateInit_轴通信异常_失败()
        {
            Assert.False(AxisInitVerifier.EvaluateInit(true, false, true, false));
        }

        [Fact]
        public void EvaluateInit_要求回零但未回零_失败()
        {
            Assert.False(AxisInitVerifier.EvaluateInit(true, true, false, true));
        }

        [Fact]
        public void EvaluateInit_不要求回零_未回零也通过()
        {
            Assert.True(AxisInitVerifier.EvaluateInit(true, true, false, false));
        }

        [Fact]
        public void EvaluateInit_全满足_通过()
        {
            Assert.True(AxisInitVerifier.EvaluateInit(true, true, true, true));
            Assert.True(AxisInitVerifier.EvaluateInit(true, true, true, false));
        }

        #endregion

        #region HomeProfile 参数默认值（配置持久化语义）

        [Fact]
        public void HomeProfile_默认参数对齐源端HomeSettingProfile()
        {
            var node = new Luster.Module.Motion.HomeProfile.Functions.HomeProfile();
            // 默认回零模式 NegativeToZ（源端常见默认）
            Assert.Equal(HomeMode.NegativeToZ, node.HomeMode);
            // 默认检查回零完成
            Assert.True(node.CheckDone);
            // 默认超时 60 秒（对齐源端 BackHomeTimeOut）
            Assert.Equal(60, node.HomeTimeout);
            // 默认覆盖轴参数
            Assert.True(node.OverrideAxisParams);
        }

        [Fact]
        public void HomeProfile_源端参数全部可配置()
        {
            // 验证源端 HomeSettingProfile 7 字段 + 速度/加速度/偏移全部有对应配置项
            var node = new Luster.Module.Motion.HomeProfile.Functions.HomeProfile
            {
                HomeMode = HomeMode.PositiveToZ,
                SearchDirection = true,
                HomeHighEffect = false,
                ReScanEnable = true,
                RetSwOffset = 100,
                HomeSpeedHigh = 1000,
                HomeSpeedLow = 100,
                HomeAcc = 500,
                HomeOffset = 0.5f,
                HomeTimeout = 30
            };
            Assert.Equal(HomeMode.PositiveToZ, node.HomeMode);
            Assert.True(node.SearchDirection);
            Assert.False(node.HomeHighEffect);
            Assert.True(node.ReScanEnable);
            Assert.Equal(100, node.RetSwOffset);
            Assert.Equal(1000u, node.HomeSpeedHigh);
            Assert.Equal(100u, node.HomeSpeedLow);
            Assert.Equal(500u, node.HomeAcc);
            Assert.Equal(0.5f, node.HomeOffset);
            Assert.Equal(30, node.HomeTimeout);
        }

        #endregion
    }
}
