using System.Collections.Generic;

namespace Luster.Module.Motion.AOI.Core.Models
{
    /// <summary>
    /// 站点 profile。同一套代码运行时通过加载不同 profile 适配 AOI#1/AOI#2/Wipe。
    /// 三站差异必须沉到 profile 中，禁止复制三套分叉代码。
    /// </summary>
    public interface IAoiSiteProfile
    {
        /// <summary>profile 唯一标识，例如 "AOI1-V2.0.0.3"。</summary>
        string ProfileId { get; }

        /// <summary>站点类型。</summary>
        AoiSiteType SiteType { get; }

        /// <summary>profile 版本，需与部署清单 PackageVersion 一致。</summary>
        string Version { get; }

        /// <summary>启用的运控业务模块清单（落 Motions/ 目录）。</summary>
        IReadOnlyList<string> RequiredMotionModules { get; }

        /// <summary>启用的设备适配模块清单（落 Devices/ 目录）。</summary>
        IReadOnlyList<string> RequiredDeviceModules { get; }

        /// <summary>设备别名 → 物理设备名称。</summary>
        IReadOnlyDictionary<string, string> DeviceNames { get; }

        /// <summary>X/Y/Z/U/V 轴 → 卡轴号映射。</summary>
        IAoiAxisMap AxisMap { get; }

        /// <summary>五轴 RTCP 配置。Wipe 站点可为 null。</summary>
        IFiveAxisRtcpProfile? RtcpProfile { get; }

        /// <summary>通讯端口（上游/下游/ICW 等通道名 → 配置文件路径）。</summary>
        IReadOnlyDictionary<string, string> HandshakeChannels { get; }

        /// <summary>配方根目录（相对路径，相对 BaseDirectory）。</summary>
        string RecipeRoot { get; }

        /// <summary>追溯/日志根目录。</summary>
        string TraceRoot { get; }

        /// <summary>日志根目录。</summary>
        string LogRoot { get; }

        /// <summary>板卡配置包路径。</summary>
        string CardConfigPath { get; }

        /// <summary>站点入口（启动后默认进入的工站模块名）。</summary>
        string EntryStation { get; }
    }
}
