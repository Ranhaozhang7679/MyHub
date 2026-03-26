using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Interfaces
{
    public  interface IParam
    {
        /// <summary>
        /// 最小值
        /// </summary>
        public int MinValue { get; set; }

        /// <summary>
        /// 最大值
        /// </summary>
        public int MaxValue { get; set; }

        /// <summary>
        /// 目标值
        /// </summary>
        public int TargetValue { get; set; }

    }
}
