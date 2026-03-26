using FreeSql.DataAnnotations;
using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Common.DataAccess.Tables
{
    [Table(Name = "AOIResult")]
    public class TbAOIResult : BaseTable
    {
        /// <summary>
        /// 返回结果
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// 产品
        /// </summary>
        public int Product_Index { get; set; }

        public string Product_Id { get; set; }

        /// <summary>
        /// 位置号，0、1、2、3，由ICW发来的得到
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// 二维码
        /// </summary>
        public string Serial { get; set; }

        public int Pc { get; set; }

        public string Result { get; set; }

        public string Resulttype { get; set; }

        /// <summary>
        /// 缺陷名称
        /// </summary>
        public string Resultname { get; set; }

        public string Status { get; set; }

        public int Resulimageindex { get; set; }

        public int Resuldefectindex { get; set; }

        /// <summary>
        /// AOI检查时间
        /// </summary>
        public double Checktime { get; set; }

        /// <summary>
        /// 缺陷等级
        /// </summary>
        public string Grade { get; set; }


        /// <summary>
        /// 缺陷等级
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 忽略
        /// </summary>
        [Column(IsIgnore = true)]
        public List<TbAOIImage> Images { get; set; }
    }
}
