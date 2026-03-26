#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IFloatingInfoService
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Assets.FloatingInfo.Services
* 文 件 名:       IFloatingInfoService.cs
* 创建时间:       2026/03/24
* 作    者:       Luster
* 版    权:       <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 唯一标识：      a1b2c3d4-e5f6-7890-abcd-ef1234567896
* 创建年份:       2026
************************************************************************************/

#endregion

using Luster.Common.Assets.FloatingInfo.Models;

namespace Luster.Common.Assets.FloatingInfo.Services
{
    /// <summary>
    /// 浮动信息窗口服务接口
    /// </summary>
    public interface IFloatingInfoService
    {
        /// <summary>
        /// 显示指定页面的浮动信息窗口
        /// </summary>
        /// <param name="pageId">页面唯一标识</param>
        void ShowFloatingInfo(string pageId);

        /// <summary>
        /// 隐藏指定页面的浮动信息窗口
        /// </summary>
        /// <param name="pageId">页面唯一标识</param>
        void HideFloatingInfo(string pageId);

        /// <summary>
        /// 隐藏所有浮动信息窗口
        /// </summary>
        void HideAllFloatingInfo();

        /// <summary>
        /// 检查指定页面的浮动信息窗口是否可见
        /// </summary>
        /// <param name="pageId">页面唯一标识</param>
        /// <returns>是否可见</returns>
        bool IsVisible(string pageId);

        /// <summary>
        /// 最小化指定页面的浮动信息窗口
        /// </summary>
        /// <param name="pageId">页面唯一标识</param>
        void MinimizeFloatingInfo(string pageId);

        /// <summary>
        /// 恢复最小化的浮动信息窗口
        /// </summary>
        /// <param name="pageId">页面唯一标识</param>
        void RestoreFloatingInfo(string pageId);

        /// <summary>
        /// 注册页面配置
        /// </summary>
        /// <param name="config">配置对象</param>
        void RegisterConfig(FloatingInfoConfig config);

        /// <summary>
        /// 打开设置对话框
        /// </summary>
        /// <param name="pageId">页面唯一标识</param>
        void OpenSettings(string pageId);

        /// <summary>
        /// 获取所有活动窗口的页面ID
        /// </summary>
        /// <returns>页面ID列表</returns>
        System.Collections.Generic.IReadOnlyCollection<string> GetActiveWindowPageIds();
    }
}
