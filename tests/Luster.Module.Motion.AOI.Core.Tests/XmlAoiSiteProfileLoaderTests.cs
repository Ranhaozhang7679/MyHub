using System.IO;
using FluentAssertions;
using Luster.Module.Motion.AOI.Core.Models;
using Luster.Module.Motion.AOI.Core.Services;
using NUnit.Framework;

namespace Luster.Module.Motion.AOI.Core.Tests
{
    /// <summary>
    /// 站点 profile 加载器的反序列化测试。
    /// 同一个 XML profile 通过加载器解析后，应得到一致的 SiteType / 模块清单 / 轴映射等。
    /// </summary>
    [TestFixture]
    public class XmlAoiSiteProfileLoaderTests
    {
        private static string GetSiteProfilesRoot()
        {
            // 仓库结构：tests/Luster.Module.Motion.AOI.Core.Tests/bin/...
            // 通过 SiteProfiles 源文件而非 bin 副本读取，便于测试无 CopyToOutput
            var dir = TestContext.CurrentContext.TestDirectory;
            for (int i = 0; i < 8; i++)
            {
                if (Directory.Exists(Path.Combine(dir, "src", "Modules", "Luster.Module.Motion.AOI.Core", "SiteProfiles")))
                {
                    return Path.Combine(dir, "src", "Modules", "Luster.Module.Motion.AOI.Core", "SiteProfiles");
                }
                var parent = Directory.GetParent(dir);
                if (parent == null) break;
                dir = parent.FullName;
            }
            Assert.Fail("找不到 SiteProfiles 源目录。");
            return string.Empty;
        }

        [Test]
        public void Load_AOI1_profile_should_parse_all_fields()
        {
            var loader = new XmlAoiSiteProfileLoader();
            var path = Path.Combine(GetSiteProfilesRoot(), "AOI1", "site-profile.xml");

            var profile = loader.LoadFromFile(path);

            profile.SiteType.Should().Be(AoiSiteType.Aoi1);
            profile.ProfileId.Should().Be("AOI1-PROD-V1");
            profile.Version.Should().Be("1.0.0");
            profile.MotionModules.Should().Contain("Luster.Module.Motion.AOI.Core");
            profile.DeviceModules.Should().Contain("Luster.SimDevice.MotionCard.ZMotion");
            profile.Devices.Should().ContainKey("MotionCard");
            profile.Axes.XAxisName.Should().Be("X");
            profile.Axes.VAxisName.Should().Be("V");
            profile.Rtcp.Should().NotBeNull();
            profile.Handshakes.Should().ContainKey("Upstream");
            profile.EntryStation.Should().Be("AOI#1 金属外观检测");
        }

        [Test]
        public void Load_AOI2_profile_should_have_different_handshake_paths()
        {
            var loader = new XmlAoiSiteProfileLoader();
            var aoi1 = loader.LoadFromFile(Path.Combine(GetSiteProfilesRoot(), "AOI1", "site-profile.xml"));
            var aoi2 = loader.LoadFromFile(Path.Combine(GetSiteProfilesRoot(), "AOI2", "site-profile.xml"));

            aoi1.Handshakes["Upstream"].Should().NotBe(aoi2.Handshakes["Upstream"]);
            aoi2.SiteType.Should().Be(AoiSiteType.Aoi2);
        }

        [Test]
        public void Load_Wipe_profile_should_have_no_RTCP_and_no_UV_axes()
        {
            var loader = new XmlAoiSiteProfileLoader();
            var path = Path.Combine(GetSiteProfilesRoot(), "Wipe", "site-profile.xml");

            var profile = loader.LoadFromFile(path);

            profile.SiteType.Should().Be(AoiSiteType.Wipe);
            profile.Rtcp.Should().BeNull("Wipe 站不需要五轴 RTCP");
            profile.Axes.UAxisName.Should().BeEmpty("Wipe 站不需要 U 轴");
            profile.DeviceModules.Should().NotContain("Luster.SimDevice.Camera.LusterCamera",
                "Wipe 站不需要相机模块");
        }

        [Test]
        public void Load_nonexistent_file_should_throw_AoiSiteProfileException()
        {
            var loader = new XmlAoiSiteProfileLoader();
            System.Action act = () => loader.LoadFromFile(Path.Combine(Path.GetTempPath(), "nope-profile.xml"));

            act.Should()
                .Throw<AoiSiteProfileException>()
                .Where(ex => ex.ValidationResult.Errors.Count > 0);
        }

        [Test]
        public void Load_invalid_root_should_throw_AoiSiteProfileException()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"bad-{System.Guid.NewGuid():N}.xml");
            File.WriteAllText(tempPath, "<NotAoiSiteProfile />");
            try
            {
                var loader = new XmlAoiSiteProfileLoader();
                System.Action act = () => loader.LoadFromFile(tempPath);
                act.Should().Throw<AoiSiteProfileException>();
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }
    }
}
