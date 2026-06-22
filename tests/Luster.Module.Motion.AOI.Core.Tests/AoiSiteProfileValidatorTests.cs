using FluentAssertions;
using Luster.Module.Motion.AOI.Core.Models;
using Luster.Module.Motion.AOI.Core.Services;
using NUnit.Framework;

namespace Luster.Module.Motion.AOI.Core.Tests
{
    /// <summary>
    /// 站点 profile 校验器测试。
    /// 覆盖启动拦截规则：SiteType 缺失、轴映射缺失、版本不一致、Wipe 误用、RTCP 缺失等。
    /// </summary>
    [TestFixture]
    public class AoiSiteProfileValidatorTests
    {
        private static AoiSiteProfile NewValidAoi1()
        {
            var p = new AoiSiteProfile
            {
                ProfileId = "AOI1-T",
                Version = "1.0.0",
                SiteType = AoiSiteType.Aoi1,
                RecipeRoot = "Config/Recipes/AOI1",
                TraceRoot = "Config/Traces/AOI1",
                LogRoot = "Logs/AOI1",
                CardConfigPath = "Config/Card/AOI1",
                EntryStation = "AOI#1",
            };
            p.MotionModules.Add("Luster.Module.Motion.AOI.Core");
            p.DeviceModules.Add("Luster.SimDevice.MotionCard.ZMotion");
            p.Devices["MotionCard"] = "ZMotion";
            p.Axes.XAxisName = "X";
            p.Axes.YAxisName = "Y";
            p.Axes.ZAxisName = "Z";
            p.Axes.UAxisName = "U";
            p.Axes.VAxisName = "V";
            p.Axes.Channels["X"] = 0;
            p.Rtcp = new FiveAxisRtcpProfile { CoordinateSystem = 0 };
            p.Handshakes["Upstream"] = "Config/HS/AOI1/up.csv";
            return p;
        }

        [Test]
        public void Valid_aoi1_profile_should_pass()
        {
            var validator = new AoiSiteProfileValidator();
            var result = validator.Validate(NewValidAoi1(), null);

            result.IsValid.Should().BeTrue(result.FormatAlarmText());
        }

        [Test]
        public void Missing_SiteType_should_be_intercepted()
        {
            var profile = NewValidAoi1();
            profile.SiteType = AoiSiteType.Unspecified;

            var validator = new AoiSiteProfileValidator();
            var result = validator.Validate(profile, null);

            result.IsValid.Should().BeFalse();
            string.Join("\n", result.Errors).Should().Contain("SiteType");
        }

        [Test]
        public void AOI_missing_U_or_V_axis_should_be_intercepted()
        {
            var profile = NewValidAoi1();
            profile.Axes.UAxisName = string.Empty;

            var validator = new AoiSiteProfileValidator();
            var result = validator.Validate(profile, null);

            result.IsValid.Should().BeFalse();
            string.Join("\n", result.Errors).Should().Contain("AxisMap.U");
        }

        [Test]
        public void AOI_missing_RTCP_should_be_intercepted()
        {
            var profile = NewValidAoi1();
            profile.Rtcp = null;

            var validator = new AoiSiteProfileValidator();
            var result = validator.Validate(profile, null);

            result.IsValid.Should().BeFalse();
            string.Join("\n", result.Errors).Should().Contain("RTCP");
        }

        [Test]
        public void Wipe_without_RTCP_should_still_pass()
        {
            var profile = NewValidAoi1();
            profile.SiteType = AoiSiteType.Wipe;
            profile.Rtcp = null;
            // Wipe 站不要求 U/V
            profile.Axes.UAxisName = string.Empty;
            profile.Axes.VAxisName = string.Empty;

            var validator = new AoiSiteProfileValidator();
            var result = validator.Validate(profile, null);

            result.IsValid.Should().BeTrue(result.FormatAlarmText());
        }

        [Test]
        public void Manifest_site_type_mismatch_should_be_intercepted()
        {
            var profile = NewValidAoi1();
            var manifest = new AoiDeploymentManifest
            {
                PackageVersion = "1.0.0",
                SiteType = AoiSiteType.Wipe,  // 与 profile.SiteType=Aoi1 不匹配
            };

            var validator = new AoiSiteProfileValidator();
            var result = validator.Validate(profile, manifest);

            result.IsValid.Should().BeFalse();
            string.Join("\n", result.Errors).Should().Contain("SiteType");
        }

        [Test]
        public void Manifest_version_mismatch_should_be_intercepted()
        {
            var profile = NewValidAoi1();
            var manifest = new AoiDeploymentManifest
            {
                PackageVersion = "0.9.0",
                SiteType = AoiSiteType.Aoi1,
            };

            var validator = new AoiSiteProfileValidator();
            var result = validator.Validate(profile, manifest);

            result.IsValid.Should().BeFalse();
            string.Join("\n", result.Errors).Should().Contain("版本");
        }

        [Test]
        public void Missing_MotionCard_device_should_be_intercepted()
        {
            var profile = NewValidAoi1();
            profile.Devices.Clear();

            var validator = new AoiSiteProfileValidator();
            var result = validator.Validate(profile, null);

            result.IsValid.Should().BeFalse();
            string.Join("\n", result.Errors).Should().Contain("MotionCard");
        }

        [Test]
        public void Missing_handshake_channels_should_be_intercepted()
        {
            var profile = NewValidAoi1();
            profile.Handshakes.Clear();

            var validator = new AoiSiteProfileValidator();
            var result = validator.Validate(profile, null);

            result.IsValid.Should().BeFalse();
            string.Join("\n", result.Errors).Should().Contain("通讯");
        }

        [Test]
        public void FormatAlarmText_should_list_all_errors()
        {
            var profile = NewValidAoi1();
            profile.SiteType = AoiSiteType.Unspecified;
            profile.Axes.XAxisName = string.Empty;
            profile.Devices.Clear();

            var validator = new AoiSiteProfileValidator();
            var result = validator.Validate(profile, null);

            var text = result.FormatAlarmText();
            text.Should().Contain("SiteType");
            text.Should().Contain("AxisMap.X");
            text.Should().Contain("MotionCard");
            result.Errors.Count.Should().BeGreaterThan(2);
        }
    }
}
