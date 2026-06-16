using Luster.Common.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Luster.SimDevice.MotionCard.LC
{
    /// <summary>
    /// LC 板卡 DLL 版本管理器。
    /// 支持两种模式：
    /// 1. 手动模式：ActiveVersion 指定使用哪个 DLL
    /// 2. 自动匹配：读取固件版本后，从 Versions[] 中自动查找匹配的 DLL 条目
    /// </summary>
    public static class LCDllManager
    {
        #region Win32 API

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        #endregion

        #region 数据模型

        private class LCVersionConfig
        {
            public string ActiveVersion { get; set; }
            public LCVersionEntry[] Versions { get; set; }
        }

        public class LCVersionEntry
        {
            public string Id { get; set; }
            public string DllSubPath { get; set; }
            public string Description { get; set; }
            public string ExpectedDllVersion { get; set; }
            public string ExpectedFpgaVersion { get; set; }
            public string ExpectedDspVersion { get; set; }
        }

        #endregion

        private static readonly object _lock = new object();
        private static bool _isLoaded = false;
        private static LCVersionConfig _config;
        private static LCVersionEntry _activeEntry;
        private static string _configError = "";

        /// <summary>当前加载的 DLL 版本描述</summary>
        public static string ActiveDescription => _activeEntry?.Description ?? "";

        /// <summary>当前 ActiveVersion 对应的 Id</summary>
        public static string ActiveVersionId => _activeEntry?.Id ?? "";

        /// <summary>通过 M_GetVersion 读取的 DLL 版本号</summary>
        public static string DllVersion { get; private set; } = "";

        /// <summary>通过 M_GetVersion 读取的 FPGA 版本号</summary>
        public static string FpgaVersion { get; private set; } = "";

        /// <summary>通过 M_GetVersion 读取的 DSP 版本号</summary>
        public static string DspVersion { get; private set; } = "";

        /// <summary>授权信息</summary>
        public static string Authorization { get; private set; } = "";

        /// <summary>版本是否匹配</summary>
        public static bool IsVersionMatched { get; private set; } = true;

        /// <summary>版本校验结果描述</summary>
        public static string VersionInfo { get; private set; } = "";

        /// <summary>配置加载错误信息</summary>
        public static string ConfigError => _configError;

        /// <summary>自动匹配到的条目（与当前加载的不同时需要重启）</summary>
        public static string AutoMatchedId { get; private set; } = "";

        /// <summary>是否需要重启才能切换到正确版本</summary>
        public static bool NeedRestart { get; private set; } = false;

        /// <summary>
        /// 确保配置指定的 DLL 已预加载。必须在任何 ecat_motion P/Invoke 调用之前执行。
        /// ActiveVersion 为空或 "auto" 时，加载 Versions 中的第一个条目作为探测用 DLL。
        /// </summary>
        public static void EnsureDllLoaded()
        {
            lock (_lock)
            {
                if (_isLoaded) return;

                try
                {
                    _config = LoadConfig();
                    if (_config == null) return;

                    var versions = _config.Versions;
                    if (versions == null || versions.Length == 0)
                    {
                        _configError = "lc_version_config.json 中无版本条目";
                        return;
                    }

                    string activeId = _config.ActiveVersion;

                    // 自动模式：ActiveVersion 为空或 "auto" 时，用第一个条目作为探测 DLL
                    if (string.IsNullOrWhiteSpace(activeId) || activeId.Equals("auto", StringComparison.OrdinalIgnoreCase))
                    {
                        _activeEntry = versions[0];
                    }
                    else
                    {
                        _activeEntry = versions.FirstOrDefault(v => v.Id == activeId);
                        if (_activeEntry == null)
                        {
                            _configError = $"配置错误：找不到 ActiveVersion='{activeId}' 对应的版本条目";
                            return;
                        }
                    }

                    string fullPath = ResolveDllPath(_activeEntry.DllSubPath);
                    if (!File.Exists(fullPath))
                    {
                        _configError = $"DLL 文件不存在：{fullPath}";
                        return;
                    }

                    var handle = LoadLibrary(fullPath);
                    if (handle == IntPtr.Zero)
                    {
                        int err = Marshal.GetLastWin32Error();
                        _configError = $"LoadLibrary 失败：{fullPath}，Win32 错误码={err}";
                        return;
                    }

                    _isLoaded = true;
                }
                catch (Exception ex)
                {
                    _configError = $"加载 LC DLL 配置异常：{ex.Message}";
                }
            }
        }

        /// <summary>
        /// 读取板卡版本信息并自动匹配。需在 M_Open 成功之后调用。
        /// <para>流程：读取版本 → 从 Versions[] 查找 FPGA 匹配的条目 → 判断是否需要切换 DLL</para>
        /// </summary>
        public static void ReadVersionInfo(short cardNo)
        {
            try
            {
                byte[] ver = new byte[100];
                short ret = ecat_motion.M_GetVersion(out ver[0], 100, cardNo);

                if (ret == 0)
                {
                    string str = Encoding.Default.GetString(ver).TrimEnd('\0');
                    string[] results = str.Split(';');

                    if (results.Length >= 7)
                    {
                        DllVersion = (results[1] + " " + results[0]).Trim();
                        FpgaVersion = (results[3] + " " + results[2]).Trim();
                        DspVersion = (results[5] + " " + results[4]).Trim();
                        Authorization = results[6].Length >= 5 ? results[6].Substring(0, 5).Trim() : results[6].Trim();

                        // 自动匹配：根据 FPGA 版本从配置中查找对应条目
                        AutoMatchAndVerify();
                    }
                    else
                    {
                        VersionInfo = $"版本信息解析失败，原始数据：{str}";
                        IsVersionMatched = true;
                    }
                }
                else
                {
                    VersionInfo = $"M_GetVersion 调用失败，错误码：{ret}";
                    IsVersionMatched = true;
                }
            }
            catch (Exception ex)
            {
                VersionInfo = $"读取版本信息异常：{ex.Message}";
                IsVersionMatched = true;
            }
        }

        /// <summary>
        /// 根据 FPGA 版本自动匹配 Versions[] 中的条目，并更新 ActiveVersion。
        /// </summary>
        private static void AutoMatchAndVerify()
        {
            if (_config?.Versions == null || _config.Versions.Length == 0)
            {
                VersionInfo = $"DLL={DllVersion}, FPGA={FpgaVersion}, DSP={DspVersion}, 授权={Authorization}";
                return;
            }

            // 查找 FPGA 版本匹配的条目
            LCVersionEntry matched = null;
            foreach (var entry in _config.Versions)
            {
                if (!string.IsNullOrWhiteSpace(entry.ExpectedFpgaVersion) && FpgaVersion == entry.ExpectedFpgaVersion.Trim())
                {
                    matched = entry;
                    break;
                }
            }

            // 构建 VersionInfo
            VersionInfo = $"DLL={DllVersion}, FPGA={FpgaVersion}, DSP={DspVersion}, 授权={Authorization}, 已加载={_activeEntry?.Description ?? "未知"}";

            if (matched == null)
            {
                // 未找到匹配的条目：可能是新固件，未收录
                VersionInfo += " | 未在配置中找到匹配的固件版本，请使用 LCVersionCollector 采集";
                IsVersionMatched = false;
                return;
            }

            AutoMatchedId = matched.Id;

            if (matched.Id == _activeEntry?.Id)
            {
                // 匹配一致，当前加载的 DLL 正确
                IsVersionMatched = true;
                VersionInfo += $", 自动匹配={matched.Description}";
            }
            else
            {
                // 匹配到了不同的条目：当前 DLL 不对，需要切换
                IsVersionMatched = false;
                NeedRestart = true;

                // 自动更新配置中的 ActiveVersion，下次启动生效
                UpdateActiveVersion(matched.Id);

                VersionInfo += $" | 固件匹配={matched.Description}，当前加载不匹配，已自动更新配置 ActiveVersion={matched.Id}，请重启软件";
            }
        }

        /// <summary>
        /// 更新配置文件中的 ActiveVersion 字段
        /// </summary>
        private static void UpdateActiveVersion(string newActiveId)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string configPath = Path.Combine(baseDir, "LC", "DLLs", "lc_version_config.json");
                if (!File.Exists(configPath)) return;

                string json = File.ReadAllText(configPath, Encoding.UTF8);
                // 简单字符串替换，避免重新序列化丢失注释等
                string oldActive = $"\"ActiveVersion\": \"{_config.ActiveVersion}\"";
                string newActive = $"\"ActiveVersion\": \"{newActiveId}\"";
                json = json.Replace(oldActive, newActive);

                _config.ActiveVersion = newActiveId;

                File.WriteAllText(configPath, json, Encoding.UTF8);
            }
            catch
            {
                // 更新失败不影响主流程
            }
        }

        private static string ResolveDllPath(string subPath)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDir, subPath.Replace('/', Path.DirectorySeparatorChar));
        }

        private static LCVersionConfig LoadConfig()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string configPath = Path.Combine(baseDir, "LC", "DLLs", "lc_version_config.json");

                if (!File.Exists(configPath))
                {
                    _configError = $"配置文件不存在：{configPath}，将使用默认 DLL";
                    return null;
                }

                string json = File.ReadAllText(configPath, Encoding.UTF8);
                return JsonTool.ToObject<LCVersionConfig>(json);
            }
            catch (Exception ex)
            {
                _configError = $"读取配置文件异常：{ex.Message}";
                return null;
            }
        }
    }
}
