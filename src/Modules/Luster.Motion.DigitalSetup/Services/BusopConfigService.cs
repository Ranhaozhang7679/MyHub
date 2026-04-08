using Aspose.Cells;
using Luster.Motion.CommonUI;
using Luster.Motion.DigitalSetup.Datas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Luster.Motion.DigitalSetup.Services
{
    /// <summary>
    /// BUSOP 配置持久化服务
    /// 管理 xlsx 文件路径和各子界面的 Sheet 页映射
    /// </summary>
    public class BusopConfigService
    {
        private readonly ICommonBus _commonBus;
        private readonly JsonSerializerOptions _jsonOptions;
        private string _configFilePath;

        // 18个子界面的默认名称（占位，后续替换）
        private static readonly string[] DefaultSubItemNames = new string[]
        {
            "BUSOP01", "BUSOP02", "BUSOP03", "BUSOP04", "BUSOP05", "BUSOP06",
            "BUSOP07", "BUSOP08", "BUSOP09", "BUSOP10", "BUSOP11", "BUSOP12",
            "BUSOP13", "BUSOP14", "BUSOP15", "BUSOP16", "BUSOP17", "BUSOP18"
        };

        public BusopConfigService(ICommonBus commonBus)
        {
            _commonBus = commonBus;
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
            InitializeConfigPath();
        }

        /// <summary>
        /// 初始化配置文件路径
        /// </summary>
        private void InitializeConfigPath()
        {
            var recipeDir = _commonBus?.CurrentRecipe?.GetRecipePath() ?? "D:\\LusterMotion\\DigitalSetup";
            var configDir = Path.Combine(recipeDir, "db", "Ass_Data");
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }
            _configFilePath = Path.Combine(configDir, "BusopConfig.json");
        }

        /// <summary>
        /// 加载配置，文件不存在时创建默认配置
        /// </summary>
        public BusopConfig LoadConfig()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    var json = File.ReadAllText(_configFilePath);
                    var config = JsonSerializer.Deserialize<BusopConfig>(json, _jsonOptions);
                    if (config != null && config.SubItems != null && config.SubItems.Count > 0)
                    {
                        return config;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载 BUSOP 配置失败: {ex.Message}");
            }
            return CreateDefaultConfig();
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public bool SaveConfig(BusopConfig config)
        {
            try
            {
                var json = JsonSerializer.Serialize(config, _jsonOptions);
                File.WriteAllText(_configFilePath, json);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存 BUSOP 配置失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 创建默认配置（18个子界面）
        /// </summary>
        private BusopConfig CreateDefaultConfig()
        {
            var config = new BusopConfig
            {
                ExcelFilePath = "",
                SubItems = new List<BusopSubItemConfig>()
            };
            foreach (var name in DefaultSubItemNames)
            {
                config.SubItems.Add(new BusopSubItemConfig { Name = name, SheetName = "" });
            }
            return config;
        }

        /// <summary>
        /// 获取 xlsx 文件的完整路径
        /// 支持绝对路径和配方相对路径
        /// </summary>
        public string GetExcelFullPath(string relativeOrAbsolutePath)
        {
            if (string.IsNullOrWhiteSpace(relativeOrAbsolutePath))
                return "";

            // 如果已经是绝对路径，直接返回
            if (Path.IsPathRooted(relativeOrAbsolutePath))
                return relativeOrAbsolutePath;

            // 相对路径：拼接配方路径
            var recipeDir = _commonBus?.CurrentRecipe?.GetRecipePath() ?? "";
            return Path.Combine(recipeDir, relativeOrAbsolutePath);
        }

        /// <summary>
        /// 从 xlsx 文件中读取所有 Sheet 页名称
        /// </summary>
        public List<string> GetSheetNames(string excelFilePath)
        {
            var sheets = new List<string>();
            try
            {
                var fullPath = GetExcelFullPath(excelFilePath);
                if (!File.Exists(fullPath))
                    return sheets;

                var workbook = new Workbook(fullPath);
                foreach (Worksheet ws in workbook.Worksheets)
                {
                    sheets.Add(ws.Name);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"读取 Sheet 名称失败: {ex.Message}");
            }
            return sheets;
        }
    }
}