#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ContentTemplateSelector
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Assets.FloatingInfo.Selectors
* 文 件 名:       ContentTemplateSelector.cs
* 创建时间:       2026/03/24
* 作    者:       Luster
* 版    权:       <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 唯一标识：      a1b2c3d4-e5f6-7890-abcd-ef1234567902
* 创建年份:       2026
************************************************************************************/

#endregion

using System.Windows;
using System.Windows.Controls;
using Luster.Common.Assets.FloatingInfo.Models;

using Luster.Common.Assets.FloatingInfo.Views;

using System.Windows.Data;

namespace Luster.Common.Assets.FloatingInfo.Selectors
{
    /// <summary>
    /// 内容模板选择器
    /// </summary>
    public class ContentTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// 文本模板
        /// </summary>
        public DataTemplate TextTemplate { get; set; }

        /// <summary>
        /// 图片模板
        /// </summary>
        public DataTemplate ImageTemplate { get; set; }

        /// <summary>
        /// 选择模板
        /// </summary>
        /// <param name="item">内容项</param>
        /// <param name="container">依赖对象</param>
        /// <returns>返回对应的Data模板</如果为null，返回默认模板</returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is TextContentItem)
                return TextTemplate;
            else if (item is ImageContentItem)
                return ImageTemplate;
            return base.SelectTemplate(item, container);
        }
    }
}
