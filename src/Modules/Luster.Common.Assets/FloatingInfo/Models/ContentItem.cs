#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ContentItem
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Assets.FloatingInfo.Models
* 文 件 名:       ContentItem.cs
* 创建时间:       2026/03/24
* 作    者:       Luster
* 版    权:       <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 唯一标识：      a1b2c3d4-e5f6-7890-abcd-ef1234567891
* 创建年份:       2026
************************************************************************************/

#endregion

using Newtonsoft.Json;
using Prism.Mvvm;

namespace Luster.Common.Assets.FloatingInfo.Models
{
    /// <summary>
    /// 浮动信息内容项基类
    /// </summary>
    [JsonConverter(typeof(ContentItemConverter))]
    public abstract class ContentItem : BindableBase
    {
        /// <summary>
        /// 内容类型
        /// </summary>
        public abstract ContentType ContentType { get; }

        /// <summary>
        /// 显示顺序
        /// </summary>
        private int _order;
        public int Order
        {
            get => _order;
            set => SetProperty(ref _order, value);
        }
    }
}
