using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LCVersionSimTest
{
    #region 数据模型（与 LCDllManager 保持一致）

    public class LCVersionConfig
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

    public class SimResult
    {
        public string Scenario { get; set; }
        public string ActiveVersion { get; set; }
        public string LoadedEntryId { get; set; }
        public string LoadedDescription { get; set; }
        public string SimulatedFpgaVersion { get; set; }
        public string AutoMatchedId { get; set; }
        public bool IsVersionMatched { get; set; }
        public bool NeedRestart { get; set; }
        public string VersionInfo { get; set; }
        public string ConfigError { get; set; }
        public bool Passed { get; set; }
        public string FailReason { get; set; }
    }

    #endregion

    /// <summary>
    /// 仿真 LCDllManager 的核心匹配逻辑，不依赖真实硬件。
    /// 用途：本地验证版本匹配逻辑是否正确。
    /// </summary>
    public class LCDllManagerSim
    {
        private LCVersionConfig _config;
        private LCVersionEntry _activeEntry;
        private string _configError = "";

        public string ActiveDescription => _activeEntry?.Description ?? "";
        public string ActiveVersionId => _activeEntry?.Id ?? "";
        public string ConfigError => _configError;

        // 模拟 M_GetVersion 读取到的版本号
        public string DllVersion { get; private set; } = "";
        public string FpgaVersion { get; private set; } = "";
        public string DspVersion { get; private set; } = "";
        public string Authorization { get; private set; } = "";

        public bool IsVersionMatched { get; private set; } = true;
        public string VersionInfo { get; private set; } = "";
        public string AutoMatchedId { get; private set; } = "";
        public bool NeedRestart { get; private set; } = false;

        /// <summary>
        /// 从 JSON 字符串加载配置并确定 ActiveEntry
        /// </summary>
        public void LoadConfig(string configJson)
        {
            _configError = "";
            _config = JsonConvert.DeserializeObject<LCVersionConfig>(configJson);

            var versions = _config.Versions;
            if (versions == null || versions.Length == 0)
            {
                _configError = "lc_version_config.json 中无版本条目";
                return;
            }

            string activeId = _config.ActiveVersion;
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
        }

        /// <summary>
        /// 仿真 M_GetVersion 返回的版本数据，然后执行自动匹配逻辑。
        /// dllVer/fpgaVer/dspVer 格式如 "1.0 V4.27"（与实际 M_GetVersion 解析后的格式一致）
        /// </summary>
        public void SimulateVersionInfo(string dllVer, string fpgaVer, string dspVer, string auth = "AUTH1")
        {
            DllVersion = dllVer;
            FpgaVersion = fpgaVer;
            DspVersion = dspVer;
            Authorization = auth;

            AutoMatchAndVerify();
        }

        private void AutoMatchAndVerify()
        {
            IsVersionMatched = true;
            NeedRestart = false;
            AutoMatchedId = "";
            VersionInfo = "";

            if (_config?.Versions == null || _config.Versions.Length == 0)
            {
                VersionInfo = $"DLL={DllVersion}, FPGA={FpgaVersion}, DSP={DspVersion}, 授权={Authorization}";
                return;
            }

            LCVersionEntry matched = null;
            foreach (var entry in _config.Versions)
            {
                if (!string.IsNullOrWhiteSpace(entry.ExpectedFpgaVersion) && FpgaVersion == entry.ExpectedFpgaVersion)
                {
                    matched = entry;
                    break;
                }
            }

            VersionInfo = $"DLL={DllVersion}, FPGA={FpgaVersion}, DSP={DspVersion}, 授权={Authorization}, 已加载={_activeEntry?.Description ?? "未知"}";

            if (matched == null)
            {
                VersionInfo += " | 未在配置中找到匹配的固件版本，请使用 LCVersionCollector 采集";
                IsVersionMatched = false;
                return;
            }

            AutoMatchedId = matched.Id;

            if (matched.Id == _activeEntry?.Id)
            {
                IsVersionMatched = true;
                VersionInfo += $", 自动匹配={matched.Description}";
            }
            else
            {
                IsVersionMatched = false;
                NeedRestart = true;
                VersionInfo += $" | 固件匹配={matched.Description}，当前加载不匹配，已自动更新配置 ActiveVersion={matched.Id}，请重启软件";
            }
        }

        public void Reset()
        {
            _config = null;
            _activeEntry = null;
            _configError = "";
            DllVersion = "";
            FpgaVersion = "";
            DspVersion = "";
            Authorization = "";
            IsVersionMatched = true;
            VersionInfo = "";
            AutoMatchedId = "";
            NeedRestart = false;
        }
    }

    class Program
    {
        static int _passed = 0;
        static int _failed = 0;

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("========================================");
            Console.WriteLine("  LC 版本匹配逻辑仿真测试");
            Console.WriteLine("  不依赖真实轴卡硬件");
            Console.WriteLine("========================================\n");

            // 运行所有测试场景
            Test1_AllExpectedVersionsEmpty();
            Test2_CorrectFpgaVersionMatch();
            Test3_WrongFpgaVersion();
            Test4_AutoModeWithMatching();
            Test5_AutoModeNeedRestart();
            Test6_MultipleVersionsOneMatch();
            Test7_ActiveVersionNotFound();
            Test8_EmptyVersions();

            Console.WriteLine("\n========================================");
            Console.WriteLine($"  测试结果: {_passed} 通过, {_failed} 失败");
            Console.WriteLine("========================================");

            if (_failed > 0)
            {
                Console.WriteLine("\n按任意键退出...");
                Console.ReadKey();
            }
        }

        static void RunTest(string scenario, string configJson, string activeVersion,
            string simFpgaVersion, string simDllVersion, string simDspVersion,
            Func<SimResult, bool> assert, string expectDesc)
        {
            var sim = new LCDllManagerSim();
            var result = new SimResult { Scenario = scenario };

            Console.WriteLine($"--- {scenario} ---");
            Console.WriteLine($"  预期: {expectDesc}");

            try
            {
                sim.LoadConfig(configJson);
                result.ConfigError = sim.ConfigError;
                result.ActiveVersion = activeVersion;
                result.LoadedEntryId = sim.ActiveVersionId;
                result.LoadedDescription = sim.ActiveDescription;

                if (string.IsNullOrEmpty(sim.ConfigError))
                {
                    sim.SimulateVersionInfo(simDllVersion, simFpgaVersion, simDspVersion);
                    result.SimulatedFpgaVersion = simFpgaVersion;
                    result.IsVersionMatched = sim.IsVersionMatched;
                    result.NeedRestart = sim.NeedRestart;
                    result.AutoMatchedId = sim.AutoMatchedId;
                    result.VersionInfo = sim.VersionInfo;
                }

                result.Passed = assert(result);
                if (result.Passed)
                {
                    _passed++;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  结果: PASS");
                }
                else
                {
                    _failed++;
                    result.FailReason = "断言失败";
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  结果: FAIL");
                }
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                _failed++;
                result.Passed = false;
                result.FailReason = ex.Message;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  结果: FAIL (异常: {ex.Message})");
                Console.ResetColor();
            }

            // 输出详细信息
            Console.WriteLine($"  加载的版本: {result.LoadedEntryId} ({result.LoadedDescription})");
            Console.WriteLine($"  仿真FPGA版本: {result.SimulatedFpgaVersion}");
            Console.WriteLine($"  匹配结果: IsMatched={result.IsVersionMatched}, AutoMatchedId={result.AutoMatchedId}, NeedRestart={result.NeedRestart}");
            if (!string.IsNullOrEmpty(result.VersionInfo))
                Console.WriteLine($"  版本信息: {result.VersionInfo}");
            if (!string.IsNullOrEmpty(result.ConfigError))
                Console.WriteLine($"  配置错误: {result.ConfigError}");
            Console.WriteLine();
        }

        #region 测试场景

        /// <summary>
        /// 场景1：所有 ExpectedFpgaVersion 为空 → 应该匹配失败
        /// 对应用户反馈的问题1
        /// </summary>
        static void Test1_AllExpectedVersionsEmpty()
        {
            string config = @"{
                ""ActiveVersion"": ""auto"",
                ""Versions"": [
                    {
                        ""Id"": ""v427"",
                        ""DllSubPath"": ""LC/DLLs/v427/ecat_motion.dll"",
                        ""Description"": ""旧版DLL"",
                        ""ExpectedDllVersion"": """",
                        ""ExpectedFpgaVersion"": """",
                        ""ExpectedDspVersion"": """"
                    }
                ]
            }";

            RunTest("场景1: auto模式, 所有ExpectedFpgaVersion为空",
                config, "auto",
                simFpgaVersion: "1.0 V4.27",
                simDllVersion: "1.0 V4.27",
                simDspVersion: "1.0 V4.27",
                assert: r => r.LoadedEntryId == "v427"
                          && !r.IsVersionMatched
                          && r.VersionInfo.Contains("未在配置中找到匹配的固件版本"),
                expectDesc: "加载v427, 但ExpectedFpgaVersion为空导致匹配失败");
        }

        /// <summary>
        /// 场景2：ExpectedFpgaVersion 正确填写 → 应该匹配成功
        /// </summary>
        static void Test2_CorrectFpgaVersionMatch()
        {
            string config = @"{
                ""ActiveVersion"": ""v427"",
                ""Versions"": [
                    {
                        ""Id"": ""v427"",
                        ""DllSubPath"": ""LC/DLLs/v427/ecat_motion.dll"",
                        ""Description"": ""旧版DLL"",
                        ""ExpectedDllVersion"": ""1.0 V4.27"",
                        ""ExpectedFpgaVersion"": ""1.0 V4.27"",
                        ""ExpectedDspVersion"": ""1.0 V4.27""
                    }
                ]
            }";

            RunTest("场景2: ActiveVersion=v427, ExpectedFpgaVersion正确",
                config, "v427",
                simFpgaVersion: "1.0 V4.27",
                simDllVersion: "1.0 V4.27",
                simDspVersion: "1.0 V4.27",
                assert: r => r.LoadedEntryId == "v427"
                          && r.IsVersionMatched
                          && r.AutoMatchedId == "v427",
                expectDesc: "加载v427, FPGA匹配成功, IsMatched=true");
        }

        /// <summary>
        /// 场景3：ExpectedFpgaVersion 故意写错 → 匹配失败
        /// 对应用户反馈的问题2
        /// </summary>
        static void Test3_WrongFpgaVersion()
        {
            string config = @"{
                ""ActiveVersion"": ""v427"",
                ""Versions"": [
                    {
                        ""Id"": ""v427"",
                        ""DllSubPath"": ""LC/DLLs/v427/ecat_motion.dll"",
                        ""Description"": ""旧版DLL"",
                        ""ExpectedDllVersion"": ""1.0 V4.27"",
                        ""ExpectedFpgaVersion"": ""9.9 WRONG"",
                        ""ExpectedDspVersion"": ""1.0 V4.27""
                    }
                ]
            }";

            RunTest("场景3: ActiveVersion=v427, ExpectedFpgaVersion故意写错",
                config, "v427",
                simFpgaVersion: "1.0 V4.27",
                simDllVersion: "1.0 V4.27",
                simDspVersion: "1.0 V4.27",
                assert: r => r.LoadedEntryId == "v427"
                          && !r.IsVersionMatched
                          && r.VersionInfo.Contains("未在配置中找到匹配的固件版本"),
                expectDesc: "加载v427, 但FPGA版本与Expected不匹配, 匹配失败");
        }

        /// <summary>
        /// 场景4：auto模式，第一个条目的FPGA版本匹配成功
        /// </summary>
        static void Test4_AutoModeWithMatching()
        {
            string config = @"{
                ""ActiveVersion"": ""auto"",
                ""Versions"": [
                    {
                        ""Id"": ""v427"",
                        ""DllSubPath"": ""LC/DLLs/v427/ecat_motion.dll"",
                        ""Description"": ""旧版DLL"",
                        ""ExpectedDllVersion"": ""1.0 V4.27"",
                        ""ExpectedFpgaVersion"": ""1.0 V4.27"",
                        ""ExpectedDspVersion"": ""1.0 V4.27""
                    },
                    {
                        ""Id"": ""v756"",
                        ""DllSubPath"": ""LC/DLLs/v756/ecat_motion.dll"",
                        ""Description"": ""新版DLL-大寰音圈电机"",
                        ""ExpectedDllVersion"": ""1.0 V7.56"",
                        ""ExpectedFpgaVersion"": ""1.0 V7.56"",
                        ""ExpectedDspVersion"": ""1.0 V7.56""
                    }
                ]
            }";

            RunTest("场景4: auto模式, 第一个条目v427的FPGA版本匹配",
                config, "auto",
                simFpgaVersion: "1.0 V4.27",
                simDllVersion: "1.0 V4.27",
                simDspVersion: "1.0 V4.27",
                assert: r => r.LoadedEntryId == "v427"
                          && r.IsVersionMatched
                          && r.AutoMatchedId == "v427"
                          && !r.NeedRestart,
                expectDesc: "加载第一个(v427), FPGA匹配成功, 不需要重启");
        }

        /// <summary>
        /// 场景5：auto模式加载v427，但实际板卡是v756 → 需要重启
        /// </summary>
        static void Test5_AutoModeNeedRestart()
        {
            string config = @"{
                ""ActiveVersion"": ""auto"",
                ""Versions"": [
                    {
                        ""Id"": ""v427"",
                        ""DllSubPath"": ""LC/DLLs/v427/ecat_motion.dll"",
                        ""Description"": ""旧版DLL"",
                        ""ExpectedDllVersion"": ""1.0 V4.27"",
                        ""ExpectedFpgaVersion"": ""1.0 V4.27"",
                        ""ExpectedDspVersion"": ""1.0 V4.27""
                    },
                    {
                        ""Id"": ""v756"",
                        ""DllSubPath"": ""LC/DLLs/v756/ecat_motion.dll"",
                        ""Description"": ""新版DLL-大寰音圈电机"",
                        ""ExpectedDllVersion"": ""1.0 V7.56"",
                        ""ExpectedFpgaVersion"": ""1.0 V7.56"",
                        ""ExpectedDspVersion"": ""1.0 V7.56""
                    }
                ]
            }";

            RunTest("场景5: auto模式, 加载v427但实际板卡是v756",
                config, "auto",
                simFpgaVersion: "1.0 V7.56",
                simDllVersion: "1.0 V4.27",
                simDspVersion: "1.0 V7.56",
                assert: r => r.LoadedEntryId == "v427"
                          && !r.IsVersionMatched
                          && r.AutoMatchedId == "v756"
                          && r.NeedRestart
                          && r.VersionInfo.Contains("请重启软件"),
                expectDesc: "加载v427, 但FPGA匹配v756, NeedRestart=true, 提示重启");
        }

        /// <summary>
        /// 场景6：多个版本，只有v440匹配
        /// </summary>
        static void Test6_MultipleVersionsOneMatch()
        {
            string config = @"{
                ""ActiveVersion"": ""v427"",
                ""Versions"": [
                    {
                        ""Id"": ""v427"",
                        ""DllSubPath"": ""LC/DLLs/v427/ecat_motion.dll"",
                        ""Description"": ""旧版DLL"",
                        ""ExpectedDllVersion"": """",
                        ""ExpectedFpgaVersion"": ""1.0 V4.27"",
                        ""ExpectedDspVersion"": """"
                    },
                    {
                        ""Id"": ""v440"",
                        ""DllSubPath"": ""LC/DLLs/v440/ecat_motion.dll"",
                        ""Description"": ""备选版本"",
                        ""ExpectedDllVersion"": """",
                        ""ExpectedFpgaVersion"": ""1.0 V4.40"",
                        ""ExpectedDspVersion"": """"
                    },
                    {
                        ""Id"": ""v756"",
                        ""DllSubPath"": ""LC/DLLs/v756/ecat_motion.dll"",
                        ""Description"": ""新版DLL"",
                        ""ExpectedDllVersion"": """",
                        ""ExpectedFpgaVersion"": ""1.0 V7.56"",
                        ""ExpectedDspVersion"": """"
                    }
                ]
            }";

            RunTest("场景6: ActiveVersion=v427, 实际板卡FPGA是v440",
                config, "v427",
                simFpgaVersion: "1.0 V4.40",
                simDllVersion: "1.0 V4.40",
                simDspVersion: "1.0 V4.40",
                assert: r => r.LoadedEntryId == "v427"
                          && !r.IsVersionMatched
                          && r.AutoMatchedId == "v440"
                          && r.NeedRestart,
                expectDesc: "加载v427, 但FPGA匹配v440, NeedRestart=true");
        }

        /// <summary>
        /// 场景7：ActiveVersion指定了不存在的版本
        /// </summary>
        static void Test7_ActiveVersionNotFound()
        {
            string config = @"{
                ""ActiveVersion"": ""v999"",
                ""Versions"": [
                    {
                        ""Id"": ""v427"",
                        ""DllSubPath"": ""LC/DLLs/v427/ecat_motion.dll"",
                        ""Description"": ""旧版DLL"",
                        ""ExpectedFpgaVersion"": ""1.0 V4.27"",
                        ""ExpectedDspVersion"": """"
                    }
                ]
            }";

            RunTest("场景7: ActiveVersion=v999不存在",
                config, "v999",
                simFpgaVersion: "1.0 V4.27",
                simDllVersion: "1.0 V4.27",
                simDspVersion: "1.0 V4.27",
                assert: r => !string.IsNullOrEmpty(r.ConfigError)
                          && r.ConfigError.Contains("找不到"),
                expectDesc: "配置错误, 找不到v999");
        }

        /// <summary>
        /// 场景8：Versions为空数组
        /// </summary>
        static void Test8_EmptyVersions()
        {
            string config = @"{
                ""ActiveVersion"": ""auto"",
                ""Versions"": []
            }";

            RunTest("场景8: Versions为空数组",
                config, "auto",
                simFpgaVersion: "1.0 V4.27",
                simDllVersion: "1.0 V4.27",
                simDspVersion: "1.0 V4.27",
                assert: r => !string.IsNullOrEmpty(r.ConfigError)
                          && r.ConfigError.Contains("无版本条目"),
                expectDesc: "配置错误, 无版本条目");
        }

        #endregion
    }
}
