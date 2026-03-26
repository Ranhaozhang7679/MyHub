using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataAccess.Tables
{
    /// <summary>
    /// 瑕疵对象
    /// </summary>
    [Table(Name = "AOIDefect")]
    public class TbAOIDefect : BaseTable
    {
        /// <summary>
        /// 隶属图片
        /// </summary>
        public long ImageID { get; set; }

        /// <summary>
        /// 缺陷名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 左
        /// </summary>
        public int Left { get; set; }

        /// <summary>
        /// 上
        /// </summary>
        public int Top { get; set; }

        /// <summary>
        /// 右
        /// </summary>
        public int Right { get; set; }

        /// <summary>
        /// 下
        /// </summary>
        public int Bottom { get; set; }

        /// <summary>
        /// 缺陷小图路径
        /// </summary>
        public string Img_def { get; set; }

        /// <summary>
        /// 缺陷面积
        /// </summary>
        public double Area { get; set; }

        /// <summary>
        /// 灰度信息
        /// </summary>
        public double Gray { get; set; }

        /// <summary>
        /// 错误代码
        /// </summary>
        public string Code { get; set; }
    }
}
