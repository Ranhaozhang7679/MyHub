using Luster.Motion.DigitalSetup.Datas;
using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Luster.Motion.DigitalSetup.Services
{
    /// <summary>
    /// 页面状态持久化服务 - 负责页面状态的保存和加载
    /// </summary>
    public class PageStatusPersistenceService
    {
        private string _configDirectory;
        private string _configFilePath;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// 构造函数
        /// </summary>
        public PageStatusPersistenceService()
        {
            _configDirectory = Path.Combine("D:\\", "LusterMotion", "DigitalSetup");
            _configFilePath = Path.Combine(_configDirectory, "PageStatusData.json");

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 支持中文等非ASCII字符
            };
        }

        /// <summary>
        /// 设置配置文件路径（基于配方路径）
        /// </summary>
        /// <param name="recipePath">配方路径</param>
        public void SetConfigPath(string recipePath)
        {
            if (string.IsNullOrEmpty(recipePath))
            {
                return;
            }

            _configDirectory = Path.Combine(recipePath, "DigitalSetUpDataValidation");
            _configFilePath = Path.Combine(_configDirectory, "PageStatusData.json");
            EnsureDirectoryExists();
        }

        /// <summary>
        /// 确保配置目录存在
        /// </summary>
        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(_configDirectory))
            {
                Directory.CreateDirectory(_configDirectory);
            }
        }

        /// <summary>
        /// 保存页面状态数据
        /// </summary>
        /// <param name="data">页面状态持久化数据</param>
        /// <returns>是否保存成功</returns>
        public bool Save(PageStatusPersistenceData data)
        {
            try
            {
                EnsureDirectoryExists();
                data.LastSavedTime = DateTime.Now;
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                File.WriteAllText(_configFilePath, json);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存页面状态失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 加载页面状态数据
        /// </summary>
        /// <returns>页面状态持久化数据，如果文件不存在或损坏则返回新实例</returns>
        public PageStatusPersistenceData Load()
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    return new PageStatusPersistenceData();
                }

                var json = File.ReadAllText(_configFilePath);
                var data = JsonSerializer.Deserialize<PageStatusPersistenceData>(json, _jsonOptions);
                return data ?? new PageStatusPersistenceData();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载页面状态失败: {ex.Message}");
                return new PageStatusPersistenceData();
            }
        }

        /// <summary>
        /// 获取当前配置文件路径
        /// </summary>
        /// <returns>配置文件完整路径</returns>
        public string GetConfigFilePath()
        {
            return _configFilePath;
        }
    }
}
