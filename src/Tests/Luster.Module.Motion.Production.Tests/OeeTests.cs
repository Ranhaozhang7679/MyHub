using Luster.Module.Motion.Production.Oee;
using System.Collections.Generic;
using Xunit;

namespace Luster.Module.Motion.ProductionTests
{
    /// <summary>
    /// TES-33 P8-E:OEE/CT/UPH/良率/设备健康/红黄预警 单测（纯逻辑）。
    /// </summary>
    public class OeeTests
    {
        #region OeeCalculator

        [Fact]
        public void Calculate_三因子乘积为OEE()
        {
            // A=0.8 P=0.9 Q=0.95 → OEE=0.684
            var r = OeeCalculator.Calculate(runTime: 800, stopTime: 100, idleTime: 50, pmTime: 50,
                actualRunTime: 720, okCount: 95, totalCount: 100);

            Assert.Equal(0.8, r.Availability, 3);    // 800/(800+100+50+50)=0.8
            Assert.Equal(0.9, r.Performance, 3);      // 720/800=0.9
            Assert.Equal(0.95, r.Quality, 3);          // 95/100
            Assert.Equal(0.684, r.Oee, 3);             // 0.8*0.9*0.95
        }

        [Fact]
        public void Calculate_全停机OEE为0()
        {
            var r = OeeCalculator.Calculate(0, 1000, 0, 0, 0, 0, 0);
            Assert.Equal(0, r.Availability);
            Assert.Equal(0, r.Oee);
        }

        [Fact]
        public void Calculate_计划时间为0_Availability为0不除零()
        {
            var r = OeeCalculator.Calculate(0, 0, 0, 0, 0, 10, 10);
            Assert.Equal(0, r.Availability);
            Assert.Equal(1, r.Quality); // 全 OK
        }

        [Fact]
        public void Calculate_Performance不超1()
        {
            // actualRunTime > runTime 时 Performance 钳到 1
            var r = OeeCalculator.Calculate(100, 0, 0, 0, actualRunTime: 150, okCount: 100, totalCount: 100);
            Assert.Equal(1, r.Performance);
        }

        [Fact]
        public void ToDashboard_百分比展示()
        {
            var r = OeeCalculator.Calculate(800, 100, 50, 50, 720, 95, 100);
            string dash = r.ToDashboard();
            Assert.Contains("OEE=", dash);
            Assert.Contains("A=", dash);
            Assert.Contains("OK=95/100", dash);
        }

        [Fact]
        public void ToPercent_0到100转换()
        {
            Assert.Equal(68.4, OeeCalculator.ToPercent(0.684));
            Assert.Equal(100, OeeCalculator.ToPercent(1.0));
            Assert.Equal(0, OeeCalculator.ToPercent(0));
        }

        #endregion

        #region DeviceHealthService

        [Theory]
        [InlineData(50, DeviceHealthLevel.Green)]
        [InlineData(79.9, DeviceHealthLevel.Green)]
        [InlineData(80, DeviceHealthLevel.Yellow)]
        [InlineData(94.9, DeviceHealthLevel.Yellow)]
        [InlineData(95, DeviceHealthLevel.Red)]
        [InlineData(100, DeviceHealthLevel.Red)]
        public void EvaluateHealth_红黄绿阈值(double usedPercent, DeviceHealthLevel expected)
        {
            Assert.Equal(expected, DeviceHealthService.EvaluateHealth(usedPercent));
        }

        [Fact]
        public void EvaluateHealth_自定义阈值()
        {
            Assert.Equal(DeviceHealthLevel.Yellow, DeviceHealthService.EvaluateHealth(70, yellowThreshold: 70, redThreshold: 90));
            Assert.Equal(DeviceHealthLevel.Red, DeviceHealthService.EvaluateHealth(90, yellowThreshold: 70, redThreshold: 90));
        }

        [Fact]
        public void BuildTip_红预警含立即保养()
        {
            var tip = DeviceHealthService.BuildTip("气缸A", 96, 4);
            Assert.Equal(DeviceHealthLevel.Red, tip.Level);
            Assert.Contains("气缸A", tip.Message);
            Assert.Contains("立即保养", tip.Message);
        }

        [Fact]
        public void BuildTip_黄预警含安排保养()
        {
            var tip = DeviceHealthService.BuildTip("电机B", 85, 15);
            Assert.Equal(DeviceHealthLevel.Yellow, tip.Level);
            Assert.Contains("安排保养", tip.Message);
        }

        [Fact]
        public void BuildTip_绿不预警()
        {
            var tip = DeviceHealthService.BuildTip("电机B", 50, 50);
            Assert.Equal(DeviceHealthLevel.Green, tip.Level);
            Assert.Contains("健康", tip.Message);
        }

        [Fact]
        public void EvaluateBatch_只返回非Green()
        {
            var percents = new Dictionary<string, double>
            {
                { "设备A", 50 },
                { "设备B", 85 },
                { "设备C", 96 }
            };
            var tips = DeviceHealthService.EvaluateBatch(percents);
            Assert.Equal(2, tips.Count); // B黄 C红，A绿不返回
        }

        #endregion

        #region AlertRuleEngine

        [Fact]
        public void EvaluateMetric_Above模式_CT超红线()
        {
            // CT 越高越危险：黄3 红5
            var level = AlertRuleEngine.EvaluateMetric("CT", 6.0, 3.0, 5.0, AlertCompareMode.Above);
            Assert.Equal(AlertLevel.Red, level);

            var yellow = AlertRuleEngine.EvaluateMetric("CT", 4.0, 3.0, 5.0, AlertCompareMode.Above);
            Assert.Equal(AlertLevel.Yellow, yellow);

            var green = AlertRuleEngine.EvaluateMetric("CT", 2.0, 3.0, 5.0, AlertCompareMode.Above);
            Assert.Equal(AlertLevel.Green, green);
        }

        [Fact]
        public void EvaluateMetric_Below模式_良率低于红线()
        {
            // 良率越低越危险：黄0.98 红0.95
            var red = AlertRuleEngine.EvaluateMetric("Yield", 0.90, 0.98, 0.95, AlertCompareMode.Below);
            Assert.Equal(AlertLevel.Red, red);

            var yellow = AlertRuleEngine.EvaluateMetric("Yield", 0.96, 0.98, 0.95, AlertCompareMode.Below);
            Assert.Equal(AlertLevel.Yellow, yellow);

            var green = AlertRuleEngine.EvaluateMetric("Yield", 0.99, 0.98, 0.95, AlertCompareMode.Below);
            Assert.Equal(AlertLevel.Green, green);
        }

        [Fact]
        public void EvaluateAll_多规则红优先于黄()
        {
            var rules = new List<AlertRule>
            {
                new AlertRule("CT", 3.0, 5.0, AlertCompareMode.Above),
                new AlertRule("Yield", 0.98, 0.95, AlertCompareMode.Below),
            };
            var metrics = new Dictionary<string, double>
            {
                { "CT", 6.0 },      // 红
                { "Yield", 0.96 },  // 黄
            };

            var alerts = AlertRuleEngine.EvaluateAll(rules, metrics);

            Assert.Equal(2, alerts.Count);
            Assert.Equal(AlertLevel.Red, alerts[0].Level); // 红排前
            Assert.Equal(AlertLevel.Yellow, alerts[1].Level);
        }

        [Fact]
        public void EvaluateAll_禁用规则不评估()
        {
            var rule = new AlertRule("CT", 3.0, 5.0, AlertCompareMode.Above) { Enable = false };
            var metrics = new Dictionary<string, double> { { "CT", 10.0 } };

            var alerts = AlertRuleEngine.EvaluateAll(new[] { rule }, metrics);

            Assert.Empty(alerts);
        }

        [Fact]
        public void EvaluateAll_无指标值不评估()
        {
            var rule = new AlertRule("CT", 3.0, 5.0, AlertCompareMode.Above);
            var metrics = new Dictionary<string, double> { { "Yield", 0.9 } }; // 无 CT

            var alerts = AlertRuleEngine.EvaluateAll(new[] { rule }, metrics);

            Assert.Empty(alerts);
        }

        [Fact]
        public void DefaultRules_含CT_Yield_OEE()
        {
            var rules = AlertRuleEngine.DefaultRules();
            Assert.Equal(3, rules.Count);
            Assert.Contains(rules, r => r.Metric == "CT");
            Assert.Contains(rules, r => r.Metric == "Yield");
            Assert.Contains(rules, r => r.Metric == "OEE");
        }

        [Fact]
        public void DefaultRules_OEE硬指标对齐()
        {
            // OEE 黄60% 红40%（Below）
            var rules = AlertRuleEngine.DefaultRules();
            var oeeRule = System.Array.Find(System.Linq.Enumerable.ToArray(rules), r => r.Metric == "OEE");
            Assert.Equal(AlertCompareMode.Below, oeeRule.CompareMode);
            Assert.Equal(0.60, oeeRule.YellowThreshold);
            Assert.Equal(0.40, oeeRule.RedThreshold);
        }

        [Fact]
        public void AlertRecord_消息含阈值()
        {
            var alerts = AlertRuleEngine.EvaluateAll(
                new[] { new AlertRule("CT", 3.0, 5.0, AlertCompareMode.Above) },
                new Dictionary<string, double> { { "CT", 6.0 } });

            Assert.Single(alerts);
            Assert.Contains("CT=6.00", alerts[0].Message);
            Assert.Contains("红线", alerts[0].Message);
        }

        #endregion
    }
}
