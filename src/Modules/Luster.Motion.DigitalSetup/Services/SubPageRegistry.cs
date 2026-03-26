using Luster.Motion.DigitalSetup.Datas;
using System.Collections.Generic;

namespace Luster.Motion.DigitalSetup.Services
{
    /// <summary>
    /// 子页面注册表 - 用于存储每个一级页面对应的二级子页面信息
    /// </summary>
    public static class SubPageRegistry
    {
        /// <summary>
        /// 存储一级页面Region名称到二级子页面列表的映射
        /// </summary>
        private static readonly Dictionary<string, List<SubPageInfo>> _subPages = new Dictionary<string, List<SubPageInfo>>();

        /// <summary>
        /// 注册子页面
        /// </summary>
        /// <param name="parentRegion">父页面的Region名称</param>
        /// <param name="subPageName">子页面名称</param>
        /// <param name="subPageRegion">子页面Region</param>
        public static void RegisterSubPage(string parentRegion, string subPageName, string subPageRegion = "")
        {
            if (!_subPages.ContainsKey(parentRegion))
            {
                _subPages[parentRegion] = new List<SubPageInfo>();
            }

            _subPages[parentRegion].Add(new SubPageInfo
            {
                Name = subPageName,
                Region = subPageRegion
            });
        }

        /// <summary>
        /// 获取指定父页面的所有子页面
        /// </summary>
        /// <param name="parentRegion">父页面的Region名称</param>
        /// <returns>子页面列表，如果没有则返回空列表</returns>
        public static List<SubPageInfo> GetSubPages(string parentRegion)
        {
            if (_subPages.TryGetValue(parentRegion, out var subPages))
            {
                return subPages;
            }
            return new List<SubPageInfo>();
        }

        /// <summary>
        /// 清空所有注册的子页面
        /// </summary>
        public static void Clear()
        {
            _subPages.Clear();
        }
    }

    /// <summary>
    /// 子页面信息
    /// </summary>
    public class SubPageInfo
    {
        public string Name { get; set; }
        public string Region { get; set; }
    }
}
