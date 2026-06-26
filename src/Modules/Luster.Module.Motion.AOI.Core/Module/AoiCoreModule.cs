using Luster.Module.Motion.AOI.Core.Models;
using Luster.Module.Motion.AOI.Core.Services;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Luster.Module.Motion.AOI.Core.Module
{
    /// <summary>
    /// AOI 站点 profile 模块。
    /// 加载现有站点 profile 数据模型、校验器、加载器，并注册占位 Function 供 ModuleFactory 发现。
    ///
    /// 站点 profile 校验应在自动流程启动前执行（通过 <see cref="IAoiSiteProfileValidator"/>）。
    /// 本模块仅负责装载接口/服务到 IoC，校验逻辑在 <see cref="AoiSiteProfileValidator"/>。
    /// </summary>
    public class AoiCoreModule : MotionModule
    {
        private const string SiteProfilesRoot = "SiteProfiles";

        /// <summary>
        /// 注册本模块的函数节点。
        /// </summary>
        public override void InitFunctions()
        {
            // 注册占位 Function 确保模块可被发现
            AddFunction<AoiSiteProfileFunction>();
        }

        /// <summary>
        /// 加载指定站点类型的 profile。
        /// 启动链应在自动流程开始前调用此方法并校验结果。
        /// </summary>
        public static AoiSiteProfile LoadSiteProfile(AoiSiteType siteType, string baseDirectory)
        {
            var subDir = siteType switch
            {
                AoiSiteType.Aoi1 => "AOI1",
                AoiSiteType.Aoi2 => "AOI2",
                AoiSiteType.Wipe => "Wipe",
                _ => throw new ArgumentException($"不支持的站点类型: {siteType}", nameof(siteType)),
            };

            var profilePath = Path.Combine(baseDirectory, SiteProfilesRoot, subDir, "site-profile.xml");
            var loader = new XmlAoiSiteProfileLoader();
            return loader.LoadFromFile(profilePath);
        }

        /// <summary>
        /// 校验 profile 并抛出异常（启动拦截使用）。
        /// </summary>
        public static AoiProfileValidationResult ValidateProfile(
            AoiSiteProfile profile,
            IAoiDeploymentManifest? manifest = null)
        {
            var validator = new AoiSiteProfileValidator();
            var result = validator.Validate(profile, manifest);

            if (!result.IsValid)
            {
                throw new AoiSiteProfileException(result);
            }

            return result;
        }

        /// <summary>
        /// 基于 BaseDirectory 下的 Motions/ 和 Devices/ 目录自动生成部署清单。
        /// </summary>
        public static AoiDeploymentManifest BuildManifest(
            AoiSiteType siteType,
            string packageVersion,
            string baseDirectory)
        {
            var motionsDir = Path.Combine(baseDirectory, "Motions");
            var devicesDir = Path.Combine(baseDirectory, "Devices");

            var manifest = new AoiDeploymentManifest
            {
                PackageVersion = packageVersion,
                SiteType = siteType,
            };

            if (Directory.Exists(motionsDir))
            {
                manifest.Modules.AddRange(
                    Directory.GetFiles(motionsDir, "*.dll")
                        .Select(Path.GetFileName)
                        .Where(x => x != null)
                        .Cast<string>());
            }

            if (Directory.Exists(devicesDir))
            {
                manifest.Devices.AddRange(
                    Directory.GetFiles(devicesDir, "*.dll")
                        .Select(Path.GetFileName)
                        .Where(x => x != null)
                        .Cast<string>());
            }

            return manifest;
        }
    }

    /// <summary>
    /// 模块 Creator，供 ModuleFactory.LoadModules 自动发现。
    /// </summary>
    public class AoiCoreCreator : MotionModuleCreator<AoiCoreModule>
    {
        public override int Sort => 10;

        public override string Icon => "\xe702";

        public override string Alias => "AOI/Core";

        public override string Tips => "AOI/Core 站点 profile 与基础设施";
    }
}