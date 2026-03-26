using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataAccess.Tables
{
    /// <summary>
    /// 图像对象
    /// </summary>
    [Table(Name = "AOIImage")]
    public class TbAOIImage : BaseTable
    {
        /// <summary>
        /// 隶属AOI ID
        /// </summary>
        public long ParentID { get; set; }

        /// <summary>
        /// 原始图片路径
        /// </summary>
        public string Img_Raw { get; set; }

        /// <summary>
        /// 缩放图片路径
        /// </summary>
        public string Img_Zoom { get; set; }


        /// <summary>
        /// 缺陷集合
        /// </summary>
        [Column(IsIgnore = true)]
        public List<TbAOIDefect> Defects { get; set; }
    }
}
