using Luster.Motion.DigitalSetup.Datas;
using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Luster.Motion.DigitalSetup.Services
{
    /// <summary>
    /// 数据验证持久化服务
    /// </summary>
    public class DataValidationPersistenceService
    {
        private string ConfigDirectory = Path.Combine(
            //Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "D:\\",
            "LusterMotion",
            "DigitalSetup");

        private string ConfigFilePath = "DataValidationConfig.json";
        private string ConfigMessageFilePath = "InfoTipConfig.json";

        private readonly JsonSerializerOptions _jsonOptions;

        public DataValidationPersistenceService()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 支持中文等非ASCII字符
            };

        }

        public void SetConfigFilePath(string folder)
        {
            ConfigDirectory = Path.Combine(folder, "DigitalSetUpDataValidation");
            ConfigFilePath = Path.Combine(ConfigDirectory, "DataValidationConfig.json");
            ConfigMessageFilePath = Path.Combine(ConfigDirectory, "InfoTipConfig.json");
            EnsureDirectoryExists();
        }

        /// <summary>
        /// 确保配置目录存在
        /// </summary>
        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(ConfigDirectory))
            {
                Directory.CreateDirectory(ConfigDirectory);
            }
        }

        /// <summary>
        /// 保存数据验证配置
        /// </summary>
        /// <param name="data">持久化数据</param>
        /// <returns>是否保存成功</returns>
        public bool Save(DataValidationPersistenceData data)
        {
            try
            {
                data.LastSavedTime = DateTime.Now;
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                File.WriteAllText(ConfigFilePath, json);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存数据验证配置失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 加载数据验证配置
        /// </summary>
        /// <returns>持久化数据，如果文件不存在则返回新实例</returns>
        public DataValidationPersistenceData Load()
        {
            try
            {
                if (!File.Exists(ConfigFilePath))
                {
                    return new DataValidationPersistenceData();
                }

                var json = File.ReadAllText(ConfigFilePath);
                var data = JsonSerializer.Deserialize<DataValidationPersistenceData>(json, _jsonOptions);
                return data ?? new DataValidationPersistenceData();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载数据验证配置失败: {ex.Message}");
                return new DataValidationPersistenceData();
            }
        }

        /// <summary>
        /// 加载消息提示配置
        /// </summary>
        /// <returns>持久化数据，如果文件不存在则返回新实例</returns>
        public PersistenceData LoadMessageConfig()
        {
            try
            {
                if (!File.Exists(ConfigMessageFilePath))
                {
                    return new PersistenceData();
                }

                var json = File.ReadAllText(ConfigMessageFilePath);
                var data = JsonSerializer.Deserialize<PersistenceData>(json, _jsonOptions);
                return data ?? new PersistenceData();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载消息提示配置失败: {ex.Message}");
                return new PersistenceData();
            }
        }

        /// <summary>
        /// 保存消息提示配置
        /// </summary>
        /// <param name="data">持久化数据</param>
        /// <returns>是否保存成功</returns>
        public bool SaveMessageConfig(PersistenceData data)
        {
            try
            {
                EnsureDirectoryExists();
                data.LastSavedTime = DateTime.Now;
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                File.WriteAllText(ConfigMessageFilePath, json);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存消息提示配置失败: {ex.Message}");
                return false;
            }
        }
    }
}
