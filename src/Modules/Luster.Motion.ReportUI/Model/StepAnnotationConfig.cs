using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Luster.Motion.ReportUI.Model
{
    /// <summary>
    /// 步骤标注配置容器，支持按 CSV 文件名映射的字典格式
    /// 配置文件格式：
    /// {
    ///   "SN001": { "Steps": [...], "ChartSettings": {...} },
    ///   "SN002": { "Steps": [...], "ChartSettings": {...} },
    ///   "_default": { "Steps": [...], "ChartSettings": {...} }
    /// }
    /// </summary>
    public class StepAnnotationConfig
    {
        /// <summary>
        /// 步骤配置列表
        /// </summary>
        public List<StepAnnotationConfigModel> Steps { get; set; } = new List<StepAnnotationConfigModel>();

        /// <summary>
        /// 图表参数设置（轴刻度、平滑、合并模式等），自动随配置文件持久化
        /// </summary>
        public ChartSettings ChartSettings { get; set; } = new ChartSettings();

        private const string ConfigFileName = "StepAnnotationConfig.json";
        /// <summary>默认键（无 CSV 文件名时使用），供外部读写时引用</summary>
        public const string DefaultKey = "_default";

        /// <summary>
        /// 配置文件路径：配方父目录/Config/StepAnnotationConfig.json
        /// </summary>
        public static string GetConfigFilePath(string recipePath)
        {
            // 配方父目录下的 Config 目录
            var parentDir = Path.GetDirectoryName(recipePath?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            var configDir = Path.Combine(parentDir ?? recipePath ?? "", "Config");
            return Path.Combine(configDir, ConfigFileName);
        }

        /// <summary>
        /// 加载整个字典配置文件
        /// </summary>
        /// <returns>字典：CSV文件名(不含扩展名) → StepAnnotationConfig</returns>
        public static Dictionary<string, StepAnnotationConfig> LoadAll(string recipePath)
        {
            var filePath = GetConfigFilePath(recipePath);
            var result = new Dictionary<string, StepAnnotationConfig>();

            if (!File.Exists(filePath))
            {
                return result;
            }

            try
            {
                var json = File.ReadAllText(filePath, Encoding.UTF8);
                var jsonObj = JObject.Parse(json);

                foreach (var prop in jsonObj.Properties())
                {
                    var config = prop.Value.ToObject<StepAnnotationConfig>();
                    if (config != null)
                    {
                        result[prop.Name] = config;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"加载步骤标注配置失败: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// 保存整个字典到配置文件
        /// </summary>
        public static void SaveAll(string recipePath, Dictionary<string, StepAnnotationConfig> allConfigs)
        {
            var filePath = GetConfigFilePath(recipePath);
            var dir = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var jsonObj = new JObject();
            foreach (var kvp in allConfigs)
            {
                jsonObj[kvp.Key] = JObject.FromObject(kvp.Value);
            }

            var json = jsonObj.ToString(Formatting.Indented);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }

        /// <summary>
        /// 按 CSV 文件名查找对应的步骤配置
        /// </summary>
        /// <param name="csvFileName">CSV 文件名（不含扩展名）</param>
        /// <param name="recipePath">配方路径</param>
        /// <returns>匹配的步骤配置，无匹配时返回默认配置或空配置</returns>
        public static StepAnnotationConfig LoadByCsvName(string csvFileName, string recipePath)
        {
            var allConfigs = LoadAll(recipePath);

            // 精确匹配文件名
            if (!string.IsNullOrEmpty(csvFileName) && allConfigs.ContainsKey(csvFileName))
            {
                return allConfigs[csvFileName];
            }

            // 兜底：使用默认配置
            if (allConfigs.ContainsKey(DefaultKey))
            {
                return allConfigs[DefaultKey];
            }

            return new StepAnnotationConfig();
        }

        /// <summary>
        /// 保存指定 CSV 文件名对应的步骤配置
        /// </summary>
        public static void SaveByCsvName(string csvFileName, string recipePath, StepAnnotationConfig config)
        {
            var allConfigs = LoadAll(recipePath);
            var key = string.IsNullOrEmpty(csvFileName) ? DefaultKey : csvFileName;
            allConfigs[key] = config;
            SaveAll(recipePath, allConfigs);
        }
    }

    /// <summary>
    /// 压力曲线堆叠图（TaikeAnnotatedContent）的图表参数持久化模型。
    /// 与 StepAnnotationConfig 共用同一个 StepAnnotationConfig.json 的 _default 条目。
    /// 旧版文件无此字段时，反序列化后使用代码默认值。
    /// </summary>
    public class ChartSettings
    {
        /// <summary>图表1 X轴刻度间隔（ms）</summary>
        public double XAxisStep { get; set; } = 50;

        /// <summary>图表1 Y轴刻度间隔（kgf）</summary>
        public double YAxisStep { get; set; } = 0.05;

        /// <summary>图表1 Y轴上限（kgf），0 表示自动</summary>
        public double YAxisMax { get; set; } = 0.3;

        /// <summary>图表2 X轴刻度间隔（ms）</summary>
        public double XAxisStep2 { get; set; } = 50;

        /// <summary>图表2 Y轴刻度间隔</summary>
        public double YAxisStep2 { get; set; } = 0.5;

        /// <summary>图表2 Y轴上限，0 表示自动</summary>
        public double YAxisMax2 { get; set; } = 5.0;

        /// <summary>是否只显示 Y 正半轴</summary>
        public bool OnlyShowPositiveIsEnabled { get; set; } = false;

        /// <summary>是否启用平滑曲线处理</summary>
        public bool SmoothCurveProcessingsEnabled { get; set; } = false;

        /// <summary>平滑窗口大小（奇数）</summary>
        public int SliderValue { get; set; } = 11;

        /// <summary>是否合并显示（单图双 Y 轴）；false 为左右双子图</summary>
        public bool IsMergedView { get; set; } = true;
    }
}
