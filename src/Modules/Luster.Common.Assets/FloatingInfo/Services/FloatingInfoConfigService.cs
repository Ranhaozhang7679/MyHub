#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       FloatingInfoConfigService
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Assets.FloatingInfo.Services
* 文 件 名:       FloatingInfoConfigService.cs
* 创建时间:       2026/03/24
* 作    者:       Luster
* 版    权:       <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 唯一标识：      a1b2c3d4-e5f6-7890-abcd-ef1234567897
* 创建年份:      2026
************************************************************************************/

#endregion

using Luster.Common.Assets.FloatingInfo.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Controls.Primitives;

namespace Luster.Common.Assets.FloatingInfo.Services
{
    /// <summary>
    /// 浮动信息配置服务实现
    /// </summary>
    public class FloatingInfoConfigService : IFloatingInfoConfigService
    {
        /// <summary>
        /// 配置文件路径
        /// </summary>
        private string _configFilePath;

        /// <summary>
        /// 配置缓存
        /// </summary>
        private readonly Dictionary<string, FloatingInfoConfig> _configs;

        /// <summary>
        /// 标记是否已加载配置
        /// </summary>
        private bool _isLoaded;

        /// <summary>
        /// 配置文件名
        /// </summary>
        private const string ConfigFileName = "FloatingInfoConfigs.json";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="basePath">配置文件基础路径</param>
        public FloatingInfoConfigService(string basePath = null)
        {
            _configs = new Dictionary<string, FloatingInfoConfig>();
            _isLoaded = false;
            _configFilePath = basePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DigitalConfig", ConfigFileName);
        }

        /// <summary>
        /// 确保配置已加载
        /// </summary>
        private void EnsureLoaded()
        {
            if (!_isLoaded)
            {
                Load();
            }
        }

        /// <summary>
        /// 获取所有配置
        /// </summary>
        public List<FloatingInfoConfig> GetAllConfigs()
        {
            EnsureLoaded();
            return _configs.Values.ToList();
        }

        /// <summary>
        /// 获取指定配置
        /// </summary>
        /// <param name="pageId">页面ID</param>
        /// <returns>配置，如果没有配置则返回默认配置</returns>
        public FloatingInfoConfig GetConfig(string pageId)
        {
            EnsureLoaded();
            if (_configs.TryGetValue(pageId, out var config))
            {
                return config;
            }
            // 返回默认配置
            return CreateDefaultConfig(pageId);
        }

        /// <summary>
        /// 创建默认配置
        /// </summary>
        /// <param name="pageId">页面ID</param>
        /// <returns>默认配置</returns>
        private FloatingInfoConfig CreateDefaultConfig(string pageId)
        {
            return new FloatingInfoConfig
            {
                PageId = pageId,
                PageName = pageId,
                IsEnabled = true,
                WindowWidth = 800,
                WindowHeight = 600,
                WindowLeft = double.NaN,
                WindowTop = double.NaN
            };
        }

        /// <summary>
        /// 保存或更新配置
        /// </summary>
        /// <param name="config">配置</param>
        public void SaveConfig(FloatingInfoConfig config)
        {
            if (config == null || string.IsNullOrEmpty(config.PageId))
            {
                return;
            }

            _configs[config.PageId] = config;
            Save();

        }

        /// <summary>
        /// 保存所有配置
        /// </summary>
        /// <param name="configs">配置列表</param>
        public void SaveAllConfigs(IEnumerable<FloatingInfoConfig> configs)
        {
            _configs.Clear();
            foreach (var config in configs)
            {
                if (config != null && !string.IsNullOrEmpty(config.PageId))
                {
                    _configs[config.PageId] = config;
                }
            }
            Save();

        }

        /// <summary>
        /// 删除配置
        /// </summary>
        /// <param name="pageId">页面ID</param>
        /// <returns>是否删除成功</returns>
        public bool DeleteConfig(string pageId)
        {
            if (_configs.Remove(pageId))
            {
                Save();
                return true;
            }
            return false;

        }

        /// <summary>
        /// 检查配置是否存在
        /// </summary>
        /// <param name="pageId">页面ID</param>
        /// <returns>是否存在</returns>
        public bool ExistsConfig(string pageId)
        {
            EnsureLoaded();
            return _configs.ContainsKey(pageId);

        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        public void Load()
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    CreateDefaultConfigFile();
                    _isLoaded = true;
                    return;
                }

                var json = File.ReadAllText(_configFilePath);
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.Indented,
                    Converters = new List<JsonConverter> { new ContentItemConverter() }
                };
                var configData = JsonConvert.DeserializeObject<FloatingInfoConfigData>(json, settings);

                _configs.Clear();
                if (configData?.Configs != null)
                {
                    // 获取基准路径
                    var basePath = GetBasePath();

                    foreach (var config in configData.Configs)
                    {
                        if (config != null && !string.IsNullOrEmpty(config.PageId))
                        {
                            // 将相对路径转换为绝对路径
                            ConvertRelativePathsToAbsolute(config, basePath);
                            _configs[config.PageId] = config;
                        }
                    }
                }
                _isLoaded = true;
            }
            catch (Exception)
            {
                // 加载失败时创建默认配置
                CreateDefaultConfigFile();
                _isLoaded = true;
            }
        }

        /// <summary>
        /// 保存配置到文件
        /// </summary>
        public void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(_configFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 获取基准路径
                var basePath = GetBasePath();

                // 在保存前将绝对路径转换为相对路径
                var configsToSave = new List<FloatingInfoConfig>();
                foreach (var config in _configs.Values)
                {
                    var clonedConfig = config.Clone();
                    ConvertAbsolutePathsToRelative(clonedConfig, basePath);
                    configsToSave.Add(clonedConfig);
                }

                var configData = new FloatingInfoConfigData
                {
                    Configs = configsToSave
                };

                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.Indented,
                    Converters = new List<JsonConverter> { new ContentItemConverter() }
                };

                var json = JsonConvert.SerializeObject(configData, settings);
                File.WriteAllText(_configFilePath, json);
            }
            catch
            {
                // 保存失败，忽略错误
            }
        }

        /// <summary>
        /// 创建默认配置文件
        /// </summary>
        private void CreateDefaultConfigFile()
        {
            try
            {
                var directory = Path.GetDirectoryName(_configFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var defaultConfig = new FloatingInfoConfigData
                {
                    Configs = new List<FloatingInfoConfig>()
                };

                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.Indented,
                    Converters = new List<JsonConverter> { new ContentItemConverter() }
                };

                var json = JsonConvert.SerializeObject(defaultConfig, settings);
                File.WriteAllText(_configFilePath, json);
            }
            catch
            {
                // 创建失败，忽略错误
            }
        }

        /// <summary>
        /// 设置配置文件路径
        /// </summary>
        /// <param name="configFilePath">配置文件的完整路径</param>
        public void SetConfigPath(string configFilePath)
        {
            if (!string.IsNullOrEmpty(configFilePath))
            {
                _configFilePath = Path.Combine(configFilePath, "DigitalSetUpDataValidation", "FloatingInfoConfigs.json");

                // 清空缓存，重置加载状态，重新加载
                _configs.Clear();
                _isLoaded = false;
                Load();
            }
        }

        /// <summary>
        /// 获取当前配置文件路径
        /// </summary>
        /// <returns>配置文件路径</returns>
        public string GetConfigPath()
        {
            return _configFilePath;
        }

        /// <summary>
        /// 获取基准路径（配置文件所在目录的父目录）
        /// </summary>
        /// <returns>基准路径</returns>
        public string GetBasePath()
        {
            try
            {
                // 配置文件位于 {RecipeDir}/DigitalSetUpDataValidation/FloatingInfoConfigs.json
                // 基准路径应为 {RecipeDir}
                var configDir = Path.GetDirectoryName(_configFilePath);
                if (string.IsNullOrEmpty(configDir))
                {
                    return AppDomain.CurrentDomain.BaseDirectory;
                }

                // 获取父目录（DigitalSetUpDataValidation 的父目录）
                var basePath =configDir;
                return basePath ?? AppDomain.CurrentDomain.BaseDirectory;
            }
            catch
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        /// <summary>
        /// 将配置中的绝对路径转换为相对路径
        /// </summary>
        /// <param name="config">配置对象</param>
        /// <param name="basePath">基准路径</param>
        private void ConvertAbsolutePathsToRelative(FloatingInfoConfig config, string basePath)
        {
            if (config?.ContentItems == null) return;

            foreach (var item in config.ContentItems)
            {
                if (item is ImageContentItem imageItem && !string.IsNullOrEmpty(imageItem.ImagePath))
                {
                    imageItem.ImagePath = PathConverter.ToRelativePath(imageItem.ImagePath, basePath);
                }
            }
        }

        /// <summary>
        /// 将配置中的相对路径转换为绝对路径
        /// </summary>
        /// <param name="config">配置对象</param>
        /// <param name="basePath">基准路径</param>
        private void ConvertRelativePathsToAbsolute(FloatingInfoConfig config, string basePath)
        {
            if (config?.ContentItems == null) return;

            foreach (var item in config.ContentItems)
            {
                if (item is ImageContentItem imageItem && !string.IsNullOrEmpty(imageItem.ImagePath))
                {
                    imageItem.ImagePath = PathConverter.ToAbsolutePath(imageItem.ImagePath, basePath);
                }
            }
        }
    }


    /// <summary>
    /// 配置数据类(用于JSON序列化)
    /// </summary>
    internal class FloatingInfoConfigData
    {
        /// <summary>
        /// 配置列表
        /// </summary>
        public List<FloatingInfoConfig> Configs { get; set; }
    }
}
