using System;
using System.Collections.Generic;

namespace Luster.Motion.DigitalSetup.Datas
{
    /// <summary>
    /// 数据验证持久化数据模型
    /// </summary>
    public class DataValidationPersistenceData
    {
        /// <summary>
        /// 验证项列表
        /// </summary>
        public List<ValidationItemData> ValidationItems { get; set; } = new List<ValidationItemData>();

        /// <summary>
        /// 最后保存时间
        /// </summary>
        public DateTime LastSavedTime { get; set; }
    }

    /// <summary>
    /// 验证项数据模型
    /// </summary>
    public class ValidationItemData
    {
        /// <summary>
        /// 验证项名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 验证类型 (对应 ValidationType 枚举值)
        /// </summary>
        public int ValidationType { get; set; }

        /// <summary>
        /// 验证状态 (对应 ValidationStatus 枚举值)
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 配置项列表
        /// </summary>
        public List<ConfigItemData> ConfigItems { get; set; } = new List<ConfigItemData>();

        /// <summary>
        /// 验证描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 最后运行时间
        /// </summary>
        public DateTime? LastRunTime { get; set; }

        /// <summary>
        /// 验证结果
        /// </summary>
        public string ValidationResult { get; set; }

        /// <summary>
        /// Python脚本路径
        /// </summary>
        public string ScriptPath { get; set; }

        /// <summary>
        /// Python解释器路径
        /// </summary>
        public string PyexePath { get; set; }
    }

    /// <summary>
    /// 配置项数据模型
    /// </summary>
    public class ConfigItemData
    {
        /// <summary>
        /// 配置键
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 配置值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// IO点检页面配置数据模型
    /// </summary>
    public class PageConfig
    {
        /// <summary>
        /// 页面名称
        /// </summary>
        public string PageName { get; set; }

        /// <summary>
        /// 点检确认消息
        /// </summary>
        public string CheckConfirmMessage { get; set; }
    }

    /// <summary>
    /// IO点检配置持久化数据模型
    /// </summary>
    public class PersistenceData
    {
        /// <summary>
        /// IO点检页面配置列表
        /// </summary>
        public List<PageConfig> PageConfigs { get; set; } = new List<PageConfig>();

        /// <summary>
        /// 最后保存时间
        /// </summary>
        public DateTime LastSavedTime { get; set; }
    }
}
