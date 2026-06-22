using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Luster.Module.Motion.AOI.Core.Models;

namespace Luster.Module.Motion.AOI.Core.Services
{
    /// <summary>
    /// 基于 <see cref="XDocument"/> 的 site-profile XML 解析实现。
    /// 解析阶段只做结构反序列化；缺字段/语义不一致由 <see cref="AoiSiteProfileValidator"/> 报错。
    /// </summary>
    public sealed class XmlAoiSiteProfileLoader : IAoiSiteProfileLoader
    {
        public AoiSiteProfile LoadFromFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new AoiSiteProfileException(new AoiProfileValidationResult
                {
                    ProfileId = "<null>",
                }.With(r => r.AddError("profile 路径为空。")));
            }

            if (!File.Exists(path))
            {
                throw new AoiSiteProfileException(new AoiProfileValidationResult
                {
                    ProfileId = path,
                }.With(r => r.AddError($"未找到 site-profile 文件: {path}")));
            }

            XDocument doc;
            try
            {
                doc = XDocument.Load(path, LoadOptions.SetLineInfo);
            }
            catch (XmlException ex)
            {
                var r = new AoiProfileValidationResult { ProfileId = path };
                r.AddError($"site-profile XML 解析失败: {ex.Message}");
                throw new AoiSiteProfileException(r);
            }

            var root = doc.Root;
            if (root == null || root.Name.LocalName != "AoiSiteProfile")
            {
                var r = new AoiProfileValidationResult { ProfileId = path };
                r.AddError("根元素必须为 <AoiSiteProfile>。");
                throw new AoiSiteProfileException(r);
            }

            var profile = new AoiSiteProfile
            {
                ProfileId = (string?)root.Attribute("ProfileId") ?? string.Empty,
                Version = (string?)root.Attribute("Version") ?? string.Empty,
                SiteType = ParseSiteType((string?)root.Attribute("SiteType")),
                RecipeRoot = (string?)root.Element("RecipeRoot") ?? string.Empty,
                TraceRoot = (string?)root.Element("TraceRoot") ?? string.Empty,
                LogRoot = (string?)root.Element("LogRoot") ?? string.Empty,
                CardConfigPath = (string?)root.Element("CardConfigPath") ?? string.Empty,
                EntryStation = (string?)root.Element("EntryStation") ?? string.Empty,
            };

            foreach (var m in root.Elements("MotionModules").Elements("Module"))
            {
                var name = (string?)m.Attribute("Name");
                if (!string.IsNullOrWhiteSpace(name))
                {
                    profile.MotionModules.Add(name!);
                }
            }

            foreach (var d in root.Elements("DeviceModules").Elements("Module"))
            {
                var name = (string?)d.Attribute("Name");
                if (!string.IsNullOrWhiteSpace(name))
                {
                    profile.DeviceModules.Add(name!);
                }
            }

            foreach (var dev in root.Elements("Devices").Elements("Device"))
            {
                var alias = (string?)dev.Attribute("Alias");
                var name = (string?)dev.Attribute("Name");
                if (!string.IsNullOrWhiteSpace(alias) && !string.IsNullOrWhiteSpace(name))
                {
                    profile.Devices[alias!] = name!;
                }
            }

            var axisMapElement = root.Element("AxisMap");
            if (axisMapElement != null)
            {
                profile.Axes.XAxisName = (string?)axisMapElement.Attribute("X") ?? string.Empty;
                profile.Axes.YAxisName = (string?)axisMapElement.Attribute("Y") ?? string.Empty;
                profile.Axes.ZAxisName = (string?)axisMapElement.Attribute("Z") ?? string.Empty;
                profile.Axes.UAxisName = (string?)axisMapElement.Attribute("U") ?? string.Empty;
                profile.Axes.VAxisName = (string?)axisMapElement.Attribute("V") ?? string.Empty;

                foreach (var ch in axisMapElement.Elements("Channel"))
                {
                    var name = (string?)ch.Attribute("Name");
                    var idStr = (string?)ch.Attribute("CardAxis");
                    if (!string.IsNullOrWhiteSpace(name)
                        && int.TryParse(idStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
                    {
                        profile.Axes.Channels[name!] = id;
                    }
                }
            }

            var rtcpElement = root.Element("Rtcp");
            if (rtcpElement != null)
            {
                var rtcp = new FiveAxisRtcpProfile
                {
                    CoordinateSystem = ParseIntOrDefault((string?)rtcpElement.Attribute("CoordinateSystem"), 0),
                    RotationCenter = ParseXyz(rtcpElement.Element("RotationCenter")),
                    ToolCenterPoint = ParseXyz(rtcpElement.Element("ToolCenterPoint")),
                };

                foreach (var v in rtcpElement.Elements("Virtual").Elements("Axis"))
                {
                    rtcp.Virtual.Add(((string?)v.Attribute("Name")) ?? string.Empty);
                }

                foreach (var v in rtcpElement.Elements("Real").Elements("Axis"))
                {
                    rtcp.Real.Add(((string?)v.Attribute("Name")) ?? string.Empty);
                }

                profile.Rtcp = rtcp;
            }

            foreach (var hs in root.Elements("Handshakes").Elements("Channel"))
            {
                var key = (string?)hs.Attribute("Key");
                var file = (string?)hs.Attribute("Config");
                if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(file))
                {
                    profile.Handshakes[key!] = file!;
                }
            }

            return profile;
        }

        private static AoiSiteType ParseSiteType(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return AoiSiteType.Unspecified;
            }

            return Enum.TryParse<AoiSiteType>(raw, ignoreCase: true, out var v)
                ? v
                : AoiSiteType.Unspecified;
        }

        private static int ParseIntOrDefault(string? raw, int defaultValue)
        {
            return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)
                ? v
                : defaultValue;
        }

        private static (double X, double Y, double Z) ParseXyz(XElement? element)
        {
            if (element == null)
            {
                return (0, 0, 0);
            }

            double Parse(string name) =>
                double.TryParse((string?)element.Attribute(name),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out var v)
                    ? v
                    : 0d;

            return (Parse("X"), Parse("Y"), Parse("Z"));
        }
    }

    internal static class FluentExtensions
    {
        // 让单行错误注入更紧凑，避免 throw 之前两行临时变量。
        public static T With<T>(this T self, Action<T> action) where T : class
        {
            action(self);
            return self;
        }
    }
}
