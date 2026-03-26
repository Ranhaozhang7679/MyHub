using FreeSql.DataAnnotations;
using Luster.Common.DataStruct.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataAccess.Tables
{
    /// <summary>
    /// NG载具信息
    /// </summary>
    [Table(Name = "NGCarrierTable")]
    public class TbNGCarrirerData:BaseTable
    {
        [PropSort(1), DisplayName("治具码")]
        public string CarrierId { get; set; }

        [PropSort(2), DisplayName("次数")]
        public int Count{ get; set; }
    }
}
