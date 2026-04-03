#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IFloatingInfoConfigService
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Assets.FloatingInfo.Services
* 文 件 名:       IFloatingInfoConfigService.cs
* 创建时间:       2026/03/24
* 作    者:       Luster
* 版    权:       <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 唯一标识：      a1b2c3d4-e5f6-7890-abcd-ef1234567895
* 创建年份:      2026
************************************************************************************/

#endregion

using Luster.Common.Assets.FloatingInfo.Models;
using System.Collections.Generic;

namespace Luster.Common.Assets.FloatingInfo.Services
{
    /// <summary>
    /// 浮动信息配置服务接口
    /// </summary>
    public interface IFloatingInfoConfigService
    {
        /// <summary>
        /// 获取所有配置
        /// </summary>
        /// <returns>配置列表</returns>
        List<FloatingInfoConfig> GetAllConfigs();

        /// <summary>
        /// 根据页面ID获取配置
        /// </summary>
        /// <param name="pageId">页面ID</param>
        /// <returns>配置</returns>
        FloatingInfoConfig GetConfig(string pageId);

        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="config">配置</param>
        void SaveConfig(FloatingInfoConfig config);

        /// <summary>
        /// 保存所有配置
        /// </summary>
        /// <param name="configs">配置列表</param>
        void SaveAllConfigs(IEnumerable<FloatingInfoConfig> configs);

        /// <summary>
        /// 删除配置
        /// </summary>
        /// <param name="pageId">页面ID</param>
        /// <returns>是否删除成功</returns>
        bool DeleteConfig(string pageId);

        /// <summary>
        /// 检查配置是否存在
        /// </summary>
        /// <param name="pageId">页面ID</param>
        /// <returns>是否存在</returns>
        bool ExistsConfig(string pageId);

        /// <summary>
        /// 加载配置文件
        /// </summary>
        void Load();

        /// <summary>
        /// 保存配置到文件
        /// </summary>
        void Save();

        /// <summary>
        /// 设置配置文件路径
        /// </summary>
        /// <param name="configFilePath">配置文件的完整路径</param>
        void SetConfigPath(string configFilePath);

        /// <summary>
        /// 获取当前配置文件路径
        /// </summary>
        /// <returns>配置文件路径</returns>
        string GetConfigPath();

        /// <summary>
        /// 获取基准路径（recipe根目录）
        /// </summary>
        /// <returns>基准路径</returns>
        string GetBasePath();
    }
}
