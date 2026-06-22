using System.IO;
using System.Linq;
using FluentAssertions;
using Luster.Module.Motion.AOI.Core.Models;
using Luster.Module.Motion.AOI.Core.Module;
using NUnit.Framework;

namespace Luster.Module.Motion.AOI.Core.Tests
{
    /// <summary>
    /// 站点切换/模块可还原能力测试。
    /// 同一套代码加载 AOI#1/AOI#2/Wipe 三套 profile 应可独立加载且正确切换。
    /// </summary>
    [TestFixture]
    public class AoiCoreModuleTests
    {
        private static string GetSiteProfilesParent()
        {
            // 通过源 SiteProfiles 目录，使用 LoadSiteProfile 时把 BaseDirectory 指向 src/Modules/Luster.Module.Motion.AOI.Core
            var dir = TestContext.CurrentContext.TestDirectory;
            for (int i = 0; i < 8; i++)
            {
                var candidate = Path.Combine(dir, "src", "Modules", "Luster.Module.Motion.AOI.Core");
                if (Directory.Exists(Path.Combine(candidate, "SiteProfiles")))
                {
                    return candidate;
                }

                var parent = Directory.GetParent(dir);
                if (parent == null) break;
                dir = parent.FullName;
            }
            Assert.Fail("找不到 SiteProfiles 源目录。");
            return string.Empty;
        }

        [Test]
        public void All_three_sites_can_be_loaded_independently()
        {
            var baseDir = GetSiteProfilesParent();

            var aoi1 = AoiCoreModule.LoadSiteProfile(AoiSiteType.Aoi1, baseDir);
            var aoi2 = AoiCoreModule.LoadSiteProfile(AoiSiteType.Aoi2, baseDir);
            var wipe = AoiCoreModule.LoadSiteProfile(AoiSiteType.Wipe, baseDir);

            aoi1.SiteType.Should().Be(AoiSiteType.Aoi1);
            aoi2.SiteType.Should().Be(AoiSiteType.Aoi2);
            wipe.SiteType.Should().Be(AoiSiteType.Wipe);

            // 三站差异：handshake 配置路径不同
            aoi1.Handshakes["Upstream"].Should().NotBe(aoi2.Handshakes["Upstream"]);
            aoi1.Handshakes["Upstream"].Should().NotBe(wipe.Handshakes["Upstream"]);
        }

        [Test]
        public void Three_sites_can_be_validated_against_a_built_manifest()
        {
            var baseDir = GetSiteProfilesParent();

            foreach (var siteType in new[] { AoiSiteType.Aoi1, AoiSiteType.Aoi2, AoiSiteType.Wipe })
            {
                var profile = AoiCoreModule.LoadSiteProfile(siteType, baseDir);

                // 构造一个匹配的 manifest（不验证 DLL 物理存在，只验证 SiteType / Version 一致）
                var manifest = new AoiDeploymentManifest
                {
                    PackageVersion = profile.Version,
                    SiteType = profile.SiteType,
                };

                // 把 profile 要求的模块/设备塞进 manifest，以通过依赖项校验
                foreach (var m in profile.MotionModules) manifest.Modules.Add($"{m}.dll");
                foreach (var d in profile.DeviceModules) manifest.Devices.Add($"{d}.dll");

                var result = AoiCoreModule.ValidateProfile(profile, manifest);
                result.IsValid.Should().BeTrue(result.FormatAlarmText());
            }
        }

        [Test]
        public void LoadSiteProfile_with_unknown_site_type_should_throw()
        {
            var baseDir = GetSiteProfilesParent();
            System.Action act = () => AoiCoreModule.LoadSiteProfile(AoiSiteType.Unspecified, baseDir);
            act.Should().Throw<System.ArgumentException>();
        }

        [Test]
        public void ValidateProfile_should_throw_when_invalid()
        {
            var baseDir = GetSiteProfilesParent();
            var profile = AoiCoreModule.LoadSiteProfile(AoiSiteType.Aoi1, baseDir);
            profile.Devices.Clear();
            profile.Handshakes.Clear();

            System.Action act = () => AoiCoreModule.ValidateProfile(profile, null);
            act.Should().Throw<AoiSiteProfileException>();
        }
    }
}
