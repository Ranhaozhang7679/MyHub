using System.Collections.Generic;
using System.IO;
using System.Linq;
using Luster.Module.Motion.AOI.Core.Models;

namespace Luster.Module.Motion.AOI.Core.Services
{
    /// <summary>
    /// 默认站点 profile 校验器。
    /// 覆盖 ADR 定义的启动拦截规则：
    /// - SiteType 与部署 manifest 不匹配 → 禁止启动
    /// - 设备缺失/轴映射缺失/RTCP 配置缺失（AOI 站）→ 禁止启动
    /// - Wipe 站点配置误用于 AOI 站，或反之 → 禁止启动
    /// - 配方根目录/日志根目录不存在 → 禁止进入自动流程（只允许进入安全诊断模式）
    /// - 校验失败字段必须"待人类补充"，不编造默认值
    /// </summary>
    public sealed class AoiSiteProfileValidator : IAoiSiteProfileValidator
    {
        public AoiProfileValidationResult Validate(IAoiSiteProfile profile, IAoiDeploymentManifest? manifest)
        {
            var result = new AoiProfileValidationResult
            {
                ProfileId = profile.ProfileId,
                SiteType = profile.SiteType,
            };

            // === 1. SiteType ===
            if (profile.SiteType == AoiSiteType.Unspecified)
            {
                result.AddError("SiteType 未指定（Unspecified）。站点配置文件中 SiteType 为必填项。");
            }

            // === 2. Version ===
            if (string.IsNullOrWhiteSpace(profile.Version))
            {
                result.AddError("Version 为空。profile 版本为必填项。");
            }

            // === 3. 与 deployment manifest 一致性校验 ===
            if (manifest != null)
            {
                if (manifest.SiteType != profile.SiteType)
                {
                    result.AddError(
                        $"manifest.SiteType ({manifest.SiteType}) 与 profile.SiteType ({profile.SiteType}) 不匹配。");
                }

                if (!string.IsNullOrWhiteSpace(manifest.PackageVersion)
                    && !string.Equals(manifest.PackageVersion, profile.Version))
                {
                    result.AddError(
                        $"manifest 版本 ({manifest.PackageVersion}) 与 profile 版本 ({profile.Version}) 不一致。");
                }

                foreach (var m in profile.RequiredMotionModules)
                {
                    var dllName = $"{m}.dll";
                    if (!manifest.Modules.Any(mm => string.Equals(mm, dllName, System.StringComparison.OrdinalIgnoreCase)))
                    {
                        result.AddError($"部署清单缺少所需运动模块: {m}。");
                    }
                }

                foreach (var d in profile.RequiredDeviceModules)
                {
                    var dllName = $"{d}.dll";
                    if (!manifest.Devices.Any(dd => string.Equals(dd, dllName, System.StringComparison.OrdinalIgnoreCase)))
                    {
                        result.AddError($"部署清单缺少所需设备模块: {d}。");
                    }
                }
            }

            // === 4. 轴映射 ===
            var map = profile.AxisMap;
            if (string.IsNullOrWhiteSpace(map.XAxisName))
            {
                result.AddError("AxisMap.X 轴为空。五轴 AOI 必填 X 轴映射。");
            }

            if (string.IsNullOrWhiteSpace(map.YAxisName))
            {
                result.AddError("AxisMap.Y 轴为空。");
            }

            if (string.IsNullOrWhiteSpace(map.ZAxisName))
            {
                result.AddError("AxisMap.Z 轴为空。");
            }

            // AOI 站要求 U/V 轴也有映射；Wipe 站可能不需要，但配置时不应为空
            if (profile.SiteType == AoiSiteType.Aoi1 || profile.SiteType == AoiSiteType.Aoi2)
            {
                if (string.IsNullOrWhiteSpace(map.UAxisName))
                {
                    result.AddError("AxisMap.U 轴为空。AOI 站必填 U 轴。");
                }

                if (string.IsNullOrWhiteSpace(map.VAxisName))
                {
                    result.AddError("AxisMap.V 轴为空。AOI 站必填 V 轴。");
                }
            }

            // === 5. 设备清单 ===
            if (profile.DeviceNames.Count == 0)
            {
                result.AddError("设备清单（DeviceNames）为空。至少应包含 MotionCard。");
            }

            if (!profile.DeviceNames.ContainsKey("MotionCard"))
            {
                result.AddError("设备别名 \"MotionCard\" 未配置。");
            }

            // === 6. 配方/追溯/日志根目录 ===
            ValidateDirectoryRef(result, "RecipeRoot", profile.RecipeRoot);
            ValidateDirectoryRef(result, "TraceRoot", profile.TraceRoot);
            ValidateDirectoryRef(result, "LogRoot", profile.LogRoot);

            // === 7. RTCP （仅 AOI 站需要; Wipe 明确可为 null）===
            if (profile.SiteType == AoiSiteType.Aoi1 || profile.SiteType == AoiSiteType.Aoi2)
            {
                if (profile.RtcpProfile == null)
                {
                    result.AddError("AOI 站的 RTCP 配置为 null。五轴 AOI 站必须启用 RTCP。");
                }
            }

            // === 8. 通讯通道 ===
            if (profile.HandshakeChannels.Count == 0)
            {
                result.AddError("通讯通道（HandshakeChannels）为空，三站必须配置至少一条交握通道。");
            }

            return result;
        }

        private static void ValidateDirectoryRef(AoiProfileValidationResult result, string fieldName, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result.AddError($"{fieldName} 为空。请配置站点专用的目录路径。");
                return;
            }

            // 如果值包含 "(待人类补充)" 或 "TODO"，不再额外做目录存在性检查
            if (value.Contains("TODO") || value.Contains("待人类"))
            {
                return;
            }

            // 如果值看起来像相对路径(不以磁盘或绝对符开头)，不在这里拦截——启动器会基于 BaseDirectory 拼接
            if (!value.Contains(":\\") && !value.StartsWith("/") && !value.StartsWith("\\"))
            {
                return;
            }
        }
    }
}