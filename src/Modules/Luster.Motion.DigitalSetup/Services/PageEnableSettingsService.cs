using Luster.Motion.CommonUI;
using Luster.Motion.DigitalSetup.Datas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Luster.Motion.DigitalSetup.Services
{
    /// <summary>
    /// 页面启用设置持久化服务
    /// </summary>
    public class PageEnableSettingsService
    {
        private string _configDirectory = Path.Combine(
            "D:\\",
            "LusterMotion",
            "DigitalSetup"
        );

        private string _configFilePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public PageEnableSettingsService(ICommonBus commonBus)
        {
            var recipeDir = commonBus?.CurrentRecipe.GetRecipePath() ?? "D:\\LusterMotion\\DigitalSetup";
            _configDirectory = recipeDir;
            _configFilePath = Path.Combine(_configDirectory, "DigitalSetUpDataValidation", "PageEnableSettings.json");
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// 设置配置文件路径
        /// </summary>
        /// <param name="folder">文件夹路径</param>
        public void SetConfigFilePath(string folder)
        {
            _configDirectory = Path.Combine(folder, "DigitalSetup");
            _configFilePath = Path.Combine(_configDirectory, "DigitalSetUpDataValidation", "PageEnableSettings.json");
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
        /// 保存页面启用设置
        /// </summary>
        /// <param name="settings">设置数据</param>
        /// <returns>是否保存成功</returns>
        public bool Save(PageEnableSettings settings)
        {
            try
            {
                EnsureDirectoryExists();
                settings.LastSavedTime = DateTime.Now;
                var json = JsonSerializer.Serialize(settings, _jsonOptions);
                File.WriteAllText(_configFilePath, json);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存页面启用设置失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 加载页面启用设置
        /// </summary>
        /// <returns>设置数据，如果文件不存在则返回新实例</returns>
        public PageEnableSettings Load()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    var json = File.ReadAllText(_configFilePath);
                    var settings = JsonSerializer.Deserialize<PageEnableSettings>(json, _jsonOptions);
                    return settings ?? new PageEnableSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载页面启用设置失败: {ex.Message}");
            }
            return new PageEnableSettings();
        }

        /// <summary>
        /// 加载本地配置，如果本地没有对应的键则使用默认配置
        /// 以本地配置为基础，确保所有页面和子页面都显示（缺失的从默认配置补充）
        /// </summary>
        /// <returns>合并后的设置数据</returns>
        public PageEnableSettings LoadOrMergeWithDefaults()
        {
            // 首先尝试加载本地配置
            var localSettings = Load();
            var defaultSettings = CreateDefaultSettings();

            // 如果本地配置为空或没有设置项，直接返回默认配置
            if (localSettings.PageSettings == null || localSettings.PageSettings.Count == 0)
            {
                return defaultSettings;
            }

            // 创建合并后的设置，以本地配置为基础，补充缺失的页面
            var mergedSettings = new PageEnableSettings();

            // 1. 先处理本地配置中已有的页面
            foreach (var localPage in localSettings.PageSettings)
            {
                var mergedPage = new PageEnableItem
                {
                    Name = localPage.Name,
                    Region = localPage.Region,
                    IsEnabled = localPage.IsEnabled
                };

                // 获取默认配置中该页面的所有子页面
                var defaultPage = FindPageSettingByName(defaultSettings, localPage.Name);
                
                // 合并子页面：以本地配置为基础，补充缺失的子页面
                if (defaultPage != null)
                {
                    // 先添加本地已有的子页面
                    foreach (var localSubPage in localPage.SubPages)
                    {
                        mergedPage.SubPages.Add(new SubPageEnableItem
                        {
                            Name = localSubPage.Name,
                            Region = localSubPage.Region,
                            IsEnabled = localSubPage.IsEnabled
                        });
                    }
                    
                    // 补充本地没有但默认配置中有的子页面
                    foreach (var defaultSubPage in defaultPage.SubPages)
                    {
                        if (FindSubPageSettingByName(mergedPage, defaultSubPage.Name) == null)
                        {
                            mergedPage.SubPages.Add(new SubPageEnableItem
                            {
                                Name = defaultSubPage.Name,
                                Region = defaultSubPage.Region,
                                IsEnabled = defaultSubPage.IsEnabled
                            });
                        }
                    }
                }
                else
                {
                    // 默认配置中没有该页面，直接使用本地配置的子页面
                    foreach (var localSubPage in localPage.SubPages)
                    {
                        mergedPage.SubPages.Add(new SubPageEnableItem
                        {
                            Name = localSubPage.Name,
                            Region = localSubPage.Region,
                            IsEnabled = localSubPage.IsEnabled
                        });
                    }
                }

                mergedSettings.PageSettings.Add(mergedPage);
            }

            // 2. 补充本地配置中没有但默认配置中有的页面
            foreach (var defaultPage in defaultSettings.PageSettings)
            {
                if (FindPageSettingByName(mergedSettings, defaultPage.Name) == null)
                {
                    var mergedPage = new PageEnableItem
                    {
                        Name = defaultPage.Name,
                        Region = defaultPage.Region,
                        IsEnabled = defaultPage.IsEnabled
                    };

                    foreach (var defaultSubPage in defaultPage.SubPages)
                    {
                        mergedPage.SubPages.Add(new SubPageEnableItem
                        {
                            Name = defaultSubPage.Name,
                            Region = defaultSubPage.Region,
                            IsEnabled = defaultSubPage.IsEnabled
                        });
                    }

                    mergedSettings.PageSettings.Add(mergedPage);
                }
            }

            return mergedSettings;
        }

        /// <summary>
        /// 在设置中根据名称查找页面设置
        /// </summary>
        private PageEnableItem FindPageSettingByName(PageEnableSettings settings, string name)
        {
            foreach (var page in settings.PageSettings)
            {
                if (page.Name == name)
                {
                    return page;
                }
            }
            return null;
        }

        /// <summary>
        /// 在页面设置中根据名称查找子页面设置
        /// </summary>
        private SubPageEnableItem FindSubPageSettingByName(PageEnableItem pageSetting, string name)
        {
            foreach (var subPage in pageSetting.SubPages)
            {
                if (subPage.Name == name)
                {
                    return subPage;
                }
            }
            return null;
        }

        /// <summary>
        /// 从DigitalAssPageModel创建默认设置
        /// </summary>
        /// <returns>默认设置数据</returns>
        public PageEnableSettings CreateDefaultSettings()
        {
            var settings = new PageEnableSettings();

            foreach (var page in DigitalAssPageModel.Pages)
            {
                var pageItem = new PageEnableItem
                {
                    Name = page.Name,
                    Region = page.Region,
                    IsEnabled = page.IsEnabled
                };

                // 添加子页面设置
                var subPages = DigitalAssPageModel.GetSubPages(page.Region);
                foreach (var subPage in subPages)
                {
                    pageItem.SubPages.Add(new SubPageEnableItem
                    {
                        Name = subPage.Name,
                        Region = subPage.Region,
                        IsEnabled = subPage.IsEnabled
                    });
                }

                settings.PageSettings.Add(pageItem);
            }

            return settings;
        }

        /// <summary>
        /// 应用设置到DigitalAssPageModel
        /// </summary>
        /// <param name="settings">设置数据</param>
        public void ApplySettings(PageEnableSettings settings)
        {
            foreach (var pageSetting in settings.PageSettings)
            {
                var page = FindPageByName(pageSetting.Name);
                if (page != null)
                {
                    page.IsEnabled = pageSetting.IsEnabled;

                    // 应用子页面设置
                    foreach (var subPageSetting in pageSetting.SubPages)
                    {
                        var subPage = DigitalAssPageModel.FindSubPage(page.Region, subPageSetting.Name);
                        if (subPage != null)
                        {
                            subPage.IsEnabled = subPageSetting.IsEnabled;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 应用所有页面的子页面设置（用于子页面加载时刷新IsEnabled状态）
        /// 加载全部配置并应用到所有子页面，无需指定parentRegion
        /// </summary>
        public void ApplySubPageSettings()
        {
            var settings = LoadOrMergeWithDefaults();
            
            // 遍历所有页面设置
            foreach (var pageSetting in settings.PageSettings)
            {
                var page = FindPageByName(pageSetting.Name);
                if (page != null)
                {
                    // 应用子页面设置
                    foreach (var subPageSetting in pageSetting.SubPages)
                    {
                        var subPage = DigitalAssPageModel.FindSubPage(page.Region, subPageSetting.Name);
                        if (subPage != null)
                        {
                            subPage.IsEnabled = subPageSetting.IsEnabled;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 根据名称查找页面
        /// </summary>
        private DigitalAssPageModel FindPageByName(string name)
        {
            foreach (var page in DigitalAssPageModel.Pages)
            {
                if (page.Name == name)
                {
                    return page;
                }
            }
            return null;
        }
    }
}
